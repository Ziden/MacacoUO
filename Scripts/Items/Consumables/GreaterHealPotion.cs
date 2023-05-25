using System;

namespace Server.Items
{
    public class GreaterHealPotion : BaseHealPotion
    {
		public override string DefaultName
        {
            get { return "Poção de Vida Forte"; }            
        }
		
        [Constructable]
        public GreaterHealPotion()
            : base(PotionEffect.VidaForte)
        {
            FiverRatio = Shard.POL_STYLE;
        }

        public GreaterHealPotion(Serial serial)
            : base(serial)
        {
            FiverRatio = Shard.POL_STYLE;
        }



        public override int MinHeal
        {
            get
            {
                return Shard.POL_STYLE ? 20 : 9;
            }
        }

        public override int MaxHeal
        {
            get
            {
                return Shard.POL_STYLE ? 60 : 30;
            }
        }

        public override double Delay
        {
            get
            {
                return 12.0;
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
