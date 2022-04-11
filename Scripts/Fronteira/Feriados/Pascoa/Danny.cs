using System;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.ContextMenus;
using Server.Gumps;
using Server.Misc;
using Server.Network;
using Server.Spells;
using System.Collections.Generic;

namespace Server.Mobiles
{
    [CorpseName("a danny corpse!")]
    public class Danny : Mobile
    {
        public virtual bool IsInvulnerable { get { return true; } }
        [Constructable]
        public Danny()
        {
            Name = "Danny";
            Body = 400;
            Hue = 1102;
            Blessed = true;

            Container pack = new Backpack();
            pack.Movable = false;
            AddItem(pack);

            Item LongPants = new LongPants();
            LongPants.Hue = 1106;
            LongPants.Movable = false;
            AddItem(LongPants);

            Item Shoes = new Shoes();
            Shoes.Hue = 176;
            Shoes.Movable = false;
            AddItem(Shoes);

            Item FancyShirt = new FancyShirt();
            FancyShirt.Hue = 176;
            FancyShirt.Movable = false;
            AddItem(FancyShirt);

            Item FeatheredHat = new FeatheredHat();
            FeatheredHat.Hue = 176;
            FeatheredHat.Movable = false;
            AddItem(FeatheredHat);

            //Item hair = new Item( 0x2046 );
            //hair.Hue = 1519;
            //hair.Layer = Layer.Hair;
            //hair.Movable = false;
            //AddItem( hair );	
            Utility.AssignRandomHair(this);




        }



        public Danny(Serial serial) : base(serial)
        {
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            list.Add(new DannyEntry(from, this));
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

        public class DannyEntry : ContextMenuEntry
        {
            private Mobile m_Mobile;
            private Mobile m_Giver;

            public DannyEntry(Mobile from, Mobile giver) : base(6146, 3)
            {
                m_Mobile = from;
                m_Giver = giver;
            }

            public override void OnClick()
            {


                if (!(m_Mobile is PlayerMobile))
                    return;

                PlayerMobile mobile = (PlayerMobile)m_Mobile;

                {
                    if (!mobile.HasGump(typeof(DannyquestGump)))
                    {
                        mobile.SendGump(new DannyquestGump(mobile));
                        mobile.AddToBackpack(new EasterBasket());

                    }
                }
            }
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            Mobile m = from;
            PlayerMobile mobile = m as PlayerMobile;

            if (mobile != null)
            {
                if (dropped is SpecialEasterEgg2)
                {
                    if (dropped.Amount != 10)
                    {
                        this.PrivateOverheadMessage(MessageType.Regular, 1153, false, "That is not the items I asked for.", mobile.NetState);
                        return false;
                    }

                    dropped.Delete();
                    mobile.AddToBackpack(new DannysGiftBox());


                    return true;
                }
                else if (dropped is SpecialEasterEgg2)
                {
                    this.PrivateOverheadMessage(MessageType.Regular, 1153, 1054071, mobile.NetState);
                    return false;
                }
                else
                {
                    this.PrivateOverheadMessage(MessageType.Regular, 1153, false, "Thank You!", mobile.NetState);
                }
            }
            return false;
        }
    }
}
