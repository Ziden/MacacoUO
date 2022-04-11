using System;
using Server;
using Server.Items;
namespace Server.Items
{

    public class StuffedBunny : Item
    {


        [Constructable]
        public StuffedBunny() : base((Utility.RandomBool() ? 0x99A4 : 0x99A3))

        {

            Weight = 1.0;
            Name = "Coelinho de Pascoa [2022]";
            Hue = Utility.RandomList(171, 291, 26, 33, 1161, 1283, 1170, 1372);


        }


        public StuffedBunny(Serial serial)
            : base(serial)
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
