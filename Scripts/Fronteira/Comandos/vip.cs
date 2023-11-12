using System;
using System.Collections.Generic;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace MeuNamespace
{
    public class ComandoVip
    {
        private static Dictionary<Mobile, int> PlayerUsageCount = new Dictionary<Mobile, int>();
        private static Dictionary<Mobile, Point3D> PlayerInitialLocations = new Dictionary<Mobile, Point3D>();
        private static Dictionary<Mobile, DateTime> PlayerLastRedeem = new Dictionary<Mobile, DateTime>();
        private static bool CommandEnabled = true; // Variável para habilitar ou desabilitar o comando .vip
        private const int MaxUsageCount = 15;
        private const int RedeemCooldownDays = 30;

        public static void Initialize()
        {
            CommandSystem.Register("vip", AccessLevel.Ajudante, new CommandEventHandler(MeuComando_OnCommand));
            CommandSystem.Register("vipadmin", AccessLevel.Administrator, new CommandEventHandler(VipAdminCommand_OnCommand));
            EventSink.Logout += new LogoutEventHandler(EventSink_Logout);
        }

        [Usage("vip")]
        [Description("Exibe um Gump de VIP.")]
        private static void MeuComando_OnCommand(CommandEventArgs e)
        {
            if (!CommandEnabled)
            {
                e.Mobile.SendMessage("O comando VIP está desabilitado no momento.");
                return;
            }

            Mobile from = e.Mobile;

            if (CanUseCommand(from))
            {
                PlayerUsageCount[from] += 1;
                int usageCount = PlayerUsageCount[from];
                from.SendMessage($"Você usou o comando VIP {usageCount}/{MaxUsageCount} vezes.");

                PlayerInitialLocations[from] = from.Location; // Armazena a localização inicial do jogador

                from.SendGump(new VIPGump(from));
            }
            else
            {
                from.SendMessage("Você atingiu o limite máximo de uso do comando VIP.");
            }
        }

        [Usage("vipadmin enable|disable")]
        [Description("Habilita ou desabilita o comando VIP.")]
        private static void VipAdminCommand_OnCommand(CommandEventArgs e)
        {
            if (e.Length == 1)
            {
                string option = e.GetString(0).ToLower();

                switch (option)
                {
                    case "enable":
                        CommandEnabled = true;
                        e.Mobile.SendMessage("O comando VIP foi habilitado.");
                        break;
                    case "disable":
                        CommandEnabled = false;
                        e.Mobile.SendMessage("O comando VIP foi desabilitado.");
                        break;
                    default:
                        e.Mobile.SendMessage("Opção inválida. Use 'enable' ou 'disable'.");
                        break;
                }
            }
            else
            {
                e.Mobile.SendMessage("Sintaxe incorreta. Use 'vipadmin enable' ou 'vipadmin disable'.");
            }
        }

        private static bool CanUseCommand(Mobile mobile)
        {
            if (!PlayerUsageCount.ContainsKey(mobile))
            {
                PlayerUsageCount[mobile] = 0;
            }

            // Verificar se o jogador já usou o comando MaxUsageCount vezes
            if (PlayerUsageCount[mobile] >= MaxUsageCount)
            {
                return false;
            }

            return true;
        }

        private static void EventSink_Logout(LogoutEventArgs e)
        {
            Mobile from = e.Mobile;

            if (PlayerInitialLocations.ContainsKey(from))
            {
                Point3D initialLocation = PlayerInitialLocations[from];

                // Retornar à localização inicial ao deslogar
                from.MoveToWorld(new Point3D(435, 254, -2), Map.Malas);
                // Remover o jogador da lista para evitar contagens e localizações incorretas
                PlayerUsageCount.Remove(from);
                PlayerInitialLocations.Remove(from);
            }
        }

        private class VIPGump : Gump
        {
            private const int ResurrectTokenID = 3622;
            private const int BrindeButtonID = 2;

            public VIPGump(Mobile from) : base(10, 10)
            {
                AddPage(0);
                AddBackground(0, 0, 300, 200, 40000);
                AddHtml(30, 20, 240, 80, "Seja bem-vindo à DG VIP! Deseja acessar agora?", true, true);
                AddHtml(30, 20, 240, 80, "Seja bem-vindo à DG VIP! Deseja acessar agora?", true, true);
                AddButton(60, 110, 1209, 1210, 1, GumpButtonType.Reply, 0);
                AddLabel(100, 110, 0, "Entrar");

                if (from.AccessLevel >= AccessLevel.GameMaster)
                {
                    //AddButton(160, 110, 4011, 4012, BrindeButtonID, GumpButtonType.Reply, 0);
                   // AddLabel(200, 110, 0, "Resgatar Brinde do Mês");
                }
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (info.ButtonID == 1)
                {
                    Mobile from = sender.Mobile;
                    from.SendMessage("Acesso ao conteúdo VIP concedido!");
                    from.MoveToWorld(new Point3D(349, 15, -1), Map.Malas);
                }
                else if (info.ButtonID == BrindeButtonID)
                {
                    Mobile from = sender.Mobile;

                    if (CanRedeemBrinde(from))
                    {
                        GiveResurrectTokens(from, 10);
                        PlayerLastRedeem[from] = DateTime.Now;
                    }
                    else
                    {
                        TimeSpan cooldownTime = GetBrindeCooldownTime(from);
                        from.SendMessage($"Você só pode resgatar o Brinde do Mês novamente em {cooldownTime.Days} dias.");
                    }
                }
            }

            private void GiveResurrectTokens(Mobile mobile, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    mobile.AddToBackpack(new Bag());
                }

                mobile.SendMessage($"Você recebeu {count} ResurrectTokens.");
            }

            private bool CanRedeemBrinde(Mobile mobile)
            {
                if (!PlayerLastRedeem.ContainsKey(mobile))
                {
                    return true;
                }

                DateTime lastRedeemTime = PlayerLastRedeem[mobile];
                TimeSpan elapsedTime = DateTime.Now - lastRedeemTime;
                return elapsedTime.TotalDays >= RedeemCooldownDays;
            }

            private TimeSpan GetBrindeCooldownTime(Mobile mobile)
            {
                if (!PlayerLastRedeem.ContainsKey(mobile))
                {
                    return TimeSpan.Zero;
                }

                DateTime lastRedeemTime = PlayerLastRedeem[mobile];
                TimeSpan elapsedTime = DateTime.Now - lastRedeemTime;
                TimeSpan remainingTime = TimeSpan.FromDays(RedeemCooldownDays) - elapsedTime;
                return remainingTime;
            }
        }
    }
}
