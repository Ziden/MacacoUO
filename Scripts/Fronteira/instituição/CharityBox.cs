using System;
using System.Drawing;
using System.Linq;

using VitaNex.Network;
using VitaNex.SuperGumps;

namespace Server.Items
{
	public class CharityBox : LargeCrate
	{
		private static bool AddToContainer(Mobile m, Item item)
		{
			var box = CharityContainer.Acquire(m);

			if (box == null || box.Deleted)
			{
				return false;
			}

			var count = 0;

			try
			{
				return AddTo(m, item, box, true, ref count);
			}
			finally
			{
				if (count > 0)
				{
					foreach (var g in SuperGump.EnumerateInstances<CharityUI>(m))
					{
						if (g.ViewStock && (g.SelectedNode.IsRoot || Charity.InCategory(item, g.Category)))
						{
							g.Refresh(true);
						}
					}
				}
			}
		}

		private static bool AddTo(Mobile m, Item item, CharityContainer box, bool fullMessage, ref int count)
		{
			if (item is Container)
			{
				var i = item.Items.Count;

				while (--i >= 0)
				{
					if (i < item.Items.Count)
					{
						AddTo(m, item.Items[i], box, false, ref count);
					}
				}
			}

			if (item.Items.Count == 0 && box.CheckHold(m, item, fullMessage))
			{
				box.DropItem(item);

				OnAdded(m, item);

				++count;

				return true;
			}

			return false;
		}

		private static void OnAdded(Mobile m, Item item)
		{
			var label = String.Format("{0:#,0} {1}", item.Amount, item.ResolveName(m));

			m.SendMessage("{0}foi adicionado ao seu estoque de caridade.", label);
		}

		public override string DefaultName { get { return "Charity Box"; } }

		public override bool DisplaysContent { get { return false; } }

#if ServUO58
		public override bool CheckHoldCount => false;
		public override bool CheckHoldWeight => false;
#endif

		[Constructable]
		public CharityBox()
		{ }

		public CharityBox(Serial serial)
			: base(serial)
		{ }

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			using (var opl = new ExtendedOPL(list))
			{
				var total = Charity.Items.Values.Count(o => o.IsValid);

				opl.Add("{0:#,0} Items".WrapUOHtmlColor(Color.SkyBlue), total);
				opl.Add("Use: Navegue pela Caridade".WrapUOHtmlColor(Color.LawnGreen));
				opl.Add("Use: Coloque itens dentro para doá-los!".WrapUOHtmlColor(Color.LawnGreen));
			}
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (!this.CheckUse(m, true, false, 2))
			{
				return;
			}

			if (Charity.CMOptions.ModuleEnabled && SuperGump.RefreshInstances<CharityUI>(m) <= 0)
			{
				new CharityUI(m).Send();
			}

			DisplayTo(m);
		}

		public override bool TryDropItem(Mobile m, Item item, bool sendFullMessage)
		{
			return AddToContainer(m, item);
		}

		public override bool OnDragDropInto(Mobile m, Item item, Point3D p)
		{
			return AddToContainer(m, item);
		}

		public override bool OnDragDrop(Mobile m, Item item)
		{
			return AddToContainer(m, item);
		}

#if ServUO58
		public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, bool checkWeight, int plusItems, int plusWeight)
#else
		public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
#endif
		{
			var box = CharityContainer.Acquire(m);

			return box != null && !box.Deleted && box.CheckHold(m, item, message);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}
}
