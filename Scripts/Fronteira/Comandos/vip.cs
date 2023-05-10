using System;
using System.Collections.Generic;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;

namespace MeuNamespace
{
    public class ComandoVip
    {
        public static void Initialize()
        {
            CommandSystem.Register("vip", AccessLevel.Ajudante, new CommandEventHandler(MeuComando_OnCommand));
        }

        [Usage("vip")]
        [Description("Exibe um Gump de VIP.")]
        private static void MeuComando_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            from.SendGump(new VIPGump(from));
        }

        private class VIPGump : Gump
        {
            public VIPGump(Mobile from) : base(10, 10)
            {
                AddPage(0);
                AddBackground(0, 0, 300, 200, 5054);
                AddHtml(30, 20, 240, 80, "Seja bem-vindo à DG VIP! Deseja acessar agora?", true, true);
                AddButton(60, 110, 1209, 1210, 1, GumpButtonType.Reply, 0);
                AddLabel(100, 110, 0, "Entrar");
            }

            public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
            {
                if (info.ButtonID == 1)
                {
                    Mobile from = sender.Mobile;
                    from.SendMessage("Acesso ao conteúdo VIP concedido!");
                    from.MoveToWorld(new Point3D(2351, 1267, -110), Map.Malas);
                }
            }
        }
    }
}
