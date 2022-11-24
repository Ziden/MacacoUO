using Server.Engines.Craft;
using Server.Engines.VeteranRewards;
using Server.Mobiles;
using Server.Scripts.Custom.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Items
{
    public class Decos
    {
        public class RandomDecoChest : Bag
        {
            [Constructable]
            public RandomDecoChest()
            {
                Name = "Sacola de Decoracao";
                this.AddItem(Decos.RandomDeco(null));
            }

            public RandomDecoChest(Serial s)
             : base(s)
            {

            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)1); // version
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();
            }
        }

        public static Type[] common_decos = new Type[] {

            typeof(GoblinTopiary), typeof(WaterBarrel),
            typeof(Obelisk), typeof(PaperLantern), typeof(CopperWire), typeof(IronWire), typeof(RoseInAVase), typeof(RuinedTapestry), typeof(RuinedDrawers), typeof(RuinedPainting), typeof(RuinedClock),
            typeof(Globe), typeof(EmptyJars), typeof(GrimWarning), typeof(HeatingStand), typeof(Lever), typeof(MeltedWax),
            typeof(Brazier), typeof(DecoBridleSouth), typeof(BrokenChair), typeof(Cards), typeof(Cards2), typeof(Cards3), typeof(DecoBrimstone),
            typeof(Cards4), typeof(Checkers), typeof(Chessboard), typeof(Countertop),typeof(MagicCrystalBall), typeof(ClownMaskEast), typeof(Checkers), typeof(Checkers2),
            typeof(DecoDeckOfTarot), typeof(DecoEyeOfNewt), typeof(FestiveCactus),
            typeof(DecoBlackmoor), typeof(DecoBloodspawn), typeof(DecoArrowShafts), typeof(DecoBottlesOfLiquor), typeof(DecoCards5), typeof(DecoDeckOfTarot),
           typeof(DecoCrystalBall), typeof(DecoMagicalCrystal), typeof(DecoFlower), typeof(DecoFlower2), typeof(IcyPatch), typeof(JadeSkull), typeof(HolidayBell),
            typeof(DecoGarlic), typeof(DecoGinseng), typeof(DecoGarlicBulb), typeof(DecoGinsengRoot), typeof(DecoGoldIngot), typeof(DecoGoldIngot2), typeof(DecoGoldIngots4),
            typeof(DecoHay), typeof(DecoHay2), typeof(DecoHorseDung), typeof(DecoPumice), typeof(DecorativeAxeNorth), typeof(DecorativeAxeWest), typeof(DecorativeBox),
            typeof(DecorativeDAxeNorth), typeof(DecorativePlant), typeof(DecorativePlantFlax), typeof(DecorativePlantLilypad), typeof(DecorativePlantPoppies),
            typeof(DecorativePlateKabuto), typeof(DecorativeShield1), typeof(DecorativeShield10), typeof(DecorativeShield11), typeof(DecorativeShield2), typeof(DecorativeShield3),
            typeof(DecorativeShield4), typeof(DecorativeShield5), typeof(DecorativeShield6), typeof(DecorativeShield7), typeof(DecorativeShield8), typeof(DecorativeShield9),
            typeof(DecorativeShieldSword1North), typeof(DecorativeShieldSword1West), typeof(DecorativeShieldSword2North), typeof(DecorativeShieldSword2West),
            typeof(DecorativeSwordNorth), typeof(DecorativeTopiary), typeof(DecorativeVines), typeof(DecoRock), typeof(DecoRock2), typeof(DecoRocks), typeof(DecoRocks2),
            typeof(DecoRoseOfTrinsic), typeof(DecoRoseOfTrinsic2), typeof(DecoRoseOfTrinsic3), typeof(DecoSilverIngot), typeof(DecoSilverIngots), typeof(DecoTarot6),
            typeof(DecoTarot5), typeof(DecoTarot3), typeof(DecoTray), typeof(DecoTray2), typeof(SnowPileDeco), typeof(CrystalSkull),typeof(DecorativeShardShield),
            typeof(HearthOfHomeFire), typeof(DecorativeDAxeNorth), typeof(DecorativeDAxeWest), typeof(ElvenAlchemyTable), typeof(EmptyJars), typeof(LargeEmptyPot),
            typeof(SmallEmptyFlask), typeof(PottedPlant), typeof(PottedPlant1), typeof(PottedPlant2), typeof(PottedPlantDeed),
            typeof(Snowman), typeof(UltimaBanner), typeof(WallSconce),  typeof(ElvenPodium), typeof(GothicChest), typeof(cowbellAddonDeed),
            typeof(Pillowbed1AddonDeed), typeof(ghostpaintingAddonDeed), typeof(MonasteryBellAddonDeed), typeof(resinAddonDeed), typeof(KepetchWax), typeof(MeltedWax),
            typeof(ShortMusicStandLeft), typeof(WritingDesk), typeof(WritingTable), typeof(BrazierTall), typeof(Tapestry1N), typeof(Tapestry2N),
            typeof(Tapestry2W), typeof(Tapestry3W),  typeof(Tapestry4W), typeof(Tapestry4N),  typeof(Tapestry5N), typeof(StackofIngots), typeof(StackofLogs), typeof(AncientArmor),
            typeof(CannonBall2), typeof(Gunpowder2), typeof(CannonFuse), typeof(CrackedCrystalBall), typeof(SeersPowder), typeof(EnchantedMarble),
            typeof(SmallEmptyPot), typeof(EmptyToolKit), typeof(EmptyToolKit2), typeof(SmallFish), typeof(SnowGlobe), typeof(PottedPlant), typeof(PottedTree), typeof(PottedTree1),
        };

        public static Type[] rare_decos = new Type[] {
            typeof(PaperLantern), typeof(RoundPaperLantern), typeof(PottedTree), typeof(PottedTree1), typeof(RockingHorse),
            typeof(WallTorch),
            typeof(OrigamiPaper), typeof(LampPost1), typeof(LampPost2), typeof(LampPost3), typeof(LampPost3), typeof(DecoSpittoon),
            typeof(HitchingPost), typeof(RandomMonsterStatuette),typeof(minostatueAddonDeed), typeof(CustomizableWallSign), typeof(DistilleryEastDeed), typeof(BrewDistillerySouthDeed),
            typeof(CupidStatue), typeof(EnchantedWheelbarrow), typeof(SorcerersRewardChest), typeof(PottedPlantDeed), typeof(RoseRugAddonDeed), typeof(TurkeyStatueAddonDeed), typeof(CrossWhiteEAddonDeed),
             typeof(IceCrystals), typeof(FlamingScarecrow), typeof(Fur), typeof(GiantReplicaAcorn), typeof(GrimWarning),
            typeof(HangingLantern), typeof(RedHangingLantern), typeof(WhiteHangingLantern), typeof(IceCrystals),
            typeof(GlobeStarsAddonDeed), typeof(HarvestLampAddonDeed), typeof(HarvestCartAddonDeed), typeof(PottedColumbineAquaAddonDeed), typeof(PottedColumbineDkOrangeAddonDeed),
            typeof(PottedColumbineFushiaAddonDeed), typeof(PottedColumbineGreenAddonDeed), typeof(PottedColumbineLilacAddonDeed), typeof(PottedColumbineOrangeAddonDeed), typeof(PottedColumbinePeachAddonDeed),
            typeof(PottedColumbinePinkAddonDeed), typeof(PottedColumbinePurpleAddonDeed), typeof(PottedColumbineRedAddonDeed), typeof(PottedColumbineTealAddonDeed), typeof(PottedColumbineWhiteAddonDeed), typeof(PottedColumbineYellowAddonDeed),
            typeof(SkullsOnPike),  typeof(BurlyBoneDecor), typeof(BrightDaemonBloodDecor), typeof(BurstingBrimstoneDecor), typeof(MightyMandrakeDecor), typeof(PerfectBlackPearlDecor),
            typeof(PottedMumsAquaAddonDeed), typeof(PottedMumsBlueAddonDeed), typeof(PottedMumsGreenAddonDeed), typeof(PottedMumsLimeAddonDeed), typeof(PottedMumsOrangeAddonDeed), typeof(PottedMumsPeachAddonDeed),
            typeof(PottedMumsPinkAddonDeed), typeof(PottedMumsPurpleAddonDeed), typeof(PottedMumsRedAddonDeed), typeof(PottedMumsTealAddonDeed), typeof(PottedMumsWhiteAddonDeed), typeof(PottedMumsYellowAddonDeed),
            typeof(MiningCartDeed), typeof(AnkhOfSacrificeDeed), typeof(SkullRugAddonDeed), typeof(RoseRugAddonDeed), typeof(DolphinRugAddonDeed), typeof(KoiPondDeed),
            typeof(WaterWheelDeed), typeof(AllegiancePouch), typeof(GlobeStarsAddonDeed), typeof(EnergyGateAddonDeed), typeof(fishpondAddonDeed), typeof(EastgateDonAddonDeed), typeof(gardenwellAddonDeed),
            typeof(StoneAnkhDeed), typeof(BloodyPentagramDeed), typeof(LighthouseAddonDeed), typeof(RewardBrazierDeed), typeof(GadgetryTableAddonDeed),
            typeof(GadgetryTableAddonDeed), typeof(TreeStumpDeed), typeof(SheepStatueDeed), typeof(CoralTheOwl), typeof(FlamingHeadDeed), typeof(PottedCactusDeed),
        };

        public static Item SuperRandomDeco()
        {
            try
            {
                switch (Utility.Random(11))
                {
                    case 0: return DecoRelPor.RandomArty();
                    case 1: return DecoLoader.RandomArty();
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                        return _RandomDecoRara();
                }
                return _RandomDecoComum();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return SuperRandomDeco();
            }
        }

        public static Item RandomDecoRara(BaseCreature bc)
        {
            if(Shard.RP)
            {
                return Decos._RandomDecoRara();
            }
            return new ValeDecoracaoRara() { QuemDropou = bc == null ? "" : bc.Name ?? bc.GetType().Name };
        }

        public static Item RandomDeco(BaseCreature bc)
        {
            if(Shard.RP)
            {
                if(Utility.RandomDouble() > 0.05)
                {
                    return Decos._RandomDecoComum();
                }
                return Decos._RandomDecoRara();
            }

            if (Utility.RandomDouble() > 0.1)
                return new ValeDecoracaoComum() { QuemDropou = bc == null ? "" : bc.Name ?? bc.GetType().Name };
            return new ValeDecoracaoRara() { QuemDropou = bc == null ? "" : bc.Name ?? bc.GetType().Name };
        }

        public static Item _RandomDecoRara()
        {
            var item = rare_decos[Utility.Random(rare_decos.Length)];
            var itemInstance = Activator.CreateInstance(item) as Item;
            itemInstance.Movable = true;
            itemInstance.LootType = LootType.Regular;
            return itemInstance;
        }

        public static Item _RandomDecoComum()
        {
            var item = common_decos[Utility.Random(common_decos.Length)];
            var itemInstance = Activator.CreateInstance(item) as Item;
            itemInstance.Movable = true;
            itemInstance.LootType = LootType.Regular;
            return itemInstance;
        }

    }
}
