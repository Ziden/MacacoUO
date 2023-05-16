#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Server.Items;
using Server.Mobiles;

#endregion

namespace Server.Commands
{
    public class OrganizeMeCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("organizar", AccessLevel.Player, OrganizeMe_OnCommand);
        }

        [Usage("organizar")]
        [Description("Organiza as porra tudo na mochila")]
        private static void OrganizeMe_OnCommand(CommandEventArgs arg)
        {
            OrganizePouch weaponPouch = null;
            OrganizePouch currencyPouch = null;
            OrganizePouch resourcePouch = null;
            OrganizePouch toolPouch = null;
            OrganizePouch miscPouch = null;

            Mobile from = arg.Mobile;
            var bp = from.Backpack as Backpack;
            var potX = 0;
            var potY = 250;

            if (from == null || bp == null)
            {
                return;
            }

            if (bp.TotalWeight >= bp.MaxWeight && from.AccessLevel < AccessLevel.GameMaster)
            {
                if (from is PlayerMobile && from.NetState != null)
                {
                    from.SendMessage("Voce esta muito pesado.");
                }
                return;
            }

            if (bp.TotalItems >= (bp.MaxItems - 10) && from.AccessLevel < AccessLevel.GameMaster)
            {
                if (from is PlayerMobile && from.NetState != null)
                {
                    from.SendMessage("Voce nao tem espaco em sua mochila.");
                }
                return;
            }

            var backpackitems = new List<Item>(bp.Items);
            var subcontaineritems = new List<Item>();

            OrganizePouch GetExistingPouch(Mobile mobile, string pouchName)
            {
                return mobile.Backpack.FindItemsByType<OrganizePouch>().FirstOrDefault(pouch => pouch.Name == pouchName);
            }

            foreach (var item in new List<BaseContainer>(backpackitems.OfType<BaseContainer>()))
            {
                var lockable = item as LockableContainer;
                if (lockable != null)
                {
                    if (lockable.CheckLocked(from))
                    {
                        continue;
                    }
                }

                var trapped = item as TrapableContainer;
                if (trapped != null)
                {
                    if (trapped.TrapType != TrapType.None)
                    {
                        continue;
                    }
                }

                // Skip the pouches that are already created
                if (item is OrganizePouch)
                {
                    if (item.Name == "Equips")
                    {
                        if (weaponPouch != null)
                        {
                            foreach (var i in new List<Item>(item.Items))
                                weaponPouch.AddItem(i);
                            item.Delete();
                        }
                        else
                            weaponPouch = item as OrganizePouch;
                    }
                    if (item.Name == "Moedas")
                    {
                        if (currencyPouch != null)
                        {
                            foreach (var i in new List<Item>(item.Items))
                                currencyPouch.AddItem(i);
                            item.Delete();
                        }
                        else
                            currencyPouch = item as OrganizePouch;
                    }
                    if (item.Name == "Recursos")
                    {
                        if (resourcePouch != null)
                        {
                            foreach (var i in new List<Item>(item.Items))
                                resourcePouch.AddItem(i);
                            item.Delete();
                        }
                        else
                            resourcePouch = item as OrganizePouch;
                    }
                    if (item.Name == "Ferramentas")
                    {
                        if (toolPouch != null)
                        {
                            foreach (var i in new List<Item>(item.Items))
                                toolPouch.AddItem(i);
                            item.Delete();
                        }
                        else
                            toolPouch = item as OrganizePouch;
                    }
                    if (item.Name == "Misc")
                    {
                        if (miscPouch != null)
                        {
                            foreach (var i in new List<Item>(item.Items))
                                miscPouch.AddItem(i);
                            item.Delete();
                        }
                        else
                            miscPouch = item as OrganizePouch;
                    }
                    continue;
                }
                subcontaineritems.AddRange(item.Items);
            }

            backpackitems.AddRange(subcontaineritems);

            weaponPouch = GetExistingPouch(from, "Equips") ?? new OrganizePouch { Name = "Equips", Hue = 92 };
            currencyPouch = GetExistingPouch(from, "Moedas") ?? new OrganizePouch { Name = "Moedas", Hue = 42 };
            resourcePouch = GetExistingPouch(from, "Recursos") ?? new OrganizePouch { Name = "Recursos", Hue = 32 };
            toolPouch = GetExistingPouch(from, "Ferramentas") ?? new OrganizePouch { Name = "Ferramentas", Hue = 22 };
            miscPouch = GetExistingPouch(from, "Misc") ?? new OrganizePouch { Name = "Misc" };

            var pouches = new List<OrganizePouch>
            {
                weaponPouch,
                currencyPouch,
                resourcePouch,
                toolPouch,
                miscPouch
            };

        
            foreach (
                Item item in
                    backpackitems.Where(
                        item =>
                            item.LootType != LootType.Blessed &&
                            !(item is Runebook) &&
                            !(item is RecallRune) &&
                            !(item is Key) &&
                            !(item is Spellbook) &&
                            item.Movable &&
                            item.LootType != LootType.Blessed))
            {
                if (item is OrganizePouch)
                {
                    continue;
                }

                if (item is BaseWeapon || item is BaseArmor || item is BaseClothing )
                {
                    weaponPouch.TryDropItem(from, item, false);
                }
                else if (item is BaseJewel)
                {
                    from.Backpack.DropItem(item);
                    item.X = potX;
                    item.Y = potY;
                    potX += 40;
                }
                else if (item is BasePotion)
                {
                    from.Backpack.DropItem(item);
                    item.X = potX;
                    item.Y = potY;
                    potX += 20;
                }
                else if (item is Bandage)
                {
                    from.Backpack.DropItem(item);
                    item.X = 0;
                    item.Y = 90;
                }
                else if (item is Gold)
                {
                    currencyPouch.TryDropItem(from, item, false);
                }
                else if (item is BaseIngot || item is BaseOre || item is Feather || item is BaseBoard || item is Log ||
                         item is BaseLeather ||
                         item is Sand || item is BaseGranite)
                {
                    resourcePouch.TryDropItem(from, item, false);
                }
                else if (item is BaseTool)
                {
                    toolPouch.TryDropItem(from, item, false);
                }
                else if (item is BaseReagent)
                {
                    from.Backpack.DropItemStack(item);
                    item.X = 300;
                    item.Y = 300;
                }
                else
                {
                    miscPouch.TryDropItem(from, item, false);
                }
            }

            var x = 45;

            foreach (var pouch in pouches)
            {
                if (pouch.TotalItems <= 0)
                {
                    continue;
                }

                if (!from.Backpack.Items.Contains(pouch))
                {
                    from.AddToBackpack(pouch);
                }

                pouch.X = x;
                pouch.Y = 65;

                x += 10;
            }
        }
    }
}