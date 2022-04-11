using System;

namespace Server.Mobiles
{
    [CorpseName("a easter bunny pet corpse")]
    //[TypeAlias("Server.Mobiles.Jackrabbit")]
    public class EasterBunnyPet : BaseCreature
    {
        [Constructable]
        public EasterBunnyPet()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "coelinho da pascoa";
            this.Body = 0x12E;
             Hue =  Utility.RandomList ( 91, 1272, 1266, 26 );


            this.SetStr(15);
            this.SetDex(36,38);
            this.SetInt(8,16);

            this.SetHits(10,15);
            this.SetMana(0);

            this.SetDamage(10);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 2, 5);

            this.SetSkill(SkillName.MagicResist, 5.0);
            this.SetSkill(SkillName.Tactics, 5.0);
            this.SetSkill(SkillName.Wrestling, 5.0);

            this.Fame = 150;
            this.Karma = 0;

            this.VirtualArmor = 4;

            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = -18.9;
        }

        public EasterBunnyPet(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int Hides
        {
            get
            {
                return 1;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies;
            }
        }
        public override int GetAttackSound() 
        { 
            return 0xC9; 
        }

        public override int GetHurtSound() 
        { 
            return 0xCA; 
        }

        public override int GetDeathSound() 
        { 
            return 0xCB; 
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
