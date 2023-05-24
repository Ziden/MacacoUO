using System;
using System.Collections.Generic;

namespace Server
{
    public class Lang
    {
        private static Dictionary<String, String> ItemCache = new Dictionary<String, String>();
        private static HashSet<String> ItemIgnore = new HashSet<String>();

        public static string ItemTrans(string msg)
        {
            if (ItemIgnore.Contains(msg))
                return null;

            String v = null;
            ItemCache.TryGetValue(msg, out v);
            if (v != null)
                return v;
            v = _ItemTrans(msg);
            if (v != null)
            {
                ItemCache[msg] = v;
                return v;
            }
            else
            {
                ItemIgnore.Add(msg);
                return null;
            }
        }

        // Nome de items
        public static string _ItemTrans(string className)
        {
            switch (className)
            {
                case "BarrelStaves": return "Tábuas de Barril";
                case "BarrelLid": return "Tampa de Barril";
                case "ShortMusicStandLeft": return "Suporte para Partitura Esquerda";
                case "ShortMusicStandRight": return "Suporte para Partitura Direita";
                case "TallMusicStandLeft": return "Suporte alto para Partitura Esquerda";
                case "TallMusicStandRight": return "Suporte alto para Partitura Direita";
                case "EasleSouth": return "Cavalete Para Quadros Sul";
                case "EasleEast": return "Cavalete Para Quadros Leste";
                case "EasleNorth": return "Cavalete Para Quadros Norte";
                case "RedHangingLantern": return "Lanterna Pendurada Vermelha";
                case "BlankScroll": return "Pergaminhos em Branco";
                case "WhiteHangingLantern": return "Lanterna Pendurada Branca";

                case "ShojiScreen": return "Divisória Japonesa";
                case "Cloth": return "Pano";
                case "BambooScreen": return "Divisória de Bamboo";

                case "FishingPole": return "Vara de Pesca";

                case "WoodenContainerEngraver": return "Ferramentas para Entalhar na Madeira";

                case "IronIngot": return "Lingote de Ferro";

                case "RunedSwitch": return "Varinha encantada";

                case "EnchantedSwitch": return "Varinha";
                case "RunedPrism": return "Cristal Magico";
                case "JeweledFiligree": return "Brinco de Joias";

                case "ArcanistStatueSouthDeed": return "Escritura de Estatua Arcanista Sul";
                case "WarriorStatueSouthDeed": return "Escritura Estatua do Guerreiro Sul";



                case "WarriorStatueEastDeed": return "Escritura Estatua do Guerreiro Leste";



                case "SquirrelStatueSouthDeed": return "Escritura Estatua do Esquilo Sul";


                case "SquirrelStatueEastDeed": return "Escritura Estatua do Esquilo Leste";



                case "GiantReplicaAcorn": return "Replica Gigante de Noz";

                case "MountedDreadHorn": return "Trofeu Chifre do Medo";

                case "PristineDreadHorn": return "Chifre do Medo de Pristine";

                case "AcidProofRope": return "Corda a Prova de Acido";
                case "ProtectionScroll": return "Pergaminho de Proteção";


                case "SwitchItem": return "Pergaminho de Proteção";


                case "GargishBanner": return "Bandeira Extravagante";
                case "Incubator": return "Incubadora";

                case "ChickenCoop": return "Galinheiro";

                case "ExodusSummoningAlter": return "Escritura Altar da Invocação do Exodo";

                case "Granite": return "Granito";

                case "SmallPieceofBlackrock": return "Pequeno Pedaço de Rocha Negra";


                case "CraftableHouseItem": return "Item de Casa Customizavel";

                case "FootStool": return "Banquinho de pés";


                case "Stool": return "Banqueta";


                case "BambooChair": return "Cadeira de Bamboo";

                case "WoodenChair": return "Cadeira de Madeira";


                case "FancyWoodenChairCushion": return "Almofada para Cadeira de Madeira Chique";


                case "WoodenChairCushion": return "Almofada para Cadeira de Madeira";


                case "WoodenBench": return "Banco de Madeira";


                case "WoodenThrone": return "Trono de Madeira";


                case "Throne": return "Trono";


                case "Nightstand": return "Criado Mudo";


                case "WritingTable": return "Escrivaninha";


                case "LargeTable": return "Mesa Grande";


                case "YewWoodTable": return "Mesa de Madeira de Teixo";


                case "ElegantLowTable": return "Mesa Baixa Elegante";


                case "PlainLowTable": return "Mesa Baixa Simples";


                case "OrnateElvenTableSouthDeed": return "Escritura para Mesa Elfica Ornada Sul";

                case "OrnateElvenTableEastDeed": return "Escritura para Mesa Elfica Ornada Leste";

                case "FancyElvenTableSouthDeed": return "Escritura para Mesa Elfica Fancy Sul";

                case "FancyElvenTableEastDeed": return "Escritura para Mesa Elfica Fancy Leste";

                case "ElvenPodium": return "Podio Elfico";

                case "OrnateElvenChair": return "Cadeira Elfica Ornamentada";

                case "BigElvenChair": return "Cadeira Elfica Grande";

                case "ElvenReadingChair": return "Cadeira Elfica de Leitura";

                case "TerMurStyleChair": return "Cadeira Estilo Ter Mur";
                case "TerMurStyleTable": return "Mesa Estilo Ter Mur";

                case "UpholsteredChairDeed": return "Escritura para Cadeira Estofada";

                case "WoodenBox": return "Caixa de Madeira";

                case "SmallCrate": return "Caixa Pequena";

                case "MediumCrate": return "Caixa Media";

                case "LargeCrate": return "Caixa Grande";

                case "WoodenChest": return "Baú de Madeira";

                case "EmptyBookcase": return "Estante de livros";

                case "FancyArmoire": return "Armario Chique";

                case "Armoire": return "Armario";

                case "PlainWoodenChest": return "Baú de Madeira Simples";

                case "OrnateWoodenChest": return "Baú de Madeira Ornamentado";

                case "GildedWoodenChest": return "Baú de Madeira Dourado";

                case "WoodenFootLocker": return "Sapateiro de Madeirao";

                case "FinishedWoodenChest": return "Baú de Madeira Acabado";

                case "TallCabinet": return "Armario Alto";

                case "ShortCabinet": return "Armario Baixo";

                case "RedArmoire": return "Armario Vermelho";

                case "ElegantArmoire": return "Armario Elegante";

                case "MapleArmoire": return "Armario Bordo";

                case "CherryArmoire": return "Armario Cereja";

                case "Keg": return "Barril";

                case "BarrelHoops": return "Aros de Barril";

                case "ArcaneBookShelfDeedSouth": return "Escritura para Estante Misteriosa de Livros Sul";

                case "OrnateElvenChestSouthDeed": return "Escritura para Baú Ornamentado dos Elfos Sul";


                case "OrnateElvenChestEastDeed": return "Escritura para Baú Ornamentado dos Elfos Leste";


                case "ElvenWashBasinSouthWithDrawerDeed": return "Escritura para Lavatorio Elfico com Gavetas Sul";


                case "ElvenWashBasinEastWithDrawerDeed": return "Escritura para Lavatorio Elfico com Gavetas Leste";

                case "ElvenDresserDeedSouth": return "Escritura para Cômoda Elfica Sul";
                // MAGO PARO AQUI
                case "EnchantedApple": return "Maçã Encantada";
                case "SackFlour": return "Saco de Farinha";
                case "WheatSheaf": return "Feixe de Trigo";
                case "Dough": return "Massa";
                case "SackFlourOpen": return "Saco de Farinha Aberto";
                case "BaseBeverage": return "Bebida Base";
                case "SweetDough": return "Massa Doce";
                case "JarHoney": return "Jarro de Mel";
                case "CakeMix": return "Mistura para Bolo";
                case "CookieMix": return "Mistura para Biscoitos";
                case "CocoaButter": return "Manteiga de Cacau";
                case "CocoaLiquor": return "Licor de Cacau";
                case "EmptyPewterBowl": return "Tigela de Estanho Vazia";
                case "WheatWort": return "Licor de Trigo";
                case "FishOilFlask": return "Frasco de Óleo de Peixe";
                case "RawFishSteak": return "Filé de Peixe Cru";
                case "FreshSeasoning": return "Tempero Fresco";
                case "Salt": return "Sal";
                case "UnbakedQuiche": return "Quiche Cru";
                case "Eggs": return "Ovos";
                case "UnbakedMeatPie": return "Torta de Carne Crua";
                case "RawRibs": return "Costelas Cruas";
                case "UncookedSausagePizza": return "Pizza de Linguiça Crua";
                case "Sausage": return "Linguiça";
                case "UncookedCheesePizza": return "Pizza de Queijo Crua";
                case "CheeseWheel": return "Roda de Queijo";
                case "UnbakedFruitPie": return "Torta de Frutas Crua";
                case "Pear": return "Pera";
                case "UnbakedPeachCobble": return "Torta de Pera Crua";
                case "UnbakedApplePie": return "Torta de Maça Crua";
                case "Apple": return "Maçã";
                case "UnbakedPumpkinPie": return "Torta de Abóbora Crua";
                case "Pumpkin": return "Abóbora";
                case "GreenTea": return "Chá verde";
                case "WasabiClumps": return "Bolinhos de Wasabi";
                case "WoodenBowlOfPeas": return "Tigela de Ervilhas";
                case "SushiRolls": return "Rolos de Sushi";
                case "SushiPlatter": return "Prato de Sushi";
                case "TribalPaint": return "Tinta Tribal";
                case "TribalBerry": return "Fruta Tribal";
                case "EggBomb": return "Bomba de Ovo";
                case "ParrotWafer": return "Bolacha para Papagaios";
                case "PlantPigment": return "PlantaPigmento";
                case "Bottle": return "Garrafa";
                case "NaturalDye": return "Tinta Natural";
                case "ColorFixative": return "Fixador de Cores";
                case "SilverSerpentVenom": return "Veneno de Serpente Prateada";
                case "WoodPulp": return "Polpa da Madeira";
                case "Charcoal": return "Carvão";
                case "Bait": return "Isca";
                case "SamuelsSecretSauce": return "Tempero Secreto do Samuel";
                case "SaltedSerpentSteaks": return "Filé de Serpente Salteado";
                case "BreadLoaf": return "Pão";
                case "Cookies": return "Biscoitos";
                case "Cake": return "Bolo";
                case "Muffins": return "Bolinhos";
                case "Quiche": return "Quiche";
                case "MeatPie": return "Torta de Carne";
                case "SausagePizza": return "Pizza de Linguiça";
                case "CheesePizza": return "Pizza de Queijo";
                case "FruitPie": return "Torta de Frutas";
                case "PeachCobbler": return "Torta de Pêssego";
                case "UnbakedPeachCobbler": return "Torta de Pêssego Crua";
                case "ApplePie": return "Torta de Maçã";
                case "PumpkinPie": return "Torta de Abóbora";
                case "MisoSoup": return "Sopa de Missô";
                case "WhiteMisoSoup": return "Sopa de Missô Branco";
                case "RedMisoSoup": return "Sopa de Missô Vermelho";
                case "AwaseMisoSoup": return "Sopa de Awase Missô";
                case "GingerBreadCookie": return "Biscoito de Pão de Gengibre";
                case "FreshGinger": return "Gengibre Fresco";
                case "ThreeTieredCake": return "Bolo de Três Andares";
                case "CookedBird": return "Pássaro Cozido";
                case "RawBird": return "Pássaro Cru";
                case "ChickenLeg": return "Coxa de Frango";
                case "RawChickenLeg": return "Coxa de Frango Crua";
                case "FishSteak": return "Filé de peixe";
                case "FriedEggs": return "Ovos Fritos";
                case "LambLeg": return "Perna de Cordeiro";
                case "RawLambLeg": return "Perna de Cordeiro Crua";
                case "Ribs": return "Costelas";
                case "BowlOfRotwormStew": return "Tigela de Sopa de Vermes Podres";
                case "RawRotwormMeat": return "Carne de Vermes Podres";
                case "BowlOfBlackrockStew": return "Tigela de Sopa de Rocha Negra";
                case "KhaldunTastyTreat": return "Guloseima de Khaldun";
                case "Hamburger": return "Hamburger";
                case "Lettuc": return "Alface";
                case "HotDog": return "Cachorro Quente";
                case "CookableSausage": return "Linguiça Cozinhável";
                case "DriedHerbs": return "Ervas secas";
                case "GrilledSerpentSteak": return "Filé de Serpente Grelhado";
                case "BBQDinoRibs": return "Churrasco De Costa de Dinossauro";
                case "SackOfSugar": return "Saco de Açúcar";
                case "WakuChicken": return "Frango Assado";
                case "FoodEngraver": return "Gravador de Comidas";
                case "GreaterHealPotion": return "Poção de Vida Maior";
                case "GreaterStrengthPotion": return "Poção de Força Maior";
                case "FruitBowl": return "Tigela de Frutas";
                case "Banana": return "Banana";
                case "SweetCocoaButter": return "Doce de Manteiga de Cacau";
                case "DarkChocolate": return "Chocolate Preto";
                case "MilkChocolate": return "Chocolate ao Leite";
                case "WhiteChocolate": return "Chocolate Branco";
                case "Vanill": return "Vanilha";
                case "ChocolateNutcracker": return "Quebra Nozes de Chocolate";
                case "CocoaLiquo": return "Licor de Chocolate";
                case "GreatBarracudaPie": return "Torta de Grande Barracuda";
                case "MentoSeasoning": return "Temperto de Menta";
                case "GiantKoiPie": return "Torta de Carpa Gigante";
                case "GiantKoiSteak": return "Filé de Carpa Gigante";
                case "RecallScroll": return "Pergaminho de Recall";
                case "FireFishPie": return "Torta de Peixe Fogo";
                case "FireFishSteak": return "Filé de Peixe Fogo";
                case "Carrot": return "Cenoura";
                case "StoneCrabPie": return "Torta de Carangueijo de Pedra";
                case "StoneCrabMeat": return "Carne de Carangueijo de Pedra";
                case "Cabbage": return "Repolho";
                case "BlueLobsterPie": return "Torta de Lagosta Azul";
                case "ReaperFishPie": return "Torta de Peixe Ceifador";
                case "CrystalFishPie": return "Torta de Peixe Cristal";
                case "BullFishPie": return "Torta de Peixe Boi";
                case "BullFishSteak": return "Filé de Peixe Boi";
                case "Squash": return "Abóbora de Pescoço";
                case "SummerDragonfishPie": return "Torta de Peixe Dragão de Verão";
                case "SummerDragonfishSteak": return "Filé de Peixe Dragão de Verão";
                case "Onion": return "Cebola";
                case "FairySalmonPie": return "Torta de Salmão Fada";
                case "FairySalmonSteak": return "Filé de Salmão Fada";
                case "EarOfCorn": return "Espiga de Milho";
                case "DarkTruffle": return "Trufa Negra";
                case "LavaFishPie": return "Torta de Peixe Lava";
                case "LavaFishSteak": return "Filé de Peixe Lava";
                case "AutumnDragonfishPie": return "Torta de Peixe Dragão de Outono";
                case "AutumnDragonfishStea": return "Filé de Peixe Dragão de Outono";
                case "SpiderCrabPie": return "Torta de Carangueijo-aranha";
                case "SpiderCrabMeat": return "Caranguejo-aranha";
                case "SpidersSilk": return "Spider Silk";
                case "GraveDust": return "Grave Dust";
                case "MandrakeRoot": return "Mandrake Root";
                case "Bloodmoss": return "Blood Moss";
                case "SulfurousAsh": return "Sulfurous Ash";
                case "BatWing": return "Bat Wing";
                case "DaemonBlood": return "Daemon Blood";
                case "PigIron": return "Pig Iron";
                case "NoxCrystal": return "Nox Crystal";
                case "Lettuce": return "Alface";
                case "YellowtailBarracudaPie": return "Torta de Barracuda de Rabo Amarela";
                case "YellowtailBarracudaSteak": return "Filé de Barracuda de Rabo Amarela";
                case "HolyMackerelPie": return "Torta de Cavala Santa";
                case "HolyMackerelSteak": return "Filé de Cavala Santa";
                case "UnicornFishPie": return "Torta de Peixe Unicórnio";
                case "UnicornFishSteak": return "Filpe de Peixe Unicórnio";
                case "CoffeeMug": return "Caneca de café";
                case "CoffeeGrounds": return "Grãos de café";
                case "BasketOfGreenTeaMug": return "Caneca de Chá Verde";
                case "GreenTeaBasket": return "Cesta de Chá Verde";
                case "HotCocoaMug": return "Caneca de Cacau Quente";

                case "GozaMatEastDeed": return "Deed de Tatame Leste";
                case "GozaMatSouthDeed": return "Deed de Tatame Sul";
                case "SquareGozaMatEastDeed": return "Deed de Tatame Quadrado Leste";
                case "SquareGozaMatSouthDeed": return "Deed de Tatame Quadrado Sul";
                case "BrocadeGozaMatEastDeed": return "Deed de Tatame Brocado Leste";
                case "BrocadeGozaMatSouthDeed": return "Deed de Tatame Brocado Sul";
                case "BrocadeSquareGozaMatEastDeed": return "Deed de Tatame Brocado Quadrado Leste";
                case "BrocadeSquareGozaMatSouthDeed": return "Deed de Tatame Brocado Quadrado Sul";
                case "SquareGozaMatDeed": return "Deed de Tatame Quadrado";
                case "DeerMask": return "Máscara de Veado";
                case "BearMask": return "Máscara de Urso";
                case "OrcMask": return "Máscara de Orc";
                case "TribalMask": return "Máscara Tribal";
                case "HornedTribalMask": return "Máscara Tribal com Chifres";
                case "CuffsOfTheArchmage": return "Algemas do Arquimago";
                case "UncutCloth": return "Tecido";
                case "AbyssalCloth": return "Pano Abissal";
                case "CutUpCloth": return "Pano Cortado";
                case "BoltOfCloth": return "Rolo de Pano";
                case "CombineCloth": return "Pano Combinado";
                case "PowderCharge": return "Carga de Pólvora";
                case "BlackPowder": return "Pó preto";
                case "CrystallineBlackrock": return "Rocha Negra Cristalina";
                case "SkullCap": return "Bandana Pirata";
                case "Bandana": return "Bandana";
                case "FloppyHat": return "Chapéu de Disquete";
                case "Cap": return "Boné";
                case "WideBrimHat": return "Chapéu de Aba Larga";
                case "StrawHat": return "Chapéu de Palha";
                case "TallStrawHat": return "Chapéu de Palha Alto";
                case "WizardsHat": return "Chapéu de Mago";
                case "Bonnet": return "Gorro";
                case "FeatheredHat": return "Chapéu Emplumado";
                case "TricorneHat": return "Chapéu de Pirata";
                case "JesterHat": return "Chapéu de Palhaço";
                case "FlowerGarland": return "Guirlanda de Flores";
                case "ClothNinjaHood": return "Balaclava Ninja de Pano";
                case "Kasa": return "Kasa";
                case "ChefsToque": return "Chapéu de Cozinheiro";
                case "KrampusMinionHat": return "Chapéu de Lacaio do Krampus";
                case "AssassinsCowl": return "Capuz de Assassino";
                case "Leather": return "Couro";
                case "VileTentacles": return "Tentáculo Vil";
                case "MagesHood": return "Capuz de Mago";
                case "VoidCore": return "Núcleo Vazio";
                case "CowlOfTheMaceAndShield": return "Capuz da Maça e Escudo";
                case "MaceAndShieldGlasses": return "Óculo da Maça e Escudo";
                case "MagesHoodOfScholarlyInsight": return "Capuz de Mago do Conhecimento Acadêmico";
                case "TheScholarsHalo": return "Aréola do Acadêmico";
                case "Doublet": return "Gibão";
                case "FancyShirt": return "Camisa Chique";
                case "Tunic": return "Túnica com Mangas";
                case "Surcoat": return "Túnica";
                case "PlainDress": return "Vestido Simples";
                case "FancyDress": return "Vestido Caprichado";
                case "Cloak": return "Capa";
                case "Robe": return "Sobretudo";
                case "JesterSuit": return "Traje de Palhaço";
                case "FurCape": return "Capa de Pele";
                case "GildedDress": return "Vestido Dourado";
                case "FormalShirt": return "Camisa Forma";
                case "ClothNinjaJacket": return "Camisa Ninja de Pano";
                case "Kamishimo": return "Kamishimo";
                case "HakamaShita": return "HakamaShita";
                case "MaleKimono": return "Kimono Masculino";
                case "FemaleKimono": return "Kimono Feminino";
                case "JinBaori": return "JinBaori";
                case "ShortPants": return "Calças curtas";
                case "LongPants": return "Calças longas";
                case "Kilt": return "Kilt";
                case "Skirt": return "Saia";
                case "FurSarong": return "Sarongue de Pele";
                case "Hakama": return "Hakama";
                case "TattsukeHakama": return "Tattsuke Hakama";
                case "ElvenShirt": return "Camisa Élfica";
                case "ElvenDarkShirt": return "Camisa Élfica Escura";
                case "ElvenPants": return "Calças Élficas";
                case "MaleElvenRobe": return "Sobretudo Élfico Masculino";
                case "FemaleElvenRobe": return "Sobretudo Élfico Feminino";
                case "WoodlandBelt": return "Cinto da Floresta";
                case "GargishRobe": return "Sobretudo Gárgula";
                case "GargishFancyRobe": return "Sobretudo Gárgula Caprichado";
                case "RobeofRite": return "Robe de Ritual";
                case "GoldDust": return "Pó de ouro";
                case "GuildedKilt": return "Kilt Dourado";
                case "CheckeredKilt": return "Kilt Xadrez";
                case "FancyKilt": return "Kilt Caprichado";
                case "FloweredDress": return "Vestido";
                case "EveningGown": return "Vestido de noite";
                case "BodySash": return "Faixa";
                case "HalfApron": return "Avental";
                case "FullApron": return "Avental Grande";
                case "Obi": return "Obi";
                case "ElvenQuiver": return "Aljava Élfica";
                case "QuiverOfFire": return "Aljava de Fogo";
                case "QuiverOfIce": return "Aljava de Gelo";
                case "WhitePearl": return "Pérola Branca";
                case "QuiverOfBlight": return "Aljava da Ruína";
                case "QuiverOfLightning": return "Aljava do Trovão";
                case "Corruption": return "Corrupção";
                case "LeatherContainerEngraver": return "Ferramenta de Gravação de Contêiner de Couro";
                case "SpoolOfThread": return "Linhas de Coser";
                case "Dyes": return "Tintas";
                case "GargoyleHalfApron": return "Avental Gárgula";
                case "GargishSash": return "Faixa Gárgula";
                case "OilCloth": return "Pano de Óleo";
                case "MaceBelt": return "Bainha de Maça";
                case "Lodestone": return "Magnetita";
                case "SwordBelt": return "Bainha de Espada";
                case "DaggerBelt": return "Bainha de Adaga";
                case "ElegantCollar": return "Colar Elegante";
                case "FeyWings": return "Asas de Fada";
                case "CrimsonMaceBelt": return "Bainha Carmesim de Maça";
                case "CrimsonCincture": return "Cinto Carmesim";
                case "CrimsonSwordBelt": return "Bainha Carmesim de Espada";
                case "CrimsonDaggerBelt": return "Bainha Carmesim de Adaga";
                case "ElegantCollarOfFortune": return "Colar Elegante da Fortuna";
                case "ElvenBoots": return "Botas Élficas";
                case "FurBoots": return "Botas de Pele";
                case "NinjaTabi": return "Botas Ninja";
                case "SamuraiTabi": return "Botas Samurai";
                case "Sandals": return "Sandálias";
                case "Shoes": return "Sapatos";
                case "Boots": return "Botas";
                case "ThighBoots": return "ThighBoots";
                case "LeatherTalons": return "Garras de Couro";
                case "JesterShoes": return "Sapato de Palhaço";
                case "KrampusMinionBoots": return "Botas de Lacaio do Krampus";
                case "KrampusMinionTalons": return "Garras de Lacaio do Krampus";
                case "Putrefaction": return "Putrefação";
                case "Scourge": return "Flagelo";
                case "Muculent": return "Muculento";
                case "StitchersMittens": return "Luvas de Costura";
                case "LeatherGorget": return "Gorgel de Couro";
                case "LeatherCap": return "Capuz de Couro";
                case "LeatherGloves": return "Luvas de Couro";
                case "LeatherArms": return "Cotoveleira de Couro";
                case "LeatherLegs": return "Calças de Couro";
                case "LeatherChest": return "Tunica de Couro";
                case "LeatherJingasa": return "Jingasa de Couro";
                case "LeatherMempo": return "Mempo de Couro";
                case "LeatherDo": return "Armadura Samurai de Couro";
                case "LeatherHiroSode": return "Cotoveleira Samurai de Couro";
                case "LeatherSuneate": return "Botas Samurai de Couro";
                case "LeatherHaidate": return "Calça Samurai de Couro";
                case "LeatherNinjaPants": return "Calça Ninja de Couro";
                case "LeatherNinjaJacket": return "Jaqueta Ninja de Couro";
                case "LeatherNinjaBelt": return "Cinto Ninja de Couro";
                case "LeatherNinjaMitts": return "Luvas Ninja de Couro";
                case "LeatherNinjaHood": return "Capuz Ninja de Couro";
                case "LeafChest": return "Túnica de Folha";
                case "LeafArms": return "Cotoveleira de Folha";
                case "LeafGloves": return "Luvas de Folha";
                case "LeafLegs": return "Calça de Folha";
                case "LeafGorget": return "Gorgel de Folha";
                case "LeafTonlet": return "Saia de Folha";
                case "GargishLeatherArms": return "Cotoveleira de Couro Gárgula";
                case "GargishLeatherChest": return "Tunica de Couro Gárgula";
                case "GargishLeatherLegs": return "Calças de Couro Gárgula";
                case "GargishLeatherKilt": return "Kilt de Couro Gárgula";
                case "FemaleGargishLeatherArms": return "Cotoveleira de Couro Gárgula Feminino";
                case "FemaleGargishLeatherChest": return "Tunica de Couro Gárgula Ferminino";
                case "FemaleGargishLeatherLegs": return "Calças de Couro Gárgula Feminino";
                case "FemaleGargishLeatherKilt": return "Kilt de Couro Gárgula Feminino";
                case "GargishLeatherWingArmor": return "Armadura de Couro de Asa Gárgula";
                case "TigerPeltChest": return "Túnica de Couro de Tigre";
                case "TigerPelt": return "Couro de Tigre";
                case "TigerPeltLegs": return "Calça de Couro de Tigre";
                case "TigerPeltShorts": return "Bermuda de Couro de Tigre";
                case "TigerPeltHelm": return "Elmo de Couro de Tigre";
                case "TigerPeltCollar": return "Colar de Couro de Tigre";
                case "DragonTurtleHideChest": return "Túnica de Couro de Tartaruga-Dragão";
                case "DragonTurtleHideLegs": return "Calça de Couro de Tartaruga-Dragão";
                case "DragonTurtleHideHelm": return "Elmo de Couro de Tartaruga-Dragão";
                case "DragonTurtleHideArms": return "Cotoveleira de Couro de Tartaruga-Dragão";
                case "GargishClothArmsArmor": return "Cotoveleira de Pano Gárgula";
                case "GargishClothChestArmor": return "Tunica de Pano Gárgula";
                case "GargishClothLegsArmor": return "Calças de Pano Gárgula";
                case "GargishClothKiltArmor": return "Kilt de Pano Gárgula";
                case "FemaleGargishClothArmsArmor": return "Cotoveleira de Pano Gárgula Feminino";
                case "FemaleGargishClothChestArmor": return "Tunica de Pano Gárgula Ferminino";
                case "FemaleGargishClothLegsArmor": return "Calças de Pano Gárgula Feminino";
                case "FemaleGargishClothKiltArmor": return "Kilt de Pano Gárgula Feminino";
                case "GargishClothWingArmor": return "Armadura de Pano de Asa Gárgula";
                case "StuddedGorget": return "Gorgel de Couro Reforçado";
                case "StuddedGloves": return "Luvas de Couro Reforçado";
                case "StuddedArms": return "Cotoveleira de Couro Reforçado";
                case "StuddedLegs": return "Calça de Couro Reforçado";
                case "StuddedChest": return "Túnica de Couro Reforçado";
                case "StuddedMempo": return "Mempo de Couro Reforçado";
                case "StuddedDo": return "Armadura Samurai de Couro Reforçado";
                case "StuddedHiroSode": return "Cotoveleira Samurai de Couro Reforçado";
                case "StuddedSuneate": return "Botas Samurai de Couro Reforçado";
                case "StuddedHaidate": return "Calça Samurai de Couro Reforçado";
                case "HideChest": return "Túnica Élfica de Couro Reforçado";
                case "HidePauldrons": return "Cotoveleira Élfica de Couro Reforçado";
                case "HideGloves": return "Luva Élfica de Couro Reforçado";
                case "HidePants": return "Calça Élfica de Couro Reforçado";
                case "HideGorget": return "Gorgel Élfica de Couro Reforçado";
                case "LeatherShorts": return "Bermuda de Couro";
                case "LeatherSkirt": return "Saia de Couro";
                case "LeatherBustierArms": return "Sutiã de Couro";
                case "StuddedBustierArms": return "Sutiça de Couro Reforçado";
                case "FemaleLeatherChest": return "Armadura Feminina de Couro";
                case "FemaleStuddedChest": return "Armadura Feminina de Couro Reforçado";
                case "TigerPeltBustier": return "Sutiã de Couro de Tigre";
                case "TigerPeltLongSkirt": return "Saia Longa de Couro de Tigre";
                case "TigerPeltSkirt": return "Saia de Couro de Tigre";
                case "DragonTurtleHideBustier": return "Sutiã de Couro de Tartaruga-Dragão";
                case "BoneHelm": return "Elmo de Ossos";
                case "BoneGloves": return "Luva de Ossos";
                case "BoneArms": return "Braços de Ossos";
                case "BoneLegs": return "Calça de Ossos";
                case "BoneChest": return "Túnica de Ossos";
                case "OrcHelm": return "Elmo Orc";
                case "Bone": return "Osso";
                case "MidnightBracers": return "Braçadeira da Meia Noite";
                case "Saphire":
                    return "Safira";
                case "Diamond":
                    return "Diamante";
                case "Ring":
                    return "Anel";
                case "Necklace":
                    return "Colar";
                case "Bracelet":
                    return "Bracelete";
                case "Earrings":
                    return "Brincos";
                case "StarSaphire":
                    return "Safira Estrela";
                case "AnvilForge": return "Forja com bigorna";
                case "BaseArtifactLight": return "Luz do artefato base";
                case "BaseDecayingItem": return "Decaimento de item básico";
                case "BaseDoor": return "Porta básica";
                case "BaseSwitch": return "Alavanca básica";
                case "BaseTrap": return "Armadilha básica";
                case "BedlamAltar": return "Altar da loucura";
                case "BlightedGroveAltar": return "Altar do bosque apodrecido";
                case "BulletinBoards": return "Quadro de avisos";
                case "Campfire": return "Fogueira de acampamento";
                case "ChickenLizardEgg": return "Ovo de lagarto galinha";
                case "CircuitTrapTrainingKit": return "Kit de treinamento para armadilhas de circuito";
                case "CitadelAltar": return "Altar da cidadela";
                case "CoralTheOwl": return "A coruja Coral";
                case "CylinderTrapTrainingKit": return "Kit de treinamento para aramdilhas de cilindro";
                case "DeceitBrazier": return "Braseiro do enganador";
                case "DespiseAnkh": return "Vida para o detestável";
                case "Doors": return "Portas";
                case "DragonTurtleEgg": return "Ovo de Tartaruga-Dragão";
                case "EtherealRetouchingTool": return "Ferramenta do retoque eterno";
                case "FarmableCabbage": return "Repolho cultivável";
                case "FarmableCarrot": return "Cenoura cultivável";
                case "FarmableCotton": return "Algodão cultivável";
                case "FarmableCrop": return "Colheita cultivável";
                case "FarmableFlax": return "Linho cultivável";
                case "FarmableLettuce": return "Alface cultivável";
                case "FarmableOnion": return "Cebola cultivável";
                case "FarmablePumpkin": return "Abóbora cultivável";
                case "FarmableTurnip": return "Nabo cultivável";
                case "FarmableWheat": return "Trigo cultivável";
                case "FireColumnTrap": return "Armadilha Coluna de fogo";
                case "FlameSpurtTrap": return "Armadilha Jato de chama";
                case "GamblingStone": return "Pedra de apostas";
                case "GasTrap": return "Armadilha de gás";
                case "GiantSpikeTrap": return "Armadilha Espinho gigante";
                case "GoblinFloorTrap": return "Armadilha de chão do goblin";
                case "Grinder": return "Moedor";
                case "HouseCraftables": return "Construiveis de casa";
                case "HouseDoors": return "Portas de casa";
                //case "HouseRaffleStone": return "XXXXX";
                case "LargeForge": return "Forja grande";
                case "MushroomTrap": return "Armadilha de cogumelos";
                case "ParoxysmusAltar": return "Altar do descontrole";
                case "Portcullis": return "Porticulo";
                case "PowerGenerator": return "Gerador de energia";
                case "PrimevalLichPuzzle": return "Quebra-cabeça do Lich ancião";
                case "PrismOfLightAltar": return "Altar de prisma da luz";
                case "PublicMoongate": return "Moongate público";
                case "SawTrap": return "Armadilha de serra";
                case "ScaleCollar": return "Colar de escamas";
                case "SecretDoors": return "Portas secretas";
                case "SerpentNest": return "Ninho de serpente";
                case "SerpentPillar": return "Pilar da serpente";
                case "SerpentsJawbone": return "Mandíbula de serpente";
                case "SliderTrapTrainingKit": return "Kit de treinamento da armadilha deslizante";
                case "SlidingDoors": return "Portas corrediças";
                case "SpiderWebbing": return "Teias de aranha";
                case "SpikeTrap": return "Armadilha de espeto";
                case "StealableArtifactsSpawner": return "Criador de artefatos roubáveis";
                case "StoneFaceTrap": return "Armadilha cara de pedra";
                case "StrangeContraption": return "Aparelho estranho";
                case "Switches": return "Interruptores";
                case "TwistedWealdAltar": return "Altar da mata retorcida";
                case "WallSafe": return "Parede segura";
                case "2018HolidayGiftToken": return "Moeda do natal de 2018";
                case "2019HolidayGiftToken": return "Moeda do natal de 2019";
                case "AlterContract": return "Alterar contrato";
                case "AnimateDeadScroll": return "Pergaminho para animar os mortos";
                case "AnimatedWeaponScroll": return "Pergaminho para animar as armas";
                case "AnniversaryPromotionalToken": return "Moeda promocional de aniversário";
                case "Asian": return "Asiatíco";
                case "BankCheck": return "Cheque bancário";
                case "BarkeepContract": return "Contrato com barman";
                case "BaseMagicalFood": return "Comida mágica básica";
                case "BaseReagent": return "Reagente básico";
                case "BaseRewardTitleDeed": return "Escritura do título da recompensa básico";
                case "BaseRewardTitleScroll": return "Pergaminho do título de recompensa básico";
                case "BaseWaterContainer": return "Recipente de água básico";
                case "Bedroll": return "Saco para dormir";
                case "Beverage": return "Bebida";
                case "BeverageEmpty": return "Bebida vazia";
                case "Bola": return "Boleadeira";
                case "Bowls": return "Tigela";
                case "Bucket": return "Balde";
                case "Cheese": return "Queijo";
                case "Chocolatiering": return "XXX";
                case "ClothingBlessDeed": return "Escritura para abençoar vestuário";
                case "CommissionPlayerVendorDeed": return "Escritura para vendedor";
                case "CommodityDeed": return "Escritura de mercadorias";
                case "CommunicationCrystals": return "Cristais de comunicação";
                case "CookableFood": return "Comida cozinhavel";

                case "Cooking": return "Cozinhando";
                case "CreepyCake": return "Bolo assustador";
                case "Dates": return "Tâmaras";
                case "DeliciouslyTastyTreat": return "Comida saborosa dos animais";
                case "DragonBardingDeed": return "Escritura da armadura do dragão do pantano";

                case "ElixirOfRebirth": return "Elixir do renascimento";
                case "EmbroideryTool": return "Ferramenta de bordado";

                case "EndlessDecanter": return "Garrafa infinita";
                case "EngravingTools": return "Ferramentas de gravação";
                case "FearEssence": return "Essência do medo";
                case "Firebomb": return "Bomba de fogo";

                case "Food": return "Comida";
                case "ForgedPardon": return "Pergaminho do perdão forjado";
                case "FrenchBread": return "Pão francês";

                case "Fruits": return "Frutas";
                case "FukiyaDarts": return "Dardos de sopro";
                case "GemMiningBook": return "Livro para mineração de gemas de qualidade";
                case "GlassblowingBook": return "Livro de sopro de vidro";
                case "Gold": return "Ouro";
                case "GoldFoil": return "Folha de ouro";
                case "GorgonLens": return "Olhos da Gorgon";
                case "GreenThorns": return "Espinho verde";

                //case "GrizzledMareStatuette": return "XXX";

                case "HairDye": return "Tintura para cabelo";
                case "HairRestylingDeed": return "Escritura para corte de cabelo";
                case "HarvestWine": return "Vinho da colheita";
                case "HealingStone": return "Pedra da cura";
                case "HeritageToken": return "Token da herança";
                case "HitchingRope": return "Corda resistente";
                case "HolidayFoods": return "Comida de natal";
                case "HolidayTreeDeed": return "Escritura da árvore de natal";
                case "ElvenFletching":
                    return "Flecheiro Élfico";
                case "Feather":
                    return "Pena";
                case "Kindling":
                    return "Graveto";
                case "Board":
                    return "Tábua";
                case "FaeryDust":
                    return "Pó de Fada";
                case "Arrow":
                    return "Flecha";
                case "Bolt":
                    return "Dardo";
                case "Crossbow":
                    return "Besta";
                case "HeavyCrossbow":
                    return "Besta Pesada";
                case "CompositeBow":
                    return "Arco Composto";
                case "RepeatingCrossbow":
                    return "Besta Repetidora";
                case "Yumi":
                    return "Yumi";
                case "ElvenCompositeLongbow":
                    return "Arco Composto Élfico";
                case "MagicalShortbow":
                    return "Arco Curto Mágico";
                case "BlightGrippedLongbow":
                    return "Arco Longo da Praga";
                case "LardOfParoxysmus":
                    return "Banha de Paroxysmus";
                case "Blight":
                    return "Praga";
                case "FaerieFire":
                    return "Fogo de Fada";
                case "Taint":
                    return "Mancha";
                case "SilvanisFeywoodBow":
                    return "Bow Silvanis Feywood";
                case "MischiefMaker":
                    return "Causacdor de Dano";
                case "DreadHornMane":
                    return "Chifre Medonho";
                case "TheNightReaper":
                    return "Ceifador Noturno";
                case "BarbedLongbow":
                    return "Arco Longo Farbado";
                case "FireRuby":
                    return "Rubi de Fogo";
                case "SlayerLongbow":
                    return "Arco Longo do Matador";
                case "BrilliantAmber":
                    return "Âmbar Brilhante";
                case "FrozenLongbow":
                    return "Arco Longo Congelante";
                case "Turquoise":
                    return "Turquesa";
                case "LongbowOfMight":
                    return "Arco Longo do Poder";
                case "BlueDiamond":
                    return "Diamante Azul";
                case "RangersShortbow":
                    return "Arco Curto de Guarda";
                case "PerfectEmerald":
                    return "Esmeralda Perfeita";
                case "LightweightShortbow":
                    return "Arco Curto Leve";

                case "MysticalShortbow":
                    return "Arco Curto Místico";
                case "EcruCitrine":
                    return "Citrino Ecru";
                case "AssassinsShortbow":
                    return "Arco Curto do Assassino";
                case "DarkSapphire":
                    return "Safira Negra";
                case "OakBoard":
                    return "Tábua de Carvalho";
                case "AshBoard":
                    return "Tábua de Cinza";
                case "YewBoard":
                    return "Tábua de Yew";
                case "HeartwoodBoard":
                    return "Tábua de Madeira de Lareira";
                case "BloodwoodBoard":
                    return "Tábua de Madeira de Sangue";
                case "FrostwoodBoard":
                    return "Tábua de Madeira de Geada";

                //case "HouseRaffleDeed": return "XXX";

                case "IrresistiblyTastyTreat": return "Comida irresistível dos animais";
                case "ItemBlessDeed": return "Escritura para abençoar item";
                case "Jellybeans": return "Jujubas";
                case "Lantern.": return "Candeeiro";
                case "LiquorPitcher": return "Jarro de licor";
                case "LockPick": return "LockPick";
                case "Backpack": return "Mochila";
                case "Lollipops": return "Pirulito";
                case "MasonryBook": return "Livro de alvenaria";
                case "MelisandesFermentedWine": return "Vinho fermentado da Melisande";
                case "MelisandesHairDye": return "Tinta da cor do cabelo da Melisande";
                case "MessageInABottle": return "Garrafa com uma mensagem";
                case "MrPlainsCookies": return "Biscoito redondo";
                case "MurkyMilk": return "Leite seboso";
                case "NameChangeDeed": return "Escritura para mudança de nome";
                case "NewPlayerTicket": return "Escritura De Noob";
                case "NougatSwirl": return "Torrone em espiral";
                case "Nutcrackers": return "Quebra-nozes";
                case "OrangePetals": return "Pétalas de laranja";

                case "PersonalAttendantDeed": return "Contrato com um atendente pessoal";
                case "PersonalAttendantToken": return "Token de atendente pessoal";
                case "PersonalBlessDeed": return "Escritura para abençoar um jogador";
                case "Pizza": return "Pizza";
                case "PlayerVendorDeed": return "Contrato com um vendedor";
                case "PoppiesDust": return "Poeira de papoilas";
                case "PowderOfTemperament": return "Pó para melhoria de durabilidade";
                case "PowderOfTranslocation": return "Pó de translocação";
                case "PowerScroll": return "Pergaminho do poder";
                case "PromotionalToken": return "Token promocional";
                case "PumpkinPizza": return "Pizza de abobora";
                case "RecipeScroll": return "Pergaminho de receita";
                case "RedLeaves": return "Folhas vermelhas";
                case "RepairDeed": return "Contrato de reparo";
                case "RewardCake": return "Bolo de comemoração";
                case "SOS": return "SOS";
                case "Sake": return "Saquê";
                case "SandMiningBook": return "Livro para mineração na areia";
                case "ScrollBinderDeed": return "Escritura para pergaminho de ligação";
                case "ScrollofAlacrity": return "Pergaminho da velocidade";
                case "ScrollofTranscendence": return "Pergaminho da transcendência";
                case "Shirt": return "Camisa";
                case "Shuriken": return "Shuriken";
                case "SkinTingeingTincture": return "Tintura para tingir a pele";
                case "SmokeBomb": return "Bomba de fumaça";
                case "SoulStone": return "Pedra da alma";
                case "SoulstonePromotionalTokens": return "Token promocional da pedra da alma";
                case "SpecialBeardDye": return "Tintura especial para barbas";
                case "SpecialFishingNet": return "Rede de pesca especial";
                case "SpecialHairDye": return "Tintura especial para cabelos";
                case "SpecialScroll": return "Pergaminho especial";
                case "StatScroll": return "Pergaminho de status";
                case "Stew": return "Ensopado";
                case "StoneMiningBook": return "Livro para minerar pedras de qualidade";
                case "Taffy": return "Caramelo";
                case "TastyTreat": return "Comida gostosa dos animais";
                case "TaxidermyKit": return "Kit de taxidermia";
                case "ThanksGivingFood": return "Comida de ação de graças";

                case "Turnip": return "Nabo";
                case "ValentineChocolate": return "Chocolate dos namorados";
                case "Vegetables": return "Vegetais";
                case "VendorRentalContract": return "Contraro de aluguel com um vendedor";
                case "WaterBarrel": return "Barril de água";
                case "WaterPitcher": return "Jarro de água";
                case "WaterTub": return "Banheira";
                case "Wine": return "Vinho";
                case "WineJar": return "Jarro de vinho";
                case "WrappedCandy": return "Doce embrulhado";
                case "WrathGrapes": return "Vinhas da ira";

                //Vegetais
                case "Lemon": return "Limão";
                case "Lime": return "Lima";
                case "Grapes": return "Uvas";
                case "Peach": return "Pêssego";

                case "Watermelon": return "Melancia";

                case "Plum": return "Ameixa";

                //Carnes
                case "SlabOfBacon": return "Pedaço de Bacon";

                case "Ham": return "Presunto";

                case "RoastPig": return "Porco Assado";

                //Instrumentos Musicais
                case "AudChar": return "Violoncelo";
                case "Drums": return "Bateria";
                case "Harp": return "Harpa";
                case "LapHarp": return "Harpa Pequena";
                case "Lute": return "Alaúde";
                case "Tambourine": return "Pandeiro";
                case "TambourineTassel": return "Tamborim";

                //Escamas
                case "YellowScales": return "Escamas Amarelas";
                case "BlueScales": return "Escamas Azuis";
                case "GreenScales": return "Escamas Verdes";
                case "RedScales": return "Escamas Vermelhas";
                case "WhiteScales": return "Escamas Árticas";
                case "BlackScales": return "Escamas Negras";

                //Tecido


                //Garrafa
                case "CrystalAltar": return "Altar de cristal";
                case "CrystalBeggarStatue": return "Estátua de cristal do necessitado";
                case "CrystalBrazier": return "Braseiro de cristal";
                case "CrystalBullStatue": return "Estátua de cristal de touro";
                case "CrystalRunnerStatue": return "Estátua de cristal do corredor";
                case "CrystalSupplicantStatue": return "Estátua de cristal do suplicante";
                case "CrystalTable": return "Mesa de cristal";
                case "CrystalThrone": return "Trono de cristal";

                case "DawnsMusicBox": return "Caixa de música do amanhecer";
                case "DawnsMusicGear": return "Engrenagem para caixa do amanhecer";
                case "DawnsMusicInfo": return "Informações sobre a caixa de música do amanhecer";

                case "15thAnniversaryLithograph": return "Gravura do 15º anivérsario";
                case "AlchemistsBookshelf": return "Estante de livros dos alquimistas";
                case "BarrelSponge": return "Esponja de barril";
                case "BirdLamp": return "Lâmpada de pássaro";
                case "BullTapestry": return "Tapeçaria de touro";
                case "CactusSponge": return "Esponja de cacto";
                case "CastlePainting": return "Quadro do castelo de Britain";
                case "DragonLantern": return "Lanterna do dragão";
                case "EmbroideredTapestry": return "Tapeçaria bordada";
                case "FirePainting": return "Quadro do elemental de fogo";
                case "FluffySponge": return "Esponja macia";
                case "FourPostBed": return "Cama elegante";
                case "GoldenTable": return "Mesa dourada";
                case "HorsePainting": return "Quadro do Bucéfalo";
                case "KoiLamp": return "Lâmpada do dourado";
                case "MarbleTable": return "Mesa de marmore";
                case "MonasteryBell": return "Sino do monastério";
                case "OrnateBed": return "Cama ornamentada";
                case "ShelfSponge": return "Mesa dourada";
                case "ShipPainting": return "Quadro do navio real";
                case "TallLamp": return "Lâmpada alta";
                case "TheTokenBox": return "Caixa da coleção real";

                case "FireDemonStatue": return "Estátua do demônio de fogo";
                case "GlobeOfSosaria": return "Globo de sosaria";
                case "ObsidianPillar": return "Pilar de obisidiana";
                case "ObsidianRock": return "Pedra de obisidiana";
                case "ShadowAltar": return "Altar das sombras";
                case "ShadowBanner": return "Faixa das sombras";
                case "ShadowFirePit": return "Fogueira das sombras";
                case "ShadowPillar": return "Pilar das sombras";
                case "SpikeColumn": return "Coluna de espinhos";
                case "SpikePost": return "Estaca de espinhos";

                case "AddonToolComponenet": return "Compomente de ferramenta";
                case "AlchemyStation": return "Estação de alquimia";
                case "BBQSmoker": return "Churrasqueira";
                case "CraftAddon": return "Construir";
                case "CraftAddonDeed": return "Escritura para construir";
                case "EnchantedSculptingTool": return "Ferramenta encantada para esculpir";
                case "FletchingStation": return "Estação do arqueiro";
                case "GlassKiln": return "Forno de vidro";

                // Addons marotos
                case "SewingMachine": return "Máquina de costura";
                case "SmithingPress": return "Prensa do ferreiro";
                case "SpinningLathe": return "Torno de fiação";
                case "WritingDesk": return "Mesa do escritor";


                case "Hotdog": return "Cachorro Quente";

                case "GrapesOfWrath": return "Uvas da Ira";

                case "BagOfSugar": return "Saco de Açúcar";
                case "Sugarcane": return "Cana de Açúcar";
                case "CocoaPulp": return "Polpa de Cacau";
                case "Batter": return "Mistura Batida";
                case "Butter": return "Manteiga";
                case "Cream": return "Creme";
                case "CookingOil": return "Óleo de Cozinha";
                case "Milk": return "Leite";
                case "Peanut": return "Amendoim";
                case "Vinegar": return "Vinagre";
                case "BottleOfWin": return "Garrafa de Vinho";
                case "BarbecueSauce": return "Molho de Churrasco";
                case "Tomato": return "Tomate";
                case "BasketOfHerbsFarm": return "Cesta de Ervas da Fazenda";
                case "Herbs": return "Ervas";
                case "CheeseSauce": return "Molho de Queijo";
                case "EnchiladaSauce": return "Molhe de Enchilada";
                case "ChiliPepper": return "Pimenta";
                case "Gravy": return "Molho de Carne";
                case "HotSauce": return "Molho Picante";
                case "SoySauce": return "Molho de Soja";
                case "BagOfSoy": return "Saco de Soja";
                case "Teriyaki": return "Teriyaki";
                case "BottleOfWine": return "Garrafa de vinho";
                case "TomatoSauce": return "Molho de tomate";
                case "VenisonSteak": return "Bife de Veado";
                case "RawVenisonSteak": return "Carne de Veado Crua";
                case "VenisonJerky": return "Charque de Veado";
                case "RawVenisonSlice": return "Fatia de Veado Crua";
                case "VenisonRoast": return "Veado Assado";
                case "RawVenisonRoast": return "Assado de Veado Cru";
                case "BeefPorterhouse": return "Filé";
                case "RawBeefPorterhouse": return "Filé Cru";
                case "BeefPrimeRib": return "Costela de Primeira";
                case "RawBeefPrimeRib": return "Costela de Primeira Crua";
                case "BeefRibeye": return "Ancho";
                case "RawBeefRibeye": return "Ancho Cru";
                case "BeefRibs": return "Costela";
                case "RawBeefRibs": return "Costela Crua";
                case "BeefRoast": return "Bife Assado";
                case "RawBeefRoast": return "Assado de Bife Cru";
                case "BeefSirloin": return "Lombo";
                case "RawBeefSirloin": return "Lombo Cru";
                case "BeefJerky": return "Carne Ceca";
                case "RawBeefSlice": return "Fatia de Bife Cru";
                case "BeefTBone": return "T-Bone";
                case "RawBeefTBone": return "T-Bone Cru";
                case "BeefTenderloin": return "Contrafilé";
                case "RawBeefTenderloin": return "Contrafilé Cru";
                case "GroundBeef": return "Carne moída";
                case "BeefHock": return "Jarrete";
                case "GoatSteak": return "Carne de Bode";
                case "RawGoatSteak": return "Carne de Bode Crua";
                case "GoatRoast": return "Bode Assado";
                case "RawGoatRoast": return "Assado de Bode Cru";
                case "RawGroundPork": return "Carne de porco moída";
                case "PorkHock": return "Jarrete de porco";
                case "Bacon": return "Bacon";
                case "RawBacon": return "Bacon Cru";
                case "BaconSlab": return "Fatia de Bacon";
                case "RawBaconSlab": return "Fatia de Bacon Crua";

                case "RawHam": return "Presunto Cru";
                case "HamSlices": return "Presunto Fatiado";
                case "RawHamSlices": return "Presunto Fatiado Cru";
                case "PigHead": return "Cabeça de Porco";
                case "RawPigHead": return "Cabeça de Porco Crua";
                case "PorkChop": return "Costelinha de Porco";
                case "RawPorkChop": return "Costelinha de Porco Crua";
                case "PorkRoast": return "Porco Assado";
                case "RawPorkRoast": return "Assado de Porco Cru";
                case "PorkSpareRibs": return "Costela de Porco";
                case "RawSpareRibs": return "Costela de Porco Crua";
                case "Trotters": return "Pés";
                case "RawTrotters": return "Pés Crus";
                case "RawPorkSlice": return "Fatia de Porco Crua";
                case "MuttonSteak": return "Bife de Carneiro";
                case "RawMuttonSteak": return "Bife de Carneiro Cru";
                case "MuttonRoast": return "Carneiro Assado";
                case "RawMuttonRoast": return "Assado de Carneiro Cru";
                case "RoastChicken": return "Frango Assado";
                case "RoastTurkey": return "Peru Assado";
                case "RawTurkey": return "Peru Cru";
                case "TurkeyLeg": return "Coxa de Peru";
                case "RawTurkeyLeg": return "Coxa de Peru Crua";
                case "TurkeyPlatter": return "Travessa de Peru";
                case "FoodPlate": return "Prato de Comida";
                case "SlicedTurkey": return "Peru Fatiado";
                case "TurkeyHock": return "Jarrete de Peru";
                case "RoastDuck": return "Pato Assado";
                case "DuckLeg": return "Coxa de Pato";
                case "RawDuckLeg": return "Coxa de Pato Crua";
                case "CookedSteak": return "Bife Cozido";
                case "PastaNoodles": return "Macarrão";
                case "PeanutButter": return "Manteiga de Amendoim";
                case "FruitJam": return "Geléia de Frutas";
                case "FruitBasket": return "Cesto de Frutas";
                case "Tortilla": return "Tortilha";
                case "BagOfCornmeal": return "Saco de Fubá";
                case "BarkFragment": return "Pedaço de Casca";
                case "ParrotWafers": return "Bolacha para Papagaios";
                case "PlantClippings": return "Cortes de Plantas";
                case "Log": return "Madeira";
                case "DriedOnions": return "Cebola em Pó";
                case "Garlic": return "Garlic";
                case "Ginseng": return "Ginseng";
                case "TanGinger": return "Gengibre";
                case "ChocolateMix": return "Mistura de Chocolate";
                case "BagOfCocoa": return "Saco de Cacau";
                case "AsianVegMix": return "Mistura de Vegetais Asiáticos";
                case "RedMushroom": return "Cogumelo Vermelho";
                case "MixedVegetables": return "Vegetais Misturados";
                case "Potato": return "Batata";
                case "Celery": return "Salsão";
                case "PizzaCrust": return "Borda da Pizza";
                case "WaffleMix": return "Mistura de Waffle";
                case "BowlCornFlakes": return "Tigela de Flocos de Milho";
                case "BowlRiceKrisps": return "Tigela de Arroz Crocante";
                case "BagOfRicemeal": return "Saco de Farinha de Arroz";

                case "Cherry": return "Cereja";
                case "Tofu": return "Tofu";
                case "PieMix": return "Mistura de Torta";
                case "GarlicBread": return "Pão de Alho";
                case "BananaBread": return "Pão de Banana";
                case "PumpkinBread": return "Pão de Abóboda";
                case "CornBread": return "Pão de Milho";
                case "AlmondCookies": return "Biscoitos de Amêndoa";
                case "Almond": return "Amêndoa";
                case "ChocChipCookies": return "Biscoitos de Chocolate";
                case "GingerSnaps": return "Salgados de Gengibre";
                case "OatmealCookies": return "Biscoitos de Aveia";
                case "BagOfOats": return "Saco de Aveia";
                case "PeanutButterCookies": return "Biscoitos de Manteiga de Amendoim";
                case "PumpkinCookies": return "Biscoitos de Abóbora";
                case "BananaCake": return "Bolo de Banana";
                case "CarrotCake": return "Bolo de Cenoura";
                case "ChocolateCake": return "Bolo de Chocolate";
                case "CoconutCake": return "Bolo de Coco";
                case "Coconut": return "Coco";
                case "LemonCake": return "Bolo de Limão";

                case "BlueberryMuffins": return "Muffins de Mirtilo";
                case "Blueberry": return "Mirtilo";
                case "PumpkinMuffins": return "Muffins de Abóbora";
                case "HamPineapplePizza": return "Pizza de Presunto com Abacaxi";
                case "UncookedPizza": return "Pizza Crua";
                case "Pineapple": return "Abacaxi";
                case "MushroomOnionPizza": return "Pizza de Cogumelo e Cebola";
                case "TanMushroom": return "Cogumelo";
                case "SausOnionMushPizza": return "Pizza de Linguiça com Cebola e Cogumelos";
                case "TacoPizza": return "Pizza de Taco";
                case "VeggiePizza": return "Pizza Vegetariana";
                case "BlueberryPie": return "Torta de Mirtilo";
                case "CherryPie": return "Torta de Cereja";
                case "KeyLimePie": return "Torta de Limão";

                case "LemonMerenguePie": return "Torta de Merengue de Limão";
                case "BlackberryCobbler": return "Bolo de Amora";
                case "Blackberry": return "Amora";
                case "ShepherdsPie": return "Escondidinho";
                case "BowlMashedPotatos": return "Tigela de Purê de Batata";
                case "Corn": return "Milho";
                case "TurkeyPie": return "Torta de Peru";
                case "ChickenPie": return "Torta de Frango";
                case "BeefPie": return "Torta de Carne";
                case "Brownies": return "Brownies";
                case "ChocSunflowerSeeds": return "Semente de Girassol";
                case "EdibleSun": return "Girassol Comestível";
                case "RiceKrispTreat": return "Doce de Arroz Crocante";
                case "BowlOatmeal": return "Tigela de Aveia";
                case "Popcorn": return "Pipoca";
                case "Pancakes": return "Panquecas";
                case "Waffles": return "Waffles";
                case "ChickenNoodleSoup": return "Sopa de Macarrão com Frango";
                case "TomatoRice": return "Arroz com Tomate";
                case "BowlRice": return "Tigela de Arroz";
                case "BowlOfStew": return "Tigela de Ensopado";
                case "BowlCookedVeggies": return "Tigela de Vegetais Cozidos";
                case "TomatoSoup": return "Sopa de Tomate";
                case "HarpyEggSoup": return "Sopa de Ovos de Harpia";
                case "HarpyEggs": return "Ovos de Harpia";
                case "BowlBeets": return "Tigela de Beterraba";
                case "Beet": return "Beterraba";
                case "BowlBroccoli": return "Tigela de Brócolis";
                case "Broccoli": return "Brócolis";
                case "BowlCauliflower": return "Tigela de Couve-flor";
                case "Cauliflower": return "Couve-flor";
                case "BowlGreenBeans": return "Tigela de Feijão Verde";
                case "GreenBean": return "Feijão Verde";
                case "BowlSpinach": return "Tigela de Espinafre";
                case "Spinach": return "Espinafre";
                case "BowlTurnips": return "Tigela de Nabo";

                case "WoodenBowlCabbage": return "Repolho na Tigela de Madeira";
                case "WoodenBowlCarrot": return "Cenoura na Tigela de Madeira";
                case "WoodenBowlCorn": return "Milho na Tigela de Madeira";
                case "WoodenBowlLettuce": return "Alface na Tigela de Madeira";
                case "WoodenBowlPea": return "Ervilha na Tigela de Madeira";
                case "Peas": return "Ervilhas";
                case "PewterBowlOfPotatos": return "Batatas na Tigela de Estanho";
                case "CornOnCob": return "Sabugo de Milho";
                case "GroundPork": return "Carne de Porco Moída";
                case "Hotwings": return "Asinhas de Frango";
                case "PotatoFries": return "Batatas Fritas";
                case "TacoShell": return "Concha de Taco";
                case "BroccoliCheese": return "Brócolis com Queijo";
                case "BroccoliCaulCheese": return "Brócolis e Couve-flor com Queijo";
                case "CauliflowerCheese": return "Couve-flor com Queijo";
                case "Meatballs": return "Almôndegas";
                case "Meatloaf": return "Rolo de Carne";
                case "PotatoStrings": return "Fios de Batata";
                case "HalibutFishSteak": return "Filé de Peixe Habilut";
                case "RawHalibutSteak": return "Peixe Habilut Cru";
                case "FlukeFishSteak": return "Filé de Peixe Fluke";
                case "RawFlukeSteak": return "Peixe Fluke Cru";
                case "MahiFishSteak": return "Filé de Peixe Mahi";
                case "RawMahiSteak": return "Peixe Mahi Cru";
                case "SalmonFishSteak": return "Filé de Salmão";
                case "RawSalmonSteak": return "Salmão Cru";
                case "RedSnapperFishSteak": return "Filé de Cioba";
                case "RawRedSnapperSteak": return "Cioba Cru";
                case "ParrotFishSteak": return "Filé de Peixe Papagaio";
                case "RawParrotFishSteak": return "Peixe Papagaio Cru";
                case "TroutFishSteak": return "Filé de Truta";
                case "RawTroutSteak": return "Truta Crua";
                case "CookedShrimp": return "Camarão Cozido";
                case "RawShrimp": return "Camarão Cru";
                case "ChickenParmesian": return "Frango à Parmegiana";
                case "CheeseEnchilada": return "Enchilada de Queijo";
                case "ChickenEnchilada": return "Enchilada de Frango";
                case "Lasagna": return "Lasanha";
                case "LemonChicken": return "Frango com Limão";
                case "OrangeChicken": return "Frango com Laranja";
                case "Orange": return "Laranja";
                case "VealParmesian": return "Vitela à Parmegiana";
                case "BeefBBQRibs": return "Churrasco de Costela";
                case "BeefBroccoli": return "Filé com Brócolis";
                case "ChoChoBeef": return "Salada de Carne";
                case "BeefSnowpeas": return "Filé com Vagem";
                case "SnowPeas": return "Vagem";
                case "BeefLoMein": return "Bife Lo Mein";
                case "BeefStirfry": return "BeefStirfry";
                case "ChickenStirfry": return "Frango Frito";
                case "MooShuPork": return "Porco Moo Shu";
                case "MoPoTofu": return "Mabu Tofu";
                case "PorkStirfry": return "Carne De Porco Frita";
                case "SweetSourChicken": return "Frango Agridoce";
                case "SweetSourPork": return "Porco Agridoce";
                case "BaconAndEgg": return "Bacon com Ovos";
                case "Spaghetti": return "Espaguete";
                case "MacaroniCheese": return "Macarrão com Queijo";
                case "Vanilla": return "Baunilha";

                case "EmptyWoodenBowl": return "Tigela de Madeira Vazia";
                case "FishOil": return "Óleo de peixe";

                case "ForkLeft": return "Garfo Esquerda";
                case "ForkRight": return "Garfo Direita";
                case "SpoonLeft": return "Colher Esquerda";
                case "SpoonRight": return "Colher Direita";
                case "KnifeLeft": return "Faca Esquerda";
                case "KnifeRight": return "Faca Direita";
                case "Plate": return "Prato";
                case "Goblet": return "Cálice";
                case "PewterMug": return "Caneca de Estanho";
                case "KeyRing": return "Chaveiro";
                case "Candelabra": return "Candelabro";
                case "Scales": return "Balança";
                case "Key": return "Chave";
                case "Globe": return "Globo";
                case "Spyglass": return "Monóculo";
                case "Lantern": return "Lanterna";
                case "HeatingStand": return "Suporte de Aquecimento";
                case "BroadcastCrystal": return "Cristal de Transmissão";
                case "TerMurStyleCandelabra": return "Candelabro Estilo de Ter-Mur";
                case "GorgonLense": return "Lentes Górgonas";
                case "MedusaLightScales": return "Escamas de Medusa Claras";
                case "MedusaDarkScales": return "Escamas de Medusa Negras";
                case "SoftenedReeds": return "Juncos Amaciados";
                case "DryReeds": return "Juncos Secos";
                case "KotlAutomatonHead": return "Cabeça de Autômato Kotl";
                case "CrystalDust": return "Pó de Cristal";
                case "BaseIngot": return "Lingote Base";
                case "Silver": return "Prata";
                case "RingOfTheElements": return "Anel dos Elementos";
                case "HatOfTheMagi": return "Chapéu do Magi";
                case "AutomatonActuator": return "Atuador de Autômato";
                case "BlackrockMoonstone": return "Rocha Negra de Pedra da Lua";
                case "GoldRing": return "Anel de Ouro";
                case "SilverBeadNecklace": return "Colar de Gotas de Prata";
                case "GoldNecklace": return "Colar de ouro";
                case "GoldEarrings": return "Brincos de Ouro";
                case "GoldBeadNecklace": return "Colar de Gotas de Ouro";
                case "GoldBracelet": return "Pulseira de Ouro";
                case "GargishNecklace": return "Colar Gárgula";
                case "GargishBracelet": return "Bracelete Gárgula";
                case "GargishRing": return "Anel Gárgula";
                case "GargishEarrings": return "Brincos Gárgula";
                case "StarSapphire": return "Safira Estrela";
                case "Emerald": return "Esmeralda";
                case "Sapphire": return "Safira";
                case "Ruby": return "Rubi";
                case "Citrine": return "Citrino";
                case "Amethyst": return "Ametista";
                case "Tourmaline": return "Turmalina";
                case "Amber": return "Âmbar";
                case "KrampusMinionEarrings": return "Brincos do Lacaio do Krampus";
                case "Nunchaku": return "Nunchaku";
                case "JointingPlane": return "Plaina";
                case "MouldingPlane": return "Plaina de Moldagem";
                case "SmoothingPlane": return "Plaina de Suaviação";
                case "ClockFrame": return "Armação de Relógio";
                case "Axle": return "Eixo";
                case "RollingPin": return "Rolo de Massa";
                case "Ramrod": return "Remo";
                case "Swab": return "Esfregão";
                case "ScouringToxin": return "Toxina de Limpeza";
                case "RoundBasket": return "Cesto Redondo";
                case "Shaft": return "Vareta";
                case "RoundBasketHandles": return "Alças de Cesta Redonda";
                case "SmallBushel": return "Pequena Cesta";
                case "PicnicBasket2": return "Cesto de Piquenique";
                case "WinnowingBasket": return "Cesto de Peneirar";
                case "SquareBasket": return "Cesto Quadrado";
                case "BasketCraftable": return "Cesto Craftável";
                case "TallRoundBasket": return "Cesto Alto e Redondo";
                case "SmallSquareBasket": return "Pequeno Cesto Quadrado";
                case "TallBasket": return "Cesto Alto";
                case "SmallRoundBasket": return "Pequeno Cesto Redondo";
                case "EnchantedPicnicBasket": return "Cesto de Piquenique Encantado";
                case "Scissors": return "Tesouras";
                case "MortarPestle": return "Morteiro e Pilão";
                case "Scorp": return "Formão";
                case "TinkerTools": return "Ferramentas de Funileiro";
                case "Hatchet": return "Machadinha";
                case "DrawKnife": return "Faca de Entalhar";
                case "SewingKit": return "Kit de Costura";
                case "Saw": return "Serra";
                case "DovetailSaw": return "Serra de Cauda de Andorinha";
                case "Froe": return "Froe";
                case "Shovel": return "Pá";
                case "Hammer": return "Martelo";
                case "Tongs": return "Alicate";
                case "SmithyHammer": return "Martelo de Ferreiro";
                case "SmithHammer": return "Martelo de Ferreiro";
                case "SledgeHammerWeapon": return "Martelo de Forja";
                case "SledgeHammer": return "Martelo de Forja";
                case "Inshave": return "Formão Circular";
                case "Pickaxe": return "Picareta";
                case "Lockpick": return "Lockpick";
                case "Skillet": return "Frigideira";
                case "FlourSifter": return "Peneira de Farinha";
                case "FletcherTools": return "Ferramentas de Fazer Flechas";
                case "MapmakersPen": return "Caneta de Cartógrafos";
                case "ScribesPen": return "Caneta de Escriba";
                case "Clippers": return "Tesoura de Poda";
                case "MetalContainerEngraver": return "Marcador de Conteneres de Metal";
                case "Springs": return "Molas";
                case "Gears": return "Engrenagens";
                case "Pitchfork": return "Forcado";
                case "ClockParts": return "Partes de Relógio";
                case "BarrelTap": return "Torneira de Barril";
                case "SextantParts": return "Partes de Sextante";
                case "Hinge": return "Dobradiça";
                case "BolaBall": return "BolaBall";
                case "ButcherKnife": return "Faca de Açougueiro";
                case "Cleaver": return "Cutelo";
                case "SkinningKnife": return "Faca de Esfolar";
                case "GargishCleaver": return "Cutelo Gárgula";
                case "GargishButcherKnife": return "Faca de Açougueiro Gárgula";
                case "ShojiLantern": return "Lanterna Shoji";
                case "PaperLantern": return "Lanterna de Papel";
                case "RoundPaperLantern": return "Lanterna Redonda de Papel";
                case "WindChimes": return "Espanta Espíritos";
                case "FancyWindChimes": return "Sinos de Vento";
                case "Matches": return "Tocha de Canhão";
                case "Matchcord": return "Corda de Tocha de Canhão";
                case "CopperWire": return "Fio de cobre";
                case "DragonLamp": return "Lâmpada de Dragão";
                case "WorkableGlass": return "Vidro Trabalhável";
                case "StainedGlassLamp": return "Lâmpada de Vitral";
                case "TallDoubleLamp": return "Lâmpada Dupla Alta";
                case "CraftableMetalHouseDoor": return "Porta de Casa Fabricável";
                case "WallSafeDeed": return "Escritura de Cofre de Parede";
                case "KotlPowerCore": return "Núcleo de Força de Kotl";
                case "MoonstoneCrystalShard": return "Fragmentos de Cristal de Pedra da Lua";
                case "WeatheredBronzeGlobeSculptureDeed": return "Escritura de Escultura Desgastada de Globo de Bronze";
                //case "BronzeIngot": return "Lingotes de Bronze";
                case "WeatheredBronzeManOnABenchDeed": return "Escritura de Escultura Desgastada de Homem Sentado em um Banco";
                case "WeatheredBronzeFairySculptureDeed": return "Escritura de Escultura Desgastada de Fada de Bronze";
                case "WeatheredBronzeArcherDeed": return "Escritura de Escultura Desgastada de Arqueiro de Bronze";
                case "AxleGears": return "Engrenagens de Eixo";
                case "ClockRight": return "Relógio Direita";
                case "ClockLeft": return "Relógio Esquerda";
                case "Sextant": return "Sextante";
                case "PotionKeg": return "Barril de Poções";
                case "ModifiedClockworkAssembly": return "Conjunto de Mecânico Modificado";
                case "ClockworkAssembly": return "Conjunto Mecânico";
                case "PowerCrystal": return "Cristal de Poder";
                case "VoidEssence": return "Essência do Vazio";
                case "Rope": return "Corda";
                case "ResolvesBridle": return "Corda de Freio";
                case "HitchingPost": return "Poste de Amarrar";
                case "AnimalPheromone": return "Feromônio Animal";
                case "PhillipsWoodenSteed": return "Estatueta de Cavalo de Madeira";
                case "ArcanicRuneStone": return "Pedra Rúnica Arcana";
                case "CrystalShards": return "Fragmentos de Cristal";
                case "VoidOrb": return "Orbe do Vazio";
                case "BlackPearl": return "Black Pearl";
                case "AdvancedTrainingDummySouthDeed": return "Escritura de Manequim de Treinamento Avançado Sul";
                case "TrainingDummySouthDeed": return "Escritura de Manequim de Treinamento Sul";
                case "PlateChest": return "Peitoral de Metal";
                case "CloseHelm": return "Elmo Fechado";
                case "AdvancedTrainingDummyEastDeed": return "Escritura de Manequim de Treinamento Avançado Leste";
                case "TrainingDummyEastDeed": return "Escritura de Manequim de Treinamento Leste";
                case "DistillerySouthAddonDeed2": return "Escritura de Addon de Destilaria Sul";
                case "MetalKeg": return "Barril de Metal";
                case "DistilleryEastAddonDeed2": return "Escritura de Addon de Destilaria Leste";
                case "StasisChamberPowerCore": return "Câmara de Estase do Núcleo de Força";
                case "InoperativeAutomatonHead": return "Cabeça de Autômato Inoperante";
                case "DartTrapCraft": return "Armadilha de Dardos";
                case "PoisonTrapCraft": return "Armadilha de Veneno";
                case "BasePoisonPotion": return "Poção de Veneno Base";
                case "ExplosionTrapCraft": return "Armadilha de Explosão";
                case "BaseExplosionPotion": return "Poção de Explosão Base";
                case "FactionGasTrapDeed": return "Escritura de Armadilha de Gás de Facção";
                case "FactionExplosionTrapDeed": return "Escritura de Armadilha de Explosão de Facção";
                case "FactionSawTrapDeed": return "Escritura de Armadilha de Serra de Facção";
                case "FactionSpikeTrapDeed": return "Escritura de Armadilha de Espetos de Facção";
                case "FactionTrapRemovalKit": return "Kit de Remoção de Armadilhas de Facção";
                case "BrilliantAmberBracelet": return "Bracelete de Âmbar Brilhante";
                case "FireRubyBracelet": return "Bracelete de Rubi de Fogo";
                case "DarkSapphireBracelet": return "Bracelete de Safira Negra";
                case "WhitePearlBracelet": return "Bracelete de Pérola Branca";
                case "EcruCitrineRing": return "Anél de Citrino Marrom";
                case "BlueDiamondRing": return "Anél de Diamante Azul";
                case "PerfectEmeraldRing": return "Anél de Esmeralda Perfeita";
                case "TurqouiseRing": return "Anél de Turquesa";
                case "ResilientBracer": return "Bracadeira da Resiliencia";
                case "CapturedEssence": return "Essência Capturada";
                case "EssenceOfBattle": return "Essência da Batalha";
                case "PendantOfTheMagi": return "Pingente do Magi";
                case "EyeOfTheTravesty": return "O Olho do Travesty";
                case "DrSpectorsLenses":
                    return "Lentes do Dr' Spector";
                case "BraceletOfPrimalConsumption": return "Bracelete do Consumo Primitivo";
                case "BloodOfTheDarkFather": return "Sangue do Pai da Escuridão";
                /*
                case "CopperIngot": return "Lingotes de Cobre";
                case "SilverIngot": return "Lingotes de Prata";
                case "NiobioIngot": return "Lingotes de Niobio";
                case "LazuritaIngot": return "Lingotes de Lazurita";
                case "QuartzoIngot": return "Lingotes de Quartzo";
                case "BeriloIngot": return "Lingotes de Berilo";
                case "VibraniumIngot": return "Lingotes de Vibranium";
                case "AdamantiumIngot": return "Lingotes de Adamantium";
                */
            }
            return null;
        }

        private static Dictionary<int, String> Cache = new Dictionary<int, String>();
        private static HashSet<int> Ignore = new HashSet<int>();

        public static string Trans(int msg)
        {
            if (Ignore.Contains(msg))
                return null;

            String v = null;
            Cache.TryGetValue(msg, out v);
            if (v != null)
                return v;
            v = _TransCliloc(msg);
            if (v != null)
            {
                Cache[msg] = v;
                return v;
            }
            else
            {
                Ignore.Add(msg);
                return null;
            }

        }

        // Frases do clicloc 
        public static string _TransCliloc(int msg)
        {
            switch (msg)
            {
                // GUMP TEXTS
                case 1011429:
                    return "Liderado por";
                case 1011457:
                    return "Impostos";
                case 1011428:
                    return "Eleicoes";
                case 1011430:
                    return "Estado da Cidade";
                case 1005561:
                    return "Voce eh um criminoso e nao pode fazer isto";
                case 1011447:
                    return "Voltar";
                case 1044457:
                    return "Materiais";
                case 1011375:
                    return "Chapeis";
                case 1111747:
                    return "Roupas Casuais";
                case 1112576:
                    return "Por favor, digite o numero que deseja fazer (1-100) e digite enter. Aperte ESC para cancelar.";
                case 1015293:
                    return "Armaduras de Couro";
                case 1015288:
                    return "Sapatos";
                case 1015300:
                    return "Couro Reforçado";
                case 1015306:
                    return "Armadura Feminina";
                case 1049149:
                    return "Armadura de Ossos";
                case 1011233:
                    return "Info";
                case 1011234:
                    return "Amigos";
                case 1011235:
                    return "Opcoes";
                case 1011441:
                    return "Sair";
                case 1011236:
                    return "Alterar nome";
                case 1153922:
                    return "Voce ja votou";
                case 1153892:
                case 1153912:
                    return "Um personagem desta conta ja foi nomiado";
                case 1011242:
                    return "Dono";
                case 1011237:
                    return "Items trancados:";
                case 1011238:
                    return "Max items trancados:";
                case 1011239:
                    return "Secures:";
                case 1011240:
                    return "Max Secures:";
                case 1060167: return "Voce nao esta mais sangrando";
                case 1060678: return "Casa Publica";
                case 1152085: return "Voce conseguiu pegar o item e o item voltou ao normal";
                case 1152083: return "O item magicamente voltou ao local de onde foi retirado";
                case 1060679: return "Casa Privada";
                case 1062209: return "Casa Infinita";
                case 1060694: return "Trocar Fechadura";
                case 1062208: return "Casa de jogador";
                case 1062207: return "Casa condenada";
                case 1072060: return "Voce nao pode usar magias quando pacificado";
                case 1060692: return "Construida em:";
                case 1060693: return "Ultima troca:";
                case 501747: return "Parece estar trancado";
                case 502069: return "Nao parece estar trancado";
                case 1061793: return "Valor da casa:";
                case 1061091: return "Voce nao pode usar magias nessa forma";
                case 1011241: return "Visitantes";
                case 1018032:
                    return "Casa devidamente colocada";
                case 1018035:
                    return "Casa com design moderno";
                case 1011266:
                    return "Sub donos";
                case 1011267:
                    return "Adicionar sub dono";
                case 1112253: return "Voce nao aprendeu a arte de fazer cestos. Talvez desse ler um livro goblinico sobre isto";
                case 1060682: return "Armazenamento";
                case 1072519: return "Armazem aumentado";
                case 1060683: return "Max secures";
                case 1060685: return "Usados p/ caixas moveis";
                case 1151482: return "Sua mana foi amaldicoada";
                case 1151485: return "Agora sua mana esta se comportando de forma estranha";
                case 1151484: return "Voce sente mana sendo extraida de voce";
                case 1151481: return "Usando a mana corrompida danifica seu corpo";
                case 1151486: return "Sua mana nao esta mais corrompida";
                case 1151483: return "Voce sente sua mana voltando ao normal";
                case 1060686: return "Usados p/ trancados";
                case 1112250: return "Voce nao tem juncos secos suficientes";
                case 1071945: return "Voce precisa de um cristal energizado";
                case 1071948: return "Voce precisa de barras de ferro";
                case 1071947: return "Voce precisa de barras de bronze";
                case 1071946: return "Voce precisa de engrenagens";
                case 1060688: return "Usados p/ secures";
                case 1060689: return "Armazem disponivel";
                case 1060690: return "Max trancados";
                case 1060691: return "Trancas disponiveis";
                case 1154602: return "Sapatos";
                case 1154603: return "Calcas";
                case 1062391: return "Vendedores";
                case 1060687: return "Usados por vendedor";
                case 501590: return "Esta criatura e muito legal para ser provocada";
                case 1049446: return "Voce nao tem chance de provocar isto";
                case 501599: return "Voce falhou em deixar o animal brabo";
                case 501602: return "Voce provocou a criatura";
                case 1060759: return "Converter em casa custom";

                case 1070696: return "Voce foi stunado por uma pancada colossal";
                case 1070695: return "Voce recupera seus sentidos";
                case 1060765: return "Customizar";
                case 502136: return "O veneno parece nao fazer mais efeito";
                case 1005603: return "Voce pode se movimentar novamente";
                case 1060760: return "Mover caixas";
                case 1004001:
                case 1060849:
                    return "Voce nao pode desarmar seu oponente";
                case 1060092: return "Voce desarmou seu inimigo";
                case 1060093: return "Voce foi desarmado";
                case 1060761: return "Alterar placa";
                case 1060762: return "Alterar metal da placa";
                case 1042060: return "Nao consigo ver o alvo";
                case 1060763: return "Alterar poste da placa";
                case 1062004: return "Alterar base";
                case 1060764: return "Renomear casa";
                case 1018036: return "Remover sub dono";
                case 1011268: return "Limpar sub donos";
                case 1061797: return "Vender casa";
                case 1061798: return "Tornar primaria";
                case 500485: return "Nada para coletar deste corpo";
                case 1011243: return "Amigos";
                case 1011244: return "Adicionar amigo";
                case 1018037: return "Remover amigo";
                case 1113034: return "Voce nao tem conhecimento de mecanica";
                case 1073994: return "Seu titulo sera:";
                case 1113054: return "Voce precisa de pelo menos 60 tinker para isto";
                case 1073996: return "Aceitar";
                case 1073997: return "Prox";
                case 1115023: return "Titulos";
                case 1115024: return "Tipos";

                case 1060675: return "Fechar";
                case 1115026: return "Prefixo Paperdoll";
                case 1115027: return "Sufixo Paperdoll";
                case 1115035: return "Aplicar titulo ?";
                case 1011046: return "Aplicar";
                case 1115036: return "Titulo aplicado";
                case 1115032: return "Monstros";
                case 1115034: return "Recompensas";
                case 1115033: return "Guilda";
                case 1115040: return "Abreviacao da guilda";
                case 1115039: return "Titulo custom do mestre da guilda";
                case 1115028: return "Titulo na cabeca";
                case 1115029: return "Subtitulo";
                case 1115025: return "Descricao";
                case 1115037: return "Titulo limpo";
                case 1154764: return "Padrao";
                case 1115128: return "Isto ira atualizar seu titulo automaticamente";
                case 1062141: return "Limpar";
                case 1115038: return "Limpar titulo ?";
                case 1074010: return "Voce escondeu seu titulo";
                case 1005564: return "Voce nao pode sair daqui enquanto batalha. Aguarde um pouco em paz.";
                case 1011245: return "Limpar amigos";
                case 1011259: return "Expulsar alguem";
                case 1011248: return "Transferir casa";
                case 1011249: return "Demolir casa";
                case 1011250: return "Mudar tipo placa";
                case 1011254: return "Placas de guildas";
                case 1011277: return "Okay ta bom";
                case 1011255: return "Opcoes de loja";
                case 501302: return "O que a placa deve dizer ?";
                case 501587: return "Quem voce deseja provocar ?";
                case 500617: return "Que instrumento musical deseja usar?";
                case 501389: return "Essa casa tem uma guildstone...";
                case 1073621: return "Voce pegou a recompensa";
                case 1061624: return "Voce nao tem dinheiro suficiente no banco";
                case 1062429: return "Voce precisa estar perto da placa da sua casa";
                case 501317:
                case 501309:
                case 501328: return "Selecione o jogador";
                case 501332:
                case 501333: return "Lista limpada";
                case 1063115: return "Voce esta confiante";
                case 1044049: return "Joias";
                case 1044042: return "Madeira";
                case 1044046: return "Ferramentas";
                case 1044047: return "Partes";
                case 1044048: return "Utils";
                case 1044050: return "Etc";
                case 1044268: return "Voce nao tem habilidade suficiente para trabalhar com este recurso";
                case 1044051: return "Mecanico";
                case 1044052: return "Armadilhas";
                case 1044294: return "Outros";
                case 1044291: return "Moveis";
                case 1044292: return "Containers";
                case 1044566: return "Armas";
                case 1062760: return "Armaduras";
                case 1044293: return "Instrumentos";
                case 1044290: return "Etc";
                case 1044297: return "Treinamento";
                case 1044298: return "Utentilios";
                case 501329:
                case 501330:
                case 501316:
                case 501318:
                case 501320:
                case 501310:
                case 501319:
                case 501307:
                case 501327: return "Voce nao eh o dono desta casa";
                case 1049540: return "Voce falhou em perturbar o alvo";
                case 500612:
                    return "Voce tocou muito mal, e nao tem efeito algum";
                case 1049539:
                    return "Voce tocou uma musica estranha que enfraquece o alvo";
                case 1049541:
                    return "Selecione o instrumento";

                case 1049066:
                    return "Voce gostaria de reportar...";
                case 1061626:
                    return "Qualquer um";
                case 1063455:
                    return "Guilda";
                case 1061279:
                    return "Amigos";
                case 1061278:
                    return "Apenas Co-Donos";
                case 1061277:
                    return "Apenas Dono";
                case 1041474:
                    return "Dono";
                case 1063101: return "Voce estava muito proximo do alvo muito tempo";
                case 1063100: return "A trajetoria ligeira ate o alvo faz com que seu ataque cause mais dano";
                case 1061276:
                    return "Setar Acesso";
                case 1045133:
                    return "Ordem Pequena";
                case 1045134:
                    return "Ordem Grande";
                case 1045135:
                    return "Obrigado, poderia me ajudar ?";
                case 1045138:
                    return "Quantidade a fazer";
                case 1045137:
                    return "Items";
                case 1061085: return "Nao tem vida suficiente ali";
                case 1061083: return "Selecione um corpo";
                case 1061086: return "Voce nao pode animar o que ja estava animado";
                case 1061689: return "Sua pele resseca e voce se sente mal";
                case 501078: return "Voce precisa estar segurando uma arma";
                case 1060508: return "Nao pode amaldicoar isto";
                case 1061688: return "Sua pele retorna ao normal";
                case 1060872: return "Sua mente se sente normal novamente";
                case 1061605: return "Voce ja tem um capetinha";
                case 1061825: return "Voce decidiu nao invocar o capetinha";
                case 1061687: return "Voce respira bem novamente";
                case 1072112: return "Voce precisa de 100 spirit speak para usar isto";
                case 502242: return "Voce pegou um pouco do liquido e colocou em um frasco";
                case 1061607: return "Ja existe uma maldicao ativa";
                case 502246:
                case 502245: return "O barril esta vazio";
                case 1061609:
                case 1061608: return "O alvo ja esta amaldicoado";
                case 1061620: return "Sua maldicao quebrou";
                case 1045140:
                    return "Adicionais";
                case 1045141:
                    return "Apenas Excepcionais";
                case 1011012:
                    return "Cancelar";
                case 1045139:
                    return "Voce aceita esta ordem ?";
                case 1157304:
                    return "Combine essa escritura com os itens contidos";
                case 1045154:
                    return "Combine esta escritura com o item";
                case 1045155:
                    return "Combine esta escritura com outra escritura";
                case 1045136:
                    return "Items pedidos";
                case 1152693:
                    return "Voce sente o poder do vazio";
                // SYS MESSAGES
                case 1070833:
                    return "A criatura cria um leque de fogo";
                case 1070829:
                    return "Sua resistencia voltou ao normal";
                case 1060163:
                    return "Voce desferiu um ataque paralizante";
                case 1060164:
                    return "O Ataque te paralizou";
                case 1008111:
                    return "O frio doi em seus ossos";
                case 502629:
                    return "Voce nao pode conjurar magias aqui";
                case 502134:
                    return "Voce agora eh tido como assassino e esta banido de cidades";
                case 1049067:
                    return "Voce foi reportado por um assassinato";
                case 500495:
                    return "Voce nao conseguiu obter nada usavel";
                case 500367:
                    return "Tente usar isto em uma maquina de tecer";
                case 1113038:
                    return "Nao esta cheio";
                case 1115895:
                    return "Este elemental nao tem magia...";
                case 1115896:
                    return "A garrafa estilhacou";
                case 1152076:
                    return "Voce foi capturado pelo carcereiro.";
                case 1115901:
                    return "O jarro magico se linkou";
                case 1115972:
                    return "O refill falhou pois o jarro nao foi linkado nesta area";
                case 1115892:
                    return "Selecione uma fonte de agua";
                case 1115899:
                    return "Voce linkou com a fonte de agua";
                case 1115900:
                    return "Alvo invalido";
                case 1115898:
                    return "Deslinkado";
                
                   
                case 1011247: return "Trocar Chave";
                case 502088: return "Um item foi colocado em sua mochila";
                case 1153599: return "Voce ja usou isto em outro ritual";
                case 1153607: return @"Contido neste Tomo está o ritual pelo qual o Êxodo pode mais uma vez ser chamado à vida em sua forma física, convocada das profundezas do Vazio. Somente quando o Rito de Invocação foi reintegrado ao tomo e somente quando o Manto do Rito cobre o lançador, a Adaga Sacrificial pode ser usada para selar seu destino. Esfaqueie a adaga neste livro e declare sua busca pela bravura, ao defender a vida deste mal, ou sacrifique seu sangue neste altar para declarar sua busca por ganância e riqueza ... só você pode julgar a si mesmo ...";
                case 1153596: return "Voce precisa estar em um grupo para fazer o ritual. Para ver o menus de grupos, clique no pergaminho no pe direito do seu paperdoll.";
                case 1153675: return "O altar deve ser construido na mao da compaixao, na caverna do exilio.";
                case 500269: return "Voce nao pode construir isso ali";
                case 1153591: return "Voce nao esta preparado para o ritual";
                case 1153603: return "Voce deve primeiro usar o rito no tomo";
                case 1153604: return "Selecione o tomo de invocacao";
                case 501662: return "Aonde deseja usar a chave ?";
                case 1049343: return "Voce nao pode fazer isto com este item";
                case 1048000: return "Trancou";
                case 500111: return "Voce esta paralizado e nao pode se mover";
                case 1048001: return "Destrancou";
                case 501282: return "Voce destrancou e trancou a porta rapidamente";
                case 501280: return "Isto esta trancado mas voce pode abrir por dentro";
                case 500493: return "Nao tem madeira suficiente aqui";
                case 500491: return "Voce colocou gravetos em sua mochila";
                case 500492: return "Um machado faria um estrago maior";
                case 500490: return "Nao cabe mais nada em sua mochila";
                // Quests
                case 1072353: return "Objetivo de missao atualizado";
                case 1072379: return "Entregar para";
                case 1062379: return "Tempo sobrando";
                case 1072207: return "Entregar";
                case 1074782: return "Retornar a";
                case 1072205: return "Obter";
                case 1018327: return "Local";
                case 1072204: return "Matar";
                case 1049073: return "Objetivo";
                case 1072208: return "Todos a seguir:";
                case 1049010: return "Oferta de Missao";
                case 1046026: return "Registro de Missao";
                case 1049000: return "Confirmar cancelar";
                case 1060836: return "Essa quest pode ser util. <br> Tem certeza que deseja abandona-la ?";
                case 1049006: return "Nao, mudei de ideia";
                case 1049005: return "Abandonar apenas esta";
                case 1075023: return "Abandonar toda corrente";
                case 1157503: return "Todo treinamento nao pode ser desaprendido. Deseja completar o treinamento ?";
                case 1157572: return "O Slot do pet ira aumentar em 1. Deseja continuar ?";
                case 1157571: return "Ajuste de Slots";
                case 1072202:
                case 1075024: return "Descricao:";
                case 1157502: return "Treinamento de Pets";
                case 1072209: return "Um dos seguintes:";
                case 3006156: return "Dialogo de Missao";
                case 1074861: return "Voce nao tem tudo o que precisa";
                case 1072201: return "Recompensa";
                case 1157543: return "A criatura ganhou bastante experiencia e esta pronta para treinar !!";
                case 1157569: return "[Power Hour] Seu Pet Treinara mais rapido durante 1 hora !";

                case 1008086: return "* Pega um item *";
                case 503170:
                    return "UEPA isso aqui nao parece um peixe";
                case 1061920:
                    return "Voce precisa regar a terra antes";
                case 1061885:
                    return "Voce precisa plantar a semente primeiro";
                case 1061839:
                    return "Voce atingiu o numero maixmo de secures";
                case 1005379:
                case 1155867: return "Voce nao tem essa quantidade ou quantidade invalida";
                    return "Voce atingiu o numero maximo de items trancados";
                case 1010424:
                    return "Voce nao pode securar isto";
                case 1010423:
                    return "Coloque o item no chao primeiro";
                case 1010550:
                    return "Isto ja esta trancado e nao pode ficar seguro";
                case 502401:
                    return "O runebook esta cheio";
                case 502409:
                    return "Voce precisa marcar a runa para inserir no runebook";
                case 500169:
                    return "Voce nao pode pegar isto";
                case 1114298:
                    return "Isto precisa estar trancado em uma casa";
                case 502503:
                    return "A porta esta trancada";
                case 502421:
                    return "Voce removeu a runa";
                case 501505:
                    return "Eu nao posso lhe ensinar nada!";
                case 501507:
                    return "Eu nao posso lhe ensinar isso";
                case 501508:
                case 501509:
                    return "Voce sabe mais disso do que eu...";
                case 1043058:
                    return "Posso lhe treinar nas seguintes habilidades";
                case 1005377:
                    return "Voce nao pode trancar/destrancar isto";
                case 1005525:
                    return "Isto nao esta em sua casa";
                case 500295:
                case 500446:
                    return "Voce esta muito longe";
                case 500378:
                    return "Voce eh um criminoso, xispa daqui rapariga";
                case 1010587:
                    return "Voce nao tem permissao nessa casa para isto";
                case 502109:
                    return "Donos nao tem um cofre para si proprio";
                case 502106:
                    return "Selecione o item que deseja remover o cofre";
                case 502094:
                    return "Voce precisa estar em sua casa para isto";
                case 1070820:
                case 1155739:
                    return "Voce da um golinho...";
                    return "A criatura cospe acido";
                case 1054025:
                    return "Voce precisa deixar a criatura fraca antes de doma-la !";
                case 1151812: return "Voce formou uma pedra elemental suprema";
                case 1072790: return "O muro fica transparente e voce passa por ele";
                case 1044361:
                case 1044362:
                case 1044363:
                case 1044364:
                case 1044365:
                case 1044366:
                case 1044367:
                case 1044368:
                    return "Voce nao tem reagentes suficientes para fazer isto";
                case 1044037:
                    return "Voce nao tem metal suficiente para fazer isto";
                case 500213:
                    return "Voce nao e forte suficiente para equipar isto";
                case 1011080:
                    return "Escudos";
                case 1011079:
                    return "Elmos";
                case 1011173:
                    return "Utilidades";
                case 502808:
                    return "Voce seria envenenado agora mas como eh um iniciante esta imune";
                case 1019036:
                    return "Voce ja e forte o suficiente e nao e mais um iniciante !";
                case 1011084:
                    return "Esmagamento";
                case 1063024:
                    return "Voce nao pode fazer isto agora";
                case 1011083:
                    return "Armas de Haste";
                case 1011081:
                    return "Laminas";
                case 1011082:
                    return "Machados";
                case 1111704:
                    return "Armadura";
                case 1116348:
                    return "Curativos";
                case 1116349:
                    return "Desempenho";
                case 1116350:
                    return "Toxinas";
                case 1116351:
                    return "Explosivos";
                case 1116353:
                    return "Estranhos";
                case 1044496:
                    return "Preparativos";
                case 1044497:
                    return "Confeitaria";
                case 1044498:
                    return "Churrasco";
                case 1080001:
                    return "Chocolates";
                case 1044495:
                    return "Ingredientes";
                case 500946:
                    return "Voce nao pode conjurar isto na cidade";
                case 1157491:
                    return "Progresso Treinamento";
                case 1157568:
                    return "Ver Progresso";
                case 1157492:
                    return "Opcoes";
                case 1158013:
                    return "Cancelar Treinamento";
                case 1157487:
                    return "Iniciar Treinamento";
                case 500450:
                    return "Voce apenas pode fazer isto em criatura mortas";
                case 1049593: return "Atributos";
                case 501666: return "Voce nao pode destrancar isto";
                case 1049578: return "Vida";
                case 1028335: return "Forca";
                case 1157565:
                case 1157573:
                case 1157574: return "A experiencia de batalha aumentou";
                case 3000113: return "Destreza";
                case 3000112: return "Inteligencia";
                case 1070793: return "Dificuldade Bardos";
                case 1049594: return "Lealdade";
                case 1075627: return "Regen Vida";
                case 1079411: return "Regen Stam";
                case 1079410: return "Regen Mana";
                case 1049666: return "Seu pet foi bondado e agora pode ser ressado";
                case 1061645: return "Resistencias";
                case 1019068: return "Ver mercadorias";
                case 1061646: return "Fisica";
                case 1061647: return "Fogo";
                case 1061648: return "Gelo";
                case 1061649: return "Veneno";
                case 1061650: return "Energia";
                case 1017319: return "Dano";
                case 1076750: return "Dano Base";
                case 3001030: return "Combate";
                case 3001032: return "Conhecimentos";
                case 1049563: return "Comida Favorita";
                case 1049565: return "Frutas e Vegetais";
                case 1049566: return "Graos e Trigo";
                case 1049568: return "Peixes";
                case 1049564: return "Carnes";
                case 1044477: return "Ovos";
                case 1049569: return "Instinto";
                case 1115783: return "Slots";
                case 1157505: return "Avancos";
                case 1157599: return "Pontos Restantes";
                case 1157598: return "Voce cancelou o plano";
                case 1157597: return "Voce iniciou o plano de treino";
                case 1157593: return "Voce tirou do seu plano de treino";
                case 1157485: return "<CENTER>TREINAMENTO DE ANIMAIS</CENTER>";
                case 1044010: return "<CENTER>CATEGORIAS</CENTER>";
                case 1044011: return "<CENTER>SELECOES</CENTER>";
                case 1157495: return "Aumentar Cap Magico";
                case 1157496: return "Aumentar Cap Fisico";
                case 1157481: return "Magias";
                case 1157480: return "Habilidades";
                case 1157479: return "Movimentos";
                case 1157482: return "Habilidades Area";
                case 1157486: return "<CENTER>CONFIRMAR TREINO</CENTER>";
                case 1114270: return "Propriedade";
                case 1114272: return "Peso";
                case 1157490: return "Pontos Restantes";
                case 1114368: return "Este pet nao e seu";
                case 1153204: return "Muito longe";
                case 1156876: return "Voce esteve em combate recentemente";
                case 1157498: return "Voce nao tem slots suficientes para isto";
                case 1157500: return "Voce nao tem pontos suficientes para treinar o pet";
                case 1157499: return "Voce precisa de um powerscroll para isto";
                case 1157501: return "Seu pet ja completou o treinamento";
                case 1157497: return "Voce treinou seu pet";
                case 1157592: return "Voce adicionou ao plano de treinamento";
                case 502805: return "Parece que a criatura ficou irritada";
                case 1005559:
                    return "A magia parece ja estar sob efeito";
                case 1005385:
                    return "Voce ja tem uma magia defensiva ativa";
                case 1049645:
                    return "Voce ja tem muitos seguidores";
                case 1044038:
                    return "Sua ferramenta quebrou";
                case 1048146:
                    return "Voce precisa usar a ferramenta que voce esta equipando";
                case 1044267:
                    return "Voce precisa estar perto de uma forja e/ou bigorna";
                case 1061637:
                    return "Voce nao tem acesso a isso";
                case 1042404:
                    return "Voce nao tem essa magia";
                case 1044043:
                    return "Voce falhou ao criar o item e perdeu alguns materiais";
                case 1044157:
                    return "Voce falhou ao criar o item mas nao perdeu materiais";
                case 502785:
                    return "Voce fez um item de baixa qualidade";
                case 1044156:
                case 1044155:
                    return "Voce fez um item excepcional";
                case 1044154:
                case 501629:
                    return "Voce criou o item";
                case 501630:
                    return "Voce falhou ao criar o item";
                case 1010086:
                case 1010018:
                    return "No que deseja usar isto ?";
                case 1062002:
                    return "Voce nao pode mais usar isto";
                case 501783:
                    return "Voce sente seu corpo resistindo a magia";
                case 500237:
                    return "Voce nao pode ver isto";
                case 502268:
                    return "Por favor tire essas correntes de mim !!";
                case 502267:
                    return "AJUUUUDAAAA EUUUUU!";
                case 502266:
                    return "Ai meu deus ai meu deus SOCORRO !";
                case 502265:
                    return "Meu Jesus bom senhor me ajude por favor !";
                case 502264:
                    return "Me ajuda prometo ser uma pessoa melhor por favor !";
                case 502263:
                    return "Por favooooooorzinho me ajuda aqui !";
                case 502262:
                    return "Ai meu deus me ajude por favor SOCORRO !";
                case 502261:
                    return "Por favor me ajude !!";
                case 500119:
                    return "Aguarde para fazer outra acao";
                case 1074846:
                    return "Removeu todas maldicoes";
                case 1010058:
                    return "Voce curou todos os venenos do alvo";
                case 1010059:
                    return "Voce foi curado de todos os venenos";
                case 1042001:
                    return "Isto precisa estar em sua mochila";
                case 500263: return "O acido danifica sua arma";
                case 1061121: return "Seu equipamento foi avariado";
                case 1150442: return "Voce plantou a semente";
                case 1150511: return "Este jardim nao eh seu";
                case 502976: return "Extra ! Extra!";
                case 502802: return "Alguem ja esta domando esta criatura";
                case 1010597: return "Voce comeca a domar a criatura";
                case 502806: return "Voce nao tem chance de domar isto";
                case 502237: return "Voce colocou o frasco em sua mochila";
                case 502469: return "Esta criatura nao pode ser domada";
                case 502236: return "Misturar pocoes nao eh uma boa ideia...(ainda)..";
                case 502232: return "Isto nao serve aqui";
                case 502233:return "O barril esta muito cheio";
                case 502801: return "Voce nao pode domar isto";
                case 1010598: return "* Comecou a domar a criatura *";
                case 1019045:
                case 502795: return "Voce esta muito longe";
                case 1005615: return "Esta criatura ja teve muitos donos e esta frustrada";
                case 1011066: return "Proxima";
                case 1111673: return "Voce se sente melhor";
                case 1111670: return "Voce recuperou da amnesia";
                case 1111669: return "O ataque da criatura te enfraquece, voce tem amnesia";
                case 1070849: return "Voce se sente melhor";
                case 1070847: return "A criatura continua sugar sua vida";
                case 1070848: return "Voce sente sua vida sendo sugada";
                case 1070845: return "A criatura continua corrompendo sua armadura";
                case 1070846: return "A criatura corrompeu sua armadura";
                case 1112473: return "Sua ferida esta doendo..";
                case 1234567: return "Te da uma mordida dolorida";
                case 1011067: return "Anterior";
                case 1153752: return "Sua velocidade de ataque foi diminuida";
                case 502804: return "A criatura ja tem dono";
                case 1155406: return "Voce so pode usar isto na Guerra Infinita";
                case 1155407: return "O limite de armadilhas foi atingido";
                case 1155408: return "Aguarde para colocar outra armadilha";
                case 502794: return "A criatura esta muito irritada para isto";
                case 502799: return "A criatura te aceitou como mestre";
                case 1155409: return "Aonde deseja colocar a armadilha ?";
                case 1042261: return "Voce nao pode colocar a armadilha ali";
                case 1155415: return "Voce precisa participar da Guerra Infinita para usar isto";
                case 1048053: return "Voce nao pode estabular isto";
                case 1042562: return "Este animal nao e seu";
                case 1049668: return "Seu animal esta morto";
                case 1042556: return "Voce nao tem dinheiro suficiente";
                case 502677: return "Voce nao tem dinheiro suficiente";
                case 502679: return "Guardarei seu animal. Diga 'claim' e pegue-o em ate uma semana senao irei vende-lo !";
                case 502673: return "Sai fora, issae eh invocado, xessus na causa";
                case 1042563: return "Por favor descarregue seu pet";
                case 1042564: return "Seu animal esta em combate";
                case 1042565: return "Voce ja tem muitos animais estabulados";
                case 502798: return "Voce falhou em domar a criatura";
                case 1042559: return "Aqui esta, e um bom dia a voce ! :)";
                case 1042558: return "Eu cobro por semana. Se nao me pagar, nao vai recuperar seu animal";
                case 1046000: return "Esses selvagens... me ajude !";
                case 502671: return "Voce nao tem nada aqui";
                case 501041: return "Isto nao esta morto";
                case 1060020: return "Voce nao tem dinheiro suficiente em seu banco.";
                //Couros
                //case 1024216: return "Peles";
                //case 1024199: return "Couro";

                //Lumberjacking
                case 1072540: return "Você cortou um pouco de Madeira e colocou na sua mochila";
                case 1072541: return "Você cortou um pouco de Madeira de Carvalho e colocou na sua mochila";
                case 1072542: return "Você cortou um pouco de Madeira de Pinho e colocou na sua mochila";
                case 1072543: return "Você cortou um pouco de Madeira de Mogno e colocou na sua mochila";
                case 1072544: return "Você cortou um pouco de Madeira de Eucalipto e colocou na sua mochila";
                case 1072545: return "Você cortou um pouco de Madeira de Carmesim e colocou na sua mochila";
                case 1072546: return "Você cortou um pouco de Madeira de Gelo e colocou na sua mochila";
                case 1011036: return "Okay";
                case 1013004: return "<center> Renunciando ser Novato</center>";
                case 1013005: return @" Sendo novato, monstros evitam te atacar e jogadores nao podem te atacar.<br><br>
                            Se voce deixar de ser novato, voce perde esta protecao.
                            Voce sera atacado por monstros que antes apenas o viam passar sem fazer nada.
                            <br><br>
                            Selecione Okay se realmente deseja renunciar seu status de novato ou cancelar.";
            };
            return null;
        }

        public static void Msg(Mobile from, int msg)
        {
            String msgstr = Trans(msg);
            if (msgstr != null)
                from.SendMessage(msgstr);
            else
                from.SendLocalizedMessage(msg);
        }
    }
}
