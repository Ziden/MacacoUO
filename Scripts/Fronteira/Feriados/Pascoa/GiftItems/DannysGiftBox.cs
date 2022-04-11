using System;
using Server.Items;
using Server.Network;

namespace Server.Items
{
    [Furniture]
    [Flipable(0x232A, 0x232B)]
    public class DannysGiftBox : BaseContainer
    {
        public override int DefaultGumpID { get { return 0x102; } }
        public override int DefaultDropSound { get { return 0x42; } }

        public override Rectangle2D Bounds
        {
            get { return new Rectangle2D(35, 10, 155, 85); }
        }

        [Constructable]
        public DannysGiftBox() : this(Utility.RandomDyedHue())
        {
        }

        public static Type[] PremiosPascoa = new Type[] { typeof(eastereggAddonDeed), typeof(easteregg1AddonDeed) };
        public static int[] PremiosMenores = new int[] { 0x47E6, 0x4CEE, 0x99A3, 0x9CA8, 0x9CA9, 0x9F13, 0x9F14, 0x9F15, 0x9F16, 0x9F17, 0x9F18, 0x9E1D, 0xA738 };

        [Constructable]
        public DannysGiftBox(int hue) : base(Utility.Random(0x232A, 2))
        {
            Weight = 2.0;
            Name = "Presente de Pascoa [2022]";

            if (Utility.RandomDouble() < 0.05)
            {
                var c = new FloppyHat();
                c.Name = "Chapeu da Pascoa [Raro][2022]";
                c.Hue = 2733;
                if (Utility.RandomBool())
                    c.Attributes.SpellDamage = 10;
                else
                    c.Attributes.WeaponDamage = 10;
                DropItem(c);
            }
            else 
            if (Utility.RandomBool())
            {
                var i = new Item(PremiosMenores[Utility.Random(PremiosMenores.Length)]);
                i.Name = "Item Decorativo da Pascoa [2022]";
                DropItem(i);
            }
            else
            {
                switch (Utility.Random(8))
                {
                    case 0:
                        DropItem(new EasterBasketLargeGiftAddonDeed()); break;
                    case 1:
                        DropItem(new StuffedBunny()); break;
                    case 2:
                        DropItem(new EasterHat()); break;
                    case 3:
                        DropItem(new MovingEasterEgg()); break;
                    case 4:
                        DropItem(new ChocolateRabbit()); break;
                    case 5:
                        DropItem(new EasterBunnyPetStatue()); break;
                    case 6:
                        DropItem(new eastereggAddonDeed()); break;
                    case 7:
                        DropItem(new easteregg1AddonDeed()); break;
                }
            }
        }

        public DannysGiftBox(Serial serial) : base(serial)
        {
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
