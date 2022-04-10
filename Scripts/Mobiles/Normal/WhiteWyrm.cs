using Server.Items;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Fourth;
using Server.Spells.Seventh;
using System;

namespace Server.Mobiles
{
    [CorpseName("a white wyrm corpse")]
    public class WhiteWyrm : BaseCreature
    {
        public override double AverageThreshold { get { return 0.25; } }

        public override bool ReduceSpeedWithDamage { get { return false; } }

        [Constructable]
        public WhiteWyrm()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = Utility.RandomBool() ? 180 : 49;
            Name = "dragao branco de olhos azuis";
            BaseSoundID = 362;

            SetStr(721, 760);
            SetDex(101, 130);
            SetInt(386, 425);

            SetHits(533, 656);

            SetDamage(17, 25);

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Cold, 50);

            SetResistance(ResistanceType.Physical, 55, 70);
            SetResistance(ResistanceType.Fire, 15, 25);
            SetResistance(ResistanceType.Cold, 80, 90);
            SetResistance(ResistanceType.Poison, 40, 50);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.Inscribe, 99.1, 100.0);
            SetSkill(SkillName.EvalInt, 99.1, 100.0);
            SetSkill(SkillName.Magery, 120, 120.0);
            SetSkill(SkillName.MagicResist, 99.1, 100.0);
            SetSkill(SkillName.Tactics, 97.6, 100.0);
            SetSkill(SkillName.Wrestling, 90.1, 100.0);

            Fame = 19000;
            Karma = -19000;

            VirtualArmor = 120;

            Tamable = true;
            ControlSlots = 3;
            MinTameSkill = 100;
            var cloth = new UncutCloth(Utility.Random(2, 6));
            cloth.Hue = 1153;
            cloth.Name = "Tecido Puro Branco";
            AddItem(cloth);
            if (Utility.RandomDouble() < 0.02)
            {
                var h = new DragonHead();
                h.Name = "Cabeca de Dragao Branco";
                h.Hue = 1154;
                AddItem(h);
            }
            if (Utility.RandomDouble() < 0.02)
            {
                var d = new DyeTub();
                d.DyedHue = 1153;
                AddItem(d);
            }
        }

        public override void OnAfterTame(Mobile tamer)
        {
            base.OnAfterTame(tamer);
            SetDamage(3, 6);
        }

        public override Spell ChooseSpell()
        {
            if (this.MagicDamageAbsorb == 0)
            {
                return new MagicReflectSpell(this, null);
            }
            var l = Utility.Random(0, 20);
            if (l < 18 && this.Hits < (this.HitsMax * 0.8))
            {
                return new GreaterHealSpell(this, null);
            }
            else if(this.Combatant is Mobile && !((Mobile)this.Combatant).Paralyzed && Utility.RandomBool())
            {
                return new ParalyzeSpell(this, null);
            }
            return null;
        }

        public WhiteWyrm(Serial serial)
        : base(serial)
        {
        }

        public override Poison PoisonImmune { get { if(ControlMaster==null) return Poison.Lethal; return null; } }

        public override bool ReacquireOnMovement
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 4;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
            }
        }
        public override int DragonBlood
        {
            get
            {
                return 8;
            }
        }
        public override int Hides
        {
            get
            {
                return 20;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Barbed;
            }
        }
        public override int Scales
        {
            get
            {
                return 9;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return ScaleType.White;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Gold;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV4);
            AddLoot(LootPack.LV4);
            AddLoot(LootPack.Gems, Utility.Random(1, 5));
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
