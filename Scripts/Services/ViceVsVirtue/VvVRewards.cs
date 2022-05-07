using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using System.Collections.Generic;
using Server.Network;
using Server.Guilds;
using System.Linq;
using Server.Engines.Points;
using Server.Items.Functional.Pergaminhos;
using Server.Misc.Custom;
using Server.Engines.VeteranRewards;
using Server.Engines.Auction;
using Server.Ziden;

namespace Server.Engines.VvV
{
    public enum TileType
    {
        North = 0,
        West = 1,
    }

    public static class VvVRewards
    {
        public static List<CollectionItem> Rewards { get; set; }

        public static void Initialize()
        {
            Rewards = new List<CollectionItem>();

            //Rewards.Add(new CollectionItem(typeof(GreaterManaPotion), 6870, 1094764, 437, 500));  // Greater Stam
            //Rewards.Add(new CollectionItem(typeof(GreaterCurePotion), 6870, 1094764, 437, 500));  // Greater Stam
            //Rewards.Add(new CollectionItem(typeof(GreaterExplosionPotion), 6870, 1094764, 437, 500));  // Greater Stam
          
            Rewards.Add(new CollectionItem(typeof(VvVPotionKeg), 6870, 1094764, 437, 500));  // Greater Stam
            Rewards.Add(new CollectionItem(typeof(VvVPotionKeg), 6870, 1094718, 13, 500));   // Supernova
            Rewards.Add(new CollectionItem(typeof(VvVPotionKeg), 6870, 1155541, 2500, 500)); // Stat Loss Removal
            Rewards.Add(new CollectionItem(typeof(KegGH), 6870, "Keg de Vida Maior", 0, 3000));  // Greater Stam

            Rewards.Add(new CollectionItem(typeof(EssenceOfCourage), 3838, 1155554, 2718, 250)); // Essence of Courage

            Rewards.Add(new CollectionItem(typeof(VvVSteedStatuette), 8484, "Cavalo de Guerra Do Caos", ViceVsVirtueSystem.VirtueHue, 100)); // Virtue War Horse
            Rewards.Add(new CollectionItem(typeof(VvVSteedStatuette), 8484, "Cavalo de Guerra Da Ordem", ViceVsVirtueSystem.ViceHue, 100));   // Vice War Horse
            Rewards.Add(new CollectionItem(typeof(VvVSteedStatuette), 8501, "Ostard de Guerra do Caos", ViceVsVirtueSystem.VirtueHue, 100)); // Virtue War Ostard
            Rewards.Add(new CollectionItem(typeof(VvVSteedStatuette), 8501, "Ostard de Guerra da Ordem", ViceVsVirtueSystem.ViceHue, 100));   // Vice War Ostard

            Rewards.Add(new CollectionItem(typeof(VvVHairDye), 3838, "Tinta da Virtude", ViceVsVirtueSystem.VirtueHue, 2500)); // Virtue Hair Dye
            Rewards.Add(new CollectionItem(typeof(VvVHairDye), 3838, "Tinta do Caos", ViceVsVirtueSystem.ViceHue, 2500));   // Vice Hair DYe

            Rewards.Add(new CollectionItem(typeof(VvVTrapKit), 7866, "Armadilha de Veneno", 0, 250));    // Poison Trap Kit
            Rewards.Add(new CollectionItem(typeof(VvVTrapKit), 7866, "Armadilha de Gelo", 0, 250));    // Freezing Trap Kit
            Rewards.Add(new CollectionItem(typeof(VvVTrapKit), 7866, "Armadilha de Shoque", 0, 250));    // Shocking Trap Kit
            Rewards.Add(new CollectionItem(typeof(VvVTrapKit), 7866, "Armadilha de Laminas", 0, 250));    // Blades Trap Kit
            Rewards.Add(new CollectionItem(typeof(VvVTrapKit), 7866, "Armadilha de Explosao", 0, 250));    // Explosion Trap Kit

            Rewards.Add(new CollectionItem(typeof(CannonTurretPlans), 5360, "Canhao de Guerra Infinita", 0, 3000));    // Cannon Turret
            Rewards.Add(new CollectionItem(typeof(ManaSpike), 2308, "Espinho de Mana", 0, 1000));            // Mana Spike
            Rewards.Add(new CollectionItem(typeof(SkillBook), 0xEFA, "Livro Cientifico", 0, 3000));   // Scroll of Transcendence
            Rewards.Add(new CollectionItem(typeof(PergaminhoSagrado), 0x14F0, "Pergaminho Sagrado 30 Dias Pertence Pessoal", 0, 10000));
            Rewards.Add(new CollectionItem(typeof(PergaminhoCarregamento), 0x14F0, "Pergaminho Carregar +Iem", 0, 5000));
            Rewards.Add(new CollectionItem(typeof(PergaminhoPeso), 0x14F0, "Pergaminho Carregar +Peso", 0, 5000));
            Rewards.Add(new CollectionItem(typeof(PergaminhoSagrado), 0x14F0, "Pergaminho Sagrado 30 Dias Pertence Pessoal", 0, 10000));
            Rewards.Add(new CollectionItem(typeof(TalismanDragao), 0x2F58, "Talisman Protecao contra Bafo de Dragao", 0, 5000));
            Rewards.Add(new CollectionItem(typeof(SandMiningBook), 0xFF4, "Manual de Minerar na Areia", 0, 1000));

            Rewards.Add(new CollectionItem(typeof(GoldBraceletBonito), 0x1086, "Bracelete de Ouro Elegante (+Stat)", 0, 2000));
            Rewards.Add(new CollectionItem(typeof(GoldRingElegante), 0x108a, "Anel de Ouro Elegante (+Stat)", 0, 2000));

            Rewards.Add(new CollectionItem(typeof(ForgedRoyalPardon), 18098, 1155524, 0, 10000));        // Royal Forged Pardon
            Rewards.Add(new CollectionItem(typeof(ScrollOfTranscendence), 5360, "Pergaminho de Skill", 0x490, 8000));   // Scroll of Transcendence

            Rewards.Add(new CollectionItem(typeof(VvVRobe), 9859, "Sobretudo da Virtude", ViceVsVirtueSystem.VirtueHue, 5000)); // virtue robe
            Rewards.Add(new CollectionItem(typeof(VvVRobe), 9859, "Sobretudo do Caos", ViceVsVirtueSystem.ViceHue, 5000)); // virtue robe

            Rewards.Add(new CollectionItem(typeof(CovetousTileDeed), 5360, 1155516, 0, 5000)); // Covetous Tile
            Rewards.Add(new CollectionItem(typeof(DeceitTileDeed), 5360, 1155517, 0, 5000)); // Deceit Tile
            Rewards.Add(new CollectionItem(typeof(DespiseTileDeed), 5360, 1155518, 0, 5000)); // Depise Tile
            Rewards.Add(new CollectionItem(typeof(DestardTileDeed), 5360, 1155519, 0, 5000)); // Destard Tile
            Rewards.Add(new CollectionItem(typeof(HythlothTileDeed), 5360, 1155520, 0, 5000)); // Hythloth Tile
            Rewards.Add(new CollectionItem(typeof(PrideTileDeed), 5360, 1155521, 0, 5000)); // Pride Tile
            Rewards.Add(new CollectionItem(typeof(ShameTileDeed), 5360, 1155522, 0, 5000)); // Shame Tile
            Rewards.Add(new CollectionItem(typeof(WrongTileDeed), 5360, 1155523, 0, 5000)); // Wrong Tile

            Rewards.Add(new CollectionItem(typeof(VvVWand1), 3571, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVWand2), 3571, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVWizardsHat), 5912, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVWand1), 3571, 0, 0, 500));

            Rewards.Add(new CollectionItem(typeof(ValeDecoracaoRara), 0x9F64, "Caixa Misteriosa", 0, 500));  // Greater Stam
            Rewards.Add(new CollectionItem(typeof(ElementalBall), 3630, "Bola de Cristal Elemental", 0, 2000));  // Greater Stam
            Rewards.Add(new CollectionItem(typeof(DaviesLockerAddonDeed), 0x14F0, "Guarda Mapas", 0, 1000));
            Rewards.Add(new CollectionItem(typeof(CannonDeed), 0x14F0, "Escritura de Canhao", 0, 5000));
            Rewards.Add(new CollectionItem(typeof(RedSoulstone), 0x32F3, "Pedra das Almas</br>Guarda 1 Skill", 0, 2000));
            Rewards.Add(new CollectionItem(typeof(CommodityDeedBox), 0x9AA, "Caixa de Commidities</br>Guarda recursos e comodities", 0, 2000));
            Rewards.Add(new CollectionItem(typeof(AuctionSafeDeed), 0x9C18, "Cofre de Leilao</br>Permite leiloar items", 0, 5000));
            Rewards.Add(new CollectionItem(typeof(BannerDeed), 0x14F0, "Banner</br>Escolha e bote um banner medieval", 0, 500));
            Rewards.Add(new CollectionItem(typeof(SpellbookDyeTub), 0xFAB, "Tinta de Livro de Magia</br>Permite pintar livros de magia", 0, 8000));
            Rewards.Add(new CollectionItem(typeof(RunebookDyeTub), 0xFAB, "Tinta de Runebook</br>Permite pintar runebooks", 0, 8000));
            Rewards.Add(new CollectionItem(typeof(MetallicDyeTub), 0xFAB, "Tinta de Armaduras</br>Permite pintar armaduras de ferro", 0, 2000));
            Rewards.Add(new CollectionItem(typeof(RepairBenchDeed), 0x14F0, "Mesa de Reparos</br>Permite Reparar Items", 0, 10000));

            Rewards.Add(new CollectionItem(typeof(KegManaMaior), 6870, "Keg de Mana Maior", 0, 3000));  // Greater Stam
            Rewards.Add(new CollectionItem(typeof(Fullbook), 0xEFA, "Fullbook", 0, 3000));  

            /*
            Rewards.Add(new CollectionItem(typeof(MorphEarrings), 4231, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(MaceAndShieldGlasses), 12216, 0, 477, 500));
            Rewards.Add(new CollectionItem(typeof(InquisitorsResolution), 5140, 0, 1266, 500));
            Rewards.Add(new CollectionItem(typeof(OrnamentOfTheMagician), 4230, 0, 1364, 500));
            Rewards.Add(new CollectionItem(typeof(VesperOrderShield), 7108, 0, 0, 500)); // Needs 0 FC
            Rewards.Add(new CollectionItem(typeof(ClaininsSpellbook), 3834, 0, 2125, 500));
            Rewards.Add(new CollectionItem(typeof(FoldedSteelGlasses), 12216, 0, 1150, 500));
            Rewards.Add(new CollectionItem(typeof(CrystallineRing), 4234, 0, 1152, 500));
            Rewards.Add(new CollectionItem(typeof(SpiritOfTheTotem), 5445, 0, 1109, 500));
            Rewards.Add(new CollectionItem(typeof(WizardsCrystalGlasses), 12216, 0, 688, 500));
            Rewards.Add(new CollectionItem(typeof(PrimerOnArmsTalisman), 12121, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(TomeOfLostKnowledge), 3834, 0, 1328, 500));
            Rewards.Add(new CollectionItem(typeof(HuntersHeaddress), 5447, 0, 1428, 500));
            Rewards.Add(new CollectionItem(typeof(HeartOfTheLion), 5141, 0, 1281, 500));
            Rewards.Add(new CollectionItem(typeof(CrimsonCincture), 5435, 0, 1157, 500));
            Rewards.Add(new CollectionItem(typeof(RingOfTheVile), 4234, 0, 1271, 500));
            Rewards.Add(new CollectionItem(typeof(HumanFeyLeggings), 5054, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(Stormgrip), 10130, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(RuneBeetleCarapace), 10109, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(KasaOfTheRajin), 10136, 0, 0, 500));
          

 
            Rewards.Add(new CollectionItem(typeof(VvVWoodlandArms), 11116, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVDragonArms), 9815, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVGargishPlateArms), 776, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVPlateArms), 5136, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVEpaulette), 0x9985, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVGargishEpaulette), 0x9986, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVGargishStoneChest), 0x286, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVStuddedChest), 5083, 0, 0, 500));
            Rewards.Add(new CollectionItem(typeof(VvVGargishEarrings), 16915, 0, 0, 500));
              */

            Rewards.Add(new CollectionItem(typeof(CompassionBanner), 39351, 1123375, 0, 5000)); // Compassion Banner
            Rewards.Add(new CollectionItem(typeof(HonestyBanner), 39353, 1123377, 0, 5000)); // Honesty Banner
            Rewards.Add(new CollectionItem(typeof(HonorBanner), 39355, 1123379, 0, 5000)); // Honor Banner
            Rewards.Add(new CollectionItem(typeof(HumilityBanner), 39357, 1123381, 0, 5000)); // Humility Banner
            Rewards.Add(new CollectionItem(typeof(JusticeBanner), 39359, 1123383, 0, 5000)); // Justice Banner
            Rewards.Add(new CollectionItem(typeof(SacraficeBanner), 39361, 1123385, 0, 5000)); // Sacrafice Banner
            Rewards.Add(new CollectionItem(typeof(SpiritualityBanner), 39363, 1123387, 0, 5000)); // Spirituality Banner
            Rewards.Add(new CollectionItem(typeof(ValorBanner), 39365, 1123389, 0, 5000)); // Valor Banner

            Rewards.Add(new CollectionItem(typeof(CovetousBanner), 39335, 1123359, 0, 2000)); // Covetous Banner
            Rewards.Add(new CollectionItem(typeof(DeceitBanner), 39337, 1123361, 0, 2000)); // Deceit Banner
            Rewards.Add(new CollectionItem(typeof(DespiseBanner), 39339, 1123363, 0, 2000)); // Depise Banner
            Rewards.Add(new CollectionItem(typeof(DestardBanner), 39341, 1123365, 0, 2000)); // Destard Banner
            Rewards.Add(new CollectionItem(typeof(HythlothBanner), 39343, 1123367, 0, 2000)); // Hythloth Banner
            Rewards.Add(new CollectionItem(typeof(PrideBanner), 39345, 1123369, 0, 2000)); // Pride Banner
            Rewards.Add(new CollectionItem(typeof(ShameBanner), 39347, 1123371, 0, 2000)); // Shame Banner
            Rewards.Add(new CollectionItem(typeof(WrongBanner), 39349, 1123373, 0, 2000)); // Wrong Banner
        }

        public static void OnRewardItemCreated(Mobile from, Item item)
        {
            if (item is IOwnerRestricted)
                ((IOwnerRestricted)item).Owner = from;

            if (item is IAccountRestricted && from.Account != null)
                ((IAccountRestricted)item).Account = from.Account.Username;

            NegativeAttributes neg = RunicReforging.GetNegativeAttributes(item);

            if (neg != null && !(item is Spellbook))
            {
                neg.Antique = 1;

                if (item is IDurability && ((IDurability)item).MaxHitPoints == 0)
                {
                    ((IDurability)item).MaxHitPoints = 255;
                    ((IDurability)item).HitPoints = 255;
                }
            }
            ViceVsVirtueSystem.Instance.AddVvVItem(item, true);
        }
    }
}
