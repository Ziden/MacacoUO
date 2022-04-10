using System;
using Server.Gumps;
using Server.Network;
using Server.Ziden.Traducao;

namespace Server.Engines.BulkOrders
{
    public class SmallBODAcceptGump : Gump
    {
        private readonly SmallBOD m_Deed;
        private readonly Mobile m_From;
        public SmallBODAcceptGump(Mobile from, SmallBOD deed)
            : base(50, 50)
        {
            m_From = from;
            m_Deed = deed;

            m_From.CloseGump(typeof(LargeBODAcceptGump));
            m_From.CloseGump(typeof(SmallBODAcceptGump));

            AddPage(0);

            AddBackground(25, 10, 430, 264, 1579);

            //AddImageTiled(33, 20, 413, 245, 2624);
            AddAlphaRegion(33, 20, 413, 245);

            AddImage(20, 5, 10460);
            AddImage(430, 5, 10460);
            AddImage(20, 249, 10460);
            AddImage(430, 249, 10460);

            AddHtmlLocalized(190, 25, 120, 20, 1045133, 1, false, false); // A bulk order
            AddHtmlLocalized(40, 48, 350, 20, 1045135, 1, false, false); // Ah!  Thanks for the goods!  Would you help me out?

            AddHtmlLocalized(40, 72, 210, 20, 1045138, 1, false, false); // Amount to make:
            AddLabel(250, 72, 1152, deed.AmountMax.ToString());

            AddHtmlLocalized(40, 96, 120, 20, 1045136, 1, false, false); // Item requested:
            AddItem(355, 96, deed.Graphic, deed.GraphicHue);
            var nome = Trads.GetNome(deed.Type);
            if(nome!=null) 
                AddHtml(40, 120, 210, 20, nome, 1, false, false);
            else
                AddHtmlLocalized(40, 120, 210, 20, deed.Number, 1, false, false);

            if (deed.RequireExceptional || deed.Material != BulkMaterialType.None)
            {
                AddHtmlLocalized(40, 144, 210, 20, 1045140, 1, false, false); // Special requirements to meet:

                if (deed.RequireExceptional)
                    AddHtmlLocalized(40, 168, 350, 20, 1045141, 1, false, false); // All items must be exceptional.

                if (deed.Material != BulkMaterialType.None)
                    AddHtml(40, deed.RequireExceptional ? 192 : 168, 350, 20, "Feitos de " + deed.Material, 1, false, false);
             
            }

            AddHtmlLocalized(40, 216, 350, 20, 1045139, 1, false, false); // Do you want to accept this order?

            AddButton(100, 240, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(135, 240, 120, 20, 1006044, 1, false, false); // Ok

            AddButton(275, 240, 4005, 4007, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(310, 240, 120, 20, 1011012, 1, false, false); // CANCEL
        }

        public override void OnServerClose(NetState owner)
        {
            Timer.DelayCall(() =>
            {
                if (m_Deed.Map == null || m_Deed.Map == Map.Internal)
                {
                    m_Deed.Delete();
                }
            });
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1) // Ok
            {
                if (m_From._PlaceInBackpack(m_Deed))
                {
                    m_From.SendMessage("Voce pegou a ordem"); // The bulk order deed has been placed in your backpack.
                }
                else
                {
                    m_From.SendMessage("Nao tem espaco suficiente na sua mochila para a ordem de compra e ela foi adicionada no seu banco"); // There is not enough room in your backpack for the deed.
                    m_From.BankBox.AddItem(m_Deed);
                }
            }
            else
            {
                m_Deed.Delete();
            }
        }
    }
}
