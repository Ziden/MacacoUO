using System;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Spells.Fourth
{
    public class GreaterHealSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Greater Heal", "In Vas Mani",
            204,
            9061,
            Reagent.Garlic,
            Reagent.Ginseng,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        public GreaterHealSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public static double GetPoisonScalar(Poison p)
        {
            if(Shard.POL_STYLE)
            {
                return 1;
            }

            if (p == null)
                return 1;

            if (p == Poison.Lesser)
                return 0.7;
            else if (p == Poison.Regular)
                return 0.6;
            else if (p == Poison.Greater)
                return 0.4;
            else if (p == Poison.Deadly)
                return 0.3;
            else if (p == Poison.Lethal)
                return 0.2;
            return 1;
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Fourth;
            }
        }

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public override TimeSpan GetCastDelay()
        {
            if (Shard.SPHERE_STYLE && Caster.Player)
                return TimeSpan.FromSeconds(3);
            if (Shard.POL_STYLE && Caster.Player)
                return base.GetCastDelay() + TimeSpan.FromSeconds(0.4);
            else 
                return base.GetCastDelay();
        }

        public void Target(Mobile m)
        {
            if (!this.Caster.CanSee(m, true))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (m is BaseCreature && ((BaseCreature)m).IsAnimatedDead)
            {
                this.Caster.SendLocalizedMessage(1061654); // You cannot heal that which is not alive.
            }
            else if (m.IsDeadBondedPet)
            {
                this.Caster.SendLocalizedMessage(1060177); // You cannot heal a creature that is already dead!
            }
            else if (m is IRepairableMobile)
            {
                this.Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500951); // You cannot heal that.
            }
            else if (Shard.SPHERE_STYLE && m.Poisoned)
            {
                this.Caster.PrivateOverheadMessage("Sua cura nao penetra o veneno...");
            }
            else if (Server.Items.MortalStrike.IsWounded(m))
            {
                this.Caster.LocalOverheadMessage(MessageType.Regular, 0x22, (this.Caster == m) ? 1005000 : 1010398);
            }
            else if (this.CheckBSequence(m))
            {
                int toHeal = (int)(this.Caster.Skills[SkillName.Magery].Value * 0.2);
                if (!Shard.POL_STYLE)
                    toHeal *= 2;
                else
                {
                    var inscript = this.Caster.Skills[SkillName.Inscribe].Value;
                    var inscriptBonus = (int)(inscript * 0.15);
                    toHeal += inscriptBonus;
                }
                toHeal += Utility.Random(1, 10);
                if (Caster is BaseCreature)
                    toHeal = (int)(toHeal * 1.2);

                if (Shard.SPHERE_STYLE && m.Poisoned)
                {
                    toHeal = 0;
                }
                else
                {
                    var scalar = GetPoisonScalar(m.Poison);
                    if (scalar < 1 && !m.IsCooldown("poisonmsg"))
                    {
                        m.SetCooldown("poisonmsg");
                        m.SendMessage(78, "Voce curou menos vida por estar envenenado. Quanto mais forte o veneno, mais dificil se curar.");
                    }
                    toHeal = (int)(toHeal * scalar);
                }

                SpellHelper.Heal(toHeal, m, this.Caster);

                Caster.MovingParticles(m, 0x376A, 7, 0, true, false, 9502, 0x376A, 0x1F2);
                m.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                m.PlaySound(0x202);
            }

            this.FinishSequence();
        }

        public virtual bool PunishRepeatedSpells { get { return false; } }

        public class InternalTarget : Target
        {
            private readonly GreaterHealSpell m_Owner;
            public InternalTarget(GreaterHealSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    this.m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}
