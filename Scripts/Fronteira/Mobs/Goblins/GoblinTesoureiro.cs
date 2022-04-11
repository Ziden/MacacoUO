using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an goblin corpse")]
    public class GoblinTesoureiro : BaseCreature
    {
        [Constructable]
        public GoblinTesoureiro()
            : base(AIType.AI_Runner, FightMode.Closest, 10, 1, 0.1, 0.1)
        {
            Name = "goblin tesoureiro";
            Body = 723;
            Hue = 1900;
            BaseSoundID = 0x600;

            SetStr(297, 297);
            SetDex(80, 80);
            SetInt(118, 118);

            SetHits(200, 300);
            SetStam(80, 80);
            SetMana(118, 118);

            SetDamage(10, 20);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 47, 47);
            SetResistance(ResistanceType.Fire, 37, 37);
            SetResistance(ResistanceType.Cold, 29, 29);
            SetResistance(ResistanceType.Poison, 10, 11);
            SetResistance(ResistanceType.Energy, 19, 19);

            SetSkill(SkillName.MagicResist, 30.6, 40);
            SetSkill(SkillName.Tactics, 80.0, 82.8);
            SetSkill(SkillName.Anatomy, 82.0, 84.8);
            SetSkill(SkillName.Wrestling, 99.2, 100.7);

            Fame = 1500;
            Karma = -1500;

            VirtualArmor = 28;

            switch (Utility.Random(20))
            {
                case 0:
                    PackItem(new Scimitar());
                    break;
                case 1:
                    PackItem(new Katana());
                    break;
                case 2:
                    PackItem(new WarMace());
                    break;
                case 3:
                    PackItem(new WarHammer());
                    break;
                case 4:
                    PackItem(new Kryss());
                    break;
                case 5:
                    PackItem(new Pitchfork());
                    break;
            }

            PackItem(new ThighBoots());

            switch (Utility.Random(3))
            {
                case 0:
                    PackItem(new Ribs());
                    break;
                case 1:
                    PackItem(new Shaft());
                    break;
                case 2:
                    PackItem(new Candle());
                    break;
            }

            if (0.2 > Utility.RandomDouble())
                PackItem(new BolaBall());

            if (Utility.RandomDouble() < 0.3)
            {
                if (Utility.RandomDouble() < 0.2)
                {
                    var livro = new Spellbook();
                    livro.Slayer = SlayerName.Goblins;
                    AddItem(livro);
                }
                else
                {
                    var livro = Loot.RandomWeapon();
                    livro.Slayer = SlayerName.Goblins;
                    AddItem(livro);
                }
            }

        }

        public static int BANDS_TIME = 6;

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {

            if (!IsCooldown("fala"))
            {
                SetCooldown("fala", TimeSpan.FromSeconds(40));
                Say("Sai fora !!! Nao vai pegar nosso tesouro !!!");
            }

            if (!IsCooldown("bands"))
            {
                Shard.Debug("KILL?" + willKill + " - " + this.Hits + " < " + this.HitsMax);
                if (!willKill && this.Hits < this.HitsMax)
                {
                    SetCooldown("bands", TimeSpan.FromSeconds(BANDS_TIME + 10));
                    PublicOverheadMessage(Network.MessageType.Regular, 0, false, "* aplicando bandagens *");
                    new HealTimer(this);
                }
            }
            base.OnDamage(amount, from, willKill);
        }

        public class HealTimer : Timer
        {
            private BaseCreature m_Defender;


            public HealTimer(BaseCreature defender)
                : base(TimeSpan.FromSeconds(Utility.Random(BaseOrc.BANDS_TIME) + 4))
            {
                m_Defender = defender;
                Start();
            }

            protected override void OnTick()
            {
                if (this.m_Defender.Poisoned || BleedAttack.IsBleeding(this.m_Defender))
                {
                    this.m_Defender.PublicOverheadMessage(Network.MessageType.Emote, 0, false, "* todo perdido *");
                    return;
                }
                var heal = 60 + Utility.Random(20);
                this.m_Defender.Heal(heal);
                this.m_Defender.PlaySound(0x57);
            }
        }

        public GoblinTesoureiro(Serial serial)
            : base(serial)
        {
        }

        public override int GetAngerSound() { return 0x600; }
        public override int GetIdleSound() { return 0x600; }
        public override int GetAttackSound() { return 0x5FD; }
        public override int GetHurtSound() { return 0x5FF; }
        public override int GetDeathSound() { return 0x5FE; }

        public override bool CanRummageCorpses { get { return true; } }
        public override int TreasureMapLevel { get { return 1; } }
        public override int Meat { get { return 1; } }
        public override OppositionGroup OppositionGroup { get { return OppositionGroup.SavagesAndOrcs; } }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.OldRich);
            this.AddLoot(LootPack.Gems);
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

    public class GoblinLadrao : BaseCreature
    {
        [Constructable]
        public GoblinLadrao()
            : base(AIType.AI_Runner, FightMode.Closest, 10, 1, 0.05, 0.05)
        {
            Name = "goblin ladrao";
            Body = 723;
            Hue = 1900;
            BaseSoundID = 0x600;

            SetStr(297, 297);
            SetDex(80, 80);
            SetInt(118, 118);

            SetHits(200, 300);
            SetStam(80, 80);
            SetMana(118, 118);

            SetDamage(10, 20);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 47, 47);
            SetResistance(ResistanceType.Fire, 37, 37);
            SetResistance(ResistanceType.Cold, 29, 29);
            SetResistance(ResistanceType.Poison, 10, 11);
            SetResistance(ResistanceType.Energy, 19, 19);

            SetSkill(SkillName.MagicResist, 30.6, 40);
            SetSkill(SkillName.Tactics, 80.0, 82.8);
            SetSkill(SkillName.Anatomy, 82.0, 84.8);
            SetSkill(SkillName.Wrestling, 99.2, 100.7);

            Fame = 1500;
            Karma = -1500;

            VirtualArmor = 28;

            switch (Utility.Random(20))
            {
                case 0:
                    PackItem(new Scimitar());
                    break;
                case 1:
                    PackItem(new Katana());
                    break;
                case 2:
                    PackItem(new WarMace());
                    break;
                case 3:
                    PackItem(new WarHammer());
                    break;
                case 4:
                    PackItem(new Kryss());
                    break;
                case 5:
                    PackItem(new Pitchfork());
                    break;
            }

            PackItem(new ThighBoots());

            switch (Utility.Random(3))
            {
                case 0:
                    PackItem(new Ribs());
                    break;
                case 1:
                    PackItem(new Shaft());
                    break;
                case 2:
                    PackItem(new Candle());
                    break;
            }

            if (0.2 > Utility.RandomDouble())
                PackItem(new BolaBall());

            if (Utility.RandomDouble() < 0.3)
            {
                if (Utility.RandomDouble() < 0.2)
                {
                    var livro = new Spellbook();
                    livro.Slayer = SlayerName.Goblins;
                    AddItem(livro);
                }
                else
                {
                    var livro = Loot.RandomWeapon();
                    livro.Slayer = SlayerName.Goblins;
                    AddItem(livro);
                }
            }

        }

        public static int BANDS_TIME = 6;

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {

            if (!IsCooldown("bands"))
            {

                if (!willKill && this.Hits < this.HitsMax)
                {
                    SetCooldown("bands", TimeSpan.FromSeconds(BANDS_TIME + 10));
                    PublicOverheadMessage(Network.MessageType.Regular, 0, false, "* aplicando bandagens *");
                    new HealTimer(this);
                }
            }
            base.OnDamage(amount, from, willKill);
        }

        public class HealTimer : Timer
        {
            private BaseCreature m_Defender;


            public HealTimer(BaseCreature defender)
                : base(TimeSpan.FromSeconds(Utility.Random(BaseOrc.BANDS_TIME) + 4))
            {
                m_Defender = defender;
                Start();
            }

            protected override void OnTick()
            {
                if (this.m_Defender.Poisoned || BleedAttack.IsBleeding(this.m_Defender))
                {
                    this.m_Defender.PublicOverheadMessage(Network.MessageType.Emote, 0, false, "* todo perdido *");
                    return;
                }
                var heal = 60 + Utility.Random(20);
                this.m_Defender.Heal(heal);
                this.m_Defender.PlaySound(0x57);
            }
        }

        public GoblinLadrao(Serial serial)
            : base(serial)
        {
        }

        public override int GetAngerSound() { return 0x600; }
        public override int GetIdleSound() { return 0x600; }
        public override int GetAttackSound() { return 0x5FD; }
        public override int GetHurtSound() { return 0x5FF; }
        public override int GetDeathSound() { return 0x5FE; }

        public override bool CanRummageCorpses { get { return true; } }
        public override int TreasureMapLevel { get { return 1; } }
        public override int Meat { get { return 1; } }
        public override OppositionGroup OppositionGroup { get { return OppositionGroup.SavagesAndOrcs; } }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.OldRich);
            this.AddLoot(LootPack.Gems);
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

    public class CoelhoDaPascoa : BaseCreature
    {
        public override int BonusExp => 500;

        public override bool Murderer => false;

        [Constructable]
        public CoelhoDaPascoa()
            : base(AIType.AI_Runner, FightMode.Closest, 10, 1, 0.05, 0.05)
        {
            Name = "coelinho da pascoa";
            Body = 205;
            Hue = 3 + (Utility.Random(20) * 5);

            SetStr(297, 297);
            SetDex(80, 80);
            SetInt(118, 118);

            SetHits(200, 200);
            SetStam(80, 80);
            SetMana(118, 118);

            SetDamage(1, 3);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 1, 1);
            SetResistance(ResistanceType.Fire, 1, 1);
            SetResistance(ResistanceType.Cold, 1, 1);
            SetResistance(ResistanceType.Poison, 1, 1);
            SetResistance(ResistanceType.Energy, 1, 1);

            SetSkill(SkillName.MagicResist, 30.6, 40);
            SetSkill(SkillName.Tactics, 80.0, 82.8);
            SetSkill(SkillName.Anatomy, 82.0, 84.8);
            SetSkill(SkillName.Wrestling, 99.2, 100.7);

            Fame = 1500;
            Karma = -1500;

            VirtualArmor = 28;
            Tamable = false;
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            var b = new DannysGiftBox();
            b.MoveToWorld(c.Location, c.Map);
            Effects.SendLocationParticles(EffectItem.Create(c.Location, c.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
            c.Delete();
            DistribuiItem(new Gold(1000));
        }

        public CoelhoDaPascoa(Serial serial)
            : base(serial)
        {
        }

        public override int GetAngerSound() { return 0x600; }
        public override int GetIdleSound() { return 0x600; }
        public override int GetAttackSound() { return 0x5FD; }
        public override int GetHurtSound() { return 0x5FF; }
        public override int GetDeathSound() { return 0x5FE; }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Gems);
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
