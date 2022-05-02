using System;
using System.Collections.Generic;
using Server.Items;
using Server.Multis;

namespace Server.Mobiles
{
    public class SBArchitect : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBArchitect()
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
                Add(new GenericBuyInfo("Construir Casa", typeof(HousePlacementTool), 500, 20, 5360, 0));
                Add(new GenericBuyInfo("Ferramenta de Decoracao", typeof(InteriorDecorator), 10001, 20, 0xFC1, 0));
                Add(new GenericBuyInfo("Teleportador", typeof(HouseTeleporterTileBag), 50000, 20, 0x40BB, 0));
                //Add(new GenericBuyInfo("Ferramenta de Decoracao", typeof(HouseTeleporterTileBag), 10000, 20, 6173, 0));

            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(InteriorDecorator), 5000);

                if (Core.AOS)
                    Add(typeof(HousePlacementTool), 301);
            }
        }
    }
}
