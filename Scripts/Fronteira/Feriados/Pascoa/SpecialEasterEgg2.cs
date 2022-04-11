
using System;
using Server;

namespace Server.Items
{
	public class SpecialEasterEgg2 : Item
	{
		[Constructable]
		public  SpecialEasterEgg2() : base( 0x9B5 )
		{
			Weight = 1.0;
			Name = "Ovo de Pascoa Especial";
            this.Stackable = true;
			 Hue =  Utility.RandomList ( 170, 33, 1161, 1266 );
		}

		public SpecialEasterEgg2( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		
		
		

			}
		}
	}	
