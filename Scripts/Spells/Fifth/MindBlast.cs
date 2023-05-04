using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Fifth
{
    public class MindBlastSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mind Blast", "Por Corp Wis",
            218,
            Core.AOS ? 9002 : 9032,
            Reagent.BlackPearl,
            Reagent.MandrakeRoot,
            Reagent.Nightshade,
            Reagent.SulfurousAsh);
        public MindBlastSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
            if (Core.AOS)
                m_Info.LeftHandEffect = m_Info.RightHandEffect = 9002;
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Fifth;
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

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m, true))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (Core.AOS)
            {
                if (Caster.CanBeHarmful(m) && CheckSequence())
                {
                    Mobile from = Caster, target = m;

                    //SpellHelper.Turn(from, target);
                    SpellHelper.CheckReflect((int)Circle, ref from, ref target);

                    int intel = Math.Min(200, Caster.Int);

                    int damage = (int)((Caster.Skills[SkillName.Magery].Value + intel) / 5) + Utility.RandomMinMax(2, 6);

                    if (damage > 60)
                        damage = 60;

                    Timer.DelayCall(TimeSpan.FromSeconds(1.0),
                        new TimerStateCallback(AosDelay_Callback),
                        new object[] { Caster, target, m, damage });
                }
            }
            else if (CheckHSequence(m))
            {
                Mobile from = Caster, target = m;

                //SpellHelper.Turn(from, target);

                from.FixedParticles(0x374A, 10, 15, 2038, EffectLayer.Head);

                target.FixedParticles(0x374A, 10, 15, 5038, EffectLayer.Head);
                target.PlaySound(0x213);

                if (target != null)
                {
                    if (SpellHelper.CheckReflect((int)Circle, ref m, ref target))
                    {
                        Timer.DelayCall(TimeSpan.FromSeconds(Spell.SECONDS_REFLECT), () =>
                        {
                            if (this.OriginalCaster == null)
                            {
                                this.OriginalCaster = Caster;
                            }
                            FinishSequence();
                            var newSpell = new MindBlastSpell(m as Mobile, null);
                            newSpell.PassSequence = true;
                            newSpell.OriginalCaster = this.OriginalCaster;
                            newSpell.OriginalCaster.NextSpellTime = Core.TickCount + 2000;
                            newSpell.Target(Caster);
                        });
                        return;
                    }
                }

                double damage = GetDamageScalar(m) * ((Caster.Int - target.Int) / 2);

                if (!m.Player)
                {
                    damage = GetDamageScalar(m, ElementoPvM.Escuridao) * ((Caster.Int - target.Int) / 2); //less damage                    
                    damage += ColarElemental.GetNivel(Caster, ElementoPvM.Escuridao) * (Caster.Int / 25);
                }

                if (m.Player && damage > 45)
                    damage = 45;

                if (CheckResisted(target))
                {
                    damage /= 2;
                    target.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                }

                SpellHelper.Damage(this, target, damage, 0, 0, 100, 0, 0, ElementoPvM.Escuridao);
            }

            FinishSequence();
        }

        public override double GetSlayerDamageScalar(Mobile target)
        {
            return 1.0; //This spell isn't affected by slayer spellbooks
        }

        private void AosDelay_Callback(object state)
        {
            object[] states = (object[])state;
            Mobile caster = (Mobile)states[0];
            Mobile target = (Mobile)states[1];
            Mobile defender = (Mobile)states[2];
            int damage = (int)states[3];

            if (caster.HarmfulCheck(defender))
            {
                target.FixedParticles(0x374A, 10, 15, 5038, 1181, 2, EffectLayer.Head);
                target.PlaySound(0x213);

                SpellHelper.Damage(this, target, Utility.RandomMinMax(damage, damage + 4), 0, 0, 100, 0, 0, Items.ElementoPvM.Escuridao);
            }
        }

        private class InternalTarget : Target
        {
            private readonly MindBlastSpell m_Owner;
            public InternalTarget(MindBlastSpell owner)
                : base(Spell.RANGE, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    m_Owner.Target((Mobile)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}
