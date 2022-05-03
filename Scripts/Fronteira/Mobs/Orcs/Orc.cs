using System;
using Server.Engines.Craft;
using Server.Items;
using Server.Misc;
using Server.Ziden;

namespace Server.Mobiles
{
    [CorpseName("an orcish corpse")]
    public class BaseOrc : BaseCreature
    {
        [Constructable]
        public BaseOrc(AIType type, FightMode mode, int n1, int n2, double n3, double n4)
            : base(type, mode, n1, n2, n3, n4)
        {
            this.Name = "orc";

            if (BaseCreature.BypassInit)
                return;

            Imp.Converte(this);

            // olha a gambiarra... eh pq o bixo ainda nao tem backpack aqui :S
            Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
            {
                if(this.Alive && !this.Deleted && this.Backpack != null)
                {
                    this.Backpack.DropItem(new Bandage(Utility.Random(1) + 2));

                    switch (Utility.Random(5))
                    {
                        case 0: this.Backpack.DropItem(new RecipeScroll((int)CookRecipesExp.Dough)); break;
                        case 1: this.Backpack.DropItem(new RecipeScroll((int)CookRecipesExp.BreadLoaf)); break;
                        case 2: this.Backpack.DropItem(new BreadLoaf()); break;
                    }
                    PackReg(2, 10);
                }
            });
        }

        public BaseOrc(Serial serial)
            : base(serial)
        {
        }

        public override bool OnBeforeDeath()
        {
            var manolos = this.GetLootingRights();
            foreach (var r in manolos)
            {
                if (r.m_HasRight && r.m_Mobile != null && r.m_Mobile is PlayerMobile)
                {
                    var p = (PlayerMobile)r.m_Mobile;
                    if (p.Wisp != null)
                    {
                        p.Wisp.MataOrc();
                    }
                }
            }
            return base.OnBeforeDeath();
        }

        public static int BANDS_TIME = 6;

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            TentaAtacarMaster(this, from);

            if (this.Backpack == null)
                return;

            var bands = this.Backpack.FindItemByType(typeof(Bandage));
            if (!IsCooldown("bands") && bands != null)
            {
                Shard.Debug("KILL?" + willKill + " - " + this.Hits + " < " + this.HitsMax);
                if (!willKill && this.Hits < this.HitsMax)
                {
                    SetCooldown("bands", TimeSpan.FromSeconds(BANDS_TIME + 12));
                    PublicOverheadMessage(Network.MessageType.Regular, 0, false, "* aplicando bandagens *");
                    bands.Consume(1);
                    this.Backpack.DropItem(new BandVeia());
                    new HealTimer(this);
                }
            }
            base.OnDamage(amount, from, willKill);
        }

        public static bool TentaAtacarMaster(BaseCreature monster, Mobile enemyDealingDamage)
        {
            if (Shard.DebugEnabled) Shard.Debug(monster.Name + " tentando atacar mestre de " + enemyDealingDamage);
            if (enemyDealingDamage == null)
                return false;

            //if (monster.Combatant == enemyDealingDamage)
            //    return false;

            if (enemyDealingDamage is BaseCreature)
            {
                var summon = (BaseCreature)enemyDealingDamage;
                var master = summon.GetMaster();
                if (master != null)
                {
                    if (monster.GetDistance(master.Location) < 20)
                    {
                        monster.Combatant = master;
                        monster.PlayAngerSound();
                        monster.OverheadMessage("!");
                        return true;
                    }
                }
            }
            return false;
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
                if (this.m_Defender == null || this.m_Defender.Deleted)
                    return;

                if (this.m_Defender.Poisoned || BleedAttack.IsBleeding(this.m_Defender) || MortalStrike.IsWounded(this.m_Defender))
                {
                    this.m_Defender.PublicOverheadMessage(Network.MessageType.Emote, 0, false, "* todo perdido *");
                    return;
                }
                var heal = this.m_Defender.HitsMax / 3 + Utility.Random(20);
                var rnd = Utility.Random(100, 200);
                if (heal > rnd)

                    heal = rnd;
                this.m_Defender.Heal(heal);
                this.m_Defender.PlaySound(0x57);
            }
        }

        public override InhumanSpeech SpeechType
        {
            get
            {
                return InhumanSpeech.Orc;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }


        public override TribeType Tribe { get { return TribeType.Orc; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.SavagesAndOrcs;
            }
        }

        public override bool IsEnemy(Mobile m)
        {
            if (m.Player && m.FindItemOnLayer(Layer.Helm) is OrcishKinMask)
                return false;

            return base.IsEnemy(m);
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


    [CorpseName("an orcish corpse")]
    public class Orc : BaseOrc
    {
        [Constructable]
        public Orc()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "orc";
            this.Body = 17;
            this.BaseSoundID = 0x45A;


            this.SetStr(96, 120);
            this.SetDex(81, 105);
            this.SetInt(36, 60);

            this.SetHits(65, 75);

            this.SetDamage(2, 9);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Fire, 20, 30);
            this.SetResistance(ResistanceType.Cold, 10, 20);
            this.SetResistance(ResistanceType.Poison, 10, 20);
            this.SetResistance(ResistanceType.Energy, 20, 30);

            this.SetSkill(SkillName.Healing, 100);
            this.SetSkill(SkillName.Anatomy, 50);
            this.SetSkill(SkillName.MagicResist, 20, 30);
            this.SetSkill(SkillName.Tactics, 55.1, 80.0);
            this.SetSkill(SkillName.Wrestling, 50.1, 70.0);

            this.Fame = 2500;
            this.Karma = -2500;

            this.VirtualArmor = 5;


            switch (Utility.Random(20))
            {
                case 0:
                    this.PackItem(new Scimitar());
                    break;
                case 1:
                    this.PackItem(new Katana());
                    break;
                case 2:
                    this.PackItem(new WarMace());
                    break;
                case 3:
                    this.PackItem(new WarHammer());
                    break;
                case 4:
                    this.PackItem(new Kryss());
                    break;
                case 5:
                    this.PackItem(new Pitchfork());
                    break;
            }

            this.PackItem(new ThighBoots());

            switch (Utility.Random(3))
            {
                case 0:
                    this.PackItem(new Ribs());
                    break;
                case 1:
                    this.PackItem(new Shaft());
                    break;
                case 2:
                    this.PackItem(new Candle());
                    break;
            }

            if (0.1 > Utility.RandomDouble())
                this.PackItem(new BolaBall());

            if (0.1 > Utility.RandomDouble())
                PackItem(new Yeast());
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            base.OnDamage(amount, from, willKill);
        }

        public Orc(Serial serial)
        : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.OldAverage);
            this.AddPackedLoot(LootPack.MeagerProvisions, typeof(Bag));
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

    [CorpseName("an orcish corpse")]
    public class OrcEstilista : BaseOrc
    {
        [Constructable]
        public OrcEstilista()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = NameList.RandomName("orc") + " o orc estilista";
            this.Body = 17;
            this.BaseSoundID = 0x45A;
            this.Hue = 66;

            this.SetStr(200, 250);
            this.SetDex(81, 105);
            this.SetInt(36, 60);

            this.SetHits(650, 800);

            this.SetDamage(8, 30);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Fire, 20, 30);
            this.SetResistance(ResistanceType.Cold, 10, 20);
            this.SetResistance(ResistanceType.Poison, 10, 20);
            this.SetResistance(ResistanceType.Energy, 20, 30);

            this.SetSkill(SkillName.MagicResist, 50.1, 75.0);
            this.SetSkill(SkillName.Tactics, 55.1, 80.0);
            this.SetSkill(SkillName.Wrestling, 50.1, 70.0);

            this.Fame = 9500;
            this.Karma = -9500;

            this.VirtualArmor = 28;

            switch (Utility.Random(20))
            {
                case 0:
                    this.PackItem(new Scimitar());
                    break;
                case 1:
                    this.PackItem(new Katana());
                    break;
                case 2:
                    this.PackItem(new WarMace());
                    break;
                case 3:
                    this.PackItem(new WarHammer());
                    break;
                case 4:
                    this.PackItem(new Kryss());
                    break;
                case 5:
                    this.PackItem(new Pitchfork());
                    break;
            }

            this.PackItem(new ThighBoots());

            switch (Utility.Random(3))
            {
                case 0:
                    this.PackItem(new Ribs());
                    break;
                case 1:
                    this.PackItem(new Shaft());
                    break;
                case 2:
                    this.PackItem(new Candle());
                    break;
            }

            var roupaRara = Loot.RandomClothing();
            roupaRara.Hue = Loot.RandomRareDye();
            if (roupaRara.Name != null)
            {
                roupaRara.Name = roupaRara.Name + " com estilo";
            }
            this.PackItem(roupaRara);

            for (var r = 0; r < 2; r++)
            {
                var ropa = Loot.RandomClothing();
                ropa.Hue = Utility.RandomBirdHue();
                this.PackItem(ropa);
            }



            if (0.2 > Utility.RandomDouble())
                this.PackItem(new BolaBall());

            if (0.5 > Utility.RandomDouble())
                PackItem(new Yeast());
        }

        public OrcEstilista(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get
            {
                return InhumanSpeech.Orc;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }

        public override TribeType Tribe { get { return TribeType.Orc; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.SavagesAndOrcs;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.OldRich);
        }

        public override bool IsEnemy(Mobile m)
        {
            if (m.Player && m.FindItemOnLayer(Layer.Helm) is OrcishKinMask)
                return false;

            return base.IsEnemy(m);
        }

        public override void AggressiveAction(Mobile aggressor, bool criminal)
        {
            base.AggressiveAction(aggressor, criminal);

            Item item = aggressor.FindItemOnLayer(Layer.Helm);

            if (item is OrcishKinMask)
            {
                AOS.Damage(aggressor, 50, 0, 100, 0, 0, 0);
                aggressor._PlaceInBackpack(item);
                aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                aggressor.PlaySound(0x307);
            }
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
