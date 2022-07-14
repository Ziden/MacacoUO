#region References
using System;

using Server.Items;
using Server.Targeting;
using Server.Ziden;
#endregion

namespace Server.Engines.Craft
{

    public enum JewelRecipes
    {
        Bracelete = 18901,
        Anel = 18902,
        Colar = 18903,
        Brinco = 18904,

        BraceleteDeOuro = 19000,
        AnelDeOuro = 19001,
        ColarDeOuro = 19002,
        BrincoDeOuro = 19011,

        DiamanteAzul = 19003,
        RubyDeFogo = 19004,
        Turqueza = 19005,
        Esmeralda = 19006,
        Citrino = 19007,
        Safira = 19008,
        Ambar = 19009,
        SafiraEstrela = 19010,
    }

    public class DefJewelcrafting : CraftSystem
    {

        public static Item GetRandomReceitaNoob()
        {
            return new RecipeScroll(18901 + Utility.Random(4));
        }

        public static Item GetReceitaPower()
        {
            return new RecipeScroll(19000 + Utility.Random(12));
        }

        public void SetNeedForno(int index, bool needCauldron)
        {
            CraftItem craftItem = CraftItems.GetAt(index);
            craftItem.NeedForno = needCauldron;
            //craftItem.NeedCauldron = needCauldron;
            craftItem.NeedOven = needCauldron;
        }

        public override SkillName MainSkill { get { return SkillName.Imbuing; } }

        private static CraftSystem m_CraftSystem;

        public static CraftSystem CraftSystem { get { return m_CraftSystem ?? (m_CraftSystem = new DefJewelcrafting()); } }

        public override CraftECA ECA { get { return CraftECA.ChanceMinusSixtyToFourtyFive; } }

        public override double GetChanceAtMin(CraftItem item)
        {
            return 0.0; // 0%
        }

        private DefJewelcrafting()
            //: base(1, 1, 1.25) // base( 1, 2, 1.7 )
            : base(DefBlacksmithy.MIN_ANIM+2, DefBlacksmithy.MAX_ANIM+2, DefBlacksmithy.DELAY)
        {
            /*
            base( MinCraftEffect, MaxCraftEffect, Delay )
            MinCraftEffect	: The minimum number of time the mobile will play the craft effect
            MaxCraftEffect	: The maximum number of time the mobile will play the craft effect
            Delay			: The delay between each craft effect
            Example: (3, 6, 1.7) would make the mobile do the PlayCraftEffect override
            function between 3 and 6 time, with a 1.7 second delay each time.
            */
        }

        public static void Translate(Mobile from, int msg)
        {
            Lang.Msg(from, msg);
        }

        public override int CanCraft(Mobile from, ITool tool, Type itemType)
        {
            int num = 0;

            if (tool == null || tool.Deleted || tool.UsesRemaining <= 0)
            {
                return 1044038; // You have worn out your tool!
            }

            if (tool is Item && !BaseTool.CheckTool((Item)tool, from))
            {
                return 1048146; // If you have a tool equipped, you must use that tool.
            }

            else if (!tool.CheckAccessible(from, ref num))
            {
                return num; // The tool must be on your person to use.
            }

            if (tool is AddonToolComponent && from.InRange(((AddonToolComponent)tool).GetWorldLocation(), 2))
            {
                return 0;
            }
            return 0;
        }

        public override void PlayCraftEffect(Mobile from)
        {
            // no animation, instant sound
            //if ( from.Body.Type == BodyType.Human && !from.Mounted )
            //	from.Animate( 9, 5, 1, true, false, 0 );
            //new InternalTimer( from ).Start();
            from.Animate(AnimationType.Attack, 3);
            from.PlaySound(0x2A);
        }

        // Delay to synchronize the sound with the hit on the anvil
        private class InternalTimer : Timer
        {
            private readonly Mobile m_From;

            public InternalTimer(Mobile from)
                : base(TimeSpan.FromSeconds(0.7))
            {
                m_From = from;
            }

            protected override void OnTick()
            {
                m_From.PlaySound(0x2A);
                m_From.Animate(AnimationType.Attack, 3);
            }
        }

        public override int PlayEndingEffect(
            Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item)
        {
            if (toolBroken)
            {
                from.SendMessage("Sua ferramenta quebrou"); // You have worn out your tool
            }

            if (failed)
            {
                if (lostMaterial)
                {
                    return 1044043; // You failed to create the item, and some of your materials are lost.
                }

                return 1044157; // You failed to create the item, but no materials were lost.
            }

            if (quality == 0)
            {
                return 502785; // You were barely able to make this item.  It's quality is below average.
            }

            if (makersMark && quality == 2)
            {
                return 1044156; // You create an exceptional quality item and affix your maker's mark.
            }

            if (quality == 2)
            {
                return 1044155; // You create an exceptional quality item.
            }
            return 1044154; // You create the item.
        }

        public static Type[,] Joias = new Type[,] {
            {
                typeof(SilverBracelet),
                typeof(SilverNecklace),
                typeof(SilverBeadNecklace),
                typeof(SilverRing),
            },
            {
                typeof(GoldEarrings),
                typeof(GoldRing),
                typeof(GoldBeadNecklace),
                typeof(GoldBracelet)
            }
        };

        public static Type[] Materiais = new Type[]
        {
            typeof(IronIngot), typeof(SilverIngot)
        };

        public static String[] Nomes = new String[]
       {
            "Ferro", "Dourado"
       };

        public static Tuple<int, int>[] Diffs = new Tuple<int, int>[]
     {
            new Tuple<int, int>(0, 80),
              new Tuple<int, int>(60, 90),

     };

        public static String[] NomesJoias = new String[]
     {
            "Bracelete", "Colar", "Colar de Bolinha", "Anel"
     };

        private static readonly Type[] Pedras = new[]
        {
            typeof(Amber), typeof(Amethyst), typeof(Citrine), typeof(Diamond), typeof(Emerald), typeof(Ruby), typeof(Sapphire),
            typeof(StarSapphire), typeof(Tourmaline)
        };

        public override void InitCraftList()
        {
            int index;

            index = AddCraft(typeof(DragonStone), "Utils", "Pedra Draconiana", 70, 110, typeof(DragonHead), "Cabeca de Dragao", 1, "Voce precisa de 1 cabeca de dragao");
            SetNeedForno(index, true);

            index = AddCraft(typeof(JoiaArma), "Utils", "Joia Slayer para Armas", 70, 110, typeof(BloodOfTheDarkFather), "Sangue do Senhor da Escuridao", 2, "Voce precisa de 2 sangue do senhor das sombras");
            SetNeedForno(index, true);

            index = AddCraft(typeof(TalismanDaProtecao), "Talismans", "Talisman de Protecao", 80, 110, typeof(RelicFragment), "Fragmento de Reliquia", 5, "Voce precisa de 5 Fragmentos de Reliquia");
            AddRes(index, typeof(SilverIngot), "Barras de Ouro", 10, "Voce precisa 10 barras de Ouro");
            SetNeedForno(index, true);

            index = AddCraft(typeof(TalismanDaMorte), "Talismans", "Talisman de Dano", 80, 110, typeof(RelicFragment), "Fragmento de Reliquia", 5, "Voce precisa de 6 Fragmentos de Reliquia");
            AddRes(index, typeof(SilverIngot), "Barras de Ouro", 10, "Voce precisa 10 barras de Ouro");
            SetNeedForno(index, true);

            // Ferro
            index = AddCraft(typeof(SilverBracelet), "Joias Basicas", "Bracelete", -25, 70,
                typeof(IronIngot), "Lingotes de Ferro", 8, "Faltam lingotes de ferro");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.Bracelete);

            index = AddCraft(typeof(SilverEarrings), "Joias Basicas", "Brinco", -25, 70, typeof(IronIngot), "Lingotes de Ferro", 8, "Faltam lingotes de ferro");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.Brinco);

            index = AddCraft(typeof(SilverNecklace), "Joias Basicas", "Colar", -25, 70, typeof(IronIngot), "Lingotes de Ferro", 8, "Faltam lingotes de ferro");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.Colar);

            index = AddCraft(typeof(SilverBeadNecklace), "Joias Basicas", "Colar de Bolinhas", -25, 70, typeof(IronIngot), "Lingotes de Ferro", 8, "Faltam lingotes de ferro");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.Colar);

            index = AddCraft(typeof(SilverRing), "Joias Basicas", "Anel", -25, 70, typeof(IronIngot), "Lingotes de Ferro", 8, "Faltam lingotes de ferro");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.Anel);

            // Ouro
            index = AddCraft(typeof(GoldBracelet), "Joias Basicas", "Bracelete de Ouro", 70, 90, typeof(SilverIngot), "Lingotes de Dourado", 2, "Faltam lingotes de Dourado");
            AddRes(index, typeof(SilverBracelet), "Bracelete", 1, "Voce precisa de um bracelete");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.BraceleteDeOuro);

            index = AddCraft(typeof(GoldEarrings), "Joias Basicas", "Brinco de Ouro", 70, 90, typeof(SilverIngot), "Lingotes de Dourado", 2, "Faltam lingotes de Dourado");
            AddRes(index, typeof(SilverEarrings), "Brinco", 1, "Voce precisa de um brinco");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.BraceleteDeOuro);

            index = AddCraft(typeof(GoldNecklace), "Joias Basicas", "Colar de Ouro", 70, 90, typeof(SilverIngot), "Lingotes de Dourado", 2, "Faltam lingotes de Dourado");
            AddRes(index, typeof(SilverNecklace), "Colar", 1, "Voce precisa de um Colar");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.ColarDeOuro);

            index = AddCraft(typeof(GoldBeadNecklace), "Joias Basicas", "Colar de Bolinhas de Ouro", 70, 90, typeof(SilverIngot), "Lingotes de Dourado", 2, "Faltam lingotes de Dourado");
            AddRes(index, typeof(SilverBeadNecklace), "Colar de Bolinhas", 1, "Voce precisa de um Colar de Bolinhas");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.ColarDeOuro);

            index = AddCraft(typeof(GoldRing), "Joias Basicas", "Bracelete", 70, 90, typeof(SilverIngot), "Lingotes de Dourado", 2, "Faltam lingotes de Dourado");
            AddRes(index, typeof(SilverRing), "Anel", 1, "Voce precisa de um Anel");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.AnelDeOuro);

            // Pedra JC
            index = AddCraft(typeof(Taint), "Pedras", "Tourmalina Derretida", 40, 120, typeof(Tourmaline), "Turmalina", 5, "Faltam pedras para criar isto");
            SetNeedForno(index, true);

            index = AddCraft(typeof(BlueDiamond), "Pedras", "Diamante Azul", 40, 120, typeof(Diamond), "Diamante", 5, "Faltam pedras para criar isto");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.DiamanteAzul);

            index = AddCraft(typeof(FireRuby), "Pedras", "Rubi de Fogo", 40, 120, typeof(Ruby), "Ruby", 5, "Faltam pedras para criar isto");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.RubyDeFogo);

            index = AddCraft(typeof(WhitePearl), "Pedras", "Perola Branca", 40, 120, typeof(StarSapphire), "Safira Estrela", 5, "Faltam pedras para criar isto");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.SafiraEstrela);

            index = AddCraft(typeof(Turquoise), "Pedras", "Turquesa", 40, 120, typeof(Amethyst), "Ametista", 3, "Faltam pedras para criar isto");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.Turqueza);

            index = AddCraft(typeof(PerfectEmerald), "Pedras", "Esmeralda Perfeita", 40, 120, typeof(Emerald), "Esmeralda",5, "Faltam pedras para criar isto");
            AddRecipe(index, (int)JewelRecipes.Esmeralda);

            index = AddCraft(typeof(EcruCitrine), "Pedras", "Citrino Ecru", 40, 120, typeof(Citrine), "Cirtino", 5, "Faltam pedras para criar isto");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.Citrino);

            index = AddCraft(typeof(DarkSapphire), "Pedras", "Safira Negra", 40, 120, typeof(Sapphire), "Safira", 5, "Faltam pedras para criar isto");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.Safira);

            index = AddCraft(typeof(BrilliantAmber), "Pedras", "Ambar Brilhante", 40, 120, typeof(Amber), "Ambar", 5, "Faltam pedras para criar isto");
            SetNeedForno(index, true);
            AddRecipe(index, (int)JewelRecipes.Ambar);

            // Joias +1 Skill
            AddMagico(typeof(BlueDiamond), "Diamante Azul", JewelRecipes.DiamanteAzul);
            AddMagico(typeof(FireRuby), "Ruby de Fogo", JewelRecipes.RubyDeFogo);
            AddMagico(typeof(WhitePearl), "Perola Branca", JewelRecipes.SafiraEstrela);
            AddMagico(typeof(Turquoise), "Turqueza", JewelRecipes.Turqueza);
            AddMagico(typeof(PerfectEmerald), "Esmeralda Perfeita", JewelRecipes.Esmeralda);
            AddMagico(typeof(EcruCitrine), "Citrina Eru", JewelRecipes.Citrino);
            AddMagico(typeof(DarkSapphire), "Safira Negra", JewelRecipes.Safira);
            AddMagico(typeof(BrilliantAmber), "Ambar Brilhante", JewelRecipes.Ambar);

            AddElegante(typeof(Diamond), "Diamante Azul", JewelRecipes.DiamanteAzul);
            AddElegante(typeof(Ruby), "Ruby de Fogo", JewelRecipes.RubyDeFogo);
            AddElegante(typeof(StarSapphire), "Perola Branca", JewelRecipes.SafiraEstrela);
            AddElegante(typeof(Amethyst), "Turqueza", JewelRecipes.Turqueza);
            AddElegante(typeof(Emerald), "Esmeralda Perfeita", JewelRecipes.Esmeralda);
            AddElegante(typeof(Citrine), "Citrina Eru", JewelRecipes.Citrino);
            AddElegante(typeof(Sapphire), "Safira Negra", JewelRecipes.Safira);
            AddElegante(typeof(Amber), "Ambar Brilhante", JewelRecipes.Ambar);



            Resmelt = true;
            Repair = false;
            MarkOption = true;
            CanEnhance = false;
            CanAlter = false;
            Jewelcraft = true;
        }

        public void AddElegante(Type pedra, string n, JewelRecipes rec)
        {
            var index = AddCraft(typeof(GoldBraceletBonito), "Joias Elegantes", "Bracelete Elegante de " + n, 80, 110, typeof(GoldBracelet), "Bracelete de Ouro", 1, "Voce nao tem a joia suficiente");
            AddRes(index, pedra, n, 1, "Voce nao tem a pedra preciosa para fazer isto");
            SetNeedForno(index, true);

            index = AddCraft(typeof(GoldNecklaceBonito), "Joias Elegantes", "Colar Elegante de " + n, 0, 80, typeof(GoldNecklace), "Colar de Ouro", 1, "Voce nao tem a joia suficiente");
            AddRes(index, pedra, n, 1, "Voce nao tem a pedra preciosa para fazer isto");
            SetNeedForno(index, true);

            index = AddCraft(typeof(GoldBeadNecklaceMagico), "Joias Elegantes", "Colar de Bolinhas Elegante de " + n, 0, 80, typeof(GoldBeadNecklace), "Colar de Bolinha de Ouro", 1, "Voce nao tem a joia suficiente");
            AddRes(index, pedra, n, 1, "Voce nao tem a pedra preciosa para fazer isto");
            SetNeedForno(index, true);

            index = AddCraft(typeof(GoldRingMagico), "Joias Elegantes", "Anel Elegante de " + n, 0, 80, typeof(GoldRing), "Anel de Ouro", 1, "Voce nao tem a joia suficiente");
            AddRes(index, pedra, n, 1, "Voce nao tem a pedra preciosa para fazer isto");
            SetNeedForno(index, true);

            index = AddCraft(typeof(GoldEarringsMagico),  "Joias Elegantes", "Brinco Elegante de " + n, 80, 80, typeof(GoldEarrings), "Brinco de Ouro", 1, "Voce nao tem a joia suficiente");
            AddRes(index, pedra, n, 1, "Voce nao tem a pedra preciosa para fazer isto");
            SetNeedForno(index, true);

            Resmelt = true;
        }

        public void AddMagico(Type pedra, string n, JewelRecipes rec)
        {
            var index = AddCraft(typeof(GoldBraceletMagico), "Joias Magicas", "Bracelete de "+n, 80, 110, typeof(GoldBracelet), "Bracelete de Ouro", 1, "Voce nao tem a joia suficiente");
            AddRes(index, pedra, n, 1, "Voce nao tem a pedra preciosa para fazer isto");
            SetNeedForno(index, true);
            //AddRecipe(index, (int)JewelRecipes.BraceleteDeOuro);
            //AddRecipe(index, (int)rec);

            index = AddCraft(typeof(GoldNecklaceMagico), "Joias Magicas", "Colar de "+n, 80, 110, typeof(GoldNecklace), "Colar de Ouro", 1, "Voce nao tem a joia suficiente");
            AddRes(index, pedra, n, 1, "Voce nao tem a pedra preciosa para fazer isto");
            SetNeedForno(index, true);
            //AddRecipe(index, (int)JewelRecipes.ColarDeOuro);
            //AddRecipe(index, (int)rec);

            index = AddCraft(typeof(GoldBeadNecklaceMagico), "Joias Magicas", "Colar de Bolinhas de "+n, 80, 110, typeof(GoldBeadNecklace), "Colar de Bolinha de Ouro", 1, "Voce nao tem a joia suficiente");
            AddRes(index, pedra, n, 1, "Voce nao tem a pedra preciosa para fazer isto");
            SetNeedForno(index, true);
            //AddRecipe(index, (int)JewelRecipes.ColarDeOuro);
            //AddRecipe(index, (int)rec);

            index = AddCraft(typeof(GoldRingMagico), "Joias Magicas", "Anel de "+n, 80, 110, typeof(GoldRing), "Anel de Ouro", 1, "Voce nao tem a joia suficiente");
            AddRes(index, pedra, n, 1, "Voce nao tem a pedra preciosa para fazer isto");
            SetNeedForno(index, true);
            //AddRecipe(index, (int)JewelRecipes.AnelDeOuro);
            //AddRecipe(index, (int)rec);

            index = AddCraft(typeof(GoldEarringsMagico), "Joias Magicas", "Brinco de " + n, 80, 110, typeof(GoldEarrings), "Brinco de Ouro", 1, "Voce nao tem a joia suficiente");
            AddRes(index, pedra, n, 1, "Voce nao tem a pedra preciosa para fazer isto");
            SetNeedForno(index, true);
            //AddRecipe(index, (int)JewelRecipes.BrincoDeOuro);
            //AddRecipe(index, (int)rec);
        }
    }
}
