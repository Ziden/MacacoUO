#if ServUO58
#define ServUOX
#endif

#region References
using System;
using System.Collections.Generic;

using Server;
using Server.Commands;
using Server.Multis;
#endregion

namespace Knives.TownHouses
{
	public class RUOVersion
	{
		private static readonly Dictionary<string, TownHouseCommandHandler> s_Commands =
			new Dictionary<string, TownHouseCommandHandler>(StringComparer.OrdinalIgnoreCase);

		public static void AddCommand(string com, AccessLevel acc, TownHouseCommandHandler cch)
		{
			s_Commands[com.ToLower()] = cch;

			CommandSystem.Register(com, acc, OnCommand);
		}

		public static void OnCommand(CommandEventArgs e)
		{
			if (s_Commands[e.Command] != null)
			{
				s_Commands[e.Command](new CommandInfo(e.Mobile, e.Command, e.ArgString, e.Arguments));
			}
		}

		public static void UpdateRegion(TownHouseSign sign)
		{
			if (sign.House == null)
			{
				return;
			}

			sign.House.UpdateRegion();

			Rectangle3D rect;

			for (var i = 0; i < sign.House.Region.Area.Length; ++i)
			{
				rect = sign.House.Region.Area[i];

				sign.House.Region.Area[i] =
					new Rectangle3D(
						new Point3D(rect.Start.X - sign.House.X, rect.Start.Y - sign.House.Y, sign.MinZ),
						new Point3D(rect.End.X - sign.House.X, rect.End.Y - sign.House.Y, sign.MaxZ));
			}

			sign.House.Region.Unregister();
			sign.House.Region.Register();
			sign.House.Region.GoLocation = sign.BanLoc;
		}

		public static bool RegionContains(Region region, Mobile m)
		{
			return m != null && m.Region.IsPartOf(region);
		}

		public static Rectangle3D[] RegionArea(Region region)
		{
			return region.Area;
		}
	}

	public class VersionHouse : BaseHouse
	{
		public override Rectangle2D[] Area { get { return new Rectangle2D[5]; } }

		public override Point3D BaseBanLocation { get { return Point3D.Zero; } }

		public VersionHouse(int id, Mobile m, int locks, int secures)
			: base(id, m, locks, secures)
		{ }

		public VersionHouse(Serial serial)
			: base(serial)
		{ }

#if !ServUOX
		public override Server.Multis.Deeds.HouseDeed GetDeed()
		{
			return null;
		}
#endif

		// ReSharper disable RedundantOverridenMember
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
		}
        // ReSharper restore RedundantOverridenMember
	}
}
