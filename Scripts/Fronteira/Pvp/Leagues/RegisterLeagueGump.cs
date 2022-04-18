using System;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class CreateLeagueGump : BaseGump
    {
        public League League { get; set; }
        public FightRules CurrentRule { get; set; }
        public int CurrentPage { get; set; }

        public CreateLeagueGump(PlayerMobile pm, League league = null)
            : base(pm, 0, 0)
        {
            League = league ?? new League();
            CurrentRule = League.AvailableRules[0];
        }

        public override void AddGumpLayout()
        {
            AddBackground(0, 0, 420, 500, 3500);
            AddHtml(0, 10, 400, 20, Center("League Creation Menu"), false, false);

            AddButton(15, 468, 4005, 4007, 10000, GumpButtonType.Reply, 0);
            AddLabel(48, 468, 0, "Register League");

            AddPage(CurrentPage == 2 ? 2 : 1);
            AddHtml(15, 40, 150, 20, "League Specs", false, false);
            AddButton(115, 40, 4006, 4007, 51000, GumpButtonType.Reply, 0);

            AddLabel(15, 70, 0, "League Name");
            AddTooltip(Localizations.GetLocalization(60));
            AddBackground(150, 70, 230, 20, 3000);
            AddTextEntry(151, 71, 229, 20, 0, 1, League.Name ?? string.Empty);

            AddLabel(15, 95, 0, "Round Duration");
            AddTooltip(Localizations.GetLocalization(62));
            AddBackground(150, 95, 51, 20, 3000);
            AddTextEntry(151, 96, 50, 20, 0, 2, League.RoundDuration.ToString());
            AddLabel(205, 95, 0, "Days");

            AddLabel(15, 120, 0, "Leage Start");
            AddTooltip(Localizations.GetLocalization(63));
            AddBackground(150, 120, 100, 20, 3000);
            AddTextEntry(151, 120, 99, 20, 0, 3, League.StartTime.ToShortDateString());
            AddLabel(256, 120, 0, "mm/dd/yyyy");

            AddLabel(15, 145, 0, "Entry Fee");
            AddTooltip(Localizations.GetLocalization(64));
            AddBackground(150, 145, 100, 20, 3000);
            AddTextEntry(151, 145, 99, 20, 0, 4, League.EntryFee.ToString());

            AddLabel(15, 170, 0, "League Size Min");
            AddBackground(150, 170, 50, 20, 3000);
            AddTextEntry(151, 170, 49, 20, 0, 6, League.MinTeams.ToString());

            AddLabel(256, 170, 0, "Size Max");
            AddBackground(340, 170, 50, 20, 3000);
            AddTextEntry(341, 170, 49, 20, 0, 7, League.MaxTeams.ToString());

            AddButton(115, 195, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(66), Localizations.GetLocalization(660 + (int)League.EliminationFormat));
            AddLabel(15, 195, 0, "Elim Format");
            AddLabel(151, 195, 0, League.EliminationFormat.ToString());

            /*AddButton(115, 220, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(67), Localizations.GetLocalization(670 + (int)League.EliminationType));
            AddLabel(15, 220, 0, "Elim Type");
            AddLabel(151, 220, 0, League.EliminationType.ToString());*/

            AddButton(115, 220, 4005, 4007, 4, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(68));
            AddLabel(15, 220, 0, "Force Arena");
            AddLabel(151, 220, 0, League.ForcedArena != null && League.ForcedArena.System != null ? League.ForcedArena.System.Name : "None");

            AddPage(CurrentPage == 2 ? 1 : 2);
            AddHtml(15, 40, 150, 20, "Fight Specs", false, false);
            AddButton(115, 40, 4006, 4007, 52000, GumpButtonType.Reply, 0);

            AddButton(115, 70, 4005, 4007, 5, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(69));
            AddLabel(15, 70, 0, "Fight Type");
            AddLabel(151, 70, 0, ArenaHelper.GetFightType(League.FightType));

            AddButton(115, 95, 4005, 4007, 6, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(70));
            AddLabel(15, 95, 0, "Team Type");
            AddLabel(151, 95, 0, League.TeamType.ToString());

            AddLabel(15, 120, 0, "Allow Ties");
            AddCheck(115, 120, 0xD2, 0xD3, League.AllowTies, 1);

            AddLabel(15, 145, 0, "Use Own Gear");
            AddCheck(115, 145, 0xD2, 0xD3, League.UseOwnGear, 2);

            AddLabel(15, 170, 0, "Fight Duration");
            AddTooltip(Localizations.GetLocalization(20));
            AddBackground(150, 170, 100, 20, 3000);
            AddTextEntry(151, 170, 99, 20, 0, 5, League.FightDuration.ToString());
            AddLabel(256, 170, 0, "minutes");

            bool hasRule = (League.Rules & CurrentRule) != 0;

            AddButton(115, 195, 4005, 4007, 7, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(71));
            AddLabel(15, 195, 0, "Fight Rules");
            AddLabel(155, 195, 0, ArenaHelper.GetRules(CurrentRule));
            AddButton(358, 195, hasRule ? 4017 : 4023, hasRule ? 4019 : 4025, 8, GumpButtonType.Reply, 0);
            AddTooltip(hasRule ? "Remove Rule" : "Add Rule");

            AddLabel(15, 220, 0, "Rules:");

            var index = 0;
            foreach (var rule in League.AvailableRules.Where(r => (League.Rules & r) != 0))
            {
                AddLabel(index % 2 == 0 ? 20 : 210, 245 + (25 * (index / 2)), 0, ArenaHelper.GetRules(rule));

                index++;
            }
        }

        private bool ValidateLeague(ref string message)
        {
            if (string.IsNullOrEmpty(League.Name))
            {
                message = "Invalid League Name";
            }
            else if ((League.Leagues != null && League.Leagues.Any(l => l.Name == League.Name)) || (League.LeagueRecords != null && League.LeagueRecords.Any(rec => rec.LeagueName == League.Name)))
            {
                message = "League name must be unique to any current or previous recorded leagues.";
            }
            else if (League.ForcedArena != null)
            {
                var arena = League.ForcedArena.System;

                if (arena == null)
                {
                    message = "That arena does not exist.";
                }
                else if (!arena.FightTypes.Any(type => type == League.FightType))
                {
                    message = String.Format("This arena does not support that type of fight.");
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID == 0)
            {
                return;
            }

            League.AllowTies = info.IsSwitched(1);
            League.UseOwnGear = info.IsSwitched(2);

            var nameRaw = info.GetTextEntry(1).Text;
            var durationRaw = info.GetTextEntry(2).Text;
            var startRaw = info.GetTextEntry(3).Text;
            var entryRaw = info.GetTextEntry(4).Text;
            var fightDurationRaw = info.GetTextEntry(5).Text;
            var leagueMinRaw = info.GetTextEntry(6).Text;
            var leagueMaxRaw = info.GetTextEntry(7).Text;
            string errorMessage = null;

            if (!String.IsNullOrEmpty(nameRaw))
            {
                League.Name = nameRaw;
            }
            
            if (!String.IsNullOrEmpty(durationRaw))
            {
                var duration = Utility.ToInt32(durationRaw);

                if (duration >= 1)
                {
                    League.RoundDuration = duration;
                }
                else
                {
                    errorMessage = "Invalid round duration. It must be set to at least 1 (day).";
                }
            }

            if (!String.IsNullOrEmpty(startRaw))
            {
                DateTime dt;

                DateTime.TryParse(startRaw, out dt);

                if (dt > DateTime.Now + TimeSpan.FromDays(Config.LeagueMinNotify))
                {
                    League.StartTime = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
                }
                else
                {
                    errorMessage = String.Format("Invalid League start date. You must allow {0} days for player notifaction.", Config.LeagueMinNotify);
                }
            }

            if (!String.IsNullOrEmpty(entryRaw))
            {
                var fee = Utility.ToInt32(entryRaw);

                if (fee >= 0)
                {
                    League.EntryFee = fee;
                }
                else
                {
                    errorMessage = "Invalid entry fee. Must be 0 or greater.";
                }
            }

            if (!String.IsNullOrEmpty(fightDurationRaw))
            {
                var duration = Utility.ToInt32(fightDurationRaw);

                if (duration > 0 && duration < 30)
                {
                    League.FightDuration = duration;
                }
                else
                {
                    errorMessage = "Invalid Fight Duration. Must be from 1 minute to 30 minutes.";
                }
            }

            if (!String.IsNullOrEmpty(leagueMinRaw))
            {
                var min = Utility.ToInt32(leagueMinRaw);

                if (min < Config.MinLeagueTeams)
                {
                    min = Config.MinLeagueTeams;
                }
                else if (min > League.MaxTeams)
                {
                    min = League.MaxTeams;
                }

                if (min % 2 != 0)
                {
                    min--;
                }

                League.MinTeams = min;

            }

            if (!String.IsNullOrEmpty(leagueMinRaw))
            {
                var max = Utility.ToInt32(leagueMaxRaw);

                if (max < League.MinTeams)
                {
                    max = League.MinTeams;
                }
                else if (max > Config.MaxLeagueTeams)
                {
                    max = Config.MaxLeagueTeams;
                }

                if (max % 2 != 0)
                {
                    max--;
                }

                League.MaxTeams = max;

            }

            switch (info.ButtonID)
            {
                case 10000: // Register
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        User.SendMessage(0x23, errorMessage);
                    }
                    else
                    {
                        ValidateLeague(ref errorMessage);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            User.SendMessage(0x23, errorMessage);
                        }
                        else
                        {
                            BaseGump.SendGump(new LeagueGump(User, League, true));
                            return;
                        }
                    }
                    Refresh();
                    break;
                case 1: // League Size
                    /*switch (League.Size)
                    {
                        default: League.Size++;
                            break;
                        case LeagueSize.Sixteen: League.Size = LeagueSize.Any;
                            break;
                    }
                    Refresh();*/
                    break;
                case 2: // Elim Format
                    switch (League.EliminationFormat)
                    {
                        default:
                            League.EliminationFormat++;
                            break;
                        case EliminationFormat.StrongVsStrong:
                            League.EliminationFormat = EliminationFormat.None;
                            break;
                    }
                    Refresh();
                    break;
                case 3: // Elim Type
                    /*switch (League.EliminationType)
                    {
                        default:
                            League.EliminationType++;
                            break;
                        case EliminationType.Half:
                            League.EliminationType = EliminationType.All;
                            break;
                    }
                    Refresh();*/
                    break;
                case 4: // Force Arena (this will need to be edited if more non-custom arenas are added
                    var arena = League.ForcedArena != null ? League.ForcedArena.System : null;
                    bool error = false;

                    if (arena != null)
                    {
                        var type = arena.SystemType;

                        switch (type)
                        {
                            // We need to exclude custom arena's
                            case SystemType.Tokuno:
                                arena = null;
                                break;
                            default:
                                arena = GetNext(type, ref error);
                                break;
                        }
                    }
                    else
                    {
                        arena = GetNext(null, ref error);
                    }

                    if (arena == null && error)
                    {
                        User.SendMessage(0x22, "Unfortunately, there are no valid arena's at this time. They may not be set up yet, or are inactive.");
                    }
                    else
                    {
                        League.ForcedArena = arena != null ? arena.Stone : null;
                    }

                    Refresh();
                    break;
                case 5: // Fight Type
                    switch (League.FightType)
                    {
                        case ArenaFightType.SingleElimination:
                            League.FightType = ArenaFightType.BestOf3;
                            break;
                        case ArenaFightType.BestOf3:
                            League.FightType = ArenaFightType.CaptureTheFlag;
                            break;
                        case ArenaFightType.LastManStanding:
                            League.FightType = ArenaFightType.CaptureTheFlag;
                            break;
                        case ArenaFightType.CaptureTheFlag:
                            League.FightType = ArenaFightType.SingleElimination;
                            break;
                    }
                    Refresh();
                    break;
                case 6: // Team Type
                    switch (League.TeamType)
                    {
                        default:
                            League.TeamType = ArenaTeamType.Single;
                            break;
                        case ArenaTeamType.Single:
                            League.TeamType = ArenaTeamType.Twosome;
                            break;
                        case ArenaTeamType.Twosome:
                            League.TeamType = ArenaTeamType.Foursome;
                            break;

                    }
                    Refresh();
                    break;
                case 7: // Cycle Rules
                    var index = Array.IndexOf(League.AvailableRules, CurrentRule);

                    if (index >= League.AvailableRules.Length - 1)
                    {
                        index = 0;
                    }
                    else
                    {
                        index++;
                    }

                    CurrentRule = League.AvailableRules[index];
                    Refresh();

                    break;
                case 8: // Add/Remove Rule
                    if ((League.Rules & CurrentRule) == 0)
                    {
                        League.Rules |= CurrentRule;

                        if (CurrentRule == FightRules.PureDexxer && (League.Rules & FightRules.PureMage) != 0)
                        {
                            League.Rules ^= FightRules.PureMage;
                        }
                        else if (CurrentRule == FightRules.PureMage && (League.Rules & FightRules.PureDexxer) != 0)
                        {
                            League.Rules ^= FightRules.PureDexxer;
                        }
                    }
                    else
                    {
                        League.Rules ^= CurrentRule;
                    }

                    Refresh();
                    break;
                case 51000: // Goto Fight Specs
                    CurrentPage = 2;
                    Refresh();
                    break;
                case 52000: // Goto League Specs
                    CurrentPage = 1;
                    Refresh();
                    break;
            }
        }

        private PVPTournamentSystem GetNext(SystemType? type, ref bool error)
        {
            var validCount = PVPTournamentSystem.SystemList.Where(sys => sys.Active && sys.Stone != null && sys.SystemType != type).Count();

            if (validCount == 0 && type == null)
            {
                error = true;
                return null;
            }

            PVPTournamentSystem temp = null;
            PVPTournamentSystem next = null;

            do
            {
                if (type == null || type == SystemType.Custom)
                {
                    temp = ArenaHelper.GetSystem((SystemType)0);
                }
                else
                {
                    temp = ArenaHelper.GetSystem(++type);
                }

                if (temp != null && temp.Active && temp.Stone != null)
                {
                    next = temp;
                }
            }
            while (next == null);

            return next;
        }
    }
}
