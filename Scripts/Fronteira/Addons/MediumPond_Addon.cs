using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class MediumPond_Addon : BaseAddon
	{
        private static int[,] m_AddOnSimpleComponents = new int[,] {
			  {2233, -3, 1, 0}, {2233, -1, 1, 0}, {13422, -3, 5, 0}// 3	4	5	
			, {13422, -3, 4, 0}, {13422, -3, 3, 0}, {13422, -4, 4, 0}// 6	7	8	
			, {13422, -4, 5, 0}, {1993, -2, 3, 0}, {1993, -2, 4, 0}// 9	10	11	
			, {1993, -2, 5, 0}, {1993, -2, 6, 0}, {1993, -2, 2, 0}// 12	13	14	
			, {2232, -3, 5, 1}, {2232, -3, 4, 1}, {2232, -3, 3, 1}// 15	16	17	
			, {2235, -3, 6, 1}, {2235, -1, 6, 1}, {2232, -1, 5, 1}// 18	19	20	
			, {2232, -1, 4, 1}, {2232, -1, 3, 1}, {2232, -1, 2, 1}// 21	22	23	
			, {1993, -1, 2, 0}, {1993, -1, 3, 0}, {1993, -1, 4, 0}// 24	25	26	
			, {1993, -1, 5, 0}, {1993, -1, 6, 0}, {3212, -4, 4, 4}// 27	28	29	
			, {4963, -5, 4, 0}, {3237, -4, 6, 0}, {3339, -3, 5, 2}// 30	31	32	
			, {6008, -4, 4, 3}, {4970, -5, 5, 0}, {4970, -4, 3, 0}// 33	34	35	
			, {3210, -3, 6, 0}, {6047, -3, 2, 0}, {6052, -4, 3, 0}// 36	37	38	
			, {6054, -4, 2, 0}, {2232, -3, 2, 1}, {3210, -3, 2, 0}// 39	40	41	
			, {13422, 7, 1, 0}, {13422, 8, 1, 0}, {13422, 9, 1, 0}// 46	47	48	
			, {13422, 10, 1, 0}, {3220, 11, 1, 4}, {4967, 7, 1, 2}// 49	50	51	
			, {3208, 9, 0, 0}, {3255, 7, 0, 0}, {3255, 6, 1, 0}// 52	53	54	
			, {3211, 8, 0, 0}, {3208, 10, 0, 1}, {13422, 5, 6, 0}// 55	56	58	
			, {13422, 5, 5, 0}, {13422, 5, 4, 0}, {13422, 6, 3, 0}// 59	60	61	
			, {13422, 6, 5, 0}, {13422, 6, 6, 0}, {13422, 7, 6, 0}// 62	63	64	
			, {13422, 7, 5, 0}, {13422, 7, 3, 0}, {13422, 7, 4, 0}// 65	66	67	
			, {13422, 7, 2, 0}, {13422, 8, 2, 0}, {13422, 8, 3, 0}// 68	69	70	
			, {13422, 8, 4, 0}, {13422, 8, 5, 0}, {13422, 8, 6, 0}// 71	72	73	
			, {13422, 9, 6, 0}, {13422, 9, 5, 0}, {13422, 9, 3, 0}// 74	75	76	
			, {13422, 9, 4, 0}, {13422, 9, 2, 0}, {13422, 10, 6, 0}// 77	78	79	
			, {13422, 10, 5, 0}, {13422, 10, 4, 0}, {13422, 10, 3, 0}// 80	81	82	
			, {13422, 10, 2, 0}, {4963, 10, 6, 1}, {6004, 10, 7, 1}// 83	84	85	
			, {4964, 10, 5, 1}, {6004, 11, 5, 1}, {13422, 4, 6, 0}// 86	87	88	
			, {13422, 3, 5, 0}, {13422, 4, 4, 0}, {13422, 3, 4, 0}// 89	90	91	
			, {13422, 4, 5, 0}, {13422, 3, 6, 0}, {13422, 2, 6, 0}// 92	93	94	
			, {13422, 2, 5, 0}, {13422, 2, 4, 0}, {13422, 1, 3, 0}// 95	96	97	
			, {13422, 1, 4, 0}, {13422, 0, 5, 0}, {13422, 0, 4, 0}// 98	99	100	
			, {13422, 0, 3, 0}, {942, 6, 3, 2}, {3237, 0, 6, 0}// 101	102	103	
			, {3237, 6, 8, 3}, {3237, 11, 6, 0}, {3237, 11, 2, 2}// 104	105	106	
			, {3239, 1, 3, 5}, {3239, 5, 3, 1}, {4967, 2, 3, 1}// 107	108	109	
			, {3208, 2, 3, 6}, {3256, 6, 4, 7}, {3220, 6, 2, 0}// 110	111	112	
			, {3339, 8, 3, 2}, {3339, 9, 5, 2}, {3223, 9, 7, 0}// 113	114	116	
			, {3239, 7, 7, 2}, {4967, 8, 7, 2}, {6004, 4, 7, 0}// 117	118	120	
			, {3211, 3, 7, 2}, {3208, 2, 7, 5}, {3208, 1, 6, 1}// 121	122	123	
			, {4967, 2, 6, 1}, {3336, 4, 6, 2}, {3336, 10, 3, 3}// 124	125	126	
			, {3338, 6, 5, 2}, {13422, 6, 4, 0}, {3338, 1, 5, 2}// 127	128	129	
			, {13422, 2, 5, 1}, {6868, 6, 3, 7}, {13446, 7, 4, 1}// 130	131	132	
			, {13451, 3, 5, 1}, {3210, 11, 3, 2}, {6045, 11, 2, 0}// 133	134	135	
			, {6045, 11, 3, 0}, {6045, 11, 4, 0}, {6045, 11, 5, 2}// 136	137	138	
			, {6049, 3, 7, 0}, {6049, 4, 7, 0}, {6049, 5, 7, 0}// 139	140	141	
			, {6049, 6, 7, 0}, {6049, 7, 7, 0}, {6049, 8, 7, 0}// 142	143	144	
			, {6049, 9, 7, 0}, {6055, 10, 7, 0}, {6047, 3, 3, 0}// 145	146	147	
			, {6047, 4, 3, 0}, {6047, 5, 3, 0}, {6047, 2, 3, 0}// 148	149	150	
			, {4963, 3, 3, 0}, {13422, 1, 5, 0}, {3256, 4, 3, 0}// 151	160	161	
					};

 
            
		public override BaseAddonDeed Deed
		{
			get
			{
				return new MediumPond_AddonDeed();
			}
		}

		[ Constructable ]
		public MediumPond_Addon()
		{

            for (int i = 0; i < m_AddOnSimpleComponents.Length / 4; i++)
                AddComponent( new AddonComponent( m_AddOnSimpleComponents[i,0] ), m_AddOnSimpleComponents[i,1], m_AddOnSimpleComponents[i,2], m_AddOnSimpleComponents[i,3] );


			AddComplexComponent( (BaseAddon) this, 40507, -11, -7, 0, 2601, -1, "", 1);// 1
			AddComplexComponent( (BaseAddon) this, 40514, -10, -7, 0, 2601, -1, "", 1);// 2
			AddComplexComponent( (BaseAddon) this, 6093, -4, 2, 0, 0, -1, "tiny flowers", 1);// 42
			AddComplexComponent( (BaseAddon) this, 6093, -3, 6, 0, 0, -1, "tiny flowers", 1);// 43
			AddComplexComponent( (BaseAddon) this, 6093, -4, 6, 3, 0, -1, "tiny flowers", 1);// 44
			AddComplexComponent( (BaseAddon) this, 6093, -4, 4, 4, 0, -1, "tiny flowers", 1);// 45
			AddComplexComponent( (BaseAddon) this, 6093, 11, 1, 4, 0, -1, "tiny flowers", 1);// 57
			AddComplexComponent( (BaseAddon) this, 41606, 9, 2, 2, 0, 0, "", 1);// 115
			AddComplexComponent( (BaseAddon) this, 10296, 10, 6, 5, 16, -1, "", 1);// 119
			AddComplexComponent( (BaseAddon) this, 6093, 6, 8, 6, 0, -1, "tiny flowers", 1);// 152
			AddComplexComponent( (BaseAddon) this, 6093, 0, 6, 3, 0, -1, "tiny flowers", 1);// 153
			AddComplexComponent( (BaseAddon) this, 6093, 2, 3, 6, 0, -1, "tiny flowers", 1);// 154
			AddComplexComponent( (BaseAddon) this, 6093, 6, 2, 0, 0, -1, "tiny flowers", 1);// 155
			AddComplexComponent( (BaseAddon) this, 6093, 11, 2, 4, 0, -1, "tiny flowers", 1);// 156
			AddComplexComponent( (BaseAddon) this, 6093, 11, 6, 3, 0, -1, "tiny flowers", 1);// 157
			AddComplexComponent( (BaseAddon) this, 6093, 7, 7, 2, 0, -1, "tiny flowers", 1);// 158
			AddComplexComponent( (BaseAddon) this, 6093, 5, 3, 1, 0, -1, "tiny flowers", 1);// 159
			AddComplexComponent( (BaseAddon) this, 41606, 1, 4, 2, 0, 0, "", 1);// 162

		}

		public MediumPond_Addon( Serial serial ) : base( serial )
		{
		}

        private static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset, int hue, int lightsource)
        {
            AddComplexComponent(addon, item, xoffset, yoffset, zoffset, hue, lightsource, null, 1);
        }

        private static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset, int hue, int lightsource, string name, int amount)
        {
            AddonComponent ac;
            ac = new AddonComponent(item);
            if (name != null && name.Length > 0)
                ac.Name = name;
            if (hue != 0)
                ac.Hue = hue;
            if (amount > 1)
            {
                ac.Stackable = true;
                ac.Amount = amount;
            }
            if (lightsource != -1)
                ac.Light = (LightType) lightsource;
            addon.AddComponent(ac, xoffset, yoffset, zoffset);
        }

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class MediumPond_AddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new MediumPond_Addon();
			}
		}

		[Constructable]
		public MediumPond_AddonDeed()
		{
			Name = "MediumPond_";
		}

		public MediumPond_AddonDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void	Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
