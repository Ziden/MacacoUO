using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Engines.BulkOrders
{
    public class SmallTailorBOD : SmallBOD
    {
        public override BODType BODType { get { return BODType.Tailor; } }

        public static double[] m_TailoringMaterialChances = new double[]
        {
            0.357421875, // None
            0.325000000, // Spined
            0.215625000, // Horned
            0.101953125  // Barbed
        };
        [Constructable]
        public SmallTailorBOD()
        {
            SmallBulkEntry[] entries;
            bool useMaterials;

            if (useMaterials = Utility.RandomBool())
                entries = SmallBulkEntry.TailorLeather;
            else
                entries = SmallBulkEntry.TailorCloth;

            if (entries.Length > 0)
            {
                int hue = 0x483;
                int amountMax = Utility.RandomList(10, 15, 20);

                BulkMaterialType material;

                if (useMaterials)
                    material = GetRandomMaterial(BulkMaterialType.Spined, m_TailoringMaterialChances);
                else
                    material = BulkMaterialType.None;

                bool reqExceptional = Utility.RandomBool() || (material == BulkMaterialType.None);

                SmallBulkEntry entry = entries[Utility.Random(entries.Length)];

                this.Hue = hue;
                this.AmountMax = amountMax;
                this.Type = entry.Type;
                this.Number = entry.Number;
                this.Graphic = entry.Graphic;
                this.RequireExceptional = reqExceptional;
                this.Material = material;
                this.GraphicHue = entry.Hue;
            }
        }

        public SmallTailorBOD(int amountCur, int amountMax, Type type, int number, int graphic, bool reqExceptional, BulkMaterialType mat, int hue)
        {
            this.Hue = 0x483;
            this.AmountMax = amountMax;
            this.AmountCur = amountCur;
            this.Type = type;
            this.Number = number;
            this.Graphic = graphic;
            this.RequireExceptional = reqExceptional;
            this.Material = mat;
            this.GraphicHue = hue;
        }

        public SmallTailorBOD(Serial serial)
            : base(serial)
        {
        }

        private SmallTailorBOD(SmallBulkEntry entry, BulkMaterialType material, int amountMax, bool reqExceptional)
        {
            this.Hue = 0x483;
            this.AmountMax = amountMax;
            this.Type = entry.Type;
            this.Number = entry.Number;
            this.Graphic = entry.Graphic;
            this.RequireExceptional = reqExceptional;
            this.Material = material;
        }

        public static SmallTailorBOD CreateRandomFor(Mobile m)
        {
            SmallBulkEntry[] entries;
            bool useMaterials = Utility.RandomBool();

            double theirSkill = BulkOrderSystem.GetBODSkill(m, SkillName.Tailoring);
            if (useMaterials && theirSkill >= 15) // Ugly, but the easiest leather BOD is Leather Cap which requires at least 6.2 skill.
                entries = SmallBulkEntry.TailorLeather;
            else
                entries = SmallBulkEntry.TailorCloth;

            if (entries.Length > 0)
            {
                int amountMax;



                BulkMaterialType material = BulkMaterialType.None;

                if (useMaterials && theirSkill >= 70.1)
                {
                    for (int i = 0; i < 20; ++i)
                    {
                        BulkMaterialType check = GetRandomMaterial(BulkMaterialType.Spined, m_TailoringMaterialChances);
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

                bool reqExceptional = (excChance > Utility.RandomDouble());

                CraftSystem system = DefTailoring.CraftSystem;

                Type res = null;
                var resource = SmallBOD.GetResource(material);
                res = CraftResources.GetFromResource(resource);

                List<SmallBulkEntry> validEntries = new List<SmallBulkEntry>();

                for (int i = 0; i < entries.Length; ++i)
                {
                    CraftItem item = system.CraftItems.SearchFor(entries[i].Type);

                    if (item != null)
                    {
                        bool allRequiredSkills = true;
                        double chance = item.GetSuccessChance(m, res, system, false, ref allRequiredSkills);

                        if (allRequiredSkills && chance >= 0.4)
                        {
                            if (reqExceptional)
                                chance = item.GetExceptionalChance(system, chance, m);

                            if (chance > 0.4)
                                validEntries.Add(entries[i]);
                        }
                    }
                }

                if (validEntries.Count > 0)
                {
                    SmallBulkEntry entry = validEntries[Utility.Random(validEntries.Count)];
                    CraftItem item = system.CraftItems.SearchFor(entry.Type);
                    bool b = false;
                    double chance = item.GetSuccessChance(m, res, system, false, ref b);
                    if (chance <= 0.7 && material != BulkMaterialType.None)
                        amountMax /= 2;
                    return new SmallTailorBOD(entry, material, amountMax, reqExceptional);
                }
            }

            return null;
        }

        public override int ComputeFame()
        {
            return TailorRewardCalculator.Instance.ComputeFame(this);
        }

        public override int ComputeGold()
        {
            return TailorRewardCalculator.Instance.ComputeGold(this);
        }

        public override List<Item> ComputeRewards(bool full)
        {
            List<Item> list = new List<Item>();

            RewardGroup rewardGroup = TailorRewardCalculator.Instance.LookupRewards(TailorRewardCalculator.Instance.ComputePoints(this));

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
