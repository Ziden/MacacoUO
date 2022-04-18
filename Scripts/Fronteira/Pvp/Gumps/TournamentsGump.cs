using System;
using System.Collections.Generic;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Gumps;
using Server.Prompts;

namespace Server.TournamentSystem
{
    public class TournamentsGump : BaseTournamentGump
    {
        private PVPTournamentSystem System { get; set; }
        private List<Tournament> UseList { get; set; }

        public TournamentsGump(PVPTournamentSystem system, PlayerMobile from) : base (from, 20, 20)
        {
            System = system;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);

            bool showAll = System == null;

            AddBackground(0, 0, showAll ? 500 : 350, 550, DarkBackground);
            AddBackground(showAll ? 162 : 88, 10, 175, 30, LightBackground);
            AddBackground(10, 55, showAll ? 480 : 330, 485, LightBackground);

            //AddHtml(0, 14, showAll ? 500 : 350, 20, ColorAndCenter("#FFFFFF", "Upcoming Tournaments"), false, false);
            AddHtml(0, 14, showAll ? 500 : 350, 20, ColorAndCenter("#FFFFFF", "Torneios Futuros"), false, false);
            /*AddLabel(65, 60, LabelHue, "Tournament");
            AddLabel(225, 60, LabelHue, "Date");
            AddLabel(20, 60, LabelHue, "Info");*/
            AddLabel(65, 60, LabelHue, "Torneio");
            AddLabel(225, 60, LabelHue, "Data");
            AddLabel(20, 60, LabelHue, "Info");

            int yOffset = 90;

            if (!showAll)
            {
                System.Tournaments.Sort();

                UseList = new List<Tournament>(System.Tournaments);
                UseList.Reverse();

                for (int i = 0; i < UseList.Count; ++i)
                {
                    Tournament t = UseList[i];
                    AddButton(20, yOffset, 4005, 4007, i + 1, GumpButtonType.Reply, 0); //Tourney Info
                    AddHtml(67, yOffset, 150, 16, String.Format("{0}", t.Name), false, false);
                    AddHtml(227, yOffset, 100, 16, String.Format("{0}", t.StartTime.ToString()), false, false);
                    yOffset += 30;
                }
            }
            else
            {
                AddLabel(350, 60, LabelHue, "Arena");

                foreach (var sys in PVPTournamentSystem.SystemList)
                {
                    if (UseList == null)
                        UseList = new List<Tournament>();

                    UseList.AddRange(sys.Tournaments);
                }

                UseList.Sort();

                for (int i = 0; i < UseList.Count; ++i)
                {
                    Tournament t = UseList[i];
                    AddButton(20, yOffset, 4005, 4007, i + 1, GumpButtonType.Reply, 0); //Tourney Info

                    if (Cooldown == null || !Cooldown.ContainsKey(User) || User.AccessLevel > AccessLevel.VIP)
                    {
                        AddButton(320, yOffset, 4014, 4016, i + 101, GumpButtonType.Reply, 0);
                        //AddTooltip(String.Format("Move to {0}", t.System.Name));
                        AddTooltip(String.Format("Mover para {0}", t.System.Name));
                    }

                    AddHtml(67, yOffset, 150, 16, t.Name, false, false);
                    AddHtml(227, yOffset, 100, 16, t.StartTime.ToString(), false, false);
                    AddHtml(352, yOffset, 150, 16, t.System.Name, false, false);
                    yOffset += 30;
                }
            }
        }

        public override void OnDispose()
        {
            if (UseList != null)
                ColUtility.Free(UseList);
        }

        public override void OnResponse(RelayInfo button)
        {
            int id = button.ButtonID;

            if (id > 100)
            {
                if (id - 101 >= 0 && id - 101 < UseList.Count)
                {
                    var t = UseList[id - 101];

                    if (t.System != null)
                    {
                        //BaseGump.SendGump(new ConfirmCallbackGump(User, String.Format("Move to {0} [{1}]?", t.System.Name, t.System.ArenaMap.ToString()), "Would you like to move to the arena where this tournament is being held?", null,
                        BaseGump.SendGump(new ConfirmCallbackGump(User, String.Format("Mover para {0} [{1}]?", t.System.Name, t.System.ArenaMap.ToString()), "Você gostaria de ir para a arena onde este torneio está sendo realizado?", null,
                            confirm:
                            (m, obj) =>
                            {
                                var p = t.System.GetRandomKickLocation();
                                var map = t.System.ArenaMap;

                                if (m.Criminal)
                                {
                                    m.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
                                }
                                else if (m.Murderer && map.Rules != MapRules.FeluccaRules)
                                {
                                    m.SendLocalizedMessage(1019004); // You are not allowed to travel there.
                                }
                                else if (Server.Spells.SpellHelper.CheckCombat(m))
                                {
                                    m.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
                                }
                                else if (Server.Misc.WeightOverloading.IsOverloaded(m))
                                {
                                    m.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
                                }
                                else if (Server.Engines.CityLoyalty.CityTradeSystem.HasTrade(m))
                                {
                                    m.SendLocalizedMessage(1151733); // You cannot do that while carrying a Trade Order.
                                }
                                else if (m.Holding != null)
                                {
                                    m.SendLocalizedMessage(1071955); // You cannot teleport while dragging an object.
                                }
                                else
                                {
                                    BaseCreature.TeleportPets(m, p, map, true);

                                    m.PlaySound(0x1FC);
                                    m.MoveToWorld(p, map);
                                    m.PlaySound(0x1FC);

                                    AddCooldown(m);
                                }
                            }));

                    }
                }
            }
            else if (id > 0)
            {
                if(id - 1 >= 0 && id - 1 < UseList.Count)
                {
                    var t = UseList[id - 1];

                    BaseGump.SendGump(new TournamentInfoGump(t.System, t, User));
                }
            }
        }

        public static Timer Timer { get; set; }
        public static Dictionary<Mobile, DateTime> Cooldown { get; set; }

        public static void AddCooldown(Mobile m)
        {
            if (Timer == null)
            {
                Timer = Timer.DelayCall(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), OnTick);
            }

            if (Cooldown == null)
            {
                Cooldown = new Dictionary<Mobile, DateTime>();
            }

            Cooldown[m] = DateTime.UtcNow + TimeSpan.FromMinutes(10);
        }

        public static void EndTimer()
        {
            if (Timer != null)
            {
                Timer.Stop();
                Timer = null;
            }
        }

        public static void OnTick()
        {
            if (Cooldown == null || Cooldown.Count == 0)
            {
                EndTimer();
            }

            List<Mobile> list = new List<Mobile>();

            foreach (var kvp in Cooldown)
            {
                if (kvp.Value < DateTime.UtcNow)
                {
                    list.Add(kvp.Key);
                }
            }

            foreach (var m in list)
            {
                Cooldown.Remove(m);
            }

            ColUtility.Free(list);

            if (Cooldown.Count == 0)
            {
                Cooldown = null;
                EndTimer();
            }
        }
    }

    public class TournamentInfoGump : BaseTournamentGump
    {
        private PVPTournamentSystem System { get; set; }
        private Tournament Tournament { get; set; }

        public TournamentInfoGump(PVPTournamentSystem sys, Tournament tourney, PlayerMobile from)
            : base(from, 20, 20)
        {
            System = sys;
            Tournament = tourney;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 350, 425, DarkBackground);
            AddBackground(75, 11, 200, 30, LightBackground);
            AddBackground(10, 54, 330, 360, LightBackground);
            AddLabel(123, 15, 0, "Tournament Info");

            AddPage(1);
            AddLabel(20, 60, LabelHue, "Tournament:");
            AddLabel(125, 60, 0, Tournament.Name);

            AddLabel(20, 80, LabelHue, "Date:");
            AddLabel(125, 80, 0, String.Format("{0}", Tournament.StartTime.ToString()));

            var tooltipString = String.Format("You {0} belong to a {1} arena team.", ArenaTeam.HasTeam(User, Tournament.FightType) ? "do" : "do not", Tournament.FightType.ToString());

            AddLabel(20, 100, LabelHue, "Teams:");
            AddTooltip(tooltipString);
            AddLabel(125, 100, 0, String.Format("{0}", Tournament.FightType));
            AddTooltip(tooltipString);

            AddLabel(20, 120, LabelHue, "Entry Fee:");
            AddLabel(125, 120, 0, Tournament.EntryFee.ToString("N0"));

            var fType = Tournament.TourneyType;

            AddLabel(20, 140, LabelHue, "Fight Type:");
            AddLabel(125, 140, 0, String.Format(ArenaHelper.GetTourneyType(Tournament)));

            switch (fType)
            {
                default:
                case TourneyType.SingleElim: AddTooltip(Localizations.GetLocalization(44)); break;
                case TourneyType.DoubleElimination: AddTooltip(Localizations.GetLocalization(45), Localizations.GetLocalization(46)); break;
                case TourneyType.BestOf3: AddTooltip(Localizations.GetLocalization(47), Localizations.GetLocalization(48)); break;
                //case TourneyType.Bracketed: AddTooltip(Localizations.GetLocalization(49), Localizations.GetLocalization(50)); break;
            }

            AddLabel(20, 160, LabelHue, "Fight Duration:");
            AddLabel(125, 160, 0, String.Format("{0} minutes", Tournament.TimePerFight.TotalMinutes.ToString()));

            AddLabel(20, 180, LabelHue, "Alternate Arena:");
            AddLabel(125, 180, 0, Tournament.UseAlternateArena ? "Yes" : "No");

            if (Tournament.UseAlternateArena && System.LinkedSystem != null)
            {
                AddTooltip(String.Format("Alternate Arena: {0}", System.LinkedSystem.Name));
            }

            var type = Tournament.TourneyStyle;

            AddLabel(20, 200, LabelHue, "Tourney Type:");
            AddLabel(125, 200, 0, ArenaHelper.GetTourneyStyle(Tournament));

            switch (type)
            {
                case TourneyStyle.Standard: AddTooltip(Localizations.GetLocalization(41)); break;
                case TourneyStyle.MagesOnly: AddTooltip(Localizations.GetLocalization(42)); break;
                case TourneyStyle.DexxersOnly: AddTooltip(Localizations.GetLocalization(43)); break;
            }

            AddBackground(18, 218, 314, 120, LightBackground);

            if (Tournament.Rules > 0)
            {
                int yOffset = 220;
                int idx = 0;

                foreach (int i in Enum.GetValues(typeof(FightRules)))
                {
                    if (!Tournament.HasRule((FightRules)i))
                        continue;

                    if (idx % 2 == 0)
                        AddLabel(22, yOffset, 0, ArenaHelper.GetRules((FightRules)i));
                    else
                    {
                        AddLabel(150, yOffset, 0, ArenaHelper.GetRules((FightRules)i));
                        yOffset += 25;
                    }

                    idx++;
                }
            }

            if ((User == Tournament.Creator && Tournament.Participants.Count == 0) || User.AccessLevel >= AccessLevel.GameMaster)
            {
                AddButton(15, 342, 4002, 4003, 3, GumpButtonType.Reply, 0);
                AddLabel(48, 342, 0, "Cancel Tournament");
            }
            else
            {
                AddLabel(20, 342, LabelHue, String.Format("Current Time: {0}", DateTime.Now.ToShortTimeString()));
            }

            if (Tournament.Participants.Count > 0)
            {
                AddLabel(48, 364, 0, "View Participants");
                AddButton(15, 364, 4008, 4009, 0, GumpButtonType.Page, 4);
            }

            AddButton(15, 388, 4005, 4006, 0, GumpButtonType.Page, 2);
            AddLabel(48, 385, 0, "View Prizes");

            if (System.CurrentTournament != Tournament && User.AccessLevel > AccessLevel.Counselor && Config.Debug)
            {
                AddLabel(193, 342, 0, "Auto Pick Parts");
                AddButton(160, 342, 4005, 4006, 6, GumpButtonType.Reply, 0);
            }

            if (System.CurrentTournament != Tournament)
            {
                AddLabel(193, 364, 0, "Sponsor");
                AddButton(160, 364, 4005, 4006, 4, GumpButtonType.Reply, 0);
            }

            ArenaTeam team = ArenaTeam.GetTeam(User, Tournament.FightType);

            if (!Tournament.Participants.Contains(team) && System.CurrentTournament != Tournament)
            {
                AddLabel(193, 388, 0, "Join This Tournament");
                AddButton(160, 388, 4005, 4006, 1, GumpButtonType.Reply, 0);
            }
            else if (System.CurrentTournament != Tournament)
            {
                AddLabel(193, 388, 0, "Withdraw from Tourny");
                AddButton(160, 388, 4017, 4018, 2, GumpButtonType.Reply, 0);
            }
            else if (System.CurrentTournament == Tournament)
            {
                AddLabel(193, 388, 0, "Tournament Ongoing");
            }

            if (User.AccessLevel >= AccessLevel.GameMaster)
            {
                AddLabel(233, 197, 0, "View Props");
                AddButton(200, 197, 4011, 4012, 5, GumpButtonType.Reply, 0);
            }

            AddPage(2);

            if (Tournament.ChampPrize != null)
            {
                AddLabel(20, 60, LabelHue, "Champion Prize:");

                Item item = Tournament.ChampPrize;
                string amount = item.Amount.ToString();

                Rectangle2D b = ItemBounds.Table[item.ItemID];
                AddItem(70 - b.Width / 2 - b.X, 125 - b.Height / 2 - b.Y, item.ItemID, item.Hue);
                AddItemProperty(item);

                if (item.Name == null)
                {
                    AddHtmlLocalized(140, 60, 200, 15, item.LabelNumber, false, false);
                    AddLabel(140, 80, 0, String.Format("Amount: {0}", amount));
                }
                else
                    AddHtml(140, 60, 200, 80, item.Name + "<br>Amount: " + amount, false, false);
            }

            if (Tournament.RunnerUpPrize != null)
            {
                AddLabel(20, 170, LabelHue, "Runner Up Prize:");

                Item item2 = Tournament.RunnerUpPrize;
                string amount2 = item2.Amount.ToString();

                Rectangle2D b2 = ItemBounds.Table[item2.ItemID];
                AddItem(70 - b2.Width / 2 - b2.X, 250 - b2.Height / 2 - b2.Y, item2.ItemID, item2.Hue);
                AddItemProperty(item2);

                if (item2.Name == null)
                {
                    AddHtmlLocalized(140, 170, 200, 15, item2.LabelNumber, false, false);
                    AddLabel(140, 190, 0, String.Format("Amount: {0}", amount2));
                }
                else
                    AddHtml(140, 170, 200, 80, item2.Name + "<br>Amount: " + amount2, false, false);
            }

            AddLabel(20, 290, LabelHue, "Collection Pot: ");
            AddLabel(20, 310, LabelHue, "Sponsor Pot:");
            AddLabel(20, 330, LabelHue, "Total Pot:");

            int total = Tournament.GoldCollected + Tournament.SponsorTotal;
            AddLabel(140, 290, 0, Tournament.GoldCollected > 0 ? Tournament.GoldCollected.ToString("N0") : "0");
            AddLabel(140, 310, 0, Tournament.SponsorTotal > 0 ? Tournament.SponsorTotal.ToString("N0") : "0");
            AddLabel(140, 330, 0, total > 0 ? total.ToString("N0") : "0");

            if (Tournament.Sponsors.Count > 0)
            {
                AddLabel(53, 370, 0, "View Sponsors");
                AddButton(20, 370, 4005, 4006, 0, GumpButtonType.Page, 3);
            }

            AddHtml(195, 370, 100, 20, "<DIV ALIGN=RIGHT>Back</DIV>", false, false);
            AddButton(300, 370, 4014, 4015, 0, GumpButtonType.Page, 1);

            AddPage(3);

            AddHtml(195, 370, 100, 20, "<DIV ALIGN=RIGHT>Back</DIV>", false, false);
            AddButton(300, 370, 4014, 4015, 0, GumpButtonType.Page, 2);

            int xStart = 20;
            int yStart = 60;
            int index = 0;

            foreach (KeyValuePair<Mobile, int> sponsors in Tournament.Sponsors)
            {
                if (sponsors.Key == null)
                    continue;

                AddLabelCropped(xStart, yStart, 70, 16, LabelHue, sponsors.Key.Name);
                AddLabelCropped(xStart + 70, yStart, 70, 16, 0, sponsors.Value.ToString("###,###,###"));
                index++;

                if (index == 15)
                {
                    xStart = 180; yStart = 60;
                }
                else
                    yStart += 20;

                if (index >= 29)
                    break;
            }

            int perPage = 20;
            int pages = 3 + (int)Math.Ceiling(Tournament.Participants.Count / (double)perPage);
            int x = 20;
            int y = 60;
            int count = 0;
            ArenaTeam fightTeam = null;

            for (int i = 1; i <= pages; ++i)
            {
                AddPage(i + 3);
                x = 20;
                y = 60;

                for (int j = 0; j < perPage; ++j)
                {
                    if (count >= Tournament.Participants.Count)
                        break;

                    fightTeam = Tournament.Participants[count];
                    count++;

                    if (fightTeam == null)
                        continue;

                    if (j == perPage / 2)
                    {
                        x = 195;
                        y = 60;
                    }

                    AddLabel(x, y, 0, fightTeam.Name);

                    y += 30;
                }

                if (i + 3 < pages)
                    AddButton(315, 365, 2224, 2224, 0, GumpButtonType.Page, (i + 3) + 1);
                if (i + 3 > 4)
                    AddButton(285, 365, 2223, 2223, 0, GumpButtonType.Page, (i + 3) - 1);
            }
        }

        public override void OnResponse(RelayInfo button)
        {
            switch (button.ButtonID)
            {
                default:
                case 0:
                    BaseGump.SendGump(new TournamentsGump(System, User));
                    break;
                case 1:
                    {
                        if (System.CurrentTournament != Tournament)
                        {
                            Tournament.TryJoinTournament(User);
                            Refresh();
                        }
                        break;
                    }
                case 2:
                    {
                        if (System.CurrentTournament != Tournament)
                        {
                            Tournament.TryWithdrawTeam(User);
                            Refresh();
                        }
                        break;
                    }
                case 3:
                    {
                        if ((User == Tournament.Creator && Tournament.Participants.Count == 0) || User.AccessLevel > AccessLevel.VIP)
                            BaseGump.SendGump(new ConfirmCancelTournament(System, Tournament, User));
                        break;
                    }
                case 4:
                    {
                        if (System.CurrentTournament != Tournament)
                        {
                            User.Prompt = new InternalPrompt(System, Tournament, User);
                            User.SendMessage("Enter sponsor amount.  Make sure you have the funds in your bank or vault!");
                        }
                        break;
                    }
                case 5:
                    {
                        if(User.AccessLevel > AccessLevel.VIP)
                            User.SendGump(new PropertiesGump(User, Tournament));
                        break;
                    }
                case 6:
                    {
                        if (User.AccessLevel > AccessLevel.Counselor)
                        {
                            int cnt = 0;

                            //foreach(ArenaTeam team in ArenaTeam.Teams)
                            for(int i = 0; i < Config.MaxEntries; i++)
                            {
                                if (i >= ArenaTeam.Teams.Count)
                                    break;

                                var team = ArenaTeam.Teams[i];

                                if (team.TeamType == Tournament.FightType && !Tournament.Participants.Contains(team) && Tournament.Participants.Count < Tournament.MaxParticipants)
                                {
                                    cnt++;
                                    Tournament.GoldCollected += Tournament.EntryFee;
                                    Tournament.DoJoinTeam(team);
                                }

                                if (cnt >= Tournament.MaxParticipants)
                                    break;
                            }

                            User.SendMessage("Entered {0} teams in the tournament.", cnt);
                        }
                        break;
                    }
            }
        }

        private class InternalPrompt : Prompt
        {
            private PVPTournamentSystem Sys { get; set; }
            private Tournament Tournament { get; set; }
            private Mobile From { get; set; }

            public InternalPrompt(PVPTournamentSystem sys, Tournament t, Mobile f)
            {
                Sys = sys;
                Tournament = t;
                From = f;
            }

            public override void OnCancel(Mobile from)
            {
                if(From is PlayerMobile)
                    BaseGump.SendGump(new TournamentInfoGump(Sys, Tournament, From as PlayerMobile));
            }

            public override void OnResponse(Mobile from, string text)
            {

                if (Sys.CurrentTournament == Tournament)
                    From.SendMessage("The tournament is already in progress!");
                else
                {
                    int amount = 0;
                    try
                    {
                        amount = Convert.ToInt32(text);

                        if (amount < 5000)
                            from.SendMessage("You can sponsor this event with a minimum of 5,000 gold.");
                        else if (!Banker.Withdraw(from, amount, true))
                            from.SendMessage("Your Bank/Vault lack the funds to support this amount.");
                        else
                        {
                            Tournament.AddSponsor(From, amount);
                            from.SendMessage("You have sponsored the {0} Tournament with {1} gold!", Tournament.Name, amount.ToString("N0"));
                        }
                    }
                    catch
                    {
                        from.SendMessage("Invalid Amount.");
                    }
                    finally
                    {
                        if(From is PlayerMobile)
                            BaseGump.SendGump(new TournamentInfoGump(Sys, Tournament, From as PlayerMobile));
                    }
                }
            }
        }
    }

    public class ConfirmCancelTournament : BaseTournamentGump
    {
        private PVPTournamentSystem System { get; set; }
        private Tournament Tournament { get; set; }

        public ConfirmCancelTournament(PVPTournamentSystem sys, Tournament tourney, PlayerMobile from) : base(from, 20, 20)
        {
            System = sys;
            Tournament = tourney;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddAlphaRegion(20, 20, 20, 50);
            AddImage(0, 0, 2070);

            AddButton(100, 75, 2074, 2075, 1, GumpButtonType.Reply, 0); //Okay
            AddButton(35, 75, 2071, 2072, 0, GumpButtonType.Reply, 0);  //Cancel

            AddHtml(30, 24, 116, 40, "Cancel Tounament?", false, false);
        }

        public override void OnResponse(RelayInfo button)
        {
            switch (button.ButtonID)
            {
                default:
                case 0:
                    BaseGump.SendGump(new TournamentInfoGump(System, Tournament, User));
                    break;
                case 1:
                    if ((User == Tournament.Creator && Tournament.Participants.Count == 0) || User.AccessLevel >= AccessLevel.GameMaster)
                    {
                        Tournament.CancelTournament();
                        User.SendMessage("You have canceled the tournament!");
                    }
                    break;
            }
        }
    }
}
