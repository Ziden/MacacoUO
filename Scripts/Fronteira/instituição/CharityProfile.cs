#region Header
//   Vorspire    _,-'/-'/  CharityProfile.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Linq;

using Server.Accounting;

using VitaNex;
#endregion

namespace Server.Items
{
	public sealed class CharityProfile : PropertyObject
	{
		[CommandProperty(Charity.Access)]
		public IAccount Account { get; private set; }

		[CommandProperty(Charity.Access)]
		public List<CharityRecord> Records { get; private set; }

		[CommandProperty(Charity.Access)]
		public List<CharityState> Stock { get; private set; }

		public CharityProfile(IAccount account)
		{
			Account = account;

			Records = new List<CharityRecord>();
			Stock = new List<CharityState>();
		}

		public CharityProfile(GenericReader reader)
			: base(reader)
		{ }

		private void SetDefaults()
		{
			Records.Clear();
		}

		public override void Clear()
		{
			base.Clear();

			SetDefaults();
		}

		public override void Reset()
		{
			base.Reset();

			SetDefaults();
		}

		public IEnumerable<CharityRecord> GetGiven()
		{
			return Records.Where(o => o.GiverAccount == Account);
		}

		public IEnumerable<CharityRecord> GetReceived()
		{
			return Records.Where(o => o.ReceiverAccount == Account);
		}

		public void CreateRecord(DateTime date, Item item, Mobile giver, Mobile receiver, int amount)
		{
			Records.Add(new CharityRecord(date, item, giver, receiver, amount));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(Account);

			writer.WriteBlockList(Records, (w, o) => o.Serialize(w));
			writer.WriteBlockList(Stock, (w, o) => o.Serialize(w));
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			Account = reader.ReadAccount();

			Records = reader.ReadBlockList(r => new CharityRecord(r), Records);
			Stock = reader.ReadBlockList(r => new CharityState(r), Stock);
		}
	}
}