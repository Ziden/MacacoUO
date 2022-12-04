using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class SBHerbalist : SBInfo 
    { 
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBHerbalist() 
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
                Add(new GenericBuyInfo(typeof(Boline), 200, 20, 0x26BB, 0));
                Add(new GenericBuyInfo(typeof(MortarPestle), 8, 20, 0xE9B, 0));
               // Add(new GenericBuyInfo(typeof(Ginseng), 6, 999, 0xF85, 0)); 
                //Add(new GenericBuyInfo(typeof(Garlic), 6, 999, 0xF84, 0)); 
                Add(new GenericBuyInfo(typeof(MandrakeRoot), 5, 999, 0xF86, 0)); 
                //Add(new GenericBuyInfo(typeof(Nightshade), 6, 999, 0xF88, 0)); 
                Add(new GenericBuyInfo(typeof(Bloodmoss), 7, 999, 0xF7B, 0)); 
                Add(new GenericBuyInfo(typeof(Bottle), 10, 999, 0xF0E, 0, true)); 
            }
        }

        public class InternalSellInfo : GenericSellInfo 
        { 
            public InternalSellInfo() 
            { 
                Add(typeof(Bloodmoss), 2); 
               // Add(typeof(MandrakeRoot), 2); 
               // Add(typeof(Garlic), 2); 
                //Add(typeof(Ginseng), 2); 
                //Add(typeof(Nightshade), 2); 
                Add(typeof(Bottle), 2); 
                Add(typeof(MortarPestle), 2); 
            }
        }
    }
}
