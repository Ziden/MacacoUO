using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
	public enum PigBreed
	{
		Duroc, //big brown aggressive 1190
		Iberian, //black 2306
		Tamworth, //orange 1192
		White, //pink (common pig) 0
		Feral, //brown (hairy) 2312
	}
	
	[CorpseName( "a pig corpse" )]
	public class FarmPig : BaseAnimal //BaseCreature
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
		
		private PigBreed m_PigFatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public PigBreed PigFatherBreed
		{
			get { return m_PigFatherBreed; }
			set {	m_PigFatherBreed = value; }
		}
		
		[Constructable]
		public FarmPig() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Eats = EatType.Omnivore;
			Animal = AnimalType.Farm;

			Name = "a pig";
			Body = 0xCB;
			BaseSoundID = 0xC4;
			
			MateActive = true;
			MaxAge = 40;
            LitterSize = 2;
            MatingSeason = Seasons.Autumn;

			SetStr( 20 );
			SetDex( 20 );
			SetInt( 5 );

			SetHits( 12 );
			SetMana( 0 );

			SetDamage( 2, 4 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 10, 15 );

			SetSkill( SkillName.MagicResist, 5.0 );
			SetSkill( SkillName.Tactics, 5.0 );
			SetSkill( SkillName.Wrestling, 5.0 );

			Fame = 150;
			Karma = 0;

			VirtualArmor = 12;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 11.1;
		}
		
		public void CalculateBonuses()
		{
			if (Age == AgeDescription.Young)
			{
				RawStr += (Utility.RandomMinMax(1,3));
				RawDex += (Utility.RandomMinMax(1,3));
				RawInt += (Utility.RandomMinMax(1,3));
				MinTameSkill += (Utility.RandomMinMax(5,15));
				DamageMax += 1;
			}
			else if (Age == AgeDescription.Adult)
			{
				RawStr += (Utility.RandomMinMax(2,4));
				RawDex += (Utility.RandomMinMax(2,4));
				RawInt += (Utility.RandomMinMax(2,4));
				MinTameSkill += (Utility.RandomMinMax(10,25));
				DamageMax += 2;
			}
			else if (Age == AgeDescription.Senior)
			{
				RawStr -= (Utility.RandomMinMax(1,2));
				RawDex -= (Utility.RandomMinMax(1,2));
				RawInt += (Utility.RandomMinMax(1,3));
				MinTameSkill -= (Utility.RandomMinMax(10,20));
				DamageMax -= 2;
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

		public override void OnAfterSpawn()
		{
			base.OnAfterSpawn();
			Female = true;
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
			SetStr( 6, 10 );
			SetDex( 6, 10 );
			SetInt( 4, 8 );
			SetDamage( 1 );
			MinTameSkill = 1.1;
			Name = "a piglet";
			base.MakeBaby();
		}
		
		public override void MakeYoung()
		{
			CalculateBonuses();
			if (Name == "a piglet") 
				Name = "a sucker";
			base.MakeYoung();
		}
		
		public override void MakeAdult()
		{
			CalculateBonuses();
			if (Name == "a sucker" || Name == "a piglet") 
				Name = "a sow";
			base.MakeAdult();
		}
		
		public override void MakeSenior()
		{
			CalculateBonuses();
			if (Name == "a sow" || Name == "a sucker" || Name == "a piglet") 
				Name = "a sow";
			base.MakeSenior();
		}
		
		public override void CreateBaby()
		{
			for (int j = 0; j < this.LitterSize; j++) 
			{
				int gender = Utility.RandomMinMax(1,4);
				if (gender == 1) 
				{
					WildBoar piglet = new WildBoar();
					piglet.Female = false;
					piglet.Mother = this;
					piglet.Spawned = false;
					piglet.TypeName = "a pig";
					if (Utility.RandomBool()) 
					{
						piglet.MotherBreed = m_MotherBreed;
						//this.Say("passes on "+m_MotherBreed);
					}
					else 
					{
						piglet.MotherBreed = m_FatherBreed;
						//this.Say("passes on "+m_FatherBreed);
					}
					piglet.FatherBreed = PigFatherBreed;
					piglet.MakeBaby();
					if (this.Brand != "" && this.Brand != null) piglet.Owner = this.Owner;
					piglet.MoveToWorld(this.Location, this.Map);
				}	
				else
				{
					FarmPig piglet = new FarmPig();
					piglet.Female = true;
					piglet.Mother = this;
					piglet.Spawned = false;
					piglet.TypeName = "a pig";
					if (Utility.RandomBool()) 
					{
						piglet.MotherBreed = m_MotherBreed;
						//this.Say("passes on "+m_MotherBreed);
					}
					else 
					{
						piglet.MotherBreed = m_FatherBreed;
						//this.Say("passes on "+m_FatherBreed);
					}
					piglet.FatherBreed = PigFatherBreed;	
					piglet.MakeBaby();
					if (this.Brand != "" && this.Brand != null) piglet.Owner = this.Owner;
					piglet.MoveToWorld(this.Location, this.Map);
				}
			}
			base.CreateBaby();
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
			//if (((int)(meatbonus *1))>1)  corpse.DropItem( new RawRibs((int)(meatbonus * 1)));
			//corpse.DropItem( new PorkHock( 1 ) );
			//corpse.DropItem( new RawHam((int)(meatbonus * 1 )));
			//corpse.DropItem( new RawBaconSlab((int)(meatbonus * 1 )));

			DropResources(from, meatbonus, corpse);
		}
		
		public virtual void DropResources(Mobile from, double meatbonus, Corpse corpse)
		{
			bool CanUse = from.CheckSkillMult( SkillName.Anatomy, 30, 70 );
			corpse.DropItem( new RawTrotters(2));
			if (CanUse) corpse.DropItem( new RawPigHead(1));
			if (meatbonus < 1) from.SendMessage("Nothing but grisle!");
			if (meatbonus > 0) corpse.DropItem( new RawBaconSlab(1));
			if (meatbonus > 1) corpse.DropItem( new RawSpareRibs(4));
			if (meatbonus > 2 && CanUse) corpse.DropItem( new RawHam(2));
			if (meatbonus > 3) corpse.DropItem( new RawPorkChop(2));
			if (meatbonus > 4 && CanUse) corpse.DropItem( new RawPorkRoast(2));
		}
		
		public override FoodType FavoriteFood{ get{ return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

        public FarmPig(Serial serial)
            : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 1);
			
			writer.Write((int)m_PigFatherBreed);
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
					m_PigFatherBreed = (PigBreed)reader.ReadInt();
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
