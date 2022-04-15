using System;
using Server.Items;

namespace Server.Mobiles
{

    [CorpseName("an ogre corpse")]
    public class BaseOgre : BaseCreature
    {
        public override int BonusExp => 15;

        [Constructable]
        public BaseOgre(AIType type, FightMode mode, int n1, int n2, double n3, double n4)
            : base(type, mode, n1, n2, n3, n4)
        {
            this.Name = "ogro";
            this.PackItem(Loot.RandomFood());
            PackItem(new Arrow(Utility.RandomMinMax(10, 20)));
           
            SetSkill(SkillName.Parry, 65);
        }

        private bool superhit = false;

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            base.OnDamage(amount, from, willKill);

            if (this.BardPacified)
                return;

            if(!willKill)
            {
                if(Combatant != null)
                {
                    if (!IsCooldown("superhit"))
                    {
                        SetCooldown("superhit", TimeSpan.FromSeconds(25));
                        PublicOverheadMessage(Network.MessageType.Emote, 32, false, "* comecando a ficar nervoso *");
                        var pls = this.GetPlayersInRange(Map, 6);
                        foreach (var e in pls)
                        {
                            e.SendMessage(32, "O ogro parece estar comecando a ficar muito nervoso, melhor sair de perto...");
                        }
                        new HitTimer(this).Start();
                    } else
                    {
                        if(Utility.Random(30)==1)
                        {
                            PublicOverheadMessage(Network.MessageType.Emote, 0, false, "Voce paga ogro, voce morto !!");
                        
                        }
                    }
                }
              
            }
        }

        public class HitTimer : Timer
        {
            private BaseOgre mob;
            private int ct = 0;

            int dMin = 0;
            int dMax = 0;
            int hue = 0;

            public HitTimer(BaseOgre defender)
                : base(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(8), 2)
            {
                mob = defender;
            }

            protected override void OnTick()
            {
                if(ct == 0)
                {
                    mob.superhit = true;
                    dMin = mob.DamageMin;
                    dMax = mob.DamageMax;
                    hue = mob.Hue;
                    mob.DamageMin = Math.Min(40, mob.DamageMin * 3);
                    mob.DamageMax = Math.Min(70, mob.DamageMax * 3);
                    mob.SetSkill(SkillName.Wrestling, 300);
                    mob.PublicOverheadMessage(Network.MessageType.Emote, 32, false, "* fica enfurecido *");
                  
                } else
                {
                    if(mob.superhit)
                    {
        
                        mob.SetSkill(SkillName.Wrestling, 80);
                        mob.superhit = false;
                        mob.SetDamage(dMin, dMax);
                        mob.PublicOverheadMessage(Network.MessageType.Emote, 0, false, "* se acalmou *");
                    }
                }
                ct++;
            }
        }

        public BaseOgre(Serial serial)
            : base(serial)
        {
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
                return 2;
            }
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV3);
            this.AddLoot(LootPack.Potions);
            AddPackedLoot(LootPack.AverageProvisions, typeof(Bag));
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

    [CorpseName("an ogre corpse")]
    public class Ogre : BaseOgre
    {
        [Constructable]
        public Ogre()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "ogro";
            this.Body = 1;
            this.BaseSoundID = 427;

            this.SetStr(166, 195);
            this.SetDex(90, 120);
            this.SetInt(46, 70);

            this.SetHits(100, 150);
            this.SetMana(0);

            this.SetDamage(9, 18);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 30, 35);
            this.SetResistance(ResistanceType.Fire, 15, 25);
            this.SetResistance(ResistanceType.Cold, 15, 25);
            this.SetResistance(ResistanceType.Poison, 15, 25);
            this.SetResistance(ResistanceType.Energy, 25);

            this.SetSkill(SkillName.MagicResist, 55.1, 70.0);
            this.SetSkill(SkillName.Tactics, 60.1, 70.0);
            this.SetSkill(SkillName.Wrestling, 70.1, 80.0);

            this.Fame = 3000;
            this.Karma = -3000;

            this.VirtualArmor = 32;

            this.PackItem(new Club());
            this.PackItem(Loot.RandomFood());
        }

        public Ogre(Serial serial)
            : base(serial)
        {
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
                return 2;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV3);
            this.AddLoot(LootPack.Potions);
            AddPackedLoot(LootPack.MeagerProvisions, typeof(Bag));
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
