using System;
using Server.Mobiles;

namespace Server.Spells.Eighth
{
    public class FireElementalSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Fire Elemental", "Kal Vas Xen Flam",
            269,
            9050,
            false,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk,
            Reagent.SulfurousAsh);
        public FireElementalSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Eighth;
            }
        }
        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            var custoSummon = 4;
            if (m_Caster.Skills.SpiritSpeak.Value >= 100)
                custoSummon = 2;

            if ((this.Caster.Followers + custoSummon) > this.Caster.FollowersMax)
            {
                this.Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            if (this.CheckSequence())
            {
                TimeSpan duration = TimeSpan.FromSeconds(40 + (2 * this.Caster.Skills.SpiritSpeak.Value));

                var ele = new FireElemental();
                ele.VirtualArmor = 0;
                ele.DamageMax = 5;
                ele.DamageMin = 1;

                var custoSummon = 4;
                if (m_Caster.Skills.SpiritSpeak.Value >= 100)
                    custoSummon = 2;
                ele.ControlSlots = custoSummon;

                ele.Elemento = Items.ElementoPvM.Fogo;
                SpellHelper.Summon(ele, this.Caster, 0x217, duration, true, true);
     
            }

            this.FinishSequence();
        }
    }
}
