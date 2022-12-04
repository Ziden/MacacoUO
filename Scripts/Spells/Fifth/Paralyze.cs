using System;
using Server.Mobiles;
using Server.Network;
using Server.Spells.Chivalry;
using Server.Targeting;

namespace Server.Spells.Fifth
{
    public class ParalyzeSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Paralyze", "An Ex Por",
            218,
            9012,
            Reagent.Garlic,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        public ParalyzeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Fifth;
            }
        }
        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public override TimeSpan GetCastDelay()
        {
            if (Shard.SPHERE_STYLE && Caster.Player)
                return TimeSpan.FromSeconds(2.5);
            else
                return base.GetCastDelay();
        }

        public void Target(Mobile m)
        {
            if (!this.Caster.CanSee(m, true))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (Core.AOS && (m.Frozen || m.Paralyzed || (m.Spell != null && m.Spell.IsCasting && !(m.Spell is PaladinSpell))))
            {
                this.Caster.SendLocalizedMessage(1061923); // The target is already frozen.
            }
            else if (this.CheckHSequence(m))
            {
                //SpellHelper.Turn(this.Caster, m);


                if (SpellHelper.CheckReflect((int)Circle, this.Caster, ref m))
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(Spell.SECONDS_REFLECT), () =>
                    {
                        if (this.OriginalCaster == null)
                        {
                            this.OriginalCaster = Caster;
                        }
                        FinishSequence();
                        var newSpell = new ParalyzeSpell(m as Mobile, null);
                        newSpell.PassSequence = true;
                        newSpell.OriginalCaster = this.OriginalCaster;
                        newSpell.OriginalCaster.NextSpellTime = Core.TickCount + 2000;
                        newSpell.Target(Caster);
                    });
                    return;
                }


                double duration;

                m.PlaySound(0x204);
                //m.FixedEffect(0x376A, 6, 1);
                Caster.MovingParticles(m, 0x374A, 8, 0, false, false, 9502, 0x374A, 0x204);

                // Algorithm: ((20% of magery) + 7) seconds [- 50% if resisted]
                duration = Utility.Random(6, 4);

                var limiteParalize = DateTime.UtcNow - TimeSpan.FromSeconds(10);
                if (duration <= 0 || this.CheckResisted(m) || (m.Skills.MagicResist.Value > 60 && m.LastParalized > limiteParalize) || (DateTime.UtcNow < m.PotAntiPara && Utility.Random(6) != 1))
                {
                    duration = 0;
                    m.SendMessage("Voce sente seu corpo resistindo a magia");
                }


                if (m is PlagueBeastLord)
                {
                    ((PlagueBeastLord)m).OnParalyzed(this.Caster);
                    duration = 120;
                }

                if (duration == 0)
                    return;

                m.PrivateOverheadMessage("* Paralizado *");

                m.Paralyze(TimeSpan.FromSeconds(duration));


                this.HarmfulSpell(m);
            }

            this.FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly ParalyzeSpell m_Owner;
            public InternalTarget(ParalyzeSpell owner)
                : base(Spell.RANGE, false, TargetFlags.Harmful)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    this.m_Owner.Target((Mobile)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}
