using System;
using Server.Mobiles;
using Server.Items;

namespace Server.Mobiles
{
	public enum HorseBreed
	{
		//dappled brown fine mane 200, dappled grey fine mane 226
		//dark brown wild mane 204, tan wild mane 228 
		Andalusian, //war horse, solid color
		Arabian, //fine mane, solid color
		Appaloosa, //spotted
		Haflinger, //Wild mane solid color
		Thoroughbred, //fine mane, racing
		Hackney, //fine mane, solid color
	}
	
	[CorpseName( "a horse corpse" )]
	//[TypeAlias( "Server.Mobiles.BrownHorse", "Server.Mobiles.DirtyHorse", "Server.Mobiles.GrayHorse", "Server.Mobiles.TanHorse" )]
	public class WildHorse : BaseAnimal
	{
        private bool m_BardingExceptional;
        private Mobile m_BardingCrafter;
        private int m_BardingHP;
        private bool m_HasBarding;
        private CraftResource m_BardingResource;
        public Body bodyVal;
        public int idVal;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile BardingCrafter
        {
            get { return m_BardingCrafter; }
            set { m_BardingCrafter = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool BardingExceptional
        {
            get { return m_BardingExceptional; }
            set { m_BardingExceptional = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BardingHP
        {
            get { return m_BardingHP; }
            set { m_BardingHP = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasBarding
        {
            get { return m_HasBarding; }
            set
            {
                m_HasBarding = value;

                if (m_HasBarding)
                {
                    Hue = CraftResources.GetHue(m_BardingResource);
                    BodyValue = 284;
                }
                else
                {
                    Hue = 0;
                    BodyValue = bodyVal;
                }

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource BardingResource
        {
            get { return m_BardingResource; }
            set
            {
                m_BardingResource = value;

                if (m_HasBarding)
                    Hue = CraftResources.GetHue(value);

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BardingMaxHP
        {
            get { return m_BardingExceptional ? 2500 : 1000; }
        }

        private static int[] m_IDs = new int[]
		{
			0xC8, 0x3E9F,
			0xE2, 0x3EA0,
			0xE4, 0x3EA1,
			0xCC, 0x3EA2
		};
		
		private HorseBreed m_MotherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public HorseBreed MotherBreed
		{
			get { return m_MotherBreed; }
			set {	m_MotherBreed = value; }
		}
	
		private HorseBreed m_FatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public HorseBreed FatherBreed
		{
			get { return m_FatherBreed; }
			set {	m_FatherBreed = value; }
		}
		
		private HorseBreed m_FoalFatherBreed;
		[CommandProperty( AccessLevel.GameMaster )]
		public HorseBreed FoalFatherBreed
		{
			get { return m_FoalFatherBreed; }
			set {	m_FoalFatherBreed = value; }
		}

		[Constructable]
		public WildHorse( ) : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Eats = EatType.Herbivore;
			Grouping = GroupingType.Herd;
			Activity = ActivityType.Day;
			Animal = AnimalType.Farm;
			
			MateActive = true;
			MaxAge = 80;
			MatingSeason = Seasons.Any;
			
			int random = Utility.Random( 4 );

            bodyVal = this.Body;
            //idVal = this.ItemID;

            Name = "a horse";
			Body = m_IDs[random * 2];
			//ItemID = m_IDs[random * 2 + 1];
			BaseSoundID = 0xA8;

			SetStr( 22, 98 );
			SetDex( 56, 75 );
			SetInt( 6, 10 );

			SetHits( 28, 45 );
			SetMana( 0 );

			SetDamage( 3, 4 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 15, 20 );

			SetSkill( SkillName.MagicResist, 25.1, 30.0 );
			SetSkill( SkillName.Tactics, 29.3, 44.0 );
			SetSkill( SkillName.Wrestling, 29.3, 44.0 );

			Fame = 300;
			Karma = 300;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 29.1;
		}

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_HasBarding && m_BardingExceptional && m_BardingCrafter != null)
                list.Add(1060853, m_BardingCrafter.Name); // armor exceptionally crafted by ~1_val~
        }

        public void CalculateBonuses()
		{
			if (Age == AgeDescription.Young)
			{
				RawStr += (Utility.RandomMinMax(10,15));
				RawDex += (Utility.RandomMinMax(10,30));
				RawInt += (Utility.RandomMinMax(1,3));
				MinTameSkill += (Utility.RandomMinMax(1,3));
				if (m_MotherBreed == HorseBreed.Andalusian || m_FatherBreed == HorseBreed.Andalusian)
				{
					double sb = (double)(Utility.RandomMinMax(5,15));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
					RawStr += (Utility.RandomMinMax(15,25))/(IsPurebred()?1:2);
					RawDex += (Utility.RandomMinMax(10,20))/(IsPurebred()?1:2);
					DamageMax += IsPurebred()?3:1;
				}
				DamageMax += 1;
			}
			else if (Age == AgeDescription.Adult)
			{
				RawStr += (Utility.RandomMinMax(10,20));
				RawDex += (Utility.RandomMinMax(20,30));
				RawInt += (Utility.RandomMinMax(2,5));
				MinTameSkill += (Utility.RandomMinMax(1,5));
				if (m_MotherBreed == HorseBreed.Andalusian || m_FatherBreed == HorseBreed.Andalusian)
				{
					double sb = (double)(Utility.RandomMinMax(15, 30));
					SetSkill( SkillName.Tactics, IsPurebred() ? (sb * 2): sb );
					SetSkill( SkillName.Wrestling, IsPurebred() ? (sb * 2): sb );
					RawStr += (Utility.RandomMinMax(20,55))/(IsPurebred()?1:2);
					RawDex += (Utility.RandomMinMax(10,30))/(IsPurebred()?1:2);
					DamageMax += IsPurebred()?4:2;
				}
				DamageMax += 2;
			}
			else if (Age == AgeDescription.Senior)
			{
				RawStr -= (Utility.RandomMinMax(5,20));
				RawDex -= (Utility.RandomMinMax(5,20));
				RawInt += (Utility.RandomMinMax(2,5));
				MinTameSkill -= (Utility.RandomMinMax(2,5));
				DamageMax -= 2;
			}
		}

		public virtual void DetermineBreed(HorseBreed cb)
		{
			if (m_MotherBreed == m_FatherBreed) this.Title = "["+m_MotherBreed+"]";
			else this.Title = "[Mestico]";
			if (cb == HorseBreed.Andalusian)
			{
				Body = 204;
				Hue = Utility.RandomList(1855, 1856, 1857, 1858, 1859, 1860, 1861, 1862, 1863);//raw sienna
			}
			else if (cb == HorseBreed.Arabian)
			{
				Body = 226;
				Hue = Utility.RandomList(2308, 2309, 2310, 2311, 2312);//raw umber
			}
			else if (cb == HorseBreed.Appaloosa)
			{
				Body = 200;
				Hue = Utility.RandomList(2313, 2314, 2315, 2316, 2317, 2318); //burnt sienna
			}
			else if (cb == HorseBreed.Haflinger)
			{
				Body = 228;
				Hue = Utility.RandomList(1882, 1883, 1884, 1885, 1886, 1887, 1888, 1889, 1890);//bland brown
			}
			else if (cb == HorseBreed.Thoroughbred)
			{
				Body = 200;
				Hue = Utility.RandomList(2500, 2301, 2302, 2303, 2304, 2305, 2306);//grays
			}
			else if (cb == HorseBreed.Hackney)
			{
				Body = 226;
				Hue = Utility.RandomList(1900, 1901, 1902, 1903, 1904, 1905, 1906, 1907, 1908);//gray blue
			}
		}
		
		public override void BreedInformation(Mobile mate)
		{
			if (mate is WildHorse)
			{
				WildHorse wh = (WildHorse) mate;
				if (Utility.RandomBool()) 
				{
					wh.FoalFatherBreed = m_FatherBreed;
					//this.Say("passes on "+m_FatherBreed);
				}
				else 
				{
					wh.FoalFatherBreed = m_MotherBreed;
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
			Name = (this.Female ? "a mare": "a stallion");
			m_MotherBreed = (HorseBreed) Utility.Random(6);
			m_FatherBreed = (HorseBreed) Utility.Random(6);
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);
			TypeName = "a horse";
		}
		
		public override void MakeBaby()
		{
			if (Utility.RandomBool()) DetermineBreed(m_MotherBreed);
			else DetermineBreed(m_FatherBreed);			
			SetStr( 2, 6 );
			SetDex( 2, 6 );
			SetInt( 2, 4 );
			SetDamage( 1 );
			MinTameSkill = 1.1;
			Name = "a foal";
			base.MakeBaby();
		}
		
		public override void MakeYoung()
		{
			CalculateBonuses();
			if (Name == "a foal") 
				Name = (this.Female ? "a filly": "a colt");
			base.MakeYoung();
		}
		
		public override void MakeAdult()
		{
			CalculateBonuses();
			if (Name == "a filly" || Name == "a colt" || Name == "a foal") 
				Name = (this.Female ? "a mare": "a stallion");
			base.MakeAdult();
		}
		
		public override void MakeSenior()
		{
			CalculateBonuses();
			if (Name == "a mare" || Name == "a stallion" || Name == "a filly" || Name == "a colt" || Name == "a foal") 
				Name = (this.Female ? "a mare": "a stallion");
			base.MakeSenior();
		}
		
		public override void CreateBaby()
		{
			for (int j = 0; j < this.LitterSize; j++) 
			{
				WildHorse foal = new WildHorse();
				foal.Female = ((Utility.RandomMinMax(1,4) == 1) ? false: true);
				foal.Mother = this;
				foal.Spawned = false;
				foal.TypeName = "a horse";
				if (Utility.RandomBool()) 
				{
					foal.MotherBreed = m_MotherBreed;
					//this.Say("passes on "+m_MotherBreed);
				}
				else 
				{
					foal.MotherBreed = m_FatherBreed;
					//this.Say("passes on "+m_FatherBreed);
				}
				foal.FatherBreed = m_FoalFatherBreed;
				foal.MakeBaby();
				if (this.Brand != "" && this.Brand != null) foal.Owner = this.Owner;
				foal.MoveToWorld(this.Location, this.Map);
			}
			base.CreateBaby();
		}
		
		public override int Meat{ get{ return 3; } }
		public override int Hides{ get{ return 10; } }
		public override FoodType FavoriteFood{ get{ return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

		public WildHorse( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 2 ); // version
			
			writer.Write((int)m_FoalFatherBreed);
			writer.Write((int)m_MotherBreed);
			writer.Write((int)m_FatherBreed);

            writer.Write((bool)m_BardingExceptional);
            writer.Write((Mobile)m_BardingCrafter);
            writer.Write((bool)m_HasBarding);
            writer.Write((int)m_BardingHP);
            writer.Write((int)m_BardingResource);
        }

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
			switch (version)
			{
                case 2:
                    {
                        m_BardingExceptional = reader.ReadBool();
                        m_BardingCrafter = reader.ReadMobile();
                        m_HasBarding = reader.ReadBool();
                        m_BardingHP = reader.ReadInt();
                        m_BardingResource = (CraftResource)reader.ReadInt();
                        goto case 1;
                    }
                case 1:
				{
					m_FoalFatherBreed = (HorseBreed)reader.ReadInt();
					m_MotherBreed = (HorseBreed)reader.ReadInt();
					m_FatherBreed = (HorseBreed)reader.ReadInt();
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