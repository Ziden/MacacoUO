using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBElfTailor : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBElfTailor()
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
                Add(new GenericBuyInfo(typeof(ElvenBoots), 10000, 999, 0x2FC4, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Scissors), 6);
                Add(typeof(SewingKit), 1);
                Add(typeof(Dyes), 4);
                Add(typeof(DyeTub), 4);

                Add(typeof(BoltOfCloth), 50);
                Add(typeof(BoltOfOilCloth), 20);
                //Add(typeof(Cloth), 1);
                //Add(typeof(UncutCloth), 1);

                Add(typeof(FancyShirt), 10);
                Add(typeof(Shirt), 6);

                Add(typeof(ShortPants), 3);
                Add(typeof(LongPants), 5);

                Add(typeof(Cloak), 4);
                Add(typeof(FancyDress), 12);
                Add(typeof(Robe), 9);
                Add(typeof(PlainDress), 7);

                Add(typeof(Skirt), 5);
                Add(typeof(Kilt), 5);

                Add(typeof(Doublet), 7);
                Add(typeof(Tunic), 9);
                Add(typeof(JesterSuit), 13);

                Add(typeof(FullApron), 5);
                Add(typeof(HalfApron), 5);

                Add(typeof(JesterHat), 6);
                Add(typeof(FloppyHat), 3);
                Add(typeof(WideBrimHat), 4);
                Add(typeof(Cap), 5);
                Add(typeof(SkullCap), 3);
                Add(typeof(Bandana), 3);
                Add(typeof(TallStrawHat), 4);
                Add(typeof(StrawHat), 4);
                Add(typeof(WizardsHat), 5);
                Add(typeof(Bonnet), 4);
                Add(typeof(FeatheredHat), 5);
                Add(typeof(TricorneHat), 4);

                Add(typeof(SpoolOfThread), 9);

                Add(typeof(Flax), 21);
                Add(typeof(Cotton), 21);
                Add(typeof(Wool), 11);
            }
        }
    }
}
