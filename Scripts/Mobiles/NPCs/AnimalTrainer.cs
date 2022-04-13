#region References
using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Targeting;
using Server.Engines.Quests;
using System.Linq;
using Shrink.ShrinkSystem;
#endregion

namespace Server.Mobiles
{
	public class AnimalTrainer : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

		[Constructable]
		public AnimalTrainer()
			: base("o domesticador")
		{
			SetSkill(SkillName.AnimalLore, 64.0, 100.0);
			SetSkill(SkillName.AnimalTaming, 90.0, 100.0);
			SetSkill(SkillName.Veterinary, 65.0, 88.0);
		}

		public AnimalTrainer(Serial serial)
			: base(serial)
		{ }

		protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }
		public override VendorShoeType ShoeType { get { return Female ? VendorShoeType.ThighBoots : VendorShoeType.Boots; } }

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBAnimalTrainer());
		}

		public override int GetShoeHue()
		{
			return 0;
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem(Utility.RandomBool() ? new QuarterStaff() : (Item)new ShepherdsCrook());
		}

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			if (from.Alive)
			{
				list.Add(new StableEntry(this, from));

				if (from.Stabled.Count > 0)
				{
					list.Add(new ClaimAllEntry(this, from));
				}
			}

			base.AddCustomContextEntries(from, list);
		}

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (PetTrainingHelper.Enabled)
            {
                list.Add(1072269); // Quest Giver
            }
        }

        private DateTime _NextTalk;

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (PetTrainingHelper.Enabled && m.Alive && !m.Hidden && m is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m;

                if (InLOS(m) && InRange(m, 8) && !InRange(oldLocation, 8) && DateTime.UtcNow >= _NextTalk)
                {
                 
                    _NextTalk = DateTime.UtcNow + TimeSpan.FromSeconds(15);
                }
            }
        }


        private class PetSaleTarget : Target
        {
            private AnimalTrainer m_Trader;

            public PetSaleTarget(AnimalTrainer trader) : base(12, false, TargetFlags.None)
            {
                m_Trader = trader;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is BaseCreature)
                {
                    m_Trader.EndPetSale(from, (BaseCreature)targeted);
                }
                else if (targeted == from)
                {
                    m_Trader.SayTo(from, 502672); // HA HA HA! Sorry, I am not an inn. 
                }

            }
        }

        public void BeginPetSale(Mobile from)
        {
            if (Deleted || !from.CheckAlive())
                return;

            SayTo(from, "Que animal esta vendendo?");

            from.Target = new PetSaleTarget(this);

        }

        //RUFO beginfunction
        private void SellPetForGold(Mobile from, BaseCreature pet, int goldamount)
        {
            Item gold = new Gold(goldamount);
            pet.ControlTarget = null;
            pet.ControlOrder = OrderType.None;
            pet.Internalize();
            pet.SetControlMaster(null);
            pet.SummonMaster = null;
            pet.Delete();
            from.PlaySound(0x2E6);
            Container backpack = from.Backpack;
            SayTo(from, "Obrigado. Aqui estao "+goldamount+" moedas.");
            if (backpack == null || !backpack.TryDropItem(from, gold, false))
            {
                gold.MoveToWorld(from.Location, from.Map);

            }

        }
        //RUFO endfunction


        public void EndPetSale(Mobile from, BaseCreature pet)
        {
            if (Deleted || !from.CheckAlive())
                return;

            if (pet is BaseAnimal)
            {
                BaseAnimal ba = (BaseAnimal)pet;
                if (from != ba.Owner && ba.Owner != null)
                {
                    SayTo(from, 1042562);
                    return;
                }
                else if ((ba.Owner == null) && (!pet.Controlled || pet.ControlMaster != from))
                {
                    SayTo(from, 1042562);
                    return;
                }
            }
            else if (!pet.Controlled || pet.ControlMaster != from)
            {
                SayTo(from, 1042562); // You do not own that pet! 
                return;
            }

            if (pet.IsDeadPet) SayTo(from, 1049668); // Living pets only, please. 
            else if (pet.Summoned) SayTo(from, "That is a summoned creature!");
            else if (pet.Body.IsHuman) SayTo(from, 502672); // HA HA HA! Sorry, I am not an inn. 
            else if ((pet is PackLlama || pet is PackHorse || pet is Beetle) && (pet.Backpack != null && pet.Backpack.Items.Count > 0)) SayTo(from, 1042563); // You need to unload your pet. 
            else if (pet.Combatant != null && pet.InRange(pet.Combatant, 12) && pet.Map == pet.Combatant.Map) SayTo(from, 1042564); // I'm sorry.  Your pet seems to be busy. 
            else
            {
                if (pet is FarmChicken)
                {
                    FarmChicken ba = (FarmChicken)pet;
                    double sellbonus = 0;
                    if (ba.MotherBreed == ChickenBreed.Leghorn || ba.FatherBreed == ChickenBreed.Leghorn)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == ChickenBreed.Barnevelder || ba.FatherBreed == ChickenBreed.Barnevelder)
                    {
                        sellbonus += (ba.IsPurebred() ? 4.0 : 2.0);
                    }
                    if (ba.MotherBreed == ChickenBreed.Orpington || ba.FatherBreed == ChickenBreed.Orpington)
                    {
                        sellbonus += (ba.IsPurebred() ? 2.0 : 1.0);
                    }
                    if (ba.MotherBreed == ChickenBreed.Poltava || ba.FatherBreed == ChickenBreed.Poltava)
                    {
                        sellbonus += (ba.IsPurebred() ? 2.0 : 1.0);
                    }
                    if (ba.MotherBreed == ChickenBreed.Bresse || ba.FatherBreed == ChickenBreed.Bresse)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == ChickenBreed.Braekel || ba.FatherBreed == ChickenBreed.Braekel)
                    {
                        sellbonus += (ba.IsPurebred() ? 3.0 : 1.5);
                    }
                    if (ba.Age == AgeDescription.Baby)
                    {
                        sellbonus += 1.0;
                    }
                    else if (ba.Age == AgeDescription.Young)
                    {
                        sellbonus += 4.0;
                    }
                    else if (ba.Age == AgeDescription.Adult)
                    {
                        sellbonus += 6.0;
                    }
                    else if (ba.Age == AgeDescription.Senior)
                    {
                        sellbonus -= 10.0;
                    }
                    sellbonus += (ba.Female ? 10 : 0);
                    SellPetForGold(from, pet, 15 + (int)sellbonus);
                }
                else if (pet is PackHorse) SellPetForGold(from, pet, 303);
                else if (pet is Rabbit) SellPetForGold(from, pet, 39);
                else if (pet is PackLlama) SellPetForGold(from, pet, 245);
                else if (pet is Dog) SellPetForGold(from, pet, 90);
                else if (pet is Cat) SellPetForGold(from, pet, 69);
                else if (pet is Pig) SellPetForGold(from, pet, 57);
                else if (pet is Bull) SellPetForGold(from, pet, 120);
                else if (pet is FarmPig)
                {
                    FarmPig ba = (FarmPig)pet;
                    double sellbonus = 0;
                    if (ba.MotherBreed == PigBreed.Duroc || ba.FatherBreed == PigBreed.Duroc)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == PigBreed.Iberian || ba.FatherBreed == PigBreed.Iberian)
                    {
                        sellbonus += (ba.IsPurebred() ? 4.0 : 2.0);
                    }
                    if (ba.MotherBreed == PigBreed.Tamworth || ba.FatherBreed == PigBreed.Tamworth)
                    {
                        sellbonus += (ba.IsPurebred() ? 2.0 : 1.0);
                    }
                    if (ba.MotherBreed == PigBreed.White || ba.FatherBreed == PigBreed.White)
                    {
                        sellbonus += (ba.IsPurebred() ? 2.0 : 1.0);
                    }
                    if (ba.MotherBreed == PigBreed.Feral || ba.FatherBreed == PigBreed.Feral)
                    {
                        sellbonus -= (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.Age == AgeDescription.Baby)
                    {
                        sellbonus += 5.0;
                    }
                    else if (ba.Age == AgeDescription.Young)
                    {
                        sellbonus += 10.0;
                    }
                    else if (ba.Age == AgeDescription.Adult)
                    {
                        sellbonus += 15.0;
                    }
                    else if (ba.Age == AgeDescription.Senior)
                    {
                        sellbonus -= 20.0;
                    }
                    sellbonus += (ba.Female ? 10 : 0);
                    SellPetForGold(from, pet, 40 + (int)sellbonus);
                }
                else if (pet is WildBoar)
                {
                    WildBoar ba = (WildBoar)pet;
                    double sellbonus = 0;
                    if (ba.MotherBreed == PigBreed.Duroc || ba.FatherBreed == PigBreed.Duroc)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == PigBreed.Iberian || ba.FatherBreed == PigBreed.Iberian)
                    {
                        sellbonus += (ba.IsPurebred() ? 4.0 : 2.0);
                    }
                    if (ba.MotherBreed == PigBreed.Tamworth || ba.FatherBreed == PigBreed.Tamworth)
                    {
                        sellbonus += (ba.IsPurebred() ? 2.0 : 1.0);
                    }
                    if (ba.MotherBreed == PigBreed.White || ba.FatherBreed == PigBreed.White)
                    {
                        sellbonus += (ba.IsPurebred() ? 2.0 : 1.0);
                    }
                    if (ba.MotherBreed == PigBreed.Feral || ba.FatherBreed == PigBreed.Feral)
                    {
                        sellbonus -= (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.Age == AgeDescription.Baby)
                    {
                        sellbonus += 5.0;
                    }
                    else if (ba.Age == AgeDescription.Young)
                    {
                        sellbonus += 10.0;
                    }
                    else if (ba.Age == AgeDescription.Adult)
                    {
                        sellbonus += 15.0;
                    }
                    else if (ba.Age == AgeDescription.Senior)
                    {
                        sellbonus -= 20.0;
                    }
                    sellbonus += (ba.Female ? 10 : 0);
                    SellPetForGold(from, pet, 40 + (int)sellbonus);
                }
                else if (pet is Horse) SellPetForGold(from, pet, 250);
                else if (pet is WildHorse)
                {
                    WildHorse ba = (WildHorse)pet;
                    double sellbonus = 0;
                    if (ba.MotherBreed == HorseBreed.Andalusian || ba.FatherBreed == HorseBreed.Andalusian)
                    {
                        sellbonus += (ba.IsPurebred() ? 10.0 : 5.0);
                    }
                    if (ba.MotherBreed == HorseBreed.Arabian || ba.FatherBreed == HorseBreed.Arabian)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == HorseBreed.Appaloosa || ba.FatherBreed == HorseBreed.Appaloosa)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == HorseBreed.Haflinger || ba.FatherBreed == HorseBreed.Haflinger)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == HorseBreed.Thoroughbred || ba.FatherBreed == HorseBreed.Thoroughbred)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == HorseBreed.Hackney || ba.FatherBreed == HorseBreed.Hackney)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.Age == AgeDescription.Baby)
                    {
                        sellbonus += 5.0;
                    }
                    else if (ba.Age == AgeDescription.Young)
                    {
                        sellbonus += 10.0;
                    }
                    else if (ba.Age == AgeDescription.Adult)
                    {
                        sellbonus += 15.0;
                    }
                    else if (ba.Age == AgeDescription.Senior)
                    {
                        sellbonus -= 20.0;
                    }
                    sellbonus += (ba.Female ? 10 : 0);
                    SellPetForGold(from, pet, 50 + (int)sellbonus);
                }
                else if (pet.GetType().Name.Contains("Ostard")) SellPetForGold(from, pet, 500);
                else if (pet is Cow) SellPetForGold(from, pet, 150);
                else if (pet is FarmCow)
                {
                    FarmCow ba = (FarmCow)pet;
                    double sellbonus = 0;
                    if (ba.MotherBreed == CowBreed.Holstein || ba.FatherBreed == CowBreed.Holstein)
                    {
                        sellbonus += (ba.IsPurebred() ? 10.0 : 5.0);
                    }
                    if (ba.MotherBreed == CowBreed.Guernsey || ba.FatherBreed == CowBreed.Guernsey)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == CowBreed.Hereford || ba.FatherBreed == CowBreed.Hereford)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == CowBreed.Angus || ba.FatherBreed == CowBreed.Angus)
                    {
                        sellbonus += (ba.IsPurebred() ? 10.0 : 5.0);
                    }
                    if (ba.MotherBreed == CowBreed.Gloucester || ba.FatherBreed == CowBreed.Gloucester)
                    {
                        sellbonus += (ba.IsPurebred() ? 3.0 : 1.5);
                    }
                    if (ba.MotherBreed == CowBreed.Montbeliarde || ba.FatherBreed == CowBreed.Montbeliarde)
                    {
                        sellbonus += (ba.IsPurebred() ? 3.0 : 1.5);
                    }
                    if (ba.MotherBreed == CowBreed.Corriente || ba.FatherBreed == CowBreed.Corriente)
                    {
                        sellbonus += (ba.IsPurebred() ? 4.0 : 2.0);
                    }
                    if (ba.MotherBreed == CowBreed.ToroBravo || ba.FatherBreed == CowBreed.ToroBravo)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.Age == AgeDescription.Baby)
                    {
                        sellbonus += 5.0;
                    }
                    else if (ba.Age == AgeDescription.Young)
                    {
                        sellbonus += 10.0;
                    }
                    else if (ba.Age == AgeDescription.Adult)
                    {
                        sellbonus += 15.0;
                    }
                    else if (ba.Age == AgeDescription.Senior)
                    {
                        sellbonus -= 20.0;
                    }
                    sellbonus += (ba.Female ? 10 : 0);
                    SellPetForGold(from, pet, 70 + (int)sellbonus);
                }
                else if (pet is FarmBull)
                {
                    FarmBull ba = (FarmBull)pet;
                    double sellbonus = 0;
                    if (ba.MotherBreed == CowBreed.Holstein || ba.FatherBreed == CowBreed.Holstein)
                    {
                        sellbonus += (ba.IsPurebred() ? 10.0 : 5.0);
                    }
                    if (ba.MotherBreed == CowBreed.Guernsey || ba.FatherBreed == CowBreed.Guernsey)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == CowBreed.Hereford || ba.FatherBreed == CowBreed.Hereford)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == CowBreed.Angus || ba.FatherBreed == CowBreed.Angus)
                    {
                        sellbonus += (ba.IsPurebred() ? 10.0 : 5.0);
                    }
                    if (ba.MotherBreed == CowBreed.Gloucester || ba.FatherBreed == CowBreed.Gloucester)
                    {
                        sellbonus += (ba.IsPurebred() ? 3.0 : 1.5);
                    }
                    if (ba.MotherBreed == CowBreed.Montbeliarde || ba.FatherBreed == CowBreed.Montbeliarde)
                    {
                        sellbonus += (ba.IsPurebred() ? 3.0 : 1.5);
                    }
                    if (ba.MotherBreed == CowBreed.Corriente || ba.FatherBreed == CowBreed.Corriente)
                    {
                        sellbonus += (ba.IsPurebred() ? 4.0 : 2.0);
                    }
                    if (ba.MotherBreed == CowBreed.ToroBravo || ba.FatherBreed == CowBreed.ToroBravo)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.Age == AgeDescription.Baby)
                    {
                        sellbonus += 5.0;
                    }
                    else if (ba.Age == AgeDescription.Young)
                    {
                        sellbonus += 10.0;
                    }
                    else if (ba.Age == AgeDescription.Adult)
                    {
                        sellbonus += 15.0;
                    }
                    else if (ba.Age == AgeDescription.Senior)
                    {
                        sellbonus -= 20.0;
                    }
                    sellbonus += (ba.Female ? 10 : 0);
                    SellPetForGold(from, pet, 100 + (int)sellbonus);
                }
                else if (pet is Hind)
                    SellPetForGold(from, pet, 75);
                else if (pet is GreatHart)
                    SellPetForGold(from, pet, 200);
                else if (pet is Eagle)
                    SellPetForGold(from, pet, 201);
                else if (pet is Sheep)
                    SellPetForGold(from, pet, 201);
                else if (pet is FarmSheep)
                {
                    FarmSheep ba = (FarmSheep)pet;
                    double sellbonus = 0;
                    if (ba.MotherBreed == SheepBreed.Cormo || ba.FatherBreed == SheepBreed.Cormo)
                    {
                        sellbonus += (ba.IsPurebred() ? 8.0 : 4.0);
                    }
                    if (ba.MotherBreed == SheepBreed.Cotswold || ba.FatherBreed == SheepBreed.Cotswold)
                    {
                        sellbonus += (ba.IsPurebred() ? 7.0 : 3.5);
                    }
                    if (ba.MotherBreed == SheepBreed.Swaledale || ba.FatherBreed == SheepBreed.Swaledale)
                    {
                        sellbonus += (ba.IsPurebred() ? 6.0 : 3.0);
                    }
                    if (ba.MotherBreed == SheepBreed.Coopworth || ba.FatherBreed == SheepBreed.Coopworth)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.0);
                    }
                    if (ba.MotherBreed == SheepBreed.Racka || ba.FatherBreed == SheepBreed.Racka)
                    {
                        sellbonus += (ba.IsPurebred() ? 10.0 : 5.0);
                    }
                    if (ba.MotherBreed == SheepBreed.Latxa || ba.FatherBreed == SheepBreed.Latxa)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == SheepBreed.Awassi || ba.FatherBreed == SheepBreed.Awassi)
                    {
                        sellbonus += (ba.IsPurebred() ? 4.0 : 2.0);
                    }
                    if (ba.Age == AgeDescription.Baby)
                    {
                        sellbonus += 2.0;
                    }
                    else if (ba.Age == AgeDescription.Young)
                    {
                        sellbonus += 5.0;
                    }
                    else if (ba.Age == AgeDescription.Adult)
                    {
                        sellbonus += 10.0;
                    }
                    else if (ba.Age == AgeDescription.Senior)
                    {
                        sellbonus -= 20.0;
                    }
                    sellbonus += (ba.Female ? 10 : 0);
                    SellPetForGold(from, pet, 60 + (int)sellbonus);
                }
                else if (pet is FarmGoat)
                {
                    FarmGoat ba = (FarmGoat)pet;
                    double sellbonus = 0;
                    if (ba.MotherBreed == GoatBreed.Pyrenean || ba.FatherBreed == GoatBreed.Pyrenean)
                    {
                        sellbonus += (ba.IsPurebred() ? 8.0 : 4.0);
                    }
                    if (ba.MotherBreed == GoatBreed.Saanen || ba.FatherBreed == GoatBreed.Saanen)
                    {
                        sellbonus += (ba.IsPurebred() ? 7.0 : 3.5);
                    }
                    if (ba.MotherBreed == GoatBreed.Angora || ba.FatherBreed == GoatBreed.Angora)
                    {
                        sellbonus += (ba.IsPurebred() ? 10.0 : 5.0);
                    }
                    if (ba.MotherBreed == GoatBreed.Cashmere || ba.FatherBreed == GoatBreed.Cashmere)
                    {
                        sellbonus += (ba.IsPurebred() ? 12.0 : 6.0);
                    }
                    if (ba.MotherBreed == GoatBreed.Boer || ba.FatherBreed == GoatBreed.Boer)
                    {
                        sellbonus += (ba.IsPurebred() ? 5.0 : 2.5);
                    }
                    if (ba.MotherBreed == GoatBreed.Stiefelgeiss || ba.FatherBreed == GoatBreed.Stiefelgeiss)
                    {
                        sellbonus += (ba.IsPurebred() ? 4.0 : 2.0);
                    }
                    if (ba.Age == AgeDescription.Baby)
                    {
                        sellbonus += 2.0;
                    }
                    else if (ba.Age == AgeDescription.Young)
                    {
                        sellbonus += 5.0;
                    }
                    else if (ba.Age == AgeDescription.Adult)
                    {
                        sellbonus += 10.0;
                    }
                    else if (ba.Age == AgeDescription.Senior)
                    {
                        sellbonus -= 20.0;
                    }
                    sellbonus += (ba.Female ? 10 : 0);
                    SellPetForGold(from, pet, 60 + (int)sellbonus);
                }
                else if (pet is BlackBear)
                    SellPetForGold(from, pet, 200);
                else if (pet is Bird)
                    SellPetForGold(from, pet, 25);
                else if (pet is TimberWolf)
                    SellPetForGold(from, pet, 384);
                else if (pet is GreyWolf)
                    SellPetForGold(from, pet, 384);
                else if (pet is DireWolf)
                    SellPetForGold(from, pet, 384);
                else if (pet is BlackBear)
                    SellPetForGold(from, pet, 210);
                else if (pet is Panther)
                    SellPetForGold(from, pet, 300);
                else if (pet is Cougar)
                    SellPetForGold(from, pet, 200);
                else if (pet is BrownBear)
                    SellPetForGold(from, pet, 427);
                else if (pet is GrizzlyBear)
                    SellPetForGold(from, pet, 250);
                else if (pet is Rat)
                    SellPetForGold(from, pet, 53);
                else if (pet is RidableLlama)
                    SellPetForGold(from, pet, 101);
                else if (pet is Llama)
                    SellPetForGold(from, pet, 150);
                else if(pet is BaseCreature && pet.MinTameSkill > 60 && !pet.IsPacifico())
                {
                    SellPetForGold(from, pet, (int)(pet.MinTameSkill * 5));
                }
                else
                    SayTo(from, "Nao quero comprar isto"); // You can't PetSale that!
            }
        }


        private Type[] _Quests = { typeof(TamingPetQuest), typeof(UsingAnimalLoreQuest), typeof(LeadingIntoBattleQuest), typeof(TeachingSomethingNewQuest) };

        public override void OnDoubleClick(Mobile m)
        {
            if (PetTrainingHelper.Enabled && m is PlayerMobile && m.InRange(Location, 5))
            {
                CheckQuest((PlayerMobile)m);
            }
        }

        public bool CheckQuest(PlayerMobile player)
        {
            for (int i = 0; i < _Quests.Length; i++)
            {
                var quest = player.Quests.FirstOrDefault(q => q.GetType() == _Quests[i]);

                if (quest != null)
                {
                    if (quest.Completed)
                    {
                        if (quest.GetType() != typeof(TeachingSomethingNewQuest))
                        {
                            quest.GiveRewards();
                        }
                        else
                        {
                            player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
                        }

                        return true;
                    }
                    else
                    {
                        player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.InProgress, false));
                        quest.InProgress();
                    }
                }
            }

            BaseQuest questt = new TamingPetQuest();
            questt.Owner = player;
            questt.Quester = this;
            player.CloseGump(typeof(MondainQuestGump));
            player.SendGump(new MondainQuestGump(questt));

            return true;
        }

		public static int GetMaxStabled(Mobile from)
		{
			var taming = from.Skills[SkillName.AnimalTaming].Value;
			var anlore = from.Skills[SkillName.AnimalLore].Value;
			var vetern = from.Skills[SkillName.Veterinary].Value;
			var sklsum = taming + anlore + vetern;

            int max = from is PlayerMobile ? ((PlayerMobile)from).RewardStableSlots : 0;

			if (sklsum >= 240.0)
			{
				max += 5;
			}
			else if (sklsum >= 200.0)
			{
				max += 4;
			}
			else if (sklsum >= 160.0)
			{
				max += 3;
			}
			else
			{
				max += 2;
			}
			
			// bonus SA stable slots
			if (Core.SA) 
 			{ 
 				max += 2;
 			}
 			//bonus ToL stable slots
 			if (Core.TOL) 
 			{ 
 				max += 2;
 			}
 
			if (taming >= 100.0)
			{
				max += (int)((taming - 90.0) / 10);
			}

			if (anlore >= 100.0)
			{
				max += (int)((anlore - 90.0) / 10);
			}

			if (vetern >= 100.0)
			{
				max += (int)((vetern - 90.0) / 10);
			}

            return max + Server.Spells.SkillMasteries.MasteryInfo.BoardingSlotIncrease(from);
		}

		private void CloseClaimList(Mobile from)
		{
			from.CloseGump(typeof(ClaimListGump));
		}

		public void BeginClaimList(Mobile from)
		{
			if (Deleted || !from.CheckAlive())
			{
				return;
			}

			var list = new List<BaseCreature>();

			for (var i = 0; i < from.Stabled.Count; ++i)
			{
				var pet = from.Stabled[i] as BaseCreature;

				if (pet == null || pet.Deleted)
				{
					if (pet != null)
					{
                        if (pet.Body.IsHuman)
                            continue;

						pet.IsStabled = false;
						pet.StabledBy = null;
					}

					from.Stabled.RemoveAt(i--);
					continue;
				}

				list.Add(pet);
			}

			if (list.Count > 0)
			{
				from.SendGump(new ClaimListGump(this, from, list));
			}
			else
			{
				SayTo(from, 502671); // But I have no animals stabled with me at the moment!
			}
		}

		public void EndClaimList(Mobile from, BaseCreature pet)
		{
			if (pet == null || pet.Deleted || from.Map != Map || !from.Stabled.Contains(pet) || !from.CheckAlive())
			{
				return;
			}

			if (!from.InRange(this, 14))
			{
				from.SendLocalizedMessage(500446); // That is too far away.
				return;
			}

			if (CanClaim(from, pet))
			{
				DoClaim(from, pet);

				from.Stabled.Remove(pet);

				if (from is PlayerMobile)
				{
					((PlayerMobile)from).AutoStabled.Remove(pet);
				}
			}
			else
			{
				SayTo(from, 1049612, pet.Name); // ~1_NAME~ remained in the stables because you have too many followers.
			}
		}

		public void BeginStable(Mobile from)
		{
			if (Deleted || !from.CheckAlive())
			{
				return;
			}

			if ((from.Backpack == null || from.Backpack.GetAmount(typeof(Gold)) < 30) && Banker.GetBalance(from) < 30)
			{
				SayTo(from, "Voce precisa de 30 moedas para estabular seu animal"); // Thou dost not have enough gold, not even in thy bank account.
				return;
			}

			/* 
			 * I charge 30 gold per pet for a real week's stable time.
			 * I will withdraw it from thy bank account.
			 * Which animal wouldst thou like to stable here?
			 */
			from.SendLocalizedMessage(1042558);

			from.Target = new StableTarget(this);
		}

		public void EndStable(Mobile from, BaseCreature pet)
		{
			if (Deleted || !from.CheckAlive())
			{
				return;
			}

			if (pet.Body.IsHuman)
			{
				SayTo(from, 502672); // HA HA HA! Sorry, I am not an inn.
			}
			else if (!pet.Controlled)
			{
				SayTo(from, 1048053); // You can't stable that!
			}
			else if (pet.ControlMaster != from)
			{
				SayTo(from, 1042562); // You do not own that pet!
			}
			else if (pet.IsDeadPet)
			{
				SayTo(from, 1049668); // Living pets only, please.
			}
			else if (pet.Summoned)
			{
				SayTo(from, 502673); // I can not stable summoned creatures.
			}
			else if (pet.Allured)
			{
				SayTo(from, 1048053); // You can't stable that!
			}
			else if ((pet is PackLlama || pet is PackHorse || pet is Beetle) &&
					 (pet.Backpack != null && pet.Backpack.Items.Count > 0))
			{
				SayTo(from, 1042563); // You need to unload your pet.
			}
			else if (pet.Combatant != null && pet.InRange(pet.Combatant, 12) && pet.Map == pet.Combatant.Map)
			{
				SayTo(from, 1042564); // I'm sorry.  Your pet seems to be busy.
			}
			else if (from.Stabled.Count >= GetMaxStabled(from))
			{
				SayTo(from, 1042565); // You have too many pets in the stables!
			}
			else if ((from.Backpack != null && from.Backpack.ConsumeTotal(typeof(Gold), 30)) || Banker.Withdraw(from, 30))
			{
				pet.ControlTarget = null;
				pet.ControlOrder = OrderType.Stay;
				pet.Internalize();

				pet.SetControlMaster(null);
				pet.SummonMaster = null;

				pet.IsStabled = true;
				pet.StabledBy = from;
				pet.Loyalty = MaxLoyalty; // Wonderfully happy
				from.Stabled.Add(pet);
				SayTo(from, "Seu animal foi guardado");
				// [AOS: Your pet has been stabled.] Very well, thy pet is stabled. 
				// Thou mayst recover it by saying 'claim' to me. In one real world week, 
				// I shall sell it off if it is not claimed!
			}
			else
			{
				SayTo(from, 502677); // But thou hast not the funds in thy bank account!
			}
		}

		public void Claim(Mobile from)
		{
			Claim(from, null);
		}

		public void Claim(Mobile from, string petName)
		{
			if (Deleted || !from.CheckAlive())
			{
				return;
			}

			var claimed = false;
			var stabled = 0;

			var claimByName = (petName != null);

			for (var i = 0; i < from.Stabled.Count; ++i)
			{
				var pet = from.Stabled[i] as BaseCreature;

                if (pet != null)
                    if (pet.Body.IsHuman)
                        continue;

                    if (pet == null || pet.Deleted)
				{
					if (pet != null)
					{
                        if (pet.Body.IsHuman)
                            continue;

                        pet.IsStabled = false;
						pet.StabledBy = null;
					}

					from.Stabled.RemoveAt(i--);
					continue;
				}

				++stabled;

				if (claimByName && !Insensitive.Equals(pet.Name, petName))
				{
					continue;
				}

				if (CanClaim(from, pet))
				{
					DoClaim(from, pet);

					from.Stabled.RemoveAt(i);

					if (from is PlayerMobile)
					{
						((PlayerMobile)from).AutoStabled.Remove(pet);
					}

					--i;

					claimed = true;
				}
				else
				{
					SayTo(from, 1049612, pet.Name); // ~1_NAME~ remained in the stables because you have too many followers.
				}
			}

			if (claimed)
			{
				SayTo(from, 1042559); // Here you go... and good day to you!
			}
			else if (stabled == 0)
			{
				SayTo(from, 502671); // But I have no animals stabled with me at the moment!
			}
			else if (claimByName)
			{
				BeginClaimList(from);
			}
		}

		public bool CanClaim(Mobile from, BaseCreature pet)
		{
			return ((from.Followers + pet.ControlSlots) <= from.FollowersMax);
		}

		private void DoClaim(Mobile from, BaseCreature pet)
		{
			pet.SetControlMaster(from);

			if (pet.Summoned)
			{
				pet.SummonMaster = from;
			}

			pet.ControlTarget = from;
			pet.ControlOrder = OrderType.Follow;

			pet.MoveToWorld(from.Location, from.Map);

			pet.IsStabled = false;
			pet.StabledBy = null;

			if (Core.SE)
			{
				pet.Loyalty = MaxLoyalty; // Wonderfully Happy
			}
		}

		public override bool HandlesOnSpeech(Mobile from)
		{
			return true;
		}

        public void Bonda(Mobile m)
        {
            m.BeginTarget(-1, true, TargetFlags.None, new TargetCallback((mob, o) =>
            {
                var bc = o as BaseCreature;
                if (bc == null)
                    return;
                if(bc.ControlMaster != m)
                {
                    m.SendMessage("O pet nao eh seu");
                    return;
                }
                if (!Banker.Withdraw(m, 100000))
                {
                    m.SendMessage("Voce precisa de 100k no banco para isto");
                    return;
                }
                bc.IsBonded = true;
                m.SendMessage("Voce bondou o animal");
            }));
        }

		public override void OnSpeech(SpeechEventArgs e)
		{
            if ((!e.Handled && (e.Speech.ToLower() == "vender")))//was sellpet
            {
                e.Handled = true;
                BeginPetSale(e.Mobile);
            }
            if (!e.Handled && (e.HasKeyword(0x0008) || e.Speech.Contains("estabulo"))) // *stable*
			{
				e.Handled = true;
                e.Mobile.SendGump(new GumpOpcoes("Estabulo", (opt) =>
                {
                    if(opt==0)
                    {
                        CloseClaimList(e.Mobile);
                        BeginStable(e.Mobile);
                    } else if(opt==1)
                    {
                        BeginClaimList(e.Mobile);
                    } else if(opt==2)
                    {
                        e.Mobile.SendGump(new GumpOpcoes("100K Moedas", (o) =>
                        {
                            if (o == 0)
                            {
                                Bonda(e.Mobile);
                            }                  
                        }, 0xE81, 0, new string[] { "Bondar Animal", "Cancelar" }));

                    }
                    else if (opt == 3)
                    {
                        e.Mobile.SendGump(new GumpOpcoes("3K Moedas", (o) =>
                        {
                            if (o == 0)
                            {
                                e.Mobile.DaTarget<BaseCreature>(c =>
                                {
                                    if(!Banker.Withdraw(e.Mobile, 3000))
                                    {
                                        e.Mobile.SendMessage("Voce nao tem dinheiro suficiente no banco");
                                        return;
                                    }
                                    var t = new ShrinkTarget(e.Mobile, null, false);
                                    t.Targeteia(e.Mobile, c);
                                    if(!t.done)
                                    {
                                        Banker.Deposit(e.Mobile, 3000);
                                    } else
                                    {
                                        this.SayTo(e.Mobile, true, "* joga uma pocao de miniatura na criatura *");
                                        this.SayTo(e.Mobile, true, "Aqui esta sua miniatura. Meu amigo eh um otimo alquimista, ele fazer essas belezinhas de pocoes de encolhimento.");
                                    }
                                });
                            }
                        }, 0xE81, 0, new string[] { "Miniaturizar ", "Cancelar" }));

                    }
                }, 0xE81, 0, new string[] { "Estabular", "Ver Lista", "Bondar", "Miniaturizar" }));   
			}
			else if (!e.Handled && e.HasKeyword(0x0009)) // *claim*
			{
				e.Handled = true;

				CloseClaimList(e.Mobile);

				var index = e.Speech.IndexOf(' ');

				if (index != -1)
				{
					Claim(e.Mobile, e.Speech.Substring(index).Trim());
				}
				else
				{
					Claim(e.Mobile);
				}
			}
            else if (!e.Handled && e.Speech.ToLower().IndexOf("stablecount") >= 0)
            {
                IPooledEnumerable eable = e.Mobile.Map.GetMobilesInRange(e.Mobile.Location, 8);
                e.Handled = true;

                foreach (Mobile m in eable)
                {
                    if (m is AnimalTrainer)
                    {
                        e.Mobile.SendLocalizedMessage(1071250, String.Format("{0}\t{1}", e.Mobile.Stabled.Count.ToString(), GetMaxStabled(e.Mobile).ToString())); // ~1_USED~/~2_MAX~ stable stalls used.
                        break;
                    }
                }

                eable.Free();
            }
            else
            {
                base.OnSpeech(e);
            }
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}

		private class StableEntry : ContextMenuEntry
		{
			private readonly Mobile m_From;
			private readonly AnimalTrainer m_Trainer;

			public StableEntry(AnimalTrainer trainer, Mobile from)
				: base(6126, 12)
			{
				m_Trainer = trainer;
				m_From = from;
			}

			public override void OnClick()
			{
				m_Trainer.BeginStable(m_From);
			}
		}

		private class ClaimListGump : Gump
		{
			private readonly Mobile m_From;
			private readonly List<BaseCreature> m_List;
			private readonly AnimalTrainer m_Trainer;

			public ClaimListGump(AnimalTrainer trainer, Mobile from, List<BaseCreature> list)
				: base(50, 50)
			{
				m_Trainer = trainer;
				m_From = from;
				m_List = list;

				from.CloseGump(typeof(ClaimListGump));

				AddPage(0);

				AddBackground(0, 0, 325, 50 + (list.Count * 20), 9250);
				AddAlphaRegion(5, 5, 315, 40 + (list.Count * 20));

				AddHtml(
					15,
					15,
					275,
					20,
                    "<BASEFONT COLOR=#000008>Selecione o animal para retirar:</BASEFONT>",
					false,
					false);

				for (var i = 0; i < list.Count; ++i)
				{
					var pet = list[i];

					if (pet == null || pet.Deleted)
					{
						continue;
					}

                    if (pet.Body.IsHuman)
                        continue;

                    AddButton(15, 39 + (i * 20), 10006, 10006, i + 1, GumpButtonType.Reply, 0);
					AddHtml(
						32,
						35 + (i * 20),
						275,
						18,
						String.Format("<BASEFONT COLOR=#C6C6EF>{0}</BASEFONT>", pet.Name),
						false,
						false);
				}
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				var index = info.ButtonID - 1;

				if (index >= 0 && index < m_List.Count)
				{
					m_Trainer.EndClaimList(m_From, m_List[index]);
				}
			}
		}

		private class ClaimAllEntry : ContextMenuEntry
		{
			private readonly Mobile m_From;
			private readonly AnimalTrainer m_Trainer;

			public ClaimAllEntry(AnimalTrainer trainer, Mobile from)
				: base(6127, 12)
			{
				m_Trainer = trainer;
				m_From = from;
			}

			public override void OnClick()
			{
				m_Trainer.Claim(m_From);
			}
		}

		private class StableTarget : Target
		{
			private readonly AnimalTrainer m_Trainer;

			public StableTarget(AnimalTrainer trainer)
				: base(12, false, TargetFlags.None)
			{
				m_Trainer = trainer;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is BaseCreature)
				{
					m_Trainer.EndStable(from, (BaseCreature)targeted);
				}
				else if (targeted == from)
				{
					m_Trainer.SayTo(from, 502672); // HA HA HA! Sorry, I am not an inn.
				}
				else
				{
					m_Trainer.SayTo(from, 1048053); // You can't stable that!
				}
			}
		}
	}
}
