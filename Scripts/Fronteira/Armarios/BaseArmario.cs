using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Fronteira.Armory
{
    public class ArmarioItem
    {
        public ArmarioItem()
        {
        }

        public ArmarioItem(Item i)
        {
            tipo = i.GetType();
            nome = i.Name;
            qtd = i.Amount;
        }

        public Type tipo;
        public string nome;
        public int qtd;
    }

    public class BaseArmario : FurnitureContainer
    {
        private List<ArmarioItem> _set = new List<ArmarioItem>();

        public BaseArmario()
            : base(0xA4F)
        {
            this.Weight = 1.0;
        }

        public BaseArmario(int itemID)
         : base(itemID)
        {
            this.Weight = 1.0;
        }

        public BaseArmario(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Acesso(this, from))
            {
                from.SendMessage("Este armario nao e seu");
                return;
            }

            AbreArmario(this, from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
            writer.Write(_set.Count);
            foreach (var i in _set)
            {
                writer.Write(i.nome);
                writer.Write(i.tipo);
                writer.Write(i.qtd);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            var qtd = reader.ReadInt();
            for (int x = 0; x < qtd; x++)
            {
                var i = new ArmarioItem();
                i.nome = reader.ReadString();
                i.tipo = reader.ReadType();
                i.qtd = reader.ReadInt();
                _set.Add(i);
            }
            DynamicFurniture.Close(this);
        }

        public static bool Acesso(FurnitureContainer armario, Mobile from)
        {
            var casa = BaseHouse.FindHouseAt(armario);
            if (casa != null && !casa.HasAccess(from))
            {
                return false;
            }
            return true;
        }

        public void AbreArmario(FurnitureContainer armario, Mobile from)
        {
            var casa = BaseHouse.FindHouseAt(armario);
            if (casa != null && !casa.HasAccess(from))
            {
                from.SendMessage("Este armario nao e seu");
                return;
            }

            from.SendGump(new GumpOpcoes("Selecione", (opt) =>
            {
                if (opt == 0)
                {
                    Ver(armario, from);
                }
                else if (opt == 1)
                {
                    SetarSet(armario, from);
                }
                else if (opt == 2)
                {
                    RetirarSet(armario, from);
                }
            }, armario.ItemID, armario.Hue, "Abrir", "Setar Set", "Retirar Set"));
        }

        private void Ver(FurnitureContainer armario, Mobile from)
        {
            armario.DisplayTo(from);
        }

        private void SetarSet(FurnitureContainer armario, Mobile from)
        {
            from.SendMessage("Selecione uma mochila com os items do set.");
            from.BeginTarget(5, false, TargetFlags.None, new TargetCallback((Mobile m, object t) =>
            {
                var mochila = t as Container;
                if (mochila == null)
                {
                    from.SendMessage("Voce apenas pode fazer isto com mochilas");
                    return;
                }
                if (mochila.Parent is Mobile)
                {
                    from.SendMessage("Nao pode fazer isto com mochilas equipadas");
                    return;
                }
                if (mochila.RootParent != from)
                {
                    from.SendMessage("Voce apenas pode selecionar mochilas que estao dentro de sua mochila");
                    return;
                }

                _set.Clear();
                m.SendMessage("Voce colocou o set no armario. Agora voce apenas pode colocar mochilas identicas a esta no armario.");
                var adds = "";
                foreach (var item in new List<Item>(mochila.Items))
                {
                    adds += (item.Amount > 1 ? item.Amount + "x " : "") + item.Name + ", ";
                    _set.Add(new ArmarioItem(item));
                    armario.DropItem(item);
                }
                m.SendMessage("Voce setou o set desse armario com os items:");
                m.SendMessage(adds);
            }));
        }

        public override bool OnDroppedOnto(Mobile from, Item dropped)
        {
            if (!EhDoSet(dropped))
            {
                from.SendMessage("Este item nao faz parte do set deste armario.");
                return false;
            }
            return base.OnDroppedOnto(from, dropped);
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!EhDoSet(dropped))
            {
                from.SendMessage("Este item nao faz parte do set deste armario.");
                return false;
            }
            return base.OnDragDrop(from, dropped);
        }

        public override bool OnDragDropInto(Mobile from, Item dropped, Point3D p)
        {
            if (!EhDoSet(dropped))
            {
                from.SendMessage("Este item nao faz parte do set deste armario.");
                return false;
            }
            Shard.Debug("Dropei into " + dropped.GetType().Name);
            return base.OnDragDropInto(from, dropped, p);
        }

        public bool EhDoSet(Item item)
        {
            foreach (var i in _set)
            {
                if (i.tipo == item.GetType())
                    return true;
            }
            return false;
        }

        private List<ArmarioItem> GetFaltando()
        {
            List<ArmarioItem> lista = new List<ArmarioItem>();
            foreach (var i in _set)
            {
                var item = this.FindItemByType(i.tipo, true);
                if (item == null || item.Amount < i.qtd)
                {
                    lista.Add(i);
                }
            }
            return lista;
        }

        private void RetirarSet(FurnitureContainer armario, Mobile from)
        {
            var faltando = GetFaltando();
            if (faltando.Count > 0)
            {
                from.SendMessage("Items faltando: " + string.Join(", ", faltando.Select(f => f.nome).ToArray()));
                return;
            }
            DynamicFurniture.Open(this, from);
            foreach (var i in _set)
            {
                var item = this.FindItemByType(i.tipo, true);
                if(item.Amount > i.qtd)
                {
                    var dupe = Mobile.LiftItemDupe(item, i.qtd);
                } 
                from._PlaceInBackpack(item);
                if (item.Layer != Layer.Invalid && from is PlayerMobile)
                {
                    ((PlayerMobile)from).SmoothForceEquip(item);
                }
                from.OverheadMessage("* pegou no armario *");
            }
            from.SendMessage("Voce pegou um set");
        }

        private static List<Backpack> GetSets(FurnitureContainer armario)
        {
            return armario.Items.Select(i => i as Backpack).Where(i => i != null).ToList();
        }

    }


}
