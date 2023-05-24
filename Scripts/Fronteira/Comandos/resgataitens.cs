using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;
using Server.Network;
using Server.Multis;

namespace Server.Commands
{
    public class ResgateCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("resgatar", AccessLevel.Administrator, new CommandEventHandler(Resgatar_Command));
        }

        [Usage("resgatar <item>")]
        [Description("Resgata itens de jogadores.")]
        private static void Resgatar_Command(CommandEventArgs e)
        {
            Mobile mobile = e.Mobile;

            if (mobile == null || mobile.Deleted)
                return;

            string itemTypeName = e.GetString(0);

            if (string.IsNullOrWhiteSpace(itemTypeName))
            {
                mobile.SendMessage("Sintaxe inválida. Use: [resgatar <item>.");
                return;
            }

            Type itemType = ScriptCompiler.FindTypeByName(itemTypeName);

            if (itemType == null || !typeof(Item).IsAssignableFrom(itemType))
            {
                mobile.SendMessage("O tipo de item especificado é inválido.");
                return;
            }

            List<Item> itemsToRescue = new List<Item>();

            foreach (Item item in World.Items.Values)
            {
                if (item.GetType() == itemType && !item.Deleted)
                {
                    if (item.RootParentEntity == null || item.RootParentEntity is Mobile || item.RootParentEntity is BaseContainer)
                    {
                        itemsToRescue.Add(item);
                    }
                }
            }

            if (itemsToRescue.Count == 0)
            {
                mobile.SendMessage("Não há itens do tipo especificado no servidor.");
                return;
            }

            // Criação e exibição do Gump para o administrador
            ResgateGump gump = new ResgateGump(itemsToRescue, mobile);
            mobile.SendGump(gump);
        }
    }

    public class ResgateGump : Gump
    {
        private List<Item> itemsToRescue;
        private Mobile admin;

        public ResgateGump(List<Item> items, Mobile admin)
            : base(100, 100)
        {
            this.itemsToRescue = items;
            this.admin = admin;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 400, 400, 9200);
            AddLabel(140, 20, 0, "Resgate de Itens");

            //AddHtml(20, 50, 360, 300, GetItemListHtml(), true, true);

            AddButton(140, 360, 4017, 4018, 0, GumpButtonType.Reply, 0);
            AddLabel(175, 360, 0, "Resgatar Todos");

            AddButton(300, 360, 4017, 4018, 1, GumpButtonType.Reply, 0);
            AddLabel(335, 360, 0, "Cancelar");

            int yPosition = 70;

            for (int i = 0; i < itemsToRescue.Count; i++)
            {
                AddHtml(20, yPosition, 200, 20, $"{itemsToRescue[i].GetType().Name} (Owner: {GetItemOwner(itemsToRescue[i])})", false, false);
                AddButton(230, yPosition, 4017, 4018, i + 2, GumpButtonType.Reply, 0);
                yPosition += 20;
            }

        }

        private string GetItemListHtml()
        {
            string html = "<basefont color=#FFFFFF>";

            for (int i = 0; i < itemsToRescue.Count; i++)
            {
                Item item = itemsToRescue[i];
                string itemName = item.GetType().Name;
                string ownerName = item.RootParentEntity is Mobile owner ? owner.Name : "N/A";

                html += $"{itemName} (Owner: {ownerName})<br>";
            }

            return html;
        }

        private string GetItemOwner(Item item)
        {
            Mobile mobileOwner = item.RootParentEntity as Mobile;
            if (mobileOwner != null)
            {
                return mobileOwner.Name;
            }

            BaseHouse houseOwner = BaseHouse.FindHouseAt(item);
            if (houseOwner != null && houseOwner.Owner != null)
            {
                return houseOwner.Owner.Name;
            }

            BaseContainer containerOwner = item.RootParentEntity as BaseContainer;
            if (containerOwner != null)
            {
                Mobile containerMobileOwner = containerOwner.RootParentEntity as Mobile;
                BaseHouse containerHouseOwner = BaseHouse.FindHouseAt(containerOwner);
                
                if (containerMobileOwner != null)
                {
                    return containerMobileOwner.Name;
                }
                
                if (containerHouseOwner != null && containerHouseOwner.Owner != null)
                {
                    return containerHouseOwner.Owner.Name;
                }
            }

            // Se não tem um dono identificável, então é um item sem dono
            return "Itens Sem Dono";
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0) // Resgatar Todos
            {
                MochilaResgateAll();
            }
            else if (info.ButtonID > 1 && info.ButtonID <= itemsToRescue.Count + 1) // Resgatar Item Específico
            {
                int itemIndex = info.ButtonID - 2;
                ResgatarItem(itemIndex);
            }
        }

        private void MochilaResgateAll()
        {
            Bag generalBag = null;
            Dictionary<string, Bag> ownerBags = new Dictionary<string, Bag>();

            foreach (Bag b in admin.Backpack.FindItemsByType<Bag>(true))
            {
                if (b.Name == "Itens Resgatados")
                {
                    generalBag = b;
                    break;
                }
            }

            if (generalBag == null)
            {
                generalBag = new Bag();
                generalBag.Name = "Itens Resgatados";
                admin.Backpack.DropItem(generalBag);
            }

            foreach (Item item in itemsToRescue)
            {
                string ownerName = GetItemOwner(item);
                if(!ownerBags.TryGetValue(ownerName, out Bag ownerBag))
                {
                    ownerBag = new Bag();
                    ownerBag.Name = $"Itens de {ownerName}";
                    generalBag.DropItem(ownerBag);
                    ownerBags[ownerName] = ownerBag;
                }

                ownerBag.DropItem(item);
            }

            admin.SendMessage("Todos os itens foram resgatados com sucesso e colocados na mochila 'Itens Resgatados'.");
        }

        private void ResgatarItem(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= itemsToRescue.Count)
                return;

            Item item = itemsToRescue[itemIndex];
            MoveItemToNewBag(item, admin);
        }

        private void MoveItemToNewBag(Item item, Mobile owner)
        {
            string ownerName = GetItemOwner(item);
            Bag bag = new Bag();
            bag.Name = $"Itens de {ownerName}";
            bag.DropItem(item);
            owner.Backpack.DropItem(bag);

            admin.SendMessage($"O item '{item.GetType().Name}' foi resgatado com sucesso e colocado em uma nova mochila na mochila de '{owner.Name}'.");
        }
    }
}
