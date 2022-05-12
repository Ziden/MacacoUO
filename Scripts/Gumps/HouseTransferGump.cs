using System;
using Server.Multis;
using Server.Network;

namespace Server.Gumps
{
    public class HouseTransferGump : Gump
    {
        private readonly Mobile m_From;
        private readonly Mobile m_To;
        private readonly BaseHouse m_House;
        public HouseTransferGump(Mobile from, Mobile to, BaseHouse house)
            : base(110, 100)
        {
            this.m_From = from;
            this.m_To = to;
            this.m_House = house;

            this.Closable = false;

            this.AddPage(0);

            this.AddBackground(0, 0, 420, 280, 9350);

            this.AddImageTiled(10, 10, 400, 20, 2624);
            this.AddAlphaRegion(10, 10, 400, 20);

            this.AddHtmlLocalized(10, 10, 400, 20, 1060635, 30720, false, false); // <CENTER>ATENCAO</CENTER>

            this.AddImageTiled(10, 40, 400, 200, 2624);
            this.AddAlphaRegion(10, 40, 400, 200);

            /* Outro jogador está tentando iniciar uma troca de casa com você.
                * Para que você veja esta janela, você e a outra pessoa estão a dois passos da casa a ser negociada.
                * Se você clicar em OK abaixo, uma rolagem de negociação da casa aparecerá em sua janela de negociação e você poderá concluir a transação.
                * Este pergaminho é de uma cor azul distinta e mostrará o nome da casa, o nome do proprietário dessa casa e as coordenadas do sextante do centro da casa quando você passar o mouse sobre ele.
                * Para que a transação seja bem-sucedida, ambos devem aceitar a troca e ambos devem permanecer a dois passos do sinal da casa.
                * <BR><BR>Aceitar esta casa em troca irá <a href = "?ForceTopic97">condenar</a> todas e quaisquer outras casas que você possa ter.
                * Todas as suas casas em <U>todos os fragmentos</U> serão afetadas.
                * <BR><BR>Além disso, você não poderá colocar outra casa ou transferir uma para você por uma (1) semana real.<BR><BR>
                * Depois de aceitar estes termos, esses efeitos não podem ser revertidos.
                * Reescrever ou transferir sua nova casa <U>não</U> não condenará sua(s) outra(s) casa(s) nem o cronômetro de uma semana será removido.<BR><BR>
                * Se você tiver certeza absoluta de que deseja continuar, clique no botão ao lado de OK, abaixo.
                * Caso não deseje trocar por esta casa, clique em CANCELAR.
                */


            this.AddHtmlLocalized(10, 40, 400, 200, 1062086, 32512, false, true);

            this.AddImageTiled(10, 250, 400, 20, 2624);
            this.AddAlphaRegion(10, 250, 400, 20);

            this.AddButton(10, 250, 30533, 30534, 1, GumpButtonType.Reply, 0);
            this.AddHtmlLocalized(40, 250, 170, 20, 1011036, 32767, false, false); // OKAY

            this.AddButton(210, 250, 30533, 30535, 0, GumpButtonType.Reply, 0);
            this.AddHtmlLocalized(240, 250, 170, 20, 1011012, 32767, false, false); // CANCEL
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (info.ButtonID == 1 && !this.m_House.Deleted)
                this.m_House.EndConfirmTransfer(this.m_From, this.m_To);
        }
    }
}
