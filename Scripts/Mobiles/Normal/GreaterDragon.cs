using System;
using Server.Engines.Craft;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a dragon corpse")]
    public class GreaterDragon : BaseCreature
    {
        [Constructable]
        public GreaterDragon()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "dragao anciao";
            Body = Utility.RandomList(12, 59);
            BaseSoundID = 362;

            SetStr(600, 700);
            SetDex(81, 148);
            SetInt(475, 675);

            SetHits(1500, 2500);

            SetDamage(24, 33);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 60, 85);
            SetResistance(ResistanceType.Fire, 65, 90);
            SetResistance(ResistanceType.Cold, 40, 55);
            SetResistance(ResistanceType.Poison, 40, 60);
            SetResistance(ResistanceType.Energy, 50, 75);

            SetSkill(SkillName.Meditation, 0);
            SetSkill(SkillName.EvalInt, 110.0, 140.0);
            SetSkill(SkillName.Magery, 110.0, 140.0);
            SetSkill(SkillName.Poisoning, 0);
            SetSkill(SkillName.Anatomy, 0);
            SetSkill(SkillName.MagicResist, 110.0, 140.0);
            SetSkill(SkillName.Tactics, 110.0, 140.0);
            SetSkill(SkillName.Wrestling, 115.0, 145.0);

            Fame = 22000;
            Karma = -15000;

            VirtualArmor = 60;

            Tamable = true;
            ControlSlots = 5;
            MinTameSkill = 104.7;

            SetWeaponAbility(WeaponAbility.BleedAttack);


            if (Utility.RandomDouble() < 0.1)
            {
                AddItem(new PetBrandingIron());
            }

            if (Hue == 1153 || Utility.RandomDouble() < 0.2)
            {
                AddItem(DefJewelcrafting.GetReceitaPower());
            }
            else
            {
                AddItem(DefJewelcrafting.GetRandomReceitaNoob());
            }
            if (Utility.Random(10) == 1)
            {
                AddItem(new DecoWyrmsHeart());
            }
            if (Utility.Random(6) == 1)
            {
                var cloth = new UncutCloth(Utility.Random(10, 10));
                cloth.Hue = Loot.RandomRareDye();
                cloth.Name = "Tecido Raro";
                AddItem(cloth);
            }
        }



        public GreaterDragon(Serial serial)
            : base(serial)
        {
        }

        public override bool StatLossAfterTame
        {
            get
            {
                return true;
            }
        }
        public override bool ReacquireOnMovement
        {
            get
            {
                return !Controlled;
            }
        }
        public override bool HasBreath
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
                return 5;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
            }
        }
        public override int Hides
        {
            get
            {
                return 30;
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
                return 7;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return (Body == 12 ? ScaleType.Yellow : ScaleType.Red);
            }
        }


        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
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
            AddLoot(LootPack.LV5, 4);
            AddLoot(LootPack.Gems, 4);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)3);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

			switch(version)
			{
        case 3:
				case 2:
					break;
				case 1:
					SetDamage(24, 33);
					SetStam(0);
					break;
				case 0:
					Server.SkillHandlers.AnimalTaming.ScaleStats(this, 0.50);
					Server.SkillHandlers.AnimalTaming.ScaleSkills(this, 0.80, 0.90); // 90% * 80% = 72% of original skills trainable to 90%
					Skills[SkillName.Magery].Base = Skills[SkillName.Magery].Cap; // Greater dragons have a 90% cap reduction and 90% skill reduction on magery
					SetStam(0);
					break;
			}

            if (version == 2)
            {
                SetWeaponAbility(WeaponAbility.BleedAttack);
            }
        }
    }
}
