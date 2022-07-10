using System;
using Server.Engines.Craft;
using Server.Mobiles;
using Server.Spells;
using Server.Targeting;

namespace Server.Items
{
    public abstract class BaseOre : Item
    {
        protected virtual CraftResource DefaultResource { get { return CraftResource.Ferro; } }

        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get
            {
                return m_Resource;
            }
            set
            {
                m_Resource = value;
                InvalidateProperties();
            }
        }

        public abstract BaseIngot GetIngot();

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 2: // Reset from Resource System
                    m_Resource = DefaultResource;
                    reader.ReadString();
                    break;
                case 1:
                    {
                        m_Resource = (CraftResource)reader.ReadInt();
                        break;
                    }
                case 0:
                    {
                        OreInfo info;

                        switch (reader.ReadInt())
                        {
                            case 0:
                                info = OreInfo.Iron;
                                break;
                            case 1:
                                info = OreInfo.DullCopper;
                                break;
                            case 2:
                                info = OreInfo.ShadowIron;
                                break;
                            case 3:
                                info = OreInfo.Copper;
                                break;
                            case 4:
                                info = OreInfo.Bronze;
                                break;
                            case 5:
                                info = OreInfo.Gold;
                                break;
                            case 6:
                                info = OreInfo.Agapite;
                                break;
                            case 7:
                                info = OreInfo.Verite;
                                break;
                            case 8:
                                info = OreInfo.Valorite;
                                break;
                            default:
                                info = null;
                                break;
                        }

                        m_Resource = CraftResources.GetFromOreInfo(info);
                        break;
                    }
            }
        }

        private static int RandomSize()
        {
            /*
            if (rand < 0.12)
                return 0x19B7;
            else if (rand < 0.18)
                return 0x19B8;
            else if (rand < 0.25)
                return 0x19BA;
            else
            */
            return 0x19B9;
        }

        public BaseOre(CraftResource resource)
            : this(resource, 1)
        {
        }

        public BaseOre(CraftResource resource, int amount)
            : base(RandomSize())
        {
            Stackable = true;
            Amount = amount;
            Hue = CraftResources.GetHue(resource);
            Weight = Shard.SPHERE_STYLE ? 0.8 : 4;
            Name = "Minerio de " + resource.ToString();
            m_Resource = resource;
        }

        public BaseOre(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (this.Amount == 1)
                list.Add("Minério de " + m_Resource); // ore
            else
                list.Add(this.Amount + " Minérios de " + m_Resource); // ore
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.RevealingAction();

            if (!Movable)
                return;

            if (RootParent != from && Movable == false)
            {
                from.SendMessage("Isto nao esta acessivel"); // That is not accessible
            }
            else if (from.InRange(GetWorldLocation(), 2))
            {
                //from.SendMessage("Selecione uma forja"); // Select the forge on which to smelt the ore, or another pile of ore with which to combine it.
                //from.Target = new InternalTarget(this);
                var temForja = false;
                Point3D forja = Point3D.Zero;

                var m = from.Location;
                for (int x = m.X - 1; x <= m.X + 1; x++)
                {
                    for (int y = m.Y - 1; y <= m.Y + 1; y++)
                    {
                        var tiles = from.Map.Tiles.GetStaticTiles(x, y);
                        if (tiles.Length > 0)
                        {
                            foreach (var tile in tiles)
                            {
                                if (IsForge(tile))
                                {
                                    forja = new Point3D(x, y, tile.Z);
                                    Shard.Debug("" + forja.ToString());
                                    temForja = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                // FireBeetle como forja...
                IPooledEnumerable eable = from.GetMobilesInRange(1);

                foreach (Mobile mob in eable)
                {

                    if (mob is BaseCreature && (mob as BaseCreature).Controlled && !(mob as BaseCreature).IsDeadBondedPet && mob is FireBeetle) //mob is typeof(FireBeetle))
                    {
                        forja = new Point3D(mob.X, mob.Y, mob.Z);
                        //Shard.Debug("FireBeetle: " + forja.ToString());
                        temForja = true;
                        break;
                    }
                }



                if (!temForja)
                {
                    var items = from.Map.GetItemsInRange(from.Location, 2);
                    foreach (var i in items)
                    {
                        if (IsForge(i))
                        {
                            temForja = true;
                            forja = i.Location;
                            break;
                        }
                    }
                }
                if (!temForja)
                {
                    from.SendMessage("Voce precisa estar proximo de uma forja");
                }
                else
                {
                    Forge(from, this, forja);
                }
            }
            else
            {
                from.SendMessage("Isto esta muito longe"); // The ore is too far away.
            }
        }

        private void Forge(Mobile from, BaseOre ore, Point3D forge)
        {
            double difficulty;

            #region Void Pool Rewards
            bool talisman = false;
            SmeltersTalisman t = from.FindItemOnLayer(Layer.Talisman) as SmeltersTalisman;
            if (t != null && t.Resource == ore.Resource)
                talisman = true;
            #endregion

            switch (ore.Resource)
            {
                /*default:
                    difficulty = 50.0;
                    break;
                case CraftResource.Berilo:
                    difficulty = 65.0;
                    break;
                case CraftResource.Vibranium:
                    difficulty = 70.0;
                    break;
                case CraftResource.Cobre:
                    difficulty = 75.0;
                    break;
                case CraftResource.Bronze:
                    difficulty = 80.0;
                    break;
                case CraftResource.Prata:
                    difficulty = 85.0;
                    break;
                case CraftResource.Niobio:
                    difficulty = 90.0;
                    break;
                case CraftResource.Lazurita:
                    difficulty = 95.0;
                    break;
                case CraftResource.Quartzo:
                    difficulty = 99.0;
                    break;*/

                default:
                    difficulty = 40.0;
                    break;
                case CraftResource.Cobre:
                    difficulty = 65.0;
                    break;
                case CraftResource.Bronze:
                    difficulty = 70.0;
                    break;
                case CraftResource.Dourado:
                    difficulty = 75.0;
                    break;
                case CraftResource.Niobio:
                    difficulty = 80.0;
                    break;
                case CraftResource.Lazurita:
                    difficulty = 90;
                    break;
                case CraftResource.Quartzo:
                    difficulty = 100;
                    break;
                case CraftResource.Berilo:
                    difficulty = 100;
                    break;
                case CraftResource.Vibranium:
                    difficulty = 110;
                    break;
                case CraftResource.Adamantium:
                    difficulty = 110;
                    break;
            }

            /*double minSkill = difficulty - 25.0;
            double maxSkill = difficulty + 25.0;*/

            double minSkill = difficulty - 20.0;
            double maxSkill = difficulty + 25.0;

            if (difficulty > 50.0 && difficulty > from.Skills[SkillName.Mining].Value && !talisman)
            {
                from.SendMessage("Você não sabe fundir esse minério estranho"); // You have no idea how to smelt this strange ore!
                return;
            }

            if (ore.ItemID == 0x19B7 && ore.Amount < 2)
            {
                from.SendMessage("Preciso de mais minerio para fazer barras");// There is not enough metal-bearing ore in this pile to make an ingot.
                return;
            }

            if (from is PlayerMobile)
            {
                var wisp = ((PlayerMobile)from).Wisp;
                if (wisp != null)
                {
                    wisp.Smelta();
                }
            }

            var upa = true;

            double mult = 1;
            if (ore.Amount < 10)
            {
                mult = ore.Amount / 10;
            }
            int toConsume = ore.Amount;
            if (toConsume <= 0)
            {
                from.SendMessage("Preciso de mais ferro para fazer barras"); // There is not enough metal-bearing ore in this pile to make an ingot.
                return;
            }

            if (toConsume > 10)
            {
                toConsume = 10;
            }

            if (ore is SilverOre)
            {
                if (!from.Backpack.ConsumeTotal(typeof(BronzeIngot), toConsume))
                {
                    from.SendMessage("Voce precisa ter lingotes de bronze para conseguir fazer a liga de dourado");
                    return;
                }
            }

            if (talisman || from.CheckTargetSkillMinMax(SkillName.Mining, forge, minSkill, maxSkill, mult))
            {

                int ingotAmount;

                ingotAmount = toConsume;

                BaseIngot ingot = ore.GetIngot();
                ingot.Amount = ingotAmount;

                if (ore.HasSocket<Caddellite>())
                {
                    ingot.AttachSocket(new Caddellite());
                }

                if (ingot is IronIngot)
                    ingot.Amount *= 2;

                //forge.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
                ore.Consume(toConsume);
                from.AddToBackpack(ingot);
                //from.PlaySound( 0x57 );



                //from.PlaySound(0x240);
                if (talisman && t != null)
                {
                    t.UsesRemaining--;
                    from.SendMessage("A magia do talisman faz voce fundir os minerios perfeitamente"); // The magic of your talisman guides your hands as you purify the metal. Success is ensured!
                }
                else
                    from.SendMessage("Você funde o minério removendo as impurezas"); // You smelt the ore removing the impurities and put the metal in your backpack.

            }
            else
            {
                if (ore.Resource > CraftResource.Niobio)
                    ore.Amount -= 5;
                else
                    ore.Amount -= 3;
                if (ore.Amount <= 2)
                    ore.Consume();

                from.SendMessage("Você não consegue fundir os minérios e perde um pouco do material"); // You burn away the impurities but are left with less useable metal.
            }

            SpellHelper.Turn(from, forge);
            from.PlayAttackAnimation();
            from.PlaySound(0x2B);

            var loc = forge;
            loc.Z = loc.Z + 7;
            Effects.SendLocationParticles(EffectItem.Create(loc, from.Map, TimeSpan.FromSeconds(1)), 0x3709, 30, 30, 5052);

            Timer.DelayCall(TimeSpan.FromSeconds(2), () =>
            {
                if (from != null && this != null && this.Amount > 0 && !this.Deleted)
                    this.OnDoubleClick(from);
            });
        }

        private bool IsForge(object obj)
        {
            if (Core.ML && obj is Mobile && ((Mobile)obj).IsDeadBondedPet)
                return false;

            if (obj.GetType().IsDefined(typeof(ForgeAttribute), false))
                return true;

            int itemID = 0;

            if (obj is Item)
                itemID = ((Item)obj).ItemID;
            else if (obj is StaticTarget)
                itemID = ((StaticTarget)obj).ItemID;
            else if (obj is StaticTile)
            {
                itemID = ((StaticTile)obj).ID;
            }


            return (itemID == 4017 || (itemID >= 6522 && itemID <= 6569));
        }

        /*
        private class InternalTarget : Target
        {
            private readonly BaseOre m_Ore;

            public InternalTarget(BaseOre ore)
                : base(2, false, TargetFlags.None)
            {
                m_Ore = ore;
            }

            private bool IsForge(object obj)
            {
                if (Core.ML && obj is Mobile && ((Mobile)obj).IsDeadBondedPet)
                    return false;

                if (obj.GetType().IsDefined(typeof(ForgeAttribute), false))
                    return true;

                int itemID = 0;

                if (obj is Item)
                    itemID = ((Item)obj).ItemID;
                else if (obj is StaticTarget)
                    itemID = ((StaticTarget)obj).ItemID;

                return (itemID == 4017 || (itemID >= 6522 && itemID <= 6569));
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Ore.Deleted)
                    return;

                if (!from.InRange(m_Ore.GetWorldLocation(), 2))
                {
                    from.SendLocalizedMessage(501976); // The ore is too far away.
                    return;
                }

                #region Combine Ore
                if (targeted is BaseOre)
                {
                    BaseOre ore = (BaseOre)targeted;

                    if (!ore.Movable)
                    {
                        return;
                    }
                    else if (m_Ore == ore)
                    {
                        from.SendLocalizedMessage(501972); // Select another pile or ore with which to combine
                        from.Target = new InternalTarget(ore);
                        return;
                    }
                    else if (ore.Resource != m_Ore.Resource)
                    {
                        from.SendLocalizedMessage(501979); // You cannot combine ores of different metals.
                        return;
                    }

                    int worth = ore.Amount;

                    if (ore.ItemID == 0x19B9)
                        worth *= 8;
                    else if (ore.ItemID == 0x19B7)
                        worth *= 2;
                    else
                        worth *= 4;

                    int sourceWorth = m_Ore.Amount;

                    if (m_Ore.ItemID == 0x19B9)
                        sourceWorth *= 8;
                    else if (m_Ore.ItemID == 0x19B7)
                        sourceWorth *= 2;
                    else
                        sourceWorth *= 4;

                    worth += sourceWorth;

                    int plusWeight = 0;
                    int newID = ore.ItemID;

                    if (ore.DefaultWeight != m_Ore.DefaultWeight)
                    {
                        if (ore.ItemID == 0x19B7 || m_Ore.ItemID == 0x19B7)
                        {
                            newID = 0x19B7;
                        }
                        else if (ore.ItemID == 0x19B9)
                        {
                            newID = m_Ore.ItemID;
                            plusWeight = ore.Amount * 2;
                        }
                        else
                        {
                            plusWeight = m_Ore.Amount * 2;
                        }
                    }

                    if ((ore.ItemID == 0x19B9 && worth > 120000) || ((ore.ItemID == 0x19B8 || ore.ItemID == 0x19BA) && worth > 60000) || (ore.ItemID == 0x19B7 && worth > 30000))
                    {
                        from.SendLocalizedMessage(1062844); // There is too much ore to combine.
                        return;
                    }
                    else if (ore.RootParent is Mobile && (plusWeight + ((Mobile)ore.RootParent).Backpack.TotalWeight) > ((Mobile)ore.RootParent).Backpack.MaxWeight)
                    {
                        from.SendLocalizedMessage(501978); // The weight is too great to combine in a container.
                        return;
                    }

                    ore.ItemID = newID;

                    if (ore.ItemID == 0x19B9)
                        ore.Amount = worth / 8;
                    else if (ore.ItemID == 0x19B7)
                        ore.Amount = worth / 2;
                    else
                        ore.Amount = worth / 4;

                    m_Ore.Delete();
                    return;
                }
                #endregion

                if (IsForge(targeted))
                {
                    double difficulty;

                    #region Void Pool Rewards
                    bool talisman = false;
                    SmeltersTalisman t = from.FindItemOnLayer(Layer.Talisman) as SmeltersTalisman;
                    if (t != null && t.Resource == m_Ore.Resource)
                        talisman = true;
                    #endregion

                    switch ( m_Ore.Resource )
                    {
                        default:
                            difficulty = 50.0;
                            break;
                        case CraftResource.Berilo:
                            difficulty = 65.0;
                            break;
                        case CraftResource.Vibranium:
                            difficulty = 70.0;
                            break;
                        case CraftResource.Cobre:
                            difficulty = 75.0;
                            break;
                        case CraftResource.Bronze:
                            difficulty = 80.0;
                            break;
                        case CraftResource.Prata:
                            difficulty = 85.0;
                            break;
                        case CraftResource.Niobio:
                            difficulty = 90.0;
                            break;
                        case CraftResource.Lazurita:
                            difficulty = 95.0;
                            break;
                        case CraftResource.Quartzo:
                            difficulty = 99.0;
                            break;
                    }

                    double minSkill = difficulty - 25.0;
                    double maxSkill = difficulty + 25.0;

                    if (difficulty > 50.0 && difficulty > from.Skills[SkillName.Mining].Value && !talisman)
                    {
                        from.SendMessage("Voce nao sabe fundir esse minerio estranho"); // You have no idea how to smelt this strange ore!
                        return;
                    }

                    if (m_Ore.ItemID == 0x19B7 && m_Ore.Amount < 2)
                    {
                        from.SendMessage("Preciso de mais ferro para fazer barras");// There is not enough metal-bearing ore in this pile to make an ingot.
                        return;
                    }

                    if (talisman || from.CheckTargetSkill(SkillName.Mining, targeted, minSkill, maxSkill))
                    {
                        int toConsume = m_Ore.Amount;

                        if (toConsume <= 0)
                        {
                            from.SendMessage("Preciso de mais ferro para fazer barras"); // There is not enough metal-bearing ore in this pile to make an ingot.
                        }
                        else
                        {
                            if (toConsume > 30000)
                                toConsume = 30000;

                            int ingotAmount;

                            if (m_Ore.ItemID == 0x19B7)
                            {
                                ingotAmount = toConsume / 2;

                                if (toConsume % 2 != 0)
                                    --toConsume;
                            }
                            else if (m_Ore.ItemID == 0x19B9)
                            {
                                ingotAmount = toConsume * 2;
                            }
                            else
                            {
                                ingotAmount = toConsume;
                            }

                            BaseIngot ingot = m_Ore.GetIngot();
                            ingot.Amount = ingotAmount;

                            if (m_Ore.HasSocket<Caddellite>())
                            {
                                ingot.AttachSocket(new Caddellite());
                            }

                            m_Ore.Consume(toConsume);
                            from.AddToBackpack(ingot);
                            //from.PlaySound( 0x57 );

                            if (talisman && t != null)
                            {
                                t.UsesRemaining--;
                                from.SendLocalizedMessage(1152620); // The magic of your talisman guides your hands as you purify the metal. Success is ensured!
                            }
                            else
                                from.SendMessage("Voce funde o minerio removendo as impurezas"); // You smelt the ore removing the impurities and put the metal in your backpack.
                        }
                    }
                    else
                    {
                        if (m_Ore.Amount < 2)
                        {
                            if (m_Ore.ItemID == 0x19B9)
                                m_Ore.ItemID = 0x19B8;
                            else
                                m_Ore.ItemID = 0x19B7;
                        }
                        else
                        {
                            m_Ore.Amount /= 2;
                        }

                        from.SendLocalizedMessage(501990); // You burn away the impurities but are left with less useable metal.
                    }
                }
            }
        }
        */
    }

    public class IronOre : BaseOre
    {
        [Constructable]
        public IronOre()
            : this(1)
        {
        }

        [Constructable]
        public IronOre(int amount)
            : base(CraftResource.Ferro, amount)
        {
            this.Name = "Minério de Ferro";
        }

        public IronOre(bool fixedSize)
            : this(1)
        {
            if (fixedSize)
                ItemID = 0x19B8;
        }

        public IronOre(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override BaseIngot GetIngot()
        {
            return new IronIngot();
        }
    }

    public class BeriloOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Berilo; } }

        [Constructable]
        public BeriloOre()
            : this(1)
        {
        }

        [Constructable]
        public BeriloOre(int amount)
            : base(CraftResource.Berilo, amount)
        {
        }

        public BeriloOre(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override BaseIngot GetIngot()
        {
            return new BeriloIngot();
        }
    }

    public class VibraniumOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Vibranium; } }

        [Constructable]
        public VibraniumOre()
            : this(1)
        {
        }

        [Constructable]
        public VibraniumOre(int amount)
            : base(CraftResource.Vibranium, amount)
        {
        }

        public VibraniumOre(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override BaseIngot GetIngot()
        {
            return new VibraniumIngot();
        }
    }

    public class AdamantiumOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Adamantium; } }

        [Constructable]
        public AdamantiumOre()
            : this(1)
        {
        }

        [Constructable]
        public AdamantiumOre(int amount)
            : base(CraftResource.Adamantium, amount)
        {
        }

        public AdamantiumOre(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override BaseIngot GetIngot()
        {
            return new AdamantiumIngot();
        }
    }

    public class CopperOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Cobre; } }

        [Constructable]
        public CopperOre()
            : this(1)
        {
        }

        [Constructable]
        public CopperOre(int amount)
            : base(CraftResource.Cobre, amount)
        {
        }

        public CopperOre(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override BaseIngot GetIngot()
        {
            return new CopperIngot();
        }
    }

    public class BronzeOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Bronze; } }

        [Constructable]
        public BronzeOre()
            : this(1)
        {
        }

        [Constructable]
        public BronzeOre(int amount)
            : base(CraftResource.Bronze, amount)
        {
        }

        public BronzeOre(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override BaseIngot GetIngot()
        {
            return new BronzeIngot();
        }
    }

    public class SilverOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Dourado; } }

        [Constructable]
        public SilverOre()
            : this(1)
        {
        }

        [Constructable]
        public SilverOre(int amount)
            : base(CraftResource.Dourado, amount)
        {
        }

        public SilverOre(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            if (version == 0 && Hue != 0x8A5)
            {
                Hue = 0x8A5;
                this.InvalidateProperties();
            }
        }

        public override BaseIngot GetIngot()
        {
            return new SilverIngot();
        }
    }

    public class NiobioOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Niobio; } }

        [Constructable]
        public NiobioOre()
            : this(1)
        {
        }

        [Constructable]
        public NiobioOre(int amount)
            : base(CraftResource.Niobio, amount)
        {
        }

        public NiobioOre(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override BaseIngot GetIngot()
        {
            return new NiobioIngot();
        }
    }

    public class LazuritaOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Lazurita; } }

        [Constructable]
        public LazuritaOre()
            : this(1)
        {
        }

        [Constructable]
        public LazuritaOre(int amount)
            : base(CraftResource.Lazurita, amount)
        {
        }

        public LazuritaOre(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override BaseIngot GetIngot()
        {
            return new LazuritaIngot();
        }
    }

    public class QuartzoOre : BaseOre
    {
        protected override CraftResource DefaultResource { get { return CraftResource.Quartzo; } }

        [Constructable]
        public QuartzoOre()
            : this(1)
        {
        }

        [Constructable]
        public QuartzoOre(int amount)
            : base(CraftResource.Quartzo, amount)
        {
        }

        public QuartzoOre(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override BaseIngot GetIngot()
        {
            return new QuartzoIngot();
        }
    }
}
