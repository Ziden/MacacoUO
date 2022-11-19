// ThanksgivingQuest = internal unquic name identifier
// Written by Aj of Age of Britannia - www.aobgames.com

using System;
using Server.Items;
using Server.Spells;
using Server.Spells.Ninjitsu;
using System.Collections.Generic;

namespace Server.Mobiles
{
    [CorpseName("um peru de ação de graças corpse")]
    public class ThanksgivingTurkey : BaseCreature
    {
        
        [Constructable]
        public ThanksgivingTurkey() : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "Peru selvagem";
            Title = "[Quest]";
            Body = 95;
            BaseSoundID = 0x66A;
            Hue = 1139;

            SetStr(112);
            SetDex(85);
            SetInt(66);

            SetHits(2000);
            SetMana(60);

            SetDamage(12,24);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Physical, 55, 65);
            SetSkill(SkillName.MagicResist, 94.0);
            SetSkill(SkillName.Ninjitsu, 95, 105);
            SetSkill(SkillName.Tactics, 95.0);
            SetSkill(SkillName.Wrestling, 95.0);

            Fame = 150;
            Karma = 0;

            Tamable = false;

            m_NextGobble = DateTime.UtcNow;
        }

        public override int Meat => 1;
        public override MeatType MeatType => MeatType.Bird;
        public override FoodType FavoriteFood => FoodType.GrainsAndHay;
        public override int Feathers => 25;

        public override int GetIdleSound()
        {
            return 0x66A;
        }

        public override int GetAngerSound()
        {
            return 0x66A;
        }

        public override int GetHurtSound()
        {
            return 0x66B;
        }

        public override int GetDeathSound()
        {
            return 0x66B;
        }

        private DateTime m_NextGobble;

        public override void OnThink()
        {
            base.OnThink();

            if (Tamable && !Controlled && m_NextGobble < DateTime.UtcNow)
            {
                Say(1153511); //*gobble* *gobble*
                PlaySound(GetIdleSound());

                m_NextGobble = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(20, 240));
            }
        }

        public ThanksgivingTurkey(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

          	list.Add( "<BASEFONT COLOR=#FF9633>[Quest]</BASEFONT>");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_NextGobble = DateTime.UtcNow;
        }
    }
}
