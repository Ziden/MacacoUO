using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using Server;
using Server.Mobiles;
using Server.Gumps;
using Server.Items;

namespace Server.TournamentSystem
{
    public class FightSelectGump : BaseGump
    {
        public List<League> Leagues { get; set; }
        public TournamentStone Stone { get; set; }

        public FightSelectGump(PlayerMobile pm, List<League> leagues, TournamentStone stone)
            : base(pm, 0, 0)
        {
            Leagues = leagues;
            Stone = stone;
        }

        public override void AddGumpLayout()
        {
            AddBackground(0, 0, 500, 200 + (Leagues.Count * 22), 3500);
            AddHtml(0, 15, 500, 20, Center("Available League Matches"), false, false);

            AddLabel(45, 40, 0, "League");
            AddLabel(200, 40, 0, "Round");
            AddLabel(300, 40, 0, "Opponent");
            AddLabel(450, 40, 0, "Ready");

            if (Stone.System != null && Leagues.Count > 0)
            {
                int y = 80;

                for (var i = 0; i < Leagues.Count; i++)
                {
                    var league = Leagues[i];
                    var match = league.GetCurrentMatch(User);
                    var opponent = match.Team1.IsInTeam(User) ? match.Team2 : match.Team1;

                    AddLabelCropped(45, y, 190, 20, 0, league.Name);
                    AddLabel(200, y, 0, league.EliminationPeriod ? League.EliminationRoundToString(league.GetEliminationSize(), league.Round) : league.Round.ToString());
                    AddLabelCropped(300, y, 150, 20, 0, opponent.Name);

                    var reason = string.Empty;
                    var canFight = CanFight(match, opponent, ref reason);

                    AddLabel(450, y, canFight ? 63 : 33, canFight ? "Yes" : "No");

                    if (canFight)
                    {
                        AddButton(10, y, 4005, 4007, i + 100, GumpButtonType.Reply, 0);
                        AddTooltip(Localizations.GetLocalization(74));
                    }
                    else
                    {
                        AddImage(15, y + 3, 0x5689);
                        AddTooltip(reason);
                    }

                    y += 22;
                }
            }
        }

        private bool CanFight(RoundMatch match, ArenaTeam opponent, ref string reason)
        {
            bool canFight = RegisterFightGump.InternalTarget.TeamsInRange(User, match.Team1, match.Team2, 20)
                        && match.PendingFight
                        && ArenaHelper.CheckAvailable(match.Team1)
                        && ArenaHelper.CheckAvailable(match.Team2)
                        && !ActionTimer.WaitingAction.ContainsKey(match.Team1.TeamLeader)
                        && !ActionTimer.WaitingAction.ContainsKey(match.Team2.TeamLeader);

            if (!match.PendingFight)
            {
                reason = Localizations.GetLocalization(76);
            }
            else if (!ArenaHelper.CheckAvailable(match.Team1))
            {
                reason = String.Format(Localizations.GetLocalization(77), match.Team1 == opponent ? "They" : "You");
            }
            else if (!ArenaHelper.CheckAvailable(match.Team2))
            {
                reason = String.Format(Localizations.GetLocalization(77), match.Team2 == opponent ? "They" : "You");
            }
            else if (ActionTimer.WaitingAction.ContainsKey(match.Team1.TeamLeader))
            {
                reason = String.Format(Localizations.GetLocalization(73), opponent == match.Team1 ? "They" : "You");
            }
            else if (ActionTimer.WaitingAction.ContainsKey(match.Team2.TeamLeader))
            {
                reason = String.Format(Localizations.GetLocalization(73), opponent == match.Team2 ? "They" : "You");
            }
            else if (!canFight)
            {
                reason = Localizations.GetLocalization(78);
            }

            return canFight;
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID >= 100)
            {
                var id = info.ButtonID - 100;

                if (id >= 0 && id < Leagues.Count)
                {
                    var league = Leagues[id];
                    var match = league.GetCurrentMatch(User);
                    var opponent = match.Team1.IsInTeam(User) ? match.Team2 : match.Team1;

                    var fight = league.ConstructFight(match, Stone.System);

                    if (!fight.UseOwnGear)
                    {
                        BaseGump.SendGump(new ChooseOwnGearGump(User, fight, (m, f) =>
                        {
                            var reason = string.Empty;

                            if (CanFight(match, opponent, ref reason))
                            {
                                new ActionTimer(User, opponent.GetLeadership(), TimeSpan.FromSeconds(60));
                                BaseGump.SendGump(new ConfirmFightGump(Stone.System, fight, (PlayerMobile)opponent.GetLeadership(), User));

                                m.SendMessage("Please wait while your opponent agrees to the terms of the fight.");
                            }
                            else
                            {
                                m.SendMessage(0x22, reason);
                            }
                        }));

                    }
                    else
                    {
                        new ActionTimer(User, opponent.GetLeadership(), TimeSpan.FromSeconds(60));
                        BaseGump.SendGump(new ConfirmFightGump(Stone.System, fight, (PlayerMobile)opponent.GetLeadership(), User));

                        User.SendMessage("Please wait while your opponent agrees to the terms of the fight.");
                    }
                }
            }
        }
    }
}
