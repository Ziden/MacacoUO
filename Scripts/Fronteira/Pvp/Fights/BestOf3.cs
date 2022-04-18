using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;

namespace Server.TournamentSystem
{
    public class BestOf3Fight : ArenaFight 
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeamAWins { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TeamBWins { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public override ArenaFightType ArenaFightType { get { return ArenaFightType.BestOf3; } }

        public BestOf3Fight(PVPTournamentSystem system, Tournament tourney)
            : base(system, tourney)
        {
            TeamAWins = 0;
            TeamBWins = 0;
        }

        public override void OnFightWon(ArenaTeam winner)
        {
            winner.BestOf3Wins++;
        }

        public override bool CheckEliminated(ArenaTeam winner, ArenaTeam loser)
        {
            if (winner == TeamA) 
                TeamAWins++;

            if (winner == TeamB) 
                TeamBWins++;

            if ((winner == TeamA && TeamAWins < 2) || (winner == TeamB && TeamBWins < 2))
            {
                return false;
            }

            return true;
        }

        public override void DoNextFight(ArenaTeam winner, ArenaTeam loser, TimeSpan ts)
        {
            EjectPlayers(ts, false);

            TimeSpan delay = TimeSpan.FromSeconds(60);

            Timer.DelayCall(delay, new TimerCallback(BeginPreFight));

            DoRestartBestOf3Fight(delay, winner, loser);
        }

        private void DoRestartBestOf3Fight(TimeSpan restart, ArenaTeam winners, ArenaTeam losers)
        {
            if (System == null)
                return;

            int seconds = (int)restart.TotalSeconds;
            string fights = TeamAWins + TeamBWins == 1 ? "first" : "second";

            foreach (Mobile m in winners.Fighters)
                DoDelayedMessage(String.Format("Your team has won the {0} fight. You have {1} seconds until the next fight!", fights, seconds.ToString()), ArenaHelper.ParticipantMessageHue, m, TimeSpan.FromSeconds(18));

            foreach (Mobile m in losers.Fighters)
                DoDelayedMessage(String.Format("Your team has lost the {0} fight. You have {1} seconds until the next fight!", fights, seconds.ToString()), ArenaHelper.ParticipantMessageHue, m, TimeSpan.FromSeconds(18));

            foreach (var mob in GetAudience().Where(m => !IsParticipant(m)))
            {
                if (winners != null && losers != null)
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "{0} have achieved victory over {1}! The next fight will occur in {2} seconds.", winners.Name, losers.Name, seconds.ToString());
                else
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "The next fight will begin in {0} seconds!", seconds.ToString());
            }
        }
    }
}