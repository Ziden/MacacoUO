using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class TournamentStatsGump2 : BaseTournamentGump
    {
        private Tournament Tournament { get; set; }
        private PVPTournamentStats Stats { get; set; }

        public TournamentStatsGump2(PVPTournamentStats stats, PlayerMobile from) : this(null, stats, from)
        {
        }

        public TournamentStatsGump2(Tournament tourney, PVPTournamentStats stats, PlayerMobile from)
            : base(from, 20, 20)
        {
            Tournament = tourney;

            if (Tournament != null)
                Stats = Tournament.Stats;
            else
                Stats = stats;
        }

        public override void AddGumpLayout()
        {
            int length = 150;

            if (Stats != null)
                length += Stats.RoundOneFights() * 20;

            AddPage(0);
            AddBackground(0, 0, 800, length, DarkBackground);
            AddBackground(10, 80, 780, length - 90, LightBackground);

            /*AddLabel(40, 80, LabelHue, "Duel");
            AddLabel(300, 80, LabelHue, "Arena");
            AddLabel(550, 80, LabelHue, "Fight Winner");*/
            AddLabel(40, 80, LabelHue, "Duelo");
            AddLabel(300, 80, LabelHue, "Arena");
            AddLabel(550, 80, LabelHue, "Vencedor");

            if (Tournament != null)
            {
                /*AddLabel(15, 10, LabelHue, String.Format("Tournament: {0}", Tournament.Name));
                AddLabel(15, 30, LabelHue, String.Format("Teams Left in Tournament: {0}", Tournament.Participants.Count));
                AddLabel(15, 50, LabelHue, String.Format("Current Round: {0}", Tournament.Round));*/
                AddLabel(15, 10, LabelHue, String.Format("Torneio: {0}", Tournament.Name));
                AddLabel(15, 30, LabelHue, String.Format("Times restantes no Torneio: {0}", Tournament.Participants.Count));
                AddLabel(15, 50, LabelHue, String.Format("Round Atual: {0}", Tournament.Round));
            }
            else if (Stats != null)
            {
                /*AddLabel(15, 10, LabelHue, String.Format("Tournament: {0}", Stats.TournamentName));
                AddLabel(15, 30, LabelHue, String.Format("Tournament Type: {0}", ArenaHelper.GetTourneyType(Stats.Type)));*/
                AddLabel(15, 10, LabelHue, String.Format("Torneio: {0}", Stats.TournamentName));
                AddLabel(15, 30, LabelHue, String.Format("Tipo do Torneio: {0}", ArenaHelper.GetTourneyType(Stats.Type)));
            }

            if (Tournament != null)
            {
                AddButton(760, 13, 4011, 4013, 1, GumpButtonType.Reply, 0);
                AddLabel(660, 13, LabelHue, "Infos do Torneio");
            }

            if (Stats != null && Stats.FightEntries.Count > 0)
            {
                int pages = Stats.GetLatestRound();
                int startY = 80;
                int view;

                if (Tournament != null)
                    view = pages; //Initially indicates current round so rounds are displayed in reverse order
                else
                    view = 1;

                List<TournamentFightEntry> entries = new List<TournamentFightEntry>(Stats.FightEntries);

                for (int i = 1; i <= pages; ++i)
                {
                    AddPage(i);
                    startY = 95;

                    for (int j = entries.Count - 1; j >= 0; j--)
                    {
                        if (Stats.FightEntries[j].Round == view)
                        {
                            TournamentFightEntry entry = Stats.FightEntries[j];

                            if (entry.Round != pages)
                                AddLabel(380, 60, LabelHue, String.Format("Round: {0}", view));
                            else if (Tournament == null)
                                //AddLabel(380, 60, LabelHue, "Championship Round");
                                AddLabel(380, 60, LabelHue, "Round do Campeonato");

                            AddLabel(20, startY + (entry.Fight * 20), 0, String.Format("{0:##}.", entry.Fight));
                            AddHtml(40, startY + (entry.Fight * 20), 258, 20, String.Format("<basefont color={0}>{1} <basefont color=black>Vs. <basefont color={2}>{3}", IsInTeam(entry.Fighter1) ? "green" : "black", entry.Fighter1, IsInTeam(entry.Fighter2) ? "green" : "black", entry.Fighter2), false, false);

                            AddHtml(550, startY + (entry.Fight * 20), 240, 20, entry.Winner != null ? String.Format("<basefont color={0}>{1}", IsInTeam(entry.Winner) ? "green" : "black", entry.Winner) : "", false, false);
                            //AddHtml(300, startY + (entry.Fight * 20), 245, 20, entry.Arena != null ? entry.Arena : "Unknown", false, false);
                            AddHtml(300, startY + (entry.Fight * 20), 245, 20, entry.Arena != null ? entry.Arena : "Desconhecido", false, false);
                        }
                    }

                    if (i != pages)
                        AddButton(760, length - 32, 4005, 4007, 0, GumpButtonType.Page, i + 1);
                    if (i > 1)
                        AddButton(725, length - 32, 4014, 4016, 0, GumpButtonType.Page, i - 1);

                    if (Tournament != null)
                        view--;
                    else
                        view++;
                }

                ColUtility.Free(entries);
            }
        }

        private bool IsInTeam(string teamName)
        {
            return ArenaTeam.Teams.Any(t => t.Name == teamName && t.IsInTeam(User));
        }

        public override void OnResponse(RelayInfo button)
        {
            switch (button.ButtonID)
            {
                default:
                case 0: break;
                case 1:
                    {
                        if (Tournament != null)
                            BaseGump.SendGump(new TournamentInfoGump(Tournament.System, Tournament, User));
                        break;
                    }
            }
        }
    }
}
