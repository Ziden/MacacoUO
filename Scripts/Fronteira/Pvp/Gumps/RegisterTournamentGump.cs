using System;
using System.Collections.Generic;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Misc;
using Server.Targeting;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class RegisterTournamentGump : BaseTournamentGump
    {
        private PVPTournamentSystem System { get; set; }
        private Tournament Tournament { get; set; }

        public RegisterTournamentGump(PVPTournamentSystem system, Tournament tourney, PlayerMobile from)
            : base(from, 325, 70)
        {
            System = system;
            Tournament = tourney;

            tourney.AddRules(FightRules.NoTies);
        }

        public override void AddGumpLayout()
        {
            AddPage(0);

            AddBackground(270, 99, 200, 320, DarkBackground);
            AddBackground(300, 109, 162, 300, LightBackground);

            AddBackground(0, 0, 300, 518, DarkBackground);
            AddBackground(10, 10, 280, 150, LightBackground);

            AddImageTiled(125, 75, 50, 20, DarkEntry);
            AddImageTiled(125, 98, 50, 20, DarkEntry);

            AddBackground(10, 170, 280, 140, LightBackground);
            AddBackground(10, 320, 280, 150, LightBackground);

            AddHtml(0, 10, 300, 16, ColorAndCenter(LabelColor, "Tournament Registration"), false, false);

            AddLabel(15, 29, 0, "Name:");
            AddTooltip(Localizations.GetLocalization(11));
            AddImageTiled(80, 29, 200, 20, DarkEntry);
            AddTextEntry(82, 29, 198, 20, 1150, 1, Tournament.Name != null ? Tournament.Name : Name != null ? Name : "");

            AddLabel(15, 52, 0, "Entry Fee:");
            AddTooltip(Localizations.GetLocalization(12));
            AddImageTiled(80, 52, 200, 20, DarkEntry);
            AddTextEntry(82, 52, 198, 20, 1150, 2, Tournament.EntryFee.ToString());

            AddLabel(15, 75, 0, "Max Participants:");
            AddTextEntry(127, 75, 44, 20, 1150, 3, Tournament.MaxParticipants.ToString());

            AddLabel(15, 98, 0, "Min Participants:");
            AddTextEntry(127, 98, 44, 20, 1150, 4, Tournament.MinParticipants.ToString());

            AddLabel(15, 121, 0, "Time:");
            AddImageTiled(60, 121, 100, 20, DarkEntry);
            AddTooltip(Localizations.GetLocalization(14));
            AddLabel(63, 121, 1150, GetMonth());
            AddButton(145, 121, 0xFD, 0xFD, 1, GumpButtonType.Reply, 0);

            AddImageTiled(170, 121, 40, 20, DarkEntry);
            AddTooltip(Localizations.GetLocalization(15));
            AddLabel(173, 121, 1150, Tournament.StartTime.Day.ToString());
            AddButton(195, 121, 0xFD, 0xFD, 2, GumpButtonType.Reply, 0);

            AddImageTiled(220, 121, 60, 20, DarkEntry);
            AddTooltip(Localizations.GetLocalization(16));
            AddLabel(223, 121, 1150, GetHour(Tournament.StartTime.Hour));
            AddButton(265, 121, 0xFD, 0xFD, 3, GumpButtonType.Reply, 0);

            AddHtml(0, 172, 300, 16, ColorAndCenter(LabelColor, "Fight Info"), false, false);

            var typeName = ArenaHelper.GetTourneyType(Tournament);

            AddLabel(15, 193, 0, "Tourney Type:");
            AddImageTiled(115, 193, 165, 20, DarkEntry);
            AddTooltip(typeName, Localizations.GetTournyTypeTooltip(Tournament));
            AddLabel(118, 193, 1150, typeName);
            AddButton(265, 193, 0xFD, 0xFD, 4, GumpButtonType.Reply, 0);

            AddLabel(15, 215, 0, "Team Size:");
            AddImageTiled(115, 215, 165, 20, DarkEntry);
            AddTooltip(Localizations.GetLocalization(18));
            AddLabel(118, 215, 1150, Tournament.FightType == ArenaTeamType.None ? "Select Team Type" : Tournament.FightType.ToString());
            AddButton(265, 215, 0xFD, 0xFD, 5, GumpButtonType.Reply, 0);

            var styleName = ArenaHelper.GetTourneyStyle(Tournament);

            AddLabel(15, 238, 0, "Tourny Style:");
            AddImageTiled(115, 238, 165, 20, DarkEntry);
            AddTooltip(styleName, Localizations.GetStyleTooltip(Tournament));
            AddLabel(118, 238, 1150, styleName);
            AddButton(265, 238, 0xFD, 0xFD, 6, GumpButtonType.Reply, 0);

            AddImageTiled(115, 261, 40, 20, DarkEntry);
            AddTooltip(Localizations.GetLocalization(20));
            AddLabel(15, 261, 0, "Fight Duration");
            AddTextEntry(117, 261, 44, 20, 1150, 5, Tournament.TimePerFight.TotalMinutes.ToString());
             
            if (System.CanUseAlternateArena)
            {
                AddCheck(15, 284, 210, 211, Tournament.UseAlternateArena, 555);
                AddTooltip(Localizations.GetLocalization(21));
                AddLabel(35, 284, 0, "Using Alternate Arena");
            }

            AddHtml(0, 322, 300, 16, ColorAndCenter(LabelColor, "Rules"), false, false);

            AddLabel(35, 345, 0, "Pure Mage");
            AddCheck(15, 345, 210, 211, Tournament.HasRule(FightRules.PureMage), 2);
            AddTooltip(Localizations.GetLocalization(22));

            AddLabel(35, 370, 0, "Pure Dexxer");
            AddCheck(15, 370, 210, 211, Tournament.HasRule(FightRules.PureDexxer), 3);
            AddTooltip(Localizations.GetLocalization(23));

            AddLabel(150, 395, 0, "No Consumables");
            AddCheck(130, 395, 210, 211, Tournament.HasRule(FightRules.NoPots), 4);
            AddTooltip(Localizations.GetLocalization(24));

            AddLabel(150, 345, 0, "No Precasting");
            AddCheck(130, 345, 210, 211, Tournament.HasRule(FightRules.NoPrecasting), 5);
            AddTooltip(Localizations.GetLocalization(25));

            AddLabel(150, 370, 0, "No Summons");
            AddCheck(130, 370, 210, 211, Tournament.HasRule(FightRules.NoSummons), 6);
            AddTooltip(Localizations.GetLocalization(26));

            AddLabel(35, 395, 0, "No Specials");
            AddCheck(15, 395, 210, 211, Tournament.HasRule(FightRules.NoSpecials), 7);
            AddTooltip(Localizations.GetLocalization(27));

            AddLabel(150, 420, 0, "Allow Resurrections");
            AddCheck(130, 420, 210, 211, Tournament.HasRule(FightRules.AllowResurrections), 8);
            AddTooltip(Localizations.GetLocalization(28));

            AddLabel(35, 420, 0, "Allow Mounts");
            AddCheck(15, 420, 210, 211, Tournament.HasRule(FightRules.AllowMounts), 9);
            AddTooltip(Localizations.GetLocalization(29));

            AddLabel(35, 445, 0, "No Ties");
            AddImage(15, 445, 211);
            AddTooltip(Localizations.GetLocalization(30));

            AddLabel(150, 445, 0, "No Area Spells");
            AddCheck(130, 445, 210, 211, Tournament.HasRule(FightRules.NoAreaSpells), 100);
            AddTooltip(Localizations.GetLocalization(31));

            AddHtml(353, 115, 150, 20, Color("#FFFFFF", "Rewards"), false, false);

            AddLabel(325, 137, 0, "Winner Prize");
            AddButton(305, 137, 1209, 1210, 7, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(32));

            AddLabel(325, 273, 0, "Runner Up Prize");
            AddButton(305, 273, 1209, 1210, 8, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(33));

            if (Tournament.ChampPrize != null)
            {
                Item item = Tournament.ChampPrize;

                Rectangle2D b = ItemBounds.Table[item.ItemID];
                AddItem(380 - b.Width / 2 - b.X, 210 - b.Height / 2 - b.Y, item.ItemID, item.Hue);
                AddItemProperty(item);
            }
            if (Tournament.RunnerUpPrize != null)
            {
                Item item = Tournament.RunnerUpPrize;

                Rectangle2D b = ItemBounds.Table[item.ItemID];
                AddItem(380 - b.Width / 2 - b.X, 350 - b.Height / 2 - b.Y, item.ItemID, item.Hue);
                AddItemProperty(item);
            }

            if (User.AccessLevel >= AccessLevel.GameMaster && Config.Debug)
            {
                AddLabel(48, 142, 0, "In 5");
                AddButton(15, 140, 4005, 4006, 50, GumpButtonType.Reply, 0);
            }

            AddButton(80, 482, 2450, 2452, 60, GumpButtonType.Reply, 0);   //Okay
            AddButton(160, 482, 2453, 2455, 0, GumpButtonType.Reply, 0);  //Cancel
        }

        public string Name { get; private set; }
        public string Fee { get; private set; }
        public string MaxPart { get; private set; }
        public string MinPart { get; private set; }
        public string Duration { get; private set; }

        public int Month { get; private set; }
        public int Day { get; private set; }
        public int Hour { get; private set; }

        public void UpdateTime()
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            int days = GetDaysInMonth();

            if (Month == 0)
                Month = Tournament.StartTime.Month;

            if (month > Month)
                year++;

            if (Day > days) Day = days;
            if (Day == 0) Day = 1;

            if (Hour < 0) Hour = 12;
            if (Hour > 23) Hour = 23;

            try
            {
                Tournament.StartTime = new DateTime(year, Month, Day, Hour, 1, 0);
            }
            catch
            {
                Console.WriteLine("{0}, {1}, {2}, {3}", year, Month, Day, Hour);
            }
        }

        public int GetDaysInMonth()
        {
            if (Month == 1 || Month == 3 || Month == 5 || Month == 7 || Month == 8 || Month == 10 || Month == 12)
                return 31;

            if (Month == 2)
                return 28;

            return 30;
        }

        public string GetMonth()
        {
            int month = Month > 0 ? Month : Tournament.StartTime.Month;

            switch (month)
            {
                default:
                case 1: return "January";
                case 2: return "February";
                case 3: return "March";
                case 4: return "April";
                case 5: return "May";
                case 6: return "June";
                case 7: return "July";
                case 8: return "August";
                case 9: return "September";
                case 10: return "October";
                case 11: return "November";
                case 12: return "December";
            }
        }

        public string GetHour(int hour)
        {
            string d = hour >= 12 && hour != 0 ? "pm" : "am";
            int value = hour > 12 && hour != 0 ? hour - 12 : hour;

            if (value == 0)
                value = 12;

            return String.Format("{0} {1}", value.ToString(), d);
        }

        public override void OnResponse(RelayInfo button)
        {
            if (button.ButtonID == 0)
                return;

            HandleSwitch(button);
            HandleRadio(button);
            HandleTextInput(button);

            int fee = Utility.ToInt32(Fee);

            if (fee < 0)
                fee = Tournament.EntryFee;
            else
                Tournament.EntryFee = fee;

            Tournament.Name = Name;
            Tournament.MinParticipants = Utility.ToInt32(MinPart);
            Tournament.MaxParticipants = Utility.ToInt32(MaxPart);

            int duration = Utility.ToInt32(Duration);

            if(duration > 0)
                Tournament.TimePerFight = TimeSpan.FromMinutes(duration);

            switch (button.ButtonID)
            {
                case 1: // Pick Month
                    if (Month == 12)
                    {
                        Month = 1;
                    }
                    else
                    {
                        Month++;
                    }

                    UpdateTime();
                    Refresh();
                    break;
                case 2: // Pick Day
                    int days = GetDaysInMonth();

                    if (Day >= days)
                    {
                        Day = 1;
                    }
                    else
                    {
                        Day++;
                    }

                    UpdateTime(); 
                    Refresh();
                    break;
                case 3: // Pick Time
                    if (Hour >= 24)
                    {
                        Hour = 1;
                    }
                    else
                    {
                        Hour++;
                    }

                    UpdateTime(); 
                    Refresh();

                    break;
                case 4: // Tourney Type
                    int type = (int)Tournament.TourneyType;

                    if (type >= 4)
                    {
                        type = 1;
                    }
                    else
                    {
                        type++;
                    }

                    Tournament.TourneyType = (TourneyType)type;
                    Refresh();
                    break;
                case 5:
                    switch (Tournament.FightType)
                    {
                        case ArenaTeamType.Single: 
                            Tournament.FightType = ArenaTeamType.Twosome; 
                            break;
                        case ArenaTeamType.Twosome: 
                            Tournament.FightType = ArenaTeamType.Foursome; 
                            break;
                        default:
                        case ArenaTeamType.Foursome: 
                            Tournament.FightType = ArenaTeamType.Single; 
                            break;
                    }

                    Refresh();
                    break;
                case 6:
                    switch (Tournament.TourneyStyle)
                    {
                        case TourneyStyle.Standard: Tournament.TourneyStyle = TourneyStyle.MagesOnly; break;
                        case TourneyStyle.MagesOnly: Tournament.TourneyStyle = TourneyStyle.DexxersOnly; break;
                        default:
                        case TourneyStyle.DexxersOnly: Tournament.TourneyStyle = TourneyStyle.Standard; break;
                    }

                    Refresh();
                    break;
                case 7:
                    User.SendMessage("Target the item you choose as the Tournament Champions Prize.");
                    User.Target = new InternalTarget(Tournament, System, true);

                    Refresh();
                    break;
                case 8:
                    User.SendMessage("Target the item you choose as the Tournament Runner Ups Prize.");
                    User.Target = new InternalTarget(Tournament, System, false);

                    Refresh();
                    break;
                case 50: // start in 5 for testing purposes
                    if (User.AccessLevel > AccessLevel.VIP)
                    {
                        Tournament.StartTime = DateTime.Now + TimeSpan.FromMinutes(5);
                        User.SendMessage("You have selected a new start time!");
                        Refresh();
                    }
                    break;
                case 60:
                    // Name
                    bool validname = ValidateName();
                    // Fee
                    bool validfee = ValidateFee();
                    // Max Participants
                    bool validmaxparticipants = ValidateMaxParticipants();
                    // Min Participants
                    bool validminparticipants = ValidateMinParticipants();
                    // Fight Duration
                    bool validfightduration = ValidateFightDuration();
                    // Start Time
                    bool validstarttime = ValidateStartTime();

                    if (Tournament.FightType == ArenaTeamType.None)
                    {
                        User.SendMessage(0x23, "You have not selected the team sizes!");
                        Refresh();
                    }
                    else if (validname && validfee && validmaxparticipants && validminparticipants && validfightduration && validstarttime)
                    {
                        BaseGump.SendGump(new ConfirmTournamentGump(System, Tournament, User));
                    }
                    else
                    {
                        Refresh();
                    }
                    break;
            }
        }

        public void HandleTextInput(RelayInfo info)
        {
            TextRelay relay = info.GetTextEntry(1);

            if (relay != null && !string.IsNullOrEmpty(relay.Text))
            {
                Name = relay.Text;
            }

            relay = info.GetTextEntry(2);

            if (relay != null && !string.IsNullOrEmpty(relay.Text))
            {
                Fee = relay.Text;
            }

            relay = info.GetTextEntry(3);

            if (relay != null && !string.IsNullOrEmpty(relay.Text))
            {
                MaxPart = relay.Text;
            }

            relay = info.GetTextEntry(4);

            if (relay != null && !string.IsNullOrEmpty(relay.Text))
            {
                MinPart = relay.Text;
            }

            relay = info.GetTextEntry(5);

            if (relay != null && !string.IsNullOrEmpty(relay.Text))
            {
                Duration = relay.Text;
            }
        }

        public void HandleSwitch(RelayInfo info)
        {
            AddOrRemoveRule(FightRules.PureMage, info.IsSwitched(2));
            AddOrRemoveRule(FightRules.PureDexxer, info.IsSwitched(3));
            AddOrRemoveRule(FightRules.NoPots, info.IsSwitched(4));
            AddOrRemoveRule(FightRules.NoPrecasting, info.IsSwitched(5));
            AddOrRemoveRule(FightRules.NoSummons, info.IsSwitched(6));
            AddOrRemoveRule(FightRules.NoSpecials, info.IsSwitched(7));
            AddOrRemoveRule(FightRules.AllowResurrections, info.IsSwitched(8));
            AddOrRemoveRule(FightRules.AllowMounts, info.IsSwitched(8));
            AddOrRemoveRule(FightRules.NoAreaSpells, info.IsSwitched(100));

            Tournament.UseAlternateArena = info.IsSwitched(555);
        }

        public void HandleRadio(RelayInfo info)
        {
            if (info.IsSwitched(11))
            {
                Tournament.TourneyType = TourneyType.BestOf3;
            }
            /*else if (info.IsSwitched(12))
            {
                Tournament.TourneyType = TourneyType.Bracketed;
            }*/
            else if (info.IsSwitched(13))
            {
                Tournament.FightType = ArenaTeamType.Single;
            }
            else if (info.IsSwitched(14))
            {
                Tournament.FightType = ArenaTeamType.Twosome;
            }
            else if (info.IsSwitched(15))
            {
                Tournament.FightType = ArenaTeamType.Foursome;
            }
        }

        private bool ValidateName()
        {
            string text = Tournament.Name;

            if (String.IsNullOrEmpty(text) || text == null || text.Length == 0)
                User.SendMessage(0x23, "You have chosen an invalid name.");
            else if (text.Length > 16)
                User.SendMessage(0x23, "Your tournament name can only be 16 characters in length.");
            else if (System.TournamentNameExists(text) || PVPTournamentStats.NameExists(System, text))
                User.SendMessage(0x23, "That tournament name already exists!");
            else if (!Server.Guilds.BaseGuildGump.CheckProfanity(text) || text == "Tournament Name")
                User.SendMessage(0x23, "That name is unacceptable.");
            else
            {
                Tournament.Name = text;
                return true;
            }
            return false;
        }

        private bool ValidateFee()
        {
            int amount = Tournament.EntryFee;

            if (amount >= 0 && (Config.MaxEntryFee == -1 || amount <= Config.MaxEntryFee))
            {
                return true;
            }
            else
            {
                User.SendMessage(0x23, "You must select an entry fee between 0 and {0} gold", Config.MaxEntryFee.ToString("N0"));
            }

            return false;
        }

        private bool ValidateMaxParticipants()
        {
            int num = Tournament.MaxParticipants;

            if (num < Config.MinEntries || num > Config.MaxEntries || num < Tournament.MinParticipants)
            {
                User.SendMessage(0x23, "You have entered an invalid amount. Choose between {0} and {1} for the maximum participants allowed.", Config.MinEntries, Config.MaxEntries);
                return false;
            }

            return true;
        }

        private bool ValidateMinParticipants()
        {
            int num = Tournament.MinParticipants;

            if (num < Config.MinEntries || num > Config.MaxEntries || num > Tournament.MaxParticipants)
            {
                User.SendMessage(0x23, "You have entered an invalid amount. Choose between {0} and {1} for the minimum participants allowed.", Config.MinEntries, Config.MaxEntries);
                return false;
            }

            return true;
        }

        private bool ValidateFightDuration()
        {
            int minutes = (int)Tournament.TimePerFight.TotalMinutes;

            if (minutes > 0 && minutes <= 30)
            {
                Tournament.TimePerFight = TimeSpan.FromMinutes(minutes);
            }
            else
            {
                User.SendMessage(0x23, "Fight durations must be between 1 and 30 minutes.");
                return false;
            }

            return true;
        }

        private bool ValidateStartTime()
        {
            int month = Tournament.StartTime.Month;
            int day = Tournament.StartTime.Day;
            int hour = Tournament.StartTime.Hour;
            int daysinmonth = GetDaysInMonth();

            if (month < 1 || month > 12)
                User.SendMessage(0x23, "Invalid Month!");
            else if (day < 1 || day > daysinmonth)
                User.SendMessage(0x23, "Invalid Day!.");
            else if (hour < 0 || hour > 23)
                User.SendMessage(0x23, "Choose an hour between 12am and 11pm");
            else
            {
                DateTime now = DateTime.Now;
                DateTime newDate = Tournament.StartTime;

                if (newDate < now)
                    Tournament.StartTime = newDate.AddYears(1);

                bool hasTourney = false;
                foreach (Tournament tourney in System.Tournaments)
                {
                    if (tourney.StartTime.Date == newDate.Date)
                    {
                        User.SendMessage(0x23, "A tournament has already been scheduled that day. Please choose another date.");
                        hasTourney = true;
                        break;
                    }
                    else if (newDate < tourney.StartTime && tourney.StartTime - newDate < TimeSpan.FromHours(12))
                    {
                        User.SendMessage(0x23, "The tournament start time must be at least 12 hours before the next tournament time.");
                        hasTourney = true;
                        break;
                    }
                    else if (newDate > tourney.StartTime && newDate - tourney.StartTime < TimeSpan.FromHours(12))
                    {
                        User.SendMessage(0x23, "The tournament start time must be at least 12 hours after the closest tournament time.");
                        hasTourney = true;
                        break;
                    }
                }
                if (hasTourney)
                {
                    return false;
                }
                if (User.AccessLevel <= AccessLevel.VIP && newDate - now < Config.TournamentWait)
                    User.SendMessage(0x23, "You must schedule a tournament at least 5 days in advance.");
                else if (User.AccessLevel <= AccessLevel.VIP && newDate - now > TimeSpan.FromDays(180))
                    User.SendMessage(0x23, "You cannot schedule a tournament more than 180 days in advance.");
                else if (!hasTourney)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddOrRemoveRule(FightRules rule, bool add)
        {
            if (add)
            {
                if ((Tournament.Rules & rule) == 0)
                    Tournament.Rules |= rule;
            }
            else
            {
                if ((Tournament.Rules & rule) != 0)
                    Tournament.Rules ^= rule;
            }
        }

        private class InternalTarget : Target
        {
            private Tournament Tournament;
            private PVPTournamentSystem System;
            private bool m_WinnerPrize;

            public InternalTarget(Tournament tourney, PVPTournamentSystem sys, bool forWinner) : base(1, false, TargetFlags.None)
            {
                Tournament = tourney;
                System = sys;
                m_WinnerPrize = forWinner;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if(!(from is PlayerMobile))
                    return;

                if (targeted is Item)
                {
                    Item item = (Item)targeted;

                    if (from.Backpack != null && item.IsChildOf(from.Backpack))
                    {
                        if (m_WinnerPrize)
                        {
                            if (item == Tournament.RunnerUpPrize)
                            {
                                from.SendMessage("The champion and runner up prize cannot be the same!");
                                BaseGump.SendGump(new RegisterTournamentGump(System, Tournament, from as PlayerMobile));

                                return;
                            }
                            if (Tournament.ChampPrize != null)
                                Tournament.ChampPrize = null;
                            Tournament.ChampPrize = item;
                        }
                        else
                        {
                            if (item == Tournament.ChampPrize)
                            {
                                from.SendMessage("The champion and runner up prize cannot be the same!");
                                BaseGump.SendGump(new RegisterTournamentGump(System, Tournament, from as PlayerMobile));

                                return;
                            }
                            if (Tournament.RunnerUpPrize != null)
                                Tournament.RunnerUpPrize = null;
                            Tournament.RunnerUpPrize = item;
                        }
                        from.SendMessage("You have selected a prize. Be sure to keep it in your backpack for the duration of the tournament registration.");
                    }
                    else
                        from.SendMessage("The targeted item must be in your backpack!");
                }

                var gump = BaseGump.GetGump<RegisterTournamentGump>(from as PlayerMobile, null);

                if (gump != null)
                {
                    gump.Refresh();
                }
                else
                {
                    BaseGump.SendGump(new RegisterTournamentGump(System, Tournament, from as PlayerMobile));
                }
            }

            protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
            {
                if (m_WinnerPrize && Tournament.ChampPrize != null)
                    Tournament.ChampPrize = null;
                if (!m_WinnerPrize && Tournament.RunnerUpPrize != null)
                    Tournament.RunnerUpPrize = null;

                if(from is PlayerMobile)
                    BaseGump.SendGump(new RegisterTournamentGump(System, Tournament, from as PlayerMobile));
            }
        }
    }

    public class ConfirmTournamentGump : BaseTournamentGump
    {
        private PVPTournamentSystem System;
        private Tournament Tournament;

        public ConfirmTournamentGump(PVPTournamentSystem system, Tournament tourney, PlayerMobile from)
            : base(from, 20, 20)
        {
            System = system;
            Tournament = tourney;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 350, 375, DarkBackground);
            AddBackground(75, 11, 200, 30, LightBackground);
            AddBackground(10, 54, 330, 310, LightBackground);

            AddHtml(0, 16, 350, 16, Center("Tournament Info"), false, false);

            AddLabel(20, 60, LabelHue, "Tournament:");
            AddLabel(125, 60, 0, Tournament.Name);

            AddLabel(20, 80, LabelHue, "Date:");
            AddLabel(125, 80, 0, String.Format("{0}", Tournament.StartTime.ToString()));

            AddLabel(20, 100, LabelHue, "Teams:");
            AddLabel(125, 100, 0, String.Format("{0}", Tournament.FightType));

            AddLabel(20, 120, LabelHue, "Entry Fee:");
            AddLabel(125, 120, 0, Tournament.EntryFee.ToString());

            AddLabel(20, 140, LabelHue, "Type:");
            AddLabel(125, 140, 0, String.Format(ArenaHelper.GetTourneyType(Tournament)));

            AddLabel(20, 160, LabelHue, "Fight Duration:");
            AddLabel(125, 160, 0, String.Format("{0} minutes", Tournament.TimePerFight.TotalMinutes.ToString()));

            AddLabel(20, 180, LabelHue, "Alternate Arena:");
            AddLabel(125, 180, 0, Tournament.UseAlternateArena ? "Yes" : "No");

            //AddLabel(20, 200, LabelHue, "Rules:");
            AddLabel(20, 200, LabelHue, "Style:");
            AddLabel(125, 200, 0, ArenaHelper.GetTourneyStyle(Tournament));
            AddBackground(18, 218, 314, 120, LightBackground);

            if (Tournament.Rules > 0)
            {
                int yStart = 220;
                int idx = 0;

                foreach (int i in Enum.GetValues(typeof(FightRules)))
                {
                    if (!Tournament.HasRule((FightRules)i))
                        continue;

                    if (idx % 2 == 0)
                        AddLabel(22, yStart, 0, ArenaHelper.GetRules((FightRules)i));
                    else
                    {
                        AddLabel(150, yStart, 0, ArenaHelper.GetRules((FightRules)i));
                        yStart += 25;
                    }

                    idx++;
                }
            }

            AddHtml(204, 342, 100, 20, "<DIV ALIGN=RIGHT>Register</DIV>", false, false);
            AddButton(309, 342, 4005, 4006, 2, GumpButtonType.Reply, 0);

            AddHtml(45, 343, 100, 20, "Back", false, false);
            AddButton(11, 342, 4014, 4015, 1, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(RelayInfo button)
        {
            switch (button.ButtonID)
            {
                default:
                case 1:
                    BaseGump.SendGump(new RegisterTournamentGump(System, Tournament, User));
                    break;
                case 2:
                    Tournament.Register(User);
                    User.SendMessage("You have registered a new tournament!");
                    break;
            }
        }
    }
}
