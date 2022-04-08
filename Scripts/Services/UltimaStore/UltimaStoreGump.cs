using System;
using System.Collections.Generic;
using System.Linq;

using Server.Mobiles;
using Server.Gumps;
using Server.Network;
using Server.Services.UltimaStore;

namespace Server.Engines.UOStore
{
    public class UltimaStoreGump : BaseGump
    {
        private readonly int[][] _Offset =
        {
            new[] { 167, 74 },
            new[] { 354, 74 },
            new[] { 541, 74 },
            new[] { 167, 294 },
            new[] { 354, 294 },
            new[] { 541, 294 }
        };

        public StoreCategory Category
        {
            get
            {
                var profile = UltimaStore.GetProfile(User, false);

                if (profile != null)
                {
                    return profile.Category;
                }

                return PlayerProfile.DefaultCategory;
            }
        }

        public SortBy SortBy
        {
            get
            {
                var profile = UltimaStore.GetProfile(User, false);

                if (profile != null)
                {
                    return profile.SortBy;
                }

                return PlayerProfile.DefaultSortBy;
            }
        }

        public Dictionary<StoreEntry, int> Cart
        {
            get
            {
                var profile = UltimaStore.GetProfile(User, false);

                if (profile != null)
                {
                    return profile.Cart;
                }

                return null;
            }
        }

        public int Page { get; private set; }
        public string SearchText { get; private set; }
        public List<StoreEntry> StoreList { get; private set; }

        public bool Search { get; private set; }

        public UltimaStoreGump(PlayerMobile pm)
            : base(pm, 100, 200)
        {
            StoreList = UltimaStore.GetList(Category);

            UltimaStore.SortList(StoreList, SortBy);

            pm.Frozen = true;
            //pm.Hidden = true;
            pm.TempSquelched = true;
        }

        public override void OnDispose()
        {
            ColUtility.Free(StoreList);

            StoreList = null;
        }

        public static int B1 = 0x9C5F;
        public static int B2 = 0x9C55;

        public static int B3 = 0x9C4F;
        public static int B4 = 0x9C4E;

        public static int I1 = 0x9C49;


        public override void AddGumpLayout()
        {
            AddPage(0);
            AddImage(0, 0, I1);

            AddECHandleInput();

            AddButton(36, 97, Category == StoreCategory.Featured ? B1 : B2, B1, 100, GumpButtonType.Reply, 0);
            AddHtml(36, 100, 125, 25, " Novidades", 0x7FFF, false, false); // Featured

            AddECHandleInput();
            AddECHandleInput();

            AddButton(36, 126, Category == StoreCategory.Character ? B1 : B2, B1, 101, GumpButtonType.Reply, 0);
            AddHtml(36, 129, 125, 25, " Personagem", 0x7FFF, false, false); // Character

            AddECHandleInput();
            AddECHandleInput();

            AddButton(36, 155, Category == StoreCategory.Equipment ? B1 : B2, B1, 102, GumpButtonType.Reply, 0);
            AddHtml(36, 158, 125, 25, " Equipamento", 0x7FFF, false, false); // Equipment

            AddECHandleInput();
            AddECHandleInput();

            AddButton(36, 184, Category == StoreCategory.Decorations ? B1 : B2, B1, 103, GumpButtonType.Reply, 0);
            AddHtml(36, 187, 125, 25, " Decoracao", 0x7FFF, false, false); // Decorations

            AddECHandleInput();
            AddECHandleInput();

            AddButton(36, 213, Category == StoreCategory.Mounts ? B1 : B2, B1, 104, GumpButtonType.Reply, 0);
            AddHtml(36, 216, 125, 25, " Montarias", 0x7FFF, false, false); // Mounts

            AddECHandleInput();
            AddECHandleInput();

            AddButton(36, 242, Category == StoreCategory.Misc ? B1 : B2, B1, 105, GumpButtonType.Reply, 0);
            AddHtml(36, 245, 125, 25, " Geral", 0x7FFF, false, false); // Miscellaneous

            AddECHandleInput();
            AddECHandleInput();

            AddButton(36, 271, B2, B1, 106, GumpButtonType.Reply, 0);
            AddHtml(36, 274, 125, 25, " Codigo", 0x7FFF, false, false); // Promotional Code

            AddECHandleInput();
            AddECHandleInput();

            AddButton(36, 300, B2, B1, 107, GumpButtonType.Reply, 0);
            AddHtml(36, 303, 125, 25, " FAQ", 0x7FFF, false, false); // FAQ

            AddECHandleInput();

            AddImage(36, 331, 0x9C4A);

            AddHtml(36, 334, 125, 25, " Ordenar", 0x2945, false, false); // Sort By

            AddButton(43, 360, SortBy == SortBy.Name ? 0x9C4F : 0x9C4E, SortBy == SortBy.Name ? 0x9C4F : 0x9C4E, 108, GumpButtonType.Reply, 0);
            AddHtml(68, 360, 88, 25, "Nome", 0x6B55, false, false); // Name

            AddButton(43, 386, SortBy == SortBy.PriceLower ? 0x9C4F : 0x9C4E, SortBy == SortBy.PriceLower ? 0x9C4F : 0x9C4E, 109, GumpButtonType.Reply, 0);
            AddHtml(68, 386, 88, 25, "Preco Baixo", 0x6B55, false, false); // Price Down
            AddImage(110, 386, 0x9C60);

            AddButton(43, 412, SortBy == SortBy.PriceHigher ? 0x9C4F : 0x9C4E, SortBy == SortBy.PriceHigher ? 0x9C4F : 0x9C4E, 110, GumpButtonType.Reply, 0);
            AddHtml(68, 412, 88, 25, "Preco Alto", 0x6B55, false, false); // Price Up
            AddImage(110, 412, 0x9C61);

            AddButton(43, 438, SortBy == SortBy.Newest ? 0x9C4F : 0x9C4E, SortBy == SortBy.Newest ? 0x9C4F : 0x9C4E, 111, GumpButtonType.Reply, 0);
            AddHtml(68, 438, 88, 25, "Novo", 0x6B55, false, false); // Newest

            AddButton(43, 464, SortBy == SortBy.Oldest ? 0x9C4F : 0x9C4E, SortBy == SortBy.Oldest ? 0x9C4F : 0x9C4E, 112, GumpButtonType.Reply, 0);
            AddHtml(68, 464, 88, 25, "Velho", 0x6B55, false, false); // Oldest

            AddECHandleInput();

            AddButton(598, 36, Category == StoreCategory.Cart ? 0x9C5E : 0x9C54, 0x9C5E, 113, GumpButtonType.Reply, 0);
            AddHtmlLocalized(618, 39, 123, 25, 1156593, String.Format("@{0}@{1}", UltimaStore.CartCount(User), Configuration.CartCapacity), 0x7FFF, false, false);

            AddECHandleInput();

            AddBackground(167, 516, 114, 22, 0x2486);
            AddTextEntry(169, 518, 110, 18, 0, 16, "", 169);

            AddECHandleInput();

            AddButton(286, 516, 0x9C52, 0x9C5C, 114, GumpButtonType.Reply, 0);
            AddHtml(286, 519, 64, 22, " Busca", 0x7FFF, false, false); // Search

            AddECHandleInput();

            AddImage(36, 74, 0x9C56);
            AddLabelCropped(59, 74, 100, 14, 0x1C7, UltimaStore.GetCurrency(User).ToString("N0"));

            AddECHandleInput();

            if (!Search && Category == StoreCategory.Cart)
            {
                var profile = UltimaStore.GetProfile(User);

                AddImage(167, 74, 0x9C4C);

                if (profile != null && profile.Cart != null && profile.Cart.Count > 0)
                {
                    var i = 0;

                    foreach (var kvp in profile.Cart)
                    {
                        var entry = kvp.Key;
                        var amount = kvp.Value;

                        var index = UltimaStore.Entries.IndexOf(entry);

                        if (entry.Name[0].Number > 0)
                            AddHtmlLocalized(175, 84 + (35 * i), 256, 25, entry.Name[0].Number, 0x6B55, false, false);
                        else
                            AddHtml(175, 84 + (35 * i), 256, 25, Color(C16232(0x6B55), entry.Name[0].String), false, false);

                        AddButton(431, 81 + (35 * i), 0x9C52, 0x9C5C, index + 2000, GumpButtonType.Reply, 0);

                        AddLabelCropped(457, 82 + (35 * i), 38, 22, 0x9C2, amount.ToString());
                        AddLabelCropped(531, 82 + (35 * i), 100, 14, 0x1C7, (entry.Cost * amount).ToString("N0"));

                        AddButton(653, 81 + (35 * i), 0x9C52, 0x9C5C, index + 3000, GumpButtonType.Reply, 0);
                        AddHtml(653, 84 + (35 * i), 64, 22, " Remover", 0x7FFF, false, false); // Remove

                        AddImage(175, 109 + (35 * i), 0x9C4D);

                        ++i;
                    }
                }

                AddHtml(508, 482, 125, 25, " Subtotal", 0x6B55, false, false); // Subtotal:
                AddImage(588, 482, 0x9C56);
                AddLabelCropped(611, 480, 100, 14, 0x1C7, UltimaStore.GetSubTotal(Cart).ToString("N0"));

                AddECHandleInput();
                AddECHandleInput();

                AddButton(653, 516, 0x9C52, 0x9C52, 115, GumpButtonType.Reply, 0);
                AddHtml(653, 519, 64, 22, " Comprar", 0x7FFF, false, false); // Buy
            }
            else
            {
                if (Search)
                {
                    StoreList = UltimaStore.GetSortedList(SearchText);

                    UltimaStore.SortList(StoreList, SortBy);

                    if (StoreList.Count == 0)
                    {
                        User.SendMessage("Nenhum item encontrado"); // No items matched your search.
                        return;
                    }
                }

                var listIndex = Page * 6;
                var pageIndex = 0;
                var pages = (int)Math.Ceiling((double)StoreList.Count / 6);

                for (var i = listIndex; i < StoreList.Count && pageIndex < 6; i++)
                {
                    var entry = StoreList[i];
                    var x = _Offset[pageIndex][0];
                    var y = _Offset[pageIndex][1];

                    AddButton(x, y, 0x9C4B, 0x9C4B, i + 1000, GumpButtonType.Reply, 0);

                    if (entry.Tooltip > 0)
                    {
                        AddTooltip(entry.Tooltip);
                    }
                    /*
                    else if(entry.TooltipStr != null)
                    {
                        //Shard.Debug("Renger TT String " + entry.TooltipStr);
                        //AddTooltip(entry.TooltipStr);
                        //Add(new GumpTooltipStr(entry.TooltipStr));
                    }
                    */
                    else
                    {
                        var item = UltimaStore.UltimaStoreContainer.FindDisplayItem(entry.ItemType);

                        if (item != null)
                        {
                            AddItemProperty(item);
                        }
                    }

                    if (entry.TooltipStr != null)
                    {
                        //AddImage(x, y + 189, 0x9C58);
                        AddButton(x, y + 189, 0x9C58, 0x9C58, i + 10000, GumpButtonType.Reply, 0);
                    }

                    for (int j = 0; j < entry.Name.Length; j++)
                    {
                        if (entry.Name[j].Number > 0)
                            AddHtmlLocalized(x, y + (j * 14) + 4, 183, 25, 1114513, String.Format("#{0}", entry.Name[j].Number.ToString()), 0x7FFF, false, false);
                        else
                            AddHtml(x, y + (j * 14) + 4, 183, 25, ColorAndCenter("#FFFFFF", entry.Name[j].String), false, false);
                    }

                    if (entry.ItemID > 0)
                    {
                        var b = ItemBounds.Table[entry.ItemID];

                        AddItem((x + 91) - b.Width / 2 - b.X, (y + 108) - b.Height / 2 - b.Y, entry.ItemID, entry.Hue);
                    }
                    else
                    {
                        AddImage((x + 91) - 72, (y + 108) - 72, entry.GumpID);
                    }

                    AddImage(x + 60, y + 192, 0x9C56);
                    AddLabelCropped(x + 80, y + 190, 143, 25, 0x9C2, entry.Cost.ToString("N0"));

                    AddECHandleInput();
                    AddECHandleInput();

                    ++pageIndex;
                    ++listIndex;
                }

                if (Page + 1 < pages)
                {
                    AddButton(692, 516, 0x9C51, 0x9C5B, 116, GumpButtonType.Reply, 0);
                }

                if (Page > 0)
                {
                    AddButton(648, 516, 0x9C50, 0x9C5A, 117, GumpButtonType.Reply, 0);
                }
            }

            if (Configuration.CurrencyDisplay)
            {
                //AddHtml(43, 496, 120, 16, Color("#FFFFFF", "Unidades:"), false, false);
                AddHtml(43, 518, 120, 16, Color("#FFFFFF", Configuration.CurrencyName), false, false);
            }
        }

        public bool IsFeatured(StoreEntry entry)
        {
            return entry.Category == StoreCategory.Featured ||
                UltimaStore.Entries.Any(e => e.ItemType == entry.ItemType && e.Category == StoreCategory.Featured);
        }

        public static void ReleaseHidden(PlayerMobile pm)
        {
            if (pm.HasGump(typeof(UltimaStoreGump)) || pm.HasGump(typeof(NoFundsGump)) ||
                pm.HasGump(typeof(ConfirmPurchaseGump)) || pm.HasGump(typeof(ConfirmCartGump)))
            {
                return;
            }

            pm.Frozen = false;
            pm.TempSquelched = false;
            pm.SendMessage("Ajuda abortada"); // Help request aborted.

            if (pm.AccessLevel < AccessLevel.Counselor)
            {
                pm.RevealingAction();
            }
        }

        public override void OnServerClose(NetState owner)
        {
            if (owner.Mobile is PlayerMobile)
            {
                ReleaseHidden((PlayerMobile)owner.Mobile);
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            var id = info.ButtonID;

            if (Shard.DebugEnabled)
                Shard.Debug("Id clicado: " + id.ToString());

            if (id == 0)
            {
                ReleaseHidden(User);
                return;
            }

            var profile = UltimaStore.GetProfile(User);

            switch (id)
            {
                // Change Category
                case 100:
                case 101:
                case 102:
                case 103:
                case 104:
                case 105:
                    {
                        Search = false;

                        var oldCat = profile.Category;

                        profile.Category = (StoreCategory)id - 99;

                        if (oldCat != profile.Category)
                        {
                            StoreList = UltimaStore.GetList(Category);
                            Page = 0;
                        }

                        Refresh();
                        return;
                    }

                // Promo Code
                case 106:
                    {
                        Refresh();
                        SendGump(new PromoCodeGump(User, this));
                        return;
                    }

                // FAQ
                case 107:
                    {
                        User.LaunchBrowser("http://www.dragonicage.com/page/doar.htm");
                        if (!String.IsNullOrWhiteSpace(Configuration.Website))
                        {
                            //User.LaunchBrowser(Configuration.Website);
                        }
                        else
                        {
                            //User.LaunchBrowser("https://www.ultimafronteirashard.com.br/");
                        }
                        Refresh();
                        return;
                    }

                // Change Sort Method
                case 108:
                case 109:
                case 110:
                case 111:
                case 112:
                    {
                        var oldSort = profile.SortBy;

                        profile.SortBy = (SortBy)id - 108;

                        if (oldSort != profile.SortBy)
                        {
                            // re-orders the list
                            if (oldSort == SortBy.Newest || oldSort == SortBy.Oldest)
                            {
                                ColUtility.Free(StoreList);

                                StoreList = UltimaStore.GetList(Category);
                            }

                            UltimaStore.SortList(StoreList, profile.SortBy);

                            Page = 0;
                        }

                        Refresh();
                        return;
                    }

                // Cart View
                case 113:
                    {
                        if (profile != null)
                        {
                            profile.Category = StoreCategory.Cart;
                        }

                        Refresh();
                        return;
                    }

                // Search
                case 114:
                    {
                        var searchTxt = info.GetTextEntry(0);

                        if (searchTxt != null && !String.IsNullOrEmpty(searchTxt.Text))
                        {
                            Search = true;
                            SearchText = searchTxt.Text;
                        }
                        else
                        {
                            User.SendMessage("Texto invalido"); // That text is unacceptable.
                        }

                        Refresh();
                        return;
                    }

                // Buy
                case 115:
                    {
                        if (UltimaStore.CartCount(User) == 0)
                        {
                            if (profile != null)
                            {
                                profile.Category = StoreCategory.Cart;
                            }

                            Refresh();
                            return;
                        }

                        int total = UltimaStore.GetSubTotal(Cart);

                        if (total <= UltimaStore.GetCurrency(User, true))
                        {
                            SendGump(new ConfirmPurchaseGump(User));
                        }
                        else
                        {
                            SendGump(new NoFundsGump(User));
                        }

                        return;
                    }

                // Next Page
                case 116:
                    {
                        ++Page;

                        Refresh();
                        return;
                    }

                // Previous Page
                case 117:
                    {
                        --Page;

                        Refresh();
                        return;
                    }
            }

            if (id < 2000) // Add To Cart
            {
                Refresh();

                var entry = StoreList[id - 1000];

                if (Cart == null || Cart.Count < 10)
                {
                    SendGump(new ConfirmCartGump(User, this, entry));
                    return;
                }

                User.SendMessage("Seu carrinho esta cheio"); // Your store cart is currently full.
            }
            else if (id < 3000) // Change Amount In Cart
            {
                Refresh();

                var entry = UltimaStore.Entries[id - 2000];

                SendGump(new ConfirmCartGump(User, this, entry, Cart != null && Cart.ContainsKey(entry) ? Cart[entry] : 0));
                return;
            }
            else if (id < 4000) // Remove From Cart
            {
                var entry = UltimaStore.Entries[id - 3000];

                if (profile != null)
                {
                    profile.RemoveFromCart(entry);
                }

                Refresh();
                return;
            }
            else if (id >= 10000)
            {
                Refresh();
                var entry = StoreList[id - 10000];
                SendGump(new InfoGump(User, this, (entry.Name.Length > 0 ? entry.Name[0].ToString() : "Erro"), entry.TooltipStr));
                return;
            }
            ReleaseHidden(User);
        }
    }

    public class ConfirmCartGump : BaseGump
    {
        public UltimaStoreGump Gump { get; private set; }
        public StoreEntry Entry { get; private set; }
        public int Current { get; private set; }

        public ConfirmCartGump(PlayerMobile pm, UltimaStoreGump gump, StoreEntry entry, int current = 0)
            : base(pm, gump.X + (760 / 2) - 205, gump.Y + (574 / 2) - 100)
        {
            Gump = gump;
            Entry = entry;
            Current = current;

            pm.CloseGump(typeof(ConfirmCartGump));
        }

        public override void AddGumpLayout()
        {
            AddBackground(0, 0, 410, 200, 0x9C40);
            AddHtml(10, 10, 400, 20, "Quantidade", 0x7FFF, false, false); // Quantity

            for (var i = 0; i < Entry.Name.Length; i++)
            {
                if (Entry.Name[i].String == null)
                {
                    AddHtmlLocalized(10, 60 + (i * 14), 400, 20, 1114513, String.Format("#{0}", Entry.Name[i].Number), 0x6B45, false, false);
                }
                else
                {
                    AddHtml(10, 60 + (i * 14), 400, 20, ColorAndCenter(C16232(0x6B45), Entry.Name[i].String), false, false);
                }
            }

            AddHtml(30, 100, 200, 20, "Quantidade p/ compra", 0x7FFF, false, false); // Quantity to Buy:

            AddBackground(233, 100, 50, 20, 0x2486);
            AddTextEntry(238, 100, 50, 20, 0, 0, Current > 0 ? Current.ToString() : "", 2);

            AddECHandleInput();

            AddButton(45, 150, 0x9C53, 0x9C5D, 195, GumpButtonType.Reply, 0);
            AddHtml(45, 153, 126, 25, "<CENTER>Okay</CENTER>", 0x7FFF, false, false); // Okay

            AddECHandleInput();
            AddECHandleInput();

            AddButton(240, 150, 0x9C53, 0x9C5D, 0, GumpButtonType.Reply, 0);
            AddHtml(240, 153, 126, 25, "<CENTER>Cancelar</CENTER>", 0x7FFF, false, false); // Cancel

            AddECHandleInput();
        }

        public override void OnServerClose(NetState owner)
        {
            if (owner.Mobile is PlayerMobile)
            {
                UltimaStoreGump.ReleaseHidden(User);
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID == 195)
            {
                var amtText = info.GetTextEntry(0);

                if (amtText != null && !String.IsNullOrWhiteSpace(amtText.Text))
                {
                    var amount = Utility.ToInt32(amtText.Text);

                    if (amount > 0)
                    {
                        if (amount <= 50)
                        {
                            UltimaStore.GetProfile(User).SetCartAmount(Entry, amount);
                            User.SendMessage("Voce adicionou ao seu carrinho. Para visualizar seu carrinho, clique no botao verde na parte superior direita da loja");
                        }
                        else
                        {
                            User.SendMessage("Quantidade incorreta, favor entrar menos de 100 items"); // That text is unacceptable.
                            //User.SendLocalizedMessage(1156836); // You can't exceed 125 items per purchase. 
                        }

                        Gump.Refresh();
                    }
                }
                else
                {
                    User.SendMessage("Texto inaceitavel"); // That text is unacceptable.
                }
            }

            UltimaStoreGump.ReleaseHidden(User);
        }
    }

    public class ConfirmPurchaseGump : BaseGump
    {
        public ConfirmPurchaseGump(PlayerMobile pm)
            : base(pm, 150, 150)
        {
            pm.CloseGump(typeof(ConfirmPurchaseGump));
        }

        public override void AddGumpLayout()
        {
            AddPage(0);

            AddBackground(0, 0, 410, 200, 0x9C40);
            AddHtml(10, 10, 400, 20, "Confirmar Compra", 0x7FFF, false, false); // Purchase Confirmation

            AddHtml(30, 60, 350, 60, "Deseja confirmar sua compra?", 0x7FFF, false, false); // Are you sure you want to complete this purchase?

            AddECHandleInput();

            AddButton(45, 150, 0x9C53, 0x9C5D, 195, GumpButtonType.Reply, 0);
            AddHtmlLocalized(45, 153, 126, 25, 1114513, "#1156596", 0x7FFF, false, false); // Okay

            AddECHandleInput();
            AddECHandleInput();

            AddButton(240, 150, 0x9C53, 0x9C5D, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(240, 153, 126, 25, 1114513, "#1006045", 0x7FFF, false, false); // Cancel

            AddECHandleInput();
        }

        public override void OnServerClose(NetState owner)
        {
            if (owner.Mobile is PlayerMobile)
            {
                UltimaStoreGump.ReleaseHidden(User);
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID == 195)
            {
                UltimaStore.TryPurchase(User);
            }

            UltimaStoreGump.ReleaseHidden(User);
        }
    }

    public class NoFundsGump : BaseGump
    {
        public NoFundsGump(PlayerMobile pm)
            : base(pm, 150, 150)
        {
            pm.CloseGump(typeof(NoFundsGump));
        }

        public override void AddGumpLayout()
        {
            AddPage(0);

            AddBackground(0, 0, 410, 200, 0x9C40);
            AddHtml(10, 10, 400, 20, "Moedas insuficientes", 0x7FFF, false, false); // Insufficient Funds

            AddHtml(30, 60, 350, 60, Color("#da0000", String.Format("Voce nao tem moedas suficientes. Visite o site do shard para obter {0}.", Configuration.CurrencyName)), false, false);

            AddECHandleInput();

            AddButton(45, 150, 0x9C53, 0x9C5D, 195, GumpButtonType.Reply, 0);
            AddHtml(45, 153, 126, 25, ColorAndCenter("#FFFFFF", "Informacoes"), false, false); // Information

            AddECHandleInput();
            AddECHandleInput();

            AddButton(240, 150, 0x9C53, 0x9C5D, 0, GumpButtonType.Reply, 0);
            AddHtml(240, 153, 126, 25, "<CENTER>Cancelar</CENTER>", 0x7FFF, false, false); // Cancel

            AddECHandleInput();
        }

        public override void OnServerClose(NetState owner)
        {
            if (owner.Mobile is PlayerMobile)
            {
                UltimaStoreGump.ReleaseHidden(User);
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID == 195)
            {
                if (!String.IsNullOrEmpty(Configuration.Website))
                {
                    User.LaunchBrowser(Configuration.Website);
                }
                else
                {
                    User.LaunchBrowser("https://uo.com/ultima-store/");
                }
            }

            UltimaStoreGump.ReleaseHidden(User);
        }
    }

    public class PromoCodeGump : BaseGump
    {
        public BaseGump Gump { get; private set; }

        public PromoCodeGump(PlayerMobile pm, BaseGump gump)
            : base(pm, 10, 10)
        {
            Gump = gump;

            pm.CloseGump(typeof(PromoCodeGump));
        }

        public override void AddGumpLayout()
        {
            AddPage(0);

            AddBackground(0, 0, 400, 340, 0x9C40);

            AddHtml(0, 10, 400, 20, "<center>Digitar Codigo</center>", 0x7FFF, false, false); // Enter Promotional Code
            AddHtml(20, 60, 355, 160, "Digite EXATAMENTE como lhe foi enviado.", C32216(0xFFFF00), false, false); // Enter your promotional code EXACTLY as it was given to you (including dashes). Enter no other text in the box aside from your promotional code.

            AddECHandleInput();

            AddBackground(80, 220, 240, 22, 0x2486);
            AddTextEntry(81, 220, 239, 20, 0, 0, "");

            AddButton(40, 260, 0x9C53, 0x9C5D, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(40, 262, 125, 25, 1114513, "#1156596", 0x7FFF, false, false);

            AddECHandleInput();
            AddECHandleInput();

            AddButton(234, 260, 0x9C53, 0x9C5D, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(234, 262, 126, 25, 1114513, "#1006045", 0x7FFF, false, false);

            AddECHandleInput();
        }

        public override void OnServerClose(NetState owner)
        {
            if (owner.Mobile is PlayerMobile)
            {
                UltimaStoreGump.ReleaseHidden(User);
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            Shard.Debug("Resp " + info.ButtonID);
            if (info.ButtonID == 1)
            {
                var text = info.GetTextEntry(0);
                Shard.Debug("Txt: " + text.Text);
                if (text != null && !String.IsNullOrEmpty(text.Text))
                {
                    Codigos.Consome(User, text.Text);
                }
            }
        }
    }

    public class InfoGump : BaseGump
    {
        public BaseGump Gump { get; private set; }
        private string item;
        private string info;

        public InfoGump(PlayerMobile pm, BaseGump gump, string item, string info)
            : base(pm, 10, 10)
        {
            Gump = gump;

            pm.CloseGump(typeof(InfoGump));
            this.item = item;
            this.info = info;
        }

        public override void AddGumpLayout()
        {
            AddPage(0);

            AddBackground(0, 0, 400, 340, 0x9C40);

            AddHtml(0, 10, 400, 20, "<center>" + item + "</center>", 0x7FFF, false, false); // Enter Promotional Code

            AddHtml(20, 60, 355, 160, info, C32216(0xFFFF00), false, false); // Enter your promotional code EXACTLY as it was given to you (including dashes). Enter no other text in the box aside from your promotional code.

            AddECHandleInput();

            AddECHandleInput();
            AddECHandleInput();


            AddECHandleInput();
        }

        public override void OnServerClose(NetState owner)
        {
            if (owner.Mobile is PlayerMobile)
            {
                UltimaStoreGump.ReleaseHidden(User);
            }
        }

        public override void OnResponse(RelayInfo info)
        {

        }
    }
}
