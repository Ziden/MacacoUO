using System;
using System.Collections.Generic;
using Server.Fronteira.Talentos;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells.Eighth;
using Server.Spells.Fifth;
using Server.Spells.First;
using Server.Spells.Fourth;
using Server.Spells.Second;
using Server.Spells.Seventh;
using Server.Spells.Third;

namespace Server.Spells
{
    public abstract class MagerySpell : Spell
    {
        public override void SayMantra()
        {
            if (Info.Mantra != null && Info.Mantra.Length > 0 && (m_Caster.Player || (m_Caster is BaseCreature && ((BaseCreature)m_Caster).ShowSpellMantra)))
            {
                m_Caster.OverheadMessage(Info.Mantra);
                /*
                var eable = m_Caster.Map.GetClientsInRange(m_Caster.Location);
                foreach (NetState state in eable)
                {
                    if (state.Mobile.CanSee(m_Caster))
                    {
                        m_Caster.PrivateOverheadMessage(Info.Mantra, state.Mobile);

                        if (state.Mobile == m_Caster || state.Mobile.Skills.Magery.Value > 50)
                        {
                            m_Caster.PrivateOverheadMessage(Info.Mantra, state.Mobile);
                        } else
                        {
                            m_Caster.PrivateOverheadMessage("* conjurando *", state.Mobile);
                        }
                    }
                }
                */
            }
        }

        // Magias q vao ficar mais dificeis castar andando
        public static readonly Type[] MovementNerfWhenRepeated =
        {
            typeof(TeleportSpell)
        };

        public virtual int GetMinSkill { get { return 0; } }
   
        private static readonly int[] m_ManaTable = new int[] { 4, 6, 9, 11, 14, 20, 40, 50 };
        private const double ChanceOffset = 20.0, ChanceLength = 100.0 / 7.0;
        public MagerySpell(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public MagerySpell() { }

        public abstract SpellCircle Circle { get; }
        public override TimeSpan CastDelayBase
        {
            get
            {
                return TimeSpan.FromSeconds((3 + (int)Circle) * CastDelaySecondsPerTick);
            }
        }
        public override bool ConsumeReagents()
        {
            if (base.ConsumeReagents())
                return true;

            if (ArcaneGem.ConsumeCharges(Caster, (Core.SE ? 1 : 1 + (int)Circle)))
                return true;

            return false;
        }

        public override bool ValidateCast(Mobile from)
        {
            /*
            if (from.RP && from.Player)
            {
                if(((PlayerMobile)from).Talentos.Tem(Talento.ArmaduraMagica);
                if (talento >= 1)
                    return true;
            }
            var circleMax = CicloArmadura(from);
            if (circleMax < (int)this.Circle + 1)
            {
                from.SendMessage("Esta armadura e muito pesada para esta magia");
                return false;
            }
             */
            return true;
        }

        public override int SkillNeeded
        {
            get
            {
                if (Circle == SpellCircle.Eighth)
                    return 90;
                if (Circle == SpellCircle.Seventh)
                    return 80;
                if (Circle == SpellCircle.Sixth)
                    return 65;

                return (int)((double)(Circle + 1) * 11);
            }
        }

        public override void GetCastSkills(out double min, out double max)
        {
            int circle = (int)Circle;

            if (Scroll != null)
                circle -= 2;

            double avg = ChanceLength * circle;

            min = avg - ChanceOffset;
            max = avg + ChanceOffset;

            var m = GetMinSkill;
            if (m != 0)
            {
                min = m;
            }
        }

        public override int GetMana()
        {
            if (Scroll is BaseWand)
                return 0;

            return m_ManaTable[(int)Circle];
        }



        public virtual bool CheckResisted(Mobile target)
        {
            if (target == Caster)
                return false;

            if (target.Player && !Caster.Player)
            {
                var lvl = ColarElemental.GetNivel(target, ElementoPvM.Luz);
                if (lvl > 0 && target.Skills.Parry.Value >= 100)
                {
                    var chance = 0.10;
                    chance += lvl / 100d;
                    if (Utility.RandomDouble() <= chance)
                    {
                        target.SendMessage("Voce bloqueou a magia com seu escudo");
                        target.FixedEffect(0x37B9, 10, 16);
                        target.Animate(AnimationType.Parry, 0);
                        return true;
                    }
                }

            }

            if (target.IsPlayer() && target.LastCast + TimeSpan.FromSeconds(1) > DateTime.UtcNow)
            {
                if (target.LastCaster != Caster)
                {
                    target.LastCaster = Caster;
                    target.LastResist = DateTime.UtcNow;
                    return true;
                }
            }
            target.LastCast = DateTime.UtcNow;
            target.LastCaster = Caster;

            double n = GetResistPercent(target);

            Shard.Debug("Resist % " + n, target);

            n /= 100.0;

            if (n <= 0.0)
                return false;

            if (n >= 1.0)
            {
                target.LastResist = DateTime.UtcNow;
                return true;
            }

            int maxSkill = (1 + (int)Circle) * 10;
            maxSkill += (1 + ((int)Circle / 6)) * 25;

            if (target.Skills[SkillName.MagicResist].Value < maxSkill && target != Caster)
                target.CheckSkillMult(SkillName.MagicResist, 0.0, target.Skills[SkillName.MagicResist].Cap);

            var resisted = (n >= Utility.RandomDouble());

            if (resisted)
            {
                Caster.PlaySound(0x1E6);
                target.FixedEffect(0x42CF, 10, 5);
                target.LastResist = DateTime.UtcNow;
            }
            return resisted;
        }

        public static HashSet<Type> DarkSpells = new HashSet<Type>(new Type[] {
            typeof(HarmSpell), typeof(MindBlastSpell),  typeof(ParalyzeSpell),
            typeof(CurseSpell), typeof(WeakenSpell), typeof(ClumsySpell), typeof(FeeblemindSpell),
            typeof(ManaDrainSpell), typeof(ManaVampireSpell)
        });

        public virtual double GetResistPercentForCircle(Mobile target, SpellCircle circle)
        {
            if (!Shard.POL_STYLE)
            {
                double value = GetResistSkill(target);
                double firstPercent = value / 5.0;
                double secondPercent = value - (((Caster.Skills[CastSkill].Value - 20.0) / 5.0) + (1 + (int)circle) * 5.0);
                return (firstPercent > secondPercent ? firstPercent : secondPercent) / 2.0; // Seems should be about half of what stratics says.
            }
            else
            {
                var resist = target.Skills[SkillName.MagicResist].Value;
                if (target.Player && !Caster.Player)
                {
                    resist += (int)(resist * Caster.GetBonusElemento(ElementoPvM.Agua));
                    resist += (int)(resist * Caster.GetBonusElemento(ElementoPvM.Luz));
                    resist += ColarElemental.GetNivel(Caster, ElementoPvM.Agua);
                    resist += ColarElemental.GetNivel(Caster, ElementoPvM.Escuridao) * 2;

                }
                if (!target.Player && Caster.Player)
                {
                    var bonus = resist * Caster.GetBonusElemento(ElementoPvM.Escuridao);
                    if (bonus > resist) bonus = resist;
                    resist -= bonus;
                }
                if (target.Player && target.RP)
                {
                    if (((PlayerMobile)target).Talentos.Tem(Talento.PeleArcana))
                        resist += 10;
                }
                if (Caster.Player && Caster.RP)
                {
                    if (((PlayerMobile)Caster).Talentos.Tem(Talento.MentePerfurante))
                        resist -= 10;
                }
                if (target.Player && Caster.Player && resist > 100)
                    resist = 100;
                

                var cap = resist / 5;

                var magery = Caster.Skills[CastSkill].Value;
                var circ = 1 + (double)circle;

                if (Caster.Player && target.Player && magery > 100)
                    magery = 100;

                var chance = ((magery * 2) / 10 + circ * circ);

                if (Shard.DebugEnabled)
                    Shard.Debug("Chance Base: " + chance + " circulo " + circ);

                chance = resist - chance;
                if (chance < cap)
                    chance = cap;

                if (Shard.SPHERE_STYLE)
                    chance *= 0.35; // sem pre cast mais dificil de resistir
                else if (Circle >= SpellCircle.Sixth)
                    chance *= 0.90;

                if (Caster is BaseCreature && target is PlayerMobile)
                    chance /= 1.5;

                Shard.Debug("Chance RS: " + chance, target);

                return chance;
            }
        }

        public virtual double GetResistPercent(Mobile target)
        {

            return GetResistPercentForCircle(target, Circle);
        }

        public override TimeSpan GetCastDelay()
        {
            if (!Core.AOS)
            {
                if (Caster is BaseCreature)
                    return TimeSpan.FromSeconds(0.7 + 0.40 * (int)Circle);

                var pl = Caster as PlayerMobile;

                if (pl.RP)
                {
                    if (pl.Talentos.Tem(Talento.Cajados) && Caster.Weapon is BaseStaff)
                        return TimeSpan.FromSeconds(1 + (0.35 * (int)(1 + Circle)));
                    return TimeSpan.FromSeconds(1 + (0.6 * (int)Circle));
                }

                if (Shard.POL_STYLE)
                    return TimeSpan.FromSeconds(0.5 + (0.4 * (int)Circle));
                else if (Shard.SPHERE_STYLE)
                    return TimeSpan.FromSeconds(1 + (0.5 * (int)Circle));
                else
                    return TimeSpan.FromSeconds(0.5 + (0.25 * (int)(1 + Circle)));

            }
            return base.GetCastDelay();
        }
    }
}
