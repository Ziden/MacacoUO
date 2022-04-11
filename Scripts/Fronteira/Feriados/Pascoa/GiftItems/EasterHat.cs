using System;
using Server;

namespace Server.Items
{



    public class EasterHat : StrawHat
    {
        [Constructable]
        public EasterHat()
        {
            Hue = 1174;
            Name = "Chapeu de Pascoa Comum [2022]";
            Attributes.Luck = 1;
            LootType = LootType.Blessed;

        }

        public EasterHat(Serial serial) : base(serial)
        {
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
