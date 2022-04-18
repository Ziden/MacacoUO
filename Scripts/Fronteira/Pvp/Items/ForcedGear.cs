using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Items;

namespace Server.TournamentSystem
{
    public enum ForcedGearType
    {
        None,
        Caster,
        Dexxer
    }

    public interface IDuelingGear : IEntity
    {
        ForcedGearType GearType { get; }
    }

    public class ForcedGear
    {
        public static Dictionary<ForcedGearType, ForcedGear> GearEntries { get; set; }
        public static List<IDuelingGear> GearPool { get; set; }

        public static Dictionary<ArenaFight, List<EquipmentEntry>> FightInfo { get; set; }
        public static Dictionary<Mobile, ForcedGearType> GearPreferences { get; set; }

        public static void Initialize()
        {
            GearEntries = new Dictionary<ForcedGearType, ForcedGear>();

            GearEntries[ForcedGearType.Caster] = new ForcedGear(7939, "Sobretudo do Mago", typeof(CastersRobe));
            GearEntries[ForcedGearType.Dexxer] = new ForcedGear(7940, "Sobretudo do Guerreiro", typeof(DexxerRobe));
        }

        public static ForcedGear GetEntry(ForcedGearType type)
        {
            if (GearEntries.ContainsKey(type))
            {
                return GearEntries[type];
            }

            return null;
        }

        public static Item GetDisplay(ForcedGearType type)
        {
            Item item = null;

            if (GearPool == null || !GearPool.Any(x => x.GearType == type))
            {
                var entry = GetEntry(type);

                if (entry != null)
                {
                    item = Loot.Construct(entry.GearType);

                    if (item != null)
                    {
                        ReturnGear(item as IDuelingGear);
                    }
                }
            }
            else
            {
                item = GearPool.FirstOrDefault(x => x.GearType == type) as Item;
            }

            return item;
        }

        public static void Dress(ArenaFight fight)
        {
            foreach (var m in fight.GetFighters())
            {
                var gear = AssignGear(GetPreference(m));

                if (fight is LastManStandingFight)
                {
                    ((Item)gear).Hue = Utility.RandomBirdHue(); 
                }
                else
                {
                    var team = fight.GetTeam(m);

                    if (team != null)
                    {
                        if (team == fight.TeamA)
                        {
                            ((Item)gear).Hue = Utility.RandomRedHue(); 
                        }
                        else
                        {
                            ((Item)gear).Hue = Utility.RandomBlueHue(); 
                        }
                    }
                    else
                    {
                        ((Item)gear).Hue = Utility.RandomBirdHue(); 
                    }
                }

                if (gear != null)
                {
                    var entry = new EquipmentEntry(m, gear);

                    List<Item> list = new List<Item>(m.Items.Where(i => i is ICombatEquipment && !(i is BaseWeapon)));
                    list.AddRange(m.Backpack.Items.Where(i => i is ICombatEquipment && !(i is BaseWeapon)));

                    bool moved = false;

                    foreach (var item in list)
                    {
                        entry.Equipment[item] = m.Items.Contains(item);
                        m.BankBox.DropItem(item);

                        if (!moved)
                            moved = true;
                    }

                    if (!m.EquipItem((Item)gear))
                    {
                        m.Backpack.DropItem((Item)gear);
                        m.SendMessage(ArenaHelper.EquipmentMessageHue, "Items movidos a mochila.{0}.", moved ? " Alguns movidos pro banco" : String.Empty);
                    }
                    else
                    {
                        m.SendMessage(ArenaHelper.EquipmentMessageHue, "Voce equipou seu equip de arena{0}.", moved ? " Alguns items motivos para o banco." : String.Empty);
                    }

                    CheckRegistration(fight);

                    FightInfo[fight].Add(entry);
                    ColUtility.Free(list);
                }
                else // Error Handling
                {
                    m.SendMessage(ArenaHelper.EquipmentMessageHue, "Erro no duelo - usando seus proprios equips.");

                    foreach (var mob in fight.GetFighters().Where(f => f != m))
                    {
                        mob.SendMessage(ArenaHelper.EquipmentMessageHue, "Erro com equips de {0}'s.", m.Name);
                    }
                }
            }
        }

        public static IDuelingGear AssignGear(ForcedGearType type)
        {
            var entry = GetEntry(type);

            if (entry != null)
            {
                IDuelingGear gear = null;

                if (GearPool != null && GearPool.Any(g => g.GearType == type))
                {
                    gear = GearPool.FirstOrDefault(g => g.GearType == type);

                    if (gear != null)
                    {
                        GearPool.Remove(gear);
                    }
                }

                if (gear == null)
                {
                    gear = Loot.Construct(entry.GearType) as IDuelingGear;
                }

                if (gear != null)
                {


                    return gear;
                }
                else
                {
                    Utility.WriteConsoleColor(ConsoleColor.Red, "Erri {0}!", entry.GearType.Name);
                }
            }

            return null;
        }

        public static void ReturnGear(IDuelingGear gear)
        {
            if (GearPool == null)
            {
                GearPool = new List<IDuelingGear>();
            }

            if (!GearPool.Contains(gear))
            {
                GearPool.Add(gear);
            }

            ((Item)gear).MoveToWorld(new Point3D(1, 1, 1), Map.Internal);
        }

        private static void CheckRegistration(ArenaFight fight)
        {
            if (FightInfo == null)
            {
                FightInfo = new Dictionary<ArenaFight, List<EquipmentEntry>>();
            }

            if (!FightInfo.ContainsKey(fight))
            {
                FightInfo[fight] = new List<EquipmentEntry>();
            }
        }

        public static void RegisterFight(ArenaFight fight)
        {
            if (!fight.UseOwnGear)
            {
                Dress(fight);
            }

            if (fight.HasRule(FightRules.PureMage))
            {
                RemoveItems(fight, typeof(BaseWeapon));
            }
        }

        public static void UnregisterFight(ArenaFight fight)
        {
            if (FightInfo != null && FightInfo.ContainsKey(fight))
            {
                foreach (var entry in FightInfo[fight])
                {
                    bool reequipAll = true;

                    if (entry.Gear != null)
                    {
                        ReturnGear(entry.Gear);
                    }

                    foreach (var kvp in entry.Equipment)
                    {
                        if (!kvp.Value || !entry.Owner.EquipItem(kvp.Key))
                        {
                            if (!entry.Owner.Backpack.TryDropItem(entry.Owner, kvp.Key, false))
                            {
                                if (reequipAll)
                                {
                                    reequipAll = false;
                                }
                            }
                        }
                    }

                    if (!reequipAll)
                    {
                        entry.Owner.SendMessage(ArenaHelper.EquipmentMessageHue, "Partes do seu equip proibido foi movido para seu banco.");
                    }
                    else
                    {
                        entry.Owner.SendMessage(ArenaHelper.EquipmentMessageHue, "Seus items proibidos foram retornados a voce.");
                    }
                }

                FightInfo.Remove(fight);

                if (FightInfo.Count == 0)
                {
                    FightInfo = null;
                }
            }
        }

        public static void RemoveItems(ArenaFight fight, Type t)
        {
            foreach (var m in fight.GetFighters())
            {
                List<Item> toTake = new List<Item>();

                foreach (var item in m.Items)
                {
                    var type = item.GetType();

                    if (type == t || type.IsSubclassOf(t))
                    {
                        toTake.Add(item);
                    }
                }

                foreach (var item in m.Backpack.Items)
                {
                    var type = item.GetType();

                    if (type == t || type.IsSubclassOf(t))
                    {
                        toTake.Add(item);
                    }
                }

                if (toTake.Count > 0)
                {
                    CheckRegistration(fight);

                    EquipmentEntry entry = new EquipmentEntry(m);

                    foreach (var item in toTake)
                    {
                        entry.Equipment[item] = m.Items.Contains(item);
                        m.BankBox.DropItem(item);
                    }

                    FightInfo[fight].Add(entry);

                    m.SendMessage(ArenaHelper.EquipmentMessageHue, "Parte dos seus items foram para seu banco e serao retornados a voce depois do duelo.");
                }

                ColUtility.Free(toTake);
            }
        }

        public static ForcedGearType GetPreference(Mobile m)
        {
            if (GearPreferences == null)
            {
                GearPreferences = new Dictionary<Mobile, ForcedGearType>();
            }

            if (!GearPreferences.ContainsKey(m))
            {
                GearPreferences[m] = ForcedGearType.Caster;
            }

            return GearPreferences[m];
        }

        public static void SetPreference(Mobile m, ForcedGearType type)
        {
            if (GearPreferences == null)
            {
                GearPreferences = new Dictionary<Mobile, ForcedGearType>();
            }

            GearPreferences[m] = type;
        }

        public static void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(GearPreferences == null ? 0 : GearPreferences.Count);

            if(GearPreferences != null)
            {
                foreach(var kvp in GearPreferences)
                {
                    writer.Write(kvp.Key);
                    writer.Write((int)kvp.Value);
                }
            }

            writer.Write(GearPool == null ? 0 : GearPool.OfType<Item>().Count());

            if(GearPool != null)
            {
                foreach(var item in GearPool.OfType<Item>())
                {
                    writer.Write(item);
                }
            }

            writer.Write(FightInfo == null ? 0 : FightInfo.Count);

            if (FightInfo != null)
            {
                foreach (var kvp in FightInfo)
                {
                    writer.Write(kvp.Value.Count);

                    foreach (var entry in kvp.Value)
                    {
                        entry.Serialize(writer);
                    }
                }
            }
        }

        public static void Deserialize(GenericReader reader)
        {
            reader.ReadInt(); // version

            int count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                var m = reader.ReadMobile();
                var t = (ForcedGearType)reader.ReadInt();

                if (m != null)
                {
                    SetPreference(m, t);
                }
            }

            count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                var gear = reader.ReadItem() as IDuelingGear;

                if (gear != null)
                {
                    ReturnGear(gear);
                }
            }

            count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                int c = reader.ReadInt();

                for (int j = 0; j < c; j++)
                {
                    new EquipmentEntry(reader);
                }
            }
        }

        public int ItemID { get; set; }
        public string DisplayName { get; set; }
        public Type GearType { get; set; }

        public ForcedGear(int itemID, string name, Type type)
        {
            ItemID = itemID;
            DisplayName = name;
            GearType = type;
        }
    }

    public class EquipmentEntry
    {
        public Mobile Owner { get; set; }
        public IDuelingGear Gear { get; set; }
        public Dictionary<Item, bool> Equipment { get; set; }

        public EquipmentEntry(Mobile owner)
            : this(owner, null)
        {
        }

        public EquipmentEntry(Mobile owner, IDuelingGear gear)
        {
            Owner = owner;
            Gear = gear;

            Equipment = new Dictionary<Item, bool>();
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(Owner);
            writer.Write((Item)Gear);

            writer.Write(Equipment.Count);
            foreach (var kvp in Equipment)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }

        public EquipmentEntry(GenericReader reader)
        {
            reader.ReadInt(); // version

            var m = reader.ReadMobile();
            var gear = reader.ReadItem() as IDuelingGear;

            if (gear != null)
            {
                ForcedGear.ReturnGear(gear);
            }

            var count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                var item = reader.ReadItem();
                var equipped = reader.ReadBool();

                if (m != null)
                {
                    if (equipped)
                    {
                        if (!m.EquipItem(item))
                        {
                            m.Backpack.TryDropItem(m, item, false);
                        }
                    }
                }
            }
        }
    }

    public class CastersRobe : Robe, IDuelingGear
    {
        public virtual ForcedGearType GearType { get { return ForcedGearType.Caster; } }

        public CastersRobe()
        {
            Name = "Sobretudo Do Mago (sujo de sangue)";

            Hue = 2746;
            LootType = LootType.Blessed;
            Attributes.LowerRegCost = 100;
        }

        public override bool CanEquip(Mobile from)
        {
            if (!from.Region.IsPartOf<FightRegion>())
            {
                from.SendMessage("Voce nao pode equipar isto aqui.");
                return false;
            }

            return base.CanEquip(from);
        }

        public CastersRobe(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (RootParent is Mobile)
            {
                ForcedGear.ReturnGear(this);
            }
        }
    }

    public class DexxerRobe : Robe, IDuelingGear
    {
        public virtual ForcedGearType GearType { get { return ForcedGearType.Dexxer; } }

        public DexxerRobe()
        {
            Name = "Sobretudo do Guerreiro (Sujo de Sangue)";

            Hue = 2720;
            LootType = LootType.Blessed;
        }

        public override bool CanEquip(Mobile from)
        {
            if (!from.Region.IsPartOf<FightRegion>())
            {
                from.SendMessage("Voce nao pode equipar isto aqui.");
                return false;
            }

            return base.CanEquip(from);
        }

        public DexxerRobe(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (RootParent is Mobile)
            {
                ForcedGear.ReturnGear(this);
            }
        }
    }
}
