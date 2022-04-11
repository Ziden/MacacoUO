using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Items;
using Server.Mobiles;
using Server.Commands;

namespace Server.Gumps
{
    public class DannyquestGump : Gump
    {
        public static void Initialize()
        {
            CommandSystem.Register("DannyquestGump", AccessLevel.GameMaster, new CommandEventHandler(DannyquestGump_OnCommand));
        }

        private static void DannyquestGump_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new DannyquestGump(e.Mobile));
        }

        public DannyquestGump(Mobile owner) : base(50, 50)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            //----------------------------------------------------------------------------------------------------

            AddPage(0);
            AddImageTiled(13, 5, 382, 433, 2524);
            AddImageTiled(9, 6, 388, 7, 40);
            AddImageTiled(11, 433, 382, 9, 40);
            AddImage(13, 18, 3005, 1152);
            AddImage(389, 188, 3003, 1152);
            AddImage(13, 187, 3005, 1152);
            AddImage(389, 17, 3003, 1152);
            AddImageTiled(15, 421, 376, 12, 50);
            AddImage(46, 12, 2080);
            AddTextEntry(82, 25, 170, 20, 33, 0, @"Easter Egg Hunt Quest!");
            // AddTextEntry(69, 52, 200, 20, 58, 0, @"Bring Me Back An Easter Eggs!");

            AddHtml(31, 93, 346, 281, "<BODY>" +
//----------------------/----------------------------------------------/
"<BASEFONT COLOR=GREEN>Olá, lindo dia!!<BR><BR>" +
"<BASEFONT COLOR=GREEN>Por que eu estaria andando por aqui, você pergunta?!<BR><BR>" +
"<BASEFONT COLOR=GREEN>O Dia de Páscoa não está longe e me disseram<BR><BR>" +
"<BASEFONT COLOR=GREEN>Há alguns ovos de pascoa especiais em torno da regiao!<BR><BR>" +
"<BASEFONT COLOR=GREEN>Então eu tenho tentado encontrar alguns desses para minha garota!<BR><BR>" +
"<BASEFONT COLOR=GREEN>Devem estar bem escondidas porque até agora só encontrei 2!<BR><BR>" +
"<BASEFONT COLOR=GREEN>Faça um acordo! Se você puder me trazer de volta, digamos!<BR><BR>" +
"<BASEFONT COLOR=GREEN>10 daqueles ovos de páscoa especiais, eu poderia te dar um muito<BR><BR>" +
"<BASEFONT COLOR=GREEN>Boa caixa de páscoa com um item dentro, em troca desses ovos!<BR><BR>" +
"<BASEFONT COLOR=GREEN>Antes que eu esqueça de te dizer, estarei aqui esperando por você!<BR><BR>" +
"<BASEFONT COLOR=GREEN>Aqui eu vou te dar essa Cesta de Páscoa para coletar os ovos!<BR><BR>" +
"<BASEFONT COLOR=GREEN>Ah, você precisará empilhar 10 ovos de páscoa especiais de uma cor<BR><BR>" +
                             "</BODY>", false, true);

            AddButton(163, 385, 247, 248, 0, GumpButtonType.Reply, 0);
            AddItem(23, 66, 10248, 24);
            AddItem(327, 66, 10248, 33);





            //--------------------------------------------------------------------------------------------------------------
        }

        public override void OnResponse(NetState state, RelayInfo info) //Function for GumpButtonType.Reply Buttons 
        {
            Mobile from = state.Mobile;

            switch (info.ButtonID)
            {
                case 0: //Case uses the ActionIDs defenied above. Case 0 defenies the actions for the button with the action id 0 
                    {


                        break;
                    }

            }
        }
    }
}
