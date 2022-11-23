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
using Server.Factions;
using Server.Engines.CityLoyalty;
using Server.Regions;
using Server.Accounting;
using Server.Engines.ArenaSystem;
using Server.Commands;

namespace Server.Engines.VvV
{
    public enum VvVType
    {
        Virtue,
        Vice
    }

    public enum VvVCity
    {
        Britain,
        Jhelom,
        Minoc,
        Moonglow,
        //  Ocllo, 
        SkaraBrae,
        Trinsic,
        Yew,
        NIUMA
    }

    public class ViceVsVirtueSystem : PointsSystem
    {
        public static int VirtueHue = 2124;
        public static int ViceHue = 2118;

        public static bool Enabled = !Shard.RP; // Config.Get("VvV.Enabled", true);
        public static int StartSilver = 0;//Config.Get("VvV.StartSilver", 1000);
        public static bool EnhancedRules = false; //  Config.Get("VvV.EnhancedRules", false);

        public static ViceVsVirtueSystem Instance { get; set; }

        public override TextDefinition Name { get { return new TextDefinition("Guerra Infinita"); } }
        public override PointsType Loyalty { get { return PointsType.ViceVsVirtue; } }
        public override bool AutoAdd { get { return false; } }
        public override double MaxPoints { get { return 10000; } }

        public bool HasGenerated { get; set; }

        public Dictionary<Guild, VvVGuildStats> GuildStats { get; set; }
        public static List<TemporaryCombatant> TempCombatants { get; set; }

        public List<VvVCity> ExemptCities { get; set; }

        public VvVBattle Battle { get; private set; }

        public override string ToString()
        {
            return "...";
        }

        public ViceVsVirtueSystem()
        {
            Instance = this;
            Battle = new VvVBattle(this);

            GuildStats = new Dictionary<Guild, VvVGuildStats>();
            ExemptCities = new List<VvVCity>();

            if(Enabled)
                Timer.DelayCall(TimeSpan.FromSeconds(10), () => { Battle.TimerRestart(); });
        }

        public override void SendMessage(PlayerMobile from, double old, double points, bool quest)
        {
        }

        public override PointsEntry GetSystemEntry(PlayerMobile pm)
        {
            return new VvVPlayerEntry(pm);
        }

        public void HandlePlayerDeath(PlayerMobile victim)
        {
            VvVPlayerEntry ventry = GetPlayerEntry<VvVPlayerEntry>(victim);

            if (ventry != null && ventry.Active)
            {
                List<DamageEntry> list = victim.DamageEntries.OrderBy(d => -d.DamageGiven).ToList();
                List<Mobile> handled = new List<Mobile>();
                bool statloss = false;

                for (int i = 0; i < list.Count; i++)
                {
                    Mobile dam = list[i].Damager;

                    if (dam == victim || dam == null)
                        continue;

                    if (dam is BaseCreature && ((BaseCreature)dam).GetMaster() is PlayerMobile)
                        dam = ((BaseCreature)dam).GetMaster();

                    bool isEnemy = IsEnemy(victim, dam);

                    if (isEnemy)
                    {
                        VvVPlayerEntry kentry = GetPlayerEntry<VvVPlayerEntry>(dam);

                        if (kentry != null && kentry.Active && !handled.Contains(dam))
                        {
                            if (Battle.IsInActiveBattle(dam, victim))
                            {
                                if (i == 0)
                                    Battle.Update(ventry, kentry, UpdateType.Kill);
                                else
                                    Battle.Update(ventry, kentry, UpdateType.Assist);
                            }

                            handled.Add(dam);
                            kentry.TotalKills++;

                            if (EnhancedRules && kentry != null)
                            {
                                kentry.AwardSilver(victim);
                            }
                        }

                        if (!handled.Contains(victim))
                        {
                            ventry.TotalDeaths++;
                            handled.Add(victim);
                        }
                    }

                    if (!statloss && isEnemy)
                        statloss = true;
                }

                //if (statloss)
                //    Faction.ApplySkillLoss(victim);

                ColUtility.Free(list);
                ColUtility.Free(handled);
            }
        }

        public void AddPlayer(PlayerMobile pm)
        {
            if (pm == null || pm.Guild == null)
                return;

            Guild g = pm.Guild as Guild;
            VvVPlayerEntry entry = GetEntry(pm, true) as VvVPlayerEntry;

            if (!entry.Active)
                entry.Active = true;

            pm.SendLocalizedMessage("Voce entrou na guerra infinita"); // You have joined Vice vs Virtue!
            //pm.SendLocalizedMessage(1063156, g.Name); // The guild information for ~1_val~ has been updated.

            pm.Delta(MobileDelta.Noto);
            pm.ProcessDelta();

            CheckBattleStatus(pm);
        }

        public bool IsResigning(PlayerMobile pm, Guild g)
        {
            VvVPlayerEntry entry = GetPlayerEntry<VvVPlayerEntry>(pm);

            return entry != null && entry.Guild == g && entry.Resigning;
        }

        public void OnResign(Mobile m, bool quitguild = false)
        {
            VvVPlayerEntry entry = GetPlayerEntry<VvVPlayerEntry>(m as PlayerMobile);

            if (entry != null && entry.Active && !entry.Resigning)
            {
                if (m.AccessLevel <= AccessLevel.VIP)
                    entry.ResignExpiration = DateTime.UtcNow + TimeSpan.FromDays(3);
                else
                    entry.ResignExpiration = DateTime.UtcNow + TimeSpan.FromMinutes(1);

                if (quitguild)
                    m.SendLocalizedMessage("Voce saiu da guerra infinita, mas podera ser atacado ainda por um tempo."); // You have quit a guild while participating in Vice vs Virtue.  You will be freely attackable by members of Vice vs Virtue until your resignation period has ended!
            }
        }

        public void CheckResignation(PlayerMobile pm)
        {
            VvVPlayerEntry entry = GetPlayerEntry<VvVPlayerEntry>(pm);

            if (entry != null && entry.Resigning && entry.ResignExpiration < DateTime.UtcNow)
            {
                pm.PrivateOverheadMessage(MessageType.Regular, 1154, false, "Voce nao faz mais parte da guerra infinita", pm.NetState); // You are no longer in Vice vs Virtue!

                entry.Active = false;
                entry.ResignExpiration = DateTime.MinValue;
                pm.Delta(MobileDelta.Noto);
                pm.ValidateEquipment();
            }
        }

        public void CheckBattleStatus(bool force = false)
        {
            Shard.Debug("Check Status Global");
            if (Battle.OnGoing)
                return;

            int count = EnemyGuildCount();

            if (count > 0 || force)
            {
                Battle.Begin();
            }
        }

        public void CheckBattleStatus(PlayerMobile pm)
        {
            Shard.Debug("Battle Status Check");

            if (!IsVvV(pm) || !Enabled)
            {
                return;
            }

            if (Battle.OnGoing)
            {
                SendVvVMessageTo(pm, "A batalha se inicia em " + Battle.City.ToString());
                // A Battle between Vice and Virtue is active! To Arms! The City of ~1_CITY~ is besieged!

                if (Battle != null && Battle.IsInActiveBattle(pm))
                {
                    Battle.CheckGump(pm);
                    Battle.CheckArrow(pm);
                }
            }
            else
            {
                int count = EnemyGuildCount();

                /*
                if (count < 1)
                {
                    SendVvVMessageTo(pm, "Para iniciar a guerra infinita precisa-se de mais jogadores"); // More players are needed before a VvV battle can begin! 
                }
                else if (Battle.InCooldown)
                {
                    SendVvVMessageTo(pm, "A guerra infinita acontecera as 21:00h"); // A VvV battle has just concluded. The next battle will begin in less than five minutes!
                }
                else
                {
                    Battle.Begin();
                }
                */
            }
        }

        public int EnemyGuildCount()
        {
            List<Guild> guilds = new List<Guild>();

            foreach (NetState ns in NetState.Instances)
            {
                Mobile m = ns.Mobile;

                if (m != null)
                {
                    Guild g = m.Guild as Guild;
                    VvVPlayerEntry entry = GetPlayerEntry<VvVPlayerEntry>(m);

                    if (g != null && entry != null && entry.Guild == g && !guilds.Contains(g))
                    {
                        bool hasally = false;

                        foreach (Guild guild in guilds)
                        {
                            if (guild.IsAlly(g))
                            {
                                hasally = true;
                                break;
                            }
                        }

                        if (!hasally)
                            guilds.Add(g);
                    }
                }
            }

            int count = guilds.Count;
            guilds.Clear();
            guilds.TrimExcess();

            return count - 1;
        }

        public void SendVvVMessage(string message)
        {
            foreach (NetState state in NetState.Instances.Where(st => st.Mobile != null && IsVvV(st.Mobile)))
            {
                Mobile m = state.Mobile;

                if (m != null)
                {
                    m.SendMessage("[Guild][GI] {0}", message);
                }
            }
        }

        public void SendVvVMessage(int cliloc, string args = "")
        {
            foreach (NetState state in NetState.Instances.Where(st => st.Mobile != null && IsVvV(st.Mobile)))
            {
                Mobile m = state.Mobile;

                if (m != null)
                {
                    SendVvVMessageTo(m, cliloc, args);
                }
            }
        }

        public void SendVvVMessageTo(Mobile m, int cliloc, string args = "")
        {
            m.SendLocalizedMessage(cliloc, false, "[Guild][GI] ", args, m is PlayerMobile ? ((PlayerMobile)m).GuildMessageHue : 0x34);
        }

        public void SendVvVMessageTo(Mobile m, string cliloc, string args = "")
        {
            m.SendMessage(m is PlayerMobile ? ((PlayerMobile)m).GuildMessageHue : 0x34, "[Guild][GI] " + cliloc);
        }

        private List<Item> VvVItems = new List<Item>();

        public void AddVvVItem(Item item, bool initial = false)
        {
            if (ViceVsVirtueSystem.Enabled && item is IVvVItem)
            {
                ((IVvVItem)item).IsVvVItem = true;

                if (!VvVItems.Contains(item))
                {
                    VvVItems.Add(item);
                }

                if (initial)
                {
                    FactionEquipment.CheckProperties(item);
                }
            }
        }

        public static void Initialize()
        {
            if (!Enabled)
                return;

            EventSink.Login += OnLogin;
            EventSink.PlayerDeath += OnPlayerDeath;

            Server.Commands.CommandSystem.Register("BattleProps", AccessLevel.GameMaster, e =>
                {
                    if (Instance.Battle != null)
                        e.Mobile.SendGump(new PropertiesGump(e.Mobile, Instance.Battle));
                });

            Server.Commands.CommandSystem.Register("ForceStartBattle", AccessLevel.GameMaster, e =>
            {
                if (Instance.Battle != null && !Instance.Battle.OnGoing)
                    Instance.Battle.Begin();
            });

            Server.Commands.CommandSystem.Register("ExemptCities", AccessLevel.Administrator, e =>
            {
                e.Mobile.SendGump(new ExemptCitiesGump());
            });

            Server.Commands.CommandSystem.Register("VvVKick", AccessLevel.GameMaster, e =>
            {
                e.Mobile.SendMessage("Target the person you'd like to remove from VvV.");
                e.Mobile.BeginTarget(-1, false, Server.Targeting.TargetFlags.None, (from, targeted) =>
                    {
                        if (targeted is PlayerMobile)
                        {
                            var pm = targeted as PlayerMobile;
                            VvVPlayerEntry entry = Instance.GetPlayerEntry<VvVPlayerEntry>(pm);

                            if (entry != null && entry.Active)
                            {
                                pm.PrivateOverheadMessage(MessageType.Regular, 1154, 1155561, pm.NetState); // You are no longer in Vice vs Virtue!

                                entry.Active = false;
                                entry.ResignExpiration = DateTime.MinValue;
                                pm.Delta(MobileDelta.Noto);
                                pm.ValidateEquipment();

                                from.SendMessage("{0} has been removed from VvV.", pm.Name);
                                pm.SendMessage("Voce foi removido da guerra infinita.");
                            }
                            else
                            {
                                from.SendMessage("{0} is not an active VvV member.", pm.Name);
                            }
                        }
                    });
            });

            if (!Instance.HasGenerated)
            {
                CreateSilverTraders();
            }
        }

        public static void OnLogin(LoginEventArgs e)
        {
            if (!Enabled)
                return;

            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null && Instance != null)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(1), Instance.CheckResignation, pm);
                Timer.DelayCall(TimeSpan.FromSeconds(2), Instance.CheckBattleStatus, pm);

                if (EnhancedRules && ShowNewRules != null && ShowNewRules.Contains(pm))
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(1), player =>
                        {
                            player.SendGump(new BasicInfoGump(_EnhancedRulesNotice));

                            ShowNewRules.Remove(player);

                            if (ShowNewRules.Count == 0)
                                ShowNewRules = null;
                        }, pm);
                }
            }
        }

        public static void OnPlayerDeath(PlayerDeathEventArgs e)
        {
            if (!Enabled)
                return;

            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null && Instance != null)
            {
                Instance.HandlePlayerDeath(pm);
            }
        }

        public static bool IsVvV(Mobile m, bool checkpet = true, bool guildedonly = true)
        {
            if (!Enabled)
                return false;

            if (m is BaseCreature && checkpet)
            {
                if (((BaseCreature)m).GetMaster() is PlayerMobile)
                    m = ((BaseCreature)m).GetMaster();
            }

            if (m.IsYoung() || m.Guild == null)
                return false;

            if (m.IsCooldown("gi") && !(m.Region is GuardedRegion))
                return true;

            VvVPlayerEntry entry = Instance.GetPlayerEntry<VvVPlayerEntry>(m as PlayerMobile);

            var cidade = ViceVsVirtueSystem.Instance.Battle.City.ToString();
            if (ViceVsVirtueSystem.Instance.Battle.City == VvVCity.SkaraBrae)
                cidade = "Skara Brae";
            var inRegion = m.Region.IsPartOf(cidade);

            /*
            if (ViceVsVirtueSystem.Instance.Battle.OnGoing && m.Player)
            {
             
                if (inRegion)
                {
                    //if(entry == null || !entry.Active)
                    //{
                    //    Instance.AddPlayer(m as PlayerMobile);
                    //}
                    //return true;
                }
            }
            */
            if (entry == null || !inRegion)
                return false;

            return entry.Active && entry.Guild != null;
        }

        public static string GetRegionName(VvVCity city)
        {
            if (city == VvVCity.SkaraBrae)
                return "Skara Brae";
            return city.ToString();
        }

        public static HashSet<BaseGuild> Entraram = new HashSet<BaseGuild>();

        public static void OnRegionChange(PlayerMobile pl, Region Old, Region New)
        {
            if (ViceVsVirtueSystem.Instance != null && ViceVsVirtueSystem.Enabled)
            {
                var oldVVV = Old.IsPartOf(GetRegionName(ViceVsVirtueSystem.Instance.Battle.City));
                var newVVV = New.IsPartOf(GetRegionName(ViceVsVirtueSystem.Instance.Battle.City));
                if (oldVVV && !newVVV)
                {
                    pl.Delta(MobileDelta.Noto);
                    if (ViceVsVirtueSystem.Instance.Battle.OnGoing)
                        pl.SetCooldown("gi", TimeSpan.FromMinutes(10));
                    pl.SendMessage("Voce saiu da regiao da guerra infinita");
                    if (Shard.DebugEnabled)
                        Shard.Debug("Temp ?" + (GetTemporario(pl) != null) + " VVV ? " + IsVvV(pl));

                    foreach (var p in pl.FindPlayersInRange(pl.Map, 20))
                        p.Delta(MobileDelta.Noto);
                }
                else if (!oldVVV && newVVV)
                {
                    if (ViceVsVirtueSystem.Instance.Battle.OnGoing)
                        pl.SetCooldown("gi", TimeSpan.FromMinutes(10));
                    pl.Delta(MobileDelta.Noto);
                    pl.SendMessage("Voce entrou na regiao da guerra infinita ");
                    Shard.Debug("Temp ?" + (GetTemporario(pl) != null) + " VVV ? " + IsVvV(pl));
                    foreach (var p in pl.FindPlayersInRange(pl.Map, 20))
                        p.Delta(MobileDelta.Noto);

                    if (pl.Guild != null && ViceVsVirtueSystem.Instance.Battle.OnGoing)
                    {
                        if (!Entraram.Contains(pl.Guild))
                        {
                            Entraram.Add(pl.Guild);
                            Anuncio.Anuncia("A guilda " + pl.Guild.Abbreviation + " entrou na batalha");
                        }
                    }
                }
            }
        }

        public static bool IsVvV(Mobile m, out VvVPlayerEntry entry, bool checkpet = true, bool guildedonly = false)
        {
            if (!Enabled)
            {
                entry = null;
                return false;
            }

            if (m is BaseCreature && checkpet)
            {
                if (((BaseCreature)m).GetMaster() is PlayerMobile)
                    m = ((BaseCreature)m).GetMaster();
            }



            entry = Instance.GetPlayerEntry<VvVPlayerEntry>(m as PlayerMobile);

            if (entry != null && !entry.Active)
                entry = null;

            return entry != null && entry.Active && (!guildedonly || entry.Guild != null);
        }

        public static bool IsEnemy(IDamageable from, IDamageable to)
        {
            if (from is Mobile && to is Mobile)
                return IsEnemy((Mobile)to, (Mobile)from);

            //TODO: VvV items, such as traps, turrets, etc
            return false;
        }

        public static bool IsEnemy(Mobile from, Mobile to)
        {

            if (!Enabled || from == to)
                return false;

            if (from is BaseCreature && ((BaseCreature)from).GetMaster() is PlayerMobile)
                from = ((BaseCreature)from).GetMaster();

            if (to is BaseCreature && ((BaseCreature)to).GetMaster() is PlayerMobile)
                to = ((BaseCreature)to).GetMaster();

            // one or the other is not a combatant
            if (!IsVvVCombatant(to) || !IsVvVCombatant(from))
                return false;

            return !IsAllied(from, to);
        }

        public static bool IsAllied(Mobile a, Mobile b)
        {
            var guildA = a.Guild as Guild;
            var guildB = b.Guild as Guild;

            if (guildA != null && guildB != null && (guildA == guildB || guildA.IsAlly(guildB)))
            {
                return true;
            }

            if (TempCombatants == null)
            {
                return false;
            }

            var tempA = TempCombatants.FirstOrDefault(c => c.From == a);
            var tempB = TempCombatants.FirstOrDefault(c => c.From == b);

            if (tempA != null && (tempA.Friendly == b || (tempA.FriendlyGuild != null && tempA.FriendlyGuild == guildB)))
            {
                return true;
            }

            if (tempB != null && (tempB.Friendly == a || (tempB.FriendlyGuild != null && tempB.FriendlyGuild == guildA)))
            {
                return true;
            }

            return false;
        }

        public static bool IsVvVCombatant(Mobile mobile)
        {
            CheckTempCombatants();

            return IsVvV(mobile) || (TempCombatants != null && TempCombatants.Any(c => c.From == mobile));
        }

        public static void CheckHarmful(Mobile attacker, Mobile defender)
        {
            CheckTempCombatants();

            if (attacker == null || defender == null || IsAllied(attacker, defender))
                return;

            if (!IsVvV(attacker) && IsVvV(defender) && !defender.Aggressed.Any(info => info.Defender == attacker))
            {
                AddTempParticipant(attacker, null);
            }
        }

        public static void CheckBeneficial(Mobile from, Mobile target)
        {
            CheckTempCombatants();

            if (from == null || target == null || (IsVvV(from) && IsAllied(from, target)))
                return;

            if (!IsVvV(from) && IsVvV(target))
            {
                if (target.Aggressors.Any(info => IsVvV(info.Attacker)) ||
                    target.Aggressed.Any(info => IsVvV(info.Defender)))
                {
                    AddTempParticipant(from, target);
                }
            }
        }

        public static Timer TempCombatantTimer { get; private set; }

        public static void AddTempCombatantTimer()
        {
            if (TempCombatantTimer == null)
            {
                TempCombatantTimer = Timer.DelayCall(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), CheckTempCombatants);
                TempCombatantTimer.Start();
            }
        }

        public static void StopTempCombatantTimer()
        {
            if (TempCombatantTimer != null)
            {
                TempCombatantTimer.Stop();
                TempCombatantTimer = null;
            }
        }

        public static void CheckTempCombatants()
        {
            if (TempCombatants == null)
            {
                StopTempCombatantTimer();
            }

            TempCombatants.IterateReverse(c =>
                {
                    if (c.Expired)
                    {
                        TempCombatants.Remove(c);
                    }
                });
        }

        public static TemporaryCombatant GetTemporario(Mobile from)
        {
            if (from == null || TempCombatants == null)
                return null;
            return TempCombatants.FirstOrDefault(c => c?.From == from);
        }

        public static TemporaryCombatant GetTempCombatant(Mobile from, Mobile to)
        {
            foreach (var combatant in TempCombatants.Where(c => c.From == from))
            {
                if (combatant.Friendly == null && to == null)
                    return combatant;

                if (combatant.Friendly == to || (combatant.FriendlyGuild != null && combatant.FriendlyGuild == from.Guild as Guild))
                    return combatant;
            }

            return null;
        }

        public static void AddTempParticipant(Mobile m, Mobile friendlyTo)
        {
            if (TempCombatants == null)
            {
                TempCombatants = new List<TemporaryCombatant>();
                AddTempCombatantTimer();
            }

            var combatant = GetTempCombatant(m, friendlyTo);

            if (combatant == null)
            {
                combatant = new TemporaryCombatant(m, friendlyTo);
            }
            else
            {
                combatant.Reset();
            }

            TempCombatants.Add(combatant);

            m.Delta(MobileDelta.Noto);
            m.ProcessDelta();
        }

        public static void OnMapChange(PlayerMobile pm)
        {
            if (TempCombatants == null || pm.Map == Map.Internal || pm.Map == null)
                return;

            TempCombatants.Where(t => t.From == pm).IterateReverse(temp =>
                {
                    RemoveTempCombatant(temp);
                });
        }

        public static void RemoveTempCombatant(TemporaryCombatant tempCombatant)
        {
            if (TempCombatants == null)
                return;

            TempCombatants.Remove(tempCombatant);
            tempCombatant.From.Delta(MobileDelta.Noto);
            tempCombatant.From.ProcessDelta();

            if (TempCombatants.Count == 0)
            {
                TempCombatants = null;
                StopTempCombatantTimer();
            }
        }

        public static bool HasBattleAggression(Mobile m)
        {
            if (!EnhancedRules || Instance == null || Instance.Battle == null || !Instance.Battle.OnGoing)
                return false;

            return Instance.Battle.HasBattleAggression(m);
        }

        public static bool IsBattleRegion(Region r)
        {
            if (r == null || Instance == null)
            {
                return false;
            }

            return Instance.Battle.OnGoing && r.IsPartOf(Instance.Battle.Region);
        }

        public static int GetCityLocalization(City city)
        {
            switch (city)
            {
                default: return 0;
                case City.Moonglow: return 1011344;
                case City.Britain: return 1011028;
                case City.Jhelom: return 1011343;
                case City.Yew: return 1011032;
                case City.Minoc: return 1011031;
                case City.Trinsic: return 1011029;
                case City.SkaraBrae: return 1011347;
                case City.NewMagincia: return 1011345;
                case City.Vesper: return 1011030;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(4);

            writer.Write(ShowNewRules == null ? 0 : ShowNewRules.Count);
            if (ShowNewRules != null)
            {
                foreach (var pm in ShowNewRules)
                    writer.Write(pm);
            }

            writer.Write(EnhancedRules);
            writer.Write(Enabled);

            writer.Write(ExemptCities.Count);
            ExemptCities.ForEach(c => writer.Write((int)c));

            writer.Write(HasGenerated);
            Battle.Serialize(writer);

            writer.Write(VvVItems.Count);
            foreach (Item item in VvVItems)
            {
                writer.Write(item);
            }

            writer.Write(GuildStats.Count);
            foreach (KeyValuePair<Guild, VvVGuildStats> kvp in GuildStats)
            {
                writer.Write(kvp.Key);
                kvp.Value.Serialize(writer);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            GuildStats = new Dictionary<Guild, VvVGuildStats>();
            ExemptCities = new List<VvVCity>();

            bool enabled = false;
            bool enhanced = false;

            switch (version)
            {
                case 4:
                    int c = reader.ReadInt();
                    for (int i = 0; i < c; i++)
                    {
                        var pm = reader.ReadMobile() as PlayerMobile;

                        if (pm != null)
                        {
                            if (ShowNewRules == null)
                            {
                                ShowNewRules = new List<PlayerMobile>();
                            }

                            ShowNewRules.Add(pm);
                        }
                    }

                    enhanced = reader.ReadBool();
                    goto case 3;
                case 3:
                    enabled = reader.ReadBool();
                    goto case 2;
                case 2:
                case 1:
                    {
                        int count = reader.ReadInt();
                        for (int i = 0; i < count; i++)
                        {
                            ExemptCities.Add((VvVCity)reader.ReadInt());
                        }

                        goto case 0;
                    }
                case 0:
                    {
                        HasGenerated = reader.ReadBool();

                        Battle = new VvVBattle(reader, this);

                        int count = reader.ReadInt();
                        for (int i = 0; i < count; i++)
                        {
                            Item item = reader.ReadItem();

                            if (item != null)
                                AddVvVItem(item);
                        }

                        count = reader.ReadInt();
                        for (int i = 0; i < count; i++)
                        {
                            Guild g = reader.ReadGuild() as Guild;
                            VvVGuildStats stats = new VvVGuildStats(g, reader);

                            if (g != null)
                                GuildStats[g] = stats;
                        }
                    }
                    break;
            }

            if (version == 1)
                Timer.DelayCall(FixVvVItems);

            if (Enabled && !enabled)
            {
                Timer.DelayCall(() =>
                    {
                        Server.Factions.Generator.RemoveFactions();
                        CreateSilverTraders();
                    });
            }
            else if (!Enabled && enabled)
            {
                DeleteSilverTraders();
            }

            if (EnhancedRules && !enhanced)
            {
                OnEnhancedRulesEnabled();
            }
        }

        public void FixVvVItems()
        {
            foreach (var item in VvVItems.Where(i => i is Spellbook))
            {
                var book = item as Spellbook;
                var attrs = RunicReforging.GetNegativeAttributes(item);

                if (attrs != null)
                {
                    attrs.Antique = 0;
                }

                book.MaxHitPoints = 0;
                book.HitPoints = 0;
            }
        }

        public static void CreateSilverTraders()
        {
            if (!Enabled)
                return;

            Map map = Map.Trammel;

            foreach (CityInfo info in CityInfo.Infos.Values)
            {
                IPooledEnumerable eable = map.GetMobilesInRange(info.TraderLoc, 3);
                bool found = false;

                foreach (Mobile m in eable)
                {
                    if (m is SilverTrader)
                    {
                        found = true;
                        break;
                    }
                }

                eable.Free();

                if (!found)
                {
                    SilverTrader trader = new SilverTrader();
                    trader.MoveToWorld(info.TraderLoc, map);
                }
            }
        }

        public static void DeleteSilverTraders()
        {
            var list = new List<Mobile>(World.Mobiles.Values.Where(m => m is SilverTrader));

            foreach (var mob in list)
            {
                mob.Delete();
            }

            ColUtility.Free(list);
        }

        public static List<PlayerMobile> ShowNewRules { get; private set; }

        private static void OnEnhancedRulesEnabled()
        {
            if (Instance == null || !Enabled)
                return;

            Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
                {
                    foreach (var pm in World.Mobiles.Values.OfType<PlayerMobile>().Where(pm => IsVvV(pm)))
                    {
                        VvVPlayerEntry entry = Instance.GetPlayerEntry<VvVPlayerEntry>(pm);

                        if (entry != null)
                        {
                            if (ShowNewRules == null)
                                ShowNewRules = new List<PlayerMobile>();

                            ShowNewRules.Add(pm);
                        }
                    }
                });
        }

        private static string _EnhancedRulesNotice = String.Format("Notice: The Vice Vs Virtue system has recently enabled enhanced rules. To avoid any issues and " +
             "unexpected deaths due to the new game mechanics, it is important that you read this message. " +
             "<br><br>New VvV Mechanics:<br><br>" +
             "- VvV combatants are attackable on all facets.<br>" +
             "- Uncontested VvV battles will reduce reduce reward silver by {0}%.<br>" +
             "- VvV players in the battle region during a battle will be subject to combat heat travel restrictions.", VvVBattle.Penalty * 100);

        public static bool RestrictSilver(Mobile a, Mobile b)
        {
            Account accountA = a.Account as Account;
            Account accountB = b.Account as Account;

            return accountA != null && (accountA == accountB || PVPArenaSystem.IsSameIP(a, b));
        }
    }

    public class VvVPlayerEntry : PointsEntry
    {
        private bool _Active;

        public bool OneTimePointsRetention { get; set; }

        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }

        public EnemyKilledEntry KilledEntry { get; set; }

        public int Score { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int ReturnedSigils { get; set; }
        public int DisarmedTraps { get; set; }
        public int StolenSigils { get; set; }

        public Guild Guild
        {
            get
            {
                return Player != null ? Player.Guild as Guild : null;
            }
        }

        public bool Active
        {
            get
            {
                return _Active;
            }
            set
            {
                if (!_Active && value)
                {
                    if (OneTimePointsRetention)
                    {
                        OneTimePointsRetention = false;
                    }
                    else
                    {
                        Points = 0;
                    }
                }

                _Active = value;
            }
        }

        public DateTime ResignExpiration { get; set; }
        public bool Resigning { get { return ResignExpiration > DateTime.MinValue; } }

        public VvVPlayerEntry(PlayerMobile pm)
            : base(pm)
        {
            _Active = true;
            Points = ViceVsVirtueSystem.StartSilver;
        }

        public void AwardSilver(Mobile victim)
        {
            if (!ViceVsVirtueSystem.RestrictSilver(Player, victim))
            {
                Player.SendMessage("Voce nao pode ganhar pratinha matando {0}!", victim.Name);
                return;
            }

            var entry = KilledEntry;

            if (entry == null)
            {
                KilledEntry = entry = new EnemyKilledEntry(victim);
            }
            else
            {
                if (entry.Expired)
                {
                    entry.TimesKilled = 1;
                }
                else
                {
                    entry.TimesKilled++;
                }
            }

            if (entry.TimesKilled > EnemyKilledEntry.MaxKillsForSilver)
            {
                Player.SendMessage("Voce nao pode ganhar mais pratinhas matando {0}.", victim.Name);
            }

            int silver = (int)((double)EnemyKilledEntry.KillSilver / (double)entry.TimesKilled);

            if (silver > 0)
            {
                Player.SendLocalizedMessage(1042736, String.Format("{0:N0} silver\t{1}", silver, victim.Name));
                // You have earned ~1_SILVER_AMOUNT~ pieces for vanquishing ~2_PLAYER_NAME~!

                Points += silver;
            }
        }

        public class EnemyKilledEntry
        {
            public static int KillSilver = 40;
            public static int MaxKillsForSilver = 3;
            public static TimeSpan ExpireTime = TimeSpan.FromHours(3);

            public Mobile Killed { get; set; }
            public int TimesKilled { get; set; }
            public DateTime Expires { get; set; }

            public bool Expired { get { return Expires < DateTime.UtcNow; } }

            public EnemyKilledEntry(Mobile killed)
            {
                Killed = killed;
                TimesKilled = 1;
                Expires = DateTime.UtcNow + ExpireTime;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(4);

            writer.Write(OneTimePointsRetention);

            writer.Write(TotalDeaths);
            writer.Write(TotalKills);

            writer.Write(Active);

            writer.Write(Score);
            writer.Write(Kills);
            writer.Write(Deaths);
            writer.Write(Assists);
            writer.Write(ReturnedSigils);
            writer.Write(DisarmedTraps);
            writer.Write(StolenSigils);
            writer.Write(ResignExpiration);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 4:
                    OneTimePointsRetention = reader.ReadBool();
                    goto case 3;
                case 3:
                    TotalDeaths = reader.ReadInt();
                    TotalKills = reader.ReadInt();
                    goto case 2;
                case 2:
                    Active = reader.ReadBool();

                    if (version == 0)
                        reader.ReadBool();

                    if (version < 2)
                        reader.ReadGuild();

                    Score = reader.ReadInt();
                    Kills = reader.ReadInt();
                    Deaths = reader.ReadInt();
                    Assists = reader.ReadInt();
                    ReturnedSigils = reader.ReadInt();
                    DisarmedTraps = reader.ReadInt();
                    StolenSigils = reader.ReadInt();
                    ResignExpiration = reader.ReadDateTime();
                    break;
            }

            if (version == 3)
            {
                OneTimePointsRetention = true;
            }
        }
    }

    public class TemporaryCombatant
    {
        public static TimeSpan TempCombatTime = TimeSpan.FromMinutes(10);

        public Mobile From { get; private set; }
        public Mobile Friendly { get; private set; }
        public DateTime StartTime { get; private set; }

        public Guild FriendlyGuild
        {
            get
            {
                if (Friendly == null)
                {
                    return null;
                }

                return Friendly.Guild as Guild;
            }
        }

        public bool Expired { get { return StartTime + TempCombatTime < DateTime.UtcNow; } }

        public TemporaryCombatant(Mobile from, Mobile friendlyTo)
        {
            From = from;
            Friendly = friendlyTo;
            StartTime = DateTime.UtcNow;
        }

        public void Reset()
        {
            StartTime = DateTime.UtcNow;
        }
    }
}
