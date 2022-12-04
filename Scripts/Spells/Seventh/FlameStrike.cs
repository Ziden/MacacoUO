using System;
using Server.Items;
using Server.Targeting;

namespace Server.Spells.Seventh
{
    public class FlameStrikeSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Flame Strike", "Kal Vas Flam",
            245,
            9042,
            Reagent.SpidersSilk,
            Reagent.SulfurousAsh
        );

        public FlameStrikeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Seventh;
            }
        }

        public override int SkillNeeded { get { return 90; } }

        public override bool DelayedDamage { get { return true; } } //removido deley do dano do FS

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public override TimeSpan GetCastDelay()
        {
            if (Shard.SPHERE_STYLE && Caster.Player)
                return TimeSpan.FromSeconds(3);
            else
                return base.GetCastDelay();
        }

        public void Target(IDamageable m)
        {
            if (!this.Caster.CanSee(m, true))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (this.CheckHSequence(m))
            {
                Mobile source = this.Caster;

                if (m != null)
                {
                    if (m != Caster)
                        Caster.MovingParticles(m, 0x3709, 7, 10, true, true, 3043, 4019, 0x160, EffectLayer.LeftFoot);
                    else
                        m.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
                    m.PlaySound(0x208);
                }

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
                        var newSpell = new FlameStrikeSpell(m as Mobile, null);
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
                    damage = GetNewAosDamage(48, 1, 5, m);
                }
                else if (m is Mobile)
                {
                    damage = Utility.Random(27, 22);

                    if (this.CheckResisted((Mobile)m))
                    {
                        damage *= 0.6;
                        ((Mobile)m).SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    if(!((Mobile)m).Player)
                    {
                        var nivel = ColarElemental.GetNivel(Caster, ElementoPvM.Fogo);
                        damage *= 1 + (nivel / 10);
                    }

                    damage *= this.GetDamageScalar((Mobile)m, ElementoPvM.Fogo);
                }

             

                if (damage > 0)
                {
                    SpellHelper.Damage(this, m, damage, 0, 100, 0, 0, 0, ElementoPvM.Fogo);
                }
            }
            this.FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly FlameStrikeSpell m_Owner;
            public InternalTarget(FlameStrikeSpell owner)
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
