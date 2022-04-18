using System;
using System.Linq;
using System.Collections.Generic;

using Server;
using Server.TournamentSystem;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
    public class TeamRumbleWaitingRoom : BaseAddon
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public CTFArena Arena { get; set; }

        public override BaseAddonDeed Deed { get { return null; } }

        [Constructable]
        public TeamRumbleWaitingRoom()
            : this(null)
        {
        }

        public TeamRumbleWaitingRoom(CTFArena arena)
        {
            AddStandard();
            AddSpecial();
            Arena = arena;
        }

        protected virtual void AddStandard()
        {
            for (int i = 0; i < AddOnSimpleComponents.Length / 4; i++)
                AddComponent(new AddonComponent(AddOnSimpleComponents[i, 0]), AddOnSimpleComponents[i, 1], AddOnSimpleComponents[i, 2], AddOnSimpleComponents[i, 3]);
        }

        protected virtual void AddSpecial()
        {
            AddComplexComponent(this, 2854, 3, 3, 0, 0, 29, "", 1);// 2
            AddComplexComponent(this, 2854, 3, -3, 0, 0, 29, "", 1);// 3
            AddComplexComponent(this, 2854, -3, 3, 0, 0, 29, "", 1);// 18
            AddComplexComponent(this, 2854, -3, -3, 0, 0, 29, "", 1);// 19
        }

        public TeamRumbleWaitingRoom(Serial serial) : base(serial)
        {
        }

        public override void OnComponentUsed(AddonComponent c, Mobile from)
        {
            if (c.ItemID == 2088 || c.ItemID == 2086)
            {
                var pm = from as PlayerMobile;

                if (pm != null && !from.HasGump(typeof(ConfirmCallbackGump)))
                {
                    var ctf = Arena.PendingTeamRumble;

                    if (pm.Y < c.Y)
                    {
                        BaseGump.SendGump(new ConfirmCallbackGump(pm, "Leave CTF Team Rumble?", String.Format("By clicking yes, you will leave the waiting room for the next CTF rumble game. {0}", ctf != null ? "Any wager paid at this point will be forfeit!" : string.Empty), null,
                            confirm: (mob, o) =>
                            {
                                Kick(mob);
                            }));
                    }
                    else if (Arena != null && ctf != null && (Arena.CurrentFight != ctf || Arena.CurrentFight.PreFight))
                    {
                        TryAdd(pm, ctf, c);
                    }
                }
            }
        }

        public virtual void TryAdd(PlayerMobile pm, TeamRumbleCaptureTheFlag ctf)
        {
            TryAdd(pm, ctf, Components.FirstOrDefault(c => c.ItemID == 2088));
        }

        protected void TryAdd(PlayerMobile pm, TeamRumbleCaptureTheFlag ctf, IPoint3D p)
        {
            if (ActionTimer.AwaitingAction(pm))
            {
                pm.SendMessage("You must wait a few moments.");
            }
            else if (!ArenaHelper.CheckAvailable(pm))
            {
                pm.SendMessage("you are already fighting or are in queue to fight.");
            }
            else if (ctf.InitialParticipants.Contains(pm))
            {
                Add(pm, ctf, p);
            }
            else
            {
                new ActionTimer(null, pm, TimeSpan.FromSeconds(60));
                BaseGump.SendGump(new ConfirmFightGump(Arena, ctf, pm, ctf.Owner as PlayerMobile,
                    (system, fight, pmm, challenger) =>
                    {
                        if (ctf.RemoveWager(pmm))
                        {
                            Add(pmm, ctf, p);
                        }
                        else
                        {
                            pmm.SendMessage("You don't have enough funds in your bank account to join this game!");
                        }
                    }));
            }
        }

        public void Add(Mobile m, TeamRumbleCaptureTheFlag ctf)
        {
            Add(m, ctf, Components.FirstOrDefault(c => c.ItemID == 2088));
        }

        public void Add(Mobile m, TeamRumbleCaptureTheFlag ctf, IPoint3D point)
        {
            if (Arena.CurrentFight != ctf || Arena.CurrentFight.PreFight)
            {
                ctf.AddParticipant(m);

                if (point != null)
                {
                    Timer.DelayCall(TimeSpan.FromMilliseconds(500), () =>
                    {
                        var p = new Point3D(point.X, point.Y - 1, point.Z);

                        if (ctf.HasRule(FightRules.AllowPets))
                        {
                            BaseCreature.TeleportPets(m, p, m.Map);
                        }

                        m.MoveToWorld(p, m.Map);
                        m.SendMessage("You have been teleported into the CTF Team Rumble Waiting Room!");
                    });
                }
                else
                {
                    m.SendMessage("You have joined the next CTF Team Rumble Game!");
                }
            }
        }

        public virtual void Kick(Mobile m)
        {
            if (Arena != null)
            {
                var ctf = Arena.PendingTeamRumble;

                if (ctf != null)
                {
                    ctf.RemoveParticipant(m);
                }
            }

            var p = Arena != null ? Arena.GetRandomKickLocation() : new Point3D(989, 520, -50);
            var map = Arena != null ? Arena.ArenaMap : Map.Malas;

            BaseCreature.TeleportPets(m, p, map);
            m.MoveToWorld(p, map);

            m.SendMessage("You leave the CTF Team Rumble Waiting Room.");
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

                var map = Arena != null ? Arena.ArenaMap : Map.Malas;
                var loc = Arena != null ? Arena.GetRandomKickLocation() : new Point3D(989, 520, -50);

                foreach (var m in toMove)
                {
                    Kick(m);
                }

                ColUtility.Free(toMove);
            }

            base.Delete();
        }

        protected static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset, int hue, int lightsource)
        {
            AddComplexComponent(addon, item, xoffset, yoffset, zoffset, hue, lightsource, null, 1);
        }

        protected static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset, int hue, int lightsource, string name, int amount)
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
                ac.Light = (LightType)lightsource;
            addon.AddComponent(ac, xoffset, yoffset, zoffset);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }

        protected virtual int[,] AddOnSimpleComponents { get { return new int[,]
        {
            { 2082, 3, 3, 0 }, { 2083, 3, -4, 0 }, { 2081, 3, -3, 0 }// 1	4	5	
            , { 2081, 3, -2, 0 }, { 2081, 3, -1, 0 }, { 2081, 3, 0, 0 }// 6	7	8	
            , { 2081, 3, 1, 0 }, { 2081, 3, 2, 0 }, { 1203, 3, -3, 0 }// 9	10	11	
            , { 1199, 3, -2, 0 }, { 1199, 3, -1, 0 }, { 1199, 3, 0, 0 }// 12	13	14	
            , { 1199, 3, 1, 0 }, { 1199, 3, 2, 0 }, { 1201, 3, 3, 0 }// 15	16	17	
            , { 2861, 2, -3, 0 }, { 2860, -3, 2, 0 }, { 2861, 1, -3, 0 }// 20	21	22	
            , { 2861, 0, -3, 0 }, { 2861, -1, -3, 0 }, { 2861, -2, -3, 0 }// 23	24	25	
            , { 2860, -3, 1, 0 }, { 2860, -3, 0, 0 }, { 2860, -3, -1, 0 }// 26	27	28	
            , { 2860, -3, -2, 0 }, { 2088, -1, 3, 0 }, { 2086, 0, 3, 0 }// 29	30	31	
            , { 2083, 2, -4, 0 }, { 2083, 1, -4, 0 }, { 2083, 0, -4, 0 }// 32	33	34	
            , { 2083, -1, -4, 0 }, { 2083, -2, -4, 0 }, { 2083, -3, -4, 0 }// 35	36	37	
            , { 2083, 1, 3, 0 }, { 2083, 2, 3, 0 }, { 2083, -2, 3, 0 }// 38	39	40	
            , { 2083, -3, 3, 0 }, { 2081, -4, 3, 0 }, { 2081, -4, 2, 0 }// 41	42	43	
            , { 2081, -4, 1, 0 }, { 2081, -4, 0, 0 }, { 2081, -4, -1, 0 }// 44	45	46	
            , { 2081, -4, -2, 0 }, { 2081, -4, -3, 0 }, { 1193, -2, 0, 0 }// 47	48	49	
            , { 1193, -1, 1, 0 }, { 1193, 0, 2, 0 }, { 1193, 2, 0, 0 }// 50	51	52	
            , { 1193, 1, -1, 0 }, { 1193, 0, -2, 0 }, { 1194, -1, 2, 0 }// 53	54	55	
            , { 1194, -2, 1, 0 }, { 1194, 2, -1, 0 }, { 1194, 1, -2, 0 }// 56	57	58	
            , { 1196, -2, 2, 0 }, { 1196, 2, -2, 0 }, { 1196, 2, 1, 0 }// 59	60	61	
            , { 1196, 1, 2, 0 }, { 1196, 1, 0, 0 }, { 1196, 0, 1, 0 }// 62	63	64	
            , { 1196, 0, -1, 0 }, { 1196, -1, 0, 0 }, { 1196, -2, -1, 0 }// 65	66	67	
            , { 1196, -1, -2, 0 }, { 1195, 2, 2, 0 }, { 1195, 1, 1, 0 }// 68	69	70	
            , { 1195, 0, 0, 0 }, { 1195, -1, -1, 0 }, { 1195, -2, -2, 0 }// 71	72	73	
            , { 1202, -3, 3, 0 }, { 1196, 2, -3, 0 }, { 1196, 1, -3, 0 }// 74	75	76	
            , { 1200, 0, -3, 0 }, { 1200, -1, -3, 0 }, { 1200, -2, -3, 0 }// 77	78	79	
            , { 1204, -3, -3, 0 }, { 1198, 2, 3, 0 }, { 1198, 1, 3, 0 }// 80	81	82	
            , { 1198, 0, 3, 0 }, { 1198, -1, 3, 0 }, { 1198, -2, 3, 0 }// 83	84	85	
            , { 1197, -3, -2, 0 }, { 1197, -3, -1, 0 }, { 1197, -3, 0, 0 }// 86	87	88	
            , { 1197, -3, 1, 0 }, { 1197, -3, 2, 0 }// 89	90	
        }; } }
    }
}
