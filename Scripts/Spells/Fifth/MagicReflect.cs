using Server.Network;
using System;
using System.Collections;

namespace Server.Spells.Fifth
{
    public class MagicReflectSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Magic Reflection", "In Jux Sanct",
            242,
            9012,
            Reagent.Garlic,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        private static readonly Hashtable m_Table = new Hashtable();
        public MagicReflectSpell(Mobile caster, Item scroll)
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
        public static bool EndReflect(Mobile m)
        {
            if (m_Table.Contains(m))
            {
                ResistanceMod[] mods = (ResistanceMod[])m_Table[m];

                if (mods != null)
                {
                    for (int i = 0; i < mods.Length; ++i)
                        m.RemoveResistanceMod(mods[i]);
                }

                m_Table.Remove(m);
                BuffInfo.RemoveBuff(m, BuffIcon.MagicReflection);
                return true;
            }
            return false;
        }

        public override bool CheckCast()
        {
            if (Core.AOS)
                return true;

            if (this.Caster.MagicDamageAbsorb > 0)
            {
                this.Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                return false;
            }
            else if (!this.Caster.CanBeginAction(typeof(DefensiveSpell)))
            {
                this.Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                return false;
            }
            return true;
        }

        public override void OnCast()
        {
            if (this.Caster.MagicDamageAbsorb > 0)
            {
                this.Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
            }
            else if (!this.Caster.CanBeginAction(typeof(DefensiveSpell)))
            {
                this.Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
            }
            else if (this.CheckSequence())
            {
                if (this.Caster.BeginAction(typeof(DefensiveSpell)))
                {
                    int skills = (int)(this.Caster.Skills[SkillName.Magery].Value + this.Caster.Skills[SkillName.Inscribe].Value);
                    var value = (int)(3 + (skills / 200) * 7.0) + Utility.Random(4) - 2;

                    this.Caster.MagicDamageAbsorb = value;
                    this.Caster.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);
                    this.Caster.PlaySound(0x1E9);
                    //this.Caster.LocalOverheadMessage(MessageType.Regular, 0, false, "MRPower )

                    //this.Caster.PrivateOverheadMessage("MR Power: " + value * 10 + "%");
                    this.Caster.SendMessage("Você tem " + value + " círculos de reflexão mágica.");
                }
                else
                {
                    this.Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                }
            }
            this.FinishSequence();
        }

        #region SA
        public static bool HasReflect(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }
        #endregion
    }
}
