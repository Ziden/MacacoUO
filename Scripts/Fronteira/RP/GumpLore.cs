

using Server.Network;
using Server.Ziden.Tutorial;
using Server.Mobiles;
using Server.Scripts.New.Adam.NewGuild;

namespace Server.Gumps
{
    public class GumpLore : Gump
    {
        public GumpLore(Mobile caller) : base(0, 0)
        {
            this.Closable = false;
            this.Disposable = false;
            this.Dragable = false;
            this.Resizable = false;

            AddPage(0);
            AddBackground(471, 230, 368, 442, 9380);
            AddHtml(497, 270, 311, 22, @"Seja Bem Vindo !", (bool)false, (bool)false);
            AddHtml(498, 300, 312, 78, @"Você se encontra em New Haven, cidade criada pela magia. ", (bool)false, (bool)false);
            AddHtml(497, 382, 311, 114, @"Sua alma foi teleportada para ca. Voce e o Avatar.", (bool)false, (bool)false);
            AddHtml(498, 499, 310, 48, @"Seu corpo ainda e fraco. Voce ficara forte e ira lutar pelo seus ideais.", (bool)false, (bool)false);
            AddHtml(498, 550, 309, 44, @"Voce eh a esperanca. Voce, trara a terceira era.", (bool)false, (bool)false);
            AddHtml(635, 612, 175, 18, @"Fada Nooba", (bool)false, (bool)false);
            AddButton(627, 649, 247, 248, (int)Buttons.Button1, GumpButtonType.Reply, 0);
            AddItem(607, 610, 4031);
        }

        public enum Buttons
        {
            Button1,
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            var from = sender.Mobile as PlayerMobile;
            //NewPlayerGuildAutoJoin.SendStarterGuild(from);
            TutorialNoob.InicializaWisp(from);
        }
    }
}
