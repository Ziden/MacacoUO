using System;
using Server;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
using System.Collections.Generic;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class BaseTournamentGump : BaseGump
    {
        public virtual int LabelHue { get { return 1153; } }
        public virtual int BodyHue { get { return 0; } }
        public virtual int LabelColor { get { return 0xFFFFFF; } }
        public virtual int DarkBackground { get { return 5120; } }
        public virtual int LightBackground { get { return 0xBB8; } }
        public virtual int LightEntry { get { return 0xBBC; } }
        public virtual int DarkEntry { get { return 5124; } }

        public BaseTournamentGump(PlayerMobile pm, int x, int y, BaseGump parent = null)
            : base (pm, x, y, parent)
        {
        }

        public override void AddGumpLayout()
        {
        }
    }
}
