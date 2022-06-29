using System;
using Server.Mobiles;
using Server.Items;
using System.Collections.Generic;
using Server.Items.Functional.Pergaminhos;
using Server.Engines.Craft;
using System.Linq;

namespace Server.Engines.BulkOrders
{
    public delegate Item ConstructCallback(int type);

    public sealed class RewardType
    {
        public static int PS105 = 20;
        public static int PS110 = 100;
        public static int PS115 = 300;
        public static int PS120 = 500;

        private readonly int m_Points;
        private readonly Type[] m_Types;

        public int Points
        {
            get
            {
                return this.m_Points;
            }
        }
        public Type[] Types
        {
            get
            {
                return this.m_Types;
            }
        }

        public RewardType(int points, params Type[] types)
        {
            this.m_Points = points;
            this.m_Types = types;
        }

        public bool Contains(Type type)
        {
            for (int i = 0; i < this.m_Types.Length; ++i)
            {
                if (this.m_Types[i] == type)
                    return true;
            }

            return false;
        }
    }

    public sealed class RewardItem
    {
        private readonly int m_Weight;
        private readonly ConstructCallback m_Constructor;
        private readonly int m_Type;

        public int Weight
        {
            get
            {
                return this.m_Weight;
            }
        }
        public ConstructCallback Constructor
        {
            get
            {
                return this.m_Constructor;
            }
        }
        public int Type
        {
            get
            {
                return this.m_Type;
            }
        }

        public RewardItem(int weight, ConstructCallback constructor)
            : this(weight, constructor, 0)
        {
        }

        public RewardItem(int weight, ConstructCallback constructor, int type)
        {
            this.m_Weight = weight;
            this.m_Constructor = constructor;
            this.m_Type = type;
        }

        public Item Construct()
        {
            try
            {
                return this.m_Constructor(this.m_Type);
            }
            catch
            {
                return null;
            }
        }
    }

    public class BODCollectionItem : CollectionItem
    {
        public ConstructCallback Constructor { get; set; }
        public int RewardType { get; set; }

        public BODCollectionItem(int itemID, int tooltip, int hue, double points, ConstructCallback constructor, int type = 0)
            : base(null, itemID, tooltip, hue, points, false)
        {
            Constructor = constructor;
            RewardType = type;
        }

        public BODCollectionItem(int itemID, string tooltip, int hue, double points, ConstructCallback constructor, int type = 0)
           : base(null, itemID, tooltip, hue, points, false)
        {
            Constructor = constructor;
            RewardType = type;
        }
    }

    public sealed class RewardGroup
    {
        private readonly int m_Points;
        private readonly RewardItem[] m_Items;

        public int Points
        {
            get
            {
                return this.m_Points;
            }
        }
        public RewardItem[] Items
        {
            get
            {
                return this.m_Items;
            }
        }

        public RewardGroup(int points, params RewardItem[] items)
        {
            this.m_Points = points;
            this.m_Items = items;
        }

        public RewardItem AcquireItem()
        {
            if (this.m_Items.Length == 0)
                return null;
            else if (this.m_Items.Length == 1)
                return this.m_Items[0];

            int totalWeight = 0;

            for (int i = 0; i < this.m_Items.Length; ++i)
                totalWeight += this.m_Items[i].Weight;

            int randomWeight = Utility.Random(totalWeight);

            for (int i = 0; i < this.m_Items.Length; ++i)
            {
                RewardItem item = this.m_Items[i];

                if (randomWeight < item.Weight)
                    return item;

                randomWeight -= item.Weight;
            }

            return null;
        }
    }

    public abstract class RewardCalculator
    {
        private RewardGroup[] m_Groups;

        public RewardGroup[] Groups
        {
            get
            {
                return this.m_Groups;
            }
            set
            {
                this.m_Groups = value;
            }
        }

        public List<CollectionItem> RewardCollection { get; set; }

        public abstract int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type);

        public abstract int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type);

        public virtual int ComputeFame(SmallBOD bod)
        {
            int points = this.ComputePoints(bod) / 50;

            return points * points;
        }

        public virtual int ComputeFame(LargeBOD bod)
        {
            int points = this.ComputePoints(bod) / 50;

            return points * points;
        }

        public virtual int ComputePoints(SmallBOD bod)
        {
            return this.ComputePoints(bod.AmountMax, bod.RequireExceptional, bod.Material, 1, bod.Type);
        }

        public virtual int ComputePoints(LargeBOD bod)
        {
            Type type = bod.Entries == null || bod.Entries.Length == 0 ? null : bod.Entries[0].Details.Type;

            return this.ComputePoints(bod.AmountMax, bod.RequireExceptional, bod.Material, bod.Entries.Length, type);
        }

        public virtual int ComputeGold(SmallBOD bod)
        {
            var gold = this.ComputeGold(bod.AmountMax, bod.RequireExceptional, bod.Material, 1, bod.Type);
            if (gold < 1000)
                gold = 1000;
            return gold;
        }

        public virtual int ComputeGold(LargeBOD bod)
        {
            return this.ComputeGold(bod.AmountMax, bod.RequireExceptional, bod.Material, bod.Entries.Length, bod.Entries[0].Details.Type);
        }

        public virtual RewardGroup LookupRewards(int points)
        {
            for (int i = this.m_Groups.Length - 1; i >= 1; --i)
            {
                RewardGroup group = this.m_Groups[i];

                if (points >= group.Points)
                    return group;
            }

            return this.m_Groups[0];
        }

        public virtual int LookupTypePoints(RewardType[] types, Type type)
        {
            for (int i = 0; i < types.Length; ++i)
            {
                if (type == null || types[i].Contains(type))
                    return types[i].Points;
            }

            return 0;
        }

        protected static Item RewardTitle(int type)
        {
            return new BODRewardTitleDeed(type);
        }

        protected static Item RewardTitleS(string type)
        {
            return new BODRewardTitleDeed(type);
        }

        protected static Item NaturalDye(int type)
        {
            switch (type)
            {
                default:
                case 0: return new SpecialNaturalDye(DyeType.WindAzul);
                case 1: return new SpecialNaturalDye(DyeType.DullRuby);
                case 2: return new SpecialNaturalDye(DyeType.PoppieWhite);
                case 3: return new SpecialNaturalDye(DyeType.WindAzul, true);
                case 4: return new SpecialNaturalDye(DyeType.UmbranViolet, true);
                case 5: return new SpecialNaturalDye(DyeType.ZentoOrchid, true);
                case 6: return new SpecialNaturalDye(DyeType.DullRuby, true);
                case 7: return new SpecialNaturalDye(DyeType.PoppieWhite, true);
                case 8: return new SpecialNaturalDye(DyeType.UmbranViolet);
                case 9: return new SpecialNaturalDye(DyeType.ZentoOrchid);
            }
        }

        protected static Item RockHammer(int type)
        {
            return new RockHammer();
        }

        /*
        protected static Item HarvestMap(int type)
        {
            return new HarvestMap((CraftResource)type);
        }
        */

        protected static Item Recipe(int type)
        {
            switch (type)
            {
                case 0: return new RecipeScroll(170);
                case 1: return new RecipeScroll(457);
                case 2: return new RecipeScroll(458);
                case 3: return new RecipeScroll(800);
                case 4: return new RecipeScroll(599);
            }

            return null;
        }

        protected static Item SmeltersTalisman(int type)
        {
            return new SmeltersTalisman((CraftResource)type);
        }

        protected static Item WoodsmansTalisman(int type)
        {
            return new WoodsmansTalisman((CraftResource)type);
        }

        public RewardCalculator()
        {
        }
    }

    #region Smith Rewards
    public sealed class SmithRewardCalculator : RewardCalculator
    {
        public SmithRewardCalculator()
        {
            if (BulkOrderSystem.NewSystemEnabled)
            {
                RewardCollection = new List<CollectionItem>();

                RewardCollection.Add(new BODCollectionItem(0x13E3, "Martelo do Ferreiro Duravel<br>Usavel 1000 vezes!", 0, 10, SmithHammer));
                //RewardCollection.Add(new BODCollectionItem(0xF39, 1157084, 0x973, 10, SturdyShovel));
                RewardCollection.Add(new BODCollectionItem(0xE86, "Picareta Duravel<br>Usavel 1000 vezes!", 0x973, 25, SturdyPickaxe));
                RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo de Armeiro", 0, 100, RewardTitle, 0));
                RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo de Ferreiro", 0, 100, RewardTitle, 1));
                RewardCollection.Add(new BODCollectionItem(0x13C6, "Luvas de Mineracao +1", 0, 100, MiningGloves, 1));
                RewardCollection.Add(new BODCollectionItem(0x13D5, "Luvas de Mineracao +3", 0, 200, MiningGloves, 3));
                RewardCollection.Add(new BODCollectionItem(0xFB4, "Marreta Mineradora Magica<br>Melhora os minerios de uma regiao.", 0, 200, ProspectorsTool));
                RewardCollection.Add(new BODCollectionItem(0xE86, "Picareta Gargula<br>Minera minerios melhores<br>Cuidado: Pode minerar elementais.", 0, 200, GargoylesPickaxe));
                RewardCollection.Add(new BODCollectionItem(0x2F5B, "Talisman do Forjador de Prata<br>100% De chance ao fundir minerios de Prata.<br>Dura um bom tempo.", CraftResources.GetHue(CraftResource.Dourado), 350, SmeltersTalisman, (int)CraftResource.Dourado));
                RewardCollection.Add(new BODCollectionItem(0x9E2A, "Talisman Salva-Falhas 10x ", 0, 400, CraftsmanTalisman, 10));
                RewardCollection.Add(new BODCollectionItem(0x13EB, "Luvas de Mineracao +5", 0, 450, MiningGloves, 5));
                RewardCollection.Add(new BODCollectionItem(0x13E3, "Martelo +5 Blacksmithy<br>Apenas aumenta chance de craft/exp.", 0x482, 50, AncientHammer, 5));
                RewardCollection.Add(new BODCollectionItem(0x13E3, "Bandana +1 Blacksmithy<br>Apenas aumenta chance de craft/exp.", 0x482, 5, BandanaBs, 1));

                RewardCollection.Add(new BODCollectionItem(4102, "Po de refinamento<br>Maximima e melhora a durabilidade de um item", 0, 450, PowderOfTemperament));
                RewardCollection.Add(new BODCollectionItem(0x2F5B, "Talisman do Forjador de Niobio<br>100% De chance ao fundir minerios de Niobio.<br>Dura um bom tempo.", CraftResources.GetHue(CraftResource.Niobio), 475, SmeltersTalisman, (int)CraftResource.Niobio));
                //RewardCollection.Add(new BODCollectionItem(0x9E7E, ", 0, 500, RockHammer));
                //RewardCollection.Add(new BODCollectionItem(0x13E3, 1157092, CraftResources.GetHue(CraftResource.Berilo), 500, RunicHammer, 1));
                RewardCollection.Add(new BODCollectionItem(0x2F5B, "Talisman do Forjador de Lazurita<br>100% De chance ao fundir minerios de Lazurita.<br>Dura um bom tempo.", CraftResources.GetHue(CraftResource.Lazurita), 525, SmeltersTalisman, (int)CraftResource.Lazurita));
                //RewardCollection.Add(new BODCollectionItem(0x13E3, 1157093, CraftResources.GetHue(CraftResource.Vibranium), 550, RunicHammer, 2));
                RewardCollection.Add(new BODCollectionItem(0x9E2A, "Talisman Salva-Falhas 25x<br>Dura alguns usos", 0, 550, CraftsmanTalisman, 25));
                RewardCollection.Add(new BODCollectionItem(0x2F5B, "Talisman do Forjador de Quartzo<br>100% De chance ao fundir minerios de Quartzo.<br>Dura um bom tempo.", CraftResources.GetHue(CraftResource.Quartzo), 575, SmeltersTalisman, (int)CraftResource.Quartzo));
                //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa de Minerar Ouro", CraftResources.GetHue(CraftResource.Dourado), 100, HarvestMap, (int)CraftResource.Dourado));
                RewardCollection.Add(new BODCollectionItem(0xFAF, "Bigorna Colorida", 0, 625, ColoredAnvil));

                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 ArmsLore<br>Aumenta seu cap de arms lore.", 0x481, RewardType.PS105, PowerScroll3, 5));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 ArmsLore<br>Aumenta seu cap de arms lore.", 0x481, RewardType.PS110, PowerScroll3, 10));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 ArmsLore<br>Aumenta seu cap de arms lore.", 0x481, RewardType.PS115, PowerScroll3, 15));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 ArmsLore<br>Aumenta seu cap de arms lore.", 0x481, RewardType.PS120, PowerScroll3, 20));

                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 Blacksmithy<br>Aumenta seu cap de blacksmithy.", 0x481, RewardType.PS105, PowerScroll, 5));
                //RewardCollection.Add(new BODCollectionItem(0x13E3, 1157094, CraftResources.GetHue(CraftResource.Cobre), 650, RunicHammer, 3));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 Blacksmithy<br>Aumenta seu cap de blacksmithy.", 0x481, RewardType.PS110, PowerScroll, 10));
                //RewardCollection.Add(new BODCollectionItem(0x13E3, 1157095, CraftResources.GetHue(CraftResource.Bronze), 700, RunicHammer, 4));

                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 Blacksmithy<br>Aumenta seu cap de blacksmithy.", 0x481, RewardType.PS115, PowerScroll, 15));

                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 Mining<br>Aumenta seu cap de mining.", 0x481, RewardType.PS115*2, PowerScroll2, 15));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 Mining<br>Aumenta seu cap de mining.", 0x481, RewardType.PS120*2, PowerScroll2, 20));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 Mining<br>Aumenta seu cap de mining.", 0x481, RewardType.PS105*2, PowerScroll2, 5));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 Mining<br>Aumenta seu cap de mining.", 0x481, RewardType.PS110*2, PowerScroll2, 10));

                RewardCollection.Add(new BODCollectionItem(0x13E3, "Martelo +10 Blacksmithy<br>Apenas aumenta chance de craft/exp", 0x482, 850, AncientHammer, 10));
                //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa de Minerar Niobio", CraftResources.GetHue(CraftResource.Niobio), 200, HarvestMap, (int)CraftResource.Niobio));
                RewardCollection.Add(new BODCollectionItem(0x9E2A, "Talisman Salva-Falhas 50x", 0, 900, CraftsmanTalisman, 50));
                //RewardCollection.Add(new BODCollectionItem(0x13E3, 1157096, CraftResources.GetHue(CraftResource.Prata), 950, RunicHammer, 5));
                //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa de Minerar Lazurita", CraftResources.GetHue(CraftResource.Lazurita), 400, HarvestMap, (int)CraftResource.Lazurita));
                RewardCollection.Add(new BODCollectionItem(0x13E3, "Martelo +20 Blacksmithy<br>Apenas aumenta chance de craft/exp", 0x482, 1000, AncientHammer, 20));
                //RewardCollection.Add(new BODCollectionItem(0x13E3, 1157097, CraftResources.GetHue(CraftResource.Niobio), 1050, RunicHammer, 6));
                //sRewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa de Minerar Quartzo", CraftResources.GetHue(CraftResource.Quartzo), 600, HarvestMap, (int)CraftResource.Quartzo));
                RewardCollection.Add(new BODCollectionItem(0x13E3, "Martelo +30 Blacksmithy<br>Apenas aumenta chance de craft/exp", 0x482, 1500, AncientHammer, 30));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 Blacksmithy<br>Aumenta seu cap de blacksmithy.", 0x481, RewardType.PS120, PowerScroll, 20));
                //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa de Minerar Vibranium", CraftResources.GetHue(CraftResource.Vibranium), 1350, HarvestMap, (int)CraftResource.Vibranium));
                //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa de Minerar Adamantium", CraftResources.GetHue(CraftResource.Adamantium), 1350, HarvestMap, (int)CraftResource.Adamantium));
                //RewardCollection.Add(new BODCollectionItem(0x13E3, 1157098, CraftResources.GetHue(CraftResource.Lazurita), 1150, RunicHammer, 7));
                //RewardCollection.Add(new BODCollectionItem(0x13E3, 1157099, CraftResources.GetHue(CraftResource.Quartzo), 1200, RunicHammer, 8));
            }
            else
            {
                this.Groups = new RewardGroup[]
                {
                    new RewardGroup(0, new RewardItem(1, SturdyShovel)),
                    new RewardGroup(25, new RewardItem(1, SturdyPickaxe)),
                    new RewardGroup(50, new RewardItem(45, SturdyShovel), new RewardItem(45, SturdyPickaxe), new RewardItem(10, MiningGloves, 1)),
                    new RewardGroup(200, new RewardItem(45, GargoylesPickaxe), new RewardItem(45, ProspectorsTool), new RewardItem(10, MiningGloves, 3)),
                    new RewardGroup(400, new RewardItem(2, GargoylesPickaxe), new RewardItem(2, ProspectorsTool), new RewardItem(1, PowderOfTemperament)),
                    new RewardGroup(450, new RewardItem(9, PowderOfTemperament), new RewardItem(1, MiningGloves, 5)),
                    new RewardGroup(500, new RewardItem(1, RunicHammer, 1)),
                    new RewardGroup(550, new RewardItem(3, RunicHammer, 1), new RewardItem(2, RunicHammer, 2)),
                    new RewardGroup(600, new RewardItem(1, RunicHammer, 2)),
                    new RewardGroup(625, new RewardItem(3, RunicHammer, 2), new RewardItem(6, PowerScroll, 5), new RewardItem(1, ColoredAnvil)),
                    new RewardGroup(650, new RewardItem(1, RunicHammer, 3)),
                    new RewardGroup(675, new RewardItem(1, ColoredAnvil), new RewardItem(6, PowerScroll, 10), new RewardItem(3, RunicHammer, 3)),
                    new RewardGroup(700, new RewardItem(1, RunicHammer, 4)),
                    new RewardGroup(750, new RewardItem(1, AncientHammer, 10)),
                    new RewardGroup(800, new RewardItem(1, PowerScroll, 15)),
                    new RewardGroup(850, new RewardItem(1, AncientHammer, 15)),
                    new RewardGroup(900, new RewardItem(1, PowerScroll, 20)),
                    new RewardGroup(950, new RewardItem(1, RunicHammer, 5)),
                    new RewardGroup(1000, new RewardItem(1, AncientHammer, 30)),
                    new RewardGroup(1050, new RewardItem(1, RunicHammer, 6)),
                    new RewardGroup(1100, new RewardItem(1, AncientHammer, 60)),
                    new RewardGroup(1150, new RewardItem(1, RunicHammer, 7)),
                    new RewardGroup(1200, new RewardItem(1, RunicHammer, 8))
                };
            }
        }

        #region Constructors
        private static readonly ConstructCallback SmithHammer = new ConstructCallback(CreateSmithHammer);
        private static readonly ConstructCallback SturdyShovel = new ConstructCallback(CreateSturdyShovel);
        private static readonly ConstructCallback SturdyPickaxe = new ConstructCallback(CreateSturdyPickaxe);
        private static readonly ConstructCallback MiningGloves = new ConstructCallback(CreateMiningGloves);
        private static readonly ConstructCallback GargoylesPickaxe = new ConstructCallback(CreateGargoylesPickaxe);
        private static readonly ConstructCallback ProspectorsTool = new ConstructCallback(CreateProspectorsTool);
        private static readonly ConstructCallback PowderOfTemperament = new ConstructCallback(CreatePowderOfTemperament);
        private static readonly ConstructCallback RunicHammer = new ConstructCallback(CreateRunicHammer);
        private static readonly ConstructCallback PowerScroll = new ConstructCallback(CreatePowerScroll);
        private static readonly ConstructCallback PowerScrollNovo = new ConstructCallback(CreatePowerScrollNovo);
        private static readonly ConstructCallback PowerScroll2 = new ConstructCallback(CreatePowerScroll2);
        private static readonly ConstructCallback PowerScroll3 = new ConstructCallback(CreatePowerScroll3);
        private static readonly ConstructCallback ColoredAnvil = new ConstructCallback(CreateColoredAnvil);
        private static readonly ConstructCallback AncientHammer = new ConstructCallback(CreateAncientHammer);
        private static readonly ConstructCallback Bandana = new ConstructCallback(BandanaBs);

        private static Item BandanaBs(int type)
        {
            var bandana = new SkullCap();
            bandana.Name = "Bandana do Ferreiro";
            bandana.Hue = TintaPreta.COR;
            bandana.SkillBonuses.Skill_1_Name = SkillName.Blacksmith;
            bandana.SkillBonuses.Skill_1_Value = type;
            return bandana;
        }


        private static Item CreateSmithHammer(int type)
        {
            var hammer = new SmithHammer();
            hammer.UsesRemaining = 1000;

            return hammer;
        }

        private static Item CreateSturdyShovel(int type)
        {
            return new SturdyShovel();
        }

        private static Item CreateSturdyPickaxe(int type)
        {
            return new SturdyPickaxe();
        }

        private static Item CreateMiningGloves(int type)
        {
            if (type == 1)
                return new LeatherGlovesOfMining(1);
            else if (type == 3)
                return new StuddedGlovesOfMining(3);
            else if (type == 5)
                return new RingmailGlovesOfMining(5);

            throw new InvalidOperationException();
        }

        private static Item CreateGargoylesPickaxe(int type)
        {
            return new GargoylesPickaxe();
        }

        private static Item CreateProspectorsTool(int type)
        {
            return new ProspectorsTool();
        }

        private static Item CreatePowderOfTemperament(int type)
        {
            return new PowderOfTemperament();
        }

        private static Item CreateRunicHammer(int type)
        {
            if (type >= 1 && type <= 8)
                return new RunicHammer(CraftResource.Ferro + type, Core.AOS ? (55 - (type * 5)) : 50);

            throw new InvalidOperationException();
        }

        private static Item CreatePowerScroll(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Blacksmith, 100 + type);

            throw new InvalidOperationException();
        }

        private static Item CreatePowerScrollNovo(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Blacksmith, 100 + type);

            throw new InvalidOperationException();
        }

        private static Item CreatePowerScroll2(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Mining, 100 + type);

            throw new InvalidOperationException();
        }

        private static Item CreatePowerScroll3(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.ArmsLore, 100 + type);

            throw new InvalidOperationException();
        }

        private static Item CreateColoredAnvil(int type)
        {
            return new ColoredAnvil();
        }

        private static Item CreateAncientHammer(int type)
        {
            //if (type == 10 || type == 15 || type == 30 || type == 60)
            return new AncientSmithyHammer(type);
            //throw new InvalidOperationException();

        }

        private static Item CraftsmanTalisman(int type)
        {
            return new MasterCraftsmanTalisman(type, 0x9E2A, TalismanSkill.Blacksmithy);
        }
        #endregion

        public static readonly SmithRewardCalculator Instance = new SmithRewardCalculator();

        private readonly RewardType[] m_Types = new RewardType[]
        {
            // Armors
            new RewardType(200, typeof(RingmailGloves), typeof(RingmailChest), typeof(RingmailArms), typeof(RingmailLegs)),
            new RewardType(300, typeof(ChainCoif), typeof(ChainLegs), typeof(ChainChest)),
            new RewardType(400, typeof(PlateArms), typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest)),

            // Weapons
            new RewardType(200, typeof(Bardiche), typeof(Halberd)),
            new RewardType(300, typeof(Dagger), typeof(ShortSpear), typeof(Spear), typeof(WarFork), typeof(Kryss)), //OSI put the dagger in there.  Odd, ain't it.
            new RewardType(350, typeof(Axe), typeof(BattleAxe), typeof(DoubleAxe), typeof(ExecutionersAxe), typeof(LargeBattleAxe), typeof(TwoHandedAxe)),
            new RewardType(350, typeof(Broadsword), typeof(Cutlass), typeof(Katana), typeof(Longsword), typeof(Scimitar), /*typeof( ThinLongsword ),*/ typeof(VikingSword)),
            new RewardType(350, typeof(WarAxe), typeof(HammerPick), typeof(Mace), typeof(Maul), typeof(WarHammer), typeof(WarMace))
        };

        public override int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int points = 0;

            if (quantity <= 30)
                points += 10;
            else if (quantity <= 45)
                points += 25;
            else
                points += 50;

            if (exceptional)
                points += 200;

            if (itemCount > 1)
                points += this.LookupTypePoints(this.m_Types, type);

            if (material >= BulkMaterialType.Cobre && material <= BulkMaterialType.Vibranium)
                points += 200 + (50 * (material - BulkMaterialType.Cobre));

            return points;
        }

        private static readonly int[][][] m_GoldTable = new int[][][]
        {
            new int[][] // 1-part (regular)
            {
                new int[] { 600, 600, 600, 600, 600, 750, 750, 1200, 1200 },
                new int[] { 600, 600, 600, 600, 600, 1125, 1125, 1800, 1800 },
                new int[] { 600, 600, 750, 800, 1050, 1500, 2250, 2400, 4000 }
            },
            new int[][] // 1-part (exceptional)
            {
                new int[] { 600, 600, 600, 750, 750, 1500, 1500, 3000, 3000 },
                new int[] { 600, 600, 600, 1125, 1125, 2250, 2250, 4500, 4500 },
                new int[] { 600, 800, 1200, 1500, 2500, 3000, 6000, 6000, 12000 }
            },
            new int[][] // Ringmail (regular)
            {
                new int[] { 3000, 5000, 5000, 7500, 7500, 10000, 10000, 15000, 15000 },
                new int[] { 4500, 7500, 7500, 11250, 11500, 15000, 15000, 22500, 22500 },
                new int[] { 6000, 10000, 15000, 15000, 20000, 20000, 30000, 30000, 50000 }
            },
            new int[][] // Ringmail (exceptional)
            {
                new int[] { 5000, 10000, 10000, 15000, 15000, 25000, 25000, 50000, 50000 },
                new int[] { 7500, 15000, 15000, 22500, 22500, 37500, 37500, 75000, 75000 },
                new int[] { 10000, 20000, 30000, 30000, 50000, 50000, 100000, 100000, 200000 }
            },
            new int[][] // Chainmail (regular)
            {
                new int[] { 4000, 7500, 7500, 10000, 10000, 15000, 15000, 25000, 25000 },
                new int[] { 6000, 11250, 11250, 15000, 15000, 22500, 22500, 37500, 37500 },
                new int[] { 8000, 15000, 20000, 20000, 30000, 30000, 50000, 50000, 100000 }
            },
            new int[][] // Chainmail (exceptional)
            {
                new int[] { 7500, 15000, 15000, 25000, 25000, 50000, 50000, 100000, 100000 },
                new int[] { 11250, 22500, 22500, 37500, 37500, 75000, 75000, 150000, 150000 },
                new int[] { 15000, 30000, 50000, 50000, 100000, 100000, 200000, 200000, 200000 }
            },
            new int[][] // Platemail (regular)
            {
                new int[] { 5000, 10000, 10000, 15000, 15000, 25000, 25000, 50000, 50000 },
                new int[] { 7500, 15000, 15000, 22500, 22500, 37500, 37500, 75000, 75000 },
                new int[] { 10000, 20000, 30000, 30000, 50000, 50000, 100000, 100000, 200000 }
            },
            new int[][] // Platemail (exceptional)
            {
                new int[] { 10000, 25000, 25000, 50000, 50000, 100000, 100000, 100000, 100000 },
                new int[] { 15000, 37500, 37500, 75000, 75000, 150000, 150000, 150000, 150000 },
                new int[] { 20000, 50000, 100000, 100000, 200000, 200000, 200000, 200000, 200000 }
            },
            new int[][] // 2-part weapons (regular)
            {
                new int[] { 3000, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 4500, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 6000, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new int[][] // 2-part weapons (exceptional)
            {
                new int[] { 5000, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 7500, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 10000, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new int[][] // 5-part weapons (regular)
            {
                new int[] { 4000, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 6000, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 8000, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new int[][] // 5-part weapons (exceptional)
            {
                new int[] { 7500, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 11250, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 15000, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new int[][] // 6-part weapons (regular)
            {
                new int[] { 4000, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 6000, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 10000, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new int[][] // 6-part weapons (exceptional)
            {
                new int[] { 7500, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 11250, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 15000, 0, 0, 0, 0, 0, 0, 0, 0 }
            }
        };

        private int ComputeType(Type type, int itemCount)
        {
            // Item count of 1 means it's a small BOD.
            if (itemCount == 1)
                return 0;

            int typeIdx;

            // Loop through the RewardTypes defined earlier and find the correct one.
            for (typeIdx = 0; typeIdx < 7; ++typeIdx)
            {
                if (this.m_Types[typeIdx].Contains(type))
                    break;
            }

            // Types 5, 6 and 7 are Large Weapon BODs with the same rewards.
            if (typeIdx > 5)
                typeIdx = 5;

            return (typeIdx + 1) * 2;
        }

        public override int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int gold = 0;

            Shard.Debug("Calculando Gold da bod");
            /*
            if (itemCount == 1 && BulkOrderSystem.NewSystemEnabled && BulkOrderSystem.ComputeGold(type, quantity, out gold))
            {
                Shard.Debug("Item Count era 1, entao retornei aqui mermo");
                return gold;
            }
            */

            int[][][] goldTable = m_GoldTable;
            Shard.Debug("Vendo Gold Table");
            int typeIndex = this.ComputeType(type, itemCount);
            int quanIndex = (quantity >= 50 ? 2 : quantity >= 30 ? 1 : 0);
            int mtrlIndex = (material >= BulkMaterialType.Cobre && material <= BulkMaterialType.Vibranium) ? 1 + (int)(material - BulkMaterialType.Cobre) : 0;

            if (exceptional)
                typeIndex++;

            gold = goldTable[typeIndex][quanIndex][mtrlIndex];

            if (gold == 0)
            {
                Shard.Debug("Ajustado gold minimo");
                gold = 500;
            }



            Shard.Debug("Gold: " + gold);
            int min = (gold * 9) / 10;
            int max = (gold * 10) / 9;

            return Utility.RandomMinMax(min, max);
        }
    }
    #endregion

    #region Tailor Rewards
    public sealed class TailorRewardCalculator : RewardCalculator
    {
        public TailorRewardCalculator()
        {
            if (BulkOrderSystem.NewSystemEnabled)
            {
                RewardCollection = new List<CollectionItem>();

                RewardCollection.Add(new BODCollectionItem(0xF9D, "Kit de Costura Duravel<br>Faz 1000 items", 0, 10, SewingKit));
                RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Costureiro", 0, 10, RewardTitle, 2));
                RewardCollection.Add(new BODCollectionItem(0x1765, "Pano de cor incomum<br>Cores aleatorias", 0, 10, Cloth, 0));
                RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Designer de Roupas", 0, 25, RewardTitle, 3));
                RewardCollection.Add(new BODCollectionItem(0x1761, "Pano de cor rara<br>Cores aleatorias", 0, 50, Cloth, 1));
                RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Grande Alfaiate", 0, 100, RewardTitle, 4));
                RewardCollection.Add(new BODCollectionItem(0x1765, "Pano de cor epica<br>Cores aleatorias", 0, 100, Cloth, 2));
                RewardCollection.Add(new BODCollectionItem(0x1765, "Pano de cor lendaria<br>Cores aleatorias", 0, 150, Cloth, 3));
                RewardCollection.Add(new BODCollectionItem(0x170D, "Sandalias de Cores Lindas<br>Cores aleatorias", 0, 150, Sandals, 3));
                RewardCollection.Add(new BODCollectionItem(0x1765, "Pano de cor mitica<br>Cores aleatorias", 0, 200, Cloth, 4));
                RewardCollection.Add(new BODCollectionItem(0x9E25, "Talisman Salva-Falhas 10x<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 200, CraftsmanTalisman, 10)); // todo: Get id
                RewardCollection.Add(new BODCollectionItem(0x14F0, "Peles de Decoracao", 0, 300, StretchedHide));
                RewardCollection.Add(new BODCollectionItem(0x1765, "Pano de cor magica<br>Cores aleatorias", 0, 300, Cloth, 5)); // TODO: Get other 4 colors
                RewardCollection.Add(new BODCollectionItem(0x9E25, "Talisman Salva-Falhas 15x<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 300, CraftsmanTalisman, 15)); // todo: Get id
                //RewardCollection.Add(new BODCollectionItem(0xF9D, 1157115, CraftResources.GetHue(CraftResource.SpinedLeather), 350, RunicKit, 1));
                RewardCollection.Add(new BODCollectionItem(0x9E25, "Talisman Salva-Falhas 20x<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 350, CraftsmanTalisman, 20)); // todo: Get id
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 Tailoring<br>Aumenta seu cap de Tailoring", 0x481, RewardType.PS105, PowerScroll, 5));
                RewardCollection.Add(new BODCollectionItem(0x14F0, "Tapetes De Flores", 0, 400, Tapestry));
                RewardCollection.Add(new BODCollectionItem(0x14F0, "Tapetes de Ursos", 0, 450, BearRug));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 Tailoring<br>Aumenta seu cap de Tailoring", 0x481, RewardType.PS110, PowerScroll, 10));
                RewardCollection.Add(new BODCollectionItem(0x14F0, "Pergaminho Sagrado<br>Torna uma roupa pertence pessoal por 1 mes.", 0, 550, ClothingBlessDeed));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 Tailoring < br > Aumenta seu cap de Tailoring", 0x481, RewardType.PS115, PowerScroll, 15));
                //RewardCollection.Add(new BODCollectionItem(0xF9D, 1157116, CraftResources.GetHue(CraftResource.HornedLeather), 600, RunicKit, 2));
                RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 Tailoring<br>Aumenta seu cap de Tailoring", 0x481, RewardType.PS120, PowerScroll, 20));
                //RewardCollection.Add(new BODCollectionItem(0xF9D, 1157117, CraftResources.GetHue(CraftResource.BarbedLeather), 700, RunicKit, 3));
            }
            else
            {
                this.Groups = new RewardGroup[]
                {
                    new RewardGroup(0, new RewardItem(1, Cloth, 0)),
                    new RewardGroup(50, new RewardItem(1, Cloth, 1)),
                    new RewardGroup(100, new RewardItem(1, Cloth, 2)),
                    new RewardGroup(150, new RewardItem(9, Cloth, 3), new RewardItem(1, Sandals)),
                    new RewardGroup(200, new RewardItem(4, Cloth, 4), new RewardItem(1, Sandals)),
                    new RewardGroup(300, new RewardItem(1, StretchedHide)),
                    new RewardGroup(350, new RewardItem(1, RunicKit, 1)),
                    new RewardGroup(400, new RewardItem(2, PowerScroll, 5), new RewardItem(3, Tapestry)),
                    new RewardGroup(450, new RewardItem(1, BearRug)),
                    new RewardGroup(500, new RewardItem(1, PowerScroll, 10)),
                    new RewardGroup(550, new RewardItem(1, ClothingBlessDeed)),
                    new RewardGroup(575, new RewardItem(1, PowerScroll, 15)),
                    new RewardGroup(600, new RewardItem(1, RunicKit, 2)),
                    new RewardGroup(650, new RewardItem(1, PowerScroll, 20)),
                    new RewardGroup(700, new RewardItem(1, RunicKit, 3))
                };
            }
        }

        #region Constructors
        private static readonly ConstructCallback SewingKit = new ConstructCallback(CreateSewingKit);
        private static readonly ConstructCallback Cloth = new ConstructCallback(CreateCloth);
        private static readonly ConstructCallback Sandals = new ConstructCallback(CreateSandals);
        private static readonly ConstructCallback StretchedHide = new ConstructCallback(CreateStretchedHide);
        private static readonly ConstructCallback RunicKit = new ConstructCallback(CreateRunicKit);
        private static readonly ConstructCallback Tapestry = new ConstructCallback(CreateTapestry);
        private static readonly ConstructCallback PowerScroll = new ConstructCallback(CreatePowerScroll);

        private static readonly ConstructCallback BearRug = new ConstructCallback(CreateBearRug);
        private static readonly ConstructCallback ClothingBlessDeed = new ConstructCallback(CreateCBD);
        private static readonly ConstructCallback CraftsmanTalisman = new ConstructCallback(CreateCraftsmanTalisman);

        private static Item CreateSewingKit(int type)
        {
            var kit = new SewingKit();
            kit.UsesRemaining = 1000;

            return kit;
        }

        private static readonly int[][] m_ClothHues = new int[][]
        {
            new int[] { 0x483, 0x48C, 0x488, 0x48A },
            new int[] { 0x495, 0x48B, 0x486, 0x485 },
            new int[] { 0x48D, 0x490, 0x48E, 0x491 },
            new int[] { 0x48F, 0x494, 0x484, 0x497 },
            new int[] { 0x489, 0x47F, 0x482, 0x47E },
            new int[] { 0xAAC, 0xAB4, 0xAAF, 0xAB5, 0xAAB },
        };

        private static Item CreateCloth(int type)
        {
            if (type >= 0 && type < m_ClothHues.Length)
            {
                UncutCloth cloth = new UncutCloth(100);
                cloth.Hue = m_ClothHues[type][Utility.Random(m_ClothHues[type].Length)];
                return cloth;
            }

            throw new InvalidOperationException();
        }

        private static readonly int[] m_SandalHues = new int[]
        {
            0x489, 0x47F, 0x482,
            0x47E, 0x48F, 0x494,
            0x484, 0x497
        };

        private static Item CreateSandals(int type)
        {
            return new Sandals(m_SandalHues[Utility.Random(m_SandalHues.Length)]);
        }

        private static Item CreateStretchedHide(int type)
        {
            switch (Utility.Random(4))
            {
                default:
                case 0:
                    return new SmallStretchedHideEastDeed();
                case 1:
                    return new SmallStretchedHideSouthDeed();
                case 2:
                    return new MediumStretchedHideEastDeed();
                case 3:
                    return new MediumStretchedHideSouthDeed();
            }
        }

        private static Item CreateTapestry(int type)
        {
            switch (Utility.Random(4))
            {
                default:
                case 0:
                    return new LightFlowerTapestryEastDeed();
                case 1:
                    return new LightFlowerTapestrySouthDeed();
                case 2:
                    return new DarkFlowerTapestryEastDeed();
                case 3:
                    return new DarkFlowerTapestrySouthDeed();
            }
        }

        private static Item CreateBearRug(int type)
        {
            switch (Utility.Random(4))
            {
                default:
                case 0:
                    return new BrownBearRugEastDeed();
                case 1:
                    return new BrownBearRugSouthDeed();
                case 2:
                    return new PolarBearRugEastDeed();
                case 3:
                    return new PolarBearRugSouthDeed();
            }
        }

        private static Item CreateRunicKit(int type)
        {
            if (type >= 1 && type <= 3)
                return new RunicSewingKit(CraftResource.RegularLeather + type, 60 - (type * 15));

            throw new InvalidOperationException();
        }

        private static Item CreatePowerScroll(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Tailoring, 100 + type);

            throw new InvalidOperationException();
        }

        private static Item CreateCBD(int type)
        {
            return new PergaminhoSagrado();
        }

        private static Item CreateCraftsmanTalisman(int type)
        {
            return new MasterCraftsmanTalisman(type, 0x9E25, TalismanSkill.Tailoring);
        }

        #endregion

        public static readonly TailorRewardCalculator Instance = new TailorRewardCalculator();

        public override int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int points = 0;

            if (quantity <= 10)
                points += 10;
            else if (quantity <= 30)
                points += 25;
            else if (quantity > 30)
                points += 50;

            if (exceptional)
                points += 100;

            if (itemCount <= 4)
                points += 300;
            else if (itemCount <= 5)
                points += 400;
            else if (itemCount >= 6)
                points += 500;

            if (material == BulkMaterialType.Spined)
                points += 50;
            else if (material == BulkMaterialType.Horned)
                points += 100;
            else if (material == BulkMaterialType.Barbed)
                points += 150;

            return points;
        }

        private static readonly int[][][] m_AosGoldTable = new int[][][]
        {
            new int[][] // 1-part (regular)
            {
                new int[] { 150, 150, 300, 300 },
                new int[] { 225, 225, 450, 450 },
                new int[] { 300, 400, 600, 750 }
            },
            new int[][] // 1-part (exceptional)
            {
                new int[] { 300, 300, 600, 600 },
                new int[] { 450, 450, 900, 900 },
                new int[] { 600, 750, 1200, 1800 }
            },
            new int[][] // 4-part (regular)
            {
                new int[] { 4000, 4000, 5000, 5000 },
                new int[] { 6000, 6000, 7500, 7500 },
                new int[] { 8000, 10000, 10000, 15000 }
            },
            new int[][] // 4-part (exceptional)
            {
                new int[] { 5000, 5000, 7500, 7500 },
                new int[] { 7500, 7500, 11250, 11250 },
                new int[] { 10000, 15000, 15000, 20000 }
            },
            new int[][] // 5-part (regular)
            {
                new int[] { 5000, 5000, 7500, 7500 },
                new int[] { 7500, 7500, 11250, 11250 },
                new int[] { 10000, 15000, 15000, 20000 }
            },
            new int[][] // 5-part (exceptional)
            {
                new int[] { 7500, 7500, 10000, 10000 },
                new int[] { 11250, 11250, 15000, 15000 },
                new int[] { 15000, 20000, 20000, 30000 }
            },
            new int[][] // 6-part (regular)
            {
                new int[] { 7500, 7500, 10000, 10000 },
                new int[] { 11250, 11250, 15000, 15000 },
                new int[] { 15000, 20000, 20000, 30000 }
            },
            new int[][] // 6-part (exceptional)
            {
                new int[] { 10000, 10000, 15000, 15000 },
                new int[] { 15000, 15000, 22500, 22500 },
                new int[] { 20000, 30000, 30000, 50000 }
            }
        };

        private static readonly int[][][] m_OldGoldTable = new int[][][]
        {
            new int[][] // 1-part (regular)
            {
                new int[] { 150, 150, 300, 300 },
                new int[] { 225, 225, 450, 450 },
                new int[] { 300, 400, 600, 750 }
            },
            new int[][] // 1-part (exceptional)
            {
                new int[] { 300, 300, 600, 600 },
                new int[] { 450, 450, 900, 900 },
                new int[] { 600, 750, 1200, 1800 }
            },
            new int[][] // 4-part (regular)
            {
                new int[] { 3000, 3000, 4000, 4000 },
                new int[] { 4500, 4500, 6000, 6000 },
                new int[] { 6000, 8000, 8000, 10000 }
            },
            new int[][] // 4-part (exceptional)
            {
                new int[] { 4000, 4000, 5000, 5000 },
                new int[] { 6000, 6000, 7500, 7500 },
                new int[] { 8000, 10000, 10000, 15000 }
            },
            new int[][] // 5-part (regular)
            {
                new int[] { 4000, 4000, 5000, 5000 },
                new int[] { 6000, 6000, 7500, 7500 },
                new int[] { 8000, 10000, 10000, 15000 }
            },
            new int[][] // 5-part (exceptional)
            {
                new int[] { 5000, 5000, 7500, 7500 },
                new int[] { 7500, 7500, 11250, 11250 },
                new int[] { 10000, 15000, 15000, 20000 }
            },
            new int[][] // 6-part (regular)
            {
                new int[] { 5000, 5000, 7500, 7500 },
                new int[] { 7500, 7500, 11250, 11250 },
                new int[] { 10000, 15000, 15000, 20000 }
            },
            new int[][] // 6-part (exceptional)
            {
                new int[] { 7500, 7500, 10000, 10000 },
                new int[] { 11250, 11250, 15000, 15000 },
                new int[] { 15000, 20000, 20000, 30000 }
            }
        };

        public override int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int gold = 0;

            if (itemCount == 1 && BulkOrderSystem.NewSystemEnabled && BulkOrderSystem.ComputeGold(type, quantity, out gold))
            {
                return gold;
            }

            int[][][] goldTable = (Core.AOS ? m_AosGoldTable : m_OldGoldTable);

            int typeIndex = ((itemCount == 6 ? 3 : itemCount == 5 ? 2 : itemCount == 4 ? 1 : 0) * 2) + (exceptional ? 1 : 0);
            int quanIndex = (quantity >= 50 ? 2 : quantity >= 30 ? 1 : 0);
            int mtrlIndex = (material == BulkMaterialType.Barbed ? 3 : material == BulkMaterialType.Horned ? 2 : material == BulkMaterialType.Spined ? 1 : 0);

            gold = goldTable[typeIndex][quanIndex][mtrlIndex];


            int min = (gold * 9) / 10;
            int max = (gold * 10) / 9;

            return Utility.RandomMinMax(min, max);
        }
    }
    #endregion

    #region Tinkering Rewards
    public sealed class TinkeringRewardCalculator : RewardCalculator
    {
        public TinkeringRewardCalculator()
        {
            RewardCollection = new List<CollectionItem>();

            RewardCollection.Add(new BODCollectionItem(0x1EBC, "Ferramentas Duraveis", 0, 10, TinkerTools));
            RewardCollection.Add(new BODCollectionItem(0x14F0, 1157186, 0, 25, RewardTitle, 5));
            RewardCollection.Add(new BODCollectionItem(0x14F0, 1157187, 0, 50, RewardTitle, 6));
            RewardCollection.Add(new BODCollectionItem(0x14F0, 1157190, 0, 210, RewardTitle, 9));
            //RewardCollection.Add(new BODCollectionItem(0x2831, "Receita para varinha negra.<br>Receita de carpentry", 0, 225, Recipe, 0));
            RewardCollection.Add(new BODCollectionItem(0x14F0, 1157188, 0, 250, RewardTitle, 7));
            RewardCollection.Add(new BODCollectionItem(0x2831, "Receita para lente de contato", 0, 310, Recipe, 1));
            RewardCollection.Add(new BODCollectionItem(0x14F0, 1157189, 0, 225, RewardTitle, 8));
            RewardCollection.Add(new BODCollectionItem(0x2831, "Receita para automacao KOTL", 0, 350, Recipe, 2));
            RewardCollection.Add(new BODCollectionItem(0x9E2B, "Talisman Salva-Falhas 10xTinker<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 400, CraftsmanTalisman, 10));
            RewardCollection.Add(new BODCollectionItem(0x14F5, "Luneta", 0, 450, Luneta, 10));
            RewardCollection.Add(new BODCollectionItem(0x2F5B, 1152674, CraftResources.GetHue(CraftResource.Dourado), 450, SmeltersTalisman, (int)CraftResource.Dourado));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, 1152665, CraftResources.GetHue(CraftResource.Dourado), 500, HarvestMap, (int)CraftResource.Dourado));
            RewardCollection.Add(new BODCollectionItem(0x9E2B, "Talisman Salva-Falhas 15x Tinker<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 550, CraftsmanTalisman, 15)); // todo: Get id
            RewardCollection.Add(new BODCollectionItem(0x2F5B, 1152675, CraftResources.GetHue(CraftResource.Niobio), 600, SmeltersTalisman, (int)CraftResource.Niobio));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, 1152666, CraftResources.GetHue(CraftResource.Niobio), 650, HarvestMap, (int)CraftResource.Niobio));
            RewardCollection.Add(new BODCollectionItem(0x1940, "Keg de Po Fino", 0, 700, CreateItem, 0)); // powder of fort keg
            RewardCollection.Add(new BODCollectionItem(0x9CE9, "Forno Mecanico", 0, 750, CreateItem, 1)); // automaton actuator
            RewardCollection.Add(new BODCollectionItem(0x2F5B, 1152676, CraftResources.GetHue(CraftResource.Lazurita), 800, SmeltersTalisman, (int)CraftResource.Lazurita));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, 1152667, CraftResources.GetHue(CraftResource.Lazurita), 850, HarvestMap, (int)CraftResource.Lazurita));
            RewardCollection.Add(new BODCollectionItem(0x9E2B, "Talisman Salva-Falhas 20x Tinker<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 900, CraftsmanTalisman, 20));
            RewardCollection.Add(new BODCollectionItem(0x9E7E, 1157216, 0, 950, RockHammer));
            RewardCollection.Add(new BODCollectionItem(0x9CAA, "Pedra da Lua Negra", 1175, 1000, CreateItem, 2));
            RewardCollection.Add(new BODCollectionItem(0x2F5B, 1152677, CraftResources.GetHue(CraftResource.Quartzo), 1050, SmeltersTalisman, (int)CraftResource.Quartzo));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, 1152668, CraftResources.GetHue(CraftResource.Quartzo), 1100, HarvestMap, (int)CraftResource.Quartzo));
            RewardCollection.Add(new BODCollectionItem(0x9DB1, "Cabeca de Robo", 1175, 1200, CreateItem, 3));

            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 Mining<br>Aumenta seu cap de mining.", 0x481, RewardType.PS115, PowerScroll2, 15));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 Mining<br>Aumenta seu cap de mining.", 0x481, RewardType.PS120, PowerScroll2, 20));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 Mining<br>Aumenta seu cap de mining.", 0x481, RewardType.PS105, PowerScroll2, 5));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 Mining<br>Aumenta seu cap de mining.", 0x481, RewardType.PS110, PowerScroll2, 10));

            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 Tinkering<br>Aumenta seu cap de Tinkering.", 0x481, RewardType.PS105, PowerScroll, 5));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 Tinkering<br>Aumenta seu cap de Tinkering.", 0x481, RewardType.PS110, PowerScroll, 10));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 Tinkering<br>Aumenta seu cap de Tinkering.", 0x481, RewardType.PS115, PowerScroll, 15));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 Tinkering<br>Aumenta seu cap de Tinkering.", 0x481, RewardType.PS120, PowerScroll, 20));
        }


        private static Item CreatePowerScroll(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Tinkering, 100 + type);

            throw new InvalidOperationException();
        }

        private static Item CreatePowerScroll2(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Mining, 100 + type);

            throw new InvalidOperationException();
        }

        private static readonly ConstructCallback PowerScroll = new ConstructCallback(CreatePowerScroll);

        private static readonly ConstructCallback PowerScroll2 = new ConstructCallback(CreatePowerScroll2);

        #region Constructors
        // Do I need these since they aren't era-specific???

        private static Item TinkerTools(int type)
        {
            BaseTool tool = new TinkerTools();
            tool.UsesRemaining = 250;

            return tool;
        }

        public static Item CreateItem(int type)
        {
            switch (type)
            {
                case 0: return new PowderOfFortKeg();
                case 1: return new AutomatonActuator();
                case 2: return new BlackrockMoonstone();
                case 3: return new BlackrockAutomatonHead();
            }

            return null;
        }

        private static Item Luneta(int type)
        {
            return new Spyglass();
        }

        private static Item CraftsmanTalisman(int type)
        {
            return new MasterCraftsmanTalisman(type, 0x9E2B, TalismanSkill.Tinkering);
        }
        #endregion

        public static readonly TinkeringRewardCalculator Instance = new TinkeringRewardCalculator();

        public override int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int points = 0;

            if (quantity <= 10)
                points += 10;
            else if (quantity <= 30)
                points += 25;
            else if (quantity > 30)
                points += 50;

            switch (itemCount)
            {
                case 3: points += 200; break;
                case 4: points += 300; break;
                case 5: points += 350; break;
                case 6: points += 400; break;
            }

            if (material >= BulkMaterialType.Cobre && material <= BulkMaterialType.Vibranium)
                points += 200 + (50 * (material - BulkMaterialType.Cobre));

            if (exceptional)
                points += 200;

            return points * 2;
        }

        private static readonly int[][][] m_GoldTable = new int[][][]
        {
            new int[][] // 1-part (regular)
            {
                new int[] { 500, 500, 500, 500, 600, 600, 700, 700, 800 },
                new int[] { 700, 700, 700, 700, 800, 800, 800, 900, 900 },
                new int[] { 1000, 1100, 1100, 1100, 1200, 1200, 1200, 1200, 1200 },
            },
            new int[][] // 1-part (exceptional)
            {
                new int[] { 500*2, 500 * 2, 500 * 2, 500 * 2, 600 * 2, 600 * 2, 700 * 2, 700 * 2, 800 *2},
                new int[] { 700 * 2, 700 * 2, 700 * 2, 700 * 2, 800 * 2, 800 * 2, 800 * 2, 900 * 2, 900*2 },
                new int[] { 1000 * 2, 1100 * 2, 1100 * 2, 1100 * 2, 1200 * 2, 1200 * 2, 1200 * 2, 1200 * 2, 1200*2 },
            },
            new int[][] // 3-part (regular)
            {
                new int[] { 2500, 2500, 2500, 3500, 3500, 3500, 4500, 4500, 4500 },
                new int[] { 4000, 4000, 4000, 5500, 5500, 5500, 7000, 7000, 7000 },
                new int[] { 6000, 7000, 7500, 8000, 8000, 9000, 9000, 10000, 10000 }
            },
            new int[][] // 3-part (exceptional)
            {
                new int[] { 4000, 4000, 5000, 5750, 6500, 6500, 6500, 7500, 8500 },
                new int[] { 6500, 6500, 7500, 8500, 10000, 10000, 10000, 12500, 12500 },
                new int[] { 8000, 10000, 10000, 12500, 12500, 15000, 15000, 20000, 20000 }
            },
            new int[][] // 4-part (regular)
            {
                new int[] { 4000, 4000, 4000, 5000, 5000, 5000, 6000, 6000, 6000 },
                new int[] { 6000, 6000, 6000, 7500, 7500, 7500, 9000, 9000, 9000 },
                new int[] { 8000, 9000, 9500, 10000, 10000, 15000, 17500, 17500, 17500 }
            },
            new int[][] // 4-part (exceptional)
            {
                new int[] { 5000, 5000, 6000, 6750, 7500, 7500, 8500, 8500, 9500 },
                new int[] { 7500, 7500, 8500, 9500, 11250, 11250, 11250, 15000, 15000 },
                new int[] { 10000, 1250, 1250, 15000, 15000, 20000, 20000, 25000, 25000 }
            },
            new int[][] // 4-part (regular)
            {
                new int[] { 4000, 4000, 4000, 5000, 5000, 5000, 7000, 7000, 7000 },
                new int[] { 6000, 6000, 6000, 7500, 7500, 7500, 9000, 9000, 9000 },
                new int[] { 8000, 9000, 9500, 10000, 10000, 15000, 15000, 20000, 20000 }
            },
            new int[][] // 4-part (exceptional)
            {
                new int[] { 5000, 5000, 6000, 6750, 7500, 7500, 9000, 9000, 15000 },
                new int[] { 7500, 7500, 8500, 9500, 11250, 11250, 15000, 15000, 15000 },
                new int[] { 10000, 1250, 1250, 15000, 15000, 20000, 20000, 25000, 25000 }
            },
            new int[][] // 5-part (regular)
            {
                new int[] { 5000, 5000, 60000, 6000, 7500, 7500, 9000, 9000, 10500 },
                new int[] { 7500, 7500, 7500, 11250, 11250, 11250, 15000, 15000, 15000 },
                new int[] { 10000, 10000, 1250, 15000, 15000, 20000, 20000, 25000, 25000 }
            },
            new int[][] // 5-part (exceptional)
            {
                new int[] { 7500, 7500, 8500, 9500, 10000, 10000, 12500, 12500, 15000 },
                new int[] { 11250, 11250, 1250, 13500, 15000, 15000, 20000, 20000, 25000 },
                new int[] { 15000, 1750, 1750, 20000, 20000, 30000, 30000, 40000, 50000 }
            },
        };

        public override int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int gold = 0;

            if (itemCount == 1 && BulkOrderSystem.NewSystemEnabled && BulkOrderSystem.ComputeGold(type, quantity, out gold))
            {
                return gold;
            }

            int[][][] goldTable = m_GoldTable;

            int typeIndex = ((itemCount == 6 ? 3 : itemCount == 5 ? 2 : itemCount == 4 ? 1 : 0) * 2) + (exceptional ? 1 : 0);
            int quanIndex = (quantity >= 50 ? 2 : quantity >= 30 ? 1 : 0);
            int mtrlIndex = (material >= BulkMaterialType.Cobre && material <= BulkMaterialType.Vibranium) ? 1 + (int)(material - BulkMaterialType.Cobre) : 0;

            gold = goldTable[typeIndex][quanIndex][mtrlIndex];


            int min = (gold * 9) / 10;
            int max = (gold * 10) / 9;

            return Utility.RandomMinMax(min, max);
        }
    }
    #endregion

    #region Carpentry Rewards
    public sealed class CarpentryRewardCalculator : RewardCalculator
    {
        public CarpentryRewardCalculator()
        {
            RewardCollection = new List<CollectionItem>();

            RewardCollection.Add(new BODCollectionItem(0x1028, "Serra Duravel<br>Contem 1000 usos.", 0, 10, DovetailSaw));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Madeireiro", 0, 25, RewardTitle, 10));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo de Carpinteiro", 0, 50, RewardTitle, 11));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo de Mestre da Madeirada", 0, 250, RewardTitle, 12));
            RewardCollection.Add(new BODCollectionItem(0x9E2C, "Talisman Salva-Falhas 10x Carpentry<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 300, CraftsmanTalisman, 10));
            RewardCollection.Add(new BODCollectionItem(0x2F5A, "Talisman para cortar tabuas de Mogno", CraftResources.GetHue(CraftResource.Mogno), 350, WoodsmansTalisman, (int)CraftResource.Mogno));
            RewardCollection.Add(new BODCollectionItem(0x9E2C, "Talisman Salva-Falhas 15x Carpentry<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 450, CraftsmanTalisman, 15));
            //RewardCollection.Add(new BODCollectionItem(0x12B3, 1157293, CraftResources.GetHue(CraftResource.Berilo), 450, RunicMalletAndChisel, 1));
            // RewardCollection.Add(new BODCollectionItem(0x12B3, 1157294, CraftResources.GetHue(CraftResource.Vibranium), 450, RunicMalletAndChisel, 2));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, 1152669, CraftResources.GetHue(CraftResource.Mogno), 500, HarvestMap, (int)CraftResource.Mogno));
            //RewardCollection.Add(new BODCollectionItem(0x1029, 1157223, CraftResources.GetHue(CraftResource.Carvalho), 550, RunicDovetailSaw, 0));
            //RewardCollection.Add(new BODCollectionItem(0x12B3, 1157295, CraftResources.GetHue(CraftResource.Cobre), 600, RunicMalletAndChisel, 3));
            //RewardCollection.Add(new BODCollectionItem(0x12B3, 1157296, CraftResources.GetHue(CraftResource.Bronze), 650, RunicMalletAndChisel, 4));
            RewardCollection.Add(new BODCollectionItem(0x2F5A, "Talisman para cortar tabuas de Eucalipto", CraftResources.GetHue(CraftResource.Eucalipto), 650, WoodsmansTalisman, (int)CraftResource.Eucalipto));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa de Coleta para Eucalipto", CraftResources.GetHue(CraftResource.Eucalipto), 200, HarvestMap, (int)CraftResource.Eucalipto));
            //RewardCollection.Add(new BODCollectionItem(0x1029, 1157224, CraftResources.GetHue(CraftResource.Pinho), 750, RunicDovetailSaw, 1));
            RewardCollection.Add(new BODCollectionItem(0x9E2C, "Talisman Salva-Falhas 20x Carpentry<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 800, CraftsmanTalisman, 20));
            RewardCollection.Add(new BODCollectionItem(0x2F5A, "Talisman para cortar tabuas de Carmesim", CraftResources.GetHue(CraftResource.Carmesim), 850, WoodsmansTalisman, (int)CraftResource.Carmesim));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa de coleta para Carmesim", CraftResources.GetHue(CraftResource.Carmesim), 800, HarvestMap, (int)CraftResource.Carmesim));
            //RewardCollection.Add(new BODCollectionItem(0x12B3, 1157297, CraftResources.GetHue(CraftResource.Prata), 900, RunicMalletAndChisel, 5));
            //RewardCollection.Add(new BODCollectionItem(0x1029, 1157225, CraftResources.GetHue(CraftResource.Mogno), 950, RunicDovetailSaw, 2));
            RewardCollection.Add(new BODCollectionItem(0x2F5A, "Talisman para cortar tabuas de madeira gelida", CraftResources.GetHue(CraftResource.Gelo), 1000, WoodsmansTalisman, (int)CraftResource.Gelo));
            //RewardCollection.Add(new BODCollectionItem(0x12B3, 1157298, CraftResources.GetHue(CraftResource.Niobio), 1000, RunicMalletAndChisel, 6));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa para coleta de madeira gelida", CraftResources.GetHue(CraftResource.Gelo), 1050, HarvestMap, (int)CraftResource.Gelo));
            //RewardCollection.Add(new BODCollectionItem(0x12B3, 1157299, CraftResources.GetHue(CraftResource.Lazurita), 1100, RunicMalletAndChisel, 7));
            //RewardCollection.Add(new BODCollectionItem(0x1029, 1157226, CraftResources.GetHue(CraftResource.Eucalipto), 1150, RunicDovetailSaw, 3));
            //RewardCollection.Add(new BODCollectionItem(0x12B3, 1157300, CraftResources.GetHue(CraftResource.Quartzo), 1150, RunicMalletAndChisel, 8));

            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 Carpentry<br>Aumenta seu cap de carpentry.", 0x481, RewardType.PS105, PowerScroll, 5));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 Carpentry<br>Aumenta seu cap de carpentry.", 0x481, RewardType.PS110, PowerScroll, 10));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 Carpentry<br>Aumenta seu cap de carpentry.", 0x481, RewardType.PS115, PowerScroll, 15));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 Carpentry<br>Aumenta seu cap de carpentry.", 0x481, RewardType.PS120, PowerScroll, 20));

            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 Lumberjacking<br>Aumenta seu cap de Lumberjacking.", 0x481, RewardType.PS105, PowerScroll2, 5));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 Lumberjacking<br>Aumenta seu cap de Lumberjacking.", 0x481, RewardType.PS110, PowerScroll2, 10));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 Lumberjacking<br>Aumenta seu cap de Lumberjacking.", 0x481, RewardType.PS115, PowerScroll2, 15));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 Lumberjacking<br>Aumenta seu cap de Lumberjacking.", 0x481, RewardType.PS120, PowerScroll2, 20));

        }

        private static Item CreatePowerScroll(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Carpentry, 100 + type);

            throw new InvalidOperationException();
        }


        private static Item CreatePowerScroll2(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Lumberjacking, 100 + type);

            throw new InvalidOperationException();
        }


        private static readonly ConstructCallback PowerScroll = new ConstructCallback(CreatePowerScroll);
        private static readonly ConstructCallback PowerScroll2 = new ConstructCallback(CreatePowerScroll2);

        #region Constructors

        private static Item DovetailSaw(int type)
        {
            BaseTool tool = new DovetailSaw();
            tool.UsesRemaining = 1000;

            return tool;
        }

        private static Item RunicMalletAndChisel(int type)
        {
            if (type >= 1 && type <= 8)
                return new RunicMalletAndChisel(CraftResource.Ferro + type, Core.AOS ? (55 - (type * 5)) : 50);

            return null;
        }

        private static Item RunicDovetailSaw(int type)
        {
            switch (type)
            {
                default:
                case 0: return new RunicDovetailSaw(CraftResource.Carvalho, 45);
                case 1: return new RunicDovetailSaw(CraftResource.Pinho, 35);
                case 2: return new RunicDovetailSaw(CraftResource.Mogno, 25);
                case 3: return new RunicDovetailSaw(CraftResource.Eucalipto, 15);
            }
        }

        private static Item CraftsmanTalisman(int type)
        {
            return new MasterCraftsmanTalisman(type, 0x9E2C, TalismanSkill.Carpentry);
        }
        #endregion

        public static readonly CarpentryRewardCalculator Instance = new CarpentryRewardCalculator();

        public override int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int points = 0;

            if (quantity <= 10)
                points += 10;
            else if (quantity <= 30)
                points += 25;
            else if (quantity > 30)
                points += 50;

            if (exceptional)
                points += 200;

            switch (material)
            {
                case BulkMaterialType.None: break;
                case BulkMaterialType.Carvalho: points += 300; break;
                case BulkMaterialType.Pinho: points += 350; break;
                case BulkMaterialType.Mogno: points += 400; break;
                case BulkMaterialType.Eucalipto: points += 450; break;
                case BulkMaterialType.Carmesin: points += 500; break;
                case BulkMaterialType.Gelo: points += 550; break;
            }

            if (itemCount > 1)
                points += this.LookupTypePoints(m_Types, type);

            return points;
        }

        private RewardType[] m_Types =
        {
            new RewardType(250, typeof(TallCabinet), typeof(ShortCabinet)),
            new RewardType(250, typeof(RedArmoire), typeof(ElegantArmoire), typeof(MapleArmoire), typeof(CherryArmoire)),
            new RewardType(300, typeof(PlainWoodenChest), typeof(OrnateWoodenChest), typeof(GildedWoodenChest), typeof(WoodenFootLocker), typeof(FinishedWoodenChest)),
            new RewardType(350, typeof(WildStaff), typeof(ArcanistsWildStaff), typeof(AncientWildStaff), typeof(ThornedWildStaff), typeof(HardenedWildStaff)),
            new RewardType(250, typeof(LapHarp), typeof(Lute), typeof(Drums), typeof(Harp)),
            new RewardType(200, typeof(GnarledStaff), typeof(QuarterStaff), typeof(ShepherdsCrook), typeof(Tetsubo), typeof(Bokuto)),
            new RewardType(300, typeof(WoodenBox), typeof(EmptyBookcase), typeof(WoodenBench), typeof(WoodenThrone)),
        };

        private static readonly int[][][] m_GoldTable = new int[][][]
        {
            new int[][] // 1-part (regular)
            {
                new int[] { 300, 300, 325, 325, 350, 350 },
                new int[] { 300, 300, 325, 325, 350, 350 },
                new int[] { 300, 400, 500, 500, 600, 750 }
            },
            new int[][] // 1-part (exceptional)
            {
                new int[] { 300, 300, 400, 500, 600, 600 },
                new int[] { 450, 450, 650, 750, 900, 900 },
                new int[] { 600, 750, 850, 1000, 1200, 1800 }
            },
            new int[][] // 2-part (regular)
            {
                new int[] { 2000, 2000, 2000, 2500, 2500, 2500 },
                new int[] { 3000, 3000, 3000, 3750, 3750, 3750 },
                new int[] { 4000, 4500, 4500, 5000, 5000, 7500 }
            },
            new int[][] // 2-part (exceptional)
            {
                new int[] { 2500, 2500, 3000, 3350, 3350, 4000 },
                new int[] { 3750, 3750, 4250, 4750, 5200, 5200 },
                new int[] { 5000, 6100, 6100, 7000, 7000, 10000 }
            },
            new int[][] // 4-part (regular)
            {
                new int[] { 4000, 4000, 4000, 5000, 5000, 5000 },
                new int[] { 6000, 6000, 6000, 7500, 7500, 7500 },
                new int[] { 8000, 9000, 9500, 10000, 10000, 15000 }
            },
            new int[][] // 4-part (exceptional)
            {
                new int[] { 5000, 5000, 6000, 6750, 7500, 7500 },
                new int[] { 7500, 7500, 8500, 9500, 11250, 11250 },
                new int[] { 10000, 1250, 1250, 15000, 15000, 20000 }
            },
            new int[][] // 5-part (regular)
            {
                new int[] { 5000, 5000, 60000, 6000, 7500, 7500 },
                new int[] { 7500, 7500, 7500, 11250, 11250, 11250 },
                new int[] { 10000, 10000, 1250, 15000, 15000, 20000 }
            },
            new int[][] // 5-part (exceptional)
            {
                new int[] { 7500, 7500, 8500, 9500, 10000, 10000 },
                new int[] { 11250, 11250, 1250, 1350, 15000, 15000 },
                new int[] { 15000, 1750, 1750, 20000, 20000, 30000 }
            },
        };

        public override int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int gold = 0;

            if (itemCount == 1 && BulkOrderSystem.NewSystemEnabled && BulkOrderSystem.ComputeGold(type, quantity, out gold))
            {
                return gold;
            }

            int[][][] goldTable = m_GoldTable;

            int typeIndex = ((itemCount == 5 ? 2 : itemCount == 4 ? 1 : 0) * 2) + (exceptional ? 1 : 0);
            int quanIndex = (quantity >= 50 ? 2 : quantity >= 30 ? 1 : 0);
            int mtrlIndex = (material == BulkMaterialType.Gelo ? 5 : material == BulkMaterialType.Carmesin ? 4 : material == BulkMaterialType.Eucalipto ? 3 : material == BulkMaterialType.Mogno ? 2 : material == BulkMaterialType.Pinho ? 1 : 0);

            gold = goldTable[typeIndex][quanIndex][mtrlIndex];


            int min = (gold * 9) / 10;
            int max = (gold * 10) / 9;

            return Utility.RandomMinMax(min, max);
        }
    }
    #endregion

    #region Inscription Rewards
    public sealed class InscriptionRewardCalculator : RewardCalculator
    {
        public InscriptionRewardCalculator()
        {
            RewardCollection = new List<CollectionItem>();

            RewardCollection.Add(new BODCollectionItem(0x0FBF, "Caneta Duravel<br>Contem 1000 cargas", 0, 10, ScribesPen));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Escritor", 0, 25, RewardTitle, 13));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Escriba", 0, 50, RewardTitle, 14));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Poeta", 0, 210, RewardTitle, 15));
            RewardCollection.Add(new BODCollectionItem(0x2831, "Receita para Atlas Runico", 0, 210, Recipe, 3));
            RewardCollection.Add(new BODCollectionItem(0x182B, "Tinta para Livros", 2741, 1250, NaturalDye, 3));
            RewardCollection.Add(new BODCollectionItem(0x9E28, "Talisman Salva-Falhas 10xInscription", 0, 275, CraftsmanTalisman, 10));
            RewardCollection.Add(new BODCollectionItem(0x182B, "Tinta para Livros", 2740, 1310, NaturalDye, 4));
            RewardCollection.Add(new BODCollectionItem(0x9E28, "Talisman Salva-Falhas 15x Inscription", 0, 350, CraftsmanTalisman, 15));
            RewardCollection.Add(new BODCollectionItem(0x182B, "Tinta para Livros", 2732, 1375, NaturalDye, 5));
            RewardCollection.Add(new BODCollectionItem(0x9E28, "Talisman Salva-Falhas 20x Inscription", 0, 410, CraftsmanTalisman, 20));
            RewardCollection.Add(new BODCollectionItem(0x182B, "Tinta para Livros", 2731, 1450, NaturalDye, 6));
            RewardCollection.Add(new BODCollectionItem(0x182B, "Tinta para Livros", 2735, 1475, NaturalDye, 7));
            //RewardCollection.Add(new BODCollectionItem(0x9E28, 1157291, 0, 500, ImprovementTalisman, 10));
        }

        #region Constructors

        private static Item ScribesPen(int type)
        {
            BaseTool tool = new ScribesPen();
            tool.UsesRemaining = 1000;
            return tool;
        }

        private static Item CraftsmanTalisman(int type)
        {
            return new MasterCraftsmanTalisman(type, 0x9E28, TalismanSkill.Inscription);
        }

        private static Item ImprovementTalisman(int type)
        {
            return new GuaranteedSpellbookImprovementTalisman(type);
        }

        #endregion

        public static readonly InscriptionRewardCalculator Instance = new InscriptionRewardCalculator();

        public override int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int points = 0;

            if (quantity <= 10)
                points += 10;
            else if (quantity <= 30)
                points += 25;
            else if (quantity > 30)
                points += 50;

            if (itemCount > 1)
                points += this.LookupTypePoints(m_Types, type);

            return points * 2;
        }

        private RewardType[] m_Types =
        {
            new RewardType(200, typeof(ClumsyScroll), typeof(FeeblemindScroll), typeof(WeakenScroll)),
            new RewardType(300, typeof(CurseScroll), typeof(GreaterHealScroll), typeof(RecallScroll)),
            new RewardType(300, typeof(PoisonStrikeScroll), typeof(WitherScroll), typeof(StrangleScroll)),
            new RewardType(250, typeof(MindRotScroll), typeof(SummonFamiliarScroll), typeof(AnimateDeadScroll), typeof(HorrificBeastScroll)),
            new RewardType(200, typeof(HealScroll), typeof(AgilityScroll), typeof(CunningScroll), typeof(CureScroll), typeof(StrengthScroll)),
            new RewardType(250, typeof(BloodOathScroll), typeof(CorpseSkinScroll), typeof(CurseWeaponScroll), typeof(EvilOmenScroll), typeof(PainSpikeScroll)),
            new RewardType(300, typeof(BladeSpiritsScroll), typeof(DispelFieldScroll), typeof(MagicReflectScroll), typeof(ParalyzeScroll), typeof(SummonCreatureScroll)),
            new RewardType(350, typeof(ChainLightningScroll), typeof(FlamestrikeScroll), typeof(ManaVampireScroll), typeof(MeteorSwarmScroll), typeof(PolymorphScroll)),
            new RewardType(400, typeof(SummonAirElementalScroll), typeof(SummonDaemonScroll), typeof(SummonEarthElementalScroll), typeof(SummonFireElementalScroll), typeof(SummonWaterElementalScroll)),
            new RewardType(450, typeof(Spellbook), typeof(NecromancerSpellbook), typeof(Runebook), typeof(RunicAtlas))
        };

        private static readonly int[][] m_GoldTable = new int[][]
        {
            new int[] // singles
            {
                300, 300, 300
            },
            new int[] // no 2 piece
            {
            },
            new int[] // 3-part
            {
                4000, 6000, 8000
            },
            new int[] // 4-part
            {
                5000, 7500, 10000
            },
            new int[] // 5-part
            {
                7500, 11250, 15000
            },
        };

        public override int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int gold = 0;

            if (itemCount == 1 && BulkOrderSystem.NewSystemEnabled && BulkOrderSystem.ComputeGold(type, quantity, out gold))
            {
                return gold;
            }

            int[][] goldTable = m_GoldTable;

            int quanIndex = (quantity >= 50 ? 2 : quantity >= 30 ? 1 : 0);

            gold = goldTable[itemCount - 1][quanIndex];

            int min = (gold * 9) / 10;
            int max = (gold * 10) / 9;

            return Utility.RandomMinMax(min, max);
        }
    }
    #endregion

    #region Cooking Rewards
    public sealed class CookingRewardCalculator : RewardCalculator
    {
        public CookingRewardCalculator()
        {
            RewardCollection = new List<CollectionItem>();

            RewardCollection.Add(new BODCollectionItem(0x97F, "Frigideira Duravel<br>Dura 1000 usos", 0, 10, Skillet));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Padeiro", 0, 25, RewardTitle, 16));
            RewardCollection.Add(new BODCollectionItem(0x2831, "Receita de Biscoitinho de Natal", 0, 25, Recipe, 4));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Cozinheiro", 0, 50, RewardTitle, 17));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Masterchef Gordao", 0, 210, RewardTitle, 18));
            RewardCollection.Add(new BODCollectionItem(0x9E27, "Talisman Salva-Falhas 10xCooking", 0, 250, CraftsmanTalisman, 10));
            RewardCollection.Add(new BODCollectionItem(0x9E27, "Talisman Salva-Falhas 15x Cooking", 0, 300, CraftsmanTalisman, 15));
            RewardCollection.Add(new BODCollectionItem(0x9E27, "Talisman Salva-Falhas 20x Cooking", 0, 350, CraftsmanTalisman, 20));
            RewardCollection.Add(new BODCollectionItem(0x153D, "Avental de Cozinha", 1990, 410, CreateItem, 0));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Addon de Pessegueira", 0, 475, CreateItem, 1));
            RewardCollection.Add(new BODCollectionItem(0x182B, "Tinta Natural<br>Pinta livros, madeiras, ossos e outras coisas.", 2740, 1525, NaturalDye, 8));
            RewardCollection.Add(new BODCollectionItem(0x182B, "Tinta Natural<br>Pinta livros, madeiras, ossos e outras coisas.", 2732, 1625, NaturalDye, 9));
            RewardCollection.Add(new BODCollectionItem(0x9E36, "Barril de Fermentacao", 0, 625, CreateItem, 2));
        }

        #region Constructors

        private static Item Skillet(int type)
        {
            BaseTool tool = new SkilletExp();
            tool.UsesRemaining = 1000;

            return tool;
        }

        private static Item CraftsmanTalisman(int type)
        {
            return new MasterCraftsmanTalisman(type, 0x9E27, TalismanSkill.Cooking);
        }

        private static Item CreateItem(int type)
        {
            switch (type)
            {
                case 0: return new MasterChefsApron();
                case 1: return new PlumTreeAddonDeed();
                case 2: return new FermentationBarrel();
            }
            return null;
        }

        #endregion

        public static readonly CookingRewardCalculator Instance = new CookingRewardCalculator();

        public override int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int points = 0;

            if (quantity <= 10)
                points += 10;
            else if (quantity <= 30)
                points += 25;
            else if (quantity > 30)
                points += 50;

            if (exceptional)
                points += 200;

            if (itemCount > 1)
                points += this.LookupTypePoints(m_Types, type);

            return points;
        }

        private RewardType[] m_Types =
        {
            new RewardType(200, typeof(SweetCocoaButter), typeof(SackFlour), typeof(Dough)),
            new RewardType(250, typeof(UnbakedFruitPie), typeof(UnbakedPeachCobbler), typeof(UnbakedApplePie), typeof(UnbakedPumpkinPie)),
            new RewardType(300, typeof(CookedBird), typeof(FishSteak), typeof(FriedEggs), typeof(LambLeg), typeof(Ribs)),
            new RewardType(350, typeof(Cookies), typeof(Cake), typeof(Muffins), typeof(ThreeTieredCake)),
            new RewardType(400, typeof(EnchantedApple), typeof(TribalPaint), typeof(GrapesOfWrath), typeof(EggBomb)),
            new RewardType(450, typeof(MisoSoup), typeof(WhiteMisoSoup), typeof(RedMisoSoup), typeof(AwaseMisoSoup)),
            new RewardType(500, typeof(WasabiClumps), typeof(SushiRolls), typeof(SushiPlatter), typeof(GreenTea)),
        };

        private static readonly int[][] m_GoldTable = new int[][]
        {
            new int[] // singles
            {
                300, 300, 300
            },
            new int[] // no 2 piece
            {
            },
            new int[] // 3-part
            {
                4000, 6000, 8000
            },
            new int[] // 4-part
            {
                5000, 7500, 10000
            },
            new int[] // 5-part
            {
                7500, 11250, 15000
            },
            new int[] // 6-part (regular)
            {
                7500, 11250, 15000
            },
        };

        public override int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int gold = 0;

            if (itemCount == 1 && BulkOrderSystem.NewSystemEnabled && BulkOrderSystem.ComputeGold(type, quantity, out gold))
            {
                return gold;
            }

            int[][] goldTable = m_GoldTable;

            int quanIndex = (quantity >= 50 ? 2 : quantity >= 30 ? 1 : 0);

            gold = goldTable[itemCount - 1][quanIndex];

            int min = (gold * 9) / 10;
            int max = (gold * 10) / 9;

            return Utility.RandomMinMax(min, max);
        }
    }
    #endregion

    #region Fletching Rewards
    public sealed class FletchingRewardCalculator : RewardCalculator
    {
        public FletchingRewardCalculator()
        {
            RewardCollection = new List<CollectionItem>();

            RewardCollection.Add(new BODCollectionItem(0x1022, "Ferramentas de Arqueiro Duravel<br>Dura 10000 usos", 0, 10, FletcherTools));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Criador de Flechas", 0, 25, RewardTitle, 19));
            RewardCollection.Add(new BODCollectionItem(0x9E29, "Talisman Salva-Falhas 10xBowcraft", 0, 210, CraftsmanTalisman, 10));
            RewardCollection.Add(new BODCollectionItem(0x2F5A, "Talisman para cortar tabuas de Mogno", CraftResources.GetHue(CraftResource.Mogno), 350, WoodsmansTalisman, (int)CraftResource.Mogno));
            RewardCollection.Add(new BODCollectionItem(0x9E2C, "Talisman Salva-Falhas 15x Bowcraft<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 450, CraftsmanTalisman, 15));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, 1152669, CraftResources.GetHue(CraftResource.Mogno), 500, HarvestMap, (int)CraftResource.Mogno));
            RewardCollection.Add(new BODCollectionItem(0x2F5A, "Talisman para cortar tabuas de Eucalipto", CraftResources.GetHue(CraftResource.Eucalipto), 650, WoodsmansTalisman, (int)CraftResource.Eucalipto));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa de Coleta para Eucalipto", CraftResources.GetHue(CraftResource.Eucalipto), 200, HarvestMap, (int)CraftResource.Eucalipto));
            RewardCollection.Add(new BODCollectionItem(0x9E2C, "Talisman Salva-Falhas 20x Bowcraft<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 800, CraftsmanTalisman, 20));
            RewardCollection.Add(new BODCollectionItem(0x2F5A, "Talisman para cortar tabuas de Carmesim", CraftResources.GetHue(CraftResource.Carmesim), 850, WoodsmansTalisman, (int)CraftResource.Carmesim));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa de coleta para Carmesim", CraftResources.GetHue(CraftResource.Carmesim), 900, HarvestMap, (int)CraftResource.Carmesim));
            RewardCollection.Add(new BODCollectionItem(0x2F5A, "Talisman para cortar tabuas de madeira gelida", CraftResources.GetHue(CraftResource.Gelo), 1000, WoodsmansTalisman, (int)CraftResource.Gelo));
            //RewardCollection.Add(new BODCollectionItem(0x14EC, "Mapa para coleta de madeira gelida", CraftResources.GetHue(CraftResource.Gelo), 1050, HarvestMap, (int)CraftResource.Gelo));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 Bowcraft<br>Aumenta seu cap de bowcraft.", 0x481, RewardType.PS105, PowerScroll, 5));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 Bowcraft<br>Aumenta seu cap de bowcraft.", 0x481, RewardType.PS110, PowerScroll, 10));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 Bowcraft<br>Aumenta seu cap de bowcraft.", 0x481, RewardType.PS115, PowerScroll, 15));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 Bowcraft<br>Aumenta seu cap de bowcraft.", 0x481, RewardType.PS120, PowerScroll, 20));

            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 Lumberjacking<br>Aumenta seu cap de Lumberjacking.", 0x481, RewardType.PS105, PowerScroll2, 5));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 Lumberjacking<br>Aumenta seu cap de Lumberjacking.", 0x481, RewardType.PS110, PowerScroll2, 10));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 Lumberjacking<br>Aumenta seu cap de Lumberjacking.", 0x481, RewardType.PS115, PowerScroll2, 15));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 Lumberjacking<br>Aumenta seu cap de Lumberjacking.", 0x481, RewardType.PS120, PowerScroll2, 20));
        }

        private static Item CreatePowerScroll(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Bowcraft, 100 + type);

            throw new InvalidOperationException();
        }

        private static Item CreatePowerScroll2(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Lumberjacking, 100 + type);

            throw new InvalidOperationException();
        }

        private static readonly ConstructCallback PowerScroll = new ConstructCallback(CreatePowerScroll);
        private static readonly ConstructCallback PowerScroll2 = new ConstructCallback(CreatePowerScroll2);

        #region Constructors

        private static Item FletcherTools(int type)
        {
            BaseTool tool = new FletcherTools();
            tool.UsesRemaining = 1000;

            return tool;
        }

        private static Item CraftsmanTalisman(int type)
        {
            return new MasterCraftsmanTalisman(type, 0x9E29, TalismanSkill.Fletching);
        }

        private static Item CreateRunicFletcherTools(int type)
        {
            switch (type)
            {
                default:
                case 0: return new RunicFletcherTool(CraftResource.Carvalho, 45);
                case 1: return new RunicFletcherTool(CraftResource.Pinho, 35);
                case 2: return new RunicFletcherTool(CraftResource.Mogno, 25);
                case 3: return new RunicFletcherTool(CraftResource.Eucalipto, 15);
            }
        }

        #endregion

        public static readonly FletchingRewardCalculator Instance = new FletchingRewardCalculator();

        public override int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int points = 0;

            if (quantity <= 10)
                points += 10;
            else if (quantity <= 30)
                points += 25;
            else if (quantity > 30)
                points += 50;

            if (exceptional)
                points += 200;

            switch (material)
            {
                case BulkMaterialType.None: break;
                case BulkMaterialType.Carvalho: points += 300; break;
                case BulkMaterialType.Pinho: points += 350; break;
                case BulkMaterialType.Mogno: points += 400; break;
                case BulkMaterialType.Eucalipto: points += 450; break;
                case BulkMaterialType.Carmesin: points += 500; break;
                case BulkMaterialType.Gelo: points += 550; break;
            }

            if (itemCount > 1)
                points += this.LookupTypePoints(m_Types, type);

            return points;
        }

        private RewardType[] m_Types =
        {
            new RewardType(200, typeof(Arrow), typeof(Bolt)),
            new RewardType(300, typeof(Bow), typeof(CompositeBow), typeof(Yumi)),
            new RewardType(300, typeof(Crossbow), typeof(HeavyCrossbow), typeof(RepeatingCrossbow)),
            new RewardType(350, typeof(MagicalShortbow), typeof(RangersShortbow), typeof(LightweightShortbow), typeof(MysticalShortbow), typeof(AssassinsShortbow)),
            new RewardType(250, typeof(ElvenCompositeLongbow), typeof(BarbedLongbow), typeof(SlayerLongbow), typeof(FrozenLongbow), typeof(LongbowOfMight)),
        };

        private static readonly int[][][] m_GoldTable = new int[][][]
        {
            new int[][] // 1-part (regular)
            {
                new int[] { 300, 300, 300, 300, 300, 300 },
                new int[] { 300, 300, 325, 325, 450, 450 },
                new int[] { 300, 400, 500, 500, 600, 750 }
            },
            new int[][] // 1-part (exceptional)
            {
                new int[] { 300, 300, 400, 500, 600, 600 },
                new int[] { 450, 450, 650, 750, 900, 900 },
                new int[] { 600, 750, 850, 1000, 1200, 1800 }
            },
            new int[][] // 4-part (regular)
            {
                new int[] { 4000, 4000, 4000, 5000, 5000, 5000 },
                new int[] { 6000, 6000, 6000, 7500, 7500, 7500 },
                new int[] { 8000, 9000, 9500, 10000, 10000, 15000 }
            },
            new int[][] // 4-part (exceptional)
            {
                new int[] { 5000, 5000, 6000, 6750, 7500, 7500 },
                new int[] { 7500, 7500, 8500, 9500, 11250, 11250 },
                new int[] { 10000, 1250, 1250, 15000, 15000, 20000 }
            },
            new int[][] // 5-part (regular)
            {
                new int[] { 5000, 5000, 60000, 6000, 7500, 7500 },
                new int[] { 7500, 7500, 7500, 11250, 11250, 11250 },
                new int[] { 10000, 10000, 1250, 15000, 15000, 20000 }
            },
            new int[][] // 5-part (exceptional)
            {
                new int[] { 7500, 7500, 8500, 9500, 10000, 10000 },
                new int[] { 11250, 11250, 1250, 1350, 15000, 15000 },
                new int[] { 15000, 1750, 1750, 20000, 20000, 30000 }
            },
        };

        public override int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int gold = 0;

            if (itemCount == 1 && BulkOrderSystem.NewSystemEnabled && BulkOrderSystem.ComputeGold(type, quantity, out gold))
            {
                return gold;
            }

            int[][][] goldTable = m_GoldTable;

            int typeIndex = ((itemCount == 5 ? 2 : itemCount == 4 ? 1 : 0) * 2) + (exceptional ? 1 : 0);
            int quanIndex = (quantity >= 50 ? 2 : quantity >= 30 ? 1 : 0);
            int mtrlIndex = (material == BulkMaterialType.Gelo ? 5 : material == BulkMaterialType.Carmesin ? 4 : material == BulkMaterialType.Eucalipto ? 3 : material == BulkMaterialType.Mogno ? 2 : material == BulkMaterialType.Pinho ? 1 : 0);

            gold = goldTable[typeIndex][quanIndex][mtrlIndex];

            int min = (gold * 9) / 10;
            int max = (gold * 10) / 9;

            return Utility.RandomMinMax(min, max);
        }
    }
    #endregion

    #region Alchemy Rewards
    public sealed class AlchemyRewardCalculator : RewardCalculator
    {
        public AlchemyRewardCalculator()
        {
            RewardCollection = new List<CollectionItem>();

            RewardCollection.Add(new BODCollectionItem(0xE9B, "Pilao Duravel<br>Dura 1000 usos", 0, 10, MortarAndPestle));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Criador de Pocoes", 0, 25, RewardTitle, 20));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Alquimista", 0, 50, RewardTitle, 21));
            RewardCollection.Add(new BODCollectionItem(0x14F0, "Titulo Criador de Dorgas", 0, 210, RewardTitle, 22));
            RewardCollection.Add(new BODCollectionItem(0x182B, "Tinta natural<br>Pinta muitas coisas diferentes", 2741, 1225, NaturalDye, 0));
            RewardCollection.Add(new BODCollectionItem(0x975, "Caldeirao de Pinho<br>Dura um tempo", CraftResources.GetHue(CraftResource.Pinho), 250, Cauldron, 0));
            RewardCollection.Add(new BODCollectionItem(0x975, "Caldeirao de Bronze<br>Dura um tempo", CraftResources.GetHue(CraftResource.Bronze), 260, Cauldron, 1));
            RewardCollection.Add(new BODCollectionItem(0x9E26, "Talisman Salva-Falhas 10xAlchemy<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 275, CraftsmanTalisman, 10)); // todo: Get id
            RewardCollection.Add(new BODCollectionItem(0x975, "Caldeirao de Mogno<br>Dura um tempo", CraftResources.GetHue(CraftResource.Mogno), 300, Cauldron, 2));
            RewardCollection.Add(new BODCollectionItem(0x975, "Caldeirao de Prata<br>Dura um tempo", CraftResources.GetHue(CraftResource.Dourado), 310, Cauldron, 3));
            RewardCollection.Add(new BODCollectionItem(0x9E26, "Talisman Salva-Falhas 15x Alchemy<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 325, CraftsmanTalisman, 15)); // todo: Get id
            RewardCollection.Add(new BODCollectionItem(0x975, "Caldeirao de Niobio<br>Dura um tempo", CraftResources.GetHue(CraftResource.Niobio), 350, Cauldron, 4));
            RewardCollection.Add(new BODCollectionItem(0x975, "Caldeirao de Eucalipto<br>Dura um tempo", CraftResources.GetHue(CraftResource.Eucalipto), 360, Cauldron, 5));
            RewardCollection.Add(new BODCollectionItem(0x9E26, "Talisman Salva-Falhas 20x Alchemy<br>Nao perde recursos ao falhar e chance de ser encantado", 0, 375, CraftsmanTalisman, 20)); // todo: Get id
            RewardCollection.Add(new BODCollectionItem(0x182B, "Tinta natural<br>Pinta muitas coisas diferentes", 2731, 1400, NaturalDye, 1));
            RewardCollection.Add(new BODCollectionItem(0x975, "Caldeirao de Carmesim<br>Dura um tempo", CraftResources.GetHue(CraftResource.Carmesim), 410, Cauldron, 6));
            RewardCollection.Add(new BODCollectionItem(0x182B, "Tinta natural<br>Pinta muitas coisas diferentes", 2735, 1425, NaturalDye, 2));
            RewardCollection.Add(new BODCollectionItem(0x975, "Caldeirao de Lazurita<br>Dura um tempo", CraftResources.GetHue(CraftResource.Lazurita), 450, Cauldron, 7));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 100-105 Alchemy<br>Aumenta seu cap de Alchemy para de 100 para 105.", 0x481, RewardType.PS105, PowerScroll, 5));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 105-110 Alchemy<br>Aumenta seu cap de Alchemy para de 105 para 110.", 0x481, RewardType.PS110, PowerScroll, 10));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 110-115 Alchemy<br>Aumenta seu cap de Alchemy para de 110 para 115.", 0x481, RewardType.PS115, PowerScroll, 15));
            RewardCollection.Add(new BODCollectionItem(0xEF3, "Powerscroll 115-120 Alchemy<br>Aumenta seu cap de Alchemy para de 115 para 120.", 0x481, RewardType.PS120, PowerScroll, 20));
        }

        private static Item CreatePowerScroll(int type)
        {
            if (type == 5 || type == 10 || type == 15 || type == 20)
                return new PowerScrollNovo(SkillName.Alchemy, 100 + type);

            throw new InvalidOperationException();
        }

        private static readonly ConstructCallback PowerScroll = new ConstructCallback(CreatePowerScroll);
        #region Constructors
        private static Item MortarAndPestle(int type)
        {
            BaseTool tool = new MortarPestle();
            tool.UsesRemaining = 1000;

            return tool;
        }

        private static Item Receita(int type)
        {
            return new RecipeScroll((int)AlchemyRecipes.Expl);
        }

        private static Item Cauldron(int type)
        {
            switch (type)
            {
                default:
                case 0: return new CauldronOfTransmutationDeed(CraftResource.Pinho);
                case 1: return new CauldronOfTransmutationDeed(CraftResource.Bronze);
                case 2: return new CauldronOfTransmutationDeed(CraftResource.Mogno);
                case 3: return new CauldronOfTransmutationDeed(CraftResource.Dourado);
                case 4: return new CauldronOfTransmutationDeed(CraftResource.Niobio);
                case 5: return new CauldronOfTransmutationDeed(CraftResource.Eucalipto);
                case 6: return new CauldronOfTransmutationDeed(CraftResource.Carmesim);
                case 7: return new CauldronOfTransmutationDeed(CraftResource.Lazurita);
            }
        }

        private static Item CraftsmanTalisman(int type)
        {
            return new MasterCraftsmanTalisman(type, 0x9E26, TalismanSkill.Alchemy);
        }
        #endregion

        public static readonly AlchemyRewardCalculator Instance = new AlchemyRewardCalculator();

        public override int ComputePoints(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int points = 0;

            if (quantity <= 15)
                points += 10;
            else if (quantity <= 30)
                points += 25;
            else if (quantity > 30)
                points += 50;

            if (itemCount == 3)
            {
                if (type == typeof(RefreshPotion) || type == typeof(HealPotion) || type == typeof(CurePotion))
                    points += 250;
                else
                    points += 300;
            }
            else if (itemCount == 4)
                points += 200;
            else if (itemCount == 5)
                points += 400;
            else if (itemCount == 6)
                points += 350;

            return (int)(points * 2.2);
        }

        private static readonly int[][] m_GoldTable = new int[][]
        {
            new int[] // singles
            {
                500, 600, 700
            },
            new int[] // no 2 piece
            {
            },
            new int[] // 3-part
            {
                4000, 6000, 8000
            },
            new int[] // 4-part
            {
                5000, 7500, 10000
            },
            new int[] // 5-part
            {
                7500, 11250, 15000
            },
            new int[] // 6-part (regular)
            {
                7500, 11250, 15000
            },
        };

        public override int ComputeGold(int quantity, bool exceptional, BulkMaterialType material, int itemCount, Type type)
        {
            int gold = 0;

            if (itemCount == 1 && BulkOrderSystem.NewSystemEnabled && BulkOrderSystem.ComputeGold(type, quantity, out gold))
            {
                return gold;
            }

            int[][] goldTable = m_GoldTable;

            int quanIndex = (quantity >= 50 ? 2 : quantity >= 30 ? 1 : 0);

            gold = goldTable[itemCount - 1][quanIndex];

            int min = (gold * 9) / 10;
            int max = (gold * 10) / 9;

            return Utility.RandomMinMax(min, max);
        }
    }
    #endregion


}
