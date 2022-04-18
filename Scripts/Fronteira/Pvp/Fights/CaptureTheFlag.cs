using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;

namespace Server.TournamentSystem
{
    public class CaptureTheFlagFight : ArenaFight
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public override ArenaFightType ArenaFightType { get { return ArenaFightType.CaptureTheFlag; } }

        private int m_CTFTick;
        private bool m_DoneHalf;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextFlagCheck { get; set; }

        public CTFFlag[] Flags { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public CTFArena CTFSystem { get { return System as CTFArena; } }

        public CaptureTheFlagFight(PVPTournamentSystem system, Tournament tourney)
            : base(system, tourney)
        {
            AddToForcedRules(FightRules.AllowResurrections);
        }

        public override void OnFightWon(ArenaTeam winner)
        {
            winner.CTFWins++;
        }

        protected override void OnBeforeFight()
        {
            base.OnBeforeFight();

            if (CTFSystem != null)
            {
                CTFSystem.SetupFlags(this);
            }
        }

        public override void DoPrefightSetup()
        {
            NextFlagCheck = DateTime.UtcNow + TimeSpan.FromSeconds(60);

            foreach (var info in Teams)
            {
                var team = info.Team;

                foreach (var fighter in team.Fighters)
                {
                    fighter.Frozen = true;
                    fighter.Hidden = true;
                    fighter.SendMessage(ArenaHelper.ParticipantMessageHue, "You cannot move until the game begins!");
                    MoveToStartSpot(fighter, System.RandomStartLocation(team), 0);
                }
            }
        }

        public override void OnBeginFight()
        {
            DoStartMessage();

            foreach (var fighter in GetFighters())
            {
                fighter.Frozen = false;
                fighter.Hidden = false;
            }

            if (CTFSystem == null || CTFSystem.FlagHolders.Where(h => h != null).Count() != Flags.Where(f => f != null).Count())
            {
                CancelFight(CancelReason.SystemError);
                return;
            }
        }

        public override void OnTick()
        {
            bool invalidate = false;

            if (NextFlagCheck < DateTime.UtcNow)
            {
                for (int i = 0; i < CTFSystem.FlagHolders.Count; i++)
                {
                    var holder = CTFSystem.FlagHolders[i];

                    if (holder != null && holder.EnemyFlags.Any(flag => flag != null))
                    {
                        var points = holder.GetRoundPoints();

                        if (points > 0)
                        {
                            var info = GetTeamInfo(holder.Owner);
                            var enemies = holder.GetEnemyLosers().ToArray();

                            if (info != null)
                            {
                                info.Points += points;
                                DoCTFTeamAccouncement(info.Team, enemies, points, info.Points);

                                if (!invalidate)
                                {
                                    invalidate = true;
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < Flags.Length; i++)
                {
                    var flag = Flags[i];

                    if (flag != null && flag.Holder == null && flag.LastTaken != DateTime.MinValue && flag.LastTaken + TimeSpan.FromMinutes(3) <= DateTime.UtcNow)
                    {
                        flag.ReturnToLast();
                    }
                }

                if (DoCTFRoundAnnouncement(m_CTFTick, m_DoneHalf))
                {
                    m_DoneHalf = true;
                }

                NextFlagCheck = DateTime.UtcNow + TimeSpan.FromSeconds(60);
                m_CTFTick++;
            }

            if (invalidate)
            {
                if (System.Stone != null)
                {
                    System.Stone.InvalidateGumps();
                }

                RefreshGumps(true);
            }
            else
            {
                RefreshGumps();
            }

            if (EndTime < DateTime.UtcNow)
            {
                CheckCTFWinner();
                EndTimer();
                return;
            }
            else if (DateTime.UtcNow + TimeSpan.FromMinutes(1) >= EndTime && !Warning)
            {
                DoWarning();
                Warning = true;
            }

            System.OnTickFight();
        }

        private void CheckCTFWinner()
        {
            List<TeamInfo> list = Teams.
                OrderByDescending(info => info.Points).
                ThenByDescending(info => info.Kills).
                ThenByDescending(info => GetTotalDamageGiven(info.Team)).
                ThenByDescending(info => info.Team.Fighters.Where(f => f.Alive).Count()).ToList();

            EndFight(list[0].Team, list[1].Team);

            ResetFlags();
        }

        public void ResetFlags()
        {
            if (Flags != null)
            {
                for (int i = 0; i < Flags.Length; i++)
                {
                    Flags[i].Delete();
                }
            }

            if (CTFSystem != null)
            {
                for (int i = 0; i < CTFSystem.FlagHolders.Count; i++)
                {
                    CTFSystem.FlagHolders[i].Reset();
                }
            }
        }

        protected virtual bool DoCTFRoundAnnouncement(int ticks, bool donehalf)
        {
            var teams = Teams.OrderByDescending(info => info.Points).ToArray();

            if (teams.Length >= 2 && FightDuration.TotalMinutes / 2 == ticks && !donehalf)
            {
                string message;
                var winningBy = Math.Abs(teams[0].Points - teams[1].Points);

                var team0 = teams[0].Points;
                var team1 = teams[1].Points;
                var team2 = teams.Length > 2 ? teams[2].Points : -1;
                var team3 = teams.Length > 3 ? teams[3].Points : -1;

                if (team0 > team1)
                {
                    message = String.Format("{0} is leading at Capture the Flag by {1} point{2}!", teams[0].Team.Name, winningBy, winningBy == 1 ? "" : "s");
                }
                else if (team1 > team0)
                {
                    message = String.Format("{0} is leading at Capture the Flag by {1} point{2}!", teams[1].Team.Name, winningBy, winningBy == 1 ? "" : "s");
                }
                else
                {
                    if (team2 > -1 && team3 > -1)
                    {
                        if (team1 > team2)
                        {
                            message = String.Format("Teams {0} and {1} are currently tied at Capture the Flag!", teams[0].Team.Name, teams[1].Team.Name);
                        }
                        else if (team1 == team2 && team2 > team3)
                        {
                            message = String.Format("Teams {0}, {1} and {2} are currently tied at Capture the Flag!", teams[0].Team.Name, teams[1].Team.Name, teams[2].Team.Name);
                        }
                        else
                        {
                            message = "There is currently a four way tie at Capture the Flag!";
                        }
                    }
                    else
                    {
                        message = String.Format("Teams {0} and {1} are currently tied at Capture the Flag!", teams[0].Team.Name, teams[1].Team.Name);
                    }
                }

                foreach (var pm in GetFighters())
                {
                    pm.SendMessage(ArenaHelper.AudienceMessageHue, message);
                }

                ArenaHelper.DoAudienceRegionMessage(message, 0, System);

                return true;
            }

            return false;
        }

        protected virtual void DoCTFTeamAccouncement(ArenaTeam team, ArenaTeam[] captured, int earned, int total)
        {
            foreach (var fighter in GetFighters())
            {
                if (team.IsInTeam(fighter))
                {
                    fighter.SendMessage(ArenaHelper.ParticipantMessageHue, "Your team has earned {0} ({1} total)!", earned == 1 ? "1 point" : string.Format("{0} points", earned.ToString()), total.ToString());
                }
                else if (captured.Any(t => t == GetTeam(fighter)))
                {
                    fighter.SendMessage(ArenaHelper.ParticipantMessageHue, "{0} has earned a point for defending your captured flag!", team.Name);
                }
                else if (captured.Length == 1)
                {
                    fighter.SendMessage(ArenaHelper.ParticipantMessageHue, "{0} has earned a point for defending {1}'s captured flag!", team.Name, captured[0].Name);
                }
                else
                {
                    fighter.SendMessage(ArenaHelper.ParticipantMessageHue, "{0} has earned a point for defending multiple teams captured flag!", team.Name, captured[0].Name);
                }
            }
        }
    }
}
