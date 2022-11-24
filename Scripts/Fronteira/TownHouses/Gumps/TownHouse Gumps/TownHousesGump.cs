#if ServUO58
#define ServUOX
#endif

#region References
using System.Collections;

using Server;
using Server.Gumps;
using Server.Multis;
#endregion

namespace Knives.TownHouses
{
	public class TownHousesGump : GumpPlusLight
	{
		public enum ListPage
		{
			Town,
			House
		}

		public static void Initialize()
		{
			RUOVersion.AddCommand("TownHouses", AccessLevel.Counselor, OnHouses);
		}

		private static void OnHouses(CommandInfo info)
		{
			new TownHousesGump(info.Mobile);
		}

		private ListPage c_ListPage;
		private int c_Page;

		public TownHousesGump(Mobile m)
			: base(m, 100, 100)
		{
			m.CloseGump(typeof(TownHousesGump));
		}

		protected override void BuildGump()
		{
			var width = 400;
			var y = 0;

			AddHtml(0, y += 10, width, "<CENTER>Menu do Townhouses");
			AddImage(width / 2 - 120, y + 2, 0x39);
			AddImage(width / 2 + 90, y + 2, 0x3B);

			var pp = 10;

			if (c_Page != 0)
			{
				AddButton(width / 2 - 10, y += 25, 0x25E4, 0x25E5, "Page Down", PageDown);
			}

			var list = new ArrayList();
			if (c_ListPage == ListPage.Town)
			{
				list.AddRange(TownHouseSign.AllSigns);
			}
			else
			{
				list.AddRange(BaseHouse.AllHouses);
			}

			list.Sort(new InternalSort());

			AddHtml(
				0,
				y += 20,
				width,
				"<CENTER>" + (c_ListPage == ListPage.Town ? "TownHouses" : "Houses") + " Count: " + list.Count);
			AddHtml(0, y += 25, width, "<CENTER>TownHouses / Casas");
			AddButton(width / 2 - 100, y + 3, c_ListPage == ListPage.Town ? 0x939 : 0x2716, "Page", Page, ListPage.Town);
			AddButton(width / 2 + 90, y + 3, c_ListPage == ListPage.House ? 0x939 : 0x2716, "Page", Page, ListPage.House);

			TownHouseSign sign = null;
			BaseHouse house = null;

			y += 5;

			for (var i = c_Page * pp; i < (c_Page + 1) * pp && i < list.Count; ++i)
			{
				if (c_ListPage == ListPage.Town)
				{
					sign = (TownHouseSign)list[i];

					AddHtml(30, y += 20, width / 2 - 20, "<DIV ALIGN=LEFT>" + sign.Name);
					AddButton(15, y + 3, 0x2716, "TownHouse Menu", TownHouseMenu, sign);

					if (sign.House != null && sign.House.Owner != null)
					{
						AddHtml(width / 2, y, width / 2 - 40, "<DIV ALIGN=RIGHT>" + sign.House.Owner.RawName);
						AddButton(width - 30, y + 3, 0x2716, "House Menu", HouseMenu, sign.House);
					}
				}
				else
				{
					house = (BaseHouse)list[i];

					AddHtml(30, y += 20, width / 2 - 20, "<DIV ALIGN=LEFT>" + house.Name);
					AddButton(15, y + 3, 0x2716, "Goto", Goto, house);

					if (house.Owner != null)
					{
						AddHtml(width / 2, y, width / 2 - 40, "<DIV ALIGN=RIGHT>" + house.Owner.RawName);
						AddButton(width - 30, y + 3, 0x2716, "House Menu", HouseMenu, house);
					}
				}
			}

			if (pp * (c_Page + 1) < list.Count)
			{
				AddButton(width / 2 - 10, y += 25, 0x25E8, 0x25E9, "Page Up", PageUp);
			}

			if (c_ListPage == ListPage.Town)
			{
				AddHtml(0, y += 35, width, "<CENTER>Adicionar nova casa");
				AddButton(width / 2 - 80, y + 3, 0x2716, "New", New);
				AddButton(width / 2 + 70, y + 3, 0x2716, "New", New);
			}

			AddBackgroundZero(0, 0, width, y + 40, 0x13BE);
		}

		private void TownHouseMenu(object obj)
		{
			if (!(obj is TownHouseSign))
			{
				return;
			}

			NewGump();

			new TownHouseSetupGump(Owner, (TownHouseSign)obj);
		}

		private void Page(object obj)
		{
			c_ListPage = (ListPage)obj;
			NewGump();
		}

		private void Goto(object obj)
		{
			if (!(obj is BaseHouse))
			{
				return;
			}

			Owner.Location = ((BaseHouse)obj).BanLocation;
			Owner.Map = ((BaseHouse)obj).Map;

			NewGump();
		}

		private void HouseMenu(object obj)
		{
			if (!(obj is BaseHouse))
			{
				return;
			}

			NewGump();

#if ServUOX
			Owner.SendGump(new HouseGump(0, Owner, (BaseHouse)obj));
#else
			if (Core.AOS)
			{
				Owner.SendGump(new HouseGumpAOS(0, Owner, (BaseHouse)obj));
			}
			else
            {
                Owner.SendGump(new HouseGump(Owner, (BaseHouse)obj));
            }
#endif
		}

		private void New()
		{
			var sign = new TownHouseSign();
			Owner.AddToBackpack(sign);
			Owner.SendMessage("A placa nova esta na sua backpack.  Vai se mexer durante o processo. Caso nao o complete, talvez queira deletar a placa.");

			NewGump();

			new TownHouseSetupGump(Owner, sign);
		}

		private void PageUp()
		{
			c_Page++;
			NewGump();
		}

		private void PageDown()
		{
			c_Page--;
			NewGump();
		}

		private class InternalSort : IComparer
		{
			public int Compare(object x, object y)
			{
				if (x == null && y == null)
				{
					return 0;
				}

				if (x is TownHouseSign)
				{
					var a = (TownHouseSign)x;
					var b = (TownHouseSign)y;

					return Insensitive.Compare(a.Name, b.Name);
				}
				else
				{
					var a = (BaseHouse)x;
					var b = (BaseHouse)y;

					if (a.Owner == null && b.Owner != null)
					{
						return -1;
					}
					if (a.Owner != null && b.Owner == null)
					{
						return 1;
					}

					return Insensitive.Compare(a.Owner.RawName, b.Owner.RawName);
				}
			}
		}
	}
}
