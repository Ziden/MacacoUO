using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Spells;

namespace Server.TournamentSystem
{
    public class LastManStandingFight : ArenaFight 
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public override ArenaFightType ArenaFightType { get { return ArenaFightType.LastManStanding; } }

        public LastManStandingFight(PVPTournamentSystem system, Tournament tourney)
            : base(system, tourney)
        {
        }

        public override void OnFightWon(ArenaTeam winner)
        {
            winner.LastManStandingWins++;
        }

        public override void ActionTimedOut(Mobile creator)
        {
                if (Teams.Count >= System.MinLastManStanding)
                    RegisterFight();
                else
                    creator.SendMessage("There must be at least {0} teams for a last man standing fight.", System.MinLastManStanding.ToString());
        }

        public override void DoPrefightSetup()
        {
            foreach (var team in Teams.Select(x => x.Team))
            {
                foreach (var fighter in team.Fighters)
                {
                    fighter.Frozen = true;
                    fighter.Hidden = true;
                    fighter.SendMessage(ArenaHelper.ParticipantMessageHue, "You cannot move until the game begins!");
                    MoveToStartSpot(fighter, RandomStartLocation(), 0);
                }
            }
        }

        private Point3D RandomStartLocation()
        {
            Point3D p = Point3D.Zero;

            do
            {
                p = System.ArenaMap.GetRandomSpawnPoint(System.RandomStartBounds);

                SpellHelper.AdjustField(ref p, System.ArenaMap, 16, false);
            }
            while (p == Point3D.Zero && !System.ArenaMap.CanSpawnMobile(p.X, p.Y, p.Z));

            return p;
        }

        public override void OnBeginFight()
        {
            DoStartMessage();

            foreach (var fighter in GetFighters())
            {
                fighter.Frozen = false;
                fighter.Hidden = false;
            }
        }

        public override void DoTieBreaker()
        {
            ArenaTeam winningTeam = null;
            ArenaTeam runnerUp = null;

            int highestDamage = 0;
            int highestKills = 0;

            //Kills as tie breaker
            foreach (TeamInfo info in Teams)
            {
                ArenaTeam team = info.Team;

                if (team == null)
                    continue;

                int kills = info.Kills;

                if (kills > highestKills)
                {
                    winningTeam = team;
                    highestKills = kills;
                }
            }

            // Now by damage
            if (winningTeam == null)
            {
                foreach (var team in Teams.Select(x => x.Team))
                {
                    if (team == null)
                        continue;

                    int damage = GetTotalDamageGiven(team);

                    if (damage > highestDamage)
                    {
                        winningTeam = team;
                        highestDamage = damage;
                    }
                }
            }

            if (winningTeam != null)
            {
                //Randomly chooses runner up
                List<TeamInfo> teams = new List<TeamInfo>(Teams);
                TeamInfo winningInfo = GetTeamInfo(winningTeam);

                if (winningInfo != null && teams.Contains(winningInfo))
                    teams.Remove(winningInfo);

                ArenaTeam runnerup = teams[Utility.Random(teams.Count)].Team;

                EndFight(winningTeam, runnerUp, false);
                ColUtility.Free(teams);
            }
            else
            {
                //Randomly chooses winner and runner up
                List<TeamInfo> teams = new List<TeamInfo>(Teams);
                winningTeam = teams[Utility.Random(teams.Count)].Team;
                TeamInfo winningInfo = GetTeamInfo(winningTeam);

                if (winningInfo != null)
                    teams.Remove(winningInfo);

                ArenaTeam runnerup = teams[Utility.Random(teams.Count)].Team;

                EndFight(winningTeam, runnerup, false);
                ColUtility.Free(teams);
            }
        }

        protected override void DoStartDelayMessage()
        {
            if (System == null || !System.DoStartDelayMessage())
                return;

            foreach (var mob in GetParticipantsAndAudience())
            {
                TimeSpan delay = System.PreFightDelay + System.StartDelay;

                if (IsParticipant(mob))
                {
                    mob.SendMessage(ArenaHelper.ParticipantMessageHue, "Get set to fight in {0} seconds!", delay.TotalSeconds.ToString());
                    mob.SendSound(0x403);
                }
                else
                {
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "Last man standing will begin in {0} seconds!", delay.TotalSeconds.ToString());
                }
            }
        }

        protected override void DoStartMessage()
        {
            if (System == null || !System.DoStartMessage())
                return;

            foreach (var mob in GetParticipantsAndAudience())
            {
                if (IsParticipant(mob))
                {
                    mob.SendMessage(ArenaHelper.ParticipantMessageHue, "FIGHT!", System.StartDelay.ToString());
                }
                else
                {
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "The last man standing fight has begun!");
                }

                mob.PlaySound(0x664); //Cannon shot
            }
        }

        protected override void DoEndFightMessage(ArenaTeam winner, ArenaTeam loser, bool forfeit)
        {
            if (System == null || winner == null || !System.DoEndOfFightMessage(winner, loser, forfeit))
                return;

            foreach (var mob in GetParticipantsAndAudience())
            {
                TeamInfo info = GetTeamInfo(mob);

                if (IsParticipant(mob) && info != null && info.Team == winner)
                {
                    continue;
                }
                else if (IsParticipant(mob) && info != null && info.Team != winner)
                {
                    mob.PlaySound(0x5B3);
                    DoDelayedMessage("Your team has been defeated despite your valiant efforts.", ArenaHelper.ParticipantMessageHue, mob, TimeSpan.FromSeconds(18));
                }
                else if (winner != null)
                {
                    mob.SendSound(mob.Female ? 0x30C : 0x41B);

                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "{0} Arena Team are the last one standing!", winner.Name);
                }
                else
                {
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "The fight has ended in a tie!");
                }
            }

            if (winner != null)
            {
                System.DoArenaKeeperMessage(String.Format("{0} has achieved victory!", winner.Name));
            }
        }
    }
}
