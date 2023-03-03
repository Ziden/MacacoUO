using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Ziden.Mounts.Mustangs.Ore
{
    [CorpseName("a mustang vibranium corpse")]
    [TypeAlias("Server.Mobiles.GrayHorse")]
    public class VibraniumMustang  : BaseMount
    {
        [Constructable]
        public VibraniumMustang()
            : this("mustang vibranium")
        {
        }

        [Constructable]
        public VibraniumMustang(string name)
            : base(name, 0xE2, 0x3EA0, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Body = new Body(0xE4);
            ItemID = 0x3EA1;
            BaseSoundID = 0xA8;

            SetStr(250);
            SetDex(100);
            SetInt(6, 10);

            SetHits(100);
            SetMana(0);

            SetDamage(3, 4);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 15, 20);

            SetSkill(SkillName.MagicResist, 25.1, 30.0);
            SetSkill(SkillName.Tactics, 29.3, 44.0);
            SetSkill(SkillName.Wrestling, 29.3, 44.0);

            Fame = 300;
            Karma = 300;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 90;
            Hue = 2733;
        }

        public VibraniumMustang(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 3;
            }
        }
        public override int Hides
        {
            get
            {
                return 10;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
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

