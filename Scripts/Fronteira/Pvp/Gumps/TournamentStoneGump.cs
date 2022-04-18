using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class TournamentStoneGump : BaseTournamentGump
    {
        private PVPTournamentSystem System { get; set; }
 
        public TournamentStoneGump(PVPTournamentSystem system, PlayerMobile from) : base(from, 20, 20)
        {
            System = system;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 350, 475, DarkBackground);
            AddBackground(10, 10, 330, 125, LightBackground);
            AddBackground(10, 145, 330, 125, LightBackground);
            AddBackground(10, 280, 330, 185, LightBackground);

            if (System == null)
            {
                return;
            }

            AddHtml(0, 15, 350, 16, ColorAndCenter(LabelColor, System.Name), false, false);

            //AddLabel(55, 45, 0, "Tournament Stats");
            AddLabel(55, 45, 0, "Estatísticas");
            AddTooltip(Localizations.GetLocalization(1));
            AddButton(15, 45, 4029, 4031, 1, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(1));

            //AddLabel(230, 45, 0, "System Info");
            AddLabel(230, 45, 0, "Informações");
            AddTooltip(Localizations.GetLocalization(2));
            AddButton(190, 45, 4029, 4031, 8, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(2));

            //AddLabel(55, 75, 0, "Team Stats");
            AddLabel(55, 75, 0, "Estatísticas Time");
            AddTooltip(Localizations.GetLocalization(3));
            AddButton(15, 75, 4029, 4031, 2, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(3));

            if (ArenaTeam.HasTeam(User))
            {
                AddButton(190, 75, 4029, 4031, 9, GumpButtonType.Reply, 0);
                AddTooltip(Localizations.GetLocalization(4));
                //AddLabel(230, 75, 0, "My Teams Info");
                AddLabel(230, 75, 0, "Meu Time");
                AddTooltip(Localizations.GetLocalization(4));
            }

            AddButton(15, 105, 4029, 4031, 6, GumpButtonType.Reply, 0);
            AddTooltip(Localizations.GetLocalization(5));
            //AddLabel(55, 105, 0, "Future Tournaments");
            AddLabel(55, 105, 0, "Torneios Futuros");
            AddTooltip(Localizations.GetLocalization(5));

            //AddHtml(0, 150, 350, 20, ColorAndCenter(LabelColor, "Fight/Register"), false, false);
            AddHtml(0, 150, 350, 20, ColorAndCenter(LabelColor, "Lutar / Registrar"), false, false);

            if (System.Active)
            {
                if ((User is PlayerMobile && !((PlayerMobile)User).Young))
                {
                    //AddLabel(55, 180, 0, "Register Team");
                    AddLabel(55, 180, 0, "Registrar Time");
                    AddTooltip(Localizations.GetLocalization(6));
                    AddButton(15, 180, 4023, 4025, 3, GumpButtonType.Reply, 0);
                    AddTooltip(Localizations.GetLocalization(6));
                }

                if (System.CanRegisterTournament(User))
                {
                    //AddLabel(55, 210, 0, "Register Tournament");
                    AddLabel(55, 210, 0, "Registrar Torneio");
                    AddTooltip(Localizations.GetLocalization(7));
                    AddButton(15, 210, 4023, 4025, 4, GumpButtonType.Reply, 0);
                    AddTooltip(Localizations.GetLocalization(7));
                }
                else if (System is CTFArena)
                {
                    var ctf = (CTFArena)System;

                    if (ctf.WaitingRoom != null)
                    {
                        var rumble = ctf.PendingTeamRumble;

                        if (rumble != null)
                        {
                            if (!rumble.CanBegin)
                            {
                                AddLabel(55, 210, 0, string.Format("Team Rumble CTF: Needs {0} players to begin!", rumble.MinPlayerCount - rumble.ParticipantCount()));
                            }
                            else
                            {
                                if (ctf.Queue.Contains(rumble))
                                {
                                    var inQueue = ctf.Queue.IndexOf(rumble);

                                    AddLabel(55, 210, 0, string.Format("Team Rumble CTF is {0} in the Queue!", inQueue == 0 ? "next" : string.Format("number {0}", inQueue + 1)));
                                }
                                else
                                {
                                    AddLabel(55, 210, 0, "Team Rumble CTF will begin shortly!");
                                }
                            }

                            if (ctf.CurrentFight != rumble || ctf.CurrentFight.PreFight)
                            {
                                if (rumble.InitialParticipants.Contains(User))
                                {
                                    AddButton(15, 210, 4016, 4017, 15, GumpButtonType.Reply, 0);
                                    AddTooltip(Localizations.GetLocalization(87));
                                }
                                else
                                {
                                    AddButton(15, 210, 4005, 4007, 14, GumpButtonType.Reply, 0);
                                    AddTooltip(Localizations.GetLocalization(86));
                                }
                            }
                            else
                            {
                                AddImage(15, 210, 4029);
                                AddTooltip(Localizations.GetLocalization(89));
                            }
                        }
                    }
                }

                if (!ActionTimer.AwaitingAction(User))
                {
                    if (System.CanRegisterFight(User) && System.FightTypes.Any(type => type == ArenaFightType.SingleElimination))
                    {
                        AddButton(190, 240, 4023, 4025, 10, GumpButtonType.Reply, 0);
                        AddTooltip(Localizations.GetLocalization(8));
                        //AddLabel(230, 240, 0, "Quick Fight");
                        AddLabel(230, 240, 0, "PVP Rápido");
                        AddTooltip(Localizations.GetLocalization(8));
                    }

                    if (System.CanRegisterFight(User))
                    {
                        string fightType = "";

                        if (System.FightTypes.Length == 1)
                            fightType = String.Format(" ({0})", PVPTournamentSystem.GetFightType(System.FightTypes[0]));


                        /*AddButton(15, 240, 4023, 4025, 11, GumpButtonType.Reply, 0);
                         AddTooltip(Localizations.GetLocalization(9));
                         //AddLabel(56, 240, 0, "New Arena Fight" + fightType);          //Desativado, não funcionou
                         AddLabel(56, 240, 0, "Novo PVP Arena" + fightType);
                         AddTooltip(Localizations.GetLocalization(9));*/

                    }
                    else if (System is CTFArena && ((CTFArena)System).PendingTeamRumble != null && System.CurrentFight != ((CTFArena)System).PendingTeamRumble && (User.AccessLevel >= AccessLevel.GameMaster || ((CTFArena)System).PendingTeamRumble.Owner == User))
                    {
                        AddButton(15, 240, 4023, 4025, 16, GumpButtonType.Reply, 0);
                        AddTooltip(Localizations.GetLocalization(88));
                        AddLabel(56, 240, 0, "Cancel Team Rumble Fight");
                        AddTooltip(Localizations.GetLocalization(88));
                    }
                }

                if (User.AccessLevel >= Config.LeagueRegistrationAccess)
                {
                    //AddLabel(230, 180, 0, "Register League");
                    AddLabel(230, 180, 0, "Registrar Liga");
                    AddButton(190, 180, 4023, 4025, 12, GumpButtonType.Reply, 0);
                    AddTooltip(Localizations.GetLocalization(73));
                }

                if (Config.LeaguesActive)
                {
                    if (League.HasValidLeagues(User, System.Stone))
                    {
                        //AddLabel(230, 210, 0, "League Match");
                        AddLabel(230, 210, 0, "Partida da Liga");
                        AddButton(190, 210, 4023, 4025, 13, GumpButtonType.Reply, 0);
                        AddTooltip(Localizations.GetLocalization(74));
                    }
                }
            }

            string status = "";
            string start = "";

            if (!System.Active)
                //status = "Arena Inactive";
                status = "Arena Inativa";
            else if (!System.InUse)
                //status = "Arena Available";
                status = "Arena Disponível";

            else if (System.CurrentFight != null && !System.CurrentFight.IsTournament)
            {
                ArenaFight currentfight = System.CurrentFight;
                //status = String.Format("In Use{1} - {0}", ArenaHelper.GetFightType(currentfight.ArenaFightType), currentfight.League != null ? " [League Match]" : String.Empty);
                status = String.Format("Em Uso{1} - {0}", ArenaHelper.GetFightType(currentfight.ArenaFightType), currentfight.League != null ? " [Partida da Liga]" : String.Empty);

                ArenaTeam teamA = currentfight.TeamA;
                ArenaTeam teamB = currentfight.TeamB;

                if (teamA != null && teamB != null)
                {
                    if (currentfight.ArenaFightType == ArenaFightType.LastManStanding)
                    {
                        if (currentfight.PreFight)
                        {
                            //status = status + String.Format("<br><br>{0} teams will be fighting shortly.", currentfight.Teams.Count);
                            status = status + String.Format("<br><br>{0} times vão se enfrentar em breve.", currentfight.Teams.Count);
                        }
                        else
                        {
                            //status = status + String.Format("<br><br>{0} teams are fighting eachother!", currentfight.Teams.Count);
                            status = status + String.Format("<br><br>{0} times estão se enfrentando!", currentfight.Teams.Count);
                        }
                    }
                    else
                    {
                        if (currentfight.PreFight)
                        {
                            //status = status + "<br><br>" + String.Format("{0} will be fighting {1} shortly.", teamA.Name, teamB.Name);
                            status = status + "<br><br>" + String.Format("{0} vai enfrentar {1} em breve.", teamA.Name, teamB.Name);
                        }
                        else
                        {
                            //status = status + "<br><br>" + String.Format("{0} is currently fighting {1} in a {2} match!", teamA.Name, teamB.Name, ArenaHelper.GetFightType(currentfight.ArenaFightType));
                            status = status + "<br><br>" + String.Format("{0} está enfrentando {1} em uma partida {2}!", teamA.Name, teamB.Name, ArenaHelper.GetFightType(currentfight.ArenaFightType));
                        }
                    }

                    switch (currentfight.ArenaFightType)
                    {
                        case ArenaFightType.CaptureTheFlag:
                            TeamInfo info1 = currentfight.GetTeamInfo(currentfight.TeamA);
                            TeamInfo info2 = currentfight.GetTeamInfo(currentfight.TeamB);

                            status += "<br>";
                            /*status += String.Format("<br>{0} Points: {1}", currentfight.TeamA.Name, info1.Points);
                            status += String.Format("<br>{0} Kills: {1}", currentfight.TeamA.Name, info1.Kills);
                            status += String.Format("<br>{0} Count: {1}", currentfight.TeamA.Name, info1.Team.Fighters.Count);*/
                            status += String.Format("<br>{0} Pontos: {1}", currentfight.TeamA.Name, info1.Points);
                            status += String.Format("<br>{0} Kills: {1}", currentfight.TeamA.Name, info1.Kills);
                            status += String.Format("<br>{0} Count: {1}", currentfight.TeamA.Name, info1.Team.Fighters.Count);
                            status += "<br>";
                            //status += String.Format("<br>{0} Points: {1}", currentfight.TeamB.Name, info2.Points);
                            status += String.Format("<br>{0} Pontos: {1}", currentfight.TeamB.Name, info2.Points);
                            status += String.Format("<br>{0} Kills: {1}", currentfight.TeamB.Name, info2.Kills);
                            status += String.Format("<br>{0} Count: {1}", currentfight.TeamB.Name, info2.Team.Fighters.Count);
                            break;
                        case ArenaFightType.BestOf3:
                            int winA = currentfight is BestOf3Fight ? ((BestOf3Fight)currentfight).TeamAWins : 0;
                            int winB = currentfight is BestOf3Fight ? ((BestOf3Fight)currentfight).TeamBWins : 0;

                            status += "<br>";

                            if (winA + winB == 0)
                            {
                                //status += String.Format("<br>This is the first fight between {0} and {1}.", currentfight.TeamA.Name, currentfight.TeamB.Name);
                                status += String.Format("<br>Este é o primeiro PVP entre {0} e {1}.", currentfight.TeamA.Name, currentfight.TeamB.Name);
                            }
                            else if (winA + winB == 1)
                            {
                                //status += String.Format("<br>{0} has won round one!", winA == 1 ? currentfight.TeamA.Name : currentfight.TeamB.Name);
                                status += String.Format("<br>{0} venceu o primeiro round!", winA == 1 ? currentfight.TeamA.Name : currentfight.TeamB.Name);
                            }
                            else
                            {
                                //status += String.Format("<br>{0} and {1} have each won a round, next kill wins!", currentfight.TeamA.Name, currentfight.TeamB.Name);
                                status += String.Format("<br>{0} e {1} ganharam um round cada, a próxima kill define o vencedor!", currentfight.TeamA.Name, currentfight.TeamB.Name);
                            }
                            break;
                        case ArenaFightType.LastManStanding:
                            //status += "<br><br>Still Alive:<br>";
                            status += "<br><br>Fique Vivo:<br>";
                            foreach (var team in currentfight.Teams.Select(x => x.Team).Where(t => !t.AllDead()))
                            {
                                status += String.Format("<br>{0}", team.Name);
                            }
                            break;
                    }
                }

                if (System.Queue.Count > 0)
                {
                    ArenaFight fight = System.Queue[0];

                    if (fight.ArenaFightType == ArenaFightType.LastManStanding)
                        status = status + "<br>" + String.Format("On Deck: Last Man Standing with {0} participants.", fight.Teams.Count.ToString());
                    else
                        status = status + "<br>" + String.Format("On Deck: {0} vs {1}", fight.TeamA != null ? fight.TeamA.Name : "Unknown", fight.TeamB != null ? fight.TeamB.Name : "Unknown");

                    if (System.Queue.Count > 1)
                    {
                        fight = System.Queue[1];
                        status = status + "<br>" + String.Format("In The Hole: {0} vs {1}", fight.TeamA != null ? fight.TeamA.Name : "Unknown", fight.TeamB != null ? fight.TeamB.Name : "Unknown");
                    }
                }
            }
            else if (System.CurrentTournament != null)
            {
                Tournament tourney = System.CurrentTournament;

                if (DateTime.Now < tourney.StartTime)
                {
                    TimeSpan ts = tourney.StartTime - DateTime.Now;

                    if (ts.TotalMinutes == 1)
                        start = "1 minute";
                    else if (ts.TotalMinutes < 1)
                        start = String.Format("{0} seconds", (int)ts.TotalSeconds);
                    else
                        start = String.Format("{0} minutes", (int)ts.TotalMinutes);

                    //status = String.Format("The {0} Tournament will begin in {1}.", tourney.Name, time);
                }
                else
                    start = "Tournament in Progress";

                start = start + "<br>";

                if (tourney.Participants != null && tourney.Participants.Count > 0 && tourney.Round > 0)
                {
                    if (tourney.RoundMatches.Count == 0)
                        status = String.Format("Round {0} is ending, waiting for round {1}", tourney.Round - 1, tourney.Round);
                    else
                    {
                        try
                        {
                            ArenaTeam a = null;
                            ArenaTeam b = null;
                            ArenaTeam c = null;
                            ArenaTeam d = null;

                            if (tourney.ArenaB != null && tourney.UseAlternateArena && tourney.OriginalArena.Count > 0)
                            {
                                a = tourney.OriginalArena[0];
                                if (tourney.OriginalArena.Count > 1)
                                    b = tourney.OriginalArena[1];
                                if (tourney.AlternateArena.Count > 0)
                                    c = tourney.AlternateArena[0];
                                if (tourney.AlternateArena.Count > 1)
                                    d = tourney.OriginalArena[1];

                                if (a != null && b != null && c != null && d != null)
                                    status = String.Format(tourney.Name + "   Round " + tourney.Round + "<br>Main Arena: {0} Vs {1}<br>Alternate Arena: {2} Vs {3}<br>Teams Left in current round: " + tourney.RoundMatches.Count.ToString() + "<br>Teams left in tournament: " + tourney.Participants.Count.ToString(), a.Name, b.Name, c.Name, d.Name);
                                else if (a != null && b != null)
                                    status = String.Format(tourney.Name + "   Round " + tourney.Round + "<br>Main Arena: {0} Vs {1}<br>Teams Left in the current round: " + tourney.RoundMatches.Count.ToString() + "<br>Teams left in tournament: " + tourney.Participants.Count.ToString(), a.Name, b.Name);
                                else
                                    status = String.Format(tourney.Name + "   Round " + tourney.Round + "<br>Teams Left in the current round: " + tourney.RoundMatches.Count.ToString() + "<br>Teams left in tournament: " + tourney.Participants.Count.ToString());
                            }
                            else if (tourney.RoundMatches.Count > 0)
                            {
                                a = tourney.RoundMatches[0];
                                if (tourney.RoundMatches.Count > 1)
                                    b = tourney.RoundMatches[1];
                                if (tourney.RoundMatches.Count > 2)
                                    c = tourney.RoundMatches[2];
                                if (tourney.RoundMatches.Count > 3)
                                    d = tourney.RoundMatches[3];

                                if (a != null && b != null && c != null && d != null)
                                    status = String.Format(tourney.Name + "   Round " + tourney.Round + "<br>Current Fight: {0} Vs {1}<br>Next Fight: {2} Vs {3}<br>Teams Left in current round: " + tourney.RoundMatches.Count.ToString() + "<br>Teams left in tournament: " + tourney.Participants.Count.ToString(), a.Name, b.Name, c.Name, d.Name);
                                else if (a != null && b != null)
                                    status = String.Format(tourney.Name + "   Round " + tourney.Round + "<br>Current Fight: {0} Vs {1}<br>Teams Left in the current round: " + tourney.RoundMatches.Count.ToString() + "<br>Teams left in tournament: " + tourney.Participants.Count.ToString(), a.Name, b.Name);
                                else
                                    status = String.Format(tourney.Name + "   Round " + tourney.Round + "<br>Teams Left in the current round: " + tourney.RoundMatches.Count.ToString() + "<br>Teams left in tournament: " + tourney.Participants.Count.ToString());
                            }
                        }
                        catch { }
                    }
                }

            }

            //AddHtml(0, 284, 350, 20, ColorAndCenter(LabelColor, "Arena Status"), false, false);
            AddHtml(0, 284, 350, 20, ColorAndCenter(LabelColor, "Status da Arena"), false, false);
            AddHtml(26, 309, 297, 140, start + status, true, true);
        }
 
        public override void OnResponse(RelayInfo button)
        {
            switch (button.ButtonID)
            {
                default:
                case 0: break;
                case 1: //Tourney Stats
                    BaseGump.SendGump(new TournamentStatsGump(User, System));
                    break;
                case 2: //Team Stats
                    BaseGump.SendGump(new PlayerStatsGump(User));
                    break;
                case 3: //Register Team
                    if (System.Active && User is PlayerMobile && !((PlayerMobile)User).Young)
                        BaseGump.SendGump(new RegisterTeamGump(User, new ArenaTeam(User), System));
                    break;
                case 4: //Register Tourney
                    if (System.Active && System.CanRegisterTournament(User))
                        BaseGump.SendGump(new RegisterTournamentGump(System, new Tournament(System), User));
                    break;
                case 5: //New Fight
                    break;
                case 6: //Upcoming Tourneys
                    BaseGump.SendGump(new TournamentsGump(System, User));
                    break;
                case 7: //My Teams Info
                    {
                    }

                    break;
                case 8: //System Info
                    {
                        if (!User.HasGump(typeof(SystemInfoGump)))
                        {
                            BaseGump.SendGump(new SystemInfoGump(User, System));
                        }
                    }

                    break;
                case 9: //My Teams Stats
                    {
                        BaseGump.SendGump(new PlayerRecordGump(User));
                    }

                    break;
                case 10:
                    {
                        if (!ActionTimer.WaitingAction.ContainsKey(User) && System.CanRegisterFight(User, false))
                        {
                            ArenaFight fight = new SingleEliminationFight(System, null);
							fight.FightType = ArenaTeamType.Single;
 
                            User.Target = new RegisterFightGump.InternalTarget(System, fight, User, true, true);
                            //User.SendMessage("Target the player you'd like to fight.");
                            User.SendMessage("Selecione o jogador que deseja enfrentar.");
                        }
                    }

                    break;
                case 11: // Register Fight
                    {
                        if (System.Active && !ActionTimer.AwaitingAction(User) && System.CanRegisterFight(User))
                        {
                            if (!ArenaTeam.HasTeam(User))
                            {
                                //User.SendMessage("You must belong to a registered arena team before starting a new fight!");
                                User.SendMessage("Você precisa estar registado em um time antes de lutar!");
                            }
                            else
                            {
                                if (System.FightTypes.Length == 1)
                                {
                                    BaseGump.SendGump(new RegisterFightGump(System, System.ConstructArenaFight(System.FightTypes[0]), User));
                                }
                                else
                                {
                                    BaseGump.SendGump(new ChooseFightTypeGump(User, System));
                                }
                            }
                        }
                    }

                    break;
                case 12: // Register League
                    {
                        if (User.AccessLevel >= Config.LeagueRegistrationAccess)
                        {
                            if (!Config.LeaguesActive)
                            {
                                //User.SendMessage("Leagues are inactive, and can be activated in the System Config File.");
                                User.SendMessage("As Ligas estão inativas e podem ser ativadas no menu de configuração.");
                            }
                            else
                            {
                                BaseGump.SendGump(new CreateLeagueGump(User));
                            }
                        }
                    }

                    break;
                case 13: // League Match
                    {
                        var leagues = League.GetValidLeagues(User, System.Stone);

                        if (leagues != null)
                        {
                            BaseGump.SendGump(new FightSelectGump(User, leagues, System.Stone));
                        }

                    }

                    break;
                case 14: // Join Team Rumble
                    {
                        var ctfArena = System as CTFArena;

                        if (ctfArena != null && ctfArena.PendingTeamRumble != null && (ctfArena.CurrentFight != ctfArena.PendingTeamRumble || ctfArena.CurrentFight.PreFight))
                        {
                            var waitingRoom = ctfArena.WaitingRoom;

                            if (waitingRoom != null)
                            {
                                waitingRoom.TryAdd(User, ctfArena.PendingTeamRumble);
                            }
                        }
                    }
                    break;
                case 15: // Remove thyself!
                    {
                        var ctfArena = System as CTFArena;

                        if (ctfArena != null && ctfArena.PendingTeamRumble != null && (ctfArena.CurrentFight != ctfArena.PendingTeamRumble || ctfArena.CurrentFight.PreFight))
                        {
                            ctfArena.PendingTeamRumble.RemoveParticipant(User);

                            if (ctfArena.WaitingRoom != null)
                            {
                                ctfArena.WaitingRoom.Kick(User);
                                User.SendMessage("You have removed yourself from this CTF Team Rumble Game.");
                            }
                        }
                    }
                    break;
                case 16: // Cancel CTF
                    {
                        var ctfArena = System as CTFArena;

                        if (ctfArena != null && ctfArena.PendingTeamRumble != null && ctfArena.CurrentFight != ctfArena.PendingTeamRumble)
                        {
                            ctfArena.PendingTeamRumble.CancelFight(CancelReason.Aborted);
                        }
                    }
                    break;
            }
        }
    }
}
