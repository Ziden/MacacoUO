using System;
using Server.Items;
using Server.Targeting;

namespace Server.Spells.Sixth
{
    public class EnergyBoltSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Energy Bolt", "Corp Por",
            230,
            9022,
            Reagent.BlackPearl,
            Reagent.Nightshade);
        public EnergyBoltSpell(Mobile caster, Item scroll)
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
        public override bool DelayedDamage
        {
            get
            {
                return false;
            }
        }
        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IDamageable m)
        {
            if (!Caster.CanSee(m, true))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                IDamageable source = Caster;
                IDamageable target = m;


                // Do the effects
                Caster.MovingParticles(m, 0x379F, 7, 0, false, true, 3043, 4043, 0x211);
                Caster.PlaySound(0x20A);


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
                        var newSpell = new EnergyBoltSpell(m as Mobile, null);
                        newSpell.PassSequence = true;
                        newSpell.OriginalCaster = this.OriginalCaster;
                        newSpell.OriginalCaster.NextSpellTime = Core.TickCount + 2000;
                        newSpell.Target(Caster);
                    });
                    return;
                }

                double damage = 0;

                if (Core.AOS)
                {
                    damage = GetNewAosDamage(40, 1, 5, m);
                }
                else if (m is Mobile)
                {
                    Mobile mob = m as Mobile;
                    damage = Utility.Random(25, 15);

                    if (CheckResisted(mob))
                    {
                        damage *= 0.7;

                        mob.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    // Scale damage based on evalint and resist
                    damage *= GetDamageScalar(mob, ElementoPvM.Raio);

                    if (!mob.Player)
                    {
                        var nivel = ColarElemental.GetNivel(Caster, ElementoPvM.Raio);
                        damage *= 1 + (nivel / 15);
                    }

                }

                if (damage > 0)
                {
                    // Deal the damage
                    SpellHelper.Damage(this, target, damage, 0, 0, 0, 0, 100, Items.ElementoPvM.Raio);
                }
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly EnergyBoltSpell m_Owner;
            public InternalTarget(EnergyBoltSpell owner)
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
