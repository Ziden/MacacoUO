#define cropsystem
using System;
using System.Collections.Generic;
using Server.Items;
using Server.Regions;
#if cropsystem
using Server.Items.Crops;
#endif

namespace Server.Mobiles
{
    public enum ActivityType
	{
		Any, //Metaturnal
		Day, //Diurnal
		Twilight, //Crepuscular
		Night, //Nocturnal
		Dawn, //Matutinal
		Dusk, //Vespertine
	}

	public enum EatType
	{
		Herbivore,
		Carnivore,
		Omnivore
	}

	public enum AnimalType
	{
		Wild,
		Town,
		Farm,
		Vermin
	}

	public enum GroupingType
	{
		None,
		Herd,
		Pack,
		Hive,
		Flock,
		MatedPair
	}
	
	public enum Seasons
	{
		Any,
		Spring,
		Summer,
		Autumn,
		Winter
	}
	
	public enum AgeDescription
	{
		Baby, // quarter points
		Young, // half points
		Adult, // whole points
		Senior // half points
	}

	public class BaseAnimal : BaseCreature
	{
		private SleepingAnimal m_AnimalBody = null;
		[CommandProperty( AccessLevel.GameMaster )]
		public SleepingAnimal AnimalBody
		{
			get { return m_AnimalBody; }
			set {	m_AnimalBody = value; }
		}
		
		public override bool CanOpenDoors{ get{ return false; } }
		
		private string m_TypeName = null;
		[CommandProperty( AccessLevel.GameMaster )]
		public string TypeName
		{
			get { return m_TypeName; }
			set { m_TypeName = value; }
		}
		
		private string m_Brand = null;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Brand
		{
			get { return m_Brand; }
			set { m_Brand = value; }
		}
		
		private Mobile m_Owner;
		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner
		{
			get { return m_Owner; }
			set { m_Owner = value; }
		}
		
		private Mobile m_Prey; //this is for checking how long a predator has been after a prey
		//[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Prey
		{
			get { return m_Prey; }
			set { m_Prey = value; }
		}
		
		private Item m_CheckFoodItem; //this is for checking how long an animal has been after a food item
		//[CommandProperty(AccessLevel.GameMaster)]
		public Item CheckFoodItem
		{
			get { return m_CheckFoodItem; }
			set { m_CheckFoodItem = value; }
		}
		
		private bool m_MateActive = false;
		[CommandProperty(AccessLevel.GameMaster)]
		public bool MateActive
		{
			get { return m_MateActive; }
			set { m_MateActive = value; }
		}
		
		private int m_LitterSize = 1;
		[CommandProperty(AccessLevel.GameMaster)]
		public int LitterSize
		{
			get { return m_LitterSize; }
			set { m_LitterSize = value; }
		}
		
		private bool m_Spawned = true;
		//[CommandProperty(AccessLevel.GameMaster)]
		public bool Spawned
		{
			get { return m_Spawned; }
			set { m_Spawned = value; }
		}
		
		private AgeDescription m_Age = AgeDescription.Adult;
		[CommandProperty(AccessLevel.GameMaster)]
		public AgeDescription Age
		{
			get { return m_Age; }
			set { m_Age = value; }
		}
	
		private int m_MaxAge = 30;
		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxAge
		{
			get { return m_MaxAge; }
			set { m_MaxAge = value; }
		}
		
		private Seasons m_MatingSeason;
		[CommandProperty(AccessLevel.GameMaster)]
		public Seasons MatingSeason
		{
			get { return m_MatingSeason; }
			set { m_MatingSeason = value; }
		}
		
		private bool m_IsPregnant;
		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsPregnant
		{
			get { return m_IsPregnant; }
			set { m_IsPregnant = value; }
		}
		
		private bool m_HasOffspring;
		[CommandProperty(AccessLevel.GameMaster)]		
		public bool HasOffspring
		{
			get { return m_HasOffspring; }
			set { m_HasOffspring = value; }
		}
	
		private Mobile m_Mother;
		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Mother
		{
			get { return m_Mother; }
			set { m_Mother = value; }
		}
		
		private bool m_Sneaking;		
		public bool Sneaking {get{return m_Sneaking;} set{m_Sneaking = value;}}
		
		private bool m_Feeding;		
		public bool Feeding	{get{return m_Feeding;} set{m_Feeding = value;}}
		
		private bool m_Update = false;
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Update {get{return m_Update;} set{m_Update = value;}}

		private Item m_FoodItem;		
		[CommandProperty(AccessLevel.GameMaster)]
		public Item FoodItem
		{
			get { return m_FoodItem; }
			set { m_FoodItem = value; }
		}
		
		private bool m_ForcedActive;
		public bool ForcedActive
		{
			get { return m_ForcedActive; }
			set { m_ForcedActive = value; }
		}

		private Mobile m_LastSurprise;
		public Mobile LastSurprise
		{
			get { return m_LastSurprise; }
			set { m_LastSurprise = value; }
		}

		private int m_Rank;
		//[CommandProperty(AccessLevel.GameMaster)]
		public int Rank
		{
			get { return m_Rank; }
			set { m_Rank = value; }
		}

		private bool m_Ambush;
		//[CommandProperty(AccessLevel.GameMaster)]
		public bool Ambush
		{
			get { return m_Ambush; }
			set { m_Ambush = value; }
		}

		private bool m_Hibernate;
		//[CommandProperty(AccessLevel.GameMaster)]
		public bool Hibernate
		{
			get { return m_Hibernate; }
			set { m_Hibernate = value; }
		}

		private int m_Size;
		[CommandProperty(AccessLevel.GameMaster)]
		public int Size
		{
			get { return m_Size; }
			set { m_Size = value; }
		}

		private ActivityType m_Activity = ActivityType.Day;
		[CommandProperty(AccessLevel.GameMaster)]
		public ActivityType Activity
		{
			get { return m_Activity; }
			set { m_Activity = value; }
		}

		private EatType m_Eats = EatType.Omnivore;
		[CommandProperty(AccessLevel.GameMaster)]
		public EatType Eats
		{
			get { return m_Eats; }
			set { m_Eats = value; }
		}

		private AnimalType m_Animal = AnimalType.Wild;
		[CommandProperty(AccessLevel.GameMaster)]
		public AnimalType Animal
		{
			get { return m_Animal; }
			set { m_Animal = value; }
		}

		private GroupingType m_Grouping = GroupingType.None;
		[CommandProperty(AccessLevel.GameMaster)]
		public GroupingType Grouping
		{
			get { return m_Grouping; }
			set { m_Grouping = value; }
		}

		private int m_HungerRange = 10;
		[CommandProperty(AccessLevel.GameMaster)]
		public int HungerRange
		{
			get { return m_HungerRange; }
			set { m_HungerRange = value; }
		}

		private bool m_Scavenger;// If set to true in the animals script than that animal will not Hunt
		//[CommandProperty(AccessLevel.GameMaster)]
		public bool Scavenger
		{
			get { return m_Scavenger; }
			set { m_Scavenger = value; }
		}

		private Mobile m_Mate; //used to make Mated pairs for animals like eagles,crows,squiruls, ferrets, etc.Kinda of a replacemet of packleader
		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Mate
		{
			get { return m_Mate; }
			set { m_Mate = value; }
		}

		private Mobile m_Predator;// this is here to pass the Predator on to the pack and for AI usage
		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Predator
		{
			get { return m_Predator; }
			set { m_Predator = value; }
		}

		private Mobile m_PackLeader;// homerange/home is centered on the packleader and helps with findfood and Predator/AI 
		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile PackLeader
		{
			get { return m_PackLeader; }
			set { m_PackLeader = value; }
		}

		private bool m_FruitEater; // If the critter is herb than true = only eats fruit, if its carn than true = allowed to eat fruit with there meat.
		//[CommandProperty(AccessLevel.GameMaster)]
		public bool FruitEater
		{
			get { return m_FruitEater; }
			set { m_FruitEater = value; }
		}

		private bool m_GrainEater;// If Set to true the animal will only eat Grains and grasses.
		//[CommandProperty(AccessLevel.GameMaster)]
		public bool GrainEater
		{
			get { return m_GrainEater; }
			set { m_GrainEater = value; }
		}
		
		//protected override BaseAI ForcedAI { get { return new WildAnimalAI( this ); } }

		public BaseAnimal() : base( AIType.AI_Animal, FightMode.Aggressor, 15, 1, 0.2, 0.4 )
		{
		}

		public BaseAnimal(AIType ai, FightMode mode, int iRangePerception, int iRangeFight, double dActiveSpeed, double dPassiveSpeed): base (ai, mode, iRangePerception, iRangeFight, dActiveSpeed, dPassiveSpeed)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if ((from.AccessLevel >= AccessLevel.GameMaster) && !ForcedActive)
			{ 
				ForcedActive = true;
				Awake();
				NameMod = Name + " (Forcado)";
			}
			else if ((from.AccessLevel >= AccessLevel.GameMaster) && ForcedActive)
			{
				ForcedActive = false;
				NameMod = null;
			}
			else base.OnDoubleClick(from);
		}

		protected override bool OnMove( Direction d )
		{
			if (!m_Sneaking) RevealingAction();
			return true;
		}
		
		public virtual bool CropFood(Item item)
		{
			if (item is Grasses || item is BaseAnimalFlowers)
				return true;
			else if (item is FarmableWheat)
				return true;
			#if cropsystem
			else if (item is WheatCrop || item is SoyCrop || item is OatsCrop || item is HayCrop )
				return true;
			#endif
			return false;
		}

		public virtual bool FruitFood(Item item)
		{
			if (item is Grasses) return true;
			if (item is FruitBasket || item is Banana || item is Bananas || item is Lemon || item is Lemons || item is Lime || item is Limes || item is Dates)
				return true;
			else if (item is Grapes || item is Peach || item is Pear || item is Apple || item is Watermelon || item is SmallWatermelon || item is Cantaloupe)
				return true;
			#if cropsystem
			else if (item is WatermelonCrop || item is HoneydewMelonCrop || item is StrawberryCrop || item is CucumberCrop || item is CantaloupeCrop )
				return true;
			#endif
			return false;
		}

		public virtual bool VegetableFood(Item item)
		{
			if (item is Grasses || item is BaseAnimalMushrooms) 
				return true;
			else if (item is Squash || item is OpenCoconut || item is Coconut || item is SplitCoconut || item is Carrot || item is Cabbage || item is Onion || item is Lettuce || item is Pumpkin || item is SmallPumpkin)
				return true;
			else if (item is FarmableCarrot || item is FarmableOnion || item is FarmableTurnip || item is FarmablePumpkin || item is FarmableCabbage || item is FarmableLettuce)
				return true;
			#if cropsystem
			else if (item is GreenSquashCrop || item is FieldCornCrop || item is SquashCrop || item is CornCrop || item is PumpkinCrop || item is CeleryCrop)
				return true;
			else if (item is CauliflowerCrop || item is BroccoliCrop || item is BeetCrop || item is OnionCrop || item is RadishCrop || item is TurnipCrop)
				return true;
			else if (item is SpinachCrop || item is SnowPeasCrop || item is PeasCrop || item is LettuceCrop || item is GreenBeanCrop || item is CabbageCrop || item is AsparagusCrop)
				return true;
			#endif
			return false;
		}

		public virtual bool MeatFood(Item item)
		{
			if (item is RawRibs || item is RawLambLeg || item is RawChickenLeg || item is RawBird || item is UnbakedMeatPie || item is Eggs || item is RawFishSteak )
				return true;
			else if (item is Bacon || item is SlabOfBacon || item is FishSteak || item is CookedBird || item is RoastPig || item is Sausage || item is Ham || item is Ribs || item is MeatPie || item is LambLeg || item is ChickenLeg || item is ChickenLeg)
				return true;
			return false;
		}

		public virtual bool CanEatIt(Item item)
		{
			if (item != null)
			{
				if (item is Corpse || item is Remains || MeatFood(item))
				{
					if (this.Eats == EatType.Carnivore || this.Eats == EatType.Omnivore)
						return true;
				}
				if (CropFood(item))
				{
					if (this.Eats == EatType.Herbivore)
					{
						if (this.FruitEater == true) return false;
						else return true;
					}
					else if (this.Eats == EatType.Omnivore)
					{
						if (this.FruitEater == true) return false;
						else return true;
					}
				}
				if (VegetableFood(item))
				{
					if (this.Eats == EatType.Herbivore)
					{
						if (this.FruitEater == true) return false;
						else if (this.GrainEater == true) return false;
						else return true;
					}
					else if (this.Eats == EatType.Omnivore)
					{
						//not likey a Omnivore will be a fruit/grain only eater but just in case. 
						if (this.FruitEater == true) return false;
						else if (this.GrainEater == true) return false;
						else return true;
					}
				}
				if (FruitFood(item))
				{
					if (this.Eats == EatType.Herbivore)
					{
						if (this.GrainEater == true) return false;
						else return true;
					}
					else if (this.Eats == EatType.Omnivore)
					{
						if (this.GrainEater == true) return false;
						else return true;
					}
					//this should allow bears ( or any carni with fruiteater set to true) to eat fruit, 
					//and not affect their other eating habits.
					else if (this.Eats == EatType.Carnivore)
					{
                        if (this.FruitEater == true)
                        {
                            return true;
                        }
                        else return false;
					}
				}
				if (item is BaseFood && item.Movable)
				{
					if (this.Eats == EatType.Omnivore) return true;
				}
				return false;
			}
			return false;
		}

		//FindFood is alot different, added Eat(item) and Hunt() so i could understand the order/behavior.
		public virtual void FindFood()
		{
            if (Hunger < (m_Size / 2))
            {
                m_HungerRange = 20;
            }
            else if (Hunger >= (m_Size / 2))
            {
                m_HungerRange = 10;
            }

			if (m_FoodItem == null && Combatant == null)
			{
				DebugSay("looking for food");
				List<Item> list = new List<Item>();
				bool foodaround = false;
				foreach (Item item in GetItemsInRange(m_HungerRange))
				{
					if (CanEatIt(item))
					{
						list.Add( item );
						foodaround = true;
					}
				}
				if (foodaround)
				{
					Item food = list[Utility.Random(list.Count)];
					if (food != m_CheckFoodItem)
					{
						m_FoodItem = food;
						m_CheckFoodItem = food;
						LastFoodCheck = DateTime.UtcNow;
						Eat(food);
					}
				}
				if ((m_Grouping == GroupingType.Pack || m_Grouping == GroupingType.MatedPair) && m_FoodItem != null)
				{
					this.DebugSay("Calling friends to help eat this");
					CallThePack(null, m_FoodItem);
				}
				// if it can't find food and it's hungry enough it will hunt
				if ( m_Eats != EatType.Herbivore && m_Scavenger == false && Hunger < m_Size / 2 )
				{
                    this.DebugSay("can't find food so I will Hunt");
					Hunt(); //changed the combat stuff from onMovement to Hunt
				}
				else DebugSay("can't find food");
			}
		}

		public virtual void Hunt()
		{
			if (!IsHungry()) return;
			if (m_FoodItem != null) return;

            if (Hunger < 10)
            {
                m_HungerRange = 20;
            }
            else if (Hunger >= 10)
            {
                m_HungerRange = 10;
            }

            this.DebugSay("Starting to hunt");
            foreach (Mobile mob in GetMobilesInRange(m_HungerRange))
			{
				if (mob != null && mob != m_Prey)
				{
					if (!mob.Hidden && InLOS(mob))
					{
						if (Combatant == null && IsPrey(mob) && m_Eats != EatType.Herbivore && m_Scavenger == false && FoodItem == null)
						{
							if (Hunger <= m_Size/2)
							{
								DebugSay("Found prey");
								Combatant = mob;
								m_Prey = mob;
								LastPreyCheck = DateTime.UtcNow;
								if (m_Grouping == GroupingType.Pack)
								{
									DebugSay("Found prey and calling the pack");
									CallThePack(mob, null);
								}

								if (m_Ambush == true)
								{
									DebugSay("starting ambush");
									Surprised(mob);
								}
							}

							// should only kill things it can eat all of unless it can call the pack or it's really hungry
							else if (mob is BaseAnimal && Hunger > m_Size/2)
							{
								BaseAnimal ba = (BaseAnimal)mob;
								if (m_Grouping != GroupingType.None)
								{
									DebugSay("Found big prey and calling pack");
									Combatant = ba;
									CallThePack(ba, null);

									if (m_Ambush == true)
									{
										DebugSay("starting ambush");
										Surprised(ba);
									}
								}
								else if ((ba.Size - (ba.Size / 5)) < (m_Size - Hunger))
								{
									DebugSay("Found prey");
									Combatant = ba;

									if (m_Ambush == true)
									{
										DebugSay("starting ambush");
										Surprised(ba);
									}
								}
							}
						}
					}
				}
			}
		}

		public virtual void Surprised(Mobile mob)
		{
			double HunterRange = GetDistanceToSqrt(mob);
			bool StrikeRange; 
			bool ShortRange;
			StrikeRange = false; 
			ShortRange = false;
            if (HunterRange < 2)
            {
                StrikeRange = true;
            }
            else if ((HunterRange >= 2) && (HunterRange < 10))
            {
                ShortRange = true;
            }

			if(ShortRange && (mob != LastSurprise))
			{
				//this.Hidden = true;
				//this.Sneaking = true;
			}
			else if(StrikeRange)
			{
				this.Sneaking = false;
				LastSurprise = mob;
			}
			else this.Sneaking = false;
		}

		public virtual void CallThePack(Mobile attacking, Item feeding)
		{
			if ((m_Grouping == GroupingType.Pack) || (m_Grouping == GroupingType.Herd)) 
			{
				DebugSay("looking for pack");
				foreach (Mobile mob in GetMobilesInRange(20))
				{
					if ((mob is BaseAnimal))
					{
						BaseAnimal ba = (BaseAnimal)mob;
						if (ba.TypeName == m_TypeName && !Blessed && !mob.Blessed)
						{
							DebugSay("found pack");
							if (attacking != null && feeding == null)
							{
								if (ba.Combatant == null)
								ba.Combatant = attacking;
								/*	if (this.PackLeader == null)
								{
									ba.Combatant = attacking;
								}
								else if (ba.PackLeader != null)
								{
									if (ba.PackLeader is BaseAnimal)
									{
										BaseAnimal bapl = (BaseAnimal)ba.PackLeader;
										if (bapl.Combatant == null)
										{
											bapl.Combatant = attacking;
											ba.Combatant = attacking;
										}
										else
										{
											ba.Combatant = bapl.Combatant;
										}
									}
								} */
							}
							else if (ba.Combatant == null && ba.FoodItem == null && attacking == null && ba.Hunger < ba.Size && feeding != null)
							{
								ba.DebugSay("coming to pack call to eat");
								DebugSay("Calling pack to Eat");
								ba.Home = feeding.Location;
								ba.RangeHome = 0;
								ba.FoodItem = feeding;
							}
						}
					}
				}
			}
		}
		
		public virtual void MoveRandomly(Item item)
		{
			//this is a module to perform the menial task of placing corpse items randomly around the remains
			int mover = Utility.RandomMinMax(1,4);
			if (mover == 1)
			{
				item.X += Utility.RandomMinMax(-1,1);
			}
			else if (mover == 2)
			{
				item.Y += Utility.RandomMinMax(-1,1);
			}
			else if (mover == 3)
			{
				item.X += Utility.RandomMinMax(-1,1);
				item.Y += Utility.RandomMinMax(-1,1);
			}
		}

		public virtual void Eat(Item food)
		{
			Item foodie = (Item) food;
			if (m_FoodItem == null && CanEatIt(food) && IsHungry())
			{
				if (m_PackLeader == null)
				{
					m_FoodItem = food;
				}
				else if (m_PackLeader != null)
				{
					BaseAnimal ba = (BaseAnimal)m_PackLeader;
					if (ba.FoodItem != null)
					{
						m_FoodItem = ba.FoodItem;
					}
					else if (ba.FoodItem == null)
					{
						ba.FoodItem = food;
						m_FoodItem = food;
					}
				}
			}
			if (m_FoodItem != null && !Blessed)
			{
				if (!InRange(m_FoodItem, 1) && InRange(m_FoodItem, m_HungerRange))
				{
					Home = FoodItem.Location;
					RangeHome = 0;
					DebugSay("Heading to food");
					
					if (m_PackLeader != null && m_PackLeader != this)
					{
						BaseAnimal ba = (BaseAnimal)m_PackLeader;
						if (ba.FoodItem != null)
						{
							Item plfood = (Item)ba.FoodItem;
							if (!InRange(plfood, 1) && InRange(plfood, m_HungerRange))
							{
								Home = plfood.Location;
								RangeHome = 0;
							}
							else if (!InRange(ba, 1) && InRange(ba, 20))
							{
								Home = ba.Location;
								RangeHome = 0;
							}
							else
							{
								RangeHome = 17;
							}
						}
					}
				}
				if (InRange(FoodItem, 1) && CanCheckEat())
				{
					LastEatCheck = DateTime.UtcNow;
					//FoodItem = food;
					if (FoodItem is Corpse)
					{
						this.DebugSay("eating corpse");
						Corpse cor = (Corpse)FoodItem;
						if (cor.Owner is PlayerMobile)
						{
							Remains remTorso = new Remains();
							remTorso.CreationTime = cor.TimeOfDeath;
							remTorso.Owner = cor.Owner;
							remTorso.Killer = cor.Killer;
							remTorso.ItemID = 0x1D9F;
							remTorso.RemainsAmount = 4;
							remTorso.Name = cor.Owner.Name;
							remTorso.MoveToWorld(cor.Location, cor.Map);
							MoveRandomly(remTorso);
                            //If you have bounty system you can uncomment this.
                            //BountiedHead head = new BountiedHead(String.Format("the head of {0}", cor.Owner.Name));
							//head.Owner = cor.Owner;
							//head.Name = cor.Owner.Name;
							//head.Killer = cor.Killer;
							//head.CreationTime = cor.TimeOfDeath;
							//head.MoveToWorld(cor.Location, cor.Map);
							//MoveRandomly(head);

                            //var bh = new BountiedHead(string.Format("the head of {0}", cor.Owner.Name))
                            //{
                            //    Player = Owner as PlayerMobile,
                            //    Bounty = BountyInformation.GetBounty(Owner),
                            //    Created = DateTime.UtcNow
                            //};
                            //bh.MoveToWorld(cor.Location, cor.Map);
                            //MoveRandomly(bh);

                            Remains remLArm = new Remains();
							remLArm.CreationTime = cor.TimeOfDeath;
							remLArm.Owner = cor.Owner;
							remLArm.Killer = cor.Killer;
							remLArm.ItemID = 0x1DA1;
							remLArm.RemainsAmount = 1;
							remLArm.Name = cor.Owner.Name;
							remLArm.MoveToWorld(cor.Location, cor.Map);
							MoveRandomly(remLArm);

							Remains remRArm = new Remains();
							remRArm.CreationTime = cor.TimeOfDeath;
							remRArm.Owner = cor.Owner;
							remRArm.Killer = cor.Killer;
							remRArm.ItemID = 0x1DA2;
							remRArm.RemainsAmount = 1;
							remRArm.Name = cor.Owner.Name;
							remRArm.MoveToWorld(cor.Location, cor.Map);
							MoveRandomly(remRArm);

							Remains remLLeg = new Remains();
							remLLeg.CreationTime = cor.TimeOfDeath;
							remLLeg.Owner = cor.Owner;
							remLLeg.Killer = cor.Killer;
							remLLeg.ItemID = 0x1DA3;
							remLLeg.RemainsAmount = 2;
							remLLeg.Name = cor.Owner.Name;
							remLLeg.MoveToWorld(cor.Location, cor.Map);
							MoveRandomly(remLLeg);

							Remains remRLeg = new Remains();
							remRLeg.CreationTime = cor.TimeOfDeath;
							remRLeg.Owner = cor.Owner;
							remRLeg.Killer = cor.Killer;
							remRLeg.ItemID = 0x1DA4;
							remRLeg.RemainsAmount = 2;
							remRLeg.Name = cor.Owner.Name;
							remRLeg.MoveToWorld(cor.Location, cor.Map);
							MoveRandomly(remRLeg);

							List<Item> items = new List<Item>(cor.Items);
							Backpack bag = new Backpack();
							for (int i = 0; i < items.Count; ++i)
							{
								Item item = items[i];
								if (((item.Layer == Layer.Hair) || (item.Layer == Layer.FacialHair)) || !item.Movable)
									continue;
								if ((item is BaseClothing) || (item is BaseArmor) || (item is BaseWeapon) || (item is BaseFood))
								{
									item.MoveToWorld(cor.Location, cor.Map);
									MoveRandomly(item);
								}
								else bag.DropItem(item);
							}
							bag.MoveToWorld(cor.Location, cor.Map);
							MoveRandomly(bag);
							new Blood(0x122D).MoveToWorld(cor.Location, cor.Map);
							foodie.Delete();
							this.Hunger += 1;
							FoodItem = null;
						}
						else if (cor.Owner is BaseAnimal)
						{
							this.DebugSay("eating corpse");
							Remains rema = new Remains();
							rema.CreationTime = cor.TimeOfDeath;
							rema.Owner = cor.Owner;
							rema.Killer = cor.Killer;
							rema.Hue = cor.Hue;
							BaseAnimal ba = (BaseAnimal)cor.Owner;
							rema.RemainsAmount = (ba.Size - (ba.Size / 5 ) );
							rema.Name = ba.Name;
							rema.ItemID = 0x2006;
							rema.Amount = ba.Body;
							rema.MoveToWorld(cor.Location, cor.Map);
							new Blood(0x122D).MoveToWorld(cor.Location, cor.Map);
							foodie.Delete();
							this.Hunger += 1;
							FoodItem = null;
						}
						else
						{
							this.DebugSay("eating corpse");
							Remains rema = new Remains();
							rema.CreationTime = cor.TimeOfDeath;
							rema.Owner = cor.Owner;
							rema.Killer = cor.Killer;
							rema.RemainsAmount = 10;
							if (cor.Owner != null) rema.Name = cor.Owner.Name;
							else rema.Name = cor.Name; //"remains";
							rema.MoveToWorld(cor.Location, cor.Map);
							List<Item> items = new List<Item>(cor.Items);
							if (items.Count > 0)
							{
								Backpack bag = new Backpack();
								for (int i = 0; i < items.Count; ++i)
								{
									Item item = items[i];
									if (((item.Layer == Layer.Hair) || (item.Layer == Layer.FacialHair)) || !item.Movable)
										continue;
									if ((item is BaseClothing) || (item is BaseArmor) || (item is BaseWeapon) || (item is BaseFood))
									{
										item.MoveToWorld(cor.Location, cor.Map);
										MoveRandomly(item);
									}
									else bag.DropItem(item);
								}
								bag.MoveToWorld(cor.Location, cor.Map);
								MoveRandomly(bag);
							}
							new Blood(0x122D).MoveToWorld(cor.Location, cor.Map);
							foodie.Delete();
							this.Hunger += 1;
							FoodItem = null;
						}
					}
					else if (FoodItem is Remains)
					{
						this.DebugSay("eating remains");
						Remains rema = (Remains)FoodItem;
						if (rema.RemainsAmount < 1)
						{
							// I don't think you can check the remains owner to know if it's town or farm or what so I figured this would work for now
							if (this.Region != null)
							{
								if (this.Region is GuardedRegion)
								{
									switch (Utility.Random(2))
									{
									case 1:
										{
											BaseAnimalMushrooms shrooms = new BaseAnimalMushrooms();
											shrooms.Location = rema.Location;
											shrooms.BaseAnimalMushroomsAmount = 25;
											shrooms.Map = rema.Map;
											shrooms.MoveToWorld(rema.Location, rema.Map);

											foodie.Delete();
											FoodItem = null;
											return;
										}
									case 2:
										{
											BaseAnimalFlowers flowers = new BaseAnimalFlowers();
											flowers.Location = rema.Location;
											flowers.BaseAnimalFlowersAmount = 25;
											flowers.Map = rema.Map;
											flowers.MoveToWorld(rema.Location, rema.Map);

											foodie.Delete();
											FoodItem = null;
											return;
										}
									}
								}
								else if (this.Region is DungeonRegion)
								{
									BaseAnimalMushrooms shrooms = new BaseAnimalMushrooms();
									shrooms.Location = rema.Location;
									shrooms.BaseAnimalMushroomsAmount = 25;
									shrooms.Map = rema.Map;
									shrooms.MoveToWorld(rema.Location, rema.Map);

									foodie.Delete();
									FoodItem = null;

								}
								else
								{
									switch (Utility.Random(3))
									{
									case 0:
										{
											Grasses grass = new Grasses();
											grass.Location = rema.Location;
											grass.GrassesAmount = 25;
											grass.Map = rema.Map;
											grass.MoveToWorld(rema.Location, rema.Map);

											foodie.Delete();
											FoodItem = null;
											return;
										}
									case 1:
										{
											BaseAnimalMushrooms shrooms = new BaseAnimalMushrooms();
											shrooms.Location = rema.Location;
											shrooms.BaseAnimalMushroomsAmount = 25;
											shrooms.Map = rema.Map;
											shrooms.MoveToWorld(rema.Location, rema.Map);

											foodie.Delete();
											FoodItem = null;
											return;
										}
									case 2:
										{
											BaseAnimalFlowers flowers = new BaseAnimalFlowers();
											flowers.Location = rema.Location;
											flowers.BaseAnimalFlowersAmount = 25;
											flowers.Map = rema.Map;
											flowers.MoveToWorld(rema.Location, rema.Map);

											foodie.Delete();
											FoodItem = null;
											return;
										}
									}
								}
							}
							else
							{
								switch (Utility.Random(3))
								{
								case 0:
									{
										Grasses grass = new Grasses();
										grass.Location = rema.Location;
										grass.GrassesAmount = 25;
										grass.Map = rema.Map;
										grass.MoveToWorld(rema.Location, rema.Map);

										foodie.Delete();
										FoodItem = null;
										return;
									}
								case 1:
									{
										BaseAnimalMushrooms shrooms = new BaseAnimalMushrooms();
										shrooms.Location = rema.Location;
										shrooms.BaseAnimalMushroomsAmount = 25;
										shrooms.Map = rema.Map;
										shrooms.MoveToWorld(rema.Location, rema.Map);

										foodie.Delete();
										FoodItem = null;
										return;
									}
								case 2:
									{
										BaseAnimalFlowers flowers = new BaseAnimalFlowers();
										flowers.Location = rema.Location;
										flowers.BaseAnimalFlowersAmount = 25;
										flowers.Map = rema.Map;
										flowers.MoveToWorld(rema.Location, rema.Map);

										foodie.Delete();
										FoodItem = null;
										return;
									}
								}
							}
						}
						else
						{
							rema.RemainsAmount -= 1;
							this.Hunger += 1;
						}
					}
					else if (FoodItem is Grasses)
					{
						Grasses grass = (Grasses)FoodItem;
						if (grass.GrassesAmount < 1)
						{
							foodie.Delete();
							FoodItem = null;
						}
						else
						{
							grass.GrassesAmount -= 1;
							this.Hunger += 1;
							//FoodItem = null;
						}
					}
					else if (FoodItem is BaseAnimalFlowers)
					{
						BaseAnimalFlowers flower = (BaseAnimalFlowers)FoodItem;
						if (flower.BaseAnimalFlowersAmount < 1)
						{
							foodie.Delete();
							FoodItem = null;
						}
						else
						{
							flower.BaseAnimalFlowersAmount -= 1;
							this.Hunger += 1;
							//FoodItem = null;
						}
					}
					//I noticed that Farmablecrops were not geting eaten.
					else if (FoodItem is BaseFarmable)
					{
						Item fi = (Item)FoodItem;
						this.Hunger += 4;
						foodie.Delete();
						FoodItem = null;
					}
					#if cropsystem
					else if (FoodItem is Server.Items.Crops.HerdingBaseCrop)
					{
						Item fi = (Item)FoodItem;
						this.Hunger += 4;
						foodie.Delete();
						FoodItem = null;
					}
					#endif
					else if (FoodItem is BaseFood)
					{
						this.DebugSay("eating food");
						BaseFood fi = (BaseFood)FoodItem;
						this.Hunger += 1;
						// I thought i would be cool to be able to poison food and leave it around as bait or to get rid of pests eating your crops.
						if (fi.Poison != null)
						{
							this.ApplyPoison(fi.Poisoner, fi.Poison);
						}
						if (foodie.Amount > 1) foodie.Amount -= 1;
						else foodie.Delete();
						FoodItem = null;
					}
					//I wanted to be able to feed the carnies some raw meat.
					else if (FoodItem is BaseCookableFood)
					{
						this.DebugSay("eating food");
						Item fi = (Item)FoodItem;
						this.Hunger += 1;
						if (foodie.Amount > 1) foodie.Amount -= 1;
						else foodie.Delete();
						FoodItem = null;
					}
					else if (FoodItem is BaseAnimalMushrooms)
					{
						this.DebugSay("eating mushrooms");
						BaseAnimalMushrooms shrooms = (BaseAnimalMushrooms)FoodItem;
						if (shrooms.BaseAnimalMushroomsAmount < 1)
						{
							foodie.Delete();
							FoodItem = null;
						}
						else
						{
							shrooms.BaseAnimalMushroomsAmount -= 1;
							this.Hunger += 1;	
						}
					}
					else if (FoodItem is Item)
					{
						this.DebugSay("eating misc item");
						Item misc = (Item)FoodItem;
						misc.Delete();
						FoodItem = null;
						this.Hunger += 1;	
					}
				}
			}
			else
			FoodItem = null;
		}

		public virtual bool IsIndependant()
		{
			if ( Summoned || Controlled ) return false;
			else return true;
		}
		
		public virtual void FormPack()
		{
			DebugSay("Forming pack");
			Mobile mob = null;
			if ((this.Grouping == GroupingType.Pack) || (this.Grouping == GroupingType.Herd))
			{
				//Looks for PackLeader
				foreach (Mobile m in this.GetMobilesInRange(20))
				{
					if (m is BaseAnimal)
					{
						BaseAnimal ba = (BaseAnimal)m;
						if (ba.TypeName == this.TypeName && !this.Blessed && !m.Blessed)
						{
							if (this.PackLeader == null)
							{
								if (ba.PackLeader == null || ba.PackLeader == m)
								{
									if (ba.Rank > this.Rank)
									mob = m;
									else if(ba.Rank == this.Rank)
									{
										if (this.Size > ba.Size) 
										{
											this.Rank += 1;
											mob = this;
										}
										else if (ba.Size > this.Size)
										{
											mob = m;
											ba.Rank += 1;
										}
									}
								}
								else if (ba.PackLeader != null && ba.PackLeader != m)
								{
									BaseAnimal pl = (BaseAnimal)ba.PackLeader;
                                    if (pl.Rank > this.Rank)
                                    {
                                        mob = ba.PackLeader;
                                    }
                                    else if (pl.Rank <= this.Rank)
                                    {
                                        ba.FormPack();
                                    }
								}
							}
							if (mob == null) mob = this;
							/*
							else if ( this.PackLeader != null)
							{
								BaseAnimal packl = (BaseAnimal)this.PackLeader;
								if (!InRange(packl.Location, 30) || packl.Deleted || !packl.Alive)
								{
									this.PackLeader = null;
								}
								else if (this.Rank > packl.Rank)
								{
									this.PackLeader = null;
								}
							}*/
							if (PackLeader != null)
							{
								BaseAnimal pl = (BaseAnimal)PackLeader;
								BaseAnimal bmob = (BaseAnimal)mob;
								if (bmob.Rank > pl.Rank) PackLeader = mob;
								else if(bmob.Rank == pl.Rank)
								{
									if (bmob.Size < pl.Size) 
									{
										pl.Rank += 1;
									}
									else if (bmob.Size > pl.Size)
									{
										PackLeader = mob;
										bmob.Rank += 1;
									}
								}
							}
							else PackLeader = mob;
						}
					}
				}
			}
		}
		
		// made this mainly for IsPrey, if the pack can be called it will hunt larger animals 
		public virtual bool CanCallPack()
		{
            if (m_Grouping == GroupingType.None)
            {
                return false;
            }
			foreach (Mobile mob in GetMobilesInRange(10))
			{
				if (mob is BaseAnimal)
				{
					BaseAnimal ba = (BaseAnimal)mob;
					if (ba.TypeName == m_TypeName && !Blessed && !mob.Blessed)
					{
						if (m_PackLeader == null)
						{
							return true;
						}
						else if (m_PackLeader != null)
						{
							if (ba.Predator == null && ba.FoodItem == null)
							{
								return true;
							}
						}
					}
					else return false;
				}
				return false;
			}
			return false;
		}
		
		public override void OnMovement(Mobile m, Point3D oldLocation)
		{
			if (IsIndependant())
			{
				if ((FightMode != FightMode.Aggressor && FightMode != FightMode.None) || (!Female && IsMatingSeason() && CanMate()))
                {
					base.OnMovement(m, oldLocation);
					if ((!m.Hidden || m.AccessLevel <= AccessLevel.VIP) && Combatant == null )
					{
						if ( InRange( m, 10) && !InRange( oldLocation, 1 ) && InLOS( m ))
						{
							if ((Combatant == null) && IsEnemy(m))
                                Combatant = m;
						}
					}
				}

				if (!m.Hidden && IsPredator(m) && InRange(m.Location, 9) && !InRange(m.Location, 15))
				{
					if (m_Grouping != GroupingType.None)
					{
						m_Predator = m;
						if (m_PackLeader == null || PackLeader == this)
						{
							foreach (Mobile mob in GetMobilesInRange(15))
							{
								if (mob is BaseAnimal)
								{
									BaseAnimal ba = (BaseAnimal)mob;
									if (ba.TypeName == m_TypeName && !Blessed && !mob.Blessed)
									{
										if (ba.Predator == null)
										{
											ba.Predator = m;
										}
									}
								}
							}
						}
					}
					else
					{
						m_Predator = m;
					}
				}

				if (m_Grouping == GroupingType.None)
				{
					m_PackLeader = null;
					//this.Mate = null;
				}
				
				// Looks for Predator Shorter range if it's eating, I figured it would be distracted while eating and a predator could get closer
				if (!m.Hidden && m_Predator == null && Combatant == null && IsPredator(m))
				{
					if (m_FoodItem == null)
					{
						// this range needs to be smaller than to Ambush Range otherwise it will always flee before the predator hides
						if (!Hidden && InRange(m, 9) && !InRange(oldLocation, 1) && InLOS(m))
						{
							m_Predator = m;
						}
					}
					else if (m_FoodItem != null)
					{
						if (Hunger < (m_Size / 2))
						{
							// if it is really hungry it has a really short range
							if (!Hidden && InRange(m, 3) && !InRange(oldLocation, 1) && InLOS(m))
							{
								m_Predator = m;
							}
						}
						else
						{
							if (!Hidden && InRange(m, 6) && !InRange(oldLocation, 1) && InLOS(m))
							{
								m_Predator = m;
							}
						}
					}
				}
			}
			else
			{
                Shard.Debug("Animal Movement +");
                var inRange = InRange(m, 10);
                var moved = !InRange(oldLocation, 1);
                if ( inRange && moved && InLOS( m ) && !m.Hidden)
				{
                    var isEnemy = IsEnemy(m);
                    if ((Combatant == null) && isEnemy) Combatant = m;
				}
			}
			base.OnMovement(m, oldLocation);
		}
		
		public override bool IsEnemy( Mobile m )
		{
			if (m is PlayerMobile) 
			{
				PlayerMobile pm = (PlayerMobile) m;
				Mobile dm = FindMostRecentDamager( true );
                //if (MobileExtensions.IsStaff(pm))
                //{
                //    return false;
                //}
                if (pm == dm)
                {
                    return true;
                }
                else if ((m == SummonMaster) || (m == ControlMaster) || (m == m_Owner))
                {
                    return false;
                }

                if (m_Eats == EatType.Herbivore)
                {
                    return false;
                }
			}

			if (m is BaseAnimal)
			{
				if (IsCompetition(m))
                    return true;
			}

            if (!IsIndependant())
            {
                return false;
            }

			return base.IsEnemy(m);
		}
	
		public bool IsCompetition(Mobile mob)
		{
			if (mob is BaseAnimal)
			{
				BaseAnimal ba = (BaseAnimal) mob;
                if (!IsMatingSeason())
                {
                    return false;
                }
                if (!CanMate() || !ba.CanMate())
                {
                    return false;
                }
                if (ba.Female || Female)
                {
                    return false;
                }

                if (ba.Size > m_Size + 2 || ba.Size < m_Size - 2)
                {
                    return false;
                }

                if (ba.Rank != m_Rank)
                {
                    return false;
                }

                if (ba.TypeName != m_TypeName)
                {
                    return false;
                }
			}
			DebugSay("Competition!!");
			return true;
		}
		
		public override void AlterMeleeDamageFrom( Mobile from, ref int damage )
		{
			if(from is BaseAnimal)
			{
				BaseAnimal ba = (BaseAnimal) from;
				if (IsCompetition(from))
				{
					damage = 1;
					if (Hits < HitsMax)
					{
						m_Rank -= 1;
						ba.Rank += 1;
						Combatant = null;
						ba.Combatant = null;
					}
				}
			}
		}

		public virtual bool IsPredator( Mobile m)
		{
            //if (MobileExtensions.IsStaff(m))
            //{
            //    return false;
            //}

            if (m == m_Owner)
            {
                return false;
            }

			if (m is BaseAnimal && !m.Blessed)
			{
				BaseAnimal ba = (BaseAnimal)m;
                if (ba.IsPrey(this))
                {
                    return false;//true
                }
                else
                {
                    return true;//false
                }
			}
			else if (m is PlayerMobile)
			{
                if (m.Skills[SkillName.AnimalTaming].Value > 50)
                {
                    return false;
                }

                if ((m_Animal == AnimalType.Town))
                {
                    return false;
                }

                if ((m_Animal == AnimalType.Farm))
                {
                    return false;
                }

                if ((m_Animal == AnimalType.Vermin))
                {
                    return true;
                }

				if (m_Animal == AnimalType.Wild)
				{
                    if ((m_Eats == EatType.Herbivore) && (m_Size < m.Str))
                    {
                        return true;
                    }
                    else if ((m_Eats != EatType.Herbivore) && (m_Size < m.Str / 2))
                    {
                        return true;
                    }
				}
			}
			return false;
		}

		public virtual bool IsPrey( Mobile m )
		{
            // I redid some of this to fix some weird fights I was seeing (like a brown and black bear fighting) and keep thing from attacking players that were much stronger.
            //if (MobileExtensions.IsStaff(m))
            //{
            //    return false;
            //}

            if (m == m_Owner)
            {
                return false;
            }

			if (m is BaseAnimal && !m.Blessed)
			{
				BaseAnimal ba = (BaseAnimal)m;
				if (m_TypeName != ba.TypeName)
				{
					if (m_Eats != EatType.Herbivore && m_Scavenger == false )
					{
						if (m_Animal == AnimalType.Town)
						{
                            if (ba.Animal == AnimalType.Vermin && (m_Size > ba.Size))
                            {
                                return true;
                            }
                            else if ((ba.Animal == AnimalType.Town) && (m_Size > ba.Size))
                            {
                                if (Utility.Random(10) > 6)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if ((ba.Animal == AnimalType.Wild) && (m_Size > ba.Size + 6))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
						}
						else if (m_Animal == AnimalType.Wild)
						{
                            if (ba.Eats == EatType.Herbivore)
                            {
                                if (m_Size > ba.Size + 4) return true;
                                else if (m_Size <= (ba.Size + 4))
                                {
                                    DebugSay("checking for help from pack");
                                    if (m_Grouping == GroupingType.Pack && CanCallPack())
                                    {
                                        if (m_Size + 4 >= ba.Size) return true;
                                    }
                                    if (m_Grouping == GroupingType.MatedPair)
                                    {
                                        if (m_Mate != null && m_Size + 2 >= ba.Size) return true;
                                    }
                                    return false;
                                }
                            }
                            else if (m_Size > ba.Size + 6)
                            {
                                return true;
                            }
                            else if (m_Size <= (ba.Size + 6))
                            {
                                DebugSay("checking for help from pack");
                                if (m_Grouping == GroupingType.Pack && CanCallPack())
                                {
                                    if ((m_Size + 6 >= ba.Size)) return true;
                                }
                                if (m_Grouping == GroupingType.MatedPair)
                                {
                                    if (m_Mate != null && m_Size + 2 >= ba.Size) return true;
                                }
                                return false;
                            }
						}
					}
					return false;
				}
				return false;
			}
			else if (m is PlayerMobile)
			{
				if ((m_Animal == AnimalType.Wild) && m_Eats != EatType.Herbivore && m_Scavenger == false)
				{
					if (m_Size > m.Str / 2) return true;
					if (m_Grouping == GroupingType.Pack && CanCallPack()) return true;
					return false;
				}
				return false;
			}
			else if (m is BaseCreature)
			{
				BaseCreature bc = (BaseCreature) m;
				if ((bc is BaseHealer) || (bc is BaseEscortable)) return false;
				else if (bc.FightMode == FightMode.None) return false;
				else if ((bc.Str / 2 + 10) < m_Size + 6) return true;
				else return false;
			}
			return false;
		}

		public override void OnAfterSpawn()
		{
			m_TypeName = Name;
			
			//A mobile's HitsMax is determined by (50 + (Str / 2))
			Size = (int)(Math.Round(((double)this.Str / 2 )+ 10));
			Rank = Utility.RandomMinMax(0, 5);
			
			Female = ((Utility.RandomMinMax(1,4) == 1) ? false: true);
			
			m_HungerRange = 15;
			Hunger = 20;
			m_Spawned = true;
			this.MakeAdult();
			base.OnAfterSpawn();
		}

        public virtual void Snoozing()//Asleep was old keyword but repalced because already used
		{
			this.AccessLevel = AccessLevel.GameMaster;
			this.Blessed = true;
			//this.Hidden = true;
			this.Sneaking = true;
			this.Predator = null;
			this.Combatant = null;// Had to add this, I saw Blessed animals trying to fight
			if (m_Mate != null && !Female)
			{
				RangeHome = 1;
				Home = m_Mate.Location;
			}
			if (this.PackLeader != null)
			{
				RangeHome = 5;
				Home = m_PackLeader.Location;
			}
			if (m_Animal == AnimalType.Farm && m_AnimalBody == null && m_Owner != null)
			{
				CantWalk = true;
				SleepingAnimal sa = new SleepingAnimal();
				sa.Hue = Hue;
				sa.Amount = Body;
				sa.Owner = this;
				sa.Name = Name + " (Dormindo)";
				m_AnimalBody = sa;
				sa.MoveToWorld(Location, Map);
			}
			if (NameMod == null) NameMod = Name + " (Descansando)";
		}

		public virtual void Awake()
		{
			this.AccessLevel = AccessLevel.Player;
			this.Blessed = false;
			this.Hidden = false;
			this.Sneaking = false;
			this.NameMod = null;
			if (m_AnimalBody != null)
			{
				CantWalk = false;
				m_AnimalBody.Delete();
				m_AnimalBody = null;
			}
		}
		
		public int AgeCheck()  //checks the age (in weeks)
		{
			TimeSpan CheckDifference = DateTime.UtcNow - this.CreationTime;
			return (int)((CheckDifference.TotalMinutes / 60 / 24 / 7)*12);
		}
		
		public virtual void MakeBaby()
		{
			m_Age = AgeDescription.Baby;
			m_Spawned = false;
		}
		
		public virtual void MakeYoung()
		{
			m_Age = AgeDescription.Young;
			m_Rank += 5;
			if (m_Mother != null && !m_Mother.Deleted)
			{
				BaseAnimal ma = (BaseAnimal) m_Mother;
				ma.HasOffspring = false;
				m_Mother = null;
			}
		}
		
		public virtual void MakeAdult()
		{
			m_Mother = null;
			m_Age = AgeDescription.Adult;
			m_Rank += 10;
		}
		
		public virtual void MakeSenior()
		{
			m_Age = AgeDescription.Senior;
			m_Rank -= 5;
		}
		
		public virtual void CreateBaby()
		{
			m_IsPregnant = false;
			m_HasOffspring = true;
			m_Rank += 1;
		}
		
		public bool CanGetToFood()
		{
			if (m_FoodItem == m_CheckFoodItem && CanCheckFood())
			{
				DebugSay("Can't get to it.");
				return false;
			}
			/*
			else if (m_FoodItem != m_CheckFoodItem && !CanCheckFood() && m_CheckFoodItem != null)
			{
				m_CheckFoodItem = m_FoodItem;
				DebugSay("Something New.");
				LastFoodCheck = DateTime.UtcNow;
				return true;
			}*/
			return true;
		}
		
		public bool CanGetToPrey()
		{
			if (this.Combatant == m_Prey && CanCheckPrey())
			{
				DebugSay("Can't get to it.");
				return false;
			}
			return true;
		}
		
		private DateTime LastFoodCheck = DateTime.UtcNow;
		private TimeSpan FoodCheckDelay = TimeSpan.FromMinutes( 2.0 );
		public bool CanCheckFood()
		{
			if( LastFoodCheck.Add( FoodCheckDelay ) < DateTime.UtcNow ) return true;
			return false;
		}
		
		private DateTime LastPreyCheck = DateTime.UtcNow;
		private TimeSpan PreyCheckDelay = TimeSpan.FromMinutes( 2.0 );
		public bool CanCheckPrey()
		{
			if( LastPreyCheck.Add( PreyCheckDelay ) < DateTime.UtcNow ) return true;
			return false;
		}
		
		private DateTime LastActiveCheck = DateTime.UtcNow;
		private TimeSpan ActiveCheckDelay = TimeSpan.FromMinutes( 5.0 );
		public bool CanCheckActive()
		{
			if( LastActiveCheck.Add( ActiveCheckDelay ) < DateTime.UtcNow ) return true;
			return false;
		}
		
		//This is just for a routein which runs every minute...to take load off onthink method
		private DateTime LastMinuteCheck = DateTime.UtcNow;
		private TimeSpan MinuteCheckDelay = TimeSpan.FromMinutes( 1.0 );
		public bool CanCheckMinute()
		{
			if( LastMinuteCheck.Add( MinuteCheckDelay ) < DateTime.UtcNow ) return true;
			return false;
		}

		//added this becuase the animals were eating too fast.
		private DateTime LastEatCheck = DateTime.UtcNow;
		private TimeSpan EatCheckDelay = TimeSpan.FromSeconds(5.0);
		public bool CanCheckEat()
		{
			if (LastEatCheck.Add(EatCheckDelay) < DateTime.UtcNow) return true;
			return false;
		}

		public virtual void CheckActive()
		{
			LastActiveCheck = DateTime.UtcNow;
			int hours, minutes;
			if (Blessed == true)
			{
				this.Hunger = ( this.Size / 4 );
				this.Combatant = null;
				this.Predator = null;
			}
			/*
			Any, (Metaturnal)
			Day, (Diurnal)
			Twilight, (Crepuscular)
			Night, (Nocturnal)
			Dawn, (Matutinal)
			Dusk, (Vespertine)
*/
			if (!(this.Region is DungeonRegion))  
			{
				Server.Items.Clock.GetTime(this.Map, this.X, this.Y, out hours, out minutes);
				if (Activity == ActivityType.Any)
				{
					Awake();
				}
				else if (Activity == ActivityType.Day)
				{
					if ((hours >= 6) && (hours < 21)) Awake();
					else Snoozing();
				}
				else if (Activity == ActivityType.Twilight)
				{
					if (((hours >= 4) && (hours < 10)) || ((hours >= 16) && (hours < 23))) Awake();
					else Snoozing();
				}
				else if (Activity == ActivityType.Night)
				{
					if (((hours >= 18) && (hours <= 23)) || ((hours >= 0) && (hours < 7))) Awake();
					else Snoozing();
				}
				else if (Activity == ActivityType.Dawn)
				{
					if ((hours >= 3) && (hours < 15)) Awake();
					else Snoozing();
				}
				else if (Activity == ActivityType.Dusk)
				{
					if ((hours >= 15) && (hours < 3)) Awake();
					else Snoozing();
				}
			}
			else Awake();
			
			if (Hibernate == true)
			{
				Map map;
				map = Map.AllMaps[1]; //2 = fall, 3 = winter
				if (map.Season == 2 || map.Season == 3 ) Snoozing();
				else Awake();
			}
		}
		
		public virtual void MinuteCheck() //Accesses less frequently to take load off processor.  General maintenance stuff
		{
			LastMinuteCheck = DateTime.UtcNow;
			//General Consistancy Check.  Make sure all animals are on same page.
			if (Size == 0) Size = (int)(Math.Round(((double)Str / 2) + 10 ));
			if (AI != AIType.AI_Animal) AI = AIType.AI_Animal;
			if (m_TypeName == null || m_TypeName == "") m_TypeName = Name;
			
			//Maintenance
			int age = AgeCheck();
			if (m_Age != AgeDescription.Baby && age < m_MaxAge * .1 && !m_Spawned && m_Mother != null) MakeBaby();
			else if (m_Age != AgeDescription.Young && age >= m_MaxAge * .1 && age < m_MaxAge * .3 && !m_Spawned) MakeYoung();
			else if (m_Age != AgeDescription.Adult && age >= m_MaxAge * .3 && age < m_MaxAge * .8 && !m_Spawned) MakeAdult();
			else if (m_Age != AgeDescription.Senior && age >= m_MaxAge * .8 && age <= m_MaxAge) MakeSenior();
			else Hits -= HitsMax / 50;
			// Wanted animals to die of hunger so if none could find food, some will die and the rest can eat them.
			if (Hunger >= 1) Hunger -= 1;
			if (Hunger < 1)
			{
				//Didn't want that ALL animals should die of hunger.  Grass everywhere and nothing to eat?  Or dungeons.
				if ((m_Animal != AnimalType.Town) && (m_Eats != EatType.Herbivore) && (!(Region is DungeonRegion)) && ((FightMode == FightMode.None) || (FightMode == FightMode.Aggressor)))
				{
					Hits -= HitsMax / 50;
					//this.Damage( this.HitsMax );
					if (Hits == 0) this.Kill();
				}
			}
			if (((m_Grouping == GroupingType.Pack) || (m_Grouping == GroupingType.Herd)) && m_PackLeader == null && m_Age != AgeDescription.Baby)
			{
				FormPack();
			}
			else if (((m_Grouping == GroupingType.Pack) || (m_Grouping == GroupingType.Herd)) && m_PackLeader != null && m_Age != AgeDescription.Baby)
			{
				double LeaderRange = GetDistanceToSqrt(m_PackLeader);
				if ((LeaderRange > 25) || m_PackLeader.Deleted || !m_PackLeader.Alive)
				{
					m_PackLeader = null;
					FormPack();
				}
			}
			//Reproduction
			if (m_IsPregnant && !Blessed)
			{
				//ToDo: different season...or time for gestation
				CreateBaby();
			}
			if (m_Mother != null)
			{
				BaseAnimal ba = (BaseAnimal) m_Mother;
				ba.HasOffspring = true;
			}
		}
		
		public virtual void FindMate()
		{
			DebugSay("Finding Mate");
			m_PackLeader = null;
			foreach (Mobile m in GetMobilesInRange(20))
			{
				if (m is BaseAnimal)
				{
					BaseAnimal ba = (BaseAnimal) m;
					if ((ba.Age == AgeDescription.Adult) && (!m.Female && Female || !Female && m.Female) && ba.TypeName == m_TypeName && !Blessed && !m.Blessed)
					{
						if (m_Mate == null && ba.CanMate())
						{
							m_Mate = m;
						}
						else if (m_Mate != null && ba.CanMate())
						{
							if (Female)
							{
								BaseAnimal amob = (BaseAnimal)m_Mate;
								BaseAnimal bmob = (BaseAnimal)m;
								if (bmob.Rank > amob.Rank) m_Mate = m;
								else if(bmob.Rank == amob.Rank)
								{
									if (bmob.Size > amob.Size)
									{
										m_Mate = m;
									}
								}
							}
						}
					}
				}
			}
		}
		
		public bool IsMatingSeason()
		{
			Map map;
			map = Map.AllMaps[1];
			if (MatingSeason == Seasons.Any) return true;
			if ((map.Season == 0) && (MatingSeason == Seasons.Spring)) return true;
			if ((map.Season == 1) && (MatingSeason == Seasons.Summer)) return true;
			if ((map.Season == 2) && (MatingSeason == Seasons.Autumn)) return true;
			if ((map.Season == 3) && (MatingSeason == Seasons.Winter)) return true;
			return false;
		}
		
		public bool CanMate()
		{
			if (Age == AgeDescription.Adult && IsMatingSeason() && !HasOffspring && !IsPregnant) return true; // && !m_Feeding) return true;
			return false;
		}
		
		public virtual void BreedInformation(Mobile mate)
		{
		}
		
		public void Reproduce()
		{
			if (m_Mate != null && !m_Mate.Deleted)
			{
				BaseAnimal ba = (BaseAnimal) m_Mate;
				if ((ba.InRange(this, 1)) && (!ba.HasOffspring) && ba.Female)
				{
					ba.IsPregnant = true;
					ba.PlaySound(ba.GetIdleSound());
					BreedInformation(ba);
					m_Rank += 1;
					if (Grouping != GroupingType.MatedPair)
					{
						m_Mate = null;
						ba.Mate = null;
					}
				}
			}
		}
		
		public virtual bool IsHungry()
		{
            if (Hunger < m_Size / 4) m_Feeding = true;
			else if (Hunger >= m_Size/2) m_Feeding = false;
			if (Blessed) return false;
			if (m_Age == AgeDescription.Baby) return false;
			if (Combatant != null) return false;
			if (Hunger < m_Size/2 && !this.Blessed ) return true;
			else if (Hunger >= m_Size/2) return false;
			return false;
		}

		public override void OnThink() 
		{ 
			if (m_Update)
			{
				OnAfterSpawn();
				m_Update = false;
			}
			if (IsIndependant())
			{
			//Security
				if (Combatant != null && m_Predator == null)
				{
                    if (IsPredator((Mobile)Combatant))
                        m_Predator = (Mobile)Combatant;
                    //if (IsPredator(m)) m_Predator = m;
                }
				if (m_Mate != null && m_Grouping == GroupingType.MatedPair && m_Age != AgeDescription.Baby)
				{
					if (m_Mate.Combatant != null)
					{
						if (Combatant == null)
						{
							Combatant = m_Mate.Combatant;
						}
					}
				}
				if (m_PackLeader != null && m_PackLeader != this && m_Grouping == GroupingType.Pack && m_Age != AgeDescription.Baby)
				{
					BaseAnimal ba = (BaseAnimal)m_PackLeader;
					if (ba.Combatant != null)
					{
						if (Combatant == null)
						{
							Combatant = ba.Combatant;
						}
					}
					if (ba.Predator != null)
					{
						if (Predator == null)
						{
							Predator = ba.Predator;
						} 
					}
				}
			
				//Hunger
				// Looks for food
				if (m_FoodItem == null && IsHungry() && m_Feeding)
				{
					if (m_HungerRange < 10) m_HungerRange = 10;
					FindFood();
					this.DebugSay("searches for food");
				}
				if (m_Prey != null && !CanGetToPrey())
				{
					m_Prey = null;
					Combatant = null;
					this.DebugSay("Done being fighty.");
				}
			
				if (m_Mother != null && m_Age == AgeDescription.Baby)
				{
					BaseAnimal ba = (BaseAnimal) m_Mother;
					this.DebugSay("going to ma");
					RangeHome = 3;
					Home = ba.Location;
					if (ba.Predator != null)
					{
						m_Predator = ba.Predator;
					}
					if (Combatant != null)
					{
						ba.Combatant = Combatant;
					}
					if (ba.InRange(this, 1))
					{
						Hunger = (Hunger + 1 <21) ? Hunger + 1: Hunger;
					}
					if (!ba.InRange(this, 30))
					{
						ba.HasOffspring = false;
						m_Mother = null;
						MakeYoung();
					}
				}
			
				if (m_FoodItem != null && m_Predator == null && !Blessed && m_Age != AgeDescription.Baby)
				{
					if (!InRange(m_FoodItem.Location, m_HungerRange) || m_FoodItem.Deleted)
					{
						DebugSay("Too far from food");
						m_FoodItem = null;
					}
					if (Combatant != null )
					{
						DebugSay("I'm fighting so I can't eat");
						m_FoodItem = null;
					}
					if (IsHungry() && m_Feeding) 
					{
						if (!CanGetToFood())
						{
							m_FoodItem = null;
							FindFood();
							DebugSay("searches for new food");
						}
						else
						{
							DebugSay("eating the food.");
							Eat(m_FoodItem);
						}
					}
					if (!IsHungry())
					{
						DebugSay("I'm full");
						m_FoodItem = null;
						if (m_PackLeader != null )
						{
							if (InRange(m_PackLeader, 30))
							{
								RangeHome = 10;
								Home = m_PackLeader.Location;
							}
							else
							{
								RangeHome = 30;
								Home = this.Location;
							}
						}
						if (m_Mate != null)
						{
							if (!CanMate() && !Female)
							{
								RangeHome = 7;
								Home = m_Mate.Location;
							}
							else if (CanMate() && !Female)
							{
								RangeHome = 0;
								Home = m_Mate.Location;
							}
						}
						if (m_Mother != null)
						{
							RangeHome = 2;
							Home = m_Mother.Location;
						}
					}
				}
				if (m_FoodItem == null && !Blessed && m_Age != AgeDescription.Baby)
				{
					RangeHome = 50;
					Home = Location;
					if (PackLeader != null)
					{
						RangeHome = 15;
						Home = m_PackLeader.Location;
					}
					if (m_Mate != null)
					{
						RangeHome = 2;
						Home = m_Mate.Location;
					}
				}
				if (CanMate() && m_MateActive) 
				{
					DebugSay("Time to Mate");
					//if (m_Grouping != GroupingType.MatedPair) m_Mate = null;
					if (m_Mate == null) FindMate();
					else Reproduce();
				}
				if (!ForcedActive && CanCheckActive()) CheckActive();
				if (CanCheckMinute() && !Blessed) MinuteCheck();
			}
			base.OnThink();
		}

		public override void OnDeath( Container c )
		{
			base.OnDeath(c);
			Mobile m = this.FindMostRecentDamager(true);
			if (m != null && m is BaseAnimal && m != this)
			{
				BaseAnimal ba = (BaseAnimal) m;
				Item food = (Item)c;
				if (this.Eats == EatType.Carnivore)
				{
					if (ba.Eats != EatType.Herbivore)
					{
						ba.Home = this.Location;
						ba.RangeHome = 0;
						ba.FoodItem = food;
						ba.Rank += 2;
					}
					else ba.Rank += 1;
					
					ba.Home = this.Location;
					ba.RangeHome = 0;
					ba.FoodItem = food;
				}
				else if (this.Eats == EatType.Omnivore)
				{
					if (ba.Eats != EatType.Herbivore)
					{
						ba.Home = this.Location;
						ba.RangeHome = 0;
						ba.FoodItem = food;
						ba.Rank += 2;
					}
					else ba.Rank += 1;
				}
				else if (this.Eats == EatType.Herbivore)
				{
					if (ba.Eats != EatType.Herbivore)
					{
						ba.Home = this.Location;
						ba.RangeHome = 0;
						ba.FoodItem = food;
						ba.Rank += 2;
					}
					else
					{
						ba.Rank += 1;
					}
				}
			}
		}
		
        public override void OnDelete()
        {
			if (m_AnimalBody != null)
			{
				m_AnimalBody.Delete();
			}
			if (m_Mother != null)
			{
				BaseAnimal ba = (BaseAnimal) m_Mother;
				ba.HasOffspring = false;
			}
            base.OnDelete();
        }
		
        public override void AddNameProperties(ObjectPropertyList list)
        {
			base.AddNameProperties(list);
			if (m_Brand != null) list.Add("["+m_Brand+"]");
		}
		
		public BaseAnimal( Serial serial ) : base( serial )
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			ProfileLocked = true;

			base.Serialize(writer);

			writer.Write((int)2); // version
			writer.Write( m_AnimalBody );
			
			writer.Write( (string) m_TypeName );
			writer.Write( (string) m_Brand );
			writer.Write((Mobile)m_Owner);
			writer.Write((bool)m_MateActive);
			writer.Write((int)m_LitterSize);
			writer.Write((bool)m_Spawned);
			writer.Write((bool)m_IsPregnant);
			writer.Write((bool)m_HasOffspring);
			writer.Write((Mobile)m_Mother);
			writer.Write((int)m_MatingSeason);
			writer.Write((int)m_MaxAge);
			writer.Write((int)m_Age);
			
			writer.Write((bool)m_GrainEater);
			writer.Write((bool)m_FruitEater);
			writer.Write((bool)m_Scavenger);
			writer.Write((int)m_Rank);
			writer.Write((bool)m_Ambush);
			writer.Write((bool)m_Hibernate);
			writer.Write((int)m_Size);
			writer.Write((int)m_Activity);
			writer.Write((int)m_Eats);
			writer.Write((int)m_Animal);
			writer.Write((int)m_Grouping);
			writer.Write((Mobile)m_PackLeader);
			writer.Write((Mobile)m_Mate);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			if (ProfileLocked)
			{
				MoreDeserialize(reader);
			}
		}

		public virtual void MoreDeserialize(GenericReader reader)
		{
			int version = reader.ReadInt();
			switch (version)
			{
			case 2:
				{
					m_AnimalBody = (SleepingAnimal)reader.ReadItem();
					goto case 1;
				}
			case 1:
				{
					m_TypeName = reader.ReadString();
					m_Brand = reader.ReadString();
					m_Owner = reader.ReadMobile();
					m_MateActive = reader.ReadBool();
					m_LitterSize = reader.ReadInt();
					m_Spawned = reader.ReadBool();
					m_IsPregnant = reader.ReadBool();
					m_HasOffspring = reader.ReadBool();
					m_Mother = reader.ReadMobile();
					m_MatingSeason = (Seasons)reader.ReadInt();
					m_MaxAge = reader.ReadInt();
					m_Age = (AgeDescription)reader.ReadInt();
					goto case 0;
				}
			case 0:
				{
					m_GrainEater = reader.ReadBool();
					m_FruitEater = reader.ReadBool();
					m_Scavenger = reader.ReadBool();
					m_Rank = reader.ReadInt();
					m_Ambush = reader.ReadBool();
					m_Hibernate = reader.ReadBool();
					m_Size = reader.ReadInt();
					m_Activity = (ActivityType)reader.ReadInt();
					m_Eats = (EatType)reader.ReadInt();
					m_Animal = (AnimalType)reader.ReadInt();
					m_Grouping = (GroupingType)reader.ReadInt();
					m_PackLeader = reader.ReadMobile();
					m_Mate = reader.ReadMobile();
					break;
				}
			}
		}
	}
}
