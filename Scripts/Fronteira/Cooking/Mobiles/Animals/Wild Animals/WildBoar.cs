using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
	[CorpseName( "a pig corpse" )]
	public class WildBoar : BaseAnimal //BaseCreature
	{
		private PigBreed m_MotherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public PigBreed MotherBreed
		{
			get { return m_MotherBreed; }
			set {	m_MotherBreed = value; }
		}
	
		private PigBreed m_FatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public PigBreed FatherBreed
		{
			get { return m_FatherBreed; }
			set {	m_FatherBreed = value; }
		}
		
		[Constructable]
		public WildBoar() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Eats = EatType.Omnivore;
			Animal = AnimalType.Farm;

			Name = "a boar";
			Body = 0x122;
			BaseSoundID = 0xC4;

			MateActive = true;
			MaxAge = 30;
            LitterSize = 2;
            MatingSeason = Seasons.Autumn;
			
			SetStr( 25 );
			SetDex( 15 );
			SetInt( 5 );

			SetHits( 15 );
			SetMana( 0 );

			SetDamage( 3, 6 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 10, 15 );
			SetResistance( ResistanceType.Fire, 5, 10 );
			SetResistance( ResistanceType.Poison, 5, 10 );

			SetSkill( SkillName.MagicResist, 9.0 );
			SetSkill( SkillName.Tactics, 9.0 );
			SetSkill( SkillName.Wrestling, 9.0 );

			Fame = 300;
			Karma = 0;

			VirtualArmor = 10;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 29.1;
		}
		
		public void CalculateBonuses()
		{
			if (Age == AgeDescription.Young)
			{
				RawStr += (Utility.RandomMinMax(2,6));
				RawDex += (Utility.RandomMinMax(2,6));
				RawInt += (Utility.RandomMinMax(2,6));
				MinTameSkill += (Utility.RandomMinMax(2,12));
				if (m_MotherBreed == PigBreed.Feral || m_FatherBreed == PigBreed.Feral)
				{
					double sb = (double)(Utility.RandomMinMax(5,15));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
				}
				DamageMax += 2;
			}
			else if (Age == AgeDescription.Adult)
			{
				RawStr += (Utility.RandomMinMax(5,10));
				RawDex += (Utility.RandomMinMax(5,10));
				RawInt += (Utility.RandomMinMax(5,10));
				MinTameSkill += (Utility.RandomMinMax(10,20));
				if (m_MotherBreed == PigBreed.Feral || m_FatherBreed == PigBreed.Feral)
				{
					double sb = (double)(Utility.RandomMinMax(15,30));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
				}
				DamageMax += 5;
			}
			else if (Age == AgeDescription.Senior)
			{
				RawStr -= (Utility.RandomMinMax(5,10));
				RawDex -= (Utility.RandomMinMax(5,10));
				RawInt += (Utility.RandomMinMax(1,10));
				DamageMax -= 3;
			}
		}

		public virtual void DetermineBreed(PigBreed pb)
		{
			if (m_MotherBreed == m_FatherBreed) this.Title = "["+m_MotherBreed+"]";
			else this.Title = "[Mestico]";
			if (pb == PigBreed.Duroc)
			{
				Body = 0x122;
				Hue = 1190;
			}
			else if (pb == PigBreed.Iberian)
			{
				Hue = 2306;
			}
			else if (pb == PigBreed.Tamworth)
			{
				Hue = 1192;
			}
			else if (pb == PigBreed.White)
			{
				Hue = 0;
			}
			else if (pb == PigBreed.Feral)
			{
				Hue = 2312;
			}
		}
		
		public bool IsPurebred()
		{
			if (m_MotherBreed == m_FatherBreed) return true;
			return false;
		}
		
		public override void BreedInformation(Mobile mate)
		{
			if (mate is FarmPig)
			{
				FarmPig p = (FarmPig) mate;
				if (Utility.RandomBool()) 
				{
					p.PigFatherBreed = m_FatherBreed;
					//this.Say("passes on "+m_FatherBreed);
				}
				else 
				{
					p.PigFatherBreed = m_MotherBreed;
					//this.Say("passes on "+m_MotherBreed); //was DebugSay
				}
			}
		}
		
		public override void OnAfterSpawn()
		{
			base.OnAfterSpawn();
			Female = false;
			m_MotherBreed = (PigBreed) (int)Utility.Random(4);
			m_FatherBreed = (PigBreed) (int)Utility.Random(4);
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);
			//HueMod = ((Hue == 0)? ((Body == 216) ? 1141: 1109): 0);//test
			TypeName = "a pig";
		}
		
		public override void MakeBaby()
		{
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);	
			SetStr( 2, 5 );
			SetDex( 3, 6 );
			SetInt( 1, 3 );
			SetDamage( 1, 3 );
			Name = "a piglet";
			base.MakeBaby();
		}
		
		public override void MakeYoung()
		{
			CalculateBonuses();
			if (Name == "a piglet") 
				Name = "a hog";
			base.MakeYoung();
		}
		
		public override void MakeAdult()
		{
			CalculateBonuses();
			if (Name == "a hog" || Name == "a piglet") 
				Name = "a boar";
			base.MakeAdult();
		}
		
		public override void MakeSenior()
		{
			CalculateBonuses();
			if (Name == "a boar" || Name == "a hog" || Name == "a piglet") 
				Name = "a boar";
			base.MakeSenior();
		}

		public override void OnCarve( Mobile from, Corpse corpse, Item with)
		{
			base.OnCarve( from, corpse, with);
			double meatbonus = 1;
			if (m_MotherBreed == PigBreed.Duroc || m_FatherBreed == PigBreed.Duroc)
			{
				meatbonus += (IsPurebred() ? 2.0 : 1.0 );
			}
			if (m_MotherBreed == PigBreed.Iberian || m_FatherBreed == PigBreed.Iberian)
			{
				meatbonus += (IsPurebred() ? 1.5 : .75 );
			}
			if (m_MotherBreed == PigBreed.Tamworth || m_FatherBreed == PigBreed.Tamworth)
			{
				meatbonus += (IsPurebred() ? 1.20 : .60 );
			}
			if (m_MotherBreed == PigBreed.White || m_FatherBreed == PigBreed.White)
			{
				meatbonus += (IsPurebred() ? 1.20 : .60 );
			}
			if (Age == AgeDescription.Baby)
			{
				meatbonus += 0;
			}
			else if (Age == AgeDescription.Young)
			{
				meatbonus += 1;
			}
			else if (Age == AgeDescription.Adult)
			{
				meatbonus += 2;
			}
			else if (Age == AgeDescription.Senior)
			{
				meatbonus += 1;
			}
            corpse.Carved = true;
			//if (((int)(meatbonus *2))>1) corpse.DropItem( new RawRibs((int)(meatbonus * 2)));
			//corpse.DropItem( new PorkHock( 2 ) );
			//corpse.DropItem( new RawHam((int)(meatbonus * 2 )));
			//corpse.DropItem( new RawBaconSlab((int)(meatbonus * 2 )));
			DropResources(from, meatbonus, corpse);
		}
		
		public virtual void DropResources(Mobile from, double meatbonus, Corpse corpse)
		{
			bool CanUse = from.CheckSkillMult( SkillName.Anatomy, 30, 70 );
			if (CanUse) corpse.DropItem( new RawTrotters(2));
			corpse.DropItem( new RawPigHead(1));
			if (meatbonus < 1) from.SendMessage("Nothing but grisle!");
			if (meatbonus > 0) corpse.DropItem( new RawBaconSlab(1));
			if (meatbonus > 1) corpse.DropItem( new RawSpareRibs(4));
			if (meatbonus > 2) corpse.DropItem( new RawHam(2));
			if (meatbonus > 3 && CanUse) corpse.DropItem( new RawPorkChop(2));
			if (meatbonus > 4 && CanUse) corpse.DropItem( new RawPorkRoast(2));
		}

		public override FoodType FavoriteFood{ get{ return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

        public WildBoar(Serial serial)
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
					m_MotherBreed = (PigBreed)reader.ReadInt();
					m_FatherBreed = (PigBreed)reader.ReadInt();
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
