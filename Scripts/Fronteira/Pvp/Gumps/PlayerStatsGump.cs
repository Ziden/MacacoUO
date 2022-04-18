using System;
using System.Linq;
using System.Collections.Generic;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Guilds;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public enum SortByOption
    {
        None,
        ArenaWins,
        TournamentWins,
        TournamentChampionships,
        Points,
        DamageRatio
    }

    public class PlayerStatsGump : BaseTournamentGump
    {
        private ArenaTeamType m_SortByTeamType;
        private SortByOption m_SortByOption;

        public override int LabelHue { get { return 1150; } }
        public int SelectHue { get { return 1160; } }

        public PlayerStatsGump(PlayerMobile from) : this(from, 0, 0)
        {
        }

        public PlayerStatsGump(PlayerMobile from, ArenaTeamType type, SortByOption option)
            : base(from, 20, 20)
        {
            m_SortByTeamType = type;
            m_SortByOption = option;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 800, 550, DarkBackground);
            AddBackground(316, 10, 167, 30, 9350);
            AddBackground(10, 55, 780, 485, LightBackground);

            AddHtml(0, 15, 800, 20, Center("Team Statistics"), false, false);

            AddHtml(45, 32, 100, 20, Color("#FFFFFF", "My Teams"), false, false);
            AddButton(10, 32, 4005, 4006, 1, GumpButtonType.Reply, 0);

            AddLabel(47, 70, LabelHue, "Time");
            AddLabel(202, 70, LabelHue, "Lutadores");
            AddLabel(282, 70, m_SortByOption == SortByOption.ArenaWins ? SelectHue : LabelHue, "Arena");
            AddLabel(382, 70, m_SortByOption == SortByOption.TournamentWins ? SelectHue : LabelHue, "Torneio");
            AddLabel(482, 70, m_SortByOption == SortByOption.TournamentChampionships ? SelectHue : LabelHue, "Campeonato");
            AddLabel(577, 70, m_SortByOption == SortByOption.Points ? SelectHue : LabelHue, "Pontos");
            AddLabel(682, 70, m_SortByOption == SortByOption.DamageRatio ? SelectHue : LabelHue, "Ratio de Dano");

            switch (m_SortByTeamType)
            {
                case ArenaTeamType.None:
                    AddLabel(54, 515, 0, "Ver Todos");
                    break;
                case ArenaTeamType.Single:
                    AddLabel(54, 515, 0, "Ver Singles");
                    break;
                case ArenaTeamType.Twosome:
                    AddLabel(54, 515, 0, "Ver Twosomes");
                    break;
                default:
                    AddLabel(54, 515, 0, "Ver Foursomes");
                    break;
            }

            AddButton(20, 515, 4011, 4012, 2, GumpButtonType.Reply, 0);

            List<ArenaTeam> useList;

            switch (m_SortByOption)
            {
                default:
                    AddLabel(207, 515, 0, "Sem Filtro");
                    useList = new List<ArenaTeam>(ArenaTeam.Teams);
                    break;
                case SortByOption.ArenaWins:
                    AddLabel(207, 515, 0, "Ver por vitorias");
                    useList = ArenaTeam.Teams.OrderByDescending(t => t.Wins).ToList();
                    break;
                case SortByOption.TournamentWins:
                    AddLabel(207, 515, 0, "Ver por torneios");
                    useList = ArenaTeam.Teams.OrderByDescending(t => t.TournamentWins).ToList();
                    break;
                case SortByOption.TournamentChampionships:
                    AddLabel(207, 515, 0, "Ver por campeonatos");
                    useList = ArenaTeam.Teams.OrderByDescending(t => t.TournamentChampionships).ToList();
                    break;
                case SortByOption.Points:
                    AddLabel(207, 515, 0, "Ver por pontos");
                    useList = ArenaTeam.Teams.OrderByDescending(t => t.Points).ToList();
                    break;
                case SortByOption.DamageRatio:
                    AddLabel(207, 515, 0, "Ver por ratio de dano");
                    useList = ArenaTeam.Teams.OrderByDescending(t => t.DamageGiven - t.DamageTaken).ToList();
                    break;
            }

            AddButton(173, 515, 4011, 4012, 3, GumpButtonType.Reply, 0);
            AddLabel(20, 492, ArenaTeam.InactiveHue, "* - Inactive");

            for (int i = useList.Count - 1; i >= 0; i--)
            {
                if (m_SortByTeamType != ArenaTeamType.None && m_SortByTeamType != useList[i].TeamType)
                {
                    useList.RemoveAt(i);
                }
            }

            int pages = (int)Math.Ceiling(useList.Count / 12.0);
            int count = 0;
            ArenaTeam team = null;

            for (int i = 1; i <= pages; i++)
            {
                AddPage(i);
                int yOffset = 100;

                for (int j = 0; j < 12; j++)
                {
                    if (count >= useList.Count)
                        break;

                    team = useList[count];
                    count++;
                    if (team == null)
                        continue;

                    string name = team.Name;
                    int color;
                    if (team.Active)
                        color = ArenaHelper.GetTeamTypeColor(team.TeamType);
                    else
                    {
                        color = ArenaTeam.InactiveHue;
                        name = "* " + team.Name;
                    }

                    int realIdx = GetTeamIndex(team);

                    AddLabelCropped(47, yOffset, 25, 20, color, String.Format("{0:##}.", count));
                    AddLabelCropped(72, yOffset, 145, 20, color, String.Format("{0}", name));

                    if (realIdx >= 0)
                    {
                        AddButton(202, yOffset + 3, 1209, 1210, 10 + realIdx, GumpButtonType.Reply, 0);
                        AddTooltip(Localizations.GetLocalization(40));
                    }

                    AddLabelCropped(222, yOffset, 55, 20, color, String.Format("{0}", team.TeamType));
                    AddLabelCropped(282, yOffset, 95, 20, color, String.Format("{0}/{1}", team.Wins, team.Losses));
                    AddLabelCropped(382, yOffset, 95, 20, color, String.Format("{0}/{1}", team.TournamentWins, team.TournamentLosses));
                    AddLabelCropped(482, yOffset, 55, 20, color, String.Format("{0}", team.TournamentChampionships));
                    AddLabelCropped(577, yOffset, 135, 20, color, String.Format("{0}", ((int)team.Points).ToString("N0")));
                    AddLabelCropped(682, yOffset, 55, 20, color, String.Format("{0}", GetDamageRatio(team)));

                    AddButton(10, yOffset, 0xFAB, 0xFAD, 100 + realIdx, GumpButtonType.Reply, 0);

                    yOffset += 30;
                }
                if (i != pages)
                    AddButton(760, 514, 2224, 2224, 0, GumpButtonType.Page, i + 1);
                if (i > 1)
                    AddButton(730, 514, 2223, 2223, 0, GumpButtonType.Page, i - 1);
            }
        }

        public static string GetDamageRatio(ArenaTeam team)
        {
            long given = Simplify(team.DamageGiven);
            long received = Simplify(team.DamageTaken);

            if (given == 0 || received == 0)
            {
                return String.Format("{0}:{1}", given, received);
            }

            long gcd = GCD(given, received);

            return String.Format("{0}:{1}", given / gcd, received / gcd);
        }

        private static long Simplify(long value)
        {
            if (value > 1000)
            {
                double g = (double)value / 1000.0;
                value = (int)g * 1000;
            }
            else if (value > 100)
            {
                double g = (double)value / 100.0;
                value = (int)g * 100;
            }

            return value;
        }

        private static long GCD(long a, long b)
        {
            return b == 0 ? Math.Abs(a) : GCD(b, a % b);
        }

        public static int GetTeamIndex(ArenaTeam team)
        {
            return ArenaTeam.Teams.IndexOf(team);
        }

        public override void OnResponse(RelayInfo button)
        {
            if (button.ButtonID < 10)
            {
                switch (button.ButtonID)
                {
                    default:
                    case 0:
                        break;
                    case 1: //My Team Stats
                        BaseGump.SendGump(new PlayerRecordGump(User));
                        break;
                    case 2: //Sort by Team Types
                        if (m_SortByTeamType >= ArenaTeamType.Foursome)
                        {
                            m_SortByTeamType = ArenaTeamType.Single;
                        }
                        else
                        {
                            m_SortByTeamType++;
                        }
                        Refresh();
                        break;
                    case 3: //Sort by Stats
                        if (m_SortByOption >= SortByOption.DamageRatio)
                        {
                            m_SortByOption = SortByOption.None;
                        }
                        else
                        {
                            m_SortByOption++;
                        }
                        Refresh();
                        break;
                }
            }
            else
            {
                if (button.ButtonID >= 100)
                {
                    var id = button.ButtonID - 100;

                    if (id >= 0 && id < ArenaTeam.Teams.Count)
                    {
                        BaseGump.SendGump(new PlayerRecordGump(User, ArenaTeam.Teams[id]));
                    }
                }
                else
                {
                    var id = button.ButtonID - 10;

                    if (id >= 0 && id < ArenaTeam.Teams.Count)
                    {
                        BaseGump.SendGump(new FightersGump(ArenaTeam.Teams[id], User, m_SortByTeamType, m_SortByOption));
                    }
                }
            }
        }
    }

    public class FightersGump : BaseTournamentGump
    {
        private ArenaTeam m_Team;
        private ArenaTeamType m_Type;
        private SortByOption m_Option;

        public FightersGump(ArenaTeam team, PlayerMobile from, ArenaTeamType type, SortByOption sort)
            : base(from, 20, 20)
        {
            m_Team = team;
            m_Type = type;
            m_Option = sort;
        }

        public override void AddGumpLayout()
        {
            AddBackground(0, 0, 300, 200, DarkBackground);
            AddBackground(10, 10, 280, 180, LightBackground);

            AddHtml(0, 15, 300, 20, ColorAndCenter("#FFFFFF", "Players"), false, false);
            int y = 50;
            int x = 20;

            for (int i = 0; i < m_Team.Fighters.Count; ++i)
            {
                Mobile fighter = m_Team.Fighters[i];
                string name;
                Guild guild = m_Team.Fighters[i].Guild as Guild;

                if (guild != null)
                {
                    name = fighter.Name + " [" + guild.Abbreviation + "]";
                }
                else
                    name = fighter.Name;

                AddLabel(x, y, 0, name);
                y += 30;
            }

            AddButton(12, 168, 4014, 4015, 1, GumpButtonType.Reply, 0);
            AddLabel(45, 168, 0, "Back");

            if (User.AccessLevel >= AccessLevel.GameMaster)
            {
                AddButton(260, 168, 4008, 4010, 2, GumpButtonType.Reply, 0);
                AddLabel(190, 168, 0, "Team Props");
            }
        }

        public override void OnResponse(RelayInfo button)
        {
            switch (button.ButtonID)
            {
                default:
                case 0: break;
                case 1:
                    BaseGump.SendGump(new PlayerStatsGump(User, m_Type, m_Option));
                    break;
                case 2:
                    User.SendGump(new PropertiesGump(User, m_Team));
                    break;
            }
        }
    }
}
