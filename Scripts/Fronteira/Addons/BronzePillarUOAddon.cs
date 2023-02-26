using System;

using Server;
using Server.Items;

namespace Server.Items
{
	public class BronzePillarUOAddon : BaseAddon
	{
		private static readonly int[,] _SimpleComponents = 
		{
			{17097, 0, 1, 0}, {17098, 1, 0, 0}
		};
		

		public override BaseAddonDeed Deed => new BronzePillarUOAddonDeed();

		[Constructable]
		public BronzePillarUOAddon()
		{
			for (int i = 0; i < _SimpleComponents.Length / 4; i++)
				AddComponent(new AddonComponent(_SimpleComponents[i, 0]), _SimpleComponents[i, 1], _SimpleComponents[i, 2], _SimpleComponents[i, 3]);
			
			AddComponent(3633, 0, 0, 19, 0, 1, "", 1);
			AddComponent(14089, 0, 0, 21, 0, 30, "", 1);
			AddComponent(92, 0, -1, 0, 1050, 44, "", 1);
			AddComponent(91, -1, 0, 0, 1050, 44, "", 1);
			AddComponent(89, 0, 0, 0, 1050, -1, "", 1);
			AddComponent(90, -1, -1, 0, 1050, -1, "", 1);
		}

		public BronzePillarUOAddon(Serial serial)
			: base(serial)
		{ }

		private void AddComponent(int item, int xoffset, int yoffset, int zoffset, int hue, int light)
		{
			AddComponent(item, xoffset, yoffset, zoffset, hue, light, null, 1);
		}

		private void AddComponent(int item, int xoffset, int yoffset, int zoffset, int hue, int light, string name, int amount)
		{
			AddonComponent ac = new AddonComponent(item);

			if (!string.IsNullOrWhiteSpace(name))
				ac.Name = name;

			if (hue >= 0)
				ac.Hue = hue;

			if (light >= 0)
				ac.Light = (LightType)light;

			if (amount > 1)
			{
				ac.Stackable = true;
				ac.Amount = amount;
			}

			AddComponent(ac, xoffset, yoffset, zoffset);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}
	}

	public class BronzePillarUOAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new BronzePillarUOAddon();

		[Constructable]
		public BronzePillarUOAddonDeed()
		{
			Name = "Bronze Pillar UO";
		}

		public BronzePillarUOAddonDeed(Serial serial)
			: base( serial )
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}
	}
}