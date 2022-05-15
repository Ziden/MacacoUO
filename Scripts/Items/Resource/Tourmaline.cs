using System;

namespace Server.Items
{
    public class Tourmaline : BasePedraPreciosa, IGem
    {
        [Constructable]
        public Tourmaline()
            : this(1)
        {
        }

        [Constructable]
        public Tourmaline(int amount)
            : base(0x0F18)
        {
            this.Stackable = true;
            this.Amount = amount;
        }

        public Tourmaline(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Use em armaduras para adicionar poderes PvM");
        }

        public static bool Tem(AosAttributes attrs)
        {
            return (attrs.WeaponDamage > 0 ||
             attrs.SpellDamage > 0 ||
            attrs.WeaponSkillDamage > 0 ||
             attrs.WeaponSpeed > 0 ||
             attrs.DefendChance > 0 ||
             attrs.LowerManaCost > 0);
        }

        public static bool Limpa(AosAttributes attrs)
        {
            if (Tem(attrs))
            {
                attrs.WeaponDamage = 0;
                attrs.WeaponSkillDamage = 0;
                attrs.WeaponSpeed = 0;
                attrs.LowerManaCost = 0;
                attrs.DefendChance = 0;
                attrs.SpellDamage = 0;
                return true;
            }
            return false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Selecione uma armadura para adicionar mais poder PvM.");
            from.DaTarget<BaseArmor>(armor =>
            {
                if (this.Deleted)
                    return;

                if (Tem(armor.Attributes))
                {
                    if(this.Amount < 3)
                    {
                        from.SendMessage("Esta armadura ja esta encantada e voce precisa de 3 turmalinas para alterar o encantamento !");
                        return;
                    } else
                    {
                        this.Consume(2);
                        Limpa(armor.Attributes);
                    }
                }

                Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
                Effects.PlaySound(from.Location, from.Map, 0x243);

                Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

                Effects.SendTargetParticles(from, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);

                this.Consume(1);

                switch (Utility.Random(6))
                {
                    case 0: armor.Attributes.WeaponDamage = Utility.Random(1, 2); break;
                    case 1: armor.Attributes.WeaponSkillDamage = Utility.Random(2, 5); break;
                    case 2: armor.Attributes.WeaponSpeed = Utility.Random(1, 2); break;
                    case 3: armor.Attributes.LowerManaCost = Utility.Random(1, 2); break;
                    case 4: armor.Attributes.DefendChance = Utility.Random(1, 2); break;
                    case 5: armor.Attributes.SpellDamage = Utility.Random(1, 2); break;
                }
            }, "Selecione uma armadura");
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

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
                ItemID = 0x0F18;
        }
    }
}
