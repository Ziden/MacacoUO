using System;

namespace Server.Items
{
    public class LesserManaPotion : BaseManaPotion 
    {
        public override string DefaultName
        {
            get { return "Poção de Mana Menor"; }
        }
        
        [Constructable]
        public LesserManaPotion()
            : base(PotionEffect.ManaFraca)
        {
            
        }

        public LesserManaPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinMana
        {
            get
            {
                return 10; //(Core.AOS ? 5 : 5);
            }
        }
        public override int MaxMana
        {
            get
            {
                return 20; //(Core.AOS ? 20 : 20);
            }
        }
        public override double Delay
        {
            get
            {
                return 30; //(Core.AOS ? 10.0 : 10.0);
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
