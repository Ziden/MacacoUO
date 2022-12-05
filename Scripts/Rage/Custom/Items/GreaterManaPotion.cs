using System;

namespace Server.Items
{
    public class GreaterManaPotion : BaseManaPotion
    {
        public override string DefaultName
        {
            get { return "Poção de Mana Maior"; }            
        }

        [Constructable]
        public GreaterManaPotion()
            : base(PotionEffect.ManaForte)
        {
        
        }

        public GreaterManaPotion(Serial serial)
            : base(serial)
        {
        }

        public override int MinMana
        {
            get
            {
                return 15; //(Core.AOS ? 18 : 18);
            }
        }
        public override int MaxMana
        {
            get
            {
                return 45; //(Core.AOS ? 35 : 35);
            }
        }

        public override double Delay
        {
            get
            {
                return 30;
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
