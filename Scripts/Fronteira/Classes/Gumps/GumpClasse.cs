using Server.Network;
using Server.Commands;
using Server.Fronteira.Classes;
using Server.Mobiles;

namespace Server.Gumps
{
    public class GumpClasse : Gump
    {
        Mobile caller;

        public static void Initialize()
        {
            CommandSystem.Register("testeclasses", AccessLevel.Administrator, new CommandEventHandler(_OnCommand));
        }

        [Usage("")]
        [Description("Makes a call to your custom gump.")]
        public static void _OnCommand(CommandEventArgs e)
        {
            var caller = e.Mobile;
            caller.SendGump(new RPClassGump());
            //Mostra(caller);
        }

        public static void Mostra(Mobile caller)
        {
            if (caller.HasGump(typeof(GumpClasse)))
                caller.CloseGump(typeof(GumpClasse));
            caller.SendGump(new GumpClasse(ClassDef.GetClasses()[0]));
        }

        public static string TUTORIAL_DOS_MAMAO = @"À esquerda estão listadas as classes da raça escolhida. Abaixo estão listadas todas as skills que você pode escolher treinar naquela classe. Você vai começar com 30 de cada uma das skills específicas da classe. A quantidade máxima de skill que um char pode ter é 700. Todas as skills permitidas da classe podem ser treinadas e esquecidas durante o jogo. Dentro de cada classe existem duas subclasses que liberam mais skills enquanto seu char vai evoluindo. Essa é uma ótima maneira de você testar várias builds diferentes dentro da mesma classe.";

        private ClassePersonagem escolhida;

        public GumpClasse(ClassePersonagem classeEscolhida) : base(0, 0)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            this.escolhida = classeEscolhida;
               
            AddPage(0);
            AddBackground(113, 64, 500, 500, 9250);

            var x = 0;
            var y = 0;

            foreach(var cl in ClassDef.GetClasses())
            {
                AddHtml(362, 180+y, 111, 15, Gump.Cor(cl.Nome, "white"), (bool)false, (bool)false);
                var id = 210;
                if (cl == classeEscolhida)
                    id = 211;
                AddButton(344, 180+y, id, 40310, cl.ID+1, GumpButtonType.Reply, 0);
                y += 20;
            }

            AddBackground(475, 154, 215, 326, 3500);

            if (classeEscolhida != null)
            {
                AddImage(480, 164, classeEscolhida.Icone);
                AddHtml(705, 180, 212, 301, classeEscolhida.Desc, (bool)true, (bool)true);
                var strSkills = "";
                foreach (var skill in classeEscolhida.ClassSkills.Keys)
                {
                    strSkills += classeEscolhida.ClassSkills[skill] + " "+skill.GetName() + " | " ;
                }
                AddHtml(345, 544, 574, 55, Gump.Cor(strSkills, "white"), (bool)false, (bool)false);
                AddHtml(704, 146, 207, 22, Gump.Cor(classeEscolhida.Nome, "white"), (bool)false, (bool)false);
            }
         
            AddHtml(110, 314, 179, 312, Gump.Cor(TUTORIAL_DOS_MAMAO, "white"), (bool)false, (bool)false);
            AddHtml(353, 148, 115, 17, Gump.Cor("Classes", "white"), (bool)false, (bool)false);
 
            AddButton(890, 604, 4005, 4007, 666, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == 666 && escolhida != null)
            {
                escolhida.ViraClasse(from as PlayerMobile);
            }

            if (info.ButtonID == 0)
                return;

            if(info.ButtonID != 666)
            {
                var classe = ClassDef.GetClasse(info.ButtonID-1);
                from.CloseGump(typeof(GumpClasse));
                from.SendGump(new GumpClasse(classe));
            }
        }
    }
}
