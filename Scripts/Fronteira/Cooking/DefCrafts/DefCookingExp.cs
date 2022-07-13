using System;
using Server.Items;

namespace Server.Engines.Craft
{
    #region Recipes
    public enum CookRecipesExp
    {
        // magical
        RotWormStew = 500,
        GingerbreadCookie = 599,

        DarkChocolateNutcracker = 600,
        MilkChocolateNutcracker = 601,
        WhiteChocolateNutcracker = 602,

        ThreeTieredCake = 603,
        BlackrockStew = 604,
        Hamburger = 605,
        HotDog = 606,
        Sausage = 607,

        #region Custom Recipes
        #region Ingredients Recipes 5000 - 5005
        SackFlour = 5000,
        BagOfSugar = 5001,
        Dough = 5002,
        SweetDough = 5003,
        CocoaButter = 5004,
        CocoaLiquor = 5005,
        #endregion
        #region Oils Recipes 5101- 5105
        Batter = 5101,
        Butter = 5102,
        Cream = 5103,
        CookingOil = 5104,
        Vinegar = 5105,
        #endregion
        #region Sauces Recipes 5201 - 5208
        BarbecueSauce = 5201,
        CheeseSauce = 5202,
        EnchiladaSauce = 5203,
        Gravy = 5204,
        HotSauce = 5205,
        SoySauce = 5206,
        Teriyaki = 5207,
        TomatoSauce = 5208,
        #endregion
        #region Raw Meat Prep Recipes 5301 - 5400
        VenisonSteak = 5301,
        VenisonJerky = 5302,
        VenisonRoast = 5303,
        BeefPorterhouse = 5304,
        BeefPrimeRib = 5305,
        BeefRibeye = 5306,
        BeefRibs = 5307,
        BeefRoast = 5308,
        BeefSirloin = 5309,
        BeefJerky = 5310,
        BeefTBone = 5311,
        BeefTenderloin = 5312,
        GroundBeef = 5313,
        GoatSteak = 5314,
        GoatRoast = 5315,
        RawGroundPork = 5317,
        Bacon = 5318,
        BaconSlab = 5319,
        Ham = 5320,
        HamSlices = 5321,
        PigHead = 5322,
        PorkChop = 5323,
        PorkRoast = 5324,
        PorkSpareRibs = 5325,
        Trotters = 5326,
        SausageExp = 5327,
        MuttonSteak = 5328,
        MuttonRoast = 5329,
        LambLeg = 5330,
        GroundPork = 5331,
        RoastChicken = 5332,
        ChickenLeg = 5333,
        RoastTurkey = 5334,
        TurkeyLeg = 5335,
        TurkeyPlatter = 5336,
        SlicedTurkey = 5337,
        RoastDuck = 5338,
        DuckLeg = 5339,
        CookedBird = 5340,
        Ribs = 5341,
        CookedSteak = 5342,
        #endregion
        #region Preparations Recipes 5401 - 5500
        PastaNoodles = 5401,
        PeanutButter = 5402,
        FruitJam = 5403,
        Tortilla = 5404,
        WoodPulp = 5405,
        GreenTea = 5406,
        WasabiClumps = 5407,
        SushiRolls = 5408,
        SushiPlatter = 5409,
        TribalPaint = 5410,
        EggBomb = 5411,
        DriedOnions = 5412,
        DriedHerbs = 5413,
        BasketOfHerbsFarm = 5414,
        CakeMix = 5415,
        CookieMix = 5416,
        ChocolateMix = 5417,
        AsianVegMix = 5418,
        MixedVegetables = 5419,
        PizzaCrust = 5420,
        WaffleMix = 5421,
        BowlCornFlakes = 5422,
        BowlRiceKrisps = 5423,
        FruitBasket = 5424,
        Tofu = 5425,
        ParrotWafer = 5426,
        PieMix = 5427,
        UnbakedQuiche = 5428,
        UnbakedMeatPie = 5429,
        UncookedSausagePizza = 5430,
        UncookedCheesePizza = 5431,
        UnbakedFruitPie = 5432,
        UnbakedPeachCobbler = 5433,
        UnbakedApplePie = 5434,
        UnbakedPumpkinPie = 5435,
        #endregion
        #region Baking Recipes 5501 - 5600
        BreadLoaf = 5501,
        GarlicBread = 5502,
        BananaBread = 5503,
        PumpkinBread = 5504,
        CornBread = 5505,
        Cookies = 5506,
        AlmondCookies = 5507,
        ChocChipCookies = 5508,
        GingerSnaps = 5509,
        OatmealCookies = 5510,
        PeanutButterCookies = 5511,
        PumpkinCookies = 5512,
        Cake = 5513,
        BananaCake = 5514,
        CarrotCake = 5515,
        ChocolateCake = 5516,
        CoconutCake = 5517,
        LemonCake = 5518,
        Muffins = 5519,
        BlueberryMuffins = 5520,
        PumpkinMuffins = 5521,
        SausagePizza = 5522,
        CheesePizza = 5523,
        HamPineapplePizza = 5524,
        MushroomOnionPizza = 5525,
        SausOnionMushPizza = 5526,
        TacoPizza = 5527,
        VeggiePizza = 5528,
        Quiche = 5529,
        MeatPie = 5530,
        FruitPie = 5531,
        PeachCobbler = 5532,
        ApplePie = 5533,
        PumpkinPie = 5534,
        BlueberryPie = 5535,
        CherryPie = 5536,
        KeyLimePie = 5537,
        LemonMerenguePie = 5538,
        BlackberryCobbler = 5539,
        ShepherdsPie = 5540,
        TurkeyPie = 5541,
        ChickenPie = 5542,
        BeefPie = 5543,
        Brownies = 5544,
        ChocSunflowerSeeds = 5545,
        RiceKrispTreat = 5546,
        BowlOatmeal = 5547,
        Popcorn = 5548,
        Pancakes = 5549,
        Waffles = 5550,
        #endregion
        #region Boiling Recipes 5601 - 5700
        ChickenNoodleSoup = 5601,
        ArrozComTomate = 5602,
        BowlOfStew = 5603,
        SopaDeTomate = 5604,
        SopaDeOvosDeHarpia = 5605,
        PokeDeBeterrabas = 5606,
        PokeDeBrocolis = 5607,
        PokeDeCouve = 5608,
        BowlGreenBeans = 5609,
        BowlRice = 5610,
        BowlSpinach = 5611,
        BowlTurnips = 5612,
        BowlMashedPotatos = 5613,
        BowlCookedVeggies = 5614,
        WoodenBowlCabbage = 5615,
        WoodenBowlCarrot = 5616,
        WoodenBowlCorn = 5617,
        WoodenBowlLettuce = 5618,
        WoodenBowlPea = 5619,
        PewterBowlOfPotatos = 5620,
        CornOnCob = 5621,
        Hotdog = 5622,
        MisoSoup = 5623,
        WhiteMisoSoup = 5624,
        RedMisoSoup = 5625,
        AwaseMisoSoup = 5626
        #endregion
        #region Begin Barbecue Recipes 5701 - 5800
        #endregion
        #region Dinners Recipes 5801 - 5900
        #endregion
        #region Begin Chocolatiering Recipes 5901 - 5999
        #endregion
        #endregion
    }
    #endregion

    #region class info
    public class DefCookingExp : CraftSystem
    {
        public static void Initialize()
        {
            var a = DefCookingExp.CraftSystem;
        }

        public static Item GetReceitaIgredienteRandom()
        {
            var num = Utility.Random(5000, 5);
            return new RecipeScroll(num);
        }

        public static Item GetReceitaOleoRandom()
        {
            var num = Utility.Random(5101, 5);
            return new RecipeScroll(num);
        }

        public static Item GetReceitaMolhoRandom()
        {
            var num = Utility.Random(5201, 8);
            return new RecipeScroll(num);
        }

        public static Item GetReceitaCarneRandom()
        {
            var num = Utility.Random(5301, 42);
            return new RecipeScroll(num);
        }

        public static Item GetReceitaPreparationRandom()
        {
            var num = Utility.Random(5401, 45);
            return new RecipeScroll(num);
        }

        public static Item GetReceitaBakingRandom()
        {
            var num = Utility.Random(5501, 50);
            return new RecipeScroll(num);
        }

        public static Item GetReceitaBoilingRandom()
        {
            var num = Utility.Random(5601, 26);
            return new RecipeScroll(num);
        }

        public static Item GetReceitaRandom()
        {
            Array values = Enum.GetValues(typeof(CookRecipesExp));
            Random random = new Random();
            var randomBar = (int)values.GetValue(random.Next(values.Length));
            var receita = new RecipeScroll(randomBar);
            return receita;
        }

        public void SetNeedCauldron(int index, bool needCauldron)
        {
            CraftItem craftItem = CraftItems.GetAt(index);
            craftItem.NeedCauldron = needCauldron;
        }
        public override SkillName MainSkill
        {
            get
            {
                return SkillName.Cooking;
            }
        }

        public override string GumpTitleString
        {
            get
            {
                return "<CENTER>COZINHEIRO</CENTER>";
            }//
        }

        private static CraftSystem m_CraftSystem;

        public static CraftSystem CraftSystem
        {
            get
            {
                if (m_CraftSystem == null)
                    m_CraftSystem = new DefCookingExp();

                return m_CraftSystem;
            }
        }

        public override CraftECA ECA
        {
            get
            {
                return CraftECA.ChanceMinusSixtyToFourtyFive;
            }
        }

        public override double GetChanceAtMin(CraftItem item)
        {
            if (item.ItemType == typeof(GrapesOfWrath) ||
                item.ItemType == typeof(EnchantedApple))
            {
                return .5;
            }

            return 0.0; // 0%
        }

        private DefCookingExp()
            : base(1, 1, 1.25)// base( 1, 1, 1.5 )
        {
        }

        public override int CanCraft(Mobile from, ITool tool, Type itemType)
        {
            int num = 0;

            if (tool == null || tool.Deleted || tool.UsesRemaining <= 0)
                return 1044038; // You have worn out your tool!
            else if (!tool.CheckAccessible(from, ref num))
                return num; // The tool must be on your person to use.

            return 0;
        }

        public override void PlayCraftEffect(Mobile from)
        {
        }

        public override int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item)
        {
            if (toolBroken)
                from.SendLocalizedMessage(1044038); // You have worn out your tool

            if (failed)
            {
                if (lostMaterial)
                    return 1044043; // You failed to create the item, and some of your materials are lost.
                else
                    return 1044157; // You failed to create the item, but no materials were lost.
            }
            else
            {
                if (quality == 0)
                    return 502785; // You were barely able to make this item.  It's quality is below average.
                else if (makersMark && quality == 2)
                    return 1044156; // You create an exceptional quality item and affix your maker's mark.
                else if (quality == 2)
                    return 1044155; // You create an exceptional quality item.
                else
                    return 1044154; // You create the item.
            }
        }
        #endregion

        public static CookRecipesExp[] Basicas = new CookRecipesExp[]
        {
            CookRecipesExp.SackFlour, CookRecipesExp.BagOfSugar, CookRecipesExp.Dough,CookRecipesExp.Batter, CookRecipesExp.Butter,
            CookRecipesExp.Vinegar, CookRecipesExp.SweetDough, CookRecipesExp.CocoaButter, CookRecipesExp.CocoaLiquor
        };

        public override void InitCraftList()
        {
            int index = -1;

            #region Ingredients
            index = AddCraft(typeof(SackFlour), 1044495, 1024153, 0.0, 100.0, typeof(WheatSheaf), 1044489, 20, 1044490);
            AddRecipe(index, (int)CookRecipesExp.SackFlour);
            SetNeedMill(index, true);

            index = AddCraft(typeof(BagOfSugar), 1044495, "Bag of Sugar", 0.0, 100.0, typeof(Sugarcane), "Sugarcane", 20, "You don't have enough sugarcane.");
            AddRecipe(index, (int)CookRecipesExp.BagOfSugar);
            SetNeedMill(index, true);

            index = AddCraft(typeof(Dough), 1044495, 1024157, 10.0, 55, typeof(SackFlour), 1044468, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Dough);

            index = AddCraft(typeof(SweetDough), 1044495, 1041340, 10.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(JarHoney), 1044472, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.SweetDough);

            index = AddCraft(typeof(CocoaButter), 1044495, 1079998, 15.0, 100.0, typeof(CocoaPulp), 1080530, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CocoaButter);
            SetItemHue(index, 0x457);
            SetNeedOven(index, true);

            index = AddCraft(typeof(CocoaLiquor), 1044495, 1079999, 15.0, 100.0, typeof(CocoaPulp), 1080530, 1, 1044253);
            AddRes(index, typeof(EmptyPewterBowl), 1025629, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CocoaLiquor);
            SetItemHue(index, 0x46A);
            SetNeedOven(index, true);

            #endregion

            #region Oils
            index = AddCraft(typeof(Batter), "Oleos", "Batter", 20.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(Eggs), "Eggs", 1, 1044253);
            AddRes(index, typeof(JarHoney), 1044472, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Batter);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(Butter), "Oleos", "Butter", 20.0, 100.0, typeof(Cream), "Cream", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Butter);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(Cream), "Oleos", "Cream", 20.0, 100.0, typeof(BaseBeverage), "Milk", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Cream);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(CookingOil), "Oleos", "Cooking Oil", 25.0, 100.0, typeof(Peanut), "Peanut", 10, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CookingOil);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(Vinegar), "Oleos", "Vinegar", 25.0, 100.0, typeof(Apple), "apples", 5, 1044253);
            AddRes(index, typeof(BottleOfWine), "Wine", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Vinegar);
            SetNeedHeat(index, true);
            #endregion

            #region Sauces
            index = AddCraft(typeof(BarbecueSauce), "Molhos", "Barbecue Sauce", 25.0, 100.0, typeof(Tomato), "Tomato", 1, 1044253);
            AddRes(index, typeof(JarHoney), "Honey", 1, 1044253);
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BarbecueSauce);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(CheeseSauce), "Molhos", "Cheese Sauce", 25.0, 100.0, typeof(Butter), "Butter", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), "Milk", 1, 1044253);
            AddRes(index, typeof(CheeseWheel), "Cheese Wheel", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CheeseSauce);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(EnchiladaSauce), "Molhos", "Enchilada Sauce", 25.0, 100.0, typeof(Tomato), "Tomato", 1, 1044253);
            AddRes(index, typeof(ChiliPepper), "Chili Pepper", 1, 1044253);
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.EnchiladaSauce);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(Gravy), "Molhos", "Gravy", 25.0, 100.0, typeof(Dough), 1044469, 2, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Gravy);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(HotSauce), "Molhos", "Hot Sauce", 25.0, 100.0, typeof(Tomato), "Tomato", 2, 1044253);
            AddRes(index, typeof(Lettuce), "Alface", 50, "Voce precisa de Alface para isto");
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.HotSauce);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(SoySauce), "Molhos", "Soy Sauce", 25.0, 100.0, typeof(BagOfSoy), "Bag of Soy", 1, 1044253);
            AddRes(index, typeof(BagOfSugar), "Bag of Sugar", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.SoySauce);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(Teriyaki), "Molhos", "Teriyaki", 25.0, 100.0, typeof(SoySauce), "Soy Sauce", 1, 1044253);
            AddRes(index, typeof(BottleOfWine), "Bottle of Wine", 1, 1044253);
            AddRes(index, typeof(JarHoney), "Honey", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Teriyaki);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(TomatoSauce), "Molhos", "Molho de Tomate", 25.0, 100.0, typeof(Tomato), "Tomato", 3, 1044253);
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.TomatoSauce);
            SetNeedHeat(index, true);
            #endregion

            #region Raw Meat Prep

            #region Game Meats

            #region Deer
            index = AddCraft(typeof(VenisonSteak), "Carnes Cruas", "Venison Steak", 25.0, 100.0, typeof(RawVenisonSteak), "Raw Venison Steak", 1, "You need more Raw Venison Steak");
            AddRecipe(index, (int)CookRecipesExp.VenisonSteak);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(VenisonJerky), "Carnes Cruas", "Venison Jerky", 25.0, 100.0, typeof(RawVenisonSlice), "Raw Venison Slice", 1, "You need more Raw Venison Slice");
            AddRecipe(index, (int)CookRecipesExp.VenisonJerky);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(VenisonRoast), "Carnes Cruas", "Venison Roast", 25.0, 100.0, typeof(RawVenisonRoast), "Raw Venison Roast", 1, "You need more Raw Venison Roast");
            AddRecipe(index, (int)CookRecipesExp.VenisonRoast);
            SetNeedHeat(index, true);


            #endregion

            #endregion

            #region Lean Ground Meats

            #region Beef
            index = AddCraft(typeof(BeefPorterhouse), "Carnes Cruas", "Beef Porterhouse", 30.0, 100.0, typeof(RawBeefPorterhouse), "Raw Beef Porterhouse", 1, "You need more Raw Beef Porterhouse");
            AddRecipe(index, (int)CookRecipesExp.BeefPorterhouse);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(BeefPrimeRib), "Carnes Cruas", "Beef Prime Rib", 30.0, 100.0, typeof(RawBeefPrimeRib), "Raw Beef Prime Rib", 1, "You need more Raw Beef Prime Rib");
            AddRecipe(index, (int)CookRecipesExp.BeefPrimeRib);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(BeefRibeye), "Carnes Cruas", "Beef Ribeye", 30.0, 100.0, typeof(RawBeefRibeye), "Raw Beef Ribeye", 1, "You need more Raw Beef Ribeye");
            AddRecipe(index, (int)CookRecipesExp.BeefRibeye);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(BeefRibs), "Carnes Cruas", "Beef Ribs", 30.0, 100.0, typeof(RawBeefRibs), "Raw Beef Ribs", 1, "You need more Raw Beef Ribs");
            AddRecipe(index, (int)CookRecipesExp.BeefRibs);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(BeefRoast), "Carnes Cruas", "Beef Roast", 30.0, 100.0, typeof(RawBeefRoast), "Raw Beef Roast", 1, "You need more Raw Beef Roast");
            AddRecipe(index, (int)CookRecipesExp.BeefRoast);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(BeefSirloin), "Carnes Cruas", "Beef Sirloin", 30.0, 100.0, typeof(RawBeefSirloin), "Raw Beef Sirloin", 1, "You need more Raw Beef Sirloin");
            AddRecipe(index, (int)CookRecipesExp.BeefSirloin);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(BeefJerky), "Carnes Cruas", "Beef Jerky", 30.0, 100.0, typeof(RawBeefSlice), "Raw Beef Slice", 1, "You need more Raw Beef Slices");
            AddRecipe(index, (int)CookRecipesExp.BeefJerky);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(BeefTBone), "Carnes Cruas", "Beef T-Bone", 30.0, 100.0, typeof(RawBeefTBone), "Raw Beef T-Bone", 1, "You need more Raw Beef T-Bone");
            AddRecipe(index, (int)CookRecipesExp.BeefTBone);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(BeefTenderloin), "Carnes Cruas", "Beef Tenderloin", 30.0, 100.0, typeof(RawBeefTenderloin), "Raw Beef Tenderloin", 1, "You need more Raw Raw Beef Tenderloin");
            AddRecipe(index, (int)CookRecipesExp.BeefTenderloin);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(GroundBeef), 1044496, "Ground Beef", 30.0, 100.0, typeof(BeefHock), "Beef Hock", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.GroundBeef);
            #endregion

            #region Goat
            index = AddCraft(typeof(GoatSteak), "Carnes Cruas", "Goat Steak", 35.0, 100.0, typeof(RawGoatSteak), "Raw Goat Steak", 1, "You need more Raw Goat Steak");
            AddRecipe(index, (int)CookRecipesExp.GoatSteak);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(GoatRoast), "Carnes Cruas", "Goat Roast", 35.0, 100.0, typeof(RawGoatRoast), "Raw Goat Roast", 1, "You need more Raw Goat Roast");
            AddRecipe(index, (int)CookRecipesExp.GoatRoast);
            SetNeedHeat(index, true);
            #endregion

            #region Pork
            index = AddCraft(typeof(RawGroundPork), "Carnes Cruas", "Raw Ground Pork", 40.0, 100.0, typeof(PorkHock), "Pork Hock", 1, "You need more Pork Hock");
            AddRecipe(index, (int)CookRecipesExp.RawGroundPork);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(Bacon), "Carnes Cruas", "Bacon", 40.0, 100.0, typeof(RawBacon), "Raw Bacon", 1, "You need more Raw Bacon");
            AddRecipe(index, (int)CookRecipesExp.Bacon);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(BaconSlab), "Carnes Cruas", "Bacon Slab", 40.0, 100.0, typeof(RawBaconSlab), "Raw Bacon Slab", 1, "You need more Raw Bacon Slab");
            AddRecipe(index, (int)CookRecipesExp.BaconSlab);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(Ham), "Carnes Cruas", "Ham", 40.0, 100.0, typeof(RawHam), "Raw Ham", 1, "You need more Raw Ham");
            AddRecipe(index, (int)CookRecipesExp.Ham);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(HamSlices), "Carnes Cruas", "Ham Slices", 40.0, 100.0, typeof(RawHamSlices), "Raw Ham Slices", 1, "You need more Raw Ham Slices");
            AddRecipe(index, (int)CookRecipesExp.HamSlices);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(PigHead), "Carnes Cruas", "Pig Head", 40.0, 100.0, typeof(RawPigHead), "Raw Pig Head", 1, "You need more Raw Pig Head");
            AddRecipe(index, (int)CookRecipesExp.PigHead);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(PorkChop), "Carnes Cruas", "Pork Chop", 40.0, 100.0, typeof(RawPorkChop), "Raw Pork Chop", 1, "You need more Raw Pork Chop");
            AddRecipe(index, (int)CookRecipesExp.PorkChop);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(PorkRoast), "Carnes Cruas", "Pork Roast", 40.0, 100.0, typeof(RawPorkRoast), "Raw Pork Roast", 1, "You need more Raw Pork Roast");
            AddRecipe(index, (int)CookRecipesExp.PorkRoast);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(PorkSpareRibs), "Carnes Cruas", "Pork Spare Ribs", 40.0, 100.0, typeof(RawSpareRibs), "Raw Spare Ribs", 1, "You need more Raw Spare Ribs");
            AddRecipe(index, (int)CookRecipesExp.PorkSpareRibs);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(Trotters), "Carnes Cruas", "Trotters", 40.0, 100.0, typeof(RawTrotters), "Raw Trotters", 1, "You need more Raw Trotters");
            AddRecipe(index, (int)CookRecipesExp.Trotters);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(Sausage), "Carnes Cruas", "Salsicha", 40.0, 100.0, typeof(RawRibsExp), "Corte de Costela Crua", 10, "Voce precisa de mais costelas suinas");
            AddRecipe(index, (int)CookRecipesExp.Sausage);
            SetNeedHeat(index, true);
            #endregion

            #region Sheep
            index = AddCraft(typeof(MuttonSteak), "Carnes Cruas", "Mutton Steak", 40.0, 100.0, typeof(RawMuttonSteak), "Raw Mutton Steak", 1, "You need more Raw Mutton Steak");
            AddRecipe(index, (int)CookRecipesExp.MuttonSteak);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(MuttonRoast), "Carnes Cruas", "Mutton Roast", 40.0, 100.0, typeof(RawMuttonRoast), "Raw Mutton Roast", 1, "You need more Raw Mutton Roast");
            AddRecipe(index, (int)CookRecipesExp.MuttonRoast);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(LambLeg), "Carnes Cruas", 1025642, 40.0, 100.0, typeof(RawLambLeg), 1044478, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.LambLeg);
            SetNeedHeat(index, true);
            SetUseAllRes(index, true);
            #endregion
            #endregion

            #region Poultry

            #region Chicken
            index = AddCraft(typeof(RoastChicken), "Carnes Cruas", 1153506, 40.0, 100.0, typeof(RawChickenExp), "Raw Chicken", 1, "You need more Raw Chicken");
            AddRecipe(index, (int)CookRecipesExp.RoastChicken);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(ChickenLeg), "Carnes Cruas", 1025640, 40.0, 100.0, typeof(RawChickenLeg), 1044473, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.ChickenLeg);
            SetNeedHeat(index, true);
            SetUseAllRes(index, true);
            #endregion

            #region Turkey
            index = AddCraft(typeof(RoastTurkey), "Carnes Cruas", 1153507, 45.0, 100.0, typeof(RawTurkey), "Raw Turkey", 1, "You need more Raw Turkey");
            AddRecipe(index, (int)CookRecipesExp.RoastTurkey);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(TurkeyLeg), "Carnes Cruas", 1153508, 45.0, 100.0, typeof(RawTurkeyLeg), "Raw Turkey Leg", 1, "You need more Raw Turkey Leg");
            AddRecipe(index, (int)CookRecipesExp.TurkeyLeg);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(TurkeyPlatter), "Carnes Cruas", 1153517, 45.0, 100.0, typeof(RawTurkey), "Raw Chicken", 1, "You need more Raw Turkey");
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            AddRecipe(index, (int)CookRecipesExp.TurkeyPlatter);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(SlicedTurkey), 1044496, "Sliced Turkey", 45.0, 100.0, typeof(TurkeyHock), "Turkey Hock", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.SlicedTurkey);

            #endregion

            #region Duck
            index = AddCraft(typeof(RoastDuck), "Carnes Cruas", 1153505, 45.0, 100.0, typeof(RawChickenExp), "Raw Chicken", 1, "You need more Raw Chicken");
            AddRecipe(index, (int)CookRecipesExp.RoastDuck);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(DuckLeg), "Carnes Cruas", "Duck Leg", 45.0, 100.0, typeof(RawDuckLeg), "Raw Duck Leg", 1, "You need more Raw Duck Legs");
            AddRecipe(index, (int)CookRecipesExp.DuckLeg);
            SetNeedHeat(index, true);
            #endregion

            #region Bird
            index = AddCraft(typeof(CookedBird), "Carnes Cruas", 1022487, 45.0, 100.0, typeof(RawBird), 1044470, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CookedBird);
            SetNeedHeat(index, true);
            SetUseAllRes(index, true);
            #endregion

            #endregion

            #region Misc Meats
            index = AddCraft(typeof(Ribs), "Carnes Cruas", 1022546, 50.0, 100.0, typeof(RawRibs), 1044485, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Ribs);
            SetNeedHeat(index, true);
            SetUseAllRes(index, true);

            index = AddCraft(typeof(CookedSteak), "Carnes Cruas", "Cooked Steak", 50.0, 100.0, typeof(RawSteakExp), "Raw Steak", 1, "You need more Raw Steak");
            AddRecipe(index, (int)CookRecipesExp.CookedSteak);
            SetNeedHeat(index, true);
            #endregion

            #endregion

            #region Preparations

            index = AddCraft(typeof(PastaNoodles), 1044496, "Pasta Noodles", 50.0, 100.0, typeof(SackFlour), "Sack of Flour", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PastaNoodles);
            AddRes(index, typeof(Eggs), "eggs", 5, 1044253);

            index = AddCraft(typeof(PeanutButter), 1044496, "Peanut Butter", 55.0, 100.0, typeof(Peanut), "Peanuts", 30, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PeanutButter);

            index = AddCraft(typeof(FruitJam), 1044496, "Fruit Jam", 55.0, 100.0, typeof(FruitBasket), "Fruit Basket", 1, 1044253);
            AddRes(index, typeof(BagOfSugar), "Bag of Sugar", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.FruitJam);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Tortilla), 1044496, "Tortilla", 55.0, 100.0, typeof(BagOfCornmeal), "Bag of Cornmeal", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Tortilla);
            SetNeedOven(index, true);

            index = AddCraft(typeof(WoodPulp), 1044496, 1113136, 60.0, 100.0, typeof(BarkFragment), 1032687, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.WoodPulp);

            index = AddCraft(typeof(DeliciouslyTastyTreat), 1044496, "Biscoito de Pet", 0, 100.0, typeof(TomatoSauce), "Molho de Tomate", 5, "Voce precisa de Molho de Tomate");
            AddRes(index, typeof(DriedOnions), "Cebolas Secas", 10, "Voce precisa de cebolas secas");
            AddRes(index, typeof(DriedHerbs), "Ervas Secas", 10, "Voce precisa de ervas secas");
            AddRes(index, typeof(Carrot), "Cenoura", 10, "Voce precisa cenouras");
            AddRes(index, typeof(Sausage), "Salsicha", 5, "Voce precisa de salsichas suinas");
            SetNeedHeat(index, true);

            index = AddCraft(typeof(GreenTea), 1044496, 1030315, 80.0, 120.0, typeof(GreenTeaBasket), 1030316, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.GreenTea);
            SetNeedOven(index, true);

            index = AddCraft(typeof(WasabiClumps), 1044496, 1029451, 70.0, 120.0, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRes(index, typeof(WoodenBowlOfPeas), 1025633, 3, 1044253);
            AddRecipe(index, (int)CookRecipesExp.WasabiClumps);

            index = AddCraft(typeof(SushiRolls), 1044496, 1030303, 90.0, 120.0, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRes(index, typeof(RawFishSteak), 1044476, 10, 1044253);
            AddRecipe(index, (int)CookRecipesExp.SushiRolls);

            index = AddCraft(typeof(SushiPlatter), 1044496, 1030305, 90.0, 120.0, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRes(index, typeof(RawFishSteak), 1044476, 10, 1044253);
            AddRecipe(index, (int)CookRecipesExp.SushiPlatter);


            index = AddCraft(typeof(TribalPaint), 1044496, 1040000, 55.0, 105.0, typeof(SackFlour), 1044468, 1, 1044253);
            AddRes(index, typeof(TribalBerry), 1046460, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.TribalPaint);

            index = AddCraft(typeof(EggBomb), 1044496, 1030249, 90.0, 120.0, typeof(Eggs), 1044477, 1, 1044253);
            AddRes(index, typeof(SackFlour), 1044468, 3, 1044253);
            AddRecipe(index, (int)CookRecipesExp.EggBomb);

            index = AddCraft(typeof(PlantPigment), 1044496, 1112132, 33.0, 83.0, typeof(PlantClippings), 1112131, 1, 1044253);
            AddRes(index, typeof(Bottle), 1023854, 1, 1044253);

            index = AddCraft(typeof(NaturalDye), 1044496, 1112136, 75.0, 100.0, typeof(PlantPigment), 1112132, 1, 1044253);
            AddRes(index, typeof(ColorFixative), 1112135, 1, 1044253);

            index = AddCraft(typeof(ColorFixative), 1044496, 1112135, 75.0, 100.0, typeof(SilverSerpentVenom), 1112173, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1044476, 1, 1044253);//TODO correct Consumption of BaseBeverage...


            index = AddCraft(typeof(WoodPulp), 1044496, 1113136, 60.0, 100.0, typeof(BarkFragment), 1032687, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);


            index = AddCraft(typeof(Charcoal), 1044496, 1116303, 0.0, 50.0, typeof(Log), 1044041, 1, 1044351);
            SetUseAllRes(index, true);
            SetNeedHeat(index, true);


            index = AddCraft(typeof(DriedOnions), 1044496, "Cebolas Secas", 60.0, 100.0, typeof(Onion), "Cebolas", 10, 1044253);
            AddRecipe(index, (int)CookRecipesExp.DriedOnions);

            index = AddCraft(typeof(DriedHerbs), 1044496, "Ervas Secas", 60.0, 100.0, typeof(Garlic), "Alho", 10, 1044253);
            AddRes(index, typeof(Lettuce), "Alface", 10, "Voce precisa de alfaces");
            AddRes(index, typeof(Carrot), "Cenoura", 10, "Voce precisa de cenouras");
            AddRecipe(index, (int)CookRecipesExp.DriedHerbs);

            index = AddCraft(typeof(BasketOfHerbsFarm), 1044496, "Basket of Herbs", 60.0, 100.0, typeof(DriedHerbs), "Dried Herbs", 1, 1044253);
            AddRes(index, typeof(DriedOnions), "Dried Onions", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BasketOfHerbsFarm);

            index = AddCraft(typeof(CakeMix), 1044496, 1041002, 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(CookingOil), "Cooking Oil", 1, 1044253);
            AddRes(index, typeof(BagOfSugar), "Bag of Sugar", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CakeMix);

            index = AddCraft(typeof(CookieMix), 1044496, 1024159, 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(Butter), "Butter", 1, 1044253);
            AddRes(index, typeof(JarHoney), "Honey", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CookieMix);

            index = AddCraft(typeof(ChocolateMix), 1044496, "Chocolate Mix", 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(BagOfCocoa), "Bag of Cocoa", 1, 1044253);
            AddRes(index, typeof(BagOfSugar), "Bag of Sugar", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.ChocolateMix);

            index = AddCraft(typeof(AsianVegMix), 1044496, "Asian Vegetable Mix", 60.0, 100.0, typeof(Cabbage), "Cabbage", 1, 1044253);
            AddRes(index, typeof(Onion), "Onion", 1, 1044253);
            AddRes(index, typeof(RedMushroom), "Red Mushroom", 1, "You need a Red Mushroom");
            AddRes(index, typeof(Carrot), "Carrot", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.AsianVegMix);

            index = AddCraft(typeof(MixedVegetables), 1044496, "Mixed Vegetables", 60.0, 100.0, typeof(Potato), "Potato", 2, 1044253);
            AddRes(index, typeof(Carrot), "Carrot", 1, 1044253);
            AddRes(index, typeof(Celery), "Celery", 1, 1044253);
            AddRes(index, typeof(Onion), "Onion", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.MixedVegetables);

            index = AddCraft(typeof(PizzaCrust), 1044496, "Pizza Crust", 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PizzaCrust);

            index = AddCraft(typeof(WaffleMix), 1044496, "Waffle Mix", 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(Eggs), "Eggs", 2, 1044253);
            AddRes(index, typeof(CookingOil), "Cooking Oil", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.WaffleMix);

            index = AddCraft(typeof(BowlCornFlakes), 1044497, "Bowl of Corn Flakes", 60.0, 100.0, typeof(BagOfCornmeal), "Bag of Cornmeal", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BowlCornFlakes);

            index = AddCraft(typeof(BowlRiceKrisps), 1044497, "Bowl of Rice Krisps", 60.0, 100.0, typeof(BagOfRicemeal), "Bag of Ricemeal", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BowlRiceKrisps);

            index = AddCraft(typeof(FruitBasket), 1044497, "Fruit Basket", 60.0, 100.0, typeof(Apple), "Apple", 5, 1044253);
            AddRes(index, typeof(Peach), "Peach", 5, 1044253);
            AddRes(index, typeof(Pear), "Pear", 5, 1044253);
            AddRes(index, typeof(Cherry), "Cherries", 5, 1044253);
            AddRecipe(index, (int)CookRecipesExp.FruitBasket);

            index = AddCraft(typeof(Tofu), 1044497, "Tofu", 60.0, 100.0, typeof(BagOfSoy), "Bag of Soy", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Tofu);

            index = AddCraft(typeof(PieMix), 1044496, "Pie Mix", 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(Butter), "Butter", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PieMix);

            index = AddCraft(typeof(UnbakedQuiche), 1044496, 1041339, 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(Eggs), 1044477, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.UnbakedQuiche);

            // TODO: This must also support chicken and lamb legs
            index = AddCraft(typeof(UnbakedMeatPie), 1044496, 1041338, 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(RawRibs), 1044482, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.UnbakedMeatPie);

            index = AddCraft(typeof(UncookedSausagePizza), 1044496, 1041337, 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(Sausage), 1044483, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.UncookedSausagePizza);

            index = AddCraft(typeof(UncookedCheesePizza), 1044496, 1041341, 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(CheeseWheel), 1044486, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.UncookedCheesePizza);

            index = AddCraft(typeof(UnbakedFruitPie), 1044496, 1041334, 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(Pear), 1044481, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.UnbakedFruitPie);

            index = AddCraft(typeof(UnbakedPeachCobbler), 1044496, 1041335, 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(Peach), 1044480, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.UnbakedPeachCobbler);

            index = AddCraft(typeof(UnbakedApplePie), 1044496, 1041336, 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(Apple), 1044479, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.UnbakedApplePie);

            index = AddCraft(typeof(UnbakedPumpkinPie), 1044496, 1041342, 60.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(Pumpkin), 1044484, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.UnbakedPumpkinPie);
            #endregion

            #region Baking
            index = AddCraft(typeof(BreadLoaf), 1044497, 1024156, 0.0, 60.0, typeof(Dough), 1044469, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BreadLoaf);
            SetNeedOven(index, true);

            index = AddCraft(typeof(GarlicBread), 1044497, "Garlic Bread", 70.0, 100.0, typeof(BreadLoaf), 1024156, 1, 1044253);
            AddRes(index, typeof(Butter), "Butter", 1, 1044253);
            AddRes(index, typeof(Garlic), "Garlic", 2, 1044253);
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.GarlicBread);
            SetNeedOven(index, true);

            index = AddCraft(typeof(BananaBread), 1044497, "Banana Bread", 70.0, 100.0, typeof(SweetDough), "Sweet Dough", 1, 1044253);
            AddRes(index, typeof(Banana), "Banana", 6, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BananaBread);
            SetNeedOven(index, true);

            index = AddCraft(typeof(PumpkinBread), 1044497, "Pumpkin Bread", 70.0, 100.0, typeof(SweetDough), "Sweet Dough", 1, 1044253);
            AddRes(index, typeof(Pumpkin), "Pumpkin", 3, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PumpkinBread);
            SetNeedOven(index, true);

            index = AddCraft(typeof(CornBread), 1044497, "Corn Bread", 70.0, 100.0, typeof(BagOfCornmeal), "Bag of Cornmeal", 1, 1044253);
            AddRes(index, typeof(Batter), "Batter", 1, 1044253);
            AddRes(index, typeof(BagOfSugar), "Bag of Sugar", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CornBread);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Cookies), 1044497, 1025643, 70.0, 100.0, typeof(CookieMix), 1044474, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Cookies);
            SetNeedOven(index, true);

            index = AddCraft(typeof(AlmondCookies), 1044497, "Almond Cookies", 70.0, 100.0, typeof(CookieMix), "Cookie Mix", 1, 1044253);
            AddRes(index, typeof(Almond), "Almond", 12, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            AddRecipe(index, (int)CookRecipesExp.AlmondCookies);
            SetNeedOven(index, true);

            index = AddCraft(typeof(ChocChipCookies), 1044497, "Chocolate Chip Cookies", 70.0, 100.0, typeof(CookieMix), "Cookie Mix", 1, 1044253);
            AddRes(index, typeof(BagOfCocoa), "Bag of Cocoa", 1, "YUou need a bag of cocoa");
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            AddRecipe(index, (int)CookRecipesExp.ChocChipCookies);
            SetNeedOven(index, true);

            index = AddCraft(typeof(GingerSnaps), 1044497, "Ginger Snaps", 70.0, 100.0, typeof(CookieMix), "Cookie Mix", 1, 1044253);
            AddRes(index, typeof(TanGinger), "Tan Ginger", 12, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            AddRecipe(index, (int)CookRecipesExp.GingerSnaps);
            SetNeedOven(index, true);

            index = AddCraft(typeof(OatmealCookies), 1044497, "Oatmeal Cookies", 70.0, 100.0, typeof(CookieMix), "Cookie Mix", 1, 1044253);
            AddRes(index, typeof(BagOfOats), "Bag of Oats", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            AddRecipe(index, (int)CookRecipesExp.OatmealCookies);
            SetNeedOven(index, true);

            index = AddCraft(typeof(PeanutButterCookies), 1044497, "Peanut Butter Cookies", 70.0, 100.0, typeof(CookieMix), "Cookie Mix", 1, 1044253);
            AddRes(index, typeof(PeanutButter), "Peanut Butter", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            AddRecipe(index, (int)CookRecipesExp.PeanutButterCookies);
            SetNeedOven(index, true);

            index = AddCraft(typeof(PumpkinCookies), 1044497, "Pumpkin Cookies", 70.0, 100.0, typeof(CookieMix), "Cookie Mix", 1, 1044253);
            AddRes(index, typeof(Pumpkin), "Pumpkin", 6, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            AddRecipe(index, (int)CookRecipesExp.PumpkinCookies);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Cake), 1044497, 1022537, 70.0, 100.0, typeof(CakeMix), 1044471, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Cake);
            SetNeedOven(index, true);

            index = AddCraft(typeof(BananaCake), 1044497, "Banana Cake", 70.0, 100.0, typeof(CakeMix), 1044471, 1, 1044253);
            AddRes(index, typeof(Banana), "Banana", 4, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BananaCake);
            SetNeedOven(index, true);

            index = AddCraft(typeof(CarrotCake), 1044497, "Carrot Cake", 70.0, 100.0, typeof(CakeMix), 1044471, 1, 1044253);
            AddRes(index, typeof(Carrot), "Carrot", 6, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CarrotCake);
            SetNeedOven(index, true);

            index = AddCraft(typeof(ChocolateCake), 1044497, "Chocolate Cake", 70.0, 100.0, typeof(CakeMix), 1044471, 1, 1044253);
            AddRes(index, typeof(BagOfCocoa), "Bag of Cocoa", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.ChocolateCake);
            SetNeedOven(index, true);

            index = AddCraft(typeof(CoconutCake), 1044497, "Coconut Cake", 70.0, 100.0, typeof(CakeMix), 1044471, 1, 1044253);
            AddRes(index, typeof(Coconut), "Coconut", 2, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CoconutCake);
            SetNeedOven(index, true);

            index = AddCraft(typeof(LemonCake), 1044497, "Lemon Cake", 70.0, 100.0, typeof(CakeMix), 1044471, 1, 1044253);
            AddRes(index, typeof(Lemon), "Lemon", 4, 1044253);
            AddRecipe(index, (int)CookRecipesExp.LemonCake);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Muffins), 1044497, 1022539, 70.0, 100.0, typeof(SweetDough), 1044475, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Muffins);
            SetNeedOven(index, true);

            index = AddCraft(typeof(BlueberryMuffins), 1044497, "Blueberry Muffins", 70.0, 100.0, typeof(SweetDough), "Sweet Dough", 1, 1044253);
            AddRes(index, typeof(Blueberry), "Blueberry", 6, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BlueberryMuffins);
            SetNeedOven(index, true);

            index = AddCraft(typeof(PumpkinMuffins), 1044497, "Pumpkin Muffins", 70.0, 100.0, typeof(SweetDough), "Sweet Dough", 1, 1044253);
            AddRes(index, typeof(Pumpkin), "Pumpkin", 2, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PumpkinMuffins);
            SetNeedOven(index, true);

            index = AddCraft(typeof(SausagePizza), 1044497, 1044517, 70.0, 100.0, typeof(UncookedSausagePizza), 1044520, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.SausagePizza);
            SetNeedOven(index, true);

            index = AddCraft(typeof(CheesePizza), 1044497, 1044516, 70.0, 100.0, typeof(UncookedCheesePizza), 1044521, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CheesePizza);
            SetNeedOven(index, true);

            index = AddCraft(typeof(HamPineapplePizza), 1044497, "Ham and Pineapple Pizza", 70.0, 100.0, typeof(UncookedPizza), "Uncooked Pizza", 1, 1044253);
            AddRes(index, typeof(RawHamSlices), "Raw Ham Slices", 1, 1044253);
            AddRes(index, typeof(Pineapple), "Pineapple", 2, 1044253);
            AddRecipe(index, (int)CookRecipesExp.HamPineapplePizza);
            SetNeedOven(index, true);

            index = AddCraft(typeof(MushroomOnionPizza), 1044497, "Mushroom and Onion Pizza", 70.0, 100.0, typeof(UncookedPizza), "Uncooked Pizza", 1, 1044253);
            AddRes(index, typeof(TanMushroom), "Tan Mushrooms", 3, 1044253);
            AddRes(index, typeof(Onion), "Onion", 3, 1044253);
            AddRecipe(index, (int)CookRecipesExp.MushroomOnionPizza);
            SetNeedOven(index, true);

            index = AddCraft(typeof(SausOnionMushPizza), 1044497, "Sausage Onion and Mushroom Pizza", 70.0, 100.0, typeof(UncookedPizza), "Uncooked Pizza", 1, 1044253);
            AddRes(index, typeof(Sausage), "Sausage", 2, 1044253);
            AddRes(index, typeof(Onion), "Onion", 2, 1044253);
            AddRes(index, typeof(RedMushroom), "Red Mushrooms", 2, 1044253);
            AddRecipe(index, (int)CookRecipesExp.SausOnionMushPizza);
            SetNeedOven(index, true);

            index = AddCraft(typeof(TacoPizza), 1044497, "Taco Pizza", 70.0, 100.0, typeof(UncookedPizza), "Uncooked Pizza", 1, 1044253);
            AddRes(index, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(CheeseWheel), "CheeseWheel", 1, 1044253);
            AddRes(index, typeof(EnchiladaSauce), "Enchilada Sauce", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.TacoPizza);
            SetNeedOven(index, true);

            index = AddCraft(typeof(VeggiePizza), 1044497, "Vegetable Pizza", 70.0, 100.0, typeof(UncookedPizza), "Uncooked Pizza", 1, 1044253);
            AddRes(index, typeof(MixedVegetables), "Mixed Vegetables", 1, 1044523);
            AddRecipe(index, (int)CookRecipesExp.VeggiePizza);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Quiche), 1044497, 1041345, 70.0, 100.0, typeof(UnbakedQuiche), 1044518, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Quiche);
            SetNeedOven(index, true);

            index = AddCraft(typeof(MeatPie), 1044497, 1041347, 70.0, 100.0, typeof(UnbakedMeatPie), 1044519, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.MeatPie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(FruitPie), 1044497, 1041346, 70.0, 100.0, typeof(UnbakedFruitPie), 1044522, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.FruitPie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(PeachCobbler), 1044497, 1041344, 70.0, 100.0, typeof(UnbakedPeachCobbler), 1044523, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PeachCobbler);
            SetNeedOven(index, true);

            index = AddCraft(typeof(ApplePie), 1044497, 1041343, 70.0, 100.0, typeof(UnbakedApplePie), 1044524, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.ApplePie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(PumpkinPie), 1044497, 1041348, 70.0, 100.0, typeof(UnbakedPumpkinPie), 1046461, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PumpkinPie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(BlueberryPie), 1044497, "Blueberry Pie", 70.0, 100.0, typeof(PieMix), "Pie Mix", 1, 1044253);
            AddRes(index, typeof(Blueberry), "Blueberry", 8, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BlueberryPie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(CherryPie), 1044497, "Cherry Pie", 70.0, 100.0, typeof(PieMix), "Pie Mix", 1, 1044253);
            AddRes(index, typeof(Cherry), "Cherry", 8, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CherryPie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(KeyLimePie), 1044497, "Key Lime Pie", 70.0, 100.0, typeof(PieMix), "Pie Mix", 1, 1044253);
            AddRes(index, typeof(Lime), "Lime", 12, 1044253);
            AddRecipe(index, (int)CookRecipesExp.KeyLimePie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(LemonMerenguePie), 1044497, "Lemon Merengue Pie", 70.0, 100.0, typeof(PieMix), "Pie Mix", 1, 1044253);
            AddRes(index, typeof(Lemon), "Lemon", 12, 1044253);
            AddRes(index, typeof(Cream), "Cream", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.LemonMerenguePie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(BlackberryCobbler), 1044497, "Blackberry Cobbler", 70.0, 100.0, typeof(PieMix), "Pie Mix", 1, 1044253);
            AddRes(index, typeof(Blackberry), "Blackberry", 10, 1044253);
            AddRes(index, typeof(JarHoney), "Honey", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BlackberryCobbler);
            SetNeedOven(index, true);

            index = AddCraft(typeof(ShepherdsPie), 1044497, "Shepherds Pie", 70.0, 100.0, typeof(PieMix), "Pie Mix", 1, 1044253);
            AddRes(index, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(BowlMashedPotatos), "Bowl of Mashed Potatos", 1, 1044253);
            AddRes(index, typeof(Corn), "Corn", 2, 1044253);
            AddRecipe(index, (int)CookRecipesExp.ShepherdsPie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(TurkeyPie), 1044497, "Turkey Pie", 70.0, 100.0, typeof(PieMix), "Pie Mix", 1, 1044253);
            AddRes(index, typeof(SlicedTurkey), "Sliced Turkey", 2, 1044253);
            AddRes(index, typeof(MixedVegetables), "Mixed Vegetables", 1, 1044253);
            AddRes(index, typeof(Gravy), "Gravy", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.TurkeyPie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(ChickenPie), 1044497, "Chicken Pie", 70.0, 100.0, typeof(RawChickenExp), "Raw Chicken", 1, 1044253);
            AddRes(index, typeof(PieMix), "Pie Mix", 1, 1044253);
            AddRes(index, typeof(MixedVegetables), "Mixed Vegetables", 1, 1044253);
            AddRes(index, typeof(Gravy), "Gravy", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.ChickenPie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(BeefPie), 1044497, "Beef Pie", 70.0, 100.0, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(PieMix), "Pie Mix", 1, 1044253);
            AddRes(index, typeof(MixedVegetables), "Mixed Vegetables", 1, 1044253);
            AddRes(index, typeof(Gravy), "Gravy", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BeefPie);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Brownies), 1044497, "Brownies", 70.0, 100.0, typeof(ChocolateMix), "Chocolate Mix", 1, 1044253);
            AddRes(index, typeof(Eggs), "Eggs", 2, 1044253);
            AddRes(index, typeof(CookingOil), "Cooking Oil", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Brownies);
            SetNeedOven(index, true);

            index = AddCraft(typeof(ChocSunflowerSeeds), 1044497, "Chocolate Sunflower Seeds", 70.0, 100.0, typeof(EdibleSun), "Sunflower Seeds", 1, 1044253);
            AddRes(index, typeof(BagOfCocoa), "Bag of Cocoa", 1, "you need a bag oc cocoa");
            AddRecipe(index, (int)CookRecipesExp.ChocSunflowerSeeds);
            SetNeedOven(index, true);

            index = AddCraft(typeof(RiceKrispTreat), 1044497, "RiceKrispTreat", 70.0, 100.0, typeof(BowlRiceKrisps), "Bowl Of Rice Krips", 1, 1044253);
            AddRes(index, typeof(Butter), "Butter", 1, 1044253);
            AddRes(index, typeof(BagOfSugar), "Bag of Sugar", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.RiceKrispTreat);
            SetNeedOven(index, true);

            index = AddCraft(typeof(BowlOatmeal), 1044497, "Bowl of Oatmeal", 70.0, 100.0, typeof(BagOfOats), "Bag of Oats", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRes(index, typeof(JarHoney), "Honey", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BowlOatmeal);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Popcorn), 1044497, "Popcorn", 70.0, 100.0, typeof(Corn), "Corn", 2, 1044253);
            AddRes(index, typeof(CookingOil), "Cooking Oil", 1, 1044253);
            AddRes(index, typeof(Butter), "Butter", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Popcorn);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Pancakes), 1044497, "Pancakes", 70.0, 100.0, typeof(Batter), "Batter", 1, 1044253);
            AddRes(index, typeof(JarHoney), "Honey", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Pancakes);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Waffles), 1044497, "Waffles", 70.0, 100.0, typeof(WaffleMix), "Waffle Mix", 1, 1044253);
            AddRes(index, typeof(JarHoney), "Honey", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Waffles);
            SetNeedOven(index, true);
            #endregion

            #region Boiling
            index = AddCraft(typeof(ChickenNoodleSoup), "Cozimento", "Chicken Noodle Soup", 80.0, 100.0, typeof(RoastChicken), 1153506, 1, 1044253);
            AddRes(index, typeof(PastaNoodles), "Pasta Noodles", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.ChickenNoodleSoup);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(TomatoRice), "Cozimento", "Tomato and Rice", 80.0, 100.0, typeof(Tomato), "Tomato", 3, 1044253);
            AddRes(index, typeof(BowlRice), "Bowl of Rice", 1, 1044253);
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.ArrozComTomate);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(BowlOfStew), "Cozimento", "Beef Stew", 80.0, 100.0, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(Gravy), "Gravy", 1, 1044253);
            AddRes(index, typeof(BowlCookedVeggies), "Cooked Bowl of Vegetables", 1, 1044253);
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BowlOfStew);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(TomatoSoup), "Cozimento", "Tomato Soup", 80.0, 100.0, typeof(Tomato), "Tomato", 5, 1044253);
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.SopaDeTomate);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(HarpyEggSoup), "Cozimento", "Harpy Egg Soup", 80.0, 100.0, typeof(HarpyEggs), "Harpy Eggs", 5, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.SopaDeOvosDeHarpia);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(BowlBeets), "Cozimento", "Bowl of Beets", 80.0, 100.0, typeof(Beet), "Beet", 4, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PokeDeBeterrabas);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(BowlBroccoli), "Cozimento", "Bowl of Broccoli", 80.0, 100.0, typeof(Broccoli), "Broccoli", 4, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PokeDeBrocolis);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(BowlCauliflower), "Cozimento", "Bowl of Cauliflower", 80.0, 100.0, typeof(Cauliflower), "Cauliflower", 4, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PokeDeCouve);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(BowlGreenBeans), "Cozimento", "Bowl of Green Beans", 80.0, 100.0, typeof(GreenBean), "Green Beans", 20, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRes(index, typeof(Bacon), "Bacon", 3, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BowlGreenBeans);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(BowlRice), "Cozimento", "Bowl of Rice", 80.0, 100.0, typeof(BagOfRicemeal), "Bag of Rice", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BowlRice);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(BowlSpinach), "Cozimento", "Bowl of Spinach", 80.0, 100.0, typeof(Spinach), "Spinach", 8, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRes(index, typeof(Vinegar), "Vinegar", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BowlSpinach);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(BowlTurnips), "Cozimento", "Bowl of Turnips", 80.0, 100.0, typeof(Turnip), "Turnip", 4, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BowlTurnips);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(BowlMashedPotatos), "Cozimento", "Bowl of Mashed Potatos", 80.0, 100.0, typeof(Potato), "Potato", 5, 1044253);
            AddRes(index, typeof(Butter), "Butter", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), "Milk", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BowlMashedPotatos);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(BowlCookedVeggies), "Cozimento", "Cooked Bowl of Vegetables", 80.0, 100.0, typeof(MixedVegetables), "Mixed Vegetables", 1, 1044253);
            AddRes(index, typeof(SoySauce), "Soy Sauce", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.BowlCookedVeggies);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(WoodenBowlCabbage), "Cozimento", "Bowl of Cabbage", 80.0, 100.0, typeof(Cabbage), "Cabbage", 2, 1044253);
            AddRes(index, typeof(Vinegar), "Vinegar", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.WoodenBowlCabbage);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(WoodenBowlCarrot), "Cozimento", "Bowl of Carrots", 80.0, 100.0, typeof(Carrot), "Carrot", 12, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.WoodenBowlCarrot);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(WoodenBowlCorn), "Cozimento", "Bowl of Corn", 80.0, 100.0, typeof(Corn), "Corn", 3, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.WoodenBowlCorn);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(WoodenBowlLettuce), "Cozimento", "Bowl of Lettuce", 80.0, 100.0, typeof(Lettuce), "Lettuce", 2, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.WoodenBowlLettuce);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(WoodenBowlPea), "Cozimento", "Bowl of Peas", 80.0, 100.0, typeof(Peas), "Peas", 20, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.WoodenBowlPea);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(PewterBowlOfPotatos), "Cozimento", "Bowl of Potatos", 80.0, 100.0, typeof(Potato), "Potato", 5, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.PewterBowlOfPotatos);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(CornOnCob), "Cozimento", "Corn on the Cob", 80.0, 100.0, typeof(Corn), "Corn", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.CornOnCob);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(Hotdog), "Cozimento", "Hotdog", 80.0, 100.0, typeof(GroundPork), "Ground Pork", 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRes(index, typeof(BreadLoaf), "Bread", 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.Hotdog);
            SetNeedCauldron(index, true);


            index = AddCraft(typeof(MisoSoup), "Cozimento", 1030317, 80.0, 110.0, typeof(RawFishSteak), 1044476, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.MisoSoup);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(WhiteMisoSoup), "Cozimento", 1030318, 80.0, 110.0, typeof(RawFishSteak), 1044476, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.WhiteMisoSoup);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(RedMisoSoup), "Cozimento", 1030319, 80.0, 110.0, typeof(RawFishSteak), 1044476, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.RedMisoSoup);
            SetNeedCauldron(index, true);

            index = AddCraft(typeof(AwaseMisoSoup), "Cozimento", 1030320, 80.0, 110.0, typeof(RawFishSteak), 1044476, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.AwaseMisoSoup);
            SetNeedCauldron(index, true);

            #endregion

            #region Begin Barbecue
            index = AddCraft(typeof(Hotwings), 1044498, "Hotwings", 90.0, 100.0, typeof(RawChickenLeg), "Raw Chicken Leg", 1, 1044253);
            AddRes(index, typeof(JarHoney), "Honey", 1, 1044253);
            AddRes(index, typeof(HotSauce), "Hot Sauce", 1, 1044253);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(PotatoFries), 1044498, "Potato Fries", 90.0, 100.0, typeof(Potato), "Potato", 3, 1044253);
            AddRes(index, typeof(Onion), "Onion", 1, 1044253);
            AddRes(index, typeof(Butter), "Butter", 1, 1044253);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(TacoShell), 1044498, "Taco Shell", 90.0, 100.0, typeof(Tortilla), "Tortilla", 1, 1044253);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(BroccoliCheese), 1044498, "Broccoli and Cheese", 90.0, 100.0, typeof(Broccoli), "Broccoli", 5, 1044253);
            AddRes(index, typeof(CheeseSauce), "Cheese Sauce", 1, 1044253);
            SetNeedOven(index, true);

            index = AddCraft(typeof(BroccoliCaulCheese), 1044498, "Broccoli, Cauliflower and Cheese", 90.0, 100.0, typeof(Broccoli), "Broccoli", 5, 1044253);
            AddRes(index, typeof(Cauliflower), "Cauliflower", 2, 1044253);
            AddRes(index, typeof(CheeseSauce), "Cheese Sauce", 1, 1044253);
            SetNeedOven(index, true);

            index = AddCraft(typeof(CauliflowerCheese), 1044498, "Cauliflower and Cheese", 90.0, 100.0, typeof(Cauliflower), "Cauliflower", 5, 1044253);
            AddRes(index, typeof(CheeseSauce), "Cheese Sauce", 1, 1044253);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Meatballs), 1044498, "Meatballs", 90.0, 100.0, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(BreadLoaf), "Bread", 1, 1044253);
            AddRes(index, typeof(Eggs), "Eggs", 1, 1044253);
            SetNeedOven(index, true);

            index = AddCraft(typeof(Meatloaf), 1044498, "Meatloaf", 90.0, 100.0, typeof(GroundBeef), "Ground Beef", 2, 1044253);
            AddRes(index, typeof(Eggs), "Eggs", 2, 1044253);
            AddRes(index, typeof(Onion), "Onion", 2, 1044253);
            SetNeedOven(index, true);

            index = AddCraft(typeof(BowlOfRotwormStew), 1044498, 1031706, 0.0, 100.0, typeof(RawRotwormMeat), 1031705, 1, 1044253);
            AddRecipe(index, (int)CookRecipesExp.RotWormStew);

            index = AddCraft(typeof(BowlOfBlackrockStew), 1044498, 1115752, 30.0, 70.0, typeof(BowlOfRotwormStew), 1031706, 1, 1044253);
            AddRes(index, typeof(SmallPieceofBlackrock), 1153836, 1, 1044253);
            SetNeedHeat(index, true);
            SetUseAllRes(index, true);
            SetItemHue(index, 1954);
            AddRecipe(index, (int)CookRecipesExp.BlackrockStew);
            ForceNonExceptional(index);

            index = AddCraft(typeof(PotatoStrings), "Classicos", "Potato Strings", 90.0, 100.0, typeof(Potato), "Potato", 1, 1044253);
            SetNeedOven(index, true);

            index = AddCraft(typeof(FriedEggs), 1044498, 1022486, 90.0, 100.0, typeof(Eggs), 1044477, 1, 1044253);
            SetNeedHeat(index, true);
            SetUseAllRes(index, true);

            index = AddCraft(typeof(FishSteak), 1044498, 1022427, 30.0, 100.0, typeof(RawFishSteak), 1044476, 1, 1044253);
            SetNeedHeat(index, true);

            index = AddCraft(typeof(HalibutFishSteak), 1044498, "Halibut Fish Steak", 50.0, 120.0, typeof(RawHalibutSteak), "Raw Halibut Fish Steak", 1, "you need more Raw Halibut Fish Steaks");
            SetNeedHeat(index, true);

            index = AddCraft(typeof(FlukeFishSteak), 1044498, "Fluke Fish Steak", 50.0, 120.0, typeof(RawFlukeSteak), "Raw Fluke Fish Steak", 1, "you need more Raw Fluke Fish Steaks");
            SetNeedHeat(index, true);

            index = AddCraft(typeof(MahiFishSteak), 1044498, "Mahi Fish Steak", 50.0, 120.0, typeof(RawMahiSteak), "Raw Mahi Fish Steak", 1, "you need more Raw Mahi Fish Steaks");
            SetNeedHeat(index, true);

            index = AddCraft(typeof(SalmonFishSteak), 1044498, "Salmon Fish Steak", 50.0, 120.0, typeof(RawSalmonSteak), "Raw Salmon Fish Steak", 1, "you need more Raw Salmon Fish Steaks");
            SetNeedHeat(index, true);

            index = AddCraft(typeof(RedSnapperFishSteak), 1044498, "Red Snapper Fish Steak", 50.0, 120.0, typeof(RawRedSnapperSteak), "Raw Red Snapper Fish Steak", 1, "you need more Raw Red Snapper Fish Steaks");
            SetNeedHeat(index, true);

            index = AddCraft(typeof(ParrotFishSteak), 1044498, "Parrot Fish Steak", 50.0, 120.0, typeof(RawParrotFishSteak), "Raw Parrot Fish Steak", 1, "you need more Raw Parrot Fish Steaks");
            SetNeedHeat(index, true);

            index = AddCraft(typeof(TroutFishSteak), 1044498, "Trout Fish Steak", 50.0, 120.0, typeof(RawTroutSteak), "Raw Trout Fish Steak", 1, "you need more Raw Trout Fish Steaks");
            SetNeedHeat(index, true);

            index = AddCraft(typeof(CookedShrimp), 1044498, "Cooked Shrimp", 50.0, 120.0, typeof(RawShrimp), "Raw Shrimp", 1, "you need more Raw Shrimp");
            SetNeedHeat(index, true);

            #endregion

            #region Dinners
            index = AddCraft(typeof(ChickenParmesian), "Jantares", "Chicken Parmesian", 100.0, 100.0, typeof(RawBird), "Raw Bird", 1, 1044253);
            AddRes(index, typeof(TomatoSauce), "Tomato Sauce", 1, 1044253);
            AddRes(index, typeof(CheeseWheel), "Cheese Wheel", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(CheeseEnchilada), "Jantares", "Cheese Enchilada", 100.0, 100.0, typeof(CheeseWheel), "Cheese Wheel", 1, 1044253);
            AddRes(index, typeof(Tortilla), "Tortilla", 1, 1044253);
            AddRes(index, typeof(EnchiladaSauce), "Enchilada Sauce", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(ChickenEnchilada), "Jantares", "Chicken Enchilada", 100.0, 100.0, typeof(RawBird), "Raw Bird", 1, 1044253);
            AddRes(index, typeof(Tortilla), "Tortilla", 1, 1044253);
            AddRes(index, typeof(EnchiladaSauce), "Enchilada Sauce", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(Lasagna), "Jantares", "Lasagna", 100.0, 100.0, typeof(PastaNoodles), "Pasta Noodles", 3, 1044253);
            AddRes(index, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(CheeseWheel), "Cheese Wheel", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(LemonChicken), "Jantares", "Lemon Chicken", 100.0, 100.0, typeof(RawBird), "Raw Bird", 1, 1044253);
            AddRes(index, typeof(Lemon), "Lemon", 1, 1044253);
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(OrangeChicken), "Jantares", "Orange Chicken", 100.0, 100.0, typeof(RawBird), "Raw Bird", 1, 1044253);
            AddRes(index, typeof(Orange), "Orange", 1, 1044253);
            AddRes(index, typeof(BasketOfHerbsFarm), "Herbs", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(VealParmesian), "Jantares", "Veal Parmesian", 100.0, 100.0, typeof(RawLambLeg), "Raw Lamb Leg", 2, 1044253);
            AddRes(index, typeof(TomatoSauce), "Tomato Sauce", 1, 1044253);
            AddRes(index, typeof(CheeseWheel), "Cheese Wheel", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(BeefBBQRibs), "Jantares", "Beef Barbecue Ribs", 100.0, 100.0, typeof(RawRibs), "Raw Ribs", 1, 1044253);
            AddRes(index, typeof(BarbecueSauce), "Barbecue Sauce", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(BeefBroccoli), "Jantares", "Beef and Broccoli", 100.0, 100.0, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(Broccoli), "Broccoli", 4, 1044253);
            AddRes(index, typeof(SoySauce), "Soy Sauce", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(ChoChoBeef), "Jantares", "Cho Cho Beef", 100.0, 100.0, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(Teriyaki), "Teriyaki", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(BeefSnowpeas), "Jantares", "Beef and Snow Peas", 100.0, 100.0, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(SnowPeas), "Snow Peas", 4, 1044253);
            AddRes(index, typeof(SoySauce), "Soy Sauce", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(Hamburger), "Jantares", "Hamburger", 100.0, 100.0, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(BreadLoaf), "Bread", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(BeefLoMein), "Jantares", "Beef Lo Mein", 100.0, 100.0, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(BowlCookedVeggies), "Cooked Mixed Vegetables", 1, 1044253);
            AddRes(index, typeof(PastaNoodles), "Pasta Noodles", 2, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(BeefStirfry), "Jantares", "Beef Stirfry", 100.0, 100.0, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(BowlCookedVeggies), "Cooked Mixed Vegetables", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(ChickenStirfry), "Jantares", "Chicken Stirfry", 100.0, 100.0, typeof(RawBird), "Raw Bird", 1, 1044253);
            AddRes(index, typeof(BowlCookedVeggies), "Cooked Mixed Vegetables", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(MooShuPork), "Jantares", "Moo Shu Pork", 100.0, 100.0, typeof(GroundPork), "Ground Pork", 1, 1044253);
            AddRes(index, typeof(BowlCookedVeggies), "Cooked Mixed Vegetables", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(MoPoTofu), "Jantares", "Mo Po Tofu", 100.0, 100.0, typeof(Tofu), "Tofu", 1, 1044253);
            AddRes(index, typeof(BowlCookedVeggies), "Cooked Mixed Vegetables", 1, 1044253);
            AddRes(index, typeof(ChiliPepper), "Chili Pepper", 3, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(PorkStirfry), "Jantares", "Pork Stirfry", 100.0, 100.0, typeof(GroundPork), "Ground Pork", 1, 1044253);
            AddRes(index, typeof(BowlCookedVeggies), "Cooked Mixed Vegetables", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(SweetSourChicken), "Jantares", "Sweet and Sour Chicken", 100.0, 100.0, typeof(RawBird), "Raw Bird", 1, 1044253);
            AddRes(index, typeof(JarHoney), "Honey", 1, 1044253);
            AddRes(index, typeof(SoySauce), "SoySauce", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(SweetSourPork), "Jantares", "Sweet and Sour Pork", 100.0, 100.0, typeof(GroundPork), "Ground Pork", 1, 1044253);
            AddRes(index, typeof(JarHoney), "Honey", 1, 1044253);
            AddRes(index, typeof(SoySauce), "SoySauce", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(BaconAndEgg), "Jantares", "Bacon and Eggs", 100.0, 100.0, typeof(Eggs), "Eggs", 2, 1044253);
            AddRes(index, typeof(RawBacon), "Raw Bacon", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(Spaghetti), "Jantares", "Spaghetti", 100.0, 100.0, typeof(PastaNoodles), "Pasta Noodles", 3, 1044253);
            AddRes(index, typeof(TomatoSauce), "Tomato Sauce", 1, 1044253);
            AddRes(index, typeof(GroundBeef), "Ground Beef", 1, 1044253);
            AddRes(index, typeof(FoodPlate), "Plate", 1, "You need a plate!");
            SetNeedOven(index, true);

            index = AddCraft(typeof(MacaroniCheese), "Jantares", "Macaroni and Cheese", 100.0, 100.0, typeof(PastaNoodles), "Pasta Noodles", 3, 1044253);
            AddRes(index, typeof(CheeseSauce), "Cheese Sauce", 1, 1044253);
            SetNeedOven(index, true);
            #endregion

            index = AddCraft(typeof(DarkChocolate), 1080001, 1079994, 15.0, 100.0, typeof(SackOfSugar), 1079997, 1, 1044253);
            AddRes(index, typeof(CocoaButter), 1079998, 1, 1044253);
            AddRes(index, typeof(CocoaLiquor), 1079999, 1, 1044253);
            SetItemHue(index, 0x465);

            index = AddCraft(typeof(MilkChocolate), 1080001, 1079995, 32.5, 107.5, typeof(SackOfSugar), 1079997, 1, 1044253);
            AddRes(index, typeof(CocoaButter), 1079998, 1, 1044253);
            AddRes(index, typeof(CocoaLiquor), 1079999, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1022544, 1, 1044253);
            SetBeverageType(index, BeverageType.Milk);
            SetItemHue(index, 0x461);

            index = AddCraft(typeof(WhiteChocolate), 1080001, 1079996, 52.5, 127.5, typeof(SackOfSugar), 1079997, 1, 1044253);
            AddRes(index, typeof(CocoaButter), 1079998, 1, 1044253);
            AddRes(index, typeof(Vanilla), 1080000, 1, 1044253);
            AddRes(index, typeof(BaseBeverage), 1022544, 1, 1044253);
            SetBeverageType(index, BeverageType.Milk);
            SetItemHue(index, 0x47E);


            index = AddCraft(typeof(FoodEngraver), 1073108, 1072951, 75.0, 100.0, typeof(Dough), 1044469, 1, 1044253);
            AddRes(index, typeof(JarHoney), 1044472, 1, 1044253);

            index = AddCraft(typeof(EnchantedApple), 1073108, 1072952, 60.0, 110.0, typeof(Apple), 1044479, 1, 1044253);
            AddRes(index, typeof(GreaterHealPotion), 1073467, 1, 1044253);

            index = AddCraft(typeof(GrapesOfWrath), 1073108, 1072953, 95.0, 145.0, typeof(Grapes), 1073468, 1, 1044253);
            AddRes(index, typeof(GreaterStrengthPotion), 1073466, 1, 1044253);

            index = AddCraft(typeof(FruitBowl), 1073108, 1072950, 55.0, 105.0, typeof(EmptyWoodenBowl), 1073472, 1, 1044253);
            AddRes(index, typeof(Pear), 1044481, 3, 1044253);
            AddRes(index, typeof(Apple), 1044479, 3, 1044253);
            AddRes(index, typeof(Banana), 1073470, 3, 1044253);

            /* End Enchanted */
            // #endregion

            #region High Seas

            // Fish Oil Flask ( Oléo de peixe )
            index = AddCraft(typeof(FishOil), 1044496, 1150863, 60.0, 120.0, typeof(BaseBeverage), 1046458, 1, 1044253);
            AddRes(index, typeof(RawFishSteak), 1044476, 8, 1044253);
            AddRes(index, typeof(SackFlour), 1044468, 1, 1044253);

            #endregion
        }
    }
}
