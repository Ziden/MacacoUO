using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Engines.BulkOrders
{
    [TypeAlias("Scripts.Engines.BulkOrders.SmallSmithBOD")]
    public class SmallSmithBOD : SmallBOD
    {
        public override BODType BODType { get { return BODType.Smith; } }

        public static double[] m_BlacksmithMaterialChances = new double[]
        {
            0.001953125, // None
            0.400000000, // Dull Copper
            0.375000000, // Shadow Iron
            0.332500000, // Copper
            0.231250000, // Bronze
            0.115625000, // Gold
            0.057812500, // Agapite
            0.023906250, // Verite
            0.011953125  // Valorite
        };
        [Constructable]
        public SmallSmithBOD()
        {
            SmallBulkEntry[] entries;
            var useMaterials = Utility.RandomBool();
            if (Utility.RandomBool())
                entries = SmallBulkEntry.BlacksmithArmor;
            else
                entries = SmallBulkEntry.BlacksmithWeapons;

            if (entries.Length > 0)
            {
                int hue = 0x44E;
                int amountMax = Utility.RandomList(10, 15, 20);

                BulkMaterialType material;

                if (useMaterials)
                    material = GetRandomMaterial(BulkMaterialType.Cobre, m_BlacksmithMaterialChances);
                else
                    material = BulkMaterialType.None;

                bool reqExceptional = Utility.RandomBool() || (material == BulkMaterialType.None);

                SmallBulkEntry entry = entries[Utility.Random(entries.Length)];

                Hue = hue;
                AmountMax = amountMax;
                Type = entry.Type;
                Number = entry.Number;
                Graphic = entry.Graphic;
                RequireExceptional = reqExceptional;
                Material = material;
                GraphicHue = entry.Hue;
            }
        }

        public SmallSmithBOD(int amountCur, int amountMax, Type type, int number, int graphic, bool reqExceptional, BulkMaterialType mat, int hue)
        {
            Hue = 0x44E;
            AmountMax = amountMax;
            AmountCur = amountCur;
            Type = type;
            Number = number;
            Graphic = graphic;
            RequireExceptional = reqExceptional;
            Material = mat;
            GraphicHue = hue;
        }

        public SmallSmithBOD(Serial serial)
            : base(serial)
        {
        }

        private SmallSmithBOD(SmallBulkEntry entry, BulkMaterialType material, int amountMax, bool reqExceptional)
        {
            Hue = 0x44E;
            AmountMax = amountMax;
            Type = entry.Type;
            Number = entry.Number;
            Graphic = entry.Graphic;
            RequireExceptional = reqExceptional;
            Material = material;
        }

        public static SmallSmithBOD CreateRandomFor(Mobile m)
        {
            SmallBulkEntry[] entries;
            bool useMaterials;

            useMaterials = Utility.RandomDouble() < 0.3;

            if (Utility.RandomBool())
                entries = SmallBulkEntry.BlacksmithArmor;
            else
                entries = SmallBulkEntry.BlacksmithWeapons;

            if (entries.Length > 0)
            {
                double theirSkill = BulkOrderSystem.GetBODSkill(m, SkillName.Blacksmith);

                BulkMaterialType material = BulkMaterialType.None;

                if (useMaterials && theirSkill >= 70.1)
                {
                    for (int i = 0; i < 20; ++i)
                    {
                        BulkMaterialType check = GetRandomMaterial(BulkMaterialType.Cobre, m_BlacksmithMaterialChances);
                        double skillReq = GetRequiredSkill(check);
                        if (Shard.DebugEnabled)
                        {
                            Shard.Debug("Checando material " + check.ToString());
                            Shard.Debug("Skill " + theirSkill + " precisa de " + skillReq);
                        }
                        if (theirSkill >= skillReq)
                        {

                            material = check;
                            break;
                        }
                    }
                }

                int amountMax;

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

                if (theirSkill >= 80.1)
                    excChance = (theirSkill + 80.0) / 200.0;

                bool reqExceptional = (excChance > Utility.RandomDouble());

                CraftSystem system = DefBlacksmithy.CraftSystem;

                List<SmallBulkEntry> validEntries = new List<SmallBulkEntry>();

                Type res = null;
                var resource = SmallBOD.GetResource(material);
                res = CraftResources.GetFromResource(resource);

                for (int i = 0; i < entries.Length; ++i)
                {
                    CraftItem item = system.CraftItems.SearchFor(entries[i].Type);

                    if (item != null)
                    {
                        bool allRequiredSkills = true;
                        double chance = item.GetSuccessChance(m, res, system, false, ref allRequiredSkills);

                        if (allRequiredSkills && chance > 0.4)
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
                    double chance = item.GetSuccessChance(m, res, system, false, ref b);
                    if (chance <= 0.6 && material != BulkMaterialType.None)
                        amountMax /= 2;
                    return new SmallSmithBOD(entry, material, amountMax, reqExceptional);
                }
            }

            return null;
        }

        public override int ComputeFame()
        {
            return SmithRewardCalculator.Instance.ComputeFame(this);
        }

        public override int ComputeGold()
        {
            return SmithRewardCalculator.Instance.ComputeGold(this);
        }

        public override List<Item> ComputeRewards(bool full)
        {
            List<Item> list = new List<Item>();

            RewardGroup rewardGroup = SmithRewardCalculator.Instance.LookupRewards(SmithRewardCalculator.Instance.ComputePoints(this));

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
