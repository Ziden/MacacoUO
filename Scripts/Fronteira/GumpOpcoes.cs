
using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Leilaum;

namespace Server.Gumps
{
    public class GumpOpcoes : Gump
    {
        private Action<int> Action;

        public GumpOpcoes(string titulo, Action<int> optionSelected, int icone=0, int hue=0, params string[] options) : base(0, 0)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            Action = optionSelected;
            AddPage(0);
            var extras = options.Length - 2;
            if (extras < 0)
                extras = 0;
            extras *= 30;

            var maiorOptionX = 0;
            foreach(var opt in options)
            {
                if (opt.Length > maiorOptionX)
                    maiorOptionX = opt.Length;
            }

            var tamanhoX = 50 + (maiorOptionX * 10);
            AddBackground(182, 188, tamanhoX, 119 + extras, 9350);
            AddHtml(196, 207, 200, 21, titulo, (bool)false, (bool)false);

            var posY = 0;
            for (var x= 0; x < options.Length; x++)
            {
                AddHtml(213, 246+posY, tamanhoX - 50, 23, options[x], (bool)true, (bool)false);
                AddButton(185, 243+posY, 2472, 2472, x+1, GumpButtonType.Reply, 0);
                posY += 30;
            }

            if(icone != 0)
            {
                AddBackground(108, 210, 74, 72, 9350);
                NewAuctionGump.AddItemCentered(108, 210, 74, 72, icone, hue, this);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if(info.ButtonID > 0 && Action != null)
            {
                var opt = info.ButtonID - 1;
                Action(opt);
            }
        
        }
    }
}
