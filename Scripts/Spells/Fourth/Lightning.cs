using System;
using Server.Targeting;
using Server.Mobiles;
using Server.Items;

namespace Server.Spells.Fourth
{
    public class LightningSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Lightning", "Por Ort Grav",
            239,
            9021,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);
        public LightningSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Fourth;
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
            Mobile mob = m as Mobile;

            if (!Caster.CanSee(m, true))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                Mobile source = Caster;
                // SpellHelper.Turn(Caster, m.Location);

                if (m is Mobile)
                {
                    Effects.SendBoltEffect(m, true, 0, false);
                }
                else
                {
                    Effects.SendBoltEffect(EffectMobile.Create(m.Location, m.Map, EffectMobile.DefaultDuration), true, 0, false);
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
                        var newSpell = new LightningSpell(m as Mobile, null);
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
                    damage = GetNewAosDamage(23, 1, 4, m);
                }
                else if (mob != null)
                {
                    damage = Utility.Random(11, 11);

                    if (CheckResisted(mob))
                    {
                        damage *= 0.7;  
                        mob.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    damage *= GetDamageScalar(mob, Items.ElementoPvM.Raio);
                }


                if (mob != null && !mob.Player)
                {
                    var nivel = ColarElemental.GetNivel(Caster, ElementoPvM.Raio);
                    damage *= 1 + (nivel / 14);
                }

                if (damage > 0)
                {
                    SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100, Items.ElementoPvM.Raio);
                }
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly LightningSpell m_Owner;
            public InternalTarget(LightningSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
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
