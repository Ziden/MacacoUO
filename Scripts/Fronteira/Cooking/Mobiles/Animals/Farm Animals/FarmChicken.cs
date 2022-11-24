using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
	public enum ChickenBreed
	{
		Leghorn, //white egglayer
		Barnevelder, //brown egglayer
		Bresse, //white meat
		Braekel, //golden meat
		Orpington, //black meat/egglayer
		Poltava, //orange meat/egglayer
	}
	
	[CorpseName( "a chicken corpse" )]
	public class FarmChicken : BaseAnimal //BaseCreature
	{
		private ChickenBreed m_MotherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public ChickenBreed MotherBreed
		{
			get { return m_MotherBreed; }
			set {	m_MotherBreed = value; }
		}
	
		private ChickenBreed m_FatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public ChickenBreed FatherBreed
		{
			get { return m_FatherBreed; }
			set {	m_FatherBreed = value; }
		}
		
		private ChickenBreed m_ChickFatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public ChickenBreed ChickFatherBreed
		{
			get { return m_ChickFatherBreed; }
			set {	m_ChickFatherBreed = value; }
		}
		
		private AnimalNest m_Nest;
		[CommandProperty( AccessLevel.GameMaster )]
		public AnimalNest Nest
		{
			get { return m_Nest; }
			set {	m_Nest = value; }
		}

		private bool m_HasLaid;
		[CommandProperty( AccessLevel.GameMaster )]
		public bool HasLaid
		{
			get { return m_HasLaid; }
			set {	m_HasLaid = value; }
		}
		
		[Constructable]
		public FarmChicken() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Eats = EatType.Herbivore;
			Animal = AnimalType.Farm;
			Grouping = GroupingType.Flock;

			MateActive = true;
			MaxAge = 20;
            LitterSize = 2;
			MatingSeason = Seasons.Any;
            Hunger = 100;
			
			Name = "a chicken";
			Body = 0xD0;
			BaseSoundID = 0x6E;

			SetStr( 5 );
			SetDex( 15 );
			SetInt( 5 );

			SetHits( 3 );
			SetMana( 0 );

			SetDamage( 1 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 1, 5 );

			SetSkill( SkillName.MagicResist, 4.0 );
			SetSkill( SkillName.Tactics, 5.0 );
			SetSkill( SkillName.Wrestling, 5.0 );

			Fame = 150;
			Karma = 0;

			VirtualArmor = 2;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = -0.9;
		}

		public void CalculateBonuses()
		{
			if (Age == AgeDescription.Young)
			{
				RawStr += (Utility.RandomMinMax(0,2));
				RawDex += (Utility.RandomMinMax(0,2));
				RawInt += (Utility.RandomMinMax(0,2));
				MinTameSkill += (Utility.RandomMinMax(1,3));
				if (m_MotherBreed == ChickenBreed.Bresse || m_FatherBreed == ChickenBreed.Bresse)
				{
					double sb = (double)(Utility.RandomMinMax(10,20));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
					DamageMax += IsPurebred() ? 2: 1;
				}
				if (m_MotherBreed == ChickenBreed.Braekel || m_FatherBreed == ChickenBreed.Braekel)
				{
					double sb = (double)(Utility.RandomMinMax(5,10));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
					DamageMax += IsPurebred() ? 1: 0;
				}
				DamageMax += 1;
			}
			else if (Age == AgeDescription.Adult)
			{
				RawStr += (Utility.RandomMinMax(0,5));
				RawDex += (Utility.RandomMinMax(0,10));
				RawInt += (Utility.RandomMinMax(0,3));
				MinTameSkill += (Utility.RandomMinMax(1,5));
				if (m_MotherBreed == ChickenBreed.Bresse || m_FatherBreed == ChickenBreed.Bresse)
				{
					double sb = (double)(Utility.RandomMinMax(15,30));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
					DamageMax += IsPurebred() ? 3: 2;
				}
				if (m_MotherBreed == ChickenBreed.Braekel || m_FatherBreed == ChickenBreed.Braekel)
				{
					double sb = (double)(Utility.RandomMinMax(10,20));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
					DamageMax += IsPurebred() ? 2: 1;
				}
				DamageMax += 1;
			}
			else if (Age == AgeDescription.Senior)
			{
				RawStr -= (Utility.RandomMinMax(1,2));
				RawDex -= (Utility.RandomMinMax(1,2));
				RawInt += (Utility.RandomMinMax(0,2));
				MinTameSkill -= (Utility.RandomMinMax(2,5));
				DamageMax -= 1;
			}
		}

		public virtual void DetermineBreed(ChickenBreed cb)
		{
			if (m_MotherBreed == m_FatherBreed) this.Title = "["+m_MotherBreed+"]";
			else this.Title = "[Mestico]";
			if (cb == ChickenBreed.Leghorn)
			{
				Hue = 2500;
			}
			else if (cb == ChickenBreed.Barnevelder)
			{
				Hue = 2110;
			}
			else if (cb == ChickenBreed.Bresse)
			{
				Hue = 2500;
			}
			else if (cb == ChickenBreed.Braekel)
			{
				Hue = 1281;
			}
			else if (cb == ChickenBreed.Orpington)
			{
				Hue = 2106;
			}
			else if (cb == ChickenBreed.Poltava)
			{
				Hue = 0;
			}
		}
		
		public override void BreedInformation(Mobile mate)
		{
			if (mate is FarmChicken)
			{
				FarmChicken s = (FarmChicken) mate;
				if (Utility.RandomBool()) 
				{
					s.ChickFatherBreed = m_FatherBreed;
					//this.Say("passes on "+m_FatherBreed);
				}
				else 
				{
					s.ChickFatherBreed = m_MotherBreed;
					//this.Say("passes on "+m_MotherBreed); //was DebugSay
				}
			}
		}
		
		public bool IsPurebred()
		{
			if (m_MotherBreed == m_FatherBreed) return true;
			return false;
		}
        public virtual void OnAdded(Mobile parent)
        {
            Name = (Female ? "a hen" : "a rooster");
            m_MotherBreed = (ChickenBreed)Utility.Random(6);
            m_FatherBreed = (ChickenBreed)Utility.Random(6);
            if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
            else DetermineBreed(m_FatherBreed);
            TypeName = "a chicken";
            //if (parent is Mobile)
            //((Mobile)parent).Name = (Female ? "a hen" : "a rooster");
        }

        public override void OnAfterSpawn()
		{
			base.OnAfterSpawn();
			Name = (Female ? "a hen": "a rooster");
			m_MotherBreed = (ChickenBreed) Utility.Random(6);
			m_FatherBreed = (ChickenBreed) Utility.Random(6);
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);
			TypeName = "a chicken";
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
			Name = "a chick";
			if (m_Nest != null)
			{
				AnimalNest anest = (AnimalNest) m_Nest;
				Home = anest.Location;
				RangeHome = 2;
			}
			base.MakeBaby();
		}
		
		public override void MakeYoung()
		{
			CalculateBonuses();
			if (Name == "a chick") 
				Name = (this.Female ? "a pullet": "a cockerel");
			base.MakeYoung();
		}
		
		public override void MakeAdult()
		{
			CalculateBonuses();
			if (Name == "a pullet" || Name == "a cockerel" || Name == "a chick") 
				Name = (Female ? "a hen": "a rooster");
			base.MakeAdult();
		}
		
		public override void MakeSenior()
		{
			CalculateBonuses();
			if (Name == "a hen" || Name == "a rooster" || Name == "a pullet" || Name == "a cockerel" || Name == "a chick") 
				Name = (Female ? "a hen": "a rooster");
			base.MakeSenior();
		}
		
		public override void CreateBaby()
		{
			if (m_HasLaid)
			{
				for (int j = 0; j < this.LitterSize; j++) 
				{
					FarmChicken chick = new FarmChicken();
					chick.Female = ((Utility.RandomMinMax(1,4) == 1) ? false: true);
					chick.Mother = this;
					chick.Spawned = false;
					chick.TypeName = "a chicken";
					if (Utility.RandomBool()) 
					{
						chick.MotherBreed = m_MotherBreed;
						//this.Say("passes on "+m_MotherBreed);
					}
					else 
					{
						chick.MotherBreed = m_FatherBreed;
						//this.Say("passes on "+m_FatherBreed);
					}
					chick.FatherBreed = ChickFatherBreed;
					chick.MakeBaby();
					chick.Owner = this.Owner;
					chick.Brand = this.Brand;
					if (m_Nest != null)
					{
						AnimalNest anest = (AnimalNest) m_Nest;
						chick.Nest = anest;
						chick.MoveToWorld(anest.Location, anest.Map);
						anest.HasEggs -= 1;
						anest.InvalidateProperties();
						if (anest.HasEggs < 1) anest.ItemID = 6869;
					}
					else chick.MoveToWorld(Location, Map);
				}
				base.CreateBaby();
			}
			else LayEggs();
		}
	
		private DateTime m_LastLaidCheck = DateTime.UtcNow;
		[ CommandProperty( AccessLevel.GameMaster ) ]
		public DateTime LastLaidCheck
		{
			get { return m_LastLaidCheck; }
			set { m_LastLaidCheck = value; }
		}
		
		private TimeSpan LaidCheckDelay = TimeSpan.FromMinutes((720.0/12));
		public virtual bool CanCheckLaid()
		{
			if (m_LastLaidCheck.Add(LaidCheckDelay) < DateTime.UtcNow)
				return true;
			return false;
		}

		public virtual void LayEggs()
		{
			double eggbonus = 0;
			if (m_MotherBreed == ChickenBreed.Leghorn || m_FatherBreed == ChickenBreed.Leghorn)
			{
				eggbonus += (IsPurebred() ? 3.0 : 1.5 );
			}
			if (m_MotherBreed == ChickenBreed.Barnevelder || m_FatherBreed == ChickenBreed.Barnevelder)
			{
				eggbonus += (IsPurebred() ? 3.0 : 1.5 );
			}
			if (m_MotherBreed == ChickenBreed.Orpington || m_FatherBreed == ChickenBreed.Orpington)
			{
				eggbonus += (IsPurebred() ? 2.0 : 1.0 );
			}
			if (m_MotherBreed == ChickenBreed.Poltava || m_FatherBreed == ChickenBreed.Poltava)
			{
				eggbonus += (IsPurebred() ? 2.0 : 1.0 );
			}
			if (m_MotherBreed == ChickenBreed.Bresse || m_FatherBreed == ChickenBreed.Bresse)
			{
				eggbonus += (IsPurebred() ? 1.0 : .50 );
			}
			if (m_MotherBreed == ChickenBreed.Braekel || m_FatherBreed == ChickenBreed.Braekel)
			{
				eggbonus += (IsPurebred() ? 1.0 : .50 );
			}
			
			if (m_Nest != null)
			{
				if (m_Nest.HasEggs + (int)eggbonus < 10) m_Nest.HasEggs += (int)eggbonus;
				else m_Nest.HasEggs = 10;
				m_Nest.ItemID = 6868;
				m_HasLaid = true;
				AnimalNest anest = (AnimalNest) m_Nest;
				Home = anest.Location;
				RangeHome = 0;
				anest.InvalidateProperties();
				LastLaidCheck = DateTime.UtcNow;
			}
			else
			{
				MakeNest();
				if (m_Nest.HasEggs + (int)eggbonus < 10) m_Nest.HasEggs += (int)eggbonus;
				else m_Nest.HasEggs = 10;
				m_Nest.ItemID = 6868;
				m_HasLaid = true;
				AnimalNest anest = (AnimalNest) m_Nest;
				Home = anest.Location;
				RangeHome = 0;
				anest.InvalidateProperties();
				LastLaidCheck = DateTime.UtcNow;
			}
		}
		
		public virtual void MakeNest()
		{
			AnimalNest anest = new AnimalNest();
			anest.ItemID = 6869;
			anest.Owner = this;
			m_Nest = anest;
			anest.MoveToWorld(Location, Map);
		}

		public override void OnThink()
		{
			base.OnThink();
			
			if (!Blessed)
			{
				if (CanCheckLaid()) m_HasLaid = false;
				if (CanMate() && m_Nest == null && Female) MakeNest();
				if (IsPregnant && !m_HasLaid) LayEggs();
				if (Female && !m_HasLaid && Age == AgeDescription.Adult) 
				{
					if (m_Nest != null)
					{
						AnimalNest anest = (AnimalNest) m_Nest;
						Home = anest.Location;
						RangeHome = 0;
						if (anest.Location == Location) LayEggs();
					}
					else LayEggs();
				}
				if (Female && HasOffspring)
				{
					if (m_Nest != null)
					{
						AnimalNest anest = (AnimalNest) m_Nest;
						Home = anest.Location;
						RangeHome = 2;
					}
				}
			}
			if (m_Nest != null)
			{
				if (m_Nest.Deleted || m_Nest.Movable) m_Nest = null;
			}
		}
		
		public override void OnDelete()
        {
			if (m_Nest != null)
			{
				AnimalNest an = (AnimalNest) m_Nest;
				if (an.HasEggs > 0)
				{
					an.Movable = true;
					an.Owner = null;
				}
				else an.Delete();
			}
            base.OnDelete();
        }

		public override int Meat{ get{ return 1; } }
		public override MeatType MeatType{ get{ return MeatType.Chicken; } }

        public override void OnCarve(Mobile from, Corpse corpse, Item with)
        {
            base.OnCarve(from, corpse, with);
            if (m_MotherBreed == ChickenBreed.Bresse || m_FatherBreed == ChickenBreed.Bresse)
			{
				//corpse.DropItem( new RawChickenLeg( 2 ) );
			}
			if (m_MotherBreed == ChickenBreed.Braekel || m_FatherBreed == ChickenBreed.Braekel)
			{
				//corpse.DropItem( new RawChickenLeg( 2 ) );
			}
			if (m_MotherBreed == ChickenBreed.Orpington || m_FatherBreed == ChickenBreed.Orpington)
			{
				//corpse.DropItem( new RawChickenLeg( 2 ) );
			}
			if (m_MotherBreed == ChickenBreed.Poltava || m_FatherBreed == ChickenBreed.Poltava)
			{
				
			}
			/*
			if (Age == AgeDescription.Baby)
			{
			}
			else if (Age == AgeDescription.Young)
			{
			}
			else if (Age == AgeDescription.Adult)
			{
			}
			else if (Age == AgeDescription.Senior)
			{
			}
			*/
            corpse.Carved = true;
			corpse.DropItem( new RawChickenLeg( 2 ) );
		}

		public override FoodType FavoriteFood{ get{ return FoodType.GrainsAndHay; } }

		public override int Feathers{ get{ return 25; } }

        public FarmChicken(Serial serial)
            : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 2);
			writer.Write(m_LastLaidCheck);

			writer.Write((int)m_ChickFatherBreed);
			writer.Write((int)m_MotherBreed);
			writer.Write((int)m_FatherBreed);
			writer.Write(m_Nest);
			writer.Write( (bool) m_HasLaid);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 2:
				{
					m_LastLaidCheck = reader.ReadDateTime();
					goto case 1;
				}
				case 1:
				{
					m_ChickFatherBreed = (ChickenBreed)reader.ReadInt();
					m_MotherBreed = (ChickenBreed)reader.ReadInt();
					m_FatherBreed = (ChickenBreed)reader.ReadInt();
					m_Nest = (AnimalNest)reader.ReadItem();
					m_HasLaid = reader.ReadBool();
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
