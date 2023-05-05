using System;
using Server.Fronteira.Talentos;
using Server.Items;
using Server.Mobiles;

namespace Server.SkillHandlers
{
    public class Stealth
    {
        private static readonly int[,] m_ArmorTable = new int[,]
        {
            //	Gorget	Gloves	Helmet	Arms	Legs	Chest	Shield
            /* Cloth	*/ { 0, 0, 0, 0, 0, 0, 0 },
            /* Leather	*/ { 0, 0, 0, 0, 0, 0, 0 },
            /* Studded	*/ { 2, 2, 0, 4, 6, 10, 0 },
            /* Bone		*/ { 0, 5, 10, 10, 15, 25, 0 },
            /* Spined	*/ { 0, 0, 0, 0, 0, 0, 0 },
            /* Horned	*/ { 0, 0, 0, 0, 0, 0, 0 },
            /* Barbed	*/ { 0, 0, 0, 0, 0, 0, 0 },
            /* Ring		*/ { 0, 5, 0, 5, 8, 8, 0 },
            /* Chain	*/ { 0, 0, 10, 0, 15, 25, 0 },
            /* Plate	*/ { 5, 5, 10, 10, 15, 25, 0 },
            /* Dragon	*/ { 0, 5, 10, 10, 15, 25, 0 }
        };
        public static double HidingRequirement
        {
            get
            {
                return 70;
            }
        }

        public static int[,] ArmorTable
        {
            get
            {
                return m_ArmorTable;
            }
        }

        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Stealth].Callback = new SkillUseCallback(OnUse);
        }

        public static int GetArmorRating(Mobile m)
        {
            int ar = 0;

            for (int i = 0; i < m.Items.Count; i++)
            {
                BaseArmor armor = m.Items[i] as BaseArmor;

                if (armor == null)
                    continue;

                int materialType = (int)armor.MaterialType;
                int bodyPosition = (int)armor.BodyPosition;

                if (materialType == (int)ArmorMaterialType.Leather || materialType == (int)ArmorMaterialType.Studded)
                    continue;

                if (materialType >= m_ArmorTable.GetLength(0) || bodyPosition >= m_ArmorTable.GetLength(1))
                    continue;

                if (armor.ArmorAttributes.MageArmor == 0)
                    ar += m_ArmorTable[materialType, bodyPosition];

            }
            return ar;
        }

        public static ArmorMaterialType ArmorMaterial(Mobile m)
        {
            var ar = ArmorMaterialType.Cloth;
            foreach(var i in m.GetEquipment())
            {
                if(i is BaseArmor)
                {
                    var art = ((BaseArmor)i).MaterialType;
                    if (art > ar)
                        ar = art;
                }
            }
            return ar;
        }

        public static TimeSpan OnUse(Mobile m)
        {
            if (!m.Hidden && (!m.RP || m is PlayerMobile && (((PlayerMobile)m).Talentos.Tem(Talento.Gatuno))))
            {
                Hiding.OnUse(m);
            }

            if (!m.Hidden)
            {
                m.SendMessage("Voce precisa se esconder primeiro"); // You must hide first
                return TimeSpan.Zero;
            }
            else if (m.Flying)
            {
                m.SendMessage("Nao pode estar voando"); // You cannot use this ability while flying.
                m.RevealingAction();
                BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
            }
            else if (m.Skills[SkillName.Hiding].Base < HidingRequirement)
            {
                m.SendMessage("Voce nao tem Hiding suficiente (70)"); // You are not hidden well enough.  Become better at hiding.
                m.RevealingAction();
                BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
            }
            else if (!m.CanBeginAction(typeof(Stealth)))
            {
                m.SendMessage("Nao pode usar isto agora"); // You cannot use this skill right now.
                m.RevealingAction();
                BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
            }
            else
            {
                int armorRating = GetArmorRating(m);
                Shard.Debug("Armor Rating: " + armorRating);
                if (ArmorMaterial(m) > ArmorMaterialType.Ringmail) //I have a hunch '42' was chosen cause someone's a fan of DNA
                {
                    m.SendMessage("Voce nao consegue andar quieto com esse monte de armadura"); // You could not hope to move quietly wearing this much armor.
                    m.RevealingAction();
                    BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
                }
                else if (m.CheckSkillMult(SkillName.Stealth, -20.0 + (armorRating * 2), 100 + (armorRating * 2)))
                {
                    //int steps = (int)(m.Skills[SkillName.Stealth].Value / (Core.AOS ? 5.0 : 10.0));
                    //Limita a 120 de skill, para fins de calculo de passos.
                    int steps = (int)(Math.Min(m.Skills[SkillName.Stealth].Value, 120) / (Core.AOS ? 5.0 : 10.0));
                    if (m.Skills[SkillName.Stealth].Value > 100)
                        steps += 2;
                    if (m.Skills[SkillName.Stealth].Value > 110)
                        steps += 4;
                    if (m.Skills[SkillName.Stealth].Value >= 120)
                        steps += 4;
                    


                    if (steps < 1)
                        steps = 1;

                    m.AllowedStealthSteps = steps;

                    m.IsStealthing = true;

                    m.SendMessage("Voce esta andando sorrateiramente, pode dar {0} passos.", steps+1); // You begin to move quietly.

                    BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.HidingAndOrStealth, 1044107, 1075655));
                    return TimeSpan.FromSeconds(15.0);
                }
                else
                {
                    m.SendMessage("Voce falhou ao andar sorrateiramente"); // You fail in your attempt to move unnoticed.
                    m.RevealingAction();
                    BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
                }
            }

            return TimeSpan.FromSeconds(15.0);
        }
    }
}
