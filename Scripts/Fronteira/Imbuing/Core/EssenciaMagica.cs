using Server.Engines.Craft;
using Server.Items;
using Server.SkillHandlers;
using Server.Targeting;
using System.Collections.Generic;

namespace Server.Ziden
{
    public class Catalizador : Item
    {
        [Constructable]
        public Catalizador() : base(7847)
        {
            Name = "Essencia Magica";
            Stackable = true;
        
        }

        public Catalizador(Serial s) : base(s)
        {

        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Usado para imbuing");
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

    public class EssenciaMagica : Item
    {

        [Constructable]
        public EssenciaMagica() : base(7847)
        {
            Name = "Essencia Magica";
            Stackable = true;
            Weight = 0.1;
        }

        public EssenciaMagica(Serial s) : base(s)
        {

        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Combine Essencias para criar uma Pedra Magica");
        }

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);

            var custo = 100;
            if (!Imbuing.CheckSoulForge(from, 4))
            {
                bool anvil, forge = false;
                DefBlacksmithy.CheckAnvilAndForge(from, 3, out anvil, out forge);
                if (!anvil || !forge)
                {
                    from.SendMessage("Voce precisa estar proximo de uma bigorna e forja para isto caso nao esteja proximo de uma forja das almas.");
                    return;
                }

                from.SendMessage("Por estar longe de uma forja das almas voce precisa de mais material");
                custo *= 3;
            }

            if (this.Amount < custo)
            {
                from.SendMessage("Voce precisa de "+custo+ " essencias do mesmo tipo para criar uma pedra magica");
                return;
            }

            var garrafa = from.FindItemByType<Bottle>();
            if (garrafa == null || garrafa.Amount == 0)
            {
                from.SendMessage("Voce precisa de uma garrafa vazia para colocar o po da essencia");
                return;
            }

            garrafa.Consume(1);
            Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
            Effects.PlaySound(from.Location, from.Map, 0x243);

            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

            Effects.SendTargetParticles(from, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);

            this.Consume(custo);
            from._PlaceInBackpack(new PedraMagica());
            from.SendMessage("Voce transformou as essencias em um po magico e guardou em uma garrafa");
            //from.CheckSkillMult(SkillName.TasteID, 0, 80);
            from.PlayAttackAnimation();
        }

    }

    public class PedraMagica : Item
    {

        [Constructable]
        public PedraMagica() : base(0x0F09)
        {
            Hue = TintaBranca.COR;
            Stackable = true;
            Name = "Jarro de Po Magico";
        }

        public PedraMagica(Serial s) : base(s)
        {

        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Com Imubing pode-se adicionar isto em uma armadura ou arma para melhora-la");
        }

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);
            from.SendMessage("Selecione uma arma ou armadura para anexar a essencia");
            from.Target = new IT(this);
        }

        public class IT : Target
        {
            private Item pedra;

            public IT(Item pedra) : base(12, false, TargetFlags.None)
            {
                this.pedra = pedra;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is BaseWeapon)
                {
                    var w = targeted as BaseWeapon;
                    if (w.LootType == LootType.Blessed)
                    {
                        from.SendMessage("Nessa arma nao pode...");
                        return;
                    }
                    bool foi = false;
                    var list = new List<int>(new int[] { 0, 1, 2 });

                    if (w.DamageLevel >= WeaponDamageLevel.Force)
                    {
                        Shard.Debug("Tira dmg");
                        list.Remove(0);
                    }

                    if (w.AccuracyLevel >= WeaponAccuracyLevel.Veloz)
                    {
                        Shard.Debug("Tira acuracy");
                        list.Remove(1);
                    }


                    if (w.DurabilityLevel >= WeaponDurabilityLevel.Massive)
                    {
                        list.Remove(2);
                        Shard.Debug("Tira durab");
                    }


                    if (list.Count == 0)
                    {
                        from.SendMessage("Este item nao pode ser melhorado...");
                        return;
                    }

                    if (!from.CheckSkillMult(SkillName.Imbuing, -20, 100, 3))
                    {
                        from.SendMessage("Voce quebrou o jarro tentando aplica-lo.");
                        this.pedra.Consume(1);
                    }

                    var random = list[Utility.Random(list.Count)];

                    Shard.Debug("Random " + random);

                    switch (random)
                    {
                        case 0:
                            if (w.DamageLevel < WeaponDamageLevel.Force)
                            {
                                w.DamageLevel++;
                                foi = true;
                            }
                            break;
                        case 1:
                            if (w.AccuracyLevel < WeaponAccuracyLevel.Veloz)
                            {
                                foi = true;
                                w.AccuracyLevel++;
                            }
                            break;
                        case 2:
                            if (w.DurabilityLevel < WeaponDurabilityLevel.Massive)
                            {
                                foi = true;
                                w.DurabilityLevel++;
                            }
                            break;
                    }
                    this.pedra.Consume(1);
                    from.SendMessage("Voce quebra a garrafa de po magico com o equipamento espalhando o po, aprimorando o equipamento");
                    from.PlaySound(0x22D);
                    w.PublicOverheadMessage("* aprimorado *");
                    w.Identified = true;
                    from.CheckSkillMult(SkillName.Imbuing, 0, 100, 1);
                }
                else if (targeted is BaseArmor)
                {
                    var w = targeted as BaseArmor;
                    if(w.LootType == LootType.Blessed)
                    {
                        from.SendMessage("Nessa armadura nao pode...");
                        return;
                    }
                    bool foi = false;
                    var list = new List<int>(new int[] { 0, 1 });

                    if (w.ProtectionLevel >= ArmorProtectionLevel.Hardening)
                        list.Remove(0);

                    if (w.Durability >= ArmorDurabilityLevel.Massive)
                        list.Remove(1);

                    if (list.Count == 0)
                    {
                        from.SendMessage("Este item nao pode ser melhorado...");
                        return;
                    }

                    if (!from.CheckSkillMult(SkillName.Imbuing, -20, 100, 3))
                    {
                        from.SendMessage("Voce quebrou o jarro tentando aplica-lo.");
                        this.pedra.Consume(1);
                    }

                    var random = list[Utility.Random(list.Count)];
                    switch (random)
                    {
                        case 0:
                            w.ProtectionLevel++;
                            break;
                        case 1:
                            w.Durability++;
                            break;
                    }
                    this.pedra.Consume(1);
                    from.SendMessage("Voce quebra a garrafa de po magico com o equipamento espalhando o po, aprimorando o equipamento");
                    from.PlaySound(0x22D);
                    w.PublicOverheadMessage("* aprimorado *");
                    from.CheckSkillMult(SkillName.Imbuing, 0, 100, 1);
                }
                else
                {
                    from.SendMessage("Use isto em uma arma ou armadura para aprimora-la !");
                }
            }
        }
    }
}
