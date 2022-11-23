using System;
using Server;

// ---------------------- Fences -----------------------
namespace Server.Items
{
	public abstract class BaseFence : Item
	{
		private RanchStone m_ranchstone;
		[CommandProperty( AccessLevel.GameMaster )]
		public RanchStone ranchstone
		{
			get { return m_ranchstone; }
			set {	m_ranchstone = value; InvalidateProperties(); }
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			if (m_ranchstone != null) list.Add(m_ranchstone.Ranch);
		}

		public BaseFence(int itemID) : base(itemID)
		{
			Weight = 50.0;
			Movable = true;
		}

		public BaseFence(Serial serial): base(serial)
		{
		}

		public override bool Decays { get { return false; } }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
			writer.Write(m_ranchstone);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			m_ranchstone = (RanchStone)reader.ReadItem();
		}
	}

	[FlipableAttribute( 0x88A, 0x88B )]
	public class Fence : BaseFence
	{
		[Constructable]
		public Fence () : base( 0x88B )
		{
			Name = "cerca";
			Weight = 50.0;
			Movable = true;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if (Movable && (!(this.IsChildOf( from.Backpack ))))
			{
				if (ItemID == 0x88A) ItemID = 0x88B;
				else ItemID = 0x88A;
				from.SendMessage("Girou a cerca.");
			}
		}
		
		public Fence ( Serial serial ) : base( serial )
		{
		}

		public override bool Decays { get { return false; } }

		public override void Serialize( GenericWriter writer ) // Default Serialize method
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader ) // Default Deserialize method
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}	
	
	public class FenceCorner : BaseFence
	{
		[Constructable]
		public FenceCorner () : base( 0x862 )
		{
			Name = "a corner fence";
			Weight = 50.0;
			Movable = true;
		}

		public FenceCorner ( Serial serial ) : base( serial )
		{
		}

		public override bool Decays { get { return false; } }

		public override void Serialize( GenericWriter writer ) // Default Serialize method
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader ) // Default Deserialize method
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class FencePost : BaseFence
	{
		[Constructable]
		public FencePost() : base( 0x85F )
		{
			Name = "a fence post";
			Weight = 50.0;
			Movable = true;
		}

		public FencePost ( Serial serial ) : base( serial )
		{
		}

		public override bool Decays { get { return false; } }

		public override void Serialize( GenericWriter writer ) // Default Serialize method
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader ) // Default Deserialize method
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}
