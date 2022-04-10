using Server.Items;
using System;

namespace Server.Mobiles
{
    [CorpseName("a terathan warrior corpse")]
    public class TerathanWarrior : BaseCreature
    {
        public override int BonusExp => 100;

        [Constructable]
        public TerathanWarrior()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "terathan guerreiro";
            this.Body = 70;
            this.BaseSoundID = 589;

            this.SetStr(166, 215);
            this.SetDex(96, 145);
            this.SetInt(41, 65);

            this.SetHits(2100, 2129);
            this.SetMana(0);

            this.SetDamage(37, 57);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 30, 35);
            this.SetResistance(ResistanceType.Fire, 20, 30);
            this.SetResistance(ResistanceType.Cold, 25, 35);
            this.SetResistance(ResistanceType.Poison, 30, 40);
            this.SetResistance(ResistanceType.Energy, 25, 35);

            this.SetSkill(SkillName.Poisoning, 60.1, 80.0);
            this.SetSkill(SkillName.MagicResist, 60.1, 75.0);
            this.SetSkill(SkillName.Tactics, 80.1, 100.0);
            this.SetSkill(SkillName.Wrestling, 100, 100);

            this.Fame = 34000;
            this.Karma = -34000;

            this.VirtualArmor = 30;

            if (Utility.RandomDouble() < 0.33)
                PackItem(new CristalTherathan());

            if (Utility.RandomDouble() < .33)
                this.PackItem(Engines.Plants.Seed.RandomPeculiarSeed(4));
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        { 
            base.OnDamage(amount, from, willKill);
            BaseOrc.TentaAtacarMaster(this, from);
        }

        public override void OnThink()
        {
            var from = Combatant;
            if (!(from is PlayerMobile))
                return;

            if (from == null)
                return;

            if (!IsCooldown("acido"))
            {
                SetCooldown("acido", TimeSpan.FromSeconds(10));

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
                    Item acid1 = NewAcido(49, "acido terathan");
                    acid1.MoveToWorld(loc1, from.Map);
                    Effects.SendMovingEffect(this, acid1, acid1.ItemID, 15, 10, true, false, acid1.Hue, 0);
                }

                if (from.Map.CanFit(loc2, 16))
                {
                    Item acid1 = NewAcido(49, "acido terathan");
                    acid1.MoveToWorld(loc2, from.Map);
                    Effects.SendMovingEffect(this, acid1, acid1.ItemID, 15, 10, true, false, acid1.Hue, 0);
                }

                OverheadMessage("* jorra acido *");

                this.SpillAcid(2, power: 20, name: "acido");
            }
        }

        public TerathanWarrior(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }

        public override void AlterMeleeDamageTo(Mobile to, ref int damage)
        {
            base.AlterMeleeDamageTo(to, ref damage);
            if (to is BaseCreature)
                damage *= 5;
        }

        public override int Meat
        {
            get
            {
                return 4;
            }
        }

        public override TribeType Tribe { get { return TribeType.Terathan; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.TerathansAndOphidians;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV3);
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
