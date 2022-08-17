using Server;
using Server.Engines.Points;
using Server.Mobiles;
using System;

namespace Server.Items
{
    public class CristalElemental : Item
	{

		[Constructable]
		public CristalElemental() : this(1)
		{
		}
	
		[Constructable]
		public CristalElemental(int amount) : base(16395)
		{
            Name = "Cristais Elementais";
			Stackable = true;
			Amount = amount;

            Hue = 2611;
		}
		
		public override void OnDoubleClick(Mobile from)
		{
			/*
			if(IsChildOf(from.Backpack))
			{
                from.SendMessage("Voce consumiu um cristal");
				PointsSystem.ShameCrystals.AwardPoints(from, 1, true, true);
                Consume(1);
			}
			*/
		}
		
		public CristalElemental(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}

    public class CristalTherathan : Item
    {

        [Constructable]
        public CristalTherathan() : this(1)
        {
        }

        [Constructable]
        public CristalTherathan(int amount) : base(16395)
        {
            Name = "Cristais Terathan";
            Stackable = true;
            Amount = amount;

            Hue = TintaPreta.COR;
        }

        public CristalTherathan(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
