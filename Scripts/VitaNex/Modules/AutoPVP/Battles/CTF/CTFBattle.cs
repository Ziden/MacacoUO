#region Header
//   Vorspire    _,-'/-'/  CTFBattle.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Linq;
using Fronteira.Discord;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Ziden.Achievements;
using VitaNex.Schedules;
#endregion

namespace VitaNex.Modules.AutoPvP.Battles
{
    public class CTFBattle : PvPBattle
    {
        [CommandProperty(AutoPvP.Access)]
        public virtual double FlagDamageInc { get; set; }

        [CommandProperty(AutoPvP.Access)]
        public virtual double FlagDamageIncMax { get; set; }

        [CommandProperty(AutoPvP.Access)]
        public virtual int FlagCapturePoints { get; set; }

        [CommandProperty(AutoPvP.Access)]
        public virtual int FlagReturnPoints { get; set; }

        public CTFBattle()
        {
            Name = "Pique Bandeira";
            Category = "Pique Bandeira";
            Description = "Capture a bandeira inimiga e traga ela a sua base.";

            AddTeam("Vermelho", 1, 5, 0x22);
            AddTeam("Azul", 1, 5, 0x55);

            Options.Missions.Enabled = true;
            Options.Missions.Team = new CTFBattleObjectives
            {
                FlagsCaptured = 5
            };

            Schedule.Info.Months = ScheduleMonths.All;
            Schedule.Info.Days = ScheduleDays.All;
            Schedule.Info.Times = ScheduleTimes.EveryQuarterHour;

            Options.Timing.QueuePeriod = TimeSpan.FromMinutes(5.0);
            Options.Timing.PreparePeriod = TimeSpan.FromMinutes(5.0);
            Options.Timing.RunningPeriod = TimeSpan.FromMinutes(15.0);
            Options.Timing.EndedPeriod = TimeSpan.FromMinutes(5.0);

            Options.Rules.AllowBeneficial = true;
            Options.Rules.AllowHarmful = true;
            Options.Rules.AllowHousing = false;
            Options.Rules.AllowPets = false;
            Options.Rules.AllowSpawn = false;
            Options.Rules.AllowSpeech = true;
            Options.Rules.CanBeDamaged = true;
            Options.Rules.CanDamageEnemyTeam = true;
            Options.Rules.CanDamageOwnTeam = false;
            Options.Rules.CanDie = false;
            Options.Rules.CanHeal = true;
            Options.Rules.CanHealEnemyTeam = false;
            Options.Rules.CanHealOwnTeam = true;
            Options.Rules.CanMount = false;
            Options.Rules.CanFly = false;
            Options.Rules.CanResurrect = true;
            Options.Rules.CanUseStuckMenu = false;
            Options.Rules.CanEquip = true;
        }

        public override void GiveWinnerReward(PlayerMobile pm)
        {
            base.GiveWinnerReward(pm);
            var trofeu = new Trofeu();
            trofeu.Hue = 0x8A5;
            var data = DateTime.UtcNow;
            trofeu.Textos = new string[]
            {
                $"Arena {Name}",
                $"{data.Day}/{data.Month}/{data.Year}",
                pm.Female ? "Campea" : "Campeao",
                pm.Name
            };
            trofeu.Name = "[OURO] Trofeu de {Name}";
            trofeu.Hue = Paragon.Hue;
            pm._PlaceInBackpack(trofeu);
        }

        public CTFBattle(GenericReader reader)
            : base(reader)
        { }

        public override bool Validate(Mobile viewer, List<string> errors, bool pop = true)
        {
            if (!base.Validate(viewer, errors, pop) && pop)
            {
                return false;
            }

            if (!Teams.All(t => t is CTFTeam))
            {
                errors.Add("One or more teams are not of the CTFTeam type.");
                errors.Add("[Options] -> [View Teams]");

                if (pop)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool AddTeam(string name, int minCapacity, int capacity, int color)
        {
            return AddTeam(new CTFTeam(this, name, minCapacity, capacity, color));
        }

        public override bool AddTeam(PvPTeam team)
        {
            if (team == null || team.Deleted)
            {
                return false;
            }

            if (team is CTFTeam)
            {
                return base.AddTeam(team);
            }

            var added = AddTeam(team.Name, team.MinCapacity, team.MinCapacity, team.Color);

            team.Delete();

            return added;
        }

        public virtual void OnFlagDropped(CTFFlag flag, PlayerMobile attacker, CTFTeam enemyTeam)
        {
            UpdateStatistics(enemyTeam, attacker, s => ++s["Flags Dropped"]);

            PlaySound(746);

            LocalBroadcast("[{0}]: {1} dropou a bandeira {2}!", enemyTeam.Name, attacker.Name, flag.Team.Name);
        }

        public virtual void OnFlagCaptured(CTFFlag flag, PlayerMobile attacker, CTFTeam enemyTeam)
        {
            UpdateStatistics(enemyTeam, attacker, s => ++s["Flags Captured"]);

            if (FlagCapturePoints > 0)
            {
                AwardPoints(attacker, FlagCapturePoints);
            }

            PlaySound(747);
            enemyTeam.Captures++;
            LocalBroadcast($"[{enemyTeam.Name}]: {attacker.Name} capturou a bandeira {flag.Team.Name} ({enemyTeam.Captures}/5)!");
            if(enemyTeam.Captures >= 5)
            {
                this.State = PvPBattleState.Terminando;
            }
        }

        public virtual void OnFlagStolen(CTFFlag flag, PlayerMobile attacker, CTFTeam enemyTeam)
        {
            UpdateStatistics(enemyTeam, attacker, s => ++s["Flags Stolen"]);

            PlaySound(748);

            LocalBroadcast("[{0}]: {1} roubou a bandeira {2}!", enemyTeam.Name, attacker.Name, flag.Team.Name);
        }

        public virtual void OnFlagReturned(CTFFlag flag, PlayerMobile defender)
        {
            UpdateStatistics(flag.Team, defender, s => ++s["Flags Returned"]);

            if (FlagReturnPoints > 0)
            {
                AwardPoints(defender, FlagReturnPoints);
            }

            PlaySound(749);

            LocalBroadcast("[{0}]: {1} retornou a bandeira {0}!", flag.Team.Name, defender.Name);
        }

        public virtual void OnFlagTimeout(CTFFlag flag)
        {
            PlaySound(749);

            LocalBroadcast("[{0}]: Bandeira retornada !", flag.Team.Name);
        }

        protected override void OnBattlePreparing()
        {
            base.OnBattlePreparing();
            Shard.Debug("Queue Count: " + Queue.Count + " Battle: ");
            if (State == PvPBattleState.Preparando)
            {
                foreach (var t in this.Teams)
                {
                    if (t.Members.Count != t.MaxCapacity)
                        return;
                }

                this.State = PvPBattleState.Batalhando;
            }
        }

        protected override void OnInviteAccept(PlayerMobile pm)
        {
            base.OnInviteAccept(pm);
            if (State == PvPBattleState.Preparando)
            {
                foreach (var t in this.Teams)
                {
                    if (t.Members.Count != t.MaxCapacity)
                        return;
                }

                this.State = PvPBattleState.Batalhando;
            }
        }

        protected override void OnQueueJoin(PlayerMobile pm, PvPTeam team)
        {
            base.OnQueueJoin(pm, team);
            var msg = $"[CTF] {pm.Name} entrou na fila. Use .pvp para participar !";
            foreach (var pl in NetState.GetOnlinePlayerMobiles())
            {
                pl.SendMessage(msg);
            }
            DiscordBot.SendMessage(":crossed_swords:" + msg);

            if (Queue.Count >= this.MaxCapacity)
                this.State = PvPBattleState.Preparando;

        }

        public override bool CheckDamage(Mobile damaged, ref int damage)
        {
            if (!base.CheckDamage(damaged, ref damage))
            {
                return false;
            }

            if (damaged != null && damaged.Player && damaged.Backpack != null && damage > 0)
            {
                var flag = damaged.Backpack.FindItemByType<CTFFlag>();

                if (flag != null)
                {
                    damage += (int)(damage * flag.DamageInc);
                }
            }

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            var version = writer.SetVersion(3);

            switch (version)
            {
                case 3:
                case 2:
                    {
                        writer.Write(FlagDamageInc);
                        writer.Write(FlagDamageIncMax);
                    }
                    goto case 1;
                case 1:
                    {
                        writer.Write(FlagCapturePoints);
                        writer.Write(FlagReturnPoints);
                    }
                    goto case 0;
                case 0:
                    writer.Write(-1); // CapsToWin
                    break;
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            if (!(Options.Missions.Team is CTFBattleObjectives))
            {
                Options.Missions.Enabled = true;

                Options.Missions.Team = new CTFBattleObjectives
                {
                    FlagsCaptured = 5
                };
            }

            var version = reader.ReadInt();

            switch (version)
            {
                case 3:
                case 2:
                    {
                        FlagDamageInc = reader.ReadDouble();
                        FlagDamageIncMax = reader.ReadDouble();
                    }
                    goto case 1;
                case 1:
                    {
                        FlagCapturePoints = reader.ReadInt();
                        FlagReturnPoints = reader.ReadInt();
                    }
                    goto case 0;
                case 0:
                    {
                        var capsToWin = reader.ReadInt();

                        if (capsToWin >= 0)
                        {
                            ((CTFBattleObjectives)Options.Missions.Team).FlagsCaptured = capsToWin;
                        }
                    }
                    break;
            }

            if (version < 3)
            {
                RewardTeam = true;
            }
        }
    }
}
