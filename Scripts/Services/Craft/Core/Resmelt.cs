using System;
using Server.Engines.Harvest;
using Server.Items;
using Server.Targeting;

namespace Server.Engines.Craft
{
    public enum SmeltResult
    {
        Success,
        Invalid,
        NoSkill
    }

    public class Resmelt
    {
        public Resmelt()
        {
        }

        public static void Do(Mobile from, CraftSystem craftSystem, ITool tool)
        {
            int num = craftSystem.CanCraft(from, tool, null);

            if (num > 0 && num != 1044267)
            {
                from.SendGump(new CraftGump(from, craftSystem, tool, num));
            }
            else
            {
                from.Target = new InternalTarget(craftSystem, tool);
                if (craftSystem.MainSkill == SkillName.Carpentry || craftSystem.MainSkill == SkillName.Bowcraft)
                    from.SendMessage("Selecione um item para desmontar"); // Target an item to recycle.
                else
                    from.SendMessage("Selecione um item para derreter"); // Target an item to recycle.
            }
        }

        private class InternalTarget : Target
        {
            private readonly CraftSystem m_CraftSystem;
            private readonly ITool m_Tool;
            public InternalTarget(CraftSystem craftSystem, ITool tool)
                : base(2, false, TargetFlags.None)
            {
                m_CraftSystem = craftSystem;
                m_Tool = tool;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                int num = m_CraftSystem.CanCraft(from, m_Tool, null);
                var str = "";
                if (num > 0)
                {
                    if (num == 1044267 && m_CraftSystem.MainSkill != SkillName.Carpentry)
                    {
                        bool anvil, forge;

                        DefBlacksmithy.CheckAnvilAndForge(from, 2, out anvil, out forge);

                        if (!anvil)
                        {
                            num = 1044266; // You must be near an anvil
                            str = "Voce precista estar perto de uma bigorna";
                        }
                        else if (!forge)
                        {
                            num = 1044265; // You must be near a forge.
                            str = "Voce precista estar perto de uma forja";
                        }

                    }

                    from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, str));
                }
                else
                {
                    SmeltResult result = SmeltResult.Invalid;
                    bool isStoreBought = false;
                    string message;

                    if (targeted is BaseArmor)
                    {
                        result = Resmelt(from, (BaseArmor)targeted, ((BaseArmor)targeted).Resource);
                        isStoreBought = !((BaseArmor)targeted).PlayerConstructed;
                    }
                    else if (targeted is BaseWeapon)
                    {
                        result = Resmelt(from, (BaseWeapon)targeted, ((BaseWeapon)targeted).Resource);
                        isStoreBought = !((BaseWeapon)targeted).PlayerConstructed;
                    }
                    else if (targeted is DragonBardingDeed)
                    {
                        result = Resmelt(from, (DragonBardingDeed)targeted, ((DragonBardingDeed)targeted).Resource);
                        isStoreBought = false;
                    }
                    else if (targeted is IResource)
                    {
                        result = Resmelt(from, (Item)targeted, ((IResource)targeted).Resource);
                        isStoreBought = false;
                    }

                    switch (result)
                    {
                        default:
                        case SmeltResult.Invalid:
                            message = "Voce nao pode fazer isto";
                            break; // You can't melt that down into ingots.
                        case SmeltResult.NoSkill:
                            message = "Voce nao sabe trabalhar com este material";
                            break; // You have no idea how to work this metal.
                        case SmeltResult.Success:
                            message = "Voce conseguiu recuperar alguns recursos";
                            break; // You melt the item down into ingots.
                    }

                    from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, message));
                }
            }

            private SmeltResult Resmelt(Mobile from, Item item, CraftResource resource)
            {
                try
                {
                    if (Ethics.Ethic.IsImbued(item))
                        return SmeltResult.Invalid;

                    if (CraftResources.GetType(resource) != CraftResourceType.Metal && CraftResources.GetType(resource) != CraftResourceType.Wood)
                        return SmeltResult.Invalid;

                    CraftResourceInfo info = CraftResources.GetInfo(resource);

                    if (info == null || info.ResourceTypes.Length == 0)
                        return SmeltResult.Invalid;

                    CraftItem craftItem = m_CraftSystem.CraftItems.SearchFor(item.GetType());
                    if (craftItem == null || craftItem.Resources.Count == 0)
                        return SmeltResult.Invalid;

                    CraftRes craftResource = craftItem.Resources.GetAt(0);

                    if (craftResource.Amount < 2)
                        return SmeltResult.Invalid; // Not enough metal to resmelt

                    double difficulty = 0.0;

                    switch (resource)
                    {
                        case CraftResource.Pinho:
                        case CraftResource.Cobre:
                            difficulty = 65;
                            break;
                        case CraftResource.Mogno:
                        case CraftResource.Eucalipto:
                        case CraftResource.Bronze:
                            difficulty = 70;
                            break;
                        case CraftResource.Carmesim:
                        case CraftResource.Dourado:
                            difficulty = 75;
                            break;
                        case CraftResource.Gelo:
                        case CraftResource.Niobio:
                            difficulty = 80;
                            break;
                        case CraftResource.Lazurita:
                            difficulty = 85;
                            break;
                        case CraftResource.Quartzo:
                            difficulty = 90;
                            break;
                        case CraftResource.Berilo:
                            difficulty = 95;
                            break;
                        case CraftResource.Vibranium:
                            difficulty = 99;
                            break;
                    }

                    double skill = Math.Max(from.Skills[SkillName.Mining].Value, from.Skills[SkillName.Blacksmith].Value);
                    if (CraftResources.GetType(resource) == CraftResourceType.Wood)
                        skill = Math.Max(from.Skills[SkillName.Lumberjacking].Value, from.Skills[SkillName.Carpentry].Value);
                    if(typeof(BaseRanged).IsAssignableFrom(craftItem.ItemType) || craftItem.ItemType.IsAssignableFrom(typeof(BaseRanged)))
                        skill = Math.Max(from.Skills[SkillName.Lumberjacking].Value, from.Skills[SkillName.Bowcraft].Value);

                    if (difficulty > skill)
                        return SmeltResult.NoSkill;

                    Type resourceType = info.ResourceTypes[0];
                    if (Shard.DebugEnabled)
                    {
                        Shard.Debug("Resmelt resource " + resourceType);
                        Shard.Debug("Info " + info.Resource);
                        Shard.Debug("Info 2 " + info.Name);
                        Shard.Debug("RES: " + resource.ToString());
                    }

                    if (item is BaseRanged && resourceType == typeof(IronIngot) && resource == CraftResource.Ferro)
                    {
                        resourceType = typeof(Board);
                        resource = CraftResource.Cedro;
                    }
                    resourceType = Lumberjacking.Converte(resourceType);

                    Item ingot = (Item)Activator.CreateInstance(resourceType);

                    if (item is DragonBardingDeed || (item is BaseArmor && ((BaseArmor)item).PlayerConstructed) || (item is BaseWeapon && ((BaseWeapon)item).PlayerConstructed) || (item is BaseClothing && ((BaseClothing)item).PlayerConstructed))
                        ingot.Amount = (int)((double)craftResource.Amount * .5);
                    else
                    {
                        var res = item as IResource;
                        if(res != null && res.Resource != CraftResource.Ferro && res.Resource != CraftResource.Cedro && res.Resource != CraftResource.RegularLeather)
                        {
                            ingot.Amount = (int)((double)craftResource.Amount * .5);
                        } else
                        {
                            ingot.Amount = 1;
                        }
                    }
                       

                    item.Delete();
                    from.AddToBackpack(ingot);

                    if (CraftResources.GetType(resource) == CraftResourceType.Wood)
                    {
                        from.PlayAttackAnimation();
                        from.PlaySound(0x23D);
                    }
                    else
                    {
                        from.PlayAttackAnimation();
                        from.PlaySound(0x2A);
                        from.PlaySound(0x240);
                    }

                    return SmeltResult.Success;
                }
                catch
                {
                }

                return SmeltResult.Invalid;
            }
        }


    }
}
