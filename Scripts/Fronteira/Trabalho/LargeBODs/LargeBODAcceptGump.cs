using System;
using Server.Gumps;
using Server.Network;
using Server.Ziden.Traducao;

namespace Server.Engines.BulkOrders
{
    public class LargeBODAcceptGump : Gump
    {
        private readonly LargeBOD m_Deed;
        private readonly Mobile m_From;
        public LargeBODAcceptGump(Mobile from, LargeBOD deed)
            : base(50, 50)
        {
            m_From = from;
            m_Deed = deed;

            m_From.CloseGump(typeof(LargeBODAcceptGump));
            m_From.CloseGump(typeof(SmallBODAcceptGump));

            LargeBulkEntry[] entries = deed.Entries;

            AddPage(0);

            AddBackground(25, 10, 430, 240 + (entries.Length * 24), 1579);

            //AddImageTiled(33, 20, 413, 221 + (entries.Length * 24), 2624);
            AddAlphaRegion(33, 20, 413, 221 + (entries.Length * 24));

            AddImage(20, 5, 10460);
            AddImage(430, 5, 10460);
            AddImage(20, 225 + (entries.Length * 24), 10460);
            AddImage(430, 225 + (entries.Length * 24), 10460);

            AddHtmlLocalized(180, 25, 120, 20, 1045134, 1, false, false); // A large bulk order

            AddHtmlLocalized(40, 48, 350, 20, 1045135, 1, false, false); // Ah!  Thanks for the goods!  Would you help me out?

            AddHtmlLocalized(40, 72, 210, 20, 1045138, 1, false, false); // Amount to make:
            AddLabel(250, 72, 1152, deed.AmountMax.ToString());

            AddHtmlLocalized(40, 96, 120, 20, 1045137, 1, false, false); // Items requested:

            int y = 120;

            for (int i = 0; i < entries.Length; ++i, y += 24)
            {
                var nome = Trads.GetNome(entries[i].Details.Type);
                if(nome== null)
                {
                    AddHtmlLocalized(40, y, 210, 20, entries[i].Details.Number, 1, false, false);
                } else
                {
                    AddHtml(40, y, 210, 20, nome, 1, false, false);
                }
            }

            if (deed.RequireExceptional || deed.Material != BulkMaterialType.None)
            {
                AddHtmlLocalized(40, y, 210, 20, 1045140, 1, false, false); // Special requirements to meet:
                y += 24;

                if (deed.RequireExceptional)
                {
                    AddHtmlLocalized(40, y, 350, 20, 1045141, 1, false, false); // All items must be exceptional.
                    y += 24;
                }

                if (deed.Material != BulkMaterialType.None)
                {
                    AddHtml(40, y, 350, 20, "Feitos de "+deed.Material, 1, false, false); // All items must be made with x material.
                    y += 24;
                }
            }

            AddHtmlLocalized(40, 192 + (entries.Length * 24), 350, 20, 1045139, 1, false, false); // Do you want to accept this order?

            AddButton(100, 216 + (entries.Length * 24), 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(135, 216 + (entries.Length * 24), 120, 20, 1006044, 1, false, false); // Ok

            AddButton(275, 216 + (entries.Length * 24), 4005, 4007, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(310, 216 + (entries.Length * 24), 120, 20, 1011012, 1, false, false); // CANCEL
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
                if (m_From.AddToBackpack(m_Deed))
                {
                    m_From.SendMessage("Voce pegou uma ordem de compra"); // The bulk order deed has been placed in your backpack.
                }
                else
                {
                    m_From.SendMessage("Nao tem espaco na sua mochila e a ordem foi adicionada no seu banco"); // There is not enough room in your backpack for the deed.
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
