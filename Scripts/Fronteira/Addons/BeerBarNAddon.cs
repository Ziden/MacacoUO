using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class BeerBarNAddon : BaseAddon
	{
        private static int[,] m_AddOnSimpleComponents = new int[,] {
			  {40787, 0, 0, 0}, {40788, -1, 0, 0}, {40786, 1, 0, 0}// 1	2	3	
					};

 
            
		public override BaseAddonDeed Deed
		{
			get
			{
				return new BeerBarNAddonDeed();
			}
		}

		[ Constructable ]
		public BeerBarNAddon()
		{

            for (int i = 0; i < m_AddOnSimpleComponents.Length / 4; i++)
                AddComponent( new AddonComponent( m_AddOnSimpleComponents[i,0] ), m_AddOnSimpleComponents[i,1], m_AddOnSimpleComponents[i,2], m_AddOnSimpleComponents[i,3] );


		}

		public BeerBarNAddon( Serial serial ) : base( serial )
		{
		}


		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class BeerBarNAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new BeerBarNAddon();
			}
		}

		[Constructable]
		public BeerBarNAddonDeed()
		{
			Name = "Cervejaria";
		}

		public BeerBarNAddonDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void	Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
