using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a rotting corpse")]
    public class Zombie : BaseCreature
    {
        public override int BonusExp => 2;

        [Constructable]
        public Zombie()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "zumbi";
            Body = 3;
            BaseSoundID = 471;

            SetStr(30, 70);
            SetDex(31, 50);
            SetInt(26, 40);

            SetHits(25, 40);

            SetDamage(3, 7);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 15, 20);
            SetResistance(ResistanceType.Cold, 20, 30);
            SetResistance(ResistanceType.Poison, 5, 10);

            SetSkill(SkillName.Tactics, 35.1, 50.0);
            SetSkill(SkillName.Wrestling, 35.1, 45.0);

            Fame = 600;
            Karma = -600;

            VirtualArmor = 0;

            PackBodyPartOrBones();
           
            PackItem(new Bloodmoss(5));

            if (Utility.Random(0, 100) < 25 && Utility.Chance(3))
            {
                var reag = Utility.Random(1, 3);
                switch (reag)
                {
                    case 1: PackItem(new MandrakeSeeds(1)); break;
                    case 2: PackItem(new NightshadeSeeds(1)); break;
                    case 3: PackItem(new GarlicSeeds(1)); break;
                }
            }
            
        }

        public override int BloodHue { get { return 862; } set { } }

        public Zombie(Serial serial)
            : base(serial)
        {
        }

        public override void OnThink()
        {
            base.OnThink();
            //Console.WriteLine("TICK " + this.Aggressors == null);
            if (this.Aggressors != null)
            {
                if (!IsCooldown("acido") && Utility.Random(10) == 1)
                {
                    if (this.Combatant is PlayerMobile)
                    {
                        var player = (PlayerMobile)this.Combatant;
                        if (player.GetDistanceToSqrt(this.Location) >= 2)
                        {
                            return;
                        }
                        SetCooldown("acido", TimeSpan.FromSeconds(10));
                        this.SpillAcid(2, power:8, name:"Vomito de zumbi");
                        PublicOverheadMessage(Network.MessageType.Emote, 0, false, "* vomita restos podres *");
                    }


                }
            }
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
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
        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV1);
        }
        
        public override bool IsEnemy(Mobile m)
        {
            if(Region.IsPartOf("Haven Island"))
            {
                return false;
            }
            
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
}
