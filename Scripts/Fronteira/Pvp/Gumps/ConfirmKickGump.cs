using System;
using System.Collections.Generic;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
using Server.Gumps;
using Server.Prompts;

namespace Server.TournamentSystem
{
    public class ConfirmKickGump : BaseTournamentGump
    {
        private ArenaTeam m_Team;
        private Mobile m_ToKick;
        private bool m_Resign;

        public ConfirmKickGump(PlayerMobile user, ArenaTeam team, Mobile kick, bool resign) : base(user, 20, 20)
        {
            m_Team = team;
            m_ToKick = kick;
            m_Resign = resign;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, 250, 200, 5120);
            AddBackground(10, 10, 230, 180, 5100);

            AddButton(30, 155, 2074, 2075, 1, GumpButtonType.Reply, 0); //Okay
            AddButton(175, 155, 2071, 2072, 2, GumpButtonType.Reply, 0);  //Cancel

            if (m_Team == null || m_ToKick == null)
                return;

            string text;
            if (m_Resign)
                //text = String.Format("You have selected the option to resign from your Arena Team, {0}. To continue, click Okay. To cancel resignation, select cancel.", m_Team.Name);
                text = String.Format("Você selecionou a opção de sair do time, {0}. Para continuar, clique em Ok. Caso queira desistir, selecione cancelar.", m_Team.Name);
            else
                //text = String.Format("You have selected to kick {0} from your Arena Team, {0}. To continue, click Okay.  To Cancel kicking {0}, select cancel.", m_ToKick.Name, m_Team.Name);
                text = String.Format("Você selecionou remover {0} do seu time, {0}. Para continuar, clique Ok. Para cancelar a expulsão de {0}, selecione cancelar.", m_ToKick.Name, m_Team.Name);

            AddHtml(25, 20, 200, 120, text, true, true);
        }

        public override void OnResponse(RelayInfo button)
        {
            bool resign = User == m_ToKick;

            switch (button.ButtonID)
            {
                default:
                case 0: break;
                case 1:
                    {
                        if (resign)
                            m_Team.Resign(m_ToKick);
                        else
                        {
                            m_Team.Kick(m_ToKick);
                            //User.SendMessage("You kick them from your arena team.");
                            User.SendMessage("Você removeu o alvo do seu time.");

                            BaseGump.SendGump(new PlayerRecordGump(User));
                        }
                        break;
                    }
                case 2:
                    {
                        string text;
                        if (resign)
                            //text = "You decide not to resign from your Arena Team.";
                            text = "Você decidiu não sair do time.";
                        else
                            //text = "You decide not to kick them from your Arena Team.";
                            text = "Você decidiu não remover o alvo do seu time.";

                        User.SendMessage(text);
                        break;
                    }

            }
        }
    }
}
