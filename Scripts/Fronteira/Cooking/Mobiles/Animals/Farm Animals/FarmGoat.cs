using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Items.Crops; 

namespace Server.Mobiles
{
	public enum GoatBreed
	{
		Pyrenean, //brown milk
		Saanen, //white milk
		Angora, //white wool (mohair)
		Cashmere, //brown wool
		Boer, //white and brown meat
		Stiefelgeiss, //brown meat
	}
	
	[CorpseName( "a goat corpse" )]
	public class FarmGoat : BaseAnimal, ICarvable//BaseCreature
	{
		private GoatBreed m_MotherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public GoatBreed MotherBreed
		{
			get { return m_MotherBreed; }
			set {	m_MotherBreed = value; }
		}
	
		private GoatBreed m_FatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public GoatBreed FatherBreed
		{
			get { return m_FatherBreed; }
			set {	m_FatherBreed = value; }
		}
		
		private GoatBreed m_KidFatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public GoatBreed KidFatherBreed
		{
			get { return m_KidFatherBreed; }
			set {	m_KidFatherBreed = value; }
		}
	
		private int m_Milk = 0;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Milk
		{
			get { return m_Milk; }
			set {	m_Milk = value; }
		}	
		
		private DateTime m_LastMilking = DateTime.UtcNow;
		[ CommandProperty( AccessLevel.GameMaster ) ]
		public DateTime LastMilking
		{
			get { return m_LastMilking; }
			set { m_LastMilking = value; }
		}
	
		private bool m_CanMilk = false;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool CanMilk
		{
			get { return m_CanMilk; }
			set {	m_CanMilk = value; }
		}
	
		[Constructable]
		public FarmGoat() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Activity = ActivityType.Day;
			Eats = EatType.Herbivore;
			Animal = AnimalType.Farm;
			
			MateActive = true;
			MaxAge = 50;
            LitterSize = 2;
			MatingSeason = Seasons.Any;

			Name = "a goat";
			Body = 0xD1;
			BaseSoundID = 0x99;

			SetStr( 19 );
			SetDex( 15 );
			SetInt( 5 );

			SetHits( 12 );
			SetMana( 0 );

			SetDamage( 3, 4 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 5, 15 );

			SetSkill( SkillName.MagicResist, 5.0 );
			SetSkill( SkillName.Tactics, 5.0 );
			SetSkill( SkillName.Wrestling, 5.0 );

			Fame = 150;
			Karma = 0;

			VirtualArmor = 10;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 11.1;
		}

        public override bool CropFood(Item item)//virtual not used in this case.
		{
			//crop system
            if (item is CropHelper.Weeds)
            {
                return true;
            }
			return base.VegetableFood(item);
		}
		
		public void CalculateBonuses()
		{
			if (Age == AgeDescription.Young)
			{
				RawStr += (Utility.RandomMinMax(1,3));
				RawDex += (Utility.RandomMinMax(1,3));
				RawInt += (Utility.RandomMinMax(1,3));
				MinTameSkill += (Utility.RandomMinMax(1,3));
				DamageMax += 1;
			}
			else if (Age == AgeDescription.Adult)
			{
				RawStr += (Utility.RandomMinMax(2,8));
				RawDex += (Utility.RandomMinMax(2,8));
				RawInt += (Utility.RandomMinMax(2,5));
				MinTameSkill += (Utility.RandomMinMax(1,5));
				DamageMax += 2;
			}
			else if (Age == AgeDescription.Senior)
			{
				RawStr -= (Utility.RandomMinMax(2,4));
				RawDex -= (Utility.RandomMinMax(2,4));
				RawInt += (Utility.RandomMinMax(2,5));
				MinTameSkill -= (Utility.RandomMinMax(2,5));
				DamageMax -= 2;
			}
		}
		
		public virtual void DetermineBreed(GoatBreed cb)
		{
			if (m_MotherBreed == m_FatherBreed) this.Title = "["+m_MotherBreed+"]";
			else this.Title = "[Mestico]";
			if (cb == GoatBreed.Pyrenean)
			{
				Hue = 1816;
			}
			else if (cb == GoatBreed.Saanen)
			{
				Hue = 0;
			}
			else if (cb == GoatBreed.Angora)
			{
				Hue = 2500;
			}
			else if (cb == GoatBreed.Cashmere)
			{
				Hue = 1861;
			}
			else if (cb == GoatBreed.Boer)
			{
				Hue = 1810;
			}
			else if (cb == GoatBreed.Stiefelgeiss)
			{
				Hue = 1842;
			}
		}
		
		public override void BreedInformation(Mobile mate)
		{
			if (mate is FarmGoat)
			{
				FarmGoat s = (FarmGoat) mate;
				if (Utility.RandomBool()) 
				{
					s.KidFatherBreed = m_FatherBreed;
					//this.Say("passes on "+m_FatherBreed);
				}
				else 
				{
					s.KidFatherBreed = m_MotherBreed;
					//this.Say("passes on "+m_MotherBreed); //was DebugSay
				}
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
			Name = (this.Female ? "a nanny goat": "a billy goat");
			m_MotherBreed = (GoatBreed) Utility.Random(6);
			m_FatherBreed = (GoatBreed) Utility.Random(6);
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);
			TypeName = "a goat";
		}
		
		public override void MakeBaby()
		{
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);			
			SetStr( 1, 4 );
			SetDex( 1, 4 );
			SetInt( 1, 2 );
			SetDamage( 1 );
			MinTameSkill = 1.1;
			Name = "a kid";
			base.MakeBaby();
		}
		
		public override void MakeYoung()
		{
			CalculateBonuses();
			if (Name == "a kid") 
				Name = "a kid";
			base.MakeYoung();
		}
		
		public override void MakeAdult()
		{
			CalculateBonuses();
			if (Name == "a kid") 
				Name = (this.Female ? "a nanny goat": "a billy goat");
			base.MakeAdult();
		}
		
		public override void MakeSenior()
		{
			CalculateBonuses();
			if (Name == "a nanny goat" || Name == "a billy goat" || Name == "a kid") 
				Name = (this.Female ? "a nanny goat": "a billy goat");
			base.MakeSenior();
		}
		
		public override void CreateBaby()
		{
			for (int j = 0; j < this.LitterSize; j++) 
			{
				FarmGoat kid = new FarmGoat();
				kid.Female = ((Utility.RandomMinMax(1,4) == 1) ? false: true);
				kid.Mother = this;
				kid.Spawned = false;
				kid.TypeName = "a goat";
				if (Utility.RandomBool()) 
				{
					kid.MotherBreed = m_MotherBreed;
					//this.Say("passes on "+m_MotherBreed);
				}
				else 
				{
					kid.MotherBreed = m_FatherBreed;
					//this.Say("passes on "+m_FatherBreed);
				}
				kid.FatherBreed = KidFatherBreed;
				kid.MakeBaby();
				if (this.Brand != "" && this.Brand != null) kid.Owner = this.Owner;
				kid.MoveToWorld(this.Location, this.Map);
			}
			m_CanMilk = true;
			base.CreateBaby();
		}
		
		
		private DateTime LastMilkCheck = DateTime.UtcNow;
		private TimeSpan MilkCheckDelay = TimeSpan.FromMinutes(1.0);
		public bool CanCheckMilk()
		{
			if (LastMilkCheck.Add(MilkCheckDelay) < DateTime.UtcNow) return true;
			return false;
		}

		public void CheckMilk() 
		{
			LastMilkCheck = DateTime.UtcNow;
			TimeSpan CheckDifference =  DateTime.UtcNow - m_LastMilking;
			double milkbonus = 1;
			if (m_MotherBreed == GoatBreed.Pyrenean || m_FatherBreed == GoatBreed.Pyrenean)
			{
				milkbonus += (IsPurebred() ? 1.50 : .75 );
			}
			if (m_MotherBreed == GoatBreed.Saanen || m_FatherBreed == GoatBreed.Saanen)
			{
				milkbonus += (IsPurebred() ? 1.20 : .60 );
			}
			int halfdays = (int)((CheckDifference.TotalMinutes / 60 / 12 )*12);
			if (halfdays > 24 && !this.HasOffspring)
			{
				m_CanMilk = false;
				m_Milk = 0;
			}
			else
			{
				int tempMilk = (int)(halfdays * milkbonus);
				m_Milk = (tempMilk <= 14)?tempMilk:14;
			}
		}
		
		private DateTime m_NextWoolTime;
		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime NextWoolTime
		{
			get{ return m_NextWoolTime; }
			set{ m_NextWoolTime = value;}
		}

        public bool Carve(Mobile from, Item item)
        {
            if (DateTime.UtcNow < m_NextWoolTime)
            {
                // This sheep is not yet ready to be shorn.
                PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500449, from.NetState);
                return false;
            }

            from.SendLocalizedMessage( 500452 ); // You place the gathered wool into your backpack.
			double woolbonus = 1;
			if (m_MotherBreed == GoatBreed.Angora || m_FatherBreed == GoatBreed.Angora)
			{
				woolbonus += (IsPurebred() ? 3.00 : 1.50 );
			}
			if (m_MotherBreed == GoatBreed.Cashmere || m_FatherBreed == GoatBreed.Cashmere)
			{
				woolbonus += (IsPurebred() ? 2.00 : 1.00 );
			}
			Wool w = new Wool();
			w.Amount = (int)woolbonus;
			w.Hue = Hue;
			//ToDo: different types of wool.  Requires modifying tailoring system.
			from.AddToBackpack(w);

			NextWoolTime = DateTime.UtcNow + TimeSpan.FromHours( 3.0 ); // TODO: Proper time delay
            return true;
		}

		public override void OnThink()
		{
			base.OnThink();
			if (CanCheckMilk() && m_CanMilk) CheckMilk();
		}

        public override void OnCarve( Mobile from, Corpse corpse, Item with)
		{
			Carve(from, corpse);
			
			base.OnCarve( from, corpse, with);
			double meatbonus = 0;
			if (m_MotherBreed == GoatBreed.Boer || m_FatherBreed == GoatBreed.Boer)
			{
				meatbonus += (IsPurebred() ? 1.50 : .75 );
			}
			if (m_MotherBreed == GoatBreed.Stiefelgeiss || m_FatherBreed == GoatBreed.Stiefelgeiss)
			{
				meatbonus += (IsPurebred() ? 2.0 : 1.0 );
			}
			if (Age == AgeDescription.Young)
			{
				meatbonus += .25;
			}
			else if (Age == AgeDescription.Adult)
			{
				meatbonus += .50;
			}
			else if (Age == AgeDescription.Senior)
			{
				meatbonus += .40;
			}
			meatbonus += (Female ? 0:1);
			//if (((int)(meatbonus*2)) > 1) corpse.DropItem( new RawRibs((int)(meatbonus * 2)));
			corpse.Carved = true;
			corpse.DropItem( new GoatHide((int)meatbonus * 2));
			DropResources(from, meatbonus, corpse);
		}
		
		public virtual void DropResources(Mobile from, double meatbonus, Corpse corpse)
		{
			bool CanUse = from.CheckSkillMult( SkillName.Anatomy, 30, 70 );
			if (meatbonus < 1) from.SendMessage("Nothing but grisle!");
			if (meatbonus > 0) corpse.DropItem( new RawGoatSteak(2));
			if (meatbonus > 1 && CanUse) corpse.DropItem( new RawGoatRoast(2));
			if (meatbonus > 2) corpse.DropItem( new RawGoatSteak(2));
			if (meatbonus > 3 && CanUse) corpse.DropItem( new RawGoatRoast(2));
		} 

		//public override int Meat{ get{ return 2; } }
		//public override int Hides{ get{ return 8; } }
		public override FoodType FavoriteFood{ get{ return FoodType.GrainsAndHay | FoodType.FruitsAndVegies; } }

        public FarmGoat(Serial serial)
            : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 1);
			
			writer.Write((int)m_KidFatherBreed);
			writer.Write((int)m_MotherBreed);
			writer.Write((int)m_FatherBreed);
			writer.Write((int)m_Milk);
			writer.Write(m_LastMilking);
			writer.Write((bool)m_CanMilk);
			writer.WriteDeltaTime( m_NextWoolTime );
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 1:
				{
					m_KidFatherBreed = (GoatBreed)reader.ReadInt();
					m_MotherBreed = (GoatBreed)reader.ReadInt();
					m_FatherBreed = (GoatBreed)reader.ReadInt();
					m_Milk = reader.ReadInt();
					m_LastMilking = reader.ReadDateTime();
					m_CanMilk = reader.ReadBool();
					m_NextWoolTime = reader.ReadDeltaTime();
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
