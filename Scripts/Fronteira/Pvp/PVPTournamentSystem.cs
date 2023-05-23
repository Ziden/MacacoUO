using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Network;
using Server.Items;
using Server.Regions;
using Server.Engines.ArenaSystem;
using Server.Spells;

namespace Server.TournamentSystem
{
    public enum SystemType
    {
        NewHavenTram,
        NewHavenFel,
        KhaldunTram,
        KhaldunFel,
        CTF1,
        CTF2,
        Tokuno,
        CTFRoyalRumble,
        Malas,
        Custom = 20000
    }

    [PropertyObject]
    public abstract class PVPTournamentSystem
    {
        #region Getters and Setters
        private string _Name;
        private bool _UseLinked;
        private bool _InUse;
        private bool m_Active;
        private ArenaFight _CurrentFight;
        private Mobile _ArenaKeeper;

        public Timer Timer { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public PVPTournamentSystem LinkedSystem { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public TournamentStone Stone { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool InUse
        { 
            get { return _InUse; } 
            set 
            { 
                _InUse = value;

                if (!_InUse)
                    CheckQueue();

                if (Stone != null)
                    Stone.InvalidateProperties();
            } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active 
        { 
            get { return m_Active; }
            set
            { 
                m_Active = value; 
                
                if (m_Active) 
                    _ArenaKeeper = SpawnArenaKeeper();

                if (Stone != null)
                    Stone.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool CanDoTourneys { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Region FightRegion { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Region AudienceRegion { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaFight CurrentFight
        {
            get { return _CurrentFight; }
            set
            {
                _CurrentFight = value; 
                
                if (Stone != null)
                    Stone.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Tournament CurrentTournament { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile ArenaKeeper 
        { 
            get 
            { 
                if (_ArenaKeeper == null && m_Active) 
                    _ArenaKeeper = SpawnArenaKeeper(); 

                return _ArenaKeeper;
            } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TournamentBoard TournamentBoard { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public StatsBoard StatsBoard { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public PrizeChest Chest { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public TeamsBoard TeamsBoard { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaBell Bell { get; set; }

        public List<Tournament> Tournaments = new List<Tournament>();
        public Dictionary<Mobile, Item> Wagers = new Dictionary<Mobile, Item>();
        public List<ArenaFight> Queue = new List<ArenaFight>();

        public bool CanUse 
        { 
            get 
            {
                if (Tournaments.Count == 0)
                    return !_InUse;
                else
                {
                    Tournaments.Sort();
                    return !_InUse && DateTime.Now + TournamentSetupTime < Tournaments[0].StartTime;
                }
            } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UseLinked
        {
            get { return _UseLinked; }
            set
            {
                if (value != _UseLinked)
                {
                    _UseLinked = value;

                    if (value)
                        SetLinkedSystem();
                    else
                        LinkedSystem = null;
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Name
        {
            get { return _Name; }
            set
            {
                var old = _Name;

                if (old != value && ValidateName(value))
                {
                    _Name = value;

                    OnNameChange(old); // we need to update tournament stats!
                }
            }
        }
        #endregion

        #region static props

        private static List<PVPTournamentSystem> m_SystemList = new List<PVPTournamentSystem>();
        public static List<PVPTournamentSystem> SystemList { get { return m_SystemList; } }

        public static void Initialize()
        {
            Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), () =>
            {
                foreach (var system in SystemList)
                {
                    system.OnTick();
                }

                League.OnTick();
            });
        }
        #endregion

        #region Overrideable Props
        public virtual ArenaFightType[] FightTypes { get { return new ArenaFightType[] { ArenaFightType.SingleElimination, ArenaFightType.BestOf3, ArenaFightType.LastManStanding }; } }

        public virtual bool NoClearRegion { get { return Config.NoClearRegion; } }

        public virtual TimeSpan TournamentSetupTime { get { return Config.TournamentSetupTime; } }

        public virtual TimeSpan StartDelay { get { return Config.StartDelay; } }
        public virtual TimeSpan EjectDelay { get { return Config.EjectDelay; } }
        public virtual TimeSpan PreFightDelay { get { return Config.PreFightDelay; } }
        public virtual TimeSpan DefaultDuration { get { return Config.DefaultDuration; } }
        public virtual int MinLastManStanding { get { return Config.MinLastManStanding; } }

        public virtual bool AudienceRegionNoAttack { get { return Config.AudienceRegionNoAttack; } }
        public virtual bool AudienceRegionNoSpells { get { return Config.AudienceRegionNoSpells; } }

        public virtual bool RandomizeStartLocations { get { return Config.RandomizeStartLocations; } }

        public virtual bool CanUseAlternateArena { get { return LinkedSystem != null; } }

        public virtual FightRegion GetFightRegion { get { return new FightRegion(this); } }
        public virtual AudienceRegion GetAudienceRegion { get { return new AudienceRegion(this); } }

        public virtual Rectangle2D[] FightingRegionBounds { get { return Definition.FightingRegionBounds; } }
        public virtual Rectangle2D[] AudienceRegionBounds { get { return Definition.AudienceRegionBounds; } }
        public virtual Rectangle2D RandomStartBounds { get { return Definition.RandomStartBounds; } }

        public virtual Rectangle2D KickZone { get { return Definition.KickZone; } }
        public virtual Rectangle2D[] WallArea { get { return Definition.WallArea; } }

        public virtual Point3D StoneLocation { get { return Definition.StoneLocation; } }
        public virtual Point3D TeamAStartLocation { get { return Definition.TeamAStartLocation; } }
        public virtual Point3D TeamBStartLocation { get { return Definition.TeamBStartLocation; } }
        public virtual Point3D ArenaKeeperLocation { get { return Definition.ArenaKeeperLocation; } }
        public virtual Point3D TeamAWageDisplay { get { return Definition.TeamAWageDisplay; } }
        public virtual Point3D TeamBWageDisplay { get { return Definition.TeamBWageDisplay; } }
        public virtual Point3D StatsBoardLocation { get { return Definition.StatsBoardLocation; } }
        public virtual Point3D TeamsBoardLocation { get { return Definition.TeamsBoardLocation; } }
        public virtual Point3D TournamentInfoBoardLocation { get { return Definition.TournamentInfoBoardLocation; } }
        public virtual Point3D ChestLocation { get { return Definition.ChestLocation; } }
        public virtual Point3D BellLocation { get { return Definition.BellLocation; } }

        public virtual Map ArenaMap { get { return null; } }
        public virtual Map ItemMap { get { return null; } }

        public virtual int WallItemID { get { return 0x80; } }

        public abstract SystemType SystemType { get; }
        public abstract ArenaDefinition Definition { get; }
        public abstract string DefaultName { get; }

        #endregion

        public PVPTournamentSystem(TournamentStone stone)
        {
            _Name = DefaultName;
            Stone = stone;
            Stone.System = this;
            CanDoTourneys = true;

            Timer.DelayCall(InitializeSystem);
        }

        public virtual void InitializeSystem()
        {
            FightRegion = GetFightRegion;
            AudienceRegion = GetAudienceRegion;

            m_SystemList.Add(this);
            _UseLinked = true;
            SetLinkedSystem();

            AddStatsBoard();
            AddTeamsBoard();
            AddTournyInfoBoard();
            AddChest();
            AddBell();

            OnSystemConfigured();
        }

        protected virtual void AddStatsBoard()
        {
            if (StatsBoardLocation != Point3D.Zero && StatsBoard == null)
            {
                StatsBoard = new StatsBoard(this);
                StatsBoard.MoveToWorld(StatsBoardLocation, ArenaMap);
            }
        }

        protected virtual void AddTeamsBoard()
        {
            if (TeamsBoardLocation != Point3D.Zero && TeamsBoard == null)
            {
                TeamsBoard = new TeamsBoard();
                TeamsBoard.MoveToWorld(TeamsBoardLocation, ArenaMap);
            }
        }

        protected virtual void AddTournyInfoBoard()
        {
            if (TournamentInfoBoardLocation != Point3D.Zero && TournamentBoard == null)
            {
                TournamentBoard = new TournamentBoard(this);
                TournamentBoard.MoveToWorld(TournamentInfoBoardLocation, ArenaMap);
            }
        }

        protected virtual void AddChest()
        {
            if (ChestLocation != Point3D.Zero)
            {
                Chest = new PrizeChest(this);
                Chest.MoveToWorld(ChestLocation, ArenaMap);
                Chest.Visible = false;
            }
        }

        protected virtual void AddBell()
        {
            if (BellLocation != Point3D.Zero && Bell != null)
            {
                Bell = new ArenaBell(this, 0x4C5C);
                Bell.MoveToWorld(BellLocation, ArenaMap);
            }
        }

        public virtual void OnSystemConfigured()
        {
        }

        public bool ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return PVPTournamentSystem.SystemList.All(sys => sys.Name != name);
        }

        public ArenaFight ConstructArenaFight(ArenaFightType type, Tournament tourney = null)
        {
            switch (type)
            {
                default:
                case ArenaFightType.SingleElimination: return new SingleEliminationFight(this, tourney);
                case ArenaFightType.BestOf3: return new BestOf3Fight(this, tourney);
                case ArenaFightType.LastManStanding: return new LastManStandingFight(this, tourney);
                case ArenaFightType.CaptureTheFlag: return new CaptureTheFlagFight(this, tourney);
                case ArenaFightType.TeamRumbleCaptureTheFlag: return new TeamRumbleCaptureTheFlag(this);
            }
        }

        private void OnNameChange(string oldArena)
        {
            foreach (var stat in PVPTournamentStats.TournamentStats.Where(s => s.Arena == oldArena))
            {
                stat.UpdateArenaName(this);
            }
        }

        public override string ToString()
        {
            return "...";
        }

        public void AddTournament(Tournament tourney)
        {
            if (!Tournaments.Contains(tourney))
            {
                Tournaments.Add(tourney);
                TournamentBoard.UpdateBoards();
            }
        }

        public void RemoveTournament(Tournament tourney)
        {
            if (Tournaments.Contains(tourney))
            {
                Tournaments.Remove(tourney);
                TournamentBoard.UpdateBoards();
            }
        }

        public virtual Mobile SpawnArenaKeeper()
        {
            if (_ArenaKeeper != null || Stone == null)
                return null;

            ArenaKeeper arenaKeeper = new ArenaKeeper(this);
            arenaKeeper.MoveToWorld(ArenaKeeperLocation, ArenaMap);
            arenaKeeper.Home = ArenaKeeperLocation;
            arenaKeeper.RangeHome = 5;

            return arenaKeeper;
        }

        public virtual bool CanRegisterTournament(Mobile from)
        {
            if (Tournaments.Count >= Config.MaxTournaments)
                return false;

            foreach (Tournament tourney in Tournaments)
                if (tourney.Creator == from)
                    return false;

            return CanDoTourneys;
        }

        public virtual bool BeginPrefight()
        {
            return true;
        }

        public virtual bool BeginFight()
        {
            return true;
        }

        public virtual void OnTick()
        {
            for (int i = 0; i < Tournaments.Count; i++)
            {
                Tournament tourney = Tournaments[i];

                if (!InUse && CurrentTournament == null && DateTime.Now + TournamentSetupTime >= tourney.StartTime)
                {
                    if (DateTime.Now > tourney.StartTime)
                        tourney.StartTime = DateTime.Now + TournamentSetupTime;  //Just incase server goes down etc ect.

                    CurrentTournament = tourney;
                    tourney.BeginTournament();
                }
                else if (tourney.WarningMessage == 1 && DateTime.Now + TimeSpan.FromHours(72) > tourney.StartTime)
                {
                    string msg = String.Format("The {0} Tournament will begin at {1}.", tourney.Name, tourney.StartTime);

                    foreach (ArenaTeam team in tourney.Participants)
                        ArenaHelper.DoTeamPM(team, "72 Hour Reminder", msg, ArenaKeeper, TimeSpan.FromHours(73));

                    tourney.WarningMessage++;
                }
                else if (tourney.WarningMessage == 2 && DateTime.Now + TimeSpan.FromHours(24) > tourney.StartTime)
                {
                    string msg = String.Format("The {0} Tournament will begin at {1}.", tourney.Name, tourney.StartTime);

                    foreach (ArenaTeam team in tourney.Participants)
                        ArenaHelper.DoTeamPM(team, "24 Hour Reminder", msg, ArenaKeeper, TimeSpan.FromHours(25));

                    tourney.WarningMessage++;
                }
            }
        }

        public virtual void OnBeforeFight()
        {
        }

        public virtual void OnTickFight()
        {
        }

        public virtual void EndFight(bool istournament, ArenaTeam winner, ArenaTeam loser, ArenaFight fight)
        {
            /*foreach (FlagHolder holder in m_FlagHolders)
            {
                if (holder != null)
                    holder.Hue = 0;
            }*/
        }

        /*public virtual void SetupFlags(CaptureTheFlagFight fight)
		{
            if (fight == null || m_FlagHolders.Count < 2)
                return;

            ArenaTeam a = fight.TeamA;
            ArenaTeam b = fight.TeamB;

            int huea = TeamAHue;
            int hueb = TeamBHue;

            if (CTFRandomStart && 0.5 > Utility.RandomDouble())
            {
                a = fight.TeamB;
                b = fight.TeamA;
                huea = TeamBHue;
                hueb = TeamAHue;
            }

			if(FlagHolders.Count > 1 && FlagHolders[0] != null && FlagHolders[1] != null)
			{
				fight.Flag1 = new CTFFlag(fight, a, 0x15B7, huea);
                fight.Flag2 = new CTFFlag(fight, b, 0x15B6, hueb);

                FlagHolders[0].Owner = a;
                FlagHolders[0].AddFlag(fight.Flag1);
                FlagHolders[0].Hue = huea;

                FlagHolders[1].Owner = b;
                FlagHolders[1].AddFlag(fight.Flag2);
                FlagHolders[1].Hue = hueb;
			}
			else
				fight.CancelFight(CancelReason.SystemError);
		}

        public void SetupFlagHolders()
        {
            if (FlagHolder1Loc != Point3D.Zero)
            {
                FlagHolder holder = new FlagHolder(this);
                m_FlagHolders.Add(holder);
                holder.MoveToWorld(FlagHolder1Loc, ArenaMap);
            }

            if (FlagHolder2Loc != Point3D.Zero)
            {
                FlagHolder holder = new FlagHolder(this);
                m_FlagHolders.Add(holder);
                holder.MoveToWorld(FlagHolder2Loc, ArenaMap);
            }

            if (FlagHolder3Loc != Point3D.Zero)
            {
                FlagHolder holder = new FlagHolder(this);
                m_FlagHolders.Add(holder);
                holder.MoveToWorld(FlagHolder3Loc, ArenaMap);
            }

            if (FlagHolder4Loc != Point3D.Zero)
            {
                FlagHolder holder = new FlagHolder(this);
                m_FlagHolders.Add(holder);
                holder.MoveToWorld(FlagHolder4Loc, ArenaMap);
            }
        }*/
		
		public virtual bool CanDoCTF()
		{
			return this is CTFArena && ((CTFArena)this).FlagHolders.Count > 0;
		}
 
        public virtual void DoArenaKeeperMessage(string message)
        {
            if (_ArenaKeeper != null)
                _ArenaKeeper.Say(message);
        }

        public virtual Point3D GetRandomKickLocation()
        {
            Point3D p = StoneLocation;
            int x, y, z;

            for (int i = 0; i < 25; i++)
            {
                x = Utility.Random(KickZone.X, KickZone.Width);
                y = Utility.Random(KickZone.Y, KickZone.Height);

                if (this is KhaldunArena || this is KhaldunArenaTram)
                    z = 25;
                else if (this is CTFRoyalRumbleArena)
                    z = -19;
                else
                    z = ArenaMap.GetAverageZ(x, y);

                if (ArenaMap.CanSpawnMobile(x, y, z))
                {
                    p = new Point3D(x, y, z);
                    break;
                }
            }

            return p;
        }
 
        public virtual Point3D RandomStartLocation(ArenaTeam team)
        {
			Point3D start = GetStartPoint(team);

			for (int i = 0; i < 25; i++)
			{
				int x = Utility.RandomMinMax(start.X - 5, start.X + 5);
				int y = Utility.RandomMinMax(start.Y - 5, start.Y + 5);
				int z = ArenaMap.GetAverageZ(x, y);

                var p = new Point3D(x, y, z);

				if (Region.Find(p, ArenaMap).IsPartOf<FightRegion>() && ArenaMap.CanSpawnMobile(x, y, z))
				{
                    start = p;
					break;
				}
			}
				
            return start;
        }

        public virtual Point3D GetStartPoint(ArenaTeam team)
        {
            return 0.5 > Utility.RandomDouble() ? TeamAStartLocation : TeamBStartLocation;
        }

        public virtual void DoWall()
        {
            if (WallItemID <= 0)
            {
                return;
            }

            foreach (var rec in WallArea)
            {
                for (int x = rec.Start.X; x <= rec.End.X; x++)
                {
                    for (int y = rec.Start.Y; y <= rec.End.Y; y++)
                    {
                        Point3D pnt = new Point3D(x, y, ArenaMap.GetAverageZ(x, y));
                        SpellHelper.AdjustField(ref pnt, ArenaMap, 16, false);

                        ArenaWall wall = new ArenaWall(this);
                        wall.MoveToWorld(pnt, ArenaMap);
                        Effects.PlaySound(pnt, ArenaMap, 0x1F6);
                    }
                }
            }
        }

        public virtual void RegionOnEnter(Mobile from)
        {
        }

        public virtual void RegionOnExit(Mobile from)
        {
        }

        public void AddWager(Dictionary<Mobile, Item> wagers)
        {
            Wagers = wagers;
        }

        public void ClearWagers()
        {
            Wagers.Clear();
        }

        public virtual bool CanRegisterFight(Mobile from)
        {
            return CanRegisterFight(from, true);
        }

        public virtual bool CanRegisterFight(Mobile from, bool checkTeam)
        {
            if (CurrentTournament != null || _CurrentFight != null)
            {
                return false;
            }

            if (!ArenaTeam.HasTeam(from))
            {
                return !checkTeam;
            }

            return ArenaHelper.CheckAvailable(from);
        }

        public void AddToQueue(ArenaFight fight)
        {
            if (!Queue.Contains(fight))
            {
                foreach (var m in fight.GetFighters())
                {
                    m.SendMessage(ArenaHelper.ParticipantMessageHue, "Your fight has been added to the queue and will fight in the order in which you were added. You are {0} in the queue.", Queue.Count == 0 ? "next" : String.Format("number {0}", Queue.Count + 1));
                }

                Queue.Add(fight);
            }
        }

        public void RemoveFromQueue(ArenaFight fight)
        {
            if (Queue.Contains(fight))
                Queue.Remove(fight);
        }

        public void CheckQueue()
        {
            if (_CurrentFight == null && !_InUse && m_Active && Queue.Count > 0)
            {
                ArenaFight next = Queue[0];
                next.RegisterFight(EjectDelay);

                for (int i = 0; i < Queue.Count; i++)
                {
                    ArenaFight fight = Queue[i];
 
                    foreach (TeamInfo info in fight.Teams)
                    {
						ArenaTeam team = info.Team;
						
						if(team == null)
							continue;
					
                        foreach (Mobile m in team.Fighters)
                        {
                            if (i == 0)
                                m.SendMessage(ArenaHelper.ParticipantMessageHue, "Your teams fight will begin shortly.");
                            else if (i == 1)
                                m.SendMessage(ArenaHelper.ParticipantMessageHue, "Your team is on deck!");
                            else if (i == 2)
                                m.SendMessage(ArenaHelper.ParticipantMessageHue, "Your team is in the hole!");
                            else 
                                m.SendMessage(ArenaHelper.ParticipantMessageHue, String.Format("You have {0} fights before your fight!", i));
                        }
                    }
                }
            }
        }

        public virtual void RemoveSystem()
        {
            if (Timer != null)
            {
                Timer.Stop();
                Timer = null;
            }

            if (FightRegion != null)
                FightRegion.Unregister();

            if (AudienceRegion != null)
                AudienceRegion.Unregister();

            if (_ArenaKeeper != null)
                _ArenaKeeper.Delete();

            if (TournamentBoard != null)
                TournamentBoard.Delete();

            if (StatsBoard != null)
                StatsBoard.Delete();

            if (TeamsBoard != null)
                TeamsBoard.Delete();

            if (Chest != null)
                Chest.Delete();

            if (Bell != null)
                Bell.Delete();

            if (Stone != null && !Stone.Deleted)
                Stone.Delete();

            m_SystemList.Remove(this);
        }

        public IEnumerable<PlayerMobile> GetAudience()
        {
            if (AudienceRegion == null)
                yield break;

            foreach (var pm in AudienceRegion.GetEnumeratedMobiles().OfType<PlayerMobile>())
            {
                yield return pm;
            }
        }

        /*public void ShowRegionBounds()
        {
            var list = new List<Static>();

            if (FightRegion != null)
            {
                foreach (var rec in FightRegion.Area)
                {
                    for (int x = rec.Start.X; x <= rec.Start.X + rec.Width; x++)
                    {
                        for (int y = rec.Start.Y; y <= rec.Start.Y + rec.Height; y++)
                        {
                            if (x == rec.Start.X || y == rec.Start.Y || x == rec.Start.X + rec.Width || y == rec.Start.Y + rec.Height)
                            {
                                var st = new Static(0x3709);
                                st.MoveToWorld(new Point3D(x, y, ArenaMap.GetAverageZ(x, y)), ArenaMap);

                                list.Add(st);
                            }
                        }
                    }
                }
            }

            if (AudienceRegion != null)
            {
                foreach (var rec in AudienceRegion.Area)
                {
                    for (int x = rec.Start.X; x <= rec.Start.X + rec.Width; x++)
                    {
                        for (int y = rec.Start.Y; y <= rec.Start.Y + rec.Height; y++)
                        {
                            if (x == rec.Start.X || y == rec.Start.Y || x == rec.Start.X + rec.Width || y == rec.Start.Y + rec.Height)
                            {
                                var st = new Static(0x3709);
                                st.Hue = 2;
                                st.MoveToWorld(new Point3D(x, y, ArenaMap.GetAverageZ(x, y)), ArenaMap);

                                list.Add(st);
                            }
                        }
                    }
                }
            }

            Timer.DelayCall<List<Static>>(TimeSpan.FromMinutes(5), lu =>
            {
                foreach (var s in lu)
                {
                    s.Delete();
                }

                Stone.ShowRegionBounds = false;
            }, list);
        }*/

        public virtual void Serialize(GenericWriter writer)
        {
            writer.Write((int)2); //Version

            writer.Write(Bell);

            writer.Write(_Name);

            writer.Write(_UseLinked);
            writer.Write(CanDoTourneys);

            writer.Write(m_Active);
            writer.Write(Chest);
            writer.Write(StatsBoard);
            writer.Write(TournamentBoard);
            writer.Write(TeamsBoard);
            writer.WriteMobile(_ArenaKeeper);

            writer.Write(Tournaments.Count);
            foreach (Tournament tourney in Tournaments)
            {
                tourney.Serialize(writer);
            }

            writer.Write(Wagers.Count);
            foreach (KeyValuePair<Mobile, Item> kvp in Wagers)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }

        public virtual void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                case 1:
                    Bell = reader.ReadItem() as ArenaBell;
                    goto case 0;
                case 0:
                    // new version CTF has its own stuff
                    if (version < 2)
                    {
                        int c = reader.ReadInt();
                        for (int i = 0; i < c; i++)
                        {
                            var flagholder = reader.ReadItem();

                            if (flagholder != null)
                            {
                                flagholder.Delete();
                            }
                        }
                    }

                    _Name = reader.ReadString();

                    _UseLinked = reader.ReadBool();
                    CanDoTourneys = reader.ReadBool();

                    m_Active = reader.ReadBool();
                    Chest = reader.ReadItem() as PrizeChest;
                    StatsBoard = reader.ReadItem() as StatsBoard;
                    TournamentBoard = reader.ReadItem() as TournamentBoard;
                    TeamsBoard = reader.ReadItem() as TeamsBoard;
                    _ArenaKeeper = reader.ReadMobile();

                    if (Chest != null)
                        Chest.System = this;

                    if (TournamentBoard != null)
                        TournamentBoard.System = this;

                    if (StatsBoard != null)
                        StatsBoard.System = this;

                    if (_ArenaKeeper != null && _ArenaKeeper is ArenaKeeper)
                        ((ArenaKeeper)_ArenaKeeper).System = this;

                    if (Bell != null)
                        Bell.System = this;

                    int tourneyCount = reader.ReadInt();

                    if (tourneyCount > 0)
                    {
                        if (Tournaments == null)
                            Tournaments = new List<Tournament>();

                        for (int i = 0; i < tourneyCount; ++i)
                        {
                            Tournament tourney = new Tournament(reader, this);

                            if (tourney != null && !Tournaments.Contains(tourney))
                                Tournaments.Add(tourney);
                        }
                    }

                    int wageCount = reader.ReadInt();

                    if (wageCount > 0)
                    {
                        for (int i = 0; i < wageCount; ++i)
                        {
                            Mobile leader = reader.ReadMobile();
                            Item item = reader.ReadItem();

                            if (item == null)
                                continue;

                            if (leader != null)
                            {
                                leader.BankBox.DropItem(item);
                                item.Movable = true;
                            }
                            else if (Chest != null)
                            {
                                Chest.DropItem(item);
                                item.Movable = true;
                            }
                            else
                                item.Delete();
                        }
                    }
                    break;
            }

            FightRegion = GetFightRegion;
            AudienceRegion = GetAudienceRegion;

            m_SystemList.Add(this);

            if (_UseLinked)
                Timer.DelayCall(TimeSpan.FromSeconds(10), new TimerCallback(SetLinkedSystem));

            if (version == 0)
            {
                Bell = new ArenaBell(this, 0x4C5C);
                Bell.MoveToWorld(BellLocation, ArenaMap);
            }
            else if (version == 1 && (this is CTFArena1 || this is CTFArena2))
            {
                Timer.DelayCall(() =>
                {
                    if (this is CTFArena1)
                    {
                        CTFArena1.Delete(null);
                        CTFArena1.Setup(null);
                        Console.WriteLine("CTF Arena 1 re-configured");
                    }
                    else if (this is CTFArena2)
                    {
                        CTFArena2.Delete(null);
                        CTFArena2.Setup(null);
                        Console.WriteLine("CTF Arena 2 re-configured");
                    }
                });
            }
        }

        public PVPTournamentSystem(GenericReader reader, TournamentStone stone)
        {
            Stone = stone;
            Stone.System = this;

            Deserialize(reader);
        }

        public virtual void HandleKill(TeamInfo killerInfo, TeamInfo victimInfo, Mobile killer, Mobile victim)
        {
            killerInfo.Kills++;
        }

        public virtual void OnAfterEject(Mobile m)
        {
            if (_ArenaKeeper != null)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(1), ArenaHelper.DoResurrect, m);
                Timer.DelayCall(TimeSpan.FromSeconds(3), ArenaHelper.RemoveCurses, m);
            }

            m.CloseGump(typeof(ParticipantGump));
        }

        public virtual void SetLinkedSystem()
        {
        }

        public virtual bool DoStartMessage()
        {
            return true;
        }

        public virtual bool DoStartDelayMessage()
        {
            return true;
        }

        public virtual bool DoTieMessage()
        {
            return true;
        }

        public virtual bool DoEndOfFightMessage(ArenaTeam winner, ArenaTeam loser, bool forfeit)
        {
            return true;
        }

        public bool TournamentNameExists(string name)
        {
            foreach (Tournament tourney in Tournaments)
            {
                if (tourney.Name == name)
                    return true;
            }
            return false;
        }

        #region Static Methods
		public static string GetFightType(ArenaFightType type)
		{
            return ArenaHelper.GetFightType(type);
		}
 
        public static PVPTournamentSystem GetRegionalSystem(Region region)
        {
            if (region is FightRegion)
                return ((FightRegion)region).System;

            if (region is AudienceRegion)
                return ((AudienceRegion)region).System;

            return null;
        }

        public static int MobileNotoriety(Mobile source, IDamageable damageable)
        {
            /*
            Mobile target = damageable as Mobile;

            if (target is Mobile)
            {
                if (target.Blessed ||
                        (target is BaseVendor && ((BaseVendor)target).IsInvulnerable) || 
                        target is PlayerVendor || target is TownCrier)
                    return Notoriety.Invulnerable;

                if (target.AccessLevel > AccessLevel.VIP)
                    return Notoriety.CanBeAttacked;

                if (IsEnemy(source, target))
                    return Notoriety.Enemy;

                if (IsFriendly(source, target))
                    return Notoriety.Ally;
            }
            */
            return Server.Misc.NotorietyHandlers.MobileNotoriety(source, damageable);
        }

        public static bool IsEnemy(Mobile source, Mobile target)
        {
            if (target is BaseCreature && ((BaseCreature)target).GetMaster() != null)
                target = ((BaseCreature)target).GetMaster();

            PVPTournamentSystem system = GetRegionalSystem(source.Region);

            if (system != null && system.CurrentFight != null && source.Region != null && source.Region.IsPartOf(typeof(FightRegion)) && target.Region != null && target.Region.IsPartOf(typeof(FightRegion)))
            {
                TeamInfo sourceInfo = system.CurrentFight.GetTeamInfo(source);
                TeamInfo targetInfo = system.CurrentFight.GetTeamInfo(target);

                return sourceInfo != null && targetInfo != null && sourceInfo != targetInfo;
            }

            return false;
        }

        public static bool IsFriendly(Mobile source, Mobile target)
        {
            if (target is BaseCreature && ((BaseCreature)target).GetMaster() != null)
                target = ((BaseCreature)target).GetMaster();

            PVPTournamentSystem system = GetRegionalSystem(source.Region);

            if (system != null && system.CurrentFight != null && source.Region != null && source.Region.IsPartOf(typeof(FightRegion)) && target.Region != null && target.Region.IsPartOf(typeof(FightRegion)))
            {
                TeamInfo sourceInfo = system.CurrentFight.GetTeamInfo(source);
                TeamInfo targetInfo = system.CurrentFight.GetTeamInfo(target);

                return sourceInfo != null && targetInfo != null && sourceInfo == targetInfo;
            }
            return false;
        }
        #endregion
    }
}
