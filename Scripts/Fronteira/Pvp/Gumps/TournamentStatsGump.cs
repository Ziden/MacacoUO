using System;
using System.Collections.Generic;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class TournamentStatsGump : BaseTournamentGump
    {
        public PVPTournamentSystem System { get; private set; }
        public bool ShowAll { get; private set; }

        public TournamentStatsGump(PlayerMobile from, PVPTournamentSystem sys) : base(from, 20, 20)
        {
            System = sys;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 700, 550, DarkBackground);
            AddBackground(150, 10, 400, 30, 9350);
            AddBackground(10, 55, 680, 485, LightBackground);

            //AddHtml(0, 15, 700, 20, Center(String.Format("Tournament Stats: {0}", ShowAll || System == null ? "All Arenas" : System.Name)), false, false);
            AddHtml(0, 15, 700, 20, Center(String.Format("Estatísticas: {0}", ShowAll || System == null ? "Todas as Arenas" : System.Name)), false, false);
            
            /*AddLabel(70, 70, LabelHue, "Tournament");
            AddLabel(200, 70, LabelHue, "Winner");
            AddLabel(400, 70, LabelHue, "Runner Up");
            AddLabel(600, 70, LabelHue, "Date");*/
            AddLabel(70, 70, LabelHue, "Torneio");
            AddLabel(200, 70, LabelHue, "Vencedor");
            AddLabel(400, 70, LabelHue, "Vice");
            AddLabel(600, 70, LabelHue, "Data");

            if (PVPTournamentStats.TournamentStats == null || PVPTournamentStats.TournamentStats.Count == 0)
            {
                return;
            }

            AddButton(10, 518, 4014, 4016, 1, GumpButtonType.Reply, 0);
            //AddHtml(55, 518, 150, 20, ShowAll ? "Show This Arena Only" : "Show All Arenas", false, false);
            AddHtml(55, 518, 150, 20, ShowAll ? "Mostrar apenas esta Arena" : "Mostrar todas as Arenas", false, false);

            List<PVPTournamentStats> statsList = new List<PVPTournamentStats>();

            foreach (PVPTournamentStats stat in PVPTournamentStats.TournamentStats)
            {
                if (System == null || ShowAll)
                {
                    statsList.Add(stat);
                }
                else if (stat.Arena == System.Name)
                {
                    statsList.Add(stat);
                }
            }

            int pages = (int)Math.Ceiling(statsList.Count / 12.0);

            int count = 0;
            int yOffset = 100;
            string name1 = "";
            string name2 = "";
            DateTime winTime = DateTime.MinValue;
            PVPTournamentStats stats = null;

            for (int i = 1; i <= pages; i++)
            {
                AddPage(i);
                yOffset = 100;

                for (int j = 0; j < 12; ++j)
                {
                    if (count >= statsList.Count)
                        break;

                    stats = statsList[count];
                    count++;

                    if (stats == null)
                        continue;

                    if (stats.Winner == null)
                        //name1 = "Unknown";
                        name1 = "Desconhecido";
                    else
                        name1 = stats.Winner;

                    if (stats.RunnerUp == null)
                        //name2 = "Unknown";
                        name2 = "Desconhecido";
                    else
                        name2 = stats.RunnerUp;

                    if (stats.WinDate != null)
                        winTime = stats.WinDate.Date;

                    int idx = GetStatsIndex(stats);

                    if (idx >= 0)
                        AddButton(15, yOffset, 1209, 1210, idx + 10, GumpButtonType.Reply, 0);
                    AddHtml(45, yOffset, 10, 16, String.Format("{0:##}.", count), false, false);
                    AddHtml(70, yOffset, 155, 16, String.Format("{0}", stats.TournamentName), false, false);
                    AddHtml(200, yOffset, 195, 16, String.Format("{0}", name1), false, false);
                    AddHtml(400, yOffset, 195, 16, String.Format("{0}", name2), false, false);
                    //AddHtml(600, yOffset, 100, 16, String.Format("{0}", winTime == DateTime.MinValue ? "Unknown" : winTime.ToString()), false, false);
                    AddHtml(600, yOffset, 100, 16, String.Format("{0}", winTime == DateTime.MinValue ? "Desconhecido" : winTime.ToString()), false, false);
                    
                    yOffset += 25;
                }

                if (i != pages)
                    AddButton(460, 515, 2224, 2224, 0, GumpButtonType.Page, i + 1); //next
                if (i > 1)
                    AddButton(430, 515, 2223, 2223, 0, GumpButtonType.Page, i - 1); //previous
            }

            ColUtility.Free(statsList);
        }

        private int GetStatsIndex(PVPTournamentStats stats)
        {
            for(int i = 0; i < PVPTournamentStats.TournamentStats.Count; ++i)
            {
                if (PVPTournamentStats.TournamentStats[i] == stats)
                    return i;
            }
            return -1;
        }

        public override void OnResponse(RelayInfo button)
        {
            if (button.ButtonID == 1)
            {
                if (ShowAll)
                    ShowAll = false;
                else
                    ShowAll = true;

                Refresh();
            }
            else if (button.ButtonID > 9)
            {
                int idx = button.ButtonID - 10;

                User.CloseGump(typeof(TournamentStatsGump2));

                try
                {
                    PVPTournamentStats stat = PVPTournamentStats.TournamentStats[idx];

                    if (System != null && System.CurrentTournament != null && System.CurrentTournament.Stats == stat)
                        BaseGump.SendGump(new TournamentStatsGump2(System.CurrentTournament, null, User));
                    else
                        BaseGump.SendGump(new TournamentStatsGump2(stat, User));
                }
                catch
                {
                    Refresh();
                    //User.SendMessage("There was an error fetching the specific stats for that tournament.");
                    User.SendMessage("Erro ao buscar estatísticas deste torneio.");
                }
            }
        }
    } 
}