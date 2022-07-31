using System;

using Server.Accounting;

using VitaNex;


namespace Server.Items
{
	public sealed class CharityRecord : PropertyObject
	{
		private static int _UID;

		[CommandProperty(Charity.Access)]
		public int UID { get; private set; }

		[CommandProperty(Charity.Access)]
		public DateTime Date { get; private set; }

		[CommandProperty(Charity.Access)]
		public string ItemType { get; private set; }

		[CommandProperty(Charity.Access)]
		public int ItemAsset { get; private set; }

		[CommandProperty(Charity.Access)]
		public int ItemHue { get; private set; }

		[CommandProperty(Charity.Access)]
		public Serial ItemSerial { get; private set; }

		[CommandProperty(Charity.Access)]
		public string ItemName { get; private set; }

		[CommandProperty(Charity.Access)]
		public IAccount GiverAccount { get; private set; }

		[CommandProperty(Charity.Access)]
		public Serial GiverSerial { get; private set; }

		[CommandProperty(Charity.Access)]
		public string GiverName { get; private set; }

		[CommandProperty(Charity.Access)]
		public IAccount ReceiverAccount { get; private set; }

		[CommandProperty(Charity.Access)]
		public Serial ReceiverSerial { get; private set; }

		[CommandProperty(Charity.Access)]
		public string ReceiverName { get; private set; }

		[CommandProperty(Charity.Access)]
		public int Amount { get; private set; }

		public CharityRecord(DateTime date, Item item, Mobile giver, Mobile receiver, int amount)
		{
			UID = ++_UID;

			Date = date;

			if (item != null)
			{
				ItemType = item.GetTypeName(true);

				ItemAsset = item.ItemID;
				ItemHue = item.Hue;

				ItemSerial = item.Serial;
				ItemName = item.ResolveName();
			}

			if (giver != null)
			{
				GiverAccount = giver.Account;
				GiverSerial = giver.Serial;
				GiverName = giver.RawName;
			}

			if (receiver != null)
			{
				ReceiverAccount = receiver.Account;
				ReceiverSerial = receiver.Serial;
				ReceiverName = receiver.RawName;
			}

			Amount = amount;
		}

		public CharityRecord(GenericReader reader)
			: base(reader)
		{
			_UID = Math.Max(_UID, UID);
		}

		public override int GetHashCode()
		{
			return UID;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(UID);

			writer.Write(Date);
			
			writer.Write(ItemType);

			writer.Write(ItemAsset);
			writer.Write(ItemHue);

			writer.Write(ItemSerial);
			writer.Write(ItemName);

			writer.Write(GiverAccount);
			writer.Write(GiverSerial);
			writer.Write(GiverName);

			writer.Write(ReceiverAccount);
			writer.Write(ReceiverSerial);
			writer.Write(ReceiverName);

			writer.Write(Amount);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			UID = reader.ReadInt();

			Date = reader.ReadDateTime();
			
			ItemType = reader.ReadString();

			ItemAsset = reader.ReadInt();
			ItemHue = reader.ReadInt();

#if ServUO58
			ItemSerial = reader.ReadSerial();
			ItemName = reader.ReadString();

			GiverAccount = reader.ReadAccount();
			GiverSerial = reader.ReadSerial();
			GiverName = reader.ReadString();

			ReceiverAccount = reader.ReadAccount();
			ReceiverSerial = reader.ReadSerial();
			ReceiverName = reader.ReadString();
#else
			ItemSerial = reader.ReadInt();
			ItemName = reader.ReadString();

			GiverAccount = reader.ReadAccount();
			GiverSerial = reader.ReadInt();
			GiverName = reader.ReadString();

			ReceiverAccount = reader.ReadAccount();
			ReceiverSerial = reader.ReadInt();
			ReceiverName = reader.ReadString();
#endif

			Amount = reader.ReadInt();
		}
	}
}
