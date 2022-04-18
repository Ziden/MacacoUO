using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using Server;
using Server.Mobiles;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class LeaguesGump : BaseGump
    {
        private const int Green = 0x3E;
        public List<League> List { get; set; }

        public LeaguesGump(PlayerMobile pm)
            : base(pm, 0, 0)
        {
            List = League.Leagues == null ? new List<League>() : League.Leagues.OrderBy(l => l.StartTime).ToList();
        }

        public override void AddGumpLayout()
        {
            int height = Math.Max(200, 200 + (List.Count * 25));
            bool staff = User.AccessLevel >= AccessLevel.GameMaster;

            AddBackground(0, 0, staff ? 850 : 750, height, 3500);

            AddHtml(0, 10, staff ? 850 : 750, 20, Center("Pending Leagues"), false, false);

            AddLabel(45, 40, 0, "League");
            AddLabel(200, 40, 0, "Start Date");
            AddLabel(300, 40, 0, "Teams");
            AddLabel(370, 40, 0, "Arena");
            AddLabel(550, 40, 0, "Status");

            if (staff)
            {
                AddLabel(750, 40, 0, "Props");

                if (Config.Debug)
                {
                    AddLabel(800, 40, 0, "Pick");
                }
            }

            var y = 60;

            for (int i = 0; i < List.Count; i++)
            {
                var league = List[i];
                var hue = league.IsParticipant(User) ? Green : 0x0;

                if (!league.PreLeague)
                {
                    AddButton(10, y, 4005, 4007, i + 100, GumpButtonType.Reply, 0);
                }
                else
                {
                    AddButton(10, y, 4005, 4007, i + 1000, GumpButtonType.Reply, 0);
                }

                AddLabelCropped(45, y, 150, 20, hue, league.Name);
                AddLabel(200, y, hue, league.StartTime.ToShortDateString());
                AddLabel(300, y, hue, String.Format("{0}/{1}", league.Teams.Count, league.MaxTeams.ToString()));
                AddLabelCropped(370, y, 180, 20, hue, league.ForcedArena == null || league.ForcedArena.System == null ? "Any" : league.ForcedArena.System.Name);
                AddLabel(550, y, hue, league.StatusToString(User));

                if (User.AccessLevel >= AccessLevel.GameMaster)
                {
                    AddButton(750, y, 4005, 4007, i + 50000, GumpButtonType.Reply, 0);

                    if (Config.Debug && !league.HasBegun && !league.PreLeague)
                    {
                        AddButton(800, y, 4005, 4007, i + 60000, GumpButtonType.Reply, 0);
                    }
                }

                y += 25;
            }

            if (League.LeagueRecords != null && League.LeagueRecords.Any(r => r.Complete))
            {
                AddButton(10, height - 30, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddHtml(45, height - 30, 150, 20, "View Completed Leagues", false, false);
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID == 1 && League.LeagueRecords != null)
            {
                BaseGump.SendGump(new LeagueRecordsGump(User));
            }
            else if (User.AccessLevel >= AccessLevel.GameMaster && info.ButtonID >= 60000)
            {
                var id = info.ButtonID - 60000;

                if (id >= 0 && id < List.Count)
                {
                    var league = List[id];
                    var list = new List<ArenaTeam>(ArenaTeam.Teams.Where(t => t.TeamType == league.TeamType && t.Active));

                    if (league.MinTeams > list.Count)
                    {
                        User.SendMessage("Not enough arena teams.");
                    }
                    else
                    {
                        for (int i = 0; i < list.Count && league.Teams.Count < league.MaxTeams; i++)
                        {
                            var team = list[i];
                            league.AddTeam(team);

                            foreach (var m in team.Fighters)
                            {
                                m.MoveToWorld(User.Location, User.Map);
                            }
                        }

                        User.SendMessage("League begins in 5 minutes!");
                        league.StartTime = DateTime.Now + TimeSpan.FromMinutes(5);
                    }
                }
            }
            else if (User.AccessLevel >= AccessLevel.GameMaster && info.ButtonID >= 50000)
            {
                var id = info.ButtonID - 50000;

                if (id >= 0 && id < List.Count)
                {
                    User.SendGump(new PropertiesGump(User, List[id]));
                }
            }
            else if (info.ButtonID >= 1000)
            {
                var id = info.ButtonID - 1000;

                if (id >= 0 && id < List.Count)
                {
                    var str = string.Empty;
                    var league = List[id];

                    BaseGump.SendGump(new RankingsGump(User, league.GetLeagueRecord(), league));
                }
            }
            else if (info.ButtonID >= 100)
            {
                var id = info.ButtonID - 100;

                if (id >= 0 && id < List.Count)
                {
                    BaseGump.SendGump(new LeagueGump(User, List[id]));
                }
            }
        }
    }

    public class LeagueGump : BaseGump
    {
        public League League { get; set; }
        public bool Registration { get; set; }

        public LeagueGump(PlayerMobile pm, League league, bool register = false)
            : base(pm, 100, 100)
        {
            League = league;
            Registration = register;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 400, 600, 3500);
            AddHtml(0, 10, 400, 20, Center(League.Name), false, false);

            AddPage(1);
            AddLabel(35, 40, 0, "Round Duration");
            AddImage(15, 43, 0x5689);
            AddTooltip(Localizations.GetLocalization(62));
            AddLabel(150, 40, 0, League.RoundDuration.ToString() + " Days");

            AddLabel(35, 65, 0, "Leage Start");
            AddImage(15, 68, 0x5689);
            AddTooltip(Localizations.GetLocalization(63));
            AddLabel(150, 65, 0, League.StartTime.ToShortDateString());

            AddLabel(35, 90, 0, "Entry Fee");
            AddImage(15, 93, 0x5689);
            AddTooltip(Localizations.GetLocalization(64));
            AddLabel(150, 90, 0, League.EntryFee.ToString("N0", CultureInfo.GetCultureInfo("en-US")));

            AddLabel(35, 115, 0, "League Size");
            AddImage(15, 118, 0x5689);
            AddTooltip(Localizations.GetLocalization(65));
            AddLabel(150, 115, 0, String.Format("{0} min - {1} max", League.MinTeams.ToString(), League.MaxTeams.ToString()));

            AddLabel(35, 140, 0, "Elim Format");
            AddImage(15, 143, 0x5689);
            AddTooltip(Localizations.GetLocalization(66), Localizations.GetLocalization(660 + (int)League.EliminationFormat));
            AddLabel(150, 140, 0, League.EliminationFormat.ToString());

            AddLabel(35, 165, 0, "Force Arena");
            AddImage(15, 168, 0x5689);
            AddTooltip(Localizations.GetLocalization(68));
            AddLabel(150, 165, 0, League.ForcedArena != null && League.ForcedArena.System != null ? League.ForcedArena.System.Name : "None");

            AddLabel(35, 190, 0, "Fight Type");
            AddImage(15, 193, 0x5689);
            AddTooltip(Localizations.GetLocalization(69));
            AddLabel(150, 190, 0, ArenaHelper.GetFightType(League.FightType));

            AddLabel(35, 215, 0, "Team Type");
            AddImage(15, 218, 0x5689);
            AddTooltip(Localizations.GetLocalization(70));
            AddLabel(150, 215, 0, League.TeamType.ToString());

            AddLabel(35, 240, 0, "Fight Duration");
            AddImage(15, 243, 0x5689);
            AddTooltip(Localizations.GetLocalization(72));
            AddLabel(150, 240, 0, League.FightDuration.ToString() + " minutes");

            AddLabel(35, 265, 0, League.AllowTies ? "Ties Allowed" : "No Ties");
            AddLabel(35, 290, 0, League.UseOwnGear ? "Use Own Gear" : "Use Arena Gear");

            AddHtml(0, 315, 400, 20, Center("Rules"), false, false);

            var index = 0;
            foreach (var rule in League.AvailableRules.Where(r => (League.Rules & r) != 0))
            {
                AddLabel(index % 2 == 0 ? 20 : 210, 340 + (25 * (index / 2)), 0, ArenaHelper.GetRules(rule));

                index++;
            }

            if (Registration)
            {
                AddButton(15, 570, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddLabel(50, 570, 0, "REGISTER");

                AddButton(355, 570, 4005, 4007, 2, GumpButtonType.Reply, 0);
                AddHtml(255, 570, 95, 20, AlignRight("Back"), false, false);
            }
            else
            {
                if (!League.PreLeague)
                {
                    if (League.IsParticipant(User))
                    {
                        var team = ArenaTeam.GetTeam(User, League.TeamType);

                        if (team != null && ArenaTeam.IsTeamLeader(User, team))
                        {
                            AddButton(15, 570, 4005, 4007, 3, GumpButtonType.Reply, 0);
                            AddLabel(50, 570, 0, "Quit League");
                        }
                    }
                    else
                    {
                        AddButton(15, 570, 4005, 4007, 1, GumpButtonType.Reply, 0);
                        AddLabel(50, 570, 0, "Join League");
                    }
                }

                if (User.AccessLevel >= Config.LeagueRegistrationAccess)
                {
                    AddButton(15, 545, 4005, 4007, 4, GumpButtonType.Reply, 0);
                    AddLabel(50, 545, 0, "Cancel League");
                }

                if (League.Teams != null && League.Teams.Count > 0)
                {
                    AddButton(355, 545, 4005, 4007, 0, GumpButtonType.Page, 2);
                    AddHtml(255, 545, 95, 20, AlignRight("Teams"), false, false);

                    AddPage(2);

                    var list = League.GetStandings();

                    for (int i = 0; i < list.Count; i++)
                    {
                        AddLabel(15, 40 + (i * 25), 0, String.Format("{0}.", i + 1));
                        AddLabel(35, 40 + (i * 25), 0, list[i].Name);
                    }

                    AddButton(355, 545, 4005, 4007, 0, GumpButtonType.Page, 1);
                    AddHtml(255, 545, 95, 20, AlignRight("League Details"), false, false);
                }

                AddButton(355, 570, 4005, 4007, 2, GumpButtonType.Reply, 0);
                AddHtml(255, 570, 95, 20, AlignRight("Back"), false, false);
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 1: // Join/Register
                    if (Registration)
                    {
                        League.AddLeague(League);
                        User.SendMessage("League added!");
                    }
                    else
                    {
                        var team = ArenaTeam.GetTeam(User, League.TeamType);

                        if (team == null)
                        {
                            User.SendMessage("You do not belong to that type of arena team.");
                        }
                        else if (!team.Active)
                        {
                            User.SendMessage("Your team must be active to join the leage.");
                        }
                        else if (!ArenaTeam.IsTeamLeader(User, team))
                        {
                            User.SendMessage("Only the leader of the arena team can join a league.");
                        }
                        else if (League.TryAddTeam(User, team))
                        {
                            foreach (var fighter in team.Fighters.Where(f => f.NetState != null))
                            {
                                fighter.SendMessage("Your {0} arena team has joined the '{1}' League!", team.TeamType.ToString(), League.Name);
                            }
                        }

                        Refresh();
                    }
                    break;
                case 2: // back
                    if (Registration)
                    {
                        BaseGump.SendGump(new CreateLeagueGump(User, League));
                    }
                    else
                    {
                        BaseGump.SendGump(new LeaguesGump(User));
                    }
                    break;
                case 3: // quit
                    if (!League.PreLeague)
                    {
                        League.RemoveTeam(ArenaTeam.GetTeam(User, League.TeamType));
                    }
                    else
                    {
                        User.SendMessage("You cannot remove your team once the league has begun!");
                    }
                    break;
                case 4:
                    User.SendGump(
                        new GenericConfirmCallbackGump<LeagueGump>(User,
                        "League Cancellation",
                        League.PreLeague || League.HasBegun ?
                        "WARNING: This league has either begun, or will begin very shortly. Are you sure you want to cancel?" :
                        "Are you sure you want to cancel this league?",
                        this,
                        null,
                        (m, g) => { g.League.CancelLeague(String.Format("The {0} league has been cancled by staff.", g.League.Name)); BaseGump.SendGump(g); },
                        (m, g) => { BaseGump.SendGump(g); }));

                    break;
            }
        }
    }

    public class LeagueRecordsGump : BaseGump
    {
        public LeagueRecordsGump(PlayerMobile pm)
            : base(pm, 50, 50)
        {
        }

        public override void AddGumpLayout()
        {
            AddPage(0);

            AddBackground(0, 0, 500, 360, 3500);
            AddHtml(0, 10, 500, 20, Center("League Stats"), false, false);

            AddLabel(45, 30, 0, "League");
            AddLabel(300, 30, 0, "Teams");
            AddLabel(370, 30, 0, "Status");

            bool staff = User.AccessLevel >= Config.LeagueRegistrationAccess;

            if (staff)
            {
                AddLabel(460, 30, 0, "Props");
            }

            if (League.LeagueRecords == null)
            {
                return;
            }

            var page = 1;
            AddPage(page);
            var y = 50;

            for (int i = 0; i < League.LeagueRecords.Count; i++)
            {
                var record = League.LeagueRecords[i];

                if (record != null && !record.Cancelled)
                {
                    AddButton(10, y, 4005, 4007, i + 100, GumpButtonType.Reply, 0);
                }

                AddLabelCropped(45, y, 250, 20, 0, record.LeagueName);
                AddLabel(300, y, 0, record.Teams == null ? "0" : record.Teams.Count.ToString());
                AddLabel(370, y, 0, record.Cancelled ? "Cancelled" : record.Complete ? "Complete" : "Ongoing");

                if (staff)
                {
                    AddButton(460, y, 4005, 4007, i + 1000, GumpButtonType.Reply, 0);
                }

                if (i > 0 && i % 10 == 0 && i < League.LeagueRecords.Count - 1)
                {
                    AddButton(252, 331, 4005, 4007, 0, GumpButtonType.Page, page + 1);
                    AddPage(++page);
                    AddButton(218, 331, 4014, 4016, 0, GumpButtonType.Page, page - 1);

                    y = 50;
                }
                else
                {
                    y += 25;
                }
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID >= 1000)
            {
                var id = info.ButtonID - 1000;

                if (id >= 0 && id < League.LeagueRecords.Count)
                {
                    User.SendGump(new PropertiesGump(User, League.LeagueRecords[id]));
                }
            }
            else if (info.ButtonID >= 100)
            {
                var id = info.ButtonID - 100;

                if (id >= 0 && id < League.LeagueRecords.Count)
                {
                    BaseGump.SendGump(new RankingsGump(User, League.LeagueRecords[id]));
                }
            }
        }
    }
}
