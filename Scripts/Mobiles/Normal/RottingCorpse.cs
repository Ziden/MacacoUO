using System;
using Server.Items;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("a rotting corpse")]
    public class RottingCorpse : BaseCreature
    {
        [Constructable]
        public RottingCorpse()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "corpo podre";
            Body = 155;
            BaseSoundID = 471;

            SetStr(200, 220);
            SetDex(75);
            SetInt(151, 200);

            SetHits(500);
            SetStam(75);
            SetMana(0);

            SetDamage(8, 10);

            SetDamageType(ResistanceType.Physical, 0);
            SetDamageType(ResistanceType.Cold, 50);
            SetDamageType(ResistanceType.Poison, 50);

            SetResistance(ResistanceType.Physical, 35, 45);
            SetResistance(ResistanceType.Fire, 20, 30);
            SetResistance(ResistanceType.Cold, 50, 70);
            SetResistance(ResistanceType.Poison, 40, 50);
            SetResistance(ResistanceType.Energy, 20, 30);

            SetSkill(SkillName.Poisoning, 120.0);
            SetSkill(SkillName.MagicResist, 150);
            SetSkill(SkillName.Tactics, 100.0);
            SetSkill(SkillName.Wrestling, 90.1, 100.0);

            Fame = 6000;
            Karma = -6000;

            VirtualArmor = 0;
        }

        public RottingCorpse(Serial serial)
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
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Greater;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            if (!(from is PlayerMobile))
                return;

            base.OnDamage(amount, from, willKill);
            if (from == null)
                return;

            if(!IsCooldown("acido"))
            {
                SetCooldown("acido", TimeSpan.FromSeconds(8));
              
                var loc1 = from.Location;
                var loc2 = from.Location;
                if (Utility.RandomBool())
                    loc1.X += 1;
                else
                    loc1.X -= 1;
                if (Utility.RandomBool())
                    loc1.Y += 1;
                else
                    loc1.Y -= 1;
                if (Utility.RandomBool())
                    loc2.X += 1;
                else
                    loc2.X -= 1;
                if (Utility.RandomBool())
                    loc2.Y += 1;
                else
                    loc2.Y -= 1;

                if (from == null || from.Map == null || from.Map == Map.Internal || !from.Alive)
                    return;

                loc1.Z = from.Map.GetAverageZ(loc1.X, loc1.Y);
                if (Math.Abs(loc1.Z - this.Location.Z) > 4)
                {
                    loc1.Z = this.Location.Z;
                }
                loc2.Z = from.Map.GetAverageZ(loc2.X, loc2.Y);
                if (Math.Abs(loc2.Z - this.Location.Z) > 4)
                {
                    loc2.Z = this.Location.Z;
                }

                if (from.Map.CanFit(loc1, 16))
                {
                    Item acid1 = NewAcido(49, "vomito de zumbi");
                    acid1.MoveToWorld(loc1, from.Map);
                    Effects.SendMovingEffect(this, acid1, acid1.ItemID, 15, 10, true, false, acid1.Hue, 0);
                }

                if (from.Map.CanFit(loc2, 16))
                {
                    Item acid1 = NewAcido(49, "vomito de zumbi");
                    acid1.MoveToWorld(loc2, from.Map);
                    Effects.SendMovingEffect(this, acid1, acid1.ItemID, 15, 10, true, false, acid1.Hue, 0);
                }

                OverheadMessage("* vomita restos podres *");

                this.SpillAcid(2, power: 20, name: "Vomito de zumbi");
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
            AddLoot(LootPack.OldFilthyRich);
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
