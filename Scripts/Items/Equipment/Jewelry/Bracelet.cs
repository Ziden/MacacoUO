using System;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
    public abstract class BaseBracelet : BaseJewel
    {
        public BaseBracelet(int itemID)
            : base(itemID, Layer.Bracelet)
        {
        }

        public BaseBracelet(Serial serial)
            : base(serial)
        {
        }

        public override int BaseGemTypeNumber
        {
            get
            {
                return 1044221;
            }
        }// star sapphire bracelet
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 1)
            {
                if (Weight == .1)
                {
                    Weight = -1;
                }
            }
        }
    }

    public class GoldBracelet : BaseBracelet
    {
        [Constructable]
        public GoldBracelet()
            : base(0x1086)
        {
            //Weight = 0.1;
        }

        public GoldBracelet(Serial serial)
            : base(serial)
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


    public enum TipoJoias
    {
        Arma, Arco, Magia, Escudo
    }

    public class BraceleteDoPoder : BaseBracelet
    {
        [CommandProperty(AccessLevel.Administrator)]
        public int Bonus { get; set; }

        [CommandProperty(AccessLevel.Administrator)]
        public TipoJoias Tipo { get; set; }

        [Constructable]
        public BraceleteDoPoder()
            : base(0x1086)
        {
            //Weight = 0.1;
            Name = "Bracelete do Poder";
            Bonus = 10;
            Hue = 1151;
            Tipo = TipoJoias.Arma;
        }

        public BraceleteDoPoder(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);
            from.SendMessage("Este bracelete ajuda com bonus nas habilidades das armas em PvM.");
            from.SendMessage("Para aprender sobre as habilidades das armas, veja nossa wiki.");

            Shard.Debug("Abrindo gump");
            from.SendGump(new ConfirmaGump(from as PlayerMobile, "Derreter Bracelete ?", "Voce gostaria de derreter este bracelete e recuperar parte do material ?", () =>
            {
                var a = false;
                var f = false;
                DefBlacksmithy.CheckAnvilAndForge(from, 3, out a, out f);
                if (!a || !f)
                {
                    from.SendMessage("Voce precisa estar proximo a uma bigorna e uma forja para isto");
                    return;
                }
                this.Consume();
                from.PlayAttackAnimation();
                from.PlaySound(0x042);
                from.OverheadMessage("* derreteu *");
                var item = new CristalDoPoder();
                if(Bonus <= 5)
                    item.Amount = 1 + Utility.Random(5);
                else
                    item.Amount = 20 + Utility.Random(10);
                from.AddToBackpack(item);
                from.SendMessage("Voce recuperou parte do material");
            }));

        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            if (Tipo == TipoJoias.Arma)
                list.Add($"Bonus Dano Hab. Armas Fisicas PvM");
            else if (Tipo == TipoJoias.Arco)
                list.Add($"Bonus Dano Hab. Arcos PvM");
            else if (Tipo == TipoJoias.Magia)
                list.Add($"Bonus Dano Hab. Magias PvM");
            else if (Tipo == TipoJoias.Escudo)
                list.Add($"Bonus Parry PvM");
            list.Add($"+{Bonus}%");
            list.Add("Apenas funciona com habilidades ativas de armas");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
            writer.Write(Bonus);
            writer.Write((int)Tipo);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            Bonus = reader.ReadInt();
            Tipo = (TipoJoias)reader.ReadInt();
        }
    }

    public class GoldBraceletMagico : BaseBracelet
    {
        [Constructable]
        public GoldBraceletMagico()
            : base(0x1086)
        {
            Name = "Bracelete Magico";
            this.SkillBonuses.Skill_1_Name = Utility.RandomSkill();
            this.SkillBonuses.Skill_1_Value = 1;
        }

        public GoldBraceletMagico(Serial serial)
            : base(serial)
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

    public class GoldBraceletBonito : BaseBracelet
    {
        [Constructable]
        public GoldBraceletBonito()
            : base(0x1086)
        {
            Name = "Bracelete Elegante";
            switch (Utility.Random(3))
            {
                case 0: this.Attributes.BonusStr = 1; break;
                case 1: this.Attributes.BonusDex = 1; break;
                case 2: this.Attributes.BonusInt = 1; break;
            }
        }

        public GoldBraceletBonito(Serial serial)
            : base(serial)
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

    public class SilverBracelet : BaseBracelet, IRepairable
    {
        public CraftSystem RepairSystem { get { return DefTinkering.CraftSystem; } }

        [Constructable]
        public SilverBracelet()
            : base(0x1F06)
        {
            //Weight = 0.1;
        }

        public SilverBracelet(Serial serial)
            : base(serial)
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
