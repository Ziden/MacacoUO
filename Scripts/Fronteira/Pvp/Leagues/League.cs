using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Gumps;
using Server.Items;
using Server.Commands;

namespace Server.TournamentSystem
{
    public enum EliminationFormat
    {
        None,
        Random,
        StrongVsWeak,
        StrongVsStrong,
    }

    [PropertyObject]
    public class League
    {
        // Fight Info
        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaFightType FightType { get; set; } // all but last man standing

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeamType TeamType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public FightRules Rules { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int FightDuration { get; set; } // in minutes

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AllowTies { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UseOwnGear { get; set; }

        // League Info
        [CommandProperty(AccessLevel.GameMaster)]
        public string Name { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Rounds { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RoundDuration { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime StartTime { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxTeams { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinTeams { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EntryFee { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public EliminationFormat EliminationFormat { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public TournamentStone ForcedArena { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool CanAcceptTeams { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PreLeague { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasBegun { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool EliminationComplete { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool EliminationPeriod { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Round { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime RoundEnds { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public LeagueRecord Record { get { return GetLeagueRecord(); } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RePick
        {
            get { return false; }
            set
            {
                if (value)
                {
                    GetLeagueRecord().RoundLineup.Clear();

                    Shuffle(Teams);

                    var lineup = GetLineup(this);

                    if (lineup != null && ValidateLineup(lineup))
                    {
                        GetLeagueRecord().RoundLineup = lineup;
                    }
                    else
                    {
                        BuildLineup();
                    }
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RecordCurrentLineup
        {
            get { return false; }
            set { RecordLineup(); }
        }

        public Dictionary<Mobile, int> Sponsors { get; set; }
        public List<ArenaTeam> Teams = new List<ArenaTeam>();

        [CommandProperty(AccessLevel.GameMaster)]
        public int TotalPot
        {
            get
            {
                var pot = Teams.Sum(t => t.Fighters.Count) * EntryFee;

                if (Sponsors != null)
                {
                    pot += Sponsors.Sum(kvp => kvp.Value);
                }

                return pot;
            }
        }

        // Championship Elimination
        public List<ArenaTeam> EliminationTeams { get; set; }

        public static List<League> Leagues { get; set; }
        public static List<LeagueRecord> LeagueRecords { get; set; }

        public League()
        {
            MinTeams = 8;
            TeamType = ArenaTeamType.Single;
            FightDuration = 10;
            UseOwnGear = true;
            RoundDuration = 3;
            MaxTeams = Config.MaxLeagueTeams;
            EntryFee = 10000;
            EliminationFormat = EliminationFormat.StrongVsWeak;
        }

        public static void AddLeague(League league)
        {
            if (Leagues == null)
            {
                Leagues = new List<League>();
            }

            if (!Leagues.Contains(league))
            {
                Leagues.Add(league);
            }
        }

        public static void RemoveLeague(League league)
        {
            if (Leagues != null && Leagues.Contains(league))
            {
                league.GetLeagueRecord().CompleteLeague();
                Leagues.Remove(league);
            }
        }

        public bool TryAddTeam(Mobile leader, ArenaTeam team)
        {
            if (Teams.Contains(team))
            {
                leader.SendMessage("You are already signed up for this league!");
            }
            else if (Teams.Count >= MaxTeams)
            {
                leader.SendMessage("This league is already full.");
            }
            else
            {
                foreach (var pm in team.Fighters)
                {
                    if (Banker.GetBalance(pm) < EntryFee)
                    {
                        leader.SendMessage("{0} does not have the {1} entry fee in their bank account.", pm.Name, EntryFee.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                        return false;
                    }
                }

                foreach (var pm in team.Fighters)
                {
                    Banker.Withdraw(pm, EntryFee, false);
                    pm.SendMessage("{0} gold has been withrawn from your account for league entry fee.", EntryFee.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                }

                AddTeam(team);

                ArenaHelper.DoTeamPM(
                    team,
                    "League Registration",
                    String.Format("Your {0} person arena team has been registered in the {1} Arena League. The league will begin at {2} on {3}. See your nearest tournamenet stone for details.", team.TeamType.ToString(), Name, StartTime.ToString("hh:mm tt"), StartTime.Date.ToString("d")),
                    leader);

                return true;
            }

            return false;
        }

        public void AddTeam(ArenaTeam team)
        {
            var record = GetLeagueRecord();

            record.AddTeam(team);

            Teams.Add(team);
        }

        public void RemoveTeam(ArenaTeam team)
        {
            if (Teams != null && Teams.Contains(team))
            {
                Teams.Remove(team);
            }
        }

        public void BeforeBeginLeague()
        {
            if (ValidateLeague())
            {
                Shuffle(Teams);
                Rounds = Teams.Count - 1;

                var lineup = GetLineup(this);

                if (lineup != null && ValidateLineup(lineup))
                {
                    GetLeagueRecord().RoundLineup = lineup;
                }
                else
                {
                    BuildLineup();
                }

                PreLeague = true;

                foreach (var team in Teams)
                {
                    ArenaHelper.DoTeamPM(
                        team,
                        String.Format("League {0} Starting Soon!", Name),
                        String.Format("League matches will begin {0}! View the league boards located at each arena to see the round lineups!", StartTime.ToShortDateString()),
                        null);
                }
            }
        }

        private Dictionary<int, List<RoundMatch>> BuildLineupStep2()
        {
            var pairs = new List<RoundMatch>();

            for (int i = 0; i < Teams.Count; i++)
            {
                var team = Teams[i];

                foreach (var t in Teams.Where(t => t != team && !pairs.Any(m => (m.Team1 == team && m.Team2 == t) || (m.Team1 == t && m.Team2 == team))))
                {
                    pairs.Add(new RoundMatch(team, t));
                }
            }

            Dictionary<int, List<RoundMatch>> lineup = null;

            do
            {
                lineup = BuildLineupStep3(pairs);
            }
            while (lineup == null);

            ColUtility.Free(pairs);

            return lineup;
        }

        private Dictionary<int, List<RoundMatch>> BuildLineupStep3(List<RoundMatch> pairs)
        {
            var lineup = new Dictionary<int, List<RoundMatch>>();

            for (int i = 1; i <= Rounds; i++)
            {
                lineup[i] = new List<RoundMatch>();
            }

            for (int i = 0; i < Teams.Count; i++)
            {
                var team = Teams[i];
                var list = new List<RoundMatch>(pairs.Where(t => t.Team1 == team || t.Team2 == team));

                for (int j = 0; j < list.Count; j++)
                {
                    var pair = list[j];
                    var rounds = lineup.Keys.Where(round => !HasMatch(lineup[round], pair.Team1) && !HasMatch(lineup[round], pair.Team2)).ToList();

                    if (rounds.Count > 0)
                    {
                        try
                        {
                            lineup[rounds[Utility.Random(rounds.Count)]].Add(pair);
                        }
                        catch
                        {
                            Console.WriteLine("ERROR: Count: {0} [{1} Teams]; lineup Count {2}: ", rounds.Count, Teams.Count, lineup.Count);
                        }
                    }
                }

                ColUtility.Free(list);
            }

            if (pairs.Any(m => GetMatches(lineup, m.Team1, m.Team2) == 0))
            {
                lineup.Clear();
                return null;
            }

            return lineup;
        }

        private void BuildLineup()
        {
            Console.WriteLine("Building {0} team lineup, this may take a few minutes...", Teams.Count);

            GetLeagueRecord().RoundLineup = BuildLineupStep2();

            Console.WriteLine("...Done!");
        }

        private bool HasMatch(int round, ArenaTeam team)
        {
            var lineup = GetLeagueRecord().RoundLineup;

            if (lineup.ContainsKey(round))
            {
                return HasMatch(lineup[round], team);
            }

            return false;
        }

        private bool HasMatch(List<RoundMatch> list, ArenaTeam team)
        {
            return list.Any(match => match.Team1 == team || match.Team2 == team);
        }

        private int GetMatches(ArenaTeam one, ArenaTeam two)
        {
            return GetMatches(GetRoundLineup(), one, two);
        }

        private int GetMatches(Dictionary<int, List<RoundMatch>> matches, ArenaTeam one, ArenaTeam two)
        {
            var count = 0;

            foreach (var kvp in matches)
            {
                foreach (var match in kvp.Value)
                {
                    if ((match.Team1 == one && match.Team2 == two) || (match.Team1 == two && match.Team2 == one))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public bool ValidateLineup(Dictionary<int, List<RoundMatch>> lineup)
        {
            foreach (var team in Teams)
            {
                for (int i = 0; i < Rounds; i++)
                {
                    if (!HasMatch(lineup[i + 1], team))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void BeginLeague()
        {
            NewRound();

            foreach (var team in Teams)
            {
                ArenaHelper.DoTeamPM(
                    team,
                    String.Format("League {0} Has Begun!", Name),
                    String.Format("League matches have begun! You will have {0} days to complete each round match!", RoundDuration),
                    null);
            }

            HasBegun = true;
        }

        public void ProcessTick()
        {
            if (RoundEnds < DateTime.Now)
            {
                if (PendingLeagueFight())
                {
                    return;
                }

                if (EliminationPeriod)
                {
                    if (EliminationComplete)
                    {
                        EndLeague();
                    }
                    else if (!CheckEndElimination())
                    {
                        NewRound();
                    }
                }
                else if (Round == Rounds)
                {
                    EndLeague();
                }
                else
                {
                    NewRound();
                }
            }
        }

        public bool EndRound()
        {
            if (Round == 0)
            {
                return true;
            }

            var record = GetLeagueRecord();

            if (EliminationPeriod)
            {
                bool pending = false;
                var lineup = GetEliminationRoundLineup();

                foreach (var match in lineup[Round].Where(m => m.PendingFight))
                {
                    ArenaHelper.DoTeamPM(
                        match.Team1,
                        "Elimination Match!",
                        "Your team missed its match! You must complete your match to move to the next round!",
                        null);

                    ArenaHelper.DoTeamPM(
                       match.Team2,
                       "Elimination Match!",
                       "Your team missed its match! You must complete your match to move to the next round!",
                       null);

                    if (!pending)
                    {
                        pending = true;
                    }
                }

                if (pending)
                {
                    return false;
                }

                GetLeagueRecord().OnEliminationRoundEnd(this);
            }
            else
            {
                record.RankTeams();
                var lineup = GetRoundLineup();

                if (lineup.Count == 0)
                {
                    CancelLeague("Invalid League Lineups");

                    return false;
                }

                var matches = new List<RoundMatch>(lineup[Round].Where(m => m.PendingFight));

                foreach (var match in matches)
                {
                    OnFightForfeit(match.Team1, match.Team2);
                }

                ColUtility.Free(matches);
            }

            return true;
        }

        public void NewRound()
        {
            var ends = DateTime.Now + TimeSpan.FromDays(RoundDuration);

            RoundEnds = new DateTime(ends.Year, ends.Month, ends.Day, 23, 59, 59);

            if (!EndRound())
            {
                return;
            }

            Round++;

            if (EliminationPeriod)
            {
                var linup = GetEliminationRoundLineup();
                linup[Round] = new List<RoundMatch>();

                var list = EliminationTeams;

                switch (EliminationFormat)
                {
                    case EliminationFormat.Random:
                        if (Round == 1)
                        {
                            Shuffle(list);
                        }
                        break;
                    case EliminationFormat.StrongVsWeak:
                        var temp = new List<ArenaTeam>();
                        list = new List<ArenaTeam>(list.OrderBy(t => GetLeagueRecord().GetPlacement(t)));

                        for (int i = 0; i < list.Count / 2; i++)
                        {
                            temp.Add(list[i]);
                            temp.Add(list[(list.Count - 1) - i]);
                        }

                        list = temp;
                        break;
                    case EliminationFormat.StrongVsStrong:
                        list = new List<ArenaTeam>(list.OrderBy(t => GetLeagueRecord().GetPlacement(t)));
                        break;
                }

                EliminationTeams = list;

                for (int i = 0; i < list.Count; i += 2)
                {
                    linup[Round].Add(new RoundMatch(EliminationTeams[i], EliminationTeams[i + 1]));

                    var roundName = EliminationRoundToString(GetEliminationSize(), Round);

                    ArenaHelper.DoTeamPM(
                        EliminationTeams[i],
                        String.Format("{0} has begun!", roundName),
                        String.Format("You have until {0} to complete the current elimination round for the {1} League.", RoundEnds.ToShortDateString(), Name),
                        null);

                    ArenaHelper.DoTeamPM(
                        EliminationTeams[i + 1],
                        String.Format("{0} has begun!", roundName),
                        String.Format("You have until {0} to complete the current elimination round for the {1} League.", RoundEnds.ToShortDateString(), Name),
                        null);
                }
            }
            else if (Round > 1)
            {
                foreach (var team in Teams)
                {
                    ArenaHelper.DoTeamPM(
                        team,
                        String.Format("Round {0} has begun!", Round),
                        String.Format("You have until {0} to complete the current round match for the {1} League.", RoundEnds.ToShortDateString(), Name),
                        null);
                }
            }
        }

        public void EndLeague()
        {
            EndRound();

            var record = GetLeagueRecord();

            if (!EliminationPeriod && EliminationFormat != EliminationFormat.None)
            {
                int elimSize = GetEliminationSize();
                var list = new List<ArenaTeam>(record.Teams);

                if (list.Count > elimSize)
                {
                    list.RemoveRange(elimSize, list.Count - elimSize);
                }

                EliminationTeams = list;
                EliminationPeriod = true;

                Round = 0;
                NewRound();
            }
            else
            {
                RemoveLeague(this);

                var champs = record.Champion;
                var runnerup = record.RunnerUp;

                if (champs != null)
                {
                    ArenaHelper.DoTeamPM(
                        champs,
                        "League Champions!",
                        String.Format("Your team has emerged as the champions of the {0} League! A special title has been bestowed upon you!", Name),
                        null);

                    var pot = (int)(TotalPot * .75);

                    foreach (var pm in champs.Fighters.OfType<PlayerMobile>())
                    {
                        pm.AddRewardTitle(String.Format("{0} League Champion", Name));

                        Banker.Deposit(pm, pot / champs.Fighters.Count, false);
                        pm.SendMessage("{0} has been deposited into your bank account as a runner up!", EntryFee.ToString(CultureInfo.GetCultureInfo("en-US")));
                    }
                }

                if (runnerup != null)
                {
                    ArenaHelper.DoTeamPM(
                        runnerup,
                        "League Runner Up!",
                        String.Format("So Close! Your team are the runner ups of the {0} League!", Name),
                        null);

                    var pot = (int)(TotalPot * .25);

                    foreach (var pm in runnerup.Fighters.OfType<PlayerMobile>())
                    {
                        pm.AddRewardTitle(String.Format("{0} League Champion", Name));

                        Banker.Deposit(pm, pot / runnerup.Fighters.Count, false);
                        pm.SendMessage("{0} has been deposited into your bank account for your teams league victory!", EntryFee.ToString(CultureInfo.GetCultureInfo("en-US")));
                    }
                }

                foreach (var team in Teams.Where(t => t != champs && t != runnerup))
                {
                    var place = GetLeagueRecord().GetPlacement(team);

                    ArenaHelper.DoTeamPM(
                        team,
                        "League Champions!",
                        String.Format("Your team has placed {0}{2} in the {1} league! View the leagues gump for the complete standings!", place, Name, place == 2 ? "nd" : place == 3 ? "rd" : "th"),
                        null);
                }
            }
        }

        public bool PendingLeagueFight()
        {
            var lineup = GetCurrentLineup();

            if (lineup.ContainsKey(Round))
            {
                if (lineup[Round].Any(m => PendingFight(m)))
                {
                    return true;
                }
            }

            return false;
        }

        public bool PendingFight(RoundMatch m)
        {
            foreach (var arena in PVPTournamentSystem.SystemList)
            {
                // Already fighting
                if (arena.CurrentFight != null && arena.CurrentFight.League == this)
                {
                    var fight = arena.CurrentFight;

                    if ((fight.TeamA == m.Team1 && fight.TeamB == m.Team2) || (fight.TeamA == m.Team2 && fight.TeamB == m.Team1))
                    {
                        return true;
                    }
                }

                // in queue to fight
                foreach (var fight in arena.Queue.Where(f => f.League == this))
                {
                    if ((fight.TeamA == m.Team1 && fight.TeamB == m.Team2) || (fight.TeamA == m.Team2 && fight.TeamB == m.Team1))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int GetEliminationSize()
        {
            if (Teams.Count <= 8)
            {
                return 4;
            }

            if (Teams.Count <= 16)
            {
                return 8;
            }

            return 16;
        }

        private bool CheckEndElimination()
        {
            var lineup = GetEliminationRoundLineup();

            if (lineup[Round].Count == 1 && lineup[Round].All(m => !m.PendingFight) && !EliminationComplete)
            {
                RoundEnds = DateTime.Now + TimeSpan.FromSeconds(10);
                EliminationComplete = true;

                return true;
            }

            return false;
        }

        public void OnFightEnd(ArenaTeam winner, ArenaTeam loser, bool tie)
        {
            GetTeamRecord(winner).RecordFight(loser, true, tie, EliminationPeriod);
            GetTeamRecord(loser).RecordFight(winner, false, tie, EliminationPeriod);

            RoundMatch match = null;
            var lineup = GetCurrentLineup();

            if (lineup.ContainsKey(Round))
            {
                if (!EliminationPeriod)
                {
                    match = GetMatch(lineup[Round], winner, loser);

                    if (match != null)
                    {
                        if (tie)
                        {
                            if (AllowTies)
                            {
                                match.Tied = true;
                            }
                            else
                            {
                                DoTieBreaker(ref winner, ref loser);

                                match.Winner = winner;
                                match.Loser = loser;
                            }
                        }
                        else
                        {
                            match.Winner = winner;
                            match.Loser = loser;
                        }
                    }

                    GetLeagueRecord().RankTeams();
                }
                else
                {
                    match = GetMatch(lineup[Round], winner, loser);

                    if (match != null)
                    {
                        if (tie)
                        {
                            DoTieBreaker(ref winner, ref loser);
                        }

                        match.Winner = winner;
                        match.Loser = loser;
                    }

                    EliminationTeams.Remove(loser);
                    CheckEndElimination();
                }

                if (match != null && lineup[Round].Where(m => m.PendingFight).Count() > 1)
                {
                    for (int i = 0; i < lineup[Round].Count; i++)
                    {
                        var check = lineup[Round][i];

                        if (match != check && check.PendingFight && i < lineup[Round].Count - 1)
                        {
                            lineup[Round].Remove(match);
                            lineup[Round].Insert(i, match);
                            break;
                        }
                    }
                }
            }
        }

        public void OnFightForfeit(ArenaTeam one, ArenaTeam two)
        {
            OnFightEnd(one, two, true);
        }

        public RoundMatch GetMatch(ArenaTeam one, ArenaTeam two)
        {
            var lineup = GetCurrentLineup();

            if (lineup.ContainsKey(Round))
            {
                return GetMatch(lineup[Round], one, two);
            }

            return null;
        }   

        public RoundMatch GetMatch(List<RoundMatch> list, ArenaTeam one, ArenaTeam two)
        {
            return list.FirstOrDefault(m => m.PendingFight && ((m.Team1 == one && m.Team2 == two) || (m.Team1 == two && m.Team2 == one)));
        }

        public TeamRecord GetTeamRecord(ArenaTeam team)
        {
            var record = team.LeagueRecord;

            if (record == null)
            {
                record = new TeamRecord();
                team.LeagueRecord = record;
            }

            return record;
        }

        private bool ValidateLeague()
        {
            var count = Teams.Count;

            if (count < MinTeams)
            {
                CancelLeague(String.Format("Due to an insufficient amount of teams, the {0} League has been canceled.", Name));
                return false;
            }

            if (count % 2 != 0)
            {
                Teams.RemoveAt(Teams.Count - 1);
            }

            return true;
        }

        public void CancelLeague(string reason)
        {
            GetLeagueRecord().Cancelled = true;

            foreach (var team in Teams)
            {
                ArenaHelper.DoTeamPM(team, "League Cancellation", reason, null);

                if (EntryFee > 0)
                {
                    foreach (var pm in team.Fighters)
                    {
                        Banker.Deposit(pm, EntryFee, false);
                        pm.SendMessage("{0} gold has been refunded, and deposited into your bank account.", EntryFee.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                    }
                }

                if (Sponsors != null && Sponsors.Count > 0)
                {
                    foreach (var kvp in Sponsors)
                    {
                        Banker.Deposit(kvp.Key, kvp.Value, false);
                        kvp.Key.SendMessage("{0} sponsor has been refunded, and deposited into your bank account.", kvp.Value.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                    }
                }
            }

            RemoveLeague(this);
        }

        public static void Shuffle<T>(List<T> list)
        {
            if (list == null || list.Count < 2)
            {
                return;
            }

            for (int i = 0; i < list.Count * 2; i++)
            {
                var select = list[0];
                list.RemoveAt(0);

                list.Insert(Utility.RandomMinMax(0, list.Count - 1), select);
            }
        }

        public List<ArenaTeam> GetStandings()
        {
            return GetLeagueRecord().Teams;
        }

        public void DoTieBreaker(ref ArenaTeam winner, ref ArenaTeam loser)
        {
            var list = new List<ArenaTeam>() { winner, loser };
            RecordComparer.SortTeamList(list, GetLeagueRecord());

            winner = list[0];
            loser = list[1];
        }

        public bool IsParticipant(PlayerMobile pm)
        {
            var team = ArenaTeam.GetTeam(pm, TeamType);

            if (team == null)
            {
                return false;
            }

            if (!EliminationPeriod)
            {
                return Teams != null && Teams.Contains(team);
            }

            var lineup = GetEliminationRoundLineup();

            return lineup.ContainsKey(Round) && lineup[Round].Any(m => m.Team1 == team || m.Team2 == team);
        }

        public ArenaTeam GetCurrentOpponent(ArenaTeam team)
        {
            var lineup = GetCurrentLineup();

            if (lineup.ContainsKey(Round))
            {
                var match = lineup[Round].FirstOrDefault(m => m.Team1 == team || m.Team2 == team);

                if (match != null)
                {
                    return match.Team1 == team ? match.Team2 : match.Team1;
                }
            }

            return null;
        }

        public string StatusToString(PlayerMobile pm)
        {
            if (StartTime > DateTime.Now)
            {
                if (PreLeague && !HasBegun)
                {
                    return "Begins Soon";
                }

                if (ArenaTeam.HasTeam(pm, TeamType) && !IsParticipant(pm))
                {
                    return "Join Now!";
                }
                else
                {
                    return Teams == null || Teams.Count < MaxTeams ? "Waiting on Teams" : "League Full";
                }
            }
            else
            {
                if (EliminationPeriod)
                {
                    return String.Format("{0} ends {1}", EliminationRoundToString(GetEliminationSize(), Round), RoundEnds.ToShortDateString());
                }
                else
                {
                    return String.Format("Round {0} ends {1}", Round.ToString(), RoundEnds.ToShortDateString());
                }
            }
        }

        public static string EliminationRoundToString(int size, int round)
        {
            switch (size)
            {
                default:
                case 16:
                    switch (round)
                    {
                        default: return "Elimination";
                        case 1: return "Elim Round 1";
                        case 2: return "Quarterfinals";
                        case 3: return "Semifinals";
                        case 4: return "Finals";
                    }
                case 8:
                    switch (round)
                    {
                        default: return "Elimination";
                        case 1: return "Quarterfinals";
                        case 2: return "Semifinals";
                        case 3: return "Finals";
                    }
                case 4:
                    switch (round)
                    {
                        default: return "Elimination";
                        case 1: return "Semifinals";
                        case 2: return "Finals";
                    }
            }
        }

        public LeagueRecord GetLeagueRecord()
        {
            var record = LeagueRecords != null ? LeagueRecords.FirstOrDefault(r => r.LeagueName == Name) : null;

            if (record == null)
            {
                record = new LeagueRecord(this);
                AddLeagueRecord(record);
            }

            return record;
        }

        public Dictionary<int, List<RoundMatch>> GetRoundLineup()
        {
            var record = GetLeagueRecord();

            return record.RoundLineup;
        }

        public Dictionary<int, List<RoundMatch>> GetEliminationRoundLineup()
        {
            var record = GetLeagueRecord();

            return record.EliminationRoundLineup;
        }

        public Dictionary<int, List<RoundMatch>> GetCurrentLineup()
        {
            return EliminationPeriod ? GetEliminationRoundLineup() : GetRoundLineup();
        }

        public RoundMatch GetCurrentMatch(Mobile from)
        {
            var team = ArenaTeam.GetTeam(from, TeamType);
            var lineup = GetCurrentLineup();

            if (lineup.ContainsKey(Round))
            {
                return lineup[Round].FirstOrDefault(m => m.Team1 == team || m.Team2 == team);
            }

            return null;
        }

        public ArenaFight ConstructFight(RoundMatch match, PVPTournamentSystem system)
        {
            var a = match.Team1;
            var b = match.Team2;

            var fight = system.ConstructArenaFight(FightType);

            fight.FightType = TeamType;
            fight.Rules = Rules;
            fight.FightDuration = TimeSpan.FromMinutes(FightDuration);
            fight.UseOwnGear = UseOwnGear;

            if (!AllowTies || EliminationPeriod)
            {
                fight.Rules |= FightRules.NoTies;
            }

            if (Utility.RandomBool())
            {
                fight.TeamA = a;
                fight.TeamB = b;
            }
            else
            {
                fight.TeamA = b;
                fight.TeamB = a;
            }

            fight.League = this;
            return fight;
        }

        public League(GenericReader reader)
        {
            reader.ReadInt();

            FightType = (ArenaFightType)reader.ReadInt();
            TeamType = (ArenaTeamType)reader.ReadInt();
            Rules = (FightRules)reader.ReadInt();
            FightDuration = reader.ReadInt();
            AllowTies = reader.ReadBool();
            UseOwnGear = reader.ReadBool();

            Name = reader.ReadString();
            Rounds = reader.ReadInt();
            StartTime = reader.ReadDateTime();
            MaxTeams = reader.ReadInt();
            MinTeams = reader.ReadInt();
            EliminationFormat = (EliminationFormat)reader.ReadInt();
            EntryFee = reader.ReadInt();
            ForcedArena = reader.ReadItem<TournamentStone>();

            CanAcceptTeams = reader.ReadBool();
            HasBegun = reader.ReadBool();
            PreLeague = reader.ReadBool();
            Round = reader.ReadInt();
            RoundEnds = reader.ReadDateTime();
            EliminationPeriod = reader.ReadBool();
            EliminationComplete = reader.ReadBool();

            int count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                Teams.Add(ArenaTeam.GetTeam(reader.ReadString()));
            }

            count = reader.ReadInt();

            if (count > 0)
            {
                EliminationTeams = new List<ArenaTeam>();

                for (int i = 0; i < count; i++)
                {
                    EliminationTeams.Add(ArenaTeam.GetTeam(reader.ReadString()));
                }
            }

            AddLeague(this);
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write((int)FightType);
            writer.Write((int)TeamType);
            writer.Write((int)Rules);
            writer.Write(FightDuration);
            writer.Write(AllowTies);
            writer.Write(UseOwnGear);

            writer.Write(Name);
            writer.Write(Rounds);
            writer.Write(StartTime);
            writer.Write(MaxTeams);
            writer.Write(MinTeams);
            writer.Write((int)EliminationFormat);
            writer.Write(EntryFee);
            writer.WriteItem<TournamentStone>(ForcedArena);

            writer.Write(CanAcceptTeams);
            writer.Write(HasBegun);
            writer.Write(PreLeague);
            writer.Write(Round);
            writer.Write(RoundEnds);
            writer.Write(EliminationPeriod);
            writer.Write(EliminationComplete);

            writer.Write(Teams.Count);

            foreach (var team in Teams)
            {
                writer.Write(team.Name);
            }

            writer.Write(EliminationTeams == null ? 0 : EliminationTeams.Count);

            if (EliminationTeams != null)
            {
                foreach (var team in EliminationTeams)
                {
                    writer.Write(team.Name);
                }
            }
        }

        public void OnAfterLoad()
        {
            var list = new List<ArenaTeam>(Teams);

            for (int i = 0; i < list.Count; i++)
            {
                var team = list[i];

                if (team == null || !team.Active)
                {
                    Teams.Remove(team);
                }
            }

            if (PreLeague && (GetRoundLineup() == null || GetRoundLineup().Count == 0))
            {
                //BuildLineup();
            }

            ColUtility.Free(list);
        }

        private void RecordLineup()
        {
            var dir = Config.DataDirectory + "/linupes";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var lineup = GetRoundLineup();

            if (lineup == null || lineup.Count == 0)
            {
                return;
            }

            string filePath = Path.Combine(dir, String.Format("lineup_{0}.xml", Teams.Count));

            using (StreamWriter op = new StreamWriter(filePath))
            {
                XmlTextWriter xml = new XmlTextWriter(op);

                xml.Formatting = Formatting.Indented;
                xml.IndentChar = '\t';
                xml.Indentation = 1;

                xml.WriteStartDocument(true);

                xml.WriteStartElement("lineups");
                xml.WriteAttributeString("count", Teams.Count.ToString());

                foreach (var kvp in lineup)
                {
                    xml.WriteStartElement("Round");
                    xml.WriteAttributeString("round", kvp.Key.ToString());

                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        var match = kvp.Value[i];

                        xml.WriteStartElement("Match");
                        xml.WriteAttributeString("match", (i + 1).ToString());

                        xml.WriteStartElement("Team1");
                        xml.WriteString(Teams.IndexOf(match.Team1).ToString());
                        xml.WriteEndElement();

                        xml.WriteStartElement("Team2");
                        xml.WriteString(Teams.IndexOf(match.Team2).ToString());
                        xml.WriteEndElement();

                        xml.WriteEndElement();
                    }

                    xml.WriteEndElement();
                }

                xml.WriteEndElement();

                xml.Close();
            }
        }

        public static Dictionary<int, List<RoundMatch>> GetLineup(League league)
        {
            var dir = Config.DataDirectory + "/linupes";

            if (!Directory.Exists(dir))
            {
                return null;
            }

            List<string> files = null;

            try
            {
                files = new List<string>(Directory.GetFiles(dir, "*.xml"));
            }
            catch { }

            if (files != null && files.Count > 0)
            {
                foreach (string file in files)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);

                    XmlElement root = doc["lineups"];

                    int lineupCount = Utility.GetXMLInt32(Utility.GetAttribute(root, "count", "0"), 0);

                    if (lineupCount != league.Teams.Count)
                    {
                        continue;
                    }

                    int round = 1;
                    var lineup = new Dictionary<int, List<RoundMatch>>();

                    foreach (XmlElement r in root.GetElementsByTagName("Round"))
                    {
                        if (!lineup.ContainsKey(round))
                        {
                            lineup[round] = new List<RoundMatch>();
                        }

                        foreach (XmlElement m in r.GetElementsByTagName("Match"))
                        {
                            var team1Index = Utility.GetXMLInt32(Utility.GetText(m["Team1"], "0"), 0);
                            var team2Index = Utility.GetXMLInt32(Utility.GetText(m["Team2"], "0"), 0);

                            try
                            {
                                lineup[round].Add(new RoundMatch(league.Teams[team1Index], league.Teams[team2Index]));
                            }
                            catch
                            {
                                lineup.Clear();
                                return null;
                            }
                        }

                        round++;
                    }

                    return lineup;
                }
            }

            return null;
        }

        public static void Save(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(Leagues == null ? 0 : Leagues.Count);

            if (Leagues != null)
            {
                for (int i = 0; i < Leagues.Count; ++i)
                    Leagues[i].Serialize(writer);
            }

            writer.Write(LeagueRecords == null ? 0 : LeagueRecords.Count);

            if (LeagueRecords != null)
            {
                foreach (var record in LeagueRecords)
                {
                    record.Serialize(writer);
                }
            }
        }

        public static void Load(GenericReader reader)
        {
            reader.ReadInt();

            int count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                new League(reader);
            }

            count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                AddLeagueRecord(new LeagueRecord(reader));
            }
        }

        [Usage("NextFight")]
        [Description("Checks the immediate area for a League Fight [debug mode].")]
        public static void NextFight_OnCommand(CommandEventArgs e)
        {
            var m = e.Mobile;

            if (Leagues == null || Leagues.Count == 0)
            {
                m.SendMessage("There are no active leagues.");
            }

            TournamentStone stone = null;
            IPooledEnumerable eable = m.Map.GetItemsInRange(m.Location, 25);

            foreach (var item in eable)
            {
                if (item is TournamentStone)
                {
                    stone = (TournamentStone)item;
                    break;
                }
            }

            eable.Free();

            if (stone == null)
            {
                m.SendMessage("You must be within 25 tiles of a tournament stone.");
            }
            else if (stone.System == null)
            {
                m.SendMessage("There is an error with that stone. Use another one.");
            }
            else if (stone.System.InUse)
            {
                m.SendMessage("That arena is currently in use.");
            }
            else
            {
                foreach (var league in League.Leagues.Where(l => l.HasBegun))
                {
                    eable = m.Map.GetMobilesInRange(m.Location, 25);
                    var list = new List<PlayerMobile>();

                    foreach (var pm in eable.OfType<PlayerMobile>())
                    {
                        var match = CheckLeagueMatch(pm, stone);

                        if (match != null)
                        {
                            var fight = league.ConstructFight(match, stone.System);
                            fight.RegisterFight();
                            return;
                        }
                    }

                    eable.Free();
                }
            }
        }

        [Usage("SimulateRound")]
        [Description("Simulates the current Round.")]
        public static void SimulateRound_OnCommand(CommandEventArgs e)
        {
            foreach (var league in League.Leagues.Where(l => l.HasBegun))
            {
                Dictionary<int, List<RoundMatch>> lineup;

                if (league.EliminationPeriod)
                {
                    lineup = league.GetLeagueRecord().EliminationRoundLineup;
                }
                else
                {
                    lineup = league.GetLeagueRecord().RoundLineup;
                }

                if (lineup.ContainsKey(league.Round))
                {
                    var list = lineup[league.Round].Where(m => m.PendingFight).ToList();

                    foreach (var match in list)
                    {
                        if (Utility.RandomBool())
                        {
                            league.OnFightEnd(match.Team1, match.Team2, false);
                        }
                        else
                        {
                            league.OnFightEnd(match.Team2, match.Team1, false);
                        }
                    }

                    ColUtility.Free(list);
                    league.RoundEnds = DateTime.Now;
                }
                else
                {
                    e.Mobile.SendMessage("Error for round {0} [Elimination: {1}", league.Round, league.EliminationPeriod);
                }
            }
        }

        public static bool HasValidLeagues(PlayerMobile pm, TournamentStone stone)
        {
            return Leagues != null && Leagues.Any(l =>
                l.Teams.Any(t => t.GetLeadership() == pm) &&
                l.IsParticipant(pm) &&
                (l.ForcedArena == null || l.ForcedArena == stone));
        }

        public static List<League> GetValidLeagues(PlayerMobile pm, TournamentStone stone)
        {
            if (Leagues == null)
                return null;

            return Leagues.Where(l =>
                l.Teams.Any(t => t.GetLeadership() == pm) &&
                l.IsParticipant(pm) &&
                (l.ForcedArena == null || l.ForcedArena == stone)).ToList();
        }

        public static RoundMatch GetRoundMatch(Mobile from, Mobile target, TournamentStone stone, ref League league)
        {
            if (Leagues == null)
            {
                return null;
            }

            foreach (var l in Leagues.Where(l => l.HasBegun))
            {
                var team = ArenaTeam.GetTeam(from, l.TeamType);
                var opponent = ArenaTeam.GetTeam(target, l.TeamType);

                if (team != null && (l.ForcedArena == null || l.ForcedArena == stone))
                {
                    var tables = l.EliminationPeriod ? l.GetLeagueRecord().EliminationRoundLineup : l.GetLeagueRecord().RoundLineup;

                    if (tables.ContainsKey(l.Round))
                    {
                        var match = l.GetMatch(tables[l.Round], team, opponent);

                        if (match != null && match.PendingFight)
                        {
                            league = l;
                            return match;
                        }
                    }
                }
            }

            return null;
        }

        public static RoundMatch CheckLeagueMatch(Mobile from, TournamentStone stone)
        {
            if (Leagues == null)
            {
                return null;
            }

            foreach (var league in Leagues.Where(l => l.HasBegun))
            {
                var team = ArenaTeam.GetTeam(from, league.TeamType);

                if (team != null && (league.ForcedArena == null || league.ForcedArena == stone))
                {
                    var record = league.GetLeagueRecord();
                    var tables = league.EliminationPeriod ? record.EliminationRoundLineup : record.RoundLineup;

                    if (tables.ContainsKey(league.Round))
                    {
                        IPooledEnumerable eable = stone.Map.GetMobilesInRange(stone.Location, 20);

                        foreach (var pm in eable.OfType<PlayerMobile>())
                        {
                            var opponent = ArenaTeam.GetTeam(pm, league.TeamType);

                            if (opponent != null)
                            {
                                var match = league.GetMatch(tables[league.Round], team, opponent);

                                if (match != null && match.PendingFight)
                                {
                                    eable.Free();
                                    return match;
                                }
                            }
                        }

                        eable.Free();
                    }
                }
            }

            return null;
        }

        public static void Initialize()
        {
            if (Leagues != null)
            {
                Leagues.ForEach(l =>
                {
                    l.OnAfterLoad();
                });
            }
        }

        public static void OnTick()
        {
            if (Leagues == null)
            {
                return;
            }

            var leagues = new List<League>(Leagues);

            foreach (var league in leagues)
            {
                if (!league.HasBegun)
                {
                    if (league.StartTime < DateTime.Now)
                    {
                        league.BeginLeague();
                    }
                    else if (!league.PreLeague && league.StartTime - TimeSpan.FromHours(24) < DateTime.Now)
                    {
                        league.BeforeBeginLeague();
                    }
                }
                else
                {
                    league.ProcessTick();
                }
            }

            ColUtility.Free(leagues);
        }

        public static void AddLeagueRecord(LeagueRecord record)
        {
            if (LeagueRecords == null)
            {
                LeagueRecords = new List<LeagueRecord>();
            }

            LeagueRecords.Add(record);
        }

        public FightRules[] AvailableRules =
        {
            FightRules.NoPrecasting,
            FightRules.NoSummons,
            FightRules.NoPots,
            FightRules.NoSpecials,
            FightRules.PureMage,
            FightRules.PureDexxer,
            FightRules.AllowResurrections,
            FightRules.AllowMounts,
            FightRules.NoAreaSpells,
            FightRules.AllowPets,
        };
    }

    /// <summary>
    /// Keeps track of each teams record vs all other teams during the tournament
    /// </summary>
    [PropertyObject]
    public class TeamRecord : Dictionary<string, int[]>
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int LeagueMatches { get { return Wins + Losses + Ties; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Wins { get { return Values.Sum(v => v[0]); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Losses { get { return Values.Sum(v => v[1]); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Ties { get { return Values.Sum(v => v[2]); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EliminationWins { get { return Values.Sum(v => v[3]); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EliminationLosses { get { return Values.Sum(v => v[4]); } }

        public TeamRecord()
        {
        }

        public void RecordFight(ArenaTeam team, bool win, bool tie, bool elimination)
        {
            var opponent = team.Name;

            if (!ContainsKey(opponent))
            {
                this[opponent] = new int[5];
            }

            if (win)
            {
                this[opponent][0]++;

                if (elimination)
                {
                    this[opponent][3]++;
                }
            }
            else if (tie)
            {
                this[opponent][2]++;
            }
            else
            {
                this[opponent][1]++;

                if (elimination)
                {
                    this[opponent][4]++;
                }
            }
        }

        public TeamRecord(GenericReader reader)
        {
            reader.ReadInt();

            int count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                this[reader.ReadString()] = new int[] { reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt() };
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(Count);

            foreach (var kvp in this)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value[0]);
                writer.Write(kvp.Value[1]);
                writer.Write(kvp.Value[2]);
                writer.Write(kvp.Value[3]);
                writer.Write(kvp.Value[4]);
            }
        }
    }

    /// <summary>
    /// Keeps track of all round matches throught the league, ie league history
    /// </summary>
    [PropertyObject]
    public class LeagueRecord
    {
        public Dictionary<int, List<RoundMatch>> RoundLineup { get; set; }
        public Dictionary<int, List<RoundMatch>> EliminationRoundLineup { get; set; }

        public List<ArenaTeam> Teams { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string LeagueName { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Complete { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Cancelled { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public EliminationFormat ElimFormat { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ElimSize
        {
            get { return EliminationRoundLineup.ContainsKey(1) ? EliminationRoundLineup[1].Count * 2 : 0; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeam Champion
        {
            get
            {
                if (Complete && Teams != null && Teams.Count > 0)
                {
                    return Teams[0];
                }

                return null;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeam RunnerUp
        {
            get
            {
                if (Complete && Teams != null && Teams.Count > 1)
                {
                    return Teams[1];
                }

                return null;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public League League
        {
            get
            {
                if (League.Leagues == null)
                {
                    return null;
                }

                return League.Leagues.FirstOrDefault(l => l.Name == LeagueName);
            }
            set { }
        }

        public override string ToString()
        {
            return "...";
        }

        public void RankTeams()
        {
            RecordComparer.SortTeamList(Teams, this);
        }

        public bool IsInElimination(League league, ArenaTeam team)
        {
            if (!league.EliminationPeriod)
            {
                return false;
            }

            return league.EliminationTeams.Contains(team);
        }

        public void OnEliminationRoundEnd(League league)
        {
            var losers = new List<ArenaTeam>();

            foreach (var match in EliminationRoundLineup[league.Round])
            {
                losers.Add(match.Loser);
                Teams.Remove(match.Loser);
            }

            RecordComparer.SortTeamList(losers, this);

            for (int i = 0; i < Teams.Count; i++)
            {
                var team = Teams[i];

                if (!IsInElimination(league, team))
                {
                    for (int j = losers.Count - 1; j >= 0; j--)
                    {
                        Teams.Insert(i, losers[j]);
                    }

                    break;
                }
            }
        }

        public void CompleteLeague()
        {
            Complete = true;
        }

        public LeagueRecord(League league)
        {
            LeagueName = league.Name;
            ElimFormat = league.EliminationFormat;
            RoundLineup = new Dictionary<int, List<RoundMatch>>();
            EliminationRoundLineup = new Dictionary<int, List<RoundMatch>>();
        }

        public void AddTeam(ArenaTeam team)
        {
            if (Teams == null)
            {
                Teams = new List<ArenaTeam>();
            }

            if (!Teams.Contains(team))
            {
                Teams.Add(team);
            }
        }

        public int GetWins(ArenaTeam team)
        {
            var wins = 0;

            foreach (var roundMatches in RoundLineup.Values)
            {
                foreach (var match in roundMatches.Where(m => !m.PendingFight && m.Winner == team))
                {
                    wins++;
                }
            }

            foreach (var roundMatches in EliminationRoundLineup.Values)
            {
                foreach (var match in roundMatches.Where(m => !m.PendingFight && m.Winner == team))
                {
                    wins++;
                }
            }

            return wins;
        }

        public int GetLosses(ArenaTeam team)
        {
            var losses = 0;

            foreach (var roundMatches in RoundLineup.Values)
            {
                foreach (var match in roundMatches.Where(m => !m.PendingFight && m.Loser == team))
                {
                    losses++;
                }
            }

            foreach (var roundMatches in EliminationRoundLineup.Values)
            {
                foreach (var match in roundMatches.Where(m => !m.PendingFight && m.Loser == team))
                {
                    losses++;
                }
            }

            return losses;
        }

        public int GetTies(ArenaTeam team)
        {
            var ties = 0;

            foreach (var roundMatches in RoundLineup.Values)
            {
                foreach (var match in roundMatches.Where(m => !m.PendingFight && m.Tied))
                {
                    ties++;
                }
            }

            foreach (var roundMatches in EliminationRoundLineup.Values)
            {
                foreach (var match in roundMatches.Where(m => !m.PendingFight && m.Tied))
                {
                    ties++;
                }
            }

            return ties;
        }

        public int GetPlacement(ArenaTeam team)
        {
            if (Teams == null || !Teams.Contains(team))
                return 0;

            return Teams.IndexOf(team) + 1;
        }

        public int GetOpponentStrength(ArenaTeam team)
        {
            int str = 0;

            foreach (var rounds in RoundLineup.Values)
            {
                foreach (var lineup in rounds)
                {
                    if (lineup.Winner == team)
                    {
                        str += 2;
                    }
                    else if ((lineup.Team1 == team || lineup.Team2 == team) && lineup.Tied)
                    {
                        str++;
                    }
                }
            }

            return str;
        }

        public LeagueRecord(GenericReader reader)
        {
            var version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    Cancelled = reader.ReadBool();
                    goto case 0;
                case 0:
                    LeagueName = reader.ReadString();
                    ElimFormat = (EliminationFormat)reader.ReadInt();
                    Complete = reader.ReadBool();

                    int count = reader.ReadInt();

                    RoundLineup = new Dictionary<int, List<RoundMatch>>();

                    for (int i = 0; i < count; i++)
                    {
                        int round = reader.ReadInt();
                        int c = reader.ReadInt();

                        RoundLineup[round] = new List<RoundMatch>();

                        for (int j = 0; j < c; j++)
                        {
                            RoundLineup[round].Add(new RoundMatch(reader));
                        }
                    }

                    count = reader.ReadInt();

                    EliminationRoundLineup = new Dictionary<int, List<RoundMatch>>();

                    for (int i = 0; i < count; i++)
                    {
                        int round = reader.ReadInt();
                        int c = reader.ReadInt();

                        EliminationRoundLineup[round] = new List<RoundMatch>();

                        for (int j = 0; j < c; j++)
                        {
                            EliminationRoundLineup[round].Add(new RoundMatch(reader));
                        }
                    }

                    count = reader.ReadInt();

                    for (int i = 0; i < count; i++)
                    {
                        AddTeam(ArenaTeam.GetTeam(reader.ReadString()));
                    }

                    break;
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(1);

            writer.Write(Cancelled);

            writer.Write(LeagueName);
            writer.Write((int)ElimFormat);
            writer.Write(Complete);

            writer.Write(RoundLineup.Count);

            foreach (var kvp in RoundLineup)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value.Count);

                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    kvp.Value[i].Serialize(writer);
                }
            }

            writer.Write(EliminationRoundLineup.Count);

            foreach (var kvp in EliminationRoundLineup)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value.Count);

                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    kvp.Value[i].Serialize(writer);
                }
            }

            writer.Write(Teams == null ? 0 : Teams.Count);

            if (Teams != null)
            {
                for (int i = 0; i < Teams.Count; i++)
                {
                    writer.Write(Teams[i].Name);
                }
            }
        }
    }

    public class RecordComparer : IComparer<ArenaTeam>
    {
        private static Dictionary<LeagueRecord, RecordComparer> _Comparers;

        public static RecordComparer GetComparer(LeagueRecord record)
        {
            if (_Comparers == null)
            {
                _Comparers = new Dictionary<LeagueRecord, RecordComparer>();
            }

            if (!_Comparers.ContainsKey(record))
            {
                _Comparers[record] = new RecordComparer(record);
            }

            return _Comparers[record];
        }

        private LeagueRecord _LeagueRecord;

        public RecordComparer(LeagueRecord record)
        {
            _LeagueRecord = record;
        }

        public static void SortTeamList(List<ArenaTeam> list, LeagueRecord record)
        {
            list.Sort(GetComparer(record));
        }

        /// <summary>
        /// Rankings:
        /// 1. Most Wins
        /// 2. Ties
        /// 3. Strengh of wins
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int Compare(ArenaTeam a, ArenaTeam b)
        {
            if (_LeagueRecord.GetWins(a) == _LeagueRecord.GetWins(b))
            {
                if (_LeagueRecord.GetTies(b) == _LeagueRecord.GetTies(a))
                {
                    return _LeagueRecord.GetOpponentStrength(a).CompareTo(_LeagueRecord.GetOpponentStrength(b));
                }
                else
                {
                    return _LeagueRecord.GetTies(b).CompareTo(_LeagueRecord.GetTies(a));
                }
            }

            return _LeagueRecord.GetWins(b).CompareTo(_LeagueRecord.GetWins(a));
        }
    }

    public class RoundMatch
    {
        public ArenaTeam Team1 { get; set; }
        public ArenaTeam Team2 { get; set; }

        public ArenaTeam Winner { get; set; }
        public ArenaTeam Loser { get; set; }
        public bool Tied { get; set; }

        public bool PendingFight { get { return Winner == null && !Tied; } }

        public RoundMatch(ArenaTeam one, ArenaTeam two)
        {
            Team1 = one;
            Team2 = two;
        }

        public RoundMatch(GenericReader reader)
        {
            reader.ReadInt();

            Team1 = ArenaTeam.GetTeam(reader.ReadString());
            Team2 = ArenaTeam.GetTeam(reader.ReadString());
            Winner = ArenaTeam.GetTeam(reader.ReadString());
            Loser = ArenaTeam.GetTeam(reader.ReadString());
            Tied = reader.ReadBool();
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(Team1.Name);
            writer.Write(Team2.Name);
            writer.Write(Winner == null ? String.Empty : Winner.Name);
            writer.Write(Loser == null ? String.Empty : Loser.Name);
            writer.Write(Tied);
        }
    }
}
