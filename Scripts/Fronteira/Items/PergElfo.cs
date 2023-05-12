using System;
using Server;
using Server.Mobiles;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class PergElfo : Item
    {
        [Constructable]
        public PergElfo()
            : base(0x1F35)
        {
            Hue = 0x480;
            Name = "Pergaminho de Transformação em Elfo";
            LootType = LootType.Blessed;
            Weight = 1.0;
        }

        public PergElfo(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Transforma o personagem em elfo permanentemente!");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from == null)
            {
                return;
            }

            if (from.Race != Race.Elf)
            {
                if (from.Female)
                {
                    from.Body = 606; // Corpo feminino de Elfo
                    from.HueMod = 0x383;
                    from.HairItemID = 0x2fc0; // Cabelo feminino de Elfo
                    from.FacialHairItemID = 0; // Sem barba
                }
                else
                {
                    from.Body = 605; // Corpo masculino de Elfo
                    from.HueMod = 0x383;
                    from.HairItemID = 0x2fc0; // Cabelo masculino de Elfo
                    from.FacialHairItemID = 0; // Sem barba
                }
                from.SendMessage("Você se transformou em um Elfo!");
                from.Race = Race.Elf;
                from.FixedParticles(0x373A, 10, 15, 5018, EffectLayer.Waist);
                from.PlaySound(0x1F2);
                this.Consume();
            }
            else
            {
                from.SendMessage("Você já é um Elfo.");
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // versão
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
