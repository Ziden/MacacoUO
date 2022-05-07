using System;
using Server.Items;

namespace Server.SkillHandlers
{
    class Meditation
    {
        public static void Initialize()
        {
            SkillInfo.Table[46].Callback = new SkillUseCallback(OnUse);
        }

        public static bool CheckOkayHolding(Item item)
        {
            if (item == null)
                return true;

            if (item is Spellbook || item is Runebook || item is BaseWand)
                return true;

            if (item is BaseStaff)
                return true;

            if (item is BaseShield) //Permitir meditar com escudo na mão
                return true;

            if (Core.AOS && item is BaseArmor && ((BaseArmor)item).Attributes.ResistMagica != 0)
                return true;

            if (item is SmugglersLantern)
                return true;

            return false;
        }

        public static TimeSpan OnUse(Mobile m)
        {
            if(m.Meditating)
            {
                m.SendMessage("Você já está meditando");
                return TimeSpan.FromSeconds(5.0);
            }

            if(m.Paralyzed || m.Frozen)
            {
                m.SendMessage("Voce esta ocupado fazendo outra coisa"); // You are busy doing something else and cannot hide.
                return TimeSpan.FromSeconds(1.0);
            }

            if(BandageContext.GetContext(m) != null)
            {
                m.SendMessage("Você não consegue se concentrar enquanto aplica bandagens");
                return TimeSpan.FromSeconds(5.0);
            }

            m.RevealingAction(false);

            if (m.Target != null)
            {
                m.SendLocalizedMessage(501845); // You are busy doing something else and cannot focus.

                return TimeSpan.FromSeconds(5.0);
            }
            /*else if (!Core.AOS && m.Hits < (m.HitsMax / 10)) // Less than 10% health
            {
                m.SendMessage("Voce esta muito fraco para meditar"); // The mind is strong but the body is weak.

                return TimeSpan.FromSeconds(5.0);
            }*/
            else if (m.Mana >= m.ManaMax)
            {
                m.PlaySound(0x0F5);
                m.PrivateOverheadMessage("* Voce esta em paz *"); // You are at peace.
                return TimeSpan.FromSeconds(5.0);
            }
            //else if (Core.AOS && Server.Misc.RegenRates.GetArmorOffset(m) > 0)
            else if (Server.Misc.RegenRates.GetArmorOffset(m) > 6) //Retirada validação AOS e liberado medit com armaduras de malha para baixo
            {
                //m.SendLocalizedMessage(500135); // Regenative forces cannot penetrate your armor!
                m.SendMessage("Seu equipamento impede a concentração");

                return TimeSpan.FromSeconds(10.0);
            }
            else 
            {
                Item oneHanded = m.FindItemOnLayer(Layer.OneHanded);
                Item twoHanded = m.FindItemOnLayer(Layer.TwoHanded);

                if (Core.AOS && m.Player)
                {
                    if (!CheckOkayHolding(oneHanded))
                        m.AddToBackpack(oneHanded);

                    if (!CheckOkayHolding(twoHanded))
                        m.AddToBackpack(twoHanded);
                }
                /*else if (!CheckOkayHolding(oneHanded) || !CheckOkayHolding(twoHanded))
                {
                    //m.SendLocalizedMessage(502626); // Your hands must be free to cast spells or meditate.
                    m.SendMessage("Você não pode meditar com armas na mão");

                    return TimeSpan.FromSeconds(2.5);
                }*/
                else if (!CheckOkayHolding(oneHanded))
                {
                    //m.SendLocalizedMessage(502626); // Your hands must be free to cast spells or meditate.
                    m.SendMessage("Você não pode meditar com armas na mão");

                    return TimeSpan.FromSeconds(2.5);
                }
                else if (!CheckOkayHolding(twoHanded))
                {
                    //m.SendLocalizedMessage(502626); // Your hands must be free to cast spells or meditate.
                    m.SendMessage("Você não pode meditar com armas na mão");

                    return TimeSpan.FromSeconds(2.5);
                }

                double skillVal = m.Skills[SkillName.Meditation].Value;
                double chance = 100; // (50.0 + ((skillVal - (m.ManaMax - m.Mana)) * 2)) / 100;

                // must bypass normal checks so passive skill checks aren't triggered
                CrystalBallOfKnowledge.TellSkillDifficultyActive(m, SkillName.Meditation, chance);

                if (m.CheckSkillMult(SkillName.Meditation, -10.0, 100.0))
                {
                    ;
                    m.PlaySound(0x0F5);
                    m.OverheadMessage("* Meditando *");
                    m.SendMessage("Você comeca a meditar");
                    m.Meditating = true;
                    BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.ActiveMeditation, 1075657));
                    m.ResetStatTimers();
                }
                else 
                {
                    m.SendLocalizedMessage("Voce nao conseguiu se concentrar"); // You cannot focus your concentration.
                }

                return TimeSpan.FromSeconds(10.0);
            }
        }
    }
}
