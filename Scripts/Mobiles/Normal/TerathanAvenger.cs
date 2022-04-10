using Server.Items;
using System;

namespace Server.Mobiles
{
    [CorpseName("a terathan avenger corpse")]
    public class TerathanAvenger : BaseCreature
    {
        public override int BonusExp => 100;

        [Constructable]
        public TerathanAvenger()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "terathan vingador";
            this.Body = 152;
            this.BaseSoundID = 0x24D;

            this.SetStr(467, 645);
            this.SetDex(77, 95);
            this.SetInt(126, 150);

            this.SetHits(2296, 2372);
            this.SetMana(46, 70);

            this.SetDamage(18, 38);

            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Poison, 50);

            this.SetResistance(ResistanceType.Physical, 40, 50);
            this.SetResistance(ResistanceType.Fire, 30, 40);
            this.SetResistance(ResistanceType.Cold, 35, 45);
            this.SetResistance(ResistanceType.Poison, 90, 100);
            this.SetResistance(ResistanceType.Energy, 35, 45);

            this.SetSkill(SkillName.EvalInt, 70.3, 100.0);
            this.SetSkill(SkillName.Magery, 70.3, 100.0);
            this.SetSkill(SkillName.Poisoning, 60.1, 80.0);
            this.SetSkill(SkillName.MagicResist, 65.1, 80.0);
            this.SetSkill(SkillName.Tactics, 90.1, 100.0);
            this.SetSkill(SkillName.Wrestling, 90.1, 100.0);

            this.Fame = 13500;
            this.Karma = -13500;
            if(Utility.RandomDouble() < 0.33)
                PackItem(new CristalTherathan());
            this.VirtualArmor = 150;
        }

        public TerathanAvenger(Serial serial)
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
                damage *= 5;
        }


        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 3;
            }
        }
        public override int Meat
        {
            get
            {
                return 2;
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
            this.AddLoot(LootPack.LV5, 1);
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

            if (this.BaseSoundID == 263)
                this.BaseSoundID = 0x24D;
        }
    }
}
