using System;
using System.Linq;
using System.Collections.Generic;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class ParticipantGump : BaseGump
    {
        public ArenaFight Fight { get; set; }
        public bool Small { get; set; }

        public ParticipantGump(PlayerMobile pm, ArenaFight fight)
            : base(pm, 0, 40)
        {
            Fight = fight;
            Small = true;

            Closable = false;
        }

        public override void AddGumpLayout()
        {
            if (Fight == null || (Fight.System != null && Fight.System.CurrentFight != Fight))
            {
                User.CloseGump(typeof(ParticipantGump));
                return;
            }

            if (Small)
            {
                AddImage(0, 0, 10600);
                AddImage(75, 0, 10601);
                AddImage(132, 0, 10601);
                AddImage(189, 0, 10602);

                AddButton(215, 3, 4029, 4030, 1, GumpButtonType.Reply, 0);
            }
            else
            {
                var myTeam = Fight.GetTeam(User);
                var opponent = myTeam == Fight.TeamA ? Fight.TeamB : Fight.TeamA;

                if (myTeam != null && opponent != null)
                {
                    int rulesCount = 0;

                    foreach (int i in Enum.GetValues(typeof(FightRules)))
                    {
                        if (Fight.HasRule((FightRules)i))
                        {
                            rulesCount++;
                        }
                    }

                    int height = 20 + (Fight.GetFighters().Count() * 20) + (((rulesCount + 1) / 2) * 20);
                    height += rulesCount == 0 ? 160 : 140;

                    if (Fight is CaptureTheFlagFight)
                        height += 60;

                    AddBackground(0, 0, 264, height, 9380);

                    //AddLabel(20, 30, 0, String.Format("Opponent: {0}", Fight.ArenaFightType == ArenaFightType.LastManStanding ? "Everone" : opponent.Name));
                    AddLabel(20, 30, 0, String.Format("Oponente: {0}", Fight.ArenaFightType == ArenaFightType.LastManStanding ? "Todos" : opponent.Name));

                    int y = 60;

                    if (rulesCount == 0)
                    {
                        //AddLabel(20, y, 0, "No Rules");
                        AddLabel(20, y, 0, "Sem Regras");
                        y += 30;
                    }
                    else
                    {
                        int index = 0;

                        foreach (int i in Enum.GetValues(typeof(FightRules)))
                        {
                            if (Fight.HasRule((FightRules)i))
                            {
                                AddLabel(index % 2 == 0 ? 20 : 132, y + ((int)(index / 2) * 20), 0, ArenaHelper.GetRules((FightRules)i));
                                index++;
                            }
                        }

                        y += 20 * ((index + 1) / 2);
                    }

                    y += 10;
                    //AddLabel(20, y, 0, "My Team:");
                    AddLabel(20, y, 0, "Meu Time:");
                    y += 20;

                    if (Fight is CaptureTheFlagFight)
                    {
                        var info = Fight.GetTeamInfo(myTeam);

                        if (info != null)
                        {
                            //AddLabel(20, y, 188, String.Format("Points: {0}", info.Points.ToString()));
                            AddLabel(20, y, 188, String.Format("Pontos: {0}", info.Points.ToString()));
                            y += 20;
                        }
                    }

                    foreach (var pm in myTeam.Fighters)
                    {
                        AddLabel(25, y, pm.Alive ? 173 : 133, pm.Name);
                        y += 20;
                    }

                    //AddLabel(20, y, 0, "Opponents:");
                    AddLabel(20, y, 0, "Oponentes:");
                    y += 20;

                    foreach (var team in Fight.Teams.Where(t => t.Team != myTeam))
                    {
                        if (Fight is CaptureTheFlagFight)
                        {
                            //AddLabel(20, y, 188, String.Format("Points: {0}", team.Points.ToString()));
                            AddLabel(20, y, 188, String.Format("Pontos: {0}", team.Points.ToString()));
                            y += 20;
                        }

                        if (Fight is LastManStandingFight)
                        {
                            foreach (var pm in team.Team.Fighters)
                            {
                                AddLabel(25, y, pm.Alive ? 173 : 133, pm.Name);
                                y += 20;
                            }
                        }
                        else
                        {
                            foreach (var pm in team.Team.Fighters)
                            {
                                AddLabel(25, y, pm.Alive ? 173 : 133, pm.Name);
                                y += 20;
                            }
                        }
                    }

                    AddButton(215, 3, 4017, 4018, 1, GumpButtonType.Reply, 0);
                }
            }

            if (!Fight.PreFight)
            {
                if (!Fight.PostFight)
                {
                    AddHtml(30, 5, 100, 20, "<basefont color=#FF0000>" + String.Format("{0:mm\\:ss}", (Fight.EndTime - DateTime.UtcNow)), false, false);
                }
                else
                {
                    //AddHtml(30, 5, 100, 20, "<basefont color=#FF0000>" + "Fight Over", false, false);
                    AddHtml(30, 5, 100, 20, "<basefont color=#FF0000>" + "PVP Encerrado", false, false);
                }
            }
            else
            {
                AddHtml(30, 5, 100, 20, "<basefont color=#FF0000>" + String.Format("{0:mm\\:ss}", (Fight.FightDuration)), false, false);
            }

            //AddHtml(140, 5, 264, 20, "Fight Details", false, false);
            AddHtml(140, 5, 264, 20, "Detalhes do PVP", false, false);
        }

        public override void OnResponse(RelayInfo button)
        {

            switch (button.ButtonID)
            {
                default:
                    break;
                case 1:
                    Small = Small ? false : true;
                    Refresh();
                    break;
            }
        }
    }
}
