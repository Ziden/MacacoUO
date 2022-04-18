using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class PowerScrollNovo : PowerScroll
    {
        private static readonly SkillName[] m_Skills = new SkillName[]
        {
            SkillName.Blacksmith,
            SkillName.Tailoring,
            SkillName.Swords,
            SkillName.Fencing,
            SkillName.Macing,
            SkillName.Archery,
            SkillName.Wrestling,
            SkillName.Parry,
            SkillName.Tactics,
            SkillName.Anatomy,
            SkillName.Healing,
            SkillName.Magery,
            SkillName.Meditation,
            SkillName.EvalInt,
            SkillName.MagicResist,
            SkillName.AnimalTaming,
            SkillName.AnimalLore,
            SkillName.Veterinary,
            SkillName.Musicianship,
            SkillName.Provocation,
            SkillName.Discordance,
            SkillName.Peacemaking
        };
        private static readonly SkillName[] m_AOSSkills = new SkillName[]
        {
            SkillName.Chivalry,
            SkillName.Focus,
            SkillName.Necromancy,
            SkillName.Stealing,
            SkillName.Stealth,
            SkillName.SpiritSpeak
        };
        private static readonly SkillName[] m_SESkills = new SkillName[]
        {
            SkillName.Ninjitsu,
            SkillName.Bushido
        };
        private static readonly SkillName[] m_MLSkills = new SkillName[]
        {
            SkillName.Spellweaving
        };

        private static SkillName[] m_SASkills = new SkillName[]
        {
        SkillName.Throwing,
        SkillName.Mysticism,
        SkillName.Imbuing
        };

        private static readonly List<SkillName> _Skills = new List<SkillName>();
        public PowerScrollNovo()
            : this(SkillName.Alchemy, 0.0)
        {
        }

        public override bool WillStack(Mobile from, Item item)
        {
            var stack = base.WillStack(from, item);
            if(stack)
            {
                if(item is PowerScrollNovo)
                {
                    var ps = item as PowerScrollNovo;
                    if (ps.Skill != this.Skill || ps.getGrupoMax() != getGrupoMax())
                        return false;
                }
            }
            return stack;
        }

        public override void OnAfterDuped(Item newItem)
        {
            base.OnAfterDuped(newItem);
            var i = newItem as PowerScrollNovo;
            i.Skill = this.Skill;
            i.Value = this.Value;
        }

        [Constructable]
        public PowerScrollNovo(SkillName skill, double value)
            : base(skill, value)
        {
            this.ItemID = 3827;
            //this.Hue = 0x481;
            this.Stackable = true;
            var grupo = getGrupoMax();
            Name = $"Pergaminho de +{this.Skill.ToString()} - {grupo - 5}-{grupo}";
            if (this.Value == 105.0 || skill == Server.SkillName.Blacksmith || skill == Server.SkillName.Tailoring)
                this.LootType = LootType.Regular;
            this.Value = getGrupoMax();
        }
 
        public PowerScrollNovo(Serial serial)
            : base(serial)
        {
        }
    
        public override string MessageStr
        {
            get
            {
                return "Usar o pergaminho aumenta o cap de uma skill especifica. Voce ainda vai precisar upar a skills";
            }
        }
        /*
        * Using a scroll increases the maximum amount of a specific skill or your maximum statistics.
        * When used, the effect is not immediately seen without a gain of points with that skill or statistics.
        * You can view your maximum skill values in your skills window.
        * You can view your maximum statistic value in your statistics window.
        */
        public override int Title
        {
            get
            {
                return 0;
            }
        }
        public override string DefaultTitle
        {
            get
            {
                return String.Format($"<basefont color=#77EE44>{Amount} Pergaminho do Conhecimento</basefont>");
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            list.Add(DefaultTitle);
            var grupo = getGrupoMax();
            list.Add($"+1 Cap {this.Skill.ToString()} - {grupo - 5}-{grupo}");
        }

        public int getGrupoMax()
        {
            if(this.Value >= 100 && this.Value <= 105)
            {
                return 105;
            }
            else if (this.Value >= 105 && this.Value <= 110)
            {
                return 110;
            }
            else if (this.Value >= 110 && this.Value <= 115)
            {
                return 115;
            }
            else if (this.Value >= 115 && this.Value <= 120)
            {
                return 120;
            }
            return 120;
        }
   
        public override bool CanUse(Mobile from)
        {
            Skill skill = from.Skills[this.Skill];

            if (skill == null)
                return false;

            var grupo = getGrupoMax();
            if (skill.Value < grupo - 5)
            {
                from.SendLocalizedMessage($"Este pergaminho eh muito poderoso para voce, use antes pergaminhos mais fracos. Voce precisa de pelo menos {grupo - 5} na skill para usar isto."); // Your ~1_type~ is too high for this power scroll.
                return false;
            }

            if (skill.Cap >= grupo)
            {
                from.SendLocalizedMessage("Sua habilidade e muito alta para este pergaminho"); // Your ~1_type~ is too high for this power scroll.
                return false;
            }

            return true;
        }

        public override void Use(Mobile from)
        {
            if (!this.CanUse(from))
                return;

            from.SendLocalizedMessage("Voce sente uma energia magica aumentando seu potencial em " + this.GetName()); // You feel a surge of magic as the scroll enhances your ~1_type~!

            from.Skills[this.Skill].Cap += 1;
            from.SendMessage("Cap Atual: " + from.Skills[this.Skill].Cap);
            Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
            Effects.PlaySound(from.Location, from.Map, 0x243);

            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

            Effects.SendTargetParticles(from, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);

            this.Consume(1);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = (this.InheritsItem ? 0 : reader.ReadInt()); // Required for SpecialScroll insertion

            if (this.Value == 105.0 || this.Skill == SkillName.Blacksmith || this.Skill == SkillName.Tailoring)
            {
                this.LootType = LootType.Regular;
            }
            else
            {
                this.LootType = LootType.Cursed;
                this.Insured = false;
            }

            this.Value = getGrupoMax();
            this.ItemID = 3827;
        }
    }
}
