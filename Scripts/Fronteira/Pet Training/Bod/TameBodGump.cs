using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System.Collections.Generic;
using Server.Items;
using Server.Ziden.Traducao;
using Server.Targeting;

namespace Server.Engines.BulkOrders
{
    public class TameBodGump : Gump
    {
        private readonly BodTamer m_Deed;
        private readonly Mobile m_From;

        public TameBodGump(Mobile from, BodTamer deed)
            : base(25, 25)
        {
            this.m_From = from;
            this.m_Deed = deed;

            this.m_From.CloseGump(typeof(TameBodGump));
            this.m_From.CloseGump(typeof(LargeBODGump));
            this.m_From.CloseGump(typeof(SmallBODGump));

            this.AddPage(0);

            int height = 0;

            this.AddBackground(50, 10, 455, 245 + height, 1579);
            //this.AddImageTiled(58, 20, 438, 226 + height, 2624);
            this.AddAlphaRegion(58, 20, 438, 226 + height);

            this.AddImage(45, 5, 10460);
            this.AddImage(480, 5, 10460);
            this.AddImage(45, 230 + height, 10460);
            this.AddImage(480, 230 + height, 10460);

            this.AddHtmlLocalized(225, 25, 120, 20, 1045133, 1, false, false); // A bulk order

            this.AddHtml(75, 48, 250, 20, "Quantidade:", 1, false, false); // Amount to make:
            this.AddLabel(275, 48, 1152, deed.Quantidade.ToString());

            this.AddHtmlLocalized(275, 76, 200, 20, 1045153, 1, false, false); // Amount finished:
            this.AddHtml(75, 72, 120, 20, "Animal:", 1, false, false); // Item requested:

            var id = ShrinkTable.Lookup(deed.GraficoBody);
            this.AddItem(400, 72, id, deed.Cor);
            var nome = deed.Nome;
            this.AddHtml(75, 96, 210, 20, nome, 1, false, false);
            this.AddLabel(275, 96, 0x480, deed.QuantidadeAtual.ToString());

            int y = 120;
            //BODContext c = BulkOrderSystem.GetContext((PlayerMobile)from);

            //AddHtml(75, y, 300, 20, String.Format("Vale {0} pontos", deed.Pontos.ToString("0.00")), 1, false, false); // Worth ~1_POINTS~ turn in points and ~2_POINTS~ bank points.
            //y += 24;

            /*
            AddButton(125, y, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddHtml(160, y, 300, 20, c.PointsMode == PointsMode.Enabled ? "Pontos Habilitados" : c.PointsMode == PointsMode.Disabled ? "Pontos Desabilitados" : "Pontos Automaticos", 1, false, false);  // Banking Points Enabled/Disabled/Automatic
            y += 24;
            */

            if (m_Deed.QuantidadeAtual < m_Deed.Quantidade)
            {
                AddButton(125, y, 4005, 4007, 2, GumpButtonType.Reply, 0);
                AddHtml(160, y, 300, 20, "Combine com a criatura", 1, false, false); // Combine this deed with the item requested.
            }

            //this.AddButton(125, y, 4005, 4007, 1, GumpButtonType.Reply, 0);
            //this.AddHtmlLocalized(160, y, 120, 20, 1011441, 1, false, false); // EXIT
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (this.m_Deed.Deleted || !this.m_Deed.IsChildOf(this.m_From.Backpack))
                return;

            switch (info.ButtonID)
            {
                case 2: // Combine
                    {
                        if (m_Deed.QuantidadeAtual >= m_Deed.Quantidade)
                        {
                            m_From.SendMessage("Completou ja...");
                            return;
                        }
                        m_From.Target = new IT(m_Deed);
                        m_From.SendMessage("Escolha o animal");
                        break;
                    }
            }
        }



        public class IT : Target
        {
            BodTamer deed;

            public IT(BodTamer deed) : base(10, false, TargetFlags.None)
            {
                this.deed = deed;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                var bc = targeted as BaseCreature;
                if (bc == null)
                {
                    from.SendMessage("Isto nao eh uma criatura");
                    return;
                }
                if (bc.ControlMaster != from)
                {
                    from.SendMessage("Esta criatura nao eh sua");
                    return;
                }

                if (bc.Summoned)
                {
                    from.SendMessage("Esta criatura foi invocada");
                    return;
                }

                // if (deed.Nome == bc.Name && (bc is Bird || bc.Name.ToLower() == "cavalo" || deed.Cor == bc.Hue))
                
                if ((!(bc is IMount) && (bc.GetType()==deed.Tipo)) || (deed.Nome == bc.Name && (deed.GraficoBody == bc.BodyValue || BodTamer.BodyValido(bc.GetType(), deed.GraficoBody))))
                {
                    bc.Delete();
                    deed.QuantidadeAtual++;
                    if (deed.QuantidadeAtual == deed.Quantidade)
                    {
                        deed.PrivateOverheadMessage(MessageType.Regular, 0, true, "* Completa *", from.NetState);
                    }
                }
                else
                {
                    if (Shard.DebugEnabled)
                    {
                        Shard.Debug(deed.Nome + " vs " + bc.Name);
                        Shard.Debug(deed.Cor + " vs " + bc.Hue);
                    }
                    from.SendMessage("Esta criatura aparenta ser diferente, nome ou cor...");
                }
                from.SendGump(new TameBodGump(from, deed));
            }
        }
    }
}
