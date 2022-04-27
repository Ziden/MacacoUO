using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System.Linq;
using Server.Services.Harvest;

namespace Server.Engines.Harvest
{
    public class Mining : HarvestSystem
    {
        private static Mining m_System;

        public static Mining System
        {
            get
            {
                if (m_System == null)
                    m_System = new Mining();

                return m_System;
            }
        }

        private readonly HarvestDefinition m_OreAndStone;

        private readonly HarvestDefinition m_Sand;

        public HarvestDefinition OreAndStone
        {
            get
            {
                return this.m_OreAndStone;
            }
        }

        public HarvestDefinition Sand
        {
            get
            {
                return this.m_Sand;
            }
        }
        public static Dificuldade FERRO = new Dificuldade(0, 60);
        public static Dificuldade COBRE = new Dificuldade(60, 80);
        public static Dificuldade BRONZE = new Dificuldade(65, 85);
        public static Dificuldade DOURADO = new Dificuldade(75, 90);
        public static Dificuldade NIOBIO = new Dificuldade(80, 95);
        public static Dificuldade LAZURITA = new Dificuldade(90, 100);
        public static Dificuldade QUARTZO = new Dificuldade(100, 105);
        public static Dificuldade BERILO = new Dificuldade(100, 110);
        public static Dificuldade VIBRANIUM = new Dificuldade(110, 125);
        public static Dificuldade ADAMANTIUM = new Dificuldade(120, 135);

        private Mining()
        {
            HarvestResource[] res;
            HarvestVein[] veins;

            #region Mining for ore and stone
            HarvestDefinition oreAndStone = this.m_OreAndStone = new HarvestDefinition();

            // Resource banks are every 8x8 tiles
            oreAndStone.BankWidth = 4;
            oreAndStone.BankHeight = 4;

            // Every bank holds from 10 to 34 ore
            oreAndStone.MinTotal = 40;
            oreAndStone.MaxTotal = 60;

            // A resource bank will respawn its content every 10 to 20 minutes - Alterado de 15 para até 40 a fim de evitar que o personagem fique sempre aguardando o respawn.
            oreAndStone.MinRespawn = TimeSpan.FromMinutes(10.0);
            oreAndStone.MaxRespawn = TimeSpan.FromMinutes(20.0);

            // Skill checking is done on the Mining skill
            oreAndStone.Skill = SkillName.Mining;

            // Set the list of harvestable tiles
            oreAndStone.Tiles = m_MountainAndCaveTiles;

            // Players must be within 2 tiles to harvest
            oreAndStone.MaxRange = 2;

            // One ore per harvest action
            oreAndStone.ConsumedPerHarvest = 1;
            oreAndStone.ConsumedPerFeluccaHarvest = 1;

            // The digging effect
            oreAndStone.EffectActions = new int[] { 3 };
            oreAndStone.EffectSounds = new int[] { 0x125, 0x126 };
            //oreAndStone.EffectCounts = new int[] { 1 };
            oreAndStone.EffectCounts = new int[] { 2 };
            oreAndStone.EffectDelay = TimeSpan.FromSeconds(2);
            oreAndStone.EffectSoundDelay = TimeSpan.FromSeconds(0.9);

            oreAndStone.NoResourcesMessage = "Não tem mais minério neste local"; // There is no metal here to mine.
            oreAndStone.DoubleHarvestMessage = "Alguém pegou o minério antes de você"; // Someone has gotten to the metal before you.
            oreAndStone.TimedOutOfRangeMessage = "Você se moveu e parou de minerar"; // You have moved too far away to continue mining.
            oreAndStone.OutOfRangeMessage = "Isto está muito longe"; // That is too far away.
            oreAndStone.FailMessage = "Você não encontrou nenhum minério"; // You loosen some rocks but fail to find any useable ore.
            oreAndStone.PackFullMessage = "Sua mochila está cheia e o item foi jogado no chão"; // Your backpack is full, so the ore you mined is lost.
            oreAndStone.ToolBrokeMessage = "Sua ferramenta quebrou!"; // You have worn out your tool!

            res = new HarvestResource[]
            {
              new HarvestResource(00.0, 00.0, 90.0, "Você encontrou minério de Ferro e colocou em sua mochila", typeof(IronOre), typeof(Granite)),
                new HarvestResource(COBRE.Required, COBRE.Min, COBRE.Max, "Você encontrou minério de Cobre e colocou em sua mochila", typeof(CopperOre), typeof(CopperGranite), typeof(CopperElemental)),
                new HarvestResource(BRONZE.Required, BRONZE.Min, BRONZE.Max, "Você encontrou minério de Bronze e colocou em sua mochila", typeof(BronzeOre), typeof(BronzeGranite), typeof(BronzeElemental)),
                new HarvestResource(DOURADO.Required, DOURADO.Min, DOURADO.Max, "Você encontrou minério de Dourado e colocou em sua mochila", typeof(SilverOre), typeof(GoldGranite), typeof(GoldenElemental)),
                new HarvestResource(NIOBIO.Required, NIOBIO.Min, NIOBIO.Max, "Você encontrou minério de Nióbio e colocou em sua mochila", typeof(NiobioOre), typeof(AgapiteGranite), typeof(AgapiteElemental)),
                new HarvestResource(LAZURITA.Required, LAZURITA.Min, LAZURITA.Max, "Você encontrou minério de Lazurita e colocou em sua mochila", typeof(LazuritaOre), typeof(VeriteGranite), typeof(VeriteElemental)),
                new HarvestResource(QUARTZO.Required, QUARTZO.Min, QUARTZO.Max, "Você encontrou minério de Quartzo e colocou em sua mochila", typeof(QuartzoOre), typeof(ValoriteGranite), typeof(ValoriteElemental)),
                new HarvestResource(BERILO.Required, BERILO.Min, BERILO.Max, "Você encontrou minério de Berillo e colocou em sua mochila", typeof(BeriloOre),    typeof(DullCopperGranite), typeof(DullCopperElemental)),
                new HarvestResource(VIBRANIUM.Required, VIBRANIUM.Min, VIBRANIUM.Max, "Você encontrou minério de Vibranium e colocou em sua mochila", typeof(VibraniumOre), typeof(ShadowIronGranite), typeof(ShadowIronElemental)),
                 new HarvestResource(ADAMANTIUM.Required, ADAMANTIUM.Min, ADAMANTIUM.Max, "Você encontrou minério de Adamantium e colocou em sua mochila", typeof(AdamantiumOre), typeof(ValoriteGranite), typeof(AdamantiumElemental)),
            };

            veins = new HarvestVein[]
            {
                new HarvestVein(92.9, 0.0, res[0], null),   // Iron
                new HarvestVein(5, 0.5, res[1], res[0]), // cobre
                new HarvestVein(1.5, 0.5, res[2], res[0]), // bronze
                new HarvestVein(0.5, 0.5, res[3], res[0]), // prata
                new HarvestVein(0.1, 0.5, res[4], res[0]), // niobio               // aqui estão os rates que, a princípio, serão mantidos pro beta/realease. Desativados pro alfa.
                new HarvestVein(0, 0.5, res[5], res[0]), // lazurita
                new HarvestVein(00, 0.5, res[6], res[0]), // quartzo
                new HarvestVein(00, 0.5, res[7], res[0]), // berillo
                new HarvestVein(0, 0.5, res[8], res[0]),  // vibranium
                new HarvestVein(0, 0.5, res[9], res[0])  // adamantium
            };

            oreAndStone.Resources = res;
            oreAndStone.Veins = veins;

            // if (Core.ML)
            // {
            oreAndStone.BonusResources = new BonusHarvestResource[]
            {
                    new BonusHarvestResource(0, 99.84, null, null), //Nothing
                    new BonusHarvestResource(100, .02, "Voce achou uma pedra preciosa", typeof(Diamond)),
                    new BonusHarvestResource(100, .02, 1072567, typeof(StarSapphire)),
                    new BonusHarvestResource(100, .02, 1072570, typeof(Citrine)),
                    new BonusHarvestResource(100, .02, 1072564, typeof(Ruby)),
                    new BonusHarvestResource(100, .02, 1072566, typeof(Emerald)),
                    new BonusHarvestResource(100, .02, 1072568, typeof(Turquoise)),
                    new BonusHarvestResource(100, .02, 1077180, typeof(SmallPieceofBlackrock)),
                    new BonusHarvestResource(100, .02, 1113344, typeof(CrystallineBlackrock))
            };
            //  }

            oreAndStone.RaceBonus = Core.ML;
            oreAndStone.RandomizeVeins = true;

            this.Definitions.Add(oreAndStone);
            #endregion

            #region Mining for sand
            HarvestDefinition sand = this.m_Sand = new HarvestDefinition();

            // Resource banks are every 8x8 tiles
            sand.BankWidth = 8;
            sand.BankHeight = 8;

            // Every bank holds from 6 to 12 sand
            sand.MinTotal = 6;
            sand.MaxTotal = 13;

            // A resource bank will respawn its content every 10 to 20 minutes
            sand.MinRespawn = TimeSpan.FromMinutes(10.0);
            sand.MaxRespawn = TimeSpan.FromMinutes(20.0);

            // Skill checking is done on the Mining skill
            sand.Skill = SkillName.Mining;

            // Set the list of harvestable tiles
            sand.Tiles = m_SandTiles;

            // Players must be within 2 tiles to harvest
            sand.MaxRange = 2;

            // One sand per harvest action
            sand.ConsumedPerHarvest = 1;
            sand.ConsumedPerFeluccaHarvest = 1;

            // The digging effect
            sand.EffectActions = new int[] { 3 };
            sand.EffectSounds = new int[] { 0x125, 0x126 };
            //oreAndStone.EffectCounts = new int[] { 1 };
            sand.EffectCounts = new int[] { 2 };
            sand.EffectDelay = TimeSpan.FromSeconds(1.2);
            sand.EffectSoundDelay = TimeSpan.FromSeconds(0.9);

            /*oreAndStone.NoResourcesMessage = "Nao tem minerio aqui"; // There is no metal here to mine.
            oreAndStone.DoubleHarvestMessage = "Alguem pegou seu minerio"; // Someone has gotten to the metal before you.
            oreAndStone.TimedOutOfRangeMessage = "Voce esta muito longe do minerio"; // You have moved too far away to continue mining.
            oreAndStone.OutOfRangeMessage = "Isto esta muito longe"; // That is too far away.
            oreAndStone.FailMessage = "Voce nao conseguiu encontrar minerio"; // You loosen some rocks but fail to find any useable ore.
            oreAndStone.PackFullMessage = "Sua mochila esta cheia"; // Your backpack is full, so the ore you mined is lost.
            oreAndStone.ToolBrokeMessage = "Sua ferramenta quebrou !"; // You have worn out your tool!*/

            sand.NoResourcesMessage = "Não tem mais areia neste local"; // There is no metal here to mine.
            sand.DoubleHarvestMessage = "Alguém pegou o areia antes de você"; // Someone has gotten to the metal before you.
            sand.TimedOutOfRangeMessage = "Você se moveu e parou de minerar"; // You have moved too far away to continue mining.
            sand.OutOfRangeMessage = "Isto está muito longe"; // That is too far away.
            sand.FailMessage = "Você não encontrou nenhuma areia"; // You loosen some rocks but fail to find any useable ore.
            sand.PackFullMessage = "Sua mochila está cheia e o item foi jogado no chão"; // Your backpack is full, so the ore you mined is lost.
            sand.ToolBrokeMessage = "Sua ferramenta quebrou!"; // You have worn out your tool!

            res = new HarvestResource[]
            {
                new HarvestResource(70, 65.0, 100.0, 1044631, typeof(Sand))
            };

            veins = new HarvestVein[]
            {
                new HarvestVein(100.0, 0.0, res[0], null)
            };

            sand.Resources = res;
            sand.Veins = veins;

            this.Definitions.Add(sand);
            #endregion
        }


        public override Type GetResourceType(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource)
        {
            if (def == this.m_OreAndStone)
            {
                #region Void Pool Items
                HarvestMap hmap = HarvestMap.CheckMapOnHarvest(from, loc, def);

                if (hmap != null && hmap.Resource >= CraftResource.Ferro && hmap.Resource <= CraftResource.Quartzo)
                {
                    hmap.UsesRemaining--;
                    hmap.InvalidateProperties();

                    CraftResourceInfo info = CraftResources.GetInfo(hmap.Resource);

                    if (info != null)
                        return info.ResourceTypes[1];
                }
                #endregion

                PlayerMobile pm = from as PlayerMobile;

                if (pm != null && pm.GemMining && pm.ToggleMiningGem && from.Skills[SkillName.Mining].Base >= 100.0 && 0.1 > Utility.RandomDouble())
                    return Loot.GemTypes[Utility.Random(Loot.GemTypes.Length)];

                double chance = tool is RockHammer ? 0.50 : 0.15;

                if (pm != null && pm.StoneMining && (pm.ToggleMiningStone || pm.ToggleStoneOnly) && from.Skills[SkillName.Mining].Base >= 100.0 && chance > Utility.RandomDouble())
                    return resource.Types[1];

                if (pm != null && pm.ToggleStoneOnly)
                {
                    return null;
                }

                return resource.Types[0];
            }

            return base.GetResourceType(from, tool, def, map, loc, resource);
        }

        public override void SendSuccessTo(Mobile from, Item item, HarvestResource resource)
        {
            if (item is BaseGranite)
                from.SendLocalizedMessage(1044606); // You carefully extract some workable stone from the ore vein!
            else if (item is IGem)
                from.SendLocalizedMessage(1112233); // You carefully extract a glistening gem from the vein!
            else if (item != null)
            {
                foreach (var res in OreAndStone.Resources.Where(r => r.Types != null))
                {
                    foreach (var type in res.Types)
                    {
                        if (item.GetType() == type)
                        {
                            res.SendSuccessTo(from);
                            return;
                        }
                    }
                }

                base.SendSuccessTo(from, item, resource);
            }
        }

        public override bool CheckResources(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
        {
            if (HarvestMap.CheckMapOnHarvest(from, loc, def) == null)
                return base.CheckResources(from, tool, def, map, loc, timed);

            return true;
        }

        public override bool CheckHarvest(Mobile from, Item tool)
        {
            if (!base.CheckHarvest(from, tool))
                return false;

            if (from.IsBodyMod && !from.Body.IsHuman)
            {
                from.SendLocalizedMessage(501865); // You can't mine while polymorphed.
                return false;
            }

            return true;
        }

        public override bool CheckHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            if (!base.CheckHarvest(from, tool, def, toHarvest))
                return false;

            if (def == this.m_Sand && !(from is PlayerMobile && from.Skills[SkillName.Mining].Base >= 60 && ((PlayerMobile)from).SandMining))
            {
                this.OnBadHarvestTarget(from, tool, toHarvest);
                return false;
            }
            else if (from.IsBodyMod && !from.Body.IsHuman)
            {
                from.SendLocalizedMessage(501865); // You can't mine while polymorphed.
                return false;
            }

            return true;
        }

        public override HarvestVein MutateVein(Mobile from, Item tool, HarvestDefinition def, HarvestBank bank, object toHarvest, HarvestVein vein)
        {
            if (tool is GargoylesPickaxe && def == this.m_OreAndStone)
            {
                int veinIndex = Array.IndexOf(def.Veins, vein);

                if (veinIndex >= 0 && veinIndex < (def.Veins.Length - 1))
                    return def.Veins[veinIndex + 1];
            }

            return base.MutateVein(from, tool, def, bank, toHarvest, vein);
        }

        private static readonly int[] m_Offsets = new int[]
        {
            -1, -1,
            -1, 0,
            -1, 1,
            0, -1,
            0, 1,
            1, -1,
            1, 0,
            1, 1
        };

        public override void OnHarvestFinished(Mobile from, Item tool, HarvestDefinition def, HarvestVein vein, HarvestBank bank, HarvestResource resource, object harvested)
        {
            if (tool is GargoylesPickaxe && def == this.m_OreAndStone && 0.1 > Utility.RandomDouble() && HarvestMap.CheckMapOnHarvest(from, harvested, def) == null)
            {
                HarvestResource res = vein.PrimaryResource;

                if (res == resource && res.Types.Length >= 3)
                {
                    try
                    {
                        Map map = from.Map;

                        if (map == null)
                            return;

                        BaseCreature spawned = Activator.CreateInstance(res.Types[2], new object[] { 25 }) as BaseCreature;

                        if (spawned != null)
                        {
                            int offset = Utility.Random(8) * 2;

                            for (int i = 0; i < m_Offsets.Length; i += 2)
                            {
                                int x = from.X + m_Offsets[(offset + i) % m_Offsets.Length];
                                int y = from.Y + m_Offsets[(offset + i + 1) % m_Offsets.Length];

                                if (map.CanSpawnMobile(x, y, from.Z))
                                {
                                    spawned.OnBeforeSpawn(new Point3D(x, y, from.Z), map);
                                    spawned.MoveToWorld(new Point3D(x, y, from.Z), map);
                                    spawned.Combatant = from;
                                    return;
                                }
                                else
                                {
                                    int z = map.GetAverageZ(x, y);

                                    if (Math.Abs(z - from.Z) < 10 && map.CanSpawnMobile(x, y, z))
                                    {
                                        spawned.OnBeforeSpawn(new Point3D(x, y, z), map);
                                        spawned.MoveToWorld(new Point3D(x, y, z), map);
                                        spawned.Combatant = from;
                                        return;
                                    }
                                }
                            }

                            spawned.OnBeforeSpawn(from.Location, from.Map);
                            spawned.MoveToWorld(from.Location, from.Map);
                            spawned.Combatant = from;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            //base.OnHarvestStarted(from, tool, def, resource);
        }

        #region High Seas
        public override bool SpecialHarvest(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc)
        {
            if (!Core.HS)
                return base.SpecialHarvest(from, tool, def, map, loc);

            HarvestBank bank = def.GetBank(map, loc.X, loc.Y);

            if (bank == null)
                return false;

            bool boat = Server.Multis.BaseBoat.FindBoatAt(from, from.Map) != null;
            bool dungeon = IsDungeonRegion(from);

            if (!boat && !dungeon)
                return false;

            if (boat || !NiterDeposit.HasBeenChecked(bank))
            {
                int luck = from is PlayerMobile ? ((PlayerMobile)from).RealLuck : from.Luck;
                double bonus = (from.Skills[SkillName.Mining].Value / 9999) + ((double)luck / 150000);

                if (boat)
                    bonus -= (bonus * .33);

                if (dungeon)
                    NiterDeposit.AddBank(bank);

                if (Utility.RandomDouble() < bonus)
                {
                    int size = Utility.RandomMinMax(1, 5);

                    if (luck / 2500.0 > Utility.RandomDouble())
                        size++;

                    NiterDeposit niter = new NiterDeposit(size);

                    if (!dungeon)
                    {
                        niter.MoveToWorld(new Point3D(loc.X, loc.Y, from.Z + 3), from.Map);
                        from.SendLocalizedMessage(1149918, niter.Size.ToString()); //You have uncovered a ~1_SIZE~ deposit of niter! Mine it to obtain saltpeter.
                        NiterDeposit.AddBank(bank);
                        return true;
                    }
                    else
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            int x = Utility.RandomMinMax(loc.X - 2, loc.X + 2);
                            int y = Utility.RandomMinMax(loc.Y - 2, loc.Y + 2);
                            int z = from.Z;

                            if (from.Map.CanSpawnMobile(x, y, z))
                            {
                                niter.MoveToWorld(new Point3D(x, y, z), from.Map);
                                from.SendLocalizedMessage(1149918, niter.Size.ToString()); //You have uncovered a ~1_SIZE~ deposit of niter! Mine it to obtain saltpeter.
                                return true;
                            }
                        }
                    }

                    niter.Delete();
                }
            }

            return false;
        }

        private bool IsDungeonRegion(Mobile from)
        {
            if (from == null)
                return false;

            Map map = from.Map;
            Region reg = from.Region;
            Rectangle2D bounds = new Rectangle2D(0, 0, 5114, 4100);

            if ((map == Map.Felucca || map == Map.Trammel) && bounds.Contains(new Point2D(from.X, from.Y)))
                return false;

            return reg != null && (reg.IsPartOf<Server.Regions.DungeonRegion>() || map == Map.Ilshenar);
        }
        #endregion

        public override bool BeginHarvesting(Mobile from, Item tool)
        {
            if (!base.BeginHarvesting(from, tool))
                return false;

            //from.SendLocalizedMessage(503033); // Where do you wish to dig?
            from.SendMessage("Onde você gostaria de minerar?");
            return true;
        }

        public override void OnHarvestStarted(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            base.OnHarvestStarted(from, tool, def, toHarvest);

            from.RevealingAction();
        }

        public override void OnBadHarvestTarget(Mobile from, Item tool, object toHarvest)
        {
            if (toHarvest is LandTarget)
            {
                //from.SendLocalizedMessage(501862); // You can't mine there.
                from.SendMessage("Você não pode minerar neste local.");
            }
            else if (!(toHarvest is LandTarget))
            {
                //from.SendLocalizedMessage(501863); // You can't mine that.
                from.SendMessage("Você não pode minerar isto.");
            }
            else if (from.Mounted || from.Flying)
            {
                from.SendMessage("Você não pode minerar montado."); // You can't dig while riding or flying.
            }
        }

        #region Tile lists
        private static readonly int[] m_MountainAndCaveTiles = new int[]
        {
            220, 221, 222, 223, 224, 225, 226, 227, 228, 229,
            230, 231, 236, 237, 238, 239, 240, 241, 242, 243,
            244, 245, 246, 247, 252, 253, 254, 255, 256, 257,
            258, 259, 260, 261, 262, 263, 268, 269, 270, 271,
            272, 273, 274, 275, 276, 277, 278, 279, 286, 287,
            288, 289, 290, 291, 292, 293, 294, 296, 296, 297,
            321, 322, 323, 324, 467, 468, 469, 470, 471, 472,
            473, 474, 476, 477, 478, 479, 480, 481, 482, 483,
            484, 485, 486, 487, 492, 493, 494, 495, 543, 544,
            545, 546, 547, 548, 549, 550, 551, 552, 553, 554,
            555, 556, 557, 558, 559, 560, 561, 562, 563, 564,
            565, 566, 567, 568, 569, 570, 571, 572, 573, 574,
            575, 576, 577, 578, 579, 581, 582, 583, 584, 585,
            586, 587, 588, 589, 590, 591, 592, 593, 594, 595,
            596, 597, 598, 599, 600, 601, 610, 611, 612, 613,
            1010, 1741, 1742, 1743, 1744, 1745, 1746, 1747, 1748, 1749,
            1750, 1751, 1752, 1753, 1754, 1755, 1756, 1757, 1771, 1772,
            1773, 1774, 1775, 1776, 1777, 1778, 1779, 1780, 1781, 1782,
            1783, 1784, 1785, 1786, 1787, 1788, 1789, 1790, 1801, 1802,
            1803, 1804, 1805, 1806, 1807, 1808, 1809, 1811, 1812, 1813,
            1814, 1815, 1816, 1817, 1818, 1819, 1820, 1821, 1822, 1823,
            1824, 1831, 1832, 1833, 1834, 1835, 1836, 1837, 1838, 1839,
            1840, 1841, 1842, 1843, 1844, 1845, 1846, 1847, 1848, 1849,
            1850, 1851, 1852, 1853, 1854, 1861, 1862, 1863, 1864, 1865,
            1866, 1867, 1868, 1869, 1870, 1871, 1872, 1873, 1874, 1875,
            1876, 1877, 1878, 1879, 1880, 1881, 1882, 1883, 1884, 1981,
            1982, 1983, 1984, 1985, 1986, 1987, 1988, 1989, 1990, 1991,
            1992, 1993, 1994, 1995, 1996, 1997, 1998, 1999, 2000, 2001,
            2002, 2003, 2004, 2028, 2029, 2030, 2031, 2032, 2033, 2100,
            2101, 2102, 2103, 2104, 2105,
            0x453B, 0x453C, 0x453D, 0x453E, 0x453F, 0x4540, 0x4541,
            0x4542, 0x4543, 0x4544, 0x4545, 0x4546, 0x4547, 0x4548,
            0x4549, 0x454A, 0x454B, 0x454C, 0x454D, 0x454E, 0x454F
        };

        private static readonly int[] m_SandTiles = new int[]
        {
            22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
            32, 33, 34, 35, 36, 37, 38, 39, 40, 41,
            42, 43, 44, 45, 46, 47, 48, 49, 50, 51,
            52, 53, 54, 55, 56, 57, 58, 59, 60, 61,
            62, 68, 69, 70, 71, 72, 73, 74, 75,
            286, 287, 288, 289, 290, 291, 292, 293, 294, 295,
            296, 297, 298, 299, 300, 301, 402, 424, 425, 426,
            427, 441, 442, 443, 444, 445, 446, 447, 448, 449,
            450, 451, 452, 453, 454, 455, 456, 457, 458, 459,
            460, 461, 462, 463, 464, 465, 642, 643, 644, 645,
            650, 651, 652, 653, 654, 655, 656, 657, 821, 822,
            823, 824, 825, 826, 827, 828, 833, 834, 835, 836,
            845, 846, 847, 848, 849, 850, 851, 852, 857, 858,
            859, 860, 951, 952, 953, 954, 955, 956, 957, 958,
            967, 968, 969, 970,
            1447, 1448, 1449, 1450, 1451, 1452, 1453, 1454, 1455,
            1456, 1457, 1458, 1611, 1612, 1613, 1614, 1615, 1616,
            1617, 1618, 1623, 1624, 1625, 1626, 1635, 1636, 1637,
            1638, 1639, 1640, 1641, 1642, 1647, 1648, 1649, 1650
        };
        #endregion
    }
}
