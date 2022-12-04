using System;
using Server.Regions;
using Server.Targeting;

namespace Server.Spells.Third
{
    public class PoisonSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Poison", "In Nox",
            203,
            9051,
            Reagent.Nightshade);

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Third;
            }
        }

        public PoisonSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
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
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                Caster.MovingParticles(m, 0x374A, 12, 10, false, false, 9502, 0x374A, 0x205);
                m.PlaySound(0x205);

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
                        var newSpell = new PoisonSpell(m as Mobile, null);
                        newSpell.PassSequence = true;
                        newSpell.OriginalCaster = this.OriginalCaster;
                        newSpell.OriginalCaster.NextSpellTime = Core.TickCount + 2000;
                        newSpell.Target(Caster);
                    });
                    return;
                }


                // Poison nao da disturb
                //if (m.Spell != null)
                //    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                var bypassresist = Utility.RandomDouble() < (0.15 + Caster.Skills[SkillName.Poisoning].Value * 0.075);

                if (!bypassresist && CheckResisted(m) || Server.Spells.Mysticism.StoneFormSpell.CheckImmunity(m))
                {
                    m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                }
                else
                {
                    int level;

                    if (Core.AOS)
                    {
                        int total = (Caster.Skills.Magery.Fixed + Caster.Skills.Poisoning.Fixed) / 2;

                        if (Core.SA && Caster.InRange(m, 8))
                        {
                            int range = (int)Caster.GetDistanceToSqrt(m.Location);

                            if (total >= 1000)
                                level = Utility.RandomDouble() <= .1 ? 4 : 3;
                            else if (total > 850)
                                level = 2;
                            else if (total > 650)
                                level = 1;
                            else
                                level = 0;

                            if (!Caster.InRange(m, 2))
                                level -= range / 2;

                            if (level < 0)
                                level = 0;
                        }
                        else if (Caster.InRange(m, 2))
                        {
                            if (total >= 1000)
                                level = 3;
                            else if (total > 850)
                                level = 2;
                            else if (total > 650)
                                level = 1;
                            else
                                level = 0;
                        }
                        else
                        {
                            level = 0;
                        }
                    }
                    else
                    {
                        /*
                        double total = Caster.Skills[SkillName.Magery].Value + Caster.Skills[SkillName.Poisoning].Value;                        
                        double dist = Caster.GetDistanceToSqrt(m);

                        if (dist >= 3.0)
                            total -= (dist - 3.0) * 10.0;

                        if (total >= 200.0 && 1 > Utility.Random(10))
                            level = 3;
                        else if (total >  170.0)
                            level = 2;
                        else if (total > 130.0)
                            level = 1;
                        else
                            level = 0;
                            */
                        level = 0;
                        if (!Shard.SPHERE_STYLE && Caster.Skills[SkillName.Poisoning].Value > 80)
                        {
                            if (!m.IsCooldown("poisonop"))
                            {
                                m.SetCooldown("poisonop");
                                m.SendMessage(78, "O mago inimigo tinha um conhecimento de envenamentos avancado e conseguiu te envenenar de uma maneira mais forte");
                            }
                            level = 1;
                            if (Utility.RandomDouble() < 0.3)
                                level = 2;
                        }

                    }
                    var p = Poison.GetPoison(level);
                    Shard.Debug("Toca " + p, m);
                    var result = m.ApplyPoison(Caster, p);
                    Shard.Debug("Poison Result: " + result.ToString(), m);
                }
            
                HarmfulSpell(m);

                if (Caster.Criminal && Caster.Region is GuardedRegion)
                {
                    ((GuardedRegion)Caster.Region).MakeGuard(Caster);
                    if (m.Poisoned)
                        m.CurePoison(m);
                }
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly PoisonSpell m_Owner;

            public InternalTarget(PoisonSpell owner)
                : base(Spell.RANGE, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}
