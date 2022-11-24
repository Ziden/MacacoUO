#region References
using System.Linq;

using Server;
using Server.Multis;
#endregion

namespace Knives.TownHouses
{
	public class General
	{
		public static string Version { get { return "3.01"; } }

		// This setting determines the suggested gold value for a single square of a home
		//  which then derives price, lockdowns and secures.
		public static int SuggestionFactor { get { return 600; } }

		// This setting determines if players need License in order to rent out their property
		public static bool RequireRenterLicense { get { return false; } }

		public static void Configure()
		{
			EventSink.WorldSave += OnSave;
			EventSink.ServerStarted += OnStarted;

			EventSink.Login += OnLogin;
			EventSink.Speech += HandleSpeech;
		}

		private static void OnStarted()
		{
			var i = TownHouse.AllTownHouses.Count;

			while (--i >= 0)
			{
				if (i >= TownHouse.AllTownHouses.Count)
					continue;

				var h = TownHouse.AllTownHouses[i];

				h.InitSectorDefinition();

				RUOVersion.UpdateRegion(h.ForSaleSign);
			}
		}

		public static void OnSave(WorldSaveEventArgs e)
		{
			var i = TownHouseSign.AllSigns.Count;

			while (--i >= 0)
			{
				if (i >= TownHouseSign.AllSigns.Count)
					continue;

				var s = TownHouseSign.AllSigns[i];

				s.ValidateOwnership();
			}
		}

		private static void OnLogin(LoginEventArgs e)
		{
			var houses = BaseHouse.AllHouses.OfType<TownHouse>();

			foreach (var house in houses.Where(h => h.IsSameAccount(h.Owner, e.Mobile)))
			{
				house.ForSaleSign.CheckDemolishTimer();
			}
		}

		private static void HandleSpeech(SpeechEventArgs e)
		{
			try
			{
				var house = BaseHouse.FindHouseAt(e.Mobile);

				if (house == null)
				{
					return;
				}

				if (house is TownHouse)
				{
					house.OnSpeech(e);
				}

				if (Insensitive.Equals(e.Speech, "create rental contract") && CanRent(e.Mobile, house, true))
				{
					e.Mobile.AddToBackpack(new RentalContract());
					e.Mobile.SendMessage("A rental contract has been placed in your bag.");
				}
				else if (Insensitive.Equals(e.Speech, "check storage"))
				{
					int count;

					e.Mobile.SendMessage(
						"You have {0:#,0} lockdowns and {1:#,0} secures available.",
						RemainingSecures(house),
						RemainingLocks(house));

					if ((count = AllRentalLocks(house)) != 0)
					{
						e.Mobile.SendMessage("Current rentals are using {0:#,0} of your lockdowns.", count);
					}

					if ((count = AllRentalSecures(house)) != 0)
					{
						e.Mobile.SendMessage("Current rentals are using {0:#,0} of your secures.", count);
					}
				}
			}
			catch
			{ }
		}

		private static bool CanRent(Mobile m, BaseHouse house, bool say)
		{
			if (house is TownHouse && ((TownHouse)house).ForSaleSign.PriceType != "Sale")
			{
				if (say)
				{
					m.SendMessage("You must own your property to rent it.");
				}

				return false;
			}

			if (RequireRenterLicense)
			{
				var lic = m.Backpack.FindItemByType<RentalLicense>();

				if (lic != null && lic.Owner == null)
				{
					lic.Owner = m;
				}

				if (lic == null || lic.Owner != m)
				{
					if (say)
					{
						m.SendMessage("You must have a renter's license to rent your property.");
					}

					return false;
				}
			}

			if (EntireHouseContracted(house))
			{
				if (say)
				{
					m.SendMessage("This entire house already has a rental contract.");
				}

				return false;
			}

			if (RemainingSecures(house) < 0 || RemainingLocks(house) < 0)
			{
				if (say)
				{
					m.SendMessage("You don't have the storage available to rent property.");
				}

				return false;
			}

			return true;
		}

		#region Rental Info
		public static bool EntireHouseContracted(BaseHouse house)
		{
			return TownHouseSign.AllSigns.OfType<RentalContract>().Any(s => house == s.ParentHouse && s.EntireHouse);
		}

		public static bool HasContract(BaseHouse house)
		{
			return TownHouseSign.AllSigns.OfType<RentalContract>().Any(s => house == s.ParentHouse);
		}

		public static bool HasOtherContract(BaseHouse house, RentalContract c)
		{
			return TownHouseSign.AllSigns.OfType<RentalContract>().Any(s => s != c && house == s.ParentHouse && s.EntireHouse);
		}

		public static int RemainingSecures(BaseHouse house)
		{
			if (house == null)
			{
				return 0;
			}

			int total;

			if (Core.AOS)
			{
				int a, b, c, d;

				total = house.GetAosMaxSecures() - house.GetAosCurSecures(out a, out b, out c, out d);
			}
			else
			{
				total = house.MaxSecures - house.SecureCount;
			}

			return total - AllRentalSecures(house);
		}

		public static int RemainingLocks(BaseHouse house)
		{
			if (house == null)
			{
				return 0;
			}

			int total;

			if (Core.AOS)
			{
				total = house.GetAosMaxLockdowns() - house.GetAosCurLockdowns();
			}
			else
			{
				total = house.MaxLockDowns - house.LockDownCount;
			}

			return total - AllRentalLocks(house);
		}

		public static int AllRentalSecures(BaseHouse house)
		{
			return TownHouseSign.AllSigns.OfType<RentalContract>()
								.Where(s => s.ParentHouse == house)
								.Aggregate(0, (c, s) => c + s.Secures);
		}

		public static int AllRentalLocks(BaseHouse house)
		{
			return TownHouseSign.AllSigns.OfType<RentalContract>()
								.Where(s => s.ParentHouse == house)
								.Aggregate(0, (c, s) => c + s.Locks);
		}
		#endregion
	}
}
