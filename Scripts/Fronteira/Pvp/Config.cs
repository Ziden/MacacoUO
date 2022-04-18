using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Items;

namespace Server.TournamentSystem
{
    /// <summary>
    /// Default values for various properties within the system
    /// </summary>
    public static class Config
    {
        public static string Version = "1.5.0.0";

        /*
        [CallPriority(int.MaxValue)]
        public static void Initialize()
        {
            Notoriety.Handler = new NotorietyHandler(PVPTournamentSystem.MobileNotoriety);
        }
        */

        #region Persistence
        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        public static string FilePath = Path.Combine("Saves/Customs", "PvPTournamentSystem.bin");
        public static bool Configured { get { return PVPTournamentSystem.SystemList.Count > 0; } }

        public static void OnSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(
                FilePath,
                writer =>
                {
                    writer.Write(4);

                    writer.Write(ArenaTeam.Teams.Count);

                    for (int i = 0; i < ArenaTeam.Teams.Count; ++i)
                        ArenaTeam.Teams[i].Serialize(writer);

                    // Version 4 leaderboard reset
                    LeaderStats.Save(writer);

                    // Version 3 Leagues
                    League.Save(writer);

                    writer.Write(PVPTournamentSystem.SystemList.Count);

                    for (int i = 0; i < PVPTournamentSystem.SystemList.Count; ++i)
                    {
                        var sys = PVPTournamentSystem.SystemList[i];

                        writer.Write((int)sys.SystemType);
                        writer.Write(sys.Stone);

                        sys.Serialize(writer);
                    }

                    ForcedGear.Serialize(writer);

                    writer.Write(true);

                    writer.Write(PVPTournamentStats.TournamentStats.Count);
                    for (int i = 0; i < PVPTournamentStats.TournamentStats.Count; ++i)
                        PVPTournamentStats.TournamentStats[i].Serialize(writer);

                    Timer.DelayCall(TimeSpan.FromSeconds(5), () =>
                    {
                        ArenaTeam.DefragTeams();
                    });
                });
        }

        public static void OnLoad()
        {
            Utility.WriteConsoleColor(ConsoleColor.Cyan, "*** Loading PVP Tournament System, Version {0}...", Version);
            var loaded = true;

            Persistence.Deserialize(
                FilePath,
                reader =>
                {
                    int version = reader.ReadInt();
                    int count = 0;

                    switch (version)
                    {
                        case 4:
                        case 3:
                        case 2:
                            count = reader.ReadInt();
                            
                            for (int i = 0; i < count; i++)
                                new ArenaTeam(reader);

                            if (version >= 4)
                            {
                                LeaderStats.Load(reader);
                            }

                            // Version 3, Leagues
                            if (version >= 3)
                            {
                                League.Load(reader);
                            }

                            count = reader.ReadInt();
                            
                            for (int i = 0; i < count; i++)
                            {
                                var type = (SystemType)reader.ReadInt();
                                var stone = reader.ReadItem() as TournamentStone;

                                ConstructArena(type, reader, stone);
                            }
                            goto case 1;
                        case 1:
                            ForcedGear.Deserialize(reader);
                            goto case 0;
                        case 0:
                            loaded = reader.ReadBool();

                            count = reader.ReadInt();

                            for (int i = 0; i < count; i++)
                                new PVPTournamentStats(reader);

                            // Moved teams to save/load first
                            if (version == 1)
                            {
                                count = reader.ReadInt();

                                for (int i = 0; i < count; i++)
                                    new ArenaTeam(reader);
                            }

                            Timer.DelayCall(() =>
                            {
                                ArenaTeam.DefragTeams();
                            });
                            break;
                    }

                    if (PVPTournamentSystem.SystemList.Any(a => a.Tournaments.Count > 0))
                    {
                        Timer.DelayCall(() => TournamentBoard.UpdateBoards());
                    }
                });

            if (!loaded)
            {
                Utility.WriteConsoleColor(ConsoleColor.Cyan, "Running system for first time. Be sure to use [SetupPVPTournamentSystem to configure system.");
            }
            else
            {
                Utility.WriteConsoleColor(ConsoleColor.Cyan, "Complete! {0} arenas setup with {1} active arena teams.", PVPTournamentSystem.SystemList.Count.ToString(), ArenaTeam.Teams.Where(t => t.Active).Count().ToString());
            }

            CheckDataDirectory();
        }

        private static void CheckDataDirectory()
        {
            var dir = DataDirectory;

            if (dir == null)
            {
                Utility.WriteConsoleColor(ConsoleColor.Red, "System Could not find Data Directory. Make sure the '{0}' folder exists.", DataLocation);
            }
            else
            {
                Utility.WriteConsoleColor(ConsoleColor.Cyan, "Data Directory Found: {0}.", dir);
            }
        }

        private static void ConstructArena(SystemType type, GenericReader reader, TournamentStone stone)
        {
            switch (type)
            {
                case SystemType.NewHavenTram: new HavenArena(reader, stone); break;
                /*
                case SystemType.NewHavenFel: new HavenFelArena(reader, stone); break;
                case SystemType.KhaldunTram: new KhaldunArenaTram(reader, stone); break;
                case SystemType.KhaldunFel: new KhaldunArena(reader, stone); break;
                case SystemType.CTF1: new CTFArena1(reader, stone); break;
                case SystemType.CTF2: new CTFArena2(reader, stone); break;
                case SystemType.Tokuno: new TokunoArena(reader, stone); break;
                case SystemType.Custom: new MultiArenaSystem(reader, stone); break;
                case SystemType.CTFRoyalRumble: new CTFRoyalRumbleArena(reader, stone); break;
                */
            }
        }
        #endregion

        #region System
        public static bool Debug { get { return false; } }

        /// <summary>
        /// how often Leader Board Stats reset. Anything but 30 days will be true time. 30 days will auto correct to 12am on the 1st of each month.
        /// </summary>
        public static readonly TimeSpan LeaderboardStatReset = TimeSpan.FromDays(30);

        /// <summary>
        /// Expire time for messages
        /// </summary>
        public static TimeSpan DefaultMessageExpire = TimeSpan.FromDays(7);

        /// <summary>
        /// Clear reagion after fight of mobiles, movable items, etc.
        /// </summary>
        public static bool NoClearRegion { get { return false; } }

        /// <summary>
        /// Uses pretty hues of fel/tram stones. If you want to use custom hues, mark this false and manually change the hue.
        /// </summary>
        public static bool UseStoneDefaultHue { get { return true; } }

        public static string DataLocation { get { return Path.Combine("PVP Tourney System", "Data"); } }

        /// <summary>
        /// This is where data will be stored.
        /// </summary>
        private static string _DataDirectory;

        public static string DataDirectory
        {
            get
            {
                return _DataDirectory ?? (_DataDirectory = ArenaHelper.GetDataDirectory(DataLocation));
            }
        }
       // public static string DataDirectory { get { return "PVP Tourney System/Data"; } }
        #endregion

        #region Arena Fights

        /// <summary>
        /// Do we want to nullify points/restrict fights when fighters are same IP? This could help combat point farming
        /// </summary>
        public static readonly bool RestrictSameIP = true;

        /// <summary>
        /// Can players be attacked in audience region?
        /// </summary>
        public static readonly bool AudienceRegionNoAttack = true;

        /// <summary>
        /// Can spells be cast in audience region?
        /// </summary>
        public static readonly bool AudienceRegionNoSpells = true;

        /// <summary>
        /// Randomized or static start locations for standard duels
        /// </summary>
        public static readonly bool RandomizeStartLocations = true;

        /// <summary>
        /// Team Hues, mainly for Capture the Flag 
        /// </summary>
        public static readonly int[] TeamHues = new[] { 37, 5, 1175, 1158 }; // Red, blue, dark gray, dark purple

        /// <summary>
        /// Team Hue Names, see above
        /// </summary>
        public static readonly string[] HueNames = new[] { "Red", "Blue", "Gray", "Purple" }; // Red, blue, dark gray, dark purple

        /// <summary>
        /// Randomized or static start locations for CTF 
        /// </summary>
        public static readonly bool CTFRandomStart = true;

        /// <summary>
        /// Enabled CTF Team Rumble
        /// </summary>
        public static readonly bool TeamRumbleCTFEnabled = true;

        /// <summary>
        /// Minimum Teams per 2 flags
        /// </summary>
        public static readonly int TeamRumbleCTFMinPlayerCount = 4;

        /// <summary>
        /// Time from when the team rumble is next in the fight queue (must meet min player reqs to do so) when fight begins
        /// </summary>
        public static readonly TimeSpan TeamRumbleCTFStartDelay = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Time from when the team rumble is created and when it expires if not enough players join
        /// </summary>
        public static readonly TimeSpan TeamRumbleCTFExpirePeriod = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Delay from the moment a duel is agreed upon, to when you are teleported into the arena
        /// </summary>
        public static readonly TimeSpan StartDelay = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Delay in which you are removed from the arena at the conclusion of the duel
        /// </summary>
        public static readonly TimeSpan EjectDelay = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Delay from when you are teleported into the arena, to when the duel actuall begins. This is also known as the wall period
        /// </summary>
        public static readonly TimeSpan PreFightDelay = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Default time the duel will last
        /// </summary>
        public static readonly TimeSpan DefaultDuration = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Minimum teams you need to start a last man standing fight
        /// </summary>
        public static readonly int MinLastManStanding = 3;

        /// <summary>
        /// Default team type (single, twosome, foursome) for a standard duel
        /// </summary>
        public static readonly ArenaTeamType DefaultTeamType = ArenaTeamType.Single;

        /// <summary>
        /// Gives players teh option of using own gear, or standardized gear for an even playing field.
        /// </summary>
        public static readonly bool AllowStandardizedGear = true;

        public static readonly Type[] NonConsumableList =
        {
            typeof(BasePotion), typeof(SpellScroll), typeof(OrangePetals), typeof(RoseOfTrinsicPetal), typeof(BaseBalmOrLotion), typeof(BaseMagicalFood),
            typeof(Food), typeof(BaseBeverage), typeof(BaseFishPie)
        };

        #endregion

        #region Tournaments
        /// <summary>
        /// The time prior to the actual tournament start time where the arena will be come un-available for other duels
        /// </summary>
        public static readonly TimeSpan TournamentSetupTime = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Time between duels during a tournament
        /// </summary>
        public static readonly TimeSpan FightWait = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Time between each round in a tournamnet
        /// </summary>
        public static readonly TimeSpan RoundWait = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Minimum time (from current time) you may schedule a tournament. For example, if its the 1st of the month, you have to schedule the tournament 
        /// out to at least the sixth of the month.
        /// </summary>
        public static readonly TimeSpan TournamentWait = TimeSpan.FromDays(5);

        /// <summary>
        /// In the event the server restarts during a tournament, this will be the wait time for players to get ready once the server is
        /// rebooted.
        /// </summary>
        public static readonly TimeSpan TournamentResumeTime = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Minimum participants a tounrmanet must have to resume. If minimum is not met, the tournament will be canceled
        /// </summary>
        public static readonly int MinEntries = 4;

        /// <summary>
        /// Maximum participants in a tournament
        /// </summary>
        public static readonly int MaxEntries = 40;

        /// <summary>
        /// Max Tournament Entry Fee. Set to -1 if you don't want a max entry fee
        /// </summary>
        public static readonly int MaxEntryFee = 500000;

        /// <summary>
        /// Maximum tournaments each arena can have pending at one time
        /// </summary>
        public static readonly int MaxTournaments = 10;

        /// <summary>
        /// Use alternate arena default value. If the arena has a fel/tram counterpart, it will use that arena for the arena duels, 
        /// as well as the actual arena. This is recommended for large tournaments and to speed the tournament up.
        /// </summary>
        public static readonly bool UseAlternateArena = false;
        #endregion

        #region Leagues
        public static bool LeaguesActive { get { return true; } }

        public static AccessLevel LeagueRegistrationAccess { get { return AccessLevel.GameMaster; } }

        public static int LeagueMinNotify { get { return 0; } }

        public static int MinLeagueTeams { get { return 2; } }

        /// <summary>
        /// Cannot get a lineup for over 20 :(
        /// </summary>
        public static int MaxLeagueTeams { get { return 20; } }
        #endregion
    }
}
