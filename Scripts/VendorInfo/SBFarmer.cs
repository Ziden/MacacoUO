using System;
using System.Collections.Generic;
using Server.Items;
using Server.Items.Crops;

namespace Server.Mobiles 
{ 
    public class SBFarmer : SBInfo 
    { 
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBFarmer() 
        { 
        }

        public override IShopSellInfo SellInfo
        {
            get
            {
                return m_SellInfo;
            }
        }
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

                /*

                */
                Add(new GenericBuyInfo(typeof(WinecraftersTools), 600, 60, 0xF00, 0x530, true));
                Add(new GenericBuyInfo(typeof(OnionSeed), 15, 60, 0x0F24, 0, true));
                Add(new GenericBuyInfo(typeof(CarrotSeed), 15, 60, 0x0F24, 0, true));
                Add(new GenericBuyInfo(typeof(LettuceSeed), 15, 60, 0x0F24, 0, true));
                Add(new GenericBuyInfo(typeof(CottonSeeds), 15, 60, 0x0F24, 0, true));
                Add(new GenericBuyInfo(typeof(NightshadeSeeds), 15, 60, 0x0F24, 0, true));
                Add(new GenericBuyInfo(typeof(GarlicSeeds), 15, 60, 0x0F24, 0, true));
                Add(new GenericBuyInfo(typeof(GinsengSeeds), 15, 60, 0x0F24, 0, true));
                Add(new GenericBuyInfo(typeof(MandrakeSeeds), 15, 60, 0x0F24, 0, true));
                Add(new GenericBuyInfo(typeof(FarmersSeedBox), 700, 60, 0x9AA, 0, true));
            }
        }

        public class InternalSellInfo : GenericSellInfo 
        { 
            public InternalSellInfo() 
            { 
                Add(typeof(Pitcher), 5);
                Add(typeof(Eggs), 1);
                Add(typeof(Apple), 1);
                Add(typeof(Grapes), 1);
                Add(typeof(Watermelon), 3);
                Add(typeof(YellowGourd), 1);
                Add(typeof(GreenGourd), 1);
                Add(typeof(Pumpkin), 5);
                Add(typeof(Onion), 1);
                Add(typeof(Lettuce), 2);
                Add(typeof(Squash), 1);
                Add(typeof(Carrot), 1);
                Add(typeof(HoneydewMelon), 3);
                Add(typeof(Cantaloupe), 3);
                Add(typeof(Cabbage), 2);
                Add(typeof(Lemon), 1);
                Add(typeof(Lime), 1);
                Add(typeof(Peach), 1);
                Add(typeof(Pear), 1);
                Add(typeof(SheafOfHay), 1);
                Add(typeof(GarlicSeeds), 9);
                Add(typeof(MandrakeSeeds), 9);
                Add(typeof(CottonSeeds), 9);
                Add(typeof(NightshadeSeeds), 9);
                Add(typeof(GinsengSeeds), 9);
            }
        }
    }
}
