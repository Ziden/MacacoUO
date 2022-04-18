using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Network;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public enum ArenaTeamType
    {
        None = 0,
        Single = 1,
        Twosome = 2,
        Foursome = 4,
        Temp = 5
    }

    [PropertyObject]
    public class ArenaTeam
    {
        public static readonly int InactiveHue = 32;

        private Mobile _TeamLeader;

        private List<Mobile> _Fighters = new List<Mobile>();

        [CommandProperty(AccessLevel.Counselor)]
        public string Name { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile TeamLeader
        {
            get
            {
                if (_TeamLeader == null && _Fighters.Count > 0)
                    _TeamLeader = _Fighters[0];

                return _TeamLeader;
            }
            set { _TeamLeader = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeamType TeamType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TournamentWins { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TournamentLosses { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TournamentChampionships { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Wins { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Losses { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Draws { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Points { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextRename { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SingleElimWins { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BestOf3Wins { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LastManStandingWins { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CTFWins { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CTFTeamRumbleWins { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public long DamageGiven { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public long DamageTaken { get; set; }

        public List<Mobile> Fighters
        {
            get
            {
                if (_Fighters == null)
                    _Fighters = new List<Mobile>();

                return _Fighters;
            }
        }

        public List<string> FighterHistory { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public TeamRecord LeagueRecord { get; set; }

        public bool Disbanded { get { return _Fighters == null || _Fighters.Count == 0; } }

        public bool Temp { get { return TeamType == ArenaTeamType.Temp; } }

        private static List<ArenaTeam> m_Teams = new List<ArenaTeam>();
        public static List<ArenaTeam> Teams { get { return m_Teams; } }

        public static void DefragTeams()
        {
            for (int i = 0; i < m_Teams.Count; i++)
            {
                for (int j = 0; j < m_Teams[i].Fighters.Count; j++)
                {
                    if (m_Teams[i].Fighters[j] == null || m_Teams[i].Fighters[j].Deleted)
                        m_Teams[i].Kick(m_Teams[i].Fighters[j]);
                }
            }
        }

        /*public static void ResetTeamRumbleStats()
        {
            var teams = m_Teams.Where(t => t.CTFTeamRumbleWins > 0).OrderByDescending(t => t.CTFTeamRumbleWins).ToList();
            var copy = new List<ArenaTeam>(teams);

            if (teams.Count > 0)
            {
                int wins = teams[0].CTFTeamRumbleWins;

                for (int i = teams.Count - 1; i >= 0; i--)
                {
                    var team = teams[i];

                    if (team.CTFTeamRumbleWins < wins)
                    {
                        copy.Remove(team);
                    }

                    team.CTFTeamRumbleWins = 0;
                }

                CTFArena.LastWinner = copy.Count > 1 ? copy[Utility.Random(copy.Count)].TeamLeader : copy[0].TeamLeader;

                foreach (var ctf in PVPTournamentSystem.SystemList.OfType<CTFArena>().Where(arena => arena.LeaderBoard != null))
                {
                    ctf.LeaderBoard.InvalidateProperties();
                }
            }

            ColUtility.Free(teams);
            ColUtility.Free(copy);

            Utility.WriteConsoleColor(ConsoleColor.Green, string.Format("CTF Team Rumble leader boards have been reset!. Next reset: {0}", CTFArena.NextCTFReset.ToShortDateString()));
        }*/

        public ArenaTeam(Mobile leader)
        {
            _TeamLeader = leader;
            _Fighters.Add(leader);
            TournamentWins = 0;
            TournamentLosses = 0;
            Wins = 0;
            Losses = 0;
            Draws = 0;
            Active = false;
            TeamType = ArenaTeamType.None;
            NextRename = DateTime.UtcNow;
        }

        public ArenaTeam()
        {
            TeamType = ArenaTeamType.Temp;
            Active = true;
        }

        public override string ToString()
        {
            if (Name != null)
                return Name;

            return "...";
        }

        public void RegisterTeam()
        {
            m_Teams.Add(this);
            Active = true;
        }

        public void RegisterWin(ArenaTeam defeated, ArenaFight fight, ArenaFightType type)
        {
            if (Temp)
                return;

            double award = fight.IsTournament ? 20 : 10;

            if (type != ArenaFightType.LastManStanding && defeated != null && defeated.Points > Points)
                award += Math.Max(20, Math.Sqrt(defeated.Points - Points));
            else if (type == ArenaFightType.LastManStanding)
                award += fight.Teams.Count / 2;

            SendMessageToFighters(String.Format("Your team has earned {0} points for achieving victory in {1}!", (int)award, ArenaHelper.GetFightType(type)));

            Points += award;
        }

        public void AddFighter(Mobile mob)
        {
            AddFighter(mob, true);
        }

        public void AddFighter(Mobile mob, bool exists)
        {
            if (!IsInTeam(mob))
                _Fighters.Add(mob);

            if (exists)
            {
                OnFighterChange();
            }

            if (FighterHistory == null)
            {
                FighterHistory = new List<string>();
            }

            FighterHistory.Add(mob.Name);
        }

        public bool IsInTeam(Mobile m)
        {
            return Fighters.Contains(m);
        }

        public void SendMessageToFighters(string message)
        {
            foreach (Mobile m in _Fighters)
            {
                if (m != null)
                    m.SendMessage(ArenaHelper.ParticipantMessageHue, message);
            }
        }

        /// <summary>
        /// justdied needs to be passed in as region OnDeath is called before a mobile.Alive is set
        /// </summary>
        /// <param name="justdied">the mobile that just died</param>
        /// <returns></returns>
        public bool AllDead(Mobile justdied)
        {
            return !Fighters.Any(
                    mob => justdied != mob && 
                    mob.Alive && 
                    mob.NetState != null &&
                    mob.Region != null &&
                    mob.Region.IsPartOf<FightRegion>());
        }

        public bool AllDead()
        {
            return !Fighters.Any(
                    mob => mob.Alive &&
                    mob.NetState != null &&
                    mob.Region != null &&
                    mob.Region.IsPartOf<FightRegion>());
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((int)4); //Version!!!

            writer.Write(FighterHistory == null ? 0 : FighterHistory.Count);

            if (FighterHistory != null)
            {
                for (int i = 0; i < FighterHistory.Count; i++)
                {
                    writer.Write(FighterHistory[i]);
                }
            }

            if (LeagueRecord != null)
            {
                writer.Write(0);
                LeagueRecord.Serialize(writer);
            }
            else
            {
                writer.Write(1);
            }

            writer.Write(SingleElimWins);
            writer.Write(BestOf3Wins);
            writer.Write(LastManStandingWins);
            writer.Write(CTFWins);
            writer.Write(CTFTeamRumbleWins);

            writer.Write(DamageGiven);
            writer.Write(DamageTaken);

            writer.Write(NextRename);
            writer.Write(Points);
            writer.Write(Name);
            writer.Write(Active);
            writer.Write(_TeamLeader);
            writer.Write(TournamentWins);
            writer.Write(TournamentLosses);
            writer.Write(TournamentChampionships);
            writer.Write(Wins);
            writer.Write(Losses);
            writer.Write(Draws);
            writer.Write((int)TeamType);

            writer.Write(_Fighters.Count);

            for (int i = 0; i < _Fighters.Count; ++i)
                writer.Write(_Fighters[i]);
        }

        public ArenaTeam(GenericReader reader)
        {
            int version = reader.ReadInt();

            FighterHistory = new List<string>();

            switch (version)
            {
                case 4:
                case 3:
                    int c = reader.ReadInt();

                    for (int i = 0; i < c; i++)
                    {
                        FighterHistory.Add(reader.ReadString());
                    }
                    goto case 2;
                case 2:
                    if (reader.ReadInt() == 0)
                    {
                        LeagueRecord = new TeamRecord(reader);
                    }
                    goto case 1;
                case 1:
                    SingleElimWins = reader.ReadInt();
                    BestOf3Wins = reader.ReadInt();
                    LastManStandingWins = reader.ReadInt();
                    CTFWins = reader.ReadInt();

                    if (version > 3)
                    {
                        CTFTeamRumbleWins = reader.ReadInt();
                    }
                    goto case 0;
                case 0:
                    DamageGiven = reader.ReadLong();
                    DamageTaken = reader.ReadLong();

                    NextRename = reader.ReadDateTime();

                    NextRename = DateTime.UtcNow;
                    Points = reader.ReadDouble();

                    Name = reader.ReadString();
                    Active = reader.ReadBool();
                    _TeamLeader = reader.ReadMobile();
                    TournamentWins = reader.ReadInt();
                    TournamentLosses = reader.ReadInt();
                    TournamentChampionships = reader.ReadInt();
                    Wins = reader.ReadInt();
                    Losses = reader.ReadInt();
                    Draws = reader.ReadInt();
                    TeamType = (ArenaTeamType)reader.ReadInt();
                    break;
            }

            if (_Fighters == null)
                _Fighters = new List<Mobile>();

            bool checkchange = false;
            int count = reader.ReadInt();

            for (int i = 0; i < count; ++i)
            {
                Mobile mob = reader.ReadMobile();

                if (mob != null && !IsInTeam(mob))
                {
                    _Fighters.Add(mob);
                }
                else if (mob == null && !checkchange)
                {
                    checkchange = true;
                }
            }

            m_Teams.Add(this);
            // For now, any team w/ BaseCreature is not added
            /*if (_Fighters.All(f => f is PlayerMobile))
            {
                m_Teams.Add(this);
            }*/

            if (checkchange)
            {
                OnFighterChange();
            }
        }

        public static bool HasTeam(Mobile from)
        {
            return m_Teams.Any(t => t.Fighters.Contains(from));
        }

        public static bool HasTeam(Mobile from, ArenaTeamType type)
        {
            return m_Teams.Any(t => t.TeamType == type && t.Fighters.Contains(from));
        }

        public static ArenaTeam GetTeam(Mobile from, ArenaTeamType type)
        {
            return GetTeam(from, type, false);
        }

        public static ArenaTeam GetTeam(Mobile from, ArenaTeamType type, bool create)
        {
            ArenaTeam team = m_Teams.FirstOrDefault(t => t.TeamType == type && t.Fighters.Contains(from));

            if (team == null && create)
            {
                team = new ArenaTeam(from);
                team.TeamType = type;
                string name = String.Format("Team {0}", from.Name);

                if (ArenaTeam.NameExists(name))
                {
                    for (int i = 0; i < 5000; i++)
                    {
                        name = name + i.ToString();

                        if (!NameExists(name))
                            break;

                    }
                }

                team.Name = name;
                team.RegisterTeam();
                from.SendMessage(0x32, "{0} Arena Team auto-created.", type.ToString());
            }

            return team;
        }

        public static ArenaTeam GetTeam(string name)
        {
            return Teams.FirstOrDefault(t => t.Name == name);
        }

        public static bool IsTeamLeader(Mobile from, ArenaTeam team)
        {
            return team.TeamLeader == from;
        }

        public static IEnumerable<ArenaTeam> GetTeams(Mobile from)
        {
            foreach (var team in m_Teams.Where(t => t.Fighters.Contains(from)))
            {
                yield return team;
            }
        }

        public void OnFighterChange()
        {
            if (Temp)
            {
                return;
            }

            if (_Fighters == null)
            {
                Active = false;
                return;
            }

            switch (_Fighters.Count)
            {
                default:
                case 0: Active = false; break;
                case 1:
                    {
                        if (TeamType != ArenaTeamType.Single) Active = false;
                        else Active = true;
                        break;
                    }
                case 2:
                    {
                        if (TeamType != ArenaTeamType.Twosome) Active = false;
                        else Active = true;
                        break;
                    }
                case 4:
                    {
                        if (TeamType != ArenaTeamType.Foursome) Active = false;
                        else Active = true;
                        break;
                    }
            }

            if (_Fighters.Count > 0 && !IsInTeam(_TeamLeader))
            {
                _TeamLeader = _Fighters[0];
                string subject = "New Leader";
                string text1 = String.Format("{0} is now the new team leader of {1} by default.", _TeamLeader.Name, Name);
                string text2 = String.Format("You are now the new team leader of {0} by default.", Name);
                foreach (Mobile mob in _Fighters)
                {
                    ArenaHelper.DoTeamPM(mob, subject, mob == _TeamLeader ? text1 : text2, _TeamLeader);
                }
            }
            else if (Disbanded)
            {
                DisbandTeam();
            }
        }

        private void DisbandTeam()
        {
        }

        public Mobile GetLeadership()
        {
            if (Temp)
            {
                if (_Fighters.Count > 0)
                {
                    return _Fighters[0];
                }

                return null;
            }
            else
            {
                if (_TeamLeader != null && _TeamLeader.NetState != null)
                {
                    return _TeamLeader;
                }

                for (int i = 0; i < _Fighters.Count; ++i)
                {
                    if (_Fighters[i].NetState != null)
                    {
                        return _Fighters[i];
                    }
                }
            }

            return null;
        }

        public int GetPlayerCount()
        {
            return _Fighters.Count;
        }

        public void Resign(Mobile from)
        {
            if (from == null)
            {
                return;
            }

            if (IsInTeam(from))
            {
                _Fighters.Remove(from);
            }

            from.SendMessage("You have resigned from {0}.", Name);

            if (_Fighters.Count > 0)
            {
                ArenaHelper.DoTeamPM(this, "Resignation", String.Format("{0} has resigned from {1} Arena Team.", from.Name, Name), TeamLeader);
            }

            OnFighterChange();
        }

        public void Kick(Mobile from)
        {
            if (from == null || from.Deleted)
            {
                if (IsInTeam(from))
                {
                    _Fighters.Remove(from);
                }

                OnFighterChange();
                return;
            }

            if (IsInTeam(from))
            {
                _Fighters.Remove(from);
            }

            ArenaHelper.DoTeamPM(from, "Kick", String.Format("You have been kicked from {0} Arena Team.", Name), TeamLeader);

            foreach (Mobile fighter in _Fighters)
            {
                if (fighter != _TeamLeader)
                {
                    ArenaHelper.DoTeamPM(fighter, "Kick", String.Format("{0} has been kicked from your arena team, {0}.", from.Name, Name), TeamLeader);
                }
            }

            OnFighterChange();
        }

        public static bool NameExists(string name)
        {
            return m_Teams.Any(t => t.Name == name);
        }

        public bool CanRename(Mobile from)
        {
            if (_TeamLeader != from || NextRename > DateTime.UtcNow)
            {
                return false;
            }

            IPooledEnumerable eable = from.Map.GetMobilesInRange(from.Location, 10);

            foreach (Mobile m in eable)
            {
                if (m is ArenaKeeper)
                {
                    eable.Free();
                    return true;
                }
            }

            eable.Free();
            return false;
        }

        public void TryRename(Mobile from, string name)
        {
            ErrorType type = RegisterTeamGump.ValidateName(name);

            switch (type)
            {
                default:
                case ErrorType.Valid:
                    IPooledEnumerable eable = from.Map.GetMobilesInRange(from.Location, 10);
                    Mobile keeper = null;
                    foreach (Mobile m in eable)
                    {
                        if (m is ArenaKeeper)
                        {
                            keeper = m;
                            break;
                        }
                    }

                    if (keeper != null)
                    {
                        if (Banker.Withdraw(from, 50000))
                        {
                            Name = name;
                            NextRename = DateTime.UtcNow + TimeSpan.FromDays(7);
                            ArenaHelper.DoArenaKeeperMessage(String.Format("Your arena team is now named {0}. You must wait about a week before you can change your name again.", name), from);
                        }
                        else
                            ArenaHelper.DoArenaKeeperMessage("You lack the required funds to change your team name.", from);
                    }
                    else
                        from.SendMessage("You must rename your team near an Arena Keeper for their blessing!");

                    break;
                case ErrorType.Invalid:
                    from.SendMessage(0x23, "You have chosen an invalid name.");
                    break;
                case ErrorType.TooManyChars:
                    from.SendMessage(0x23, "The name must be no more than 16 characters.");
                    break;
                case ErrorType.NotEnoughChars:
                    from.SendMessage(0x23, "Name too short.");
                    break;
                case ErrorType.AlreadyExists:
                    from.SendMessage(0x23, "That arena team name already exists.");
                    break;
                case ErrorType.Unacceptable:
                    from.SendMessage(0x23, "That name is unacceptable.");
                    break;
            }
        }
    }
}
