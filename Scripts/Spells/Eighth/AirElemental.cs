using System;
using Server.Mobiles;

namespace Server.Spells.Eighth
{
    public class AirElementalSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Air Elemental", "Kal Vas Xen Hur",
            269,
            9010,
            false,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        public AirElementalSpell(Mobile caster, Item scroll)
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



            var custoSummon = 2;
            if (m_Caster.Skills.AnimalLore.Value >= 100)
                custoSummon = 1;

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
                TimeSpan duration = TimeSpan.FromSeconds(20 + (2 * this.Caster.Skills.SpiritSpeak.Value));

                var ele = new AirElemental();

                var custoSummon = 4;
                if (m_Caster.Skills.SpiritSpeak.Value >= 100)
                    custoSummon = 2;
                ele.ControlSlots = custoSummon;

                SpellHelper.Summon(ele, this.Caster, 0x217, duration, true, true);
                ele.VirtualArmor = 0;
            }

            this.FinishSequence();
        }
    }
}
