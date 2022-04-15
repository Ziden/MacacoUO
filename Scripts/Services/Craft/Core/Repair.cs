using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Factions;

namespace Server.Engines.Craft
{
    public interface IRepairable
    {
        CraftSystem RepairSystem { get; }
    }

    public class Repair
    {
        public Repair()
        {
        }

        public static void Do(Mobile from, CraftSystem craftSystem, ITool tool)
        {
            from.Target = new InternalTarget(craftSystem, tool);
            from.SendLocalizedMessage(1044276); // Target an item to repair.
        }

        public static void Do(Mobile from, CraftSystem craftSystem, RepairDeed deed)
        {
            from.Target = new InternalTarget(craftSystem, deed);
            from.SendLocalizedMessage(1044276); // Target an item to repair.
        }

        public static void Do(Mobile from, CraftSystem craftSystem, RepairBenchAddon addon)
        {
            from.Target = new InternalTarget(craftSystem, addon);
            from.SendLocalizedMessage(500436); // Select item to repair.
        }

        private class InternalTarget : Target
        {
            private readonly CraftSystem m_CraftSystem;
            private readonly ITool m_Tool;
            private readonly RepairDeed m_Deed;
            private readonly RepairBenchAddon m_Addon;

            public InternalTarget(CraftSystem craftSystem, ITool tool)
                : base(10, false, TargetFlags.None)
            {
                m_CraftSystem = craftSystem;
                m_Tool = tool;
            }

            public InternalTarget(CraftSystem craftSystem, RepairDeed deed)
                : base(2, false, TargetFlags.None)
            {
                m_CraftSystem = craftSystem;
                m_Deed = deed;
            }

            public InternalTarget(CraftSystem craftSystem, RepairBenchAddon addon)
                : base(2, false, TargetFlags.None)
            {
                m_CraftSystem = craftSystem;
                m_Addon = addon;
            }

            private static void EndMobileRepair(object state)
            {
                ((Mobile)state).EndAction(typeof(IRepairableMobile));
            }

            private int GetWeakenChance(Mobile mob, SkillName skill, int curHits, int maxHits)
            {
                double value = 0;

                if (m_Deed != null)
                {
                    value = m_Deed.SkillLevel;
                }
                else if (m_Addon != null)
                {
                    value = m_Addon.Tools.Find(x => x.System == m_CraftSystem).SkillValue;
                }
                else
                {
                    value = mob.Skills[skill].Value;
                }

                // 40% - (1% per hp lost) - (1% per 10 craft skill)
                return (40 + (maxHits - curHits)) - (int)(value / 10);
            }

            private bool CheckWeaken(Mobile mob, SkillName skill, int curHits, int maxHits)
            {
                return (GetWeakenChance(mob, skill, curHits, maxHits) > Utility.Random(100));
            }

            private int GetRepairDifficulty(int curHits, int maxHits)
            {
                return (((maxHits - curHits) * 1250) / Math.Max(maxHits, 1)) - 250;
            }

            private bool CheckRepairDifficulty(Mobile mob, SkillName skill, int curHits, int maxHits)
            {
                double difficulty = GetRepairDifficulty(curHits, maxHits) * 0.1;

                if (m_Deed != null)
                {
                    double value = m_Deed.SkillLevel;
                    double minSkill = difficulty - 25.0;
                    double maxSkill = difficulty + 25;

                    if (value < minSkill)
                        return false; // Too difficult
                    else if (value >= maxSkill)
                        return true; // No challenge

                    double chance = (value - minSkill) / (maxSkill - minSkill);

                    return (chance >= Utility.RandomDouble());
                }
                else if (m_Addon != null)
                {
                    double value = m_Addon.Tools.Find(x => x.System == m_CraftSystem).SkillValue;
                    double minSkill = difficulty - 25.0;
                    double maxSkill = difficulty + 25;

                    if (value < minSkill)
                        return false; // Too difficult
                    else if (value >= maxSkill)
                        return true; // No challenge

                    double chance = (value - minSkill) / (maxSkill - minSkill);

                    return (chance >= Utility.RandomDouble());
                }
                else
                {
                    SkillLock sl = mob.Skills[SkillName.Tinkering].Lock;
                    mob.Skills[SkillName.Tinkering].SetLockNoRelay(SkillLock.Locked);

                    bool check = mob.CheckSkillMult(skill, difficulty - 25.0, difficulty + 25.0);

                    mob.Skills[SkillName.Tinkering].SetLockNoRelay(sl);

                    return check;
                }
            }

            private bool CheckDeed(Mobile from)
            {
                if (m_Deed != null)
                {
                    return m_Deed.Check(from);
                }

                return true;
            }

            private bool CheckSpecial(Item item)
            {
                return item is IRepairable && ((IRepairable)item).RepairSystem == m_CraftSystem;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                bool usingDeed = (m_Deed != null) || (m_Addon != null);
                bool toDelete = false;
                //int number;
                string number = "";

                double value = 0;

                if (m_Deed != null)
                {
                    value = m_Deed.SkillLevel;
                }
                else if (m_Addon != null)
                {
                    var tool = m_Addon.Tools.Find(x => x.System == m_CraftSystem);

                    if (tool.Charges == 0)
                    {
                        from.SendMessage("Este item esta sem cargas");// This item is out of charges.
                        m_Addon.Using = false;
                        return;
                    }

                    value = tool.SkillValue;
                }
                else
                {
                    value = from.Skills[m_CraftSystem.MainSkill].Base;
                }

                if (m_CraftSystem is DefTinkering && targeted is IRepairableMobile)
                {
                    if (TryRepairMobile(from, (IRepairableMobile)targeted, usingDeed, out toDelete))
                    {
                        number = "Voce concertou o item"; // You repair the item.
                    }
                    else
                    {
                        number = "Voce nao pode arrumar isto"; // You can't repair that.
                    }
                }
                else if (targeted is Item)
                {
                    if (from.InRange(((Item)targeted).GetWorldLocation(), 2))
                    {
                        if (!CheckDeed(from))
                        {
                            if (m_Addon != null)
                                m_Addon.Using = false;

                            return;
                        }

                        if (!AllowsRepair(targeted, m_CraftSystem))
                        {
                            from.SendMessage("Voce nao pode reparar isto"); // You can't repair that.

                            if (m_Addon != null)
                                m_Addon.Using = false;

                            return;
                        }

                        if (m_CraftSystem.CanCraft(from, m_Tool, targeted.GetType()) == 1044267)
                        {
                            number = "Voce precisa estar perto de uma forja ou bigorna"; // You must be near a forge and and anvil to repair items. * Yes, there are two and's *
                        }
                        else if (!usingDeed && m_CraftSystem is DefTinkering && targeted is BrokenAutomatonHead)
                        {
                            if (((BrokenAutomatonHead)targeted).TryRepair(from))
                                number = "Voce arruma o item"; // You repair the item.
                            else
                                number = "Voce nao conseguiu arrumar o item"; // You fail to repair the item.
                        }
                        else if (targeted is BaseWeapon)
                        {
                            BaseWeapon weapon = (BaseWeapon)targeted;
                            SkillName skill = m_CraftSystem.MainSkill;
                            int toWeaken = 0;

                            if (Core.AOS)
                            {
                                toWeaken = 1;
                            }
                            else if (skill != SkillName.Tailoring)
                            {
                                double skillLevel = value;

                                if (skillLevel >= 110)
                                    toWeaken = 1;
                                else if (skillLevel >= 101)
                                    toWeaken = 2;
                                else if (skillLevel >= 90)
                                    toWeaken = 3;
                                else if (skillLevel >= 80)
                                    toWeaken = 4;
                                else
                                    toWeaken = 5;
                            }

                            if (m_CraftSystem.CraftItems.SearchForSubclass(weapon.GetType()) == null && !CheckSpecial(weapon))
                            {
                                number = "Voce nao pode arrumar isto assim"; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
                            }
                            else if (!weapon.IsChildOf(from.Backpack) && (!Core.ML || weapon.Parent != from))
                            {
                                number = "O item precisa estar em sua mochila"; // The item must be in your backpack to repair it.
                            }
                            else if (!Core.AOS && weapon.PoisonCharges != 0)
                            {
                                number = "Voce nao pode arrumar o item nesse estado"; // You cannot repair an item while a caustic substance is on it.
                            }
                            else if (weapon.MaxHitPoints <= 0 || weapon.HitPoints == weapon.MaxHitPoints)
                            {
                                number = "O item ta novinho"; // That item is in full repair
                            }
                            else if (weapon.MaxHitPoints <= toWeaken)
                            {
                                number = "Este item ja foi arrumado muitas vezes"; // That item has been repaired many times, and will break if repairs are attempted again.
                            }
                            else if (weapon.NegativeAttributes.NoRepair > 0)
                            {
                                number = "Este item nao pode ser arrumado"; // That item cannot be repaired.
                            }
                            else
                            {
                                if (CheckWeaken(from, skill, weapon.HitPoints, weapon.MaxHitPoints))
                                {
                                    weapon.MaxHitPoints -= toWeaken;
                                    weapon.HitPoints = Math.Max(0, weapon.HitPoints - toWeaken);
                                }

                                if (CheckRepairDifficulty(from, skill, weapon.HitPoints, weapon.MaxHitPoints))
                                {
                                    number = "Voce arrumou o item"; // You repair the item.
                                    m_CraftSystem.PlayCraftEffect(from);
                                    weapon.HitPoints = weapon.MaxHitPoints;
                                    /*
                                    if(weapon.Elemento != ElementoPvM.None)
                                    {
                                        weapon.Elemento = ElementoPvM.None;
                                        from.SendMessage("O equipamento perdeu o elemento ao reparar");
                                    }
                                    */
                                }
                                else
                                {
                                    number = "Voce falhou em arrumar o item"; // You fail to repair the item. [And the contract is destroyed]
                                    m_CraftSystem.PlayCraftEffect(from);
                                }

                                toDelete = true;
                            }
                        }
                        else if (targeted is BaseArmor)
                        {
                            BaseArmor armor = (BaseArmor)targeted;
                            SkillName skill = m_CraftSystem.MainSkill;
                            int toWeaken = 0;

                            if (Core.AOS)
                            {
                                toWeaken = 1;
                            }
                            else if (skill != SkillName.Tailoring)
                            {
                                double skillLevel = value;

                                if (skillLevel >= 90.0)
                                    toWeaken = 1;
                                else if (skillLevel >= 70.0)
                                    toWeaken = 2;
                                else
                                    toWeaken = 3;
                            }

                            if (m_CraftSystem.CraftItems.SearchForSubclass(armor.GetType()) == null && !CheckSpecial(armor))
                            {
                                number ="Voce nao pode arrumar isto"; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
                            }
                            else if (!armor.IsChildOf(from.Backpack) && (!Core.ML || armor.Parent != from))
                            {
                                number = "O item precisa estar em sua mochila"; // The item must be in your backpack to repair it.
                            }
                            else if (armor.MaxHitPoints <= 0 || armor.HitPoints == armor.MaxHitPoints)
                            {
                                number = "O item ta novinho bonitinho"; // That item is in full repair
                            }
                            else if (armor.MaxHitPoints <= toWeaken)
                            {
                                number = "O item ja foi arrumado muitas vezes"; // That item has been repaired many times, and will break if repairs are attempted again.
                            }
                            else if (armor.NegativeAttributes.NoRepair > 0)
                            {
                                number = "Este item nao pode ser arrumado"; // That item cannot be repaired.
                            }
                            else
                            {
                                if (CheckWeaken(from, skill, armor.HitPoints, armor.MaxHitPoints))
                                {
                                    armor.MaxHitPoints -= toWeaken;
                                    armor.HitPoints = Math.Max(0, armor.HitPoints - toWeaken);
                                }

                                if (CheckRepairDifficulty(from, skill, armor.HitPoints, armor.MaxHitPoints))
                                {
                                    number = "Voce arrumou o item"; // You repair the item.
                                    m_CraftSystem.PlayCraftEffect(from);
                                    armor.HitPoints = armor.MaxHitPoints;
                                    /*
                                    if (armor.Elemento != ElementoPvM.None)
                                    {
                                        armor.Elemento = ElementoPvM.None;
                                        from.SendMessage("O equipamento perdeu o elemento ao reparar");
                                    }
                                    */
                                }
                                else
                                {
                                    number = "Voce falhou em arrumar o item"; // You fail to repair the item. [And the contract is destroyed]
                                    m_CraftSystem.PlayCraftEffect(from);
                                }

                                toDelete = true;
                            }
                        }
                        else if (targeted is BaseJewel)
                        {
                            BaseJewel jewel = (BaseJewel)targeted;
                            SkillName skill = m_CraftSystem.MainSkill;
                            int toWeaken = 0;

                            if (Core.AOS)
                            {
                                toWeaken = 1;
                            }
                            else if (skill != SkillName.Tailoring)
                            {
                                double skillLevel = value;

                                if (skillLevel >= 90.0)
                                    toWeaken = 1;
                                else if (skillLevel >= 70.0)
                                    toWeaken = 2;
                                else
                                    toWeaken = 3;
                            }

                            if (m_CraftSystem.CraftItems.SearchForSubclass(jewel.GetType()) == null && !CheckSpecial(jewel))
                            {
                                number ="Item nao pode ser arrumado"; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
                            }
                            else if (!jewel.IsChildOf(from.Backpack))
                            {
                                number = "O item precisa estar em sua mochila para arrumar"; // The item must be in your backpack to repair it.
                            }
                            else if (jewel.MaxHitPoints <= 0 || jewel.HitPoints == jewel.MaxHitPoints)
                            {
                                number = "O item esta arrumado"; // That item is in full repair
                            }
                            else if (jewel.MaxHitPoints <= toWeaken)
                            {
                                number = "O item ja foi arrumado muitas vezes"; // That item has been repaired many times, and will break if repairs are attempted again.
                            }
                            else if (jewel.NegativeAttributes.NoRepair > 0)
                            {
                                number = "O item nao pode ser arrumado"; // That item cannot be repaired.
                            }
                            else
                            {
                                if (CheckWeaken(from, skill, jewel.HitPoints, jewel.MaxHitPoints))
                                {
                                    jewel.MaxHitPoints -= toWeaken;
                                    jewel.HitPoints = Math.Max(0, jewel.HitPoints - toWeaken);
                                }

                                if (CheckRepairDifficulty(from, skill, jewel.HitPoints, jewel.MaxHitPoints))
                                {
                                    number = "Voce arrumou o item"; // You repair the item.
                                    m_CraftSystem.PlayCraftEffect(from);
                                    jewel.HitPoints = jewel.MaxHitPoints;
                                }
                                else
                                {
                                    number = "Voce falhou em arrumar o item"; // You fail to repair the item. [And the contract is destroyed]
                                    m_CraftSystem.PlayCraftEffect(from);
                                }

                                toDelete = true;
                            }
                        }
                        else if (targeted is BaseClothing)
                        {
                            BaseClothing clothing = (BaseClothing)targeted;
                            SkillName skill = m_CraftSystem.MainSkill;
                            int toWeaken = 0;

                            if (Core.AOS)
                            {
                                toWeaken = 1;
                            }
                            else if (skill != SkillName.Tailoring)
                            {
                                double skillLevel = value;

                                if (skillLevel >= 90.0)
                                    toWeaken = 1;
                                else if (skillLevel >= 70.0)
                                    toWeaken = 2;
                                else
                                    toWeaken = 3;
                            }

                            if (m_CraftSystem.CraftItems.SearchForSubclass(clothing.GetType()) == null && !CheckSpecial(clothing))
                            {
                                number = "Nao posso arrumar isso"; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
                            }
                            else if (!clothing.IsChildOf(from.Backpack) && (!Core.ML || clothing.Parent != from))
                            {
                                number = "Precisa estar em sua mochila"; // The item must be in your backpack to repair it.
                            }
                            else if (clothing.MaxHitPoints <= 0 || clothing.HitPoints == clothing.MaxHitPoints)
                            {
                                number = "O item esta totalmente reparado"; // That item is in full repair
                            }
                            else if (clothing.MaxHitPoints <= toWeaken)
                            {
                                number = "Isto ja foi arrumado muitas vezes."; // That item has been repaired many times, and will break if repairs are attempted again.
                            }
                            else if (clothing.NegativeAttributes.NoRepair > 0)// quick fix
                            {
                                number = "Este item nao pode ser arrumado"; // That item cannot be repaired.
                            }
                            else
                            {
                                if (CheckWeaken(from, skill, clothing.HitPoints, clothing.MaxHitPoints))
                                {
                                    clothing.MaxHitPoints -= toWeaken;
                                    clothing.HitPoints = Math.Max(0, clothing.HitPoints - toWeaken);
                                }

                                if (CheckRepairDifficulty(from, skill, clothing.HitPoints, clothing.MaxHitPoints))
                                {
                                    number = "Voce arrumou o item"; // You repair the item.
                                    m_CraftSystem.PlayCraftEffect(from);
                                    clothing.HitPoints = clothing.MaxHitPoints;
                                }
                                else
                                {
                                    number = "Falhou em concertar o item"; // You fail to repair the item. [And the contract is destroyed]
                                    m_CraftSystem.PlayCraftEffect(from);
                                }

                                toDelete = true;
                            }
                        }
                        else if (targeted is BaseTalisman)
                        {
                            BaseTalisman talisman = (BaseTalisman)targeted;
                            SkillName skill = m_CraftSystem.MainSkill;
                            int toWeaken = 0;

                            if (Core.AOS)
                            {
                                toWeaken = 1;
                            }
                            else if (skill != SkillName.Tailoring)
                            {
                                double skillLevel = value;

                                if (skillLevel >= 90.0)
                                    toWeaken = 1;
                                else if (skillLevel >= 70.0)
                                    toWeaken = 2;
                                else
                                    toWeaken = 3;
                            }

                            if (!(m_CraftSystem is DefTinkering))
                            {
                                number = "Nao posso arrumar isso"; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
                            }
                            else if (!talisman.IsChildOf(from.Backpack) && (!Core.ML || talisman.Parent != from))
                            {
                                number = "Precisa estar na mochila"; // The item must be in your backpack to repair it.
                            }
                            else if (talisman.MaxHitPoints <= 0 || talisman.HitPoints == talisman.MaxHitPoints)
                            {
                                number = "O item esta novo em folha"; // That item is in full repair
                            }
                            else if (talisman.MaxHitPoints <= toWeaken)
                            {
                                number = "Este item ja foi arrumado muitas vezes..."; // That item has been repaired many times, and will break if repairs are attempted again.
                            }
                            else if (!talisman.CanRepair)// quick fix
                            {
                                number = "Item nao pode ser arrumado"; // That item cannot be repaired.
                            }
                            else
                            {
                                if (CheckWeaken(from, skill, talisman.HitPoints, talisman.MaxHitPoints))
                                {
                                    talisman.MaxHitPoints -= toWeaken;
                                    talisman.HitPoints = Math.Max(0, talisman.HitPoints - toWeaken);
                                }

                                if (CheckRepairDifficulty(from, skill, talisman.HitPoints, talisman.MaxHitPoints))
                                {
                                    number = "Voce arrumou o item"; // You repair the item.
                                    m_CraftSystem.PlayCraftEffect(from);
                                    talisman.HitPoints = talisman.MaxHitPoints;
                                }
                                else
                                {
                                    number = "Voce falhou em arrumar o item"; // You fail to repair the item. [And the contract is destroyed]
                                    m_CraftSystem.PlayCraftEffect(from);
                                }

                                toDelete = true;
                            }
                        }
                        else if (targeted is BlankScroll)
                        {
                            if (!usingDeed)
                            {
                                SkillName skill = m_CraftSystem.MainSkill;

                                if (from.Skills[skill].Value >= 50.0)
                                {
                                    ((BlankScroll)targeted).Consume(1);
                                    RepairDeed deed = new RepairDeed(RepairDeed.GetTypeFor(m_CraftSystem), from.Skills[skill].Value, from);
                                    from.AddToBackpack(deed);

                                    number = "Voce criou o item e colocou em sua mochila"; // You create the item and put it in your backpack.
                                }
                                else
                                {
                                    number = "Voce nao tem nivel pra fazer isto."; // You must be at least apprentice level to create a repair service contract.
                                }
                            }
                            else
                            {
                                number = "Nao pode arrumar este item com esse tipo de contrato"; // You cannot repair that item with this type of repair contract.
                            }
                        }
                        else
                        {
                            number = "Voce nao pode arrumar isso"; // You can't repair that.
                        }
                    }
                    else
                    {
                        number = "Isso esta muito longe"; // That is too far away.
                    }
                }
                else
                {
                    number = "Voce nao pode arrumar isso"; // You can't repair that.
                }

                if (!usingDeed)
                {
                    CraftContext context = m_CraftSystem.GetContext(from);
                    from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, number));
                }
                else
                {
                    if (m_Addon != null && !m_Addon.Deleted)
                    {
                        var tool = m_Addon.Tools.Find(x => x.System == m_CraftSystem);

                        tool.Charges--;

                        from.SendGump(new RepairBenchGump(from, m_Addon));

                        from.SendMessage(number);
                    }
                    else
                    {
                        from.SendMessage(number);

                        if (toDelete)
                            m_Deed.Delete();
                    }                   
                }
            }

            protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
            {
                if (m_Addon != null && !m_Addon.Deleted)
                {
                    from.SendGump(new RepairBenchGump(from, m_Addon));
                }
            }

            public bool TryRepairMobile(Mobile from, IRepairableMobile m, bool usingDeed, out bool toDelete)
            {
                int damage = m.HitsMax - m.Hits;
                BaseCreature bc = m as BaseCreature;
                toDelete = false;

                string name = bc != null ? bc.Name : "the creature";

                if (!from.InRange(m.Location, 2))
                {
                    from.SendLocalizedMessage(1113612, name); // You must move closer to attempt to repair ~1_CREATURE~.
                }
                else if (bc != null && bc.IsDeadBondedPet)
                {
                    from.SendLocalizedMessage(500426); // You can't repair that.
                }
                else if (damage <= 0)
                {
                    from.SendLocalizedMessage(1113613, name); // ~1_CREATURE~ doesn't appear to be damaged.
                }
                else
                {
                    double value = 0;

                    if (m_Deed != null)
                    {
                        value = m_Deed.SkillLevel;
                    }
                    else if (m_Addon != null)
                    {
                        value = m_Addon.Tools.Find(x => x.System == m_CraftSystem).SkillValue;
                    }
                    else
                    {
                        value = from.Skills[SkillName.Tinkering].Value;
                    }

                    double skillValue = value;
                    double required = m is KotlAutomaton ? 80.0 : 0.1;

                    if (skillValue < required)
                    {
                        if (required == 80.0)
                            from.SendLocalizedMessage(1157049, name); // You must have at least 80 tinkering skill to attempt to repair ~1_CREATURE~.
                        else
                            from.SendLocalizedMessage(1113614, name); // You must have some tinkering skills to attempt to repair a ~1_CREATURE~.
                    }
                    else if (!from.CanBeginAction(typeof(IRepairableMobile)))
                    {
                        from.SendLocalizedMessage(1113611, name); // You must wait a moment before attempting to repair ~1_CREATURE~ again.
                    }
                    else if (bc != null && bc.GetMaster() != null && bc.GetMaster() != from && !bc.GetMaster().InRange(from.Location, 10))
                    {
                        from.SendLocalizedMessage(1157045); // The pet's owner must be nearby to attempt repair.
                    }
                    else if (!from.CanBeBeneficial(bc, false, false))
                    {
                        from.SendLocalizedMessage(1001017); // You cannot perform beneficial acts on your target.
                    }
                    else
                    {
                        if (damage > (int)(skillValue * 0.6))
                            damage = (int)(skillValue * 0.6);

                        SkillLock sl = from.Skills[SkillName.Tinkering].Lock;
                        from.Skills[SkillName.Tinkering].SetLockNoRelay(SkillLock.Locked);

                        if (!from.CheckSkillMult(SkillName.Tinkering, 0.0, 100.0))
                            damage /= 6;

                        from.Skills[SkillName.Tinkering].SetLockNoRelay(sl);

                        Container pack = from.Backpack;

                        if (pack != null)
                        {
                            int v = pack.ConsumeUpTo(m.RepairResource, (damage + 4) / 5);

                            if (v <= 0 && m is Golem)
                                v = pack.ConsumeUpTo(typeof(BronzeIngot), (damage + 4) / 5);

                            if (v > 0)
                            {
                                m.Hits += damage;

                                if (damage > 1)
                                    from.SendLocalizedMessage(1113616, name); // You repair ~1_CREATURE~.
                                else
                                    from.SendLocalizedMessage(1157030, name); // You repair ~1_CREATURE~, but it barely helps.

                                toDelete = true;
                                double delay = 10 - (skillValue / 16.65);

                                from.BeginAction(typeof(IRepairableMobile));
                                Timer.DelayCall(TimeSpan.FromSeconds(delay), new TimerStateCallback(EndMobileRepair), from);

                                return true;
                            }
                            else if (m is Golem)
                            {
                                from.SendLocalizedMessage(1113615, name); // You need some iron or bronze ingots to repair the ~1_CREATURE~.
                            }
                            else
                            {
                                from.SendLocalizedMessage(1044037); // You do not have sufficient metal to make that.
                            }
                        }
                        else
                        {
                            from.SendLocalizedMessage(1044037); // You do not have sufficient metal to make that.
                        }
                    }
                }

                return false;
            }
        }

        public static bool AllowsRepair(object targeted, CraftSystem system)
        {
            if (targeted is IFactionItem && ((IFactionItem)targeted).FactionItemState != null)
                return false;

            if (targeted is BrokenAutomatonHead || targeted is IRepairableMobile)
                return true;

            return (targeted is BlankScroll ||
                    (targeted is BaseArmor && ((BaseArmor)targeted).CanRepair) ||
                    (targeted is BaseWeapon && ((BaseWeapon)targeted).CanRepair) ||
                    (targeted is BaseClothing && ((BaseClothing)targeted).CanRepair) ||
                    (targeted is BaseJewel && ((BaseJewel)targeted).CanRepair)) ||
                    (targeted is BaseTalisman && ((BaseTalisman)targeted).CanRepair);
        }
    }
}
