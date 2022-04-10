using System;
using System.Collections.Generic;
using System.Linq;

using Server.Targeting;
using Server.Mobiles;
using Server.Items;

namespace Server.Spells.Seventh
{
    public class ChainLightningSpell : MagerySpell
    {
        public override DamageType SpellDamageType { get { return DamageType.SpellAOE; } }

        private static readonly SpellInfo m_Info = new SpellInfo(
            "Chain Lightning", "Vas Ort Grav",
            209,
            9022,
            false,
            Reagent.BlackPearl,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);
        public ChainLightningSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Seventh;
            }
        }
        public override bool DelayedDamage
        {
            get
            {
                //return true;
                return false;
            }
        }
        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                if (p is Item)
                    p = ((Item)p).GetWorldLocation();

                var targets = AcquireIndirectTargets(p, 5, false).ToList();
                var count = Math.Max(1, targets.Count);

                if(targets.Count ==0)
                {
                    Caster.PlaySound(0x029);
                }

                foreach (var dam in targets)
                {
                    if (!Caster.InLOS(dam))
                        continue;

                    var id = dam;
                    var m = id as Mobile;
                    double damage;

                    if (Core.AOS)
                        damage = GetNewAosDamage(51, 1, 5, id is PlayerMobile, id);
                    else
                        damage = Utility.Random(27, 22);

                    //if (Core.AOS && count > 2)
                    //    damage = (damage * 2) / count;
                    //else if (!Core.AOS && count > 2)
                    //    damage /= (count/2);

                    var danoMax = dam.Hits * 0.8;
                    if (damage > danoMax)
                        damage = danoMax;

                    if (!Core.AOS && m != null && CheckResisted(m))
                    {
                        damage *= 0.75;

                        m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    Mobile source = Caster;
                    SpellHelper.CheckReflect((int)Circle, ref source, ref id, SpellDamageType);

                    if (m != null)
                    {
                        damage *= GetDamageScalar(m, ElementoPvM.Raio);
                    }

                    Effects.SendBoltEffect(id, true, 0, false);
                    id.PlaySound(0x029);
                    Caster.DoHarmful(id);
                    SpellHelper.Damage(this, id, damage, 0, 0, 0, 0, 100, Items.ElementoPvM.Raio);
                }

                ColUtility.Free(targets);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly ChainLightningSpell m_Owner;
            public InternalTarget(ChainLightningSpell owner)
                : base(Spell.RANGE, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                IPoint3D p = o as IPoint3D;

                if (p != null)
                    m_Owner.Target(p);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}
