using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class RankingsGump : BaseGump
    {
        public LeagueRecord Record { get; set; }
        public League League { get; set; }
        public int ViewPage = 20000;

        public RankingsGump(PlayerMobile pm, LeagueRecord record, League league = null)
            : base(pm, 0, 0)
        {
            Record = record;
            League = league;
        }

        public override void AddGumpLayout()
        {
            AddBackground(0, 0, 700, 620, 3500);

            if (User.AccessLevel >= Config.LeagueRegistrationAccess && !Record.Complete)
            {
                AddButton(10, 593, 4017, 4019, 10000, GumpButtonType.Reply, 0);
                AddHtml(45, 593, 150, 20, "Cancel League", false, false);
            }
            else if (League != null)
            {
                AddHtml(0, 593, 395, 20, ColorAndCenter("#008000", League.StatusToString(User)), false, false);
            }

            switch (ViewPage)
            {
                case 20000:
                    AddImageTiled(395, 8, 1, 584, 96);
                    BuildLeagueRankings();

                    if (Record.RoundLineup != null && Record.RoundLineup.Count > 0)
                    {
                        BuildRounds();
                    }
                    break;
                default:
                    BuildRoundLineup();
                    AddButton(660, 593, 4005, 4007, 20000, GumpButtonType.Reply, 0);
                    AddHtml(456, 593, 200, 20, AlignRight("Back"), false, false);
                    break;
            }
        }

        private void BuildLeagueRankings()
        {
            AddHtml(0, 10, 395, 20, Center("Rankings"), false, false);

            AddLabel(30, 30, 0, "Team");
            AddLabel(270, 30, 0, "Wins");
            AddLabel(320, 30, 0, "Losses");
            AddLabel(370, 30, 0, "Ties");

            var list = Record.Teams;

            if (list != null)
            {
                var y = 60;

                for (int i = 0; i < list.Count; i++)
                {
                    var team = list[i];

                    AddLabel(10, y, 0, String.Format("{0}.", i + 1));
                    AddLabelCropped(30, y, 130, 20, 0, team.Name);
                    AddLabel(270, y, 0, Record.GetWins(team).ToString());
                    AddLabel(320, y, 0, Record.GetLosses(team).ToString());
                    AddLabel(370, y, 0, Record.GetTies(team).ToString());

                    y += 22;
                }
            }
        }

        private void BuildRounds()
        {
            var y = 40;

            AddLabel(420, 10, 0, "Rounds");

            for (int i = 0; i < Record.RoundLineup.Count; i++)
            {
                string hue;

                if (League != null && !League.EliminationPeriod && League.Round == i + 1)
                {
                    hue = "#00FFFF";
                    AddHtml(525, y, 150, 20, Color(hue, String.Format("Ends: {0}", League.RoundEnds.ToShortDateString())), false, false);
                }
                else
                {
                    hue = Record.RoundLineup[i + 1].All(m => !m.PendingFight) ? "#008000" : "#0d0d0d";
                }

                AddButton(420, y, 4005, 4007, i + 100, GumpButtonType.Reply, 0);
                AddHtml(455, y, 80, 20, Color(hue, String.Format("Round {0}", i + 1)), false, false);

                y += 22;
            }

            if (Record.EliminationRoundLineup.Count > 0)
            {
                y += 10;

                AddLabel(420, y, 0, "Elimination Rounds");

                y += 30;

                for (int i = 0; i < Record.EliminationRoundLineup.Count; i++)
                {
                    string hue;

                    if (League != null && League.EliminationPeriod && League.Round == i + 1)
                    {
                        hue = "#00FFFF";
                        AddHtml(525, y, 150, 20, Color("#00FFFF", String.Format("Ends: {0}", League.RoundEnds.ToShortDateString())), false, false);
                    }
                    else
                    {
                        hue = Record.EliminationRoundLineup[i + 1].All(m => !m.PendingFight) ? "#008000" : "#0d0d0d";
                    }

                    AddButton(420, y, 4005, 4007, i + 1000, GumpButtonType.Reply, 0);
                    AddHtml(455, y, 155, 20, Color(hue, League.EliminationRoundToString(Record.ElimSize, i + 1)), false, false);

                    y += 22;
                }

                AddButton(660, 593, 4005, 4007, 36000, GumpButtonType.Reply, 0);
                AddHtml(456, 593, 200, 20, AlignRight("Bracket Standings"), false, false);
            }
        }

        private void BuildRoundLineup()
        {
            AddLabel(10, 30, 0, "Match");
            AddLabel(400, 30, 0, "Results");

            if (ViewPage < 1000)
            {
                var round = (ViewPage - 100) + 1;

                AddHtml(0, 10, 700, 20, Center(String.Format("Round {0}", round)), false, false);

                if (Record.RoundLineup.ContainsKey(round))
                {
                    var y = 60;

                    foreach (var match in Record.RoundLineup[round])
                    {
                        AddLabelCropped(10, y, 390, 20, 0, String.Format("{0}[{1}] vs {2}[{3}]", match.Team1.Name, Record.GetPlacement(match.Team1).ToString(), match.Team2.Name, Record.GetPlacement(match.Team2).ToString()));

                        if (match.Winner != null)
                        {
                            AddLabelCropped(400, y, 300, 20, 0, String.Format("Winner: {0}", match.Winner.Name));
                        }
                        else if (match.Tied)
                        {
                            AddLabel(400, y, 0, "Tied");
                        }
                        else
                        {
                            AddLabel(400, y, 0, "Pending");
                        }

                        y += 22;
                    }
                }
            }
            else
            {
                var round = (ViewPage - 1000) + 1;

                AddHtml(0, 10, 700, 20, Center(String.Format("Elimination Round {0}", round)), false, false);

                if (Record.EliminationRoundLineup.ContainsKey(round))
                {
                    var y = 60;

                    foreach (var match in Record.EliminationRoundLineup[round])
                    {
                        AddLabelCropped(10, y, 390, 20, 0, String.Format("{0}[{1}] vs {2}[{3}]", match.Team1.Name, Record.GetPlacement(match.Team1).ToString(), match.Team2.Name, Record.GetPlacement(match.Team2).ToString()));

                        if (match.Winner != null)
                        {
                            AddLabelCropped(400, y, 300, 20, 0, String.Format("Winner: {0}", match.Winner.Name));
                        }
                        else if (match.Tied)
                        {
                            AddLabel(400, y, 0, "Tied");
                        }
                        else
                        {
                            AddLabel(400, y, 0, "Pending");
                        }

                        y += 22;
                    }
                }
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID != 0)
            {
                if (info.ButtonID == 10000)
                {
                    if (League.Leagues != null)
                    {
                        var league = League.Leagues.FirstOrDefault(l => l.Name == Record.LeagueName);

                        User.SendGump(
                            new GenericConfirmCallbackGump<League>(User,
                            "League Cancellation",
                            "WARNING: This league has either begun, or will begin very shortly. Are you sure you want to cancel?",
                            league,
                            null,
                            (m, l) => { l.CancelLeague(String.Format("The {0} league has been cancled by staff.", l.Name)); }));
                    }
                }
                else if (info.ButtonID == 36000)
                {
                    Refresh();

                    BaseGump.SendGump(new BracketGump(User, Record));
                }
                else
                {
                    ViewPage = info.ButtonID;
                    Refresh();
                }
            }
        }
    }
}
