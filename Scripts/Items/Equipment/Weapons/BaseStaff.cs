using Server.Engines.Craft;
using System;

namespace Server.Items
{
    public abstract class BaseStaff : BaseMeleeWeapon
    {
        public BaseStaff(int itemID)
            : base(itemID)
        {
        }


        public BaseStaff(Serial serial)
            : base(serial)
        {
        }

        public override bool AllowEquipedCast(Mobile from)
        {
            return true;
        }

        public override int DefHitSound
        {
            get
            {
                return 0x13C;
            }
        }
        public override int DefMissSound
        {
            get
            {
                return 0x239;
            }
        }
        public override SkillName DefSkill
        {
            get
            {
                return SkillName.Macing;
            }
        }
        public override WeaponType DefType
        {
            get
            {
                return WeaponType.Staff;
            }
        }
        public override WeaponAnimation DefAnimation
        {
            get
            {
                return WeaponAnimation.Bash2H;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }

        public override void OnHit(Mobile attacker, IDamageable defender, double damageBonus)
        {
            base.OnHit(attacker, defender, damageBonus);

            //if(defender is Mobile)
            //    ((Mobile)defender).Stam -= Utility.Random(3, 3); // 3-5 points of stamina loss
        }
    }
}
