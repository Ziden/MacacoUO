using System;
using System.Collections.Generic;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
using Server.Misc;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public enum ErrorType
    {
        Valid,
        Invalid,
        TooManyChars,
        NotEnoughChars,
        AlreadyExists,
        Unacceptable,
        BadCount,
    }

    public class RegisterTeamGump : BaseTournamentGump
    {
        private ArenaTeam Team { get; set; }
        private PVPTournamentSystem System { get; set; }
        public string TextInput { get; set; }

        public RegisterTeamGump(PlayerMobile from, ArenaTeam team, PVPTournamentSystem sys) : base(from, 325, 40)
        {
            Team = team;
            System = sys;
        }

        public override void AddGumpLayout()
        {
            int fighters = Team.Fighters.Count;
            int type = (int)Team.TeamType;
            int yOffset = (fighters - 1) * 30;

            AddPage(0);
            AddBackground(0, 0, 300, 260 + yOffset, DarkBackground);
            AddImageTiled(13, 60, 200, 20, LightEntry);

            AddHtml(0, 7, 300, 16, ColorAndCenter(LabelColor, "Team Registration"), false, false);

            AddImageTiled(13, 30, 160, 21, LightEntry);
            AddTooltip(Localizations.GetLocalization(10));
            AddLabel(15, 30, 0, Team.TeamType == ArenaTeamType.None ? "Team Type" : Team.TeamType.ToString());
            AddButton(158, 30, 0xFD, 0xFD, 200, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(10));

            AddTextEntry(15, 60, 178, 20, 0, 0, (Team.Name == null || Team.Name == "") ? "Team Name" : Team.Name);
            AddTooltip(Localizations.GetLocalization(11));

            AddHtml(15, 90, 275, 16, Color(LabelColor, String.Format("Team Leader: {0}", Team.TeamLeader != null ? Team.TeamLeader.Name : "Nobody")), false, false);

            int startY = 120;
            for (int i = 0; i < Team.Fighters.Count; ++i)
            {
                if (i == 0)
                    continue;

                AddHtml(15, startY, 250, 16, Color(LabelColor, String.Format("Player {0}: {1}", i + 1, Team.Fighters[i].Name)), false, false);
                startY += 30;
            }

            if (type > 1 && fighters < type)
            {
                AddHtml(48, startY, 250, 16, Color(LabelColor, "Choose Fighter"), false, false);
                AddButton(15, startY, 4005, 4006, 1, GumpButtonType.Reply, 0);
                AddTooltip(Localizations.GetLocalization(12));
            }

            AddButton(80, 230 + yOffset, 2450, 2452, 6, GumpButtonType.Reply, 0); //Okay
            AddButton(160, 230 + yOffset, 2453, 2455, 0, GumpButtonType.Reply, 0); //Cancel
        }

        public override void OnResponse(RelayInfo button)
        {
            if (button.ButtonID == 0)
                return;

            HandleTextInput(button);

            ArenaTeamType original = Team.TeamType;

            string name = TextInput;

            ErrorType nameType = ValidateName(name);

            if (nameType == ErrorType.Valid)
            {
                Team.Name = name;
            }

            switch (button.ButtonID)
            {
                case 1:
                    {
                        User.Target = new InternalTarget(User, Team, System);
                        User.SendMessage("Target the player you'd like to have in your arena team.");
                        break;
                    }
                case 200:
                    {
                        switch (Team.TeamType)
                        {
                            case ArenaTeamType.Single: Team.TeamType = ArenaTeamType.Twosome; break;
                            case ArenaTeamType.Twosome: Team.TeamType = ArenaTeamType.Foursome; break;
                            default:
                            case ArenaTeamType.Foursome: Team.TeamType = ArenaTeamType.Single; break;
                        }

                        Refresh();
                    }
                    break;
                case 6:
                    {
                        bool register = true;
                        ErrorType teamType = ValidateTeamType();

                        if (nameType != ErrorType.Valid)
                        {
                            register = false;

                            switch(nameType)
                            {
                                default:
                                case ErrorType.Invalid: 
                                    User.SendMessage(0x23, "You have chosen an invalid name.");
                                    break;
                                case ErrorType.TooManyChars: 
                                    User.SendMessage(0x23, "The name must be no more than 16 characters.");
                                    break;
                                case ErrorType.NotEnoughChars:
                                    User.SendMessage(0x23, "Name too short.");
                                    break;
                                case ErrorType.AlreadyExists:
                                    User.SendMessage(0x23, "That arena team name already exists.");
                                    break;
                                case ErrorType.Unacceptable:
                                    User.SendMessage(0x23, "That name is unacceptable.");
                                    break;
                            }
                        }

                        if (teamType != ErrorType.Valid)
                        {
                            Team.TeamType = original;
                            User.SendMessage(0x23, "Invalid response. Make sure your team amount is consistent with the amount of players you've chosen and that you don't belong to another team of that capacity.");
                            register = false;
                        }

                        if (register)
                        {
                            Team.Name = name;
                            Team.RegisterTeam();

                            foreach (Mobile mob in Team.Fighters)
                                mob.SendMessage("You have {1} the {0} Arena Team.", Team.Name, Team.TeamLeader == mob ? "created" : "joined");
                        }
                        else
                        {
                            Refresh();
                        }
                        break;
                    }
            }
        }

        public void HandleTextInput(RelayInfo info)
        {
            TextRelay relay = info.GetTextEntry(0);

            if (relay != null && !string.IsNullOrEmpty(relay.Text))
            {
                TextInput = relay.Text.Trim();
            }
        }

        public static ErrorType ValidateName(string text)
        {
            if (text == null || String.IsNullOrEmpty(text) || text.Length == 0)
            {
                return ErrorType.NotEnoughChars;
            }
            else if (text.ToLower() == "team name")
            {
                return ErrorType.Invalid;
            }
            else if (text.Length > 16)
            {
                return ErrorType.TooManyChars;
            }
            else if (ArenaTeam.NameExists(text))
            {
                return ErrorType.AlreadyExists;
            }
            else if (!Server.Guilds.BaseGuildGump.CheckProfanity(text))
            {
                return ErrorType.Unacceptable;
            }
            else
            {
                return ErrorType.Valid;
            }
        }

        private ErrorType ValidateTeamType()
        {
            int amount = (int)Team.TeamType;

            if (amount < 1 || amount == 3 || amount > 4)
            {
                return ErrorType.BadCount;
            }

            if (Team.Fighters.Count != amount)
            {
                return ErrorType.BadCount;
            }
            
            if (amount == 1 && !ArenaTeam.HasTeam(User, ArenaTeamType.Single))
                Team.TeamType = ArenaTeamType.Single;
            else if (amount == 2 && !ArenaTeam.HasTeam(User, ArenaTeamType.Twosome))
                Team.TeamType = ArenaTeamType.Twosome;
            else if (amount == 4 && !ArenaTeam.HasTeam(User, ArenaTeamType.Foursome))
                Team.TeamType = ArenaTeamType.Foursome;
            else
            {
                return ErrorType.BadCount;
            }

            return ErrorType.Valid;
        }

        public class InternalTarget : Target
        {
            private PlayerMobile From { get; set; }
            private ArenaTeam Team { get; set; }
            private PVPTournamentSystem System { get; set; }

            public InternalTarget(PlayerMobile from, ArenaTeam team, PVPTournamentSystem sys): base(12, false, TargetFlags.None)
            {
                From = from;
                Team = team;
                System = sys;
                CheckLOS = false;
            }

            protected override void OnTarget(Mobile from, object target)
            {
                if (target is Mobile)
                {
                    Mobile mob = (Mobile)target;

                    if (mob is PlayerMobile && mob.Alive)
                    {
                        Mobile keeper = System.ArenaKeeper;

                        if (keeper == null || !keeper.InRange(mob.Location, 25))
                            from.SendMessage("They must be within sight of the Arena Keeper to register as a fighter.");
                        else if (mob is PlayerMobile && ((PlayerMobile)mob).Young)
                            from.SendMessage("You cannot add a young player to your team!");
                        else if (ArenaHelper.IsSameAccount(from, mob) && Server.TournamentSystem.Config.RestrictSameIP)
                            From.SendMessage("That is your character.  Please choose a character that is not of your own!");
                        else if (ArenaTeam.HasTeam(mob, Team.TeamType))
                            From.SendMessage("They already belong to a team with that many fighters.");
                        else if (mob == From)
                            From.SendMessage("You are automatically added...No need to add yourself again!");
                        else if (Team.IsInTeam(mob))
                            From.SendMessage("You have already added them to the team!");
                        else if (ActionTimer.WaitingAction.ContainsKey(mob))
                            From.SendMessage("They are busy deciding other actions.  Try them again in a few moments.");
                        else
                        {
                            new ActionTimer(From, mob, TimeSpan.FromSeconds(60));
                            BaseGump.SendGump(new AddFighterGump(From, mob as PlayerMobile, Team, false, System));

                            From.SendMessage("Please wait while they contemplate joining the team.");
                        }

                    }
                    else
                        From.SendMessage("You cannot add them as a member of the team.");
                }

                BaseGump.SendGump(new RegisterTeamGump(From, Team, System));
            }

            protected override void OnTargetCancel(Mobile from, TargetCancelType type)
            {
                BaseGump.SendGump(new RegisterTeamGump(From, Team, System));
            }
        }
    }

    public class AddFighterGump : BaseTournamentGump
    {
        private ArenaTeam Team { get; set; }
        private PlayerMobile Offerer { get; set; }
        private bool TeamExists { get; set; }
        private PVPTournamentSystem System { get; set; }

        public AddFighterGump(PlayerMobile from, PlayerMobile user, ArenaTeam team, bool exists, PVPTournamentSystem sys) : base(user, 0, 0)
        {
            Team = team;
            Offerer = from;
            TeamExists = exists;
            System = sys;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 250, 200, 5120);
            AddBackground(10, 10, 230, 180, 5100);

            if (Team == null || Offerer == null)
                return;

            AddButton(30, 155, 2074, 2075, 1, GumpButtonType.Reply, 0); //Okay
            AddButton(175, 155, 2071, 2072, 2, GumpButtonType.Reply, 0);  //Cancel

            string text = String.Format("{0} has been asked to join {2} {1} player arena team. Joining a team is not permanent; You can always resign using the \"My Team Info\" gump.", Offerer.Name, (int)Team.TeamType, Offerer.Female ? "her" : "his");
            AddHtml(25, 20, 200, 120, text, true, true);
        }

        public override void OnResponse(RelayInfo button)
        {
            PlayerMobile from = User;

            switch (button.ButtonID)
            {
                default:
                case 0: break;
                case 1:
                    {
                        if (Team.Fighters.Count >= (int)Team.TeamType)
                            from.SendMessage("The team has already been filled to its maximum capacity.");
                        else
                        {
                            Team.AddFighter(from, TeamExists);
                            Offerer.SendMessage("{0} has agreed to joined your team!", from.Name);
                            from.SendMessage("You have agreed to join {0}'s Arena Team!", Offerer.Name);
                            ActionTimer.EndAction(from);
                            
                            if (Offerer.HasGump(typeof(RegisterTeamGump)))
                                Offerer.CloseGump(typeof(RegisterTeamGump));
                            if (Offerer.HasGump(typeof(PlayerRecordGump)))
                                Offerer.CloseGump(typeof(PlayerRecordGump));
                            
                            if (!TeamExists)
                                BaseGump.SendGump(new RegisterTeamGump(Offerer, Team, System));
                            else
                                BaseGump.SendGump(new PlayerRecordGump(Offerer));
                        }
                        break;
                    }
                case 2:
                    {
                        Offerer.SendMessage("They have chosen not to join your team at this time.");
                        from.SendMessage("You choose not to join their team at this time.");

                        if (Offerer.HasGump(typeof(RegisterTeamGump)))
                            Offerer.CloseGump(typeof(RegisterTeamGump));

                        if (!TeamExists)
                            BaseGump.SendGump(new RegisterTeamGump(Offerer, Team, System));
                        else
                            BaseGump.SendGump(new PlayerRecordGump(Offerer));
                        ActionTimer.EndAction(from);
                        break;
                    }

            }
        }
    }
}
