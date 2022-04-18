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
    public class PlayerRecordGump : BaseGump
    {
        public ArenaTeam Viewing { get; set; }
        public ArenaTeam[] Teams { get; set; }

        public bool ViewingOwn { get { return Viewing == null || Viewing.IsInTeam(User); } }

        public PlayerRecordGump(PlayerMobile pm)
            : this(pm, null)
        {
        }

        public PlayerRecordGump(PlayerMobile pm, ArenaTeam viewing)
            : base(pm, 0, 0)
        {
            Viewing = viewing;
        }

        public override void AddGumpLayout()
        {
            int count = 0;

            if (Viewing != null)
            {
                count = 1;
                Teams = new ArenaTeam[] { Viewing };
            }
            else
            {
                var single = ArenaTeam.GetTeam(User, ArenaTeamType.Single);
                var twosome = ArenaTeam.GetTeam(User, ArenaTeamType.Twosome);
                var foursome = ArenaTeam.GetTeam(User, ArenaTeamType.Foursome);

                if (single != null)
                {
                    count++;
                }

                if (twosome != null)
                {
                    count++;
                }

                if (foursome != null)
                {
                    count++;
                }

                Teams = new ArenaTeam[count];

                for (int i = 0; i < Teams.Length; i++)
                {
                    if (single != null)
                    {
                        Teams[i] = single;
                        single = null;
                    }
                    else if (twosome != null)
                    {
                        Teams[i] = twosome;
                        twosome = null;
                    }
                    else if (foursome != null)
                    {
                        Teams[i] = foursome;
                        foursome = null;
                    }
                }
            }

            int h = 65 + (280 * count);

            AddBackground(0, 0, 900, h, 3500);

            AddImageTiled(300, 8, 1, h - 16, 96);
            AddImageTiled(600, 8, 1, h - 16, 96);
            AddHtml(10, 15, 900, 20, Center(ViewingOwn ? "My Team Stats" : String.Format("{0} Team Stats", Viewing != null ? Viewing.Name : "Unknown")), false, false);

            AddImageTiled(8, 8, 292, 30, 0x28DE);
            AddImageTiled(600, 8, 292, 30, 0x28DE);

            AddImageTiled(8, 40, 884, 1, 96);
            AddImageTiled(8, 60, 884, 1, 96);
            AddHtml(0, 40, 300, 20, Center("Overall"), false, false);
            AddHtml(300, 40, 300, 20, Center("Tournament"), false, false);
            AddHtml(600, 40, 300, 20, Center("League"), false, false);

            var y = 65;

            for (int i = 0; i < Teams.Length; i++)
            {
                var team = Teams[i];

                AddLabel(10, y, !team.Active ? ArenaTeam.InactiveHue : ArenaHelper.GetTeamTypeColor(team.TeamType), String.Format("Team: {0} {1}", team.Name, !team.Active ? "[Inactive]" : team.TeamType.ToString()));

                if ((int)team.TeamType > 1)
                {
                    AddLabel(10, y + 20, !team.Active ? ArenaTeam.InactiveHue : ArenaHelper.GetTeamTypeColor(team.TeamType), String.Format("Leader: {0}", team.TeamLeader != null ? team.TeamLeader.Name : "None"));
                }

                AddLabel(10, y + 40, 0, "Duels:");
                AddLabel(10, y + 60, 0, "Wins:");
                AddLabel(10, y + 80, 0, "Draws:");
                AddLabel(10, y + 100, 0, "Losses:");
                AddLabel(10, y + 120, 0, "Damage Ratio:");
                AddLabel(10, y + 140, 0, "Points:");

                AddLabel(10, y + 160, 0, "Single Elim Wins:");
                AddLabel(10, y + 180, 0, "Best of 3 Wins:");
                AddLabel(10, y + 200, 0, "Last Man Standing:");
                AddLabel(10, y + 220, 0, "Capture the Flag:");
                AddLabel(10, y + 240, 0, "Team Rumble CTF:");

                AddLabel(200, y + 40, 0, (team.Wins + team.Losses + team.Draws).ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(200, y + 60, 0, team.Wins.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(200, y + 80, 0, team.Draws.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(200, y + 100, 0, team.Losses.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(200, y + 120, 0, PlayerStatsGump.GetDamageRatio(team));
                AddLabel(200, y + 140, 0, ((int)team.Points).ToString("N0", CultureInfo.GetCultureInfo("en-US")));

                AddLabel(200, y + 160, 0, team.SingleElimWins.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(200, y + 180, 0, team.BestOf3Wins.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(200, y + 200, 0, team.LastManStandingWins.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(200, y + 220, 0, team.CTFWins.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(200, y + 240, 0, team.CTFTeamRumbleWins.ToString("N0", CultureInfo.GetCultureInfo("en-US")));

                AddLabel(310, y, 0, "Duels:");
                AddLabel(310, y + 20, 0, "Wins:");
                AddLabel(310, y + 40, 0, "Losses:");
                AddLabel(310, y + 60, 0, "Championships:");

                AddLabel(500, y, 0, (team.TournamentWins + team.TournamentLosses).ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(500, y + 20, 0, team.TournamentWins.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(500, y + 40, 0, team.TournamentLosses.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                AddLabel(500, y + 60, 0, team.TournamentChampionships.ToString("N0", CultureInfo.GetCultureInfo("en-US")));

                var yOffset = y + 80;
                bool leader = team.TeamLeader == User;
                AddImageTiled(300, yOffset, 300, 1, 96);

                if (ViewingOwn)
                {
                    AddHtml(300, yOffset, 300, 20, Center("Team Options"), false, false);
                    yOffset += 20;

                    AddButton(310, yOffset, 4002, 4004, i + 100, GumpButtonType.Reply, 0);
                    AddTooltip(Localizations.GetLocalization(79));
                    AddLabel(342, yOffset, 0, "Resign");
                    yOffset += 22;

                    if (leader && team.CanRename(User))
                    {
                        AddButton(310, yOffset, 4005, 4007, i + 5000, GumpButtonType.Reply, 0);
                        AddTooltip(Localizations.GetLocalization(82));
                        AddLabel(342, yOffset, 0, "Rename");
                        yOffset += 25;
                    }
                }

                if (team.TeamType != ArenaTeamType.Single)
                {
                    AddLabel(310, yOffset, 0, "Fighters:");
                    yOffset += 25;

                    var fighters = new Mobile[(int)team.TeamType];

                    for (int j = 0; j < team.Fighters.Count; j++)
                    {
                        fighters[j] = team.Fighters[j];
                    }

                    for (int j = 0; j < fighters.Length; j++)
                    {
                        var fighter = fighters[j];

                        AddLabel(leader ? 342 : 312, yOffset + (j * 22), 0, fighter == null ? String.Format("Open {0}", leader ? "[Click to Add Fighter]" : string.Empty) : fighter.Name);

                        if (leader && team.TeamLeader != fighter)
                        {
                            if (fighter != null)
                            {
                                AddButton(310, yOffset + (j * 22), 4020, 4022, 25000 + ((i * 10) + j), GumpButtonType.Reply, 0);
                                AddTooltip(Localizations.GetLocalization(80));
                            }
                            else
                            {
                                AddButton(310, yOffset + (j * 22), 4023, 4025, i + 1000, GumpButtonType.Reply, 0); //Add Button
                                AddTooltip(Localizations.GetLocalization(81));
                            }
                        }
                    }
                }

                var record = team.LeagueRecord;

                if (record != null)
                {
                    AddLabel(610, y, 0, "Matches:");
                    AddLabel(610, y + 20, 0, "Wins:");
                    AddLabel(610, y + 40, 0, "Draws:");
                    AddLabel(610, y + 60, 0, "Losses:");
                    AddLabel(610, y + 80, 0, "Elimination Wins:");
                    AddLabel(610, y + 100, 0, "Elimination Losses:");
                    AddLabel(610, y + 120, 0, "First Placement:");
                    AddLabel(610, y + 140, 0, "Second Placement:");

                    var first = League.LeagueRecords == null ? 0 : League.LeagueRecords.Where(r => r.Complete && r.Champion == team).Count();
                    var seconds = League.LeagueRecords == null ? 0 : League.LeagueRecords.Where(r => r.Complete && r.RunnerUp == team).Count();

                    AddLabel(800, y, 0, record.LeagueMatches.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                    AddLabel(800, y + 20, 0, record.Wins.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                    AddLabel(800, y + 40, 0, record.Ties.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                    AddLabel(800, y + 60, 0, record.Losses.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                    AddLabel(800, y + 80, 0, record.EliminationWins.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                    AddLabel(800, y + 100, 0, record.EliminationLosses.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                    AddLabel(800, y + 120, 0, first.ToString("N0", CultureInfo.GetCultureInfo("en-US")));
                    AddLabel(800, y + 140, 0, seconds.ToString("N0", CultureInfo.GetCultureInfo("en-US")));

                    AddButton(610, y + 170, 4006, 4005, i + 30000, GumpButtonType.Reply, 0);
                    AddLabel(642, y + 170, 0, "View League Records"); 
                }

                if (i < Teams.Length - 1)
                {
                    AddImageTiled(8, y + 250, 884, 1, 96);
                    y += 260;
                }
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID == 0)
            {
                return;
            }
            if (info.ButtonID >= 30000)
            {
                int id = info.ButtonID - 30000;

                if (id >= 0 && id < Teams.Length)
                {
                    BaseGump.SendGump(new LeagueRecordGump(User, Teams[id]));
                }
            }
            else if (info.ButtonID >= 25000)
            {
                int id = info.ButtonID - 25000;

                if (id < 10 && Teams.Length > 0)
                {
                    int kickidx = id;
                    var team = Teams[0];

                    if (team.TeamLeader == User && kickidx >= 0 && kickidx < team.Fighters.Count)
                    {
                        BaseGump.SendGump(new ConfirmKickGump(User, Teams[0], team.Fighters[kickidx], false));
                    }
                }
                else if (id < 20 && Teams.Length > 1)
                {
                    int kickidx = id - 10;
                    var team = Teams[1];

                    if (team.TeamLeader == User && kickidx >= 0 && kickidx < team.Fighters.Count)
                    {
                        BaseGump.SendGump(new ConfirmKickGump(User, team, team.Fighters[kickidx], false));
                    }
                }
                else if (id < 30 && Teams.Length > 2)
                {
                    int kickidx = id - 20;
                    var team = Teams[2];

                    if (team.TeamLeader == User && kickidx >= 0 && kickidx < team.Fighters.Count)
                    {
                        BaseGump.SendGump(new ConfirmKickGump(User, team, team.Fighters[kickidx], false));
                    }
                }
            }
            else if (info.ButtonID >= 5000)
            {
                int id = info.ButtonID - 5000;

                if (id >= 0 && id < Teams.Length)
                {
                    var team = Teams[id];

                    if (team != null && team.CanRename(User))
                    {
                        User.SendMessage("What would you like to rename this team to?");
                        User.Prompt = new InternalPrompt(team);
                    }
                }
            }
            else if (info.ButtonID >= 1000)
            {
                int id = info.ButtonID - 1000;

                if (id >= 0 && id < Teams.Length)
                {
                    var team = Teams[id];

                    if (team.Fighters.Count != (int)team.TeamType)
                    {
                        User.Target = new InternalTarget(User, team);
                        User.SendMessage("Target the player you'd like to have in your arena team.");
                    }
                    else
                        Refresh();
                }
            }
            else if (info.ButtonID >= 100)
            {
                int id = info.ButtonID - 100;

                if (id >= 0 && id < Teams.Length)
                {
                    BaseGump.SendGump(new ConfirmKickGump(User, Teams[id], User, true));
                }
            }
        }

        private class InternalPrompt : Prompt
        {
            private ArenaTeam m_Team;

            public InternalPrompt(ArenaTeam team)
            {
                m_Team = team;
            }

            public override void OnResponse(Mobile from, string text)
            {
                if (from is PlayerMobile && m_Team != null && text != null && text.Length > 0)
                {
                    m_Team.TryRename(from, text.Trim());
                    BaseGump.SendGump(new PlayerRecordGump((PlayerMobile)from));
                }
            }
        }

        public class InternalTarget : Target
        {
            private Mobile m_From;
            private ArenaTeam m_Team;

            public InternalTarget(Mobile from, ArenaTeam team)
                : base(12, false, TargetFlags.None)
            {
                m_From = from;
                m_Team = team;
            }

            protected override void OnTarget(Mobile from, object target)
            {
                if (target is Mobile)
                {
                    Mobile mob = (Mobile)target;

                    if (mob is PlayerMobile && mob.Alive)
                    {
                        if (ArenaTeam.HasTeam(mob, m_Team.TeamType))
                            m_From.SendMessage("They already belong to a team with that many fighters.");
                        else if (mob == m_From)
                            m_From.SendMessage("You are already on the team!");
                        else if (m_Team.IsInTeam(mob))
                            m_From.SendMessage("They are already on the team!");
                        else if (mob.HasGump(typeof(AddFighterGump)))
                            m_From.SendMessage("They are already deciding on joining an arena team. Try again later.");
                        else if (from is PlayerMobile)
                        {
                            BaseGump.SendGump(new AddFighterGump(m_From as PlayerMobile, mob as PlayerMobile, m_Team, true, null));
                            m_From.SendMessage("Please wait while they contemplate joining your team.");
                        }
                    }
                    else
                        m_From.SendMessage("You cannot add them as a member of the team.");
                }
            }

            protected override void OnTargetCancel(Mobile from, TargetCancelType type)
            {
                if (m_From is PlayerMobile)
                    BaseGump.SendGump(new PlayerRecordGump((PlayerMobile)m_From));
            }
        }
    }

    public class LeagueRecordGump : BaseGump
    {
        public ArenaTeam Team { get; set; }

        public LeagueRecordGump(PlayerMobile user, ArenaTeam team)
            : base(user, 100, 100)
        {
            Team = team;
        }

        public override void AddGumpLayout()
        {
            AddBackground(0, 0, 600, 420, 3500);

            var record = Team.LeagueRecord;

            if (record == null)
            {
                return;
            }

            List<string> teams = record.Keys.ToList();
            teams.Sort();

            AddHtml(0, 15, 600, 20, Center(String.Format("{0} Records Vs...", Team.IsInTeam(User) ? "Your" : TeamName())), false, false);

            AddButton(8, 390, 4014, 2016, 1, GumpButtonType.Reply, 0);
            AddLabel(40, 390, 0, "Back");

            AddLabelCropped(10, 40, 190, 20, 0, "Opponent");
            AddLabel(200, 40, 0, "Wins");
            AddLabel(275, 40, 0, "Losses");
            AddLabel(350, 40, 0, "Ties");
            AddLabel(425, 40, 0, "Elim Wins");
            AddLabel(500, 40, 0, "Elim Losses");

            var y = 60;
            var page = 1;
            AddPage(page);

            for (int i = 0; i < teams.Count; i++)
            {
                var data = record[teams[i]];

                AddLabel(10, y, 0, teams[i]);
                AddLabel(200, y, 0, data[0].ToString());
                AddLabel(275, y, 0, data[1].ToString());
                AddLabel(350, y, 0, data[2].ToString());
                AddLabel(425, y, 0, data[3].ToString());
                AddLabel(500, y, 0, data[4].ToString());

                AddHtml(296, 390, 8, 20, Center(page.ToString()), false, false);

                if (i > 0 && i % 13 == 0 && i < teams.Count - 1)
                {
                    y = 60;

                    AddButton(304, 390, 4005, 4007, 0, GumpButtonType.Page, page + 1);
                    AddPage(++page);
                    AddHtml(296, 390, 8, 20, Center(page.ToString()), false, false);
                    AddButton(266, 390, 4014, 4016, 0, GumpButtonType.Page, page - 1);
                }
                else
                {
                    y += 22;
                }
            }
        }

        private string TeamName()
        {
            if (Team.IsInTeam(User))
            {
                return "Your";
            }

            var name = Team.Name;

            if (name.ToLower().EndsWith("s") || name.ToLower().EndsWith("z"))
            {
                return name + "'";
            }

            return name + "'s";
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID == 1)
            {
                BaseGump.SendGump(new PlayerRecordGump(User, Team));
            }
        }
    }
}

