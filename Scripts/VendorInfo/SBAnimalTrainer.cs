using Server.Items;
using Shrink.ShrinkSystem;
using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class SBAnimalTrainer : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBAnimalTrainer()
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
                if(!Shard.RP)
                {
                    Add(new AnimalBuyInfo(1, typeof(Horse), 600, 10, 204, 0));
                    Add(new AnimalBuyInfo(1, typeof(PackHorse), 600, 10, 291, 0));
                    Add(new AnimalBuyInfo(1, typeof(Llama), 2000, 10, 0xDC, 0));
                    Add(new AnimalBuyInfo(1, typeof(ForestOstard), 5000, 10, 0xDB, 0));
                    Add(new AnimalBuyInfo(1, typeof(Eagle), 402, 10, 5, 0));
                    Add(new AnimalBuyInfo(1, typeof(BrownBear), 855, 10, 167, 0));
                    Add(new AnimalBuyInfo(1, typeof(GrizzlyBear), 1767, 10, 212, 0));
                    Add(new AnimalBuyInfo(1, typeof(TimberWolf), 768, 10, 225, 0));
                } else
                {
                    Add(new AnimalBuyInfo(1, typeof(Horse), 5000, 10, 204, 0));
                }
                Add(new AnimalBuyInfo(1, typeof(Rat), 107, 10, 238, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
            }
        }
    }
}
