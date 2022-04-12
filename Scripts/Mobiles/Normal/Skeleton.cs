using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skeletal corpse")]
    public class Skeleton : BaseCreature
    {
        public override int BonusExp => 2;

        [Constructable]
        public Skeleton()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "esqueleto";
            this.Body = Utility.RandomList(50, 56);
            this.BaseSoundID = 0x48D;

            this.SetStr(40, 80);
            this.SetDex(56, 75);
            this.SetInt(16, 40);

            this.SetHits(25, 30);

            this.SetDamage(2, 7);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 15, 20);
            this.SetResistance(ResistanceType.Fire, 5, 10);
            this.SetResistance(ResistanceType.Cold, 25, 40);
            this.SetResistance(ResistanceType.Poison, 25, 35);
            this.SetResistance(ResistanceType.Energy, 5, 15);

            this.SetSkill(SkillName.MagicResist, 35.1, 60.0);
            this.SetSkill(SkillName.Tactics, 45.1, 60.0);
            this.SetSkill(SkillName.Wrestling, 45.1, 55.0);

            this.Fame = 450;
            this.Karma = -450;

            this.VirtualArmor = 0;

            PackItem(new Bone());

            switch ( Utility.Random(25))
            {
                case 0:
                    this.PackItem(new BoneArms());
                    break;
                case 1:
                    this.PackItem(new BoneChest());
                    break;
                case 2:
                    this.PackItem(new BoneGloves());
                    break;
                case 3:
                    this.PackItem(new BoneLegs());
                    break;
                case 4:
                    this.PackItem(new BoneHelm());
                    break;
            }
        }

        public Skeleton(Serial serial)
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

        public override bool HasBlood { get { return false; } }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            if(Utility.Random(0, 10)==1)
                Effects.ItemToFloor(this, new Bone(), Effects.TryGetNearRandomLoc(this), this.Map);
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lesser;
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

        public override void OnThink()
        {
            base.OnThink();
            if (this.Combatant != null)
            {
                if(!IsCooldown("bonethrow"))
                {
                    if(this.Combatant is PlayerMobile)
                    {
                        var player = (PlayerMobile)this.Combatant;
                        var dist = player.GetDistanceToSqrt(this.Location);
                        if (dist <= 3 || dist >= 9 || !this.InLOS(player))
                        {
                            return;
                        }
                        SetCooldown("bonethrow", TimeSpan.FromSeconds(10));
                        this.MovingParticles(player, 0xF7E, 9, 0, false, false, 9502, 4019, 0x160);
                        AOS.Damage(player, 1+Utility.Random(2), 0, 0, 0, 0, 0);
                        PublicOverheadMessage(Network.MessageType.Regular, 0, false, "* joga um osso *");
                    }
                   
                 
                }
            }
        }
        
        public override bool IsEnemy(Mobile m)
        {
            return base.IsEnemy(m);
        }
        
       public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV1);
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
