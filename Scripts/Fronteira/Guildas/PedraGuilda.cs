using Server.Guilds;
using Server.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Fronteira.Guildas
{
    public class PergaminhoBancoGuilda : Item
    {
        [Constructable]
        public PergaminhoBancoGuilda()
            : this(0x14F0)
        {
            this.Hue = 356;
            this.Name = "Pergaminho de Banco de Guilda";
        }

        public PergaminhoBancoGuilda(int itemID)
           : base(itemID)
        {
            this.Hue = 356;
            this.Name = "Pergaminho de Banco de Guilda";
        }

        public PergaminhoBancoGuilda(Serial serial)
            : base(serial)
        {
            this.Hue = 356;
            this.Name = "Pergaminho de Banco de Guilda";
        }

        public override void OnDoubleClick(Mobile from)
        {
            if(from.Guild == null)
            {
                from.SendMessage("Voce precisa de uma guilda");
                return;
            }
            var g = from.Guild as Guild;
            if(g.Banco != null && !g.Banco.Deleted)
            {
                from.SendMessage("Esta guilda ja tem um banco");
                return;
            }
            g.Banco = new BauDeGuilda(g.Abbreviation);
            g.Banco.MoveToWorld(from.Location, from.Map);
            g.Banco.HonestyItem = true;
        }

        public virtual void AddNameProperties(ObjectPropertyList list)
        {
            list.Add("Pergaminho de Banco de Guilda");
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

    public class BauDeGuilda : Server.Items.Container
    {
        public string tag = null;

        public BauDeGuilda(string tag)
            : base(0x9AB)
        {
            tag = tag;
            Visible = false;
        }

        public BauDeGuilda(Serial serial)
            : base(serial)
        {
            Name = "Bau de Guilda";
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add(tag);
        }

        public override bool IsAccessibleTo(Mobile check)
        {
            if ((check.Guild?.Abbreviation == tag || check.AccessLevel >= AccessLevel.GameMaster))
            {
                return true;
            }
            return false;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
