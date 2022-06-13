using System;
using Server.Items;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("a rotting corpse")]
    public class ZeFoguinho : BaseCreature
    {
        [Constructable]
        public ZeFoguinho()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "La Bareda";
            Body = 155;
            BaseSoundID = 471;
            SetStr(200, 220);
            SetDex(75);
            SetInt(151, 200);

            SetHits(5000);
            SetStam(5000);
            SetMana(5000);

            SetDamage(25, 35);

            SetDamageType(ResistanceType.Fire, 100);

            SetResistance(ResistanceType.Fire, 100, 100);


            SetSkill(SkillName.Poisoning, 120.0);
            SetSkill(SkillName.MagicResist, 250.0);
            SetSkill(SkillName.Tactics, 100.0);
            SetSkill(SkillName.Wrestling, 90.1, 100.0);
            SetSkill(SkillName.Magery, 160);

            Fame = 36000;
            Karma = -36000;

            VirtualArmor = 40;
        }

        public ZeFoguinho(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Greater;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
        }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV7, 2);
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
