using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class ThanksgivingQuest : BaseQuest
	{
		public ThanksgivingQuest() : base()
		{
			//The player must slay 10 ThanksgivingTurkey
			this.AddObjective(new SlayObjective(typeof(ThanksgivingTurkey), "Peru selvagem", 10 ));
			//Reward the Player Gold
			this.AddReward(new BaseReward("Chifre da Abundância"));
			//Reward the Player Magic Item(s)
			//this.AddReward(new BaseReward("2 Magic Item(s)"));
		}


		//Quest Title
		public override object Title { get { return "Ajude a salvar o Dia de Ação de Graças"; } }
		//Quest Description
		public override object Description { get { return "Saudações, companheiro peregrino, preciso de sua ajuda! Pois veja, perdi todos os perus de Ação de Graças... Preciso que você encontre e mate 10 perus selvagens para mim ou o jantar de Ação de Graças será arruinado! Eles podem aparecer e em torno das Florestas de Yew. Eles são difíceis de encontrar e ainda mais difíceis de matar, então boa sorte, meu amigo! Volte para mim depois de matar 10 deles para uma recompensa"; } }
		//Quest Refuse Message
		public override object Refuse { get { return "Quem vai salvar o Dia de Ação de Graças agora!"; } } //Might be fun to trigger generic monster spawn or npc animation here
		//Quest Uncompleted Message
		public override object Uncomplete { get { return "Você precisa matar 10 perus selvagens e me devolver para receber uma recompensa!"; } }
		//Quest Completed Message
		public override object Complete { get { return "Ahhh sim, muito bem, meu amigo aqui, aceite isso como sua recompensa e FELIZ AÇÃO DE GRAÇAS..."; } }

		public override void GiveRewards()
		{
			//Give Gold to player in form of a bank check
			//BankCheck gold = new BankCheck(Utility.RandomMinMax(125, 150));
			//if(!Owner.AddToBackpack( gold ))
			//gold.MoveToWorld(Owner.Location,Owner.Map);
			Owner.AddToBackpack(new HornOfPlenty());

			//Item item;

			//Random Magic Item #1
			//item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();
			//if( item is BaseWeapon )
			//	BaseRunicTool.ApplyAttributesTo((BaseWeapon)item, 10, 50, 500 );
			//if( item is BaseArmor )
			//	BaseRunicTool.ApplyAttributesTo((BaseArmor)item, 10, 50, 500 );
			//if( item is BaseJewel )
			//	BaseRunicTool.ApplyAttributesTo((BaseJewel)item, 10, 50, 500 );
			//if( item is BaseHat )
			//	BaseRunicTool.ApplyAttributesTo((BaseHat)item, 10, 50, 500 );
			//if(!Owner.AddToBackpack( item ) )
			//{
			//	item.MoveToWorld(Owner.Location,Owner.Map);
			//}

			//Random Magic Item #2
			//item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();
			//if( item is BaseWeapon )
			//	BaseRunicTool.ApplyAttributesTo((BaseWeapon)item, 10, 50, 500 );
			//if( item is BaseArmor )
			//	BaseRunicTool.ApplyAttributesTo((BaseArmor)item, 10, 50, 500 );
			//if( item is BaseJewel )
			//	BaseRunicTool.ApplyAttributesTo((BaseJewel)item, 10, 50, 500 );
			//if( item is BaseHat )
			//	BaseRunicTool.ApplyAttributesTo((BaseHat)item, 10, 50, 500 );
			//if(!Owner.AddToBackpack( item ) )
			//{
			//	item.MoveToWorld(Owner.Location,Owner.Map);
			//}
//
			base.GiveRewards();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
    public class HungryPilgrim : MondainQuester
    {
        [Constructable]
        public HungryPilgrim() 
            : base("Um Peregrino Faminto", " - Missão de Ação de Graças")
        {
        }

        public HungryPilgrim(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new Type[]
                {
                    typeof(ThanksgivingQuest) //Must Match line 11
                };
        public override void InitBody()
        {
            InitStats(100, 100, 25);
	    	Body = 184;
            Female = true;
            CantWalk = true;
            Race = Race.Human;

            Hue = 0x83F4;
            HairItemID = 0x203B;
            HairHue = 0x470;
        }

        public override void InitOutfit()
        {
            AddItem(new Boots(747));
			AddItem(new LongPants(747));
            AddItem(new Tunic(747));
            AddItem(new Cloak(747));
			AddItem(new FloppyHat(747));
			AddItem(new HalfApron(1150));
			AddItem(new Lantern());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
