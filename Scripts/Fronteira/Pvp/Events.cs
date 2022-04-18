using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Commands;

namespace Server.TournamentSystem
{
    public delegate void OnLeaderBoardResetEventHandler(LeaderBoardResetEventArgs e);

    public static class TournyEventSink
    {
        public static event OnLeaderBoardResetEventHandler OnLeaderBoardReset;

        public static void InvokeOnLeaderBoardReset(LeaderBoardResetEventArgs e)
        {
            if (OnLeaderBoardReset != null)
            {
                OnLeaderBoardReset(e);
            }
        }
    }

    public class LeaderBoardResetEventArgs : EventArgs
    {
        public ArenaTeam Team { get; set; }
        public ArenaFightType FightType { get; set; }

        public LeaderBoardResetEventArgs(ArenaTeam team, ArenaFightType fightType)
        {
            Team = team;
            FightType = fightType;
        }
    }
}
