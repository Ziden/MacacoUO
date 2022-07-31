using System;
using Server.Engines.CannedEvil;
using Server.Fronteira.Elementos;
using Server.Items;
using Server.Spells.Fifth;
using Server.Spells.Seventh;

namespace Server.Mobiles
{
    [CorpseName("a Lucifer corpse")]
    public class Lucifer : BaseCreature
    {
        [Constructable]
        public Lucifer()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Lucifer";
            Body = 1248;
            BaseSoundID = 357;

            SetStr(9860, 11850);
            SetDex(1770, 2550);
            SetInt(1510, 2500);

            SetHits(59200, 71100);

            SetDamage(220, 290);

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Fire, 25);
            SetDamageType(ResistanceType.Energy, 25);

            SetResistance(ResistanceType.Physical, 65, 80);
            SetResistance(ResistanceType.Fire, 60, 80);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.Anatomy, 100.1, 200.0);
            SetSkill(SkillName.EvalInt,100.1, 200.0);
            SetSkill(SkillName.Magery, 100.5, 200.0);
            SetSkill(SkillName.Meditation, 25.1, 50.0);
            SetSkill(SkillName.MagicResist, 100.5, 150.0);
            SetSkill(SkillName.Tactics, 90.1, 100.0);
            SetSkill(SkillName.Wrestling, 90.1, 100.0);

            Fame = 24000;
            Karma = -24000;

            VirtualArmor = 90;

            PackItem(new Longsword());
        }

        public Lucifer(Serial serial)
            : base(serial)
        {
        }

        public override bool CanFly
        {
            get { return true; }
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override double HitPoisonChance
        {
            get
            {
                return 0.8;
            }
        }
        private DateTime m_GazeDelay;
        private DateTime m_StoneDelay;
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV7, 1);
            AddLoot(LootPack.Gems, 40);
        }

        public void Polymorph(Mobile m)
        {
            if (!m.CanBeginAction(typeof(PolymorphSpell)) || !m.CanBeginAction(typeof(IncognitoSpell)) || m.IsBodyMod)
                return;

            IMount mount = m.Mount;

            if (mount != null)
                mount.Rider = null;

            if (m.Flying)
                m.ToggleFlying();

            if (m.Mounted)
                return;

            if (m.BeginAction(typeof(PolymorphSpell)))
            {
                m.BodyMod = 1248;
                m.HueMod = 0;
                if (m == this)
                {
                    m_SlayerVulnerabilities.Add("Vermin");
                    m_SlayerVulnerabilities.Add("Repond");
                }

                new ExpirePolymorphTimer(m).Start();
            }
        }

        public virtual int BonusExp => 1800;

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
