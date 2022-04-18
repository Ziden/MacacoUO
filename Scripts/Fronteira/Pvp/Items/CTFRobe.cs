using System;

namespace Server.Items
{
	[FlipableAttribute( 0x1f03, 0x1f04 )]
	public class CTFRobe : BaseOuterTorso
	{
		public CTFRobe( int hue ) : base( 0x1F03, hue )
		{
			Name = "[Item de Evento]";
			Weight = 0.1;
			Movable = false;
			Attributes.LowerRegCost = 100;
			//LootType = LootType.Cursed;
		}

		public CTFRobe( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

    public class GargishCTFRobe : GargishRobe
    {
        public GargishCTFRobe(int hue)
        {
            Hue = hue;
            Name = "[Item de Evento]";
            Weight = 0.1;
            Movable = false;
            Attributes.LowerRegCost = 100;
            Attributes.CastRecovery = 6;
            Attributes.Resistence = 2;
            Attributes.LowerManaCost = 30;
            Attributes.RegenMana = 5;
            Resistances.Physical = 60;
            Resistances.Fire = 60;
            Resistances.Cold = 60;
            Resistances.Poison = 60;
            Resistances.Energy = 60;
            //LootType = LootType.Cursed;
        }

        public GargishCTFRobe(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

}
