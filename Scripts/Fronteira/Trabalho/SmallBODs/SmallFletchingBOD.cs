using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Engines.BulkOrders
{
    public class SmallFletchingBOD : SmallBOD
    {
        public static double[] m_FletchingMaterialChances = new double[]
        {
            0.113718750, // None
            0.3, // Oak
            0.2, // Ash
            0.1, // Yew
            0.05, // Heartwood
            0.03, // Bloodwood
            0.01 // Frostwood
        };

        public override BODType BODType { get { return BODType.Fletching; } }

        [Constructable]
        public SmallFletchingBOD()
        {
            SmallBulkEntry[] entries;
            bool useMaterials = false;

            if (0.20 > Utility.RandomDouble())
            {
                entries = SmallBulkEntry.FletchingSmallsRegular;
            }
            else
            {
                useMaterials = true;
                entries = SmallBulkEntry.FletchingSmalls;
            }

            if (entries.Length > 0)
            {
                 SmallBulkEntry entry = entries[Utility.Random(entries.Length)];

                int amountMax = Utility.RandomList(10, 15, 20);

                BulkMaterialType material;

                if (useMaterials)
                    material = GetRandomMaterial(BulkMaterialType.Carvalho, m_FletchingMaterialChances);
                else
                    material = BulkMaterialType.None;

                bool reqExceptional = false;

                if(useMaterials)
                    reqExceptional = Utility.RandomBool() || (material == BulkMaterialType.None);

                this.Hue = 1425;
                this.AmountMax = amountMax;
                this.Type = entry.Type;
                this.Number = entry.Number;
                this.Graphic = entry.Graphic;
                this.RequireExceptional = reqExceptional;
                this.Material = material;
                this.GraphicHue = entry.Hue;
            }
        }

        public SmallFletchingBOD(int amountCur, int amountMax, Type type, int number, int graphic, bool reqExceptional, BulkMaterialType mat, int hue)
        {
            this.Hue = 1425;
            this.AmountMax = amountMax;
            this.AmountCur = amountCur;
            this.Type = type;
            this.Number = number;
            this.Graphic = graphic;
            this.RequireExceptional = reqExceptional;
            this.Material = mat;
            this.GraphicHue = hue;
        }

        public SmallFletchingBOD(Serial serial)
            : base(serial)
        {
        }

        private SmallFletchingBOD(SmallBulkEntry entry, BulkMaterialType material, int amountMax, bool reqExceptional)
        {
            this.Hue = 1425;
            this.AmountMax = amountMax;
            this.Type = entry.Type;
            this.Number = entry.Number;
            this.Graphic = entry.Graphic;
            this.RequireExceptional = reqExceptional;
            this.Material = material;
        }

        public static SmallFletchingBOD CreateRandomFor(Mobile m)
        {
            SmallBulkEntry[] entries;

            double theirSkill = BulkOrderSystem.GetBODSkill(m, SkillName.Bowcraft);
            bool useMaterials = false;

            if (theirSkill < 30.0 || .20 > Utility.RandomDouble())
            {
                entries = SmallBulkEntry.FletchingSmallsRegular;
            }
            else
            {
                useMaterials = true;
                entries = SmallBulkEntry.FletchingSmalls;
            }

            if (entries.Length > 0)
            {
                int amountMax;

                BulkMaterialType material = BulkMaterialType.None;

                if (useMaterials && theirSkill >= 70.1)
                {
                    for (int i = 0; i < 20; ++i)
                    {
                        BulkMaterialType check = GetRandomMaterial(BulkMaterialType.Carvalho, m_FletchingMaterialChances);
                        double skillReq = GetRequiredSkill(check);

                        if (theirSkill >= skillReq)
                        {
                            material = check;
                            break;
                        }
                    }
                }

                if (material != BulkMaterialType.None)
                {
                    if (theirSkill >= 100)
                        amountMax = Utility.RandomList(20, 20, 25, 30);
                    else
                        amountMax = Utility.RandomList(10, 10, 15, 20);
                }
                else
                {
                    if (theirSkill >= 110)
                        amountMax = Utility.RandomList(75, 70, 85, 85);
                    else if (theirSkill >= 100)
                        amountMax = Utility.RandomList(55, 70, 65, 75);
                    else if (theirSkill >= 70.1)
                        amountMax = Utility.RandomList(35, 35, 40, 40);
                    else if (theirSkill >= 50.1)
                        amountMax = Utility.RandomList(15, 20, 20, 25);
                    else
                        amountMax = Utility.RandomList(10, 10, 15, 20);
                }

                double excChance = 0.0;

                if (theirSkill >= 70.1)
                    excChance = (theirSkill + 80.0) / 200.0;

                bool reqExceptional = useMaterials && excChance > Utility.RandomDouble();

                CraftSystem system = DefBowFletching.CraftSystem;

                List<SmallBulkEntry> validEntries = new List<SmallBulkEntry>();

                for (int i = 0; i < entries.Length; ++i)
                {
                    CraftItem item = system.CraftItems.SearchFor(entries[i].Type);

                    if (item != null)
                    {
                        bool allRequiredSkills = true;
                        double chance = item.GetSuccessChance(m, null, system, false, ref allRequiredSkills);

                        if (allRequiredSkills && chance >= 0.2)
                        {
                            if (reqExceptional)
                                chance = item.GetExceptionalChance(system, chance, m);

                            if (chance > 0.2)
                                validEntries.Add(entries[i]);
                        }
                    }
                }

                if (validEntries.Count > 0)
                {
                    SmallBulkEntry entry = validEntries[Utility.Random(validEntries.Count)];
                    CraftItem item = system.CraftItems.SearchFor(entry.Type);
                    bool b = false;
                    double chance = item.GetSuccessChance(m, null, system, false, ref b);
                    if (chance <= 0.5 && material != BulkMaterialType.None)
                        amountMax /= 2;
                    return new SmallFletchingBOD(entry, material, amountMax, reqExceptional);
                }
            }

            return null;
        }

        public override int ComputeFame()
        {
            return FletchingRewardCalculator.Instance.ComputeFame(this);
        }

        public override int ComputeGold()
        {
            return FletchingRewardCalculator.Instance.ComputeGold(this);
        }

        public override List<Item> ComputeRewards(bool full)
        {
            List<Item> list = new List<Item>();

            RewardGroup rewardGroup = FletchingRewardCalculator.Instance.LookupRewards(FletchingRewardCalculator.Instance.ComputePoints(this));

            if (rewardGroup != null)
            {
                if (full)
                {
                    for (int i = 0; i < rewardGroup.Items.Length; ++i)
                    {
                        Item item = rewardGroup.Items[i].Construct();

                        if (item != null)
                            list.Add(item);
                    }
                }
                else
                {
                    RewardItem rewardItem = rewardGroup.AcquireItem();

                    if (rewardItem != null)
                    {
                        Item item = rewardItem.Construct();

                        if (item != null)
                            list.Add(item);
                    }
                }
            }

            return list;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
