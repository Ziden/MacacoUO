using System;
using Server.Network;
using Server.Items;

namespace Server.Items
{
	public class ChocolateRabbit : Food
	{
		[Constructable]
		public ChocolateRabbit() : this( 1 )
		{
		}

		[Constructable]
		public ChocolateRabbit( int amount ) : base( amount, 0x2125 )
		{
			this.Weight = 1;
			this.FillFactor = 2;
			this.Name = "Coelinho de Chocolate";
			this.Hue = 1121;
                                                
		}     

		public ChocolateRabbit( Serial serial ) : base( serial )
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
}
