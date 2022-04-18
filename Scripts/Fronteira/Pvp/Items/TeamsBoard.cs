using System;
using Server;
using Server.Mobiles;
using Server.TournamentSystem;
using Server.Gumps;

namespace Server.Items
{
    public class TeamsBoard : Item
    {
        public override bool ForceShowProperties { get { return true; } }

        [Constructable]
        public TeamsBoard() : base(7775)
        {
            Movable = false;
            Name = "Quadro de Times";
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(Location, 3))
                from.SendLocalizedMessage(500446); // That is too far away.
            else
            {
                if (from is PlayerMobile && !from.HasGump(typeof(PlayerStatsGump)))
                    BaseGump.SendGump(new PlayerStatsGump(from as PlayerMobile));
            }
        }

        public TeamsBoard(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
