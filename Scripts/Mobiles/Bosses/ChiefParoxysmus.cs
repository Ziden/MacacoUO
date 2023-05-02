using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a chief paroxysmus corpse")]
    public class ChiefParoxysmus : BasePeerless
    {
        [Constructable]
        public ChiefParoxysmus()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Chefe Paroxysmus";
            Body = 0x100;

            SetStr(1232, 1400);
            SetDex(76, 82);
            SetInt(76, 85);

            SetHits(50000);

            SetDamage(27, 31);

            SetDamageType(ResistanceType.Physical, 80);
            SetDamageType(ResistanceType.Poison, 20);

            SetResistance(ResistanceType.Physical, 75, 85);
            SetResistance(ResistanceType.Fire, 40, 50);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 55, 65);
            SetResistance(ResistanceType.Energy, 50, 60);
			
            SetSkill(SkillName.Wrestling, 120.0);
            SetSkill(SkillName.Tactics, 120.0);
            SetSkill(SkillName.MagicResist, 120.0);
            SetSkill(SkillName.Anatomy, 120.0);
            SetSkill(SkillName.Poisoning, 120.0);

            PackResources(8);
            PackTalismans(5);
            Timer.DelayCall(TimeSpan.FromSeconds(1), new TimerCallback(SpawnBulbous));  //BulbousPutrification

            Fame = 25000;
            Karma = -25000;

            SetAreaEffect(AreaEffect.PoisonBreath);
        }

        public ChiefParoxysmus(Serial serial)
            : base(serial)
        {
        }

        public override bool GivesMLMinorArtifact
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
                return Poison.Lethal;
            }
        }

        public override Poison HitAreaPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }

        public override int AreaPoisonDamage { get { return 50; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.AosSuperBoss, 8);
        }

        public override bool ReduceSpeedWithDamage => false;
        public override bool IsSmart => true;
        public override bool UseSmartAI => true;

        public virtual int BonusExp => 2900;

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            DistribuiPs(110);
            DistribuiPs(115);
            DistribuiPs(120);
            DistribuiItem(new Gold(20000));
            var talisman = new RandomTalisman();
            talisman.ItemID = 0x9E28;
            talisman.Slayer = (TalismanSlayerName)Utility.Random(18);
            SorteiaItem(talisman);
            SorteiaItem(new LardOfParoxysmus());
			
            switch ( Utility.Random(3) )
            {
                case 0:
                    SorteiaItem(new ParoxysmusDinner());
                    break;
                case 1:
                    SorteiaItem(new ParoxysmusCorrodedStein());
                    break;
                case 2:
                    SorteiaItem(new StringOfPartsOfParoxysmusVictims());
                    break;
            }

            if (Utility.RandomDouble() < 0.6)
                SorteiaItem(new ParrotItem());
			
            if (Utility.RandomBool())
                SorteiaItem(new SweatOfParoxysmus());
				
            if (Utility.RandomDouble() < 0.15)
                SorteiaItem(new ParoxysmusSwampDragonStatuette());
				
            //if (Utility.RandomDouble() < 0.15)
            //    SorteiaItem(new ScepterOfTheChief());
        }

        public override int GetDeathSound()
        {
            return 0x56F;
        }

        public override int GetAttackSound()
        {
            return 0x570;
        }

        public override int GetIdleSound()
        {
            return 0x571;
        }

        public override int GetAngerSound()
        {
            return 0x572;
        }

        public override int GetHurtSound()
        {
            return 0x573;
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            base.OnDamage(amount, from, willKill);
			
            // eats pet or summons
            if (from is BaseCreature)
            {
                BaseCreature creature = (BaseCreature)from;
				
                if (creature.Controlled || creature.Summoned)
                {
                    Heal(creature.Hits);					
                    creature.Kill();				
					
                    Effects.PlaySound(Location, Map, 0x574);
                }
            }
			
            // teleports player near
            if (from is PlayerMobile && !InRange(from.Location, 1))
            {
                Combatant = from;
				
                from.MoveToWorld(GetSpawnPosition(1), Map);				
                from.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
                from.PlaySound(0x1FE);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
			
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
			
            int version = reader.ReadInt();
        }

        public virtual void SpawnBulbous()
        {
            for (int i = 0; i < 3; i++)
            {
                SpawnHelper(new BulbousPutrification(), GetSpawnPosition(4));
            }
        }
    }
}
