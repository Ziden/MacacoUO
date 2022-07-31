#region Header
//   Vorspire    _,-'/-'/  CharityOptions.cs
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

using VitaNex;
#endregion

namespace Server.Items
{
	public sealed class CharityOptions : CoreModuleOptions
	{
		[CommandProperty(Charity.Access)]
		public int StockLimit { get; set; }

		[CommandProperty(Charity.Access)]
		public TimeSpan AgeLimit { get; set; }

		public CharityOptions()
			: base(typeof(Charity))
		{
			SetDefaults();
		}

		public CharityOptions(GenericReader reader)
			: base(reader)
		{ }

		public void SetDefaults()
		{
			StockLimit = 50;
			AgeLimit = Accounting.Account.YoungDuration;
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

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(1);
			
			writer.Write(StockLimit);
			writer.Write(AgeLimit);
		}

		public override void Deserialize(GenericReader reader)
		{
			SetDefaults();

			base.Deserialize(reader);

			var v = reader.GetVersion();
			
			StockLimit = reader.ReadInt();

			if (v >= 1)
				AgeLimit = reader.ReadTimeSpan();
		}
	}
}
