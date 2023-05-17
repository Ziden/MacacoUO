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

        //This command will not move spellbooks, runebooks, blessed items.
        [Usage("organizar")]
        [Description("Organiza as porra tudo na mochila")]
        private static void OrganizeMe_OnCommand(CommandEventArgs arg)
        {
            OrganizePouch weaponPouch = null;
            OrganizePouch jewelPouch = null;
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

                    // Skip all the items in the pouches since they should already be organized
                    continue;
                }

                // Add all the subcontainer items, but dont go all the way to comeplete depth
                subcontaineritems.AddRange(item.Items);
            }

            backpackitems.AddRange(subcontaineritems);

            if (weaponPouch == null)
            {
                weaponPouch = new OrganizePouch { Name = "Equips", Hue = 92 };
            }
            if (jewelPouch == null)
            {
                jewelPouch = new OrganizePouch { Name = "Joias", Hue = 62 };
            }
            if (currencyPouch == null)
            {
                currencyPouch = new OrganizePouch { Name = "Moedas", Hue = 42 };
            }
            if (resourcePouch == null)
            {
                resourcePouch = new OrganizePouch { Name = "Recursos", Hue = 32 };
            }
            if (toolPouch == null)
            {
                toolPouch = new OrganizePouch { Name = "Ferramentas", Hue = 22 };
            }
            if (miscPouch == null)
            {
                miscPouch = new OrganizePouch { Name = "Misc" };
            }
            var pouches = new List<OrganizePouch>
            {
                weaponPouch,
                jewelPouch,
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
                // Lets not add the pouches to themselves
                if (item is OrganizePouch)
                {
                    continue;
                }

                if (item is BaseWeapon || item is BaseArmor || item is BaseClothing || item is BaseJewel)
                {
                    weaponPouch.TryDropItem(from, item, false);
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

                // AddToBackpack doesnt do anything if the item is already in the backpack
                // calls DropItem internally

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
