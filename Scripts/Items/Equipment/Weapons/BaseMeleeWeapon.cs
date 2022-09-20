using System;
using Server.Spells.Spellweaving;
using Server.Mobiles;

namespace Server.Items
{
    public abstract class BaseMeleeWeapon : BaseWeapon
    {
        public BaseMeleeWeapon(int itemID)
            : base(itemID)
        {
        }

        public BaseMeleeWeapon(Serial serial)
            : base(serial)
        {
        }

        public override Tuple<int, bool> AbsorbDamage(Mobile attacker, Mobile defender, int damage)
        {
            var mods = base.AbsorbDamage(attacker, defender, damage);

            damage = mods.Item1;

            if (Core.AOS)
                return mods;

            int absorb = defender.MeleeDamageAbsorb;

            if (absorb > 0)
            {
                if (absorb > damage)
                {
                    int react = damage / 5;

                    if (react <= 0)
                        react = 1;

                    if(Shard.DebugEnabled)
                        Shard.Debug("Abs damage " + damage);

                    defender.MeleeDamageAbsorb -= damage;
                    damage = 0;

                    attacker.Damage(react, defender);

                    attacker.PlaySound(0x1F1);
                    attacker.FixedEffect(0x374A, 10, 16);

                    if (attacker is PlayerMobile && defender is PlayerMobile)
                    {
                        defender.MeleeDamageAbsorb = 0;
                        defender.SendLocalizedMessage("Armadura reativa resiste apenas a um golpe outros jogadores");
                        BuffInfo.RemoveBuff(defender, BuffIcon.ReactiveArmor);
                        DefensiveSpell.Nullify(defender);
                    }

                }
                else
                {
                    defender.MeleeDamageAbsorb = 0;
                    defender.SendLocalizedMessage("Sua armadura reativa acabou"); // Your reactive armor spell has been nullified.
                    BuffInfo.RemoveBuff(defender, BuffIcon.ReactiveArmor);
                    DefensiveSpell.Nullify(defender);
                }
            }


            return new Tuple<int, bool>(damage, mods.Item2);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }
}
