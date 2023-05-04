
#region References
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Server.Accounting;
using Server.ContextMenus;
using Server.Engines.BulkOrders;
using Server.Engines.CannedEvil;
using Server.Engines.Craft;
using Server.Engines.Help;
using Server.Engines.PartySystem;
using Server.Engines.Points;
using Server.Engines.Quests;
using Server.Engines.Shadowguard;
using Server.Engines.VoidPool;
using Server.Engines.VvV;
using Server.Ethics;
using Server.Factions;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Movement;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Services.Virtues;
using Server.SkillHandlers;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.First;
using Server.Spells.Fifth;
using Server.Spells.Fourth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.SkillMasteries;
using Server.Spells.Spellweaving;
using Server.Engines.SphynxFortune;
using Server.Engines.VendorSearching;
using Server.Targeting;

using RankDefinition = Server.Guilds.RankDefinition;
using Server.Services;
using Server.Misc.Templates;
using Felladrin.Automations;
using Server.Engines.TexasHoldem;
using Server.Ziden;
using Server.Commands;
using Server.Fronteira.RP;
using Server.Fronteira.Talentos;
using Server.Fronteira.Elementos;
using Server.Fronteira.Pvm;
using Server.Menus.Questions;
using Fronteira.Discord;
using Server.Fronteira.Guildas;
using VitaNex;
#endregion

namespace Server.Mobiles
{

    #region Enums
    [Flags]
    public enum PlayerFlag : ulong // First 16 bits are reserved for default-distro use, start custom flags at 0x00010000
    {
        None = 0x00000000,
        Glassblowing = 0x00000001,
        Masonry = 0x00000002,
        SandMining = 0x00000004,
        StoneMining = 0x00000008,
        ToggleMiningStone = 0x00000010,
        KarmaLocked = 0x00000020,
        AutoRenewInsurance = 0x00000040,
        UseOwnFilter = 0x00000080,
        PublicMyRunUO = 0x00000100,
        PagingSquelched = 0x00000200,
        Young = 0x00000400,
        AcceptGuildInvites = 0x00000800,
        DisplayChampionTitle = 0x00001000,
        HasStatReward = 0x00002000,
        Bedlam = 0x00010000,
        LibraryFriend = 0x00020000,
        Spellweaving = 0x00040000,
        GemMining = 0x00080000,
        ToggleMiningGem = 0x00100000,
        BasketWeaving = 0x00200000,
        AbyssEntry = 0x00400000,
        ToggleClippings = 0x00800000,
        ToggleCutClippings = 0x01000000,
        ToggleCutReeds = 0x02000000,
        MechanicalLife = 0x04000000,
        RP = 0x08000000,
        ToggleCutTopiaries = 0x10000000,
        HasValiantStatReward = 0x20000000,
        RefuseTrades = 0x40000000,
        DisabledPvpWarning = 0x80000000,
        HasPvMTag = 0x00030000,
    }

    [Flags]
    public enum ExtendedPlayerFlag
    {
        Unused = 0x00000001,
        ToggleStoneOnly = 0x00000002,
        CanBuyCarpets = 0x00000004,
        VoidPool = 0x00000008,
    }

    public enum NpcGuild
    {
        None,
        MagesGuild,
        WarriorsGuild,
        ThievesGuild,
        RangersGuild,
        HealersGuild,
        MinersGuild,
        MerchantsGuild,
        TinkersGuild,
        TailorsGuild,
        FishermensGuild,
        BardsGuild,
        BlacksmithsGuild
    }

    public enum SolenFriendship
    {
        None,
        Red,
        Black
    }
    #endregion

    public partial class PlayerMobile : Mobile, IHonorTarget
    {
        public int Almas = 0;
        public object Arrastando;
        public bool Mamonita = false;
        public int NivelBanco = 0;
        public WispGuia Wisp = null;
        public int Anuncios = 0;
        public int RankingFama = 0;
        public DateTime UltimaMorte;

        public List<CustomDungeons> DungeonsCompletas { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public byte Nivel { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PassoWispGuia { get; set; }

        private ElementoPvM _e;

        [CommandProperty(AccessLevel.GameMaster)]
        public override ElementoPvM Elemento
        {
            get { return _e; }
            set
            {
                if (NetState != null)
                {
                    InvalidateProperties();
                }
                _e = value;
            }
        }

        public override double GetBonusElemento(ElementoPvM elemento)
        {
            if (elemento != Elemento)
                return 0;
            return Elementos.BonusPorNivel(Elementos.GetNivel(Elemento));
        }

        public override bool Frozen { get => base.Frozen; set  {
                base.Frozen = value;
                if(value)
                {
                    var ctx = BandageContext.GetContext(this);
                    if(ctx != null)
                    {
                        ctx.StopHeal();
                        SendMessage("Voce parou de se curar");
                    }
                }
            } }

        public EventCalendarAccount m_EventCalendarAccount = null;

        [CommandProperty(AccessLevel.GameMaster)]
        public byte PontosTalento { get; set; }

        public int ExpTotal;

        public Talentos Talentos = new Talentos();

        [CommandProperty(AccessLevel.GameMaster)]
        public int Vidas { get; set; }

        private ElementoData _elemento = new ElementoData();

        [CommandProperty(AccessLevel.GameMaster)]
        public ElementoData Elementos { get { return _elemento; } set { _elemento = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public PatenteRP PatenteRP
        {
            get
            {
                return FichaRP.Patente;
            }
            set
            {
                FichaRP.Patente = value;
                if (value == PatenteRP.Desertor)
                {
                    SendMessage(38, "Voce agora e um desertor, e nao tera mais protecao");
                    PlaySound(0x5B3);
                    return;
                }
                else
                {
                    SendMessage(78, "Sua patente agora eh " + value.ToString());
                    PlaySound(0x5B5);
                }
            }
        }

        public void DesaprendeTalento(Talento t)
        {
            var def = DefTalentos.GetDef(t);
            this.Talentos.Desaprende(t);
            SendMessage("Voce desaprendeu " + def.Desc1);
        }

        public void AprendeTalento(Talento t)
        {

            this.Talentos.Aprende(t);
            var def = DefTalentos.GetDef(t);
            SendMessage("Voce aprendeu o talento " + def.Nome);
            SendMessage(def.Desc1);
            SendMessage(78, "Use .talentos para ver seus talentos");
            if (Talentos.IsHabilidade(t))
                SendMessage(78, "Este talento pode ser ativado ! Use .habilidades para ver suas habilidades !");

            TalentoEffect.GanhaEfeito(this, t);
            var from = this;
            Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 78, 0, 5060, 0);
            Effects.PlaySound(from.Location, from.Map, 0x243);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 78, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 78, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 78, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendTargetParticles(from, 0x375A, 35, 90, 78, 0x00, 9502, (EffectLayer)255, 0x100);
        }

        public override int VirtualArmorMod
        {
            get
            {
                return base.VirtualArmorMod + (Talentos.Tem(Talento.ProtecaoPesada) ? 10 : 0);
            }
            set { base.VirtualArmorMod = value; }
        }

        public static List<PlayerMobile> Instances { get; private set; }

        private Dictionary<string, DateTime> cooldowns = new Dictionary<string, DateTime>();

        public void SetCooldown(string name, TimeSpan span)
        {
            if (cooldowns.ContainsKey(name))
                cooldowns.Remove(name);

            var data = DateTime.UtcNow + span;
            cooldowns.Add(name, data);
        }

        public bool IsCooldown(string name)
        {
            if (!cooldowns.ContainsKey(name))
                return false;
            return cooldowns[name] > DateTime.UtcNow;
        }

        public Point3D castLocation = new Point3D(0, 0, 0);
        public object LastTarget = null;
        public Item LastItem = null;
        public int tutorialStep = 0;

        static PlayerMobile()
        {
            Instances = new List<PlayerMobile>(0x1000);
        }

        public void HarvestAnimation(int emote = 1)
        {
            if (Mounted)
            {
                var twoHand = FindItemOnLayer(Layer.TwoHanded);
                if (twoHand != null && twoHand is BaseWeapon)
                    Animate(AnimationType.Attack, 0);
                else
                    Animate(AnimationType.Attack, 3);
            }
            else
            {
                Animate(AnimationType.Emote, emote);
            }
        }

        private string _camps = "";

        [CommandProperty(AccessLevel.GameMaster)]
        public string CampfireLocations { get { return _camps; } set { _camps = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string GetCampFires { get { return CampfireLocations; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsSpiritSpeak { get { return SkillHandlers.SpiritSpeak.IsInSpiritSpeak(this); } set { SkillHandlers.SpiritSpeak.Remove(this); } }

        public long LastDamage = 0;

        public long GetMillisSinceLastDamage()
        {
            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            return now - LastDamage;
        }

        #region Mount Blocking
        public void SetMountBlock(BlockMountType type, TimeSpan duration, bool dismount)
        {
            if (dismount)
            {
                BaseMount.Dismount(this, this, type, duration, false);
            }
            else
            {
                BaseMount.SetMountPrevention(this, type, duration);
            }
        }
        #endregion

        #region dungeonmount

        public Mobile DungeonMount;

        public void StoreMount()
        {
            Shard.Debug("Store Mount", this);
            if (Mounted)
            {
                if (Mount is BaseMount)
                {
                    var mount = ((BaseMount)Mount);
                    DungeonMount = mount;
                    mount.Rider = null;
                    mount.ControlOrder = OrderType.Stay;
                    mount.Internalize();
                    LocalOverheadMessage(MessageType.Regular, 0, false, "Voce deixou sua montaria em um local seguro");
                }
                else if (Mount is EtherealMount)
                {
                    var ethy = (EtherealMount)Mount;
                    ethy.Rider = null;
                }
            }

            AnimalFormContext context = AnimalForm.GetContext(this);
            if (Shard.DebugEnabled)
                Shard.Debug("ANIMAL CTX = " + (context == null), this);
            if (context != null && context.SpeedBoost)
            {
                Shard.Debug("Cortando Speed Boost", this);
                this.SendSpeedControl(SpeedControlType.WalkSpeed);
            }
        }

        public override bool CorreEmStealth()
        {
            return this.StealthCorrendo || this.Talentos.Tem(Talento.CorrerStealth);
        }

        public void UnstoreMount()
        {
            AnimalFormContext context = AnimalForm.GetContext(this);
            if (context != null && context.SpeedBoost)
            {
                if (context.Type == typeof(Llama))
                    this.StealthCorrendo = true;
                else
                    this.StealthCorrendo = false;
                this.SendSpeedControl(SpeedControlType.MountSpeed);
            }

            var transform = TransformationSpellHelper.GetContext(this);
            if (transform != null && !this.Body.IsHuman)
            {
                return; // n pega montaria de volta se tiver transformado
            }

            if (this.Region is DungeonRegion || this.Region is DungeonGuardedRegion)
            {
                return;
            }

            if (this.Alive)
            {
                if (DungeonMount != null)
                {
                    DungeonMount.MoveToWorld(Location, Map);
                    ((BaseMount)DungeonMount).Rider = this;
                    DungeonMount = null;
                    LocalOverheadMessage(MessageType.Regular, 0, false, "Voce pegou sua montaria novamente");
                }
                else
                {
                    var ethy = Backpack.FindItemByType<EtherealMount>();
                    if (ethy != null)
                    {
                        ethy.Rider = this;
                        LocalOverheadMessage(MessageType.Regular, 0, false, "Voce invocou sua montaria magica novamente");
                    }
                }

            }
        }

        private bool DicaCombate = true;
        private bool DicaBands = true;

        public override bool Warmode { get { return base.Warmode; } set { base.Warmode = value; if (value) DicaCombate = false; } }


        public override void OnRegionChange(Region Old, Region New)
        {
            base.OnRegionChange(Old, New);

            var newDungeon = New is DungeonRegion || New is DungeonGuardedRegion;
            var oldDungeon = Old is DungeonRegion || Old is DungeonGuardedRegion;

            ViceVsVirtueSystem.OnRegionChange(this, Old, New);

            if (oldDungeon != null && StuckMenu.IsInSecondAgeArea(this))
            {
                PrivateOverheadMessage("* perigo *", 32);
                SendMessage(32, "[PERIGO] Voce esta nas terras perdidas. Volte se nao quiser morrer.");
            }

            Timer.DelayCall(TimeSpan.FromSeconds(3), () =>
            {
                if (DicaCombate && Wisp != null && Young && New is DungeonGuardedRegion && !(Old is DungeonGuardedRegion))
                {
                    if (!IsCooldown("dicacbt"))
                    {
                        SetCooldown("dicacbt", TimeSpan.FromSeconds(10));
                        Wisp.Fala("Aperte ALT+P e clique em PEACE para entrar em modo guerra. Clique 2x no monstro para bater.");

                    }
                }
            });


            DecideMusic(Old, New);

            if (newDungeon)
            {
                if (!oldDungeon)
                {
                    this.LastDungeonEntrance = this.Location;
                }
                Timer.DelayCall(TimeSpan.FromMilliseconds(50), new TimerCallback(StoreMount));
            }
            else if (oldDungeon)
            {
                Timer.DelayCall(TimeSpan.FromMilliseconds(100), new TimerCallback(UnstoreMount));
            }
        }

        #endregion

        #region Stygian Abyss
        public override void ToggleFlying()
        {
            if (Race != Race.Gargoyle)
                return;

            if (Frozen)
            {
                SendLocalizedMessage(1060170); // You cannot use this ability while frozen.
                return;
            }

            if (!Flying)
            {
                if (BeginAction(typeof(FlySpell)))
                {
                    if (this.Spell is Spell)
                        ((Spell)this.Spell).Disturb(DisturbType.Algo, false, false);

                    Spell spell = new FlySpell(this);
                    spell.Cast();

                    Timer.DelayCall(TimeSpan.FromSeconds(3), () => EndAction(typeof(FlySpell)));
                }
                else
                {
                    LocalOverheadMessage(MessageType.Regular, 0x3B2, 1075124); // You must wait before casting that spell again.
                }
            }
            else if (IsValidLandLocation(Location, Map))
            {
                if (BeginAction(typeof(FlySpell)))
                {
                    if (this.Spell is Spell)
                        ((Spell)this.Spell).Disturb(DisturbType.Algo, false, false);

                    Animate(AnimationType.Land, 0);
                    Flying = false;
                    BuffInfo.RemoveBuff(this, BuffIcon.Fly);

                    Timer.DelayCall(TimeSpan.FromSeconds(3), () => EndAction(typeof(FlySpell)));
                }
                else
                {
                    LocalOverheadMessage(MessageType.Regular, 0x3B2, 1075124); // You must wait before casting that spell again.
                }
            }
            else
                LocalOverheadMessage(MessageType.Regular, 0x3B2, 1113081); // You may not land here.
        }

        public static bool IsValidLandLocation(Point3D p, Map map)
        {
            return map.CanFit(p.X, p.Y, p.Z, 16, false, false);
        }
        #endregion

        private class CountAndTimeStamp
        {
            private int m_Count;
            private DateTime m_Stamp;

            public DateTime TimeStamp { get { return m_Stamp; } }

            public int Count
            {
                get { return m_Count; }
                set
                {
                    m_Count = value;
                    m_Stamp = DateTime.UtcNow;
                }
            }
        }

        private DesignContext m_DesignContext;

        private NpcGuild m_NpcGuild;
        private DateTime m_NpcGuildJoinTime;
        private TimeSpan m_NpcGuildGameTime;
        private PlayerFlag m_Flags;
        private ExtendedPlayerFlag m_ExtendedFlags;
        private int m_Profession;

        private int m_NonAutoreinsuredItems;
        // number of items that could not be automaitically reinsured because gold in bank was not enough

        /*
		* a value of zero means, that the mobile is not executing the spell. Otherwise,
		* the value should match the BaseMana required
		*/
        private int m_ExecutesLightningStrike; // move to Server.Mobiles??

        private DateTime m_LastOnline;
        private RankDefinition m_GuildRank;
        private bool m_NextEnhanceSuccess;

        public bool TemTalentoNovo()
        {
            return PontosTalento > 0;
        }

        public void GanhaExpRP(int pontos)
        {
            PointsSystem.Exp.AwardPoints(this, pontos, false, false);
            ExpTotal += pontos;
            if (!IsCooldown("xpp"))
            {
                SetCooldown("xpp", TimeSpan.FromHours(1));
                SendMessage(78, "Digite .xp para usar sua EXP para subir skills");
            }

            if (Nivel < 11 && ExpTotal > ExpProximoNivel())
            {
                ExpTotal = 0;
                Nivel += 1;
                PontosTalento += 1;
                SendMessage(78, "Voce ganhou um novo ponto de talento ! Digite .talento para usa-lo !");
                this.SendGump(new AnuncioGump(this, "Digite .talento para usar seu ponto de talento"));
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool NextEnhanceSuccess { get { return m_NextEnhanceSuccess; } set { m_NextEnhanceSuccess = value; } }

        private int m_GuildMessageHue, m_AllianceMessageHue;

        private List<Mobile> m_AutoStabled;
        private List<Mobile> m_AllFollowers;
        private List<Mobile> m_RecentlyReported;

        public bool UseSummoningRite { get; set; }

        #region Guantlet Points
        private double m_GauntletPoints;

        [CommandProperty(AccessLevel.Administrator)]
        public double GauntletPoints { get { return m_GauntletPoints; } set { m_GauntletPoints = value; } }
        #endregion

        #region Points System
        private PointsSystemProps _PointsSystemProps;
        private BODProps _BODProps;
        private AccountGoldProps _AccountGold;

        [CommandProperty(AccessLevel.GameMaster)]
        public PointsSystemProps PointSystems
        {
            get
            {
                if (_PointsSystemProps == null)
                    _PointsSystemProps = new PointsSystemProps(this);

                return _PointsSystemProps;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODProps BODData
        {
            get
            {
                if (_BODProps == null)
                {
                    _BODProps = new BODProps(this);
                }

                return _BODProps;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AccountGoldProps AccountGold
        {
            get
            {
                if (_AccountGold == null)
                {
                    _AccountGold = new AccountGoldProps(this);
                }

                return _AccountGold;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MoedasMagicas
        {
            get
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    return acct.MoedasMagicas;
                }

                return 0;
            }
            set
            {
                var acct = Account as Account;

                if (acct != null)
                {
                    acct.SetMoedasMagicas(value);
                }
            }
        }

        public bool DepositSovereigns(int amount)
        {
            var acct = Account as Account;

            if (acct != null)
            {
                return acct.DepositarMoedasMagicas(amount);
            }

            return false;
        }

        public bool WithdrawSovereigns(int amount)
        {
            var acct = Account as Account;

            if (acct != null)
            {
                return acct.SacarMoedasMagicas(amount);
            }

            return false;
        }
        #endregion

        #region Getters & Setters
        public List<Mobile> RecentlyReported { get { return m_RecentlyReported; } set { m_RecentlyReported = value; } }

        public List<Mobile> AutoStabled { get { return m_AutoStabled; } }

        public bool NinjaWepCooldown { get; set; }

        public List<Mobile> AllFollowers
        {
            get
            {
                if (m_AllFollowers == null)
                {
                    m_AllFollowers = new List<Mobile>();
                }

                return m_AllFollowers;
            }
        }

        [CommandProperty(AccessLevel.GameMaster, true)]
        public RankDefinition GuildRank
        {
            get
            {
                if (AccessLevel >= AccessLevel.GameMaster)
                {
                    return RankDefinition.Leader;
                }
                else
                {
                    return m_GuildRank;
                }
            }
            set { m_GuildRank = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GuildMessageHue { get { return m_GuildMessageHue; } set { m_GuildMessageHue = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AllianceMessageHue { get { return m_AllianceMessageHue; } set { m_AllianceMessageHue = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Profession
        {
            get { return m_Profession; }
            set
            {
                m_Profession = value;
                if (Wisp != null)
                {
                    Wisp.TrocaTemplate(value);
                }
            }
        }

        public int StepsTaken { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public NpcGuild NpcGuild { get { return m_NpcGuild; } set { m_NpcGuild = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NpcGuildJoinTime { get { return m_NpcGuildJoinTime; } set { m_NpcGuildJoinTime = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextBODTurnInTime { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastOnline { get { return m_LastOnline; } set { m_LastOnline = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public long LastMoved { get { return LastMoveTime; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NpcGuildGameTime { get { return m_NpcGuildGameTime; } set { m_NpcGuildGameTime = value; } }

        private int m_ToTItemsTurnedIn;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ToTItemsTurnedIn { get { return m_ToTItemsTurnedIn; } set { m_ToTItemsTurnedIn = value; } }

        private int m_ToTTotalMonsterFame;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ToTTotalMonsterFame { get { return m_ToTTotalMonsterFame; } set { m_ToTTotalMonsterFame = value; } }

        public int ExecutesLightningStrike { get { return m_ExecutesLightningStrike; } set { m_ExecutesLightningStrike = value; } }

        private int m_VASTotalMonsterFame;

        [CommandProperty(AccessLevel.GameMaster)]
        public int VASTotalMonsterFame { get { return m_VASTotalMonsterFame; } set { m_VASTotalMonsterFame = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ToothAche { get { return BaseSweet.GetToothAche(this); } set { BaseSweet.SetToothAche(this, value, true); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool MechanicalLife { get { return GetFlag(PlayerFlag.MechanicalLife); } set { SetFlag(PlayerFlag.MechanicalLife, value); } }
        #endregion

        #region PlayerFlags
        public PlayerFlag Flags { get { return m_Flags; } set { m_Flags = value; } }
        public ExtendedPlayerFlag ExtendedFlags { get { return m_ExtendedFlags; } set { m_ExtendedFlags = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PagingSquelched { get { return GetFlag(PlayerFlag.PagingSquelched); } set { SetFlag(PlayerFlag.PagingSquelched, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Glassblowing { get { return GetFlag(PlayerFlag.Glassblowing); } set { SetFlag(PlayerFlag.Glassblowing, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Masonry { get { return GetFlag(PlayerFlag.Masonry); } set { SetFlag(PlayerFlag.Masonry, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SandMining { get { return GetFlag(PlayerFlag.SandMining); } set { SetFlag(PlayerFlag.SandMining, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool StoneMining { get { return GetFlag(PlayerFlag.StoneMining); } set { SetFlag(PlayerFlag.StoneMining, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool GemMining { get { return GetFlag(PlayerFlag.GemMining); } set { SetFlag(PlayerFlag.GemMining, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool BasketWeaving { get { return GetFlag(PlayerFlag.BasketWeaving); } set { SetFlag(PlayerFlag.BasketWeaving, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleMiningStone { get { return GetFlag(PlayerFlag.ToggleMiningStone); } set { SetFlag(PlayerFlag.ToggleMiningStone, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AbyssEntry { get { return GetFlag(PlayerFlag.AbyssEntry); } set { SetFlag(PlayerFlag.AbyssEntry, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleMiningGem { get { return GetFlag(PlayerFlag.ToggleMiningGem); } set { SetFlag(PlayerFlag.ToggleMiningGem, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool KarmaLocked { get { return GetFlag(PlayerFlag.KarmaLocked); } set { SetFlag(PlayerFlag.KarmaLocked, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AutoRenewInsurance { get { return GetFlag(PlayerFlag.AutoRenewInsurance); } set { SetFlag(PlayerFlag.AutoRenewInsurance, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UseOwnFilter { get { return GetFlag(PlayerFlag.UseOwnFilter); } set { SetFlag(PlayerFlag.UseOwnFilter, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AcceptGuildInvites { get { return GetFlag(PlayerFlag.AcceptGuildInvites); } set { SetFlag(PlayerFlag.AcceptGuildInvites, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasStatReward { get { return GetFlag(PlayerFlag.HasStatReward); } set { SetFlag(PlayerFlag.HasStatReward, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasValiantStatReward { get { return GetFlag(PlayerFlag.HasValiantStatReward); } set { SetFlag(PlayerFlag.HasValiantStatReward, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RefuseTrades
        {
            get { return GetFlag(PlayerFlag.RefuseTrades); }
            set { SetFlag(PlayerFlag.RefuseTrades, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DisabledPvpWarning
        {
            get { return GetFlag(PlayerFlag.DisabledPvpWarning); }
            set { SetFlag(PlayerFlag.DisabledPvpWarning, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool CanBuyCarpets
        {
            get { return GetFlag(ExtendedPlayerFlag.CanBuyCarpets); }
            set { SetFlag(ExtendedPlayerFlag.CanBuyCarpets, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool VoidPool
        {
            get { return GetFlag(ExtendedPlayerFlag.VoidPool); }
            set { SetFlag(ExtendedPlayerFlag.VoidPool, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleStoneOnly
        {
            get { return GetFlag(ExtendedPlayerFlag.ToggleStoneOnly); }
            set { SetFlag(ExtendedPlayerFlag.ToggleStoneOnly, value); }
        }

        #region Plant system
        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleClippings { get { return GetFlag(PlayerFlag.ToggleClippings); } set { SetFlag(PlayerFlag.ToggleClippings, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleCutReeds { get { return GetFlag(PlayerFlag.ToggleCutReeds); } set { SetFlag(PlayerFlag.ToggleCutReeds, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleCutClippings { get { return GetFlag(PlayerFlag.ToggleCutClippings); } set { SetFlag(PlayerFlag.ToggleCutClippings, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleCutTopiaries { get { return GetFlag(PlayerFlag.ToggleCutTopiaries); } set { SetFlag(PlayerFlag.ToggleCutTopiaries, value); } }

        private DateTime m_SSNextSeed;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SSNextSeed { get { return m_SSNextSeed; } set { m_SSNextSeed = value; } }

        private DateTime m_SSSeedExpire;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SSSeedExpire { get { return m_SSSeedExpire; } set { m_SSSeedExpire = value; } }

        private Point3D m_SSSeedLocation;

        public Point3D SSSeedLocation { get { return m_SSSeedLocation; } set { m_SSSeedLocation = value; } }

        private Map m_SSSeedMap;

        public Map SSSeedMap { get { return m_SSSeedMap; } set { m_SSSeedMap = value; } }
        #endregion

        #endregion

        /* Begin Captcha Mod */
        private DateTime _NextCaptchaTime;
        public DateTime NextCaptchaTime
        {
            get { return _NextCaptchaTime; }
            set { _NextCaptchaTime = value; }
        }

        [CommandProperty(AccessLevel.Seer)]
        public TimeSpan CaptchaDelay
        {
            get
            {
                if (DateTime.Now >= _NextCaptchaTime)
                    return TimeSpan.FromSeconds(0);

                return (_NextCaptchaTime - DateTime.Now);
            }
            set { _NextCaptchaTime = DateTime.Now + value; }
        }
        /* End Captcha Mod */

        #region Auto Arrow Recovery
        private readonly Dictionary<Type, int> m_RecoverableAmmo = new Dictionary<Type, int>();

        public Dictionary<Type, int> RecoverableAmmo { get { return m_RecoverableAmmo; } }

        public void RecoverAmmo()
        {
            if (Core.SE && Alive)
            {
                foreach (var kvp in m_RecoverableAmmo)
                {
                    if (kvp.Value > 0)
                    {
                        Item ammo = null;

                        try
                        {
                            ammo = Activator.CreateInstance(kvp.Key) as Item;
                        }
                        catch
                        { }

                        if (ammo != null)
                        {
                            string name = ammo.Name;
                            ammo.Amount = kvp.Value;

                            if (name == null)
                            {
                                if (ammo is Arrow)
                                {
                                    name = "arrow";
                                }
                                else if (ammo is Bolt)
                                {
                                    name = "bolt";
                                }
                            }

                            if (name != null && ammo.Amount > 1)
                            {
                                name = String.Format("{0}s", name);
                            }

                            if (name == null)
                            {
                                name = String.Format("#{0}", ammo.LabelNumber);
                            }

                            _PlaceInBackpack(ammo);
                            SendLocalizedMessage(1073504, String.Format("{0}\t{1}", ammo.Amount, name)); // You recover ~1_NUM~ ~2_AMMO~.
                        }
                    }
                }

                m_RecoverableAmmo.Clear();
            }
        }
        #endregion

        #region Reward Stable Slots
        [CommandProperty(AccessLevel.GameMaster)]
        public int RewardStableSlots { get; set; }
        #endregion

        private DateTime m_AnkhNextUse;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime AnkhNextUse { get { return m_AnkhNextUse; } set { m_AnkhNextUse = value; } }

        #region Mondain's Legacy
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Bedlam { get { return GetFlag(PlayerFlag.Bedlam); } set { SetFlag(PlayerFlag.Bedlam, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool LibraryFriend { get { return GetFlag(PlayerFlag.LibraryFriend); } set { SetFlag(PlayerFlag.LibraryFriend, value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Spellweaving { get { return GetFlag(PlayerFlag.Spellweaving); } set { SetFlag(PlayerFlag.Spellweaving, value); } }
        #endregion

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan DisguiseTimeLeft { get { return DisguiseTimers.TimeRemaining(this); } }

        private DateTime m_PeacedUntil;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime PeacedUntil { get { return m_PeacedUntil; } set { m_PeacedUntil = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Famoso { get; set; }

        private void DaFama()
        {
            if (!Famoso)
            {
                Famoso = true;
                var tit = (Female ? "Lady" : "Lord");
                SendMessage(78, "Seu nivel de fama lhe tornou famoso. Agora voce eh conhecido como " + tit);
                Anuncio.Anuncia(Name + " conquistou o titulo de " + tit);
            }
        }

        [CommandProperty(AccessLevel.Decorator)]
        public override string TitleName
        {
            get
            {
                string name;

                if (Famoso || Fame >= 15000)
                {
                    DaFama();
                    name = String.Format("{0} {1}", Female ? "Lady" : "Lord", RawName);
                }
                else
                    name = RawName;

                if (RP)
                {
                    name = string.Format("{0} {1}", FichaRP.Patente.ToString(), name);
                }

                if (RankingFama < 10)
                {
                    name = (Female ? "A Famosa " : "O Famoso ") + name;
                }




                return name;
            }
        }

        #region Scroll of Alacrity
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime AcceleratedStart { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName AcceleratedSkill { get; set; }
        #endregion

        public static Direction GetDirection4(Point3D from, Point3D to)
        {
            int dx = from.X - to.X;
            int dy = from.Y - to.Y;

            int rx = dx - dy;
            int ry = dx + dy;

            Direction ret;

            if (rx >= 0 && ry >= 0)
            {
                ret = Direction.West;
            }
            else if (rx >= 0 && ry < 0)
            {
                ret = Direction.South;
            }
            else if (rx < 0 && ry < 0)
            {
                ret = Direction.East;
            }
            else
            {
                ret = Direction.North;
            }

            return ret;
        }

        public override void Lift(Item item, int amount, out bool rejected, out LRReason reject)
        {
            var delay = false;
            if (item != null && item.Parent != null)
            {
                if (item.Parent is Corpse)
                {
                    var corpse = (Corpse)item.Parent;
                    if (corpse.Owner is BaseCreature)
                    {
                        if (((BaseCreature)corpse.Owner).GetLootingRights().Count > 1)
                            delay = true;
                    }
                    else if (corpse.Owner is PlayerMobile)
                    {
                        delay = true;
                    }
                }
            }
            base.Lift(item, amount, out rejected, out reject);
            if (delay)
            {
                NextActionTime = Core.TickCount + 200 + Utility.Random(100); // nao ta funfando
            }
        }

        public override bool OnDroppedItemToWorld(Item item, Point3D location)
        {
            if (!base.OnDroppedItemToWorld(item, location))
            {
                return false;
            }

            if (Core.AOS)
            {
                IPooledEnumerable mobiles = Map.GetMobilesInRange(location, 0);

                foreach (Mobile m in mobiles)
                {
                    if (m.Z >= location.Z && m.Z < location.Z + 16)
                    {
                        mobiles.Free();
                        return false;
                    }
                }

                mobiles.Free();
            }

            BounceInfo bi = item.GetBounce();

            if (bi != null && (!Core.SA || AccessLevel > AccessLevel.Counselor))
            {
                Type type = item.GetType();

                if (type.IsDefined(typeof(FurnitureAttribute), true) || type.IsDefined(typeof(DynamicFlipingAttribute), true))
                {
                    var objs = type.GetCustomAttributes(typeof(FlipableAttribute), true);

                    if (objs != null && objs.Length > 0)
                    {
                        FlipableAttribute fp = objs[0] as FlipableAttribute;

                        if (fp != null)
                        {
                            var itemIDs = fp.ItemIDs;

                            Point3D oldWorldLoc = bi.m_WorldLoc;
                            Point3D newWorldLoc = location;

                            if (oldWorldLoc.X != newWorldLoc.X || oldWorldLoc.Y != newWorldLoc.Y)
                            {
                                Direction dir = GetDirection4(oldWorldLoc, newWorldLoc);

                                if (itemIDs.Length == 2)
                                {
                                    switch (dir)
                                    {
                                        case Direction.North:
                                        case Direction.South:
                                            item.ItemID = itemIDs[0];
                                            break;
                                        case Direction.East:
                                        case Direction.West:
                                            item.ItemID = itemIDs[1];
                                            break;
                                    }
                                }
                                else if (itemIDs.Length == 4)
                                {
                                    switch (dir)
                                    {
                                        case Direction.South:
                                            item.ItemID = itemIDs[0];
                                            break;
                                        case Direction.East:
                                            item.ItemID = itemIDs[1];
                                            break;
                                        case Direction.North:
                                            item.ItemID = itemIDs[2];
                                            break;
                                        case Direction.West:
                                            item.ItemID = itemIDs[3];
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var house = BaseHouse.FindHouseAt(this);
            if (house != null && (house.IsFriend(this) || house.IsOwner(this) || house.IsCoOwner(this)))
            {
                Timer.DelayCall(TimeSpan.FromMilliseconds(5), () =>
                {
                    item.LastMoved = DateTime.UtcNow + TimeSpan.FromMinutes(int.MaxValue);
                });

                if (!this.IsCooldown("msgcasa"))
                {
                    this.SetCooldown("msgcasa");
                    this.SendMessage(78, "Use .trancar em items no chao de casas para eles nao sumirem.");
                    this.SendMessage(78, "Para trancar um bau e torna-lo seguro, use .secure. Mais informacoes na nossa wiki");
                    if (this.Young && !this.IsCooldown("msgcasa2"))
                    {
                        this.SendGump(new GumpFala(n => { }, Faces.GM_PRETO, "Voce botou um item em uma casa.", "Para tornar baus seguros use o comando .secure", "Para trancar items use .trancar", "", "Items nao seguros e nao trancados VAO SUMIR"));
                        this.SetCooldown("msgcasa2");
                    }
                }
            }
            return true;
        }

        #region PlayerTemplates
        public PlayerTemplates Templates = new PlayerTemplates();
        public string CurrentTemplate = "default";
        #endregion

        public override int GetPacketFlags()
        {
            int flags = base.GetPacketFlags();
            return flags;
        }

        public override int GetOldPacketFlags()
        {
            int flags = base.GetOldPacketFlags();
            return flags;
        }

        public bool GetFlag(PlayerFlag flag)
        {
            return ((m_Flags & flag) != 0);
        }

        public void SetFlag(PlayerFlag flag, bool value)
        {
            if (value)
            {
                m_Flags |= flag;
            }
            else
            {
                m_Flags &= ~flag;
            }
        }

        public bool GetFlag(ExtendedPlayerFlag flag)
        {
            return ((m_ExtendedFlags & flag) != 0);
        }

        public void SetFlag(ExtendedPlayerFlag flag, bool value)
        {
            if (value)
            {
                m_ExtendedFlags |= flag;
            }
            else
            {
                m_ExtendedFlags &= ~flag;
            }
        }

        public DesignContext DesignContext { get { return m_DesignContext; } set { m_DesignContext = value; } }

        public static void Initialize()
        {
            if (FastwalkPrevention)
            {
                PacketHandlers.RegisterThrottler(0x02, MovementThrottle_Callback);
            }

            EventSink.Login += OnLogin;
            EventSink.Logout += OnLogout;
            EventSink.Connected += EventSink_Connected;
            EventSink.Disconnected += EventSink_Disconnected;

            #region Enchanced Client
            EventSink.TargetedSkill += Targeted_Skill;
            EventSink.EquipMacro += EquipMacro;
            EventSink.UnequipMacro += UnequipMacro;
            #endregion

            if (Core.SE)
            {
                Timer.DelayCall(TimeSpan.Zero, CheckPets);
            }
        }

        #region Enhanced Client
        private static void Targeted_Skill(TargetedSkillEventArgs e)
        {
            Mobile from = e.Mobile;
            int SkillId = e.SkillID;
            IEntity target = e.Target;

            if (from == null || target == null)
                return;

            from.TargetLocked = true;

            if (e.SkillID == 35)
            {
                AnimalTaming.DisableMessage = true;
                AnimalTaming.DeferredTarget = false;
            }

            if (from.UseSkill(e.SkillID) && from.Target != null)
            {
                from.Target.Invoke(from, target);
            }

            if (e.SkillID == 35)
            {
                AnimalTaming.DeferredTarget = true;
                AnimalTaming.DisableMessage = false;
            }

            from.TargetLocked = false;
        }

        public static void EquipMacro(EquipMacroEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null && pm.Backpack != null && pm.Alive && e.List != null && e.List.Count > 0)
            {
                Container pack = pm.Backpack;

                e.List.ForEach(serial =>
                {
                    Item item = pack.Items.FirstOrDefault(i => i.Serial == serial);

                    if (item != null)
                    {
                        Item toMove = pm.FindItemOnLayer(item.Layer);

                        if (toMove != null)
                        {
                            //pack.DropItem(toMove);
                            toMove.Internalize();

                            if (!pm.EquipItem(item))
                            {
                                pm.EquipItem(toMove);
                            }
                            else
                            {
                                pack.DropItem(toMove);
                            }
                        }
                        else
                        {
                            pm.EquipItem(item);
                        }
                    }
                });
            }
        }

        public static void UnequipMacro(UnequipMacroEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null && pm.Backpack != null && pm.Alive && e.List != null && e.List.Count > 0)
            {
                Container pack = pm.Backpack;

                List<Item> worn = new List<Item>(pm.Items);

                foreach (var item in worn)
                {
                    if (e.List.Contains((int)item.Layer))
                    {
                        pack.TryDropItem(pm, item, false);
                    }
                }

                ColUtility.Free(worn);
            }
        }
        #endregion

        private static void CheckPets()
        {
            foreach (PlayerMobile pm in World.Mobiles.Values.OfType<PlayerMobile>())
            {
                if (((!pm.Mounted || (pm.Mount != null && pm.Mount is EtherealMount)) &&
                     (pm.AllFollowers.Count > pm.AutoStabled.Count)) ||
                    (pm.Mounted && (pm.AllFollowers.Count > (pm.AutoStabled.Count + 1))))
                {
                    pm.AutoStablePets(); /* autostable checks summons, et al: no need here */
                }
            }
        }

        public override void OnSkillsQuery(Mobile from)
        {
            base.OnSkillsQuery(from);
            /*
            if (from == this && this.Profession > 0)
            {
                this.CloseGump(typeof(TemplateHotkey));
                this.SendGump(new TemplateHotkey(this));
            }
            */
        }

        public override void OnSkillInvalidated(Skill skill)
        {
            if (Core.AOS && skill.SkillName == SkillName.MagicResist)
            {
                UpdateResistances();
            }
        }

        public override int GetMaxResistance(ResistanceType type)
        {
            if (IsStaff())
            {
                return 100;
            }

            int max = base.GetMaxResistance(type);
            int refineBonus = BaseArmor.GetRefinedResist(this, type);

            if (refineBonus != 0)
            {
                max += refineBonus;
            }
            else
            {
                max += Spells.Mysticism.StoneFormSpell.GetMaxResistBonus(this);
            }

            if (Core.ML && Race == Race.Elf && type == ResistanceType.Energy)
            {
                max += 5; //Intended to go after the 60 max from curse
            }

            if (type != ResistanceType.Physical && 60 < max && Spells.Fourth.CurseSpell.UnderEffect(this))
            {
                max -= 10;
                //max = 60;
            }

            if ((type == ResistanceType.Fire || type == ResistanceType.Poison) && CorpseSkinSpell.IsUnderEffects(this))
            {
                max = CorpseSkinSpell.GetResistMalus(this);
            }

            return max;
        }

        public override void ComputeResistances()
        {
            base.ComputeResistances();

            for (int i = 0; i < Resistances.Length; ++i)
            {
                Resistances[i] = 0;
            }

            Resistances[0] += BasePhysicalResistance;
            Resistances[1] += BaseFireResistance;
            Resistances[2] += BaseColdResistance;
            Resistances[3] += BasePoisonResistance;
            Resistances[4] += BaseEnergyResistance;

            for (int i = 0; ResistanceMods != null && i < ResistanceMods.Count; ++i)
            {
                ResistanceMod mod = ResistanceMods[i];
                int v = (int)mod.Type;

                if (v >= 0 && v < Resistances.Length)
                {
                    Resistances[v] += mod.Offset;
                }
            }

            for (int i = 0; i < Items.Count; ++i)
            {
                Item item = Items[i];

                if (item.CheckPropertyConfliction(this))
                {
                    continue;
                }

                ISetItem setItem = item as ISetItem;

                Resistances[0] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Physical) : item.PhysicalResistance;
                Resistances[1] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Fire) : item.FireResistance;
                Resistances[2] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Cold) : item.ColdResistance;
                Resistances[3] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Poison) : item.PoisonResistance;
                Resistances[4] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Energy) : item.EnergyResistance;
            }

            for (int i = 0; i < Resistances.Length; ++i)
            {
                int min = GetMinResistance((ResistanceType)i);
                int max = GetMaxResistance((ResistanceType)i);

                if (max < min)
                {
                    max = min;
                }

                if (Resistances[i] > max)
                {
                    Resistances[i] = max;
                }
                else if (Resistances[i] < min)
                {
                    Resistances[i] = min;
                }
            }
        }

        protected override void OnRaceChange(Race oldRace)
        {
            if (oldRace == Race.Gargoyle && Flying)
            {
                Flying = false;
                SendSpeedControl(SpeedControlType.Disable);
                BuffInfo.RemoveBuff(this, BuffIcon.Fly);
            }
            else if (oldRace != Race.Gargoyle && Race == Race.Gargoyle && Mounted)
            {
                Mount.Rider = null;
            }

            ValidateEquipment();
            UpdateResistances();
        }

        public override int MaxWeight
        {
            get
            {
                return 10 + (int)(4 * Str) + (int)(Skills[SkillName.Camping].Value * 2) + BonusPeso + (this.RP && this.TemTalento(Talento.BurroDeCarga) ? 200 : 0);
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public int PropMaxWeigt { get { return MaxWeight; } set { } }

        [CommandProperty(AccessLevel.Administrator)]
        public int BonusPeso { get; set; }

        private int m_LastGlobalLight = -1, m_LastPersonalLight = -1;

        public override void OnNetStateChanged()
        {
            m_LastGlobalLight = -1;
            m_LastPersonalLight = -1;
        }

        public override void ComputeBaseLightLevels(out int global, out int personal)
        {
            global = LightCycle.ComputeLevelFor(this);

            bool racialNightSight = (Core.ML && Race == Race.Elf);

            if (LightLevel < 21 && (AosAttributes.GetValue(this, AosAttribute.NightSight) > 0 || racialNightSight))
            {
                personal = 21;
            }
            else
            {
                personal = LightLevel;
            }
        }

        public override void CheckLightLevels(bool forceResend)
        {
            NetState ns = NetState;

            if (ns == null)
            {
                return;
            }

            int global, personal;

            ComputeLightLevels(out global, out personal);

            if (!forceResend)
            {
                forceResend = (global != m_LastGlobalLight || personal != m_LastPersonalLight);
            }

            if (!forceResend)
            {
                return;
            }

            m_LastGlobalLight = global;
            m_LastPersonalLight = personal;

            ns.Send(GlobalLightLevel.Instantiate(global));
            ns.Send(new PersonalLightLevel(this, personal));
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GlobalLight { get { return m_LastGlobalLight; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PersonalLight { get { return m_LastPersonalLight; } }

        public override bool SendSpeedControl(SpeedControlType type)
        {
            AnimalFormContext context = AnimalForm.GetContext(this);

            if (context != null && context.SpeedBoost)
            {
                switch (type)
                {
                    case SpeedControlType.WalkSpeed: return base.SendSpeedControl(SpeedControlType.WalkSpeedFast);
                    case SpeedControlType.Disable: return base.SendSpeedControl(SpeedControlType.MountSpeed);
                }
            }

            return base.SendSpeedControl(type);
        }

        public override int GetMinResistance(ResistanceType type)
        {
            int magicResist = (int)(Skills[SkillName.MagicResist].Value * 10);
            int min = int.MinValue;

            if (magicResist >= 1000)
            {
                min = 40 + ((magicResist - 1000) / 50);
            }
            else if (magicResist >= 400)
            {
                min = (magicResist - 400) / 15;
            }

            return Math.Max(MinPlayerResistance, Math.Min(MaxPlayerResistance, min));
        }

        #region City Loyalty
        public override int GetResistance(ResistanceType type)
        {
            int resistance = base.GetResistance(type) + SphynxFortune.GetResistanceBonus(this, type);

            if (Server.Engines.CityLoyalty.CityLoyaltySystem.HasTradeDeal(this, Server.Engines.CityLoyalty.TradeDeal.SocietyOfClothiers))
            {
                resistance++;
                return Math.Min(resistance, GetMaxResistance(type));
            }

            return resistance;
        }
        #endregion

        public override void OnManaChange(int oldValue)
        {
            base.OnManaChange(oldValue);
            if (m_ExecutesLightningStrike > 0)
            {
                if (Mana < m_ExecutesLightningStrike)
                {
                    SpecialMove.ClearCurrentMove(this);
                }
            }
        }

        private static void OnLogin(LoginEventArgs e)
        {
            Mobile from = e.Mobile;

            CheckAtrophies(from);

            if (AccountHandler.LockdownLevel > AccessLevel.VIP)
            {
                string notice;

                Account acct = from.Account as Account;

                if (acct == null || !acct.HasAccess(from.NetState))
                {
                    if (from.IsPlayer())
                    {
                        notice = "The server is currently under lockdown. No players are allowed to log in at this time.";
                    }
                    else
                    {
                        notice = "The server is currently under lockdown. You do not have sufficient access level to connect.";
                    }

                    Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(Disconnect), from);
                }
                else if (from.AccessLevel >= AccessLevel.Administrator)
                {
                    notice =
                        "The server is currently under lockdown. As you are an administrator, you may change this from the [Admin gump.";
                }
                else
                {
                    notice = "The server is currently under lockdown. You have sufficient access level to connect.";
                }

                from.SendGump(new NoticeGump(1060637, 30720, notice, 0xFFC000, 300, 140, null, null));
                return;
            }

            if (from is PlayerMobile)
            {
                ((PlayerMobile)from).ClaimAutoStabledPets();
                ((PlayerMobile)from).ValidateEquipment();

                ReportMurdererGump.CheckMurderer(from);
            }
            else if (Siege.SiegeShard && from.Map == Map.Trammel && from.AccessLevel <= AccessLevel.VIP)
            {
                from.Map = Map.Felucca;
            }

            if (((from.Map == Map.Trammel && from.Region.IsPartOf("Blackthorn Castle")) || from.Region.IsPartOf("Ver Lor Reg")) && from.Player && from.AccessLevel <= AccessLevel.VIP && from.CharacterOut)
            {
                StormLevelGump menu = new StormLevelGump(from);
                menu.BeginClose();
                from.SendGump(menu);
            }

            if (from.NetState != null && from.NetState.IsEnhancedClient && from.Mount is EtherealMount)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(1), mount =>
                {
                    if (mount.IsChildOf(from.Backpack))
                    {
                        mount.Rider = from;
                    }
                },
                (EtherealMount)from.Mount);
            }

            if (from.NetState != null && from.Region != null)
            {
                if (from.Region.Music != Region.DEFAULT)
                    from.PlayGameMusic(from.Region.Music);
                else
                    from.DecideMusic(null, from.Region);
            }
        }

        private bool m_NoDeltaRecursion;

        public void ValidateEquipment()
        {
            if (m_NoDeltaRecursion || Map == null || Map == Map.Internal)
            {
                return;
            }

            if (Items == null)
            {
                return;
            }

            m_NoDeltaRecursion = true;
            Timer.DelayCall(TimeSpan.Zero, ValidateEquipment_Sandbox);
        }

        private void ValidateEquipment_Sandbox()
        {
            try
            {
                if (Map == null || Map == Map.Internal)
                {
                    return;
                }

                var items = Items;

                if (items == null)
                {
                    return;
                }

                bool moved = false;

                int str = Str;
                int dex = Dex;
                int intel = Int;

                #region Factions
                int factionItemCount = 0;
                #endregion

                Mobile from = this;

                #region Ethics
                Ethic ethic = Ethic.Find(from);
                #endregion

                for (int i = items.Count - 1; i >= 0; --i)
                {
                    if (i >= items.Count)
                    {
                        continue;
                    }

                    Item item = items[i];

                    #region Ethics
                    if ((item.SavedFlags & 0x100) != 0)
                    {
                        if (item.Hue != Ethic.Hero.Definition.PrimaryHue)
                        {
                            item.SavedFlags &= ~0x100;
                        }
                        else if (ethic != Ethic.Hero)
                        {
                            from.AddToBackpack(item);
                            moved = true;
                            continue;
                        }
                    }
                    else if ((item.SavedFlags & 0x200) != 0)
                    {
                        if (item.Hue != Ethic.Evil.Definition.PrimaryHue)
                        {
                            item.SavedFlags &= ~0x200;
                        }
                        else if (ethic != Ethic.Evil)
                        {
                            from.AddToBackpack(item);
                            moved = true;
                            continue;
                        }
                    }
                    #endregion

                    bool morph = from.FindItemOnLayer(Layer.Earrings) is MorphEarrings;

                    if (item is BaseWeapon)
                    {
                        BaseWeapon weapon = (BaseWeapon)item;

                        bool drop = false;

                        if (dex < weapon.DexRequirement)
                        {
                            drop = true;
                        }
                        else if (str < AOS.Scale(weapon.StrRequirement, 100 - weapon.GetLowerStatReq()))
                        {
                            drop = true;
                        }
                        else if (intel < weapon.IntRequirement)
                        {
                            drop = true;
                        }
                        else if (weapon.RequiredRace != null && weapon.RequiredRace != Race && !morph)
                        {
                            drop = true;
                        }

                        if (drop)
                        {
                            string name = weapon.Name;

                            if (name == null)
                            {
                                name = String.Format("#{0}", weapon.LabelNumber);
                            }

                            from.SendLocalizedMessage(1062001, name); // You can no longer wield your ~1_WEAPON~
                            from.AddToBackpack(weapon);
                            moved = true;
                        }
                    }
                    else if (item is BaseArmor)
                    {
                        BaseArmor armor = (BaseArmor)item;

                        bool drop = false;

                        if (!armor.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (!armor.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (armor.RequiredRace != null && armor.RequiredRace != Race && !morph)
                        {
                            drop = true;
                        }
                        else
                        {
                            int strBonus = armor.ComputeStatBonus(StatType.Str), strReq = armor.ComputeStatReq(StatType.Str);
                            int dexBonus = armor.ComputeStatBonus(StatType.Dex), dexReq = armor.ComputeStatReq(StatType.Dex);
                            int intBonus = armor.ComputeStatBonus(StatType.Int), intReq = armor.ComputeStatReq(StatType.Int);

                            if (dex < dexReq || (dex + dexBonus) < 1)
                            {
                                drop = true;
                            }
                            else if (str < strReq || (str + strBonus) < 1)
                            {
                                drop = true;
                            }
                            else if (intel < intReq || (intel + intBonus) < 1)
                            {
                                drop = true;
                            }
                        }

                        if (drop)
                        {
                            string name = armor.Name;

                            if (name == null)
                            {
                                name = String.Format("#{0}", armor.LabelNumber);
                            }

                            if (armor is BaseShield)
                            {
                                from.SendLocalizedMessage(1062003, name); // You can no longer equip your ~1_SHIELD~
                            }
                            else
                            {
                                from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~
                            }

                            from.AddToBackpack(armor);
                            moved = true;
                        }
                    }
                    else if (item is BaseClothing)
                    {
                        BaseClothing clothing = (BaseClothing)item;

                        bool drop = false;

                        if (!clothing.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        else if (!clothing.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
                        {
                            drop = true;
                        }
                        /*
                        else if (clothing.RequiredRace != null && clothing.RequiredRace != Race && !morph && clothing.RequiredRace != Race.Elf)
                        {
                            drop = true;
                        }
                        */
                        else
                        {
                            int strBonus = clothing.ComputeStatBonus(StatType.Str);
                            int strReq = clothing.ComputeStatReq(StatType.Str);

                            if (str < strReq || (str + strBonus) < 1)
                            {
                                drop = true;
                            }
                        }

                        if (drop)
                        {
                            string name = clothing.Name;

                            if (name == null)
                            {
                                name = String.Format("#{0}", clothing.LabelNumber);
                            }

                            from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~

                            from.AddToBackpack(clothing);
                            moved = true;
                        }
                    }
                    else if (item is BaseQuiver)
                    {
                        if (Race == Race.Gargoyle)
                        {
                            from.AddToBackpack(item);

                            from.SendLocalizedMessage(1062002, "quiver"); // You can no longer wear your ~1_ARMOR~
                            moved = true;
                        }
                    }

                    FactionItem factionItem = FactionItem.Find(item);

                    if (factionItem != null)
                    {
                        bool drop = false;

                        PlayerState state = PlayerState.Find(this);
                        Faction ourFaction = null;

                        if (state != null)
                            ourFaction = state.Faction;

                        if (ourFaction == null || ourFaction != factionItem.Faction)
                        {
                            drop = true;
                        }
                        else if (state != null && state.Rank.Rank < factionItem.MinRank)
                        {
                            drop = true;
                        }
                        else if (++factionItemCount > FactionItem.GetMaxWearables(this))
                        {
                            drop = true;
                        }

                        if (drop)
                        {
                            from.AddToBackpack(item);
                            moved = true;
                        }
                    }

                    #region Vice Vs Virtue
                    IVvVItem vvvItem = item as IVvVItem;

                    if (vvvItem != null && vvvItem.IsVvVItem && !Engines.VvV.ViceVsVirtueSystem.IsVvV(from))
                    {
                        from.AddToBackpack(item);
                        moved = true;
                    }
                    #endregion
                }

                if (moved)
                {
                    from.SendLocalizedMessage(500647); // Some equipment has been moved to your backpack.
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                m_NoDeltaRecursion = false;
            }
        }

        public override void Delta(MobileDelta flag)
        {
            base.Delta(flag);

            if ((flag & MobileDelta.Stat) != 0)
            {
                ValidateEquipment();
            }

            InvalidateProperties();
        }

        private static void Disconnect(object state)
        {
            NetState ns = ((Mobile)state).NetState;

            if (ns != null)
            {
                Shard.Debug("Kitou", ((Mobile)state));
                ns.Dispose();
            }
        }

        private static void OnLogout(LogoutEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            #region Scroll of Alacrity
            if (pm.AcceleratedStart > DateTime.UtcNow)
            {
                pm.AcceleratedStart = DateTime.UtcNow;
                ScrollOfAlacrity.AlacrityEnd(pm);
            }
            #endregion

            BaseFamiliar.OnLogout(pm);

            BaseEscort.DeleteEscort(pm);
        }

        private static void EventSink_Connected(ConnectedEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null)
            {
                pm.m_SessionStart = DateTime.UtcNow;

                if (pm.m_Quest != null)
                {
                    pm.m_Quest.StartTimer();
                }

                #region Mondain's Legacy
                QuestHelper.StartTimer(pm);
                #endregion

                pm.BedrollLogout = false;
                pm.BlanketOfDarknessLogout = false;
                pm.LastOnline = DateTime.UtcNow;
            }

            DisguiseTimers.StartTimer(e.Mobile);

            Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(ClearSpecialMovesCallback), e.Mobile);
        }

        private static void ClearSpecialMovesCallback(object state)
        {
            Mobile from = (Mobile)state;

            SpecialMove.ClearAllMoves(from);
        }

        private static void EventSink_Disconnected(DisconnectedEventArgs e)
        {
            Mobile from = e.Mobile;
            DesignContext context = DesignContext.Find(from);

            if (context != null)
            {
                /* Client disconnected
				*  - Remove design context
				*  - Eject all from house
				*  - Restore relocated entities
				*/
                // Remove design context
                DesignContext.Remove(from);

                // Eject all from house
                from.RevealingAction();

                foreach (Item item in context.Foundation.GetItems())
                {
                    item.Location = context.Foundation.BanLocation;
                }

                foreach (Mobile mobile in context.Foundation.GetMobiles())
                {
                    mobile.Location = context.Foundation.BanLocation;
                }

                // Restore relocated entities
                context.Foundation.RestoreRelocatedEntities();
            }

            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null)
            {
                pm.m_GameTime += (DateTime.UtcNow - pm.m_SessionStart);

                if (pm.m_Quest != null)
                {
                    pm.m_Quest.StopTimer();
                }

                #region Mondain's Legacy
                QuestHelper.StopTimer(pm);
                #endregion

                pm.m_SpeechLog = null;
                pm.LastOnline = DateTime.UtcNow;

                if (pm.Alive)
                    pm.AutoStablePets();
            }

            DisguiseTimers.StopTimer(from);
        }

        public override void RevealingAction(bool msg = true)
        {
            if (m_DesignContext != null)
            {
                return;
            }

            InvisibilitySpell.RemoveTimer(this);

            var wasHidden = Hidden;
            base.RevealingAction();
        }

        public override void OnHiddenChanged()
        {
            base.OnHiddenChanged();

            RemoveBuff(BuffIcon.Invisibility);
            //Always remove, default to the hiding icon EXCEPT in the invis spell where it's explicitly set

            if (!Hidden)
            {
                RemoveBuff(BuffIcon.HidingAndOrStealth);
            }
            else // if( !InvisibilitySpell.HasTimer( this ) )
            {
                BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655)); //Hidden/Stealthing & You Are Hidden
            }
        }

        public override void OnSubItemAdded(Item item)
        {
            if (AccessLevel < AccessLevel.GameMaster && item.IsChildOf(Backpack))
            {
                int curWeight = BodyWeight + TotalWeight;

                if (curWeight > MaxWeight)
                {
                    SendLocalizedMessage(1019035, true, String.Format(" : {0} / {1}", curWeight, MaxWeight));
                }
            }
        }

        public override void OnSubItemRemoved(Item item)
        {
            if (Server.Engines.UOStore.UltimaStore.HasPendingItem(this))
                Timer.DelayCall<PlayerMobile>(TimeSpan.FromSeconds(1.5), Server.Engines.UOStore.UltimaStore.CheckPendingItem, this);
        }

        public override void AggressiveAction(Mobile aggressor, bool criminal)
        {
            base.AggressiveAction(aggressor, criminal);

            if (aggressor is BaseCreature)
            {
                var bc = (BaseCreature)aggressor;

                if (bc.IsControlledBy(this))
                {
                    Shard.Debug("Meu bixo me batendo");
                    return;
                }

                if (bc.ControlMaster != null && bc.ControlMaster != this)
                {
                    Mobile aggressiveMaster = bc.ControlMaster;

                    if (NotorietyHandlers.CheckAggressor(Aggressors, aggressor))
                    {
                        Aggressors.Add(AggressorInfo.Create(aggressiveMaster, this, criminal));
                        aggressiveMaster.Delta(MobileDelta.Noto);

                        if (NotorietyHandlers.CheckAggressed(aggressor.Aggressed, this))
                            aggressiveMaster.Aggressed.Add(AggressorInfo.Create(aggressiveMaster, this, criminal));

                        if (aggressiveMaster is PlayerMobile || (aggressiveMaster is BaseCreature && !((BaseCreature)aggressiveMaster).IsMonster))
                        {
                            BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HeatOfBattleStatus, 1153801, 1153827, Aggression.CombatHeatDelay, this, true));
                            BuffInfo.AddBuff(aggressiveMaster, new BuffInfo(BuffIcon.HeatOfBattleStatus, 1153801, 1153827, Aggression.CombatHeatDelay, aggressiveMaster, true));
                        }
                    }
                }

            }
        }


        public override void DoHarmful(IDamageable damageable, bool indirect)
        {
            base.DoHarmful(damageable, indirect);

            if (ViceVsVirtueSystem.Enabled && (ViceVsVirtueSystem.EnhancedRules || Map == Faction.Facet) && damageable is Mobile)
            {
                ViceVsVirtueSystem.CheckHarmful(this, (Mobile)damageable);
            }

            if ((this.Criminal || this.Murderer) && this.Region is GuardedRegion)
            {
                var region = this.Region as GuardedRegion;
                if (region.IsGuardCandidate(this))
                    region.MakeGuard(this);
            }
        }

        public override int Tilematrix
        {
            get { return 0; }
            set
            {
                TileMatrix.GroundGate();
            }
        }

        public override void DoBeneficial(Mobile target)
        {
            base.DoBeneficial(target);

            if (ViceVsVirtueSystem.Enabled && (ViceVsVirtueSystem.EnhancedRules || Map == Faction.Facet) && target != null)
            {
                ViceVsVirtueSystem.CheckBeneficial(this, target);
            }
        }

        public override bool CanBeHarmful(IDamageable damageable, bool message, bool ignoreOurBlessedness)
        {
            Mobile target = damageable as Mobile;

            if (target is BaseCreature && !target.Deleted && target.Alive && !(((BaseCreature)target).ControlMaster is PlayerMobile))
            {
                if (this.Region is HouseRegion)
                {
                    if (target.Region is HouseRegion)
                    {
                        var h = BaseHouse.FindHouseAt(target);
                        if (h != null && h.Map != null)
                            target.MoveToWorld(h.BanLocation, h.Map);
                    }
                    return false;
                }
            }

            if (m_DesignContext != null || (target is PlayerMobile && ((PlayerMobile)target).m_DesignContext != null))
            {
                return false;
            }

            #region Mondain's Legacy
            if (Peaced)
            {
                //!+ TODO: message
                return false;
            }
            #endregion

            if ((target is BaseVendor && ((BaseVendor)target).IsInvulnerable) || target is PlayerVendor || target is TownCrier)
            {
                if (message)
                {
                    if (target.Title == null)
                    {
                        SendMessage("{0} the vendor cannot be harmed.", target.Name);
                    }
                    else
                    {
                        SendMessage("{0} {1} cannot be harmed.", target.Name, target.Title);
                    }
                }

                return false;
            }

            if (damageable is IDamageableItem && !((IDamageableItem)damageable).CanDamage)
            {
                if (message)
                    SendMessage("That cannot be harmed.");

                return false;
            }

            return base.CanBeHarmful(damageable, message, ignoreOurBlessedness);
        }

        public bool HitPronto = false;

        public override bool CanBeBeneficial(Mobile target, bool message, bool allowDead)
        {
            if (m_DesignContext != null || (target is PlayerMobile && ((PlayerMobile)target).m_DesignContext != null))
            {
                return false;
            }

            return base.CanBeBeneficial(target, message, allowDead);
        }

        public override bool CheckContextMenuDisplay(IEntity target)
        {
            return (m_DesignContext == null);
        }

        public override void OnItemAdded(Item item)
        {
            base.OnItemAdded(item);

            if (item is BaseArmor || item is BaseWeapon)
            {
                Hits = Hits;
                Stam = Stam;
                Mana = Mana;
            }

            if (NetState != null)
            {
                CheckLightLevels(false);
            }
        }

        private BaseWeapon m_LastWeapon;

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseWeapon LastWeapon { get { return m_LastWeapon; } set { m_LastWeapon = value; } }

        public override void OnItemRemoved(Item item)
        {
            base.OnItemRemoved(item);

            if (item is BaseArmor || item is BaseWeapon)
            {
                Hits = Hits;
                Stam = Stam;
                Mana = Mana;
            }

            if (item is BaseWeapon)
            {
                m_LastWeapon = item as BaseWeapon;
            }

            if (NetState != null)
            {
                CheckLightLevels(false);
            }
        }

        public override double ArmorRating
        {
            get
            {
                //BaseArmor ar;
                double rating = 0.0;

                AddArmorRating(ref rating, NeckArmor);
                AddArmorRating(ref rating, HandArmor);
                AddArmorRating(ref rating, HeadArmor);
                AddArmorRating(ref rating, ArmsArmor);
                AddArmorRating(ref rating, LegsArmor);
                AddArmorRating(ref rating, ChestArmor);
                AddArmorRating(ref rating, ShieldArmor);

                return VirtualArmor + VirtualArmorMod + rating;
            }
        }

        private void AddArmorRating(ref double rating, Item armor)
        {
            BaseArmor ar = armor as BaseArmor;

            if (ar != null && (!Core.AOS || ar.ArmorAttributes.MageArmor == 0))
            {
                rating += ar.ArmorRatingScaled;
            }
        }

        #region [Stats]Max
        [CommandProperty(AccessLevel.GameMaster)]
        public override int HitsMax
        {
            get
            {
                if (!Shard.POL_STYLE && !Shard.RP)
                    return 50 + (RawStr / 2);
                var stats = (RawStr + RawDex + RawInt) / 225d;
                var bonus = 0;
                if (Talentos.Tem(Talento.Perseveranca))
                    bonus = 40;
                return 70 + (int)(stats * 30) + bonus;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int StamMax
        {
            get
            {
                var bonus = 0;
                if (RP && Talentos.Tem(Talento.FisicoPerfeito))
                    bonus = 40;
                return Math.Max(10, (Shard.POL_STYLE ? StamPOL : base.ManaMax) + AosAttributes.GetValue(this, AosAttribute.BonusStam)) + bonus;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int ManaMax
        {
            get
            {
                var bonus = 0;
                if (RP && Talentos.Tem(Talento.Sabedoria))
                    bonus = 40;

                return (Shard.POL_STYLE ? ManaPOL : base.ManaMax) + AosAttributes.GetValue(this, AosAttribute.BonusMana) +
                       MasteryInfo.IntuitionBonus(this) +
                       UraliTranceTonic.GetManaBuff(this) + bonus;
            }
        }

        private int ManaPOL => (RawInt * 3 + RawStr) / 4;
        private int StamPOL => (RawDex * 3 + RawStr) / 4;
        #endregion

        #region Stat Getters/Setters
        [CommandProperty(AccessLevel.GameMaster)]
        public override int Str
        {
            get
            {
                if (Core.ML && IsPlayer())
                {
                    return Math.Min(base.Str, StrMaxCap);
                }

                return base.Str;
            }
            set { base.Str = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int Int
        {
            get
            {
                if (Core.ML && IsPlayer())
                {
                    return Math.Min(base.Int, IntMaxCap);
                }

                return base.Int;
            }
            set { base.Int = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int Dex
        {
            get
            {
                if (Core.ML && IsPlayer())
                {
                    return Math.Min(base.Dex, DexMaxCap);
                }

                return base.Dex;
            }
            set { base.Dex = value; }
        }
        #endregion

        public long NextPassiveDetectHidden { get; set; }

        public override bool Move(Direction d)
        {
            NetState ns = NetState;

            if (ns != null)
            {
                if (HasGump(typeof(ResurrectGump)))
                {
                    if (Alive)
                    {
                        CloseGump(typeof(ResurrectGump));
                    }
                    else
                    {
                        SendLocalizedMessage(500111); // You are frozen and cannot move.
                        return false;
                    }
                }
            }

            int speed = ComputeMovementSpeed(d);

            bool res;

            if (!Alive)
            {
                MovementImpl.IgnoreMovableImpassables = true;
            }

            res = base.Move(d);

            MovementImpl.IgnoreMovableImpassables = false;

            if (!res)
            {
                return false;
            }

            if(RP && Weapon is BaseWeapon w)
            {
                w.OnMovimento(this);
            }

            m_NextMovementTime += speed;

            if (!Siege.SiegeShard && Core.TickCount - NextPassiveDetectHidden >= 0)
            {
                DetectHidden.DoPassiveDetect(this);
                NextPassiveDetectHidden = Core.TickCount + (int)TimeSpan.FromSeconds(3).TotalMilliseconds;
            }
            return true;
        }

        public override bool CheckMovement(Direction d, out int newZ)
        {
            DesignContext context = m_DesignContext;

            if (context == null)
            {
                bool check = base.CheckMovement(d, out newZ);

                if (check && Sigil.ExistsOn(this, true) && !Server.Engines.VvV.VvVSigil.CheckMovement(this, d))
                {
                    SendLocalizedMessage(1155414); // You may not remove the sigil from the battle region!
                    return false;
                }

                return check;
            }

            HouseFoundation foundation = context.Foundation;

            newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

            int newX = X, newY = Y;
            Movement.Movement.Offset(d, ref newX, ref newY);

            int startX = foundation.X + foundation.Components.Min.X + 1;
            int startY = foundation.Y + foundation.Components.Min.Y + 1;
            int endX = startX + foundation.Components.Width - 1;
            int endY = startY + foundation.Components.Height - 2;

            return (newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map);
        }

        public override void OnHitsChange(int oldValue)
        {
            if (Race == Race.Gargoyle)
            {
                if (Hits <= HitsMax / 2)
                {
                    BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.Berserk, 1080449, 1115021, String.Format("{0}\t{1}", GetRacialBerserkBuff(false), GetRacialBerserkBuff(true)), false));
                    Delta(MobileDelta.WeaponDamage);
                }
                else if (oldValue < Hits && Hits > HitsMax / 2)
                {
                    BuffInfo.RemoveBuff(this, BuffIcon.Berserk);
                    Delta(MobileDelta.WeaponDamage);
                }
            }

            base.OnHitsChange(oldValue);
        }

        /// <summary>
        /// Returns Racial Berserk value, for spell or melee
        /// </summary>
        /// <param name="spell">true for spell damage, false for damage increase (melee)</param>
        /// <returns></returns>
        public virtual int GetRacialBerserkBuff(bool spell)
        {
            if (Race != Race.Gargoyle || Hits > HitsMax / 2)
                return 0;

            double perc = ((double)Hits / (double)HitsMax) * 100;
            int value = 0;

            perc = (100 - perc) / 20;

            if (perc > 4)
                value += spell ? 12 : 60;
            else if (perc >= 3)
                value += spell ? 9 : 45;
            else if (perc >= 2)
                value += spell ? 6 : 30;
            else if (perc >= 1)
                value += spell ? 3 : 15;

            return value;
        }

        public override void OnHeal(ref int amount, Mobile from)
        {
            base.OnHeal(ref amount, from);

            if (from == null)
                return;

            BestialSetHelper.OnHeal(this, from, ref amount);

            if (amount > 0 && from != null && from != this)
            {
                for (int i = Aggressed.Count - 1; i >= 0; i--)
                {
                    var info = Aggressed[i];

                    if (info.Defender.InRange(Location, Core.GlobalMaxUpdateRange) && info.Defender.DamageEntries.Any(de => de.Damager == this))
                    {
                        info.Defender.RegisterDamage(amount / 2, from);
                    }

                    if (info.Defender.Player && from.CanBeHarmful(info.Defender, false))
                    {
                        from.DoHarmful(info.Defender, true);
                    }
                }

                for (int i = Aggressors.Count - 1; i >= 0; i--)
                {
                    var info = Aggressors[i];

                    if (info.Attacker.InRange(Location, Core.GlobalMaxUpdateRange) && info.Attacker.DamageEntries.Any(de => de.Damager == this))
                    {
                        info.Attacker.RegisterDamage(amount / 2, from);
                    }

                    if (info.Attacker.Player && from.CanBeHarmful(info.Attacker, false))
                    {
                        from.DoHarmful(info.Attacker, true);
                    }
                }
            }
            DamageNumbers.ShowDamage(-amount, null, this, 0);
        }

        public void FreeEquip(Layer layer)
        {
            var equipped = FindItemOnLayer(layer);
            if (equipped != null)
            {
                RemoveItem(equipped);
                Backpack.AddItem(equipped);
            }
        }

        public bool SmoothForceEquip(Item item)
        {
            if (item.HueRaridade != 0)
            {
                item.HueRaridade = 0;
                item.ReleaseWorldPackets();
                item.Delta(ItemDelta.Update);
            }

            if (item is BaseWeapon)
            {
                if (item.Layer == Layer.TwoHanded)
                {
                    if (item is BaseShield)
                    {
                        FreeEquip(Layer.TwoHanded);
                    }
                    else
                    {
                        FreeEquip(Layer.TwoHanded);
                        FreeEquip(Layer.OneHanded);
                    }
                }
                else
                {
                    var escudo = FindItemOnLayer(Layer.TwoHanded);
                    if (escudo != null && escudo is BaseShield)
                    {
                        FreeEquip(Layer.OneHanded);
                    }
                    else
                    {
                        FreeEquip(Layer.TwoHanded);
                        FreeEquip(Layer.OneHanded);
                    }
                }
            }

            if (item is BaseArmor)
            {
                var ar = (BaseArmor)item;
                FreeEquip(ar.Layer);
            }
            else if (item is BaseClothing)
            {
                var ar = (BaseClothing)item;
                FreeEquip(ar.Layer);
            }
            Backpack.RemoveItem(item);
            if (!EquipItem(item))
            {
                Backpack.AddItem(item);
                return false;
            }
            PlaySound(0x57);
            return true;
        }

        public override void Use(Item item)
        {
            if (this.Frozen && !(item is OilCloth))
            {
                return;
            }

            if (Young && DicaBands && item is Bandage)
            {
                DicaBands = false;
            }


            if (this.Paralyzed)
            {
                if (item is Container)
                {
                    if (IsCooldown("trapmsg"))
                    {
                        SetCooldown("trapmsg");
                        SendMessage(78, "Para sair de paralize, use harm em si mesmo com timing bom ou pocao anti paralizia");
                    }
                    return;
                }
            }

            // AUTO EQUIP SPHERE
            if ((item is FishingPole || item is BaseClothing || item is BaseWeapon || item is BaseArmor && !(item is Dagger)) && item.IsChildOf(Backpack))
            {
                SmoothForceEquip(item);
                if (item is BaseAxe || item is BaseTool)
                    return;
            }

            if (item is Bandage)
            {
                BandageContext c = BandageContext.GetContext(this);
                if (c != null)
                {
                    SendMessage("Voce ja esta aplicando bandagens");
                    return;
                }
            }

            /*
            if (item.Parent != null && !(item is Container))
            {
                if (item.Parent is Corpse)
                {

                    if(this.IsCooldown("loot"))
                    {
                        return;
                    }

                    this.SetCooldown("loot", TimeSpan.FromMilliseconds(500));

                    if (SplitGold.Split(this, item))
                        return;

                    var corpse = (Corpse)item.Parent;
                    corpse.RemoveItem(item);
                    Backpack.AddItem(item);
                    PlaySound(0x57);
                    return;
                }
            }
            */

            base.Use(item);
        }

        public override bool AllowItemUse(Item item)
        {
            return DesignContext.Check(this);
        }

        public SkillName[] AnimalFormRestrictedSkills { get { return m_AnimalFormRestrictedSkills; } }

        private readonly SkillName[] m_AnimalFormRestrictedSkills = new[]
        {
            SkillName.ArmsLore, SkillName.Begging, SkillName.Discordance, SkillName.Forensics, SkillName.Inscribe,
            SkillName.ItemID, SkillName.Meditation, SkillName.Peacemaking, SkillName.Provocation, SkillName.RemoveTrap,
            SkillName.SpiritSpeak, SkillName.Stealing, SkillName.TasteID
        };

        public override bool AllowSkillUse(SkillName skill)
        {
            if (AnimalForm.UnderTransformation(this))
            {
                for (int i = 0; i < m_AnimalFormRestrictedSkills.Length; i++)
                {
                    if (m_AnimalFormRestrictedSkills[i] == skill)
                    {
                        #region Mondain's Legacy
                        AnimalFormContext context = AnimalForm.GetContext(this);

                        if (skill == SkillName.Stealing && context.StealingMod != null && context.StealingMod.Value > 0)
                        {
                            continue;
                        }
                        #endregion

                        SendLocalizedMessage(1070771); // You cannot use that skill in this form.
                        return false;
                    }
                }
            }

            return DesignContext.Check(this);
        }

        private bool m_LastProtectedMessage;
        private int m_NextProtectionCheck = 10;

        public virtual void RecheckTownProtection()
        {
            m_NextProtectionCheck = 10;

            GuardedRegion reg = (GuardedRegion)Region.GetRegion(typeof(GuardedRegion));
            bool isProtected = (reg != null && !reg.IsDisabled());

            if (isProtected != m_LastProtectedMessage)
            {
                if (isProtected)
                {
                    SendMessage("Voce esta em uma regiao protegida pelos guardas"); // You are now under the protection of the town guards.
                }
                else
                {
                    SendMessage("Voce saiu da protecao dos guardas"); // You have left the protection of the town guards.
                }
                m_LastProtectedMessage = isProtected;
            }
        }

        public override void MoveToWorld(Point3D loc, Map map)
        {
            base.MoveToWorld(loc, map);

            if (Wisp != null)
            {
                Wisp.MoveToWorld(loc, map);
            }

            RecheckTownProtection();

            if (NetState != null && Region != null)
                PlayGameMusic(Region.Music);
        }

        public override void SetLocation(Point3D loc, bool isTeleport)
        {
            if (!isTeleport && IsPlayer() && !Flying)
            {
                // moving, not teleporting
                int zDrop = (Location.Z - loc.Z);

                if (zDrop > 20) // we fell more than one story
                {
                    Hits -= ((zDrop / 20) * 10) - 5; // deal some damage; does not kill, disrupt, etc
                    SendMessage("Ouch!");
                }
            }

            base.SetLocation(loc, isTeleport);

            if (isTeleport || --m_NextProtectionCheck == 0)
            {
                RecheckTownProtection();
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from == this)
            {
                if (Core.HS && Alive)
                {
                    list.Add(new SearchVendors(this));
                }

                BaseHouse house = BaseHouse.FindHouseAt(this);

                if (house != null)
                {
                    if (house.IsCoOwner(this))
                    {
                        list.Add(new CallbackEntry(6205, ReleaseCoOwnership));
                    }
                }

                if (Core.SA)
                {
                    list.Add(new TitlesMenuEntry(this));

                    if (Alive)
                    {
                        QuestHelper.GetContextMenuEntries(list);
                    }
                }
                else if (Core.ML)
                {
                    #region Mondains Legacy
                    if (Alive)
                    {
                        QuestHelper.GetContextMenuEntries(list);

                        if (m_RewardTitles.Count > 0)
                        {
                            list.Add(new CallbackEntry(6229, ShowChangeTitle));
                        }
                    }
                    #endregion
                }

                if (m_Quest != null)
                {
                    m_Quest.GetContextMenuEntries(list);
                }

                if (Alive && Core.SA)
                {
                    list.Add(new Engines.Points.LoyaltyRating(this));
                }

                if (Alive && InsuranceEnabled)
                {
                    list.Add(new CallbackEntry(6201, ToggleItemInsurance));

                    if (Core.SA)
                    {
                        list.Add(new CallbackEntry(1114299, OpenItemInsuranceMenu));
                    }
                    else if (AutoRenewInsurance)
                    {
                        list.Add(new CallbackEntry(6202, CancelRenewInventoryInsurance));
                    }
                    else
                    {
                        list.Add(new CallbackEntry(6200, AutoRenewInventoryInsurance));
                    }
                }
                else if (Siege.SiegeShard)
                {
                    list.Add(new CallbackEntry(3006168, SiegeBlessItem));
                }

                if (house != null)
                {
                    if (Alive && house.InternalizedVendors.Count > 0 && house.IsOwner(this))
                    {
                        list.Add(new CallbackEntry(6204, GetVendor));
                    }

                    if (house.IsAosRules)
                    {
                        list.Add(new CallbackEntry(6207, LeaveHouse));
                    }
                }

                if (Core.HS)
                {
                    list.Add(new CallbackEntry(RefuseTrades ? 1154112 : 1154113, ToggleTrades)); // Allow Trades / Refuse Trades				
                }

                if (m_JusticeProtectors.Count > 0)
                {
                    list.Add(new CallbackEntry(6157, CancelProtection));
                }

                #region Void Pool
                if (VoidPool || Region.IsPartOf<VoidPoolRegion>())
                {
                    var controller = Map == Map.Felucca ? VoidPoolController.InstanceFel : VoidPoolController.InstanceTram;

                    if (controller != null)
                    {
                        if (!VoidPool)
                        {
                            VoidPool = true;
                        }

                        list.Add(new VoidPoolInfo(this, controller));
                    }
                }
                #endregion

                #region TOL Shadowguard
                if (ShadowguardController.GetInstance(Location, Map) != null)
                {
                    list.Add(new ExitEntry(this));
                }
                #endregion

                if (Core.UOR && !Core.SA && Alive)
                {
                    list.Add(new CallbackEntry(6210, ToggleChampionTitleDisplay));
                }

                if (DisabledPvpWarning)
                {
                    list.Add(new CallbackEntry(1113797, EnablePvpWarning));
                }
            }
            else
            {
                if (Alive && Core.AOS)
                {
                    Party theirParty = from.Party as Party;
                    Party ourParty = Party as Party;

                    if (theirParty == null && ourParty == null)
                    {
                        list.Add(new AddToPartyEntry(from, this));
                    }
                    else if (theirParty != null && theirParty.Leader == from)
                    {
                        if (ourParty == null)
                        {
                            list.Add(new AddToPartyEntry(from, this));
                        }
                        else if (ourParty == theirParty)
                        {
                            list.Add(new RemoveFromPartyEntry(from, this));
                        }
                    }
                }

                if (Core.TOL && from.InRange(this, 10))
                {
                    list.Add(new CallbackEntry(1077728, () => OpenTrade(from))); // Trade
                }

                BaseHouse curhouse = BaseHouse.FindHouseAt(this);

                if (curhouse != null)
                {
                    if (Alive && Core.AOS && curhouse.IsAosRules && curhouse.IsFriend(from))
                    {
                        list.Add(new EjectPlayerEntry(from, this));
                    }
                }
            }
        }

        private void CancelProtection()
        {
            for (int i = 0; i < m_JusticeProtectors.Count; ++i)
            {
                Mobile prot = m_JusticeProtectors[i];

                string args = String.Format("{0}\t{1}", Name, prot.Name);

                prot.SendLocalizedMessage(1049371, args);
                // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
                SendLocalizedMessage(1049371, args);
                // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
            }

            m_JusticeProtectors.Clear();
        }

        #region Insurance
        private void ToggleItemInsurance()
        {
            if (!CheckAlive())
            {
                return;
            }

            BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);
            SendLocalizedMessage(1060868); // Target the item you wish to toggle insurance status on <ESC> to cancel
        }

        private bool CanInsure(Item item)
        {
            if (((item is Container) && !(item is BaseQuiver)) || item is BagOfSending || item is KeyRing || item is MountItem)
            {
                return false;
            }

            if ((item is Spellbook && item.LootType == LootType.Blessed) || item is Runebook || item is PotionKeg ||
                item is Sigil)
            {
                return false;
            }

            if (item is BaseBalmOrLotion || item is GemOfSalvation || item is SeedOfLife || item is ManaDraught)
            {
                return false;
            }

            if (item.Stackable)
            {
                return false;
            }

            if (item.LootType == LootType.Cursed)
            {
                return false;
            }

            if (item.ItemID == 0x204E) // death shroud
            {
                return false;
            }

            if (item.LootType == LootType.Blessed)
                return false;

            if (item is BaseJewel || item is BaseClothing || item is BaseTalisman || item is BaseGlovesOfMining)
                return true;

            return false;
        }

        private void ToggleItemInsurance_Callback(Mobile from, object obj)
        {
            if (!CheckAlive())
                return;

            ToggleItemInsurance_Callback(from, obj as Item, true);
        }

        private void ToggleItemInsurance_Callback(Mobile from, Item item, bool target)
        {
            if (item == null || !item.IsChildOf(this))
            {
                if (target)
                    BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));

                SendLocalizedMessage(1060871, "", 0x23); // You can only insure items that you have equipped or that are in your backpack
            }
            else if (item.Insured)
            {
                item.Insured = false;

                SendLocalizedMessage(1060874, "", 0x35); // You cancel the insurance on the item

                if (target)
                {
                    BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
                    SendLocalizedMessage(1060868, "", 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
                }
            }
            else if (!CanInsure(item))
            {
                if (target)
                    BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));

                SendLocalizedMessage(1060869, "", 0x23); // You cannot insure that
            }
            else
            {
                if (!item.PayedInsurance)
                {
                    int cost = GetInsuranceCost(item);

                    if (Banker.Withdraw(from, cost))
                    {
                        SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
                        item.PayedInsurance = true;
                    }
                    else
                    {
                        SendLocalizedMessage(1061079, "", 0x23); // You lack the funds to purchase the insurance
                        return;
                    }
                }

                item.Insured = true;

                SendLocalizedMessage(1060873, "", 0x23); // You have insured the item

                if (target)
                {
                    BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
                    SendLocalizedMessage(1060868, "", 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
                }
            }
        }

        public int GetInsuranceCost(Item item)
        {
            if (item is BaseTalisman)
                return 4000;

            if (item is BaseHat)
                return 1000;

            return 3000;
        }

        private void AutoRenewInventoryInsurance()
        {
            if (!CheckAlive())
            {
                return;
            }

            SendLocalizedMessage(1060881, "", 0x23); // You have selected to automatically reinsure all insured items upon death
            AutoRenewInsurance = true;
        }

        private void CancelRenewInventoryInsurance()
        {
            if (!CheckAlive())
            {
                return;
            }

            if (Core.SE)
            {
                if (!HasGump(typeof(CancelRenewInventoryInsuranceGump)))
                {
                    SendGump(new CancelRenewInventoryInsuranceGump(this, null));
                }
            }
            else
            {
                SendLocalizedMessage(1061075, "", 0x23); // You have cancelled automatically reinsuring all insured items upon death
                AutoRenewInsurance = false;
            }
        }

        #region Siege Bless Item
        private Item _BlessedItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public Item BlessedItem { get { return _BlessedItem; } set { _BlessedItem = value; } }

        private void SiegeBlessItem()
        {
            if (_BlessedItem != null && _BlessedItem.Deleted)
                _BlessedItem = null;

            BeginTarget(2, false, TargetFlags.None, (from, targeted) =>
            {
                Siege.TryBlessItem(this, targeted);
            });
        }

        public override bool Drop(Point3D loc)
        {
            if (!Siege.SiegeShard || _BlessedItem == null)
                return base.Drop(loc);

            Item item = Holding;
            bool drop = base.Drop(loc);

            if (item != null && drop && item.Parent == null && _BlessedItem != null && _BlessedItem == item)
            {
                _BlessedItem = null;
                item.LootType = LootType.Regular;

                SendLocalizedMessage(1075292, item.Name != null ? item.Name : "#" + item.LabelNumber.ToString()); // ~1_NAME~ has been unblessed.
            }

            return drop;
        }
        #endregion

        private class CancelRenewInventoryInsuranceGump : Gump
        {
            private readonly PlayerMobile m_Player;
            private readonly ItemInsuranceMenuGump m_InsuranceGump;

            public CancelRenewInventoryInsuranceGump(PlayerMobile player, ItemInsuranceMenuGump insuranceGump)
                : base(250, 200)
            {
                m_Player = player;
                m_InsuranceGump = insuranceGump;

                AddBackground(0, 0, 240, 142, 0x13BE);
                AddImageTiled(6, 6, 228, 100, 0xA40);
                AddImageTiled(6, 116, 228, 20, 0xA40);
                AddAlphaRegion(6, 6, 228, 142);

                AddHtmlLocalized(8, 8, 228, 100, 1071021, 0x7FFF, false, false);
                // You are about to disable inventory insurance auto-renewal.

                AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

                AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(148, 118, 450, 20, 1071022, 0x7FFF, false, false); // DISABLE IT!
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (!m_Player.CheckAlive())
                {
                    return;
                }

                if (info.ButtonID == 1)
                {
                    m_Player.SendLocalizedMessage(1061075, "", 0x23);
                    // You have cancelled automatically reinsuring all insured items upon death
                    m_Player.AutoRenewInsurance = false;
                }
                else
                {
                    m_Player.SendLocalizedMessage(1042021); // Cancelled.
                }

                if (m_InsuranceGump != null)
                    m_Player.SendGump(m_InsuranceGump.NewInstance());
            }
        }

        public void OpenItemInsuranceMenu()
        {
            if (!CheckAlive())
                return;

            List<Item> items = new List<Item>();

            foreach (Item item in Items)
            {
                if (DisplayInItemInsuranceGump(item))
                    items.Add(item);
            }

            Container pack = Backpack;

            if (pack != null)
                items.AddRange(pack.FindItemsByType<Item>(true, DisplayInItemInsuranceGump));

            // TODO: Investigate item sorting

            CloseGump(typeof(ItemInsuranceMenuGump));

            if (items.Count == 0)
                SendLocalizedMessage(1114915, "", 0x35); // None of your current items meet the requirements for insurance.
            else
                SendGump(new ItemInsuranceMenuGump(this, items.ToArray()));
        }

        private bool DisplayInItemInsuranceGump(Item item)
        {
            if (item.Parent is LockableContainer && ((LockableContainer)item.Parent).Locked)
                return false;

            return ((item.Visible || AccessLevel >= AccessLevel.GameMaster) && (item.Insured || CanInsure(item)));
        }

        private class ItemInsuranceMenuGump : Gump
        {
            private PlayerMobile m_From;
            private Item[] m_Items;
            private bool[] m_Insure;
            private int m_Page;

            public ItemInsuranceMenuGump(PlayerMobile from, Item[] items)
                : this(from, items, null, 0)
            {
            }

            public ItemInsuranceMenuGump(PlayerMobile from, Item[] items, bool[] insure, int page)
                : base(25, 50)
            {
                m_From = from;
                m_Items = items;

                if (insure == null)
                {
                    insure = new bool[items.Length];

                    for (int i = 0; i < items.Length; ++i)
                        insure[i] = items[i].Insured;
                }

                m_Insure = insure;
                m_Page = page;

                AddPage(0);

                AddBackground(0, 0, 520, 510, 9200);
                AddBackground(10, 10, 500, 30, 3000);
                AddBackground(10, 50, 500, 355, 3000);
                AddBackground(10, 415, 500, 80, 3000);
                AddAlphaRegion(10, 10, 500, 485);

                AddButton(15, 470, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(50, 472, 80, 20, 1011012, 0, false, false); // CANCEL

                if (from.AutoRenewInsurance)
                    AddButton(360, 10, 9723, 9724, 1, GumpButtonType.Reply, 0);
                else
                    AddButton(360, 10, 9720, 9722, 1, GumpButtonType.Reply, 0);

                AddHtmlLocalized(395, 14, 105, 20, 1114122, 0, false, false); // AUTO REINSURE

                AddButton(395, 470, 0xFA5, 0xFA6, 2, GumpButtonType.Reply, 0);
                AddHtmlLocalized(430, 472, 50, 20, 1006044, 0, false, false); // OK

                AddHtml(10, 14, 150, 20, "<CENTER>Menu de Insurance</CENTER>", 0, false, false); // <CENTER>ITEM INSURANCE MENU</CENTER>

                AddHtmlLocalized(45, 54, 70, 20, 1062214, 0, false, false); // Item
                AddHtmlLocalized(250, 54, 70, 20, 1061038, 0, false, false); // Cost
                AddHtmlLocalized(400, 54, 70, 20, 1114311, 0, false, false); // Insured

                int balance = Banker.GetBalance(from);
                int cost = 0;

                for (int i = 0; i < items.Length; ++i)
                {
                    if (insure[i])
                        cost += m_From.GetInsuranceCost(items[i]);
                }

                AddHtmlLocalized(15, 420, 300, 20, 1114310, 0, false, false); // GOLD AVAILABLE:
                AddLabel(215, 420, 0x481, balance.ToString());
                AddHtmlLocalized(15, 435, 300, 20, 1114123, 0, false, false); // TOTAL COST OF INSURANCE:
                AddLabel(215, 435, 0x481, cost.ToString());

                if (cost != 0)
                {
                    AddHtml(15, 450, 300, 20, "Mortes que voce pode pagar", 0, false, false); // NUMBER OF DEATHS PAYABLE:
                    AddLabel(215, 450, 0x481, (balance / cost).ToString());
                }

                for (int i = page * 4, y = 72; i < (page + 1) * 4 && i < items.Length; ++i, y += 75)
                {
                    Item item = items[i];
                    Rectangle2D b = ItemBounds.Table[item.ItemID];

                    AddImageTiledButton(40, y, 0x918, 0x918, 0, GumpButtonType.Page, 0, item.ItemID, item.Hue, 40 - b.Width / 2 - b.X, 30 - b.Height / 2 - b.Y);
                    AddItemProperty(item.Serial);

                    if (insure[i])
                    {
                        AddButton(400, y, 9723, 9724, 100 + i, GumpButtonType.Reply, 0);
                        AddLabel(250, y, 0x481, m_From.GetInsuranceCost(item).ToString());
                    }
                    else
                    {
                        AddButton(400, y, 9720, 9722, 100 + i, GumpButtonType.Reply, 0);
                        AddLabel(250, y, 0x66C, m_From.GetInsuranceCost(item).ToString());
                    }
                }

                if (page >= 1)
                {
                    AddButton(15, 380, 0xFAE, 0xFAF, 3, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(50, 380, 450, 20, 1044044, 0x7FFF, false, false); // PREV PAGE
                }

                if ((page + 1) * 4 < items.Length)
                {
                    AddButton(400, 380, 0xFA5, 0xFA7, 4, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(435, 380, 70, 20, 1044045, 0x7FFF, false, false); // NEXT PAGE
                }
            }

            public ItemInsuranceMenuGump NewInstance()
            {
                return new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page);
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (info.ButtonID == 0 || !m_From.CheckAlive())
                    return;

                switch (info.ButtonID)
                {
                    case 1: // Auto Reinsure
                        {
                            if (m_From.AutoRenewInsurance)
                            {
                                if (!m_From.HasGump(typeof(CancelRenewInventoryInsuranceGump)))
                                    m_From.SendGump(new CancelRenewInventoryInsuranceGump(m_From, this));
                            }
                            else
                            {
                                m_From.AutoRenewInventoryInsurance();
                                m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));
                            }

                            break;
                        }
                    case 2: // OK
                        {
                            m_From.SendGump(new ItemInsuranceMenuConfirmGump(m_From, m_Items, m_Insure, m_Page));

                            break;
                        }
                    case 3: // Prev
                        {
                            if (m_Page >= 1)
                                m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page - 1));

                            break;
                        }
                    case 4: // Next
                        {
                            if ((m_Page + 1) * 4 < m_Items.Length)
                                m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page + 1));

                            break;
                        }
                    default:
                        {
                            int idx = info.ButtonID - 100;

                            if (idx >= 0 && idx < m_Items.Length)
                                m_Insure[idx] = !m_Insure[idx];

                            m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));

                            break;
                        }
                }
            }
        }

        private class ItemInsuranceMenuConfirmGump : Gump
        {
            private PlayerMobile m_From;
            private Item[] m_Items;
            private bool[] m_Insure;
            private int m_Page;

            public ItemInsuranceMenuConfirmGump(PlayerMobile from, Item[] items, bool[] insure, int page)
                : base(250, 200)
            {
                m_From = from;
                m_Items = items;
                m_Insure = insure;
                m_Page = page;

                AddBackground(0, 0, 240, 142, 0x13BE);
                AddImageTiled(6, 6, 228, 100, 0xA40);
                AddImageTiled(6, 116, 228, 20, 0xA40);
                AddAlphaRegion(6, 6, 228, 142);

                AddHtmlLocalized(8, 8, 228, 100, 1114300, 0x7FFF, false, false); // Do you wish to insure all newly selected items?

                AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

                AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(148, 118, 450, 20, 1073996, 0x7FFF, false, false); // ACCEPT
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (!m_From.CheckAlive())
                    return;

                if (info.ButtonID == 1)
                {
                    for (int i = 0; i < m_Items.Length; ++i)
                    {
                        Item item = m_Items[i];

                        if (item.Insured != m_Insure[i])
                            m_From.ToggleItemInsurance_Callback(m_From, item, false);
                    }
                }
                else
                {
                    m_From.SendLocalizedMessage(1042021); // Cancelled.
                    m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));
                }
            }
        }

        #endregion

        private void ToggleTrades()
        {
            RefuseTrades = !RefuseTrades;
        }

        private void GetVendor()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (CheckAlive() && house != null && house.IsOwner(this) && house.InternalizedVendors.Count > 0)
            {
                CloseGump(typeof(ReclaimVendorGump));
                SendGump(new ReclaimVendorGump(house));
            }
        }

        private void LeaveHouse()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null)
            {
                Location = house.BanLocation;
            }
        }

        private void ReleaseCoOwnership()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null && house.IsCoOwner(this))
            {
                SendGump(new WarningGump(1060635, 30720, 1062006, 32512, 420, 280, new WarningGumpCallback(ClearCoOwners_Callback), house));
            }
        }

        public void ClearCoOwners_Callback(Mobile from, bool okay, object state)
        {
            BaseHouse house = (BaseHouse)state;

            if (house.Deleted)
                return;

            if (okay && house.IsCoOwner(from))
            {
                if (house.CoOwners != null)
                    house.CoOwners.Remove(from);

                from.SendLocalizedMessage(501300); // You have been removed as a house co-owner.
            }
        }

        private void EnablePvpWarning()
        {
            DisabledPvpWarning = false;
            SendLocalizedMessage(1113798); // Your PvP warning query has been re-enabled.
        }

        private delegate void ContextCallback();

        private class CallbackEntry : ContextMenuEntry
        {
            private readonly ContextCallback m_Callback;

            public CallbackEntry(int number, ContextCallback callback)
                : this(number, -1, callback)
            { }

            public CallbackEntry(int number, int range, ContextCallback callback)
                : base(number, range)
            {
                m_Callback = callback;
            }

            public override void OnClick()
            {
                if (m_Callback != null)
                {
                    m_Callback();
                }
            }
        }

        public override void OnAosSingleClick(Mobile from)
        {
            if (ViewOPL)
            {
                base.OnAosSingleClick(from);
            }
            else if (from.Region.OnSingleClick(from, this))
            {
                OnSingleClick(from);
            }
        }

        public override void DisruptiveAction(bool cancelMedit = true)
        {
            if (cancelMedit && Meditating)
            {
                RemoveBuff(BuffIcon.ActiveMeditation);
            }

            base.DisruptiveAction(cancelMedit);
        }

        public override bool Meditating
        {
            set
            {
                base.Meditating = value;
                if (value == false)
                {
                    RemoveBuff(BuffIcon.ActiveMeditation);
                    if (Mana == ManaMax)
                    {
                        PrivateOverheadMessage("* Você está em paz *");
                    }
                }
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (this == from && !Warmode)
            {
                IMount mount = Mount;

                if (mount != null && !DesignContext.Check(this))
                {
                    return;
                }
            }
            base.OnDoubleClick(from);
        }

        public override void DisplayPaperdollTo(Mobile to)
        {
            if (DesignContext.Check(this))
            {
                base.DisplayPaperdollTo(to);
            }
        }

        private static bool m_NoRecursion;

        public override bool CheckEquip(Item item)
        {
            if (!base.CheckEquip(item))
            {
                return false;
            }

            Region r = Region.Find(Location, Map);

            if (r is Server.Engines.ArenaSystem.ArenaRegion)
            {
                if (!((Server.Engines.ArenaSystem.ArenaRegion)r).AllowItemEquip(this, item))
                {
                    return false;
                }
            }

            #region Factions
            FactionItem factionItem = FactionItem.Find(item);

            if (factionItem != null)
            {
                PlayerState state = PlayerState.Find(this);
                Faction faction = null;

                if (state != null)
                {
                    faction = state.Faction;
                }

                if (faction == null)
                {
                    SendLocalizedMessage(1010371); // You cannot equip a faction item!
                    return false;
                }
                else if (faction != factionItem.Faction)
                {
                    SendLocalizedMessage(1010372); // You cannot equip an opposing faction's item!
                    return false;
                }
                else if (state != null && state.Rank.Rank < factionItem.MinRank)
                {
                    SendLocalizedMessage(1094804); // You are not high enough in rank to equip this item.
                    return false;
                }
                else
                {
                    int maxWearables = FactionItem.GetMaxWearables(this);

                    for (int i = 0; i < Items.Count; ++i)
                    {
                        Item equiped = Items[i];

                        if (item != equiped && FactionItem.Find(equiped) != null)
                        {
                            if (--maxWearables == 0)
                            {
                                SendLocalizedMessage(1010373); // You do not have enough rank to equip more faction items!
                                return false;
                            }
                        }
                    }
                }
            }
            #endregion

            #region Vice Vs Virtue
            IVvVItem vvvItem = item as IVvVItem;

            if (vvvItem != null && vvvItem.IsVvVItem && !Engines.VvV.ViceVsVirtueSystem.IsVvV(this))
            {
                return false;
            }
            #endregion

            if (AccessLevel < AccessLevel.GameMaster && item.Layer != Layer.Mount && HasTrade)
            {
                BounceInfo bounce = item.GetBounce();

                if (bounce != null)
                {
                    if (bounce.m_Parent is Item)
                    {
                        Item parent = (Item)bounce.m_Parent;

                        if (parent == Backpack || parent.IsChildOf(Backpack))
                        {
                            return true;
                        }
                    }
                    else if (bounce.m_Parent == this)
                    {
                        return true;
                    }
                }

                SendLocalizedMessage(1004042); // You can only equip what you are already carrying while you have a trade pending.
                return false;
            }

            return true;
        }

        public List<Type> PartyLoots = new List<Type>();

        public override bool OnDragLift(Item item)
        {

            if (item.RootParent != this && item.RootParent != this.Backpack)
            {
                RevealingAction();
            }

            if (item.HueRaridade != 0)
            {
                item.HueRaridade = 0;
                item.ReleaseWorldPackets();
                item.Delta(ItemDelta.Update);
            }

            Shard.Debug("Drag Lift");
            if (item is IPromotionalToken && ((IPromotionalToken)item).GumpType != null)
            {
                Type t = ((IPromotionalToken)item).GumpType;

                if (HasGump(t))
                    CloseGump(t);
            }


            if (item.BoundTo != null && item.BoundTo != this.RawName)
            {
                SendMessage(38, "Isto nao pertence a voce");
                return false;
            }

            if (item.BoundTo == null && item.PartyLoot && !item.IsChildOf(this.Backpack) && !item.IsChildOf(this))
            {

                if (item.Parent is Bag)
                {
                    SendMessage("Items de grupo em sacolas nao podem ser arrastados. Arraste a sacola !");
                    return false;
                }

                if (!PartyLoots.Contains(item.GetType()))
                {
                    PartyLoots.Add(item.GetType());
                    var copia = Dupe.DupeItem(item);
                    copia.PartyLoot = true;
                    item.PartyLoot = false;
                    if (item.LootType == LootType.Blessed)
                        item.BoundTo = this.RawName;

                    if (item.Parent is Mobile)
                    {
                        ((Mobile)item.Parent).AddItem(copia);
                    }
                    else if (item.Parent is Container)
                    {
                        ((Container)item.Parent).DropItem(copia);
                    }
                }
                else
                {
                    SendMessage(38, "Voce ja pegou este item");
                    if (item.Parent is Container && item.Parent != null)
                    {
                        ((Container)item.Parent).RandomizeLocation(item);
                    }
                    return false;
                }
            }

            if (Shard.RP)
            {
                if (!item.IsChildOf(this.Backpack) && this.FindItemOnLayer(item.Layer) != item)
                {
                    var name = item.Name;
                    if (name == "" || name == null)
                    {
                        name = ClilocLNG.ENU.GetString(item.GetType());
                    }
                    PublicOverheadMessage(MessageType.Emote, 0, false, "* pegou " + name + " *");
                }
            }

            return base.OnDragLift(item);
        }

        public override bool CheckTrade(
            Mobile to, Item item, SecureTradeContainer cont, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            int msgNum = 0;

            if (_BlessedItem != null && _BlessedItem == item)
            {
                msgNum = 1075282; // You cannot trade a blessed item.
            }

            if (msgNum == 0 && cont == null)
            {
                if (to.Holding != null)
                {
                    msgNum = 1062727; // You cannot trade with someone who is dragging something.
                }
                else if (HasTrade)
                {
                    msgNum = 1062781; // You are already trading with someone else!
                }
                else if (to.HasTrade)
                {
                    msgNum = 1062779; // That person is already involved in a trade
                }
                else if (to is PlayerMobile && ((PlayerMobile)to).RefuseTrades)
                {
                    msgNum = 1154111; // ~1_NAME~ is refusing all trades.
                }
                else if (item is IFactionItem && ((IFactionItem)item).FactionItemState != null)
                {
                    msgNum = 1094803; // This faction reward is bound to you, and cannot be traded.
                }
            }

            if (msgNum == 0 && item != null)
            {
                if (cont != null)
                {
                    plusItems += cont.TotalItems;
                    plusWeight += cont.TotalWeight;
                }

                if (Backpack == null || !Backpack.CheckHold(this, item, false, checkItems, plusItems, plusWeight))
                {
                    msgNum = 1004040; // You would not be able to hold this if the trade failed.
                }
                else if (to.Backpack == null || !to.Backpack.CheckHold(to, item, false, checkItems, plusItems, plusWeight))
                {
                    msgNum = 1004039; // The recipient of this trade would not be able to carry 
                }
                else
                {
                    msgNum = CheckContentForTrade(item);
                }
            }

            if (msgNum == 0)
            {
                return true;
            }

            if (!message)
            {
                return false;
            }

            if (msgNum == 1154111)
            {
                if (to != null)
                {
                    SendLocalizedMessage(msgNum, to.Name);
                }
            }
            else
            {
                SendLocalizedMessage(msgNum);
            }

            return false;
        }

        private static int CheckContentForTrade(Item item)
        {
            if (item is TrapableContainer && ((TrapableContainer)item).TrapType != TrapType.None)
            {
                return 1004044; // You may not trade trapped items.
            }

            if (StolenItem.IsStolen(item))
            {
                return 1004043; // You may not trade recently stolen items.
            }

            if (item is Container)
            {
                foreach (Item subItem in item.Items)
                {
                    int msg = CheckContentForTrade(subItem);

                    if (msg != 0)
                    {
                        return msg;
                    }
                }
            }

            return 0;
        }

        public override bool CheckNonlocalLift(Mobile from, Item item)
        {
            if (from.IsControlledBy(this))
            {
                //return true;
            }
            return base.CheckNonlocalLift(from, item);
        }

        public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
        {
            if (from.IsControlledBy(this))
            {
                //return true;
            }

            if (!base.CheckNonlocalDrop(from, item, target))
            {
                return false;
            }

            if (from.AccessLevel >= AccessLevel.GameMaster)
            {
                return true;
            }

            Container pack = Backpack;
            if (from == this && HasTrade && (target == pack || target.IsChildOf(pack)))
            {
                BounceInfo bounce = item.GetBounce();

                if (bounce != null && bounce.m_Parent is Item)
                {
                    Item parent = (Item)bounce.m_Parent;

                    if (parent == pack || parent.IsChildOf(pack))
                    {
                        return true;
                    }
                }

                SendLocalizedMessage(1004041); // You can't do that while you have a trade pending.
                return false;
            }

            return true;
        }

        protected override void OnLocationChange(Point3D oldLocation)
        {
            CheckLightLevels(false);

            DesignContext context = m_DesignContext;

            if (context == null || m_NoRecursion)
            {
                return;
            }

            m_NoRecursion = true;

            HouseFoundation foundation = context.Foundation;

            int newX = X, newY = Y;
            int newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

            int startX = foundation.X + foundation.Components.Min.X + 1;
            int startY = foundation.Y + foundation.Components.Min.Y + 1;
            int endX = startX + foundation.Components.Width - 1;
            int endY = startY + foundation.Components.Height - 2;

            if (newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map)
            {
                if (Z != newZ)
                {
                    Location = new Point3D(X, Y, newZ);
                }

                m_NoRecursion = false;
                return;
            }

            Location = new Point3D(foundation.X, foundation.Y, newZ);
            Map = foundation.Map;

            m_NoRecursion = false;
        }

        public override bool OnMoveOver(Mobile m)
        {
            if (m is BaseCreature && !((BaseCreature)m).Controlled)
            {
                return (!Alive || !m.Alive || IsDeadBondedPet || m.IsDeadBondedPet) || (Hidden && IsStaff());
            }

            return base.OnMoveOver(m);
        }

        public override bool CheckShove(Mobile shoved)
        {
            if (TransformationSpellHelper.UnderTransformation(shoved, typeof(WraithFormSpell)))
            {
                return true;
            }

            if (shoved is BaseCreature)
            {
                var bc = (BaseCreature)shoved;
                if (bc.ControlMaster != null && bc.ControlMaster == this)
                {
                    return true;
                }
            }
            if (this.Region is GuardedRegion)
                return true;

            if (shoved.Str <= 30)
                return true;

            if (Mounted && shoved.Str <= 60)
                return true;

            return base.CheckShove(shoved);
        }

        protected override void OnMapChange(Map oldMap)
        {
            ViceVsVirtueSystem.OnMapChange(this);

            if (NetState != null && NetState.IsEnhancedClient)
            {
                Waypoints.OnMapChange(this, oldMap);
            }

            if ((Map != Faction.Facet && oldMap == Faction.Facet) || (Map == Faction.Facet && oldMap != Faction.Facet))
            {
                InvalidateProperties();
            }

            BaseGump.CheckCloseGumps(this);

            DesignContext context = m_DesignContext;

            if (context == null || m_NoRecursion)
            {
                return;
            }

            m_NoRecursion = true;

            HouseFoundation foundation = context.Foundation;

            if (Map != foundation.Map)
            {
                Map = foundation.Map;
            }

            m_NoRecursion = false;
        }

        public override void OnBeneficialAction(Mobile target, bool isCriminal)
        {
            if (m_SentHonorContext != null)
            {
                m_SentHonorContext.OnSourceBeneficialAction(target);
            }

            if (Siege.SiegeShard && isCriminal)
            {
                Criminal = true;
                return;
            }

            base.OnBeneficialAction(target, isCriminal);
        }

        public override bool IsBeneficialCriminal(Mobile target)
        {
            if (!target.Criminal && target is BaseCreature && ((BaseCreature)target).GetMaster() == this)
                return false;

            return base.IsBeneficialCriminal(target);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            int disruptThreshold;

            LastDamage = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (Wisp != null && Young && Region is DungeonGuardedRegion)
            {
                if (!IsCooldown("dicacbt") && !Warmode)
                {
                    SetCooldown("dicacbt", TimeSpan.FromSeconds(10));
                    Wisp.Fala("Aperte ALT+P e clique em PEACE para entrar em modo guerra. Clique nos monstros 2x para bater neles.");

                }
                else if (!IsCooldown("dicabands") && DicaBands)
                {
                    SetCooldown("dicabands", TimeSpan.FromSeconds(10));
                    Wisp.Fala("Clique 2x nas bandagens em sua mochila e em vc para se curar, mas evite tomar dano enquanto se cura !");

                }
            }

            if(Frozen || Paralyzed)
            {
                var teias = this.FindItemsInRange(this.Map, 0);
                foreach(var teia in teias)
                {
                    if(teia is Teia)
                    {
                        Frozen = false;
                        SendMessage("Voce escapou da teia");
                        OverheadMessage("* escapou *");
                    }
                }
            }

            if (!Core.AOS)
            {
                disruptThreshold = 0;
            }
            else if (from != null && from.Player)
            {
                disruptThreshold = 19;
            }
            else
            {
                disruptThreshold = 26;
            }

            if (Core.SA)
            {
                disruptThreshold += Dex / 12;
            }

            if (amount > disruptThreshold)
            {
                BandageContext c = BandageContext.GetContext(this);

                if (c != null)
                {
                    c.Slip(from == null || !from.Player, amount);
                }
            }

            if (Confidence.IsRegenerating(this))
            {
                Confidence.StopRegenerating(this);
            }

            if (m_ReceivedHonorContext != null)
            {
                m_ReceivedHonorContext.OnTargetDamaged(from, amount);
            }
            if (m_SentHonorContext != null)
            {
                m_SentHonorContext.OnSourceDamaged(from, amount);
            }

            if (willKill && from is PlayerMobile)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(10), ((PlayerMobile)@from).RecoverAmmo);
            }

            #region Mondain's Legacy
            if (InvisibilityPotion.HasTimer(this))
            {
                InvisibilityPotion.Iterrupt(this);
            }
            #endregion

            UndertakersStaff.TryRemoveTimer(this);

            base.OnDamage(amount, from, willKill);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ContaRP
        {
            get
            {
                var acct = Account as Account;
                return acct.RP;
            }
            set
            {
                var acct = Account as Account;
                acct.RP = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool RP
        {
            get { return Shard.RP || GetFlag(PlayerFlag.RP); }
            set
            {

                SetFlag(PlayerFlag.RP, value);
                InvalidateProperties();
            }
        }

        public static int MAX_MORTES = 5;

        public override void OnDeathsChange(int oldValue)
        {
            if (RP) // ganhou ponto de death
            {
                //this.SendMessage("Voce agora tem " + this.Deaths + "/5 Mortes...");
                if (Deaths >= MAX_MORTES)
                {
                    SendGump(new AnuncioGump(this, "!!! VOCE MORREU !!!"));
                    SendMessage(38, "!!! VOCE MORREU !!!");
                }
            }
            else
            {
                SendMessage("Suas Mortes: " + Deaths);
            }
            base.OnDeathsChange(oldValue);
        }

        public override void OnConnected()
        {
            //Caso esteja morto 
            if (RP) // ganhou ponto de death
            {
                if (Deaths >= MAX_MORTES)
                {
                    SendGump(new AnuncioGump(this, "!!! VOCÊ ESTÁ MORTO !!!"));
                    PrivateOverheadMessage(MessageType.Emote, 88, false, "!!! VOCÊ ESTÁ MORTO !!!", NetState);
                }
                else if (!Alive && (m_ExpireDeathTimer == null))
                {
                    m_ExpireDeathTimer = new ExpireDeathTimer(this);
                    m_ExpireDeathTimer.Start();
                }
            }
            else
            {
                SendMessage("Suas Mortes: " + Deaths);
            }
        }

        public override void Kill()
        {
            base.Kill();
            var cordaAmarrada = Rope.GetCordaAmarrada(this);
            if (cordaAmarrada != null)
            {
                cordaAmarrada.Visible = false;
            }
        }

        DateTime lastRes = DateTime.MinValue;

        public bool IsResProtected()
        {
            return DateTime.Now < lastRes + TimeSpan.FromSeconds(5);
        }

        public override void Resurrect()
        {

            if (RP && Corpse != null && !Corpse.Deleted) //FE: Leva a alminha pra perto do corpo se ele ainda existir
            {
                Map = Corpse.Map;
                Location = Corpse.Location;
            }
            else if (!this.IsStaff() && RP)
            {
                SendMessage("Seu corpo já se decompôs!");
                return;
            }

            if (RP && Deaths >= MAX_MORTES)
            {
                SendMessage("Voce morreu... e apenas pode vagar no mundo como alma");
                return;
            }

            bool wasAlive = Alive;

            base.Resurrect();

            lastRes = DateTime.Now;

            Elemento = ElementoPvM.None;

            if (Alive && !wasAlive)
            {
                if (!RP)
                {
                    Item deathRobe = new DeathRobe();

                    if (!EquipItem(deathRobe))
                    {
                        deathRobe.Delete();
                    }
                }
                ClearScreen();
                SendEverything();

                if (NetState != null && NetState.IsEnhancedClient)
                {
                    Waypoints.RemoveHealers(this, Map);
                }

                #region Scroll of Alacrity
                if (AcceleratedStart > DateTime.UtcNow)
                {
                    BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.ArcaneEmpowerment, 1078511, 1078512, AcceleratedSkill.ToString()));
                }
                #endregion
            }
            if (Region == null || (!(Region is DungeonRegion) && !(Region is DungeonGuardedRegion)))
            {
                this.UnstoreMount();
            }

            this.Hits = HitsMax;
            this.Stam = 10;
            this.Mana = 0;

            //FE: para o contador de ressurreição quando ressuscitado
            if (RP)
            {
                if (m_ExpireDeathTimer != null)
                {
                    m_ExpireDeathTimer.Stop();
                }

                //FE: equipa o que sobrou do corpo e apaga ele
                if (Corpse != null && !Corpse.Deleted)
                {
                    ((Corpse)Corpse).AutoLoot(this);
                    Corpse.Delete();
                }

                Emote("* vitalizado *");
            }

            var cordaAmarrada = Rope.GetCordaAmarrada(this);
            if (cordaAmarrada != null)
            {
                this.Frozen = true;
                cordaAmarrada.Visible = true;
                Emote("* amarrado *");
            }

            //FE: Wake up sounds
            //TODO: Verificar porque alguns sons estão errados (Alinhar com o patch novo)

            PlayOmphSound();
        }

        public void PlayOmphSound()
        {
            if (Female)
            {
                PlaySound(0x325 + Utility.Random(7)); //0x325 a 0x32B (7 sons de oomph) female f_oomph_01.wav até f_oomph_07.wav
            }
            else
            {
                PlaySound(0x435 + Utility.Random(9)); //0x435 a 0x43D (9 sons de oomph) male m_oomph_01.wav até m_oomph_09.wav
            }
        }

        public override double RacialSkillBonus
        {
            get
            {
                if (Core.ML && Race == Race.Human)
                {
                    return 20.0;
                }

                return 0;
            }
        }

        public override double GetRacialSkillBonus(SkillName skill)
        {
            if (Core.ML && Race == Race.Human)
                return 20.0;

            if (Core.SA && Race == Race.Gargoyle)
            {
                if (skill == SkillName.Imbuing)
                    return 30.0;

                if (skill == SkillName.Throwing)
                    return 20.0;
            }

            return RacialSkillBonus;
        }

        public override void OnWarmodeChanged()
        {
            if (!Warmode)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(10), RecoverAmmo);
            }
            Timer.DelayCall(TimeSpan.FromSeconds(2.5), new TimerCallback(DungeonMusic));

            if (Warmode && Meditating)
            {
                Meditating = false;
                SendMessage(12, "Você parou de meditar");
            }
        }



        public void DungeonMusic()
        {
            PlayGameMusic(Region.Music);
        }

        private Mobile m_InsuranceAward;
        private int m_InsuranceCost;
        private int m_InsuranceBonus;

        private List<Item> m_EquipSnapshot;

        public List<Item> EquipSnapshot { get { return m_EquipSnapshot; } }

        private bool FindItems_Callback(Item item)
        {
            if (!item.Deleted && (item.LootType == LootType.Blessed || item.Insured))
            {
                if (Backpack != item.Parent)
                {
                    return true;
                }
            }
            return false;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool Criminal
        {
            get
            {
                return base.Criminal;
            }
            set
            {
                if (Shard.WARSHARD)
                {
                    base.Criminal = false;
                    return;
                }
                bool crim = base.Criminal;
                base.Criminal = value;

                if (value != crim)
                {
                    if (value)
                        BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.CriminalStatus, 1153802, 1153828));
                    else
                        BuffInfo.RemoveBuff(this, BuffIcon.CriminalStatus);
                }
            }
        }

        public override bool OnBeforeDeath()
        {
            NetState state = NetState;

            if (state != null)
            {
                state.CancelAllTrades();
            }

            if (Criminal)
                BuffInfo.RemoveBuff(this, BuffIcon.CriminalStatus);

            DropHolding();

            if (Core.AOS && Backpack != null && !Backpack.Deleted)
            {
                var ilist = Backpack.FindItemsByType<Item>(FindItems_Callback);

                for (int i = 0; i < ilist.Count; i++)
                {
                    Backpack.AddItem(ilist[i]);
                }
            }

            m_EquipSnapshot = new List<Item>(Items);

            m_InsuranceAward = base.FindMostRecentDamager(false);

            if (m_InsuranceAward is BaseCreature)
            {
                Mobile master = ((BaseCreature)m_InsuranceAward).GetMaster();

                if (master != null)
                {
                    m_InsuranceAward = master;
                }
            }

            if (m_InsuranceAward != null && (!m_InsuranceAward.Player || m_InsuranceAward == this))
            {
                m_InsuranceAward = null;
            }

            if (m_InsuranceAward is PlayerMobile)
            {
                ((PlayerMobile)m_InsuranceAward).m_InsuranceBonus = 0;
            }

            if (m_ReceivedHonorContext != null)
            {
                m_ReceivedHonorContext.OnTargetKilled();
            }

            if (m_SentHonorContext != null)
            {
                m_SentHonorContext.OnSourceKilled();
            }

            RecoverAmmo();

            if (NetState != null && NetState.IsEnhancedClient)
            {
                Waypoints.AddCorpse(this);
            }

            return base.OnBeforeDeath();
        }

        private bool CheckInsuranceOnDeath(Item item)
        {
            if (Young)
                return false;

            if (InsuranceEnabled && item.Insured)
            {
                int insuredAmount = GetInsuranceCost(item);

                if (AutoRenewInsurance)
                {
                    int cost = (m_InsuranceAward == null ? insuredAmount : insuredAmount / 2);

                    if (Banker.Withdraw(this, cost))
                    {
                        m_InsuranceCost += cost;
                        item.PayedInsurance = true;
                        SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
                    }
                    else
                    {
                        SendLocalizedMessage(1061079, "", 0x23); // You lack the funds to purchase the insurance
                        item.PayedInsurance = false;
                        item.Insured = false;
                        m_NonAutoreinsuredItems++;
                    }
                }
                else
                {
                    item.PayedInsurance = false;
                    item.Insured = false;
                }

                if (m_InsuranceAward != null)
                {
                    if (Banker.Deposit(m_InsuranceAward, insuredAmount / 4))
                    {
                        if (m_InsuranceAward is PlayerMobile)
                        {
                            ((PlayerMobile)m_InsuranceAward).m_InsuranceBonus += insuredAmount / 4;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public override DeathMoveResult GetParentMoveResultFor(Item item)
        {
            if (item is Uniforme)
            {
                var ropa = item as Uniforme;
                if (ropa != null)
                {
                    var guilda = (item.RootParent as PlayerMobile).Guild as Guild;

                    if (guilda == null)
                    {
                        item.Consume();
                        return DeathMoveResult.RemainEquiped;
                    }
                    else if (!item.Name.Contains(guilda.Abbreviation))
                    {
                        item.Consume();
                        return DeathMoveResult.RemainEquiped;
                    }
                    else if (guilda.ArmarioUniforme == null)
                    {
                        item.Consume();
                        return DeathMoveResult.RemainEquiped;
                    }
                    var panosGuilda = guilda.ArmarioUniforme.QtdPanos();
                    if (panosGuilda < ArmarioUniforme.CUSTO_PANOS)
                    {
                        SendMessage("Sua guilda nao tinha panos suficientes para repor partes do seu uniforme de guilda");
                        item.Consume();
                        return DeathMoveResult.RemainEquiped;
                    }
                    else if (!guilda.ArmarioUniforme.Paga())
                    {
                        SendMessage("Sua guilda nao tinha panos suficientes para repor partes do seu uniforme de guilda");
                        item.Consume();
                        return DeathMoveResult.RemainEquiped;
                    }
                    QtdPanosGuilda = panosGuilda - ArmarioUniforme.CUSTO_PANOS;
                    return DeathMoveResult.RemainEquiped;
                }
            }

            if (CheckInsuranceOnDeath(item) && !Young)
            {
                return DeathMoveResult.MoveToBackpack;
            }

            DeathMoveResult res = base.GetParentMoveResultFor(item);

            if (res == DeathMoveResult.MoveToCorpse && !(item is BankBox || item.RootParent is BankBox) && ViceVsVirtueSystem.Enabled && ViceVsVirtueSystem.Instance.Battle.OnGoing)
            {
                if (ViceVsVirtueSystem.IsVvV(this))
                    return DeathMoveResult.MoveToBackpack;

                if (ViceVsVirtueSystem.GetTemporario(this) != null)
                {
                    return DeathMoveResult.MoveToBackpack;
                }
            }

            if (res == DeathMoveResult.MoveToCorpse && item.Movable && Young)
            {
                res = DeathMoveResult.MoveToBackpack;
            }

            return res;
        }

        public int QtdPanosGuilda = -1;

        public override DeathMoveResult GetInventoryMoveResultFor(Item item)
        {
            if (ViceVsVirtueSystem.Enabled && ViceVsVirtueSystem.Instance.Battle.OnGoing)
            {
                if (ViceVsVirtueSystem.IsVvV(this))
                    return DeathMoveResult.MoveToBackpack;

                if (ViceVsVirtueSystem.GetTemporario(this) != null)
                {
                    return DeathMoveResult.MoveToBackpack;
                }
            }

            if (item is Uniforme)
            {
                var ropa = item as Uniforme;
                if (ropa != null)
                {
                    var guilda = (item.RootParent as PlayerMobile).Guild as Guild;

                    if (guilda == null)
                    {
                        item.Consume();
                        return DeathMoveResult.RemainEquiped;
                    }
                    else if (!item.Name.Contains(guilda.Abbreviation))
                    {
                        item.Consume();
                        return DeathMoveResult.RemainEquiped;
                    }
                    else if(guilda.ArmarioUniforme == null)
                    {
                        item.Consume();
                        return DeathMoveResult.RemainEquiped;
                    }
                    var panosGuilda = guilda.ArmarioUniforme.QtdPanos();
                    if (panosGuilda < ArmarioUniforme.CUSTO_PANOS)
                    {
                        SendMessage("Sua guilda nao tinha panos suficientes para repor partes do seu uniforme de guilda");
                        item.Consume();
                        return DeathMoveResult.RemainEquiped;
                    } else if(!guilda.ArmarioUniforme.Paga())
                    {
                        SendMessage("Sua guilda nao tinha panos suficientes para repor partes do seu uniforme de guilda");
                        item.Consume();
                        return DeathMoveResult.RemainEquiped;
                    }
                    QtdPanosGuilda = panosGuilda - ArmarioUniforme.CUSTO_PANOS;
                    return DeathMoveResult.RemainEquiped;
                }
            }

            if (CheckInsuranceOnDeath(item) && !Young)
            {
                return DeathMoveResult.MoveToBackpack;
            }

            DeathMoveResult res = base.GetInventoryMoveResultFor(item);

            if (res == DeathMoveResult.MoveToCorpse && item.Movable && Young)
            {
                res = DeathMoveResult.MoveToBackpack;
            }

            return res;
        }

        //FE: Evento de contagem de tempo para acordar do desmaio

        private Timer m_ExpireDeathTimer; //FLS controle de tempo de desmaio

        private static TimeSpan m_ExpireDeathDelay = TimeSpan.FromMinutes(3.0); //Definir o tempo padrão

        public static TimeSpan ExpireDeathDelay { get { return m_ExpireDeathDelay; } set { m_ExpireDeathDelay = value; } }

        private class ExpireDeathTimer : Timer
        {
            private readonly Mobile m_Mobile;

            public ExpireDeathTimer(Mobile m)
                : base(m_ExpireDeathDelay)
            {
                Priority = TimerPriority.FiveSeconds;
                m_Mobile = m;
            }

            protected override void OnTick()
            {
                if (!m_Mobile.Alive)
                {
                    m_Mobile.Resurrect();
                }
            }
        }

        //FE: Fim do Evento
        public override void OnDeath(Container c)
        {
            if (NetState != null && NetState.IsEnhancedClient)
            {
                Waypoints.OnDeath(this);
            }

            if(LastKiller is PlayerMobile)
            {
                UltimaMorte = DateTime.UtcNow;
            }
          
            if (Wisp != null)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(10), () =>
                {
                    Wisp.Fala("Eu sei que voce morreu, mas nao desista !! Como voce e novato, eu nao deixei voce perder seus items ! hihi");
                });
            }

            Mobile m = FindMostRecentDamager(false);
            if (!this.IsCooldown("avisomorte") && m != null && m.Name != null)
            {
                this.SetCooldown("avisomorte", TimeSpan.FromMinutes(10));
                if (m is PlayerMobile)
                {
                    DiscordBot.SendMessage($":crossed_swords:[PvP] {m.Name} matou {Name} {(m.Region != null && m.Region.Name != null ? $" em {m.Region.Name}" : "")}");
                }
                else if (m is BaseCreature)
                {
                    if (((BaseCreature)m).IsBoss)
                    {
                        DiscordBot.SendMessage($":skull_crossbones: [BOSS] {Name} foi obliterado pelo boss {m.Name} !", 160);
                    } else
                        DiscordBot.SendMessage($":skull_crossbones: {Name} foi morto por um(a) {m.Name}", 160);
                }
            }
            PlayerMobile killer = m as PlayerMobile;
            var bc = m as BaseCreature;

            if (killer == null && bc != null)
            {
                killer = bc.GetMaster() as PlayerMobile;
                if (killer == null)
                {
                    if (bc.IsParagon)
                    {
                        if (bc.DeleteTimeLeft == TimeSpan.Zero)
                        {
                            bc.BeginDeleteTimer(6);
                        }
                    }
                }
            }

            if (m_NonAutoreinsuredItems > 0)
            {
                SendLocalizedMessage(1061115);
            }

            base.OnDeath(c);

            if(QtdPanosGuilda >= 0)
            {
                SendMessage($"Voce usou panos de guilda para continuar com seu uniforme. Panos restantes: {QtdPanosGuilda}");
                QtdPanosGuilda = 0;
            }

            //FE: Sistema de demaios BEGIN
            if (RP)
            {
                SetLocation(new Point3D(1503, 1630, 10), true); //definir local da sala dos mortos
                double azar = Utility.RandomDouble();
                if (azar > 0.8)
                {
                    Deaths += 2;
                }
                else if (azar > 0.2)
                {
                    Deaths++;
                }

                if (Deaths >= MAX_MORTES)
                {
                    Send(DeathStatus.Instantiate(true));
                    Send(DeathStatus.Instantiate(false));
                    SendGump(new AnuncioGump(this, "!!! VOCÊ MORREU !!!"));
                    SendMessage(38, "!!! VOCÊ MORREU !!!");
                    PrivateOverheadMessage(MessageType.Emote, 88, false, "!!! VOCÊ MORREU !!!", NetState);
                }
                else if (this.Deaths >= (MAX_MORTES - 2))
                {
                    PrivateOverheadMessage(MessageType.Emote, 88, false, "!!! MORIBUNDO !!!", NetState);
                    SendGump(new AnuncioGump(this, "!!! VOCÊ ESTÁ DESMAIADO E MORIBUNDO !!!"));
                    SendMessage(38, "!!! VOCÊ ESTÁ DESMAIADO E MORIBUNDO !!!");
                    PrivateOverheadMessage(MessageType.Emote, 88, false, "!!! VOCÊ ESTÁ DESMAIADO E MORIBUNDO !!!", NetState);
                }
                else
                {
                    SendGump(new AnuncioGump(this, "!!! VOCÊ ESTÁ DESMAIADO !!!"));
                    SendMessage(38, "!!! VOCÊ ESTÁ DESMAIADO !!!");
                    PrivateOverheadMessage(MessageType.Emote, 88, false, "!!! DESMAIADO !!!", NetState);
                }
                m_ExpireDeathTimer = new ExpireDeathTimer(this);
                m_ExpireDeathTimer.Start();
            }
            else
            {
                Deaths++;
                Send(DeathStatus.Instantiate(true));
                Send(DeathStatus.Instantiate(false));
            }

            //FE Sistema de desmaios END

            m_EquipSnapshot = null;

            HueMod = -1;
            NameMod = null;
            SavagePaintExpiration = TimeSpan.Zero;

            SetHairMods(-1, -1);

            PolymorphSpell.StopTimer(this);
            IncognitoSpell.StopTimer(this);
            DisguiseTimers.RemoveTimer(this);

            WeakenSpell.RemoveEffects(this);
            ClumsySpell.RemoveEffects(this);
            FeeblemindSpell.RemoveEffects(this);
            CurseSpell.RemoveEffect(this);

            EndAction(typeof(PolymorphSpell));
            EndAction(typeof(IncognitoSpell));

            MeerMage.StopEffect(this, false);

            BaseEscort.DeleteEscort(this);

            #region Stygian Abyss
            if (Flying)
            {
                Flying = false;
                BuffInfo.RemoveBuff(this, BuffIcon.Fly);
            }
            #endregion

            StolenItem.ReturnOnDeath(this, c);

            if (m_PermaFlags.Count > 0)
            {
                m_PermaFlags.Clear();

                if (c is Corpse)
                {
                    ((Corpse)c).Criminal = true;
                }

                if (Stealing.ClassicMode)
                {
                    Criminal = true;
                }
            }

            if (killer != null && Murderer && DateTime.UtcNow >= killer.m_NextJustAward)
            {
                // This scales 700.0 skill points to 1000 valor points
                int pointsToGain = (int)(SkillsTotal / 7);

                // This scales 700.0 skill points to 7 minutes wait
                int minutesToWait = Math.Max(1, (int)(SkillsTotal / 1000));

                bool gainedPath = false;

                if (VirtueHelper.Award(m, VirtueName.Justice, pointsToGain, ref gainedPath))
                {
                    if (gainedPath)
                    {
                        m.SendLocalizedMessage(1049367); // You have gained a path in Justice!
                    }
                    else
                    {
                        m.SendLocalizedMessage(1049363); // You have gained in Justice.
                    }

                    m.FixedParticles(0x375A, 9, 20, 5027, EffectLayer.Waist);
                    m.PlaySound(0x1F7);

                    killer.m_NextJustAward = DateTime.UtcNow + TimeSpan.FromMinutes(minutesToWait);
                }
            }

            if (m_InsuranceAward is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m_InsuranceAward;

                if (pm.m_InsuranceBonus > 0)
                {
                    pm.SendLocalizedMessage(1060397, pm.m_InsuranceBonus.ToString());
                    // ~1_AMOUNT~ gold has been deposited into your bank box.
                }
            }

            if (Young)
            {
                /*
                if (YoungDeathTeleport())
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(2.5), SendYoungDeathNotice);
                }
                */
            }

            Faction.HandleDeath(this, killer);

            Guilds.Guild.HandleDeath(this, killer);

            if (m_BuffTable != null)
            {
                var list = new List<BuffInfo>();

                foreach (BuffInfo buff in m_BuffTable.Values)
                {
                    if (!buff.RetainThroughDeath)
                    {
                        list.Add(buff);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    RemoveBuff(list[i]);
                }
            }

            /*
            #region Stygian Abyss
            if (Region.IsPartOf("Abyss") && SSSeedExpire > DateTime.UtcNow)
            {
                SendGump(new ResurrectGump(this, ResurrectMessage.SilverSapling));
            }
            */

            if (LastKiller is BaseVoidCreature)
                ((BaseVoidCreature)LastKiller).Mutate(VoidEvolution.Killing);

            if (this.Region is DungeonRegion)
                SendGump(new DeadGump(this));
            else
                DeadGump.AchaCurandeiro(this);
        }

        private List<Mobile> m_PermaFlags;
        private readonly List<Mobile> m_VisList;
        private readonly Hashtable m_AntiMacroTable;
        private TimeSpan m_GameTime;
        public TimeSpan m_ShortTermElapse;
        public TimeSpan m_LongTermElapse;
        private DateTime m_SessionStart;
        private DateTime m_SavagePaintExpiration;
        private SkillName m_Learning = (SkillName)(-1);

        public SkillName Learning { get { return m_Learning; } set { m_Learning = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan SavagePaintExpiration
        {
            get
            {
                TimeSpan ts = m_SavagePaintExpiration - DateTime.UtcNow;

                if (ts < TimeSpan.Zero)
                {
                    ts = TimeSpan.Zero;
                }

                return ts;
            }
            set { m_SavagePaintExpiration = DateTime.UtcNow + value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextSmithBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Smith, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Smith, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextTailorBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Tailor, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Tailor, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextAlchemyBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Alchemy, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Alchemy, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextInscriptionBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Inscription, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Inscription, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextTinkeringBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Tinkering, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Tinkering, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextFletchingBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Fletching, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Fletching, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextCarpentryBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Carpentry, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Carpentry, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextCookingBulkOrder
        {
            get
            {
                return BulkOrderSystem.GetNextBulkOrder(BODType.Cooking, this);
            }
            set
            {
                BulkOrderSystem.SetNextBulkOrder(BODType.Cooking, this, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastEscortTime { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastPetBallTime { get; set; }

        public PokerGame PokerGame;
        public DateTime PokerJoinTimer;
        public static int COR_SKILL = 1234;

        public PlayerMobile()
        {
            Nivel = 1;
            Instances.Add(this);

            m_AutoStabled = new List<Mobile>();

            DungeonsCompletas = new List<CustomDungeons>();
            #region Mondain's Legacy
            m_Quests = new List<BaseQuest>();
            m_Chains = new Dictionary<QuestChain, BaseChain>();
            m_DoneQuests = new List<QuestRestartInfo>();
            m_Collections = new Dictionary<Collection, int>();
            m_RewardTitles = new List<object>();

            m_PeacedUntil = DateTime.UtcNow;
            #endregion

            m_VisList = new List<Mobile>();
            m_PermaFlags = new List<Mobile>();
            m_AntiMacroTable = new Hashtable();
            m_RecentlyReported = new List<Mobile>();

            //m_BOBFilter = new BOBFilter();

            m_GameTime = TimeSpan.Zero;
            m_ShortTermElapse = TimeSpan.FromHours(8.0);
            m_LongTermElapse = TimeSpan.FromHours(40.0);

            m_JusticeProtectors = new List<Mobile>();
            m_GuildRank = RankDefinition.Lowest;

            m_ChampionTitles = new ChampionTitleInfo();

            this.Blessed = false;
        }

        public override bool MutateSpeech(List<Mobile> hears, ref string text, ref object context)
        {
            if (Alive)
            {
                return false;
            }

            if (Core.ML && Skills[SkillName.SpiritSpeak].Value >= 100.0)
            {
                return false;
            }

            if (Core.AOS)
            {
                for (int i = 0; i < hears.Count; ++i)
                {
                    Mobile m = hears[i];

                    if (m != this && m.Skills[SkillName.SpiritSpeak].Value >= 100.0)
                    {
                        return false;
                    }
                }
            }

            return base.MutateSpeech(hears, ref text, ref context);
        }

        public override void DoSpeech(string text, int[] keywords, MessageType type, int hue)
        {

            if (Guilds.Guild.NewGuildSystem && (type == MessageType.Guild || type == MessageType.Alliance))
            {
                Guild g = Guild as Guild;
                if (g == null)
                {
                    SendLocalizedMessage(1063142); // You are not in a guild!
                }
                else if (type == MessageType.Alliance)
                {
                    if (g.Alliance != null && g.Alliance.IsMember(g))
                    {
                        //g.Alliance.AllianceTextMessage( hue, "[Alliance][{0}]: {1}", Name, text );
                        g.Alliance.AllianceChat(this, text);
                        SendToStaffMessage(this, "[Alliance]: {0}", text);

                        m_AllianceMessageHue = hue;
                    }
                    else
                    {
                        SendLocalizedMessage(1071020); // You are not in an alliance!
                    }
                }
                else //Type == MessageType.Guild
                {
                    m_GuildMessageHue = hue;

                    g.GuildChat(this, text);
                    SendToStaffMessage(this, "[Guild]: {0}", text);
                }
            }
            else
            {
                base.DoSpeech(text, keywords, type, hue);
            }
        }

        private static void SendToStaffMessage(Mobile from, string text)
        {
            Packet p = null;

            foreach (NetState ns in from.GetClientsInRange(8))
            {
                Mobile mob = ns.Mobile;

                if (mob != null && mob.AccessLevel >= AccessLevel.GameMaster && mob.AccessLevel > from.AccessLevel)
                {
                    if (p == null)
                    {
                        p =
                            Packet.Acquire(
                                new UnicodeMessage(
                                    from.Serial, from.Body, MessageType.Regular, from.SpeechHue, 3, from.Language, from.Name, text));
                    }

                    ns.Send(p);
                }
            }

            Packet.Release(p);
        }

        private static void SendToStaffMessage(Mobile from, string format, params object[] args)
        {
            SendToStaffMessage(from, String.Format(format, args));
        }


        #region Poison
        public override void OnCured(Mobile from, Poison oldPoison)
        {
            BuffInfo.RemoveBuff(this, BuffIcon.Poison);
        }

        public override ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
        {

            if (poison == null)
                return ApplyPoisonResult.Immune;

            if (!Alive)
            {
                return ApplyPoisonResult.Immune;
            }

            if (!from.Player && Skills.TasteID.Value >= 50)
            {
                if (Utility.RandomDouble() < Skills.TasteID.Value / 500)
                {
                    return ApplyPoisonResult.Immune;
                }

                if (Utility.RandomDouble() < Skills.TasteID.Value / 100)
                {
                    if (poison.Level > 0)
                        poison = PoisonImpl.DecreaseLevel(poison);
                    else
                        return ApplyPoisonResult.Immune;
                }

                if (Utility.RandomDouble() < Skills.TasteID.Value / 200)
                {
                    if (poison.Level > 0)
                        poison = PoisonImpl.DecreaseLevel(poison);
                    else
                        return ApplyPoisonResult.Immune;
                }
            }

            //Skill Masteries
            if (Spells.SkillMasteries.ResilienceSpell.UnderEffects(this) && 0.25 > Utility.RandomDouble())
            {
                return ApplyPoisonResult.Immune;
            }

            if (EvilOmenSpell.TryEndEffect(this))
            {
                poison = PoisonImpl.IncreaseLevel(poison);
            }

            if (Shard.COMBATE_SPHERE)
            {
                var ctx = BandageContext.GetContext(this);
                if (ctx != null)
                {
                    ctx.StopHeal();
                }
            }

            //Skill Masteries
            if ((this.Poison == null || this.Poison.Level < poison.Level) && ToleranceSpell.OnPoisonApplied(this))
            {
                poison = PoisonImpl.DecreaseLevel(poison);

                if (poison == null || poison.Level <= 0)
                {
                    PrivateOverheadMessage("Voce sente seu corpo resistindo o veneno"); // * You feel yourself resisting the effects of the poison *
                    return ApplyPoisonResult.Immune;
                }
            }

            ApplyPoisonResult result = base.ApplyPoison(from, poison);

            if (from != null && result == ApplyPoisonResult.Poisoned && PoisonTimer is PoisonImpl.PoisonTimer)
            {
                (PoisonTimer as PoisonImpl.PoisonTimer).From = from;
            }

            return result;
        }

        public override bool CheckPoisonImmunity(Mobile from, Poison poison)
        {
            /*
            if (Young)
            {
                return true;
            }
            */

            return base.CheckPoisonImmunity(from, poison);
        }

        public override void OnPoisonImmunity(Mobile from, Poison poison)
        {
            if (Young)
            {
                SendLocalizedMessage(502808);
                // You would have been poisoned, were you not new to the land of Britannia. Be careful in the future.
            }
            else
            {
                base.OnPoisonImmunity(from, poison);
            }
        }
        #endregion

        public PlayerMobile(Serial s)
            : base(s)
        {
            Instances.Add(this);

            m_VisList = new List<Mobile>();
            m_AntiMacroTable = new Hashtable();
        }

        public override bool SendGump(Gump g)
        {
            return base.SendGump(g);
        }

        public List<Mobile> VisibilityList { get { return m_VisList; } }

        public List<Mobile> PermaFlags { get { return m_PermaFlags; } }

        public override int Luck { get { return AosAttributes.GetValue(this, AosAttribute.Luck) + TenthAnniversarySculpture.GetLuckBonus(this); } }

        public int RealLuck
        {
            get
            {
                int facetBonus = !Siege.SiegeShard && this.Map == Map.Felucca ? RandomItemGenerator.FeluccaLuckBonus : 250;

                return Luck + FountainOfFortune.GetLuckBonus(this) + facetBonus;
            }
        }

        public override bool IsHarmfulCriminal(IDamageable damageable)
        {
            Mobile target = damageable as Mobile;

            if (Stealing.ClassicMode && target is PlayerMobile && ((PlayerMobile)target).m_PermaFlags.Count > 0)
            {
                int noto = Notoriety.Compute(this, target);

                if (noto == Notoriety.Innocent)
                {
                    target.Delta(MobileDelta.Noto);
                }

                return false;
            }

            if (target is BaseCreature && ((BaseCreature)target).InitialInnocent && !((BaseCreature)target).Controlled)
            {
                return false;
            }

            if (Core.ML && target is BaseCreature && ((BaseCreature)target).Controlled && ((BaseCreature)target).ControlMaster == this)
            {
                return false;
            }

            if (target is BaseCreature && ((BaseCreature)target).Summoned && ((BaseCreature)target).SummonMaster == this)
            {
                return false;
            }

            return base.IsHarmfulCriminal(damageable);
        }

        public bool AntiMacroCheck(Skill skill, object obj)
        {
            if (obj == null || m_AntiMacroTable == null)
            {
                return true;
            }

            Hashtable tbl = (Hashtable)m_AntiMacroTable[skill];
            if (tbl == null)
            {
                m_AntiMacroTable[skill] = tbl = new Hashtable();
            }

            CountAndTimeStamp count = (CountAndTimeStamp)tbl[obj];
            if (count != null)
            {
                if (count.TimeStamp + SkillCheck.AntiMacroExpire <= DateTime.UtcNow)
                {
                    count.Count = 1;
                    return true;
                }
                else
                {
                    ++count.Count;
                    if (count.Count <= SkillCheck.Allowance)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                tbl[obj] = count = new CountAndTimeStamp();
                count.Count = 1;

                return true;
            }
        }

        private void RevertHair()
        {
            SetHairMods(-1, -1);
        }

        public BOBFilter BOBFilter
        {
            get
            {
                return BulkOrderSystem.GetBOBFilter(this);
            }
        }

        public Template SetDefaultTemplate()
        {
            if (Templates.Templates.Count == 0)
            {
                var padrao = new Template();
                padrao.Name = "Template Inicial";
                padrao.FromPlayer(this);
                Templates.Templates.Add(padrao);
                CurrentTemplate = padrao.Name;
                return padrao;
            }
            return null;
        }

        #region Serialization
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            switch (version)
            {
                case 55:
                    m_EventCalendarAccount = reader.ReadItem() as EventCalendarAccount;
                    goto case 54;
                case 54:
                    BonusPeso = reader.ReadInt();
                    goto case 53;
                case 53:
                    PassoWispGuia = reader.ReadInt();
                    goto case 52;
                case 52:
                    DungeonsCompletas = DungeonsCompletasSerializer.Deserialize(reader);
                    goto case 51;
                case 51:
                    Elementos.Deserialize(reader);
                    goto case 50;
                case 50:
                    PontosTalento = reader.ReadByte();
                    goto case 49;
                case 49:
                    Nivel = reader.ReadByte();
                    ExpTotal = reader.ReadInt();
                    goto case 48;
                case 48:
                    Talentos.Deserialize(reader);
                    goto case 46;
                case 47:
                    Talentos.DeserialzieOld(reader);
                    goto case 46;
                case 46:
                    FichaRP.Deserialize(reader);
                    goto case 45;
                case 45:
                    Vidas = reader.ReadInt();
                    goto case 44;
                case 44:
                    Famoso = reader.ReadBool();
                    goto case 43;
                case 43:
                    LastDungeonEntrance = reader.ReadPoint3D();
                    goto case 42;

                case 42:
                    goto case 41;
                case 41:
                    NivelBanco = reader.ReadInt();
                    goto case 40;
                case 40:
                    CurrentTemplate = reader.ReadString();
                    Templates.Deserialize(reader);
                    goto case 39;
                case 39:
                    DungeonMount = (BaseCreature)reader.ReadMobile();
                    goto case 38;
                case 38:
                    CampfireLocations = reader.ReadString();
                    if (CampfireLocations == null)
                        CampfireLocations = "";
                    goto case 37;
                case 37:
                    m_ExtendedFlags = (ExtendedPlayerFlag)reader.ReadInt();
                    goto case 36;
                case 36:
                    RewardStableSlots = reader.ReadInt();
                    goto case 35;
                case 35: // Siege Blessed Item
                    _BlessedItem = reader.ReadItem();
                    goto case 34;
                // Version 34 - new BOD System
                case 34:
                case 33:
                    {
                        m_ExploringTheDeepQuest = (ExploringTheDeepQuestChain)reader.ReadInt();
                        goto case 31;
                    }
                case 32:
                case 31:
                    {
                        DisplayGuildTitle = version > 31 && reader.ReadBool();
                        m_FameKarmaTitle = reader.ReadString();
                        m_PaperdollSkillTitle = reader.ReadString();
                        m_OverheadTitle = reader.ReadString();
                        m_SubtitleSkillTitle = reader.ReadString();

                        m_CurrentChampTitle = reader.ReadString();
                        m_CurrentVeteranTitle = reader.ReadInt();
                        goto case 30;
                    }
                case 30: goto case 29;
                case 29:
                    {
                        m_GauntletPoints = reader.ReadDouble();

                        m_SSNextSeed = reader.ReadDateTime();
                        m_SSSeedExpire = reader.ReadDateTime();
                        m_SSSeedLocation = reader.ReadPoint3D();
                        m_SSSeedMap = reader.ReadMap();

                        if (version < 30)
                        {
                            reader.ReadLong(); // Old m_LevelExp
                            int points = (int)reader.ReadLong();
                            if (points > 0)
                            {
                                Server.Engines.Points.PointsSystem.QueensLoyalty.ConvertFromOldSystem(this, points);
                            }

                            reader.ReadInt(); // Old m_Level
                            reader.ReadString(); // Old m_ExpTitle
                        }

                        m_VASTotalMonsterFame = reader.ReadInt();

                        m_Quests = QuestReader.Quests(reader, this);
                        m_Chains = QuestReader.Chains(reader);

                        m_Collections = new Dictionary<Collection, int>();
                        m_RewardTitles = new List<object>();

                        for (int i = reader.ReadInt(); i > 0; i--)
                        {
                            m_Collections.Add((Collection)reader.ReadInt(), reader.ReadInt());
                        }

                        for (int i = reader.ReadInt(); i > 0; i--)
                        {
                            m_RewardTitles.Add(QuestReader.Object(reader));
                        }

                        m_SelectedTitle = reader.ReadInt();

                        goto case 28;
                    }
                case 28:
                    {
                        m_PeacedUntil = reader.ReadDateTime();

                        goto case 27;
                    }
                case 27:
                    {
                        m_AnkhNextUse = reader.ReadDateTime();

                        goto case 26;
                    }
                case 26:
                    {
                        m_AutoStabled = reader.ReadStrongMobileList();

                        goto case 25;
                    }
                case 25:
                    {
                        int recipeCount = reader.ReadInt();

                        if (recipeCount > 0)
                        {
                            m_AcquiredRecipes = new Dictionary<int, bool>();

                            for (int i = 0; i < recipeCount; i++)
                            {
                                int r = reader.ReadInt();
                                if (reader.ReadBool()) //Don't add in recipies which we haven't gotten or have been removed
                                {
                                    m_AcquiredRecipes.Add(r, true);
                                }
                            }
                        }
                        goto case 24;
                    }
                case 24:
                    {
                        m_LastHonorLoss = reader.ReadDeltaTime();
                        goto case 23;
                    }
                case 23:
                    {
                        m_ChampionTitles = new ChampionTitleInfo(reader);
                        goto case 22;
                    }
                case 22:
                    {
                        m_LastValorLoss = reader.ReadDateTime();
                        goto case 21;
                    }
                case 21:
                    {
                        m_ToTItemsTurnedIn = reader.ReadEncodedInt();
                        m_ToTTotalMonsterFame = reader.ReadInt();
                        goto case 20;
                    }
                case 20:
                    {
                        m_AllianceMessageHue = reader.ReadEncodedInt();
                        m_GuildMessageHue = reader.ReadEncodedInt();

                        goto case 19;
                    }
                case 19:
                    {
                        int rank = reader.ReadEncodedInt();
                        int maxRank = RankDefinition.Ranks.Length - 1;
                        if (rank > maxRank)
                        {
                            rank = maxRank;
                        }

                        m_GuildRank = RankDefinition.Ranks[rank];
                        m_LastOnline = reader.ReadDateTime();
                        goto case 18;
                    }
                case 18:
                    {
                        m_SolenFriendship = (SolenFriendship)reader.ReadEncodedInt();

                        goto case 17;
                    }
                case 17: // changed how DoneQuests is serialized
                case 16:
                    {
                        m_Quest = QuestSerializer.DeserializeQuest(reader);

                        if (m_Quest != null)
                        {
                            m_Quest.From = this;
                        }

                        int count = reader.ReadEncodedInt();

                        if (count > 0)
                        {
                            m_DoneQuests = new List<QuestRestartInfo>();

                            for (int i = 0; i < count; ++i)
                            {
                                Type questType = QuestSerializer.ReadType(QuestSystem.QuestTypes, reader);
                                DateTime restartTime;

                                if (version < 17)
                                {
                                    restartTime = DateTime.MaxValue;
                                }
                                else
                                {
                                    restartTime = reader.ReadDateTime();
                                }

                                m_DoneQuests.Add(new QuestRestartInfo(questType, restartTime));
                            }
                        }

                        m_Profession = reader.ReadEncodedInt();
                        goto case 15;
                    }
                case 15:
                    {
                        m_LastCompassionLoss = reader.ReadDeltaTime();
                        goto case 14;
                    }
                case 14:
                    {
                        m_CompassionGains = reader.ReadEncodedInt();

                        if (m_CompassionGains > 0)
                        {
                            m_NextCompassionDay = reader.ReadDeltaTime();
                        }

                        goto case 13;
                    }
                case 13: // just removed m_PayedInsurance list
                case 12:
                    {
                        if (version < 34)
                            BulkOrderSystem.SetBOBFilter(this, new BOBFilter(reader));
                        goto case 11;
                    }
                case 11:
                    {
                        if (version < 13)
                        {
                            var payed = reader.ReadStrongItemList();

                            for (int i = 0; i < payed.Count; ++i)
                            {
                                payed[i].PayedInsurance = true;
                            }
                        }

                        goto case 10;
                    }
                case 10:
                    {
                        if (reader.ReadBool())
                        {
                            m_HairModID = reader.ReadInt();
                            m_HairModHue = reader.ReadInt();
                            m_BeardModID = reader.ReadInt();
                            m_BeardModHue = reader.ReadInt();
                        }

                        goto case 9;
                    }
                case 9:
                    {
                        SavagePaintExpiration = reader.ReadTimeSpan();

                        if (SavagePaintExpiration > TimeSpan.Zero)
                        {
                            BodyMod = (Female ? 184 : 183);
                            HueMod = 0;
                        }

                        goto case 8;
                    }
                case 8:
                    {
                        m_NpcGuild = (NpcGuild)reader.ReadInt();
                        m_NpcGuildJoinTime = reader.ReadDateTime();
                        m_NpcGuildGameTime = reader.ReadTimeSpan();
                        goto case 7;
                    }
                case 7:
                    {
                        m_PermaFlags = reader.ReadStrongMobileList();
                        goto case 6;
                    }
                case 6:
                    {
                        if (version < 34)
                            reader.ReadTimeSpan();
                        goto case 5;
                    }
                case 5:
                    {
                        if (version < 34)
                            reader.ReadTimeSpan();
                        goto case 4;
                    }
                case 4:
                    {
                        m_LastJusticeLoss = reader.ReadDeltaTime();
                        m_JusticeProtectors = reader.ReadStrongMobileList();
                        goto case 3;
                    }
                case 3:
                    {
                        m_LastSacrificeGain = reader.ReadDeltaTime();
                        m_LastSacrificeLoss = reader.ReadDeltaTime();
                        m_AvailableResurrects = reader.ReadInt();
                        goto case 2;
                    }
                case 2:
                    {
                        m_Flags = (PlayerFlag)reader.ReadInt();
                        goto case 1;
                    }
                case 1:
                    {
                        m_LongTermElapse = reader.ReadTimeSpan();
                        m_ShortTermElapse = reader.ReadTimeSpan();
                        m_GameTime = reader.ReadTimeSpan();
                        goto case 0;
                    }
                case 0:
                    {
                        if (version < 26)
                        {
                            m_AutoStabled = new List<Mobile>();
                        }
                        break;
                    }
            }

            SetDefaultTemplate();

            if (version < 29)
            {
                m_SSNextSeed = m_SSSeedExpire = DateTime.UtcNow;
                m_SSSeedLocation = Point3D.Zero;
            }

            if (m_RecentlyReported == null)
            {
                m_RecentlyReported = new List<Mobile>();
            }

            #region Mondain's Legacy
            if (m_Quests == null)
            {
                m_Quests = new List<BaseQuest>();
            }

            if (m_Chains == null)
            {
                m_Chains = new Dictionary<QuestChain, BaseChain>();
            }

            if (m_DoneQuests == null)
            {
                m_DoneQuests = new List<QuestRestartInfo>();
            }

            if (m_Collections == null)
            {
                m_Collections = new Dictionary<Collection, int>();
            }

            if (m_RewardTitles == null)
            {
                m_RewardTitles = new List<object>();
            }
            #endregion

            // Professions weren't verified on 1.0 RC0
            if (!CharacterCreation.VerifyProfession(m_Profession))
            {
                m_Profession = 0;
            }

            if (m_PermaFlags == null)
            {
                m_PermaFlags = new List<Mobile>();
            }

            if (m_JusticeProtectors == null)
            {
                m_JusticeProtectors = new List<Mobile>();
            }

            if (m_GuildRank == null)
            {
                m_GuildRank = RankDefinition.Member;
                //Default to member if going from older version to new version (only time it should be null)
            }

            if (m_LastOnline == DateTime.MinValue && Account != null)
            {
                m_LastOnline = ((Account)Account).LastLogin;
            }

            if (m_ChampionTitles == null)
            {
                m_ChampionTitles = new ChampionTitleInfo();
            }

            var list = Stabled;

            for (int i = 0; i < list.Count; ++i)
            {
                BaseCreature bc = list[i] as BaseCreature;

                if (bc != null)
                {
                    bc.IsStabled = true;
                    bc.StabledBy = this;
                }
            }

            CheckAtrophies(this);

            if (Hidden) //Hiding is the only buff where it has an effect that's serialized.
            {
                AddBuff(new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655));
            }

            if (_BlessedItem != null)
            {
                Timer.DelayCall(
                b =>
                {
                    if (_BlessedItem == b && b.RootParent != this)
                    {
                        _BlessedItem = null;
                    }
                },
                _BlessedItem);
            }

            //if (this.Backpack.MaxItems == 100)
            //    this.Backpack.MaxItems = 125;
        }

        public ObjetoFicha FichaRP = new ObjetoFicha();

        public int ExpProximoNivel()
        {
            var V = (int)Math.Pow(Nivel * 10, 2);
            if (V <= 0)
                V = 1;

            return Shard.AVENTURA ? V / 10 : V;
        }

        public override void Serialize(GenericWriter writer)
        {
            //cleanup our anti-macro table
            foreach (Hashtable t in m_AntiMacroTable.Values)
            {
                ArrayList remove = new ArrayList();
                foreach (CountAndTimeStamp time in t.Values)
                {
                    if (time.TimeStamp + SkillCheck.AntiMacroExpire <= DateTime.UtcNow)
                    {
                        remove.Add(time);
                    }
                }

                for (int i = 0; i < remove.Count; ++i)
                {
                    t.Remove(remove[i]);
                }
            }

            CheckKillDecay();
            CheckAtrophies(this);
            base.Serialize(writer);
            writer.Write(55); // version
            writer.Write(m_EventCalendarAccount);
            writer.Write(BonusPeso);
            writer.Write(PassoWispGuia);
            DungeonsCompletasSerializer.Serialize(this, writer);
            Elementos.Serialize(writer);
            writer.Write(PontosTalento);
            writer.Write(Nivel);
            writer.Write(ExpTotal);
            Talentos.Serialize(writer);
            FichaRP.Serialize(writer);
            writer.Write(Vidas);
            writer.Write(Famoso);
            writer.Write(LastDungeonEntrance);
            writer.Write(NivelBanco);
            writer.Write(CurrentTemplate);
            Templates.Serialize(writer);
            writer.WriteMobile<Mobile>(DungeonMount);
            writer.Write(CampfireLocations);
            writer.Write((int)m_ExtendedFlags);

            writer.Write(RewardStableSlots);

            if (_BlessedItem != null && _BlessedItem.RootParent != this)
            {
                _BlessedItem = null;
            }

            writer.Write(_BlessedItem);

            writer.Write((int)m_ExploringTheDeepQuest);

            // Version 31/32 Titles
            writer.Write(DisplayGuildTitle);
            writer.Write(m_FameKarmaTitle);
            writer.Write(m_PaperdollSkillTitle);
            writer.Write(m_OverheadTitle);
            writer.Write(m_SubtitleSkillTitle);
            writer.Write(m_CurrentChampTitle);
            writer.Write(m_CurrentVeteranTitle);

            // Version 30 open to take out old Queens Loyalty Info

            // Version 29
            writer.Write(m_GauntletPoints);

            #region Plant System
            writer.Write(m_SSNextSeed);
            writer.Write(m_SSSeedExpire);
            writer.Write(m_SSSeedLocation);
            writer.Write(m_SSSeedMap);
            #endregion

            writer.Write(m_VASTotalMonsterFame);

            #region Mondain's Legacy
            QuestWriter.Quests(writer, m_Quests);
            QuestWriter.Chains(writer, m_Chains);

            if (m_Collections == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(m_Collections.Count);

                foreach (var pair in m_Collections)
                {
                    writer.Write((int)pair.Key);
                    writer.Write(pair.Value);
                }
            }

            if (m_RewardTitles == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(m_RewardTitles.Count);

                for (int i = 0; i < m_RewardTitles.Count; i++)
                {
                    QuestWriter.Object(writer, m_RewardTitles[i]);
                }
            }

            writer.Write(m_SelectedTitle);
            #endregion

            // Version 28
            writer.Write(m_PeacedUntil);
            writer.Write(m_AnkhNextUse);
            writer.Write(m_AutoStabled, true);

            if (m_AcquiredRecipes == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(m_AcquiredRecipes.Count);

                foreach (var kvp in m_AcquiredRecipes)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
            }

            writer.WriteDeltaTime(m_LastHonorLoss);

            ChampionTitleInfo.Serialize(writer, m_ChampionTitles);

            writer.Write(m_LastValorLoss);
            writer.WriteEncodedInt(m_ToTItemsTurnedIn);
            writer.Write(m_ToTTotalMonsterFame); //This ain't going to be a small #.

            writer.WriteEncodedInt(m_AllianceMessageHue);
            writer.WriteEncodedInt(m_GuildMessageHue);

            writer.WriteEncodedInt(m_GuildRank.Rank);
            writer.Write(m_LastOnline);

            writer.WriteEncodedInt((int)m_SolenFriendship);

            QuestSerializer.Serialize(m_Quest, writer);

            if (m_DoneQuests == null)
            {
                writer.WriteEncodedInt(0);
            }
            else
            {
                writer.WriteEncodedInt(m_DoneQuests.Count);

                for (int i = 0; i < m_DoneQuests.Count; ++i)
                {
                    QuestRestartInfo restartInfo = m_DoneQuests[i];

                    QuestSerializer.Write(restartInfo.QuestType, QuestSystem.QuestTypes, writer);
                    writer.Write(restartInfo.RestartTime);
                }
            }

            writer.WriteEncodedInt(m_Profession);

            writer.WriteDeltaTime(m_LastCompassionLoss);

            writer.WriteEncodedInt(m_CompassionGains);

            if (m_CompassionGains > 0)
            {
                writer.WriteDeltaTime(m_NextCompassionDay);
            }

            bool useMods = (m_HairModID != -1 || m_BeardModID != -1);

            writer.Write(useMods);

            if (useMods)
            {
                writer.Write(m_HairModID);
                writer.Write(m_HairModHue);
                writer.Write(m_BeardModID);
                writer.Write(m_BeardModHue);
            }

            writer.Write(SavagePaintExpiration);

            writer.Write((int)m_NpcGuild);
            writer.Write(m_NpcGuildJoinTime);
            writer.Write(m_NpcGuildGameTime);

            writer.Write(m_PermaFlags, true);

            writer.WriteDeltaTime(m_LastJusticeLoss);
            writer.Write(m_JusticeProtectors, true);

            writer.WriteDeltaTime(m_LastSacrificeGain);
            writer.WriteDeltaTime(m_LastSacrificeLoss);
            writer.Write(m_AvailableResurrects);

            writer.Write((int)m_Flags);

            writer.Write(m_LongTermElapse);
            writer.Write(m_ShortTermElapse);
            writer.Write(GameTime);
        }
        #endregion

        public static void CheckAtrophies(Mobile m)
        {
            SacrificeVirtue.CheckAtrophy(m);
            JusticeVirtue.CheckAtrophy(m);
            CompassionVirtue.CheckAtrophy(m);
            ValorVirtue.CheckAtrophy(m);

            if (m is PlayerMobile)
            {
                ChampionTitleInfo.CheckAtrophy((PlayerMobile)m);
            }
        }

        public void CheckKillDecay()
        {
            if (m_ShortTermElapse < GameTime)
            {
                m_ShortTermElapse += TimeSpan.FromHours(5);
                if (ShortTermMurders > 0)
                {
                    --ShortTermMurders;
                }
            }

            if (m_LongTermElapse < GameTime)
            {
                m_LongTermElapse += TimeSpan.FromHours(40);
                if (Kills > 0)
                {
                    --Kills;
                }
            }
        }


        public void ResetKillTime()
        {
            m_ShortTermElapse = GameTime + TimeSpan.FromHours(8);
            m_LongTermElapse = GameTime + TimeSpan.FromHours(40);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SessionStart { get { return m_SessionStart; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan GameTime
        {
            get
            {
                if (NetState != null)
                {
                    return m_GameTime + (DateTime.UtcNow - m_SessionStart);
                }
                else
                {
                    return m_GameTime;
                }
            }
        }

        public override bool CanSee(Mobile m, bool los = false)
        {
            if (m is IConditionalVisibility v && !v.CanBeSeenBy(this))
                return false;

            if (m != this && !m.VisivelPraOutrosPlayers)
                return false;
            /*
            if (!Alive && m != this)
            {
                // Nao ve mobs em dg
                if (!m.Player)
                    return !(Region is DungeonRegion);

                // Soh ve players da sua guilda ou party
                return m.Player && ((Party != null && Party == m.Party) || (Guild != null && Guild == m.Guild));
            }
            */

            if (!Alive && m != this)
            {
                // Jogadores mortos só podem ver a si mesmos, membros da party, guilda ou criaturas BaseHealer
                return (m is BaseHealer) || m.IsControlledBy(this) || (m.Player && ((Party != null && Party == m.Party) || (Guild != null && Guild == m.Guild)));
            }

            if (m is BaseCreature)
            {
                if (m.Hidden)
                {
                    if (m.IsControlledBy(this))
                    {
                        return true;
                    }
                }
            }


            if (m is CharacterStatue)
            {
                ((CharacterStatue)m).OnRequestedAnimation(this);
            }

            if (m is PlayerMobile && ((PlayerMobile)m).m_VisList.Contains(this))
            {
                return true;
            }

            return base.CanSee(m, los);
        }

        public override bool CanSee(Item item)
        {
            if (item is IConditionalVisibility && !((IConditionalVisibility)item).CanBeSeenBy(this))
                return false;

            if (!Alive && Region is DungeonRegion)
            {
                return item.Visible && !(item is Container) && !item.IsArtifact;
            }

            if (m_DesignContext != null && m_DesignContext.Foundation.IsHiddenToCustomizer(this, item))
            {
                return false;
            }
            else if (AccessLevel <= AccessLevel.VIP)
            {
                Region r = item.GetRegion();

                if (r is BaseRegion && !((BaseRegion)r).CanSee(this, item))
                {
                    return false;
                }
            }

            return base.CanSee(item);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            Instances.Remove(this);

            Faction faction = Faction.Find(this);

            if (faction != null)
            {
                faction.RemoveMember(this);
            }

            try
            {
                BaseHouse.HandleDeletion(this);
            }
            catch (Exception e)
            {

            }

            DisguiseTimers.RemoveTimer(this);
        }

        public override bool NewGuildDisplay { get { return Guilds.Guild.NewGuildSystem; } }

        public delegate void PlayerPropertiesEventHandler(PlayerPropertiesEventArgs e);

        public static event PlayerPropertiesEventHandler PlayerProperties;

        public class PlayerPropertiesEventArgs : EventArgs
        {
            public PlayerMobile Player = null;
            public ObjectPropertyList PropertyList = null;

            public PlayerPropertiesEventArgs(PlayerMobile player, ObjectPropertyList list)
            {
                Player = player;
                PropertyList = list;
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);


            if (m_SubtitleSkillTitle != null)
                list.Add(1042971, m_SubtitleSkillTitle);

            if (m_CurrentVeteranTitle > 0)
                list.Add(m_CurrentVeteranTitle);


            #region Mondain's Legacy Titles
            if (m_RewardTitles != null && m_SelectedTitle > -1)
            {
                if (m_SelectedTitle < m_RewardTitles.Count)
                {
                    if (m_RewardTitles[m_SelectedTitle] is int)
                    {
                        string cust = null;

                        if ((int)m_RewardTitles[m_SelectedTitle] == 1154017 && Server.Engines.CityLoyalty.CityLoyaltySystem.HasCustomTitle(this, out cust))
                        {
                            list.Add(1154017, cust); // ~1_TITLE~ of ~2_CITY~
                        }
                        else
                            list.Add((int)m_RewardTitles[m_SelectedTitle]);
                    }
                    else if (m_RewardTitles[m_SelectedTitle] is string)
                    {
                        list.Add(1070722, (string)m_RewardTitles[m_SelectedTitle]);
                    }
                }
            }
            #endregion

            if (Map == Faction.Facet)
            {
                PlayerState pl = PlayerState.Find(this);

                if (pl != null)
                {
                    Faction faction = pl.Faction;

                    if (faction.Commander == this)
                    {
                        list.Add(1042733, faction.Definition.PropName); // Commanding Lord of the ~1_FACTION_NAME~
                    }
                    else if (pl.Sheriff != null)
                    {
                        list.Add(1042734, "{0}\t{1}", pl.Sheriff.Definition.FriendlyName, faction.Definition.PropName);
                        // The Sheriff of  ~1_CITY~, ~2_FACTION_NAME~
                    }
                    else if (pl.Finance != null)
                    {
                        list.Add(1042735, "{0}\t{1}", pl.Finance.Definition.FriendlyName, faction.Definition.PropName);
                        // The Finance Minister of ~1_CITY~, ~2_FACTION_NAME~
                    }
                    else if (pl.MerchantTitle != MerchantTitle.None)
                    {
                        list.Add(1060776, "{0}\t{1}", MerchantTitles.GetInfo(pl.MerchantTitle).Title, faction.Definition.PropName);
                        // ~1_val~, ~2_val~
                    }
                    else
                    {
                        list.Add(1060776, "{0}\t{1}", pl.Rank.Title, faction.Definition.PropName); // ~1_val~, ~2_val~
                    }
                }
            }

            if (Core.ML)
            {
                for (int i = AllFollowers.Count - 1; i >= 0; i--)
                {
                    BaseCreature c = AllFollowers[i] as BaseCreature;

                    if (c != null && c.ControlOrder == OrderType.Guard)
                    {
                        list.Add(501129); // guarded
                        break;
                    }
                }
            }

            if (TestCenter.Enabled && Core.TOL)
            {
                Server.Engines.VvV.VvVPlayerEntry entry = Server.Engines.Points.PointsSystem.ViceVsVirtue.GetPlayerEntry<Server.Engines.VvV.VvVPlayerEntry>(this);

                list.Add(String.Format("Kills: {0} / Deaths: {1} / Assists: {2}", // no cliloc for this!
                    entry == null ? "0" : entry.Kills.ToString(), entry == null ? "0" : entry.Deaths.ToString(), entry == null ? "0" : entry.Assists.ToString()));

                list.Add(1060415, AosAttributes.GetValue(this, AosAttribute.AttackChance).ToString()); // hit chance increase ~1_val~%
                list.Add(1060408, AosAttributes.GetValue(this, AosAttribute.DefendChance).ToString()); // defense chance increase ~1_val~%
                list.Add(1060486, AosAttributes.GetValue(this, AosAttribute.WeaponSpeed).ToString()); // swing speed increase ~1_val~%
                list.Add(1060401, AosAttributes.GetValue(this, AosAttribute.WeaponDamage).ToString()); // damage increase ~1_val~%
                list.Add(1060483, AosAttributes.GetValue(this, AosAttribute.SpellDamage).ToString()); // spell damage increase ~1_val~%
                list.Add(1060433, AosAttributes.GetValue(this, AosAttribute.LowerManaCost).ToString()); // lower mana cost
            }

            if (PlayerProperties != null)
            {
                PlayerProperties(new PlayerPropertiesEventArgs(this, list));
            }
        }

        public string GetLetraSexo()
        {
            return Female ? "a" : "o";
        }

        public override void OnSingleClick(Mobile from)
        {
            if (Rope.Preso(this))
                this.PrivateOverheadMessage(MessageType.Regular, 0x35, true, "* amarrad" + GetLetraSexo() + " *", from.NetState);

            if (!Shard.RP && Shard.TITULOS_RP)
            {
                if (this.RP)
                {
                    this.PrivateOverheadMessage(MessageType.Regular, 0x35, true, "[ Personagem RP ]", from.NetState);
                }

                if (from != null && !from.RP && this.RP && !from.IsCooldown("avisorp"))
                {
                    from.SetCooldown("avisorp", TimeSpan.FromMinutes(10));
                    from.SendMessage(78, "[ATENCAO] Voce esta se aproximando de jogadores em modo RP. Atrapalhar jogadores RP podera resultar em Jail e ate Ban !");
                }
            }


            /*
            if (Map == Faction.Facet)
            {
                PlayerState pl = PlayerState.Find(this);

                if (pl != null)
                {
                    string text;
                    bool ascii = false;

                    Faction faction = pl.Faction;

                    if (faction.Commander == this)
                    {
                        text = String.Concat(
                            Female ? "(Commanding Lady of the " : "(Commanding Lord of the ", faction.Definition.FriendlyName, ")");
                    }
                    else if (pl.Sheriff != null)
                    {
                        text = String.Concat(
                            "(The Sheriff of ", pl.Sheriff.Definition.FriendlyName, ", ", faction.Definition.FriendlyName, ")");
                    }
                    else if (pl.Finance != null)
                    {
                        text = String.Concat(
                            "(The Finance Minister of ", pl.Finance.Definition.FriendlyName, ", ", faction.Definition.FriendlyName, ")");
                    }
                    else
                    {
                        ascii = true;

                        if (pl.MerchantTitle != MerchantTitle.None)
                        {
                            text = String.Concat(
                                "(", MerchantTitles.GetInfo(pl.MerchantTitle).Title.String, ", ", faction.Definition.FriendlyName, ")");
                        }
                        else
                        {
                            text = String.Concat("(", pl.Rank.Title.String, ", ", faction.Definition.FriendlyName, ")");
                        }
                    }

                    int hue = (Faction.Find(from) == faction ? 98 : 38);

                    PrivateOverheadMessage(MessageType.Label, hue, ascii, text, from.NetState);
                }
            }
            */
            base.OnSingleClick(from);
        }

        public bool Correndo()
        {
            return (this.Direction & Direction.Running) != 0;
        }

        protected override bool OnMove(Direction d)
        {
            if (Party != null && NetState != null)
            {
                Waypoints.UpdateToParty(this);
            }

            BandageContext c = BandageContext.GetContext(this);
            if (RP && c != null && !Talentos.Tem(Talento.Curandeiro) && Utility.RandomDouble() < 0.25)
            {
                c.Slip();
            }

            if (RP && Mounted && !Talentos.Tem(Talento.Hipismo) && (d & Direction.Running) != 0)
            {
                if (Utility.RandomDouble() < 0.01)
                {
                    this.Mount.Rider = null;
                    OverheadMessage("* caiu *");
                    SendMessage("Voce caiu da sua montaria");
                    AOS.Damage(this, Utility.Random(5, 10), DamageType.Melee);
                    this.PlayHurtSound();
                    this.PlayDamagedAnimation();
                    this.Paralyze(TimeSpan.FromSeconds(1));
                }
            }

            if (PokerGame != null)
            {
                CloseGump(typeof(PokerLeaveGump));
                SendGump(new PokerLeaveGump(PokerGame));
                return false;
            }

            if (IsStaff())
            {
                return true;
            }

            if (Hidden && DesignContext.Find(this) == null) //Hidden & NOT customizing a house
            {
                if (Skills.Stealth.Value >= 25.0)
                {
                    bool running = (d & Direction.Running) != 0;

                    if (running)
                    {
                        if ((AllowedStealthSteps -= 2) <= 0)
                        {
                            RevealingAction(false);
                        }
                    }
                    else if (Shard.RP && Mounted && AllowedStealthSteps > 0)
                    {
                        var mount = this.Mount;
                        this.Mount.Rider = null;
                        if (mount is BaseMount)
                        {
                            var bc = (BaseMount)mount;
                            bc.ControlOrder = OrderType.Stay;
                            bc.Hidden = true;
                            bc.OverheadMessage("* deixou a montaria escondida *");
                        }
                    }

                    else if (AllowedStealthSteps-- < 0)
                    {
                        //Stealth.OnUse(this);
                        RevealingAction(false);
                    }
                }
                else
                {
                    RevealingAction(false);
                }
            }

            #region Mondain's Legacy
            if (InvisibilityPotion.HasTimer(this))
            {
                InvisibilityPotion.Iterrupt(this);
            }
            #endregion

            return true;
        }

        public bool BedrollLogout { get; set; }
        public bool BlanketOfDarknessLogout { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool Paralyzed
        {
            get { return base.Paralyzed; }
            set
            {
                base.Paralyzed = value;
                if (value)
                {
                    var bands = BandageContext.GetContext(this);
                    if (bands != null)
                    {
                        SendMessage("Voce parou de se curar por ter sido paralizado");
                        bands.StopHeal();
                    }
                    AddBuff(new BuffInfo(BuffIcon.Paralyze, 1075827)); //Paralyze/You are frozen and can not move
                }
                else
                {
                    RemoveBuff(BuffIcon.Paralyze);
                }
            }
        }

        #region Ethics
        private Player m_EthicPlayer;

        [CommandProperty(AccessLevel.GameMaster)]
        public Player EthicPlayer { get { return m_EthicPlayer; } set { m_EthicPlayer = value; } }
        #endregion

        #region Factions
        public PlayerState FactionPlayerState { get; set; }
        #endregion

        #region Quests
        private QuestSystem m_Quest;
        private List<QuestRestartInfo> m_DoneQuests;
        private SolenFriendship m_SolenFriendship;

        public QuestSystem Quest { get { return m_Quest; } set { m_Quest = value; } }

        public List<QuestRestartInfo> DoneQuests { get { return m_DoneQuests; } set { m_DoneQuests = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public SolenFriendship SolenFriendship { get { return m_SolenFriendship; } set { m_SolenFriendship = value; } }
        #endregion

        #region Mondain's Legacy
        private List<BaseQuest> m_Quests;
        private Dictionary<QuestChain, BaseChain> m_Chains;

        public List<BaseQuest> Quests { get { return m_Quests; } }

        public Dictionary<QuestChain, BaseChain> Chains { get { return m_Chains; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Peaced
        {
            get
            {
                if (m_PeacedUntil > DateTime.UtcNow)
                {
                    return true;
                }

                return false;
            }
        }

        private Dictionary<Collection, int> m_Collections;
        private List<object> m_RewardTitles;
        private int m_SelectedTitle;

        public Dictionary<Collection, int> Collections { get { return m_Collections; } }

        public List<object> RewardTitles { get { return m_RewardTitles; } }

        public int SelectedTitle { get { return m_SelectedTitle; } }

        public bool RemoveRewardTitle(object o, bool silent)
        {
            if (m_RewardTitles.Contains(o))
            {
                int i = m_RewardTitles.IndexOf(o);

                if (i == m_SelectedTitle)
                    SelectRewardTitle(-1, silent);
                else if (i > m_SelectedTitle)
                    SelectRewardTitle(m_SelectedTitle - 1, silent);

                m_RewardTitles.Remove(o);

                return true;
            }

            return false;
        }

        public int GetCollectionPoints(Collection collection)
        {
            if (m_Collections == null)
            {
                m_Collections = new Dictionary<Collection, int>();
            }

            int points = 0;

            if (m_Collections.ContainsKey(collection))
            {
                m_Collections.TryGetValue(collection, out points);
            }

            return points;
        }

        public void AddCollectionPoints(Collection collection, int points)
        {
            if (m_Collections == null)
            {
                m_Collections = new Dictionary<Collection, int>();
            }

            if (m_Collections.ContainsKey(collection))
            {
                m_Collections[collection] += points;
            }
            else
            {
                m_Collections.Add(collection, points);
            }
        }

        public void SelectRewardTitle(int num, bool silent = false)
        {
            if (num == -1)
            {
                m_SelectedTitle = num;

                if (!silent)
                    SendLocalizedMessage(1074010); // You elect to hide your Reward Title.
            }
            else if (num < m_RewardTitles.Count && num >= -1)
            {
                if (m_SelectedTitle != num)
                {
                    m_SelectedTitle = num;

                    if (m_RewardTitles[num] is string && !silent)
                    {
                        SendMessage("Titulo alterado para " + (string)m_RewardTitles[num]); // You change your Reward Title to "~1_TITLE~".
                    }
                }
                else if (!silent)
                {
                    SendMessage("Deixei o titulo quieto"); // You decide to leave your title as it is.
                }
            }

            InvalidateProperties();
        }

        public bool AddRewardTitle(object title)
        {
            if (m_RewardTitles == null)
            {
                m_RewardTitles = new List<object>();
            }

            if (title != null && !m_RewardTitles.Contains(title))
            {
                m_RewardTitles.Add(title);

                InvalidateProperties();
                return true;
            }

            return false;
        }

        public void ShowChangeTitle()
        {
            SendGump(new SelectTitleGump(this, m_SelectedTitle));
        }
        #endregion

        #region Titles
        private string m_FameKarmaTitle;
        private string m_PaperdollSkillTitle;
        private string m_SubtitleSkillTitle;
        private string m_CurrentChampTitle;
        private string m_OverheadTitle;
        private int m_CurrentVeteranTitle;

        [CommandProperty(AccessLevel.CoOwner)]
        public string FameKarmaTitle
        {
            get { return m_FameKarmaTitle; }
            set { m_FameKarmaTitle = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.CoOwner)]
        public string PaperdollSkillTitle
        {
            get { return m_PaperdollSkillTitle; }
            set { m_PaperdollSkillTitle = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.CoOwner)]
        public string SubtitleSkillTitle
        {
            get { return m_SubtitleSkillTitle; }
            set { m_SubtitleSkillTitle = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.CoOwner)]
        public string CurrentChampTitle
        {
            get { return m_CurrentChampTitle; }
            set { m_CurrentChampTitle = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.CoOwner)]
        public string OverheadTitle
        {
            get { return m_OverheadTitle; }
            set { m_OverheadTitle = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.CoOwner)]
        public int CurrentVeteranTitle
        {
            get { return m_CurrentVeteranTitle; }
            set { m_CurrentVeteranTitle = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.CoOwner)]
        public override bool ShowAccessTitle
        {
            get
            {
                switch (AccessLevel)
                {
                    case AccessLevel.VIP:
                    case AccessLevel.Counselor:
                    case AccessLevel.GameMaster:
                    case AccessLevel.Seer:
                        return true;
                }

                return false;
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            string name = Name;

            if (name == null)
            {
                name = String.Empty;
            }

            string prefix = "";
            bool lord = Fame >= 35000;
            if (RP)
            {
                prefix = FichaRP.Patente.ToString() + " ";
            }

            if (lord)
            {
                DaFama();
                prefix += Female ? "Lady" : "Lord";
                if (RankingFama < 10)
                {
                    prefix = (Female ? "A Famosa " : "O Famoso ") + prefix;
                }
            }

            if (lord)
                list.Add(Female ? "Poderosa Lady" : "Poderoso Lord");

            string suffix = "";

            if (PropertyTitle && Title != null && Title.Length > 0)
            {
                suffix = Title;
            }

            BaseGuild guild = Guild;
            bool vvv = Server.Engines.VvV.ViceVsVirtueSystem.IsVvV(this) && (ViceVsVirtueSystem.EnhancedRules || this.Map == Faction.Facet);


            if (!vvv && m_OverheadTitle != null)
            {
                int loc = Utility.ToInt32(m_OverheadTitle);

                if (loc > 0)
                {
                    if (Server.Engines.CityLoyalty.CityLoyaltySystem.ApplyCityTitle(this, list, prefix, loc))
                        return;
                }
                else if (suffix.Length > 0)
                    suffix = String.Format("{0} {1}", suffix, m_OverheadTitle);
                else
                    suffix = String.Format("{0}", m_OverheadTitle);
            }
            else if (vvv || (guild != null && DisplayGuildAbbr))
            {
                if (vvv)
                {
                    if (guild != null && DisplayGuildAbbr)
                        suffix = String.Format("[{0}] [GW]", Utility.FixHtml(guild.Abbreviation));
                    else
                        suffix = "[GW]";
                }
                else if (suffix.Length > 0)
                    suffix = String.Format("{0} [{1}]", suffix, Utility.FixHtml(guild.Abbreviation));
                else
                    suffix = String.Format("[{0}]", Utility.FixHtml(guild.Abbreviation));
            }

            suffix = ApplyNameSuffix(suffix);

            list.Add(1050045, "{0} \t{1}\t {2}", prefix, name, suffix); // ~1_PREFIX~~2_NAME~~3_SUFFIX~
            if (guild != null)
            {
                list.Add(guild.Name);
            }

            if (guild != null && DisplayGuildTitle)
            {
                string title = GuildTitle;

                if (title == null)
                {
                    title = "";
                }
                else
                {
                    title = title.Trim();
                }

                if (title.Length > 0)
                {
                    list.Add("{0}, {1}", Utility.FixHtml(title), Utility.FixHtml(guild.Name));
                }
            }

            if (Rope.Preso(this))
                list.Add(Gump.Cor("<CENTER>[ Amarrad" + GetLetraSexo() + " ]</CENTER>", "red"));

            if (Elemento != ElementoPvM.None)
                list.Add(Gump.Cor(Elemento.ToString() + " level " + Elementos.GetNivel(Elemento), BaseArmor.CorElemento(Elemento)));

            if (Shard.TITULOS_RP && RP && !Shard.RP)
            {
                list.Add(Gump.Cor("<CENTER>[ Personagem RP ]</CENTER>", "yellow"));
                if (Deaths >= 5)
                    list.Add(Gump.Cor("<CENTER>MORTO</CENTER>", "red"));

            }
            else if (Shard.RP)
            {
                if (Deaths >= 5)
                    list.Add(Gump.Cor("<CENTER>MORTO</CENTER>", "red"));
            }
        }
        #endregion

        public override void OnKillsChange(int oldValue)
        {
            if (Young && Kills > oldValue)
            {
                Account acc = Account as Account;

                if (acc != null)
                {
                    acc.RemoveYoungStatus(0);
                }
            }
        }

        public override void OnKarmaChange(int oldValue)
        {
            EpiphanyHelper.OnKarmaChange(this);
        }

        public override void OnExpGain(SkillName skill, ushort exp)
        {
            if (this.Skills[skill].Value < this.Skills[skill].Cap)
            {

                if (!SkillExpGump.NaoMostra.Contains(skill))
                {
                    var gump = typeof(SkillExperienceGump);
                    if (this.HasGump(gump))
                        this.CloseGump(gump);
                    this.SendGump(new SkillExperienceGump(this, skill, exp));
                }
            }

            if (Young && Wisp != null && Skills.Magery.Value >= 50 && !IsCooldown("dicamagia") && Region is DungeonGuardedRegion)
            {
                SetCooldown("dicamagia", TimeSpan.FromSeconds(600));
                Wisp.Fala("Voce pode clicar 2x no seu livro de magias e arrastar icones de magia para fora para usa-las.");
            }
        }

        public override void OnSkillChange(SkillName skill, double oldBase)
        {
            if (skill == SkillName.Camping)
            {
                Delta(MobileDelta.Weight);
            }

            if (Young)
            {
                if (SkillsTotal >= 5500 && (!Core.AOS && Skills[skill].Base >= 80.0))
                {
                    if (this.Wisp != null)
                    {
                        this.Wisp.Delete();
                        this.Wisp = null;
                        SendMessage(78, "Sua fada foi-se embora, pois voce ja esta ficando forte");
                        SendMessage(78, "Enquanto voce tiver (Novato) no seu nome PKs nao podem te matar e voce nao pode ser roubado.");
                    }
                }

            }

            if (skill != SkillName.Alchemy && Skills.CurrentMastery == skill && Skills[skill].Value < MasteryInfo.MinSkillRequirement)
            {
                //SendLocalizedMessage(1156236, String.Format("{0}\t{1}", MasteryInfo.MinSkillRequirement.ToString(), Skills[skill].Info.Name)); // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that mastery.

                SkillName mastery = Skills.CurrentMastery;
                Skills.CurrentMastery = SkillName.Alchemy;

                Server.Spells.SkillMasteries.MasteryInfo.OnMasteryChanged(this, mastery);
            }

            TransformContext context = TransformationSpellHelper.GetContext(this);

            if (context != null)
            {
                TransformationSpellHelper.CheckCastSkill(this, context);
            }
        }

        public override void OnAccessLevelChanged(AccessLevel oldLevel)
        {
            if (IsPlayer())
            {
                IgnoreMobiles = false;
            }
            else
            {
                IgnoreMobiles = true;
            }
        }

        public override void OnDelete()
        {
            Instances.Remove(this);

            if (m_ReceivedHonorContext != null)
            {
                m_ReceivedHonorContext.Cancel();
            }
            if (m_SentHonorContext != null)
            {
                m_SentHonorContext.Cancel();
            }
        }

        #region Fastwalk Prevention
        private static bool FastwalkPrevention = true; // Is fastwalk prevention enabled?

        private static int FastwalkThreshold = 400; // Fastwalk prevention will become active after 0.4 seconds

        private long m_NextMovementTime;
        private bool m_HasMoved;

        public long NextMovementTime { get { return m_NextMovementTime; } }

        public virtual bool UsesFastwalkPrevention { get { return IsPlayer(); } }

        public override int ComputeMovementSpeed(Direction dir, bool checkTurning)
        {
            if (checkTurning && (dir & Direction.Mask) != (Direction & Direction.Mask))
            {
                return RunMount; // We are NOT actually moving (just a direction change)
            }

            TransformContext context = TransformationSpellHelper.GetContext(this);

            if (context != null && context.Type == typeof(ReaperFormSpell))
            {
                return WalkFoot;
            }

            bool running = ((dir & Direction.Running) != 0);

            bool onHorse = Mount != null || Flying;

            AnimalFormContext animalContext = AnimalForm.GetContext(this);

            if (onHorse || (animalContext != null && animalContext.SpeedBoost))
            {
                return (running ? RunMount : WalkMount);
            }

            return (running ? RunFoot : WalkFoot);
        }

        public static bool MovementThrottle_Callback(NetState ns, out bool drop)
        {
            drop = false;

            PlayerMobile pm = ns.Mobile as PlayerMobile;

            if (pm == null || !pm.UsesFastwalkPrevention)
            {
                return true;
            }

            if (!pm.m_HasMoved)
            {
                // has not yet moved
                pm.m_NextMovementTime = Core.TickCount;
                pm.m_HasMoved = true;
                return true;
            }

            long ts = pm.m_NextMovementTime - Core.TickCount;

            if (ts < 0)
            {
                // been a while since we've last moved
                pm.m_NextMovementTime = Core.TickCount;
                return true;
            }

            return (ts < FastwalkThreshold);
        }
        #endregion

        #region Hair and beard mods
        private int m_HairModID = -1, m_HairModHue;
        private int m_BeardModID = -1, m_BeardModHue;

        public void SetHairMods(int hairID, int beardID)
        {
            if (hairID == -1)
            {
                InternalRestoreHair(true, ref m_HairModID, ref m_HairModHue);
            }
            else if (hairID != -2)
            {
                InternalChangeHair(true, hairID, ref m_HairModID, ref m_HairModHue);
            }

            if (beardID == -1)
            {
                InternalRestoreHair(false, ref m_BeardModID, ref m_BeardModHue);
            }
            else if (beardID != -2)
            {
                InternalChangeHair(false, beardID, ref m_BeardModID, ref m_BeardModHue);
            }
        }

        private void CreateHair(bool hair, int id, int hue)
        {
            if (hair)
            {
                //TODO Verification?
                HairItemID = id;
                HairHue = hue;
            }
            else
            {
                FacialHairItemID = id;
                FacialHairHue = hue;
            }
        }

        private void InternalRestoreHair(bool hair, ref int id, ref int hue)
        {
            if (id == -1)
            {
                return;
            }

            if (hair)
            {
                HairItemID = 0;
            }
            else
            {
                FacialHairItemID = 0;
            }

            //if( id != 0 )
            CreateHair(hair, id, hue);

            id = -1;
            hue = 0;
        }

        private void InternalChangeHair(bool hair, int id, ref int storeID, ref int storeHue)
        {
            if (storeID == -1)
            {
                storeID = hair ? HairItemID : FacialHairItemID;
                storeHue = hair ? HairHue : FacialHairHue;
            }
            CreateHair(hair, id, 0);
        }
        #endregion

        #region Virtues
        private DateTime m_LastSacrificeGain;
        private DateTime m_LastSacrificeLoss;
        private int m_AvailableResurrects;

        public DateTime LastSacrificeGain { get { return m_LastSacrificeGain; } set { m_LastSacrificeGain = value; } }
        public DateTime LastSacrificeLoss { get { return m_LastSacrificeLoss; } set { m_LastSacrificeLoss = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AvailableResurrects { get { return m_AvailableResurrects; } set { m_AvailableResurrects = value; } }

        private DateTime m_NextJustAward;
        private DateTime m_LastJusticeLoss;
        private List<Mobile> m_JusticeProtectors;

        public DateTime LastJusticeLoss { get { return m_LastJusticeLoss; } set { m_LastJusticeLoss = value; } }
        public List<Mobile> JusticeProtectors { get { return m_JusticeProtectors; } set { m_JusticeProtectors = value; } }

        private DateTime m_LastCompassionLoss;
        private DateTime m_NextCompassionDay;
        private int m_CompassionGains;

        public DateTime LastCompassionLoss { get { return m_LastCompassionLoss; } set { m_LastCompassionLoss = value; } }
        public DateTime NextCompassionDay { get { return m_NextCompassionDay; } set { m_NextCompassionDay = value; } }
        public int CompassionGains { get { return m_CompassionGains; } set { m_CompassionGains = value; } }

        private DateTime m_LastValorLoss;

        public DateTime LastValorLoss { get { return m_LastValorLoss; } set { m_LastValorLoss = value; } }

        private DateTime m_LastHonorLoss;
        private HonorContext m_ReceivedHonorContext;
        private HonorContext m_SentHonorContext;
        public DateTime m_hontime;

        public DateTime LastHonorLoss { get { return m_LastHonorLoss; } set { m_LastHonorLoss = value; } }

        public DateTime LastHonorUse { get; set; }

        public bool HonorActive { get; set; }

        public HonorContext ReceivedHonorContext { get { return m_ReceivedHonorContext; } set { m_ReceivedHonorContext = value; } }
        public HonorContext SentHonorContext { get { return m_SentHonorContext; } set { m_SentHonorContext = value; } }
        #endregion

        #region Young system
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Young
        {
            get { return GetFlag(PlayerFlag.Young); }
            set
            {
                if (Wisp != null)
                {
                    SendMessage("Como voce nao e mais iniciante, sua fada foi embora para ajudar outros jogadores.");
                    Wisp.Delete();
                }
                SetFlag(PlayerFlag.Young, value);
                InvalidateProperties();
            }
        }

        public override string ApplyNameSuffix(string suffix)
        {
            if (Young)
            {
                if (suffix.Length == 0)
                {
                    suffix = "(Novato)";
                }
                else
                {
                    suffix = String.Concat(suffix, " (Novato)");
                }
            }

            if (HasPvMTag)
            {
                if (suffix.Length == 0)
                {
                    suffix = "(Jogador PvM)";
                }
                else
                {
                    suffix = String.Concat(suffix, " (Jogador PvM)");
                }
            }

            #region Ethics
            if (m_EthicPlayer != null)
            {
                if (suffix.Length == 0)
                {
                    suffix = m_EthicPlayer.Ethic.Definition.Adjunct.String;
                }
                else
                {
                    suffix = String.Concat(suffix, " ", m_EthicPlayer.Ethic.Definition.Adjunct.String);
                }
            }
            #endregion

            if (Core.ML && Map == Faction.Facet)
            {
                Faction faction = Faction.Find(this);

                if (faction != null)
                {
                    string adjunct = String.Format("[{0}]", faction.Definition.Abbreviation);
                    if (suffix.Length == 0)
                    {
                        suffix = adjunct;
                    }
                    else
                    {
                        suffix = String.Concat(suffix, " ", adjunct);
                    }
                }
            }

            return base.ApplyNameSuffix(suffix);
        }

        public override TimeSpan GetLogoutDelay()
        {
            if (BedrollLogout || BlanketOfDarknessLogout || TestCenter.Enabled)
            {
                return TimeSpan.Zero;
            }

            return base.GetLogoutDelay();
        }

        private DateTime m_LastYoungMessage = DateTime.MinValue;

        public bool CheckYoungProtection(Mobile from)
        {
            if (!Young)
            {
                return false;
            }

            if (Region is BaseRegion && !((BaseRegion)Region).YoungProtected)
            {
                return false;
            }

            if (from is BaseCreature && ((BaseCreature)from).IgnoreYoungProtection)
            {
                return false;
            }

            if (Quest != null && Quest.IgnoreYoungProtection(from))
            {
                return false;
            }

            if (DateTime.UtcNow - m_LastYoungMessage > TimeSpan.FromMinutes(1.0))
            {
                m_LastYoungMessage = DateTime.UtcNow;
                SendLocalizedMessage(1019067);
                // A monster looks at you menacingly but does not attack.  You would be under attack now if not for your status as a new citizen of Britannia.
            }

            return true;
        }

        private DateTime m_LastYoungHeal = DateTime.MinValue;

        public bool CheckYoungHealTime()
        {
            if (DateTime.UtcNow - m_LastYoungHeal > TimeSpan.FromMinutes(5.0))
            {
                m_LastYoungHeal = DateTime.UtcNow;
                return true;
            }

            return false;
        }

        private static readonly Point3D[] m_TrammelDeathDestinations = new[]
        {
            new Point3D(1481, 1612, 20), new Point3D(2708, 2153, 0), new Point3D(2249, 1230, 0), new Point3D(5197, 3994, 37),
            new Point3D(1412, 3793, 0), new Point3D(3688, 2232, 20), new Point3D(2578, 604, 0), new Point3D(4397, 1089, 0),
            new Point3D(5741, 3218, -2), new Point3D(2996, 3441, 15), new Point3D(624, 2225, 0), new Point3D(1916, 2814, 0),
            new Point3D(2929, 854, 0), new Point3D(545, 967, 0), new Point3D(3469, 2559, 36)
        };

        private static readonly Point3D[] m_IlshenarDeathDestinations = new[]
        {
            new Point3D(1216, 468, -13), new Point3D(723, 1367, -60), new Point3D(745, 725, -28), new Point3D(281, 1017, 0),
            new Point3D(986, 1011, -32), new Point3D(1175, 1287, -30), new Point3D(1533, 1341, -3), new Point3D(529, 217, -44),
            new Point3D(1722, 219, 96)
        };

        private static readonly Point3D[] m_MalasDeathDestinations = new[]
        {new Point3D(2079, 1376, -70), new Point3D(944, 519, -71)};

        private static readonly Point3D[] m_TokunoDeathDestinations = new[]
        {new Point3D(1166, 801, 27), new Point3D(782, 1228, 25), new Point3D(268, 624, 15)};

        public bool YoungDeathTeleport()
        {
            if (Region.IsPartOf<Jail>() || Region.IsPartOf("Samurai start location") ||
                Region.IsPartOf("Ninja start location") || Region.IsPartOf("Ninja cave"))
            {
                return false;
            }

            Point3D loc;
            Map map;

            DungeonRegion dungeon = (DungeonRegion)Region.GetRegion(typeof(DungeonRegion));
            if (dungeon != null && dungeon.EntranceLocation != Point3D.Zero)
            {
                loc = dungeon.EntranceLocation;
                map = dungeon.EntranceMap;
            }
            else
            {
                loc = Location;
                map = Map;
            }

            Point3D[] list;

            if (map == Map.Trammel)
            {
                list = m_TrammelDeathDestinations;
            }
            else if (map == Map.Ilshenar)
            {
                list = m_IlshenarDeathDestinations;
            }
            else if (map == Map.Malas)
            {
                list = m_MalasDeathDestinations;
            }
            else if (map == Map.Tokuno)
            {
                list = m_TokunoDeathDestinations;
            }
            else
            {
                return false;
            }

            Point3D dest = Point3D.Zero;
            int sqDistance = int.MaxValue;

            for (int i = 0; i < list.Length; i++)
            {
                Point3D curDest = list[i];

                int width = loc.X - curDest.X;
                int height = loc.Y - curDest.Y;
                int curSqDistance = width * width + height * height;

                if (curSqDistance < sqDistance)
                {
                    dest = curDest;
                    sqDistance = curSqDistance;
                }
            }

            MoveToWorld(dest, map);
            return true;
        }

        private void SendYoungDeathNotice()
        {
            SendGump(new YoungDeathNotice());
        }
        #endregion

        #region Speech
        private SpeechLog m_SpeechLog;
        private bool m_TempSquelched;

        public SpeechLog SpeechLog { get { return m_SpeechLog; } }

        [CommandProperty(AccessLevel.Administrator)]
        public bool TempSquelched { get { return m_TempSquelched; } set { m_TempSquelched = value; } }

        public override void OnSpeech(SpeechEventArgs e)
        {
            if (SpeechLog.Enabled && NetState != null)
            {
                if (m_SpeechLog == null)
                {
                    m_SpeechLog = new SpeechLog();
                }

                m_SpeechLog.Add(e.Mobile, e.Speech);
            }
        }

        public override void OnSaid(SpeechEventArgs e)
        {
            if (m_TempSquelched)
            {
                if (Core.ML)
                {
                    SendLocalizedMessage(500168); // You can not say anything, you have been muted.
                }
                else
                {
                    SendMessage("You can not say anything, you have been squelched."); //Cliloc ITSELF changed during ML.
                }

                e.Blocked = true;
            }
            else
            {
                base.OnSaid(e);
            }
        }
        #endregion

        #region Player PvM
        //Adds a new flag for PlayerKill protection when inside a dungeon
        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasPvMTag
        {
            get { return GetFlag(PlayerFlag.HasPvMTag); }
            set
            {
                SetFlag(PlayerFlag.HasPvMTag, value);
                SendMessage("Agora você está imune a PvP dentro de Dungeons.");
                InvalidateProperties();
            }
        }
        #endregion


        #region Champion Titles
        [CommandProperty(AccessLevel.GameMaster)]
        public bool DisplayChampionTitle { get { return GetFlag(PlayerFlag.DisplayChampionTitle); } set { SetFlag(PlayerFlag.DisplayChampionTitle, value); } }

        private ChampionTitleInfo m_ChampionTitles;

        [CommandProperty(AccessLevel.GameMaster)]
        public ChampionTitleInfo ChampionTitles { get { return m_ChampionTitles; } set { } }

        private void ToggleChampionTitleDisplay()
        {
            if (!CheckAlive())
            {
                return;
            }

            if (DisplayChampionTitle)
            {
                SendLocalizedMessage(1062419, "", 0x23); // You have chosen to hide your monster kill title.
            }
            else
            {
                SendLocalizedMessage(1062418, "", 0x23); // You have chosen to display your monster kill title.
            }

            DisplayChampionTitle = !DisplayChampionTitle;
        }

        [PropertyObject]
        public class ChampionTitleInfo
        {
            public static TimeSpan LossDelay = TimeSpan.FromDays(1.0);
            public const int LossAmount = 90;

            private class TitleInfo
            {
                private int m_Value;
                private DateTime m_LastDecay;

                public int Value { get { return m_Value; } set { m_Value = value; } }
                public DateTime LastDecay { get { return m_LastDecay; } set { m_LastDecay = value; } }

                public TitleInfo()
                { }

                public TitleInfo(GenericReader reader)
                {
                    int version = reader.ReadEncodedInt();

                    switch (version)
                    {
                        case 0:
                            {
                                m_Value = reader.ReadEncodedInt();
                                m_LastDecay = reader.ReadDateTime();
                                break;
                            }
                    }
                }

                public static void Serialize(GenericWriter writer, TitleInfo info)
                {
                    writer.WriteEncodedInt(0); // version

                    writer.WriteEncodedInt(info.m_Value);
                    writer.Write(info.m_LastDecay);
                }
            }

            private TitleInfo[] m_Values;

            private int m_Harrower; //Harrower titles do NOT decay

            public int GetValue(ChampionSpawnType type)
            {
                return GetValue((int)type);
            }

            public void SetValue(ChampionSpawnType type, int value)
            {
                SetValue((int)type, value);
            }

            public void Award(ChampionSpawnType type, int value)
            {
                Award((int)type, value);
            }

            public int GetValue(int index)
            {
                if (m_Values == null || index < 0 || index >= m_Values.Length)
                {
                    return 0;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                return m_Values[index].Value;
            }

            public DateTime GetLastDecay(int index)
            {
                if (m_Values == null || index < 0 || index >= m_Values.Length)
                {
                    return DateTime.MinValue;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                return m_Values[index].LastDecay;
            }

            public void SetValue(int index, int value)
            {
                if (m_Values == null)
                {
                    m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                if (value < 0)
                {
                    value = 0;
                }

                if (index < 0 || index >= m_Values.Length)
                {
                    return;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                m_Values[index].Value = value;
            }

            public void Award(int index, int value)
            {
                if (m_Values == null)
                {
                    m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                if (index < 0 || index >= m_Values.Length || value <= 0)
                {
                    return;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                m_Values[index].Value += value;
            }

            public void Atrophy(int index, int value)
            {
                if (m_Values == null)
                {
                    m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                if (index < 0 || index >= m_Values.Length || value <= 0)
                {
                    return;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                int before = m_Values[index].Value;

                if ((m_Values[index].Value - value) < 0)
                {
                    m_Values[index].Value = 0;
                }
                else
                {
                    m_Values[index].Value -= value;
                }

                if (before != m_Values[index].Value)
                {
                    m_Values[index].LastDecay = DateTime.UtcNow;
                }
            }

            public override string ToString()
            {
                return "...";
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Abyss { get { return GetValue(ChampionSpawnType.Abyss); } set { SetValue(ChampionSpawnType.Abyss, value); } }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Arachnid { get { return GetValue(ChampionSpawnType.Arachnid); } set { SetValue(ChampionSpawnType.Arachnid, value); } }

            [CommandProperty(AccessLevel.GameMaster)]
            public int ColdBlood { get { return GetValue(ChampionSpawnType.ColdBlood); } set { SetValue(ChampionSpawnType.ColdBlood, value); } }

            [CommandProperty(AccessLevel.GameMaster)]
            public int ForestLord { get { return GetValue(ChampionSpawnType.ForestLord); } set { SetValue(ChampionSpawnType.ForestLord, value); } }

            [CommandProperty(AccessLevel.GameMaster)]
            public int SleepingDragon { get { return GetValue(ChampionSpawnType.SleepingDragon); } set { SetValue(ChampionSpawnType.SleepingDragon, value); } }

            [CommandProperty(AccessLevel.GameMaster)]
            public int UnholyTerror { get { return GetValue(ChampionSpawnType.UnholyTerror); } set { SetValue(ChampionSpawnType.UnholyTerror, value); } }

            [CommandProperty(AccessLevel.GameMaster)]
            public int VerminHorde { get { return GetValue(ChampionSpawnType.VerminHorde); } set { SetValue(ChampionSpawnType.VerminHorde, value); } }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Harrower { get { return m_Harrower; } set { m_Harrower = value; } }

            #region Mondain's Legacy Peerless Champion
            [CommandProperty(AccessLevel.GameMaster)]
            public int Glade { get { return GetValue(ChampionSpawnType.Glade); } set { SetValue(ChampionSpawnType.Glade, value); } }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Corrupt { get { return GetValue(ChampionSpawnType.Corrupt); } set { SetValue(ChampionSpawnType.Corrupt, value); } }
            #endregion

            public ChampionTitleInfo()
            { }

            public ChampionTitleInfo(GenericReader reader)
            {
                int version = reader.ReadEncodedInt();

                switch (version)
                {
                    case 0:
                        {
                            m_Harrower = reader.ReadEncodedInt();

                            int length = reader.ReadEncodedInt();
                            m_Values = new TitleInfo[length];

                            for (int i = 0; i < length; i++)
                            {
                                m_Values[i] = new TitleInfo(reader);
                            }

                            if (m_Values.Length != ChampionSpawnInfo.Table.Length)
                            {
                                var oldValues = m_Values;
                                m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

                                for (int i = 0; i < m_Values.Length && i < oldValues.Length; i++)
                                {
                                    m_Values[i] = oldValues[i];
                                }
                            }
                            break;
                        }
                }
            }

            public static void Serialize(GenericWriter writer, ChampionTitleInfo titles)
            {
                writer.WriteEncodedInt(0); // version

                writer.WriteEncodedInt(titles.m_Harrower);

                int length = titles.m_Values.Length;
                writer.WriteEncodedInt(length);

                for (int i = 0; i < length; i++)
                {
                    if (titles.m_Values[i] == null)
                    {
                        titles.m_Values[i] = new TitleInfo();
                    }

                    TitleInfo.Serialize(writer, titles.m_Values[i]);
                }
            }

            public static void CheckAtrophy(PlayerMobile pm)
            {
                ChampionTitleInfo t = pm.m_ChampionTitles;
                if (t == null)
                {
                    return;
                }

                if (t.m_Values == null)
                {
                    t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                for (int i = 0; i < t.m_Values.Length; i++)
                {
                    if ((t.GetLastDecay(i) + LossDelay) < DateTime.UtcNow)
                    {
                        t.Atrophy(i, LossAmount);
                    }
                }
            }

            public static void AwardHarrowerTitle(PlayerMobile pm)
            //Called when killing a harrower.  Will give a minimum of 1 point.
            {
                ChampionTitleInfo t = pm.m_ChampionTitles;
                if (t == null)
                {
                    return;
                }

                if (t.m_Values == null)
                {
                    t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                int count = 1;

                for (int i = 0; i < t.m_Values.Length; i++)
                {
                    if (t.m_Values[i].Value > 900)
                    {
                        count++;
                    }
                }

                t.m_Harrower = Math.Max(count, t.m_Harrower); //Harrower titles never decay.
            }

            public bool HasChampionTitle(PlayerMobile pm)
            {
                if (m_Harrower > 0)
                    return true;

                if (m_Values == null)
                    return false;

                foreach (TitleInfo info in m_Values)
                {
                    if (info.Value > 300)
                        return true;
                }

                return false;
            }
        }
        #endregion

        #region Recipes
        private Dictionary<int, bool> m_AcquiredRecipes;

        public virtual bool HasRecipe(Recipe r)
        {
            if (r == null)
            {
                return false;
            }

            return HasRecipe(r.ID);
        }

        public virtual bool HasRecipe(int recipeID)
        {
            if (m_AcquiredRecipes != null && m_AcquiredRecipes.ContainsKey(recipeID))
            {
                return m_AcquiredRecipes[recipeID];
            }

            return false;
        }

        public virtual void AcquireRecipe(Recipe r)
        {
            if (r != null)
            {
                AcquireRecipe(r.ID);
            }
        }

        public virtual void AcquireRecipe(int recipeID)
        {
            if (m_AcquiredRecipes == null)
            {
                m_AcquiredRecipes = new Dictionary<int, bool>();
            }

            m_AcquiredRecipes[recipeID] = true;
        }

        public virtual void ResetRecipes()
        {
            m_AcquiredRecipes = null;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int KnownRecipes
        {
            get
            {
                if (m_AcquiredRecipes == null)
                {
                    return 0;
                }

                return m_AcquiredRecipes.Count;
            }
        }
        #endregion

        #region Buff Icons
        public void ResendBuffs()
        {
            if (!BuffInfo.Enabled || m_BuffTable == null)
            {
                return;
            }

            NetState state = NetState;

            if (state != null && state.BuffIcon)
            {
                foreach (BuffInfo info in m_BuffTable.Values)
                {
                    state.Send(new AddBuffPacket(this, info));
                }
            }
        }

        private Dictionary<BuffIcon, BuffInfo> m_BuffTable;

        public void AddBuff(BuffInfo b)
        {
            if (!BuffInfo.Enabled || b == null)
            {
                return;
            }

            RemoveBuff(b); //Check & subsequently remove the old one.

            if (m_BuffTable == null)
            {
                m_BuffTable = new Dictionary<BuffIcon, BuffInfo>();
            }

            m_BuffTable.Add(b.ID, b);

            NetState state = NetState;

            if (state != null && state.BuffIcon)
            {
                state.Send(new AddBuffPacket(this, b));
            }
        }

        public void RemoveBuff(BuffInfo b)
        {
            if (b == null)
            {
                return;
            }

            RemoveBuff(b.ID);
        }

        public void RemoveBuff(BuffIcon b)
        {
            if (m_BuffTable == null || !m_BuffTable.ContainsKey(b))
            {
                return;
            }

            BuffInfo info = m_BuffTable[b];

            if (info.Timer != null && info.Timer.Running)
            {
                info.Timer.Stop();
            }

            m_BuffTable.Remove(b);

            NetState state = NetState;

            if (state != null && state.BuffIcon)
            {
                state.Send(new RemoveBuffPacket(this, b));
            }

            if (m_BuffTable.Count <= 0)
            {
                m_BuffTable = null;
            }
        }
        #endregion

        #region Exploring the Deep
        private ExploringTheDeepQuestChain m_ExploringTheDeepQuest;

        [CommandProperty(AccessLevel.GameMaster)]
        public ExploringTheDeepQuestChain ExploringTheDeepQuest { get { return m_ExploringTheDeepQuest; } set { m_ExploringTheDeepQuest = value; } }
        #endregion

        public static bool PetAutoStable { get { return true; } }

        public void AutoStablePets()
        {
            if (PetAutoStable && AllFollowers.Count > 0)
            {
                for (int i = m_AllFollowers.Count - 1; i >= 0; --i)
                {
                    BaseCreature pet = AllFollowers[i] as BaseCreature;

                    if (pet == null)
                    {
                        continue;
                    }

                    if (pet.Summoned && pet.Map != Map)
                    {
                        pet.PlaySound(pet.GetAngerSound());

                        Timer.DelayCall(pet.Delete);

                        continue;
                    }

                    if (!pet.CanAutoStable)
                    {
                        continue;
                    }

                    pet.ControlTarget = null;
                    pet.ControlOrder = OrderType.Stay;
                    pet.Internalize();

                    pet.SetControlMaster(null);
                    pet.SummonMaster = null;

                    pet.IsStabled = true;
                    pet.StabledBy = this;

                    //pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully happy

                    if (Shard.DebugEnabled)
                    {
                        Shard.Debug("Auto stable de " + pet.Name, this);
                    }

                    Stabled.Add(pet);
                    m_AutoStabled.Add(pet);
                }
            }
        }

        public void ClaimAutoStabledPets()
        {
            if (Region is GuardedRegion && !PetAutoStable || !Region.AllowAutoClaim(this) || m_AutoStabled.Count <= 0)
            {
                if (m_AutoStabled.Count > 0)
                {
                    SendMessage(78, "Voce tem " + m_AutoStabled.Count + " pets guardados no estabulo");
                }
                return;
            }

            if (!Alive)
            {
                SendGump(new ReLoginClaimGump());
                return;
            }

            for (int i = m_AutoStabled.Count - 1; i >= 0; --i)
            {
                BaseCreature pet = m_AutoStabled[i] as BaseCreature;

                if (pet == null || pet.Deleted)
                {
                    pet.IsStabled = false;
                    pet.StabledBy = null;

                    if (Stabled.Contains(pet))
                    {
                        Stabled.Remove(pet);
                    }

                    continue;
                }

                if ((Followers + pet.ControlSlots) <= FollowersMax)
                {
                    pet.SetControlMaster(this);

                    if (pet.Summoned)
                    {
                        pet.SummonMaster = this;
                    }

                    pet.ControlTarget = this;
                    pet.ControlOrder = OrderType.Follow;

                    Shard.Debug("Auto teleport", pet);
                    pet.MoveToWorld(Location, Map);

                    pet.IsStabled = false;
                    pet.StabledBy = null;

                    //pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully Happy

                    if (Stabled.Contains(pet))
                    {
                        Stabled.Remove(pet);
                    }
                }
                else
                {
                    SendLocalizedMessage(1049612, pet.Name); // ~1_NAME~ remained in the stables because you have too many followers.
                }
            }

            m_AutoStabled.Clear();
        }
    }
}
