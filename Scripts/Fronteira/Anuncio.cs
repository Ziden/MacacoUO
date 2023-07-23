using System;
using Server.Network;
using Server.Mobiles;
using Server.Misc.Custom;
using Fronteira.Discord;

namespace Server.Gumps
{
    public class AnuncioGump : Gump
    {
        private static readonly TimeSpan CloseDelay = TimeSpan.FromSeconds(60); // Defina o tempo de fechamento em segundos

        public static void Texto(string msg)
        {
            foreach (Mobile pl in NetState.GetOnlinePlayerMobiles())
            {
                pl.SendMessage(2, msg);
            }
            DiscordBot.SendMessage(msg);
        }

        public AnuncioGump(PlayerMobile from, string texto) : base(0, 20)
        {
            if (!from.HasGump(typeof(AnuncioGump)))
                from.Anuncios = 0;

            var y = from.Anuncios * 30;

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = false;
            this.Resizable = false;

            AddPage(0);
            AddBackground(-1, 27 + y, 537, 25, 9350);
            AddHtml(28, 30 + y, 421, 22, texto, (bool)false, (bool)false);
            AddButton(502, 30 + y, 30535, 30535, 0, GumpButtonType.Reply, 0);
            AddItem(-6, 29 + y, 3636);
            from.Anuncios++;

            // Configurar o temporizador para fechar o Gump após 10 segundos
            Timer.DelayCall(CloseDelay, () => CloseGump(from));
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            var from = sender.Mobile as PlayerMobile;
            CloseGump(from);
        }

        private void CloseGump(PlayerMobile player)
        {
            // Certifique-se de que o jogador ainda tem este Gump aberto antes de fechá-lo
            if (player.HasGump(typeof(AnuncioGump)))
            {
                player.CloseGump(typeof(AnuncioGump));
                player.Anuncios--;
            }
        }
    }
}
