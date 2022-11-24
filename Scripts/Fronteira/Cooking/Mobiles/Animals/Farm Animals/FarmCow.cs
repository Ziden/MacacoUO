using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Engines.Craft;

namespace Server.Mobiles
{
	public enum CowBreed
	{
		//brown bull 232, brown spotted bull 233, black spotted cow 216, brown spotted cow 231
		Holstein, //black and white milk cow
		Guernsey, //red and white milk cow
		Hereford, //red and white beef cow
		Angus, //black beef cow -hue 1109
		Gloucester, //black and white milk/beef cow
		Montbeliarde, //red and white milk/beef cow
		Corriente, //black and white sport/beef cow
		ToroBravo, //black fighting cow (bull)
	}
	
	[CorpseName( "a cow corpse" )]
	public class FarmCow : BaseAnimal //BaseCreature
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
		
		private CowBreed m_CalfFatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public CowBreed CalfFatherBreed
		{
			get { return m_CalfFatherBreed; }
			set {	m_CalfFatherBreed = value; }
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
		public FarmCow() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Grouping = GroupingType.Herd;
			Eats = EatType.Herbivore;
			Animal = AnimalType.Farm;
			
			MateActive = true;
			MaxAge = 100;
            LitterSize = 2;
			MatingSeason = Seasons.Any;

			Name = "a cow";
			Body = Utility.RandomList( 0xD8, 0xE7 );
			BaseSoundID = 0x78;

			SetStr( 30 );
			SetDex( 15 );
			SetInt( 5 );

			SetHits( 18 );
			SetMana( 0 );

			SetDamage( 1, 4 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 5, 15 );

			SetSkill( SkillName.MagicResist, 5.5 );
			SetSkill( SkillName.Tactics, 5.5 );
			SetSkill( SkillName.Wrestling, 5.5 );

			Fame = 300;
			Karma = 0;

			VirtualArmor = 10;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 11.1;

			if ( Core.AOS && Utility.Random( 1000 ) == 0 ) // 0.1% chance to have mad cows
				FightMode = FightMode.Closest;
		}
		
		public void CalculateBonuses()
		{
			if ( Core.AOS && Utility.Random( 1000 ) == 0 ) // 0.1% chance to have mad cows
				FightMode = FightMode.Closest;
			if (Age == AgeDescription.Young)
			{
				RawStr += (Utility.RandomMinMax(1,6));
				RawDex += (Utility.RandomMinMax(1,6));
				RawInt += (Utility.RandomMinMax(1,6));
				MinTameSkill += (Utility.RandomMinMax(1,6));
				if (m_MotherBreed == CowBreed.ToroBravo || m_FatherBreed == CowBreed.ToroBravo)
				{
					double sb = (double)(Utility.RandomMinMax(1,10));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
				}
				DamageMax += 1;
			}
			else if (Age == AgeDescription.Adult)
			{
				RawStr += (Utility.RandomMinMax(2,10));
				RawDex += (Utility.RandomMinMax(2,10));
				RawInt += (Utility.RandomMinMax(2,10));
				MinTameSkill += (Utility.RandomMinMax(5,10));
				if (m_MotherBreed == CowBreed.ToroBravo || m_FatherBreed == CowBreed.ToroBravo)
				{
					double sb = (double)(Utility.RandomMinMax(5,20));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
				}
				DamageMax += 2;
			}
			else if (Age == AgeDescription.Senior)
			{
				RawStr -= (Utility.RandomMinMax(2,10));
				RawDex -= (Utility.RandomMinMax(2,10));
				RawInt += (Utility.RandomMinMax(2,10));
				MinTameSkill -= (Utility.RandomMinMax(10,10));
				DamageMax -= 2;
			}
		}
		
		public virtual void DetermineBreed(CowBreed cb)
		{
			if (m_MotherBreed == m_FatherBreed) this.Title = "["+m_MotherBreed+"]";
			else this.Title = "[Mestico]";
			if (cb == CowBreed.Holstein)
			{
				Body = 216;
				Hue = 0;
			}
			else if (cb == CowBreed.Guernsey)
			{
				Body = 231;
				Hue = 0;
			}
			else if (cb == CowBreed.Hereford)
			{
				Body = 231;
				Hue = 0;
			}
			else if (cb == CowBreed.Angus)
			{
				Body = 216;
				Hue = 1109;
			}
			else if (cb == CowBreed.Gloucester)
			{
				Body = 216;
				Hue = 0;
			}
			else if (cb == CowBreed.Montbeliarde)
			{
				Body = 231;
				Hue = 0;
			}
			else if (cb == CowBreed.Corriente)
			{
				Body = 216;
				Hue = 0;
			}
			else if (cb == CowBreed.ToroBravo)
			{
				Body = 216;
				Hue = 1109;
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
			int mb = Utility.Random(8);
			int fb = Utility.Random(8);
			m_MotherBreed = (CowBreed) mb;
			m_FatherBreed = (CowBreed) fb;
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);
			//HueMod = ((Hue == 0)? ((Body == 216) ? 1141: 1109): 0);//test
			TypeName = "a cow";
		}


        public override void MakeBaby()
		{
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);			
			SetStr( 6, 16 );
			SetDex( 6, 16 );
			SetInt( 4, 12 );
			SetDamage( 1 );
			MinTameSkill = 1.1;
			Name = "a calf";
			base.MakeBaby();
		}
		
		public override void MakeYoung()
		{
			CalculateBonuses();
			if (Name == "a calf") 
				Name = "a heifer";
			base.MakeYoung();
		}
		
		public override void MakeAdult()
		{
			CalculateBonuses();
			if (Name == "a heifer" || Name == "a calf") 
				Name = "a cow";
			base.MakeAdult();
		}
		
		public override void MakeSenior()
		{
			CalculateBonuses();
			if (Name == "a cow" || Name == "a heifer" || Name == "a calf") 
				Name = "a cow";
			base.MakeSenior();
		}
		
		public override void CreateBaby()
		{
			for (int j = 0; j < this.LitterSize; j++) 
			{
				int gender = Utility.RandomMinMax(1,4);
				if (gender == 1) 
				{
					FarmBull calf = new FarmBull();
					calf.Female = false;
					calf.Mother = this;
					calf.Spawned = false;
					calf.TypeName = "a cow";
					if (Utility.RandomBool()) 
					{
						calf.MotherBreed = m_MotherBreed;
						//this.Say("passes on "+m_MotherBreed);
					}
					else 
					{
						calf.MotherBreed = m_FatherBreed;
						//this.Say("passes on "+m_FatherBreed);
					}
					calf.FatherBreed = CalfFatherBreed;
					calf.MakeBaby();
					if (this.Brand != "" && this.Brand != null) calf.Owner = this.Owner;
					calf.MoveToWorld(this.Location, this.Map);
				}	
				else
				{
					FarmCow calf = new FarmCow();
					calf.Female = true;
					calf.Mother = this;
					calf.Spawned = false;
					calf.TypeName = "a cow";
					if (Utility.RandomBool()) 
					{
						calf.MotherBreed = m_MotherBreed;
						//this.Say("passes on "+m_MotherBreed);
					}
					else 
					{
						calf.MotherBreed = m_FatherBreed;
						//this.Say("passes on "+m_FatherBreed);
					}
					calf.FatherBreed = CalfFatherBreed;	
					calf.MakeBaby();
					if (this.Brand != "" && this.Brand != null) calf.Owner = this.Owner;
					calf.MoveToWorld(this.Location, this.Map);
				}
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
			if (m_MotherBreed == CowBreed.Holstein || m_FatherBreed == CowBreed.Holstein)
			{
				milkbonus += (IsPurebred() ? 1.50 : .75 );
			}
			if (m_MotherBreed == CowBreed.Guernsey || m_FatherBreed == CowBreed.Guernsey)
			{
				milkbonus += (IsPurebred() ? 1.20 : .60 );
			}
			if (m_MotherBreed == CowBreed.Gloucester || m_FatherBreed == CowBreed.Gloucester)
			{
				milkbonus += (IsPurebred() ? .80 : .40 );
			}
			if (m_MotherBreed == CowBreed.Montbeliarde || m_FatherBreed == CowBreed.Montbeliarde)
			{
				milkbonus += (IsPurebred() ? .80 : .40 );
			}
			int quarterdays = (int)((CheckDifference.TotalMinutes / 60 / 6 )*12);
			if (quarterdays > 28 && !this.HasOffspring)
			{
				m_CanMilk = false;
				m_Milk = 0;
			}
			else
			{
				int tempMilk = (int)(quarterdays * milkbonus);
				m_Milk = (tempMilk <= 25)?tempMilk:25;
			}
		}
		
		public override void OnThink() 
		{
			if (CanCheckMilk() && m_CanMilk) CheckMilk();
			base.OnThink();
		}
		
		//public override int Meat{ get{ return 8; } }
		//public override int Hides{ get{ return 12; } }
		public override FoodType FavoriteFood{ get{ return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

        public override void OnCarve(Mobile from, Corpse corpse, Item with)
        {
            base.OnCarve(from, corpse, with);
            double meatbonus = 1;
			if (m_MotherBreed == CowBreed.Angus || m_FatherBreed == CowBreed.Angus)
			{
				meatbonus += (IsPurebred() ? 2.0 : 1.0 );
			}
			if (m_MotherBreed == CowBreed.Hereford || m_FatherBreed == CowBreed.Hereford)
			{
				meatbonus += (IsPurebred() ? 1.5 : .75 );
			}
			if (m_MotherBreed == CowBreed.Gloucester || m_FatherBreed == CowBreed.Gloucester)
			{
				meatbonus += (IsPurebred() ? 1.20 : .60 );
			}
			if (m_MotherBreed == CowBreed.Montbeliarde || m_FatherBreed == CowBreed.Montbeliarde)
			{
				meatbonus += (IsPurebred() ? 1.20 : .60 );
			}
			if (Age == AgeDescription.Baby)
			{
				meatbonus += 0;
			}
			else if (Age == AgeDescription.Young)
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

		public override void OnDoubleClick( Mobile from )
		{
			base.OnDoubleClick( from );

			int random = Utility.Random( 100 );

			if ( random < 5 )
				Tip();
			else if ( random < 20 )
				PlaySound( 120 );
			else if ( random < 40 )
				PlaySound( 121 );
		}

		public void Tip()
		{
			PlaySound( 121 );
			Animate( 8, 0, 3, true, false, 0 );
		}

        public FarmCow(Serial serial)
            : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 1);

			writer.Write((int)m_CalfFatherBreed);
			writer.Write((int)m_MotherBreed);
			writer.Write((int)m_FatherBreed);
			writer.Write((int)m_Milk);
			writer.Write(m_LastMilking);
			writer.Write((bool)m_CanMilk);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 1:
				{
					m_CalfFatherBreed = (CowBreed)reader.ReadInt();
					m_MotherBreed = (CowBreed)reader.ReadInt();
					m_FatherBreed = (CowBreed)reader.ReadInt();
					m_Milk = reader.ReadInt();
					m_LastMilking = reader.ReadDateTime();
					m_CanMilk = reader.ReadBool();
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
