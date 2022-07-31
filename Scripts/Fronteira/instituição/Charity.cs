using System;
using System.Collections.Generic;
using System.Linq;

using Server.Accounting;
using Server.Mobiles;

using VitaNex;
using VitaNex.IO;
using VitaNex.Items;


namespace Server.Items
{
	[CoreModule("Contêiner de Caridade", "0.1")]
	public static class Charity
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public static CharityOptions CMOptions { get; private set; }

		public static Dictionary<string, Type[]> Categories { get; private set; }

		public static Dictionary<Item, CharityState> Items { get; private set; }

		public static BinaryDataStore<IAccount, CharityProfile> Profiles { get; private set; }

		public static List<CharityContainer> Chests { get { return CharityContainer.Instances; } }

		static Charity()
		{
			CMOptions = new CharityOptions();

			Categories = new Dictionary<string, Type[]>
			{
				{"Currencies", new[] {typeof(IVendorToken)}},

				//{"Pets", new[] {typeof(ShrinkItem)}},
				{"Pets|Ethereal", new[] {typeof(EtherealMount)}},

                {"Spells|Reagents", new[] {typeof(BaseReagent)}},
				{"Spells|Scrolls", new[] {typeof(SpellScroll)}},
				{"Spells|Wands", new[] {typeof(BaseWand)}},
				
                {"Resources|Granite", new[] {typeof(BaseGranite)}},
				{"Resources|Leather", new[] {typeof(BaseHides), typeof(BaseLeather)}},
				{"Resources|Metal", new[] {typeof(BaseOre), typeof(BaseIngot)}},
				{"Resources|Scales", new[] {typeof(BaseScales)}},
				
#if ServUO
				{"Resources|Wood", new[] {typeof(BaseLog), typeof(BaseWoodBoard)}},
#else
				{"Resources|Wood", new[] {typeof(BaseLog), typeof(BaseBoard)}},
#endif

				{"Tools|Crafting", new[] {typeof(BaseTool)}},
				{"Tools|Harvesting", new[] {typeof(BaseHarvestTool)}},
				{"Tools|Runic", new[] {typeof(BaseRunicTool)}},

				{"Equipment|Armor", new[] {typeof(BaseArmor)}},
				{"Equipment|Weapon", new[] {typeof(BaseWeapon)}},
				{"Equipment|Clothing", new[] {typeof(BaseClothing)}},
				{"Equipment|Jewelry", new[] {typeof(BaseJewel)}},
				{"Equipment|Talisman", new[] {typeof(BaseTalisman)}},
				{"Equipment|Quiver", new[] {typeof(BaseQuiver)}},
				{"Equipment|Wand", new[] {typeof(BaseWand)}},
				{"Equipment|Light", new[] {typeof(BaseEquipableLight)}}
            };

			Items = new Dictionary<Item, CharityState>();

			Profiles = new BinaryDataStore<IAccount, CharityProfile>(
				VitaNexCore.SavesDirectory + "/Charity",
				"Profiles")
			{
				Async = true,
				OnSerialize = SerializeProfiles,
				OnDeserialize = DeserializeProfiles
			};
		}

		private static void CMConfig()
		{
			EventSink.DeleteRequest += OnDeleteRequest;
		}

		private static void CMSave()
		{
			Profiles.Export();
		}

		private static void CMLoad()
		{
			Profiles.Import();

			foreach (var p in Profiles.Values)
			{
				foreach (var o in p.Stock.Where(o => o.Item != null))
				{
					Items[o.Item] = o;
				}

				foreach (var c in p.Account.FindMobiles().SelectMany(m => m.Items.OfType<CharityContainer>()))
				{
					foreach (var o in c.Items)
					{
						if (!Items.TryGetValue(o, out var state) || state == null)
						{
							Items[o] = new CharityState(c, o);
						}
					}
				}
			}
		}

		private static bool SerializeProfiles(GenericWriter writer)
		{
			writer.SetVersion(0);

			writer.WriteBlockDictionary(Profiles, (w, k, v) => v.Serialize(w));

			return true;
		}

		private static bool DeserializeProfiles(GenericReader reader)
		{
			reader.GetVersion();

			reader.ReadBlockDictionary(
				r =>
				{
					var o = new CharityProfile(r);

					return new KeyValuePair<IAccount, CharityProfile>(o.Account, o);
				},
				Profiles);

			return true;
		}

		public static IEnumerable<string> FindCategories(Item item)
		{
			if (item == null)
			{
				return Enumerable.Empty<string>();
			}

			return FindCategories(item.GetType());
		}

		public static IEnumerable<string> FindCategories(Type item)
		{
			if (item == null)
			{
				yield break;
			}

			foreach (var o in Categories)
			{
				if (o.Value.Any(item.IsEqualOrChildOf))
				{
					yield return o.Key;
				}
			}
		}

		public static bool HasCategory(Item item)
		{
			return item != null && HasCategory(item.GetType());
		}

		public static bool HasCategory(Type item)
		{
			if (item == null)
			{
				return false;
			}

			foreach (var o in Categories)
			{
				if (o.Value.Any(item.IsEqualOrChildOf))
				{
					return true;
				}
			}

			return false;
		}

		public static bool InCategory(Item item, string category)
		{
			return item != null && InCategory(item.GetType(), category);
		}

		public static bool InCategory(Type item, string category)
		{
			if (item == null)
			{
				return false;
			}

			if (Categories.ContainsKey(category))
			{
				return Categories[category].Any(item.IsEqualOrChildOf);
			}

			return FindCategories(item).Any(o => o.StartsWith(category, StringComparison.OrdinalIgnoreCase));
		}

		public static CharityProfile AcquireProfile(Mobile m)
		{
			if (m != null && m.Account != null)
			{
				return AcquireProfile(m.Account);
			}

			return null;
		}

		public static CharityProfile AcquireProfile(IAccount a)
		{
			if (a == null)
			{
				return null;
			}

			if (!Profiles.TryGetValue(a, out var profile) || profile == null)
			{
				Profiles[a] = profile = new CharityProfile(a);
			}

			return profile;
		}

		public static CharityProfile FindProfile(Mobile m)
		{
			if (m != null && m.Account != null)
			{
				return FindProfile(m.Account);
			}

			return null;
		}

		public static CharityProfile FindProfile(IAccount a)
		{
			if (a == null)
			{
				return null;
			}

			return Profiles.GetValue(a);
		}

		public static bool DestroyState(Item item)
		{
			if (item == null)
			{
				return false;
			}

			var state = Items.GetValue(item);

			if (state != null)
			{
				state.Dispose();
				return true;
			}

			return false;
		}

		public static CharityState FindState(Item item)
		{
			return item != null ? Items.GetValue(item) : null;
		}

		public static CharityState AcquireState(Item item)
		{
			if (item == null || item.Deleted || !HasCategory(item))
			{
				return null;
			}

			if (!Items.TryGetValue(item, out var state) || state == null)
			{
				CharityContainer c = null;

				var p = item.Parent as Item;

				while (p != null)
				{
					if (p is CharityContainer)
					{
						c = (CharityContainer)p;
						break;
					}

					p = p.Parent as Item;
				}

				if (c != null)
				{
					Items[item] = state = new CharityState(c, item);

					var profile = AcquireProfile(c.Owner);

					if (profile != null)
					{
						profile.Stock.Add(state);
					}
				}
			}

			return state;
		}

		public static void CreateRecords(Item item, Mobile giver, Mobile receiver, int amount)
		{
			var date = DateTime.UtcNow;

			var sp = AcquireProfile(giver);

			if (sp != null)
			{
				sp.CreateRecord(date, item, giver, receiver, amount);
			}

			var bp = AcquireProfile(receiver);

			if (bp != null)
			{
				bp.CreateRecord(date, item, giver, receiver, amount);
			}
		}
		
		private static void OnDeleteRequest(DeleteRequestEventArgs e)
		{
			if (e.State == null || e.State.Account == null || e.Index < 0 || e.Index >= e.State.Account.Length)
			{
				return;
			}

			var d = e.State.Account[e.Index];

			if (d == null)
			{
				return;
			}

			var m = e.State.Account.FindMobiles<PlayerMobile>(o => o != d).Highest(o => o.GameTime);

			if (m != null)
			{
				var source = CharityContainer.Acquire(d);
				var target = CharityContainer.Acquire(m);

				if (source != null && target != null)
				{
					source.Relocate(target);
				}
			}
		}
	}
}
