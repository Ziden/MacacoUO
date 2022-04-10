using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a terathan matriarch corpse")]
    public class TerathanMatriarch : BaseCreature
    {
        public override int BonusExp => 300;

        [Constructable]
        public TerathanMatriarch()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "terathan matriarca";
            this.Body = 72;
            this.BaseSoundID = 599;

            this.SetStr(316, 405);
            this.SetDex(96, 115);
            this.SetInt(366, 455);

            this.SetHits(2190, 2243);

            this.SetDamage(21, 34);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 45, 55);
            this.SetResistance(ResistanceType.Fire, 30, 40);
            this.SetResistance(ResistanceType.Cold, 35, 45);
            this.SetResistance(ResistanceType.Poison, 40, 50);
            this.SetResistance(ResistanceType.Energy, 35, 45);

            this.SetSkill(SkillName.EvalInt, 90.1, 100.0);
            this.SetSkill(SkillName.Magery, 90.1, 100.0);
            this.SetSkill(SkillName.MagicResist, 90.1, 100.0);
            this.SetSkill(SkillName.Tactics, 50.1, 70.0);
            this.SetSkill(SkillName.Wrestling, 60.1, 80.0);

            this.Fame = 15000;
            this.Karma = -15000;

            this.PackItem(new SpidersSilk(5));
            this.PackNecroReg(Utility.RandomMinMax(4, 10));

            PackItem(new T2ARecallRune());
            PackItem(new CristalTherathan());
        }

        public TerathanMatriarch(Serial serial)
            : base(serial)
        {
        }

        public override void AlterMeleeDamageTo(Mobile to, ref int damage)
        {
            base.AlterMeleeDamageTo(to, ref damage);
            if (to is BaseCreature)
                damage *= 4;
        }

        public override void AlterSpellDamageTo(Mobile to, ref int damage, ElementoPvM elemento)
        {
            base.AlterSpellDamageTo(to, ref damage, elemento);
            if (to is BaseCreature)
                damage *= 4;
        }

        public override int TreasureMapLevel
        {
            get
            {
                return 4;
            }
        }

        public override TribeType Tribe { get { return TribeType.Terathan; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.TerathansAndOphidians;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV4);
            this.AddLoot(LootPack.LV3, 2);
            this.AddLoot(LootPack.MedScrolls, 2);
            this.AddLoot(LootPack.Potions);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
