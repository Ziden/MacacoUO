
using System;
using System.Collections.Generic;
using Server.Items;
using Server.Ziden.RecompensaLogin;

namespace Server.Mobiles
{
    public class SBMage : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBMage()
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

        public static int PRECO_REAGS = 4;

        public static void BuyReagents(List<GenericBuyInfo> info)
        {
            info.Add(new GenericBuyInfo(typeof(BlackPearl), PRECO_REAGS, 999, 0xF7A, 0));
            info.Add(new GenericBuyInfo(typeof(Bloodmoss), PRECO_REAGS, 999, 0xF7B, 0));
           // info.Add(new GenericBuyInfo(typeof(Garlic), PRECO_REAGS * 2, 999, 0xF84, 0));
            //info.Add(new GenericBuyInfo(typeof(Ginseng), PRECO_REAGS * 2, 999, 0xF85, 0));
            //info.Add(new GenericBuyInfo(typeof(MandrakeRoot), PRECO_REAGS * 2, 999, 0xF86, 0));
            //info.Add(new GenericBuyInfo(typeof(Nightshade), PRECO_REAGS * 2, 999, 0xF88, 0));
            info.Add(new GenericBuyInfo(typeof(SpidersSilk), PRECO_REAGS, 999, 0xF8D, 0));
            info.Add(new GenericBuyInfo(typeof(SulfurousAsh), PRECO_REAGS, 999, 0xF8C, 0));

            info.Add(new GenericBuyInfo(typeof(BatWing), PRECO_REAGS + 2, 999, 0xF78, 0));
            info.Add(new GenericBuyInfo(typeof(DaemonBlood), PRECO_REAGS + 2, 999, 0xF7D, 0));
            info.Add(new GenericBuyInfo(typeof(PigIron), PRECO_REAGS + 2, 999, 0xF8A, 0));
            info.Add(new GenericBuyInfo(typeof(NoxCrystal), PRECO_REAGS + 2, 999, 0xF8E, 0));
            info.Add(new GenericBuyInfo(typeof(GraveDust), PRECO_REAGS + 2, 999, 0xF8F, 0));
        }

        public class InternalBuyInfo : List<GenericBuyInfo>
        {
            public InternalBuyInfo()
            {
                Add(new GenericBuyInfo(typeof(Spellbook), 18, 10, 0xEFA, 0));

                Add(new GenericBuyInfo(typeof(SpellbookPreto),  300000, 10, 0xEFA, TintaPreta.COR));
                Add(new GenericBuyInfo(typeof(SpellbookBranco), 500000, 10, 0xEFA, TintaBranca.COR));

                if (Core.AOS)
                    Add(new GenericBuyInfo(typeof(NecromancerSpellbook), 115, 10, 0x2253, 0));

                Add(new GenericBuyInfo(typeof(ScribesPen), 8, 10, 0xFBF, 0));

                Add(new GenericBuyInfo(typeof(BlankScroll), 5, 20, 0x0E34, 0));

                BuyReagents(this);

                //Add(new GenericBuyInfo("Chapeu Magico", typeof(MagicWizardsHat), 20000, 10, 0x1718, Utility.RandomDyedHue()));

                Add(new GenericBuyInfo(typeof(RecallRune), 1000, 10, 0x1F14, 0));

                Type[] types = Loot.RegularScrollTypes;

                int circles = 4;

                for (int i = 0; i < circles * 8 && i < types.Length; ++i)
                {
                    int itemID = 0x1F2E + i;

                    if (i == 6)
                        itemID = 0x1F2D;
                    else if (i > 6)
                        --itemID;

                    Add(new GenericBuyInfo(types[i], 50 + i * 50, 20, itemID, 0, true));
                }
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(WizardsHat), 15);
                Add(typeof(BlackPearl), 1);
                Add(typeof(Bloodmoss), 1);
                Add(typeof(MandrakeRoot), 1);
                Add(typeof(Garlic), 1);
                Add(typeof(Ginseng), 1);
                Add(typeof(Nightshade), 1);
                Add(typeof(SpidersSilk), 1);
                Add(typeof(SulfurousAsh), 1);

                if (Core.AOS)
                {
                    Add(typeof(BatWing), 1);
                    Add(typeof(DaemonBlood), 3);
                    Add(typeof(PigIron), 2);
                    Add(typeof(NoxCrystal), 3);
                    Add(typeof(GraveDust), 1);
                }

                Add(typeof(RecallRune), 13);
                Add(typeof(Spellbook), 25);

                if (Core.SE)
                {
                    Add(typeof(ExorcismScroll), 3);
                    Add(typeof(AnimateDeadScroll), 8);
                    Add(typeof(BloodOathScroll), 8);
                    Add(typeof(CorpseSkinScroll), 8);
                    Add(typeof(CurseWeaponScroll), 8);
                    Add(typeof(EvilOmenScroll), 8);
                    Add(typeof(PainSpikeScroll), 8);
                    Add(typeof(SummonFamiliarScroll), 8);
                    Add(typeof(HorrificBeastScroll), 8);
                    Add(typeof(MindRotScroll), 10);
                    Add(typeof(PoisonStrikeScroll), 10);
                    Add(typeof(WraithFormScroll), 15);
                    Add(typeof(LichFormScroll), 16);
                    Add(typeof(StrangleScroll), 16);
                    Add(typeof(WitherScroll), 16);
                    Add(typeof(VampiricEmbraceScroll), 20);
                    Add(typeof(VengefulSpiritScroll), 20);
                }
            }
        }
    }
}
