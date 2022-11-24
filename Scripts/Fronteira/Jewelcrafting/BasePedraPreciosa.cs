using Server.Gumps;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class BasePedraPreciosa : Item, IGem
    {
        public static Dictionary<Type, ElementoPvM> Elementos = new Dictionary<Type, ElementoPvM>();

        public override bool NaoPodeBancoRP => false;

        public static Type GetTipoPedra(ElementoPvM elemento)
        {
            foreach(var tipo in Elementos.Keys)
            {
                if (Elementos[tipo] == elemento)
                    return tipo;
            }
            return null;
        }

        public static void Configure()
        {
            Shard.Info("Inicializando sistema de pedras preciosas elementais");
            Elementos.Add(typeof(Ruby), ElementoPvM.Fogo);
            Elementos.Add(typeof(Amethyst), ElementoPvM.Agua);
            Elementos.Add(typeof(Diamond), ElementoPvM.Gelo);
            Elementos.Add(typeof(Amber), ElementoPvM.Raio);
            Elementos.Add(typeof(Citrine), ElementoPvM.Vento);
            Elementos.Add(typeof(Emerald), ElementoPvM.Terra);
            Elementos.Add(typeof(StarSapphire), ElementoPvM.Luz);
            Elementos.Add(typeof(Sapphire), ElementoPvM.Escuridao);
        }



        public ElementoPvM GetElemento()
        {
            if(Elementos.ContainsKey(this.GetType())) {
                return Elementos[this.GetType()];
            }
            return ElementoPvM.None;
        }

        public BasePedraPreciosa(int itemID)
            : base(itemID)
        {
            this.Stackable = true;
        }

        public BasePedraPreciosa(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Pedra Preciosa");
            var elemento = GetElemento();
            if(elemento != ElementoPvM.None)
                list.Add(Gump.Cor(elemento.ToString(), BaseArmor.CorElemento(elemento)));
        }

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);
            from.Target = new IT(this);
            from.SendMessage("Escolha o equipamento que deseja imbuir a pedra preciosa usando a skill Imbuing");
        }

        public class IT : Target
        {
            BasePedraPreciosa pedra;

            public IT(BasePedraPreciosa pedra): base(5, false, TargetFlags.None)
            {
                this.pedra = pedra;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                var armadura = targeted as BaseArmor;
                var arma = targeted as BaseWeapon;
                var ropa = targeted as BaseHat;
                Item item = armadura == null ? (Item)arma : armadura;
                if (item == null)
                    item = ropa;

                if (item == null)
                    return;

                if(armadura == null && arma == null && ropa == null)
                {
                    from.SendMessage("Voce apenas pode colocar isto assim em armas e armaduras.");
                    return;
                }

                if(!item.IsChildOf(from.Backpack))
                {
                    from.SendMessage("O item precisa estar em sua mochila.");
                    return;
                }

                /*
                if(arma != null)
                {
                    if(arma.Elemento != ElementoPvM.None)
                    {
                        from.SendMessage("Este equipamento ja tem um elemento.");
                        return;
                    }
                }
                if (armadura != null)
                {
                    if (armadura.Elemento != ElementoPvM.None)
                    {
                        from.SendMessage("Este equipamento ja tem um elemento.");
                        return;
                    }
                }
                */

                if (from.Skills.Imbuing.Value < 40)
                {
                    from.SendMessage("Voce nao tem Imbuing suficiente para isto");
                    return;
                }

                from.PlayAttackAnimation();
                from.PlaySound(0x2A);

                this.pedra.Consume(1);
                Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
                {
                    if (from.Deleted || !from.Alive)
                        return;
                });
                if (!from.CheckSkillMult(SkillName.Imbuing, 40, 90))
                {
                    from.SendMessage("Voce falhou ao imbuir a pedra preciosa no equipamento");
                    return;
                }
                if (arma != null)
                {
                    arma.Elemento = pedra.GetElemento();
                }
                if (armadura != null)
                {
                    armadura.Elemento = pedra.GetElemento();
                }
                if (ropa != null)
                {
                    ropa.Elemento = pedra.GetElemento();
                }
                //armadura.Hue = BaseArmor.HueElemento(armadura.Elemento);
                from.OverheadMessage("* encantou *");
                from.SendMessage("Voce colocou a pedra no item");
                from.PlaySound(0x202);
            }
        }

        public override double DefaultWeight
        {
            get
            {
                return 0.1;
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
}
