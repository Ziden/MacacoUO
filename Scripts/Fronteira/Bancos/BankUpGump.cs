
using Server.Network;
using Server.Mobiles;
using Server.Misc.Custom;

namespace Server.Gumps
{
    public class UpgradeBankGump : Gump
    {
        public UpgradeBankGump(Mobile caller) : base(0, 0)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            var nivel = ((PlayerMobile)caller).NivelBanco;
            var nivelStr = (nivel + 1).ToString();
            var proximo = (nivel + 2).ToString();

            var info = BankLevels.Niveis[nivel];
            var proxInfo = BankLevels.Niveis[nivel + 1];

            AddPage(0);
            AddBackground(252, 72, 364, 447, 9350);
            AddHtml(368, 100, 200, 24, @"Aprimoramento de Banco", (bool)false, (bool)false);
            AddItem(271, 390, 3820);
            AddItem(290, 91, 3649); 
            AddItem(534, 90, 3648);
            AddBackground(276, 140, 319, 244, 3500);
            AddHtml(333, 158, 200, 26, @"Aprimorar do Nivel "+nivelStr+" para "+proximo, (bool)false, (bool)false);
            AddHtml(334, 200, 239, 28, @"Peso em Stones", (bool)true, (bool)false);
            AddHtml(299, 242, 273, 28, @"De "+info.Stones+" para "+ proxInfo.Stones, (bool)false, (bool)false);
            AddHtml(337, 289, 235, 28, @"Quantidade de Items", (bool)true, (bool)false);
            AddHtml(299, 327, 272, 28, @"De "+info.Items+" para "+proxInfo.Items, (bool)false, (bool)false);
            AddHtml(327, 402, 263, 28, @proxInfo.Preco+" Moedas", (bool)true, (bool)false);
            AddButton(290, 460, 30534, 30534, 1, GumpButtonType.Reply, 0);
            AddButton(350, 460, 30535, 30535, 0, GumpButtonType.Reply, 0);
            AddItem(284, 406, 3820);
            AddItem(260, 405, 3820);
            AddItem(293, 289, 7150); // barrinhas de ouro
            AddItem(292, 199, 6225); // balanca
        }

        public enum Buttons
        {
            Nada,
            Aceitar,
            Cancelar,
        }


        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case (int)Buttons.Aceitar:
                    {
                        var nivel = ((PlayerMobile)from).NivelBanco;
                        var i = BankLevels.Niveis[nivel + 1];

                        if(Banker.Withdraw(from, i.Preco))
                        {
                            ((PlayerMobile)from).NivelBanco++;
                            from.SendMessage("Voce aprimorou seu banco para guardar mais items");
                            from.CloseGump(typeof(UpgradeBankGump));
                            from.CloseGump(typeof(BankInfoGump));
                            from.SendGump(new BankInfoGump(from));
                        } else
                        {
                            from.SendMessage("Voce nao tem dinheiro suficiente na sua conta bancaria");
                        }

                        break;
                    }
                case (int)Buttons.Cancelar:
                    {
                        from.CloseGump(typeof(UpgradeBankGump));
                        break;
                    }

            }
        }
    }
}
