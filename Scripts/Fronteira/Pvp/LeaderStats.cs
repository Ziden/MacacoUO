using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Commands;

namespace Server.TournamentSystem
{
    public class LeaderStats
    {
        public static void ForceResetLeaderBoard(CommandEventArgs e)
        {
            NextReset = DateTime.Now;
            CheckLeaderBoard();
        }

        public ArenaTeam Team { get; set; }
        public int SingleElimWins { get; set; }
        public int BestOf3Wins { get; set; }
        public int LastManStandingWins { get; set; }
        public int CTFWins { get; set; }
        public int CTFTeamRumbleWins { get; set; }

        public int TotalWins { get { return SingleElimWins + BestOf3Wins + LastManStandingWins + CTFWins + CTFTeamRumbleWins; } }

        public LeaderStats(ArenaTeam team)
        {
            Team = team;
        }

        public LeaderStats(GenericReader reader)
        {
            reader.ReadInt();

            Team = ArenaTeam.GetTeam(reader.ReadString());

            SingleElimWins = reader.ReadInt();
            BestOf3Wins = reader.ReadInt();
            LastManStandingWins = reader.ReadInt();
            CTFWins = reader.ReadInt();
            CTFTeamRumbleWins = reader.ReadInt();
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(Team.Name);

            writer.Write(SingleElimWins);
            writer.Write(BestOf3Wins);
            writer.Write(LastManStandingWins);
            writer.Write(CTFWins);
            writer.Write(CTFTeamRumbleWins);
        }

        public static List<LeaderStats> Stats = new List<LeaderStats>();
        public static List<LeaderHistory> History = new List<LeaderHistory>();

        public static DateTime NextReset { get; set; }

        public static void OnFightEnd(ArenaTeam team, ArenaFightType fightType)
        {
            if (team.Temp)
            {
                foreach (var fighter in team.Fighters)
                {
                    var single = ArenaTeam.GetTeam(fighter, ArenaTeamType.Single);

                    if (single != null)
                    {
                        RecordWin(single, fightType);
                    }
                }
            }
            else
            {
                RecordWin(team, fightType);
            }
        }

        public static void RecordWin(ArenaTeam team, ArenaFightType fightType)
        {
            var stats = GetStats(team);

            switch (fightType)
            {
                case ArenaFightType.SingleElimination: stats.SingleElimWins++; break;
                case ArenaFightType.BestOf3: stats.BestOf3Wins++; break;
                case ArenaFightType.LastManStanding: stats.LastManStandingWins++; break;
                case ArenaFightType.CaptureTheFlag: stats.CTFWins++; break;
                case ArenaFightType.TeamRumbleCaptureTheFlag: stats.CTFTeamRumbleWins++; break;
            }
        }

        public static LeaderStats GetStats(ArenaTeam team)
        {
            var stats = Stats.FirstOrDefault(s => s.Team == team);

            if (stats == null)
            {
                stats = new LeaderStats(team);
                Stats.Add(stats);
            }

            return stats;
        }

        public static void CheckLeaderBoard()
        {
            if (Config.LeaderboardStatReset == TimeSpan.Zero)
                return;

            if (NextReset == DateTime.MinValue)
            {
                if (Config.LeaderboardStatReset == TimeSpan.FromDays(30))
                {
                    var now = DateTime.Now;
                    NextReset = new DateTime(now.Year, now.Month, 1, 0, 0, 0).AddMonths(1);
                }
                else
                {
                    NextReset = DateTime.Now + Config.LeaderboardStatReset;
                }
            }

            if (Config.LeaderboardStatReset == TimeSpan.FromDays(30))
            {
                var now = DateTime.Now;

                if (NextReset.Month == now.Month)
                {
                    OnReset(new DateTime(now.Year, now.Month, 1, 0, 0, 0).AddMonths(1));
                }
            }
            else if (NextReset < DateTime.Now)
            {
                OnReset(DateTime.Now + Config.LeaderboardStatReset);
            }
        }

        /// <summary>
        /// Invokes an event for shard owners to customzize how they handle the top leaders for each fight type
        /// </summary>
        /// <param name="dt"></param>
        public static void OnReset(DateTime dt)
        {
            NextReset = dt;

            foreach (int i in Enum.GetValues(typeof(ArenaFightType)))
            {
                var list = Stats.OrderByDescending(s => CheckFightTypeFilter(s, i)).ToList();

                if (list.Count > 0)
                {
                    var top = CheckFightTypeFilter(list[0], i);

                    if (top > 0)
                    {
                        foreach (var stat in list.Where(s => CheckFightTypeFilter(s, i) == top))
                        {
                            TournyEventSink.InvokeOnLeaderBoardReset(new LeaderBoardResetEventArgs(stat.Team, (ArenaFightType)i));
                        }
                    }
                }
            }

            AddToHistory();
            Stats.Clear();
        }

        public static string FormatHistoricLeaderBoard(LeaderHistory history)
        {
            var timeStamp = history.TimeStamp;
            var display = timeStamp - TimeSpan.FromDays(1);

            return display.ToShortDateString();
        }

        public static string FormatHistoricLeaderBoardByMonthYear(DateTime dt)
        {
            var display = dt - TimeSpan.FromDays(1);

            return string.Format("{0} de {1}", display.ToString("MMMM"), display.Year.ToString());
        }

        public static bool CheckTeamTypeFilter(LeaderStats stat, int teamType)
        {
            return teamType == -1 || stat.Team.TeamType == (ArenaTeamType)teamType;
        }

        public static int CheckFightTypeFilter(LeaderStats stat, int fightType)
        {
            switch (fightType)
            {
                default:
                case -1: return stat.TotalWins;
                case 0: return stat.SingleElimWins;
                case 1: return stat.BestOf3Wins;
                case 2: return stat.LastManStandingWins;
                case 3: return stat.CTFWins;
                case 4: return stat.CTFTeamRumbleWins;
            }
        }

        public static void AddToHistory()
        {
            if (Stats.Count > 0)
            {
                History.Add(new LeaderHistory(Stats));
            }
        }

        public static void Save(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(NextReset);

            writer.Write(Stats.Count);

            for (int i = 0; i < Stats.Count; i++)
            {
                Stats[i].Serialize(writer);
            }

            writer.Write(History.Count);

            for (int i = 0; i < History.Count; i++)
            {
                History[i].Serialize(writer);
            }
        }

        public static void Load(GenericReader reader)
        {
            reader.ReadInt(); // version

            NextReset = reader.ReadDateTime();

            var count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                var stats = new LeaderStats(reader);

                if (stats.Team != null)
                {
                    Stats.Add(stats);
                }
            }

            count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                History.Add(new LeaderHistory(reader));
            }

            Timer.DelayCall(() =>
            {
                CheckLeaderBoard();
            });
        }
    }

    public class LeaderHistory : List<LeaderStats>
    {
        public DateTime TimeStamp { get; set; }

        public LeaderHistory(IEnumerable<LeaderStats> list)
            : base(list)
        {
            TimeStamp = DateTime.Now;
        }

        public LeaderHistory(GenericReader reader)
        {
            reader.ReadInt();

            TimeStamp = reader.ReadDateTime();

            var count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                Add(new LeaderStats(reader));
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(TimeStamp);

            writer.Write(Count);

            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(writer);
            }
        }
    }
}
