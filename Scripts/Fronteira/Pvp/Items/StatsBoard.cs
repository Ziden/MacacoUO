using System;
using Server;
using Server.Mobiles;
using Server.Targeting;
using Server.TournamentSystem;
using Server.Gumps;

namespace Server.Items
{
    public class StatsBoard : Item
    {
        public override bool ForceShowProperties { get { return true; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public PVPTournamentSystem System { get; set; }

        [Constructable]
        public StatsBoard() : this(null)
        {
        }

        [Constructable]
        public StatsBoard(PVPTournamentSystem sys) : base(7775)
        {
            System = sys;
            Movable = false;
            Name = "Quadro de Torneios";
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(Location, 3))
                from.SendLocalizedMessage(500446); // That is too far away.
            else
            {
                if (from is PlayerMobile && !from.HasGump(typeof(TournamentStatsGump)))
                    BaseGump.SendGump(new TournamentStatsGump(from as PlayerMobile, System));
            }
        }

        public StatsBoard(Serial serial)
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
