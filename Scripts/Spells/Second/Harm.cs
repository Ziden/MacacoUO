using System;
using Server.Items;
using Server.Targeting;

namespace Server.Spells.Second
{
    public class HarmSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Harm", "An Mani",
            212,
            Core.AOS ? 9001 : 9041,
            Reagent.Nightshade,
            Reagent.SpidersSilk);
        public HarmSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Second;
            }
        }
        public override bool DelayedDamage
        {
            get
            {
                return false;
            }
        }
        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public override double GetSlayerDamageScalar(Mobile target)
        {
            return 1.0; //This spell isn't affected by slayer spellbooks
        }

        public void Target(IDamageable m)
        {
            Mobile mob = m as Mobile;

            if (!this.Caster.CanSee(m))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (this.CheckHSequence(m))
            {

                SpellHelper.CheckReflect((int)Circle, Caster, ref m);

                // no pol harm tem delay igual explosion
                if (Shard.POL_STYLE)
                {
                    InternalTimer t = new InternalTimer(this, Caster, m);
                    t.Start();
                    return;
                }

                //SpellHelper.Turn(this.Caster, m);
                Mobile source = this.Caster;

                SpellHelper.CheckReflect((int)this.Circle, ref source, ref m);

                double damage = 0;

                if (mob != null)
                {
                    damage = Utility.Random(1, 15);

                    if (this.CheckResisted(mob))
                    {
                        damage *= 0.75;

                        mob.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    damage *= this.GetDamageScalar(mob, ElementoPvM.Escuridao);
                }

                if (!this.Caster.InRange(m, 3))
                    damage *= 0.25; // 1/4 damage at > 2 tile range
                else if (!this.Caster.InRange(m, 2))
                    damage *= 0.50; // 1/2 damage at 2 tile range

                if (mob != null)
                {
                    mob.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
                    mob.PlaySound(0x0FC);
                }
                else
                {
                    Effects.SendLocationParticles(m, 0x374A, 10, 30, 1153, 2, 5013, 0);
                    Effects.PlaySound(m.Location, m.Map, 0x0FC);
                }

                if (damage > 0)
                {
                    SpellHelper.Damage(this, m, damage, 0, 0, 100, 0, 0);
                }
            }

            this.FinishSequence();
        }

        private class InternalTimer : Timer
        {
            private readonly MagerySpell m_Spell;
            private readonly IDamageable m_Target;
            private readonly Mobile m_Attacker;
            private readonly Mobile Caster;

            public InternalTimer(MagerySpell spell, Mobile attacker, IDamageable target)
                : base(TimeSpan.FromSeconds(3))
            {
                m_Spell = spell;
                m_Attacker = attacker;
                m_Target = target;
                this.Caster = attacker;

                if (m_Spell != null)
                    m_Spell.StartDelayedDamageContext(attacker, this);

                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                Mobile defender = m_Target as Mobile;

                if (m_Attacker.HarmfulCheck(m_Target))
                {
                    //SpellHelper.Turn(this.Caster, m_Target);
                    Mobile source = this.Caster;

                    double damage = 0;

                    if (m_Target != null)
                    {
                        damage = Utility.Random(1, 8);


                        if (m_Spell.CheckResisted(defender))
                        {
                            damage *= 0.75;

                            defender.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                        }

                        damage *= m_Spell.GetDamageScalar(defender, ElementoPvM.Escuridao);
                    }

                    if (m_Target == Caster)
                        damage = 1;

                    Caster.MovingParticles(defender, 0x374A, 7, 0, false, false, 5013, 5013, 0x1F1);
                    defender.FixedParticles(0x374A, 10, 15, 5013, EffectLayer.Waist);
                    defender.PlaySound(0x1F1);

                    if (damage > 0)
                    {
                        SpellHelper.Damage(this.m_Spell, defender, damage, 0, 0, 100, 0, 0, ElementoPvM.Escuridao);
                    }

                    if (m_Spell != null)
                        m_Spell.RemoveDelayedDamageContext(m_Attacker);
                }
            }
        }

        private class InternalTarget : Target
        {
            private readonly HarmSpell m_Owner;
            public InternalTarget(HarmSpell owner)
                : base(Spell.RANGE, false, TargetFlags.Harmful)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IDamageable)
                {
                    this.m_Owner.Target((IDamageable)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}
