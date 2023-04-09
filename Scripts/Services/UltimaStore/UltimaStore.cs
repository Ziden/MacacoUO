using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Server.Commands;
using Server.Mobiles;
using Server.Items;
using Server.Engines.VendorSearching;
using Server.Gumps;
using Server.Network;
using Server.Engines.Points;
using Server.Multis;
using Server.Items.Functional.Pergaminhos;
using Server.Multis.Deeds;
using Server.Ziden.Achievements;
using Server.Ziden;

namespace Server.Engines.UOStore
{
    public enum StoreCategory
    {
        None,
        Featured,
        Character,
        Equipment,
        Decorations,
        Mounts,
        Misc,
        Cart,
        Houses,
        Vestuario,
    }

    public enum SortBy
    {
        Name,
        PriceLower,
        PriceHigher,
        Newest,
        Oldest
    }

    public static class UltimaStore
    {
        public static readonly string FilePath = Path.Combine("Saves/Misc", "UltimaStore.bin");

        public static bool Enabled { get { return Configuration.Enabled; } set { Configuration.Enabled = value; } }

        public static List<StoreEntry> Entries { get; private set; }
        public static Dictionary<Mobile, List<Item>> PendingItems { get; private set; }

        private static UltimaStoreContainer _UltimaStoreContainer;

        public static UltimaStoreContainer UltimaStoreContainer
        {
            get
            {
                if (_UltimaStoreContainer != null && _UltimaStoreContainer.Deleted)
                {
                    _UltimaStoreContainer = null;
                }

                return _UltimaStoreContainer ?? (_UltimaStoreContainer = new UltimaStoreContainer());
            }
        }

        static UltimaStore()
        {
            Entries = new List<StoreEntry>();
            PendingItems = new Dictionary<Mobile, List<Item>>();
            PlayerProfiles = new Dictionary<Mobile, PlayerProfile>();
        }

        public static void Configure()
        {
            PacketHandlers.Register(0xFA, 1, true, UOStoreRequest);

            CommandSystem.Register("Store", AccessLevel.Player, e => OpenStore(e.Mobile as PlayerMobile));

            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        public static void Initialize()
        {
            StoreCategory cat = StoreCategory.Featured; // SEI
            //GERAL
            Register<Sandalhadahumildade>(new TextDefinition("Sandalha da Humildade"), "Roupas Especiais", 0x170D, 0, 0, 2000, cat);
            Register<AventalDaLuz>(new TextDefinition("Avental Da Luz"), "Roupas Especiais", 0x153b, 0, 0, 2000, cat);
            Register<BandanaDaluz>(new TextDefinition("Bandana  Da Luz"), "Roupas Especiais", 0x1540, 0, 0, 2000, cat);
            Register<RobeAnjoDaluz>(new TextDefinition("Robe Anjo Da Luz"), "Roupas Especiais", 0x1F03, 0, 0, 5000, cat);
            Register<AventalDaDasTrevas>(new TextDefinition("Avental Das Trevas"), "Roupas True Black", 0x153b, 0, 0, 2000, cat);
            Register<BandanaDasTrevas>(new TextDefinition("Bandana  Das Trevas"), "Roupas True Black", 0x1540, 0, 0, 2000, cat);
            Register<RobeAnjoDasTrevas>(new TextDefinition("Robe Das Trevas"), "Roupas True Black", 0x1F03, 0, 0, 5000, cat);
            Register<DragonicRobe>(new TextDefinition("Robe Dragonic "), "Roupa Comemorativa<b>Estoque limitado.", 0x1F03, 0, 0, 6000, cat);
            Register<SobretudodoCarrasco>(new TextDefinition("Sobretudo Do Carrasco"), "Sobretudo Especial<b> Pode ser pintado.", 0x2687, 0, 0, 5000, cat);
            Register<CharacterStatueMaker>("Estatua de Jogador", "Construa uma estatua do seu personagem.<br>Se eternalize !.", 0x32F0, 0, 0, 8000, cat);
            Register<PergaminhoPerdao>("Pergaminho do Perdao", "Remove 1 long count.", 0x1F35, 0, 55, 500, cat);
            Register<SacolaCristais>("100 Cristais Elementais", "Sacola com 100 cristais elementais.", 16395, 0, 2611, 25000, cat);
            Register<SacolaCristalTherathan>("50 Cristais Terathan", "Sacola com 50 Cristais Terathan.", 16395, 0, 2611, 12500, cat);
            Register<Spellbook>("Fullbook", "Fullbook.<br>Tem todas as magias.", 0x14F0, 0, 0, 5000, cat, ConstructSpellbook);
            Register<DoubleExpDeed>("PowerHour 2x Exp", "Ativa double exp para o shard todo por 1h.</ br > Todos vao te amar um pouco mais.", 0x14F0, 0, 256, 500, cat);
            Register<DoubleGoldDeed>("Power Hour 2x Gold", "Ativa double gold para o shard todo por 1h.</br>Todos vao te amar um pouco mais.", 0x14F0, 0, 54, 500, cat);
            
            cat = StoreCategory.Misc;
            //MORADIAS
            Register<CastleDeed>("Castelo", "Deed de Castelo imenso.<br>Moradia super glamurosa para realeza.", 0x14F0, 0, 0, 10000, cat);
            Register<KeepDeed>("Keep", "Deed de Keep.<br>Moradia chique para lords.", 0x14F0, 0, 0, 7500, cat);
            Register<TowerDeed>("Torre", "Deed de Torre.<br>Moradia chique para lords.", 0x14F0, 0, 0, 5000, cat);
            Register<LargeMarbleDeed>("Marble", "Deed de Marble.<br>Moradia chique para lords.", 0x14F0, 0, 0, 3000, cat);
            Register<TrinsicKeepDeed>("Keep Trinsic", "Deed de Moradia Keep 3 andares .<br>Moradia chique para lords.", 0x14F0, 0, 0, 15000, cat);
            Register<CasaMogaDeed>("Casa Moga", "Deed de Moradia.<br>Moradia chique para lords.", 0x14F0, 0, 0, 20000, cat);
            Register<RobinsNestDeed>("Casa The robin’s", "Deed de Moradia.<br>Moradia chique para lords.", 0x14F0, 0, 0, 15000, cat);
            Register<CastleOfOceaniaDeed>("Oceania Castle", "Deed de Moradia.<br>Moradia chique para lords.", 0x14F0, 0, 0, 15000, cat);
            Register<FeudalCastleDeed>("Castle Feudal", "Deed de Moradia.<br>Moradia chique para lords.", 0x14F0, 0, 0, 15000, cat);
            Register<ElsaCastleDeed>("Elsa Castle", "Deed de Moradia.<br>Moradia chique para lords.", 0x14F0, 0, 0, 15000, cat);
            Register<SpiresDeed>("Spires Castle", "Deed de Moradia.<br>Moradia chique para lords.", 0x14F0, 0, 0, 15000, cat);
            Register<GothicRoseCastleDeed>("Gothic Rose Castle", "Deed de Moradia.<br>Moradia chique para lords.", 0x14F0, 0, 0, 15000, cat);
            Register<TraditionalKeepDeed>("Traditional Keep Deed", "Deed de Moradia.<br>Moradia chique para lords.", 0x14F0, 0, 0, 15000, cat);
            Register<SandalwoodKeepDeed>("Sandalwood Keep Deed", "Deed de Moradia.<br>Moradia chique para lords.", 0x14F0, 0, 0, 15000, cat);
            Register<DarkthornKeepDeed>("Darkthorn Keep Deed", "Deed de Moradia.<br>Moradia chique para lords.", 0x14F0, 0, 0, 15000, cat); 
            Register<Sobrado7x7>("Deed Casa Custom 7x7", "Atenção.<br>Custa gold para construir partes.", 0x14F0, 0, 0, 5000, cat); 
            Register<Sobrado11x11>("Deed Casa Custom 11x11", "Atenção.<br>Custa gold para construir partes.", 0x14F0, 0, 0, 15000, cat);
            Register<TorreMansao>("Deed Casa Custom 18x18", "Atenção.<br>Custa gold para construir partes.", 0x14F0, 0, 0, 15000, cat);


            cat = StoreCategory.Character;
            //PERSONAGEM
            //Register<CombatSkillBook>("Livro +5000 Exp", "Livro que garante instantaneamente 5000 EXP.</br>Nao funciona para elementos.", 0xEFA, 0, 0xA33, 1000, cat, ConstructLivro);
            Register<PergaminhoSkillcap>("Pergaminho de Skillcap", "Usar aumenta seu skillcap em + 1 <br> Limitado ate +20 ", 0x1F49, 0, 0, 1000, cat);
            Register<StableSlotIncreaseToken>("+1 Slot Estabulo", "Aumenta um slot para deixar animais no estabulo", 0x2AAA, 0, 0, 2000, cat);
            Register<AbyssalHairDye>("Tinta para Cabelos", "Vermelho Abissal", 0, 0x9C7A, 0, 1000, cat);
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Verde Limao", 0, 0x9C78, 0, 1000, cat, ConstructHairDye); // Lemon Lime
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Marrom bom bom", 0, 0x9C6D, 0, 1000, cat, ConstructHairDye); // Yew Brown 
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Vermelho Pau Brasil", 0, 0x9C6E, 0, 1000, cat, ConstructHairDye); // Bloodwood Red
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Azul Vivido", 0, 0x9C6F, 0, 1000, cat, ConstructHairDye); // Vivid Blue
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Loiro de Cinzas", 0, 0x9C71, 0, 1000, cat, ConstructHairDye); // Ash Blonde
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Verde Floresta", 0, 0x9C72, 0, 1000, cat, ConstructHairDye); // Heartwood Green
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Loiro Carvalho", 0, 0x9C85, 0, 1000, cat, ConstructHairDye); // Oak Blonde
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Branco Sagrado", 0, 0x9C70, 0, 1000, cat, ConstructHairDye); // Sacred White
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Rainha de Gelo", 0, 0x9C73, 0, 1000, cat, ConstructHairDye); // Frostwood Ice Green
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Loiro de Fogo", 0, 0x9C76, 0, 1000, cat, ConstructHairDye); // Fiery Blonde
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Marrom Amargo", 0, 0x9C77, 0, 1000, cat, ConstructHairDye); // Bitter Brown
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Azul Doidao", 0, 0x9C74, 0, 1000, cat, ConstructHairDye); // Gnaw's Twisted Blue
            Register<SpecialHairDye>(new TextDefinition("Tinta para Cabelos"), "Preto do Nevoeiro", 0, 0x9C75, 0, 1000, cat, ConstructHairDye); // Dusk Black
            Register<GenderChangeToken>(new TextDefinition[] { "Trocar de Genero", 1156615 }, 1156642, 0x2AAA, 0, 0, 1000, cat);
            Register<NameChangeToken>(new TextDefinition[] { "Trocar de Nome", 1156615 }, 1156641, 0x2AAA, 0, 0, 1000, cat);
            //Register<PergaminhoSagradoSupremo>("Pergaminho de Item Pessoal (Newbie)", "Torna uma roupa um percence pessoal (newbie) para sempre.</br>Esta roupa nao sera perdida quando morrer e nao pode ser destruida exceto por acido.", 0x14F0, 0, 0, 2500, cat);
            Register<TintaPreta>("Tinta Preta", "Balde de tinta preta.<br>Ma-ra-vi-lhosa com roupas sombrias.", 0xFAB, 0, TintaPreta.COR, 500, cat);
            Register<TintaBranca>("Tinta Branca", "Balde de tinta branca.<br>Divina cor para iluminados e praticantes da luz.", 0xFAB, 0, TintaBranca.COR, 500, cat);

            cat = StoreCategory.Equipment;
            //EQUIPAMENTOS
            Register<SmugglersLantern>(new TextDefinition("Lanterna Magica"), "Percente Pessoal<br>Permite usar magias com a lanterna na mao.<br>Vem em cores sortidas.", 0xA25, 0, 0, 2000, cat);
            Register<Kasa>(new TextDefinition("Chapeu Oriental"), "Percence Pessoal. <br>Apenas cosmetico. <br> Pode ser pintado.", 0x2798, 0, 0, 1000, cat, ConstructNewbie);
            Register<DeerMask> (new TextDefinition("Máscara de Viado Sacrificado"), "Percence Pessoal.", 0x1547, 0, 0, 3000, cat, ConstructNewbie);
            Register< LeatherNinjaHood> (new TextDefinition("Capuz de Ninja"), "Percence Pessoal.", 0x27DA, 0, 0, 2000, cat, ConstructNewbie);
            Register<Tessen>(new TextDefinition("Tessen"), "Percence Pessoal.", 0x27A3, 0, 0, 2000, cat, ConstructNewbie);
           
            //DECORAÇÃO
            cat = StoreCategory.Decorations;
            Register<GardenShedDeed>(new TextDefinition("Abrigo de jardim"), "é um item extraordinário que funciona como um complemento da casa, garantindo armazenamento extra em qualquer casa. .", 0x14EF, 0, 0, 4500, cat);
            Register<DecorativeBlackwidowDeed>(1157897, 1157898, 0, 0x9CD7, 0, 600, cat);
            Register<HildebrandtDragonRugDeed>(1157889, 1157890, 0, 0x9CD8, 0, 700, cat);
            Register<SmallWorldTreeRugAddonDeed>(1157206, 1157898, 0, 0x9CBA, 0, 300, cat);
            Register<LargeWorldTreeRugAddonDeed>(1157207, 1157898, 0, 0x9CBA, 0, 500, cat);
            Register<MountedPixieWhiteDeed>(new TextDefinition[] { 1074482, 1156915 }, 1156974, 0x2A79, 0, 0, 500, cat);
            Register<MountedPixieLimeDeed>(new TextDefinition[] { 1074482, 1156914 }, 1156974, 0x2A77, 0, 0, 500, cat);
            Register<MountedPixieBlueDeed>(new TextDefinition[] { 1074482, 1156913 }, 1156974, 0x2A75, 0, 0, 500, cat);
            Register<MountedPixieOrangeDeed>(new TextDefinition[] { 1074482, 1156912 }, 1156974, 0x2A73, 0, 0, 500, cat);
            Register<MountedPixieGreenDeed>(new TextDefinition[] { 1074482, 1156911 }, 1156974, 0x2A71, 0, 0, 500, cat);
            Register<UnsettlingPortraitDeed>(1074480, 1156973, 0x2A65, 0, 0, 500, cat);
            Register<CreepyPortraitDeed>(1074481, 1156972, 0x2A69, 0, 0, 500, cat);
            Register<DisturbingPortraitDeed>(1074479, 1156955, 0x2A5D, 0, 0, 500, cat);
            Register<DawnsMusicBox>(1075198, 1156968, 0x2AF9, 0, 0, 500, cat);
            Register<BedOfNailsDeed>(1074801, 1156975, 0, 0x9C8D, 0, 500, cat);
            Register<BrokenCoveredChairDeed>(1076257, 1156950, 0xC17, 0, 0, 500, cat);
            Register<BoilingCauldronDeed>(1076267, 1156949, 0, 0x9CB9, 0, 500, cat);
            Register<SuitOfGoldArmorDeed>(1076265, 1156943, 0x3DAA, 0, 0, 500, cat);
            Register<BrokenBedDeed>(1076263, 1156945, 0, 0x9C8F, 0, 500, cat);
            Register<BrokenArmoireDeed>(1076262, 1156946, 0xC12, 0, 0, 500, cat);
            Register<BrokenVanityDeed>(1076260, 1156947, 0, 0x9C90, 0, 500, cat);
            Register<BrokenBookcaseDeed>(1076258, 1156948, 0xC14, 0, 0, 500, cat);
            Register<SacrificialAltarDeed>(1074818, 1156954, 0, 0x9C8E, 0, 500, cat);
            Register<HauntedMirrorDeed>(1074800, 1156953, 0x2A7B, 0, 0, 500, cat);
            Register<BrokenChestOfDrawersDeed>(1076261, 1156951, 0xC24, 0, 0, 500, cat);
            Register<StandingBrokenChairDeed>(1076259, 1156952, 0xC1B, 0, 0, 500, cat);
            Register<FountainOfLifeDeed>(1075197, 1156964, 0x2AC0, 0, 0, 500, cat);
            Register<TapestryOfSosaria>(1062917, 1156961, 0x234E, 0, 0, 500, cat);
            Register<RoseOfTrinsic>(1062913, 1156960, 0x234D, 0, 0, 500, cat);
            Register<HearthOfHomeFireDeed>(1062919, 1156958, 0, 0x9C97, 0, 500, cat);
            Register<GreenGoblinStatuette>(1125133, 1158015, 0xA095, 0, 0, 1500, cat);
            Register<WelcomeMat> (new TextDefinition("Tapete de boas vindas"), "Decoração.", 0x47DA, 0, 0, 2000, cat);
            Register<HitchingPost>("Poste de Estabulo", "Permite estabular e retirar animais em casa. </br>Tem 30 cargas mas pode ser recarregado com cordas de estabulo.", 0x14E7, 0, 0, 5000, cat, ConstructHitchingPost);
            Register<HitchingRope>("Corda de Estabulo", "Recarrega o poste de estabulo", 0x14F8, 0, 0, 100, cat, ConstructHitchingPost);
            


            cat = StoreCategory.Mounts;
            //MONTARIAS
            Register<EtherealHorse>(new TextDefinition("Cavalo Magico"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um cavalo magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel", 0x20DD, 0, 0, 2000, cat, CavaloEthy);
            Register<EtherealOstard>(new TextDefinition("Ostard Magico"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um ostard magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel", 0x2135, 0, 0, 7000, cat, OstardEthy);
            Register<EtherealWindrunner>(new TextDefinition("Windrunner"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um cavalo magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel.<br>", 0x9ED5, 0, 0, 10000, cat, WindRunner);
            Register<EtherealLlama>(new TextDefinition("Lhama Magica"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um cavalo magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel", 0x20F6, 0, 0, 3000, cat);    
            Register<EtherealBeetle>(new TextDefinition("Barata Magica"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um cavalo magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel", 0x260F, 0, 0, 3000, cat);
            Register<EtherealSwampDragon>(new TextDefinition("Dragão  do Pantano <b> Magico"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um cavalo magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel", 0x2619, 0, 0, 5000, cat);
            Register<RideablePolarBear>(new TextDefinition("Urso Polar Magico"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um cavalo magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel", 0x20E1, 0, 0, 5000, cat);
            Register<EtherealHiryu>(new TextDefinition("Hiryu Magico"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um cavalo magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel", 0x276A, 0, 0, 5000, cat);
            Register<EtherealAncientHellHound>(new TextDefinition("HellHound Magico"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um cavalo magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel", 0x3FFD, 0, 0, 5000, cat);
            Register<EtherealTarantula>(new TextDefinition("Tarantula"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um cavalo magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel", 0x9DD6, 0, 0, 5000, cat);
            Register<EtherealSerpentineDragon>(new TextDefinition("SerpentineDragon"), "Item pertence pessoal que nao se perde ao morrer. <br>Pode ser usado a qualquer momento para invocar um cavalo magico.<br>Pode ser usado para sempre.<br>Nao consome slots de animais<br>Intransferivel", 0xA010, 0, 0, 80000, cat);
            

                                                  
            //- - - REFERENCIAS --- //
            //Register<MythicCharacterToken>(new TextDefinition[] { 1156614, 1156615 }, 1156679, 0x2AAA, 0, 0, 2500, cat); // Colocar 5 skills até 90
            //Register<Tekagi>(new TextDefinition("Tekagi"), "Percence Pessoal.", 0x27AB, 0, 0, 1000, cat, ConstructNewbie);//trocar valor
            //Register<Daisho>(new TextDefinition("Daisho"), "Percence Pessoal.", 0x27A9, 0, 0, 1000, cat, ConstructNewbie);//trocar valor

            // Register<WindrunnerStatue>(new TextDefinition("Windrunner"), "Montaria. <br>Esta montaria vem Bound e pode ser ressada com Veterinary.<br>", 0x9ED5, 0, 0, 3000, cat, WindRunner);

            /*
            cat = StoreCategory.Equipment;
            Register<VirtueShield>(1109616, 1158384, 0x7818, 0, 0, 1500, cat);
            Register<HoodedBritanniaRobe>(1125155, 1158016, 0xA0AB, 0, 0, 1500, cat, ConstructRobe);
            Register<HoodedBritanniaRobe>(1125155, 1158016, 0xA0AC, 0, 0, 1500, cat, ConstructRobe);
            Register<HoodedBritanniaRobe>(1125155, 1158016, 0xA0AD, 0, 0, 1500, cat, ConstructRobe);
            Register<HoodedBritanniaRobe>(1125155, 1158016, 0xA0AE, 0, 0, 1500, cat, ConstructRobe);
            Register<HoodedBritanniaRobe>(1125155, 1158016, 0xA0AF, 0, 0, 1500, cat, ConstructRobe);
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1157275 }, 1156671, 0, 0x9CBF, 0, 400, cat, ConstructHaochisPigment); // Heartwood Sienna
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1157274 }, 1156671, 0, 0x9CBD, 0, 400, cat, ConstructHaochisPigment); // Campion White
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1157273 }, 1156671, 0, 0x9CC2, 0, 400, cat, ConstructHaochisPigment); // Yewish Pine
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1157272 }, 1156671, 0, 0x9CC0, 0, 400, cat, ConstructHaochisPigment); // Minocian Fire
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1157269 }, 1156671, 0, 0x9CC1, 0, 400, cat, ConstructHaochisPigment); // Celtic Lime

            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070994 }, 1156906, 0, 0x9CA8, 0, 400, cat, ConstructPigments); // Nox Green
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079584 }, 1156906, 0, 0x9CAF, 0, 400, cat, ConstructPigments); // Midnight Coal
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070995 }, 1156906, 0, 0x9CA5, 0, 400, cat, ConstructPigments); // Rum Red
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079580 }, 1156906, 0, 0x9CA4, 0, 400, cat, ConstructPigments); // Coal
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079582 }, 1156906, 0, 0x9CA3, 0, 400, cat, ConstructPigments); // Storm Bronze
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079581 }, 1156906, 0, 0x9CA2, 0, 400, cat, ConstructPigments); // Faded Gold
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070988 }, 1156906, 0, 0x9CA1, 0, 400, cat, ConstructPigments); // Violet Courage Purple
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079585 }, 1156906, 0, 0x9CA2, 0, 400, cat, ConstructPigments); // Faded Bronze
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070996 }, 1156906, 0, 0x9C9F, 0, 400, cat, ConstructPigments); // Fire Orange
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079586 }, 1156906, 0, 0x9C9E, 0, 400, cat, ConstructPigments); // Faded Rose
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079583 }, 1156906, 0, 0x9CA7, 0, 400, cat, ConstructPigments); // Rose
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079587 }, 1156906, 0, 0x9CA9, 0, 400, cat, ConstructPigments); // Deep Rose
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070990 }, 1156906, 0, 0x9CAA, 0, 400, cat, ConstructPigments); // Luna White

            Register<CommemorativeRobe>(1157009, 1156908, 0x4B9D, 0, 0, 500, cat);

            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070992 }, 1156906, 0, 0x9CAF, 0, 400, cat, ConstructPigments); // Shadow Dancer Black
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070989 }, 1156906, 0, 0x9CAE, 0, 400, cat, ConstructPigments); // Invulnerability Blue
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070991 }, 1156906, 0, 0x9CAD, 0, 400, cat, ConstructPigments); // Dryad Green
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070993 }, 1156906, 0, 0x9CAC, 0, 400, cat, ConstructPigments); // Berserker Red
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079579 }, 1156906, 0, 0x9CAB, 0, 400, cat, ConstructPigments); // Faded Coal
            Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070987 }, 1156906, 0, 0x9C9D, 0, 400, cat, ConstructPigments); // Paragon Gold

            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1071246 }, 1156671, 0, 0x9CAF, 0, 400, cat, ConstructHaochisPigment); // Ninja Black
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1018352 }, 1156671, 0, 0x9C83, 0, 400, cat, ConstructHaochisPigment); // Olive
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1071247 }, 1156671, 0, 0x9C7D, 0, 400, cat, ConstructHaochisPigment); // Dark Reddish Brown
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1071245 }, 1156671, 0, 0x9C85, 0, 400, cat, ConstructHaochisPigment); // Yellow
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1071244 }, 1156671, 0, 0x9C80, 0, 400, cat, ConstructHaochisPigment); // Pretty Pink
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1071248 }, 1156671, 0, 0x9C81, 0, 400, cat, ConstructHaochisPigment); // Midnight Blue
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1023856 }, 1156671, 0, 0x9C7F, 0, 400, cat, ConstructHaochisPigment); // Emerald
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1115467 }, 1156671, 0, 0x9C82, 0, 400, cat, ConstructHaochisPigment); // Smoky Gold
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1115468 }, 1156671, 0, 0x9C7E, 0, 400, cat, ConstructHaochisPigment); // Ghost's Grey
            Register<HaochisPigment>(new TextDefinition[] { 1071249, 1115471 }, 1156671, 0, 0x9C84, 0, 400, cat, ConstructHaochisPigment); // Ocean Blue   

            // Featured
            // StoreCategory cat = StoreCategory.Featured;
            /*
             Register<VirtueShield>(1109616, 1158384, 0x7818, 0, 0, 1500, cat);
             Register<SoulstoneToken>(1158404, 1158405, 0x2A93, 0, 2598, 1000, cat, ConstructSoulstone);
             Register<DeluxeStarterPackToken>(1158368, 1158369, 0, 0x9CCB, 0, 2000, cat);
             Register<GreenGoblinStatuette>(1125133, 1158015, 0xA095, 0, 0, 600, cat);
             //Register<TotemOfChromaticFortune>(1157606, 1157604, 0, 0x9CC9, 0, 300, cat);

             // TOKEN PICA
             Register<MythicCharacterToken>(new TextDefinition[] { 1156614, 1156615 }, 1156679, 0x2AAA, 0, 0, 2500, cat);
             */
            // Character
            /*
           cat = StoreCategory.Character;

           Register<HABPromotionalToken>(new TextDefinition[] { 1158741, 1156615 }, 1158740, 0x2AAA, 0, 0, 600, cat);
           Register<MysticalPolymorphTotem>(1158780, 1158781, 0xA276, 0, 0, 600, cat);
           //Register<DeluxeStarterPackToken>(1158368, 1158369, 0, 0x9CCB, 0, 2000, cat);
           Register<GreenGoblinStatuette>(1125133, 1158015, 0xA095, 0, 0, 600, cat);
           Register<GreyGoblinStatuette>(1125135, 1158015, 0xA097, 0, 0, 600, cat);
           Register<StableSlotIncreaseToken>(1157608, 1157609, 0x2AAA, 0, 0, 500, cat);
           Register<MythicCharacterToken>(new TextDefinition[] { 1156614, 1156615 }, 1156679, 0x2AAA, 0, 0, 2500, cat);
           Register<CharacterReincarnationToken>(new TextDefinition[] { 1156612, 1156615 }, 1156677, 0x2AAA, 0, 0, 2000, cat);

           Register<AbyssalHairDye>(1149822, 1156676, 0, 0x9C7A, 0, 400, cat);
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071439 }, 1156676, 0, 0x9C78, 0, 400, cat, ConstructHairDye); // Lemon Lime
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071470 }, 1156676, 0, 0x9C6D, 0, 400, cat, ConstructHairDye); // Yew Brown 
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071471 }, 1156676, 0, 0x9C6E, 0, 400, cat, ConstructHairDye); // Bloodwood Red
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071438 }, 1156676, 0, 0x9C6F, 0, 400, cat, ConstructHairDye); // Vivid Blue
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071469 }, 1156676, 0, 0x9C71, 0, 400, cat, ConstructHairDye); // Ash Blonde
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071472 }, 1156676, 0, 0x9C72, 0, 400, cat, ConstructHairDye); // Heartwood Green
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071472 }, 1156676, 0, 0x9C85, 0, 400, cat, ConstructHairDye); // Oak Blonde
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071474 }, 1156676, 0, 0x9C70, 0, 400, cat, ConstructHairDye); // Sacred White
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071473 }, 1156676, 0, 0x9C73, 0, 400, cat, ConstructHairDye); // Frostwood Ice Green
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071440 }, 1156676, 0, 0x9C76, 0, 400, cat, ConstructHairDye); // Fiery Blonde
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071437 }, 1156676, 0, 0x9C77, 0, 400, cat, ConstructHairDye); // Bitter Brown
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071442 }, 1156676, 0, 0x9C74, 0, 400, cat, ConstructHairDye); // Gnaw's Twisted Blue
           Register<SpecialHairDye>(new TextDefinition[] { 1071387, 1071441 }, 1156676, 0, 0x9C75, 0, 400, cat, ConstructHairDye); // Dusk Black

           Register<GenderChangeToken>(new TextDefinition[] { 1156609, 1156615 }, 1156642, 0x2AAA, 0, 0, 1000, cat);
           Register<NameChangeToken>(new TextDefinition[] { 1156608, 1156615 }, 1156641, 0x2AAA, 0, 0, 1000, cat);

           // Equipment
           cat = StoreCategory.Equipment;
           Register<VirtueShield>(1109616, 1158384, 0x7818, 0, 0, 1500, cat);
           Register<HoodedBritanniaRobe>(1125155, 1158016, 0xA0AB, 0, 0, 1500, cat, ConstructRobe);
           Register<HoodedBritanniaRobe>(1125155, 1158016, 0xA0AC, 0, 0, 1500, cat, ConstructRobe);
           Register<HoodedBritanniaRobe>(1125155, 1158016, 0xA0AD, 0, 0, 1500, cat, ConstructRobe);
           Register<HoodedBritanniaRobe>(1125155, 1158016, 0xA0AE, 0, 0, 1500, cat, ConstructRobe);
           Register<HoodedBritanniaRobe>(1125155, 1158016, 0xA0AF, 0, 0, 1500, cat, ConstructRobe);

           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1157275 }, 1156671, 0, 0x9CBF, 0, 400, cat, ConstructHaochisPigment); // Heartwood Sienna
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1157274 }, 1156671, 0, 0x9CBD, 0, 400, cat, ConstructHaochisPigment); // Campion White
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1157273 }, 1156671, 0, 0x9CC2, 0, 400, cat, ConstructHaochisPigment); // Yewish Pine
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1157272 }, 1156671, 0, 0x9CC0, 0, 400, cat, ConstructHaochisPigment); // Minocian Fire
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1157269 }, 1156671, 0, 0x9CC1, 0, 400, cat, ConstructHaochisPigment); // Celtic Lime

           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070994 }, 1156906, 0, 0x9CA8, 0, 400, cat, ConstructPigments); // Nox Green
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079584 }, 1156906, 0, 0x9CAF, 0, 400, cat, ConstructPigments); // Midnight Coal
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070995 }, 1156906, 0, 0x9CA5, 0, 400, cat, ConstructPigments); // Rum Red
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079580 }, 1156906, 0, 0x9CA4, 0, 400, cat, ConstructPigments); // Coal
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079582 }, 1156906, 0, 0x9CA3, 0, 400, cat, ConstructPigments); // Storm Bronze
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079581 }, 1156906, 0, 0x9CA2, 0, 400, cat, ConstructPigments); // Faded Gold
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070988 }, 1156906, 0, 0x9CA1, 0, 400, cat, ConstructPigments); // Violet Courage Purple
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079585 }, 1156906, 0, 0x9CA2, 0, 400, cat, ConstructPigments); // Faded Bronze
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070996 }, 1156906, 0, 0x9C9F, 0, 400, cat, ConstructPigments); // Fire Orange
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079586 }, 1156906, 0, 0x9C9E, 0, 400, cat, ConstructPigments); // Faded Rose
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079583 }, 1156906, 0, 0x9CA7, 0, 400, cat, ConstructPigments); // Rose
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079587 }, 1156906, 0, 0x9CA9, 0, 400, cat, ConstructPigments); // Deep Rose
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070990 }, 1156906, 0, 0x9CAA, 0, 400, cat, ConstructPigments); // Luna White

           Register<CommemorativeRobe>(1157009, 1156908, 0x4B9D, 0, 0, 500, cat);

           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070992 }, 1156906, 0, 0x9CAF, 0, 400, cat, ConstructPigments); // Shadow Dancer Black
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070989 }, 1156906, 0, 0x9CAE, 0, 400, cat, ConstructPigments); // Invulnerability Blue
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070991 }, 1156906, 0, 0x9CAD, 0, 400, cat, ConstructPigments); // Dryad Green
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070993 }, 1156906, 0, 0x9CAC, 0, 400, cat, ConstructPigments); // Berserker Red
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1079579 }, 1156906, 0, 0x9CAB, 0, 400, cat, ConstructPigments); // Faded Coal
           Register<PigmentsOfTokuno>(new TextDefinition[] { 1070933, 1070987 }, 1156906, 0, 0x9C9D, 0, 400, cat, ConstructPigments); // Paragon Gold

           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1071246 }, 1156671, 0, 0x9CAF, 0, 400, cat, ConstructHaochisPigment); // Ninja Black
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1018352 }, 1156671, 0, 0x9C83, 0, 400, cat, ConstructHaochisPigment); // Olive
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1071247 }, 1156671, 0, 0x9C7D, 0, 400, cat, ConstructHaochisPigment); // Dark Reddish Brown
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1071245 }, 1156671, 0, 0x9C85, 0, 400, cat, ConstructHaochisPigment); // Yellow
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1071244 }, 1156671, 0, 0x9C80, 0, 400, cat, ConstructHaochisPigment); // Pretty Pink
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1071248 }, 1156671, 0, 0x9C81, 0, 400, cat, ConstructHaochisPigment); // Midnight Blue
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1023856 }, 1156671, 0, 0x9C7F, 0, 400, cat, ConstructHaochisPigment); // Emerald
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1115467 }, 1156671, 0, 0x9C82, 0, 400, cat, ConstructHaochisPigment); // Smoky Gold
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1115468 }, 1156671, 0, 0x9C7E, 0, 400, cat, ConstructHaochisPigment); // Ghost's Grey
           Register<HaochisPigment>(new TextDefinition[] { 1071249, 1115471 }, 1156671, 0, 0x9C84, 0, 400, cat, ConstructHaochisPigment); // Ocean Blue   

           Register<SmugglersEdge>(1071499, 1156664, 0, 0x9C63, 0, 400, cat);
           Register<UndertakersStaff>(1071498, 1156663, 0x13F8, 0, 0, 500, cat);
           Register<ReptalonFormTalisman>(new TextDefinition[] { 1157010, 1075202 }, 1156967, 0x2F59, 0, 0, 100, cat);
           Register<QuiverOfInfinity>(1075201, 1156971, 0x2B02, 0, 0, 100, cat);
           Register<CuSidheFormTalisman>(new TextDefinition[] { 1157010, 1031670 }, 1156970, 0x2F59, 0, 0, 100, cat);
           Register<FerretFormTalisman>(new TextDefinition[] { 1157010, 1031672 }, 1156969, 0x2F59, 0, 0, 100, cat);
           Register<LeggingsOfEmbers>(1062911, 1156956, 0x1411, 0, 0x2C, 100, cat);
           Register<ShaminoCrossbow>(1062915, 1156957, 0x26C3, 0, 0x504, 100, cat);
           Register<SamuraiHelm>(1062923, 1156959, 0x236C, 0, 0, 100, cat);
           Register<HolySword>(1062921, 1156962, 0xF61, 0, 0x482, 100, cat);
           Register<DupresShield>(1075196, 1156963, 0x2B01, 0, 0, 100, cat);
           Register<OssianGrimoire>(1078148, 1156965, 0x2253, 0, 0, 100, cat);
           Register<SquirrelFormTalisman>(new TextDefinition[] { 1157010, 1031671 }, 1156966, 0x2F59, 0, 0, 100, cat);
           Register<EarringsOfProtection>(new TextDefinition[] { 1156821, 1156822 }, 1156659, 0, 0x9C66, 0, 200, cat, ConstructEarrings); // Physcial
           Register<EarringsOfProtection>(1071092, 1156659, 0, 0x9C66, 0, 200, cat, ConstructEarrings); // Fire
           Register<EarringsOfProtection>(1071093, 1156659, 0, 0x9C66, 0, 200, cat, ConstructEarrings); // Cold
           Register<EarringsOfProtection>(1071094, 1156659, 0, 0x9C66, 0, 200, cat, ConstructEarrings); // Poison
           Register<EarringsOfProtection>(1071095, 1156659, 0, 0x9C66, 0, 200, cat, ConstructEarrings); // Energy
           Register<HoodedShroudOfShadows>(1079727, 1156643, 0x2684, 0, 0x455, 1000, cat);

           // decorations
           cat = StoreCategory.Decorations;
           Register<DecorativeBlackwidowDeed>(1157897, 1157898, 0, 0x9CD7, 0, 600, cat);
           Register<HildebrandtDragonRugDeed>(1157889, 1157890, 0, 0x9CD8, 0, 700, cat);
           Register<SmallWorldTreeRugAddonDeed>(1157206, 1157898, 0, 0x9CBA, 0, 300, cat);
           Register<LargeWorldTreeRugAddonDeed>(1157207, 1157898, 0, 0x9CBA, 0, 500, cat);
           Register<MountedPixieWhiteDeed>(new TextDefinition[] { 1074482, 1156915 }, 1156974, 0x2A79, 0, 0, 100, cat);
           Register<MountedPixieLimeDeed>(new TextDefinition[] { 1074482, 1156914 }, 1156974, 0x2A77, 0, 0, 100, cat);
           Register<MountedPixieBlueDeed>(new TextDefinition[] { 1074482, 1156913 }, 1156974, 0x2A75, 0, 0, 100, cat);
           Register<MountedPixieOrangeDeed>(new TextDefinition[] { 1074482, 1156912 }, 1156974, 0x2A73, 0, 0, 100, cat);
           Register<MountedPixieGreenDeed>(new TextDefinition[] { 1074482, 1156911 }, 1156974, 0x2A71, 0, 0, 100, cat);
           Register<UnsettlingPortraitDeed>(1074480, 1156973, 0x2A65, 0, 0, 100, cat);
           Register<CreepyPortraitDeed>(1074481, 1156972, 0x2A69, 0, 0, 100, cat);
           Register<DisturbingPortraitDeed>(1074479, 1156955, 0x2A5D, 0, 0, 100, cat);
           Register<DawnsMusicBox>(1075198, 1156968, 0x2AF9, 0, 0, 100, cat);
           Register<BedOfNailsDeed>(1074801, 1156975, 0, 0x9C8D, 0, 100, cat);
           Register<BrokenCoveredChairDeed>(1076257, 1156950, 0xC17, 0, 0, 100, cat);
           Register<BoilingCauldronDeed>(1076267, 1156949, 0, 0x9CB9, 0, 100, cat);
           Register<SuitOfGoldArmorDeed>(1076265, 1156943, 0x3DAA, 0, 0, 100, cat);
           Register<BrokenBedDeed>(1076263, 1156945, 0, 0x9C8F, 0, 100, cat);
           Register<BrokenArmoireDeed>(1076262, 1156946, 0xC12, 0, 0, 100, cat);
           Register<BrokenVanityDeed>(1076260, 1156947, 0, 0x9C90, 0, 100, cat);
           Register<BrokenBookcaseDeed>(1076258, 1156948, 0xC14, 0, 0, 100, cat);
           Register<SacrificialAltarDeed>(1074818, 1156954, 0, 0x9C8E, 0, 100, cat);
           Register<HauntedMirrorDeed>(1074800, 1156953, 0x2A7B, 0, 0, 100, cat);
           Register<BrokenChestOfDrawersDeed>(1076261, 1156951, 0xC24, 0, 0, 100, cat);
           Register<StandingBrokenChairDeed>(1076259, 1156952, 0xC1B, 0, 0, 100, cat);
           Register<FountainOfLifeDeed>(1075197, 1156964, 0x2AC0, 0, 0, 100, cat);
           Register<TapestryOfSosaria>(1062917, 1156961, 0x234E, 0, 0, 100, cat);
           Register<RoseOfTrinsic>(1062913, 1156960, 0x234D, 0, 0, 100, cat);
           Register<HearthOfHomeFireDeed>(1062919, 1156958, 0, 0x9C97, 0, 100, cat);
           // TODO: Singing Ball
           // TODO: Secret Chest

           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1157015 }, 1156916, 0, 0x9CB5, 0, 200, cat, ConstructMiniHouseDeed); // two story wood & plaster
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011317 }, 1156916, 0x22F5, 0, 0, 200, cat, ConstructMiniHouseDeed); // small stone tower
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011307 }, 1156916, 0x22E0, 0, 0, 200, cat, ConstructMiniHouseDeed); // wood and plaster house
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011308 }, 1156916, 0x22E1, 0, 0, 200, cat, ConstructMiniHouseDeed); // thathed-roof cottage
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011312 }, 1156916, 0, 0x9CB2, 0, 200, cat, ConstructMiniHouseDeed); // Tower
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011313 }, 1156916, 0, 0x9CB1, 0, 200, cat, ConstructMiniHouseDeed); // Small stone keep
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011314 }, 1156916, 0, 0x9CB0, 0, 200, cat, ConstructMiniHouseDeed); // Castle

           Register<HangingSwordsDeed>(1076272, 1156936, 0, 0x9C96, 0, 100, cat);
           Register<UnmadeBedDeed>(1076279, 1156935, 0, 0x9C9B, 0, 100, cat);
           Register<CurtainsDeed>(1076280, 1156934, 0, 0x9C93, 0, 100, cat);
           Register<TableWithOrangeClothDeed>(new TextDefinition[] { 1157012, 1157013 }, 1156933, 0x118E, 0, 0, 100, cat);

           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011320 }, 1156916, 0x22F3, 0, 0, 200, cat, ConstructMiniHouseDeed); // sanstone house with patio
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011316 }, 1156916, 0, 0x9CB3, 0, 200, cat, ConstructMiniHouseDeed); // marble house with patio
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011319 }, 1156916, 0x2300, 0, 0, 200, cat, ConstructMiniHouseDeed); // two story villa
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1157014 }, 1156916, 0, 0x9CB6, 0, 200, cat, ConstructMiniHouseDeed); // two story stone & plaster
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011315 }, 1156916, 0, 0x9CB4, 0, 200, cat, ConstructMiniHouseDeed); // Large house with patio
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011309 }, 1156916, 0, 0x9CB7, 0, 200, cat, ConstructMiniHouseDeed); // brick house
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011304 }, 1156916, 0x22C9, 0, 0, 200, cat, ConstructMiniHouseDeed); // field stone house
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011306 }, 1156916, 0x22DF, 0, 0, 200, cat, ConstructMiniHouseDeed); // wooden house
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011305 }, 1156916, 0x22DE, 0, 0, 200, cat, ConstructMiniHouseDeed); // small brick house
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011303 }, 1156916, 0x22E1, 0, 0, 200, cat, ConstructMiniHouseDeed); // stone and plaster house
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011318 }, 1156916, 0x22FB, 0, 0, 200, cat, ConstructMiniHouseDeed); // two-story log cabin
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011321 }, 1156916, 0x22F6, 0, 0, 200, cat, ConstructMiniHouseDeed); // small stone workshop
           Register<MiniHouseDeed>(new TextDefinition[] { 1062096, 1011322 }, 1156916, 0x22F4, 0, 0, 200, cat, ConstructMiniHouseDeed); // small marble workshop

           Register<TableWithBlueClothDeed>(1076276, 1156932, 0x118C, 0, 0, 100, cat);
           Register<CherryBlossomTreeDeed>(1076268, 1156940, 0, 0x9C91, 0, 100, cat);
           Register<IronMaidenDeed>(1076288, 1156924, 0x1249, 0, 0, 100, cat);
           Register<SmallFishingNetDeed>(1076286, 1156923, 0x1EA3, 0, 0, 100, cat);
           Register<StoneStatueDeed>(1076284, 1156922, 0, 0x9C9A, 0, 100, cat);
           Register<WallTorchDeed>(1076282, 1156921, 0x3D98, 0, 0, 100, cat);
           Register<HouseLadderDeed>(1076287, 1156920, 0x2FDE, 0, 0, 100, cat);
           Register<LargeFishingNetDeed>(1076285, 1156919, 0x3D8E, 0, 0, 100, cat);
           Register<FountainDeed>(1076283, 1156918, 0, 0x9C94, 0, 100, cat);
           Register<ScarecrowDeed>(1076608, 1156917, 0x1E34, 0, 0, 100, cat);
           Register<HangingAxesDeed>(1076271, 1156937, 0, 0x9C95, 0, 100, cat);
           Register<AppleTreeDeed>(1076269, 1156938, 0, 0x9C8C, 0, 100, cat);
           Register<GuillotineDeed>(1024656, 1156941, 0x125E, 0, 0, 100, cat);
           Register<SuitOfSilverArmorDeed>(1076266, 1156942, 0x3D86, 0, 0, 100, cat);
           Register<PeachTreeDeed>(1076270, 1156939, 0, 0x9C98, 0, 100, cat);
           Register<CherryBlossomTrunkDeed>(1076784, 1156925, 0x26EE, 0, 0, 100, cat);
           Register<PeachTrunkDeed>(1076786, 1156926, 0xD9C, 0, 0, 100, cat);
           Register<BrokenFallenChairDeed>(1076264, 1156944, 0xC19, 0, 0, 100, cat);
           Register<TableWithRedClothDeed>(1076277, 1156930, 0x118E, 0, 0, 100, cat);
           Register<VanityDeed>(1074027, 1156931, 0, 0x9C9C, 0, 100, cat);
           Register<AppleTrunkDeed>(1076785, 1156927, 0xD98, 0, 0, 100, cat);
           Register<TableWithPurpleClothDeed>(new TextDefinition[] { 1157011, 1157013 }, 1156929, 0x118B, 0, 0, 100, cat);
           Register<WoodenCoffinDeed>(1076274, 1156928 , 0, 0x9C92, 0, 100, cat);
           Register<RaisedGardenDeed>(new TextDefinition[] { 1150359, 1156688 }, 1156680, 0, 0x9C8B, 0, 2000, cat, ConstructRaisedGarden);
           Register<HouseTeleporterTileBag>(new TextDefinition[] { 1156683, 1156826 }, 1156668, 0x40B9, 0, 1201, 1000, cat);
           Register<WoodworkersBenchDeed>(1026641, 1156670, 0x14F0, 0, 0, 600, cat);
           Register<LargeGlowingLadyBug>(1026641, 1156660, 0x2CFD, 0, 0, 200, cat);
           Register<FreshGreenLadyBug>(1071401, 1156661, 0x2D01, 0, 0, 200, cat);
           Register<WillowTreeDeed>(1071105, 1156658, 0x224A, 0, 0, 200, cat);

           Register<FallenLogDeed>(1071088, 1156649, 0, 0x9C88, 0, 200, cat);
           Register<LampPost2>(1071089, 1156650, 0xB22, 0, 0, 200, cat, ConstructLampPost);
           Register<HitchingPost>(1071090, 1156651, 0x14E7, 0, 0, 200, cat, ConstructHitchingPost);
           Register<AncestralGravestone>(1071096, 1156653, 0x1174, 0, 0, 200, cat);
           Register<WoodenBookcase>(1071102, 1156655, 0x0A9D, 0, 0, 200, cat);
           Register<SnowTreeDeed>(1071103, 1156656, 0, 0x9C8A, 0, 200, cat);
           Register<MapleTreeDeed>(1071104, 1156657, 0, 0x9C87, 0, 200, cat);

           // mounts
           cat = StoreCategory.Mounts;
           Register<WindrunnerStatue>(1124685, 1157373, 0x9ED5, 0, 0, 1000, cat);
           Register<LasherStatue>(1157214, 1157305, 0x9E35, 0, 0, 1000, cat);
           Register<ChargerOfTheFallen>(1075187, 1156646, 0x2D9C, 0, 0, 1000, cat);
           Register<EowmuStatue>(1158082, 1158433, 0xA0C0, 0, 0, 1000, cat);

           // misc
           cat = StoreCategory.Misc;
           Register<SoulstoneToken>(1158404, 1158405, 0x2A93, 0, 2598, 1000, cat, ConstructSoulstone);
           Register<BagOfBulkOrderCovers>(1071116, 1157603, 0, 0x9CC6, 0, 200, cat, ConstructBOBCoverOne);

           //TODO: UndeadWeddingBundle, TotemOfChromaticFortune, 

           Register<PetBrandingIron>(1157314, 1157372, 0, 0x9CC3, 0, 600, cat);
           Register<PetBondingPotion>(1152921, 1156678, 0, 0x9CBC, 0, 500, cat); 

           Register<ForgedMetalOfArtifacts>(new TextDefinition[] { 1149868, 1156686 }, 1156674, 0, 0x9C65, 0, 1000, cat, ConstructForgedMetal);
           Register<ForgedMetalOfArtifacts>(new TextDefinition[] { 1149868, 1156687 }, 1156675, 0, 0x9C65, 0, 600, cat, ConstructForgedMetal);
           Register<PenOfWisdom>(1115358, 1156669, 0, 0x9C62, 0, 600, cat);

           Register<BritannianShipDeed>(1150100, 1156673, 0, 0x9C6A, 0, 1200, cat);

           Register<SoulstoneToken>(1078835, 1158405, 0x2ADC, 0, 0, 1000, cat, ConstructSoulstone);
           Register<SoulstoneToken>(1078834, 1158405, 0x2A93, 0, 0, 1000, cat, ConstructSoulstone);

           Register<MerchantsTrinket>(new TextDefinition[] { 1156827, 1156681 }, 1156666, 0, 0x9C67, 0, 300, cat, ConstructMerchantsTrinket);
           Register<MerchantsTrinket>(new TextDefinition[] { 1156828, 1156682 }, 1156667, 0, 0x9C67, 0, 500, cat, ConstructMerchantsTrinket);

           Register<ArmorEngravingToolToken>(1080547, 1156652, 0, 0x9C65, 0, 200, cat);
           Register<BagOfBulkOrderCovers>(1071116, 1156654, 0, 0x9CC6, 0, 200, cat, ConstructBOBCoverTwo);
       */
        }

        public static void Register<T>(TextDefinition name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null) where T : Item
        {
            Register(typeof(T), name, tooltip, itemID, gumpID, hue, cost, cat, constructor);
        }

        public static void Register<T>(TextDefinition name, string tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null) where T : Item
        {
            Register(new StoreEntry(typeof(T), name, tooltip, itemID, gumpID, hue, cost, cat, constructor));
        }

        public static void Register(Type itemType, TextDefinition name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null)
        {
            Register(new StoreEntry(itemType, name, tooltip, itemID, gumpID, hue, cost, cat, constructor));
        }

        public static void Register<T>(TextDefinition[] name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null) where T : Item
        {
            Register(typeof(T), name, tooltip, itemID, gumpID, hue, cost, cat, constructor);
        }

        public static void Register(Type itemType, TextDefinition[] name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null)
        {
            Register(new StoreEntry(itemType, name, tooltip, itemID, gumpID, hue, cost, cat, constructor));
        }

        public static void Register(StoreEntry entry)
        {

            Entries.Add(entry);
        }

        public static bool CanSearch(Mobile m)
        {
            return m != null && m.Region.GetLogoutDelay(m) <= TimeSpan.Zero;
        }

        public static void UOStoreRequest(NetState state, PacketReader pvSrc)
        {
            OpenStore(state.Mobile as PlayerMobile);
        }

        public static void OpenStore(PlayerMobile user)
        {
            if (user == null || user.NetState == null)
            {
                return;
            }

            if (!Enabled)
            {
                // The promo code redemption system is currently unavailable. Please try again later.
                user.SendLocalizedMessage(1062904);
                return;
            }

            if (Configuration.CurrencyImpl == CurrencyType.None)
            {
                // The promo code redemption system is currently unavailable. Please try again later.
                user.SendLocalizedMessage(1062904);
                return;
            }

            if (!user.NetState.UltimaStore)
            {
                user.SendMessage("You must update Ultima Online in order to use the in game store.");
                return;
            }

            if (user.AccessLevel < AccessLevel.Counselor && !CanSearch(user))
            {
                // Before using the in game store, you must be in a safe log-out location
                // such as an inn or a house which has you on its Owner, Co-owner, or Friends list.
                user.SendMessage("Voce precisa estar em um local seguro para isto");
                return;
            }

            if (!user.HasGump(typeof(UltimaStoreGump)))
            {
                BaseGump.SendGump(new UltimaStoreGump(user));
            }
        }

        #region Constructors
        public static Item CavaloEthy(Mobile m, StoreEntry entry)
        {
            var cavalo = new EtherealHorse();
            cavalo.Transparent = false;
            cavalo.BoundTo = m.RawName;
            return cavalo;
        }

        public static Item WindRunner(Mobile m, StoreEntry entry)
        {
            var cavalo = new WindrunnerStatue();
            return cavalo;
        }

        public static Item OstardEthy(Mobile m, StoreEntry entry)
        {
            var cavalo = new EtherealOstard();
            cavalo.Transparent = false;
            cavalo.BoundTo = m.RawName;
            //cavalo.Hue = TintaPreta.COR;
            //cavalo.NonTransparentMountedHue = TintaPreta.COR;
            return cavalo;
        }

        public static Item LhamaEthy(Mobile m, StoreEntry entry)
        {
            var cavalo = new EtherealLlama();
            cavalo.Transparent = false;
            return cavalo;
        }

        public static Item ConstructNewbie(Mobile m, StoreEntry entry)
        {
            var item = (Item)Activator.CreateInstance(entry.ItemType);
            if (item.Name != null)
            {
                if (item.Name.Substring(item.Name.Length - 1) == "o")
                    item.Name += " sofisticado";
                else
                    item.Name += " sofisticada";
            }
            item.LootType = LootType.Blessed;
            return item;
        }

        public static Item ConstructRobeMorto(Mobile m, StoreEntry entry)
        {
            var shroud = new HoodedShroudOfShadows();
            shroud.LootType = LootType.Blessed;
            shroud.Name = "Manto dos Mortos";
            shroud.Hue = TintaPreta.COR;
            shroud.BoundTo = m.RawName;
            return shroud;
        }

        public static Item ConstructSpellbook(Mobile m, StoreEntry entry)
        {
            var shroud = new Spellbook();
            shroud.Content = ulong.MaxValue;
            return shroud;
        }

        public static Item ConstructLivro(Mobile m, StoreEntry entry)
        {
            var shroud = new CombatSkillBook();
            shroud.Exp = 5000;
            return shroud;
        }

        public static Item ConstructHairDye(Mobile m, StoreEntry entry)
        {
            Shard.Debug("ENTRY ITEM ID: " + entry.GumpID);
            var info = NaturalHairDye.Table.FirstOrDefault(x => x.GumpID == entry.GumpID);

            if (info != null)
            {
                Shard.Debug("Achei a tinta");
                return new NaturalHairDye(info.Type);
            }
            Shard.Debug("Nao achei o item ID... Possiveis :");
            var list = NaturalHairDye.Table.Select(h => h.GumpID);
            Shard.Debug(list.ToString());
            return null;
        }

        public static Item ConstructHaochisPigment(Mobile m, StoreEntry entry)
        {
            var info = HaochisPigment.Table.FirstOrDefault(x => x.Localization == entry.Name[1].Number);

            if (info != null)
            {
                return new HaochisPigment(info.Type, 50);
            }

            return null;
        }

        public static Item ConstructPigments(Mobile m, StoreEntry entry)
        {
            PigmentType type = PigmentType.None;

            for (int i = 0; i < PigmentsOfTokuno.Table.Length; i++)
            {
                if (PigmentsOfTokuno.Table[i][1] == entry.Name[1].Number)
                {
                    type = (PigmentType)i;
                    break;
                }
            }

            if (type != PigmentType.None)
            {
                return new PigmentsOfTokuno(type, 50);
            }

            return null;
        }

        public static Item ConstructEarrings(Mobile m, StoreEntry entry)
        {
            AosElementAttribute ele = AosElementAttribute.Physical;

            switch (entry.Name[0].Number)
            {
                case 1071092: ele = AosElementAttribute.Fire; break;
                case 1071093: ele = AosElementAttribute.Cold; break;
                case 1071094: ele = AosElementAttribute.Poison; break;
                case 1071095: ele = AosElementAttribute.Energy; break;
            }

            return new EarringsOfProtection(ele);
        }

        public static Item ConstructRobe(Mobile m, StoreEntry entry)
        {
            return new HoodedBritanniaRobe(entry.ItemID);
        }

        public static Item ConstructMiniHouseDeed(Mobile m, StoreEntry entry)
        {
            int label = entry.Name[1].Number;

            switch (label)
            {
                default:
                    for (int i = 0; i < MiniHouseInfo.Info.Length; i++)
                    {
                        if (MiniHouseInfo.Info[i].LabelNumber == entry.Name[1].Number)
                        {
                            var type = (MiniHouseType)i;

                            return new MiniHouseDeed(type);
                        }
                    }
                    return null;
                case 1157015: return new MiniHouseDeed(MiniHouseType.TwoStoryWoodAndPlaster);
                case 1157014: return new MiniHouseDeed(MiniHouseType.TwoStoryStoneAndPlaster);
            }
        }

        public static Item ConstructRaisedGarden(Mobile m, StoreEntry entry)
        {
            var bag = new Bag();

            bag.DropItem(new RaisedGardenDeed());
            bag.DropItem(new RaisedGardenDeed());
            bag.DropItem(new RaisedGardenDeed());

            return bag;
        }

        public static Item ConstructLampPost(Mobile m, StoreEntry entry)
        {
            var item = new LampPost2
            {
                Movable = true,
                LootType = LootType.Blessed
            };

            return item;
        }

        public static Item ConstructForgedMetal(Mobile m, StoreEntry entry)
        {
            switch (entry.Name[1].Number)
            {
                case 1156686: return new ForgedMetalOfArtifacts(10);
                case 1156687: return new ForgedMetalOfArtifacts(5);
            }

            return null;
        }

        public static Item ConstructSoulstone(Mobile m, StoreEntry entry)
        {
            switch (entry.Name[0].Number)
            {
                case 1078835: return new SoulstoneToken(SoulstoneType.Blue);
                case 1078834: return new SoulstoneToken(SoulstoneType.Green);
                case 1158404: return new SoulstoneToken(SoulstoneType.Violet);
            }

            return null;
        }

        public static Item ConstructMerchantsTrinket(Mobile m, StoreEntry entry)
        {
            switch (entry.Name[0].Number)
            {
                case 1156827: return new MerchantsTrinket(false);
                case 1156828: return new MerchantsTrinket(true);
            }

            return null;
        }

        public static Item ConstructBOBCoverOne(Mobile m, StoreEntry entry)
        {
            return new BagOfBulkOrderCovers(12, 25);
        }

        public static Item ConstructBOBCoverTwo(Mobile m, StoreEntry entry)
        {
            return new BagOfBulkOrderCovers(1, 11);
        }

        public static Item ConstructHitchingPost(Mobile m, StoreEntry entry)
        {
            return new HitchingPost(false);
        }
        #endregion

        public static void AddPendingItem(Mobile m, Item item)
        {
            List<Item> list;

            if (!PendingItems.TryGetValue(m, out list))
            {
                PendingItems[m] = list = new List<Item>();
            }

            if (!list.Contains(item))
            {
                list.Add(item);
            }

            UltimaStoreContainer.DropItem(item);
        }

        public static bool HasPendingItem(PlayerMobile pm)
        {
            return PendingItems.ContainsKey(pm);
        }

        public static void CheckPendingItem(Mobile m)
        {
            List<Item> list;

            if (PendingItems.TryGetValue(m, out list))
            {
                var index = list.Count;

                while (--index >= 0)
                {
                    if (index >= list.Count)
                    {
                        continue;
                    }

                    var item = list[index];

                    if (item != null)
                    {
                        if (m.Backpack != null && m.Alive && m.Backpack.TryDropItem(m, item, false))
                        {
                            if (item is IPromotionalToken && ((IPromotionalToken)item).ItemName != null)
                            {
                                // A token has been placed in your backpack. Double-click it to redeem your ~1_PROMO~.
                                m.SendLocalizedMessage(1075248, ((IPromotionalToken)item).ItemName.ToString());
                            }
                            else if (item.LabelNumber > 0 || item.Name != null)
                            {
                                var name = item.LabelNumber > 0 ? ("#" + item.LabelNumber) : item.Name;

                                // Your purchase of ~1_ITEM~ has been placed in your backpack.
                                m.SendLocalizedMessage(1156844, name);
                            }
                            else
                            {
                                // Your purchased item has been placed in your backpack.
                                m.SendLocalizedMessage(1156843);
                            }

                            list.RemoveAt(index);
                        }
                    }
                    else
                    {
                        list.RemoveAt(index);
                    }
                }

                if (list.Count == 0 && PendingItems.Remove(m))
                {
                    list.TrimExcess();
                }
            }
        }

        public static List<StoreEntry> GetSortedList(string searchString)
        {
            var list = new List<StoreEntry>();

            list.AddRange(Entries.Where(e => Insensitive.Contains(GetStringName(e.Name), searchString)));

            return list;
        }

        public static string GetStringName(TextDefinition[] text)
        {
            var str = string.Empty;

            foreach (var td in text)
            {
                if (td.Number > 0 && VendorSearch.StringList != null)
                {
                    str += String.Format("{0} ", VendorSearch.StringList.GetString(td.Number));
                }
                else if (!String.IsNullOrWhiteSpace(td.String))
                {
                    str += String.Format("{0} ", td.String);
                }
            }

            return str;
        }

        public static string GetStringName(TextDefinition text)
        {
            var str = text.String;

            if (text.Number > 0 && VendorSearch.StringList != null)
            {
                str = VendorSearch.StringList.GetString(text.Number);
            }

            return str ?? String.Empty;
        }

        public static List<StoreEntry> GetList(StoreCategory cat)
        {
            return Entries.Where(e => e.Category == cat).ToList();
        }

        public static void SortList(List<StoreEntry> list, SortBy sort)
        {
            switch (sort)
            {
                case SortBy.Name:
                    list.Sort((a, b) => String.CompareOrdinal(GetStringName(a.Name), GetStringName(b.Name)));
                    break;
                case SortBy.PriceLower:
                    list.Sort((a, b) => a.Price.CompareTo(b.Price));
                    break;
                case SortBy.PriceHigher:
                    list.Sort((a, b) => b.Price.CompareTo(a.Price));
                    break;
                case SortBy.Newest:
                    break;
                case SortBy.Oldest:
                    list.Reverse();
                    break;
            }
        }

        public static int CartCount(Mobile m)
        {
            var profile = GetProfile(m, false);

            if (profile != null)
            {
                return profile.Cart.Count;
            }

            return 0;
        }

        public static int GetSubTotal(Dictionary<StoreEntry, int> cart)
        {
            if (cart == null || cart.Count == 0)
            {
                return 0;
            }

            var sub = 0.0;

            foreach (var kvp in cart)
            {
                sub += kvp.Key.Cost * kvp.Value;
            }

            return (int)sub;
        }

        public static int GetCurrency(Mobile m, bool sendMessage = false)
        {
            switch (Configuration.CurrencyImpl)
            {
                case CurrencyType.Sovereigns:
                    {
                        if (m is PlayerMobile)
                        {
                            return ((PlayerMobile)m).MoedasMagicas;
                        }
                    }
                    break;
                case CurrencyType.Gold:
                    return Banker.GetBalance(m);
                case CurrencyType.PointsSystem:
                    {
                        var sys = PointsSystem.GetSystemInstance(Configuration.PointsImpl);

                        if (sys != null)
                        {
                            return (int)Math.Min(Int32.MaxValue, sys.GetPoints(m));
                        }
                    }
                    break;
                case CurrencyType.Custom:
                    return Configuration.GetCustomCurrency(m);
            }

            return 0;
        }

        public static void TryPurchase(Mobile m)
        {
            var cart = GetCart(m);
            var total = GetSubTotal(cart);

            if (cart == null || cart.Count == 0 || total == 0)
            {
                // Purchase failed due to your cart being empty.
                m.SendLocalizedMessage(1156842);
            }
            else if (total > GetCurrency(m, true))
            {
                if (m is PlayerMobile)
                {
                    BaseGump.SendGump(new NoFundsGump((PlayerMobile)m));
                }
            }
            else
            {
                var subtotal = 0;
                var fail = false;

                var remove = new List<StoreEntry>();

                foreach (var entry in cart)
                {
                    for (var i = 0; i < entry.Value; i++)
                    {
                        if (!entry.Key.Construct(m))
                        {
                            fail = true;

                            try
                            {
                                using (var op = File.AppendText("UltimaStoreError.log"))
                                {
                                    op.WriteLine("Bad Constructor: {0}", entry.Key.ItemType.Name);

                                    Utility.WriteConsoleColor(ConsoleColor.Red, "[Ultima Store]: Bad Constructor: {0}", entry.Key.ItemType.Name);
                                }
                            }
                            catch
                            { }
                        }
                        else
                        {
                            remove.Add(entry.Key);

                            subtotal += entry.Key.Cost;
                        }
                    }
                }

                if (subtotal > 0)
                {
                    DeductCurrency(m, subtotal);
                }

                var profile = GetProfile(m);

                foreach (var entry in remove)
                {
                    profile.RemoveFromCart(entry);
                }

                if (fail)
                {
                    // Failed to process one of your items. Please check your cart and try again.
                    m.SendLocalizedMessage(1156853);
                }
            }
        }

        /// <summary>
        /// Should have already passed GetCurrency
        /// </summary>
        /// <param name="m"></param>
        /// <param name="amount"></param>
        public static int DeductCurrency(Mobile m, int amount)
        {
            switch (Configuration.CurrencyImpl)
            {
                case CurrencyType.Sovereigns:
                    {
                        if (m is PlayerMobile && ((PlayerMobile)m).WithdrawSovereigns(amount))
                        {
                            return amount;
                        }
                    }
                    break;
                case CurrencyType.Gold:
                    {
                        if (Banker.Withdraw(m, amount, true))
                        {
                            return amount;
                        }
                    }
                    break;
                case CurrencyType.PointsSystem:
                    {
                        var sys = PointsSystem.GetSystemInstance(Configuration.PointsImpl);

                        if (sys != null && sys.DeductPoints(m, amount, true))
                        {
                            return amount;
                        }
                    }
                    break;
                case CurrencyType.Custom:
                    return Configuration.DeductCustomCurrecy(m, amount);
            }

            return 0;
        }

        #region Player Persistence
        public static Dictionary<Mobile, PlayerProfile> PlayerProfiles { get; private set; }

        public static PlayerProfile GetProfile(Mobile m, bool create = true)
        {
            PlayerProfile profile;

            if ((!PlayerProfiles.TryGetValue(m, out profile) || profile == null) && create)
            {
                PlayerProfiles[m] = profile = new PlayerProfile(m);
            }

            return profile;
        }

        public static Dictionary<StoreEntry, int> GetCart(Mobile m)
        {
            var profile = GetProfile(m, false);

            if (profile != null)
            {
                return profile.Cart;
            }

            return null;
        }

        public static void OnSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(FilePath, Serialize);
        }

        public static void OnLoad()
        {
            Persistence.Deserialize(FilePath, Deserialize);
        }

        private static void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(_UltimaStoreContainer);

            writer.Write(PendingItems.Count);

            foreach (var kvp in PendingItems)
            {
                writer.Write(kvp.Key);
                writer.WriteItemList(kvp.Value, true);
            }

            writer.Write(PlayerProfiles.Count);

            foreach (var pe in PlayerProfiles)
            {
                pe.Value.Serialize(writer);
            }
        }

        private static void Deserialize(GenericReader reader)
        {
            reader.ReadInt();

            _UltimaStoreContainer = reader.ReadItem<UltimaStoreContainer>();

            var count = reader.ReadInt();

            for (var i = 0; i < count; i++)
            {
                var m = reader.ReadMobile();
                var list = reader.ReadStrongItemList<Item>();

                if (m != null && list.Count > 0)
                {
                    PendingItems[m] = list;
                }
            }

            count = reader.ReadInt();

            for (var i = 0; i < count; i++)
            {
                var pe = new PlayerProfile(reader);

                if (pe.Player != null)
                {
                    PlayerProfiles[pe.Player] = pe;
                }
            }
        }
        #endregion
    }

    internal class ninjashirt
    {
    }

    internal class AljavaNew
    {
    }

    [DeleteConfirm("This is the Ultima Store item display container. You should not delete this.")]
    public sealed class UltimaStoreContainer : Container
    {
        private static readonly List<Item> _DisplayItems = new List<Item>();

        public override bool Decays { get { return false; } }

        public override string DefaultName { get { return "Ultima Store Display Container"; } }

        public UltimaStoreContainer()
            : base(0) // No Draw
        {
            Movable = false;
            Visible = false;

            Internalize();
        }

        public UltimaStoreContainer(Serial serial)
            : base(serial)
        { }

        public void AddDisplayItem(Item item)
        {
            if (item == null)
            {
                return;
            }

            if (!_DisplayItems.Contains(item))
            {
                _DisplayItems.Add(item);
            }

            DropItem(item);
        }

        public Item FindDisplayItem(Type t)
        {
            var item = GetDisplayItem(t);

            if (item == null)
            {
                item = Loot.Construct(t);

                if (item != null)
                {
                    AddDisplayItem(item);
                }
            }

            return item;
        }

        public Item GetDisplayItem(Type t)
        {
            return _DisplayItems.FirstOrDefault(x => x.GetType() == t);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);

            writer.WriteItemList(_DisplayItems, true);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            reader.ReadInt();

            var list = reader.ReadStrongItemList();

            if (list.Count > 0)
            {
                Timer.DelayCall(o => o.ForEach(AddDisplayItem), list);
            }
        }
    }
}
