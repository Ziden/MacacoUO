using System;
using Server.Items;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.First;
using Server.Spells.Fourth;
using Server.Spells.Third;

namespace Server.Mobiles
{
    [CorpseName("a ghostly corpse")]
    public class Shade : BaseCreature
    {
        public override int BonusExp => 1;

        public override Spell ChooseSpell()
        {
            if(this.Combatant is Mobile && !WeakenSpell.IsUnderEffects(this.Combatant as Mobile))
            {
                return new WeakenSpell(this, null);
            }
            var l = Utility.Random(0, 20);
            if(l < 18)
            {
                return new MagicArrowSpell(this, null);
            }
            else if (l < 19)
            {
                return new LightningSpell(this, null);
            }
            else
            {
                return new FireballSpell(this, null);
            }
        }

        [Constructable]
        public Shade()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "sombra";
            this.Body = 26;
            this.Hue = 0x4001;
            this.BaseSoundID = 0x482;

            this.SetStr(76, 100);
            this.SetDex(76, 95);
            this.SetInt(35, 40);

            this.SetHits(15, 30);

            this.SetDamage(1, 3);

            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Cold, 50);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Cold, 15, 25);
            this.SetResistance(ResistanceType.Poison, 10, 20);

            this.SetSkill(SkillName.EvalInt, 35.1, 70.0);
            this.SetSkill(SkillName.Magery, 30.1, 50.0);
            this.SetSkill(SkillName.MagicResist, 55.1, 70.0);
            this.SetSkill(SkillName.Tactics, 45.1, 60.0);
            this.SetSkill(SkillName.Wrestling, 45.1, 55.0);

            this.Fame = 4000;
            this.Karma = -4000;

            this.VirtualArmor = 28;

            this.PackReg(10);
        }



        /*
        public override void OnStartCombat(Mobile m)
        {
            base.OnStartCombat(m);
            if (m != FocusMob && m != null && m is PlayerMobile)
            {
                var player = (PlayerMobile)m;
                if (!player.IsCooldown("specfreeze"))
                {
                    player.SetCooldown("specfreeze", TimeSpan.FromSeconds(40));
                    PublicOverheadMessage(Network.MessageType.Regular, 0, false, "* preparando um olhar petrificante *");
                    new Wraith.FreezeTimer(this, m).Start();
                }
            }
        }
        */

        public Shade(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }

        public override TribeType Tribe { get { return TribeType.MortoVivo; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV2);
        }

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

    public class GreaterShade : BaseCreature
    {
        public override Spell ChooseSpell()
        {
            var alvo = Combatant as Mobile;
            if (alvo!=null)
            {
                if(!CurseSpell.UnderEffect(alvo))
                    return new CurseSpell(this, null);
                if (!alvo.Paralyzed)
                    return new ParalyzeSpell(this, null);
            }
            return new MindBlastSpell(this, null);
        }

        [Constructable]
        public GreaterShade()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "sombra intelectual";
            this.Body = 26;
            this.Hue = 0x4001;
            this.BaseSoundID = 0x482;

            this.SetStr(76, 100);
            this.SetDex(76, 95);
            this.SetInt(140, 140);

            this.SetHits(250, 350);

            this.SetDamage(1, 35);

            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Cold, 50);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Cold, 15, 25);
            this.SetResistance(ResistanceType.Poison, 10, 20);

            this.SetSkill(SkillName.EvalInt, 120, 120);
            this.SetSkill(SkillName.Magery, 120, 120);
            this.SetSkill(SkillName.MagicResist, 55.1, 70.0);
            this.SetSkill(SkillName.Tactics, 45.1, 60.0);
            this.SetSkill(SkillName.Wrestling, 45.1, 55.0);

            this.Fame = 10000;
            this.Karma = -10000;

            this.VirtualArmor = 28;

            this.PackReg(10);
        }

        public GreaterShade(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }

        public override TribeType Tribe { get { return TribeType.MortoVivo; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV5);
        }

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
