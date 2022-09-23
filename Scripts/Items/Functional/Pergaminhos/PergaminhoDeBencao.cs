using Server.Targeting;
using System;
using Server.Engines.Craft;

namespace Server.Items.Functional.Pergaminhos
{
    public class PergaminhoSagrado : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int Dias { get; set; }

        [Constructable]
        public PergaminhoSagrado()
            : this(0x14F0)
        {
            this.Dias = 30;
            this.Hue = 356;
            this.Name = "Pergaminho de Bencao";
        }

        public PergaminhoSagrado(int itemID)
           : base(itemID)
        {
            this.Dias = 30;
            this.Hue = 356;
            this.Name = "Pergaminho de Bencao";
        }

        public PergaminhoSagrado(Serial serial)
            : base(serial)
        {
            this.Dias = 30;
            this.Hue = 356;
            this.Name = "Pergaminho de Bencao";
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Escolha uma roupa ou runebook para abencoar");
            from.Target = new InternalTarget(from, this);
        }

        public class InternalTarget : Target
        {
            private Mobile from;
            private PergaminhoSagrado scroll;
            public InternalTarget(Mobile from, PergaminhoSagrado scroll) : base(1, false, TargetFlags.None)
            {
                this.from = from;
                this.scroll = scroll;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (scroll.Deleted)
                    return;

                if(targeted is BaseClothing || targeted is Runebook)
                {
                    var item = (Item)targeted;
                    if(item.LootType == LootType.Blessed)
                    {
                        item.PrivateMessage("Este item ja esta abencoado", from);
                    } else
                    {
                        item.BlessedUntil = DateTime.UtcNow + TimeSpan.FromDays(scroll.Dias+1);
                        item.LootType = LootType.Blessed;
                        from.FixedEffect(0x37C4, 87, 2000, 4, 3);
                        from.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                        from.PlaySound(0x202);
                        item.PrivateMessage("* Abencoado por "+scroll.Dias+ " dias *", from);
                        from.SendMessage("Voce embrulha o item no pergaminho, desfazendo o pergaminho e abencoando o item por "+scroll.Dias+" dias");
                        scroll.Consume();
                    }
                } else
                {
                    from.SendMessage("Voce apenas pode usar isto em roupas ou runebooks");
                }
            }
        }

        public virtual void AddNameProperties(ObjectPropertyList list)
        {
            list.Add("Pergaminho Sagrado");
            list.Add("Abencoa um item por " + Dias + " dias");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
            writer.Write(Dias);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            Dias = reader.ReadInt();
        }
    }

    public class PergaminhoSagradoSupremo : PergaminhoSagrado
    {

        [CommandProperty(AccessLevel.GameMaster)]
        public int Dias { get; set; }

        [Constructable]
        public PergaminhoSagradoSupremo()
            : base()
        {
            this.Dias = 30 * 6;
            this.Hue = 356;
            this.Name = "Pergaminho de Item Pessoal";
        }

        public PergaminhoSagradoSupremo(int itemID)
           : base(itemID)
        {
            this.Dias = 30 * 6;
            this.Hue = 356;
            this.Name = "Pergaminho de Item Pessoal";
        }

        public PergaminhoSagradoSupremo(Serial serial)
            : base(serial)
        {

        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Escolha um item (roupa) para sempre");
            from.Target = new InternalTarget(from, this);
        }

        public class InternalTarget : Target
        {
            private Mobile from;
            private PergaminhoSagrado scroll;
            public InternalTarget(Mobile from, PergaminhoSagrado scroll) : base(1, false, TargetFlags.None)
            {
                this.from = from;
                this.scroll = scroll;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (scroll.Deleted)
                    return;

                if (targeted is BaseClothing || targeted is Runebook)
                {
                    var item = (Item)targeted;
                    if (item.LootType == LootType.Blessed)
                    {
                        item.PrivateMessage("Este item ja esta abencoado", from);
                    }
                    else
                    {
                        item.LootType = LootType.Blessed;
                        from.FixedEffect(0x37C4, 87, 2000, 4, 3);
                        from.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                        from.PlaySound(0x202);
                        item.PrivateMessage("* Abencoado *", from);
                        from.SendMessage("Voce embrulha o item no pergaminho, desfazendo o pergaminho e abencoando o item");
                        scroll.Consume();
                    }
                }
                else
                {
                    from.SendMessage("Voce apenas pode usar isto em roupas ou runebooks");
                }
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            list.Add("Abencoa um item para sempre");
            list.Add("tornando-o pertence pessoal");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class PergaminhoSagradoDeRunebook : PergaminhoSagrado
    {

        [CommandProperty(AccessLevel.GameMaster)]
        public int Dias { get; set; }

        [Constructable]
        public PergaminhoSagradoDeRunebook()
            : base()
        {
            this.Dias = 30 * 6;
            this.Hue = 356;
            this.Name = "Pergaminho Sagrado de Runebook";
        }

        public PergaminhoSagradoDeRunebook(int itemID)
           : base(itemID)
        {
            this.Dias = 30 * 6;
            this.Hue = 356;
            this.Name = "Pergaminho Sagrado de Runebook";
        }

        public PergaminhoSagradoDeRunebook(Serial serial)
            : base(serial)
        {

        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Escolha um runebook");
            from.Target = new InternalTarget(from, this);
        }

        public class InternalTarget : Target
        {
            private Mobile from;
            private PergaminhoSagrado scroll;
            public InternalTarget(Mobile from, PergaminhoSagrado scroll) : base(1, false, TargetFlags.None)
            {
                this.from = from;
                this.scroll = scroll;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (scroll.Deleted)
                    return;

                if (targeted is Runebook)
                {
                    var item = (Item)targeted;
                    if (item.LootType == LootType.Blessed)
                    {
                        item.PrivateMessage("Este item ja esta abencoado", from);
                    }
                    else
                    {
                        item.LootType = LootType.Blessed;
                        from.FixedEffect(0x37C4, 87, 2000, 4, 3);
                        from.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                        from.PlaySound(0x202);
                        item.PrivateMessage("* Abencoado *", from);
                        from.SendMessage("Voce embrulha o item no pergaminho, desfazendo o pergaminho e abencoando o item");
                        scroll.Consume();
                    }
                }
                else
                {
                    from.SendMessage("Voce apenas pode usar isto em runebooks");
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class PergaminhoSagradoPvM : PergaminhoSagrado
    {

        [CommandProperty(AccessLevel.GameMaster)]
        public int Dias { get; set; }

        [Constructable]
        public PergaminhoSagradoPvM()
            : base()
        {
            this.Dias = 30;
            this.Hue = 356;
            this.Name = "Pergaminho Sagrado PvM";
        }

        public PergaminhoSagradoPvM(int itemID)
           : base(itemID)
        {
            this.Dias = 30;
            this.Hue = 356;
            this.Name = "Pergaminho Sagrado PvM";
        }

        public PergaminhoSagradoPvM(Serial serial)
            : base(serial)
        {

        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Escolha um acessorio PvM");
            from.Target = new InternalTarget(from, this);
        }

        public class InternalTarget : Target
        {
            private Mobile from;
            private PergaminhoSagrado scroll;
            public InternalTarget(Mobile from, PergaminhoSagrado scroll) : base(1, false, TargetFlags.None)
            {
                this.from = from;
                this.scroll = scroll;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (scroll.Deleted)
                    return;

                if (targeted is BaseJewel || targeted is BaseTalisman)
                {
                    var item = (Item)targeted;
                    if (item.LootType == LootType.Blessed)
                    {
                        item.PrivateMessage("Este item ja esta abencoado", from);
                    }
                    else
                    {
                        item.LootType = LootType.Blessed;
                        from.FixedEffect(0x37C4, 87, 2000, 4, 3);
                        from.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                        from.PlaySound(0x202);
                        item.PrivateMessage("* Abencoado *", from);
                        from.SendMessage("Voce embrulha o item no pergaminho, desfazendo o pergaminho e abencoando o item");
                        scroll.Consume();
                    }
                }
                else
                {
                    from.SendMessage("Voce apenas pode usar isto em acessorios PvM");
                }
            }
        }


        public virtual void AddNameProperties(ObjectPropertyList list)
        {
            list.Add("Pergaminho Sagrado PvM");
            list.Add("Abencoa um acessorio PvM por " + Dias + " dias");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class PergaminhoSagradoSupremoPvM : PergaminhoSagrado
    {

        [CommandProperty(AccessLevel.GameMaster)]
        public int Dias { get; set; }

        [Constructable]
        public PergaminhoSagradoSupremoPvM()
            : base()
        {
            this.Dias = 30 * 6;
            this.Hue = 356;
            this.Name = "Pergaminho Sagrado Supremo PvM";
        }

        public PergaminhoSagradoSupremoPvM(int itemID)
           : base(itemID)
        {
            this.Dias = 30 * 6;
            this.Hue = 356;
            this.Name = "Pergaminho Sagrado Supremo PvM";
        }

        public PergaminhoSagradoSupremoPvM(Serial serial)
            : base(serial)
        {

        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Escolha um acessorio PvM");
            from.Target = new InternalTarget(from, this);
        }

        public class InternalTarget : Target
        {
            private Mobile from;
            private PergaminhoSagrado scroll;
            public InternalTarget(Mobile from, PergaminhoSagrado scroll) : base(1, false, TargetFlags.None)
            {
                this.from = from;
                this.scroll = scroll;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (scroll.Deleted)
                    return;

                if (targeted is BaseJewel || targeted is BaseTalisman)
                {
                    var item = (Item)targeted;
                    if (item.LootType == LootType.Blessed)
                    {
                        item.PrivateMessage("Este item ja esta abencoado", from);
                    }
                    else
                    {
                        item.LootType = LootType.Blessed;
                        from.FixedEffect(0x37C4, 87, 2000, 4, 3);
                        from.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                        from.PlaySound(0x202);
                        item.PrivateMessage("* Abencoado *", from);
                        from.SendMessage("Voce embrulha o item no pergaminho, desfazendo o pergaminho e abencoando o item");
                        scroll.Consume();
                    }
                }
                else
                {
                    from.SendMessage("Voce apenas pode usar isto em acessorios PvM");
                }
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            list.Add("Abencoa um acessorio PvM para sempre");
            list.Add("tornando-o pertence pessoal");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }
}
