
using System;
using System.Collections.Generic;
using Fronteira.Discord;
using Server;
using Server.Accounting;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Felladrin.Engines
{
    public static class GlobalChat
    {
        public static class Config
        {
            public static bool Enabled = true;               // Is this system enabled?
            public static bool OpenHistoryOnLogin = true;    // Should we display the history when player logs in?
            public static bool AutoColoredNames = false;      // Should we auto color the players names?
            public static int HistorySize = 50;              // How many messages should we keep in the history?
            public static int MessageHue = 1154;             // What is the hue of the chat messages?
        }

        public static void Initialize()
        {
            if (Config.Enabled)
            {
                CommandSystem.Register("ignorarchat", AccessLevel.Player, new CommandEventHandler(OnCommandToggle));
                CommandSystem.Register("C", AccessLevel.Player, new CommandEventHandler(OnCommandChat));
                CommandSystem.Register("Chat", AccessLevel.Player, new CommandEventHandler(OnCommandChat));
            }
        }

        static readonly List<string> History = new List<string>();

        static HashSet<int> DisabledPlayers = new HashSet<int>();

        [Usage("ChatToggle")]
        [Description("Enables or Disables the Chat.")]
        static void OnCommandToggle(CommandEventArgs e)
        {
            var pm = e.Mobile as PlayerMobile;
            var acc = pm.Account as Account;

            if (acc.GetTag("Chat") == null || acc.GetTag("Chat") == "Enabled")
            {
                DisabledPlayers.Add(pm.Serial.Value);
                acc.SetTag("Chat", "Disabled");
                pm.SendMessage(38, "Chat desabilitado.");

                if (pm.HasGump(typeof(ChatHistoryGump)))
                    pm.CloseGump(typeof(ChatHistoryGump));
            }
            else
            {
                DisabledPlayers.Remove(pm.Serial.Value);
                acc.SetTag("Chat", "Enabled");
                pm.SendMessage(68, "Chat habilitado.");
                pm.SendGump(new ChatHistoryGump());
            }
        }

        [Usage("ChatHistory")]
        [Description("Opens the Chat History.")]
        static void OnCommandHistory(CommandEventArgs e)
        {
            var pm = e.Mobile as PlayerMobile;

            if (DisabledPlayers.Contains(pm.Serial.Value))
            {
                pm.SendMessage(38, "Chat is currently disabled for your account. Type [ChatToggle to enable it.");
                return;
            }

            if (pm.HasGump(typeof(ChatHistoryGump)))
                pm.CloseGump(typeof(ChatHistoryGump));

            pm.SendGump(new ChatHistoryGump());
        }

        [Usage("C <message>")]
        [Description("Sistema de chat")]
        static void OnCommandChat(CommandEventArgs e)
        {
            var pm = e.Mobile as PlayerMobile;
            MsgChatGlobal(pm, e.ArgString);
        }

        public static void MsgChatGlobal(Mobile pm, string msg)
        {
            if(!pm.IsCooldown("dicachat"))
            {
                pm.SetCooldown("dicachat");
                pm.SendMessage(78, "Voce pode usar o comando .c sem nenhuma msg para abrir o historico do chat");
            }

            /*
            if(!Banker.Withdraw(pm, 1))
            {
                if(!pm.Backpack.ConsumeTotal(typeof(Gold), 1))
                {
                    pm.SendMessage("Voce nao tem dinheiro suficiente");
                    return;
                }
            }
            pm.SendMessage("Voce pagou 1 moedas para falar no chat global");
            */

            if (DisabledPlayers.Contains(pm.Serial.Value))
            {
                pm.SendMessage(38, "Chat desabilitado. Use .ignorarchat pra habilitar.");
                return;
            }

            if (msg.Length == 0)
            {
                if (pm.HasGump(typeof(ChatHistoryGump)))
                    pm.CloseGump(typeof(ChatHistoryGump));

                pm.SendGump(new ChatHistoryGump());
            }
            else
            {
                Broadcast(pm, msg);
            }
        }

        static void OnLogin(LoginEventArgs e)
        {
            var pm = e.Mobile as PlayerMobile;
            var acc = pm.Account as Account;

            if (acc.GetTag("Chat") == "Disabled")
            {
                DisabledPlayers.Add(pm.Serial.Value);
                pm.SendMessage("Chat Global desabilitado. Use .ignorarchat para habilitar.");
            }
            else
            {
                pm.SendMessage("Chat global habilitado. Use .ignorarchat para desabilitar");
                if (Config.OpenHistoryOnLogin)
                    pm.SendGump(new ChatHistoryGump());
            }
        }

        static void Broadcast(Mobile sender, string message)
        {
            if(sender.IsCooldown("chat"))
            {
                sender.SendMessage("Aguarde para enviar outra msg");
                return;
            }
            sender.SetCooldown("chat", TimeSpan.FromSeconds(2));

            if (History.Count > Config.HistorySize)
                History.RemoveAt(0);

            History.Add(string.Format("<basefont size=5 color=#524759>[{0}] <basefont size=5 color=#{1}>{2}<basefont color=#14131A> {3}", (System.DateTime.UtcNow - TimeSpan.FromHours(3)).ToString("<basefont size=5 color=#151524>HH:mm"), (Config.AutoColoredNames ? (sender.Name.GetHashCode() >> 8).ToString() : "54432D"), sender.Name, Utility.FixHtml(message)));
            var mess = string.Format("[{0}] {1}", sender.Name, message);
            //DiscordBot.SendMessage(":speaking_head:" + mess);

            foreach (NetState ns in NetState.Instances)
            {
                var player = ns.Mobile as PlayerMobile;

                if (player == null || DisabledPlayers.Contains(player.Serial.Value))
                    continue;

                player.SendMessage(Config.MessageHue, mess);

                if (player.HasGump(typeof(ChatHistoryGump)))
                {
                    player.CloseGump(typeof(ChatHistoryGump));
                    player.SendGump(new ChatHistoryGump());
                }
            }
        }

        static string GenerateHistoryHTML()
        {
            if (History.Count == 0)
                return "<basefont size=5 color=#151524>Bem vindo ! Diga um ola !";

            string HTML = "";

            foreach (string msg in History)
                HTML = msg + " <br/>" + HTML;

            return HTML;
        }

        public class ChatHistoryGump : Gump
        {
            public ChatHistoryGump() : base(110, 100)
            {
                Closable = true;
                Dragable = true;
                Disposable = true;
                Resizable = false;

                AddPage(0);
                AddPage(0);
                AddBackground(184, 286, 570, 245, 9200);
                AddHtml(195, 330, 547, 166, GenerateHistoryHTML(), true, true);
                AddHtml(423, 297, 96, 22, "Chat Global", false, false);
               // AddHtml(568, 502, 164, 20, "Custo: 1 Moeda", false, false);
                AddHtml(202, 502, 247, 20, "Use .c (msg) para falar", false, false);
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if(info.ButtonID==1)
                {


                    var msg = info.TextEntries[0].Text;
                    GlobalChat.MsgChatGlobal(sender.Mobile, msg);
                    sender.Mobile.SendGump(new ChatHistoryGump());
                }
            }
        }
    }
}
