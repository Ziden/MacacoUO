using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;

namespace Server.TournamentSystem
{
    public class PVPTournamentStats
    {
        private string m_TournamentName;
        private string m_Winner;
        private string m_RunnerUp;
        private string m_Arena;
        public TourneyType m_Type;
        private DateTime m_WinDate;
        private List<TournamentFightEntry> m_FightEntries = new List<TournamentFightEntry>();

        public string TournamentName { get { return m_TournamentName; } }
        public string Winner { get { return m_Winner; } }
        public string RunnerUp { get { return m_RunnerUp; } }
        public string Arena { get { return m_Arena; } }
        public TourneyType Type { get { return m_Type; } }
        public DateTime WinDate { get { return m_WinDate; } }
        public List<TournamentFightEntry> FightEntries { get { return m_FightEntries; } }

        private static List<PVPTournamentStats> m_TournamentStats = new List<PVPTournamentStats>();
        public static List<PVPTournamentStats> TournamentStats { get { return m_TournamentStats; } }

        public PVPTournamentStats(PVPTournamentSystem system, string name, DateTime winTime)
        {
            m_TournamentName = name;
            m_Arena = system.Name;
            m_WinDate = winTime;
            m_TournamentStats.Add(this);

            if (system != null)
            {
                m_Type = system.CurrentTournament.TourneyType;
            }
        }

        public void UpdateArenaName(PVPTournamentSystem system)
        {
            Utility.WriteConsoleColor(ConsoleColor.Green, "Updating Tournament stats: {0} to {1}...", m_Arena, system.Name);

            m_Arena = system.Name;
        }

        public void AddFightEntry(TournamentFightEntry entry)
        {
            if (!m_FightEntries.Contains(entry))
                m_FightEntries.Insert(0, entry);
        }

        public void WriteNewRound(Tournament tournament)
        {
            int fight = 0;
            ArenaTeam a = null;
            ArenaTeam b = null;
            string arena = null;

            for (int i = 0; i < tournament.RoundMatches.Count; ++i)
            {
                if (a == null)
                    a = tournament.RoundMatches[i];
                else if (a != null && b == null)
                {
                    arena = tournament.System.Name;
                    b = tournament.RoundMatches[i];

                    if (tournament.IsAlternateArena(a) && tournament.IsAlternateArena(b) && tournament.ArenaB != null)
                        arena = tournament.ArenaB.Name;

                    fight++;
                    AddFightEntry(new TournamentFightEntry(a, b, tournament.Round, fight, arena));

                    a = null;
                    b = null;
                }
            }

            arena = tournament.System.Name;

            if (tournament.IsAlternateArena(a) && tournament.IsAlternateArena(b) && tournament.ArenaB != null)
                arena = tournament.ArenaB.Name;

            if(a != null && b == null)  //Wildcard
                AddFightEntry(new TournamentFightEntry(a, null, tournament.Round, fight + 1, arena));
        }

        public void UpdateFight(ArenaTeam winner, ArenaTeam loser, int round, int fight, bool forfeit)
        {
            UpdateFight(winner, loser, round, fight, forfeit, false);
        }

        public void UpdateFight(ArenaTeam winner, ArenaTeam loser, int round, int fight, bool forfeit, bool nowinner)
        {
            string winnername = winner != null ? winner.Name : "null";
            string losername = loser != null ? loser.Name : "null";

            for (int i = 0; i < m_FightEntries.Count; ++i)
            {
                TournamentFightEntry entry = m_FightEntries[i];
                
                if (entry.Round == round && (winnername == entry.Fighter1 || losername == entry.Fighter1))
                {
                    if (nowinner)
                    {
                        entry.Winner = "Nobody";
                    }
                    else if (winner != null)
                    {
                        if (entry.Fighter1 == winner.Name || entry.Fighter2 == winner.Name)
                        {
                            entry.Winner = !forfeit ? winner.Name : "*" + winner.Name;
                        }

                        if (entry.Fighter1 == "Wildcard" && winner.Name != entry.Fighter2)
                        {
                            entry.Fighter1 = winner.Name;
                            entry.Winner = !forfeit ? winner.Name : "* " + winner.Name;
                        }

                        else if (entry.Fighter2 == "Wildcard" && winner.Name != entry.Fighter1)
                        {
                            entry.Fighter2 = winner.Name;
                            entry.Winner = !forfeit ? winner.Name : "* " + winner.Name;
                        }

                        if (loser != null)
                        {
                            if (entry.Fighter1 == "Wildcard" && loser.Name != entry.Fighter2)
                            {
                                entry.Fighter1 = loser.Name;
                            }
                            else if (entry.Fighter2 == "Wildcard" && loser.Name != entry.Fighter1)
                            {
                                entry.Fighter2 = loser.Name;
                            }
                        }
                    }
                    else if (winner == null && loser == null)
                    {
                        entry.Winner = "Nobody";
                    }

                    break;
                }
            }
        }

        public int GetLatestRound()
        {
            int round = 0;
            foreach (TournamentFightEntry entry in m_FightEntries)
            {
                if (entry.Round > round)
                    round = entry.Round;
            }
            return round;
        }

        public void OnTournamentEnd(ArenaTeam winner, ArenaTeam runnerup, bool forfeit)
        {
            m_Winner = "Nobody";
            m_RunnerUp = "Nobody";

            if (winner != null)
                m_Winner = !forfeit ? winner.Name : "* " + winner.Name;
            if (runnerup != null)
                m_RunnerUp = runnerup.Name;
        }

        public int RoundOneFights()
        {
            int fights = 0;
            foreach (TournamentFightEntry entry in m_FightEntries)
            {
                if (entry.Round == 1)
                    fights++;
            }
            return fights;
        }

        public static void ClearStats()
        {
            m_TournamentStats.Clear();
        }

        public static bool NameExists(PVPTournamentSystem sys, string name)
        {
            foreach (PVPTournamentStats stat in m_TournamentStats)
            {
                if (stat.Arena == sys.Name && stat.TournamentName == name)
                    return true;
            }
            return false;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((int)0);

            writer.Write(m_Arena);
            writer.Write(m_TournamentName);
            writer.Write(m_WinDate);
            writer.Write(m_Winner);
            writer.Write(m_RunnerUp);
            writer.Write((int)m_Type);

            writer.Write(m_FightEntries.Count);
            for (int i = 0; i < m_FightEntries.Count; ++i)
            {
                m_FightEntries[i].Serialize(writer);
            }
        }

        public PVPTournamentStats(GenericReader reader)
        {
            int version = reader.ReadInt();

            m_Arena = reader.ReadString();
            m_TournamentName = reader.ReadString();
            m_WinDate = reader.ReadDateTime();
            m_Winner = reader.ReadString();
            m_RunnerUp = reader.ReadString();
            m_Type = (TourneyType)reader.ReadInt();

            int count = reader.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                new TournamentFightEntry(reader, this);
            }

            m_TournamentStats.Add(this);
        }
    }

    public class TournamentFightEntry
    {
        private int m_Round;
        private int m_Fight;
        private string m_Fighter1;
        private string m_Fighter2;
        private string m_Winner;
        private string m_Arena;

        public int Round { get { return m_Round; } }
        public int Fight { get { return m_Fight; } }
        public string Fighter1 { get { return m_Fighter1; } set { m_Fighter1 = value; } }
        public string Fighter2 { get { return m_Fighter2; } set { m_Fighter2 = value; } }
        public string Winner { get { return m_Winner; } set { m_Winner = value; } }
        public string Arena { get { return m_Arena; } set { m_Arena = value; } }

        public TournamentFightEntry(ArenaTeam team1, ArenaTeam team2, int round, int fight, string arenaName)
        {
            m_Fighter1 = "Wildcard";
            m_Fighter2 = "Wildcard";

            if (team1 != null)
                m_Fighter1 = team1.Name;
            if (team2 != null)
                m_Fighter2 = team2.Name;

            m_Round = round;
            m_Fight = fight;
            m_Winner = null;
            m_Arena = arenaName;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((int)1);

            writer.Write(m_Arena);

            writer.Write(m_Round);
            writer.Write(m_Fight);
            writer.Write(m_Fighter1);
            writer.Write(m_Fighter2);
            writer.Write(m_Winner);
        }

        public TournamentFightEntry(GenericReader reader, PVPTournamentStats stats)
        {
            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    m_Arena = reader.ReadString();
                    goto case 0;
                case 0:
                    m_Round = reader.ReadInt();
                    m_Fight = reader.ReadInt();
                    m_Fighter1 = reader.ReadString();
                    m_Fighter2 = reader.ReadString();
                    m_Winner = reader.ReadString();
                    break;
            }
            stats.AddFightEntry(this);
        }
    }
}