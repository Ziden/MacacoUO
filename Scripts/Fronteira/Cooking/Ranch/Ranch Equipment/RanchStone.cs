using System;
using Server;
using System.Collections;
using System.Collections.Generic;
using Server.Targeting;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Prompts;
using Server.Multis;
using Server.Gumps;

namespace Server.Items
{
	[Flipable( 0x14F0, 0x14EF )]
	public class RanchDeed : Item
	{
		[Constructable]
		public RanchDeed() : base( 0x14F0 )
		{
			Name = "escritura de rancho";
			Weight = 1.0;
		}

		public virtual bool NotForbiddenRegions(Mobile from)
		{
			if (from.Region is NoHousingRegion) return false;
			if (from.Region is GuardedRegion) return false;
			if (from.Region is DungeonRegion) return false;
			if (from.Region is TownRegion) return false;
			return true;
		}

        public static void FenceOnCorners(Map map, Point3D center, int size)
        {
            var p1 = new Point3D(center.X + size, center.Y + size, center.Z);
            var fence = new FencePost();
            p1.Z = map.GetAverageZ(new Point2D(p1.X, p1.Y));
            fence.MoveToWorld(p1, map);

            var p2 = new Point3D(center.X - size, center.Y + size, center.Z);
            var fence2 = new FencePost();
            p2.Z = map.GetAverageZ(new Point2D(p2.X, p2.Y));
            fence2.MoveToWorld(p2, map);

            var p3 = new Point3D(center.X + size, center.Y - size, center.Z);
            var fence3 = new FencePost();
            p3.Z = map.GetAverageZ(new Point2D(p3.X, p3.Y));
            fence3.MoveToWorld(p3, map);

            var p4 = new Point3D(center.X - size, center.Y - size, center.Z);
            var fence4 = new FencePost();
            p4.Z = map.GetAverageZ(new Point2D(p4.X, p4.Y));
            fence4.MoveToWorld(p4, map);
        }

		public override void OnDoubleClick( Mobile from )
		{
			if (this.IsChildOf( from.Backpack ))
			{
				bool canplace = true;

				foreach ( Item item in this.GetItemsInRange( 11 ) ) 
				{
					if ((item is BaseHouse ) || (item is RanchStone)) canplace = false;
				}

				if ( (NotForbiddenRegions(from) && canplace) || (from.AccessLevel >= AccessLevel.GameMaster))
				{
					Point3D rd = new Point3D(0,0,0);
					if (from.Direction == Direction.North) rd = new Point3D(from.X, from.Y -1, from.Z);
					else if (from.Direction == Direction.South) rd = new Point3D(from.X, from.Y +1, from.Z);
					else if (from.Direction == Direction.West) rd = new Point3D(from.X-1, from.Y, from.Z);
					else if (from.Direction == Direction.East) rd = new Point3D(from.X+1, from.Y, from.Z);

					else if (from.Direction == Direction.Up) rd = new Point3D(from.X-1, from.Y-1, from.Z);
					else if (from.Direction == Direction.Down) rd = new Point3D(from.X+1, from.Y+1, from.Z);
					else if (from.Direction == Direction.Left) rd = new Point3D(from.X-1, from.Y+1, from.Z);
					else rd = new Point3D(from.X+1, from.Y-1, from.Z);
	
					RanchStone rs = new RanchStone();
					rs.Owner = from;
                    rs.Ranch = "rancho de " + from.Name;
					rs.Movable = false;
					rs.MoveToWorld(rd, from.Map);
                    FenceOnCorners(rs.Map, rs.Location, rs.Size);
                    this.Delete();
				}
				else
				{
					from.SendMessage("Nao pode colocar aqui");
				}
			}
			else
			{
				from.SendMessage("That must be in your backpack to use");
			}
		}

		public RanchDeed( Serial serial ) : base( serial )
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

	public class RanchStone : Item, IChopable
	{
		private Mobile m_Owner; 
		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner
		{
			get{ return m_Owner; }
			set{ m_Owner = value; }
		}

		private String m_Ranch; 
		[CommandProperty( AccessLevel.GameMaster )]
		public String Ranch
		{
			get{ return m_Ranch; }
			set{ m_Ranch = value; InvalidateProperties(); }
		}

		private int m_Size = 10; 
		[CommandProperty( AccessLevel.GameMaster )]
		public int Size
		{
			get{ return m_Size; }
			set{ m_Size = value; InvalidateProperties(); }
		}
		
		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			list.Add(m_Ranch);
			if (m_Owner != null && !m_Owner.Deleted) list.Add("Dono: " + m_Owner.Name);
		}

		[Constructable]
		public RanchStone() : base(0xED6) 
		{
			Name = "pedra de rancho";
			m_Size = 10;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if (!(from.HasGump( typeof( RanchStoneGump))))
			{
                if (Owner == null || Name == null)
                {
                    from.SendMessage("Coloque o dono primeiro!");
                    return;
                }
                if (from == m_Owner || from.AccessLevel >= AccessLevel.GameMaster)
				{
					from.SendGump( new RanchStoneGump(this) );
				}
				else from.SendMessage("Esse rancho nao eh seu.");
			}
		}

		public virtual void OnChop(Mobile from)
		{
			if (from == Owner)
			{
				Effects.PlaySound( GetWorldLocation(), Map, 0x3B3 );
				RanchDeed rd = new RanchDeed();
				from.AddToBackpack(rd);
				from.SendMessage("Seu rancho foi obtido");
				if(m_Size > 10)
				{
					for ( int i = 0; i < ((m_Size-10)/5); ++i )
					{
						from.AddToBackpack(new RanchExtensionDeed());
					}
				}
				this.Delete();
			}
		}

		public virtual void UpdateFences()
		{
			foreach ( Item item in this.GetItemsInRange( m_Size+1 ) ) 
			{
				if (item is FenceGate)
				{
					FenceGate fg = (FenceGate) item;
					if (fg.ranchstone == this)
					{
						fg.InvalidateProperties();
					}
				}
				else if (item is BaseFence)
				{
					BaseFence f = (BaseFence) item;
					if (f.ranchstone == this)
					{
						f.InvalidateProperties();
					}
				}
			}
		}

		public override void OnDelete()
		{
			foreach ( Item item in this.GetItemsInRange( m_Size+1 ) ) 
			{
				if (item is FenceGate)
				{
					FenceGate fg = (FenceGate) item;
					if (fg.ranchstone == this)
					{
						fg.ranchstone = null;
						fg.Movable = true;
					}
				}
				else if (item is BaseFence)
				{
					BaseFence f = (BaseFence) item;
					if (f.ranchstone == this)
					{
						f.ranchstone = null;
						f.Movable = true;
					}
				}
			}
		}

		public RanchStone( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer ) // Default Serialize method
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
			
			writer.Write(m_Size);
			
			writer.Write( m_Owner );
			writer.Write( (string) m_Ranch );
		}

		public override void Deserialize( GenericReader reader ) // Default Deserialize method
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			switch (version)
			{
			case 1:
				{
					m_Size = reader.ReadInt();
					goto case 0;
				}
			case 0:
				{
					m_Owner = reader.ReadMobile();
					m_Ranch = reader.ReadString();
					break;
				}
			}
		}
	}
}
	
namespace Server.Gumps
{
	public class RanchStoneGump : Gump
	{
		public enum Buttons
		{
			None,
			BrandingIron,
			FencingHammer,
			UnlockFences,
			RanchName,
			Redeed,
			TransferOwnership,
			ReclaimAnimal,
			AbsorbRanch,
			TransferAnimal,
		}
		
		private RanchStone m_rs;
		
		public RanchStoneGump(RanchStone rs) : base( 0, 0 )
		{
			m_rs = rs;

			this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;
			this.AddPage(0);
			this.AddBackground(0, 0, 250, 300, 9200);
			//this.AddLabel(15, 45, 1160, @"Ranch Name: "+m_rs.Ranch);//ranch name
			this.AddHtml(15, 40, 220, 35, (String.Format("<CENTER><BIG><BASEFONT SIZE=7 COLOR=#FFFF00>{0}<BASEFONT COLOR=#000000></BIG></CENTER>",m_rs.Ranch)), false, false);//"Ranch Name: "+m_rs.Ranch);//ranch name
			this.AddHtml(15, 60, 220, 35, (String.Format("<CENTER><BASEFONT SIZE=4 COLOR=#FFFF00>Dono: {0} <BASEFONT COLOR=#000000></CENTER>",m_rs.Owner.Name)), false, false);//ranch owner
			this.AddLabel(30, 80, 0, @"Ferro de Marcar");
			this.AddLabel(30, 100, 0, @"Pegar animal");
			this.AddLabel(30, 120, 0, @"Transferir animal");
			this.AddLabel(30, 140, 0, @"Martelo de cercas");
			this.AddLabel(30, 160, 0, @"Destravar cercas");
			this.AddLabel(30, 180, 0, @"Renomear rancho");
			this.AddLabel(30, 200, 0, @"Desmontar rancho");
			this.AddLabel(30, 220, 0, @"Absorver outro rancho");
			this.AddLabel(30, 240, 0, @"Transferir titularidade");
			this.AddButton(190, 85, 1209, 1210, (int)Buttons.BrandingIron, GumpButtonType.Reply, 0);
			this.AddButton(190, 105, 1209, 1210, (int)Buttons.ReclaimAnimal, GumpButtonType.Reply, 0);
			this.AddButton(190, 125, 1209, 1210, (int)Buttons.TransferAnimal, GumpButtonType.Reply, 0);
			this.AddButton(190, 145, 1209, 1210, (int)Buttons.FencingHammer, GumpButtonType.Reply, 0);
			this.AddButton(190, 165, 1209, 1210, (int)Buttons.UnlockFences, GumpButtonType.Reply, 0);
			this.AddButton(190, 185, 1209, 1210, (int)Buttons.RanchName, GumpButtonType.Reply, 0);
			this.AddButton(190, 205, 1209, 1210, (int)Buttons.Redeed, GumpButtonType.Reply, 0);
			this.AddButton(190, 225, 1209, 1210, (int)Buttons.AbsorbRanch, GumpButtonType.Reply, 0);
			this.AddButton(190, 245, 1209, 1210, (int)Buttons.TransferOwnership, GumpButtonType.Reply, 0);
			this.AddImage(5, 5, 10460);
			this.AddImage(210, 5, 10460);
			this.AddImage(5, 265, 10460);
			this.AddImage(210, 265, 10460);
			this.AddImage(42, 262, 11392);
			this.AddLabel(50, 270, 1160, @"Tamanho: "+ m_rs.Size);//ranch size
			this.AddLabel(120, 270, 1160, @"Area: "+ (int) (m_rs.Size * m_rs.Size * 3.14159));//ranch area
			this.AddImage(42, 5, 11392);
			this.AddLabel(85, 12, 1152, @"Pedra do Rancho");
		}
		
		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile from = sender.Mobile;
			if (info.ButtonID == (int)Buttons.BrandingIron)
			{
				Container pack = from.Backpack;
				ArrayList bagitems = new ArrayList( pack.Items );
				int ironTotal = 0;
				foreach (Item item in bagitems)
				{
					if (item is BrandingIron)
					{
						BrandingIron iron = (BrandingIron)item;
						if (iron.ranchstone == m_rs)
						{
							iron.Delete();
							ironTotal++;
						}
					}
				}

				if (ironTotal > 0)
				{
					from.SendMessage("Voce retornou o ferro de marcar.");
				}
				else
				{
					from.SendMessage("Voce pegou o ferro de marcar. Os animais te olham com lagrimas em seus olhos.");
					BrandingIron bi = new BrandingIron();
					bi.ranchstone = m_rs;
					bi.Hot = true;
					from.AddToBackpack(bi);
				}
				from.SendGump( new RanchStoneGump(m_rs) );
			}
			else if (info.ButtonID == (int)Buttons.FencingHammer)
			{
				Container pack = from.Backpack;
				ArrayList equipitems = new ArrayList(from.Items);
				ArrayList bagitems = new ArrayList( pack.Items );

				int hammerTotal = 0;

				foreach (Item item in equipitems)
				{
					if (item is FencingHammer)
					{
						FencingHammer hammer = (FencingHammer)item;
						if (hammer.ranchstone == m_rs)
						{
							hammer.Delete();
							hammerTotal++;
						}
					}
				}
				foreach (Item item in bagitems)
				{
					if (item is FencingHammer)
					{
						FencingHammer hammer = (FencingHammer)item;
						if (hammer.ranchstone == m_rs)
						{
							hammer.Delete();
							hammerTotal++;
						}
					}
				}

				if (hammerTotal > 0)
				{
					from.SendMessage("Voce retornou o martelo.");
				}
				else
				{
					from.SendMessage("Voce pegou o martelo de cercas.");
					FencingHammer fh = new FencingHammer();
					fh.Identified = true;
					fh.ranchstone = m_rs;
					from.AddToBackpack(fh);
				}
				from.SendGump( new RanchStoneGump(m_rs) );
			}
			else if (info.ButtonID == (int)Buttons.UnlockFences)
			{
				foreach ( Item item in m_rs.GetItemsInRange( m_rs.Size+1 ) ) 
				{
					if (item is FenceGate)
					{
						FenceGate fg = (FenceGate) item;
						if (fg.ranchstone == m_rs)
						{
							fg.ranchstone = null;
							fg.Movable = true;
						}
					}
					else if (item is BaseFence)
					{
						BaseFence f = (BaseFence) item;
						if (f.ranchstone == m_rs)
						{
							f.ranchstone = null;
							f.Movable = true;
						}
					}
				}
				from.SendGump( new RanchStoneGump(m_rs) );
			}
			else if (info.ButtonID == (int)Buttons.RanchName)
			{
				from.SendMessage ( "Digite o nome do rancho." ); 
				from.Prompt = new RenamePrompt( m_rs );
			}
			else if (info.ButtonID == (int)Buttons.Redeed)
			{
				RanchDeed rd = new RanchDeed();
				from.AddToBackpack(rd);
				from.SendMessage("Voce desmontou seu rancho.");
				if(m_rs.Size > 10)
				{
					for ( int i = 0; i < ((m_rs.Size-10)/5); ++i )
					{
						from.AddToBackpack(new RanchExtensionDeed());
					}
				}
				m_rs.Delete();
			}
			else if (info.ButtonID == (int)Buttons.TransferOwnership)
			{
				from.SendMessage ( "Selecione a pessoa." ); 
				from.Target = new OwnershipTarget(m_rs);
			}
			else if (info.ButtonID == (int)Buttons.ReclaimAnimal)
			{
				from.SendMessage ( "Selecione o animal." ); 
				from.Target = new ClaimTarget(m_rs);
			}
			else if (info.ButtonID == (int)Buttons.TransferAnimal)
			{
				from.SendMessage ( "Selecione o animal." ); 
				from.Target = new TransferTarget(m_rs);
			}
			else if (info.ButtonID == (int)Buttons.AbsorbRanch)
			{
				from.SendMessage ( "Selecione o rancho." ); 
				from.Target = new AbsorbTarget(m_rs);
			}
		}
		
		private class RenamePrompt : Prompt
		{
			private RanchStone m_RS;

			public RenamePrompt( RanchStone RS )
			{
				m_RS = RS;
			}

			public override void OnResponse( Mobile from, string text )
			{
				m_RS.Ranch = text;
				m_RS.UpdateFences();
				from.SendMessage ( "Alterado." ); 
				from.SendGump( new RanchStoneGump(m_RS) );
			}
		}
		
		private class OwnershipTarget : Target
		{
			private RanchStone t_rs;

			public OwnershipTarget(RanchStone rs) : base( 1, false, TargetFlags.None )
			{
				t_rs = rs;
			}

			protected override void OnTarget( Mobile from, object targ )
			{
				if (targ is PlayerMobile)
				{
					PlayerMobile pm = (PlayerMobile) targ;
					t_rs.Owner = pm;
					from.SendMessage("Transferido.");
					pm.SendMessage("Voce eh o dono de "+t_rs.Ranch);
					pm.SendGump( new RanchStoneGump(t_rs) );
				}
				else
				{
					from.SendMessage ("Nao pode controlar isto.");
					from.SendGump( new RanchStoneGump(t_rs) );
				}
			}
		}
		
		private class ClaimTarget : Target
		{
			private RanchStone t_rs;

			public ClaimTarget(RanchStone rs) : base( 10, false, TargetFlags.None )
			{
				t_rs = rs;
			}

			protected override void OnTarget( Mobile from, object targ )
			{
				if (targ is BaseAnimal)
				{
					BaseAnimal ba = (BaseAnimal) targ;
					if (ba.Owner == from)
					{
						if (from.Followers + ba.ControlSlots <= from.FollowersMax)
						{
							ba.Controlled = true;
							ba.ControlMaster = from;
							ba.ControlTarget = from;
							ba.ControlOrder = OrderType.Follow;
							from.SendMessage("Voce pegou seu animal.");
						}
						else from.SendMessage("Muitos seguidos!");
						from.SendGump( new RanchStoneGump(t_rs) );
					}
					else
					{
						from.SendMessage("Nao eh seu.");
						from.SendGump( new RanchStoneGump(t_rs) );
					}
				}
				else
				{
					from.SendMessage ("Voce nao pode pegar isto.");
					from.SendGump( new RanchStoneGump(t_rs) );
				}
			}
		}
		
		private class TransferTarget : Target
		{
			private RanchStone t_rs;

			public TransferTarget(RanchStone rs) : base( 10, false, TargetFlags.None )
			{
				t_rs = rs;
			}

			protected override void OnTarget( Mobile from, object targ )
			{
				if (targ is BaseAnimal)
				{
					BaseAnimal ba = (BaseAnimal) targ;
					if (ba.Owner == from)
					{
						from.SendMessage("Selecione a pessoa.");
						from.Target = new TransferToTarget(ba, t_rs);
					}
					else
					{
						from.SendMessage("Nao eh seu.");
						from.SendGump( new RanchStoneGump(t_rs) );
					}
				}
				else
				{
					from.SendMessage ("Nao pode pegar isto.");
					from.SendGump( new RanchStoneGump(t_rs) );
				}
			}
		}
		
		private class TransferToTarget : Target
		{
			private BaseAnimal t_ba;
			private RanchStone t_rs;

			public TransferToTarget(BaseAnimal ba, RanchStone rs) : base( 10, false, TargetFlags.None )
			{
				t_ba = ba;
				t_rs = rs;
			}

			protected override void OnTarget( Mobile from, object targ )
			{
				if (targ is PlayerMobile)
				{
					if (targ != from)
					{
						PlayerMobile pm = (PlayerMobile) targ;
						if (pm.Followers + t_ba.ControlSlots <= pm.FollowersMax)
						{
							t_ba.Owner = pm;
							t_ba.Controlled = true;
							t_ba.ControlMaster = pm;
							t_ba.ControlTarget = pm;
							t_ba.ControlOrder = OrderType.Follow;
							from.SendMessage(t_ba.Name + " transferido para " + pm.Name + ".");
							pm.SendMessage(from.Name + " transferiu " + t_ba.Name + " a voce.");
						}
						else
						{
							pm.SendMessage("Muitos seguidores!");
							from.SendMessage(pm.Name + " tem muitos seguidores!");
						}
					}
					else from.SendMessage("Voce vai transferir pra voce mesmo ? ta bobo ?!");
				}
				else from.SendMessage ("Voce nao pode fazer isto.");
				from.SendGump( new RanchStoneGump(t_rs) );
			}
		}
		
		private class AbsorbTarget : Target
		{
			private RanchStone t_rs;

			public AbsorbTarget(RanchStone rs) : base( 1, false, TargetFlags.None )
			{
				t_rs = rs;
			}

			protected override void OnTarget( Mobile from, object targ )
			{
				if (targ is RanchStone)
				{
					RanchStone stone = (RanchStone) targ;
					if (stone.Owner == from)
					{
						if(t_rs.Map == stone.Map)
						{
							int ranchrange = (int)(from.GetDistanceToSqrt(t_rs.Location));
							int ranchto = (int)(stone.Size/2);
							int ranchfrom = (int)(t_rs.Size/2);
							if ((ranchfrom >= (ranchrange - (ranchto + ranchfrom))) && t_rs.Size > stone.Size)
							{
								foreach ( Item item in stone.GetItemsInRange( stone.Size+1 ) ) 
								{
									if (item is FenceGate)
									{
										FenceGate fg = (FenceGate) item;
										if (fg.ranchstone == stone)
										{
											fg.ranchstone = t_rs;
											fg.InvalidateProperties();
										}
									}
									else if (item is BaseFence)
									{
										BaseFence f = (BaseFence) item;
										if (f.ranchstone == stone)
										{
											f.ranchstone = t_rs;
											f.InvalidateProperties();
										}
									}
								}
								RanchDeed rd = new RanchDeed();
								from.AddToBackpack(rd);
								from.SendMessage("Voce absorveu o rancho.");
								stone.Delete();
							}
							else from.SendMessage("Seu rancho nao eh grande suficiente!");
						}
						else from.SendMessage("Outro mapa!");
					}
					else from.SendMessage("O rancho nao eh seu!");
				}
				else
				{
					from.SendMessage ("Nao pode absorver isto!");
				}
				from.SendGump( new RanchStoneGump(t_rs) );
			}
		}
	}
}

