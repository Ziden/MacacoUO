using System;
using Server.Items;
using Server.Targeting;

namespace Server.Spells.Sixth
{
    public class ExplosionSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Explosion", "Vas Ort Flam",
            230,
            9041,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);
        public ExplosionSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Sixth;
            }
        }
        public override bool DelayedDamageStacking
        {
            get
            {
                return !Core.AOS;
            }
        }
        public override bool DelayedDamage
        {
            get
            {
                return true;
            }
        }
        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IDamageable m)
        {
            if (Core.SA && HasDelayContext(m))
            {
                DoHurtFizzle();
                return;
            }

            if (!Caster.CanSee(m, true))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (Caster.CanBeHarmful(m) && CheckSequence())
            {
                Mobile attacker = Caster;

                var wtf = m;
                if (SpellHelper.CheckReflect((int)Circle, this.Caster, ref wtf))
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(Spell.SECONDS_REFLECT), () =>
                    {
                        if (this.OriginalCaster == null)
                        {
                            this.OriginalCaster = Caster;
                        }
                        FinishSequence();
                        var newSpell = new ExplosionSpell(m as Mobile, null);
                        newSpell.PassSequence = true;
                        newSpell.OriginalCaster = this.OriginalCaster;
                        newSpell.OriginalCaster.NextSpellTime = Core.TickCount + 2000;
                        newSpell.Target(Caster);
                    });
                    return;
                }

                InternalTimer t = new InternalTimer(this, attacker, m);
                t.Start();
            }

            FinishSequence();
        }

        private class InternalTimer : Timer
        {
            private readonly MagerySpell m_Spell;
            private readonly IDamageable m_Target;
            private readonly Mobile m_Attacker;

            public InternalTimer(MagerySpell spell, Mobile attacker, IDamageable target)
                : base(spell.GetCastDelay() + TimeSpan.FromMilliseconds(Mobile.EquipItemDelay))
            {
                m_Spell = spell;
                m_Attacker = attacker;
                m_Target = target;

                if (m_Spell != null)
                    m_Spell.StartDelayedDamageContext(attacker, this);

                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                Mobile defender = m_Target as Mobile;

                if (m_Attacker.HarmfulCheck(m_Target))
                {
                    double damage = 0;
                    var aoe = false;

                    if (Core.AOS)
                    {
                        damage = m_Spell.GetNewAosDamage(40, 1, 5, m_Target);
                    }
                    else if (defender != null)
                    {
                        damage = Utility.Random(20, 22);

                        if (m_Spell.CheckResisted(defender))
                        {
                            damage *= 0.6;

                            defender.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                        }

                        damage *= m_Spell.GetDamageScalar(defender, ElementoPvM.Fogo);
                    }

                    if (defender != null)
                    {
                        //defender.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                        m_Attacker.MovingParticles(defender, 0x36BD, 6, 10, true, true, 9502, 4019, 0x207);
                        defender.PlaySound(0x207);
                    }
                    else
                    {
                        Effects.SendLocationParticles(m_Target, 0x36BD, 20, 10, 5044);
                        Effects.PlaySound(m_Target.Location, m_Target.Map, 0x207);
                    }

                    if (damage > 0)
                    {
                        SpellHelper.Damage(m_Spell, m_Target, damage, 0, 100, 0, 0, 0, Items.ElementoPvM.Fogo);
                    }

                    if (m_Spell != null)
                        m_Spell.RemoveDelayedDamageContext(m_Attacker);
                }
            }
        }

        private class InternalTarget : Target
        {
            private readonly ExplosionSpell m_Owner;
            public InternalTarget(ExplosionSpell owner)
                : base(Spell.RANGE, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IDamageable)
                    m_Owner.Target((IDamageable)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}
