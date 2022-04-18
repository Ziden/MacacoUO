using System;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Commands;

using System.Collections.Generic;
using System.Linq;
 
namespace Server.TournamentSystem
{
    public abstract class CTFArena : PVPTournamentSystem
    {
        public List<FlagHolder> FlagHolders = new List<FlagHolder>();
        public TeamRumbleWaitingRoom WaitingRoom { get; set; }

        public TeamRumbleCaptureTheFlag PendingTeamRumble { get; set; }

        public override TimeSpan StartDelay
        {
            get
            {
                if (CurrentFight is TeamRumbleCaptureTheFlag)
                {
                    return Config.TeamRumbleCTFStartDelay;
                }

                return Config.StartDelay;
            }
        }

        public override ArenaFightType[] FightTypes { get { return new ArenaFightType[] { ArenaFightType.CaptureTheFlag, ArenaFightType.TeamRumbleCaptureTheFlag }; } }

        public virtual bool CTFRandomStart { get { return Config.CTFRandomStart; } }

        public abstract Point3D FlagHolder1Loc { get; }
        public abstract Point3D FlagHolder2Loc { get; }
        public abstract Point3D FlagHolder3Loc { get; }
        public abstract Point3D FlagHolder4Loc { get; }

        public CTFArena(TournamentStone stone)
            : base(stone)
        {
            Active = true;
            CanDoTourneys = false;
        }

        public CTFArena(GenericReader reader, TournamentStone stone)
            : base(reader, stone)
        {
        }

        public override void InitializeSystem()
        {
            base.InitializeSystem();

            SetupFlagHolders();
        }

        protected override void AddTeamsBoard()
        {
            if (TeamsBoardLocation != Point3D.Zero)
            {
                TeamsBoard = new TeamsBoard();
                TeamsBoard.MoveToWorld(TeamsBoardLocation, ArenaMap);

                TeamsBoard.ItemID = 7774;
            }
        }

        public override Point3D GetStartPoint(ArenaTeam team)
        {
            Point3D start;

            var holder = FlagHolders.FirstOrDefault(h => h.Owner == team);

            if (holder != null)
            {
                start = holder.Location;
            }
            else
            {
                start = 0.5 > Utility.RandomDouble() ? TeamAStartLocation : TeamBStartLocation;
            }

            return start;
        }

        public override void EndFight(bool istournament, ArenaTeam winner, ArenaTeam loser, ArenaFight fight)
        {
            base.EndFight(istournament, winner, loser, fight);

            foreach (FlagHolder holder in FlagHolders)
            {
                if (holder != null)
                {
                    holder.SetHue(0);
                }
            }

            if (fight == PendingTeamRumble)
            {
                PendingTeamRumble = null;
            }
        }

        public void RegisterTeamRumbleCTF(PlayerMobile owner, TeamRumbleCaptureTheFlag fight)
        {
            if (fight == null)
            {
                owner.SendMessage("There is an error registering CTF Team Rumble, please contact staff if this persists.");
                return;
            }

            if (PendingTeamRumble != null)
            {
                owner.SendMessage("Team Rumble CTF is currenlty pending in this arena. You can join this game or wait for it to end to start a new one.");
            }
            else
            {
                PendingTeamRumble = fight;
                PendingTeamRumble.Owner = owner;
                PendingTeamRumble.AddParticipant(owner);

                if (WaitingRoom != null)
                {
                    WaitingRoom.Add(owner, fight);
                }

                owner.SendMessage("You have initiated a Team Rumble Capture the Flag game.");

                if (Config.Debug && WaitingRoom != null)
                {
                    foreach (var pm in GetAudience())
                    {
                        if (pm != owner)
                        {
                            WaitingRoom.Add(pm, PendingTeamRumble);
                        }
                    }
                }
            }
        }

        public override void OnTick()
        {
            base.OnTick();

            if (PendingTeamRumble != null)
            {
                if (PendingTeamRumble.CanBegin)
                {
                    if (CurrentFight != PendingTeamRumble && !Queue.Contains(PendingTeamRumble))
                    {
                        if (CanUse)
                        {
                            PendingTeamRumble.RegisterFight();
                            PendingTeamRumble.Begins = DateTime.UtcNow + StartDelay;

                            if (ArenaKeeper != null)
                            {
                                int seconds = (int)StartDelay.TotalSeconds;

                                ArenaKeeper.Say("You have {0} to join the next CTF Team Rumble Game if you haven't already done so!",
                                    string.Format("{0} {1}",
                                    seconds >= 60 ? seconds / 60 : seconds,
                                    seconds == 60 ? "minute" : seconds > 60 ? "minutes" : "seconds"));
                            }
                        }
                        else
                        {
                            AddToQueue(PendingTeamRumble);

                            if (ArenaKeeper != null)
                            {
                                int seconds = (int)StartDelay.TotalSeconds;

                                ArenaKeeper.Say("Once the CTF Team Rumble Game is next in queue, you have {0} to join if you haven't already done so1",
                                    string.Format("{0} {1}",
                                    seconds >= 60 ? seconds / 60 : seconds,
                                    seconds == 60 ? "minute" : seconds > 60 ? "minutes" : "seconds"));
                            }
                        }
                    }
                }
                else if (CurrentFight != PendingTeamRumble)
                {
                    PendingTeamRumble.CheckExpired();
                }

                PendingTeamRumble.PreFightTick();
            }
        }

        public virtual void SetupFlags(CaptureTheFlagFight fight)
        {
            var teamCount = fight.Teams.Count;

            if (fight == null || FlagHolders.Count != teamCount || teamCount > Config.TeamHues.Length)
            {
                fight.CancelFight(CancelReason.SystemError);
                return;
            }

            if (CTFRandomStart)
            {
                League.Shuffle(fight.Teams);
            }

            fight.Flags = new CTFFlag[teamCount];
            var list = CTFFlag.FlagIDs.ToList();

            for (int i = 0; i < teamCount; i++)
            {
                var team = fight.Teams[i].Team;
                var hue = Config.TeamHues[i];
                var id = list[Utility.Random(list.Count)];

                if (team.TeamType == ArenaTeamType.Temp)
                {
                    team.Name = string.Format("Team {0}", Config.HueNames[i]);
                }

                var flag = new CTFFlag(fight, team, id, hue);

                fight.Flags[i] = flag;
                FlagHolders[i].Owner = team;
                FlagHolders[i].AddFlag(flag);
                FlagHolders[i].SetHue(hue);

                list.Remove(id);
            }

            ColUtility.Free(list);
        }

        public void SetupFlagHolders()
        {
            if (FlagHolder1Loc != Point3D.Zero)
            {
                FlagHolder holder = new FlagHolder(this);
                FlagHolders.Add(holder);
                holder.MoveToWorld(FlagHolder1Loc, ArenaMap);
            }

            if (FlagHolder2Loc != Point3D.Zero)
            {
                FlagHolder holder = new FlagHolder(this);
                FlagHolders.Add(holder);
                holder.MoveToWorld(FlagHolder2Loc, ArenaMap);
            }

            if (FlagHolder3Loc != Point3D.Zero)
            {
                FlagHolder holder = new FlagHolder(this);
                FlagHolders.Add(holder);
                holder.MoveToWorld(FlagHolder3Loc, ArenaMap);
            }

            if (FlagHolder4Loc != Point3D.Zero)
            {
                FlagHolder holder = new FlagHolder(this);
                FlagHolders.Add(holder);
                holder.MoveToWorld(FlagHolder4Loc, ArenaMap);
            }
        }

        public override void RemoveSystem()
        {
            foreach (var holder in FlagHolders.Where(h => h != null && !h.Deleted))
            {
                holder.Delete();
            }

            if (WaitingRoom != null)
            {
                WaitingRoom.Delete();
            }

            var ctf = CurrentFight as CaptureTheFlagFight;

            if (ctf != null)
            {
                foreach (var flag in ctf.Flags.Where(f => f != null && !f.Deleted))
                {
                    flag.Delete();
                }
            }

            base.RemoveSystem();
        }

        public virtual void CreateWaitingRoom()
        {
            WaitingRoom = new TeamRumbleWaitingRoom(this);
            WaitingRoom.MoveToWorld(Definition.WaitingRoomLocation, ArenaMap);
        }

        #region Old Item Serialization Vars used for CTFArena base class insertion
        /* DO NOT USE!*/
        private bool m_InheritsItem;

        protected bool InheritsItem { get { return m_InheritsItem; } }
        #endregion

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1);

            writer.WriteItemList<FlagHolder>(FlagHolders, true);
            writer.WriteItem<TeamRumbleWaitingRoom>(WaitingRoom);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
            {
                m_InheritsItem = true;
                CanDoTourneys = false;
            }
            else
            {
                FlagHolders = reader.ReadStrongItemList<FlagHolder>();
                WaitingRoom = reader.ReadItem<TeamRumbleWaitingRoom>();

                if (WaitingRoom != null)
                {
                    WaitingRoom.Arena = this;
                }

                for (int i = 0; i < FlagHolders.Count; i++)
                {
                    FlagHolders[i].System = this;
                }

                if (WaitingRoom == null)
                {
                    Timer.DelayCall(CreateWaitingRoom);
                }
            }
        }
    }

    public class CTFArena1 : CTFArena
    {
        public override string DefaultName { get { return "Capture the Flag Arena 1"; } }
        public override SystemType SystemType { get { return SystemType.CTF1; } }
        public override int WallItemID { get { return 0; } }
        public override Map ArenaMap { get { return Map.Felucca; } }
		
		public override Point3D FlagHolder1Loc { get { return new Point3D(5131, 1290, 0); } }
		public override Point3D FlagHolder2Loc { get { return new Point3D(5187, 1292, 0); } }
        public override Point3D FlagHolder3Loc { get { return Point3D.Zero; } }
        public override Point3D FlagHolder4Loc { get { return Point3D.Zero; } }

        private ArenaDefinition _Definition;

        public override ArenaDefinition Definition
        {
            get
            {
                if (_Definition == null)
                {
                    _Definition = new ArenaDefinition(new Point3D(5220, 1287, 0), new Point3D(5131, 1290, 0), new Point3D(5187, 1292, 0),
                                                     new Point3D(5223, 1296, 2), new Point3D(5220, 1290, 9), new Point3D(5220, 1291, 9),
                                                     new Point3D(5220, 1286, 2), Point3D.Zero, new Point3D(5222, 1286, 2),
                                                     new Point3D(5224, 1286, 2), new Point3D(5224, 1286, 11),
                                                     new Rectangle2D(5220, 1287, 5, 9), new Rectangle2D(0, 0, 0, 0),
                                                     new Rectangle2D[] { new Rectangle2D( 5124, 1285, 95, 103 ) },
                                                     new Rectangle2D[] { new Rectangle2D( 5219, 1273, 15, 27 ) },
                                                     new Rectangle2D(0, 0, 0, 0),
                                                     Point3D.Zero,
                                                     new Point3D(5224, 1277, 9));
                }

                return _Definition;
            }
        }

        public CTFArena1(TournamentStone stone) : base(stone)
        {
            Active = true;
        }

        public override void InitializeSystem()
        {
            base.InitializeSystem();

            for (int i = 0; i < 13; i++)
            {
                Point3D p = new Point3D(5237 + i, 1495, 0);

                var st = new Static(2083);
                st.MoveToWorld(p, ArenaMap);

                var los = new LOSBlocker();
                los.MoveToWorld(p, ArenaMap);
            }
        }

        public static void Setup(Mobile from)
        {
            if (ArenaHelper.HasArena<CTFArena1>())
            {
                if (from != null)
                {
                    from.SendMessage(22, "CTF Arena 1 already exists!");
                }
                else
                {
                    Console.WriteLine("CTF Arena 1 already exists!");
                }

                return;
            }

            CTFArena1Stone stone = new CTFArena1Stone();
            CTFArena1 arena = new CTFArena1(stone);
            stone.MoveToWorld(arena.StoneLocation, arena.ArenaMap);

            arena.CreateWaitingRoom();

            if (from != null)
            {
                from.SendMessage(1154, "CTF Arena 1 Setup!");
            }
            else
            {
                Console.WriteLine("CTF Arena 1 Setup!");
            }
        }

        public static void Delete(Mobile from)
        {
            var arena = ArenaHelper.GetArena<CTFArena1>();

            if (arena == null)
                return;

            var map = arena.ArenaMap;

            if (arena.Stone != null)
            {
                arena.Stone.Delete();

                if (from != null)
                {
                    from.SendMessage(22, "CTF Arena 1 removed!");
                }
                else
                {
                    Console.WriteLine("CTF Arena 1 removed!");
                }
            }
            else if (from != null)
            {
                from.SendMessage(22, "Error removaing CTF Arena 1.");
            }

            if (map != null)
            {
                for (int i = 0; i < 13; i++)
                {
                    Point3D p = new Point3D(5237 + i, 1495, 0);

                    IPooledEnumerable eable = map.GetItemsInRange(p, 0);

                    foreach (Item item in eable)
                    {
                        if (item is Static || item is LOSBlocker)
                        {
                            item.Delete();
                        }
                    }

                    eable.Free();
                }
            }
        }
 
        public override void SetLinkedSystem()
        {
            foreach (PVPTournamentSystem system in PVPTournamentSystem.SystemList)
            {
                if (system is CTFArena2)
                {
                    LinkedSystem = system;
 
                    if (system.LinkedSystem != this)
                        system.LinkedSystem = this;
                }
            }
        }
 
        public CTFArena1(GenericReader reader, TournamentStone stone)
            : base(reader, stone)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = (InheritsItem ? 0 : reader.ReadInt()); // Required for CTFArena base class insertion
        }
    }

    [DeleteConfirm("Are you sure you want to delete this? Deleting this stone will remove any upcoming tournaments and any prize items and all of the arena.")]
    public class CTFArena1Stone : TournamentStone
    {
        public CTFArena1Stone()
        {
        }
 
        public CTFArena1Stone(Serial serial) : base (serial)
        {
        }
 
        public override void LoadSystem(GenericReader reader)
        {
            System = new CTFArena1(reader, this);
        }
 
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }
 
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    public class CTFArena2 : CTFArena
    {
        public override string DefaultName { get { return "Capture the Flag Arena 2"; } }
        public override SystemType SystemType { get { return SystemType.CTF2; } }
        public override int WallItemID { get { return 0; } }
        public override Map ArenaMap { get { return Map.Felucca; } }

        public override Point3D FlagHolder1Loc { get { return new Point3D(5299, 1483, 0); } }
        public override Point3D FlagHolder2Loc { get { return new Point3D(5156, 1508, 0); } }
        public override Point3D FlagHolder3Loc { get { return Point3D.Zero; } }
        public override Point3D FlagHolder4Loc { get { return Point3D.Zero; } }

        private ArenaDefinition _Defintion;

        public override ArenaDefinition Definition
        {
            get
            {
                if (_Defintion == null)
                {
                    _Defintion = new ArenaDefinition(new Point3D(5226, 1498, 0), new Point3D(5299, 1483, 0), new Point3D(5156, 1508, 0),
                                                     new Point3D(5235, 1500, 0), new Point3D(5238, 1496, 9), new Point3D(5248, 1496, 9),
                                                     new Point3D(5234, 1496, 0), Point3D.Zero, new Point3D(5231, 1496, 0),
                                                     new Point3D(5223, 1495, 2), new Point3D(5228, 1496, 7),
                                                     new Rectangle2D(5220, 1496, 40, 12), new Rectangle2D(0, 0, 0, 0),
                                                     new Rectangle2D[] { new Rectangle2D(5125, 1412, 248, 84), new Rectangle2D(5125, 1495, 69, 21) },
                                                     new Rectangle2D[] { new Rectangle2D( 5220, 1496, 40, 12 ), new Rectangle2D(5249, 1482, 15, 11) },
                                                     new Rectangle2D(0, 0, 0, 0),
                                                     Point3D.Zero,
                                                     new Point3D(5256, 1489, 29));
                }

                return _Defintion;
            }
        }

        public CTFArena2(TournamentStone stone)
            : base(stone)
        {
            Active = true;
        }

        public override void InitializeSystem()
        {
            base.InitializeSystem();

            for (int i = 0; i < 10; i++)
            {
                Point3D p = new Point3D(5219, 1287 + i, 0);

                var st = new Static(2081);
                st.MoveToWorld(p, ArenaMap);

                var los = new LOSBlocker();
                los.MoveToWorld(p, ArenaMap);
            }
        }

        public static void Setup(Mobile from)
        {
            if (ArenaHelper.HasArena<CTFArena2>())
            {
                if (from != null)
                {
                    from.SendMessage(22, "CTF Arena 2 already exists!");
                }

                return;
            }

            CTFArena2Stone stone = new CTFArena2Stone();
            CTFArena2 arena = new CTFArena2(stone);
            stone.MoveToWorld(arena.StoneLocation, arena.ArenaMap);

            arena.CreateWaitingRoom();

            if (from != null)
            {
                from.SendMessage(1154, "CTF Arena 2 Setup!");
            }
            else
            {
                Console.WriteLine("CTF Arena 2 Setup!");
            }
        }

        public static void Delete(Mobile from)
        {
            var arena = ArenaHelper.GetArena<CTFArena2>();

            if (arena == null)
                return;

            var map = arena.ArenaMap;

            if (arena.Stone != null)
            {
                arena.Stone.Delete();

                if (from != null)
                {
                    from.SendMessage(22, "CTF Arena 2 removed!");
                }
                else
                {
                    Console.WriteLine("CTF Arena 2 removed!");
                }
            }
            else if (from != null)
            {
                from.SendMessage(22, "Error removing CTF Arena 2.");
            }

            if (map != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    Point3D p = new Point3D(5219, 1287 + i, 0);

                    IPooledEnumerable eable = map.GetItemsInRange(p, 0);

                    foreach (Item item in eable)
                    {
                        if (item is Static || item is LOSBlocker)
                        {
                            item.Delete();
                        }
                    }

                    eable.Free();
                }
            }
        }

        public override void SetLinkedSystem()
        {
            foreach (PVPTournamentSystem system in PVPTournamentSystem.SystemList)
            {
                if (system is CTFArena1)
                {
                    LinkedSystem = system;
 
                    if (system.LinkedSystem != this)
                        system.LinkedSystem = this;
                }
            }
        }

        public CTFArena2(GenericReader reader, TournamentStone stone)
            : base(reader, stone)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = (InheritsItem ? 0 : reader.ReadInt()); // Required for CTFArena base class insertion
        }
    }

    [DeleteConfirm("Are you sure you want to delete this? Deleting this stone will remove any upcoming tournaments and any prize items and all of the arena.")]
    public class CTFArena2Stone : TournamentStone
    {
        public CTFArena2Stone()
        {
        }

        public CTFArena2Stone(Serial serial)
            : base(serial)
        {
        }

        public override void LoadSystem(GenericReader reader)
        {
            System = new CTFArena2(reader, this);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    public class CTFRoyalRumbleArena : CTFArena
    {
        public override string DefaultName { get { return "CTF Royal Rumble"; } }
        public override SystemType SystemType { get { return SystemType.CTFRoyalRumble; } }
        public override int WallItemID { get { return 0; } }
        public override Map ArenaMap { get { return Map.Felucca; } }

        public override Point3D FlagHolder1Loc { get { return new Point3D(5908, 404, -22); } }
        public override Point3D FlagHolder2Loc { get { return new Point3D(5995, 403, -22); } }
        public override Point3D FlagHolder3Loc { get { return new Point3D(5907, 491, -22); } }
        public override Point3D FlagHolder4Loc { get { return new Point3D(5994, 491, -22); } }

        public override ArenaFightType[] FightTypes { get { return new ArenaFightType[] { ArenaFightType.TeamRumbleCaptureTheFlag }; } }

        public static Point3D EntranceLocation { get { return new Point3D(1429, 1693, 0); } }
        public static Point3D EntranceTargetLocation { get { return new Point3D(5942, 458, -19); } }

        private ArenaDefinition _Defintion;

        public override ArenaDefinition Definition
        {
            get
            {
                if (_Defintion == null)
                {
                    _Defintion = new ArenaDefinition(new Point3D(5937, 456, -19), Point3D.Zero, Point3D.Zero, // b and c
                                                     new Point3D(5945, 458, -19), Point3D.Zero, Point3D.Zero,
                                                     new Point3D(5943, 455, -18), Point3D.Zero, new Point3D(5945, 455, -18),
                                                     new Point3D(5945, 455, 0), new Point3D(5942, 455, -19),
                                                     new Rectangle2D(5936, 456, 10, 4), new Rectangle2D(0, 0, 0, 0),
                                                     new Rectangle2D[] { new Rectangle2D(5900, 395, 105, 105) },
                                                     new Rectangle2D[] { new Rectangle2D(5935, 455, 14, 8) },
                                                     new Rectangle2D(0, 0, 0, 0),
                                                     Point3D.Zero,
                                                     new Point3D(5942, 457, -19));
                }

                return _Defintion;
            }
        }

        public ConfirmationMoongate EntranceGate { get; set; }

        public CTFRoyalRumbleArena(TournamentStone stone)
            : base(stone)
        {
            Active = true;
        }

        public static void Setup(Mobile from)
        {
            if (ArenaHelper.HasArena<CTFRoyalRumbleArena>())
            {
                if (from != null)
                {
                    from.SendMessage(22, "CTF Arena 2 already exists!");
                }

                return;
            }

            CTFRoyalRumbleArenaStone stone = new CTFRoyalRumbleArenaStone();
            CTFRoyalRumbleArena arena = new CTFRoyalRumbleArena(stone);
            stone.MoveToWorld(arena.StoneLocation, arena.ArenaMap);

            arena.CreateWaitingRoom();
            arena.CreateMoongate();

            if (from != null)
            {
                from.SendMessage(1154, "CTF Royal Rumble Setup!");
            }
            else
            {
                Console.WriteLine("CTF Royal Rumble Setup!");
            }
        }

        public static void Delete(Mobile from)
        {
            var arena = ArenaHelper.GetArena<CTFRoyalRumbleArena>();

            if (arena == null)
                return;

            var map = arena.ArenaMap;

            if (arena.Stone != null)
            {
                arena.Stone.Delete();

                if (from != null)
                {
                    from.SendMessage(22, "CTF Royal Rumble removed!");
                }
                else
                {
                    Console.WriteLine("CTF Royal Rumble removed!");
                }
            }
            else if (from != null)
            {
                from.SendMessage(22, "Error removing CTF Royal Rumble.");
            }

            if (arena.EntranceGate != null)
            {
                arena.EntranceGate.Delete();
            }
        }

        public override void CreateWaitingRoom()
        {
            WaitingRoom = new TeamRumbleWaitingRoom2(this);
            WaitingRoom.MoveToWorld(Definition.WaitingRoomLocation, ArenaMap);
        }

        public override bool CanRegisterFight(Mobile from, bool checkTeam)
        {
            if (PendingTeamRumble != null)
            {
                return false;
            }

            return base.CanRegisterFight(from, checkTeam);
        }

        public void CreateMoongate()
        {
            EntranceGate = new ConfirmationMoongate()
            {
                Dispellable = false,
                Hue = 2751,
                Name = "CTF Royal Rumble Arena",
                Target = EntranceTargetLocation,
                TargetMap = ArenaMap,
                GumpWidth = 420,
                GumpHeight = 150,
                MessageColor = 0xDC143C,
                MessageString = "Would you like to enter the CTF Royal Rumble Audience Region?",
                TitleColor = 0xDC143C,
                TitleString = "Entrance to CTF Royal Rumble",
            };

            EntranceGate.MoveToWorld(EntranceLocation, ArenaMap);
        }

        public CTFRoyalRumbleArena(GenericReader reader, TournamentStone stone)
            : base(reader, stone)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.WriteItem<ConfirmationMoongate>(EntranceGate);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            EntranceGate = reader.ReadItem<ConfirmationMoongate>();
        }
    }

    [DeleteConfirm("Are you sure you want to delete this? Deleting this stone will remove any upcoming tournaments and any prize items and all of the arena.")]
    public class CTFRoyalRumbleArenaStone : TournamentStone
    {
        public CTFRoyalRumbleArenaStone()
        {
        }

        public CTFRoyalRumbleArenaStone(Serial serial)
            : base(serial)
        {
        }

        public override void LoadSystem(GenericReader reader)
        {
            System = new CTFRoyalRumbleArena(reader, this);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
