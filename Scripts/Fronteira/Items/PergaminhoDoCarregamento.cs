using Server.Items;
using Server.Misc;
using Server.Misc.Custom;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Ziden
{
    public class PergaminhoCarregamento : Item
    {

        [Constructable]
        public PergaminhoCarregamento() : base(0x1F35)
        {
            this.Name = "Pergaminho do Carregamento";
            this.Stackable = true;
        }

        public PergaminhoCarregamento(Serial s) : base(s) { }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Usar aumenta a quantidade de items carregados");
            list.Add("+1 Item carregado na mochila");
        }

        public override void OnDoubleClick(Mobile from)
        {
            var bp = from.Backpack;
            if (bp == null)
                return;

            if (bp.MaxItems >= 200)
            {
                from.SendMessage("Voce pode apenas upar ate 200 items com este item !");
                return;
            }

            bp.MaxItems++;
            Consume(1);

            Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
            Effects.PlaySound(from.Location, from.Map, 0x243);

            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

            Effects.SendTargetParticles(from, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);
            from.SendMessage("Sua mochila agora carrega um pouco mais de peso");
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

    public class PergaminhoPerdao : Item
    {

        [Constructable]
        public PergaminhoPerdao() : base(0x1F35)
        {
            this.Name = "Pergaminho do Perdao";
            Hue = 55;
            this.Stackable = true;
        }

        public PergaminhoPerdao(Serial s) : base(s) { }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Diminui em 1 suas kills (longs)");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.Kills <= 0)
            {
                from.SendMessage("Voce nao precisa disso");
                return;
            }

            Consume(1);

            Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
            Effects.PlaySound(from.Location, from.Map, 0x243);

            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

            Effects.SendTargetParticles(from, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);
            from.Kills--;
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

    public class PergaminhoPeso : Item
    {

        [Constructable]
        public PergaminhoPeso() : base(0x1F35)
        {
            this.Name = "Pergaminho da Mochila Grande";
            this.Stackable = true;
        }

        public PergaminhoPeso(Serial s) : base(s) { }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Usar aumenta a quantidade peso carregado");
            list.Add("+2 Peso carregado na mochila");
        }

        public override void OnDoubleClick(Mobile from)
        {
            var pl = from as PlayerMobile;
            if (pl == null)
                return;

            var bp = from.Backpack;
            if (bp == null)
                return;

            if (pl.BonusPeso >= 400)
            {
                from.SendMessage("Voce pode apenas upar ate 400 bonus peso !");
                return;
            }

            pl.BonusPeso += 2;
            Consume(1);

            Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
            Effects.PlaySound(from.Location, from.Map, 0x243);

            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

            Effects.SendTargetParticles(from, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);
            from.SendMessage("Sua mochila agora carrega um pouco mais de peso");
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

    public class PergaminhoSkill : Item
    {
        private SkillName skill;

        [Constructable]
        public PergaminhoSkill(SkillName skill) : base(0x1F35)
        {
            this.skill = skill;
            this.Name = "Pergaminho grande mestre " + skill.ToString();
            this.Stackable = true;
        }

        public PergaminhoSkill(Serial s) : base(s) { }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("GMiza instantaneamente a skill");
        }

        public override void OnDoubleClick(Mobile from)
        {
            var pl = from as PlayerMobile;
            if (pl == null)
                return;

            var bp = from.Backpack;
            if (bp == null)
                return;

            var skillFalta = 100 - from.Skills[skill].Value;

            for (var i = 0; i < skillFalta; i++)
            {
                SkillCheck.Gain(from, from.Skills[skill], 10);
            }

            Consume(1);

            Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
            Effects.PlaySound(from.Location, from.Map, 0x243);

            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

            Effects.SendTargetParticles(from, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);
            from.SendMessage("Voce agora tem maior conhecimento na skill " + skill.ToString());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)skill);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            skill = (SkillName)reader.ReadInt();
        }
    }
}
