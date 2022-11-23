using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
	public class SBStockman : SBInfo
	{
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBStockman()
		{
		}

        public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
        public override List<GenericBuyInfo> BuyInfo
        {
            get
            {
                return m_BuyInfo;
            }
        }

        public class InternalBuyInfo : List<GenericBuyInfo>
		{
			public InternalBuyInfo()
			{
				Add( new GenericBuyInfo( typeof( RanchDeed ), 50000, 5, 0x14F0 , 0 ) );
				Add( new GenericBuyInfo( typeof( RanchExtensionDeed ), 30000, 10, 0x14F0 , 0 ) );
				Add( new GenericBuyInfo( typeof( NorthGate ), 200, 50, 0x867 , 0 ) );
				Add( new GenericBuyInfo( typeof( WestGate ), 200, 50, 0x866, 0 ) );
				Add( new GenericBuyInfo( typeof( Fence ), 100, 50, 0x88B , 0 ) );
				Add( new GenericBuyInfo( typeof( FenceCorner ), 200, 5, 0x862 , 0 ) );
				Add( new GenericBuyInfo( typeof( FencePost), 20, 10, 0x85F , 0 ) );
				Add( new GenericBuyInfo( typeof( Bridle), 50, 10, 0x1374 , 0 ) );
				Add( new GenericBuyInfo( typeof( Saddlebag), 200, 10, 0x9B2 , 0 ) );
				Add( new GenericBuyInfo( typeof( FencingKit), 1000, 5, 0x1EBA , 0 ) );
				Add( new GenericBuyInfo( typeof( MilkingBucket), 20, 5, 0x14E0 , 0 ) );
				Add( new GenericBuyInfo( typeof( MilkKeg), 100, 5, 6870 , 2500 ) );
				Add( new GenericBuyInfo( typeof( CreamPitcher), 15, 5, 0x9D6 , 1191 ) );
				Add( new GenericBuyInfo( typeof( ButterChurn), 50, 5, 0x24D8 , 0 ) );
				Add( new GenericBuyInfo( typeof( MeasuringCup), 20, 5, 0x1F81 , 0 ) );
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
				Add( typeof( RanchDeed ), 145000 );
				Add( typeof( RanchExtensionDeed ), 75000 );
				Add( typeof( NorthGate ), 50 );
				Add( typeof( WestGate ), 50 );
				Add( typeof( Fence ), 20 );
				Add( typeof( FenceCorner ), 25 );
				Add( typeof( FencePost ), 10 );
				Add( typeof( Bridle ), 10 );
				Add( typeof( Saddlebag ), 20 );
				Add( typeof( FencingKit ), 50 );
				Add( typeof( FencingKit ), 5 );
				Add( typeof( MilkingBucket ), 5);
				Add( typeof( MilkKeg ), 50);
				Add( typeof( CreamPitcher ), 5);
				Add( typeof( ButterChurn ), 20);
				Add( typeof( MeasuringCup ), 10);
			}
		}
	}
}
