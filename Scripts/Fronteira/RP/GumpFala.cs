


using Server.Network;
using Server.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Gumps
{

    public enum Faces
    {
        GM_PRETO = 2741,
        PUNK_BARBUDO = 2734,
        FADA = 1641,
        CACHORRO = 2745,
        ENGENHEIRA = 2740
    }

    public class Fala
    {
        public Faces Face;
        public string[] Texto;

        public Fala(Faces face, params string[] falas)
        {
            Texto = falas;
            Face = face;
        }

        public Fala(Faces face)
        {
            Face = face;
        }

        public Fala Textos(params string[] texto)
        {
            Texto = texto;
            return this;
        }

        public Fala(Faces face, string falas)
        {
            Texto = new string[1] { falas };
            Face = face;
        }

        public Fala(Faces face, string falas, string falas2)
        {
            Texto = new string[2] { falas, falas2 };
            Face = face;
        }
    }


    public class GumpFala : Gump
    {

        private Action<int> Callback;


        private static void FalaResponse(Mobile m, List<Fala> lista, Action prox)
        {
            var proxima = lista.FirstOrDefault();
            if (proxima == null)
            {
                prox();
                return;
            }
               
            m.SendGump(new GumpFala(_ =>
            {
                lista.RemoveAt(0);
                FalaResponse(m, lista, prox);
            }, proxima.Face, proxima.Texto));
        }

        public static void MostraFalas(Mobile m, Action prox, params Fala[] falas)
        {
            var popFalas = new List<Fala>(falas);
            FalaResponse(m, popFalas, prox);
        }

        public GumpFala(Action<int> callback, Faces face = Faces.GM_PRETO, params string [] lines) : base(0, 0)
        {
            this.Callback = callback;
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            var oX = 0;
            var oY = 0;
            if(face == Faces.FADA)
            {
                oX = 20;
                oY = 20;
            }

            AddPage(0);
            AddBackground(81, 29, 627, 256, 3000);
            //AddHtml(248, 269, 411, 21, titulo, (bool)false, (bool)false);
            AddBackground(86, 33, 154, 148, 3500);
            AddBackground(238, 35, 457, 146, 3500);
            AddHtml(259, 51, 416, 110, string.Join("</br>", lines), (bool)false, (bool)false);
            AddImage(108+oX, 52+oY, (int)face);
            AddImage(187, 172, 1520);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            if(this.Callback != null)
                this.Callback(info.ButtonID);
        }
    }
}
