using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Mobiles
{
    public class SBCook : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBCook()
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
            private static Random rnd = new Random();

            public InternalBuyInfo()
            {
                Add(new GenericBuyInfo(typeof(BreadLoaf), 5, 20, 0x103B, 0, true));

                Add(new GenericBuyInfo(typeof(SkilletExp), 800, 20, 0x97F, 0));
                Add(new GenericBuyInfo(typeof(EmptyPewterBowlExp), 2, 20, 0x15FD, 0));
                Add(new GenericBuyInfo(typeof(EmptyWoodenBowlExp), 2, 20, 0x15FD, 0));

                Add(new GenericBuyInfo(typeof(SackFlour), 3, 20, 0x1039, 0, true));
                Add(new GenericBuyInfo(typeof(SackFlour), 3, 20, 0x1039, 0, true));
                Add(new GenericBuyInfo(typeof(JarHoney), 3, 20, 0x9EC, 0, true));
                Add(new GenericBuyInfo(typeof(RollingPinExp), 2, 20, 0x1043, 0));
                Add(new GenericBuyInfo(typeof(FlourSifterExp), 2, 20, 0x103E, 0));

                var values = Enum.GetValues(typeof(CookRecipesExp)).CastToList<int>();
  
                foreach(var recipe in values)
                {
                    if (Recipe.Recipes.ContainsKey(recipe))
                    {
                        Add(new GenericBuyInfo(typeof(RecipeScroll), 20000, 10, 0x2831, 0, new object[] { recipe }));
                    }
                } 
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(CheeseWheel), 12);
                Add(typeof(CookedBird), 8);
                Add(typeof(RoastPig), 53);
                Add(typeof(Cake), 5);
                Add(typeof(JarHoney), 1);
                Add(typeof(SackFlour), 1);
                Add(typeof(BreadLoaf), 2);
                Add(typeof(ChickenLeg), 3);
                Add(typeof(LambLeg), 4);
                Add(typeof(Skillet), 1);
                Add(typeof(FlourSifter), 1);
                Add(typeof(RollingPin), 1);
                Add(typeof(Muffins), 1);
                Add(typeof(ApplePie), 3);

                Add(typeof(WoodenBowlOfCarrots), 1);
                Add(typeof(WoodenBowlOfCorn), 1);
                Add(typeof(WoodenBowlOfLettuce), 1);
                Add(typeof(WoodenBowlOfPeas), 1);
                Add(typeof(EmptyPewterBowl), 1);
                Add(typeof(PewterBowlOfCorn), 1);
                Add(typeof(PewterBowlOfLettuce), 1);
                Add(typeof(PewterBowlOfPeas), 1);
                Add(typeof(PewterBowlOfPotatos), 1);
                Add(typeof(WoodenBowlOfStew), 1);
                Add(typeof(WoodenBowlOfTomatoSoup), 1);
            }
        }
    }
}
