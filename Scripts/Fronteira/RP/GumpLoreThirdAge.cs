using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Ziden.Tutorial;
using Server.Mobiles;

namespace Server.Gumps
{
    public class GumpLoreRP : Gump
    {
        public GumpLoreRP() : base(0, 0)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);
            AddBackground(136, 63, 627, 299, 3000);
            AddImage(54, 62, 337);
            AddImage(233, 205, 1520);
            AddBackground(293, 69, 457, 146, 3500);
            AddHtml(316, 83, 383, 39, @"Parte da sua vida passa diante de seus olhos.", (bool)false, (bool)false);
            AddHtml(316, 126, 383, 22, @"Voce esta completo.", (bool)false, (bool)false);
            AddHtml(315, 152, 398, 20, @"Seu corpo ainda e fraco, mas pode se mover", (bool)false, (bool)false);
            AddHtml(315, 177, 398, 21, @"Voce sente a madeira que segura seu corpo.", (bool)false, (bool)false);
            AddHtml(332, 303, 398, 21, @"Agora e a hora de ir.", (bool)false, (bool)false);
            AddImage(759, 60, 337);
            AddBackground(141, 69, 152, 146, 3500);
            AddImage(163, 87, 2741);
            AddButton(407, 330, 247, 248, 0, GumpButtonType.Reply, 0);
            AddImage(124, 339, 113);
            AddImage(748, 341, 113);
            AddImage(747, 49, 113);
            AddImage(122, 50, 113);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            var from = sender.Mobile as PlayerMobile;
            
        }
    }
}
