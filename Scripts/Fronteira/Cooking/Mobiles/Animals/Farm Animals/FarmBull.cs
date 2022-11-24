using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
	[CorpseName( "a bull corpse" )]
	public class FarmBull : BaseAnimal //BaseCreature
	{
		private CowBreed m_MotherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public CowBreed MotherBreed
		{
			get { return m_MotherBreed; }
			set {	m_MotherBreed = value; }
		}
	
		private CowBreed m_FatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public CowBreed FatherBreed
		{
			get { return m_FatherBreed; }
			set {	m_FatherBreed = value; }
		}
		
		[Constructable]
		public FarmBull() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Eats = EatType.Herbivore;
			Animal = AnimalType.Farm;
			
			MateActive = true;
			MaxAge = 100;
            LitterSize = 2;
			MatingSeason = Seasons.Any;

			Name = "a bull";
			Body = Utility.RandomList( 0xE8, 0xE9 );
			BaseSoundID = 0x64;

			if ( 0.5 >= Utility.RandomDouble() )
				Hue = 0x901;

			SetStr( 77, 111 );
			SetDex( 56, 75 );
			SetInt( 47, 75 );

			SetHits( 50, 64 );
			SetMana( 0 );

			SetDamage( 4, 9 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 25, 30 );
			SetResistance( ResistanceType.Cold, 10, 15 );

			SetSkill( SkillName.MagicResist, 17.6, 25.0 );
			SetSkill( SkillName.Tactics, 67.6, 85.0 );
			SetSkill( SkillName.Wrestling, 40.1, 57.5 );

			Fame = 600;
			Karma = 0;

			VirtualArmor = 28;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 71.1;
		}

		public void CalculateBonuses()
		{
			if ( Core.AOS && Utility.Random( 1000 ) == 0 ) // 0.1% chance to have mad cows
				FightMode = FightMode.Closest;
			if (Age == AgeDescription.Young)
			{
				RawStr += (Utility.RandomMinMax(5,10));
				RawDex += (Utility.RandomMinMax(5,10));
				RawInt += (Utility.RandomMinMax(5,10));
				MinTameSkill += (Utility.RandomMinMax(2,12));
				if (m_MotherBreed == CowBreed.ToroBravo || m_FatherBreed == CowBreed.ToroBravo)
				{
					double sb = (double)(Utility.RandomMinMax(5,15));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
					DamageMax += 2;
				}
				if (m_MotherBreed == CowBreed.Corriente || m_FatherBreed == CowBreed.Corriente)
				{
					double sb = (double)(Utility.RandomMinMax(2,10));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
					DamageMax += 1;
				}
				DamageMax += 3;
			}
			else if (Age == AgeDescription.Adult)
			{
				RawStr += (Utility.RandomMinMax(10,20));
				RawDex += (Utility.RandomMinMax(10,20));
				RawInt += (Utility.RandomMinMax(10,20));
				MinTameSkill += (Utility.RandomMinMax(10,20));
				if (m_MotherBreed == CowBreed.ToroBravo || m_FatherBreed == CowBreed.ToroBravo)
				{
					double sb = (double)(Utility.RandomMinMax(15,30));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
					DamageMax += 5;
				}
				if (m_MotherBreed == CowBreed.Corriente || m_FatherBreed == CowBreed.Corriente)
				{
					double sb = (double)(Utility.RandomMinMax(5,10));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
					DamageMax += 2;
				}
				DamageMax += 7;
			}
			else if (Age == AgeDescription.Senior)
			{
				RawStr -= (Utility.RandomMinMax(10,15));
				RawDex -= (Utility.RandomMinMax(10,15));
				RawInt += (Utility.RandomMinMax(1,10));
				DamageMax -= 5;
			}
		}
		
		public virtual void DetermineBreed(CowBreed cb)
		{
			if (m_MotherBreed == m_FatherBreed) this.Title = "["+m_MotherBreed+"]";
			else this.Title = "[Mestico]";
			if (cb == CowBreed.Holstein)
			{
				Body = 233;
				Hue = 2500; 
			}
			else if (cb == CowBreed.Guernsey)
			{
				Body = 233;
				Hue = 0;
			}
			else if (cb == CowBreed.Hereford)
			{
				Body = 233;
				Hue = 0;
			}
			else if (cb == CowBreed.Angus)
			{
				Body = 232;
				Hue = 1109;
			}
			else if (cb == CowBreed.Gloucester)
			{
				Body = 233;
				Hue = 2500; 
			}
			else if (cb == CowBreed.Montbeliarde)
			{
				Body = 233;
				Hue = 0;
			}
			else if (cb == CowBreed.Corriente)
			{
				Body = 233;
				Hue = 2500; 
			}
			else if (cb == CowBreed.ToroBravo)
			{
				Body = 232;
				Hue = 1109;
			}
		}
		
		public bool IsPurebred()
		{
			if (m_MotherBreed == m_FatherBreed) return true;
			return false;
		}
		
		public override void BreedInformation(Mobile mate)
		{
			if (mate is FarmCow)
			{
				FarmCow c = (FarmCow) mate;
				if (Utility.RandomBool()) 
				{
					c.CalfFatherBreed = m_FatherBreed;
					//this.Say("passes on "+m_FatherBreed);
				}
				else 
				{
					c.CalfFatherBreed = m_MotherBreed;
					//this.Say("passes on "+m_MotherBreed); //was DebugSay
				}
			}
		}
		
		public override void OnAfterSpawn()
		{
			base.OnAfterSpawn();
			Female = false;
			int mb = Utility.Random(8);
			int fb = Utility.Random(8);
			m_MotherBreed = (CowBreed) mb;
			m_FatherBreed = (CowBreed) fb;
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);
			//HueMod = ((Hue == 0)? ((Body == 233)?1141: 1109):0);//test
			TypeName = "a cow";
		}
		
		public override void MakeBaby()
		{
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);	
			SetStr( 10, 20 );
			SetDex( 10, 40 );
			SetInt( 5, 10 );
			SetDamage( 1, 3 );
			Name = "a calf";
			base.MakeBaby();
		}
		
		public override void MakeYoung()
		{
			CalculateBonuses();
			if (Name == "a calf") 
				Name = "a yearling";
			base.MakeYoung();
		}
		
		public override void MakeAdult()
		{
			CalculateBonuses();
			if (Name == "a yearling" || Name == "a calf") 
				Name = "a bull";
			base.MakeAdult();
		}
		
		public override void MakeSenior()
		{
			CalculateBonuses();
			if (Name == "a bull" || Name == "a yearling" || Name == "a calf") 
				Name = "a bull";
			base.MakeSenior();
		}
		
		//public override int Meat{ get{ return 10; } }
		//public override int Hides{ get{ return 15; } }
		public override FoodType FavoriteFood{ get{ return FoodType.GrainsAndHay; } }
		public override PackInstinct PackInstinct{ get{ return PackInstinct.Bull; } }

        public override void OnCarve(Mobile from, Corpse corpse, Item with)
        {
            base.OnCarve(from, corpse, with);
            double meatbonus = 1;
			if (m_MotherBreed == CowBreed.Angus || m_FatherBreed == CowBreed.Angus)
			{
				meatbonus += (IsPurebred() ? 4 : 2 );
			}
			if (m_MotherBreed == CowBreed.Hereford || m_FatherBreed == CowBreed.Hereford)
			{
				meatbonus += (IsPurebred() ? 3.5 : 1.75 );
			}
			if (m_MotherBreed == CowBreed.Gloucester || m_FatherBreed == CowBreed.Gloucester)
			{
				meatbonus += (IsPurebred() ? 2 : 1 );
			}
			if (m_MotherBreed == CowBreed.Montbeliarde || m_FatherBreed == CowBreed.Montbeliarde)
			{
				meatbonus += (IsPurebred() ? 1.5 : .75 );
			}
			if (Age == AgeDescription.Baby)
			{
				meatbonus += 0;
			}
			else if (Age == AgeDescription.Young)
			{
				meatbonus += 2;
			}
			else if (Age == AgeDescription.Adult)
			{
				meatbonus += 5;
			}
			else if (Age == AgeDescription.Senior)
			{
				meatbonus += 2;
			}
            corpse.Carved = true;
			DropResources(from, meatbonus, corpse);
			corpse.DropItem(new Hides((int)meatbonus + 4));
			//if (((int)(meatbonus *10))>1) corpse.DropItem( new RawRibs((int)(meatbonus * 10)));
			//corpse.DropItem( new BeefHock( 4 ) );
		}
		
		public virtual void DropResources(Mobile from, double meatbonus, Corpse corpse)
		{
			bool CanUse = from.CheckSkillMult( SkillName.Anatomy, 30, 70 );
			if (meatbonus < 1) from.SendMessage("Nothing but grisle!");
			if (meatbonus > 0 && CanUse) corpse.DropItem( new RawBeefRoast(2));
			if (meatbonus > 1) corpse.DropItem( new RawBeefSirloin(4));
			if (meatbonus > 2) corpse.DropItem( new RawBeefRibs(2));
			if (meatbonus > 3 && CanUse) corpse.DropItem( new RawBeefTBone(2));
			if (meatbonus > 4) corpse.DropItem( new RawBeefTenderloin(2));
			if (meatbonus > 5) corpse.DropItem( new RawBeefRibeye(2));
			if (meatbonus > 6 && CanUse) corpse.DropItem( new RawBeefPrimeRib(2));
			if (meatbonus > 7 && CanUse) corpse.DropItem( new RawBeefPorterhouse(2));
			if (meatbonus > 8) corpse.DropItem( new RawBeefRoast(2));
			if (meatbonus > 9) corpse.DropItem( new RawBeefRibs(4));
		}
		
		public override int GetAngerSound() 
		{ 
			if (m_MotherBreed == CowBreed.Angus || m_FatherBreed == CowBreed.Angus) return 0x3Fa;
			return 0x064; 
		} 

		public override int GetIdleSound() 
		{ 
			if (m_MotherBreed == CowBreed.Angus || m_FatherBreed == CowBreed.Angus) return 0x3F8;
			else return 0x065; 
		} 

		public override int GetAttackSound() 
		{ 
			if (m_MotherBreed == CowBreed.Angus || m_FatherBreed == CowBreed.Angus) return 0x3F9;
			return 0x066; 
		} 

		public override int GetHurtSound() 
		{ 
			if (m_MotherBreed == CowBreed.Angus || m_FatherBreed == CowBreed.Angus) return 0x3Fb;
			return 0x067; 
		} 

		public override int GetDeathSound() 
		{ 
			if (m_MotherBreed == CowBreed.Angus || m_FatherBreed == CowBreed.Angus) return 0x3Fc;
			return 0x068; 
		}

        public FarmBull(Serial serial)
            : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 1);
			writer.Write((int)m_MotherBreed);
			writer.Write((int)m_FatherBreed);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 1:
				{
					m_MotherBreed = (CowBreed)reader.ReadInt();
					m_FatherBreed = (CowBreed)reader.ReadInt();
					goto case 0;
				}
				case 0:
				{
					break;
				}
			}
		}
	}
}
