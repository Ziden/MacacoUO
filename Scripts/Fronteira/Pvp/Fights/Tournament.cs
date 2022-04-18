using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Network;
using Server.Items;

namespace Server.TournamentSystem
{
    public enum TourneyType
    {
        SingleElim = 1,
        DoubleElimination,
        BestOf3,
    }

    public enum TourneyStyle
    {
        Standard, 
        MagesOnly,
        DexxersOnly
    }

    [PropertyObject]
    public class Tournament : IComparable, IDisposable
    {
        private PVPTournamentSystem m_System;
        public Mobile m_Creator;
        private string m_Name;
        private int m_EntryFee, m_GoldCollected, m_SponsorTotal;
        private int m_MaxParticipants, m_MinParticipants;
        private int m_Round, m_WarningMessage, m_Fight;
        private ArenaTeamType m_FightType;
        private TourneyStyle m_TourneyStyle;
        private TourneyType m_TourneyType;
        private PVPTournamentStats m_Stats;
        private TimeSpan m_TimePerFight;
        private DateTime m_StartTime;
        private FightRules m_Rules;
        private List<ArenaTeam> m_Participants = new List<ArenaTeam>();
        private List<ArenaTeam> m_OrigPartList = new List<ArenaTeam>();

        private List<ArenaTeam> m_OriginalArena = new List<ArenaTeam>();
        private List<ArenaTeam> m_AlternateArena = new List<ArenaTeam>();

        public List<ArenaTeam> OriginalArena { get { return m_OriginalArena; } }
        public List<ArenaTeam> AlternateArena { get { return m_AlternateArena; } }

        private bool m_OnGoing;
        private bool m_UseAlternateArena;

        private Item m_ChampPrize;
        private Item m_RunnerUpPrize;

        private PVPTournamentSystem m_ArenaA;
        private PVPTournamentSystem m_ArenaB;

        public PVPTournamentSystem System 
        { 
            get { return m_System; } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Creator
        {
            get { return m_Creator; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Name 
        { 
            get { return m_Name; } 
            set { m_Name = value; } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EntryFee
        {
            get { return m_EntryFee; }
            set { m_EntryFee = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GoldCollected
        {
            get { return m_GoldCollected; }
            set { m_GoldCollected = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SponsorTotal
        {
            get { return m_SponsorTotal; }
            set { m_SponsorTotal = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxParticipants 
        { 
            get { return m_MaxParticipants; } 
            set 
            { 
                m_MaxParticipants = value; 
                
                if (m_MaxParticipants > Config.MaxEntries) 
                    m_MaxParticipants = Config.MaxEntries; 
            } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinParticipants 
        { 
            get { return m_MinParticipants; } 
            set 
            { 
                m_MinParticipants = value;

                if (m_MinParticipants < Config.MinEntries)
                    m_MinParticipants = Config.MinEntries;
            } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Round 
        { 
            get { return m_Round; } 
            set { m_Round = value; } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int WarningMessage
        {
            get { return m_WarningMessage; }
            set { m_WarningMessage = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Fight
        {
            get { return m_Fight; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeamType FightType 
        { 
            get { return m_FightType; }
            set { m_FightType = value; } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TourneyStyle TourneyStyle
        {
            get { return m_TourneyStyle; }
            set 
            {
                var oldStyle = m_TourneyStyle;

                m_TourneyStyle = value;

                if (oldStyle != m_TourneyStyle)
                {
                    OnTourneyStyleChange();
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TourneyType TourneyType
        {
            get { return m_TourneyType; }
            set { m_TourneyType = value; }
        }

        public PVPTournamentStats Stats
        {
            get { return m_Stats; }
            set { m_Stats = value; }
        }

        public FightRules Rules
        {
            get { return m_Rules; }
            set { m_Rules = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan TimePerFight 
        { 
            get { return m_TimePerFight; } 
            set { m_TimePerFight = value; } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime StartTime 
        { 
            get { return m_StartTime; } 
            set { m_StartTime = value; } 
        }

        public List<ArenaTeam> Participants
        { 
            get { return m_Participants; } 
        }

        public bool OnGoing
        {
            get { return m_OnGoing; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UseAlternateArena
        {
            get { return m_UseAlternateArena; }
            set 
            {
                if (m_System == null || !m_System.CanUseAlternateArena || m_System.LinkedSystem == null)
                {
                    m_UseAlternateArena = false;
                }
                else
                {
                    m_UseAlternateArena = value;
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Item ChampPrize
        { 
            get { return m_ChampPrize; } 
            set { m_ChampPrize = value; } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Item RunnerUpPrize
        { 
            get { return m_RunnerUpPrize; } 
            set { m_RunnerUpPrize = value; } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PVPTournamentSystem ArenaA
        {
            get { return m_ArenaA; }
            set { m_ArenaA = value; } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PVPTournamentSystem ArenaB
        {
            get { return m_ArenaB; }
            set { m_ArenaB = value; }
        }

        private List<ArenaTeam> m_RoundMatches = new List<ArenaTeam>();
        public List<ArenaTeam> RoundMatches { get { return m_RoundMatches; } }

        private List<ArenaTeam> m_LosersBracket = new List<ArenaTeam>();

        private Dictionary<ArenaTeam, int> m_LossTable = new Dictionary<ArenaTeam, int>();
        private Dictionary<ArenaTeam, int> m_WildCards = new Dictionary<ArenaTeam, int>();

        private Dictionary<Mobile, int> m_Sponsors = new Dictionary<Mobile, int>();
        public Dictionary<Mobile, int> Sponsors { get { return m_Sponsors; } }

        public Tournament(PVPTournamentSystem sys)
        {
            m_System = sys;
            m_WarningMessage = 0;
            m_EntryFee = 0;
            m_MaxParticipants = Config.MaxEntries;
            m_MinParticipants = Config.MinEntries;
            m_FightType = ArenaTeamType.None;
            m_TimePerFight = sys.DefaultDuration;
            m_StartTime = new DateTime(DateTime.Now.Year, 1, 1, 12, 0, 0);
            m_Round = 0;
            m_OnGoing = false;
            m_TourneyType = TourneyType.SingleElim;

            UseAlternateArena = m_System.LinkedSystem != null && Config.UseAlternateArena;
        }

        ~Tournament()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            m_Sponsors.Clear();
            m_WildCards.Clear();
            m_LossTable.Clear();

            m_Stats = null;

            ColUtility.Free(m_LosersBracket);
            ColUtility.Free(m_RoundMatches);

            ColUtility.Free(m_OriginalArena);
            ColUtility.Free(m_AlternateArena);
            ColUtility.Free(m_Participants);
            ColUtility.Free(m_OrigPartList);
        }

        public override string ToString()
        {
            return "...";
        }

        public void BeginTournament()
        {
            if (!ValidateTournament())
                return;

            m_ArenaA = m_System;

            if (UseAlternateArena && m_System.LinkedSystem != null)
            {
                m_ArenaB = m_System.LinkedSystem;
                m_ArenaB.InUse = true;

                m_ArenaB.CurrentTournament = this;
            }

            string text = String.Format("The {0} Tournament will begin in {1} minutes! View the tournament board to see the matchups for round one!", m_Name, m_System.TournamentSetupTime.TotalMinutes.ToString());
            DoTournamentMessage(text, true);
            m_System.InUse = true;
            DisplayPrizes();
            m_OrigPartList = new List<ArenaTeam>(m_Participants);
            m_Stats = new PVPTournamentStats(m_System, m_Name, DateTime.Now);
            m_OnGoing = true;
            NewRound();
            Timer.DelayCall(TimeSpan.FromSeconds(10), new TimerCallback(AwardHighestSponsor));
        }

        public void OnTournamentEnd(ArenaTeam winner, ArenaTeam runnerUp)
        {
            OnTournamentEnd(winner, runnerUp, false);
        }

        public void OnTournamentEnd(ArenaTeam winner, ArenaTeam runnerUp, bool forfeitWin)
        {
            string text;
            if (winner != null && runnerUp != null)
                text = String.Format("{0} has emerged as the victors of the {1} Tournament with {2} as the runners up!", winner.Name, m_Name, runnerUp.Name);
            else if (winner != null && runnerUp == null)
                text = String.Format("{0} has emerged as the victors of the {1} Tournament by default.", winner.Name, m_Name);
            else
                text = string.Format("Nobody has won the {0} Tournament due to no-show participants in the championship fight.", m_Name);

            DoTournamentMessage(text, true);

            if(m_Stats != null)
                m_Stats.OnTournamentEnd(winner, runnerUp, forfeitWin);

            if (winner != null)
            {
                winner.TournamentChampionships++;
                winner.Points += 50;
            }

            if(runnerUp != null)
                runnerUp.Points += 25;

            if (m_System != null)
            {
                m_System.CurrentTournament = null;
                m_System.RemoveTournament(this);
                m_System.InUse = false;

                if (m_ArenaB != null)
                    m_ArenaB.InUse = false;
            }

            TourneyRewards.GenerateTrophies(winner, runnerUp, m_Name, m_OrigPartList);

            m_GoldCollected += m_SponsorTotal;

            int champAmount = m_GoldCollected / 2;
            int runupAmount = m_GoldCollected / 4;
            int creatAmount = m_GoldCollected / 4;

            if (m_Creator != null)
            {
                if (creatAmount > 0)
                {
                    Banker.Deposit(m_Creator, creatAmount, true);
                }
            }
            else if (creatAmount > 0)
            {
                champAmount = creatAmount / 2;
                runupAmount = creatAmount / 2;
            }

            if (winner != null)
            {
                if (champAmount > 0)
                {
                    foreach (var pm in winner.Fighters)
                    {
                        Banker.Deposit(pm, champAmount / winner.Fighters.Count, true);
                    }
                }
            }

            if (runnerUp != null)
            {
                if (runupAmount > 0)
                {
                    foreach (var pm in runnerUp.Fighters)
                    {
                        Banker.Deposit(pm, runupAmount / runnerUp.Fighters.Count, true);
                    }
                }
            }

            AwardPrize(winner, m_ChampPrize, m_System);
            AwardPrize(runnerUp, m_RunnerUpPrize, m_System);
        }

        private bool ValidateTournament()
        {
            DefragParticipantList();

            int cnt = m_Participants.Count;

            if (cnt < m_MinParticipants)
            {
                CancelTournament();
                return false;
            }
            return true;
        }

        public void CancelTournament()
        {
            m_OnGoing = false;

            if (!m_System.Tournaments.Contains(this))
                return;

            if (m_EntryFee > 0)
            {
                for (int i = 0; i < m_Participants.Count; ++i)
                {
                    foreach (var pm in m_Participants[i].Fighters)
                    {
                        Banker.Deposit(pm, m_EntryFee, true);
                    }
                }
            }

            if (m_Creator != null)
            {
                if (m_ChampPrize != null)
                {
                    m_Creator.BankBox.DropItem(m_ChampPrize);
                    m_ChampPrize.Movable = true;
                }
                if (m_RunnerUpPrize != null)
                {
                    m_Creator.BankBox.DropItem(m_RunnerUpPrize);
                    m_RunnerUpPrize.Movable = true;
                }
            }
            else
            {
                if (m_ChampPrize != null)
                    m_ChampPrize.Delete();
                
                if (m_RunnerUpPrize != null)
                    m_RunnerUpPrize.Delete();
            }

            Mobile sender = null;
            if (m_Creator != null)
                sender = m_Creator;
            else if (m_System.ArenaKeeper != null)
                sender = m_System.ArenaKeeper;

            string subject = "Tournament Canceled";
            string text = String.Format("The {0} Tournament has been canceled. Any entry fees have been deposited in your team leaders bank box.", m_Name);

            foreach (ArenaTeam team in m_Participants)
                ArenaHelper.DoTeamPM(team, subject, text, sender);

            if (m_Participants.Count > 0)
            {
                string msg = String.Format("The {0} Tournament has been canceled.", m_Name);
                DoTournamentMessage(msg, true);
            }

            if (m_System.CurrentTournament == this)
            {
                m_System.CurrentTournament = null;
                m_System.InUse = false;

                if (m_System.CurrentFight != null)
                    m_System.CurrentFight.CancelFight();

                if (m_ArenaB != null && m_ArenaB.CurrentFight != null)
                    m_ArenaB.CurrentFight.CancelFight();

                OnTournamentEnd(null, null, false);
            }

            RefundSponsors();
            m_System.RemoveTournament(this);

            if (m_Stats != null)
            {
                var stats = PVPTournamentStats.TournamentStats.FirstOrDefault(s =>
                    s.TournamentName == m_Stats.TournamentName &&
                    s.Arena == m_Stats.Arena);

                if (stats != null)
                {
                    PVPTournamentStats.TournamentStats.Remove(stats);
                }
            }
        }

        private void KickParticipant(ArenaTeam part)
        {
            if (m_Participants.Contains(part))
                m_Participants.Remove(part);

            if (m_EntryFee > 0)
            {
                foreach (var pm in part.Fighters)
                {
                    Banker.Deposit(pm, m_EntryFee, true);
                    m_GoldCollected -= m_EntryFee;
                }
            }

            string subject = "Tournament Cancelation";
            string text = "Your teams participation in the tournament has been canceled for being the odd team out.";
            ArenaHelper.DoTeamPM(part, subject, text, m_System.ArenaKeeper);
        }

        public void EndFight(ArenaTeam winners, ArenaTeam losers, bool forfeit, int damageDone)
        {
            if (m_LossTable.ContainsKey(losers)) //Adds to loss table
                m_LossTable[losers]++;
            else
                m_LossTable[losers] = 1;

            if (!m_WildCards.ContainsKey(losers))
                m_WildCards[losers] = damageDone;
            else
                m_WildCards[losers] += damageDone;

            if (losers != null) losers.TournamentLosses++;
            if (winners != null) winners.TournamentWins++;

            switch(m_TourneyType)
            {
                case TourneyType.SingleElim:
                case TourneyType.BestOf3:
                    {
                        EliminateTeam(losers, winners);
                        m_Stats.UpdateFight(winners, losers, m_Round, m_Fight, forfeit);
                        RemoveTeamsFromRound(winners, losers);
                        m_Fight++;
                        break;
                    }
                case TourneyType.DoubleElimination:
                    {
                        if (m_LossTable[losers] >= 2)
                        {
                            EliminateTeam(losers, winners);
                        }
                        else
                        {
                            DoTournamentMessage(String.Format("{0} has lost the arena fight. Next loss, and they are eliminated!", losers.Name), false);
                        }

                        m_Stats.UpdateFight(winners, losers, m_Round, m_Fight, forfeit);
                        RemoveTeamsFromRound(winners, losers);
                        m_Fight++;

                        break;
                    }
            }

            if (!m_OnGoing)
                return;

            if (m_RoundMatches.Count == 0 && m_Participants.Count == 1)       //Tournament is Over
            {
                OnTournamentEnd(winners, losers);
            }
            else if (m_RoundMatches.Count == 1)
            {
                ArenaTeam wildCard = ChooseWildCard();

                if (wildCard != null && !m_RoundMatches.Contains(wildCard))
                {
                    m_RoundMatches.Add(wildCard);

                    if (m_OriginalArena.Count == 1)
                        m_OriginalArena.Add(wildCard);
                    else if (m_AlternateArena.Count == 1)
                        m_OriginalArena.Add(wildCard);

                    Timer.DelayCall(Config.FightWait, new TimerCallback(NewFight));
                }
                else
                {
                    NewRound();
                }
            }
            else if (m_RoundMatches.Count == 0 && m_Participants.Count > 1)    //Round is Over
            {
                NewRound();
            }
            else
            {
                Timer.DelayCall(Config.FightWait, new TimerCallback(NewFight)); //New fight - same round
            }

            if (forfeit)
            {
                string text = String.Format("{0} has beat {1} due to forfeit!", winners.Name, losers.Name);
                DoTournamentMessage(text, false);
            }
        }

        private void RemoveTeamsFromRound(ArenaTeam winners, ArenaTeam losers)
        {
            if (m_RoundMatches.Contains(winners)) m_RoundMatches.Remove(winners);
            if (m_RoundMatches.Contains(losers)) m_RoundMatches.Remove(losers);
            if (m_OriginalArena.Contains(winners)) m_OriginalArena.Remove(winners);
            if (m_OriginalArena.Contains(losers)) m_OriginalArena.Remove(losers);
            if (m_AlternateArena.Contains(winners)) m_AlternateArena.Remove(winners);
            if (m_AlternateArena.Contains(losers)) m_AlternateArena.Remove(losers);
        }

        private void DoEndOfFightMessage(ArenaTeam a, ArenaTeam b)
        {
            if (a == null || b == null)
                return;

            string msg = null;
            /*switch (m_TourneyType)
            {
                default:
                case TourneyType.SingleElim: break;
                case TourneyType.BestOf3: break;
                case TourneyType.Bracketed:
                    if (m_Round == 1)
                        msg = String.Format("{0} has been moved to the losers bracket.", b.Name);
                    break;
            }*/

            DoTournamentMessage(String.Format("{0} has defeated {1} at the {2} Tournament!", a.Name, b.Name, m_Name), false);

            if (msg != null)
                DoTournamentMessage(msg, false);
        }

        private void EliminateTeam(ArenaTeam losers)
        {
            EliminateTeam(losers, null);
        }

        private void EliminateTeam(ArenaTeam losers, ArenaTeam winners)
        {
            if (m_Participants.Contains(losers))
                m_Participants.Remove(losers);

            if (winners != null)
                DoTournamentMessage(String.Format("{0} has eliminated {1} from the {2} Tournament!", winners.Name, losers.Name, m_Name), false);
            else
                DoTournamentMessage(String.Format("{0} has been eliminated from the {1} Tournament!", losers.Name, m_Name), false);
        }

        public void NewFight()
        {
            if (!m_OnGoing)
                return;

            if (m_RoundMatches.Count == 0)
            {
                NewRound();
                return;
            }

            ArenaTeam a = null;
            ArenaTeam b = null;
            PVPTournamentSystem system = m_System;

            if (m_ArenaB != null)
            {
                bool arenaAAvailable = m_ArenaA != null && m_ArenaA.CurrentFight == null;
                bool arenaBAvailable = m_ArenaB != null && m_ArenaB.CurrentFight == null;

                if (arenaAAvailable && m_OriginalArena.Count > 0)
                {
                    a = m_OriginalArena[0];

                    if (m_OriginalArena.Count == 1)
                        b = ChooseWildCard();
                    else
                        b = m_OriginalArena[1];

                    if(b != null)
                        BeginFight(a, b, m_ArenaA);
                }

                if (arenaBAvailable && m_AlternateArena.Count > 0)
                {
                    a = m_AlternateArena[0];

                    if (m_AlternateArena.Count == 1)
                        b = ChooseWildCard();
                    else
                        b = m_AlternateArena[1];

                    if(b != null)
                        BeginFight(a, b, m_ArenaB);
                }
            }
            else
            {
                a = m_RoundMatches[0];

                if (m_RoundMatches.Count == 1)
                    b = ChooseWildCard();
                else
                    b = m_RoundMatches[1];

                if(b != null)
                    BeginFight(a, b, m_ArenaA);
            }
        }

        private void BeginFight(ArenaTeam a, ArenaTeam b, PVPTournamentSystem system)
        {
            if (IsFighting(a) || IsFighting(b))
                return;

            ArenaFight fight = system.ConstructArenaFight(
                m_TourneyType == TourneyType.BestOf3 ? ArenaFightType.BestOf3 : ArenaFightType.SingleElimination, this);

            fight.TeamA = a;
            fight.TeamB = b;
            fight.FightType = m_FightType;
            fight.Rules = m_Rules;
            fight.FightDuration = m_TimePerFight;

            fight.RegisterFight();
        }

        public bool IsFighting(ArenaTeam team)
        {
            if (m_ArenaA != null && m_ArenaA.CurrentFight != null && m_ArenaA.CurrentFight.Teams.Contains(m_ArenaA.CurrentFight.GetTeamInfo(team)))
                return true;

            if (m_ArenaB != null && m_ArenaB.CurrentFight != null && m_ArenaB.CurrentFight.Teams.Contains(m_ArenaB.CurrentFight.GetTeamInfo(team)))
                return true;

            return false;
        }

        public void NewRound()
        {
            //m_LossTable.Clear();

            if (m_Participants.Count <= 1)
            {
                if (m_Participants.Count == 1)
                {
                    ArenaTeam b = ChooseWildCard();

                    if (b == null)
                    {
                        OnTournamentEnd(m_Participants[0], null, true);
                        return;
                    }
                }
                else if (m_WildCards.Count == 1)
                {
                    OnTournamentEnd(ChooseWildCard(), null, true);
                    return;
                }
                else if (m_WildCards.Count > 1)
                {
                    ChooseWildCard();
                    ChooseWildCard();
                }
                else
                {
                    OnTournamentEnd(null, null, true);
                    return;
                }
            }

            m_Round++;
            m_Fight = 1;

            if (OddParticipants())
            {
                if (/*(m_Round > 2 && m_TourneyType == TourneyType.Bracketed) ||*/ m_Round != 1)
                    ChooseWildCard();
            }

            m_RoundMatches = null;

            /*if (m_Round == 2 && m_TourneyType == TourneyType.Bracketed)
                m_RoundMatches = new List<ArenaTeam>(m_LosersBracket);
            else*/
            m_RoundMatches = new List<ArenaTeam>(m_Participants);

            if (m_RoundMatches.Count > 2)
            {
                int shuffles = m_RoundMatches.Count * 10;
                int ran = 0;

                for (int i = 0; i < shuffles; ++i)
                {
                    ran = Utility.Random(m_RoundMatches.Count);
                    ArenaTeam team = m_RoundMatches[ran];

                    if (team == null)
                        continue;

                    m_RoundMatches.Remove(team);
                    m_RoundMatches.Insert(0, team);
                }
            }

            if (m_ArenaB != null)
            {
                bool alternate = m_Participants.Count > 4;

                for (int i = 0; i < m_RoundMatches.Count; i++)
                {
                    if (!alternate || m_OriginalArenaIndexes.Any(x => x == i))
                        m_OriginalArena.Add(m_RoundMatches[i]);
                    else
                        m_AlternateArena.Add(m_RoundMatches[i]);
                }
            }

            if (m_Stats == null)
                m_Stats = new PVPTournamentStats(m_System, m_Name, DateTime.Now);

            m_Stats.WriteNewRound(this);

            TimeSpan wait;
            string text = null;

            if (m_Round == 1)
                wait = m_System.TournamentSetupTime;
            else
                wait = Config.RoundWait;

            Timer.DelayCall(wait, new TimerCallback(NewFight));

            if (m_Round == 1)
            {
                //text = String.Format("Get ready for round {0} of the {1} Tournament to begin in {2} Minutes!", m_Round, m_Name, m_System.TournamentSetupTime.TotalMinutes.ToString());
            }
            else if (m_Participants.Count > 2)
                text = String.Format("Round {0} has ended, get ready for round {1} to begin in {2} Minutes!", m_Round - 1, m_Round, Config.RoundWait.TotalMinutes.ToString());
            else
                text = String.Format("The Championship round between {0} and {1} will begin in {2} minutes!", m_Participants[0].Name, m_Participants[1].Name, Config.RoundWait.TotalMinutes.ToString());
            
            if (m_Participants.Count > 4) //semi finals round keep losers in wildcard race
                m_WildCards.Clear();

            DoTournamentMessage(text, m_Round == 0 || m_Participants.Count == 2);
        }

        public bool IsAlternateArena(ArenaTeam team)
        {
            return m_AlternateArena.Contains(team);
        }

        public void CancelTournamentFight(ArenaTeam a, ArenaTeam b)
        {
            if (m_LossTable.ContainsKey(a))
                m_LossTable[a]++;
            else
                m_LossTable[a] = 1;

            if (m_LossTable.ContainsKey(b))
                m_LossTable[b]++;
            else
                m_LossTable[b] = 1;

            switch (m_TourneyType)
            {
                case TourneyType.SingleElim:
                case TourneyType.BestOf3:
                case TourneyType.DoubleElimination:
                    {
                        int canLose = m_TourneyType == TourneyType.DoubleElimination ? 2 : 1;

                        if (m_LossTable[a] >= canLose)
                        {
                            EliminateTeam(a);

                            if (m_Round == 1 && m_OrigPartList.Contains(a))
                                m_OrigPartList.Remove(a);
                        }

                        if (m_LossTable[b] >= canLose)
                        {
                            EliminateTeam(b);

                            if (m_Round == 1 && m_OrigPartList.Contains(b))
                                m_OrigPartList.Remove(b);
                        }

                        RemoveTeamsFromRound(a, b);
                        m_Stats.UpdateFight(a, b, m_Round, m_Fight, false, true);
                        m_Fight++;
                        break;
                    }
            }

            if(a != null) a.TournamentLosses++;
            if(b != null) b.TournamentLosses++;

            if (!m_OnGoing)
                return;

            if (m_RoundMatches.Count == 0 && m_Participants.Count == 1)                  //Tournament is Over
            {
                OnTournamentEnd(m_Participants[0], ChooseWildCard(), true);
            }
            else if (m_RoundMatches.Count == 0 && m_Participants.Count == 0)             //Really? The champion fight and they dont show up?
            {
                if (m_WildCards.Count < 1)
                    OnTournamentEnd(null, null, true);
                else if (m_WildCards.Count == 1)
                {
                    foreach (KeyValuePair<ArenaTeam, int> kvp in m_WildCards)
                    {
                        OnTournamentEnd(kvp.Key, null, true);
                        break;
                    }
                }
                else
                {
                    ArenaTeam teama = ChooseWildCard();
                    ArenaTeam teamb = ChooseWildCard();

                    if (teama != null && teamb != null)
                    {
                        m_RoundMatches.Add(teama);
                        m_RoundMatches.Add(teamb);
                        m_OriginalArena.Add(teama);
                        m_OriginalArena.Add(teamb);
                        NewRound();                                                     //New round...Let's try this again!
                    }
                }
            }
            else if (m_RoundMatches.Count < 2 && m_Participants.Count > 0)              //Round is Over
            {
                ArenaTeam wildCard = ChooseWildCard();
                if (wildCard != null && !m_RoundMatches.Contains(wildCard))
                {
                    m_RoundMatches.Add(wildCard);

                    if (m_OriginalArena.Count == 1)
                        m_OriginalArena.Add(wildCard);
                    else if (m_AlternateArena.Count == 1)
                        m_AlternateArena.Add(wildCard);

                    Timer.DelayCall(Config.FightWait, new TimerCallback(NewFight));
                }
                else
                    NewRound();
            }
            else
                Timer.DelayCall(Config.FightWait, new TimerCallback(NewFight));                //New fight - same round
        }

        private ArenaTeam ChooseWildCard()
        {
            ArenaTeam wildCard = null;
            int highest = 0;

            foreach (KeyValuePair<ArenaTeam, int> kvp in m_WildCards)
            {
                if (kvp.Value > highest || wildCard == null)
                {
                    wildCard = kvp.Key;
                    highest = kvp.Value;
                }
            }

            if (wildCard != null && !m_Participants.Contains(wildCard))
            {
                string text = String.Format("{0} has won a wildcard and will continue on to the next round!", wildCard.Name);
                DoTournamentMessage(text, false);

                string subject = "Wildcard";
                string text2 = "Your team has won a wildcard and will continue to the next round!";
                ArenaHelper.DoTeamPM(wildCard, subject, text2, m_System.ArenaKeeper, TimeSpan.FromMinutes(30));

                m_Participants.Add(wildCard);
                m_WildCards.Remove(wildCard);
            }
            return wildCard;
        }

        private bool OddParticipants()
        {
            return m_Participants.Count % 2 != 0;
        }

        public void DoJoinTeam(ArenaTeam team)
        {
            if (!m_Participants.Contains(team))
            {
                m_Participants.Add(team);

                if (m_EntryFee > 0)
                {
                    foreach (var pm in team.Fighters)
                    {
                        Banker.Withdraw(pm, m_EntryFee);
                        m_GoldCollected += m_EntryFee;

                        pm.SendMessage("The entry fee of {0} has been deducted from your bank account.", m_EntryFee);
                    }
                }
            }

            string subject = "New Tournament";
            string text = String.Format("Your team has joined the {0} tournmant being held on {1}.", m_Name, m_StartTime.ToString());
            ArenaHelper.DoTeamPM(team, subject, text, m_System.ArenaKeeper);
        }

        public void DoWithdrawTeam(ArenaTeam team)
        {
            if (m_Participants.Contains(team))
                m_Participants.Remove(team);

            if (m_EntryFee > 0)
            {
                foreach (var pm in team.Fighters)
                {
                    Banker.Deposit(pm, m_EntryFee, true);  
                    m_GoldCollected -= m_EntryFee;
                }
            }

            string subject = "Tournament Withdrawal";
            string text = String.Format("Your team withdrawn from the {0} tournament being held on {1}.", m_Name, m_StartTime.ToString());
            ArenaHelper.DoTeamPM(team, subject, text, m_System.ArenaKeeper);
        }

        public void Register(Mobile creator)
        {
            if (m_System == null)
                return;

            if (m_System.Chest == null)
            {
                m_System.Chest = new PrizeChest(m_System);
                m_System.Chest.MoveToWorld(m_System.ChestLocation, m_System.ArenaMap);
                m_System.Chest.Visible = false;
            }

            m_System.AddTournament(this);

            if (creator != null && creator.AccessLevel <= AccessLevel.VIP)
                m_Creator = creator;

            if (m_ChampPrize != null)
            {
                m_ChampPrize.Movable = false;
                m_System.Chest.DropItem(m_ChampPrize);
            }
            if (m_RunnerUpPrize != null)
            {
                m_RunnerUpPrize.Movable = false;
                m_System.Chest.DropItem(m_RunnerUpPrize);
            }

            m_WarningMessage++;
        }

        private int[] m_OriginalArenaIndexes = new int[]
        {
            0, 1, 
            4, 5, 
            8, 9, 
            12, 13, 
            16, 17, 
            20, 21, 
            24, 25, 
            28, 29, 
            32, 33, 
            36, 37,
            40, 41, 
            44, 45, 
            48, 49, 
            52, 53,
            54, 55, 
            58, 59,
            62, 63,
            66, 67,
            70, 71
        };

        public int CompareTo(object obj)
        {
            TimeSpan ts = ((Tournament)obj).StartTime - m_StartTime;
            return (int)ts.TotalMinutes;
        }

        public void TryJoinTournament(Mobile from)
        {
            if (!m_System.Tournaments.Contains(this))
                from.SendMessage("That tournament is not longer registered!");
            else if (m_System.CurrentTournament == this)
                from.SendMessage("You cannot joint that tournament as it has already begun.");
            else
            {
                ArenaTeam team = ArenaTeam.GetTeam(from, m_FightType, m_FightType == ArenaTeamType.Single);

                if (team != null)
                {
                    foreach (ArenaTeam participant in m_Participants)
                    {
                        if (!ArenaHelper.CanRecievePoints(participant, team) && Config.RestrictSameIP && from.AccessLevel <= AccessLevel.VIP)
                        {
                            from.SendMessage("A memeber of your team is already a participant of another team registered in this tournament.");
                            return;
                        }
                    }

                    if (m_Participants.Contains(team))
                        from.SendMessage("Your team has already joined this tournament!");
                    else if (!ArenaTeam.IsTeamLeader(from, team))
                        from.SendMessage("Only your Arena Team Leader can sing a team up for a tournament.");
                    else if (!ValidateTourneyStyle(team))
                        from.SendMessage("This tournament is for {0}.  Your team does not match this criteria.", ArenaHelper.GetTourneyStyle(this));
                    else if (m_Participants.Count >= m_MaxParticipants)
                        from.SendMessage("This tournament is already at its maximum capacity. Check back later in case someone drops out.");
                    else if (!team.Active)
                        from.SendMessage("Your team must be active to register as a participant for this tournament.");
                    else if (!ArenaHelper.CheckWager(team, m_EntryFee))
                        from.SendMessage("The entry fee of {0} must be available in your bank box or vault.", m_EntryFee);
                    else
                    {
                        DoJoinTeam(team);
                        from.SendMessage("You have joined your {0} team in the tournament.", m_FightType);
                    }
                }
                else
                    from.SendMessage("You must be a memeber of {0} arena team to enter this tournament.", m_FightType);
            }
        }

        public void TryWithdrawTeam(Mobile from)
        {
            if (m_System.CurrentTournament == this || !m_System.Tournaments.Contains(this))
            {
                from.SendMessage("It is too late to withdraw from this tournament.");
                return;
            }

            ArenaTeam team = ArenaTeam.GetTeam(from, m_FightType);

            if (team != null)
            {
                if (m_Participants.Contains(team))
                    DoWithdrawTeam(team);
                else
                    from.SendMessage("You aren't a participant in this tournament!");
            }
            else
                from.SendMessage("You don't even belong to a {0} team!", m_FightType);
        }

        public bool ValidateTourneyStyle(ArenaTeam team)
        {
            switch (m_TourneyStyle)
            {
                default:
                case TourneyStyle.Standard: 
                    return true;
                case TourneyStyle.MagesOnly: 
                    return team.Fighters.All(f => f.Skills[SkillName.Magery].Base + f.Skills[SkillName.EvalInt].Base >= 150);
                case TourneyStyle.DexxersOnly:  
                    return team.Fighters.All(f => f.Skills[SkillName.Magery].Base < 50);
            }
        }

        public void OnTourneyStyleChange()
        {
            switch (m_TourneyStyle)
            {
                case TourneyStyle.Standard: return;
                case TourneyStyle.MagesOnly:
                    AddRules(FightRules.PureMage);
                    RemoveRules(FightRules.PureDexxer);
                    RemoveRules(FightRules.NoPrecasting);
                    break;
                case TourneyStyle.DexxersOnly:
                    AddRules(FightRules.PureDexxer);
                    AddRules(FightRules.NoPrecasting);
                    RemoveRules(FightRules.PureMage);
                    break;
            }
        }

        public void DoTournamentMessage(string text, bool worldBroadcast)
        {
            if (m_System == null || m_System.AudienceRegion == null || m_System.FightRegion == null || text == null || text == String.Empty)
                return;

            if (worldBroadcast)
            {
                World.Broadcast(1153, true, text);
                return;
            }

            ArenaHelper.DoRegionMessage(text, 0, m_System);

            if (m_System.ArenaKeeper != null)
                m_System.ArenaKeeper.Say(text);
        }

        private void DisplayPrizes()
        {
            if (m_ChampPrize != null)
                m_ChampPrize.MoveToWorld(m_System.TeamAWageDisplay, m_System.ArenaMap);
            if (m_RunnerUpPrize != null)
                m_RunnerUpPrize.MoveToWorld(m_System.TeamBWageDisplay, m_System.ArenaMap);
        }

        public bool HasRule(FightRules rule)
        {
            return (m_Rules & rule) != 0;
        }

        public void AddRules(FightRules rule)
        {
            if (!HasRule(rule))
            {
                m_Rules |= rule;
            }
        }

        public void RemoveRules(FightRules rule)
        {
            if (HasRule(rule))
            {
                m_Rules ^= rule;
            }
        }

        private void DefragParticipantList()
        {
            var list = new List<ArenaTeam>(m_Participants);

            foreach (var team in list.Where(team =>
                                                     team == null || 
                                                     !team.Active || 
                                                     team.Fighters.Count < 1 || 
                                                     !ArenaTeam.Teams.Contains(team) || 
                                                     !ValidateTourneyStyle(team)))
            {
                m_Participants.Remove(team);
            }

            ColUtility.Free(list);
        }

        public void AddSponsor(Mobile from, int amount)
        {
            if (m_Sponsors.ContainsKey(from))
                m_Sponsors[from] += amount;
            else
                m_Sponsors[from] = amount;

            m_SponsorTotal += amount;
        }

        public void RefundSponsors()
        {
            foreach (KeyValuePair<Mobile, int> kvp in m_Sponsors)
            {
                if (kvp.Key != null && !kvp.Key.Deleted)
                {
                    if (Banker.Deposit(kvp.Key, kvp.Value, true))
                    {
                        m_SponsorTotal -= kvp.Value;
                    }
                }
            }
        }

        public void AwardPrize(ArenaTeam team, Item prize, PVPTournamentSystem system)
        {
            if (prize != null)
            {
                if (team != null)
                {
                    Mobile leader = team.TeamLeader;

                    if (leader == null)
                    {
                        foreach (Mobile mob in team.Fighters) // get first available
                        {
                            leader = mob;
                            break;
                        }
                    }

                    if (leader != null) // lets give it to the leader
                    {
                        leader.BankBox.DropItem(prize);
                        prize.Movable = true;
                    }
                    else if (system != null && system.Chest != null) // next, put it in the box
                    {
                        system.Chest.DropItem(prize);
                        prize.Visible = true;
                    }
                    else if (m_Creator != null) // next, goes to the creator
                    {
                        m_Creator.BankBox.DropItem(prize);
                        prize.Movable = true;
                    }
                    else // lastly, make it not visible
                    {
                        prize.Visible = false;
                    }
                }
                else if (m_Creator != null) // next, goes to the creator
                {
                    m_Creator.BankBox.DropItem(prize);
                    prize.Movable = true;
                }
                else // lastly, make it not visible
                {
                    prize.Visible = false;
                }
            }
        }

        public void AwardHighestSponsor()
        {
            if (m_Sponsors.Count == 0 || m_System == null || m_System.ArenaKeeper == null)
                return;

            int most = 0;
            Mobile highest = null;
            foreach (KeyValuePair<Mobile, int> sponsors in m_Sponsors)
            {
                if (highest == null || sponsors.Value >= most)
                {
                    highest = sponsors.Key;
                    most = sponsors.Value;
                }
            }

            if(highest != null)
                TourneyRewards.GenerateSponsorReward(highest, this);
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((int)0); //Version!!!

            writer.Write(UseAlternateArena);

            writer.Write(m_RoundMatches.Count);
            foreach (ArenaTeam team in m_RoundMatches)
                writer.Write(team.Name);

            writer.Write(m_OrigPartList.Count);
            foreach (ArenaTeam team in m_OrigPartList)
                writer.Write(team.Name);

            writer.Write(UseAlternateArena);

            writer.Write(m_Sponsors.Count);
            foreach (KeyValuePair<Mobile, int> sponsors in m_Sponsors)
            {
                writer.Write(sponsors.Key);
                writer.Write(sponsors.Value);
            }

            writer.Write((int)m_TourneyType);
            writer.Write(m_Name);
            writer.Write(m_Creator);
            writer.Write(m_MaxParticipants);
            writer.Write(m_MinParticipants);
            writer.Write(m_EntryFee);
            writer.Write(m_GoldCollected);
            writer.Write(m_Round);
            writer.Write(m_WarningMessage);
            writer.Write(m_Fight);
            writer.Write((int)m_FightType);

            writer.Write(m_TimePerFight);
            writer.Write(m_StartTime);

            writer.Write(m_OnGoing);

            writer.Write(m_ChampPrize);
            writer.Write(m_RunnerUpPrize);
            writer.Write((int)m_Rules);

            writer.Write(m_Participants.Count);
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                writer.Write(m_Participants[i].Name);
            }
        }

        public Tournament(GenericReader reader, PVPTournamentSystem sys)
        {
            int version = reader.ReadInt();

            UseAlternateArena = reader.ReadBool();

            int c = reader.ReadInt();

            for (int i = 0; i < c; i++)
                m_RoundMatches.Add(ArenaTeam.GetTeam(reader.ReadString()));

            c = reader.ReadInt();

            for (int i = 0; i < c; i++)
                m_OrigPartList.Add(ArenaTeam.GetTeam(reader.ReadString()));

            UseAlternateArena = reader.ReadBool();

            m_SponsorTotal = 0;
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                Mobile mob = reader.ReadMobile();
                int amt = reader.ReadInt();

                if (mob != null && amt > 0)
                {
                    m_Sponsors.Add(mob, amt);
                    m_SponsorTotal += amt;
                }
            }

            m_TourneyType = (TourneyType)reader.ReadInt();

            m_System = sys;
            m_Name = reader.ReadString();
            m_Creator = reader.ReadMobile();
            m_MaxParticipants = reader.ReadInt();
            m_MinParticipants = reader.ReadInt();
            m_EntryFee = reader.ReadInt();
            m_GoldCollected = reader.ReadInt();
            m_Round = reader.ReadInt();
            m_WarningMessage = reader.ReadInt();
            m_Fight = reader.ReadInt();
            m_FightType = (ArenaTeamType)reader.ReadInt();

            m_TimePerFight = reader.ReadTimeSpan();
            m_StartTime = reader.ReadDateTime();

            m_OnGoing = reader.ReadBool();

            m_ChampPrize = reader.ReadItem();
            m_RunnerUpPrize = reader.ReadItem();
            m_Rules = (FightRules)reader.ReadInt();

            if (m_Participants == null)
                m_Participants = new List<ArenaTeam>();

            int teams = reader.ReadInt();
            //List<string> temp = new List<string>();

            for (int i = 0; i < teams; ++i)
            {
                m_Participants.Add(ArenaTeam.GetTeam(reader.ReadString()));
            }
            // temp.Add(reader.ReadString());

            //if (temp.Count > 0)
            //    Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(BuildParticipantList), new object[] { temp, 2 });

                //This should only happen if server goes down in the middle of a tourney
            if (m_OnGoing)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(15), new TimerCallback(ResumeTournament));
                m_System.InUse = true;
                m_System.CurrentTournament = this;

                m_ArenaA = m_System;
            }

            m_System.AddTournament(this);
        }

        public void ResumeTournament()
        {
            m_Stats = PVPTournamentStats.TournamentStats.FirstOrDefault(stats => stats.Arena == m_System.Name && 
                                                                                 stats.TournamentName == m_Name);

            if (UseAlternateArena)
            {
                m_ArenaB = m_System.LinkedSystem;
                m_ArenaB.InUse = true;

                m_ArenaB.CurrentTournament = this;
            }

            new ResumeTimer(this);
        }

        private void BuildParticipantList(object o)
        {
            object[] obj = (object[])o;

            List<string> temp = obj[0] as List<string>;
            int type = (int)obj[1];

            if (temp == null || temp.Count == 0)
                return;

            for (int i = 0; i < temp.Count; ++i)
            {
                foreach (ArenaTeam team in ArenaTeam.Teams)
                {
                    if (team.Name == temp[i])
                    {
                        switch (type)
                        {
                            case 0: m_RoundMatches.Add(team); break;
                            case 1: m_OrigPartList.Add(team); break;
                            case 2: m_Participants.Add(team); break;
                        }

                        break;
                    }
                }
            }

            ColUtility.Free(temp);
        }

        private class ResumeTimer : Timer
        {
            private Tournament m_Tourney;
            private DateTime m_StartTime;
            private bool m_SentPM;

            public ResumeTimer(Tournament tourney) : base(TimeSpan.FromSeconds(60.0), TimeSpan.FromSeconds(60.0))
            {
                m_Tourney = tourney;
                m_StartTime = DateTime.Now + Config.TournamentResumeTime;
                m_SentPM = false;
                this.Start();
            }

            protected override void OnTick()
            {
                if (!m_Tourney.OnGoing)
                {
                    this.Stop();
                }
                else if (m_StartTime > DateTime.Now)
                {
                    TimeSpan ts = m_StartTime - DateTime.Now;
                    int minutes = (int)ts.Minutes;

                    string str;

                    if (minutes > 0)
                        str = String.Format("We appologize for the interruption.  The tournament will resume in approximately {0} {1}.", minutes.ToString(), minutes == 1 ? "minute" : "minutes");
                    else
                        str = "We appologize for the interruption.  The tournament will resume in less than a minute.";

                    if (!m_SentPM)
                    {
                        foreach (ArenaTeam team in m_Tourney.Participants)
                            ArenaHelper.DoTeamPM(team, "Resume Tournament", str, m_Tourney.System.ArenaKeeper, TimeSpan.FromMinutes(30));

                        m_SentPM = true;
                    }

                    m_Tourney.DoTournamentMessage(str, false);
                }
                else
                {
                    m_Tourney.NewFight();
                    this.Stop();
                }
            }
        }
    }
}
