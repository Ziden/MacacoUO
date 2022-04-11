using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
namespace Server.Items
{

    public class EasterBasket : BaseContainer

    {

        [Constructable]
        public EasterBasket() : base(0x990)

        {
            Weight = 1.0;
            Hue = 86;
            Name = "Cesta de Pascoa [2022]";



        }


        public EasterBasket(Serial serial)
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
