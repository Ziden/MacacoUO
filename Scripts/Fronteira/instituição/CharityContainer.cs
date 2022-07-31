using System;
using System.Collections.Generic;
using System.Linq;


namespace Server.Items
{
	public sealed class CharityContainer : Container
	{
		public static List<CharityContainer> Instances { get; private set; }

		static CharityContainer()
		{
			Instances = new List<CharityContainer>();
		}

		public static CharityContainer Acquire(Mobile m)
		{
			if (m == null || m.Deleted)
			{
				return null;
			}

			var box = m.Items.OfType<CharityContainer>().FirstOrDefault();

			if (box == null || box.Deleted)
			{
				box = new CharityContainer(m);
			}

			if (m.NetState != null)
			{
				box.SendInfoTo(m.NetState);
			}

			return box;
		}

		private static void AcquireState(Item item)
		{
			if (item is Container)
			{
				foreach (var o in item.Items)
				{
					AcquireState(o);
				}
			}

			var state = Charity.AcquireState(item);

			if (Charity.CMOptions.ModuleDebug)
			{
				var message = "Criar estado ({0}) [{1}]";

				Charity.CMOptions.ToConsole(message, item, state != null ? "Criado" : "Ignorado");
			}
		}

		private static void DestroyState(Item item)
		{
			if (item is Container)
			{
				foreach (var o in item.Items)
				{
					DestroyState(o);
				}
			}
			
			var result = Charity.DestroyState(item);

			if (Charity.CMOptions.ModuleDebug)
			{
				var message = "Remover estado ({0}) [{1}]";

				Charity.CMOptions.ToConsole(message, item, result ? "Removido" : "Ignorado");
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner { get; private set; }

		public override bool IsVirtualItem { get { return true; } }

		public override string DefaultName { get { return "Contêiner de Caridade"; } }

		private CharityContainer(Mobile owner)
			: base(0xE42)
		{
			Owner = owner;

			Movable = false;
			Visible = false;

			Layer = (Layer)127;

			Owner.AddItem(this);

			Instances.Add(this);
		}

		public CharityContainer(Serial serial)
			: base(serial)
		{
			Instances.Add(this);
		}

		public override void Delete()
		{
			foreach (var item in Items)
			{
				DestroyState(item);
			}

			base.Delete();
		}

		public override void OnDelete()
		{
			base.OnDelete();

			Instances.Remove(this);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Owner = null;

			Instances.Remove(this);
		}

		public override DeathMoveResult OnParentDeath(Mobile parent)
		{
			return DeathMoveResult.RemainEquiped;
		}

		public override bool CheckContentDisplay(Mobile m)
		{
			return m == null || Owner == null || m == Owner || m.AccessLevel >= Owner.AccessLevel;
		}

		public override bool IsAccessibleTo(Mobile m)
		{
			return m == null || Owner == null || m == Owner || m.AccessLevel >= Owner.AccessLevel;
		}

		public override bool OnDragDrop(Mobile m, Item item)
		{
			return IsAccessibleTo(m) && base.OnDragDrop(m, item);
		}

		public override bool OnDragDropInto(Mobile m, Item item, Point3D p)
		{
			return IsAccessibleTo(m) && base.OnDragDropInto(m, item, p);
		}

#if ServUO58
		public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, bool checkWeight, int plusItems, int plusWeight)
#else
		public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
#endif
		{
			if (item.BlessedFor != null)
			{
				if (message)
				{
					m.SendMessage("Itens Soulbound não podem ser doados para instituições de caridade.");
				}

				return false;
			}

			if (item.Insured)
			{
				if (message)
				{
					m.SendMessage("Itens segurados não podem ser doados para instituições de caridade.");
				}

				return false;
			}

			if (item.QuestItem)
			{
				if (message)
				{
					m.SendMessage("Itens de missão não podem ser doados para caridade.");
				}

				return false;
			}

			if (!Charity.HasCategory(item))
			{
				if (message)
				{
					m.SendMessage("Esse item não pode ser doado para caridade.");
				}

				return false;
			}

			if (item is Container && item.Items.Count > 0)
			{
				if (message)
				{
					m.SendMessage("Esse recipiente deve estar vazio antes de doar para caridade.");
				}

				return false;
			}

			var limit = Charity.CMOptions.StockLimit;

			if (limit > 0 && Items.Count >= limit)
			{
				if (message)
				{
					m.SendMessage("Você atingiu seu limite de estoque de caridade de {0:#,0} itens.", limit);
				}

				return false;
			}

			return true;
		}

		private bool _Moving;

		public bool Relocate(CharityContainer to)
		{
			if (to == null || to.Deleted || to == this)
			{
				return false;
			}

			if (Items.Count > 0)
			{
				_Moving = to._Moving = true;

				Items.ForEachReverse(
					o =>
					{
						to.DropItem(o);

						var state = Charity.Items.GetValue(o);

						if (state != null)
						{
							state.Container = to;
						}
					});

				_Moving = to._Moving = false;
			}

			return true;
		}

		public override void OnItemAdded(Item item)
		{
			base.OnItemAdded(item);

			if (!_Moving)
			{
				AcquireState(item);
			}
		}

		public override void OnSubItemAdded(Item item)
		{
			base.OnSubItemAdded(item);

			if (!_Moving)
			{
				AcquireState(item);
			}
		}

		public override void OnItemRemoved(Item item)
		{
			base.OnItemRemoved(item);

			if (!_Moving)
			{
				DestroyState(item);
			}
		}

		public override void OnSubItemRemoved(Item item)
		{
			base.OnSubItemRemoved(item);

			if (!_Moving)
			{
				DestroyState(item);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(Owner);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();

			Owner = reader.ReadMobile();

			if (Owner == null || Owner.Deleted)
			{
				Timer.DelayCall(Delete);
			}
		}
	}
}
