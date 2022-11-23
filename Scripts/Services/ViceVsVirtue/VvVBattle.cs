using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Network;
using Server.Guilds;
using Server.Engines.Points;
using Server.Engines.CityLoyalty;
using Server.SkillHandlers;
using Server.Multis;
using Server.Regions;
using Server.Misc;
using Server.Commands;
using Fronteira.Discord;

namespace Server.Engines.VvV
{
    public enum UpdateType
    {
        Kill,
        Assist,
        Steal,
        TurnInVice,
        TurnInVirtue,
        Disarm
    }

    [PropertyObject]
    public class VvVBattle
    {
        public static TimeSpan ultimoCd = TimeSpan.Zero;

        public static readonly int Duration = 30;

        [CommandProperty(AccessLevel.GameMaster)]
        public int TempoAteProxima
        {
            get
            {
                /*
                if(ultimoCd != TimeSpan.Zero)
                {
                    ultimoCd.Add(TimeSpan.FromHours(24));
                    return (int)ultimoCd.TotalMinutes;
                }
                */

                var dateNow = DateTime.UtcNow;
                var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 23, 0, 0);
                TimeSpan ts;
                if (date > dateNow)
                    ts = date - dateNow;
                else
                {
                    date = date.AddDays(1);
                    ts = date - dateNow;
                }
                var cooldown = ts;
                /*
                var today = DateTime.Today;
                var agora = DateTime.Now;
                var proximaGuerra = today.AddHours(20);

                var cooldown = proximaGuerra - agora;
                if (proximaGuerra < agora)
                {
                    cooldown = agora - proximaGuerra;
                    cooldown.Add(TimeSpan.FromHours(24));
                } else
                {
                     cooldown = proximaGuerra - agora;
                }
                */
                /*
                var ult = today.Subtract(TimeSpan.FromHours(4));

                var cooldown = agora - ult;
                if (agora > ult)
                {
                    ult = today.AddHours(20);
                    cooldown = ult - agora;
                }
                */
                Shard.Debug("-------------- COOLDOWN VVV ------------------");
                //Shard.Debug("Horas: " + cooldown.TotalHours);
                //ultimoCd = cooldown;
                return (int)cooldown.TotalMinutes;
            }
        }
        public static readonly int Announcement = 2;
        public static readonly int KillCooldownDuration = 5;
        public static readonly int MaxTraps = 20;
        public static readonly int MaxTurrets = 10;

        //Battle Scoring
        public static readonly double ScoreToWin = 10000;
        public static readonly double OccupyPoints = 300;
        public static readonly double AltarPoints = 1000;
        public static readonly double KillPoints = 600;
        public static readonly double TurnInPoints = 500;

        public static readonly int AltarSilver = 100;
        public static readonly int TurnInSilver = 100;
        public static readonly int KillSilver = 50;
        public static readonly int WinSilver = 500;
        public static readonly int DisarmSilver = 50;

        // silver penalty in the event the battle is uncontested
        public static readonly double Penalty = 0.5;

        [CommandProperty(AccessLevel.GameMaster)]
        public double SilverPenalty
        {
            get
            {
                if (!ViceVsVirtueSystem.EnhancedRules)
                    return 1.0;

                return UnContested ? Penalty : 1.0;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ViceVsVirtueSystem System { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime StartTime { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime CooldownEnds { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastOccupationCheck { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextSigilSpawn { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextAnnouncement { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextAltarActivate { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextManaSpike { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime ManaSpikeEndEffects { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public VvVCity City { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public VvVSigil Sigil { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool OnGoing { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UnContested { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Region Region
        {
            get
            {
                CityInfo info = CityInfo.Infos[City];

                if (info != null)
                {
                    return info.Region;
                }

                return null;
            }

        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ForceStart
        {
            get { return false; }
            set
            {
                if (!OnGoing && value)
                    Begin();
            }
        }


        [CommandProperty(AccessLevel.GameMaster)]
        public bool ForceEnd
        {
            get { return false; }
            set
            {
                if (OnGoing && value)
                    EndBattle();
            }
        }

        public List<BattleTeam> Teams { get; set; }

        public Dictionary<Mobile, DateTime> KillCooldown { get; set; }
        public Dictionary<Mobile, DateTime> BattleAggression { get; set; }
        public List<string> Messages { get; set; }

        public List<VvVAltar> Altars { get; set; }
        public int AltarIndex { get; set; }

        public List<VvVTrap> Traps { get; set; }
        public List<CannonTurret> Turrets { get; set; }
        public List<Mobile> Warned { get; set; }

        public VvVPriest VicePriest { get; private set; }
        public VvVPriest VirtuePriest { get; private set; }

        public Timer Timer { get; private set; }

        public int TrapCount { get { return Traps.Where(t => !t.Deleted).Count(); } }
        public int TurretCount { get { return Turrets.Where(t => !t.Deleted).Count(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool InCooldown
        {
            get { return CooldownEnds > DateTime.UtcNow; }
        }

        public DateTime NextCombatHeatCycle { get; private set; }

        public VvVBattle(ViceVsVirtueSystem sys)
        {
            System = sys;
        }

        public void Begin(VvVCity cidade = VvVCity.NIUMA)
        {
            ViceVsVirtueSystem.Entraram.Clear();

            OnGoing = true;
            PisouDentro.Clear();
            NextCombatHeatCycle = DateTime.UtcNow;
            VvVCity newCity = City;
            List<VvVCity> cities = new List<VvVCity>();

            Teams = new List<BattleTeam>();
            KillCooldown = new Dictionary<Mobile, DateTime>();
            Messages = new List<string>();
            Altars = new List<VvVAltar>();
            Traps = new List<VvVTrap>();
            Warned = new List<Mobile>();
            Turrets = new List<CannonTurret>();

            /*
            for (int i = 0; i < 7; i++)
            {
                if (!System.ExemptCities.Contains((VvVCity)i) && (VvVCity)i != newCity)
                    cities.Add((VvVCity)i);
            }

            if (cities.Count > 0)
            {
                newCity = cities[Utility.Random(cities.Count)];
            }
            else if (System.ExemptCities.Contains(newCity))
            {
                System.SendVvVMessage("As cidades estao vazias.");
                return;
            }
            */
            newCity = VvVCity.Jhelom;

            ColUtility.Free(cities);
            City = newCity;

            BeginTimer();

            StartTime = DateTime.UtcNow;
            NextSigilSpawn = DateTime.UtcNow + TimeSpan.FromMinutes(Utility.RandomMinMax(1, 3));

            AltarIndex = 0;
            SpawnAltars();
            SpawnPriests(false);

            if (Region is GuardedRegion)
            {
                GuardedRegion.Disable((GuardedRegion)Region);
            }

            NextAltarActivate = DateTime.UtcNow + TimeSpan.FromMinutes(1);

            Anuncio.Anuncia("A Guerra Infinita Se Inicia em " + City.ToString());
            Anuncio.Anuncia("Guildas, lutem pelo dominio da cidade !");
            //System.SendVvVMessage("A guerra infinita se inicia na cidade " + ViceVsVirtueSystem.GetCityLocalization(City).ToString());
            // A Battle between Vice and Virtue is active! To Arms! The City of ~1_CITY~ is besieged!
        }

        public void SpawnAltars()
        {

            foreach (Point3D p in CityInfo.Infos[City].AltarLocs)
            {
                VvVAltar altar = new VvVAltar(this);
                altar.MoveToWorld(p, Map.Trammel);

                Altars.Add(altar);
            }
        }

        public void SpawnPriests(bool movetoworld = true)
        {
            if (VicePriest == null || VicePriest.Deleted)
                VicePriest = new VvVPriest(VvVType.Vice, this);

            if (VirtuePriest == null || VirtuePriest.Deleted)
                VirtuePriest = new VvVPriest(VvVType.Virtue, this);

            if (movetoworld)
            {
                Point3D p;

                do
                {
                    p = Map.Trammel.GetRandomSpawnPoint(CityInfo.Infos[City].PriestLocation);
                }
                while (!Map.Trammel.CanSpawnMobile(p));

                VicePriest.MoveToWorld(p, Map.Trammel);

                do
                {
                    p = Map.Trammel.GetRandomSpawnPoint(CityInfo.Infos[City].PriestLocation);
                }
                while (!Map.Trammel.CanSpawnMobile(p));

                VirtuePriest.MoveToWorld(p, Map.Trammel);
            }
        }

        public void BeginTimer()
        {
            EndTimer();

            Timer = Timer.DelayCall(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), OnTick);
            Timer.Start();
        }

        public void EndTimer()
        {
            if (Timer != null)
            {
                Timer.Stop();
                Timer = null;
            }
        }

        public void OnTick()
        {
            if (!OnGoing)
                return;

            CheckParticipation();
            UpdateAllGumps();

            if (StartTime + TimeSpan.FromMinutes(Duration) < DateTime.UtcNow)
            {
                EndBattle();
                return;
            }

            if (LastOccupationCheck + TimeSpan.FromMinutes(1) < DateTime.UtcNow)
            {
                CheckOccupation();

                LastOccupationCheck = DateTime.UtcNow;
            }

            if (NextAltarActivate != DateTime.MinValue && NextAltarActivate < DateTime.UtcNow)
            {
                ActivateAltar();
                NextAltarActivate = DateTime.MinValue;
            }

            if (NextSigilSpawn != DateTime.MinValue && NextSigilSpawn < DateTime.UtcNow)
            {
                if (Sigil == null || Sigil.Deleted)
                {
                    SpawnSigil();
                }
            }

            ActivateArrows();

            if (KillCooldown != null)
            {
                List<Mobile> list = KillCooldown.Keys.Where(mob => KillCooldown[mob] < DateTime.UtcNow).ToList();

                foreach (Mobile m in list)
                {
                    KillCooldown.Remove(m);
                }

                ColUtility.Free(list);
            }

            if (Turrets != null)
            {
                Turrets.ForEach(t => { t.Scan(); });
            }
        }

        public void CheckParticipation()
        {
            if (Region == null)
                return;

            BattleTeam team = null;
            UnContested = true;
            bool checkAggression = ViceVsVirtueSystem.EnhancedRules && NextCombatHeatCycle < DateTime.UtcNow;

            foreach (PlayerMobile pm in this.Region.GetEnumeratedMobiles().OfType<PlayerMobile>())
            {
                bool vvv = ViceVsVirtueSystem.IsVvV(pm);

                if (!vvv && !Warned.Contains(pm) && pm.AccessLevel <= AccessLevel.VIP && pm.Guild != null)
                {
                    pm.SendGump(new BattleWarningGump(pm));
                    Warned.Add(pm);
                }
                else if (vvv && pm.Alive && !pm.Hidden && BaseBoat.FindBoatAt(pm.Location, pm.Map) == null && BaseHouse.FindHouseAt(pm) == null)
                {
                    Guild g = pm.Guild as Guild;

                    if (g != null)
                    {
                        var t = GetTeam(g);

                        if (team == null)
                        {
                            team = t;
                        }
                        else if (t != team && UnContested)
                        {
                            UnContested = false;
                        }
                    }
                }

                if (checkAggression && (vvv || ViceVsVirtueSystem.IsVvVCombatant(pm)))
                {
                    AddAggression(pm);
                }
            }

            if (checkAggression)
            {
                CheckBattleAggression();

                NextCombatHeatCycle = DateTime.UtcNow + TimeSpan.FromMinutes(1);
            }
        }

        public void AddAggression(Mobile m)
        {
            if (m is PlayerMobile)
            {
                if (BattleAggression == null)
                {
                    BattleAggression = new Dictionary<Mobile, DateTime>();
                }

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.HeatOfBattleStatus, 1153801, 1153827, Aggression.CombatHeatDelay, m, true));
                BattleAggression[m] = DateTime.UtcNow + TimeSpan.FromMinutes(2);
            }
        }

        private void CheckBattleAggression()
        {
            if (BattleAggression == null)
                return;

            var list = new List<Mobile>(BattleAggression.Keys);

            foreach (var m in list)
            {
                if (BattleAggression[m] < DateTime.UtcNow && !m.Region.IsPartOf(Region))
                {
                    BattleAggression.Remove(m);
                }
            }

            ColUtility.Free(list);
        }

        public bool HasBattleAggression(Mobile m)
        {
            if (BattleAggression == null || !BattleAggression.ContainsKey(m))
            {
                return false;
            }

            if (BattleAggression[m] < DateTime.UtcNow)
            {
                BattleAggression.Remove(m);
                return false;
            }

            return true;
        }

        public List<Mobile> PisouDentro = new List<Mobile>();

        public void EndBattle()
        {
            OnGoing = false;
            EndTimer();
            
            if (Region is GuardedRegion)
            {
                ((GuardedRegion)Region).Disabled = false;

                foreach (PlayerMobile pm in Region.GetEnumeratedMobiles().OfType<PlayerMobile>())
                {
                    pm.RecheckTownProtection();
                    pm.Delta(MobileDelta.Noto);
                }
            }

            foreach (VvVAltar altar in Altars)
            {
                if (!altar.Deleted)
                    altar.Delete();
            }

            foreach (VvVTrap trap in Traps)
            {
                if (!trap.Deleted)
                    trap.Delete();
            }

            foreach (CannonTurret turret in Turrets)
            {
                if (!turret.Deleted)
                    turret.Delete();
            }

            if (VicePriest != null)
            {
                VicePriest.Delete();
                VicePriest = null;
            }

            if (VirtuePriest != null)
            {
                VirtuePriest.Delete();
                VirtuePriest = null;
            }

            if (Sigil != null)
            {
                Sigil.Delete();
                Sigil = null;
            }

            TallyStats();
            SendBattleStatsGump();

            System.SendVvVMessage("A Guerra Infinita terminou !"); // A VvV battle has just concluded. The next battle will begin in less than five minutes!

            if (BattleAggression != null)
            {
                BattleAggression.Clear();
            }

            ColUtility.Free(Altars);
            ColUtility.Free(Teams);
            KillCooldown.Clear();
            ColUtility.Free(Messages);
            ColUtility.Free(Traps);
            ColUtility.Free(Warned);
            ColUtility.Free(Turrets);

            if (Region is GuardedRegion)
            {
                ((GuardedRegion)Region).Disabled = false;
            }

            NextSigilSpawn = DateTime.MinValue;
            LastOccupationCheck = DateTime.MinValue;
            NextAnnouncement = DateTime.MinValue;
            StartTime = DateTime.MinValue;
            NextAltarActivate = DateTime.MinValue;
            ManaSpikeEndEffects = DateTime.MinValue;
            NextManaSpike = DateTime.MinValue;

            foreach (PlayerMobile pm in Region.GetEnumeratedMobiles().OfType<PlayerMobile>())
            {
                pm.Delta(MobileDelta.Noto);
            }

            TimerRestart();
        }

        public void TimerRestart()
        {
            if (!ViceVsVirtueSystem.Enabled)
                return;

            var cooldown = TempoAteProxima;
            CooldownEnds = DateTime.UtcNow + TimeSpan.FromMinutes(cooldown);

            Timer.DelayCall(TimeSpan.FromMinutes(cooldown), () =>
            {
                System.CheckBattleStatus(true);
            });
            var msg = "A guerra infinita eh um evento PvP. Jogadores com ou sem guilda podem ir a cidade da guerra para lutar e ganhar premios. Nao se perde items e ganha-se premios por participar se tiver uma guilda !";

            var msg1 = cooldown - 60;
            Timer.DelayCall(TimeSpan.FromMinutes(msg1), () =>
            {
                Anuncio.Anuncia("Guerra Infinita inicia em 1 hora");
                foreach (var pl in NetState.GetOnlinePlayerMobiles())
                    pl.SendMessage(78, msg);
            });

            var msg2 = cooldown - 30;
            Timer.DelayCall(TimeSpan.FromMinutes(msg2), () =>
            {
                Anuncio.Anuncia("Guerra Infinita inicia em meia hora");
            });

            var msg3 = cooldown - 10;
            Timer.DelayCall(TimeSpan.FromMinutes(msg3), () =>
            {
                Anuncio.Anuncia("Guerra Infinita inicia em 10 minutos");
                foreach (var pl in NetState.GetOnlinePlayerMobiles())
                    pl.SendMessage(78, msg);
            });

            var msg4 = cooldown - 3;
            Timer.DelayCall(TimeSpan.FromMinutes(msg4), () =>
            {
                Anuncio.Anuncia("Guerra Infinita inicia em 3 minutos");
                foreach (var pl in NetState.GetOnlinePlayerMobiles())
                    pl.SendMessage(78, msg);
            });

            var msg5 = cooldown - 1;
            Timer.DelayCall(TimeSpan.FromMinutes(msg4), () =>
            {
                Anuncio.Anuncia("Guerra Infinita inicia em 1 minutos");
            });
        }

        public void TallyStats()
        {
            BattleTeam leader = GetLeader();
            List<Guild> added = new List<Guild>();

            if (leader == null || leader.Guild == null)
                return;

            var oposition = OppositionCount(leader.Guild);
            leader.Silver += AwardSilver(WinSilver + (oposition * 50));

            foreach (Mobile m in this.Region.GetEnumeratedMobiles())
            {
                Guild g = m.Guild as Guild;

                if (g == null)
                    continue;

                PlayerMobile pm = m as PlayerMobile;

                if (pm != null)
                {
                    BattleTeam team = GetTeam(g);
                    VvVPlayerBattleStats stats = GetPlayerStats(pm);
                    VvVPlayerEntry entry = ViceVsVirtueSystem.Instance.GetPlayerEntry<VvVPlayerEntry>(pm);

                    if (entry != null)
                    {
                        entry.Score += team.Score;
                        entry.Points += team.Silver;
                        entry.Kills += stats.Kills;
                        entry.Deaths += stats.Deaths;
                        entry.Assists += stats.Assists;
                        entry.ReturnedSigils += stats.ReturnedSigils;
                        entry.DisarmedTraps += stats.Disarmed;
                        entry.StolenSigils += stats.Stolen;

                        if (added.Contains(g))
                            continue;
                        else
                            added.Add(g);

                        if (!ViceVsVirtueSystem.Instance.GuildStats.ContainsKey(g))
                            ViceVsVirtueSystem.Instance.GuildStats[g] = new VvVGuildStats(g);

                        VvVGuildStats gstats = ViceVsVirtueSystem.Instance.GuildStats[g];

                        gstats.Kills += team.Kills;
                        gstats.ReturnedSigils += team.ReturnedSigils;
                        gstats.Score += team.Score;
                    }
                }
            }

            ColUtility.Free(added);
        }

        public void SpawnSigil()
        {
            Point3D p = CityInfo.Infos[City].SigilLocs[Utility.Random(CityInfo.Infos[City].SigilLocs.Length)];
            Sigil = new VvVSigil(this, p);

            Sigil.MoveToWorld(p, Map.Trammel);

            UpdateAllGumps();
        }

        public void ActivateAltar()
        {
            if (AltarIndex == 2)
                AltarIndex = 0;
            else
                AltarIndex++;

            Altars[AltarIndex].Activate();
            ActivateArrows();

            SendStatusMessage("Lute pelos altares !", true);
        }

        public void CheckArrow(PlayerMobile pm)
        {
            if (pm.NetState == null)
                return;

            foreach (VvVAltar altar in Altars)
            {
                if (altar.IsActive)
                {
                    pm.QuestArrow = new AltarArrow(pm, altar);
                    break;
                }
            }
        }

        public void ActivateArrows()
        {
            if (Altars == null || Region == null)
                return;

            foreach (PlayerMobile pm in this.Region.GetEnumeratedMobiles().OfType<PlayerMobile>())
            {
                if (pm.NetState != null && pm.QuestArrow == null)
                {
                    foreach (VvVAltar altar in Altars)
                    {
                        if (altar.IsActive)
                        {
                            pm.QuestArrow = new AltarArrow(pm, altar);
                            break;
                        }
                    }
                }
            }
        }

        public VvVPlayerBattleStats GetPlayerStats(PlayerMobile pm)
        {
            if (pm == null || pm.Guild == null)
                return null;

            Guild g = pm.Guild as Guild;

            BattleTeam team = GetTeam(g);
            VvVPlayerBattleStats stats = team.PlayerStats.FirstOrDefault(s => s.Player == pm);

            if (stats == null)
            {
                stats = new VvVPlayerBattleStats(pm);
                team.PlayerStats.Add(stats);
            }

            return stats;
        }

        public BattleTeam GetTeam(Guild g)
        {
            BattleTeam team = Teams.FirstOrDefault(t => t.Guild != null && (t.Guild == g || t.Guild.IsAlly(g)));

            if (team != null)
                return team;

            team = new BattleTeam(g);
            Teams.Add(team);

            return team;
        }

        public void Update(Mobile m, UpdateType type)
        {
            VvVPlayerEntry entry = System.GetPlayerEntry<VvVPlayerEntry>(m);

            if (entry != null)
            {
                Update(null, entry, type);
            }
        }

        public void Update(VvVPlayerEntry victim, VvVPlayerEntry killer, UpdateType type)
        {
            if (killer == null || killer.Player == null || killer.Guild == null)
                return;

            VvVPlayerBattleStats killerStats = GetPlayerStats(killer.Player);
            VvVPlayerBattleStats victimStats = victim == null ? null : GetPlayerStats(victim.Player);

            BattleTeam killerTeam = GetTeam(killer.Guild);
            BattleTeam victimTeam = null;

            if (victim != null)
                victimTeam = GetTeam(victim.Guild);

            switch (type)
            {
                case UpdateType.Kill:
                    if (killerStats != null) killerStats.Kills++;
                    if (victimStats != null) victimStats.Deaths++;

                    if (killerTeam != null)
                        killerTeam.Kills++;

                    if (victimTeam != null)
                        victimTeam.Deaths++;

                    if (victim != null && victim.Player != null)
                    {
                        if (!KillCooldown.ContainsKey(victim.Player) || KillCooldown[victim.Player] < DateTime.UtcNow)
                        {
                            if (killerTeam != null)
                            {
                                killerTeam.Score += (int)KillPoints;
                                killerTeam.Silver += AwardSilver(KillSilver + (OppositionCount(killer.Guild) * 30));
                            }

                            SendStatusMessage(String.Format("{0} matou {1}!", killer.Player.Name, victim.Player.Name), false, true);
                            KillCooldown[victim.Player] = DateTime.UtcNow + TimeSpan.FromMinutes(KillCooldownDuration);
                        }
                    }

                    break;
                case UpdateType.Assist:
                    if (killerStats != null)
                        killerStats.Assists++;

                    if (killerTeam != null)
                        killerTeam.Assists++;

                    break;
                case UpdateType.Steal:
                    if (killerStats != null)
                    {
                        killerStats.Stolen++;
                        SendStatusMessage(String.Format("{0} roubou o sigilo !", killer.Player.Name));
                    }

                    if (killerTeam != null)
                        killerTeam.Stolen++;

                    break;
                case UpdateType.TurnInVice:
                case UpdateType.TurnInVirtue:
                    if (killerTeam != null)
                    {
                        killerTeam.Score += (int)TurnInPoints;
                        killerTeam.Silver += AwardSilver(TurnInSilver + (OppositionCount(killer.Guild) * 30));
                    }

                    if (killerStats != null && killerTeam != null)
                    {
                        if (type == UpdateType.TurnInVirtue)
                        {
                            killerStats.VirtueReturned++;
                            killerTeam.VirtueReturned++;
                        }
                        else
                        {
                            killerStats.ViceReturned++;
                            killerTeam.ViceReturned++;
                        }
                    }

                    SendStatusMessage(String.Format("{0} retornou o sigilo!", killer.Player.Name));

                    NextSigilSpawn = DateTime.UtcNow + TimeSpan.FromMinutes(1);
                    RemovePriests();

                    break;
                case UpdateType.Disarm:
                    SendStatusMessage(String.Format("{0} disarmou a armadilha!", killer.Player.Name));

                    if (killerStats != null)
                    {
                        killerStats.Disarmed++;
                    }

                    if (killerTeam != null)
                    {
                        killerTeam.Silver += AwardSilver(DisarmSilver + (OppositionCount(killer.Guild) * 50));
                        killerTeam.Disarmed++;
                    }
                    break;
            }

            CheckScore();
        }

        public int AwardSilver(int amount)
        {
            return (int)((double)amount * SilverPenalty);
        }

        public void RemovePriests()
        {
            Timer.DelayCall(TimeSpan.FromSeconds(4), () =>
                {
                    if (VicePriest != null)
                        VicePriest.Internalize();

                    if (VirtuePriest != null)
                        VirtuePriest.Internalize();
                });
        }

        public void OccupyAltar(Guild g)
        {
            if (!OnGoing || g == null)
                return;

            BattleTeam team = GetTeam(g);

            team.Score += (int)AltarPoints;
            team.Silver += AwardSilver(AltarSilver + (OppositionCount(g) * 30));

            SendStatusMessage(String.Format("{0} conquistou um altar!", g != null ? g.Name : "alguem"));

            foreach (PlayerMobile p in Region.GetEnumeratedMobiles().Where(player => player is PlayerMobile))
            {
                if (p.QuestArrow != null)
                    p.QuestArrow = null;
            }

            CheckScore();
            NextAltarActivate = DateTime.UtcNow + TimeSpan.FromMinutes(2);
        }

        public void CheckOccupation()
        {
            if (!OnGoing)
                return;

            if (Teams.Count == 1)
            {
                BattleTeam team = Teams[0];

                team.Score += (int)OccupyPoints;
                UpdateAllGumps();
                CheckScore();

                if (OnGoing && NextAnnouncement < DateTime.UtcNow)
                {
                    System.SendVvVMessage(team.Guild.Name + " esta dominando a cidade"); // ~1_NAME~ is occupying the city!
                    NextAnnouncement = DateTime.UtcNow + TimeSpan.FromMinutes(Announcement);
                }
            }
            else // Is this a bug?  Verified on EA this is how it behaves
            {
                if (NextAnnouncement < DateTime.UtcNow)
                {
                    System.SendVvVMessage("A cidade esta sem dominio !"); // The City is unoccupied! Slay opposing forces to claim the city for your guild!
                    NextAnnouncement = DateTime.UtcNow + TimeSpan.FromMinutes(Announcement);
                }
            }
        }

        public void CheckScore()
        {
            int score;
            GetLeader(out score);

            if (score >= ScoreToWin)
            {
                EndBattle();
                return;
            }

            UpdateAllGumps();
        }

        public BattleTeam GetLeader()
        {
            int score;
            return GetLeader(out score);
        }

        /// <summary>
        /// Gets real time leader of battle, out parameter of their score
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public BattleTeam GetLeader(out int score)
        {
            score = 0;

            List<BattleTeam> teams = new List<BattleTeam>(Teams);
            teams.Sort();

            if (teams.Count > 0)
            {
                score = teams[0].Score;
                return teams[0];
            }

            return null;
        }

        /// <summary>
        /// Returns enemy count, by alliance
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public int OppositionCount(Guild g)
        {
            List<Guild> exempt = new List<Guild>();
            int count = 0;

            foreach (BattleTeam team in Teams)
            {
                if (team.Guild == null || team.Guild == g || team.Guild.IsAlly(g) || exempt.Contains(g))
                    continue;

                count++;

                if (team.Guild.Alliance != null)
                {
                    foreach (Guild guil in team.Guild.Alliance.Members.Where(guil => !exempt.Contains(guil)))
                        exempt.Add(guil);
                }
            }

            ColUtility.Free(exempt);

            return count;
        }

        public bool IsInActiveBattle(Mobile one, Mobile two)
        {
            return IsInActiveBattle(one) && IsInActiveBattle(two);
        }

        public bool IsInActiveBattle(Mobile m)
        {
            if (!OnGoing)
                return false;

            Region r = Region.Find(m.Location, m.Map);

            return r == this.Region;
        }

        public void OnEnterRegion(Mobile m)
        {
            if (m is PlayerMobile && OnGoing)
            {
                if (m.HasGump(typeof(VvVBattleStatusGump)))
                    m.CloseGump(typeof(VvVBattleStatusGump));

                BaseGump.SendGump(new VvVBattleStatusGump((PlayerMobile)m, this));
            }
        }

        public void CheckGump(Mobile m)
        {
            if (m is PlayerMobile && OnGoing)
            {
                BaseGump.SendGump(new VvVBattleStatusGump((PlayerMobile)m, this));
            }
        }

        public void UpdateAllGumps()
        {
            if (Region == null)
                return;

            foreach (PlayerMobile m in this.Region.GetEnumeratedMobiles().OfType<PlayerMobile>())
            {
                if (!ViceVsVirtueSystem.IsVvV(m) || m.NetState == null)
                    continue;

                VvVBattleStatusGump g = m.FindGump(typeof(VvVBattleStatusGump)) as VvVBattleStatusGump;

                if (g == null)
                {
                    BaseGump.SendGump(new VvVBattleStatusGump(m, this));
                }
                else
                {
                    g.Refresh(true, false);
                }
            }
        }

        public void SendBattleStatsGump()
        {
            if (Region == null)
                return;

            foreach (PlayerMobile m in this.Region.GetEnumeratedMobiles().OfType<PlayerMobile>())
            {
                if (ViceVsVirtueSystem.IsVvV(m))
                {
                    m.CloseGump(typeof(VvVBattleStatusGump));
                    m.SendGump(new BattleStatsGump((PlayerMobile)m, this));
                }
            }
        }

        public void SendStatusMessage(string message, bool sendgumps = false, bool disc=true)
        {
            if(disc)
                DiscordBot.SendMessage(":european_castle:[GW] " + message);
            Messages.Add(message);

            if (sendgumps)
                UpdateAllGumps();
        }

        public void AddCannonTurret(CannonTurret turret)
        {
            if (!Turrets.Contains(turret))
                Turrets.Add(turret);
        }

        public VvVBattle(GenericReader reader, ViceVsVirtueSystem system)
        {
            int version = reader.ReadInt();
            System = system;

            Altars = new List<VvVAltar>();
            KillCooldown = new Dictionary<Mobile, DateTime>();
            Messages = new List<string>();
            Traps = new List<VvVTrap>();
            Warned = new List<Mobile>();
            Turrets = new List<CannonTurret>();
            Teams = new List<BattleTeam>();

            OnGoing = reader.ReadBool();

            if (reader.ReadInt() == 0)
            {
                StartTime = reader.ReadDateTime();
                CooldownEnds = reader.ReadDateTime();
                LastOccupationCheck = reader.ReadDateTime();
                NextSigilSpawn = reader.ReadDateTime();
                NextAnnouncement = reader.ReadDateTime();
                NextAltarActivate = reader.ReadDateTime();
                City = (VvVCity)reader.ReadInt();
                Sigil = reader.ReadItem() as VvVSigil;
                VicePriest = reader.ReadMobile() as VvVPriest;
                VirtuePriest = reader.ReadMobile() as VvVPriest;

                if (Sigil != null)
                    Sigil.Battle = this;

                if (VicePriest != null)
                    VicePriest.Battle = this;

                if (VirtuePriest != null)
                    VirtuePriest.Battle = this;

                int count = reader.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    VvVAltar altar = reader.ReadItem() as VvVAltar;
                    if (altar != null)
                    {
                        altar.Battle = this;
                        Altars.Add(altar);
                    }
                }

                if (version == 1)
                {
                    count = reader.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        BattleTeam team = new BattleTeam(reader);
                        Teams.Add(team);
                    }
                }
                else
                {
                    count = reader.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        Guild g = reader.ReadGuild() as Guild;
                        VvVGuildBattleStats stats = new VvVGuildBattleStats(reader, g);
                    }
                }

                count = reader.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    VvVTrap t = reader.ReadItem() as VvVTrap;

                    if (t != null)
                        Traps.Add(t);
                }

                Timer.DelayCall(TimeSpan.FromSeconds(10), () =>
                    {
                        if (Region is GuardedRegion)
                        {
                            GuardedRegion.Disable((GuardedRegion)Region);
                        }

                        BeginTimer();

                        ActivateArrows();
                    });
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(1);

            writer.Write(OnGoing);

            if (OnGoing)
            {
                writer.Write(0);
                writer.Write(StartTime);
                writer.Write(CooldownEnds);
                writer.Write(LastOccupationCheck);
                writer.Write(NextSigilSpawn);
                writer.Write(NextAnnouncement);
                writer.Write(NextAltarActivate);
                writer.Write((int)City);
                writer.Write(Sigil);
                writer.Write(VicePriest);
                writer.Write(VirtuePriest);

                writer.Write(Altars.Count);
                Altars.ForEach(altar => writer.Write(altar));

                /*writer.Write(GuildStats.Count);
                foreach (KeyValuePair<Guild, VvVGuildBattleStats> kvp in GuildStats)
                {
                    writer.Write(kvp.Key);
                    kvp.Value.Serialize(writer);
                }*/
                writer.Write(Teams.Count);
                foreach (BattleTeam team in Teams)
                {
                    team.Serialize(writer);
                }

                writer.Write(Traps.Count);
                Traps.ForEach(t => writer.Write(t));
            }
            else
                writer.Write(1);

        }
    }
}
