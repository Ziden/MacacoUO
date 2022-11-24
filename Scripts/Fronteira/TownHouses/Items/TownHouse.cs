#if ServUO58
#define ServUOX
#endif

#region References
using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Multis;
using Server.Targeting;
#endregion

namespace Knives.TownHouses
{
	public class TownHouse : VersionHouse
	{
		public static List<TownHouse> AllTownHouses { get; private set; }

		static TownHouse()
		{
			AllTownHouses = new List<TownHouse>();
		}

		private readonly List<Sector> _Sectors = new List<Sector>();

		private Item _Hanger;

		public Item Hanger
		{
			get
			{
				return _Hanger ?? (_Hanger = new Item(0xB98)
				{
					Movable = false,
					Location = Sign.Location,
					Map = Sign.Map
				});
			}
			set { _Hanger = value; }
		}

		public TownHouseSign ForSaleSign { get; private set; }

		public override Rectangle2D[] Area
		{
			get
			{
				if (ForSaleSign == null)
				{
					return new Rectangle2D[100];
				}

				var rects = new Rectangle2D[ForSaleSign.Blocks.Count];

				for (var i = 0; i < ForSaleSign.Blocks.Count && i < rects.Length; ++i)
				{
					rects[i] = (Rectangle2D)ForSaleSign.Blocks[i];
				}

				return rects;
			}
		}

		public TownHouse(Mobile m, TownHouseSign sign, int locks, int secures)
			: base(0x1DD6 | 0x4000, m, locks, secures)
		{
			ForSaleSign = sign;

			SetSign(0, 0, 0);

			AllTownHouses.Add(this);
		}

		public TownHouse(Serial serial)
			: base(serial)
		{
			AllTownHouses.Add(this);
		}

		public void InitSectorDefinition()
		{
			if (ForSaleSign == null || ForSaleSign.Blocks.Count == 0)
			{
				return;
			}

			var blocks = (Rectangle2D)ForSaleSign.Blocks[0];

			var minX = blocks.Start.X;
			var minY = blocks.Start.Y;
			var maxX = blocks.End.X;
			var maxY = blocks.End.Y;

			foreach (Rectangle2D rect in ForSaleSign.Blocks)
			{
				if (rect.Start.X < minX)
				{
					minX = rect.Start.X;
				}

				if (rect.Start.Y < minY)
				{
					minY = rect.Start.Y;
				}

				if (rect.End.X > maxX)
				{
					maxX = rect.End.X;
				}

				if (rect.End.Y > maxY)
				{
					maxY = rect.End.Y;
				}
			}

			foreach (var sector in _Sectors)
			{
				sector.OnMultiLeave(this);
			}

			_Sectors.Clear();

			for (var x = minX; x < maxX; ++x)
			{
				for (var y = minY; y < maxY; ++y)
				{
					var s = Map.GetSector(new Point2D(x, y));

					if (!_Sectors.Contains(s))
						_Sectors.Add(s);
				}
			}

			foreach (var sector in _Sectors)
			{
				sector.OnMultiEnter(this);
			}

			Components.Resize(maxX - minX, maxY - minY);
			Components.Add(0x520, Components.Width - 1, Components.Height - 1, -5);
		}

		public override bool IsInside(Point3D p, int height)
		{
			if (ForSaleSign == null)
			{
				return false;
			}

			if (Map == null || Region == null)
			{
				Delete();
				return false;
			}

			Sector sector = null;

			try
			{
				if (ForSaleSign is RentalContract && Region.Contains(p))
				{
					return true;
				}

				sector = Map.GetSector(p);

				if (
					sector.Multis.Any(
						m =>
							m != this && m is TownHouse && ((TownHouse)m).ForSaleSign is RentalContract && ((TownHouse)m).IsInside(p, height)))
				{
					return false;
				}

				return Region.Contains(p);
			}
			catch (Exception e)
			{
				Errors.Report("Error occured in IsInside().  More information on the console.");
				Console.WriteLine("Info:{0}, {1}, {2}, {3}", Map, sector, Region, sector != null ? "" + sector.Multis : "**");
				Console.WriteLine(e.Source);
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
		}

		public static int MaxVendors = 50;

#if ServUOX
		public override int GetVendorSystemMaxVendors()
		{
			return MaxVendors;
		}
#else
        public override int GetNewVendorSystemMaxVendors()
        {
            return MaxVendors;
        }
#endif

        public override int GetAosMaxSecures()
		{
			return MaxSecures;
		}

		public override int GetAosMaxLockdowns()
		{
			return MaxLockDowns;
		}

		public override void OnMapChange()
		{
			base.OnMapChange();

			if (_Hanger != null)
			{
				_Hanger.Map = Map;
			}
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			base.OnLocationChange(oldLocation);

			if (_Hanger != null)
			{
				_Hanger.Location = Sign.Location;
			}
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (e.Mobile != Owner || !IsInside(e.Mobile))
			{
				return;
			}

			if (e.Speech.ToLower() == "check house rent")
			{
				ForSaleSign.CheckRentTimer();
			}

			Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(AfterSpeech), e.Mobile);
		}

		private void AfterSpeech(object o)
		{
			if (!(o is Mobile))
			{
				return;
			}

			var m = (Mobile)o;

			if (!(m.Target is HouseBanTarget) || ForSaleSign == null || !ForSaleSign.NoBanning)
			{
				return;
			}

			m.Target.Cancel(m, TargetCancelType.Canceled);
			m.SendMessage(0x161, "Voce nao pode banir ninguem dessa casa.");
		}

		public override void OnDelete()
		{
			if (_Hanger != null)
			{
				_Hanger.Delete();
			}

			foreach (var item in Sign.GetItemsInRange(0).Where(item => item != Sign))
			{
				item.Visible = true;
			}

			ForSaleSign.ClearHouse();
			Doors.Clear();

			AllTownHouses.Remove(this);

			base.OnDelete();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			AllTownHouses.Remove(this);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(4);

			// Version 2

			writer.Write(_Hanger);

			// Version 1

			writer.Write(ForSaleSign);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			if (version >= 2)
			{
				_Hanger = reader.ReadItem();
			}

			ForSaleSign = reader.ReadItem<TownHouseSign>();

			if (version <= 2)
			{
				var count = reader.ReadInt();
				for (var i = 0; i < count; ++i)
				{
					reader.ReadRect2D();
				}
			}

			ItemID = 0x1DD6 | 0x4000;

			Price = Math.Max(1, Price);
		}
	}
}
