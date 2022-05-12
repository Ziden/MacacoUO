using System;
using System.Collections.Generic;
using System.Linq;
using Server.Commands;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Prompts;
using Server.Regions;

namespace Server.Items
{
    public class DungeonsGump : Gump
    {
        private List<DungeonRegion> m_List;


        public static void Initialize()
        {
            CommandSystem.Register("verdungeons", AccessLevel.Player, new CommandEventHandler(_OnCommand));
        }

        [Usage("vernpcs")]
        [Description("Makes a call to your custom gump.")]
        public static void _OnCommand(CommandEventArgs e)
        {
            if (e.Mobile.HasGump(typeof(DungeonsGump)))
                e.Mobile.CloseGump(typeof(DungeonsGump));
            e.Mobile.SendGump(new DungeonsGump(e.Mobile));
        }

        private int m_Page;

        private const int LabelColor = 0xFFFFFF;

        public int GetIndexForPage(int page)
        {
            int index = 0;

            while (page-- > 0)
                index += GetCountForIndex(index);

            return index;
        }

        public int GetCountForIndex(int index)
        {
            int slots = 0;
            int count = 0;

            List<DungeonRegion> list = m_List;

            for (int i = index; i >= 0 && i < list.Count; ++i)
            {
                var recipe = list[i];

                //if (CheckFilter(recipe))
                //
                int add;

                add = 1;

                if ((slots + add) > 10)
                    break;

                slots += add;
                //}

                ++count;
            }

            return count;
        }

        public DungeonsGump(Mobile from)
            : this(from, 0, null)
        {
        }

        private string filtro;

        public DungeonsGump(Mobile from, int page, List<DungeonRegion> list, string filtro = null)
            : base(12, 24)
        {
            from.CloseGump(typeof(CurrentRecipesGump));
            from.CloseGump(typeof(RecipeScrollFilterGump));

            var player = from as PlayerMobile;

            if (filtro != null && filtro != "")
                this.filtro = filtro;
            else if (filtro == null)
                this.filtro = "";

            if (player == null)
                return;

            m_Page = page;

            if (list == null)
            {
                list = new List<DungeonRegion>();


                foreach (var region in Region.Regions)
                {
                    var dg = region as DungeonRegion;
                    if (dg == null)
                        continue;

                    if (dg.EntranceMap == player.Map)
                    {
                        list.Add(dg);
                    }
                }
            }

            m_List = list;

            int index = GetIndexForPage(page);
            int count = GetCountForIndex(index);

            int tableIndex = 0;

            int width = 600;

            width = 516;

            X = (624 - width) / 2;

            AddPage(0);

            AddBackground(10, 10, width, 439, 9350);
            AddImageTiled(18, 20, width - 17, 420, 2624);

            AddImageTiled(58, 64, 36, 352, 200);
            AddImageTiled(96, 64, 233, 352, 1416);
            //AddImageTiled(231, 64, 80, 352, 200);
            AddImageTiled(313, 64, 100, 352, 1416);
            AddImageTiled(415, 64, 76, 352, 200);

            //list = list.OrderBy(x => x.ID).ToList();

            for (int i = index; i < (index + count) && i >= 0 && i < list.Count; ++i)
            {
                var recipe = list[i];

                //if (!CheckFilter(recipe))
                //    continue;

                AddImageTiled(24, 94 + (tableIndex * 32), 489, 2, 2624);

                ++tableIndex;
            }

            AddAlphaRegion(18, 20, width - 17, 420);
            AddImage(0, 0, 10460);
            //AddImage(width - 15, 5, 10460);
            AddImage(0, 429, 10460);
            AddImage(width - 15, 429, 10460);

            AddHtml(266, 32, 200, 32, "Dungeons", LabelColor, false, false); // Recipe Book

            // FILTRO
            AddHtml(80 - 3, 32 - 3, 100, 20, "", true, false);
            AddTextEntry(80, 32, 100, 32, 0, 0, filtro);

            //AddBackground(80, 32, 200, 32, 9359);
            AddButton(35, 28, 30534, 30534, 1, GumpButtonType.Reply, 0);

            AddHtml(147, 64, 200, 32, "Nome", LabelColor, false, false); // Item
            AddHtml(246, 64, 200, 32, "Local", LabelColor, false, false); // Expansion
            //AddHtml(336, 64, 200, 32, "", LabelColor, false, false); // Crafting
            AddHtml(429, 64, 100, 32, "Localizar", LabelColor, false, false); // Amount

            AddButton(375, 416, 30535, 30535, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(410, 416, 120, 20, 1011441, LabelColor, false, false); // EXIT

            tableIndex = 0;

            if (page > 0)
            {
                AddButton(75, 416, 30533, 30534, 2, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 416, 150, 20, 1011067, LabelColor, false, false); // Previous page
            }

            if (GetIndexForPage(page + 1) < list.Count)
            {
                AddButton(225, 416, 30533, 30534, 3, GumpButtonType.Reply, 0);
                AddHtmlLocalized(260, 416, 150, 20, 1011066, LabelColor, false, false); // Next page
            }

            for (int i = index; i < (index + count) && i >= 0 && i < list.Count; ++i)
            {
                var element = list[i];

                //if (!CheckFilter(element))
                //    continue;

                int y = 96 + (tableIndex++ * 32);

                //if (recipe.Amount > 0 && (canDrop || canLocked))
                //    AddButton(35, y + 2, 5602, 5606, 4 + (i * 2), GumpButtonType.Reply, 0);


                AddLabel(61, y, 0x480, String.Format("{0}", i));

                AddHtml(103, y, 230, 32, element.Name, 0xFFFFFF, false, false); // ~1_val~

                //AddLabel(235, y, 0x480, ""+recipe.RecipeID);
                //AddHtml(316, y, 100, 20, element.Region != null && element.Region.Name != null ? element.Region.Name : "Desconhecido", 0xFFFFFF, false, false); // ~1_val~

                AddButton(421, y - 8, 0x1196, 0x1196, i + 100, GumpButtonType.Reply, 0);
                //AddLabel(421, y, 0x480, "BUTAUM");

            }


            /*
            if (canDrop || (canBuy && recipe.Price > 0))
            {
                AddButton(579, y + 2, 2117, 2118, 5 + (i * 2), GumpButtonType.Reply, 0);
                AddLabel(495, y, 1152, recipe.Price.ToString("N0"));
            }
            */
        }


        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            int index = info.ButtonID;

            var novoFiltro = info.TextEntries[0].Text;

            if (index >= 100)
            {
                var idx = index - 100;
                var npc = m_List[idx];
                from.QuestArrow = new QuestArrow(from, npc.EntranceLocation);
                from.QuestArrow.Update();
                from.CloseAllGumps();
                from.SendMessage("Voce esta indo ao seu destino " + npc.EntranceLocation.ToString() + " no mapa " + npc.Map.Name);
            }

            switch (index)
            {
                case 1:
                    {
                        from.SendGump(new DungeonsGump(from, m_Page, m_List, novoFiltro));
                        break;
                    }
                case 2:
                    {
                        if (m_Page > 0)
                            from.SendGump(new DungeonsGump(from, m_Page - 1, m_List, novoFiltro));

                        return;
                    }
                case 3:
                    {
                        if (GetIndexForPage(m_Page + 1) < m_List.Count)
                            from.SendGump(new DungeonsGump(from, m_Page + 1, m_List, novoFiltro));

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}
