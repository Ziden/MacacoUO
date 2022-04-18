//#define PVP_UsingNewMultis

using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.TournamentSystem
{
#if PVP_UsingNewMultis
    public abstract class BaseMultiArena : BaseStructure
#else
    public abstract class BaseMultiArena : BaseMulti
#endif
    {
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active 
		{ 
			get 
			{ 
				return Stone != null && Stone.System != null && Stone.System.Active; 
			}
			set
			{
				if(Stone != null && Stone.System != null)
				{
					Stone.System.Active = value;
				}
			}
		}
		
		[CommandProperty(AccessLevel.GameMaster)]
		public TournamentStone Stone { get; set; }

        public abstract ArenaDefinition Definition { get; }

#if PVP_UsingNewMultis
        public BaseMultiArena(string design)
            : base(1)
        {
            ApplyDesignFromFile(Config.DataDirectory + "/MultiDesigns", design);
        }

        public BaseMultiArena(StructureState state)
            : base(state)
        {
        }
#else
        public List<Item> Fixtures = new List<Item>();
#endif
        public BaseMultiArena(int multiID)
            : base(multiID)
        {
        }

        protected void AddMultiFixtures(MultiTileEntry[] entries)
        {
#if PVP_UsingNewMultis
            AddFixtures(entries);
#else
            foreach(var entry in entries)
            {
                Item st = new Static((int)entry.m_ItemID);

                st.MoveToWorld(new Point3D(X + entry.m_OffsetX, Y + entry.m_OffsetY, Z + entry.m_OffsetZ), Map);
                Fixtures.Add(st);
            }
#endif
        }

        private void ConfigureSystem()
		{
			Stone = new AddonArenaStone();
			var sys = new MultiArenaSystem(this, Stone);
			
			Stone.Location = sys.StoneLocation;

            sys.Active = true;
		}

        public virtual void OnSystemConfigured()
        {
        }
		
		public override void OnLocationChange(Point3D oldLoc)
		{
			base.OnLocationChange(oldLoc);

            if (Stone == null)
            {
                ConfigureSystem();
            }

            if (Stone != null)
            {
                var sys = Stone.System;
                Stone.Location = sys.StoneLocation;

                if (sys != null)
                {
                    if (sys.StatsBoard != null)
                        sys.StatsBoard.Location = sys.StatsBoardLocation;

                    if (sys.TournamentBoard != null)
                        sys.TournamentBoard.Location = sys.TournamentInfoBoardLocation;

                    if (sys.TeamsBoard != null)
                        sys.TeamsBoard.Location = sys.TeamsBoardLocation;

                    if (sys.Chest != null)
                        sys.Chest.Location = sys.ChestLocation;

                    int xOffset = X - oldLoc.X;
                    int yOffset = Y - oldLoc.Y;
                    int zOffset = Z - oldLoc.Z;

                    if (sys.ArenaKeeper != null)
                        sys.ArenaKeeper.Location = sys.ArenaKeeperLocation;

                    if (sys.Wagers != null && sys.Wagers.Count > 0)
                    {
                        foreach (var wage in sys.Wagers.Values)
                        {
                            wage.Location = new Point3D(wage.X + xOffset, wage.Y + yOffset, wage.Z + zOffset);
                        }
                    }

                    var list = new List<Mobile>();

                    if (sys.FightRegion != null)
                    {
                        foreach (var m in sys.FightRegion.GetEnumeratedMobiles().Where(mob => mob != sys.ArenaKeeper))
                        {
                            list.Add(m);
                        }

                        sys.FightRegion.Unregister();
                        sys.FightRegion = sys.GetFightRegion;
                    }

                    if (sys.AudienceRegion != null)
                    {
                        foreach (var m in sys.AudienceRegion.GetEnumeratedMobiles().Where(mob => mob != sys.ArenaKeeper && !list.Contains(mob)))
                        {
                            list.Add(m);
                        }

                        sys.AudienceRegion.Unregister();
                        sys.AudienceRegion = sys.GetAudienceRegion;
                    }

                    foreach (var m in list)
                    {
                        m.Location = new Point3D(m.X + xOffset, m.Y + yOffset, m.Z + zOffset);
                    }

                    ColUtility.Free(list);
                }
            }

#if !PVP_UsingNewMultis
            foreach(var fixture in Fixtures)
            {
                fixture.Location = new Point3D(X + (fixture.X - oldLoc.X), Y + (fixture.Y - oldLoc.Y), Z + (fixture.Z - oldLoc.Z));
            }
#endif
        }

        public override void OnMapChange()
		{
			base.OnMapChange();
			
			if(Map == null || Map == Map.Internal)
			{
				if(Stone != null)
				{
					Stone.System.Active = false;
				}
			}
			else if (Stone != null)
			{
				MultiArenaSystem sys = Stone.System as MultiArenaSystem;
				Stone.Map = Map;

                if (sys != null)
                {
                    sys.SetMap(Map);

                    if (sys.StatsBoard != null)
                        sys.StatsBoard.Map = Map;

                    if (sys.TournamentBoard != null)
                        sys.TournamentBoard.Map = Map;

                    if (sys.TeamsBoard != null)
                        sys.TeamsBoard.Map = Map;

                    if (sys.Chest != null)
                        sys.Chest.Map = Map;

                    if (sys.ArenaKeeper != null)
                        sys.ArenaKeeper.Map = Map;

                    if (sys.Wagers != null && sys.Wagers.Count > 0)
                    {
                        foreach (var wage in sys.Wagers.Values)
                        {
                            wage.Map = Map;
                        }
                    }

                    if (sys.FightRegion != null)
                    {
                        foreach (var m in sys.FightRegion.GetEnumeratedMobiles())
                        {
                            m.Map = Map;
                        }

                        sys.FightRegion.Unregister();
                        sys.FightRegion = sys.GetFightRegion;
                    }

                    if (sys.AudienceRegion != null)
                    {
                        sys.AudienceRegion.Unregister();
                        sys.AudienceRegion = sys.GetAudienceRegion;
                    }
                }
                else
                {
                    Stone.Delete();
                }
			}

#if !PVP_UsingNewMultis
            foreach(var fixture in Fixtures)
            {
                fixture.Map = Map;
            }
#endif
        }

        public override void Delete()
		{
			base.Delete();

            if (Stone != null)
            {
                Stone.Delete();
            }

#if !PVP_UsingNewMultis
            foreach(var fixture in Fixtures)
            {
                fixture.Delete();
            }

            ColUtility.Free(Fixtures);
#endif
        }

        public BaseMultiArena(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			
			writer.Write(Stone);

#if !PVP_UsingNewMultis
            writer.WriteItemList<Item>(Fixtures, true);
#endif
        }

        public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
			
			Stone = reader.ReadItem() as TournamentStone;

#if !PVP_UsingNewMultis
            Fixtures = reader.ReadStrongItemList<Item>();
#endif
        }
    }
	
	public class MultiArenaSystem : PVPTournamentSystem
	{
		public override Rectangle2D KickZone { get { return ConvertOffset(Definition.KickZone); } }
		public override Rectangle2D[] WallArea { get { return ConvertOffset(Definition.WallArea); } }
		
		public override Rectangle2D[] FightingRegionBounds { get { return ConvertOffset(Definition.FightingRegionBounds); } }
		public override Rectangle2D[] AudienceRegionBounds { get { return ConvertOffset(Definition.AudienceRegionBounds); } }
        public override Rectangle2D RandomStartBounds { get { return ConvertOffset(Definition.RandomStartBounds); } }

        public override Point3D StoneLocation { get { return ConvertOffset(Definition.StoneLocation); } }
		public override Point3D TeamAStartLocation { get { return ConvertOffset(Definition.TeamAStartLocation); } }
		public override Point3D TeamBStartLocation { get { return ConvertOffset(Definition.TeamBStartLocation); } }
		public override Point3D ArenaKeeperLocation { get { return ConvertOffset(Definition.ArenaKeeperLocation); } }
		public override Point3D TeamAWageDisplay { get { return ConvertOffset(Definition.TeamAWageDisplay); } }
		public override Point3D TeamBWageDisplay { get { return ConvertOffset(Definition.TeamBWageDisplay); } }
		public override Point3D StatsBoardLocation { get { return ConvertOffset(Definition.StatsBoardLocation); } }
		public override Point3D TournamentInfoBoardLocation { get { return ConvertOffset(Definition.TournamentInfoBoardLocation); } }
		public override Point3D TeamsBoardLocation { get { return ConvertOffset(Definition.TeamsBoardLocation); } }
		public override Point3D ChestLocation { get { return ConvertOffset(Definition.ChestLocation); } }
        public override Point3D BellLocation { get { return ConvertOffset(Definition.BellLocation); } }

        private Map _Map;
		private BaseMultiArena _Multi;

        public override string DefaultName
        {
            get
            {
                string name = string.Empty;
                int i = 0;

                do
                {
                    name = String.Format("PVP Arena #{0}", ++i);
                }
                while (!ValidateName(name));

                return name;
            }
        }

		[CommandProperty(AccessLevel.GameMaster)]
		public BaseMultiArena Multi 
		{
			get { return _Multi; }
			set
			{
				if(_Multi != null && value == null)
				{
                    if (Stone != null)
                    {
                        Stone.Delete();
                    }
				}

                _Multi = value;
			}
		}

        public override SystemType SystemType { get { return SystemType.Custom; } }
        public override Map ArenaMap { get { return _Map; } }
		public override ArenaDefinition Definition { get { return _Multi.Definition; } }
		
		public MultiArenaSystem(BaseMultiArena multi, TournamentStone stone)
			: base(stone)
		{
            _Multi = multi;

            if (!string.IsNullOrEmpty(_Multi.DefaultName))
            {
                Name = _Multi.DefaultName;
            }

            UseLinked = false;
		}

        public void SetMap(Map map)
        {
            _Map = map;
        }
		
		private Point3D ConvertOffset(Point3D offset)
		{
            return new Point3D(_Multi.X + offset.X, _Multi.Y + offset.Y, _Multi.Z + offset.Z);
		}
		
		private Rectangle2D ConvertOffset(Rectangle2D rec)
		{
            return new Rectangle2D(_Multi.X + rec.X, _Multi.Y + rec.Y, rec.Width, rec.Height);
		}
		
		private Rectangle2D[] ConvertOffset(Rectangle2D[] recs)
		{
            var newRec = new Rectangle2D[recs.Length];
			
			for(int i = 0; i < recs.Length; i++)
			{
				newRec[i] = ConvertOffset(recs[i]);
			}
			
			return newRec;
		}

        public override void OnSystemConfigured()
        {
            if (_Multi != null)
            {
                _Multi.OnSystemConfigured();
            }
        }

        private bool _Loading;

        public override FightRegion GetFightRegion { get { return _Loading ? null : new FightRegion(this); } }
        public override AudienceRegion GetAudienceRegion { get { return _Loading ? null : new AudienceRegion(this); } }

        public MultiArenaSystem(GenericReader reader, TournamentStone stone)
            : base(reader, stone)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
            writer.Write(_Multi);
            writer.Write(_Map);

			base.Serialize(writer);
			writer.Write(0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
            _Multi = reader.ReadItem() as BaseMultiArena;
            _Map = reader.ReadMap();

            _Loading = true;

			base.Deserialize(reader);
			reader.ReadInt();

            _Loading = false;

            if (_Multi == null)
            {
                if (Stone != null)
                {
                    Timer.DelayCall(() => Stone.Delete());
                }
            }
            else
            {
                Timer.DelayCall(() =>
                    {
                        if (FightRegion != null)
                        {
                            FightRegion.Unregister();
                            FightRegion = null;
                        }

                        if (AudienceRegion != null)
                        {
                            AudienceRegion.Unregister();
                            AudienceRegion = null;
                        }

                        FightRegion = GetFightRegion;
                        AudienceRegion = GetAudienceRegion;
                    });
            }
		}
	}
	
	public class AddonArenaStone : TournamentStone
	{
		public AddonArenaStone()
		{
		}
		
		public AddonArenaStone(Serial serial)
            : base(serial)
		{
		}

        public override void Delete()
        {
            if (System != null && System is MultiArenaSystem && ((MultiArenaSystem)System).Multi != null && !((MultiArenaSystem)System).Multi.Deleted)
            {
                ((MultiArenaSystem)System).Multi.Delete();
            }

            base.Delete();
        }

		public override void LoadSystem(GenericReader reader)
		{
			System = new MultiArenaSystem(reader, this);
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
	}
}
