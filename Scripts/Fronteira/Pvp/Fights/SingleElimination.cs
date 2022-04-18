using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;

namespace Server.TournamentSystem
{
    public class SingleEliminationFight : ArenaFight 
    {
        public SingleEliminationFight(PVPTournamentSystem system, Tournament tourney)
            : base(system, tourney)
        {
        }

        public override void OnFightWon(ArenaTeam winner)
        {
            winner.SingleElimWins++;
        }
    }
}