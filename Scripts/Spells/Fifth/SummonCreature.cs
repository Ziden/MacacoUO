using System;
using Server.Mobiles;

namespace Server.Spells.Fifth
{
    public class SummonCreatureSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Summon Creature", "Kal Xen",
            16,
            false,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        // NOTE: Creature list based on 1hr of summon/release on OSI.
        private static readonly Type[] m_Types = new Type[]
        {
            typeof(PolarBear),
            typeof(GrizzlyBear),
            typeof(BlackBear),
            typeof(Horse),
            typeof(Walrus),
            typeof(Chicken),
            typeof(Scorpion),
            typeof(GiantSerpent),
            typeof(Llama),
            typeof(Alligator),
            typeof(GreyWolf),
            typeof(Slime),
            typeof(Eagle),
            typeof(Gorilla),
            typeof(SnowLeopard),
            typeof(Hind),
        };

        private static readonly Type[] topTypes = new Type[]
       {
            typeof(BlackBear),
            typeof(GiantSerpent),
            typeof(SnowLeopard),
            typeof(GreatHart),
            typeof(Scorpion)
       };

        public SummonCreatureSpell(Mobile caster, Item scroll)
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
                try
                {
                    var type = m_Types[Utility.Random(m_Types.Length)];
                    var pl = this.Caster as PlayerMobile;

                    if (pl != null)
                    {
                        if (pl.Almas > 20)
                        {
                            type = topTypes[Utility.Random(topTypes.Length)];
                        }
                    }

                    BaseCreature creature = (BaseCreature)Activator.CreateInstance(type);

                    if (pl != null)
                    {
                        if (pl.Almas > 0)
                        {
                            creature.HitsMaxSeed = creature.HitsMaxSeed + (pl.Almas * 10);
                            creature.Hits = creature.HitsMax;
                        }
                        if (pl.Almas > 10)
                        {
                            creature.ControlSlots = 1;
                        }

                    }

                    if (m_Caster.Skills.AnimalLore.Value >= 100)
                    {
                        creature.HitsMaxSeed = creature.HitsMaxSeed + 100;
                        creature.Hits = creature.HitsMax;
                    }

                    TimeSpan duration;

                    if (Core.AOS)
                        duration = TimeSpan.FromSeconds((2 * this.Caster.Skills.SpiritSpeak.Fixed) / 5);
                    else
                        duration = TimeSpan.FromSeconds(15 + this.Caster.Skills[SkillName.SpiritSpeak].Value * 2);

                    SpellHelper.Summon(creature, this.Caster, 0x215, duration, true, true, true, SkillName.SpiritSpeak);
                }
                catch
                {
                }
            }

            this.FinishSequence();
        }

        public override TimeSpan GetCastDelay()
        {
            if (Core.AOS)
                return TimeSpan.FromTicks(base.GetCastDelay().Ticks * 5);

            return base.GetCastDelay();
        }
    }
}
