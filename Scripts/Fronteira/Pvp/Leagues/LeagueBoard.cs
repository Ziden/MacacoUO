using System;
using Server;
using Server.Mobiles;
using Server.Targeting;
using Server.TournamentSystem;
using Server.Gumps;

namespace Server.Items
{
    public class LeagueBoard : Item
    {
        public override bool ForceShowProperties { get { return true; } }

        [Constructable]
        public LeagueBoard() : base(0xA0C5)
        {
            Movable = false;
            Name = "League Board";
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(Location, 3))
                from.SendLocalizedMessage(500446); // That is too far away.
            else
            {
                if (from is PlayerMobile)
                {
                    if (!from.HasGump(typeof(LeaguesGump)) && League.Leagues != null && League.Leagues.Count > 0)
                    {
                        BaseGump.SendGump(new LeaguesGump(from as PlayerMobile));
                    }
                    else if (!from.HasGump(typeof(LeagueRecordsGump)) && League.LeagueRecords != null && League.LeagueRecords.Count > 0)
                    {
                        BaseGump.SendGump(new LeagueRecordsGump(from as PlayerMobile));
                    }
                }
            }
        }

        public LeagueBoard(Serial serial)
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
