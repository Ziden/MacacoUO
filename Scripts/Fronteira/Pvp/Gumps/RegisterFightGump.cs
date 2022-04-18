using System;
using System.Linq;
using System.Collections.Generic;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
using Server.Items;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class ChooseFightTypeGump : BaseTournamentGump
    {
        public PVPTournamentSystem System { get; set; }

        public ChooseFightTypeGump(PlayerMobile pm, PVPTournamentSystem system)
            : base(pm, 200, 200)
        {
            System = system;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 200, 250, DarkBackground);
            AddBackground(10, 10, 180, 230, LightBackground);

            AddHtml(0, 15, 200, 20, Center("Choose Fight Type"), false, false);
            var x = 50;

            foreach (int i in Enum.GetValues(typeof(ArenaFightType)))
            {
                var type = (ArenaFightType)i;

                if (System.FightTypes.Any(types => types == (ArenaFightType)i))
                {
                    AddButton(15, x, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
                    AddTooltip(Localizations.GetLocalization(100 + i));
                    AddHtml(48, x, 142, 20, ArenaHelper.GetFightType(type), false, false);

                    x += 25;
                }
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID == 0)
                return;

            BaseGump.SendGump(new RegisterFightGump(System, System.ConstructArenaFight((ArenaFightType)info.ButtonID - 1), User));
        }
    }

    public class RegisterFightGump : BaseTournamentGump
    {
        private PVPTournamentSystem System { get; set; }
        private ArenaFight Fight { get; set; }

        public ArenaFightType FightType { get; set; }
        public string Text3 { get; set; }
        public string Text4 { get; set; }

        public RegisterFightGump(PVPTournamentSystem system, ArenaFight fight, PlayerMobile from) : base (from, 20, 20)
        {
            System = system;
            Fight = fight;
        }

        public override void AddGumpLayout()
        {
            bool lastman = Fight.ArenaFightType == ArenaFightType.LastManStanding;
            bool ctf = Fight.ArenaFightType == ArenaFightType.CaptureTheFlag;
            bool ctfRumble = Fight.ArenaFightType == ArenaFightType.TeamRumbleCaptureTheFlag;

            AddPage(0);
            AddBackground(0, 0, 300, 477, DarkBackground);
            AddBackground(10, 10, 280, 160, LightBackground); // main
            AddBackground(10, 180, 280, 60, LightBackground); // wager gold
            AddBackground(10, 250, 280, 204, LightBackground); // rules

            if (!ctfRumble)
            {
                AddHtml(20, 15, 280, 16, String.Format("Opponent: {0}", Fight.TeamB != null ? Fight.TeamB.Name : "None"), false, false);
            }

            if (Fight.FightType != ArenaTeamType.None && Fight.TeamA != null && Fight.TeamA.TeamType != Fight.FightType)
            {
                Fight.FightType = ArenaTeamType.None;
            }

            if (Fight.FightType != ArenaTeamType.None && !lastman && !ctfRumble)
            {
                AddHtml(48, 35, 150, 16, "Select Opponent", false, false);
                AddButton(15, 35, 4005, 4006, 1, GumpButtonType.Reply, 0);
                AddTooltip(Localizations.GetLocalization(37));
            }

            AddHtml(15, 60, 150, 16, Color(LabelColor, "Team Size"), false, false);

            if (lastman || ctfRumble)
                AddImage(15, 80, 211);
            else
                AddButton(15, 80, (int)Fight.FightType == 1 ? 211 : 210, (int)Fight.FightType == 1 ? 211 : 210, 2, GumpButtonType.Reply, 0);
            
            AddLabel(38, 80, (int)Fight.FightType == 1 ? 1925 : LabelHue, "1");

            if (lastman || ctfRumble)
                AddImage(15, 100, 210);
            else
                AddButton(15, 100, (int)Fight.FightType == 2 ? 211 : 210, (int)Fight.FightType == 2 ? 211 : 210, 3, GumpButtonType.Reply, 0);

            AddLabel(38, 100, (int)Fight.FightType == 2 ? 1925 : LabelHue, "2");

            if (lastman || ctfRumble)
                AddImage(15, 120, 210);
            else
                AddButton(15, 120, (int)Fight.FightType == 4 ? 211 : 210, (int)Fight.FightType == 4 ? 211 : 210, 4, GumpButtonType.Reply, 0);
            
            AddLabel(38, 120, (int)Fight.FightType == 4 ? 1925 : LabelHue, "4");

            AddHtml(15, 145, 250, 16, "Fight Time [Minutes]:", false, false);
            AddTooltip(Localizations.GetLocalization(20));
            AddBackground(150, 145, 130, 20, 0xBB8);
            AddTextEntry(152, 145, 44, 20, 1150, 3, Fight.FightDuration.TotalMinutes.ToString());

            AddHtml(150, 15, 150, 16, Color(LabelColor, "Fight Type"), false, false);
            AddHtml(150, 35, 145, 16, ArenaHelper.GetFightType(Fight.ArenaFightType), false, false);

            AddHtml(15, 200, 100, 16, "Wager Amount: ", false, false);
            AddTooltip(Localizations.GetLocalization(38), Localizations.GetLocalization(39));
            AddBackground(150, 200, 130, 20, 0xBB8);
            AddTextEntry(152, 200, 128, 20, 1150, 4, Fight.Wager.ToString("N0"));

            AddHtml(0, 253, 300, 16, ColorAndCenter(LabelColor, "Rules"), false, false);

            AddLabel(35, 275, 0, "Pure Mage");

            if (Fight.HasForcedRule(FightRules.PureMage))
                AddImage(15, 275, 211);
            else
                AddCheck(15, 275, 210, 211, Fight.HasRule(FightRules.PureMage), 1);
            AddTooltip(Localizations.GetLocalization(22));

            AddLabel(35, 300, 0, "Pure Dexxer");

            if (Fight.HasForcedRule(FightRules.PureDexxer))
                AddImage(15, 300, 211);
            else
                AddCheck(15, 300, 210, 211, Fight.HasRule(FightRules.PureDexxer), 2);
            AddTooltip(Localizations.GetLocalization(23));

            AddLabel(35, 325, 0, "No Consumables");

            if (Fight.HasForcedRule(FightRules.NoPots))
                AddImage(15, 325, 211);
            else
                AddCheck(15, 325, 210, 211, Fight.HasRule(FightRules.NoPots), 3);
            AddTooltip(Localizations.GetLocalization(24));

            AddLabel(150, 275, 0, "No Precasting");

            if (Fight.HasForcedRule(FightRules.NoPrecasting))
                AddImage(130, 275, 211);
            else
                AddCheck(130, 275, 210, 211, Fight.HasRule(FightRules.NoPrecasting), 4);
            AddTooltip(Localizations.GetLocalization(25));

            AddLabel(150, 300, 0, "No Summons");

            if (Fight.HasForcedRule(FightRules.NoSummons))
                AddImage(130, 300, 211);
            else
                AddCheck(130, 300, 210, 211, Fight.HasRule(FightRules.NoSummons), 5);
            AddTooltip(Localizations.GetLocalization(26));

            AddLabel(150, 325, 0, "No Specials");

            if (Fight.HasForcedRule(FightRules.NoSpecials))
                AddImage(130, 325, 211);
            else
                AddCheck(130, 325, 210, 211, Fight.HasRule(FightRules.NoSpecials), 6);
            AddTooltip(Localizations.GetLocalization(27));

            AddLabel(150, 350, 0, "Allow Resurrections");

            if (Fight.HasForcedRule(FightRules.AllowResurrections))
                AddImage(130, 350, 211);
            else
                AddCheck(130, 350, 210, 211, Fight.HasRule(FightRules.AllowResurrections), 7);
            AddTooltip(Localizations.GetLocalization(28));

            AddLabel(35, 350, 0, "Allow Mounts");

            if (Fight.HasForcedRule(FightRules.AllowMounts))
                AddImage(15, 350, 211);
            else
                AddCheck(15, 350, 210, 211, Fight.HasRule(FightRules.AllowMounts), 8);
            AddTooltip(Localizations.GetLocalization(29));

            AddLabel(35, 375, 0, "No Ties");

            if (Fight.HasForcedRule(FightRules.NoTies))
                AddImage(15, 375, 211);
            else
                AddCheck(15, 375, 210, 211, Fight.HasRule(FightRules.NoTies), 9);
            AddTooltip(Localizations.GetLocalization(30));

            AddLabel(150, 375, 0, "No Area Spells");

            if (Fight.HasForcedRule(FightRules.NoAreaSpells))
                AddImage(130, 375, 211);
            else
                AddCheck(130, 375, 210, 211, Fight.HasRule(FightRules.NoAreaSpells), 10);
            AddTooltip(Localizations.GetLocalization(31));

            AddLabel(35, 400, 0, "Allow Pets");

            if (Fight.HasForcedRule(FightRules.AllowPets))
                AddImage(15, 400, 211);
            else
                AddCheck(15, 400, 210, 211, Fight.HasRule(FightRules.AllowPets), 11);
            AddTooltip(Localizations.GetLocalization(85));

            if (Config.AllowStandardizedGear)
            {
                AddButton(15, 429, Fight.UseOwnGear ? 211 : 210, Fight.UseOwnGear ? 211 : 210, 8, GumpButtonType.Reply, 0);
                AddTooltip(Localizations.GetLocalization(58), Localizations.GetLocalization(59));
                AddLabel(35, 429, 0, "Use Own Gear");

                if (!Fight.UseOwnGear)
                {
                    AddBackground(300, 140, 150, 200, DarkBackground);
                    AddBackground(310, 150, 130, 180, LightBackground);

                    var pref = ForcedGear.GetPreference(User);
                    var entry = ForcedGear.GetEntry(pref);

                    AddHtml(300, 155, 150, 20, Center(String.Format("Gear: {0}", pref.ToString())), false, false);

                    if (entry != null)
                    {
                        Rectangle2D b = ItemBounds.Table[entry.ItemID];
                        AddItem(375 - b.Width / 2 - b.X, 240 - b.Height / 2 - b.Y, entry.ItemID);

                        var display = ForcedGear.GetDisplay(pref);

                        if (display != null)
                        {
                            AddItemProperty(display);
                        }
                    }

                    AddButton(376, 308, 4005, 4007, 9, GumpButtonType.Reply, 0);
                    AddButton(346, 308, 4014, 4016, 10, GumpButtonType.Reply, 0);
                }
            }

            AddButton(80, 454, 2450, 2452, 7, GumpButtonType.Reply, 0);  //Okay
            AddButton(160, 454, 2453, 2455, 0, GumpButtonType.Reply, 0); //Cancel
        }

        public override void OnResponse(RelayInfo button)
        {
            if (System == null || button.ButtonID == 0)
                return;

            HandleText(button);
            HandleSwitch(button);
 
            string duration;
            string wager;

            if (string.IsNullOrEmpty(Text3))
                duration = "10";
            else
                duration = Text3;

            bool validDuration = ValidateDuration(duration);

            if (!string.IsNullOrEmpty(Text4))
                wager = Text4;
            else
                wager = Fight.Wager.ToString();

            int wage = Utility.ToInt32(wager);

            if (wage > 0 && Fight.Wager != wage)
            {
                Fight.Wager = wage;
            }

            switch (button.ButtonID)
            {
                default: break;
                case 1: //New Opponent Target
                    {
                        User.Target = new InternalTarget(System, Fight, User, true, false);
                        User.SendMessage("Target the person who belongs to the team you want to fight.");
                        break;
                    }
                case 2:
                case 3:
                case 4: //Team Size Text
                    {
                        ArenaTeamType original = Fight.FightType;
 
                        switch (button.ButtonID)
                        {
                            default: User.SendMessage("Arena Teams can only consist of 1, 2 or 4 players."); break;
                            case 2: Fight.FightType = ArenaTeamType.Single; break;
                            case 3: Fight.FightType = ArenaTeamType.Twosome; break;
                            case 4: Fight.FightType = ArenaTeamType.Foursome; break;
                        }
 
                        ArenaTeam team = ArenaTeam.GetTeam(User, Fight.FightType);
 
                        if (Fight.TeamB != null && Fight.TeamB.TeamType != Fight.FightType)
                            Fight.TeamB = null;
 
                        if (team == null)
                        {
                            Fight.FightType = original;
                            User.SendMessage("You cannot register a fight when you don't belong to that type of team!");
                        }
                        else if (!team.Active)
                        {
                            Fight.FightType = original;
                            User.SendMessage("You cannot register a fight if the team you belong to is not active!");
                        }
                        else
                        {
                            User.SendMessage("You have selected a new team size!");
                            Fight.TeamA = team;
                        }

                        Refresh();
                        break;
                    }
                case 6: //No Rules 
                    {
                        break;
                    }
                case 7: //Register
                    {
                        if (Fight.ArenaFightType == ArenaFightType.LastManStanding)
                        {
                            ArenaTeam ourTeam = ArenaTeam.GetTeam(User, ArenaTeamType.Single, true);

                            Fight.FightType = ArenaTeamType.Single;
                            Fight.FightDuration = System.DefaultDuration;
                            Fight.TeamA = ourTeam;

                            if (!ArenaHelper.CheckWager(ourTeam, Fight.Wager))
                            {
                                User.SendMessage(0x23, "Your account lacks sufficient funds to match the gold wager of this fight.");
                                return;
                            }

                            if (!validDuration)
                            {
                                User.SendMessage(0x23, "You have entered an invalid amount. Choose the amount of time (in minutes) the fight will last, from 0 to 30 minutes.");
                                return;
                            }

                            var eligables = new List<Mobile>();
                            IPooledEnumerable eable = User.Map.GetMobilesInRange(User.Location, 15);

                            foreach (Mobile m in eable)
                            {
                                if (!m.Alive)
                                    continue;

                                ArenaTeam theirTeam = ArenaTeam.GetTeam(m, ArenaTeamType.Single, true);

                                if (theirTeam != null && theirTeam != ourTeam && m.NetState != null && !ActionTimer.AwaitingAction(m))
                                {
                                    eligables.Add(m);
                                }
                            }

                            eable.Free();

                            if (eligables.Count + 1 >= System.MinLastManStanding)
                            {
                                foreach (Mobile m in eligables)
                                {
                                    if (m is PlayerMobile)
                                    {
                                        ArenaTeam theirTeam = ArenaTeam.GetTeam(m, Fight.FightType);
                                        BaseGump.SendGump(new ConfirmFightGump(System, Fight, m as PlayerMobile, User));
                                    }
                                }

                                new LastManStandingActionTimer(User, eligables, TimeSpan.FromSeconds(60), Fight);
                                User.SendMessage("You have challenged {0} teams to a last man standing fight!", eligables.Count);
                            }
                            else
                            {
                                User.SendMessage(0x23, "Last man standing requires at least {0} arena teams in the immediate area to participate.", System.MinLastManStanding.ToString());
                            }
                        }
                        else if (Fight.ArenaFightType == ArenaFightType.TeamRumbleCaptureTheFlag)
                        {
                            var ctf = System as CTFArena;

                            if (ctf != null)
                            {
                                if (ActionTimer.AwaitingAction(User))
                                {
                                    User.SendMessage("You must wait a few moments.");
                                }
                                else if (!ArenaHelper.CheckAvailable(User))
                                {
                                    User.SendMessage("you are already fighting or are in queue to fight.");
                                }
                                else if (Fight.RemoveWager(User))
                                {
                                    ctf.RegisterTeamRumbleCTF(User, Fight as TeamRumbleCaptureTheFlag);
                                }
                                else
                                {
                                    User.SendMessage("You don't have enough funds in your bank account to start!");
                                }
                            }
                        }
                        else
                        {
                            if (Fight.TeamB == null)
                                User.SendMessage(0x23, "You haven't picked a valid opponent!");
                            else if (Fight.TeamB.TeamLeader == null || Fight.TeamB.TeamLeader.NetState == null)
                                User.SendMessage(0x23, "Your opponents team leader must be available to fight.");
                            else if (!ArenaHelper.CanRecievePoints(Fight.TeamA, Fight.TeamB) && Server.TournamentSystem.Config.RestrictSameIP)
                                User.SendMessage(0x23, "You cannot fight that opponent as one or more of thier fighters is one of your own!");
                            else if (Fight.TeamB.TeamLeader != null && Fight.TeamB.TeamLeader.Region != null && !Fight.TeamB.TeamLeader.Region.IsPartOf(System.AudienceRegion))
                                User.SendMessage(0x23, "Your opponents team leader must be in the area to ask for a fight.");
                            else if (Fight.FightType == ArenaTeamType.None)
                                User.SendMessage(0x23, "You have not selected team sizes yet!");
                            else if (Fight.TeamA == null || Fight.TeamA.TeamType != Fight.FightType)
                                User.SendMessage(0x23, "Make sure you belong to an arena team that supports this type of fight.");
                            else if (!ArenaHelper.CheckWager(Fight.TeamA, Fight.Wager))
                                User.SendMessage(0x23, "Your team lacks sufficient funds to match the gold wager of this fight.");
                            else if (!ArenaHelper.CheckWager(Fight.TeamB, Fight.Wager))
                                User.SendMessage(0x23, "Your opponent will be unable to meet that gold wager.");
                            else if (validDuration)
                            {
                                Mobile opp = Fight.TeamB.GetLeadership(); //Just incase!

                                if (opp != null && opp is PlayerMobile && !ActionTimer.AwaitingAction(opp))
                                {
                                    new ActionTimer(User, opp, TimeSpan.FromSeconds(60));
                                    BaseGump.SendGump(new ConfirmFightGump(System, Fight, opp as PlayerMobile, User));

                                    User.SendMessage("Please wait while your opponent agrees to the terms of the fight.");
                                    break;
                                }
                                else
                                {
                                    User.SendMessage(0x23, "That team is not eligable at this time. Try again soon!");
                                }
                            }

                            Refresh();
                        }
                    }
                    break;
                case 8:
                    if (Fight.UseOwnGear)
                    {
                        Fight.UseOwnGear = false;
                    }
                    else
                    {
                        Fight.UseOwnGear = true;
                    }

                    Refresh();
                    break;
                case 9:
                    var pref = ForcedGear.GetPreference(User);

                    if (pref == ForcedGearType.Dexxer)
                    {
                        pref = ForcedGearType.Caster;
                    }
                    else
                    {
                        pref++;
                    }

                    ForcedGear.SetPreference(User, pref);

                    Refresh();
                    break;
                case 10:
                    var preference = ForcedGear.GetPreference(User);

                    if (preference == ForcedGearType.Caster)
                    {
                        preference = ForcedGearType.Dexxer;
                    }
                    else
                    {
                        preference--;
                    }

                    ForcedGear.SetPreference(User, preference);

                    Refresh();
                    break;
            }
        }

        public void HandleText(RelayInfo info)
        {
            TextRelay relay = info.GetTextEntry(3);

            if (relay != null && !string.IsNullOrEmpty(relay.Text))
            {
                Text3 = relay.Text.Trim();
            }

            relay = info.GetTextEntry(4);

            if (relay != null && !string.IsNullOrEmpty(relay.Text))
            {
                Text4 = relay.Text.Trim();
            }
        }

        public void HandleSwitch(RelayInfo info)
        {
            if (info.IsSwitched(1))
            {
                Fight.AddToRules(FightRules.PureMage);
            }
            else
            {
                Fight.RemoveRules(FightRules.PureMage);
            }

            if (info.IsSwitched(2))
            {
                Fight.AddToRules(FightRules.PureDexxer);
            }
            else
            {
                Fight.RemoveRules(FightRules.PureDexxer);
            }

            if (info.IsSwitched(3))
            {
                Fight.AddToRules(FightRules.NoPots);
            }
            else
            {
                Fight.RemoveRules(FightRules.NoPots);
            }

            if (info.IsSwitched(4))
            {
                Fight.AddToRules(FightRules.NoPrecasting);
            }
            else
            {
                Fight.RemoveRules(FightRules.NoPrecasting);
            }

            if (info.IsSwitched(5))
            {
                Fight.AddToRules(FightRules.NoSummons);
            }
            else
            {
                Fight.RemoveRules(FightRules.NoSummons);
            }

            if (info.IsSwitched(6))
            {
                Fight.AddToRules(FightRules.NoSpecials);
            }
            else
            {
                Fight.RemoveRules(FightRules.NoSpecials);
            }

            if (info.IsSwitched(7))
            {
                Fight.AddToRules(FightRules.AllowResurrections);
            }
            else
            {
                Fight.RemoveRules(FightRules.AllowResurrections);
            }

            if (info.IsSwitched(8))
            {
                Fight.AddToRules(FightRules.AllowMounts);
            }
            else
            {
                Fight.RemoveRules(FightRules.AllowMounts);
            }

            if (info.IsSwitched(9))
            {
                Fight.AddToRules(FightRules.NoTies);
            }
            else
            {
                Fight.RemoveRules(FightRules.NoTies);
            }

            if (info.IsSwitched(10))
            {
                Fight.AddToRules(FightRules.NoAreaSpells);
            }
            else
            {
                Fight.RemoveRules(FightRules.NoAreaSpells);
            }

            if (info.IsSwitched(11))
            {
                Fight.AddToRules(FightRules.AllowPets);
            }
            else
            {
                Fight.RemoveRules(FightRules.AllowPets);
            }
        }

        private bool ValidateDuration(string duration)
        {
            int amount = Utility.ToInt32(duration);
 
            if (amount > 0 && amount <= 30)
            {
                if (Fight.ArenaFightType == ArenaFightType.CaptureTheFlag && amount < 10)
                {
                    User.SendMessage("CTF must be at least 10 minutes.");
                    amount = 10;
                }

                Fight.FightDuration = TimeSpan.FromMinutes(amount);

                return true;
            }
            else if (amount == 0)
            {
                Fight.FightDuration = System.DefaultDuration;
                return true;
            }
                
            return false;
        }
 
        public class InternalTarget : Target
        {
            public ArenaFight Fight { get; set; }
            public PVPTournamentSystem System { get; set; }
            public PlayerMobile From { get; set; }
            public bool PickOP { get; set; }
            public bool QuickFight { get; set; }
 
            public InternalTarget(PVPTournamentSystem sys, ArenaFight fight, PlayerMobile from, bool pickOpponent, bool quickfight) : base(12, false, TargetFlags.None)
            {
                Fight = fight;
                System = sys;
                From = from;
                PickOP = pickOpponent;
                QuickFight = quickfight;

                CheckLOS = false;
            }
 
            protected override void OnTarget(Mobile from, object targeted)
            {
                if (PickOP)
                {
                    if (targeted is PlayerMobile)
                    {
                        Mobile target = (Mobile)targeted;
                        Mobile keeper = System.ArenaKeeper;
                        ArenaTeam tarTeam = ArenaTeam.GetTeam(target, Fight.FightType, QuickFight);
                        ArenaTeam fromTeam = ArenaTeam.GetTeam(from, Fight.FightType, QuickFight);

                        if (keeper == null || !keeper.InRange(target.Location, 25))
                            From.SendMessage("They must be within a reasonable distance from the Arena Keeper to target them as an opponent.");
                        else if (!target.Alive)
                            From.SendMessage("You cannot challenge them, they are dead!");
                        else if (tarTeam == null)
                        {
                            From.SendMessage("The targeted player does not belong to a {0} arena team.", Fight.FightType);
                            target.SendMessage("You have declined a {0} arena match since you are not a member of that type of arena team.", Fight.FightType);
                        }
                        else if (fromTeam == null)
                            From.SendMessage("You do not belong to a {0} arena team!", Fight.FightType);
                        else if (tarTeam == fromTeam)
                            From.SendMessage("You cannot fight your own team!");
                        else if (tarTeam.TeamLeader != null && ActionTimer.AwaitingAction(tarTeam.TeamLeader))
                            From.SendMessage("They are already pending a fight. Try again in a moment.");
                        else if (!ArenaHelper.CheckAvailable(tarTeam))
                            From.SendMessage("They are not available to fight.");
                        else if (!ArenaHelper.CheckWager(tarTeam, Fight.Wager))
                            From.SendMessage("Your opponent will be unable to meet that gold wager.");
                        else if (!ArenaHelper.CheckWager(fromTeam, Fight.Wager))
                            From.SendMessage("Your team will be unable to meet that gold wager.");
                        else if (!TeamsInRange(keeper, fromTeam, tarTeam, 30))
                            From.SendMessage("Make sure both teams have thier players within a reasonable distance from the arena keeper. You cannot proceed unless the teams are even and they are all alive.");
                        else
                        {
                            if (Fight.TeamA == null)
                                Fight.TeamA = fromTeam;

                            Fight.TeamB = tarTeam;

                            if (QuickFight)
                            {
                                Mobile opp = Fight.TeamB.GetLeadership(); //Just incase!

                                if (opp != null && opp is PlayerMobile && !ActionTimer.AwaitingAction(opp))
                                {
                                    new ActionTimer(From, opp, TimeSpan.FromSeconds(30));
                                    BaseGump.SendGump(new ConfirmFightGump(System, Fight, opp as PlayerMobile, From));
                                    From.SendMessage("Please wait while your opponent agrees to the terms of the fight.");
                                }
                                else
                                {
                                    From.SendMessage(0x23, "That team is not eligable at this time. Try again soon!");
                                }

                                return;
                            }
                        }
                    }
                    else
                        From.SendMessage("You can only choose players to fight in the arena!");
                }
                else
                {
                }

                if(from is PlayerMobile && !QuickFight)
                    BaseGump.SendGump(new RegisterFightGump(System, Fight, from as PlayerMobile));
            }

            public static bool TeamsInRange(IEntity e, ArenaTeam a, ArenaTeam b, int range)
            {
                var count1 = 0;
                var count2 = 0;

                IPooledEnumerable eable = e.Map.GetMobilesInRange(e.Location, range);

                foreach (Mobile mob in eable)
                {
                    if (a.IsInTeam(mob) && mob.NetState != null && mob.Alive)
                        count1++;

                    if (b.IsInTeam(mob) && mob.NetState != null && mob.Alive)
                        count2++;
                }

                eable.Free();

                return count1 == count2;
            }
 
            protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
            {
                if(!QuickFight && from is PlayerMobile)
                    BaseGump.SendGump(new RegisterFightGump(System, Fight, from as PlayerMobile));

            }
        }
    }
 
    public class ConfirmFightGump : BaseTournamentGump
    {
        public PVPTournamentSystem System { get; set; }
        public ArenaFight Fight { get; set; }
        public PlayerMobile Challenger { get; set; }

        public ForcedGearType GearType { get; set; }

        public Action<PVPTournamentSystem, ArenaFight, PlayerMobile, PlayerMobile> AcceptCallback { get; set; }
        public Action<PVPTournamentSystem, ArenaFight, PlayerMobile, PlayerMobile> DeclineCallback { get; set; }

        public ConfirmFightGump(PVPTournamentSystem sys, ArenaFight fight, PlayerMobile user, PlayerMobile challenger,
            Action<PVPTournamentSystem, ArenaFight, PlayerMobile, PlayerMobile> accept = null,
            Action<PVPTournamentSystem, ArenaFight, PlayerMobile, PlayerMobile> decline = null)
            : base(user, 20, 20)
        {
            System = sys;
            Fight = fight;
            Challenger = challenger;
            Closable = false;

            AcceptCallback = accept;
            DeclineCallback = decline;
        }

        public override void AddGumpLayout()
        {
            string amend = "";

            
            if (!Fight.UseOwnGear)
            {
                if (Fight.League != null)
                {
                    amend = String.Format("Sua liga faz com que voce tenha que usar equipamentos padroes. Apenas armas poderam ser usadas..<br><br>");
                }
                else
                {
                    amend = String.Format("{0} Este estilo nao permite usar suas armaduras, apenas armas.<br><br>", Challenger.Name);
                }
            }

            if (Fight.Wager > 0)
            {
                amend = String.Format("Para desafiar, {0} perde {0} moedas de sua conta, as quais serao retiradas ao acertar o duelo.", Fight.FightType == ArenaTeamType.Single ? "voce" : "todos participantes", Fight.Wager.ToString("N0"));
            }

            string text;

            if (Fight.League != null)
            {
                text = String.Format("{0} desafio seu time na liga {1}. Veja as regras e " +
                    "decida se quer aceitar. {2}", Challenger != null ? Challenger.Name : "Alguem", Fight.League.Name, amend);
            }
            else
            {
                text = String.Format("{0} desafiou seu time na arena {1}. Veja as regras e " +
                "decida se deseja aceitar. {2}", Challenger != null ? Challenger.Name : "Alguem", System.Name, amend);
            }

            AddPage(0);
            AddBackground(0, 0, 300, 480, DarkBackground);
            AddBackground(10, 10, 280, 200, LightBackground);
            AddBackground(10, 220, 280, 60, LightBackground);
            AddBackground(10, 290, 280, 160, LightBackground);

            AddHtml(23, 18, 254, 102, text, true, true);

            AddLabel(15, 125, 0, "Challenger:");
            AddLabel(15, 145, 0, "Team Size:");
            AddLabel(15, 165, 0, "Fight Duration:");
            AddLabel(15, 185, 0, "Fight Type:");

            AddLabel(130, 125, 0, Fight.TeamA.Name);
            AddLabel(130, 145, 0, Fight.FightType.ToString());
            AddLabel(130, 165, 0, String.Format("{0} {1}", Fight.FightDuration != TimeSpan.Zero ? Fight.FightDuration.TotalMinutes.ToString() : "30", Fight.FightDuration.TotalMinutes == 1 ? "minute" : "minutes"));
            AddLabel(130, 185, 0, ArenaHelper.GetFightType(Fight.ArenaFightType));

            AddHtml(0, 220, 300, 20, Center("Gold Wager"), false, false);
            AddLabel(15, 240, 0, Fight.Wager.ToString("N0") + " gold");
            
            AddHtml(0, 290, 300, 20, Center("Rules"), false, false);

            if (Fight.Rules > 0)
            {
                int yStart = 310;
                int idx = 0;

                foreach (int i in Enum.GetValues(typeof(FightRules)))
                {
                    if (!Fight.HasRule((FightRules)i))
                        continue;

                    if (idx % 2 == 0)
                        AddLabel(15, yStart, 0, ArenaHelper.GetRules((FightRules)i));
                    else
                    {
                        AddLabel(150, yStart, 0, ArenaHelper.GetRules((FightRules)i));
                        yStart += 25;
                    }

                    idx++;
                }
            }

            if (!Fight.UseOwnGear)
            {
                AddBackground(300, 140, 150, 200, DarkBackground);
                AddBackground(310, 150, 130, 180, LightBackground);

                var pref = ForcedGear.GetPreference(User);
                var entry = ForcedGear.GetEntry(pref);

                AddHtml(300, 155, 150, 20, Center(String.Format("Gear: {0}", pref.ToString())), false, false);

                if (entry != null)
                {
                    Rectangle2D b = ItemBounds.Table[entry.ItemID];
                    AddItem(375 - b.Width / 2 - b.X, 240 - b.Height / 2 - b.Y, entry.ItemID);

                    var display = ForcedGear.GetDisplay(pref);

                    if (display != null)
                    {
                        AddItemProperty(display);
                    }
                }

                AddButton(376, 308, 4005, 4007, 2, GumpButtonType.Reply, 0);
                AddButton(346, 308, 4014, 4016, 3, GumpButtonType.Reply, 0);
            }

            AddHtml(43, 454, 100, 20, Color("#FFFFFF", "Accept"), false, false);
            AddButton(10, 452, 4023, 4024, 1, GumpButtonType.Reply, 0);

            AddHtml(157, 454, 100, 20, ColorAndAlignRight("#FFFFFF", "Decline"), false, false);
            AddButton(260, 452, 4017, 4018, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(RelayInfo info)
        {
            PlayerMobile from = User;

            switch (info.ButtonID)
            {
                default:
                case 0: // Decline
                    {
                        if (DeclineCallback != null)
                        {
                            DeclineCallback(System, Fight, User, Challenger);
                            ActionTimer.EndAction(from);
                        }
                        else
                        {
                            if (Fight.ArenaFightType == ArenaFightType.LastManStanding)
                            {
                                LastManStandingActionTimer.OnGumpSelection(from);
                            }
                            else if (Fight.League == null)
                            {
                                BaseGump.SendGump(new RegisterFightGump(System, Fight, Challenger));
                                ActionTimer.EndAction(from);
                            }

                            if (Fight.ArenaFightType != ArenaFightType.TeamRumbleCaptureTheFlag)
                            {
                                Challenger.SendMessage("{0} did not accept your challenge.", from.Name);
                                from.SendMessage("You decline the challenge.");
                            }
                        }
                        break;
                    }
                case 1: // Accept
                    {
                        if (AcceptCallback != null)
                        {
                            AcceptCallback(System, Fight, User, Challenger);
                            ActionTimer.EndAction(from);
                        }
                        else
                        {
                            if (Fight.ArenaFightType == ArenaFightType.LastManStanding)
                            {
                                ArenaTeam team = ArenaTeam.GetTeam(from, ArenaTeamType.Single);

                                if (team != null)
                                {
                                    if (!ArenaHelper.CheckWager(team, Fight.Wager))
                                    {
                                        from.SendMessage(0x23, "Your account lacks sufficient funds to match the gold wager of this fight.");
                                    }
                                    else
                                    {
                                        foreach (TeamInfo tInfo in Fight.Teams)
                                        {
                                            if (tInfo == null || tInfo.Team == null)
                                                continue;

                                            foreach (Mobile m in tInfo.Team.Fighters)
                                                m.SendMessage("{0} has joined the last man standing fight!", User.Name);
                                        }

                                        Fight.Teams.Add(new TeamInfo(team));
                                        LastManStandingActionTimer.OnGumpSelection(from);
                                    }
                                }
                            }
                            else if (Fight.Wager <= 0 || ArenaHelper.CheckWager(Fight.TeamB, Fight.Wager))
                            {
                                Fight.RegisterFight();
                                Challenger.SendMessage("{0} has accepted your challenge! Prepare for battle!", from.Name);
                                from.SendMessage("You accept the challenge. Prepare for battle!");
                                ActionTimer.EndAction(from);
                            }
                            else
                            {
                                Challenger.SendMessage(0x23, "Your oppoents lack sufficient funds to match the gold wager.");
                                from.SendMessage(0x23, "Your account lacks sufficient funds to match the gold wager.");
                            }
                        }

                        break;
                    }
                case 2:
                    var pref = ForcedGear.GetPreference(User);

                    if (pref == ForcedGearType.Dexxer)
                    {
                        pref = ForcedGearType.Caster;
                    }
                    else
                    {
                        pref++;
                    }

                    ForcedGear.SetPreference(User, pref);

                    Refresh();
                    break;
                case 3:
                    var preference = ForcedGear.GetPreference(User);

                    if (preference == ForcedGearType.Caster)
                    {
                        preference = ForcedGearType.Dexxer;
                    }
                    else
                    {
                        preference--;
                    }

                    ForcedGear.SetPreference(User, preference);

                    Refresh();
                    break;
            }
        }
    }
}
