using Server.TournamentSystem;
using Server.Services.Virtues;

namespace Server.Mobiles 
{

    public partial class PlayerMobile : Mobile, IHonorTarget
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeam ArenaSingle
        {
            get
            {
                return ArenaTeam.GetTeam(this, ArenaTeamType.Single);
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeam ArenaTwosome
        {
            get
            {
                return ArenaTeam.GetTeam(this, ArenaTeamType.Twosome);
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeam ArenaFoursome
        {
            get
            {
                return ArenaTeam.GetTeam(this, ArenaTeamType.Foursome);
            }
            set
            {
            }
        }
    }
}
