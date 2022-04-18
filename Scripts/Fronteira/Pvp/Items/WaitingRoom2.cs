using System;
using System.Collections.Generic;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.TournamentSystem;

namespace Server.Items
{
	public class TeamRumbleWaitingRoom2 : TeamRumbleWaitingRoom
    {
		public override BaseAddonDeed Deed { get { return null; } }

        [Constructable]
        public TeamRumbleWaitingRoom2()
            : base(null)
        {
        }

        public TeamRumbleWaitingRoom2(CTFArena arena)
            : base(arena)
        {
        }

        protected override void AddSpecial()
        {
            AddComplexComponent((BaseAddon)this, 2854, 7, -2, 0, 0, 29, "", 1);// 1
            AddComplexComponent((BaseAddon)this, 2854, 7, 4, 0, 0, 29, "", 1);// 2
            AddComplexComponent((BaseAddon)this, 2854, 1, 4, 0, 0, 29, "", 1);// 68
            AddComplexComponent((BaseAddon)this, 2854, -6, -2, 0, 0, 29, "", 1);// 90
            AddComplexComponent((BaseAddon)this, 2854, -6, 4, 0, 0, 29, "", 1);// 91
            AddComplexComponent((BaseAddon)this, 2854, 0, 4, 0, 0, 29, "", 1);// 93
        }

        public TeamRumbleWaitingRoom2(Serial serial) : base(serial)
        {
        }

        public override void TryAdd(PlayerMobile pm, TeamRumbleCaptureTheFlag ctf)
        {
            TryAdd(pm, ctf, null);
        }

        public override void OnComponentUsed(AddonComponent c, Mobile from)
        {
            var pm = from as PlayerMobile;

            if (pm != null && !pm.HasGump(typeof(ConfirmCallbackGump)) && (c.ItemID == 2089 || c.ItemID == 2085) && from.InRange(c.Location, 2))
            {
                bool ctf = Arena.PendingTeamRumble != null && Arena.PendingTeamRumble.InitialParticipants.Contains(from);

                BaseGump.SendGump(new ConfirmCallbackGump(pm, "Leave This Arena?", String.Format("By clicking yes, you will be transported back to the arena entry gate. {0}", ctf ? "Leaving the waiting room will remove you from the pending CTF Royal Rumble Game. Any wage paid will be forfeit." : string.Empty), null,
                    confirm: (mob, o) =>
                    {
                        Remove(mob);
                    }));
            }
        }

        private void Remove(Mobile m)
        {
            var arena = Arena as CTFRoyalRumbleArena;

            if (arena != null)
            {
                var ctf = arena.PendingTeamRumble;

                if (ctf != null)
                {
                    ctf.RemoveParticipant(m);
                }
            }

            var p = arena != null ? CTFRoyalRumbleArena.EntranceLocation : new Point3D(989, 520, -50);
            var map = arena != null ? Arena.ArenaMap : Map.Malas;

            BaseCreature.TeleportPets(m, p, map);
            m.MoveToWorld(p, map);
            m.PlaySound(0x1FE);

            m.SendMessage("You have left the CTF Royal Rumble audience region.");
        }

        public override void Kick(Mobile m)
        {
        }

        public override void Delete()
        {
            if (Map != null)
            {
                var toMove = new List<Mobile>();

                foreach (var comp in Components)
                {
                    IPooledEnumerable eable = Map.GetMobilesInRange(comp.Location, 0);

                    foreach (Mobile m in eable)
                    {
                        toMove.Add(m);
                    }
                }

                foreach (var m in toMove)
                {
                    Remove(m);
                }

                ColUtility.Free(toMove);
            }

            base.Delete();
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

        protected override int[,] AddOnSimpleComponents { get { return new int[,]
        {
              {1198, 4, 4, 0}, {1198, 5, 4, 0}, {1198, 6, 4, 0}// 3	4	5	
			, {1200, 4, -2, 0}, {1196, 5, -2, 0}, {1196, 6, -2, 0}// 6	7	8	
			, {1195, 4, 1, 0}, {1195, 5, 2, 0}, {1195, 6, 3, 0}// 9	10	11	
			, {1196, 4, 0, 0}, {1196, 4, 2, 0}, {1196, 5, 1, 0}// 12	13	14	
			, {1196, 5, 3, 0}, {1196, 6, 2, 0}, {1196, 6, -1, 0}// 15	16	17	
			, {1194, 5, -1, 0}, {1194, 6, 0, 0}, {1193, 4, -1, 0}// 18	19	20	
			, {1193, 5, 0, 0}, {1193, 6, 1, 0}, {1193, 4, 3, 0}// 21	22	23	
			, {2083, 6, 4, 0}, {2083, 5, 4, 0}, {2083, 4, -3, 0}// 24	25	26	
			, {2083, 5, -3, 0}, {2083, 6, -3, 0}, {2083, 4, 4, 0}// 27	28	29	
			, {2861, 4, -2, 0}, {2861, 5, -2, 0}// 30	31	32	
			, {1201, 7, 4, 0}, {1199, 7, 3, 0}, {1199, 7, 2, 0}// 33	34	35	
			, {1199, 7, 1, 0}, {1199, 7, 0, 0}, {1199, 7, -1, 0}// 36	37	38	
			, {1203, 7, -2, 0}, {2081, 7, 3, 0}, {2081, 7, 2, 0}// 39	40	41	
			, {2081, 7, 1, 0}, {2081, 7, 0, 0}, {2081, 7, -1, 0}// 42	43	44	
			, {2081, 7, -2, 0}, {2083, 7, -3, 0}, {1198, 3, 4, 0}// 45	46	47	
			, {1200, 3, -2, 0}, {1195, 3, 0, 0}, {1196, 3, -1, 0}// 48	49	50	
			, {1196, 3, 1, 0}, {1194, 3, 3, 0}, {1193, 3, 2, 0}// 51	52	53	
			, {2083, 3, -3, 0}, {2083, 3, 4, 0}, {2861, 3, -2, 0}// 54	55	56	
			, {1198, 2, 4, 0}, {1200, 2, -2, 0}, {1195, 2, -1, 0}// 57	58	59	
			, {1196, 2, 0, 0}, {1196, 2, 3, 0}, {1194, 2, 2, 0}// 60	61	62	
			, {1193, 2, 1, 0}, {2083, 2, 4, 0}, {2083, 2, -3, 0}// 63	64	65	
			, {2861, 2, -2, 0}, {1194, 1, 3, 0}, {1196, 1, 2, 0}// 66	69	70	
			, {1194, 1, 1, 0}, {1194, 1, 0, 0}, {1196, 1, -1, 0}// 71	72	73	
			, {1196, 1, -2, 0}, {1198, 1, 4, 0} // 74	75	76	
			, {2083, 1, 4, 0}, {2083, 1, -3, 0}, {2860, 6, -1, 0}// 83	84	85	
			, {2860, 6, 0, 0}, {2860, 6, 1, 0}, {2860, 6, 2, 0}// 86	87	88	
			, {2860, 6, 3, 0}, {1197, -6, 3, 0}, {1197, -6, 2, 0}// 89	94	95	
			, {1197, -6, 1, 0}, {1197, -6, 0, 0}, {1197, -6, -1, 0}// 96	97	98	
			, {1198, -5, 4, 0}, {1198, -4, 4, 0}, {1198, -3, 4, 0}// 99	100	101	
			, {1198, -2, 4, 0}, {1198, -1, 4, 0}, {1204, -6, -2, 0}// 102	103	104	
			, {1200, -5, -2, 0}, {1200, -4, -2, 0}, {1200, -3, -2, 0}// 105	106	107	
			, {1196, -2, -2, 0}, {1196, -1, -2, 0}, {1202, -6, 4, 0}// 108	109	110	
			, {1195, -5, -1, 0}, {1195, -4, 0, 0}, {1195, -3, 1, 0}// 111	112	113	
			, {1195, -2, 2, 0}, {1195, -1, 3, 0}, {1196, -4, -1, 0}// 114	115	116	
			, {1196, -5, 0, 0}, {1196, -4, 1, 0}, {1196, -3, 0, 0}// 117	118	119	
			, {1196, -3, 2, 0}, {1196, -2, 1, 0}, {1196, -2, 3, 0}// 120	121	122	
			, {1196, -1, 2, 0}, {1196, -1, -1, 0}, {1196, -5, 3, 0}// 123	124	125	
			, {1194, -2, -1, 0}, {1194, -1, 0, 0}, {1194, -5, 2, 0}// 126	127	128	
			, {1194, -4, 3, 0}, {1193, -3, -1, 0}, {1193, -2, 0, 0}// 129	130	131	
			, {1193, -1, 1, 0}, {1193, -3, 3, 0}, {1193, -4, 2, 0}// 132	133	134	
			, {1193, -5, 1, 0}, {2081, -7, -2, 0}, {2081, -7, -1, 0}// 135	136	137	
			, {2085, -7, 0, 0}, {2089, -7, 1, 0}, {2081, -7, 2, 0}// 138	139	140	
			, {2081, -7, 3, 0}, {2081, -7, 4, 0}, {2083, -6, 4, 0}// 141	142	143	
			, {2083, -5, 4, 0}, {2083, -1, 4, 0}, {2083, -2, 4, 0}// 144	145	146	
			, {2083, -6, -3, 0}, {2083, -5, -3, 0}, {2083, -4, -3, 0}// 147	148	149	
			, {2083, -3, -3, 0}, {2083, -2, -3, 0}, {2083, -1, -3, 0}// 150	151	152	
			, {2083, -3, 4, 0}, {2083, -4, 4, 0}, {2860, -6, -1, 0}// 153	154	155	
			, {2860, -6, 0, 0}, {2860, -6, 1, 0}, {2860, -6, 2, 0}// 156	157	158	
			, {2861, -5, -2, 0}, {2861, -4, -2, 0}, {2861, -3, -2, 0}// 159	160	161	
			, {2861, -2, -2, 0}, {2860, -6, 3, 0}, {2861, -1, -2, 0}// 162	163	164	
			, {1198, 0, 4, 0}, {1194, 0, 3, 0}, {1196, 0, 2, 0}// 165	166	167	
			, {1194, 0, 1, 0}, {1196, 0, 0, 0}, {1194, 0, -1, 0}// 168	169	170	
			, {1196, 0, -2, 0}, {2861, 0, -2, 0 }, {2861, 1, -2, 0 }
            , {2083, 0, -3, 0}, {2082, 7, 4, 0}, {2083, 0, 4, 0}// 177	178	
        }; } }
    }
}
