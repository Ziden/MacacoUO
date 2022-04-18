using System;
using Server;
using Server.Mobiles;
using Server.Gumps;
using Server.TournamentSystem;

namespace Server.Items
{
    [DeleteConfirm("Are you sure you want to delete this? Deleting this stone will delete any unclaimed prizes or prizes for upcoming tournaments.")]
    public class PrizeChest : WoodenChest
    {
        private PVPTournamentSystem m_System;

        [CommandProperty(AccessLevel.GameMaster)]
        public PVPTournamentSystem System
        {
            get { return m_System; }
            set { m_System = value; }
        }

        public override string DefaultName
        {
            get 
            {
                if (m_System != null)
                    return String.Format("{0} - Bau de Premio", m_System.Name);

                return "Caixa de Premiacao";
            }
        }

        [Constructable]
        public PrizeChest()
        {
            ItemID = 18705;
            Movable = false;
            Visible = false;
        }

        [Constructable]
        public PrizeChest(PVPTournamentSystem system)
        {
            m_System = system;
            Movable = false;
        }

        public PrizeChest(Serial serial) : base (serial)
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
