using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Fronteira.Guildas
{
    [Furniture]
    [Flipable(0x2D07, 0x2D08)]
    public class ArmarioUniforme : BaseContainer
    {
        public Guild Guild;

        public static int CUSTO_PANOS = 5;

        [Constructable]
        public ArmarioUniforme() : base(0x2D07)
        {
            Name = "Armario de Uniformes";
            this.Weight = 1.0;
        }

        public override bool OnDragLift(Mobile from)
        {
            var pode = base.OnDragLift(from);
            if (pode && Guild != null)
            {
                Guild.ArmarioUniforme = null;
                Guild.GuildTextMessage($"{from.Name} retirou o armario de uniformes da guilda");
                Guild = null;
            }
            return pode;
        }

        public override bool OnDroppedToWorld(Mobile from, Point3D p)
        {
            var casa = BaseHouse.FindHouseAt(from.Location, from.Map, 6);
            if (casa == null)
            {
                from.SendMessage("Coloque isto em uma casa");
                return false;
            }
              
            var pl = from as PlayerMobile;
            if (this.Guild == null && pl != null)
            {
                var g = pl.Guild as Guild;
                if (g == null)
                {
                    pl.SendMessage("Voce precisa de uma guilda para colocar isto");
                    return false;
                }

                if (g.ArmarioUniforme != null)
                {
                    pl.SendMessage("Sua guilda ja tem um armario de uniformes");
                    return false;
                }

                g.ArmarioUniforme = this;
                this.Guild = g;
                Guild.GuildTextMessage($"{from.Name} colocou um armario de uniformes da guilda");
                this.InvalidateProperties();
            }
            return base.OnDroppedToWorld(from, p);
        }

        public ArmarioUniforme(Guild g)
            : base(0x2D07)
        {
            this.Guild = g;
            this.Weight = 1.0;
            Name = "Armario de Uniformes";
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();
            if (Guild == null)
                return;

            if (Guild.ArmarioUniforme == this)
            {
                Guild.ArmarioUniforme = null;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if(Guild == null)
            {
                from.SendMessage("Coloque isto em uma casa");
                return;
            }
            from.SendGump(new GumpOpcoes("Uniformes", (int n) => {
                if (n == 0)
                    base.OnDoubleClick(from);
                else if (n == 1)
                    PegaUniforme(from);
            }, 0x2D07, 0, "Abrir", "Pegar Uniforme"));
        }

        public bool PodePagar()
        {
            return this.HasItem<UncutCloth>(CUSTO_PANOS);
        }

        public bool Paga()
        {
            var items = new List<Item>(Items);
            foreach(var i in items)
            {
                if(i is UncutCloth && i.Amount >= CUSTO_PANOS)
                {
                    i.Consume(CUSTO_PANOS);
                    return true;
                }
            }
            return false;
        }

        public int QtdPanos()
        {
            var i = 0;
            foreach(var item in Items)
            {
                if(item is UncutCloth)
                {
                    i += item.Amount;
                }
            }
            return i;
        }

        public void PegaUniforme(Mobile m)
        {
            if(!Paga())
            {
                m.SendMessage("Este armario nao tem panos suficientes para pegar uniformes. Deposite panos.");
                return;
            }
   
            foreach (var item in this.Items)
            {
                if (item is BaseClothing && item.Layer != Layer.Invalid)
                {
                    var uni = new Uniforme(Guild, item as BaseClothing);
                    uni.BoundTo = m.RawName;
                    if (!m.EquipItem(uni))
                        m.AddToBackpack(uni);
                } 
            }
            m.SendMessage("Voce pegou o uniforme de guilda");
            Guild.GuildTextMessage($"{m.Name} retirou um uniforme de guilda, panos restantes: {QtdPanos()}");
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            if (this.Guild != null)
            {
                list.Add($"Uniformes da Guilda {this.Guild.Name}");
            }
            list.Add("Deposite panos para criar uniformes de guilda");
            list.Add("Deposite roupas para setar elas como parte do uniforme");
            list.Add($"Ao morrer {CUSTO_PANOS} panos serao cobrados para mater o uniforme");
        }

        public ArmarioUniforme(Serial serial)
            : base(serial)
        {
        }

        private bool PodeDrop(Item dropped, Mobile from)
        {
            if (!(dropped is UncutCloth) && !(dropped is BaseClothing))
            {
                from.SendMessage("Voce apenas pode colocar pano ou roupas no armario de uniformes");
                return false;
            }
            if (dropped is BaseClothing)
            {
                var ropa = dropped as BaseClothing;
                if (ropa.Hue >= 1000)
                {
                    from.SendMessage("Esta roupa possui uma cor rara. Para usar cores raras em uniformes obtenha o alfinete de guildas magico na dungeon DOOM");
                    return false;
                }
                var jaTem = new HashSet<Layer>();
                foreach (var item in this.Items)
                {
                    if (item is BaseClothing && item.Layer != Layer.Invalid)
                    {
                        jaTem.Add(item.Layer);
                    }
                    if (jaTem.Contains(ropa.Layer))
                    {
                        from.SendMessage("Ja tem uma peca de uniforme deste tipo no armario");
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            return base.OnDragDrop(from, dropped) && PodeDrop(dropped, from);
        }

        public override bool OnDroppedOnto(Mobile from, Item dropped)
        {
            return base.OnDroppedOnto(from, dropped) && PodeDrop(dropped, from);
        }


        public override bool OnDragDropInto(Mobile from, Item dropped, Point3D p)
        {
            return base.OnDragDropInto(from, dropped, p) && PodeDrop(dropped, from);
        }

        public override int DefaultGumpID
        {
            get
            {
                return 0x4E;
            }
        }
        public override int DefaultDropSound
        {
            get
            {
                return 0x42;
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1031527;
            }
        }
        public override Rectangle2D Bounds
        {
            get
            {
                return new Rectangle2D(30, 30, 90, 150);
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

    public class Uniforme : BaseClothing
    {
        public Uniforme(Guild g, BaseClothing roupa) : base(roupa.ItemID, roupa.Layer)
        {
            Name = $"{roupa.Name} {g.Abbreviation}";
            Hue = roupa.Hue;
            EngravedText = g.Name;
            LootType = LootType.Blessed;
        }

        public Uniforme(Serial s) : base(s) { }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Uniforme de Guilda");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var v = reader.ReadInt();
        }
    }

}
