using System;
using Server;
using Server.Items;
namespace Server.Items
{

    public class MovingEasterEgg : Item
    {

        [Constructable]
        public MovingEasterEgg() : base(0x4CFD)
        {
            Weight = 1.0;
            Name = "Ovo de Pascoa [2022]";
            ItemID = 19709;

        }

        public MovingEasterEgg(Serial serial)
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
