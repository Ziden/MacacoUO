using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using Server;
using Server.Mobiles;
using Server.Gumps;
using Server.Prompts;
using Server.Targeting;

namespace Server.TournamentSystem
{
    public class LeaderBoardGump : BaseTournamentGump
    {
        public override int LabelHue { get { return 1150; } }
        public int SelectHue { get { return 1160; } }

        public int FilterTeamType = -1;
        public int FilterFightType = -1;

        public List<LeaderStats> Stats { get; set; }

        public LeaderBoardGump(PlayerMobile from)
            : base(from, 20, 20)
        {
            Stats = LeaderStats.Stats;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 800, 550, DarkBackground);
            AddBackground(250, 10, 300, 30, 9350);
            AddBackground(10, 55, 780, 485, LightBackground);

            if (Stats is LeaderHistory)
            {
                //AddHtml(0, 15, 800, 20, Center(string.Format("Leader Board ending {0}", LeaderStats.FormatHistoricLeaderBoard((LeaderHistory)Stats))), false, false);
                AddHtml(0, 15, 800, 20, Center(string.Format("Classificação Final {0}", LeaderStats.FormatHistoricLeaderBoard((LeaderHistory)Stats))), false, false);
            }
            else if (Stats is LeaderHistory)
            {
                //AddHtml(0, 15, 800, 20, Center("Current Leader Board"), false, false);
                AddHtml(0, 15, 800, 20, Center("Classificação Atual"), false, false);
            }

            int yStart = 70;

            //AddLabel(15, yStart - 2, LabelHue, "Team");
            AddLabel(15, yStart - 2, LabelHue, "Time");
            /*AddLabel(215, yStart - 2, FilterFightType == 0 ? SelectHue : LabelHue, "Single Elim");
            AddLabel(315, yStart - 2, FilterFightType == 1 ? SelectHue : LabelHue, "Best of 3");
            AddLabel(415, yStart - 2, FilterFightType == 2 ? SelectHue : LabelHue, "Last Man");
            AddLabel(515, yStart - 2, FilterFightType == 3 ? SelectHue : LabelHue, "CTF");
            AddLabel(615, yStart - 2, FilterFightType == 4 ? SelectHue : LabelHue, "Team Rumble");
            AddLabel(715, yStart - 2, FilterFightType == -1 ? SelectHue : LabelHue, "Total");*/
            AddLabel(215, yStart - 2, FilterFightType == 0 ? SelectHue : LabelHue, "Melhor de 1");
            AddLabel(315, yStart - 2, FilterFightType == 1 ? SelectHue : LabelHue, "Melhor de 3");
            AddLabel(415, yStart - 2, FilterFightType == 2 ? SelectHue : LabelHue, "Último Sobrevivente");
            AddLabel(515, yStart - 2, FilterFightType == 3 ? SelectHue : LabelHue, "CTF");
            AddLabel(615, yStart - 2, FilterFightType == 4 ? SelectHue : LabelHue, "Team Rumble");
            AddLabel(715, yStart - 2, FilterFightType == -1 ? SelectHue : LabelHue, "Total");

            var reset = Config.LeaderboardStatReset;
            var stats = Stats.Where(s => LeaderStats.CheckTeamTypeFilter(s, FilterTeamType)).OrderByDescending(s => LeaderStats.CheckFightTypeFilter(s, FilterFightType)).ToList();

            int pages = (int)Math.Ceiling(stats.Count / 12.0);
            int count = 0;
            LeaderStats stat = null;

            //AddHtml(12, 460, 376, 79, Color("#0d0d0d", string.Format("Leader Board stats will reset {0}. If you wish to be tracked for CTF Team Rumble, make sure you are in a registered single arena team.", reset == TimeSpan.FromDays(30) ? "on the 1st of every month" : string.Format("every {0}{1}", reset.TotalDays, reset.TotalDays == 1 ? "day" : "days"))), true, true);
            AddHtml(12, 460, 376, 79, Color("#0d0d0d", string.Format("Quadro de Líderes vai resetar {0}. Se deseja participar, tenha certeza que você está registrado em um time.", reset == TimeSpan.FromDays(30) ? "no primeira dia de cada mês" : string.Format("a cada {0}{1}", reset.TotalDays, reset.TotalDays == 1 ? "dia" : "dias"))), true, true);

            AddButton(393, 460, 4005, 4007, 1, GumpButtonType.Reply, 0);
            //AddLabel(426, 460, 0, string.Format("Fight Type: {0}", FilterFightType == -1 ? "Total" : ArenaHelper.GetFightType((ArenaFightType)FilterFightType)));
            AddLabel(426, 460, 0, string.Format("Tipo do PVP: {0}", FilterFightType == -1 ? "Total" : ArenaHelper.GetFightType((ArenaFightType)FilterFightType)));

            AddButton(393, 485, 4005, 4007, 2, GumpButtonType.Reply, 0);
            //AddLabel(426, 485, 0, string.Format("Team Type: {0}", ArenaHelper.GetTeamType((ArenaTeamType)FilterTeamType)));
            AddLabel(426, 485, 0, string.Format("Tipo do Time: {0}", ArenaHelper.GetTeamType((ArenaTeamType)FilterTeamType)));

            if (LeaderStats.History.Count > 0)
            {
                AddButton(393, 510, 4005, 4007, 3, GumpButtonType.Reply, 0);
                //AddLabel(426, 510, 0, "Past Leader Boards");
                AddLabel(426, 510, 0, "Classificações Anteriores");
            }

            for (int i = 1; i <= pages; i++)
            {
                AddPage(i);
                var yOffset = yStart + 20;

                for (int j = 0; j < 12; j++)
                {
                    if (count >= stats.Count)
                        break;

                    stat = stats[count++];

                    if (stat == null)
                        continue;
                    
                    //AddLabelCropped(15, yOffset, 250, 20, LabelHue, stat.Team == null ? "Unknown Team" : stat.Team.Name);
                    AddLabelCropped(15, yOffset, 250, 20, LabelHue, stat.Team == null ? "Time Desconhecido" : stat.Team.Name);
                    AddLabelCropped(215, yOffset, 95, 20, FilterFightType == 0 ? SelectHue : LabelHue, stat.SingleElimWins.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US")));
                    AddLabelCropped(315, yOffset, 95, 20, FilterFightType == 1 ? SelectHue : LabelHue, stat.BestOf3Wins.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US")));
                    AddLabelCropped(415, yOffset, 95, 20, FilterFightType == 2 ? SelectHue : LabelHue, stat.LastManStandingWins.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US")));
                    AddLabelCropped(515, yOffset, 95, 20, FilterFightType == 3 ? SelectHue : LabelHue, stat.CTFWins.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US")));
                    AddLabelCropped(615, yOffset, 95, 20, FilterFightType == 4 ? SelectHue : LabelHue, stat.CTFTeamRumbleWins.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US")));
                    AddLabelCropped(715, yOffset, 95, 20, FilterFightType == -1 ? SelectHue : LabelHue, stat.TotalWins.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US")));

                    yOffset += 30;
                }

                if (i != pages)
                    AddButton(760, 517, 2224, 2224, 0, GumpButtonType.Page, i + 1);
                if (i > 1)
                    AddButton(727, 517, 2223, 2223, 0, GumpButtonType.Page, i - 1);
            }

            ColUtility.Free(stats);
        }

        public override void OnResponse(RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 0: break;
                case 1:
                    if (FilterFightType < 4)
                    {
                        FilterFightType++;
                    }
                    else
                    {
                        FilterFightType = -1;
                    }

                    Refresh();
                    break;
                case 2:
                    switch (FilterTeamType)
                    {
                        default:
                        case -1: FilterTeamType = 1; break;
                        case 0: FilterTeamType = 1; break;
                        case 1: FilterTeamType = 2; break;
                        case 2: FilterTeamType = 4; break;
                        case 3: FilterTeamType = 4; break;
                        case 4: FilterTeamType = -1; break;
                    }

                    Refresh();
                    break;
                case 3:
                    User.CloseGump(typeof(ChooseHistoryGump));
                    Refresh();
                    BaseGump.SendGump(new ChooseHistoryGump(User, this));
                    break;
            }
        }
    }

    public class ChooseHistoryGump : BaseTournamentGump
    {
        public ChooseHistoryGump(PlayerMobile pm, LeaderBoardGump parent)
            : base(pm, 250, 250, parent)
        {
        }

        public override void AddGumpLayout()
        {
            var count = LeaderStats.History.Count;
            var length = Math.Min(200 + (25 * count), 500);

            AddPage(0);
            AddBackground(0, 0, 400, length, DarkBackground);
            AddBackground(100, 10, 200, 30, 9350);
            AddBackground(10, 55, 380, length - 65, LightBackground);

            //AddHtml(0, 15, 400, 20, Center("Past Leaderboards"), false, false);
            AddHtml(0, 15, 400, 20, Center("Classificações Anteriores"), false, false);

            int pages = (int)Math.Ceiling(LeaderStats.History.Count / 12.0);
            int index = 0;

            for (int i = 1; i <= pages; i++)
            {
                AddPage(i);
                var yOffset = 60;

                for (int j = 0; j < 12; j++)
                {
                    if (index >= LeaderStats.History.Count)
                    {
                        break;
                    }

                    var history = LeaderStats.History[index];

                    if (history == null)
                    {
                        break;
                    }

                    AddButton(15, yOffset, 4005, 4007, index + 100, GumpButtonType.Reply, 0);
                    AddLabel(48, yOffset, 0, LeaderStats.FormatHistoricLeaderBoard(history));

                    index++;
                    yOffset += 25;
                }

                if (i != pages)
                    AddButton(760, 517, 2224, 2224, 0, GumpButtonType.Page, i + 1);
                if (i > 1)
                    AddButton(727, 517, 2223, 2223, 0, GumpButtonType.Page, i - 1);
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID >= 100)
            {
                var id = info.ButtonID - 100;
                var parent = Parent as LeaderBoardGump;

                if (parent != null && id < LeaderStats.History.Count)
                {
                    parent.Stats = LeaderStats.History[id];
                    parent.Refresh();
                }
            }
        }
    }
}

