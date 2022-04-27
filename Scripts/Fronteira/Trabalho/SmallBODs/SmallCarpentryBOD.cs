using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Engines.BulkOrders
{
    public class SmallCarpentryBOD : SmallBOD
    {
        public static double[] m_CarpentryMaterialChances = new double[]
        {
            0.013718750, // None
            0.392968750, // Oak
            0.217187500, // Ash
            0.246875000, // Yew
            0.118750000, // Heartwood
            0.007500000, // Bloodwood
            0.003000000 // Frostwood
        };

        public override BODType BODType { get { return BODType.Carpentry; } }

        [Constructable]
        public SmallCarpentryBOD()
        {
            SmallBulkEntry[] entries;
            bool useMaterials = Utility.RandomDouble() < 0.2;

            entries = SmallBulkEntry.CarpentrySmalls;

            if (entries.Length > 0)
            {
                int amountMax = Utility.RandomList(10, 15, 20);

                BulkMaterialType material = BulkMaterialType.None;

                if (useMaterials)
                    material = GetRandomMaterial(BulkMaterialType.Carvalho, m_CarpentryMaterialChances);

                bool reqExceptional = Utility.RandomBool() || (material == BulkMaterialType.None);

                SmallBulkEntry entry = entries[Utility.Random(entries.Length)];

                this.Hue = 1512;
                this.AmountMax = amountMax;
                this.Type = entry.Type;
                this.Number = entry.Number;
                this.Graphic = entry.Graphic;
                this.RequireExceptional = reqExceptional;
                this.Material = material;
                this.GraphicHue = entry.Hue;
            }
        }

        public SmallCarpentryBOD(int amountCur, int amountMax, Type type, int number, int graphic, bool reqExceptional, BulkMaterialType mat, int hue)
        {
            this.Hue = 1512;
            this.AmountMax = amountMax;
            this.AmountCur = amountCur;
            this.Type = type;
            this.Number = number;
            this.Graphic = graphic;
            this.RequireExceptional = reqExceptional;
            this.Material = mat;
            this.GraphicHue = hue;
        }

        public SmallCarpentryBOD(Serial serial)
            : base(serial)
        {
        }

        private SmallCarpentryBOD(SmallBulkEntry entry, BulkMaterialType material, int amountMax, bool reqExceptional)
        {
            this.Hue = 1512;
            this.AmountMax = amountMax;
            this.Type = entry.Type;
            this.Number = entry.Number;
            this.Graphic = entry.Graphic;
            this.RequireExceptional = reqExceptional;
            this.Material = material;
        }

        public static SmallCarpentryBOD CreateRandomFor(Mobile m)
        {
            SmallBulkEntry[] entries;
            bool useMaterials = Utility.RandomDouble() < 0.3;

            double theirSkill = BulkOrderSystem.GetBODSkill(m, SkillName.Carpentry);

            entries = SmallBulkEntry.CarpentrySmalls;

            if (entries.Length > 0)
            {
                int amountMax;

                BulkMaterialType material = BulkMaterialType.None;

                if (useMaterials && theirSkill >= 70.1)
                {
                    for (int i = 0; i < 20; ++i)
                    {
                        BulkMaterialType check = GetRandomMaterial(BulkMaterialType.Carvalho, m_CarpentryMaterialChances);
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

                CraftSystem system = DefCarpentry.CraftSystem;

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

                        if (allRequiredSkills && chance >= 0.5)
                        {
                            if (reqExceptional)
                                chance = item.GetExceptionalChance(system, chance, m);

                            if (chance > 0.5)
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
                    if (chance <= 0.5 && material != BulkMaterialType.None)
                        amountMax /= 2;

                    return new SmallCarpentryBOD(entry, material, amountMax, reqExceptional);
                }
            }

            return null;
        }

        public override int ComputeFame()
        {
            return CarpentryRewardCalculator.Instance.ComputeFame(this);
        }

        public override int ComputeGold()
        {
            return CarpentryRewardCalculator.Instance.ComputeGold(this);
        }

        public override List<Item> ComputeRewards(bool full)
        {
            return null;
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
