using System;
using Server;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
	public enum SheepBreed
	{
		Cotswold, //wool, meat
		Cormo, //wool
		Swaledale, //wool, meat (looks like goat)
		Racka, //meat, wool, milk (looks like goat)
		Latxa, //milk
		Coopworth, //meat, wool
		Awassi, //milk
	}
	
	[CorpseName( "a sheep corpse" )]
	public class FarmSheep : BaseAnimal, ICarvable //BaseCreature, ICarvable
	{
		private SheepBreed m_MotherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public SheepBreed MotherBreed
		{
			get { return m_MotherBreed; }
			set {	m_MotherBreed = value; }
		}
	
		private SheepBreed m_FatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public SheepBreed FatherBreed
		{
			get { return m_FatherBreed; }
			set {	m_FatherBreed = value; }
		}
		
		private SheepBreed m_LambFatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public SheepBreed LambFatherBreed
		{
			get { return m_LambFatherBreed; }
			set {	m_LambFatherBreed = value; }
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
		public FarmSheep() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Grouping = GroupingType.Herd;
			Eats = EatType.Herbivore;
			Animal = AnimalType.Farm;

			MateActive = true;
			MaxAge = 50;
            LitterSize = 2;
            MatingSeason = Seasons.Spring;
			
			Name = "a sheep";
			Body = 0xCF;
			BaseSoundID = 0xD6;

			SetStr( 19 );
			SetDex( 25 );
			SetInt( 5 );

			SetHits( 12 );
			SetMana( 0 );

			SetDamage( 1, 2 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 5, 10 );

			SetSkill( SkillName.MagicResist, 5.0 );
			SetSkill( SkillName.Tactics, 6.0 );
			SetSkill( SkillName.Wrestling, 5.0 );

			Fame = 300;
			Karma = 0;

			VirtualArmor = 6;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 11.1;

			// this is (was?) in OSI
			if ( Utility.RandomDouble() < 0.1 )
				Hue = 902;
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
		
		public virtual void DetermineBreed(SheepBreed cb)
		{
			if (m_MotherBreed == m_FatherBreed) this.Title = "["+m_MotherBreed+"]";
			else this.Title = "[Mestico]";
			if (cb == SheepBreed.Cotswold)
			{
				Hue = 2500;
			}
			else if (cb == SheepBreed.Cormo)
			{
				
			}
			else if (cb == SheepBreed.Swaledale)
			{
				Hue = 1882;
			}
			else if (cb == SheepBreed.Racka)
			{
				Hue = 1191;
			}
			else if (cb == SheepBreed.Latxa)
			{
				Hue = 1126;
			}
			else if (cb == SheepBreed.Coopworth)
			{
				
			}
			else if (cb == SheepBreed.Awassi)
			{
				Hue = 1120;
			}
		}
		
		public override void BreedInformation(Mobile mate)
		{
			if (mate is FarmSheep)
			{
				FarmSheep s = (FarmSheep) mate;
				if (Utility.RandomBool()) 
				{
					s.LambFatherBreed = m_FatherBreed;
					//this.Say("passes on "+m_FatherBreed);
				}
				else 
				{
					s.LambFatherBreed = m_MotherBreed;
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
			Name = (this.Female ? "a ewe": "a ram");
			m_MotherBreed = (SheepBreed) Utility.Random(7);
			m_FatherBreed = (SheepBreed) Utility.Random(7);
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);
			TypeName = "a sheep";
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
			Name = "a lamb";
			base.MakeBaby();
		}
		
		public override void MakeYoung()
		{
			CalculateBonuses();
			if (Name == "a lamb") 
				Name = "a lamb";
			base.MakeYoung();
		}
		
		public override void MakeAdult()
		{
			CalculateBonuses();
			if (Name == "a lamb") 
				Name = (this.Female ? "a ewe": "a ram");
			base.MakeAdult();
		}
		
		public override void MakeSenior()
		{
			CalculateBonuses();
			if (Name == "a ewe" || Name == "a ram" || Name == "a lamb") 
				Name = (this.Female ? "a ewe": "a ram");
			base.MakeSenior();
		}
		
		public override void CreateBaby()
		{
			for (int j = 0; j < this.LitterSize; j++) 
			{
				FarmSheep lamb = new FarmSheep();
				lamb.Female = ((Utility.RandomMinMax(1,4) == 1) ? false: true);
				lamb.Mother = this;
				lamb.Spawned = false;
				lamb.TypeName = "a sheep";
				if (Utility.RandomBool()) 
				{
					lamb.MotherBreed = m_MotherBreed;
					//this.Say("passes on "+m_MotherBreed);
				}
				else 
				{
					lamb.MotherBreed = m_FatherBreed;
					//this.Say("passes on "+m_FatherBreed);
				}
				lamb.FatherBreed = LambFatherBreed;
				lamb.MakeBaby();
				if (this.Brand != "" && this.Brand != null) lamb.Owner = this.Owner;
				lamb.MoveToWorld(this.Location, this.Map);
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
			if (m_MotherBreed == SheepBreed.Racka || m_FatherBreed == SheepBreed.Racka)
			{
				milkbonus += (IsPurebred() ? 1.50 : .75 );
			}
			if (m_MotherBreed == SheepBreed.Latxa || m_FatherBreed == SheepBreed.Latxa)
			{
				milkbonus += (IsPurebred() ? 1.20 : .60 );
			}
			if (m_MotherBreed == SheepBreed.Awassi || m_FatherBreed == SheepBreed.Awassi)
			{
				milkbonus += (IsPurebred() ? .80 : .40 );
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
				m_Milk = (tempMilk <= 10)?tempMilk:10;
			}
		}
		
		private DateTime m_NextWoolTime;
		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime NextWoolTime
		{
			get{ return m_NextWoolTime; }
			set{ m_NextWoolTime = value; Body = ( DateTime.UtcNow >= m_NextWoolTime ) ? 0xCF : 0xDF; }
		}

		public bool Carve( Mobile from, Item item )
		{
            if (DateTime.UtcNow < m_NextWoolTime)
            {
                // This sheep is not yet ready to be shorn.
                PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500449, from.NetState);
                return false;
            }

            from.SendLocalizedMessage( 500452 ); // You place the gathered wool into your backpack.
			//from.AddToBackpack( new Wool( Map == Map.Felucca ? 2 : 1 ) );
			double woolbonus = 0;
			if (m_MotherBreed == SheepBreed.Cormo || m_FatherBreed == SheepBreed.Cormo)
			{
				woolbonus += (IsPurebred() ? 3.00 : 1.50 );
			}
			if (m_MotherBreed == SheepBreed.Cotswold || m_FatherBreed == SheepBreed.Cotswold)
			{
				woolbonus += (IsPurebred() ? 2.00 : 1.00 );
			}
			if (m_MotherBreed == SheepBreed.Swaledale || m_FatherBreed == SheepBreed.Swaledale)
			{
				woolbonus += (IsPurebred() ? 2.00 : 1.00 );
			}
			if (m_MotherBreed == SheepBreed.Coopworth || m_FatherBreed == SheepBreed.Coopworth)
			{
				woolbonus += (IsPurebred() ? 2.00 : 1.00 );
			}
			if (m_MotherBreed == SheepBreed.Racka || m_FatherBreed == SheepBreed.Racka)
			{
				woolbonus += (IsPurebred() ? 1.50 : .75 );
			}
			if (m_MotherBreed == SheepBreed.Latxa || m_FatherBreed == SheepBreed.Latxa)
			{
				woolbonus += (IsPurebred() ? 1.00 : .50 );
			}
			if (m_MotherBreed == SheepBreed.Awassi || m_FatherBreed == SheepBreed.Awassi)
			{
				woolbonus += (IsPurebred() ? 1.00 : .50 );
			}
			Wool w = new Wool();
			w.Amount = (int)woolbonus;
			w.Hue = Hue;
			//ToDo: different types of wool.  Requires modifying tailoring system.
			from.AddToBackpack(w);

            if (from is PlayerMobile)
            {
                PlayerMobile player = (PlayerMobile)from;
                foreach (BaseQuest quest in player.Quests)
                {
                    if (quest is ShearingKnowledgeQuest)
                    {
                        if (!quest.Completed &&
                            (from.Map == Map.Trammel || from.Map == Map.Felucca))
                        {
                            from.AddToBackpack(new BritannianWool(1));
                        }
                        break;
                    }
                }
            }

            NextWoolTime = DateTime.UtcNow + TimeSpan.FromHours( 3.0 ); // TODO: Proper time delay
            return true;
        }

		public override void OnThink()
		{
			base.OnThink();
			if (CanCheckMilk() && m_CanMilk) CheckMilk();
			Body = ( DateTime.UtcNow >= m_NextWoolTime ) ? 0xCF : 0xDF;
		}

		public override void OnCarve( Mobile from, Corpse corpse, Item with)
		{
			Carve(from, corpse);
			
			base.OnCarve( from, corpse, with);
			double meatbonus = 1;
			if (m_MotherBreed == SheepBreed.Cotswold || m_FatherBreed == SheepBreed.Cotswold)
			{
				meatbonus += (IsPurebred() ? 1.50 : .75 );
			}
			if (m_MotherBreed == SheepBreed.Swaledale || m_FatherBreed == SheepBreed.Swaledale)
			{
				meatbonus += (IsPurebred() ? 2.0 : 1.0 );
			}
			if (m_MotherBreed == SheepBreed.Racka || m_FatherBreed == SheepBreed.Racka)
			{
				meatbonus += (IsPurebred() ? 2.0 : 1.0 );
			}
			if (m_MotherBreed == SheepBreed.Coopworth || m_FatherBreed == SheepBreed.Coopworth)
			{
				meatbonus += (IsPurebred() ? 1.20 : .60 );
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
			//if (((int)(meatbonus *2))>1) corpse.DropItem( new RawRibs((int)(meatbonus * 2)));
			//I have SheepHide on my shard
			corpse.DropItem( new SheepHide((int)meatbonus * 2) );
			//corpse.DropItem( new RawLambLeg( 4 ) );
			from.SendMessage( "you skin the sheep corpse" );
			corpse.Carved = true;
			DropResources(from, meatbonus, corpse);
		}
		
		public virtual void DropResources(Mobile from, double meatbonus, Corpse corpse)
		{
			bool CanUse = from.CheckSkillMult( SkillName.Anatomy, 30, 70 );
			if (meatbonus < 1) from.SendMessage("Nothing but grisle!");
			if (meatbonus > 0) corpse.DropItem( new RawLambLeg(2));
			if (meatbonus > 1) corpse.DropItem( new RawMuttonSteak(2));
			if (meatbonus > 2 && CanUse) corpse.DropItem( new RawMuttonRoast(2));
			if (meatbonus > 3) corpse.DropItem( new RawMuttonSteak(2));
		}  

		public override FoodType FavoriteFood{ get{ return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

		public override int Wool{ get{ return (Body == 0xCF ? 3 : 0); } }

		public FarmSheep( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 2 );

			writer.Write((int)m_LambFatherBreed);
			writer.Write((int)m_MotherBreed);
			writer.Write((int)m_FatherBreed);
			writer.Write((int)m_Milk);
			writer.Write(m_LastMilking);
			writer.Write((bool)m_CanMilk);
			
			writer.WriteDeltaTime( m_NextWoolTime );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 2:
				{
					m_LambFatherBreed = (SheepBreed)reader.ReadInt();
					m_MotherBreed = (SheepBreed)reader.ReadInt();
					m_FatherBreed = (SheepBreed)reader.ReadInt();
					m_Milk = reader.ReadInt();
					m_LastMilking = reader.ReadDateTime();
					m_CanMilk = reader.ReadBool();
					goto case 1;
				}
				case 1:
				{
					NextWoolTime = reader.ReadDeltaTime();
					break;
				}
			}
		}
	}
}
