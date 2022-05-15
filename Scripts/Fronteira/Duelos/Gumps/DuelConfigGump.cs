using System;
using Server;
using Server.Gumps;

namespace Server.Dueling
{
    public class DuelConfigGump : Gump
    {
        private Duel _Duel;
        private bool _DeclinesDuels;

        public DuelConfigGump( Duel duel )
            : base( 200, 00 )
        {
            _Duel = duel;
            _DeclinesDuels = DuelController.DeclineDuelList.Contains( _Duel.Creator );

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            this.AddPage( 0 );
            this.AddBackground( 36, 25, 402, 426, 9350);
            this.AddItem( 172, 276, 7410 );
            this.AddItem( 127, 179, 7414 );
            this.AddItem( 261, 143, 7420 );
            this.AddItem( 284, 164, 7421 );
            this.AddItem( 194, 291, 7409 );
            this.AddItem( 238, 138, 7419 );
            this.AddItem( 240, 236, 7427 );
            this.AddItem( 194, 137, 7417 );
            this.AddItem( 171, 143, 7416 );
            this.AddItem( 149, 158, 7415 );
            this.AddItem( 104, 214, 7413 );
            this.AddItem( 127, 223, 7412 );
            this.AddItem( 306, 223, 7424 );
            this.AddItem( 150, 209, 7431 );
            this.AddItem( 172, 231, 7430 );
            this.AddItem( 195, 246, 7429 );
            this.AddItem( 217, 272, 7428 );
            this.AddItem( 240, 291, 7427 );
            this.AddItem( 262, 271, 7426 );
            this.AddItem( 284, 251, 7425 );
            this.AddItem( 216, 180, 7441 );
            this.AddItem( 193, 203, 7440 );
            this.AddItem( 217, 236, 7439 );
            this.AddItem( 328, 214, 7423 );
            this.AddItem( 150, 245, 7411 );
            this.AddItem( 284, 205, 7436 );
            this.AddItem( 267, 176, 7435 );
            this.AddItem( 239, 156, 7434 );
            this.AddItem( 193, 156, 7433 );
            this.AddItem( 172, 179, 7432 );
            this.AddItem( 245, 203, 7442 );
            this.AddItem( 215, 138, 7418 );
            this.AddItem( 306, 178, 7422 );
            this.AddItem( 262, 222, 7437 );
            this.AddItem( 236, 251, 7438 );
            this.AddItem( 212, 204, 7960 );
            this.AddLabel( 166, 40, 36, @"Duelos Dragonic Age" );
            this.AddLabel( 166, 39, 36, @"Duelos Dragonic Age");
            this.AddLabel( 136, 62, 36, @"Escolha o tamanho do duelo." );
            this.AddLabel( 136, 61, 36, @"Escolha o tamanho do duelo."); 
            this.AddLabel( 154, 116, 36, @"1vs1" );
            this.AddLabel( 154, 115, 36, @"1vs1" );
            this.AddLabel( 287, 116, 36, @"2vs2" );
            this.AddLabel( 287, 115, 36, @"2vs2" );
            this.AddLabel( 98, 227, 36, @"3vs3" );
            this.AddLabel( 98, 226, 36, @"3vs3" );
            this.AddLabel( 348, 227, 36, @"4vs4" );
            this.AddLabel( 348, 226, 36, @"4vs4" );
            this.AddLabel( 223, 307, 36, @"5vs5" );
            this.AddLabel( 223, 306, 36, @"5vs5" );
            this.AddLabel( 65, 424, 36, @"" );
            this.AddLabel( 65, 423, 36, @"" );
            this.AddLabel( 79, 392, 36, @"Aceitar Convites" );
            this.AddLabel( 79, 391, 36, @"Aceitar Convites");
            this.AddButton( 401, 46, 3, 4, ( int )Buttons.closeBtn, GumpButtonType.Reply, 0 );
            this.AddButton( 159, 137, 30533, 30534, ( int )Buttons.oneBtn, GumpButtonType.Reply, 0 );
            this.AddButton( 294, 137, 30533, 30534, ( int )Buttons.twoBtn, GumpButtonType.Reply, 0 );
            this.AddButton( 115, 246, 30533, 30534, ( int )Buttons.threeBtn, GumpButtonType.Reply, 0 );
            this.AddButton( 344, 246, 30533, 30534, ( int )Buttons.fourBtn, GumpButtonType.Reply, 0 );
            this.AddButton( 230, 327, 30533, 30534, ( int )Buttons.fiveBtn, GumpButtonType.Reply, 0 );

            this.AddButton( 54, 387, _DeclinesDuels ? 9792 : 9793, _DeclinesDuels ? 9792 : 9793, ( int )Buttons.toggleBtn, GumpButtonType.Reply, 0 );
        }

        public enum Buttons
        {
            closeBtn,
            oneBtn,
            twoBtn,
            threeBtn,
            fourBtn,
            fiveBtn,
            toggleBtn,
        }

        public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
        {
            Mobile m = sender.Mobile;

            if( m == null || _Duel == null )
                return;

            if( _DeclinesDuels && ( (Buttons)info.ButtonID != Buttons.toggleBtn && (Buttons)info.ButtonID != Buttons.closeBtn ) )
            {
                m.SendMessage( "Selecione para aceitar duelos antes de iniciar um duelo." );
                m.SendGump( new DuelConfigGump( _Duel ) );
                return;
            }

            switch( ( Buttons )info.ButtonID )
            {
                case Buttons.closeBtn:
                    {
                        DuelController.DestroyDuel( _Duel );
                        m.SendMessage( "Duelo nao criado." );
                        break;
                    }
                case Buttons.oneBtn:
                    {
                        _Duel.SpotsRemaing = 1;
                        _Duel.Configure( 1 );
                        _Duel.Contestants.Add( _Duel.Creator );

                        m.SendMessage( "Escolha o jogador." );
                        m.Target = new DuelTarget( m, _Duel );

                        break;
                    }
                case Buttons.twoBtn:
                    {
                        _Duel.SpotsRemaing = 3;
                        _Duel.Configure( 2 );
                        _Duel.Contestants.Add( _Duel.Creator );

                        m.SendMessage( "Escolha o jogador." );
                        m.Target = new DuelTarget( m, _Duel );
                        break;
                    }
                case Buttons.threeBtn:
                    {
                        _Duel.SpotsRemaing = 5;
                        _Duel.Configure( 3 );
                        _Duel.Contestants.Add( _Duel.Creator );

                        m.SendMessage("Escolha o jogador.");
                        m.Target = new DuelTarget( m, _Duel );
                        break;
                    }
                case Buttons.fourBtn:
                    {
                        _Duel.SpotsRemaing = 7;
                        _Duel.Configure( 4 );
                        _Duel.Contestants.Add( _Duel.Creator );

                        m.SendMessage("Escolha o jogador.");
                        m.Target = new DuelTarget( m, _Duel );
                        break;
                    }
                case Buttons.fiveBtn:
                    {
                        _Duel.SpotsRemaing = 9;
                        _Duel.Configure( 5 );
                        _Duel.Contestants.Add( _Duel.Creator );

                        m.SendMessage("Escolha o jogador.");
                        m.Target = new DuelTarget( m, _Duel );
                        break;
                    }
                case Buttons.toggleBtn:
                    {
                        if( _DeclinesDuels )
                            DuelController.DeclineDuelList.Remove( _Duel.Creator );
                        else
                            DuelController.DeclineDuelList.Add( _Duel.Creator );

                        m.CloseGump( typeof( DuelConfigGump ) );
                        m.SendGump( new DuelConfigGump( _Duel ) );

                        break;
                    }
            }
        }

    }
}
