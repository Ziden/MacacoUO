#region References
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Server.Accounting;
using Server.ContextMenus;
using Server.Engines.BulkOrders;
using Server.Engines.Points;
using Server.Engines.Quests;
using Server.Engines.XmlSpawner2;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Services.Virtues;
using Server.Targeting;
#endregion

namespace Server.Mobiles
{
    public enum VendorShoeType
    {
        None,
        Shoes,
        Boots,
        Sandals,
        ThighBoots
    }

    public abstract class BaseVendor : BaseCreature, IVendor
    {
        public static bool UseVendorEconomy = false; //Core.AOS && !Siege.SiegeShard;
        public static int BuyItemChange = Config.Get("Vendors.BuyItemChange", 1000);
        public static int SellItemChange = Config.Get("Vendors.SellItemChange", 1000);
        public static int EconomyStockAmount = Config.Get("Vendors.EconomyStockAmount", 500);
        public static TimeSpan DelayRestock = TimeSpan.FromMinutes(Config.Get("Vendors.RestockDelay", 60));
        public static int MaxSell = Config.Get("Vendors.MaxSell", 500);

        public static void PegaRecompensa(Mobile from, BaseVendor vendor)
        {
            if (!BulkOrderSystem.CanClaimRewards(from, vendor.BODType))
            {
                vendor.SayTo(from, true, "Voce precisa pegar sua ultima recompensa para podermos continuar negocio"); // You must claim your last turn-in reward in order for us to continue doing business.
            }
            else
            {
                int pending = BulkOrderSystem.GetPendingRewardFor(from, vendor.BODType);

                if (pending > 0)
                {
                    from.SendGump(new RewardsGump(vendor, (PlayerMobile)from, vendor.BODType, pending));
                }
                else
                {
                    from.SendGump(new RewardsGump(vendor, (PlayerMobile)from, vendor.BODType));
                }
            }
        }

        public void Treinar(Mobile from)
        {
            from.SendMessage("Custo de treino: 500 moedas");
            List<SkillName> ensina = new List<SkillName>();
            if (CanTeach && from.Alive)
            {
                Skills ourSkills = Skills;
                Skills theirSkills = from.Skills;

                for (int i = 0; i < ourSkills.Length && i < theirSkills.Length; ++i)
                {
                    Skill skill = ourSkills[i];
                    Skill theirSkill = theirSkills[i];

                    if (skill != null && theirSkill != null && skill.Base >= 60.0 && CheckTeach(skill.SkillName, from))
                    {
                        int toTeach = skill.BaseFixedPoint / 3;

                        if (toTeach > 420)
                        {
                            toTeach = 420;
                        }
                        ensina.Add((SkillName)i);
                    }
                }
            }

            from.CloseGump(typeof(GumpOpcoes));

            from.SendGump(new GumpOpcoes("Custo: 500", (opt) =>
            {
                if (!Banker.Withdraw(from, 500))
                {
                    if (!from.Backpack.ConsumeTotal(typeof(Gold), 500))
                    {
                        from.SendMessage("Voce precisa de 500 moedas para isto");
                        return;
                    }
                }
                var selecionado = ensina[opt];
                this.Teach(selecionado, from, 500, true);
                //from.Skills[selecionado].Base = 50;

            }, 3708, 0, ensina.Select(e => e.ToString()).ToArray()));
        }

        public static void OfereceBulkOrder(Mobile from, BaseVendor vendor)
        {



            if (vendor is AnimalTrainer)
            {
                BodTamer.EntregaBodTamer(from as PlayerMobile, vendor);
                return;
            }

            EventSink.InvokeBODOffered(new BODOfferEventArgs(from, vendor));

            Shard.Debug("Oferecendo bod", from);

            if (!vendor.SupportsBulkOrders(from))
            {
                vendor.SayTo(from, true, "Voce precisa de mais habilidade na minha profissao para trabalhar para mim");
                return;
            }

            if (from is PlayerMobile)
            {
                if (BulkOrderSystem.NewSystemEnabled)
                {
                    if (BulkOrderSystem.CanGetBulkOrder(from, vendor.BODType) || from.AccessLevel > AccessLevel.VIP)
                    {
                        Item bulkOrder = BulkOrderSystem.CreateBulkOrder(from, vendor.BODType, true);
                        if (bulkOrder == null)
                        {
                            for (var i = 0; i < 6; i++)
                            {
                                bulkOrder = BulkOrderSystem.CreateBulkOrder(from, vendor.BODType, true);
                                if (bulkOrder != null)
                                    break;
                            }
                        }

                        if (bulkOrder != null && from != null)
                            bulkOrder.BoundTo = from.RawName;
                        else
                        {
                            vendor.SayTo(from, true, "Nao encontrei um trabalho para voce ...tente novamente...");
                        }


                        if (bulkOrder is LargeBOD)
                        {
                            from.SendGump(new LargeBODAcceptGump(from, (LargeBOD)bulkOrder));
                        }
                        else if (bulkOrder is SmallBOD)
                        {
                            from.SendGump(new SmallBODAcceptGump(from, (SmallBOD)bulkOrder));
                        }
                    }
                    else
                    {
                        TimeSpan ts = BulkOrderSystem.GetNextBulkOrder(vendor.BODType, (PlayerMobile)from);

                        int totalSeconds = (int)ts.TotalSeconds;
                        int totalHours = (totalSeconds + 3599) / 3600;
                        int totalMinutes = (totalSeconds + 59) / 60;

                        vendor.SayTo(from, "Terei trabalho em " + totalMinutes.ToString() + " minutos"); // An offer may be available in about ~1_minutes~ minutes.
                    }
                }
                else
                {
                    TimeSpan ts = vendor.GetNextBulkOrder(from);

                    int totalSeconds = (int)ts.TotalSeconds;
                    int totalHours = (totalSeconds + 3599) / 3600;
                    int totalMinutes = (totalSeconds + 59) / 60;

                    if (((Core.SE) ? totalMinutes == 0 : totalHours == 0))
                    {
                        from.SendLocalizedMessage(1049038); // You can get an order now.

                        if (Core.AOS)
                        {
                            Item bulkOrder = vendor.CreateBulkOrder(from, true);
                            bulkOrder.BoundTo = from.RawName;

                            if (bulkOrder is LargeBOD)
                            {
                                from.SendGump(new LargeBODAcceptGump(from, (LargeBOD)bulkOrder));
                            }
                            else if (bulkOrder is SmallBOD)
                            {
                                from.SendGump(new SmallBODAcceptGump(from, (SmallBOD)bulkOrder));
                            }
                        }
                    }
                    else
                    {
                        int oldSpeechHue = vendor.SpeechHue;
                        vendor.SpeechHue = 0x3B2;

                        if (Core.SE)
                        {
                            vendor.SayTo(from, 1072058, totalMinutes.ToString(), 0x3B2);
                            // An offer may be available in about ~1_minutes~ minutes.
                        }
                        else
                        {
                            vendor.SayTo(from, true, "Aguarde " + totalHours.ToString() + " horas"); // An offer may be available in about ~1_hours~ hours.
                        }

                        vendor.SpeechHue = oldSpeechHue;
                    }
                }
            }
        }

        public static List<BaseVendor> AllVendors { get; private set; }

        static BaseVendor()
        {
            AllVendors = new List<BaseVendor>(0x4000);
        }

        protected abstract List<SBInfo> SBInfos { get; }

        private readonly ArrayList m_ArmorBuyInfo = new ArrayList();
        private readonly ArrayList m_ArmorSellInfo = new ArrayList();

        private DateTime m_LastRestock;

        public override bool CanTeach { get { return true; } }

        public override bool BardImmune { get { return true; } }

        public override bool PlayerRangeSensitive { get { return true; } }

        public override bool UseSmartAI { get { return true; } }

        public virtual bool IsActiveVendor { get { return true; } }
        public virtual bool IsActiveBuyer { get { return IsActiveVendor && !Siege.SiegeShard; } } // response to vendor SELL
        public virtual bool IsActiveSeller { get { return IsActiveVendor; } } // repsonse to vendor BUY
        public virtual bool HasHonestyDiscount { get { return true; } }

        public virtual NpcGuild NpcGuild { get { return NpcGuild.None; } }

        public virtual bool ChangeRace { get { return true; } }

        public override bool IsInvulnerable { get { return true; } }

        public virtual DateTime NextTrickOrTreat { get; set; }
        public virtual double GetMoveDelay { get { return (double)Utility.RandomMinMax(30, 120); } }

        public override bool ShowFameTitle { get { return false; } }

        public virtual bool IsValidBulkOrder(Item item)
        {
            return false;
        }

        public virtual Item CreateBulkOrder(Mobile from, bool fromContextMenu)
        {
            return null;
        }

        public virtual bool SupportsBulkOrders(Mobile from)
        {
            return false;
        }

        public virtual TimeSpan GetNextBulkOrder(Mobile from)
        {
            return TimeSpan.Zero;
        }

        public virtual void OnSuccessfulBulkOrderReceive(Mobile from)
        { }

        public virtual BODType BODType { get { return BODType.Smith; } }

        #region Faction
        public virtual int GetPriceScalar()
        {
            Town town = Town.FromRegion(Region);

            if (town != null)
            {
                return (100 + town.Tax);
            }

            return 100;
        }

        public void UpdateBuyInfo()
        {
            int priceScalar = GetPriceScalar();

            var buyinfo = (IBuyItemInfo[])m_ArmorBuyInfo.ToArray(typeof(IBuyItemInfo));

            if (buyinfo != null)
            {
                foreach (IBuyItemInfo info in buyinfo)
                {
                    info.PriceScalar = priceScalar;
                }
            }
        }
        #endregion

        public class BulkOrderInfoEntry : ContextMenuEntry
        {
            private readonly Mobile m_From;
            private readonly BaseVendor m_Vendor;

            public BulkOrderInfoEntry(Mobile from, BaseVendor vendor)
                : base(6152, 3)
            {
                Enabled = vendor.CheckVendorAccess(from);

                m_From = from;
                m_Vendor = vendor;
            }

            public override void OnClick()
            {
                if (!m_From.InRange(m_Vendor.Location, 3))
                    return;

                OfereceBulkOrder(m_From, m_Vendor);

            }
        }

        public static void Suborna(Mobile m_From, BaseVendor m_Vendor)
        {
            if (!m_From.InRange(m_Vendor.Location, 2) || !(m_From is PlayerMobile))
                return;

            if (m_Vendor.SupportsBulkOrders(m_From) && m_From is PlayerMobile)
            {
                if (m_From.NetState != null && m_From.NetState.IsEnhancedClient)
                {
                    Timer.DelayCall(TimeSpan.FromMilliseconds(100), m_Vendor.TryBribe, m_From);
                }
                else
                {
                    m_Vendor.TryBribe(m_From);
                }
            }
            else
            {
                m_Vendor.SayTo(m_From, "Nao aceto propinas. Mas sei que normalmente os comerciantes de trabalho aceitam proprina para trocar ordens de trabalho...");
            }
        }

        private class BribeEntry : ContextMenuEntry
        {
            private Mobile m_From;
            private BaseVendor m_Vendor;

            public BribeEntry(Mobile from, BaseVendor vendor)
                : base(1152294, 2)
            {
                Enabled = vendor.CheckVendorAccess(from);

                m_From = from;
                m_Vendor = vendor;
            }

            public override void OnClick()
            {
                Suborna(m_From, m_Vendor);
            }
        }

        private class ClaimRewardsEntry : ContextMenuEntry
        {
            private readonly Mobile m_From;
            private readonly BaseVendor m_Vendor;

            public ClaimRewardsEntry(Mobile from, BaseVendor vendor)
                : base(1155593, 3)
            {
                Enabled = vendor.CheckVendorAccess(from);

                m_From = from;
                m_Vendor = vendor;
            }

            public override void OnClick()
            {
                if (!m_From.InRange(m_Vendor.Location, 3) || !(m_From is PlayerMobile))
                    return;

                PegaRecompensa(m_From, m_Vendor);
            }
        }

        public BaseVendor(string title)
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 5)
        {
            AllVendors.Add(this);

            LoadSBInfo();

            InitBody();
            InitOutfit();

            if (Female && title != null)
            {
                title = title.Substring(2);
                if (title.Substring(title.Length - 1) == "o")
                {
                    title = title.Substring(0, title.Length - 1) + "a";
                }
                title = " a " + title;
            }

            this.Title = title;

            Container pack;
            //these packs MUST exist, or the client will crash when the packets are sent
            pack = new Backpack();
            pack.Layer = Layer.ShopBuy;
            pack.Movable = false;
            pack.Visible = false;
            AddItem(pack);

            pack = new Backpack();
            pack.Layer = Layer.ShopResale;
            pack.Movable = false;
            pack.Visible = false;
            AddItem(pack);

            BribeMultiplier = Utility.Random(10);

            m_LastRestock = DateTime.UtcNow;

            Timer.DelayCall(TimeSpan.FromMilliseconds(500), b => SetStr(20), 500);
        }

        public BaseVendor(Serial serial)
            : base(serial)
        {
            AllVendors.Add(this);
        }

        public override void OnDelete()
        {
            base.OnDelete();

            AllVendors.Remove(this);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            AllVendors.Remove(this);
        }

        public DateTime LastRestock { get { return m_LastRestock; } set { m_LastRestock = value; } }

        public virtual TimeSpan RestockDelay { get { return DelayRestock; } }

        public Container BuyPack
        {
            get
            {
                Container pack = FindItemOnLayer(Layer.ShopBuy) as Container;

                if (pack == null)
                {
                    pack = new Backpack();
                    pack.Layer = Layer.ShopBuy;
                    pack.Visible = false;
                    AddItem(pack);
                }

                return pack;
            }
        }

        public abstract void InitSBInfo();

        public virtual bool IsTokunoVendor { get { return false; } }
        public virtual bool IsStygianVendor { get { return false; } }

        protected void LoadSBInfo()
        {
            m_LastRestock = DateTime.UtcNow;

            for (int i = 0; i < m_ArmorBuyInfo.Count; ++i)
            {
                GenericBuyInfo buy = m_ArmorBuyInfo[i] as GenericBuyInfo;

                if (buy != null)
                {
                    buy.DeleteDisplayEntity();
                }
            }

            SBInfos.Clear();

            InitSBInfo();

            m_ArmorBuyInfo.Clear();
            m_ArmorSellInfo.Clear();

            for (int i = 0; i < SBInfos.Count; i++)
            {
                SBInfo sbInfo = SBInfos[i];
                m_ArmorBuyInfo.AddRange(sbInfo.BuyInfo);
                m_ArmorSellInfo.Add(sbInfo.SellInfo);
            }
        }

        public virtual bool GetGender()
        {
            return Utility.RandomBool();
        }

        public virtual void InitBody()
        {
            InitStats(100, 100, 25);

            SpeechHue = Utility.RandomDyedHue();
            Hue = Utility.RandomSkinHue();
            Female = GetGender();

            if (Female)
            {
                Body = 0x191;
                Name = NameList.RandomName("female");
            }
            else
            {
                Body = 0x190;
                Name = NameList.RandomName("male");
            }
        }

        public virtual int GetRandomHue()
        {
            switch (Utility.Random(5))
            {
                default:
                case 0:
                    return Utility.RandomBlueHue();
                case 1:
                    return Utility.RandomGreenHue();
                case 2:
                    return Utility.RandomRedHue();
                case 3:
                    return Utility.RandomYellowHue();
                case 4:
                    return Utility.RandomNeutralHue();
            }
        }

        public virtual int GetShoeHue()
        {
            if (0.1 > Utility.RandomDouble())
            {
                return 0;
            }

            return Utility.RandomNeutralHue();
        }

        public virtual VendorShoeType ShoeType { get { return VendorShoeType.Shoes; } }

        public virtual void CheckMorph()
        {
            if (!ChangeRace)
                return;

            if (CheckGargoyle())
            {
                return;
            }
            #region SA
            else if (CheckTerMur())
            {
                return;
            }
            #endregion

            else if (CheckNecromancer())
            {
                return;
            }
            else if (CheckTokuno())
            {
                return;
            }
        }

        public virtual bool CheckTokuno()
        {
            if (Map != Map.Tokuno)
            {
                return false;
            }

            NameList n;

            if (Female)
            {
                n = NameList.GetNameList("tokuno female");
            }
            else
            {
                n = NameList.GetNameList("tokuno male");
            }

            if (!n.ContainsName(Name))
            {
                TurnToTokuno();
            }

            return true;
        }

        public virtual void TurnToTokuno()
        {
            if (Female)
            {
                Name = NameList.RandomName("tokuno female");
            }
            else
            {
                Name = NameList.RandomName("tokuno male");
            }
        }

        public virtual bool CheckGargoyle()
        {
            Map map = Map;

            if (map != Map.Ilshenar)
            {
                return false;
            }

            if (!Region.IsPartOf("Gargoyle City"))
            {
                return false;
            }

            if (Body != 0x2F6 || (Hue & 0x8000) == 0)
            {
                TurnToGargoyle();
            }

            return true;
        }

        #region SA Change
        public virtual bool CheckTerMur()
        {
            return false;
        }
        #endregion

        public virtual bool CheckNecromancer()
        {
            Map map = Map;

            if (map != Map.Malas)
            {
                return false;
            }

            if (!Region.IsPartOf("Umbra"))
            {
                return false;
            }

            if (Hue != 0x83E8)
            {
                TurnToNecromancer();
            }

            return true;
        }

        public override void OnAfterSpawn()
        {
            CheckMorph();
        }

        protected override void OnMapChange(Map oldMap)
        {
            base.OnMapChange(oldMap);

            CheckMorph();

            LoadSBInfo();
        }

        public virtual int GetRandomNecromancerHue()
        {
            switch (Utility.Random(20))
            {
                case 0:
                    return 0;
                case 1:
                    return 0x4E9;
                default:
                    return Utility.RandomList(0x485, 0x497);
            }
        }

        public virtual void TurnToNecromancer()
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                Item item = Items[i];

                if (item is Hair || item is Beard)
                {
                    item.Hue = 0;
                }
                else if (item is BaseClothing || item is BaseWeapon || item is BaseArmor || item is BaseTool)
                {
                    item.Hue = GetRandomNecromancerHue();
                }
            }

            HairHue = 0;
            FacialHairHue = 0;

            Hue = 0x83E8;
        }

        public virtual void TurnToGargoyle()
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                Item item = Items[i];

                if (item is BaseClothing || item is Hair || item is Beard)
                {
                    item.Delete();
                }
            }

            HairItemID = 0;
            FacialHairItemID = 0;

            Body = 0x2F6;
            Hue = Utility.RandomBrightHue() | 0x8000;
            Name = NameList.RandomName("gargoyle vendor");

            CapitalizeTitle();
        }

        #region SA
        public virtual void TurnToGargRace()
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                Item item = Items[i];

                if (item is BaseClothing)
                {
                    item.Delete();
                }
            }

            Race = Race.Gargoyle;

            Hue = Race.RandomSkinHue();

            HairItemID = Race.RandomHair(Female);
            HairHue = Race.RandomHairHue();

            FacialHairItemID = Race.RandomFacialHair(Female);
            if (FacialHairItemID != 0)
            {
                FacialHairHue = Race.RandomHairHue();
            }
            else
            {
                FacialHairHue = 0;
            }

            InitGargOutfit();

            if (Female = GetGender())
            {
                Body = 0x29B;
                Name = NameList.RandomName("gargoyle female");
            }
            else
            {
                Body = 0x29A;
                Name = NameList.RandomName("gargoyle male");
            }

            CapitalizeTitle();
        }
        #endregion

        public virtual void CapitalizeTitle()
        {
            string title = Title;

            if (title == null)
            {
                return;
            }

            var split = title.Split(' ');

            for (int i = 0; i < split.Length; ++i)
            {
                if (Insensitive.Equals(split[i], "the"))
                {
                    continue;
                }

                if (split[i].Length > 1)
                {
                    split[i] = Char.ToUpper(split[i][0]) + split[i].Substring(1);
                }
                else if (split[i].Length > 0)
                {
                    split[i] = Char.ToUpper(split[i][0]).ToString();
                }
            }

            Title = String.Join(" ", split);
        }

        public virtual int GetHairHue()
        {
            return Utility.RandomHairHue();
        }

        public virtual void InitOutfit()
        {
            switch (Utility.Random(3))
            {
                case 0:
                    SetWearable(new FancyShirt(GetRandomHue()));
                    break;
                case 1:
                    SetWearable(new Doublet(GetRandomHue()));
                    break;
                case 2:
                    SetWearable(new Shirt(GetRandomHue()));
                    break;
            }

            switch (ShoeType)
            {
                case VendorShoeType.Shoes:
                    SetWearable(new Shoes(GetShoeHue()));
                    break;
                case VendorShoeType.Boots:
                    SetWearable(new Boots(GetShoeHue()));
                    break;
                case VendorShoeType.Sandals:
                    SetWearable(new Sandals(GetShoeHue()));
                    break;
                case VendorShoeType.ThighBoots:
                    SetWearable(new ThighBoots(GetShoeHue()));
                    break;
            }

            int hairHue = GetHairHue();

            Utility.AssignRandomHair(this, hairHue);
            Utility.AssignRandomFacialHair(this, hairHue);

            if (Body == 0x191)
            {
                FacialHairItemID = 0;
            }

            if (Body == 0x191)
            {
                switch (Utility.Random(6))
                {
                    case 0:
                        SetWearable(new ShortPants(GetRandomHue()));
                        break;
                    case 1:
                    case 2:
                        SetWearable(new Kilt(GetRandomHue()));
                        break;
                    case 3:
                    case 4:
                    case 5:
                        SetWearable(new Skirt(GetRandomHue()));
                        break;
                }
            }
            else
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        SetWearable(new LongPants(GetRandomHue()));
                        break;
                    case 1:
                        SetWearable(new ShortPants(GetRandomHue()));
                        break;
                }
            }

            if (!Siege.SiegeShard)
                PackGold(100, 200);
        }

        #region SA
        public virtual void InitGargOutfit()
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                Item item = Items[i];

                if (item is BaseClothing)
                {
                    item.Delete();
                }
            }

            if (Female)
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        SetWearable(new FemaleGargishClothLegs(GetRandomHue()));
                        SetWearable(new FemaleGargishClothKilt(GetRandomHue()));
                        SetWearable(new FemaleGargishClothChest(GetRandomHue()));
                        break;
                    case 1:
                        SetWearable(new FemaleGargishClothKilt(GetRandomHue()));
                        SetWearable(new FemaleGargishClothChest(GetRandomHue()));
                        break;
                }
            }
            else
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        SetWearable(new MaleGargishClothLegs(GetRandomHue()));
                        SetWearable(new MaleGargishClothKilt(GetRandomHue()));
                        SetWearable(new MaleGargishClothChest(GetRandomHue()));
                        break;
                    case 1:
                        SetWearable(new MaleGargishClothKilt(GetRandomHue()));
                        SetWearable(new MaleGargishClothChest(GetRandomHue()));
                        break;
                }
            }

            if (!Siege.SiegeShard)
                PackGold(100, 200);
        }
        #endregion

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ForceRestock
        {
            get { return false; }
            set
            {
                if (value)
                {
                    Restock();
                    Say("Restocked!");
                }
            }
        }

        public virtual void Restock()
        {
            m_LastRestock = DateTime.UtcNow;

            var buyInfo = GetBuyInfo();

            foreach (IBuyItemInfo bii in buyInfo)
            {
                bii.OnRestock();
            }
        }

        private static readonly TimeSpan InventoryDecayTime = TimeSpan.FromHours(1.0);

        public virtual void VendorBuy(Mobile from)
        {
            if (!IsActiveSeller)
            {
                return;
            }

            if (!from.CheckAlive())
            {
                return;
            }

            if (!CheckVendorAccess(from))
            {
                Say(501522); // I shall not treat with scum like thee!
                return;
            }

            if (DateTime.UtcNow - m_LastRestock > RestockDelay)
            {
                Restock();
            }

            UpdateBuyInfo();

            int count = 0;
            List<BuyItemState> list;
            var buyInfo = GetBuyInfo();
            var sellInfo = GetSellInfo();

            list = new List<BuyItemState>(buyInfo.Length);
            Container cont = BuyPack;

            List<ObjectPropertyList> opls = null;

            for (int idx = 0; idx < buyInfo.Length; idx++)
            {
                IBuyItemInfo buyItem = buyInfo[idx];

                if (buyItem.Amount <= 0 || list.Count >= 250)
                {
                    continue;
                }

                // NOTE: Only GBI supported; if you use another implementation of IBuyItemInfo, this will crash
                GenericBuyInfo gbi = (GenericBuyInfo)buyItem;
                IEntity disp = gbi.GetDisplayEntity();

                if (Siege.SiegeShard && !Siege.VendorCanSell(gbi.Type))
                {
                    continue;
                }

                list.Add(
                    new BuyItemState(
                        buyItem.Name,
                        cont.Serial,
                        disp == null ? (Serial)0x7FC0FFEE : disp.Serial,
                        buyItem.Price,
                        buyItem.Amount,
                        buyItem.ItemID,
                        buyItem.Hue));
                count++;

                if (opls == null)
                {
                    opls = new List<ObjectPropertyList>();
                }

                if (disp is Item)
                {
                    opls.Add(((Item)disp).PropertyList);
                }
                else if (disp is Mobile)
                {
                    opls.Add(((Mobile)disp).PropertyList);
                }
            }

            var playerItems = cont.Items;

            for (int i = playerItems.Count - 1; i >= 0; --i)
            {
                if (i >= playerItems.Count)
                {
                    continue;
                }

                Item item = playerItems[i];

                if ((item.LastMoved + InventoryDecayTime) <= DateTime.UtcNow)
                {
                    item.Delete();
                }
            }

            for (int i = 0; i < playerItems.Count; ++i)
            {
                Item item = playerItems[i];

                if (Siege.SiegeShard && !Siege.VendorCanSell(item.GetType()))
                {
                    continue;
                }

                int price = 0;
                string name = null;

                foreach (IShopSellInfo ssi in sellInfo)
                {
                    if (ssi.IsSellable(item))
                    {
                        price = ssi.GetBuyPriceFor(item, this);
                        name = ssi.GetNameFor(item);
                        break;
                    }
                }

                if (name != null && list.Count < 250)
                {
                    list.Add(new BuyItemState(name, cont.Serial, item.Serial, price, item.Amount, item.ItemID, item.Hue));
                    count++;

                    if (opls == null)
                    {
                        opls = new List<ObjectPropertyList>();
                    }

                    opls.Add(item.PropertyList);
                }
            }

            if (list.Count > 0)
            {
                list.Sort(new BuyItemStateComparer());

                SendPacksTo(from);

                NetState ns = from.NetState;

                if (ns == null)
                {
                    return;
                }

                if (ns.ContainerGridLines)
                {
                    from.Send(new VendorBuyContent6017(list));
                }
                else
                {
                    from.Send(new VendorBuyContent(list));
                }

                from.Send(new VendorBuyList(this, list));

                if (ns.HighSeas)
                {
                    from.Send(new DisplayBuyListHS(this));
                }
                else
                {
                    from.Send(new DisplayBuyList(this));
                }

                from.Send(new MobileStatusExtended(from)); //make sure their gold amount is sent

                if (opls != null)
                {
                    for (int i = 0; i < opls.Count; ++i)
                    {
                        if (opls[i] != null)
                            from.Send(opls[i]);
                    }
                }

                SayTo(from, "Bem vindo, o que voce procura?", 0x3B2); // Greetings.  Have a look around.
            }
        }

        public virtual void SendPacksTo(Mobile from)
        {
            Item pack = FindItemOnLayer(Layer.ShopBuy);

            if (pack == null)
            {
                pack = new Backpack();
                pack.Layer = Layer.ShopBuy;
                pack.Movable = false;
                pack.Visible = false;
                SetWearable(pack);
            }

            from.Send(new EquipUpdate(pack));

            pack = FindItemOnLayer(Layer.ShopSell);

            if (pack != null)
            {
                from.Send(new EquipUpdate(pack));
            }

            pack = FindItemOnLayer(Layer.ShopResale);

            if (pack == null)
            {
                pack = new Backpack();
                pack.Layer = Layer.ShopResale;
                pack.Movable = false;
                pack.Visible = false;
                SetWearable(pack);
            }

            from.Send(new EquipUpdate(pack));
        }

        public virtual void VendorSell(Mobile from)
        {
            if (!IsActiveBuyer)
            {
                return;
            }

            if (!from.CheckAlive())
            {
                return;
            }

            if (!CheckVendorAccess(from))
            {
                Say("Não tratarei com escória como você!"); // I shall not treat with scum like thee!
                return;
            }

            Container pack = from.Backpack;

            if (pack != null)
            {
                var info = GetSellInfo();

                Dictionary<Item, SellItemState> table = new Dictionary<Item, SellItemState>();

                foreach (IShopSellInfo ssi in info)
                {
                    var items = pack.FindItemsByType(ssi.Types);

                    foreach (Item item in items)
                    {
                        if (item is Container && (item).Items.Count != 0)
                        {
                            continue;
                        }

                        if (item.IsStandardLoot() && item.Movable && ssi.IsSellable(item))
                        {
                            table[item] = new SellItemState(item, ssi.GetSellPriceFor(item, this), ssi.GetNameFor(item));
                        }
                    }
                }

                if (table.Count > 0)
                {
                    SendPacksTo(from);

                    from.Send(new VendorSellList(this, table.Values));
                }
                else
                {
                    Say(true, "Você não tem nada em que eu estaria interessado.");
                }
            }
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (ConvertsMageArmor && dropped is BaseArmor && CheckConvertArmor(from, (BaseArmor)dropped))
            {
                return false;
            }

            if (dropped is BodTamer)
            {
                var bod = dropped as BodTamer;

                if (bod.BoundTo != null && bod.BoundTo != from.RawName)
                {
                    return false;
                }

                if (bod.Quantidade > bod.QuantidadeAtual)
                {
                    from.SendMessage("Esta ordem nao esta completa...");
                    return false;
                }
                if (from.Skills.AnimalTaming.Value > 70 && bod.Skill + 20 < from.Skills.AnimalTaming.Value)
                {
                    from.SendMessage("Voce ja tem muito conhecimento para esta ordem");
                    return false;
                }
                bod.Delete();
                SayTo(from, true, "Obrigado pelo trabalho, irei cuidar bem dos animais");

                if (from.Skills[SkillName.AnimalTaming].Value < 65)
                {
                    SkillCheck.Gain(from, from.Skills[SkillName.AnimalTaming], 10);
                    from._PlaceInBackpack(new Gold(Utility.RandomMinMax(100, 300)));
                }
                else if (from.Skills[SkillName.AnimalTaming].Value < 70)
                {
                    SkillCheck.Gain(from, from.Skills[SkillName.AnimalTaming], 5);
                    from._PlaceInBackpack(new Gold(Utility.RandomMinMax(150, 350)));
                }
                else if (from.Skills[SkillName.AnimalTaming].Value < 80)
                {
                    SkillCheck.Gain(from, from.Skills[SkillName.AnimalTaming], 4);
                    from._PlaceInBackpack(new Gold(Utility.RandomMinMax(200, 400)));
                }
                else if (from.Skills[SkillName.AnimalTaming].Value < 90)
                {
                    SkillCheck.Gain(from, from.Skills[SkillName.AnimalTaming], 3);
                    from._PlaceInBackpack(new Gold(Utility.RandomMinMax(250, 450)));
                }
                else if (from.Skills[SkillName.AnimalTaming].Value < 95)
                {
                    SkillCheck.Gain(from, from.Skills[SkillName.AnimalTaming], 2);
                    from._PlaceInBackpack(new Gold(Utility.RandomMinMax(300, 500)));
                }
                else
                {
                    SkillCheck.Gain(from, from.Skills[SkillName.AnimalTaming], 1);
                    from._PlaceInBackpack(new Gold(Utility.RandomMinMax(350, 550)));
                }
                from.PlaySound(0x5B5);
                return false;
            }

            if (dropped is SmallBOD || dropped is LargeBOD)
            {
                PlayerMobile pm = from as PlayerMobile;
                IBOD bod = dropped as IBOD;

                if (bod != null && BulkOrderSystem.NewSystemEnabled && Bribes != null && Bribes.ContainsKey(from) && Bribes[from].BOD == bod)
                {
                    if (BulkOrderSystem.CanExchangeBOD(from, this, bod, Bribes[from].Amount))
                    {
                        DoBribe(from, bod);
                        return false;
                    }
                }

                if (pm != null && pm.NextBODTurnInTime > DateTime.UtcNow)
                {
                    SayTo(from, "Você terá que esperar alguns segundos enquanto eu inspeciono o último pedido.", 0x3B2); // You'll have to wait a few seconds while I inspect the last order.
                    return false;
                }
                else if (!IsValidBulkOrder(dropped) || !SupportsBulkOrders(from))
                {
                    SayTo(from, "Essa ordem é para algum outro lojista.", 0x3B2); // That order is for some other shopkeeper.
                    return false;
                }
                else if ((dropped is SmallBOD && !((SmallBOD)dropped).Complete) ||
                         (dropped is LargeBOD && !((LargeBOD)dropped).Complete))
                {
                    SayTo(from, "Voce ainda nao completou esta ordem", 0x3B2); // You have not completed the order yet.
                    return false;
                }

                Item reward;
                int gold, fame;

                if (dropped is SmallBOD)
                {
                    ((SmallBOD)dropped).GetRewards(out reward, out gold, out fame);
                }
                else
                {
                    ((LargeBOD)dropped).GetRewards(out reward, out gold, out fame);
                }

                Shard.Debug("Gold Final: " + gold);

                var pontos = gold / 3;
                PointsSystem.PontosTrabalho.AwardPoints(from, pontos, false);

                from.SendSound(0x3D);

                if (BulkOrderSystem.NewSystemEnabled && from is PlayerMobile)
                {
                    SayTo(from, "Hoho obrigado !", 0x3B2); // Ho! Ho! Thank ye ~1_PLAYER~ for giving me a Bulk Order Deed!
                    var type = ((IBOD)dropped).BODType;
                    var skill = BulkOrderSystem.GetSkillForBOD(type);

                    from.SendMessage(78, "Diga 'Recompensa' para o NPC para ver os items que voce pode pegar de recompensa por trabalhar !");


                    if (dropped.BoundTo == from.RawName || from.AccessLevel > AccessLevel.VIP)
                    {
                        ushort exp = 1000;
                        if (from.Skills[skill].Value < 60)
                            exp += 43000;
                        else if (from.Skills[skill].Value < 70)
                            exp += 13000;
                        if (from.Skills[skill].Value < 80)
                            exp += 9000;
                        else if (from.Skills[skill].Value < 90)
                            exp += 5000;
                        else if (from.Skills[skill].Value < 100)
                            exp += 3000;
                        else if (from.Skills[skill].Value < 110)
                            exp += 1000;
                        /*
                        else if (from.Skills[skill].Value > 100)
                            exp = (ushort)(exp * 0.7);
                        else if (from.Skills[skill].Value > 105)
                            exp = (ushort)(exp * 0.6);
                        else if (from.Skills[skill].Value > 110)
                            exp = (ushort)(exp * 0.5);
                        else if (from.Skills[skill].Value > 115)
                            exp = (ushort)(exp * 0.3);
                            */

                        if (dropped is LargeBOD)
                        {
                            var large = (LargeBOD)dropped;
                            var matBonus = (int)large.Material;

                            if (large.Material >= BulkMaterialType.Spined && large.Material <= BulkMaterialType.Horned)
                                matBonus -= 8;

                            if (large.Material >= BulkMaterialType.Carvalho && large.Material <= BulkMaterialType.Gelo)
                                matBonus -= 11;

                            exp *= 6;

                            exp += (ushort)(matBonus * 2500);
                            if (large.RequireExceptional)
                                exp += 1000;

                        }
                        else if (dropped is SmallBOD)
                        {
                            var small = (SmallBOD)dropped;
                            var matBonus = (int)small.Material;

                            if (small.Material >= BulkMaterialType.Spined && small.Material <= BulkMaterialType.Horned)
                                matBonus -= 8;

                            if (small.Material >= BulkMaterialType.Carvalho && small.Material <= BulkMaterialType.Gelo)
                                matBonus -= 11;

                            exp += (ushort)(matBonus * 2500);
                            if (small.RequireExceptional)
                                exp += 1000;
                        }

                        var ups = (int)Math.Floor(exp / 1000d);
                        ushort resto = (ushort)(exp % 1000);

                        if (ups > 0)
                        {
                            SkillCheck.Gain(from, from.Skills[skill], ups);
                        }

                        if(resto > 0)
                        {
                            if (from.Skills[skill].IncreaseExp(resto))
                            {
                                SkillCheck.Gain(from, from.Skills[skill]);
                            }
                        }
                    }

                    else
                    {
                        from.SendMessage("Voce nao ganhou conhecimento em " + skill.ToString() + " pois esta ordem de trabalho nao era sua");
                    }

                    BODContext context = BulkOrderSystem.GetContext(from);

                    int points = 0;
                    double banked = 0.0;

                    if (dropped is SmallBOD)
                        BulkOrderSystem.ComputePoints((SmallBOD)dropped, out points, out banked);
                    else
                        BulkOrderSystem.ComputePoints((LargeBOD)dropped, out points, out banked);

                    switch (context.PointsMode)
                    {
                        case PointsMode.Enabled:
                            from.SendGump(new ConfirmBankPointsGump((PlayerMobile)from, this, (Item)bod, this.BODType, points, banked));
                            break;
                        case PointsMode.Disabled:
                            from.SendGump(new RewardsGump(this, (PlayerMobile)from, this.BODType, points));
                            break;
                        case PointsMode.Automatic:
                            {
                                BulkOrderSystem.SetPoints(from, this.BODType, banked);
                                from.SendGump(new RewardsGump(this, (PlayerMobile)from, this.BODType));
                            }
                            break;
                    }

                    // On EA, you have to choose the reward before you get the gold/fame reward.  IF you right click the gump, you lose 
                    // the gold/fame for that bod.

                    Banker.Deposit(from, gold, true);
                }
                else
                {
                    SayTo(from, "Obrigado, aqui esta uma recompensa pelo seu esforco", 0x3B2); // Thank you so much!  Here is a reward for your effort.

                    if (reward != null)
                    {
                        from.AddToBackpack(reward);
                    }

                    Banker.Deposit(from, gold, true);
                }

                Titles.AwardFame(from, fame, true);

                from.PlaySound(0x5B5);

                OnSuccessfulBulkOrderReceive(from);
                Server.Engines.CityLoyalty.CityLoyaltySystem.OnBODTurnIn(from, gold);

                if (pm != null)
                {
                    pm.NextBODTurnInTime = DateTime.UtcNow + TimeSpan.FromSeconds(2.0);
                }

                dropped.Delete();
                return true;
            }
            else if (AcceptsGift(from, dropped))
            {
                dropped.Delete();
            }

            return base.OnDragDrop(from, dropped);
        }

        public bool AcceptsGift(Mobile from, Item dropped)
        {
            string name;

            if (dropped.Name != null)
            {
                if (dropped.Amount > 0)
                {
                    name = String.Format("{0} {1}", dropped.Amount, dropped.Name);
                }
                else
                {
                    name = dropped.Name;
                }
            }
            else
            {
                name = Server.Engines.VendorSearching.VendorSearch.GetItemName(dropped);
            }

            if (!String.IsNullOrEmpty(name))
            {
                this.SayTo(from, String.Format("Voce esta me dando {0}.", name));
                //PrivateOverheadMessage(MessageType.Regular, 0x3B2, true, String.Format("Voce esta me dando {0}.", name), from.NetState);
            }
            else
            {
                this.SayTo(from, String.Format("Voce esta me dando isso ?", dropped.LabelNumber.ToString()), 0x3B2); // Thou art giving me ~1_VAL~?
            }

            if (from is PlayerMobile && QuestHelper.CheckItem((PlayerMobile)from, dropped, this))
            {
                this.SayTo(from, "!");
                return false;
            }

            if (dropped is Gold)
            {
                this.SayTo(from, this.Female ? "Obrigada" : "Obrigado", 0x3B2); // I thank thee.
                Titles.AwardFame(from, dropped.Amount / 100, true);

                return true;
            }

            var info = GetSellInfo();

            /*
            foreach (IShopSellInfo ssi in info)
            {
                if (ssi.IsSellable(dropped))
                {
                    this.SayTo(from, this.Female ? "Obrigada" : "Obrigado", 0x3B2); // I thank thee.
                    Titles.AwardFame(from, ssi.GetSellPriceFor(dropped, this) * dropped.Amount, true);

                    return true;
                }
            }
            */

            this.SayTo(from, "Nao tenho interesse nisto", 0x3B2); // I am not interested in this.

            return false;
        }

        #region BOD Bribing
        [CommandProperty(AccessLevel.GameMaster)]
        public int BribeMultiplier { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextMultiplierDecay { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime WatchEnds { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RecentBribes { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UnderWatch { get { return WatchEnds > DateTime.MinValue; } }

        public Dictionary<Mobile, PendingBribe> Bribes { get; set; }

        private void CheckNextMultiplierDecay(bool force = true)
        {
            int minDays = Config.Get("Vendors.BribeDecayMinTime", 25);
            int maxDays = Config.Get("Vendors.BribeDecayMaxTime", 30);

            if (force || (NextMultiplierDecay > DateTime.UtcNow + TimeSpan.FromDays(maxDays)))
                NextMultiplierDecay = DateTime.UtcNow + TimeSpan.FromDays(Utility.RandomMinMax(minDays, maxDays));
        }

        public void TryBribe(Mobile m)
        {
            if (UnderWatch)
            {
                if (WatchEnds < DateTime.UtcNow)
                {
                    WatchEnds = DateTime.MinValue;
                    RecentBribes = 0;
                }
                else
                {
                    SayTo(m, "Cara minha guilda ta de olho em mim. Nao posso fazer isso agora.", 0x3B2); // My business is being watched by the Guild, so I can't be messing with bulk orders right now. Come back when there's less heat on me!
                    return;
                }
            }

            SayTo(m, "Entao, voce quer fazer negocios por baixo dos panos ?", 0x3B2); // So you want to do a little business under the table?
            m.SendMessage("Selecione uma Order de Compra para o comerciante ver se consegue trocar"); // Target a bulk order deed to show to the shopkeeper.

            m.BeginTarget(-1, false, Server.Targeting.TargetFlags.None, (from, targeted) =>
            {
                IBOD bod = targeted as IBOD;

                if (bod is Item && ((Item)bod).IsChildOf(from.Backpack))
                {
                    if (BulkOrderSystem.CanExchangeBOD(from, this, bod, -1))
                    {
                        int amount = BulkOrderSystem.GetBribe(bod);
                        amount *= BribeMultiplier * 10;

                        if (Bribes == null)
                            Bribes = new Dictionary<Mobile, PendingBribe>();

                // Per EA, new bribe replaced old pending bribe
                if (!Bribes.ContainsKey(m))
                        {
                            Bribes[m] = new PendingBribe(bod, amount);
                        }
                        else
                        {
                            Bribes[m].BOD = bod;
                            Bribes[m].Amount = amount;
                        }

                        SayTo(from, "Me ajuda q eu te ajudo. Eu troco sua ordem por uma melhor, por " + amount, 0x3B2);
                // If you help me out, I'll help you out. I can replace that bulk order with a better one, but it's gonna cost you ~1_amt~ gold coin. Payment is due immediately. Just hand me the order and I'll pull the old switcheroo.
            }
                }
                else if (bod == null)
                {
                    SayTo(from, "Isto nao eh uma ordem de compra", 0x3B2); // That is not a bulk order deed.
        }
            });
        }

        public void DoBribe(Mobile m, IBOD bod)
        {
            BulkOrderSystem.MutateBOD(bod);

            RecentBribes++;

            if (RecentBribes >= 2 && Utility.Random(6) < RecentBribes)
            {
                WatchEnds = DateTime.UtcNow + TimeSpan.FromMinutes(Utility.RandomMinMax(120, 180));
            }

            SayTo(m, "Foi um prazer nieh heh heh", 0x3B2); // You'll find this one much more to your liking. It's been a pleasure, and I look forward to you greasing my palm again very soon.

            if (Bribes.ContainsKey(m))
            {
                Bribes.Remove(m);
            }

            BribeMultiplier++;
            CheckNextMultiplierDecay();
        }

        #endregion

        private GenericBuyInfo LookupDisplayObject(object obj)
        {
            var buyInfo = GetBuyInfo();

            for (int i = 0; i < buyInfo.Length; ++i)
            {
                GenericBuyInfo gbi = (GenericBuyInfo)buyInfo[i];

                if (gbi.GetDisplayEntity() == obj)
                {
                    return gbi;
                }
            }

            return null;
        }

        private void ProcessSinglePurchase(
            BuyItemResponse buy,
            IBuyItemInfo bii,
            List<BuyItemResponse> validBuy,
            ref int controlSlots,
            ref bool fullPurchase,
            ref double cost)
        {
            int amount = buy.Amount;

            if (amount > bii.Amount)
            {
                amount = bii.Amount;
            }

            if (amount <= 0)
            {
                return;
            }

            int slots = bii.ControlSlots * amount;

            if (controlSlots >= slots)
            {
                controlSlots -= slots;
            }
            else
            {
                fullPurchase = false;
                return;
            }

            cost = (double)bii.Price * amount;
            validBuy.Add(buy);
        }

        private void ProcessValidPurchase(int amount, IBuyItemInfo bii, Mobile buyer, Container cont)
        {
            if (amount > bii.Amount)
            {
                amount = bii.Amount;
            }

            if (amount < 1)
            {
                return;
            }

            bii.Amount -= amount;

            IEntity o = bii.GetEntity();

            if (o is Item)
            {
                Item item = (Item)o;

                if (item is BaseWeapon)
                {
                    ((BaseWeapon)item).Quality = ItemQuality.Low;
                    if (!buyer.IsCooldown("msgq"))
                    {
                        buyer.SetCooldown("msgq", TimeSpan.FromMinutes(10));
                        buyer.SendMessage(78, "[DICA] Items comprados de NPC tem qualidade muito inferior");
                    }
                }
                else if (item is BaseArmor)
                {
                    ((BaseArmor)item).Quality = ItemQuality.Low;
                    if (!buyer.IsCooldown("msgq"))
                    {
                        buyer.SetCooldown("msgq", TimeSpan.FromMinutes(10));
                        buyer.SendMessage(78, "[DICA] Items comprados de NPC tem qualidade muito inferior");
                    }
                }
                else if (item is BaseTool)
                {
                    ((BaseTool)item).Quality = ItemQuality.Low;
                    if (!buyer.IsCooldown("msgq"))
                    {
                        buyer.SetCooldown("msgq", TimeSpan.FromMinutes(10));
                        buyer.SendMessage(78, "[DICA] Items comprados de NPC tem qualidade muito inferior");
                    }
                }
                else if (item is BaseInstrument)
                {
                    ((BaseInstrument)item).Quality = ItemQuality.Low;
                    if (!buyer.IsCooldown("msgq"))
                    {
                        buyer.SetCooldown("msgq", TimeSpan.FromMinutes(10));
                        buyer.SendMessage(78, "[DICA] Items comprados de NPC tem qualidade muito inferior");
                    }
                }

                //if (buyer.RP)
                //    item.RP = true;

                bii.OnBought(this, amount);

                if (item.Stackable)
                {
                    item.Amount = amount;

                    if (cont == null || !cont.TryDropItem(buyer, item, false))
                    {
                        item.MoveToWorld(buyer.Location, buyer.Map);
                    }
                }
                else
                {
                    item.Amount = 1;

                    if (cont == null || !cont.TryDropItem(buyer, item, false))
                    {
                        item.MoveToWorld(buyer.Location, buyer.Map);
                    }

                    for (int i = 1; i < amount; i++)
                    {
                        item = bii.GetEntity() as Item;

                        if (item != null)
                        {
                            item.Amount = 1;

                            if (cont == null || !cont.TryDropItem(buyer, item, false))
                            {
                                item.MoveToWorld(buyer.Location, buyer.Map);
                            }
                        }
                    }
                }
            }
            else if (o is Mobile)
            {
                Mobile m = (Mobile)o;

                bii.OnBought(this, amount);

                m.Direction = (Direction)Utility.Random(8);
                m.MoveToWorld(buyer.Location, buyer.Map);
                m.PlaySound(m.GetIdleSound());

                if (m is BaseCreature)
                {
                    ((BaseCreature)m).SetControlMaster(buyer);
                }

                for (int i = 1; i < amount; ++i)
                {
                    m = bii.GetEntity() as Mobile;

                    if (m != null)
                    {
                        m.Direction = (Direction)Utility.Random(8);
                        m.MoveToWorld(buyer.Location, buyer.Map);

                        if (m is BaseCreature)
                        {
                            ((BaseCreature)m).SetControlMaster(buyer);
                        }
                    }
                }
            }
        }

        public virtual bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
        {
            if (!IsActiveSeller)
            {
                return false;
            }

            if (!buyer.CheckAlive())
            {
                return false;
            }

            if (!CheckVendorAccess(buyer))
            {
                Say("Não tratarei com escória como você!"); // I shall not treat with scum like thee!
                return false;
            }

            UpdateBuyInfo();

            //var buyInfo = GetBuyInfo();
            var info = GetSellInfo();
            var totalCost = 0.0;
            var validBuy = new List<BuyItemResponse>(list.Count);
            Container cont;
            bool bought = false;
            bool fromBank = false;
            bool fullPurchase = true;
            int controlSlots = buyer.FollowersMax - buyer.Followers;

            foreach (BuyItemResponse buy in list)
            {
                Serial ser = buy.Serial;
                int amount = buy.Amount;
                double cost = 0;

                if (ser.IsItem)
                {
                    Item item = World.FindItem(ser);

                    if (item == null)
                    {
                        continue;
                    }

                    GenericBuyInfo gbi = LookupDisplayObject(item);

                    if (gbi != null)
                    {
                        ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref cost);
                    }
                    else if (item != BuyPack && item.IsChildOf(BuyPack))
                    {
                        if (amount > item.Amount)
                        {
                            amount = item.Amount;
                        }

                        if (amount <= 0)
                        {
                            continue;
                        }

                        foreach (IShopSellInfo ssi in info)
                        {
                            if (ssi.IsSellable(item))
                            {
                                if (ssi.IsResellable(item))
                                {
                                    cost = (double)ssi.GetBuyPriceFor(item, this) * amount;
                                    validBuy.Add(buy);
                                    break;
                                }
                            }
                        }
                    }

                    if (validBuy.Contains(buy))
                    {
                        if (ValidateBought(buyer, item))
                        {
                            totalCost += cost;
                        }
                        else
                        {
                            validBuy.Remove(buy);
                        }
                    }
                }
                else if (ser.IsMobile)
                {
                    Mobile mob = World.FindMobile(ser);

                    if (mob == null)
                    {
                        continue;
                    }

                    GenericBuyInfo gbi = LookupDisplayObject(mob);

                    if (gbi != null)
                    {
                        ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref cost);
                    }

                    if (validBuy.Contains(buy))
                    {
                        if (ValidateBought(buyer, mob))
                        {
                            totalCost += cost;
                        }
                        else
                        {
                            validBuy.Remove(buy);
                        }
                    }
                }
            } //foreach

            if (fullPurchase && validBuy.Count == 0)
            {
                SayTo(buyer, "Voce nao comprou nada", 0x3B2); // Thou hast bought nothing!
            }
            else if (validBuy.Count == 0)
            {
                SayTo(buyer, "Sua ordem nao pode ser completada. Tente novamente.", 0x3B2); // Your order cannot be fulfilled, please try again.
            }

            if (validBuy.Count == 0)
            {
                return false;
            }

            bought = buyer.AccessLevel >= AccessLevel.GameMaster;
            cont = buyer.Backpack;

            var discount = 0.0;

            if (Core.SA && HasHonestyDiscount)
            {
                double discountPc = 0;
                switch (VirtueHelper.GetLevel(buyer, VirtueName.Honesty))
                {
                    case VirtueLevel.Seeker:
                        discountPc = .1;
                        break;
                    case VirtueLevel.Follower:
                        discountPc = .2;
                        break;
                    case VirtueLevel.Knight:
                        discountPc = .3; break;
                    default:
                        discountPc = 0;
                        break;
                }

                discount = totalCost - (totalCost * (1.0 - discountPc));
                totalCost -= discount;
            }

            if (!bought && cont != null && ConsumeGold(cont, totalCost))
            {
                bought = true;
            }

            if (!bought)
            {
                if (totalCost <= Int32.MaxValue)
                {
                    if (Banker.Withdraw(buyer, (int)totalCost))
                    {
                        bought = true;
                        fromBank = true;
                    }
                }
                else if (buyer.Account != null && AccountGold.Enabled)
                {
                    if (buyer.Account.WithdrawCurrency(totalCost / AccountGold.CurrencyThreshold))
                    {
                        bought = true;
                        fromBank = true;
                    }
                }
            }

            if (!bought)
            {
                cont = buyer.FindBankNoCreate();

                if (cont != null && ConsumeGold(cont, totalCost))
                {
                    bought = true;
                    fromBank = true;
                }
            }

            if (!bought)
            {
                // ? Begging thy pardon, but thy bank account lacks these funds. 
                // : Begging thy pardon, but thou casnt afford that.
                SayTo(buyer, "Voce nao tem dinheiro suficiente", 0x3B2);

                return false;
            }

            buyer.PlaySound(0x32);

            cont = buyer.Backpack ?? buyer.BankBox;

            foreach (BuyItemResponse buy in validBuy)
            {
                Serial ser = buy.Serial;
                int amount = buy.Amount;

                if (amount < 1)
                {
                    continue;
                }

                if (ser.IsItem)
                {
                    Item item = World.FindItem(ser);

                    if (item == null)
                    {
                        continue;
                    }

                    GenericBuyInfo gbi = LookupDisplayObject(item);

                    if (gbi != null)
                    {
                        ProcessValidPurchase(amount, gbi, buyer, cont);
                    }
                    else
                    {
                        if (amount > item.Amount)
                        {
                            amount = item.Amount;
                        }

                        foreach (IShopSellInfo ssi in info)
                        {
                            if (ssi.IsSellable(item))
                            {
                                if (ssi.IsResellable(item))
                                {
                                    Item buyItem;

                                    if (amount >= item.Amount)
                                    {
                                        buyItem = item;
                                    }
                                    else
                                    {
                                        buyItem = LiftItemDupe(item, item.Amount - amount);

                                        if (buyItem == null)
                                        {
                                            buyItem = item;
                                        }
                                    }

                                    if (cont == null || !cont.TryDropItem(buyer, buyItem, false))
                                    {
                                        //if (buyer.RP)
                                        //    buyItem.RP = true;

                                        buyItem.MoveToWorld(buyer.Location, buyer.Map);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                else if (ser.IsMobile)
                {
                    Mobile mob = World.FindMobile(ser);

                    if (mob == null)
                    {
                        continue;
                    }

                    GenericBuyInfo gbi = LookupDisplayObject(mob);

                    if (gbi != null)
                    {
                        ProcessValidPurchase(amount, gbi, buyer, cont);
                    }
                }
            } //foreach

            if (discount > 0)
            {
                SayTo(buyer, 1151517, discount.ToString(), 0x3B2);
            }

            if (fullPurchase)
            {
                if (buyer.AccessLevel >= AccessLevel.GameMaster)
                {
                    SayTo(
                        buyer,
                        0x3B2,
                        "Pra voce eh de graca patrao, staff eh staff, ne.",
                        null,
                        !Core.AOS);
                }
                else if (fromBank)
                {
                    SayTo(
                        buyer,
                        0x3B2,
                        "Total de compra foi de {0} gold, Obrigado !",
                        totalCost.ToString(),
                        !Core.AOS);
                }
                else
                {
                    SayTo(buyer, String.Format("Total da compra foi de {0} gold. Obrigado pela preferencia.", totalCost), 0x3B2, true);
                }
            }
            else
            {
                if (buyer.AccessLevel >= AccessLevel.GameMaster)
                {
                    SayTo(
                        buyer,
                        0x3B2,
                        "Pra vc eu vendo ate meu corpo, staff.",
                        null,
                        !Core.AOS);
                }
                else if (fromBank)
                {
                    SayTo(
                        buyer,
                        0x3B2,
                        "Sua compra deu {0}gp. Obrigado.",
                        totalCost.ToString(),
                        !Core.AOS);
                }
                else
                {
                    SayTo(
                        buyer,
                        0x3B2,
                        "Minha compra deu {0}gp. Muito obrigado.",
                        totalCost.ToString(),
                        !Core.AOS);
                }
            }

            return true;
        }

        public virtual bool ValidateBought(Mobile buyer, Item item)
        {
            return true;
        }

        public virtual bool ValidateBought(Mobile buyer, Mobile m)
        {
            return true;
        }

        public static bool ConsumeGold(Container cont, double amount)
        {
            return ConsumeGold(cont, amount, true);
        }

        public static bool ConsumeGold(Container cont, double amount, bool recurse)
        {
            var gold = new Queue<Gold>(FindGold(cont, recurse));
            var total = gold.Aggregate(0.0, (c, g) => c + g.Amount);

            if (total < amount)
            {
                gold.Clear();

                return false;
            }

            var consume = amount;

            while (consume > 0)
            {
                var g = gold.Dequeue();

                if (g.Amount > consume)
                {
                    g.Consume((int)consume);

                    consume = 0;
                }
                else
                {
                    consume -= g.Amount;

                    g.Delete();
                }
            }

            gold.Clear();

            return true;
        }

        private static IEnumerable<Gold> FindGold(Container cont, bool recurse)
        {
            if (cont == null || cont.Items.Count == 0)
            {
                yield break;
            }

            if (cont is ILockable && ((ILockable)cont).Locked)
            {
                yield break;
            }

            if (cont is TrapableContainer && ((TrapableContainer)cont).TrapType != TrapType.None)
            {
                yield break;
            }

            var count = cont.Items.Count;

            while (--count >= 0)
            {
                if (count >= cont.Items.Count)
                {
                    continue;
                }

                var item = cont.Items[count];

                if (item is Container)
                {
                    if (!recurse)
                    {
                        continue;
                    }

                    foreach (var gold in FindGold((Container)item, true))
                    {
                        yield return gold;
                    }
                }
                else if (item is Gold)
                {
                    yield return (Gold)item;
                }
            }
        }

        public virtual bool CheckVendorAccess(Mobile from)
        {
            GuardedRegion reg = (GuardedRegion)Region.GetRegion(typeof(GuardedRegion));

            if (reg != null && !reg.CheckVendorAccess(this, from))
            {
                return false;
            }

            if (Region != from.Region)
            {
                reg = (GuardedRegion)from.Region.GetRegion(typeof(GuardedRegion));

                if (reg != null && !reg.CheckVendorAccess(this, from))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual bool OnSellItems(Mobile seller, List<SellItemResponse> list)
        {
            if (!IsActiveBuyer)
            {
                return false;
            }

            if (!seller.CheckAlive())
            {
                return false;
            }

            if (!CheckVendorAccess(seller))
            {
                Say("Saia daqui, escoria !"); // I shall not treat with scum like thee!
                return false;
            }

            seller.PlaySound(0x32);

            var info = GetSellInfo();
            var buyInfo = GetBuyInfo();
            int GiveGold = 0;
            int Sold = 0;
            Container cont;

            foreach (SellItemResponse resp in list)
            {
                if (resp.Item.RootParent != seller || resp.Amount <= 0 || !resp.Item.IsStandardLoot() || !resp.Item.Movable ||
                    (resp.Item is Container && (resp.Item).Items.Count != 0))
                {
                    continue;
                }

                foreach (IShopSellInfo ssi in info)
                {
                    if (ssi.IsSellable(resp.Item))
                    {
                        Sold += resp.Amount;
                        break;
                    }
                }
            }

            Shard.Debug("VENDI " + Sold + " MAX " + MaxSell);
            if (Sold > MaxSell)
            {
                SayTo(seller, "So posso comprar {0} item por vez!", MaxSell, 0x3B2, true);
                return false;
            }
            else if (Sold == 0)
            {
                return true;
            }

            foreach (SellItemResponse resp in list)
            {
                if (resp.Item.RootParent != seller || resp.Amount <= 0 || !resp.Item.IsStandardLoot() || !resp.Item.Movable ||
                    (resp.Item is Container && (resp.Item).Items.Count != 0))
                {
                    continue;
                }

                foreach (IShopSellInfo ssi in info)
                {
                    if (ssi.IsSellable(resp.Item))
                    {
                        int amount = resp.Amount;

                        if (amount > resp.Item.Amount)
                        {
                            amount = resp.Item.Amount;
                        }

                        if (ssi.IsResellable(resp.Item))
                        {
                            bool found = false;

                            foreach (var bii in buyInfo)
                            {
                                if (bii.Restock(resp.Item, amount))
                                {
                                    bii.OnSold(this, amount);

                                    resp.Item.Consume(amount);
                                    found = true;

                                    break;
                                }
                            }

                            if (!found)
                            {
                                cont = BuyPack;

                                if (amount < resp.Item.Amount)
                                {
                                    Item item = LiftItemDupe(resp.Item, resp.Item.Amount - amount);

                                    if (item != null)
                                    {
                                        item.SetLastMoved();
                                        cont.DropItem(item);
                                    }
                                    else
                                    {
                                        resp.Item.SetLastMoved();
                                        cont.DropItem(resp.Item);
                                    }
                                }
                                else
                                {
                                    resp.Item.SetLastMoved();
                                    cont.DropItem(resp.Item);
                                }
                            }
                        }
                        else
                        {
                            if (amount < resp.Item.Amount)
                            {
                                resp.Item.Amount -= amount;
                            }
                            else
                            {
                                resp.Item.Delete();
                            }
                        }

                        GiveGold += ssi.GetSellPriceFor(resp.Item, this) * amount;
                        break;
                    }
                }
            }

            if (GiveGold > 0)
            {
                while (GiveGold > 60000)
                {
                    seller.AddToBackpack(new Gold(60000));
                    GiveGold -= 60000;
                }

                seller.AddToBackpack(new Gold(GiveGold));

                seller.PlaySound(0x0037); //Gold dropping sound

                if (seller is PlayerMobile)
                {
                    var fada = ((PlayerMobile)seller).Wisp;
                    if (fada != null)
                    {
                        fada.Vende();
                    }
                }

                if (SupportsBulkOrders(seller))
                {
                    Item bulkOrder = CreateBulkOrder(seller, false);
                    if (bulkOrder != null && seller != null)
                        bulkOrder.BoundTo = seller.RawName;

                    if (bulkOrder is LargeBOD)
                    {
                        seller.SendGump(new LargeBODAcceptGump(seller, (LargeBOD)bulkOrder));
                    }
                    else if (bulkOrder is SmallBOD)
                    {
                        seller.SendGump(new SmallBODAcceptGump(seller, (SmallBOD)bulkOrder));
                    }
                }
            }
            //no cliloc for this?
            //SayTo( seller, true, "Thank you! I bought {0} item{1}. Here is your {2}gp.", Sold, (Sold > 1 ? "s" : ""), GiveGold );

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(3); // version

            writer.Write(BribeMultiplier);
            writer.Write(NextMultiplierDecay);
            writer.Write(RecentBribes);

            var sbInfos = SBInfos;

            for (int i = 0; sbInfos != null && i < sbInfos.Count; ++i)
            {
                SBInfo sbInfo = sbInfos[i];
                var buyInfo = sbInfo.BuyInfo;

                for (int j = 0; buyInfo != null && j < buyInfo.Count; ++j)
                {
                    GenericBuyInfo gbi = buyInfo[j];

                    int maxAmount = gbi.MaxAmount;
                    int doubled = 0;
                    int bought = gbi.TotalBought;
                    int sold = gbi.TotalSold;

                    switch (maxAmount)
                    {
                        case 40:
                            doubled = 1;
                            break;
                        case 80:
                            doubled = 2;
                            break;
                        case 160:
                            doubled = 3;
                            break;
                        case 320:
                            doubled = 4;
                            break;
                        case 640:
                            doubled = 5;
                            break;
                        case 999:
                            doubled = 6;
                            break;
                    }

                    if (doubled > 0 || bought > 0 || sold > 0)
                    {
                        writer.WriteEncodedInt(1 + ((j * sbInfos.Count) + i));
                        writer.WriteEncodedInt(doubled);
                        writer.WriteEncodedInt(bought);
                        writer.WriteEncodedInt(sold);
                    }
                }
            }

            writer.WriteEncodedInt(0);

            if (NextMultiplierDecay != DateTime.MinValue && NextMultiplierDecay < DateTime.UtcNow)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(10), () =>
                {
                    if (BribeMultiplier > 0)
                        BribeMultiplier /= 2;

                    CheckNextMultiplierDecay();
                });
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            LoadSBInfo();

            var sbInfos = SBInfos;

            switch (version)
            {
                case 3:
                case 2:
                    BribeMultiplier = reader.ReadInt();
                    NextMultiplierDecay = reader.ReadDateTime();
                    CheckNextMultiplierDecay(false); // Reset NextMultiplierDecay if it is out of range of the config
                    RecentBribes = reader.ReadInt();
                    goto case 1;
                case 1:
                    {
                        int index;

                        while ((index = reader.ReadEncodedInt()) > 0)
                        {
                            int doubled = reader.ReadEncodedInt();
                            int bought = 0;
                            int sold = 0;

                            if (version >= 3)
                            {
                                bought = reader.ReadEncodedInt();
                                sold = reader.ReadEncodedInt();
                            }

                            if (sbInfos != null)
                            {
                                index -= 1;
                                int sbInfoIndex = index % sbInfos.Count;
                                int buyInfoIndex = index / sbInfos.Count;

                                if (sbInfoIndex >= 0 && sbInfoIndex < sbInfos.Count)
                                {
                                    SBInfo sbInfo = sbInfos[sbInfoIndex];
                                    var buyInfo = sbInfo.BuyInfo;

                                    if (buyInfo != null && buyInfoIndex >= 0 && buyInfoIndex < buyInfo.Count)
                                    {
                                        GenericBuyInfo gbi = buyInfo[buyInfoIndex];

                                        int amount = 20;

                                        switch (doubled)
                                        {
                                            case 0:
                                                break;
                                            case 1:
                                                amount = 40;
                                                break;
                                            case 2:
                                                amount = 80;
                                                break;
                                            case 3:
                                                amount = 160;
                                                break;
                                            case 4:
                                                amount = 320;
                                                break;
                                            case 5:
                                                amount = 640;
                                                break;
                                            case 6:
                                                amount = 999;
                                                break;
                                        }

                                        amount *= 2;

                                        if (version == 2 && gbi.Stackable)
                                        {
                                            gbi.Amount = gbi.MaxAmount = BaseVendor.EconomyStockAmount;
                                        }
                                        else
                                        {
                                            gbi.Amount = gbi.MaxAmount = amount;
                                        }

                                        gbi.TotalBought = bought;
                                        gbi.TotalSold = sold;
                                    }
                                }
                            }
                        }

                        break;
                    }
            }

            if (IsParagon)
            {
                IsParagon = false;
            }

            if (version == 1)
            {
                BribeMultiplier = Utility.Random(10);
            }

            Timer.DelayCall(TimeSpan.Zero, CheckMorph);
        }

        public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
        {
            if (ConvertsMageArmor)
            {
                list.Add(new UpgradeMageArmor(from, this));
            }

            if (from.Alive && IsActiveVendor)
            {
                if (SupportsBulkOrders(from))
                {
                    list.Add(new BulkOrderInfoEntry(from, this));

                    if (BulkOrderSystem.NewSystemEnabled)
                    {
                        list.Add(new BribeEntry(from, this));
                        list.Add(new ClaimRewardsEntry(from, this));
                    }
                }

                if (IsActiveSeller)
                {
                    list.Add(new VendorBuyEntry(from, this));
                }

                if (IsActiveBuyer)
                {
                    list.Add(new VendorSellEntry(from, this));
                }
            }

            base.AddCustomContextEntries(from, list);
        }

        public virtual IShopSellInfo[] GetSellInfo()
        {
            return (IShopSellInfo[])m_ArmorSellInfo.ToArray(typeof(IShopSellInfo));
        }

        public virtual IBuyItemInfo[] GetBuyInfo()
        {
            return (IBuyItemInfo[])m_ArmorBuyInfo.ToArray(typeof(IBuyItemInfo));
        }

        #region Mage Armor Conversion
        public virtual bool ConvertsMageArmor { get { return false; } }

        private List<PendingConvert> _PendingConvertEntries = new List<PendingConvert>();

        private bool CheckConvertArmor(Mobile from, BaseArmor armor)
        {
            var convert = GetConvert(from, armor);

            if (convert == null || !(from is PlayerMobile))
                return false;

            object state = convert.Armor;

            RemoveConvertEntry(convert);
            from.CloseGump(typeof(Server.Gumps.ConfirmCallbackGump));

            from.SendGump(new Server.Gumps.ConfirmCallbackGump((PlayerMobile)from, 1049004, 1154115, state, null,
                (m, obj) =>
                {
                    BaseArmor ar = obj as BaseArmor;

                    if (!Deleted && ar != null && armor.IsChildOf(m.Backpack) && CanConvertArmor(m, ar))
                    {
                        if (!InRange(m.Location, 3))
                        {
                            m.SendLocalizedMessage(1149654); // You are too far away.
                }
                        else if (!Banker.Withdraw(m, 250000, true))
                        {
                            m.SendLocalizedMessage(1019022); // You do not have enough gold.
                }
                        else
                        {
                            ConvertMageArmor(m, ar);
                        }
                    }
                },
                (m, obj) =>
                {
                    var con = GetConvert(m, armor);

                    if (con != null)
                    {
                        RemoveConvertEntry(con);
                    }
                }));

            return true;
        }

        protected virtual bool CanConvertArmor(Mobile from, BaseArmor armor)
        {
            if (armor == null || armor is BaseShield || armor.ArtifactRarity != 0 || armor.IsArtifact)
            {
                from.SendLocalizedMessage(1113044); // You can't convert that.
                return false;
            }

            if (armor.ArmorAttributes.MageArmor == 0 &&
                Server.SkillHandlers.Imbuing.GetTotalMods(armor) > 4)
            {
                from.SendLocalizedMessage(1154119); // This action would exceed a stat cap
                return false;
            }

            return true;
        }

        public void TryConvertArmor(Mobile from, BaseArmor armor)
        {
            if (CanConvertArmor(from, armor))
            {
                from.SendLocalizedMessage(1154117); // Ah yes, I will convert this piece of armor but it's gonna cost you 250,000 gold coin. Payment is due immediately. Just hand me the armor.

                var convert = GetConvert(from, armor);

                if (convert != null)
                {
                    convert.ResetTimer();
                }
                else
                {
                    _PendingConvertEntries.Add(new PendingConvert(from, armor, this));
                }
            }
        }

        public virtual void ConvertMageArmor(Mobile from, BaseArmor armor)
        {
            if (armor.ArmorAttributes.MageArmor > 0)
                armor.ArmorAttributes.MageArmor = 0;
            else
                armor.ArmorAttributes.MageArmor = 1;

            from.SendLocalizedMessage(1154118); // Your armor has been converted.
        }

        private void RemoveConvertEntry(PendingConvert convert)
        {
            _PendingConvertEntries.Remove(convert);

            if (convert.Timer != null)
            {
                convert.Timer.Stop();
            }
        }

        private PendingConvert GetConvert(Mobile from, BaseArmor armor)
        {
            return _PendingConvertEntries.FirstOrDefault(c => c.From == from && c.Armor == armor);
        }

        protected class PendingConvert
        {
            public Mobile From { get; set; }
            public BaseArmor Armor { get; set; }
            public BaseVendor Vendor { get; set; }

            public Timer Timer { get; set; }
            public DateTime Expires { get; set; }

            public bool Expired { get { return DateTime.UtcNow > Expires; } }

            public PendingConvert(Mobile from, BaseArmor armor, BaseVendor vendor)
            {
                From = from;
                Armor = armor;
                Vendor = vendor;

                ResetTimer();
            }

            public void ResetTimer()
            {
                if (Timer != null)
                {
                    Timer.Stop();
                    Timer = null;
                }

                Expires = DateTime.UtcNow + TimeSpan.FromSeconds(120);

                Timer = Timer.DelayCall(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), OnTick);
                Timer.Start();
            }

            public void OnTick()
            {
                if (Expired)
                {
                    Vendor.RemoveConvertEntry(this);
                }
            }
        }
        #endregion
    }
}

namespace Server.ContextMenus
{
    public class VendorBuyEntry : ContextMenuEntry
    {
        private readonly BaseVendor m_Vendor;

        public VendorBuyEntry(Mobile from, BaseVendor vendor)
            : base(6103, 8)
        {
            m_Vendor = vendor;
            Enabled = vendor.CheckVendorAccess(from);
        }

        public override void OnClick()
        {
            m_Vendor.VendorBuy(Owner.From);
        }
    }

    public class VendorSellEntry : ContextMenuEntry
    {
        private readonly BaseVendor m_Vendor;

        public VendorSellEntry(Mobile from, BaseVendor vendor)
            : base(6104, 8)
        {
            m_Vendor = vendor;
            Enabled = vendor.CheckVendorAccess(from);
        }

        public override void OnClick()
        {
            m_Vendor.VendorSell(Owner.From);
        }
    }

    public class UpgradeMageArmor : ContextMenuEntry
    {
        public Mobile From { get; set; }
        public BaseVendor Vendor { get; set; }

        public UpgradeMageArmor(Mobile from, BaseVendor vendor)
            : base(1154114) // Convert Mage Armor
        {
            Enabled = vendor.CheckVendorAccess(from);

            From = from;
            Vendor = vendor;
        }

        public override void OnClick()
        {
            From.Target = new InternalTarget(From, Vendor);
            From.SendLocalizedMessage(1154116); // Target a piece of armor to show to the guild master.
        }

        private class InternalTarget : Target
        {
            public Mobile From { get; set; }
            public BaseVendor Vendor { get; set; }

            public InternalTarget(Mobile from, BaseVendor vendor)
                : base(1, false, TargetFlags.None)
            {
                From = from;
                Vendor = vendor;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is BaseArmor)
                {
                    BaseArmor armor = (BaseArmor)targeted;
                    Vendor.TryConvertArmor(from, armor);
                }
            }
        }
    }
}

namespace Server
{
    public interface IShopSellInfo
    {
        //get display name for an item
        string GetNameFor(Item item);

        //get price for an item which the player is selling
        int GetSellPriceFor(Item item);
        int GetSellPriceFor(Item item, BaseVendor vendor);

        //get price for an item which the player is buying
        int GetBuyPriceFor(Item item);
        int GetBuyPriceFor(Item item, BaseVendor vendor);

        //can we sell this item to this vendor?
        bool IsSellable(Item item);

        //What do we sell?
        Type[] Types { get; }

        //does the vendor resell this item?
        bool IsResellable(Item item);
    }

    public interface IBuyItemInfo
    {
        //get a new instance of an object (we just bought it)
        IEntity GetEntity();

        int ControlSlots { get; }

        int PriceScalar { get; set; }

        bool Stackable { get; set; }
        int TotalBought { get; set; }
        int TotalSold { get; set; }

        void OnBought(BaseVendor vendor, int amount);
        void OnSold(BaseVendor vendor, int amount);

        //display price of the item
        int Price { get; }

        //display name of the item
        string Name { get; }

        //display hue
        int Hue { get; }

        //display id
        int ItemID { get; }

        //amount in stock
        int Amount { get; set; }

        //max amount in stock
        int MaxAmount { get; }

        //Attempt to restock with item, (return true if restock sucessful)
        bool Restock(Item item, int amount);

        //called when its time for the whole shop to restock
        void OnRestock();
    }
}
