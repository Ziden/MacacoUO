using System;
using System.Collections.Generic;
using Server.Items;
using Server.Items.Crops;

namespace Server.Mobiles
{
	public class SBGardener : SBInfo
	{
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBGardener(){}

        public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
        public override List<GenericBuyInfo> BuyInfo
        {
            get
            {
                return this.m_BuyInfo;
            }
        }

        public class InternalBuyInfo : List<GenericBuyInfo>
		{
			public InternalBuyInfo()
			{
				this.Add( new GenericBuyInfo( typeof( MilkBucket ), 800, 10, 0x0FFA, 0 ) );
				this.Add( new GenericBuyInfo( typeof( CheeseForm ), 800, 10, 0x0E78, 0 ) );

				this.Add( new GenericBuyInfo( "Vaso de Plantas", typeof( Engines.Plants.PlantBowl ), 2, 20, 0x15FD, 0 ) );
				this.Add( new GenericBuyInfo( "Terra Fertil", typeof( FertileDirt ), 10, 40, 0xF81, 0 ) );
				this.Add( new GenericBuyInfo( "Random Plant Seed", typeof( Engines.Plants.Seed ), 2, 20, 0xDCF, 0 ) );
 				//this.Add( new GenericBuyInfo( typeof( GreaterCurePotion ), 45, 20, 0xF07, 0 ) );
				//this.Add( new GenericBuyInfo( typeof( GreaterPoisonPotion ), 45, 20, 0xF0A, 0 ) );
				//this.Add( new GenericBuyInfo( typeof( GreaterStrengthPotion ), 45, 20, 0xF09, 0 ) );
				//this.Add( new GenericBuyInfo( typeof( GreaterHealPotion ), 45, 20, 0xF0C, 0 ) );
				this.Add(new GenericBuyInfo("Semente de Tomate", typeof(TomatoSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Asparagus Seed", typeof(AsparagusSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Beet Seed", typeof(BeetSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Broccoli Seed", typeof(BroccoliSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Cabbage Seed", typeof(CabbageSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Carrot Seed", typeof(CarrotSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Cauliflower Seed", typeof(CauliflowerSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Celery Seed", typeof(CelerySeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Eggplant Seed", typeof(EggplantSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("GreenBean Seed", typeof(GreenBeanSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Lettuce Seed", typeof(LettuceSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Onion Seed", typeof(OnionSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Peanut Seed", typeof(PeanutSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Peas Seed", typeof(PeasSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Potato Seed", typeof(PotatoSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Radish Seed", typeof(RadishSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("SnowPeas Seed", typeof(SnowPeasSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Soy Seed", typeof(SoySeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Spinach Seed", typeof(SpinachSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Strawberry Seed", typeof(StrawberrySeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("SweetPotato Seed", typeof(SweetPotatoSeed), 5, 20, 0xF27, 0));
        this.Add(new GenericBuyInfo("Turnip Seed", typeof(TurnipSeed), 5, 20, 0xF27, 0));
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
				Add( typeof( MilkBucket ), 400 );
				Add( typeof( CheeseForm ), 400 );

				Add( typeof( Apple ), 1 );
				Add( typeof( Grapes ), 1 );
				Add( typeof( Watermelon ), 3 );
				Add( typeof( YellowGourd ), 1 );
				Add( typeof( Pumpkin ), 5 );
				Add( typeof( Onion ), 1 );
				Add( typeof( Lettuce ), 2 );
				Add( typeof( Squash ), 1 );
				Add( typeof( Carrot ), 1 );
				Add( typeof( HoneydewMelon ), 3 );
				Add( typeof( Cantaloupe ), 3 );
				Add( typeof( Cabbage ), 2 );
				Add( typeof( Lemon ), 1 );
				Add( typeof( Lime ), 1 );
				Add( typeof( Peach ), 1 );
				Add( typeof( Pear ), 1 );
			}
		}
	}
}
