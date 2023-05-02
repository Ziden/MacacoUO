#region References
using System;
using System.Collections.Generic;
using System.Linq;

using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Engines.XmlSpawner2;
using Server.Ethics;
using Server.Factions;
using Server.Mobiles;
using Server.Network;
using Server.Services.Virtues;
using Server.SkillHandlers;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Sixth;
using Server.Spells.Spellweaving;
using Server.Spells.SkillMasteries;
using Server.Gumps;
using Server.Fronteira.Talentos;
#endregion

namespace Server.Items
{
    public interface ISlayer
    {
        SlayerName Slayer { get; set; }
        SlayerName Slayer2 { get; set; }
    }

    public abstract class BaseWeapon : Item, IWeapon, IFactionItem, IUsesRemaining, ICraftable, ISlayer, IDurability, ISetItem, IVvVItem, IOwnerRestricted, IResource, IArtifact, ICombatEquipment, IEngravable
    {
        #region Damage Helpers
        public static BaseWeapon GetDamageOutput(Mobile wielder, out int min, out int max)
        {
            return GetDamageOutput(wielder, null, out min, out max);
        }

        public string GetDurabString()
        {
            return m_Hits + " / " + m_MaxHits;
        }

        public static BaseWeapon GetDamageOutput(Mobile wielder, BaseWeapon weapon, out int min, out int max)
        {
            int minRaw, maxRaw;

            return GetDamageOutput(wielder, weapon, out minRaw, out maxRaw, out min, out max);
        }

        public static BaseWeapon GetDamageOutput(
            Mobile wielder,
            out int minRaw,
            out int maxRaw,
            out int minVal,
            out int maxVal)
        {
            return GetDamageOutput(wielder, null, out minRaw, out maxRaw, out minVal, out maxVal);
        }

        public static BaseWeapon GetDamageOutput(
            Mobile wielder,
            BaseWeapon weapon,
            out int minRaw,
            out int maxRaw,
            out int minVal,
            out int maxVal)
        {
            minRaw = maxRaw = 0;
            minVal = maxVal = 0;

            if (wielder == null)
            {
                return null;
            }

            if (weapon == null)
            {
                weapon = wielder.Weapon as BaseWeapon ?? Fists;
            }

            if (weapon == null)
            {
                return null;
            }

            weapon.GetBaseDamageRange(wielder, out minVal, out maxVal);

            if (wielder is BaseCreature)
            {
                if (((BaseCreature)wielder).DamageMin >= 0 || (weapon is Fists && !wielder.Body.IsHuman))
                {
                    minRaw = minVal;
                    maxRaw = maxVal;
                    return weapon;
                }
            }

            minRaw = weapon.MinDamage;
            maxRaw = weapon.MaxDamage;

            Shard.Debug("Min Max Antes de Scalar: " + minRaw + "/" + maxRaw, wielder);
            minVal = (int)weapon.ScaleDamageOld(wielder, minVal, false);
            maxVal = (int)weapon.ScaleDamageOld(wielder, maxVal, false);
            Shard.Debug("Min Max Dps de Scalar: " + minRaw + "/" + maxRaw, wielder);
            return weapon;
        }
        #endregion

        private string m_EngravedText;

        [CommandProperty(AccessLevel.GameMaster)]
        public string EngravedText
        {
            get { return m_EngravedText; }
            set
            {
                m_EngravedText = value;
                InvalidateProperties();
            }
        }

        #region Factions
        private FactionItem m_FactionState;

        public FactionItem FactionItemState
        {
            get { return m_FactionState; }
            set
            {
                m_FactionState = value;

                if (m_FactionState == null)
                {
                    Hue = CraftResources.GetHue(Resource);
                }

                LootType = (m_FactionState == null ? LootType.Regular : LootType.Blessed);
            }
        }
        #endregion

        #region IUsesRemaining members
        private int m_UsesRemaining;
        private bool m_ShowUsesRemaining;

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining { get { return m_UsesRemaining; } set { m_UsesRemaining = value; InvalidateProperties(); } }

        public bool ShowUsesRemaining { get { return m_ShowUsesRemaining; } set { m_ShowUsesRemaining = value; InvalidateProperties(); } }

        public void ScaleUses()
        {
            m_UsesRemaining = (m_UsesRemaining * GetUsesScalar()) / 100;
            InvalidateProperties();
        }

        public void UnscaleUses()
        {
            m_UsesRemaining = (m_UsesRemaining * 100) / GetUsesScalar();
        }

        public int GetUsesScalar()
        {
            var scalar = 100;
            if (this is IResource)
            {
                var Resource = ((IResource)this).Resource;
                if (Resource == CraftResource.Cobre)
                    scalar += 50;
                else if (Resource == CraftResource.Bronze)
                    scalar += 80;
                else if (Resource == CraftResource.Dourado)
                    scalar += 90;
                else if (Resource == CraftResource.Niobio)
                    scalar += 110;
                else if (Resource == CraftResource.Lazurita)
                    scalar += 150;
                else if (Resource == CraftResource.Quartzo)
                    scalar += 350;
                else if (Resource == CraftResource.Berilo)
                    scalar += 230;
                else if (Resource == CraftResource.Vibranium)
                    scalar += 230;
                else if (Resource == CraftResource.Adamantium)
                    scalar += 130;
                else if (Resource == CraftResource.Carmesim)
                    scalar += 250;
                else if (Resource == CraftResource.Gelo)
                    scalar += 250;
                else if (Resource == CraftResource.Eucalipto)
                    scalar += 150;
                else if (Resource == CraftResource.Mogno)
                    scalar += 120;
                else if (Resource == CraftResource.Pinho)
                    scalar += 100;
                else if (Resource == CraftResource.Carvalho)
                    scalar += 50;
            }
            if (m_Quality == ItemQuality.Low)
                scalar -= 70;

            if (m_Quality == ItemQuality.Exceptional)
                scalar += 100;

            if (m_Quality == ItemQuality.ObraPrima)
                scalar += 1000;

            return scalar;
        }
        #endregion

        private bool _VvVItem;
        private Mobile _Owner;
        private string _OwnerName;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsVvVItem
        {
            get { return _VvVItem; }
            set { _VvVItem = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner
        {
            get { return _Owner; }
            set { _Owner = value; if (_Owner != null) _OwnerName = _Owner.Name; InvalidateProperties(); }
        }

        public virtual string OwnerName
        {
            get { return _OwnerName; }
            set { _OwnerName = value; InvalidateProperties(); }
        }

        // Instance values. These values are unique to each weapon.
        private WeaponDamageLevel m_DamageLevel;
        private WeaponAccuracyLevel m_AccuracyLevel;
        private WeaponDurabilityLevel m_DurabilityLevel;
        private ItemQuality m_Quality;
        private Mobile m_Crafter;
        private Poison m_Poison;
        private int m_PoisonCharges;
        private bool m_Identified;
        private int m_Hits;
        private int m_MaxHits;
        private SlayerName m_Slayer;
        private SlayerName m_Slayer2;

        #region Mondain's Legacy
        private TalismanSlayerName m_Slayer3;
        #endregion

        private SkillMod m_SkillMod, m_MageMod, m_MysticMod;
        private CraftResource m_Resource;
        private bool m_PlayerConstructed;

        private bool m_Altered;

        private AosAttributes m_AosAttributes;
        private AosWeaponAttributes m_AosWeaponAttributes;
        private AosSkillBonuses m_AosSkillBonuses;
        private AosElementAttributes m_AosElementDamages;
        private SAAbsorptionAttributes m_SAAbsorptionAttributes;
        private NegativeAttributes m_NegativeAttributes;
        private ExtendedWeaponAttributes m_ExtendedWeaponAttributes;

        // Overridable values. These values are provided to override the defaults which get defined in the individual weapon scripts.
        private int m_StrReq, m_DexReq, m_IntReq;
        private int m_MinDamage, m_MaxDamage;
        private int m_HitSound, m_MissSound;
        private float m_Speed;
        private int m_MaxRange;
        private SkillName m_Skill;
        private WeaponType m_Type;
        private WeaponAnimation m_Animation;

        #region Stygian Abyss
        private int m_TimesImbued;
        private bool m_IsImbued;
        private bool m_DImodded;
        #endregion

        #region High Seas
        private bool m_SearingWeapon;
        #endregion

        #region Runic Reforging
        private ElementoPvM m_ItemPower;
        private ReforgedPrefix m_ReforgedPrefix;
        private ReforgedSuffix m_ReforgedSuffix;
        #endregion


        #region Virtual Properties
        public virtual WeaponAbility PrimaryAbility { get { return null; } }
        public virtual WeaponAbility SecondaryAbility { get { return null; } }

        public virtual int DefMaxRange { get { return 1; } }
        public virtual int DefHitSound { get { return 0; } }
        public virtual int DefMissSound { get { return 0; } }
        public virtual SkillName DefSkill { get { return SkillName.Swords; } }
        public virtual WeaponType DefType { get { return WeaponType.Slashing; } }
        public virtual WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash1H; } }

        public virtual int AosStrengthReq { get { return 0; } }
        public virtual int AosDexterityReq { get { return 0; } }
        public virtual int AosIntelligenceReq { get { return 0; } }
        public virtual int AosMinDamage { get { return 0; } }
        public virtual int AosMaxDamage { get { return 0; } }
        public virtual int AosSpeed { get { return 0; } }
        public virtual float MlSpeed { get { return 0.0f; } }
        public virtual int AosMaxRange { get { return DefMaxRange; } }
        public virtual int AosHitSound { get { return DefHitSound; } }
        public virtual int AosMissSound { get { return DefMissSound; } }
        public virtual SkillName AosSkill { get { return DefSkill; } }
        public virtual WeaponType AosType { get { return DefType; } }
        public virtual WeaponAnimation AosAnimation { get { return DefAnimation; } }

        public virtual int OldStrengthReq { get { return 0; } }
        public virtual int OldDexterityReq { get { return 0; } }
        public virtual int OldIntelligenceReq { get { return 0; } }
        public virtual int OldMinDamage { get { return 0; } }
        public virtual int OldMaxDamage { get { return 0; } }
        public virtual int OldSpeed { get { return 0; } }
        public virtual int OldMaxRange { get { return DefMaxRange; } }
        public virtual int OldHitSound { get { return DefHitSound; } }
        public virtual int OldMissSound { get { return DefMissSound; } }
        public virtual SkillName OldSkill { get { return DefSkill; } }
        public virtual WeaponType OldType { get { return DefType; } }
        public virtual WeaponAnimation OldAnimation { get { return DefAnimation; } }

        public virtual int InitMinHits { get { return 0; } }
        public virtual int InitMaxHits { get { return 0; } }

        public virtual bool CanFortify { get { return NegativeAttributes.Antique < 4; } }
        public virtual bool CanRepair { get { return m_NegativeAttributes.NoRepair == 0; } }
        public virtual bool CanAlter { get { return true; } }

        public override int PhysicalResistance { get { return m_AosWeaponAttributes.ResistPhysicalBonus; } }
        public override int FireResistance { get { return m_AosWeaponAttributes.ResistFireBonus; } }
        public override int ColdResistance { get { return m_AosWeaponAttributes.ResistColdBonus; } }
        public override int PoisonResistance { get { return m_AosWeaponAttributes.ResistPoisonBonus; } }
        public override int EnergyResistance { get { return m_AosWeaponAttributes.ResistEnergyBonus; } }

        public virtual SkillName AccuracySkill { get { return SkillName.Tactics; } }

        public override double DefaultWeight
        {
            get
            {
                if (NegativeAttributes == null || NegativeAttributes.Unwieldly == 0)
                    return base.DefaultWeight;

                return 50;
            }
        }

        #region Personal Bless Deed
        private Mobile m_BlessedBy;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile BlessedBy
        {
            get { return m_BlessedBy; }
            set
            {
                m_BlessedBy = value;
                InvalidateProperties();
            }
        }

        private class UnBlessEntry : ContextMenuEntry
        {
            private readonly Mobile m_From;
            private readonly BaseWeapon m_Weapon; // BaseArmor, BaseWeapon or BaseClothing

            public UnBlessEntry(Mobile from, BaseWeapon weapon)
                : base(6208, -1)
            {
                m_From = from;
                m_Weapon = weapon;
            }

            public override void OnClick()
            {
                m_Weapon.BlessedFor = null;
                m_Weapon.BlessedBy = null;

                Container pack = m_From.Backpack;

                if (pack != null)
                {
                    pack.DropItem(new PersonalBlessDeed(m_From));
                    m_From.SendLocalizedMessage(1062200); // A personal bless deed has been placed in your backpack.
                }
            }
        }
        #endregion

        #endregion

        #region Getters & Setters
        [CommandProperty(AccessLevel.GameMaster)]
        public AosAttributes Attributes { get { return m_AosAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosWeaponAttributes WeaponAttributes { get { return m_AosWeaponAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosSkillBonuses SkillBonuses { get { return m_AosSkillBonuses; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosElementAttributes AosElementDamages { get { return m_AosElementDamages; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public SAAbsorptionAttributes AbsorptionAttributes { get { return m_SAAbsorptionAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public NegativeAttributes NegativeAttributes { get { return m_NegativeAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ExtendedWeaponAttributes ExtendedWeaponAttributes { get { return m_ExtendedWeaponAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ConsecratedWeaponContext ConsecratedContext { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Identified
        {
            get { return m_Identified; }
            set
            {
                m_Identified = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPoints
        {
            get { return m_Hits; }
            set
            {
                if (m_Hits == value)
                {
                    return;
                }

                if (value > m_MaxHits)
                {
                    value = m_MaxHits;
                }

                m_Hits = value;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxHitPoints
        {
            get { return m_MaxHits; }
            set
            {
                m_MaxHits = value;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonCharges
        {
            get { return m_PoisonCharges; }
            set
            {
                m_PoisonCharges = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Poison Poison
        {
            get { return m_Poison; }
            set
            {
                m_Poison = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ItemQuality Quality
        {
            get { return m_Quality; }
            set
            {
                UnscaleDurability();
                UnscaleUses();
                m_Quality = value;
                ScaleDurability();
                ScaleUses();
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Crafter
        {
            get { return m_Crafter; }
            set
            {
                m_Crafter = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SlayerName Slayer
        {
            get { return m_Slayer; }
            set
            {
                m_Slayer = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SlayerName Slayer2
        {
            get { return m_Slayer2; }
            set
            {
                m_Slayer2 = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TalismanSlayerName Slayer3
        {
            get { return m_Slayer3; }
            set
            {
                m_Slayer3 = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set
            {
                UnscaleDurability();
                m_Resource = value;
                Hue = CraftResources.GetHue(m_Resource);
                InvalidateProperties();
                ScaleDurability();
                ScaleUses();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponDamageLevel DamageLevel
        {
            get { return m_DamageLevel; }
            set
            {
                m_DamageLevel = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponDurabilityLevel DurabilityLevel
        {
            get { return m_DurabilityLevel; }
            set
            {
                UnscaleDurability();
                m_DurabilityLevel = value;
                InvalidateProperties();
                ScaleDurability();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PlayerConstructed { get { return m_PlayerConstructed; } set { m_PlayerConstructed = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxRange
        {
            get
            {
                if (AnimalForm.UnderTransformation(this.RootParent as PlayerMobile, typeof(BullFrog)))
                    return 5;
                return (m_MaxRange == -1 ? Core.AOS ? AosMaxRange : OldMaxRange : m_MaxRange);
            }
            set
            {
                m_MaxRange = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponAnimation Animation { get { return (m_Animation == (WeaponAnimation)(-1) ? Core.AOS ? AosAnimation : OldAnimation : m_Animation); } set { m_Animation = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponType Type { get { return (m_Type == (WeaponType)(-1) ? Core.AOS ? AosType : OldType : m_Type); } set { m_Type = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill
        {
            get { return (m_Skill == (SkillName)(-1) ? Core.AOS ? AosSkill : OldSkill : m_Skill); }
            set
            {
                m_Skill = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitSound { get { return (m_HitSound == -1 ? Core.AOS ? AosHitSound : OldHitSound : m_HitSound); } set { m_HitSound = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MissSound { get { return (m_MissSound == -1 ? Core.AOS ? AosMissSound : OldMissSound : m_MissSound); } set { m_MissSound = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinDamage
        {
            get { return (m_MinDamage == -1 ? Core.AOS ? AosMinDamage : OldMinDamage : m_MinDamage); }
            set
            {
                m_MinDamage = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxDamage
        {
            get { return (m_MaxDamage == -1 ? Core.AOS ? AosMaxDamage : OldMaxDamage : m_MaxDamage); }
            set
            {
                m_MaxDamage = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public float Speed
        {
            get
            {
                if (m_Speed != -1)
                {
                    return m_Speed;
                }

                if (Core.ML)
                {
                    return MlSpeed;
                }
                else if (Core.AOS)
                {
                    return AosSpeed;
                }

                return OldSpeed;
            }
            set
            {
                m_Speed = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StrRequirement
        {
            get
            {
                if (m_NegativeAttributes.Massive > 0)
                {
                    return 125;
                }

                return m_StrReq == -1 ? Core.AOS ? AosStrengthReq : OldStrengthReq : m_StrReq;
            }
            set
            {
                m_StrReq = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DexRequirement { get { return (m_DexReq == -1 ? Core.AOS ? AosDexterityReq : OldDexterityReq : m_DexReq); } set { m_DexReq = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int IntRequirement { get { return (m_IntReq == -1 ? Core.AOS ? AosIntelligenceReq : OldIntelligenceReq : m_IntReq); } set { m_IntReq = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponAccuracyLevel AccuracyLevel
        {
            get { return m_AccuracyLevel; }
            set
            {
                if (m_AccuracyLevel != value)
                {
                    m_AccuracyLevel = value;

                    if (UseSkillMod)
                    {
                        if (m_AccuracyLevel == WeaponAccuracyLevel.Regular)
                        {
                            if (m_SkillMod != null)
                            {
                                m_SkillMod.Remove();
                            }

                            m_SkillMod = null;
                        }
                        else if (m_SkillMod == null && Parent is Mobile)
                        {
                            m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                            ((Mobile)Parent).AddSkillMod(m_SkillMod);
                        }
                        else if (m_SkillMod != null)
                        {
                            m_SkillMod.Value = (int)m_AccuracyLevel * 5;
                        }
                    }

                    InvalidateProperties();
                }
            }
        }

        public Mobile FocusWeilder { get; set; }
        public Mobile EnchantedWeilder { get; set; }

        #region Stygian Abyss
        [CommandProperty(AccessLevel.GameMaster)]
        public int TimesImbued
        {
            get { return m_TimesImbued; }
            set { m_TimesImbued = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsImbued
        {
            get
            {
                if (TimesImbued >= 1 && !m_IsImbued)
                    m_IsImbued = true;

                return m_IsImbued;
            }
            set
            {
                if (TimesImbued >= 1)
                    m_IsImbued = true;
                else
                    m_IsImbued = value; InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DImodded
        {
            get { return m_DImodded; }
            set { m_DImodded = value; }
        }

        public int[] BaseResists
        {
            get
            {
                return new int[] { 0, 0, 0, 0, 0 };
            }
        }

        public virtual void OnAfterImbued(Mobile m, int mod, int value)
        {
        }
        #endregion

        #region High Seas
        [CommandProperty(AccessLevel.GameMaster)]
        public bool SearingWeapon
        {
            get { return m_SearingWeapon; }
            set { m_SearingWeapon = value; InvalidateProperties(); }
        }
        #endregion

        #region Runic Reforging

        [CommandProperty(AccessLevel.GameMaster)]
        public ElementoPvM Elemento
        {
            get { return m_ItemPower; }
            set { m_ItemPower = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ReforgedPrefix ReforgedPrefix
        {
            get { return m_ReforgedPrefix; }
            set { m_ReforgedPrefix = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ReforgedSuffix ReforgedSuffix
        {
            get { return m_ReforgedSuffix; }
            set { m_ReforgedSuffix = value; InvalidateProperties(); }
        }
        #endregion
        #endregion

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (BlessedFor == from && BlessedBy == from && RootParent == from)
            {
                list.Add(new UnBlessEntry(from, this));
            }
        }

        public override void OnAfterDuped(Item newItem)
        {
            base.OnAfterDuped(newItem);

            BaseWeapon weap = newItem as BaseWeapon;

            if (weap == null)
            {
                return;
            }

            weap.m_AosAttributes = new AosAttributes(newItem, m_AosAttributes);
            weap.m_AosElementDamages = new AosElementAttributes(newItem, m_AosElementDamages);
            weap.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
            weap.m_AosWeaponAttributes = new AosWeaponAttributes(newItem, m_AosWeaponAttributes);
            weap.m_NegativeAttributes = new NegativeAttributes(newItem, m_NegativeAttributes);
            weap.m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(newItem, m_ExtendedWeaponAttributes);

            #region Mondain's Legacy
            weap.m_SetAttributes = new AosAttributes(newItem, m_SetAttributes);
            weap.m_SetSkillBonuses = new AosSkillBonuses(newItem, m_SetSkillBonuses);
            #endregion

            #region SA
            weap.m_SAAbsorptionAttributes = new SAAbsorptionAttributes(newItem, m_SAAbsorptionAttributes);
            #endregion
        }

        public virtual void UnscaleDurability()
        {
            int scale = 100 + GetDurabilityBonus();

            m_Hits = ((m_Hits * 100) + (scale - 1)) / scale;
            m_MaxHits = ((m_MaxHits * 100) + (scale - 1)) / scale;

            InvalidateProperties();
        }

        public virtual void ScaleDurability()
        {
            int scale = 100 + GetDurabilityBonus();

            m_Hits = ((m_Hits * scale) + 99) / 100;
            m_MaxHits = ((m_MaxHits * scale) + 99) / 100;

            if (m_MaxHits > 255)
                m_MaxHits = 255;

            if (m_Hits > 255)
                m_Hits = 255;

            InvalidateProperties();
        }

        public int GetDurabilityBonus()
        {
            int bonus = 1;

            if (m_Quality >= ItemQuality.Exceptional)
            {
                bonus += 40;
            }

            if (this.Crafter is PlayerMobile pm)
            {
                Shard.Debug("Crafter player");
                if (pm.RP && pm.TemTalento(Talento.DurabilidadeArmas)) {
                    Shard.Debug("Bonus de talento +100%");
                    bonus += 100;
                }
            } else
            {
                Shard.Debug("Crafter sem player");
            }

            switch (m_DurabilityLevel)
            {
                case WeaponDurabilityLevel.Durable:
                    bonus += 20;
                    break;
                case WeaponDurabilityLevel.Substantial:
                    bonus += 50;
                    break;
                case WeaponDurabilityLevel.Massive:
                    bonus += 70;
                    break;
                case WeaponDurabilityLevel.Fortified:
                    bonus += 100;
                    break;
                case WeaponDurabilityLevel.Indestructible:
                    bonus += 120;
                    break;
            }

            switch (Resource)
            {
                case CraftResource.Carvalho:
                case CraftResource.Cobre:

                    bonus += 35;
                    break;
                case CraftResource.Pinho:
                case CraftResource.Bronze:
                    bonus += 40;
                    break;
                case CraftResource.Mogno:
                case CraftResource.Dourado:
                case CraftResource.Niobio:
                case CraftResource.Vibranium:
                    bonus += 60;
                    break;
                case CraftResource.Eucalipto:

                case CraftResource.Lazurita:
                case CraftResource.Quartzo:
                case CraftResource.Berilo:
                    bonus += 80;
                    break;
                case CraftResource.Carmesim:
                    bonus += 90;
                    break;
                case CraftResource.Gelo:
                    if (this is BaseStaff)
                        bonus += 100;
                    else
                        bonus += 50;
                    break;
                case CraftResource.Adamantium:

                    bonus += 100;
                    break;
            }

            if (Core.AOS)
            {
                bonus += m_AosWeaponAttributes.DurabilityBonus;

                #region Mondain's Legacy
                if (m_Resource == CraftResource.Eucalipto)
                {
                    return bonus;
                }
                #endregion

                CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);
                CraftAttributeInfo attrInfo = null;

                if (resInfo != null)
                {
                    attrInfo = resInfo.AttributeInfo;
                }

                if (attrInfo != null)
                {
                    bonus += attrInfo.WeaponDurability;
                }
            }

            return bonus;
        }

        public int GetLowerStatReq()
        {
            if (!Core.AOS)
            {
                return 0;
            }

            int v = m_AosWeaponAttributes.LowerStatReq;

            #region Mondain's Legacy
            if (m_Resource == CraftResource.Eucalipto)
            {
                return v;
            }
            #endregion

            CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

            if (info != null)
            {
                CraftAttributeInfo attrInfo = info.AttributeInfo;

                if (attrInfo != null)
                {
                    v += attrInfo.WeaponLowerRequirements;
                }
            }

            if (v > 100)
            {
                v = 100;
            }

            return v;
        }

        public static void BlockEquip(Mobile m, TimeSpan duration)
        {
            if (m.BeginAction(typeof(BaseWeapon)))
            {
                new ResetEquipTimer(m, duration).Start();
            }
        }

        private class ResetEquipTimer : Timer
        {
            private readonly Mobile m_Mobile;

            public ResetEquipTimer(Mobile m, TimeSpan duration)
                : base(duration)
            {
                m_Mobile = m;
            }

            protected override void OnTick()
            {
                m_Mobile.EndAction(typeof(BaseWeapon));
            }
        }

        public override bool CheckConflictingLayer(Mobile m, Item item, Layer layer)
        {
            if (base.CheckConflictingLayer(m, item, layer))
            {
                return true;
            }

            if (Layer == Layer.TwoHanded && layer == Layer.OneHanded)
            {
                m.PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, "Voce ja tem algo em suas maos", m.NetState); // You already have something in both hands.
                return true;
            }
            else if (Layer == Layer.OneHanded && layer == Layer.TwoHanded && !(item is BaseShield) && !(item is BaseEquipableLight))
            {
                m.PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, "Voce pode ter apenas uma arma em suas maos", m.NetState); // // You can only wield one weapon at a time.
                return true;
            }

            return false;
        }

        public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
        {
            if (!Ethic.CheckTrade(from, to, newOwner, this))
            {
                return false;
            }

            return base.AllowSecureTrade(from, to, newOwner, accepted);
        }

        public virtual Race RequiredRace { get { return null; } }
        //On OSI, there are no weapons with race requirements, this is for custom stuff

        #region SA
        public virtual bool CanBeWornByGargoyles { get { return false; } }
        #endregion

        public override bool CanEquip(Mobile from)
        {
            if (!Ethic.CheckEquip(from, this))
            {
                return false;
            }

            if (from.IsPlayer())
            {
                if (_Owner != null && _Owner != from)
                {
                    from.SendLocalizedMessage(501023); // You must be the owner to use this item.
                    return false;
                }

                if (this is IAccountRestricted && ((IAccountRestricted)this).Account != null)
                {
                    Accounting.Account acct = from.Account as Accounting.Account;

                    if (acct == null || acct.Username != ((IAccountRestricted)this).Account)
                    {
                        from.SendLocalizedMessage(1071296); // This item is Account Bound and your character is not bound to it. You cannot use this item.
                        return false;
                    }
                }

                if (IsVvVItem && !Engines.VvV.ViceVsVirtueSystem.IsVvV(from))
                {
                    from.SendLocalizedMessage(1155496); // This item can only be used by VvV participants!
                    return false;
                }
            }

            bool morph = from.FindItemOnLayer(Layer.Earrings) is MorphEarrings;

            if (from.Race == Race.Gargoyle && !CanBeWornByGargoyles && from.IsPlayer())
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1111708); // Gargoyles can't wear this.
                return false;
            }

            if (RequiredRace != null && from.Race != RequiredRace && !morph)
            {
                if (RequiredRace == Race.Gargoyle)
                {
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1111707); // Only gargoyles can wear this.
                }
                else
                {
                    from.SendMessage("Only {0} may use ", RequiredRace.PluralName);
                }

                return false;
            }
            else if (from.Dex < DexRequirement)
            {
                from.SendMessage("You are not nimble enough to equip that.");
                return false;
            }
            else if (from.Str < AOS.Scale(StrRequirement, 100 - GetLowerStatReq()))
            {
                from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
                return false;
            }
            else if (from.Int < IntRequirement)
            {
                from.SendMessage("You are not smart enough to equip that.");
                return false;
            }
            else if (!from.CanBeginAction(typeof(BaseWeapon)))
            {
                return false;
            }
            #region Personal Bless Deed
            else if (BlessedBy != null && BlessedBy != from)
            {
                from.SendLocalizedMessage(1075277); // That item is blessed by another player.

                return false;
            }
            else if (!XmlAttach.CheckCanEquip(this, from))
            {
                return false;
            }
            #endregion

            else
            {
                return base.CanEquip(from);
            }
        }

        public virtual bool UseSkillMod { get { return !Core.AOS; } }

        private static long aux;
        private static long delay;

        public override bool OnEquip(Mobile from)
        {
            int strBonus = m_AosAttributes.BonusStr;
            int dexBonus = m_AosAttributes.BonusDex;
            int intBonus = m_AosAttributes.BonusInt;

            if ((strBonus != 0 || dexBonus != 0 || intBonus != 0))
            {
                Mobile m = from;

                string modName = Serial.ToString();

                if (strBonus != 0)
                {
                    m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));
                }

                if (dexBonus != 0)
                {
                    m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));
                }

                if (intBonus != 0)
                {
                    m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
                }
            }

            if (Shard.TROCA_ARMA_RAPIDA)
            {
                Shard.Debug("Trocando arma para " + this.GetType());
                var transformado = AnimalForm.GetContext(from) != null || TransformationSpellHelper.GetContext(from) != null;
                // pode atacar
                // && from.NextCombatTimes.TryGetValue(from.Weapon.GetType(), out aux) && Core.TickCount - aux >= 0 &&
                if (!Shard.SPHERE_STYLE && !transformado && !from.IsCooldown("saque") && from.Skills[SkillName.ArmsLore].Value >= 100 && from.RawStr >= 100 && from.CanAttack(this.GetType()))
                {
                    from.SetCooldown("saque", TimeSpan.FromSeconds(8));
                    from.PrivateOverheadMessage("* armou rapidamente *");
                    if (!from.IsCooldown("dicaeq"))
                    {
                        from.SetCooldown("dicaeq", TimeSpan.FromDays(1));
                        from.SendMessage(78, "Por ter muita forca e conhecimento em armas, voce equipou a arma rapidamente, podendo atacar com elas sem precisar esperar seu delay completo.");
                    }
                    delay = (long)(GetDelay(from).TotalMilliseconds / (this is BaseRanged ? 2.2 : 10));
                }
                else // a arma ainda ta em cooldown, entao d
                {
                    if (Shard.SPHERE_STYLE)
                        delay = (long)GetDelay(from).TotalMilliseconds / (from.CanAttack(this.GetType()) ? 4 : 1); // sphere estilo KDS, pouco delay dps de botar arma na mao
                    else
                    {
                        if (this is BaseRanged)
                        {
                            delay = (long)(GetDelay(from).TotalMilliseconds);
                        }
                        else
                        {
                            delay = (long)(GetDelay(from).TotalMilliseconds / (from.CanAttack(this.GetType()) ? 2 : 1));
                        }

                    }

                }
                from.NextCombatTimes[this.GetType()] = Core.TickCount + delay;
            }
            else
            {
                from.NextCombatTime = Core.TickCount + (int)GetDelay(from).TotalMilliseconds;
            }


            if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular)
            {
                if (m_SkillMod != null)
                {
                    m_SkillMod.Remove();
                }

                m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                from.AddSkillMod(m_SkillMod);
            }

            if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30)
            {
                if (m_MageMod != null)
                {
                    m_MageMod.Remove();
                }

                m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
                from.AddSkillMod(m_MageMod);
            }

            if (Core.TOL)
            {
                if ((m_ExtendedWeaponAttributes.MysticWeapon != 0 && m_ExtendedWeaponAttributes.MysticWeapon != 30) || Enhancement.GetValue(from, ExtendedWeaponAttribute.MysticWeapon) > 0)
                    AddMysticMod(from);
            }

            XmlAttach.CheckOnEquip(this, from);

            InDoubleStrike = false;

            return true;
        }

        public override void OnAdded(object parent)
        {
            base.OnAdded(parent);

            if (parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                //if (Core.AOS)
                //{
                m_AosSkillBonuses.AddTo(from);
                //}

                #region Mondain's Legacy Sets
                if (IsSetItem)
                {
                    m_SetEquipped = SetHelper.FullSetEquipped(from, SetID, Pieces);

                    if (m_SetEquipped)
                    {
                        m_LastEquipped = true;
                        SetHelper.AddSetBonus(from, SetID);
                    }
                }
                #endregion

                if (HasSocket<Caddellite>())
                {
                    Caddellite.UpdateBuff(from);
                }

                from.CheckStatTimers();
                from.Delta(MobileDelta.WeaponDamage);
            }
        }

        public override void OnRemoved(object parent)
        {
            if (parent is Mobile)
            {
                Mobile m = (Mobile)parent;
                BaseWeapon weapon = m.Weapon as BaseWeapon;

                string modName = Serial.ToString();

                m.RemoveStatMod(modName + "Str");
                m.RemoveStatMod(modName + "Dex");
                m.RemoveStatMod(modName + "Int");

                if (weapon != null)
                {
                    if (Shard.TROCA_ARMA_RAPIDA)
                    {
                        // Shard.Debug("Salvo combat time de " + this.GetType());
                        // m.NextCombatTimes[weapon.GetType()] = Core.TickCount + (int)GetDelay(m).TotalMilliseconds;
                        // m.NextCombatTimes[weapon.GetType()] = Core.TickCount;
                    }
                    else
                    {
                        m.NextCombatTime = Core.TickCount + (int)weapon.GetDelay(m).TotalMilliseconds;
                    }
                }

                if (UseSkillMod && m_SkillMod != null)
                {
                    m_SkillMod.Remove();
                    m_SkillMod = null;
                }

                if (m_MageMod != null)
                {
                    m_MageMod.Remove();
                    m_MageMod = null;
                }

                //if (Core.AOS)
                //{
                m_AosSkillBonuses.Remove();
                //}

                ImmolatingWeaponSpell.StopImmolating(this, (Mobile)parent);
                Spells.Mysticism.EnchantSpell.OnWeaponRemoved(this, m);

                if (FocusWeilder != null)
                    FocusWeilder = null;

                //Skill Masteries
                SkillMasterySpell.OnWeaponRemoved(m, this);

                #region Mondain's Legacy Sets
                if (IsSetItem && m_SetEquipped)
                {
                    SetHelper.RemoveSetBonus(m, SetID, this);
                }
                #endregion

                if (HasSocket<Caddellite>())
                {
                    Caddellite.UpdateBuff(m);
                }

                m.CheckStatTimers();

                m.Delta(MobileDelta.WeaponDamage);

                XmlAttach.CheckOnRemoved(this, parent);
            }
        }

        public void AddMysticMod(Mobile from)
        {
            if (m_MysticMod != null)
                m_MysticMod.Remove();

            int value = m_ExtendedWeaponAttributes.MysticWeapon;

            if (Enhancement.GetValue(from, ExtendedWeaponAttribute.MysticWeapon) > value)
                value = Enhancement.GetValue(from, ExtendedWeaponAttribute.MysticWeapon);

            m_MysticMod = new DefaultSkillMod(SkillName.Mysticism, true, -30 + value);
            from.AddSkillMod(m_MysticMod);
        }

        public void RemoveMysticMod()
        {
            if (m_MysticMod != null)
            {
                m_MysticMod.Remove();
                m_MysticMod = null;
            }
        }

        public virtual SkillName GetUsedSkill(Mobile m, bool checkSkillAttrs)
        {
            SkillName sk;

            if (this is Dagger && m.Skills[SkillName.Ninjitsu].Value > m.Skills[SkillName.Fencing].Value)
            {
                return SkillName.Ninjitsu;
            }

            else if (Skill == SkillName.Wrestling)
            {
                var ctx = AnimalForm.GetContext(m);
                if (ctx != null)
                {
                    return SkillName.Ninjitsu;
                }
            }



            if (checkSkillAttrs && m_AosWeaponAttributes.UseBestSkill != 0)
            {
                double swrd = m.Skills[SkillName.Swords].Value;
                double fenc = m.Skills[SkillName.Fencing].Value;
                double mcng = m.Skills[SkillName.Macing].Value;
                double val;

                sk = SkillName.Swords;
                val = swrd;

                if (fenc > val)
                {
                    sk = SkillName.Fencing;
                    val = fenc;
                }
                if (mcng > val)
                {
                    sk = SkillName.Macing;
                    val = mcng;
                }
            }
            else if (m_AosWeaponAttributes.MageWeapon != 0)
            {
                if (m.Skills[SkillName.Magery].Value > m.Skills[Skill].Value)
                {
                    sk = SkillName.Magery;
                }
                else
                {
                    sk = Skill;
                }
            }
            else if (m_ExtendedWeaponAttributes.MysticWeapon != 0 || Enhancement.GetValue(m, ExtendedWeaponAttribute.MysticWeapon) > 0)
            {
                if (m.Skills[SkillName.Mysticism].Value > m.Skills[Skill].Value)
                {
                    sk = SkillName.Mysticism;
                }
                else
                {
                    sk = Skill;
                }
            }
            else
            {
                sk = Skill;
                if (sk != SkillName.Wrestling && !m.Player && !m.Body.IsHuman &&
                    m.Skills[SkillName.Wrestling].Value > m.Skills[sk].Value)
                {
                    sk = SkillName.Wrestling;
                }
            }
            return sk;
        }

        public virtual double GetAttackSkillValue(Mobile attacker, Mobile defender)
        {
            return Math.Min(attacker.Skills[GetUsedSkill(attacker, true)].Value, 100);
        }

        public virtual double GetDefendSkillValue(Mobile attacker, Mobile defender)
        {
            return Math.Min(defender.Skills[GetUsedSkill(defender, true)].Value, 100);
        }

        public static bool CheckAnimal(Mobile m, Type type)
        {
            return AnimalForm.UnderTransformation(m, type);
        }

        public virtual bool CheckHit(Mobile attacker, IDamageable damageable)
        {
            Mobile defender = damageable as Mobile;

            if (defender == null)
            {
                if (damageable is IDamageableItem)
                    return ((IDamageableItem)damageable).CheckHit(attacker);

                return true;
            }

            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

            Skill atkSkill = attacker.Skills[atkWeapon.Skill];
            Skill defSkill = defender.Skills[defWeapon.Skill];

            double atkValue = atkWeapon.GetAttackSkillValue(attacker, defender);
            double defValue = defWeapon.GetDefendSkillValue(attacker, defender);

            double ourValue, theirValue;

            int bonus = GetHitChanceBonus();

            if (atkValue <= -50.0)
            {
                atkValue = -49.9;
            }

            if (defValue <= -50.0)
            {
                defValue = -49.9;
            }

            ourValue = (atkValue + 50.0);
            theirValue = (defValue + 50.0);

            ForceArrow.ForceArrowInfo forceInfo = ForceArrow.GetInfo(attacker, defender);
            if (forceInfo != null && forceInfo.Defender == defender)
                bonus -= forceInfo.DefenseChanceMalus;

            if (defender.Player)
            {
                if (((PlayerMobile)defender).Talentos.Tem(Talento.Esquiva))
                {
                    bonus -= 15;
                }
            }

            if (attacker.Player)
            {
                if (((PlayerMobile)attacker).Talentos.Tem(Talento.Precisao))
                {
                    bonus += 25;
                }
            }
            else
            {
                var bonusElemental = defender.GetBonusElemento(ElementoPvM.Fogo) + (defender.GetBonusElemento(ElementoPvM.Vento) / 2) + defender.GetBonusElemento(ElementoPvM.Raio) + defender.GetBonusElemento(ElementoPvM.Gelo);
                bonusElemental /= 2;
                bonus -= (int)(bonusElemental * 100);
                if (Shard.DebugEnabled && bonusElemental > 0)
                {
                    Shard.Debug("Bonus elemental de esquiva: " + bonusElemental);
                }
                if (bonus < -80)
                    bonus = -80;
            }

            double chance = ourValue / (theirValue * 1.8);
            chance *= 1.0 + ((double)bonus / 100d);

            var bonusArmsLore = defender.Skills.ArmsLore.Value / 2000; // 0.05 = 5%
            chance -= bonusArmsLore;
            if (Shard.DebugEnabled && bonusArmsLore > 0)
                Shard.Debug("Bonus esquiva armsLore: " + bonusArmsLore);

            if (attacker is PlayerMobile || defender is PlayerMobile)
            {
                chance += 0.1; // +10% em players
            }

            if (defender.Weapon is BaseRanged)
            {
                chance += 0.1; // +10% chance hit em ranged
            }

            if (defender.Spell != null && defender.Spell.IsCasting)
            {
                if (!(defender.Weapon is Fists || defender.Weapon.AllowEquipedCast(attacker)))
                {
                    chance += 0.2;
                }
            }

            if (attacker.Player)
            {
                var diffDex = attacker.Dex - defender.Dex;
                if (diffDex > 0)
                {
                    chance += diffDex / 150;
                }
            }

            if (defender.Weapon is Fists && defender is PlayerMobile)
            {
                if (defender.Skills.Wrestling.Value >= 100)
                {
                    chance -= 0.1;
                }
                else if (defender.Skills.Wrestling.Value >= 80)
                {
                    chance -= 0.04;
                }
            }

            if (atkWeapon is BaseThrown)
            {
                //Distance malas
                if (attacker.InRange(defender, 1))  //Close Quarters
                {
                    chance -= (.12 - (double)Math.Min(12, (attacker.Skills[SkillName.Throwing].Value + (double)attacker.RawDex) / 20) / 10);
                }
                else if (attacker.GetDistanceToSqrt(defender) < ((BaseThrown)atkWeapon).MinThrowRange)  //too close
                {
                    chance -= .12;
                }

                //shield penalty
                BaseShield shield = attacker.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

                if (shield != null)
                {
                    double malus = Math.Min(90, 1200 / Math.Max(1.0, attacker.Skills[SkillName.Parry].Value));

                    chance = chance - (chance * (malus / 100));
                }
            }

            if (atkWeapon is BaseRanged)
            {
                chance += (attacker.Dex + attacker.Skills[SkillName.Archery].Value) / 2000; // +10% Hit pra arcos com 100 dex 100 archery
            }

            if (Tracking.IsTracking(attacker, defender) && AnimalForm.GetContext(attacker) == null)
            {
                chance += attacker.Skills[SkillName.Tracking].Value / 1000; // +10 track hit
            }

            if (defWeapon is BaseThrown)
            {
                BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

                if (shield != null)
                {
                    double malus = Math.Min(90, 1200 / Math.Max(1.0, defender.Skills[SkillName.Parry].Value));

                    chance = chance + (chance * (malus / 100));
                }
            }


            if (chance < 0.05)
            {
                chance = 0.05;
            }

            Shard.Debug("Chance Hit Final: " + chance, attacker);
            return attacker.CheckSkill(atkSkill.SkillName, chance);
        }

        public virtual TimeSpan GetDelay(Mobile m, Mobile defender = null)
        {
            double speed = Speed;
            if (speed == 0)
            {
                return TimeSpan.FromHours(1.0);
            }

            double delayInSeconds;

            var semBonusStr = AnimalForm.GetContext(m) != null || this is BaseRanged || !m.Player;
            // FORMULA DE SPEED
            int v = 0;
            if (Shard.POL_STYLE)
                v = ((m.Dex + (m.Str / (semBonusStr ? m.Str : 5))) + 100) * (int)speed;
            else
            {
                if (!m.Player)
                    v = (m.Dex + 105) * (int)speed;
                else
                {
                    var stat = (int)(m.Dex * (m.Stam / m.StamMax));
                    v = (stat + 100) * (int)speed;
                }
            }

            if (v <= 0)
            {
                v = 1;
            }
            if (semBonusStr)
                v = (int)(v * 0.85);

            if (m.Player && defender != null && !defender.Player)
            {
                var bonusSpeedArma = (m.Weapon as BaseWeapon).Attributes.WeaponSpeed;
                var bonus = m.GetBonusElemento(ElementoPvM.Vento) / 2;
                if (bonus > 0.9) bonus = 0.9;
                bonus += bonusSpeedArma / 100d;
                v += (int)(bonus * v);
            }

            delayInSeconds = 15000.0 / v;

            if (Shard.DebugEnabled)
            {
                Shard.Debug("Velocidade de ataque: " + delayInSeconds + " segundos", m);
                Shard.Debug("V: " + v, m);
            }


            var delay = TimeSpan.FromSeconds(delayInSeconds);

            if (DivineFurySpell.UnderEffect(m) && !(this is BaseRanged))
            {
                //Shard.Debug("Divine fury atk");
                delay -= TimeSpan.FromSeconds(m.Skills.Chivalry.Value / 300);
            }
            return delay;
        }

        public virtual void OnBeforeSwing(Mobile attacker, IDamageable damageable)
        {
            Mobile defender = damageable as Mobile;

            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

            if (a != null && (!a.OnBeforeSwing(attacker, defender)))
            {
                WeaponAbility.ClearCurrentAbility(attacker);
            }

            SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            if (move != null && (!move.OnBeforeSwing(attacker, defender) || SkillMasterySpell.CancelSpecialMove(attacker)))
            {
                SpecialMove.ClearCurrentMove(attacker);
            }
        }

        public virtual TimeSpan OnSwing(Mobile attacker, IDamageable damageable)
        {
            return OnSwing(attacker, damageable, 1.0);
        }

        public virtual TimeSpan OnSwing(Mobile attacker, IDamageable damageable, double damageBonus)
        {
            bool canSwing = true;

            if(attacker.RP && !(WeaponAbility.GetCurrentAbility(attacker) is MovingShot))
            {
                long nextSwing = attacker.LastMoveTime + attacker.ComputeMovementSpeed() + 50;

                attacker.RevealingAction();

                if(Shard.DebugEnabled)
                {
                    Shard.Debug("nextSwing " + nextSwing + " - " + Core.TickCount);

                }
                if (nextSwing > Core.TickCount)
                {
                    if(!attacker.IsCooldown("dicaswing"))
                    {
                        attacker.SetCooldown("dicaswing", TimeSpan.FromHours(3));
                        attacker.SendMessage(78, "[DICA] Voce precisa ficar parado para conseguir desferir ataques");
                    }

                    return TimeSpan.FromSeconds(0.25);
                }
            }

            var diffZ = Math.Abs(attacker.Location.Z - damageable.Location.Z);

            canSwing = diffZ < 10;

            if (attacker.RP || attacker.Weapon is BaseRanged)
                SpellHelper.Turn(attacker, damageable);

            if (attacker.RP && attacker.Mounted && attacker is PlayerMobile && !((PlayerMobile)attacker).Talentos.Tem(Talento.Hipismo))
            {
                if (Utility.RandomDouble() < 0.01)
                {
                    attacker.Mount.Rider = null;
                    attacker.SendMessage("Voce se desequilibrou da sua montaria ao atacar");
                    AOS.Damage(attacker, Utility.Random(5, 10));
                    attacker.PlayHurtSound();
                    attacker.PlayDamagedAnimation();
                    attacker.Paralyze(TimeSpan.FromSeconds(1));
                }
            }


            if (attacker.RP && !attacker.TemTalento(Talento.Curandeiro))
            {
                BandageContext c = BandageContext.GetContext(attacker);
                if (c != null)
                    c.Slip();
            }

            if (Core.AOS)
            {
                canSwing = (!attacker.Paralyzed && !attacker.Frozen);

                if (canSwing)
                {
                    Spell sp = attacker.Spell as Spell;

                    canSwing = (sp == null || !sp.IsCasting || !sp.BlocksMovement);
                }
            }

            // Peacemaking PvP
            if (canSwing)
            {
                PlayerMobile p = attacker as PlayerMobile;

                canSwing = (p == null || p.PeacedUntil <= DateTime.UtcNow);
            }

            if (canSwing && attacker.HarmfulCheck(damageable))
            {
                attacker.DisruptiveAction();

                if (attacker.NetState != null)
                {
                    attacker.Send(new Swing(0, attacker, damageable));
                }

                if (attacker is BaseCreature)
                {
                    BaseCreature bc = (BaseCreature)attacker;
                    WeaponAbility ab = bc.TryGetWeaponAbility();

                    if (ab != null)
                    {
                        if (bc.WeaponAbilityChance > Utility.RandomDouble())
                        {
                            Shard.Debug("PvM Weapon Ability Setado", bc);
                            WeaponAbility.SetCurrentAbility(bc, ab);
                        }
                        else
                        {
                            WeaponAbility.ClearCurrentAbility(bc);
                        }
                    }
                }

                var defender = damageable as Mobile;
                if (attacker != null && defender != null && defender.Player)
                {
                    var bc = attacker as BaseCreature;
                    if (bc == null || bc.ControlMaster == null || !bc.IsControlled())
                    {
                        if (defender.Weapon.MaxRange <= defender.GetDistance(attacker))
                        {
                            if (defender.Combatant == null || !defender.Combatant.Alive || (defender.Combatant != null && defender.Combatant.GetDistance(defender) > defender.Weapon.MaxRange + 20))
                            {
                                if (Shard.DebugEnabled)
                                {
                                    Shard.Debug("Trocando alvo", defender);
                                }
                                defender.AggressiveAction(attacker, false);
                                defender.Combatant = attacker;
                            }
                        }
                    }
                }

                if (Shard.COMBATE_SPHERE)
                {
                    var delay = GetDelay(attacker, damageable as Mobile);
                    SphereSwing(attacker, damageable, damageBonus, delay);
                    return delay;
                }
                    
                else
                    DoHit(attacker, damageable, damageBonus);
            }

            return GetDelay(attacker, damageable as Mobile);
        }

        public void DoHit(Mobile attacker, IDamageable damageable, double damageBonus)
        {
            attacker.HitPronto = false;
            if (CheckHit(attacker, damageable))
            {
                OnHit(attacker, damageable, damageBonus);
            }
            else
            {
                OnMiss(attacker, damageable);
            }
        }

        public void OnMovimento(Mobile attacker)
        {
            if(attacker.TimerAtaque != null && !(WeaponAbility.GetCurrentAbility(attacker) is MovingShot))
            {
                attacker.TimerAtaque.Stop();
                attacker.HitPronto = false;
                attacker.TimerAtaque = null;
                if (!attacker.IsCooldown("dicamoveswing"))
                {
                    attacker.SetCooldown("dicamoveswing", TimeSpan.FromHours(3));
                    attacker.SendMessage(78, "[DICA] Voce Voce se moveu antes de terminar seu ataque e cancelou o ataque");
                }
            }
          
        }

        public void SphereSwing(Mobile attacker, IDamageable damageable, double damageBonus, TimeSpan delay)
        {
            if (attacker.HitPronto)
            {
                DoHit(attacker, damageable, damageBonus);
                return;
            }
            if (attacker.TimerAtaque != null)
            {
                return;
            }

            if(Shard.RP && delay > TimeSpan.FromSeconds(3))
            {
                delay = TimeSpan.FromSeconds(3);
            }
            var frames = delay.TotalSeconds * 1.5;
            var half = delay.TotalMilliseconds / 2;
            var buffer = 20;
            if(attacker.RP)
            {
                SpellHelper.Turn(attacker, damageable);
                //attacker.Freeze(TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 0.8));
                Timer.DelayCall(TimeSpan.FromMilliseconds(buffer), () =>
                {
                    PlaySwingAnimation(attacker, (int)Math.Round(frames));
                });
            } else
            {
                PlaySwingAnimation(attacker, (int)Math.Round(frames));
            }
            
            attacker.TimerAtaque = Timer.DelayCall(TimeSpan.FromMilliseconds(half + buffer), () =>
            {
                if (!attacker.Alive || attacker.Deleted || attacker.TimerAtaque == null)
                {
                    attacker.HitPronto = false;
                    attacker.TimerAtaque = null;
                    OnMiss(attacker, damageable);
                    return;
                }

                if(!attacker.RP)
                {
                    attacker.HitPronto = true;
                }
               
                attacker.TimerAtaque = null;
                if (attacker.InRange(damageable, attacker.Weapon.MaxRange + 1))
                {
                    DoHit(attacker, damageable, damageBonus);
                } else
                {
                    OnMiss(attacker, damageable);
                }
            });
        }

        #region Sounds
        public virtual int GetHitAttackSound(Mobile attacker, Mobile defender)
        {
            int sound = attacker.GetAttackSound();

            if (sound == -1)
            {
                sound = AnimalForm.AnimalFormEntry.GetAttackSound(attacker);
                if (sound == -1)
                    sound = HitSound;
            }

            return sound;
        }

        public virtual int GetHitDefendSound(Mobile attacker, Mobile defender)
        {
            return defender.GetHurtSound();
        }

        public virtual int GetMissAttackSound(Mobile attacker, Mobile defender)
        {
            if (attacker.GetAttackSound() == -1)
            {
                return MissSound;
            }
            else
            {
                return -1;
            }
        }

        public virtual int GetMissDefendSound(Mobile attacker, Mobile defender)
        {
            return -1;
        }
        #endregion

        public static bool CheckParry(Mobile defender, Mobile attacker)
        {
            if (defender == null)
            {
                return false;
            }

            Shard.Debug("Check Parry", defender);
            BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

            double parry = defender.Skills[SkillName.Parry].Value;
            double bushidoNonRacial = defender.Skills[SkillName.Bushido].NonRacialValue;
            double bushido = defender.Skills[SkillName.Bushido].Value;
            double armsLore = defender.Skills[SkillName.ArmsLore].Value;

            if (shield != null || !defender.Player)
            {
                double chance = parry / 400; // 25%

                if (defender.Player && parry > 70 && attacker is BaseCreature)
                {
                    var p = ((PlayerMobile)defender);
                    chance += AosAttributes.GetValue(defender, AosAttribute.DefendChance, true) / 100d;

                    if (!Shard.SPHERE_STYLE && defender.Skills.Wrestling.Value >= 60)
                    {
                        chance += defender.Skills.Wrestling.Value / 100 * 0.30; // 30%
                    }

                    if (defender.Weapon is Fists)
                        chance += 0.1;

                    // chance += parry * 0.25; // +25%
                    //  var brace = defender.FindItemOnLayer(Layer.Bracelet) as BraceleteDoPoder;
                    // if (brace != null && brace.Tipo == TipoJoias.Escudo)
                    //     chance += brace.Bonus / 400;
                }

                if (shield is WoodenShield || shield is WoodenKiteShield)
                    chance *= 0.87;

                if (defender is BaseCreature && !((BaseCreature)defender).IsControlled())
                    chance *= 3.5;

                if (attacker is PlayerMobile && ((PlayerMobile)attacker).Talentos.Tem(Talento.Finta))
                    chance -= 0.2;

                if (attacker != null)
                {
                    var armaAtk = attacker.FindItemOnLayer(Layer.TwoHanded) as BaseRanged;
                    if (armaAtk != null)
                    {
                        Shard.Debug("Bonus parry vs arcos");
                        chance += parry / 2000; // +5% bonus parry arrows
                    }
                }

                // As per OSI, no negitive effect from the Racial stuffs, ie, 120 parry and '0' bushido with humans

                if (chance < 0) // chance shouldn't go below 0
                {
                    chance = defender.Player ? 0 : .1;
                }

                // Skill Masteries
                chance += HeightenedSensesSpell.GetParryBonus(defender);

                // Parry/Bushido over 100 grants a 5% bonus.
                if (parry >= 100.0 && bushido >= 100.0)
                {
                    chance += 0.05;
                }

                // Evasion grants a variable bonus post ML. 50% prior.
                if (Evasion.IsEvading(defender))
                {
                    chance *= Evasion.GetParryScalar(defender);
                }

                // Low dexterity lowers the chance.
                //if (defender.Player && defender.Dex < 80)
                //{
                if (defender.Player)
                    chance = chance * (defender.RawStr * 1.00) / 100;
                //}

                Shard.Debug("Chance Parry: " + chance, defender);

                return defender.CheckSkill(SkillName.Parry, chance);
            }
            else if (!(defender.Weapon is Fists) && !(defender.Weapon is BaseRanged))
            {
                BaseWeapon weapon = defender.Weapon as BaseWeapon;

                if (Core.HS && weapon.Attributes.WeaponSkillDamage > 0)
                {
                    return false;
                }

                double divisor = (weapon.Layer == Layer.OneHanded && defender.Player) ? 48000.0 : 41140.0;

                double chance = (parry * bushido) / divisor;

                double aosChance = parry / 800.0;

                // Parry or Bushido over 100 grant a 5% bonus.
                if (parry >= 100.0)
                {
                    chance += 0.05;
                    aosChance += 0.05;
                }
                else if (bushido >= 100.0)
                {
                    chance += 0.05;
                }

                // Evasion grants a variable bonus post ML. 50% prior.
                if (Evasion.IsEvading(defender))
                {
                    chance *= Evasion.GetParryScalar(defender);
                }

                // Low dexterity lowers the chance.
                if (defender.Dex < 80)
                {
                    chance = chance * (20 + defender.Dex) / 100d;
                }

                if (chance > aosChance)
                {
                    return defender.CheckSkill(SkillName.Parry, chance);
                }
                else
                {
                    return (aosChance > Utility.RandomDouble());
                    // Only skillcheck if wielding a shield & there's no effect from Bushido
                }
            }
            return false;
        }

        public Tuple<int, bool> ParryModifidaDano(Mobile attacker, Mobile defender, int damage)
        {
            Shard.Debug("Calculando Parry");
            var originalDamage = damage;
            BaseWeapon weapon = defender.Weapon as BaseWeapon;
            BaseWeapon attackerWeapon = attacker.Weapon as BaseWeapon;

            var blocked = CheckParry(defender, attacker);

            BaseShield attackerShield = attacker.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
            if (attackerShield != null && defender is BaseCreature)
            {
                damage = (int)(damage * 0.85);
            }

            BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

            if (shield != null && defender.Weapon is Fists && defender.Player && attacker is BaseCreature)
            {
                damage = (int)(damage * 0.85);
            }

            if (blocked)
            {

                attacker.SendMessage("O alvo bloqueou seu ataque");
                defender.SendMessage("Voce bloqueou o ataque");
                defender.FixedEffect(0x37B9, 10, 16);
                defender.Animate(AnimationType.Parry, 0);
                var max = (int)(damage * 0.7);
                var ratioBloqueio = (attacker is BaseCreature ? 2 : attackerWeapon is BaseRanged ? 1.2 : 1);
                var bloqueado = 0;
                if (shield != null)
                {
                    if (attacker is BaseCreature)
                    {
                        bloqueado = (int)(shield.ArmorRating * (attacker is BaseCreature ? 1.4 : attackerWeapon is BaseRanged ? 1.5 : 1.2));
                    }
                    else
                    {

                        bloqueado = (int)(shield.ArmorRating * (attackerWeapon is BaseRanged ? 1.2 : 1.0));

                    }
                }
                else
                {
                    if (!defender.Player)
                    {
                        bloqueado += (int)(damage * 0.65);
                        if (attackerWeapon is BaseRanged)
                            bloqueado += (int)(damage * 0.3);
                    }
                    else
                    {
                        bloqueado = (int)(damage * 0.2);
                    }
                }
                if (bloqueado > max)
                    bloqueado = max;

                if (Shard.DebugEnabled)
                    Shard.Debug("Bloqueado dano: " + bloqueado, defender);
                damage -= bloqueado;

                // Successful block removes the Honorable Execution penalty.
                HonorableExecution.RemovePenalty(defender);

                if (CounterAttack.IsCountering(defender))
                {
                    if (weapon != null)
                    {
                        var combatant = defender.Combatant;

                        defender.FixedParticles(0x3779, 1, 15, 0x158B, 0x0, 0x3, EffectLayer.Waist);
                        weapon.OnSwing(defender, attacker);

                        if (combatant != null && defender.Combatant != combatant && combatant.Alive)
                            defender.Combatant = combatant;
                    }

                    CounterAttack.StopCountering(defender);
                }

                if (Confidence.IsConfident(defender))
                {
                    defender.SendLocalizedMessage(1063117);
                    // Your confidence reassures you as you successfully block your opponent's blow.

                    double bushido = defender.Skills.Bushido.Value;

                    defender.Hits += Utility.RandomMinMax(1, (int)(bushido / 12)) + MasteryInfo.AnticipateHitBonus(defender) / 10;
                    defender.Stam += Utility.RandomMinMax(1, (int)(bushido / 5)) + MasteryInfo.AnticipateHitBonus(defender) / 10;
                }
                SkillMasterySpell.OnParried(attacker, defender);

                if (shield != null)
                {
                    shield.OnHit(this, damage);

                    #region Stygian Abyss
                    if (shield.ArmorAttributes.ReactiveParalyze > 0 && 30 > Utility.Random(100))
                    {
                        int secs = (int)Math.Max(1, Math.Max(8, defender.Skills[SkillName.EvalInt].Value / 10) - attacker.Skills[SkillName.MagicResist].Value / 10);

                        attacker.Paralyze(TimeSpan.FromSeconds(secs));
                        attacker.PlaySound(0x204);
                        attacker.FixedEffect(0x376A, 6, 1);
                    }
                    #endregion

                    XmlAttach.OnArmorHit(attacker, defender, shield, this, originalDamage);
                }

                #region Stygian Abyss
                else if (weapon != null && weapon.Layer == Layer.TwoHanded && weapon.WeaponAttributes.ReactiveParalyze > 0 && 30 > Utility.Random(100))
                {
                    int secs = (int)Math.Max(1, Math.Max(8, defender.Skills[SkillName.EvalInt].Value / 10) - attacker.Skills[SkillName.MagicResist].Value / 10);

                    attacker.Paralyze(TimeSpan.FromSeconds(secs));
                    attacker.PlaySound(0x204);
                    attacker.FixedEffect(0x376A, 6, 1);
                }
                #endregion

                //Skill Masteries

            }
            if (Shard.DebugEnabled)
                Shard.Debug("Dano depois do parry: " + damage, defender);
            return new Tuple<int, bool>(damage, blocked);
        }

        public virtual Tuple<int, bool> AbsorbDamageAOS(Mobile attacker, Mobile defender, int damage)
        {
            int originalDamage = damage;

            bool blocked = false;

            var mods = new Tuple<int, bool>(damage, false);

            if (defender.RP && defender.Player)
            {
                var perto = defender.GetEntitiesInRange(defender.Map, 2);
                foreach (var mob in perto)
                {
                    var pl = mob as PlayerMobile;
                    if (pl == null)
                        continue;

                    var def = pl.Talentos.Tem(Talento.Defensor);
                    var chanceBlockGrupo = 0.0;
                    if (def)
                        chanceBlockGrupo = 0.3; // 30%

                    if (chanceBlockGrupo > 0 && Utility.RandomDouble() < chanceBlockGrupo)
                    {
                        pl.MoveToWorld(mob.Location, mob.Map);
                        pl.OverheadMessage("* pulou e bloqueou *");
                        damage = (int)(damage * 0.2);
                        blocked = true;
                        defender.FixedEffect(0x37B9, 10, 16);
                        defender.SendMessage(mob.Name + " bloqueou o ataque em voce");
                    }
                }

            }

            if (defender.Player || defender.Skills[SkillName.Parry].Value > 50 || (defender is BaseCreature &&
                                                            ((BaseCreature)defender).Controlled &&
                                                            defender.Skills[SkillName.Wrestling].Base >= 100))
            {

                Shard.Debug("Checando Parry");
                mods = ParryModifidaDano(attacker, defender, damage);
                damage = mods.Item1;
                blocked = mods.Item2;
            }

            if (!blocked)
            {
                IWearableDurability toHit = GetRandomValidItem(defender) as IWearableDurability;

                if (toHit != null)
                {
                    toHit.OnHit(this, damage); // call OnHit to lose durability

                    if (toHit is Item && !((Item)toHit).Deleted && (attacker is VeriteElemental || attacker is ValoriteElemental))
                        VeriteElemental.OnHit(defender, (Item)toHit, damage);

                    damage -= XmlAttach.OnArmorHit(attacker, defender, (Item)toHit, this, originalDamage);
                }
            }

            return mods;
        }

        private Item GetRandomValidItem(Mobile m)
        {
            Item[] items = m.Items.Where(item => _DamageLayers.Contains(item.Layer) && item is IWearableDurability).ToArray();

            if (items.Length == 0)
                return null;

            return items[Utility.Random(items.Length)];
        }

        private List<Layer> _DamageLayers = new List<Layer>()
        {
            Layer.FirstValid,
            Layer.OneHanded,
            Layer.TwoHanded,
            Layer.Shoes,
            Layer.Pants,
            Layer.Shirt,
            Layer.Helm,
            Layer.Arms,
            Layer.Gloves,
            Layer.Ring,
            Layer.Talisman,
            Layer.Neck,
            Layer.Waist,
            Layer.InnerTorso,
            Layer.Bracelet,
            Layer.MiddleTorso,
            Layer.Earrings,
            Layer.Cloak,
            Layer.OuterTorso,
            Layer.OuterLegs,
            Layer.InnerLegs,
        };

        public virtual Tuple<int, bool> AbsorbDamage(Mobile attacker, Mobile defender, int d)
        {
            var damage = (double)d;
            if (Core.AOS)
            {
                return AbsorbDamageAOS(attacker, defender, (int)damage);
            }

            var mods = ParryModifidaDano(attacker, defender, (int)damage);
            damage = mods.Item1;
            var blocked = mods.Item2;

            /*
            // Parry de Monstros
            if (defender is BaseCreature && defender.Skills[SkillName.Parry].Value > 30)
            {
                var arma = attacker.FindItemOnLayer(Layer.TwoHanded);

                double chanceParry = defender.Skills[SkillName.Parry].Value / 150;

                if (((BaseCreature)defender).Controlled)
                    chanceParry *= 0.9;

                if (arma == null || !(arma is BaseRanged))
                {
                    chanceParry /= 2;
                }

                Shard.Debug("Chance Parry Mob: " + chanceParry);

                if (Utility.RandomDouble() < chanceParry)
                {
                    defender.FixedEffect(0x37B9, 10, 16);
                    damage /= 4;
                    attacker.SendMessage("A criatura bloqueou seu ataque.");
                }
            }
            */

            double chance = Utility.RandomDouble();

            Shard.Debug("Absorb Random: " + chance + " em dano " + damage);

            Item armorItem;

            if (chance < 0.07)
            {
                armorItem = defender.NeckArmor;
            }
            else if (chance < 0.14)
            {
                armorItem = defender.HandArmor;
            }
            else if (chance < 0.28)
            {
                armorItem = defender.ArmsArmor;
            }
            else if (chance < 0.43)
            {
                armorItem = defender.HeadArmor;
            }
            else if (chance < 0.65)
            {
                armorItem = defender.LegsArmor;
            }
            else
            {
                armorItem = defender.ChestArmor;
            }

            if (armorItem != null)
                Shard.Debug("Armor: " + armorItem.Name);

            IWearableDurability armor = armorItem as IWearableDurability;

            if (armor != null)
            {
                var oldDmg = damage;
                armor.OnHit(this, (int)damage);
                if (Shard.DebugEnabled)
                    Shard.Debug("Armor mitigou " + (oldDmg - damage) + " dano", defender);
            }

            var virtualArmor = defender.ArmorRating;

            if (defender.Player && !attacker.Player)
            {
                //Shard.Debug("DEFESA ANTES ELEMENTO - Armor: " + virtualArmor + " dano: " + damage, defender);
                virtualArmor += virtualArmor * ((defender.GetBonusElemento(ElementoPvM.Terra) + defender.GetBonusElemento(ElementoPvM.Luz)));
                damage -= (int)Math.Ceiling(damage * (AosAttributes.GetValue(defender, AosAttribute.DefendChance, true) / 100d));
                //Shard.Debug("DEFESA DEPOIS ELEMENTO - Armor: " + virtualArmor + " dano: " + damage, defender);
            }

            if (!defender.Player && attacker.Player)
            {
                var bonus = virtualArmor * attacker.GetBonusElemento(ElementoPvM.Vento);
                if (bonus > virtualArmor)
                    bonus = virtualArmor;
                virtualArmor -= bonus;
                damage += (int)Math.Ceiling(damage * (AosAttributes.GetValue(attacker, AosAttribute.WeaponDamage, true) / 100));
            }

            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);
            SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            WeaponAbility weavabil;
            bool bladeweaving = Bladeweave.BladeWeaving(attacker, out weavabil);
            bool ignoreArmor = (a is ArmorIgnore || (move != null && move.IgnoreArmor(attacker)) || (bladeweaving && weavabil is ArmorIgnore));

            if (a != null && a is ArmorIgnore)
            {
                virtualArmor = 0;
                Shard.Debug("Ignorou armadura", attacker);
                damage = (int)(damage * 0.9);
            }

            if (attacker.Player && attacker.RP && (attacker.Hunger < 5 || attacker.Thirst < 5))
            {
                damage /= 2;
                Shard.Debug("50% Dano Fome", attacker);
            }
            if (defender.Player && defender.RP && (defender.Hunger < 5 || defender.Thirst < 5))
            {
                damage *= 2;
                Shard.Debug("Dobro Dano Fome", defender);
            }

            if (attacker.Player && attacker.RP)
            {
                if (Shard.DebugEnabled)
                    Shard.Debug("Dano antes talentos " + damage);

                var pl = (PlayerMobile)attacker;
                if (pl.RP)
                {
                    if (pl.Talentos.Tem(Talento.Potencia))
                        damage += (int)(damage * 0.2);

                    if (pl.Weapon is BaseBashing && !pl.Talentos.Tem(Talento.Porretes))
                        damage *= 0.8;

                    if (pl.Weapon is BaseSword && !pl.Talentos.Tem(Talento.Espadas))
                        damage *= 0.8;

                    if (pl.Weapon is BaseSpear && !pl.Talentos.Tem(Talento.Lancas))
                        damage *= 0.8;

                    if (pl.Weapon is BaseAxe && !pl.Talentos.Tem(Talento.Machados))
                        damage *= 0.8;

                    if (pl.Weapon is BasePoleArm && !pl.Talentos.Tem(Talento.Hastes))
                        damage *= 0.8;

                    if (pl.Weapon is Dagger && pl.Talentos.Tem(Talento.Adagas))
                        damage *= 1.30;
                }

                if (Shard.DebugEnabled)
                    Shard.Debug("Dano depois talentos " + damage);
            }

            if (Shard.DebugEnabled)
                Shard.Debug("Armor Rating: " + virtualArmor);

            var armorMiti = XmlAttach.OnArmorHit(attacker, defender, armorItem, this, (int)damage);
            //var shieldMiti = XmlAttach.OnArmorHit(attacker, defender, shield, this, damage);
            damage -= armorMiti;
            //damage -= shieldMiti;
            if (armorMiti > 0 && Shard.DebugEnabled)
                Shard.Debug("Mitigacao ArmorXML: " + armorMiti, defender);

            if (virtualArmor > 0)
            {
                double scalar;

                if (chance < 0.14)
                {
                    scalar = 0.12;
                }
                else if (chance < 0.28)
                {
                    scalar = 0.14;
                }
                else if (chance < 0.43)
                {
                    scalar = 0.16;
                }
                else if (chance < 0.65)
                {
                    scalar = 0.17;
                }
                else
                {
                    scalar = 0.20;
                }

                if (defender is PlayerMobile)
                {
                    if (attacker is BaseCreature || defender is BaseCreature)
                    {
                        scalar *= 2.5;
                    }
                    else
                    {
                        scalar *= 3;
                    }
                }

                int from = (int)(virtualArmor * scalar) / 2;
                int to = (int)(virtualArmor * scalar);
                var redux = Utility.Random(from, (to - from) + 1);
                damage -= redux;

                if (damage < 1)
                    damage = 1;

                if (Shard.DebugEnabled)
                {
                    Shard.Debug("Virtual Armor: " + virtualArmor + " Scalar: " + scalar + " REDUX " + redux);
                    Shard.Debug("Virtual MinMax: " + from + "/" + to + " - Calculando media", defender);
                    if (attacker.Hidden)
                    {
                        Shard.Debug("Escondidinho");
                    }
                }
            }

            if (attacker.Player && !defender.Player)
            {
                var bonus = attacker.GetBonusElemento(ElementoPvM.Terra) + attacker.GetBonusElemento(ElementoPvM.Raio);
                damage += damage * bonus;
                if (Shard.DebugEnabled)
                    Shard.Debug("Bonus elemento PvM: " + bonus);
            }


            if (Shard.DebugEnabled)
                Shard.Debug("Dano depois da reducao de armor " + damage);
            return new Tuple<int, bool>((int)damage, mods.Item2);
        }

        public virtual int GetPackInstinctBonus(Mobile attacker, Mobile defender)
        {
            if (attacker.Player || defender.Player)
            {
                return 0;
            }

            BaseCreature bc = attacker as BaseCreature;

            if (bc == null || bc.PackInstinct == PackInstinct.None || (!bc.Controlled && !bc.Summoned))
            {
                return 0;
            }

            Mobile master = bc.ControlMaster;

            if (master == null)
            {
                master = bc.SummonMaster;
            }

            if (master == null)
            {
                return 0;
            }

            int inPack = 1;

            IPooledEnumerable eable = defender.GetMobilesInRange(1);

            foreach (Mobile m in eable)
            {
                if (m != attacker && m is BaseCreature)
                {
                    BaseCreature tc = (BaseCreature)m;

                    if ((tc.PackInstinct & bc.PackInstinct) == 0 || (!tc.Controlled && !tc.Summoned))
                    {
                        continue;
                    }

                    Mobile theirMaster = tc.ControlMaster;

                    if (theirMaster == null)
                    {
                        theirMaster = tc.SummonMaster;
                    }

                    if (master == theirMaster && tc.Combatant == defender)
                    {
                        ++inPack;
                    }
                }
            }

            eable.Free();

            if (inPack >= 5)
            {
                return 100;
            }
            else if (inPack >= 4)
            {
                return 75;
            }
            else if (inPack >= 3)
            {
                return 50;
            }
            else if (inPack >= 2)
            {
                return 25;
            }

            return 0;
        }

        private bool m_InDoubleStrike;
        private bool m_ProcessingMultipleHits;

        public bool InDoubleStrike
        {
            get { return m_InDoubleStrike; }
            set
            {
                m_InDoubleStrike = value;

                if (m_InDoubleStrike)
                    ProcessingMultipleHits = true;
                else
                    ProcessingMultipleHits = false;
            }
        }

        public bool ProcessingMultipleHits
        {
            get { return m_ProcessingMultipleHits; }
            set
            {
                m_ProcessingMultipleHits = value;

                if (!m_ProcessingMultipleHits)
                    BlockHitEffects = false;
            }
        }

        public bool EndDualWield { get; set; }
        public bool BlockHitEffects { get; set; }
        public DateTime NextSelfRepair { get; set; }

        public void OnHit(Mobile attacker, IDamageable damageable)
        {
            OnHit(attacker, damageable, 1.0);
        }

        public void OnPrehit(Mobile attacker, IDamageable damageable)
        {
            OnHit(attacker, damageable, 1.0);
        }

        public virtual void OnHit(Mobile attacker, IDamageable damageable, double damageBonus)
        {
            if (EndDualWield)
            {
                ProcessingMultipleHits = false;
                EndDualWield = false;
            }

            Mobile defender = damageable as Mobile;
            Clone clone = null;



            if (defender != null)
            {
                clone = MirrorImage.GetDeflect(attacker, defender);
            }

            if (clone != null)
            {
                defender = clone;
            }

            if (!Shard.COMBATE_SPHERE)
                PlaySwingAnimation(attacker);

            if(attacker.RP)
            {

            }

            if (defender != null)
                PlayHurtAnimation(defender);

            if (attacker.Player && ((PlayerMobile)attacker).Mamonita)
            {
                attacker.PlaySound(0x2E6);
                defender.PlaySound(0x2E6);
            }
            else
            {
                attacker.PlaySound(GetHitAttackSound(attacker, defender));
                if (defender != null)
                    defender.PlaySound(GetHitDefendSound(attacker, defender));
            }

            int damage = ComputeDamage(attacker, defender);

            if (Shard.DebugEnabled)
                Shard.Debug("Dano Base Final: " + damage, attacker);

            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);
            SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            if (damageable is PlayerMobile)
            {
                var tomou = damageable as PlayerMobile;
                if (tomou.RP && tomou.Mounted && !tomou.Talentos.Tem(Talento.Hipismo))
                {
                    if (Utility.RandomDouble() < 0.01)
                    {
                        tomou.Mount.Rider = null;
                        tomou.SendMessage("Voce se desequilibrou da sua montaria ao receber o golpe");
                        AOS.Damage(tomou, Utility.Random(5, 10));
                        tomou.PlayHurtSound();
                        tomou.PlayDamagedAnimation();
                        tomou.Paralyze(TimeSpan.FromSeconds(1));
                    }
                }
            }

            bool ranged = this is BaseRanged;
            int phys, fire, cold, pois, nrgy, chaos, direct;

            if (a is MovingShot)
            {
                phys = 100;
                fire = cold = pois = nrgy = chaos = direct = 0;
            }
            else
            {
                GetDamageTypes(attacker, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

                if (!OnslaughtSpell.HasOnslaught(attacker, defender) &&
                    ConsecratedContext != null &&
                    ConsecratedContext.Owner == attacker &&
                    ConsecratedContext.ConsecrateProcChance >= Utility.Random(100))
                {
                    phys = damageable.PhysicalResistance;
                    fire = damageable.FireResistance;
                    cold = damageable.ColdResistance;
                    pois = damageable.PoisonResistance;
                    nrgy = damageable.EnergyResistance;

                    int low = phys, type = 0;

                    if (fire < low) { low = fire; type = 1; }
                    if (cold < low) { low = cold; type = 2; }
                    if (pois < low) { low = pois; type = 3; }
                    if (nrgy < low) { low = nrgy; type = 4; }

                    phys = fire = cold = pois = nrgy = chaos = direct = 0;

                    if (type == 0) phys = 100;
                    else if (type == 1) fire = 100;
                    else if (type == 2) cold = 100;
                    else if (type == 3) pois = 100;
                    else if (type == 4) nrgy = 100;
                }
                else if (ranged)
                {
                    IRangeDamage rangeDamage = attacker.FindItemOnLayer(Layer.Cloak) as IRangeDamage;

                    if (rangeDamage != null)
                    {
                        rangeDamage.AlterRangedDamage(ref phys, ref fire, ref cold, ref pois, ref nrgy, ref chaos, ref direct);
                    }
                }
            }

            var transform = AnimalForm.GetContext(attacker);
            if (transform != null)
            {
                if (transform.Type == typeof(Rabbit))
                {
                    damageBonus -= 0.50;
                }

                if (transform.Type == typeof(BullFrog))
                {
                    damageBonus -= 0.15;
                    attacker.MovingEffect(damageable, 0x0F53, 18, 1, false, false, 78, 1);
                }

                if (transform.Type == typeof(Llama) && this is BaseRanged)
                {
                    damageBonus -= 0.05;
                }

                if (transform.Type == typeof(GreyWolf) && !(this is BaseRanged))
                {
                    if (!BleedAttack.IsBleeding(defender) && Utility.RandomDouble() < 0.25)
                    {
                        attacker.OverheadMessage("* mordida feroz *");
                        BleedAttack.BeginBleed(defender, attacker);
                    }
                }
            }

            bool splintering = false;
            if (!(a is Disarm) && m_AosWeaponAttributes.SplinteringWeapon > 0 && m_AosWeaponAttributes.SplinteringWeapon > Utility.Random(100))
            {
                if (SplinteringWeaponContext.CheckHit(attacker, defender, this))
                    splintering = true;
            }

            double chance = NegativeAttributes.Antique > 0 ? 5 : 0;
            bool acidicTarget = MaxRange <= 1 && m_AosAttributes.ResistMagica == 0 && !(this is Fists) && (defender is Slime || defender is ToxicElemental || defender is CorrosiveSlime);

            if (acidicTarget || (defender != null && splintering) || Utility.Random(40) <= chance)
            {
                if (MaxRange <= 1 && acidicTarget)
                {
                    attacker.PrivateOverheadMessage("O sangue acido corroi sua arma"); // *Acid blood scars your weapon!*
                }

                int selfRepair = !Core.AOS ? 0 : m_AosWeaponAttributes.SelfRepair + (IsSetItem && m_SetEquipped ? m_SetSelfRepair : 0);

                if (selfRepair > 0 && NextSelfRepair < DateTime.UtcNow)
                {
                    HitPoints += selfRepair;

                    NextSelfRepair = DateTime.UtcNow + TimeSpan.FromSeconds(60);
                }
                else
                {
                    if (m_MaxHits > 0)
                    {
                        if (m_Hits >= 1)
                        {
                            if (splintering)
                            {
                                HitPoints = Math.Max(0, HitPoints - 10);
                            }
                            else
                            {
                                HitPoints--;
                            }
                        }
                        else if (m_MaxHits > 0)
                        {
                            MaxHitPoints--;

                            if (Parent is Mobile)
                                ((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.

                            if (m_MaxHits <= 0)
                                Delete();
                        }
                    }
                }
            }

            /*
            WeaponAbility weavabil;
            bool bladeweaving = Bladeweave.BladeWeaving(attacker, out weavabil);
            bool ignoreArmor = (a is ArmorIgnore || (move != null && move.IgnoreArmor(attacker)) || (bladeweaving && weavabil is ArmorIgnore));

            // object is not a mobile, so we end here
            if (defender == null)
            {
                AOS.Damage(damageable, attacker, damage, ignoreArmor, phys, fire, cold, pois, nrgy, chaos, direct, false, ranged ? Server.DamageType.Ranged : Server.DamageType.Melee);

                // TODO: WeaponAbility/SpecialMove OnHit(...) convert target to IDamageable
                // Figure out which specials work on items. For now AI only.
                if (ignoreArmor)
                {
                    Effects.PlaySound(damageable.Location, damageable.Map, 0x56);
                    Effects.SendTargetParticles(damageable, 0x3728, 200, 25, 0, 0, 9942, EffectLayer.Waist, 0);
                }

                WeaponAbility.ClearCurrentAbility(attacker);
                SpecialMove.ClearCurrentMove(attacker);

                return;
            }
            */

            #region Damage Multipliers
            /*
            * The following damage bonuses multiply damage by a factor.
            * Capped at x3 (300%).
            */
            int percentageBonus = 0;

            if (a != null)
            {
                var bonus = defender.Player ? a.DamageScalar : (a.DamageScalar * 1.3);
                if (!defender.Player)
                {
                    var skillDmg = AosAttributes.GetValue(attacker, AosAttribute.WeaponSkillDamage, true) / 100d;
                    bonus += skillDmg;

                }
                percentageBonus += (int)(bonus * 100) - 100;
            }

            if (move != null)
            {
                percentageBonus += (int)(move.GetDamageScalar(attacker, defender) * 100) - 100;
            }

            if (Shard.DebugEnabled)
                Shard.Debug("Null? " + (ConsecratedContext == null));
            if (ConsecratedContext != null && ConsecratedContext.Owner == attacker)
            {
                percentageBonus += ConsecratedContext.ConsecrateDamageBonus;
                if (Shard.DebugEnabled)
                    Shard.Debug("Bonus Dano Consecrated Weapon: " + ConsecratedContext.ConsecrateDamageBonus);
            }

            percentageBonus += (int)(damageBonus * 100) - 100;

            CheckSlayerResult cs1 = CheckSlayers(attacker, defender, Slayer);
            CheckSlayerResult cs2 = CheckSlayers(attacker, defender, Slayer2);
            CheckSlayerResult suit = CheckSlayers(attacker, defender, SetHelper.GetSetSlayer(attacker));
            CheckSlayerResult tal = CheckTalismanSlayer(attacker, defender);

            if (cs1 == CheckSlayerResult.None && cs2 == CheckSlayerResult.None)
            {
                cs1 = CheckSlayers(attacker, defender, SlayerSocket.GetSlayer(this));
            }

            if (cs1 != CheckSlayerResult.None)
            {
                if (cs1 == CheckSlayerResult.SuperSlayer)
                    percentageBonus += 75;
                else if (cs1 == CheckSlayerResult.Slayer)
                    percentageBonus += 150;
            }

            if (cs2 != CheckSlayerResult.None)
            {
                if (cs2 == CheckSlayerResult.SuperSlayer)
                    percentageBonus += 75;
                else if (cs2 == CheckSlayerResult.Slayer)
                    percentageBonus += 150;
            }

            if (suit != CheckSlayerResult.None)
            {
                percentageBonus += 75;
            }

            if (tal != CheckSlayerResult.None)
            {
                percentageBonus += 75;

            }

            if (CheckSlayerOpposition(attacker, defender) != CheckSlayerResult.None)
            {
                percentageBonus += 75;
                defender.FixedEffect(0x37B9, 10, 5);
            }
            else if (cs1 != CheckSlayerResult.None || cs2 != CheckSlayerResult.None || suit != CheckSlayerResult.None || tal != CheckSlayerResult.None)
            {
                defender.FixedEffect(0x37B9, 10, 5);
            }

            #region Enemy of One
            var enemyOfOneContext = EnemyOfOneSpell.GetContext(defender);

            if (enemyOfOneContext != null && !enemyOfOneContext.IsWaitingForEnemy && !enemyOfOneContext.IsEnemy(attacker))
            {
                percentageBonus += 100;
            }
            else
            {
                enemyOfOneContext = EnemyOfOneSpell.GetContext(attacker);

                if (enemyOfOneContext != null)
                {
                    enemyOfOneContext.OnHit(defender);

                    if (enemyOfOneContext.IsEnemy(defender))
                    {
                        defender.FixedEffect(0x37B9, 10, 5, 1160, 0);
                        percentageBonus += enemyOfOneContext.DamageScalar;
                    }
                }
            }
            #endregion

            int packInstinctBonus = GetPackInstinctBonus(attacker, defender);

            if (packInstinctBonus != 0)
            {
                percentageBonus += packInstinctBonus;
            }

            if (m_InDoubleStrike)
            {
                percentageBonus -= 10;
            }

            TransformContext context = TransformationSpellHelper.GetContext(defender);

            if ((m_Slayer == SlayerName.Undeads || m_Slayer2 == SlayerName.Undeads || SetHelper.GetSetSlayer(attacker) == SlayerName.Undeads)
                && ((context != null && context.Spell is NecromancerSpell && context.Type != typeof(HorrificBeastSpell))
                || (defender is BaseCreature && (defender.Body == 747 || defender.Body == 748 || defender.Body == 749 || defender.Hue == 0x847E))))
            {
                // Every necromancer transformation other than horrific beast takes an additional 25% damage
                percentageBonus += 25;
            }

            if (attacker is PlayerMobile && !(Core.ML && defender is PlayerMobile))
            {
                PlayerMobile pmAttacker = (PlayerMobile)attacker;

                if (pmAttacker.HonorActive && pmAttacker.InRange(defender, 1))
                {
                    percentageBonus += 25;
                }

                if (pmAttacker.SentHonorContext != null && pmAttacker.SentHonorContext.Target == defender)
                {
                    percentageBonus += pmAttacker.SentHonorContext.PerfectionDamageBonus;
                }
            }

            percentageBonus -= Block.GetMeleeReduction(defender);

            #region Stygian Abyss
            percentageBonus += BattleLust.GetBonus(attacker, defender);

            if (this is BaseThrown)
            {
                double dist = attacker.GetDistanceToSqrt(defender);
                int max = ((BaseThrown)this).MaxThrowRange;

                if (dist > max)
                    percentageBonus -= 47;
            }

            if (RunedSashOfWarding.IsUnderEffects(defender, WardingEffect.WeaponDamage) && !(attacker is PlayerMobile))
                percentageBonus -= 10;
            #endregion


            BaseTalisman talisman = attacker.Talisman as BaseTalisman;

            if (talisman != null && talisman.Killer != null)
            {
                percentageBonus += talisman.Killer.DamageBonus(defender);
            }

            if (talisman != null && talisman.Charges > 0 && tal != CheckSlayerResult.None)
            {
                talisman.Charges -= 1;
            }

            if (this is ButchersWarCleaver)
            {
                if (defender is Bull || defender is Cow || defender is Gaman)
                {
                    percentageBonus += 100;
                }
            }

            percentageBonus += ForceOfNature.GetBonus(attacker, defender);

            if (m_ExtendedWeaponAttributes.AssassinHoned > 0 && GetOppositeDir(attacker.Direction) == defender.Direction)
            {
                if (!ranged || 0.5 > Utility.RandomDouble())
                {
                    percentageBonus += (int)(146.0 / MlSpeed);
                }
            }

            percentageBonus = Math.Min(percentageBonus, 300);

            // bonus is seprate from weapon damage, ie not capped
            percentageBonus += Spells.Mysticism.StoneFormSpell.GetMaxResistBonus(attacker);

            damage = AOS.Scale(damage, 100 + percentageBonus);

            #endregion

            var mods = AbsorbDamage(attacker, defender, damage);
            damage = mods.Item1;
            var blocked = mods.Item2;
            if (!Core.AOS && damage < 1)
            {
                damage = 1;
            }

            if (attacker.Player && !defender.Player && !attacker.Poisoned && !BleedAttack.IsBleeding(attacker))
            {
                var bonusCura = attacker.GetBonusElemento(ElementoPvM.Luz);
                var cura = (damage / 2) * bonusCura;               
                
                if (cura > 0)
                    attacker.Heal((int)cura);
            }
            if (attacker.Player && !defender.Player && (attacker.Poisoned || BleedAttack.IsBleeding(attacker)))
            {
                var bonusCura = attacker.GetBonusElemento(ElementoPvM.Luz);
                // Adiciona chance de curar veneno ou sangramento
                var chanceCura = bonusCura / 5;
                if (Utility.RandomDouble() <= chanceCura)
                {
                    if (attacker.Poisoned)
                    {
                        attacker.CurePoison(attacker);
                    }
                    if (BleedAttack.IsBleeding(attacker))
                    {
                        BleedAttack.EndBleed(attacker, true);
                    }
                }
            }

            if (blocked) // parried
            {
                if ((a != null && a.Validate(attacker)) || (move != null && move.Validate(attacker)))
                // Parried special moves have no mana cost - era specific
                {
                    if (true) // Core.SE || (a != null && a.CheckMana(attacker, true)))
                    {
                        WeaponAbility.ClearCurrentAbility(attacker);
                        SpecialMove.ClearCurrentMove(attacker);

                        attacker.SendLocalizedMessage("Seu ataque foi bloqueado"); // Your attack was parried!
                    }
                }

                //if(attacker.Player && defender.Player)
                //    return;
            }

            // Skill Masteries
            if (WhiteTigerFormSpell.CheckEvasion(defender))
            {
                defender.Emote("* esquivou *"); // Is this right?
                return;
            }

            #region Mondain's Legacy
            if (ImmolatingWeaponSpell.IsImmolating(attacker, this))
            {
                int d = ImmolatingWeaponSpell.GetImmolatingDamage(attacker);
                d = AOS.Damage(defender, attacker, d, 0, 100, 0, 0, 0);

                ImmolatingWeaponSpell.DoDelayEffect(attacker, defender);

                if (d > 0)
                {
                    defender.Damage(d);
                }
            }
            #endregion

            #region SA
            if (defender != null && m_SearingWeapon && attacker.Mana > 0)
            {
                int d = SearingWeaponContext.Damage;

                if ((ranged && 10 > Utility.Random(100)) || 20 > Utility.Random(100))
                {
                    AOS.Damage(defender, attacker, d, 0, 100, 0, 0, 0);
                    AOS.Damage(attacker, null, 4, false, 0, 0, 0, 0, 0, 0, 100, false, false, false);

                    defender.FixedParticles(0x36F4, 1, 11, 0x13A8, 0, 0, EffectLayer.Waist);

                    SearingWeaponContext.CheckHit(attacker, defender);
                    attacker.Mana--;
                }
            }
            #endregion

            #region BoneBreaker/Swarm/Sparks
            bool sparks = false;
            if (a == null && move == null)
            {
                if (m_ExtendedWeaponAttributes.BoneBreaker > 0)
                    damage += BoneBreakerContext.CheckHit(attacker, defender);

                if (m_ExtendedWeaponAttributes.HitSwarm > 0 && Utility.Random(100) < m_ExtendedWeaponAttributes.HitSwarm)
                    SwarmContext.CheckHit(attacker, defender);

                if (m_ExtendedWeaponAttributes.HitSparks > 0 && Utility.Random(100) < m_ExtendedWeaponAttributes.HitSparks)
                {
                    SparksContext.CheckHit(attacker, defender);
                    sparks = true;
                }
            }
            #endregion

            Timer.DelayCall(d => AddBlood(d, damage), defender);

            int damageGiven = damage;

            if (a != null && !a.OnBeforeDamage(attacker, defender))
            {
                WeaponAbility.ClearCurrentAbility(attacker);
                a = null;
            }

            if (move != null && !move.OnBeforeDamage(attacker, defender))
            {
                SpecialMove.ClearCurrentMove(attacker);
                move = null;
            }

            if (Feint.Registry.ContainsKey(defender) && Feint.Registry[defender].Enemy == attacker)
                damage -= (int)((double)damage * ((double)Feint.Registry[defender].DamageReduction / 100));

            // Skill Masteries
            if (this is Fists)
                damage += (int)((double)damage * ((double)MasteryInfo.GetKnockoutModifier(attacker, defender is PlayerMobile) / 100.0));

            SkillMasterySpell.OnHit(attacker, defender, ref damage);

            // Bane
            if (m_ExtendedWeaponAttributes.Bane > 0 && defender.Hits < defender.HitsMax / 2)
            {
                double inc = Math.Min(350, (double)defender.HitsMax * .3);
                inc -= (double)((double)defender.Hits / (double)defender.HitsMax) * inc;

                Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0x30, 3);

                damage += (int)inc;
            }

            if(!defender.Player)
            {
                double colarVento = ColarElemental.GetNivel(attacker, ElementoPvM.Vento);
                double chancevento = 0.10d + (colarVento / 120d);
                Shard.Debug("Chance de Critico: " + chancevento);
                if (colarVento > 0 && Utility.RandomDouble() < chancevento)
                {
                    defender.PublicOverheadMessage(MessageType.Regular, 38, true, "* crit *");
                    damage *= 2;
                }

                double colarRaio = ColarElemental.GetNivel(attacker, ElementoPvM.Raio);
                double chanceraio = 0.10d + (colarRaio / 100d);
                Shard.Debug("Chance Golpe Eletrico: " + chanceraio);
                if (colarRaio > 0 && Utility.RandomDouble() < chanceraio)
                {
                    DoLightning(attacker, defender);
                    attacker.OverheadMessage("* Golpe Eletrico *");
                }
            }

            damageGiven = AOS.Damage(
                defender,
                attacker,
                damage,
                false,
                phys,
                fire,
                cold,
                pois,
                nrgy,
                chaos,
                direct,
                false,
                ranged ? Server.DamageType.Ranged : Server.DamageType.Melee);

            DualWield.DoHit(attacker, defender, damage);

            if (TransformationSpellHelper.UnderTransformation(defender, typeof(LichFormSpell)))
            {
                damage = (int)(damage * 1.6);
            }

            if (CurseWeaponSpell.IsCursed(attacker, this))
            {
                var pct = attacker.Skills[SkillName.Necromancy].Value / 2;
                var toHealCursedWeaponSpell = (int)(AOS.Scale(damageGiven, (int)pct)); // Additional 50% life leech for cursed weapons (necro spell)
                if (toHealCursedWeaponSpell > 0)
                    attacker.Heal(toHealCursedWeaponSpell, attacker, false);
            }

            if (sparks)
            {
                int mana = attacker.Mana + damageGiven;
                if (!defender.Player) mana *= 2;
                attacker.Mana = Math.Min(attacker.ManaMax, attacker.Mana + mana);
            }

            double propertyBonus = (move == null) ? 1.0 : move.GetPropertyBonus(attacker);

            if (attacker is VampireBatFamiliar || attacker is VampireBat)
            {
                BaseCreature bc = (BaseCreature)attacker;
                Mobile caster = bc.ControlMaster;

                if (caster == null)
                {
                    caster = bc.SummonMaster;
                }

                if (caster != null && caster.Map == bc.Map && caster.InRange(bc, 2))
                {
                    caster.Hits += damage;
                }
                else
                {
                    bc.Hits += damage;
                }
            }

            if (!defender.Player || Core.AOS)
            {
                int lifeLeech = 0;
                int stamLeech = 0;
                int manaLeech = 0;

                if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechStam) * propertyBonus) >
                    Utility.Random(100))
                {
                    stamLeech += 100; // HitLeechStam% chance to leech 100% of damage as stamina
                }

                if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechHits) * propertyBonus) >
                    Utility.Random(100))
                {
                    lifeLeech += 30; // HitLeechHits% chance to leech 30% of damage as hit points
                }

                if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechMana) * propertyBonus) >
                    Utility.Random(100))
                {
                    manaLeech += 40; // HitLeechMana% chance to leech 40% of damage as mana
                }

                int toHealCursedWeaponSpell = 0;

                if (CurseWeaponSpell.IsCursed(attacker, this))
                {
                    toHealCursedWeaponSpell += (int)(AOS.Scale(damageGiven, 50)); // Additional 50% life leech for cursed weapons (necro spell)
                }

                context = TransformationSpellHelper.GetContext(attacker);

                if (stamLeech != 0)
                {
                    attacker.Stam += AOS.Scale(damageGiven, stamLeech);
                }

                if (toHealCursedWeaponSpell != 0)
                {
                    attacker.Hits += toHealCursedWeaponSpell;
                }

                if (lifeLeech != 0)
                {
                    attacker.Hits += AOS.Scale(damageGiven, lifeLeech);
                }

                if (manaLeech != 0)
                {
                    attacker.Mana += AOS.Scale(damageGiven, manaLeech);
                }

                if (lifeLeech != 0 || stamLeech != 0 || manaLeech != 0 || toHealCursedWeaponSpell != 0)
                {
                    attacker.PlaySound(0x44D);
                }
            }

            if (!defender.Player && !BlockHitEffects)
            {
                int physChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitPhysicalArea) * propertyBonus);
                int fireChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireArea) * propertyBonus);
                int coldChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitColdArea) * propertyBonus);
                int poisChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitPoisonArea) * propertyBonus);
                int nrgyChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitEnergyArea) * propertyBonus);

                if (physChance != 0 && physChance > Utility.Random(100))
                {
                    DoAreaAttack(attacker, defender, damageGiven, 0x10E, 50, 100, 0, 0, 0, 0);
                }

                if (fireChance != 0 && fireChance > Utility.Random(100))
                {
                    DoAreaAttack(attacker, defender, damageGiven, 0x11D, 1160, 0, 100, 0, 0, 0);
                }

                if (coldChance != 0 && coldChance > Utility.Random(100))
                {
                    DoAreaAttack(attacker, defender, damageGiven, 0x0FC, 2100, 0, 0, 100, 0, 0);
                }

                if (poisChance != 0 && poisChance > Utility.Random(100))
                {
                    DoAreaAttack(attacker, defender, damageGiven, 0x205, 1166, 0, 0, 0, 100, 0);
                }

                if (nrgyChance != 0 && nrgyChance > Utility.Random(100))
                {
                    DoAreaAttack(attacker, defender, damageGiven, 0x1F1, 120, 0, 0, 0, 0, 100);
                }

                int maChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitMagicArrow) * propertyBonus);
                int harmChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitHarm) * propertyBonus);
                int fireballChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireball) * propertyBonus);
                int lightningChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLightning) * propertyBonus);
                int dispelChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitDispel) * propertyBonus);

                if (Shard.DebugEnabled)
                {
                    Shard.Debug("Change light " + lightningChance);
                }

                #region Mondains Legacy
                int velocityChance = this is BaseRanged ? (int)((BaseRanged)this).Velocity : 0;
                #endregion

                #region Stygian Abyss
                int curseChance = (int)(m_AosWeaponAttributes.HitCurse * propertyBonus);
                int fatigueChance = (int)(m_AosWeaponAttributes.HitFatigue * propertyBonus);
                int manadrainChance = (int)(m_AosWeaponAttributes.HitManaDrain * propertyBonus);
                #endregion

                if (maChance != 0 && maChance > Utility.Random(100))
                {
                    DoMagicArrow(attacker, defender);
                }

                if (harmChance != 0 && harmChance > Utility.Random(100))
                {
                    DoHarm(attacker, defender);
                }

                if (fireballChance != 0 && fireballChance > Utility.Random(100))
                {
                    DoFireball(attacker, defender);
                }

                if (lightningChance != 0 && lightningChance > Utility.Random(100))
                {
                    DoLightning(attacker, defender);
                }

                if (dispelChance != 0 && dispelChance > Utility.Random(100))
                {
                    DoDispel(attacker, defender);
                }

                #region Mondains Legacy
                if (velocityChance != 0 && velocityChance > Utility.Random(100))
                {
                    DoHitVelocity(attacker, damageable);
                }
                #endregion

                #region Stygian Abyss
                if (curseChance != 0 && curseChance > Utility.Random(100))
                {
                    DoCurse(attacker, defender);
                }

                if (fatigueChance != 0 && fatigueChance > Utility.Random(100))
                {
                    DoFatigue(attacker, defender, damageGiven);
                }

                if (manadrainChance != 0 && manadrainChance > Utility.Random(100))
                {
                    DoManaDrain(attacker, defender, damageGiven);
                }

                int laChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLowerAttack) * propertyBonus);

                if (laChance != 0 && laChance > Utility.Random(100))
                {
                    DoLowerAttack(attacker, defender);
                }

                if (!Core.HS)
                {
                    int ldChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLowerDefend) * propertyBonus);

                    if (ldChance != 0 && ldChance > Utility.Random(100))
                    {
                        DoLowerDefense(attacker, defender);
                    }
                }
                else
                {
                    int hldWep = m_AosWeaponAttributes.HitLowerDefend;
                    int hldGlasses = 0;

                    var helm = attacker.FindItemOnLayer(Layer.Helm);

                    if (helm != null)
                    {
                        var attrs = RunicReforging.GetAosWeaponAttributes(helm);

                        if (attrs != null)
                            hldGlasses = attrs.HitLowerDefend;
                    }

                    if ((hldWep > 0 && hldWep > Utility.Random(100)) || (hldGlasses > 0 && hldGlasses > Utility.Random(100)))
                    {
                        DoLowerDefense(attacker, defender);
                    }
                }
            }

            if (attacker is BaseCreature)
            {
                ((BaseCreature)attacker).OnGaveMeleeAttack(defender);
            }

            if (defender is BaseCreature)
            {
                ((BaseCreature)defender).OnGotMeleeAttack(attacker);
            }

            if (a != null)
            {
                a.OnHit(attacker, defender, damage);
                //a.ApplyCooldown(attacker);
            }

            if (move != null)
            {
                move.OnHit(attacker, defender, damage);
            }

            if (defender is IHonorTarget && ((IHonorTarget)defender).ReceivedHonorContext != null)
            {
                ((IHonorTarget)defender).ReceivedHonorContext.OnTargetHit(attacker);
            }

            if (!ranged)
            {
                if (AnimalForm.UnderTransformation(attacker, typeof(GiantSerpent)))
                {
                    defender.ApplyPoison(attacker, Poison.Lesser);
                }
            }

            BaseFamiliar.OnHit(attacker, damageable);
            WhiteTigerFormSpell.OnHit(attacker, defender);
            XmlAttach.OnWeaponHit(this, attacker, defender, damageGiven);
        }

        public Direction GetOppositeDir(Direction d)
        {
            Direction direction = Direction.Down;

            if (d == Direction.West)
                direction = Direction.East;

            if (d == Direction.East)
                direction = Direction.West;

            if (d == Direction.North)
                direction = Direction.South;

            if (d == Direction.South)
                direction = Direction.North;

            if (d == Direction.Right)
                direction = Direction.Left;

            if (d == Direction.Left)
                direction = Direction.Right;

            if (d == Direction.Up)
                direction = Direction.Down;

            if (d == Direction.Down)
                direction = Direction.Up;

            return direction;
        }

        public virtual double GetAosSpellDamage(Mobile attacker, Mobile defender, int bonus, int dice, int sides)
        {
            int damage = Utility.Dice(dice, sides, bonus) * 100;
            int damageBonus = 0;

            int inscribeSkill = attacker.Skills[SkillName.Inscribe].Fixed;
            int inscribeBonus = (inscribeSkill + (1000 * (inscribeSkill / 1000))) / 200;

            damageBonus += inscribeBonus;
            damageBonus += attacker.Int / 10;
            damageBonus += SpellHelper.GetSpellDamageBonus(attacker, defender, SkillName.Magery, attacker is PlayerMobile && defender is PlayerMobile);
            damage = AOS.Scale(damage, 100 + damageBonus);

            if (defender != null && Feint.Registry.ContainsKey(defender) && Feint.Registry[defender].Enemy == attacker)
                damage -= (int)((double)damage * ((double)Feint.Registry[defender].DamageReduction / 100));

            // All hit spells use 80 eval
            int evalScale = 30 + ((9 * 800) / 100);

            damage = AOS.Scale(damage, evalScale);

            return damage / 100d;
        }

        #region Do<AoSEffect>
        public virtual void DoMagicArrow(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
            {
                return;
            }

            attacker.DoHarmful(defender);

            double damage = GetAosSpellDamage(attacker, defender, 10, 1, 4);

            attacker.MovingParticles(defender, 0x36E4, 5, 0, false, true, 3006, 4006, 0);
            attacker.PlaySound(0x1E5);

            SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }

        public virtual void DoHarm(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
            {
                return;
            }

            attacker.DoHarmful(defender);

            double damage = GetAosSpellDamage(attacker, defender, 17, 1, 5);

            if (!defender.InRange(attacker, 2))
            {
                damage *= 0.25; // 1/4 damage at > 2 tile range
            }
            else if (!defender.InRange(attacker, 1))
            {
                damage *= 0.50; // 1/2 damage at 2 tile range
            }

            defender.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
            defender.PlaySound(0x0FC);

            SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 100, 0, 0);

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }

        public virtual void DoFireball(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
            {
                return;
            }

            attacker.DoHarmful(defender);

            double damage = GetAosSpellDamage(attacker, defender, 19, 1, 5);

            attacker.MovingParticles(defender, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
            attacker.PlaySound(0x15E);

            SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }

        public virtual void DoLightning(Mobile attacker, Mobile defender)
        {
            Shard.Debug("Hit light");
            if (!attacker.CanBeHarmful(defender, false))
            {
                return;
            }

            attacker.DoHarmful(defender);

            double damage = GetAosSpellDamage(attacker, defender, 23, 1, 4);

            defender.BoltEffect(0);

            SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 0, 0, 100);

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }

        public virtual void DoDispel(Mobile attacker, Mobile defender)
        {
            bool dispellable = false;

            if (defender is BaseCreature)
            {
                dispellable = ((BaseCreature)defender).Summoned && !((BaseCreature)defender).IsAnimatedDead;
            }

            if (!dispellable)
            {
                return;
            }

            if (!attacker.CanBeHarmful(defender, false))
            {
                return;
            }

            attacker.DoHarmful(defender);

            MagerySpell sp = new DispelSpell(attacker, null);

            if (sp.CheckResisted(defender))
            {
                defender.FixedEffect(0x3779, 10, 20);
            }
            else
            {
                Effects.SendLocationParticles(
                    EffectItem.Create(defender.Location, defender.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                Effects.PlaySound(defender, defender.Map, 0x201);

                defender.Delete();
            }
        }

        public virtual void DoHitVelocity(Mobile attacker, IDamageable damageable)
        {
            int bonus = (int)attacker.GetDistanceToSqrt(damageable);

            if (bonus > 0)
            {
                AOS.Damage(damageable, attacker, bonus * 3, 100, 0, 0, 0, 0);

                if (attacker.Player)
                {
                    attacker.SendLocalizedMessage(1072794); // Your arrow hits its mark with velocity!
                }

                if (damageable is Mobile && ((Mobile)damageable).Player)
                {
                    ((Mobile)damageable).SendLocalizedMessage(1072795); // You have been hit by an arrow with velocity!
                }
            }

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }

        #region Stygian Abyss
        public virtual void DoCurse(Mobile attacker, Mobile defender)
        {
            attacker.SendLocalizedMessage(1113717); // You have hit your target with a curse effect.
            defender.SendLocalizedMessage(1113718); // You have been hit with a curse effect.

            defender.FixedParticles(0x374A, 10, 15, 5028, EffectLayer.Waist);
            defender.PlaySound(0x1EA);
            TimeSpan duration = TimeSpan.FromSeconds(30);

            defender.AddStatMod(
                new StatMod(StatType.Str, String.Format("[Magic] {0} Curse", StatType.Str), -10, duration));
            defender.AddStatMod(
                new StatMod(StatType.Dex, String.Format("[Magic] {0} Curse", StatType.Dex), -10, duration));
            defender.AddStatMod(
                new StatMod(StatType.Int, String.Format("[Magic] {0} Curse", StatType.Int), -10, duration));

            int percentage = -10; //(int)(SpellHelper.GetOffsetScalar(Caster, m, true) * 100);
            string args = String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", percentage, percentage, percentage, 10, 10, 10, 10);

            Server.Spells.Fourth.CurseSpell.AddEffect(defender, duration, 10, 10, 10);
            BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.Curse, 1075835, 1075836, duration, defender, args));

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }

        public virtual void DoFatigue(Mobile attacker, Mobile defender, int damagegiven)
        {
            // Message?
            // Effects?
            defender.Stam -= (damagegiven * (100 - m_AosWeaponAttributes.HitFatigue)) / 100;

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }

        public virtual void DoManaDrain(Mobile attacker, Mobile defender, int damagegiven)
        {
            // Message?
            defender.FixedParticles(0x3789, 10, 25, 5032, EffectLayer.Head);
            defender.PlaySound(0x1F8);
            defender.Mana -= (damagegiven * (100 - m_AosWeaponAttributes.HitManaDrain)) / 100;

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }
        #endregion

        public virtual void DoLowerAttack(Mobile from, Mobile defender)
        {
            if (HitLower.ApplyAttack(defender))
            {
                defender.PlaySound(0x28E);
                Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0xA, 3);
            }
        }

        public virtual void DoLowerDefense(Mobile from, Mobile defender)
        {
            if (HitLower.ApplyDefense(defender))
            {
                defender.PlaySound(0x28E);
                Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0x23, 3);
            }
        }

        public virtual void DoAreaAttack(Mobile from, Mobile defender, int damageGiven, int sound, int hue, int phys, int fire, int cold, int pois, int nrgy)
        {
            Map map = from.Map;

            if (map == null)
            {
                return;
            }

            var list = SpellHelper.AcquireIndirectTargets(from, from, defender.Map, 5);

            var count = 0;

            foreach (var m in list)
            {
                if (!(m is PlayerMobile) && !(((BaseCreature)m).Controlled && ((BaseCreature)m).ControlMaster is PlayerMobile))
                {
                    ++count;
                    from.DoHarmful(m, true);
                    m.FixedEffect(0x3779, 1, 15, hue, 0);
                    AOS.Damage(m, from, (int)(damageGiven / 2), phys, fire, cold, pois, nrgy, Server.DamageType.SpellAOE);
                }
            }

            if (count > 0)
            {
                Effects.PlaySound(from.Location, map, sound);
            }

            if (ProcessingMultipleHits)
                BlockHitEffects = true;
        }
        #endregion

        public virtual CheckSlayerResult CheckSlayers(Mobile attacker, Mobile defender, SlayerName slayer)
        {
            if (slayer == SlayerName.None)
                return CheckSlayerResult.None;

            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(slayer);

            if (atkSlayer != null && atkSlayer.Slays(defender) && _SuperSlayers.Contains(atkSlayer.Name))
            {
                return CheckSlayerResult.SuperSlayer;
            }

            if (atkSlayer != null && atkSlayer.Slays(defender))
            {
                return CheckSlayerResult.Slayer;
            }

            return CheckSlayerResult.None;
        }

        public CheckSlayerResult CheckSlayerOpposition(Mobile attacker, Mobile defender)
        {
            ISlayer defISlayer = Spellbook.FindEquippedSpellbook(defender);

            if (defISlayer == null)
            {
                defISlayer = defender.Weapon as ISlayer;
            }

            if (defISlayer != null)
            {
                SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
                SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);
                SlayerEntry defSetSlayer = SlayerGroup.GetEntryByName(SetHelper.GetSetSlayer(defender));

                if (defISlayer is Item && defSlayer == null && defSlayer2 == null)
                {
                    defSlayer = SlayerGroup.GetEntryByName(SlayerSocket.GetSlayer((Item)defISlayer));
                }

                if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(attacker) ||
                    defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(attacker) ||
                    defSetSlayer != null && defSetSlayer.Group.OppositionSuperSlays(attacker))
                {
                    return CheckSlayerResult.Opposition;
                }
            }

            return CheckSlayerResult.None;
        }

        public CheckSlayerResult CheckTalismanSlayer(Mobile attacker, Mobile defender)
        {
            BaseTalisman talisman = attacker.Talisman as BaseTalisman;

            if (talisman != null && talisman.MaxCharges > 0 && talisman.Charges <= 0)
                return CheckSlayerResult.None;

            if (talisman != null && TalismanSlayer.Slays(talisman.Slayer, defender))
            {
                return CheckSlayerResult.Slayer;
            }
            else if (Slayer3 != TalismanSlayerName.None && TalismanSlayer.Slays(Slayer3, defender))
            {
                return CheckSlayerResult.Slayer;
            }

            return CheckSlayerResult.None;
        }

        private List<SlayerName> _SuperSlayers = new List<SlayerName>()
        {
            SlayerName.Humanoides, SlayerName.Undeads, SlayerName.Fadas,
            SlayerName.Elemental, SlayerName.Exorcismo, SlayerName.Aracnideos,
            SlayerName.Repteis, SlayerName.Dinossauros, SlayerName.Myrmidex,
            SlayerName.Eodon
        };

        #region Blood
        public static void AddBlood(Mobile defender, int damage)
        {
            if (damage <= 5 || defender == null || defender.Map == null || !defender.HasBlood || !CanDrawBlood(defender))
            {
                return;
            }

            var m = defender.Map;
            var b = new Rectangle2D(defender.X - 1, defender.Y - 1, 3, 3);

            var count = 3;

            if (damage < 10)
                count = 1;
            else if (damage < 20)
                count = 2;
            else
                count = 3;

            for (var i = 0; i < count; i++)
            {
                AddBlood(defender, m.GetRandomSpawnPoint(b, defender.Location.Z), m);
            }
        }

        public static void AddBlood(Mobile defender, Point3D target, Map map)
        {
            var blood = CreateBlood(defender);

            var id = blood.ItemID;

            blood.ItemID = 1; // No Draw

            blood.OnBeforeSpawn(target, map);
            blood.MoveToWorld(target, map);
            blood.OnAfterSpawn();

            Effects.SendMovingEffect(defender, blood, id, 7, 10, true, false, blood.Hue, 0);

            Timer.DelayCall(TimeSpan.FromMilliseconds(500), b => b.ItemID = id, blood);
        }

        public static bool CanDrawBlood(Mobile defender)
        {
            return defender.HasBlood;
        }

        public static Item CreateBlood(Mobile defender)
        {
            var b = new Blood
            {
                Hue = defender.BloodHue
            };
            if (defender is BaseCreature)
            {
                var bc = (BaseCreature)defender;
                if (bc.Tribe == TribeType.MortoVivo)
                {
                    b.Hue = 862;
                }
                else if (bc.Tribe == TribeType.Orc)
                {
                    b.Hue = 67;
                }
            }
            return b;
        }
        #endregion

        #region Elemental Damage
        public static int[] GetElementDamages(Mobile m)
        {
            var o = new[] { 100, 0, 0, 0, 0, 0, 0 };

            var w = m.Weapon as BaseWeapon ?? Fists;

            if (w != null)
            {
                w.GetDamageTypes(m, out o[0], out o[1], out o[2], out o[3], out o[4], out o[5], out o[6]);
            }

            return o;
        }

        public virtual void GetDamageTypes(
            Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
        {
            if (wielder is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)wielder;

                phys = bc.PhysicalDamage;
                fire = bc.FireDamage;
                cold = bc.ColdDamage;
                pois = bc.PoisonDamage;
                nrgy = bc.EnergyDamage;
                chaos = bc.ChaosDamage;
                direct = bc.DirectDamage;
            }
            else
            {
                fire = m_AosElementDamages.Fire;
                cold = m_AosElementDamages.Cold;
                pois = m_AosElementDamages.Poison;
                nrgy = m_AosElementDamages.Energy;
                chaos = m_AosElementDamages.Chaos;
                direct = m_AosElementDamages.Direct;

                phys = 100 - fire - cold - pois - nrgy - chaos - direct;

                CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

                if (resInfo != null)
                {
                    CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

                    if (attrInfo != null)
                    {
                        int left = phys;

                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponColdDamage, ref cold, left);
                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponEnergyDamage, ref nrgy, left);
                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponFireDamage, ref fire, left);
                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponPoisonDamage, ref pois, left);
                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponChaosDamage, ref chaos, left);
                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponDirectDamage, ref direct, left);

                        phys = left;
                    }
                }
            }
        }

        private int ApplyCraftAttributeElementDamage(int attrDamage, ref int element, int totalRemaining)
        {
            if (totalRemaining <= 0)
            {
                return 0;
            }

            if (attrDamage <= 0)
            {
                return totalRemaining;
            }

            int appliedDamage = attrDamage;

            if ((appliedDamage + element) > 100)
            {
                appliedDamage = 100 - element;
            }

            if (appliedDamage > totalRemaining)
            {
                appliedDamage = totalRemaining;
            }

            element += appliedDamage;

            return totalRemaining - appliedDamage;
        }
        #endregion

        public virtual void OnMiss(Mobile attacker, IDamageable damageable)
        {
            Mobile defender = damageable as Mobile;

            if (!Shard.COMBATE_SPHERE)
                PlaySwingAnimation(attacker);

            attacker.PlaySound(GetMissAttackSound(attacker, defender));

            if (defender != null)
                defender.PlaySound(GetMissDefendSound(attacker, defender));

            WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

            if (ability != null)
            {
                ability.OnMiss(attacker, defender);
            }

            SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            if (move != null)
            {
                move.OnMiss(attacker, defender);
            }

            if (defender is IHonorTarget && ((IHonorTarget)defender).ReceivedHonorContext != null)
            {
                ((IHonorTarget)defender).ReceivedHonorContext.OnTargetMissed(attacker);
            }

            SkillMasterySpell.OnMiss(attacker, defender);
        }

        public static bool SemVariar = false;

        public virtual void GetBaseDamageRange(Mobile attacker, out int min, out int max)
        {
            if (attacker is BaseCreature)
            {
                BaseCreature c = (BaseCreature)attacker;

                if (c.DamageMin >= 0)
                {
                    min = c.DamageMin;
                    max = c.DamageMax;
                    return;
                }

                if (this is Fists && !attacker.Body.IsHuman)
                {
                    min = attacker.Str / 28;
                    max = attacker.Str / 28;
                    return;
                }
            }

            if (attacker != null && this is Fists && TransformationSpellHelper.UnderTransformation(attacker, typeof(HorrificBeastSpell)))
            {
                min = 5;
                max = 15;
            }
            else
            {
                min = MinDamage;
                max = MaxDamage;
            }
            if (SemVariar)
                min = max;
        }

        public virtual double GetBaseDamage(Mobile attacker)
        {
            int min, max;

            GetBaseDamageRange(attacker, out min, out max);

            if (Shard.DebugEnabled)
                Shard.Debug("Weapon Min/Max: " + min + "/" + max);

            var armsLoreBonus = attacker.Skills[SkillName.ArmsLore].Value / 10;

            min += (int)Math.Round(armsLoreBonus);
            if (min > max)
                min = max;
            int damage = Utility.RandomMinMax(min, max);

            var bonusOre = 0;
            switch (Resource)
            {
                case CraftResource.Ferro:
                case CraftResource.Cedro:
                    bonusOre += 3;
                    break;
                case CraftResource.Carvalho:
                case CraftResource.Cobre:
                    bonusOre += 4;
                    break;
                case CraftResource.Pinho:
                case CraftResource.Bronze:
                    bonusOre += 4;
                    break;
                case CraftResource.Mogno:
                case CraftResource.Dourado:
                    bonusOre += 4;
                    break;
                case CraftResource.Niobio:
                case CraftResource.Quartzo:
                case CraftResource.Vibranium:
                    bonusOre += 4;
                    break;
                case CraftResource.Eucalipto:
                case CraftResource.Lazurita:
                    bonusOre += 5;
                    break;
                case CraftResource.Berilo:
                    bonusOre += 7;
                    break;
                case CraftResource.Carmesim:
                    if (this is BaseStaff)
                        bonusOre += 4;
                    else
                        bonusOre += 9;
                    break;
                case CraftResource.Gelo:
                    if (this is BaseStaff)
                        bonusOre += 9;
                    else
                        bonusOre += 4;
                    break;
                case CraftResource.Adamantium:
                    bonusOre += 9;
                    break;
            }

            if (m_DamageLevel != WeaponDamageLevel.Regular)
            {
                if (bonusOre <= 3)
                {
                    damage += (2 * (int)m_DamageLevel) - 1;
                }
                else // armas color, menor bonus de enchants
                {
                    damage += (int)m_DamageLevel - 1;
                }
            }
            else
            {
                damage += bonusOre;
            }

            return damage;
        }

        public virtual double GetBonus(double value, double scalar, double threshold, double offset)
        {
            double bonus = value * scalar;

            if (value >= threshold)
            {
                bonus += offset;
            }

            return bonus / 100;
        }

        public virtual int GetHitChanceBonus()
        {
            /*
            if (!Core.AOS)
            {
                return 0;
            }
            */

            int bonus = 0;

            switch (m_AccuracyLevel)
            {
                case WeaponAccuracyLevel.Leve:
                    bonus += 02;
                    break;
                case WeaponAccuracyLevel.Agil:
                    bonus += 04;
                    break;
                case WeaponAccuracyLevel.Veloz:
                    bonus += 06;
                    break;
                case WeaponAccuracyLevel.Exceedingly:
                    bonus += 08;
                    break;
                case WeaponAccuracyLevel.Supremely:
                    bonus += 10;
                    break;
            }

            return bonus;
        }

        public virtual int GetdDamageBonusSoPraMostrarProClient()
        {
            #region Stygian Abyss
            if (m_DImodded)
                return 0;
            #endregion

            int bonus = VirtualDamageBonus;

            if (!Core.AOS)
            {
                switch (m_Quality)
                {
                    case ItemQuality.Low:
                        bonus -= 5;
                        break;
                    case ItemQuality.Exceptional:
                    case ItemQuality.ObraPrima:
                        bonus += 5;
                        break;
                }


                switch (Resource)
                {
                    case CraftResource.Ferro:
                    case CraftResource.Cedro:
                        bonus += 3;
                        break;
                    case CraftResource.Carvalho:
                    case CraftResource.Cobre:
                        bonus += 5;
                        break;
                    case CraftResource.Pinho:
                    case CraftResource.Bronze:
                        bonus += 7;
                        break;
                    case CraftResource.Mogno:
                    case CraftResource.Dourado:
                        bonus += 8;
                        break;
                    case CraftResource.Niobio:
                        bonus += 9;
                        break;
                    case CraftResource.Lazurita:
                        bonus += 10;
                        break;
                    case CraftResource.Quartzo:
                        bonus += 11;
                        break;
                    case CraftResource.Vibranium:
                        bonus += 12;
                        break;
                    case CraftResource.Eucalipto:
                    case CraftResource.Berilo:
                        bonus += 16;
                        break;
                    case CraftResource.Carmesim:
                        bonus += 18;
                        break;
                    case CraftResource.Gelo:
                    case CraftResource.Adamantium:
                        bonus += 20;
                        break;
                }


                switch (m_DamageLevel)
                {
                    case WeaponDamageLevel.Ruin:
                        bonus += 5;
                        break;
                    case WeaponDamageLevel.Might:
                        bonus += 10;
                        break;
                    case WeaponDamageLevel.Force:
                        bonus += 15;
                        break;
                    case WeaponDamageLevel.Power:
                        bonus += 20;
                        break;
                    case WeaponDamageLevel.Vanq:
                        bonus += 25;
                        break;
                }
            }

            return bonus;
        }


        // Retorna os danos q mostra no status soh nao influencia no combate
        public virtual void GetStatusDamage(Mobile from, out int min, out int max)
        {
            int baseMin, baseMax;

            GetBaseDamageRange(from, out baseMin, out baseMax);

            //var
            var bonus = GetdDamageBonusSoPraMostrarProClient();
            var armslore = from.Skills[SkillName.ArmsLore].Value / 20;
            min = Math.Max((int)ScaleDamageOld(from, baseMin, false), 1) + bonus;
            max = Math.Max((int)ScaleDamageOld(from, baseMax, false), 1) + bonus;
            min += (int)armslore;
            if (min > max)
                min = max;

        }

        public virtual int VirtualDamageBonus { get { return 0; } }

        public virtual double ScaleDamageOld(Mobile attacker, double damage, bool checkSkills)
        {
            var anat = SkillName.Anatomy;
            if (checkSkills)
            {
                attacker.CheckSkillMult(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap);
                // Passively check tactics for gain
                if (anat == SkillName.Anatomy)
                    attacker.CheckSkillMult(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap);
                // Passively check Anatomy for gain

                if (Type == WeaponType.Axe)
                {
                    attacker.CheckSkillMult(SkillName.Lumberjacking, 0.0, 80); // Passively check Lumberjacking for gain
                }

                if (Type == WeaponType.Bashing && !attacker.RP)
                {
                    attacker.CheckSkillMult(SkillName.Mining, 0.0, 80);
                }
                //attacker.CheckSkillMult(SkillName.ArmsLore, 0.0, 100.0);
            }

            if (this is BaseRanged)
            {
                var quiver = attacker.FindItemOnLayer(Layer.Cloak) as BaseQuiver;
                if (quiver != null)
                    damage += damage * quiver.DamageIncrease / 100;
            }


            /* Compute tactics modifier
            * :   0.0 = 50% loss
            * :  50.0 = unchanged
            * : 100.0 = 50% bonus
            */
            var ttBonus = (attacker.Skills[SkillName.Tactics].Value - 50.0) / 100.0;
            damage += (damage * ttBonus);

            /* Compute strength modifier
            * : 1% bonus for every 5 strength
            */
            double modifiers = (attacker.Str / 5.0) / 100.0;

            /* Compute anatomy modifier
            * : 1% bonus for every 5 points of anatomy
            * : +10% bonus at Grandmaster or higher
            */
            double anatomyValue = attacker.Skills[anat].Value;
            modifiers += ((anatomyValue / 5.0) / 100.0);

            if (anatomyValue >= 100.0)
            {
                modifiers += 0.1;
            }

            /* Compute lumberjacking bonus
            * : 1% bonus for every 5 points of lumberjacking
            * : +10% bonus at Grandmaster or higher
            */

            if (Type == WeaponType.Axe)
            {
                double lumberValue = attacker.Skills[SkillName.Lumberjacking].Value;
                lumberValue = (lumberValue / 5.0) / 100.0;
                if (lumberValue > 0.2)
                    lumberValue = 0.2;

                modifiers += lumberValue;

                if (attacker.Skills[SkillName.Lumberjacking].Value >= 100.0)
                {
                     modifiers += 0.03;
                }
            }

            // New quality bonus:
            if (m_Quality != ItemQuality.Normal)
            {
                var q = (int)m_Quality;
                if (q > 2)
                    q = 2;
                modifiers += ((q - 1) * 0.1);
            }

            // Virtual damage bonus:
            if (VirtualDamageBonus != 0)
            {
                modifiers += (VirtualDamageBonus / 100.0);
            }

            if (Type == WeaponType.Bashing)
            {
                // Nerfzinho no cajado
                if (this is BaseStaff)
                {
                    if (Shard.DebugEnabled)
                        Shard.Debug("Nerfando cajado " + modifiers);
                    modifiers -= 0.2;
                }
                else
                {
                    double mine = attacker.Skills[SkillName.Mining].Value;
                    mine = (mine / 5.0) / 100.0;
                    if (mine > 0.2)
                        mine = 0.2;

                    modifiers += mine;
                    if (attacker.Skills[SkillName.Mining].Value >= 100.0)
                    {
                        modifiers += 0.02;
                    }
                }

            }

            if (Shard.DebugEnabled)
                Shard.Debug("Modifiers de dano " + modifiers * 100 + "%", attacker);



            // Apply bonuses
            damage += (damage * modifiers);

            if (m_Quality == ItemQuality.Low)
            {
                damage *= 0.98;
            }

            return ScaleDamageByDurability((int)damage);
        }

        public virtual int ScaleDamageByDurability(int damage)
        {
            int scale = 100;

            if (m_MaxHits > 0 && m_Hits < m_MaxHits)
            {
                scale = 50 + ((50 * m_Hits) / m_MaxHits);
            }

            return AOS.Scale(damage, scale);
        }

        public virtual int ComputeDamage(Mobile attacker, Mobile defender)
        {
            var baseDamage = GetBaseDamage(attacker);

            int damage = (int)ScaleDamageOld(attacker, baseDamage, true);
            if (Shard.DebugEnabled)
            {
                Shard.Debug("Base Weapon Damage: " + baseDamage, attacker);
                Shard.Debug("Scaled Weapon Damage: " + damage, attacker);
            }
            // pre-AOS, halve damage if the defender is a player or the attacker is not a player
            if (damage > 5 && defender is PlayerMobile || (!(attacker is PlayerMobile)))
            {
                if (Shard.DebugEnabled)
                    Shard.Debug("Half Damage on players", attacker);
                damage = (int)(damage / 2.0);
            }

            return damage;
        }

        public virtual void PlayHurtAnimation(Mobile from)
        {
            if (from.Mounted)
            {
                return;
            }

            if (Core.SA)
            {
                from.Animate(AnimationType.Impact, 0);
            }
            else
            {
                int action;
                int frames;

                switch (from.Body.Type)
                {
                    case BodyType.Sea:
                    case BodyType.Animal:
                        {
                            action = 7;
                            frames = 5;
                            break;
                        }
                    case BodyType.Monster:
                        {
                            action = 10;
                            frames = 4;
                            break;
                        }
                    case BodyType.Human:
                        {
                            action = 20;
                            frames = 5;
                            break;
                        }
                    default:
                        return;
                }

                from.Animate(action, frames, 1, true, false, 0);
            }
        }

        public virtual void PlaySwingAnimation(Mobile from, int delay = 0, int frameCount = 5)
        {
            int action;

            if(from.RP && from is PlayerMobile pl)
            {
                pl.PlayOmphSound();
            }

            if (!Shard.COMBATE_SPHERE)
            {
                action = GetNewAnimationAction(from);

                if (!from.Mounted)
                    from.Animate(AnimationType.Attack, action);
                else
                    from.Animate(action, frameCount, 1, true, false, 0);
            }
            else
            {
                if (from.Mounted)
                {
                    action = GetNewAnimationAction(from);
                    from.Animate(action, frameCount, 1, true, false, delay);
                    return;
                }
                switch (from.Body.Type)
                {
                    case BodyType.Sea:
                    case BodyType.Animal:
                        {
                            action = Utility.Random(5, 2);
                            break;
                        }
                    case BodyType.Monster:
                        {
                            switch (Animation)
                            {
                                default:
                                case WeaponAnimation.Wrestle:
                                case WeaponAnimation.Bash1H:
                                case WeaponAnimation.Pierce1H:
                                case WeaponAnimation.Slash1H:
                                case WeaponAnimation.Bash2H:
                                case WeaponAnimation.Pierce2H:
                                case WeaponAnimation.Slash2H:
                                    action = Utility.Random(4, 3);
                                    break;
                                case WeaponAnimation.ShootBow:
                                    return; // 7
                                case WeaponAnimation.ShootXBow:
                                    return; // 8
                            }

                            break;
                        }
                    case BodyType.Human:
                        {
                            if (!from.Mounted)
                            {
                                action = (int)Animation;
                            }
                            else
                            {
                                switch (Animation)
                                {
                                    default:
                                    case WeaponAnimation.Wrestle:
                                    case WeaponAnimation.Bash1H:
                                    case WeaponAnimation.Pierce1H:
                                    case WeaponAnimation.Slash1H:
                                        action = 26;
                                        break;
                                    case WeaponAnimation.Bash2H:
                                    case WeaponAnimation.Pierce2H:
                                    case WeaponAnimation.Slash2H:
                                        action = 29;
                                        break;
                                    case WeaponAnimation.ShootBow:
                                        action = 27;
                                        break;
                                    case WeaponAnimation.ShootXBow:
                                        action = 28;
                                        break;
                                }
                            }

                            break;
                        }
                    default:
                        return;
                }

                from.Animate(action, frameCount, 1, true, false, delay);
            }
        }

        public int GetNewAnimationAction(Mobile from)
        {
            if (from.Mounted)
            {
                switch (Animation)
                {
                    case WeaponAnimation.ShootBow:
                        return 27;
                    case WeaponAnimation.ShootXBow:
                        return 28;
                    case WeaponAnimation.Bash2H:
                    case WeaponAnimation.Pierce2H:
                    case WeaponAnimation.Slash2H:
                        return 29;
                    default:
                        return 26;

                }
            }
            else
            {
                switch (Animation)
                {
                    default:
                    case WeaponAnimation.Wrestle: return 0;
                    case WeaponAnimation.Bash1H: return 3;
                    case WeaponAnimation.Pierce1H: return 5;
                    case WeaponAnimation.Slash1H: return 4;
                    case WeaponAnimation.Bash2H: return 6;
                    case WeaponAnimation.Pierce2H: return 8;
                    case WeaponAnimation.Slash2H: return 7;
                    case WeaponAnimation.ShootBow: return 1;
                    case WeaponAnimation.ShootXBow: return 2;
                    case WeaponAnimation.Throwing: return 9;
                }
            }

        }

        #region Serialization/Deserialization
        private static void SetSaveFlag(ref SaveFlag flags, SaveFlag toSet, bool setIf)
        {
            if (setIf)
            {
                flags |= toSet;
            }
        }

        private static bool GetSaveFlag(SaveFlag flags, SaveFlag toGet)
        {
            return ((flags & toGet) != 0);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(18); // version

            // Version 18 - removed VvV Item (handled in VvV System) and BlockRepair (Handled as negative attribute)

            writer.Write(m_UsesRemaining);
            writer.Write(m_ShowUsesRemaining);

            writer.Write(_Owner);
            writer.Write(_OwnerName);

            // Version 15 converts old leech to new leech

            //Version 14
            writer.Write(m_IsImbued);

            //version 13, converted SaveFlags to long, added negative attributes

            //version 12
            #region Runic Reforging
            writer.Write((int)m_ReforgedPrefix);
            writer.Write((int)m_ReforgedSuffix);
            writer.Write((int)m_ItemPower);
            #endregion

            #region Stygian Abyss
            writer.Write(m_DImodded);
            writer.Write(m_SearingWeapon);

            // Version 11
            writer.Write(m_TimesImbued);

            #endregion

            // Version 10
            writer.Write(m_BlessedBy); // Bless Deed

            #region Veteran Rewards
            writer.Write(m_EngravedText);
            #endregion

            #region Mondain's Legacy
            writer.Write((int)m_Slayer3);
            #endregion

            #region Mondain's Legacy Sets
            SetFlag sflags = SetFlag.None;

            SetSaveFlag(ref sflags, SetFlag.Attributes, !m_SetAttributes.IsEmpty);
            SetSaveFlag(ref sflags, SetFlag.SkillBonuses, !m_SetSkillBonuses.IsEmpty);
            SetSaveFlag(ref sflags, SetFlag.Hue, m_SetHue != 0);
            SetSaveFlag(ref sflags, SetFlag.LastEquipped, m_LastEquipped);
            SetSaveFlag(ref sflags, SetFlag.SetEquipped, m_SetEquipped);
            SetSaveFlag(ref sflags, SetFlag.SetSelfRepair, m_SetSelfRepair != 0);
            SetSaveFlag(ref sflags, SetFlag.PhysicalBonus, m_SetPhysicalBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.FireBonus, m_SetFireBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.ColdBonus, m_SetColdBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.PoisonBonus, m_SetPoisonBonus != 0);
            SetSaveFlag(ref sflags, SetFlag.EnergyBonus, m_SetEnergyBonus != 0);

            writer.WriteEncodedInt((int)sflags);

            if (GetSaveFlag(sflags, SetFlag.PhysicalBonus))
            {
                writer.WriteEncodedInt((int)m_SetPhysicalBonus);
            }

            if (GetSaveFlag(sflags, SetFlag.FireBonus))
            {
                writer.WriteEncodedInt((int)m_SetFireBonus);
            }

            if (GetSaveFlag(sflags, SetFlag.ColdBonus))
            {
                writer.WriteEncodedInt((int)m_SetColdBonus);
            }

            if (GetSaveFlag(sflags, SetFlag.PoisonBonus))
            {
                writer.WriteEncodedInt((int)m_SetPoisonBonus);
            }

            if (GetSaveFlag(sflags, SetFlag.EnergyBonus))
            {
                writer.WriteEncodedInt((int)m_SetEnergyBonus);
            }

            if (GetSaveFlag(sflags, SetFlag.Attributes))
            {
                m_SetAttributes.Serialize(writer);
            }

            if (GetSaveFlag(sflags, SetFlag.SkillBonuses))
            {
                m_SetSkillBonuses.Serialize(writer);
            }

            if (GetSaveFlag(sflags, SetFlag.Hue))
            {
                writer.Write(m_SetHue);
            }

            if (GetSaveFlag(sflags, SetFlag.LastEquipped))
            {
                writer.Write(m_LastEquipped);
            }

            if (GetSaveFlag(sflags, SetFlag.SetEquipped))
            {
                writer.Write(m_SetEquipped);
            }

            if (GetSaveFlag(sflags, SetFlag.SetSelfRepair))
            {
                writer.WriteEncodedInt(m_SetSelfRepair);
            }
            #endregion

            // Version 9
            SaveFlag flags = SaveFlag.None;

            SetSaveFlag(ref flags, SaveFlag.DamageLevel, m_DamageLevel != WeaponDamageLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.AccuracyLevel, m_AccuracyLevel != WeaponAccuracyLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.DurabilityLevel, m_DurabilityLevel != WeaponDurabilityLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.Quality, m_Quality != ItemQuality.Normal);
            SetSaveFlag(ref flags, SaveFlag.Hits, m_Hits != 0);
            SetSaveFlag(ref flags, SaveFlag.MaxHits, m_MaxHits != 0);
            SetSaveFlag(ref flags, SaveFlag.Slayer, m_Slayer != SlayerName.None);
            SetSaveFlag(ref flags, SaveFlag.Poison, m_Poison != null);
            SetSaveFlag(ref flags, SaveFlag.PoisonCharges, m_PoisonCharges != 0);
            SetSaveFlag(ref flags, SaveFlag.Crafter, m_Crafter != null);
            SetSaveFlag(ref flags, SaveFlag.Identified, m_Identified);
            SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);
            SetSaveFlag(ref flags, SaveFlag.DexReq, m_DexReq != -1);
            SetSaveFlag(ref flags, SaveFlag.IntReq, m_IntReq != -1);
            SetSaveFlag(ref flags, SaveFlag.MinDamage, m_MinDamage != -1);
            SetSaveFlag(ref flags, SaveFlag.MaxDamage, m_MaxDamage != -1);
            SetSaveFlag(ref flags, SaveFlag.HitSound, m_HitSound != -1);
            SetSaveFlag(ref flags, SaveFlag.MissSound, m_MissSound != -1);
            SetSaveFlag(ref flags, SaveFlag.Speed, m_Speed != -1);
            SetSaveFlag(ref flags, SaveFlag.MaxRange, m_MaxRange != -1);
            SetSaveFlag(ref flags, SaveFlag.Skill, m_Skill != (SkillName)(-1));
            SetSaveFlag(ref flags, SaveFlag.Type, m_Type != (WeaponType)(-1));
            SetSaveFlag(ref flags, SaveFlag.Animation, m_Animation != (WeaponAnimation)(-1));
            SetSaveFlag(ref flags, SaveFlag.Resource, m_Resource != CraftResource.Ferro);
            SetSaveFlag(ref flags, SaveFlag.xAttributes, !m_AosAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.xWeaponAttributes, !m_AosWeaponAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, m_PlayerConstructed);
            SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.Slayer2, m_Slayer2 != SlayerName.None);
            SetSaveFlag(ref flags, SaveFlag.ElementalDamages, !m_AosElementDamages.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.EngravedText, !String.IsNullOrEmpty(m_EngravedText));
            SetSaveFlag(ref flags, SaveFlag.xAbsorptionAttributes, !m_SAAbsorptionAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.xNegativeAttributes, !m_NegativeAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.Altered, m_Altered);
            SetSaveFlag(ref flags, SaveFlag.xExtendedWeaponAttributes, !m_ExtendedWeaponAttributes.IsEmpty);

            writer.Write((long)flags);

            if (GetSaveFlag(flags, SaveFlag.DamageLevel))
            {
                writer.Write((int)m_DamageLevel);
            }

            if (GetSaveFlag(flags, SaveFlag.AccuracyLevel))
            {
                writer.Write((int)m_AccuracyLevel);
            }

            if (GetSaveFlag(flags, SaveFlag.DurabilityLevel))
            {
                writer.Write((int)m_DurabilityLevel);
            }

            if (GetSaveFlag(flags, SaveFlag.Quality))
            {
                writer.Write((int)m_Quality);
            }

            if (GetSaveFlag(flags, SaveFlag.Hits))
            {
                writer.Write(m_Hits);
            }

            if (GetSaveFlag(flags, SaveFlag.MaxHits))
            {
                writer.Write(m_MaxHits);
            }

            if (GetSaveFlag(flags, SaveFlag.Slayer))
            {
                writer.Write((int)m_Slayer);
            }

            if (GetSaveFlag(flags, SaveFlag.Poison))
            {
                Poison.Serialize(m_Poison, writer);
            }

            if (GetSaveFlag(flags, SaveFlag.PoisonCharges))
            {
                writer.Write(m_PoisonCharges);
            }

            if (GetSaveFlag(flags, SaveFlag.Crafter))
            {
                writer.Write(m_Crafter);
            }

            if (GetSaveFlag(flags, SaveFlag.StrReq))
            {
                writer.Write(m_StrReq);
            }

            if (GetSaveFlag(flags, SaveFlag.DexReq))
            {
                writer.Write(m_DexReq);
            }

            if (GetSaveFlag(flags, SaveFlag.IntReq))
            {
                writer.Write(m_IntReq);
            }

            if (GetSaveFlag(flags, SaveFlag.MinDamage))
            {
                writer.Write(m_MinDamage);
            }

            if (GetSaveFlag(flags, SaveFlag.MaxDamage))
            {
                writer.Write(m_MaxDamage);
            }

            if (GetSaveFlag(flags, SaveFlag.HitSound))
            {
                writer.Write(m_HitSound);
            }

            if (GetSaveFlag(flags, SaveFlag.MissSound))
            {
                writer.Write(m_MissSound);
            }

            if (GetSaveFlag(flags, SaveFlag.Speed))
            {
                writer.Write(m_Speed);
            }

            if (GetSaveFlag(flags, SaveFlag.MaxRange))
            {
                writer.Write(m_MaxRange);
            }

            if (GetSaveFlag(flags, SaveFlag.Skill))
            {
                writer.Write((int)m_Skill);
            }

            if (GetSaveFlag(flags, SaveFlag.Type))
            {
                writer.Write((int)m_Type);
            }

            if (GetSaveFlag(flags, SaveFlag.Animation))
            {
                writer.Write((int)m_Animation);
            }

            if (GetSaveFlag(flags, SaveFlag.Resource))
            {
                writer.Write((int)m_Resource);
            }

            if (GetSaveFlag(flags, SaveFlag.xAttributes))
            {
                m_AosAttributes.Serialize(writer);
            }

            if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
            {
                m_AosWeaponAttributes.Serialize(writer);
            }

            if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
            {
                m_AosSkillBonuses.Serialize(writer);
            }

            if (GetSaveFlag(flags, SaveFlag.Slayer2))
            {
                writer.Write((int)m_Slayer2);
            }

            if (GetSaveFlag(flags, SaveFlag.ElementalDamages))
            {
                m_AosElementDamages.Serialize(writer);
            }

            if (GetSaveFlag(flags, SaveFlag.EngravedText))
            {
                writer.Write(m_EngravedText);
            }

            #region SA
            if (GetSaveFlag(flags, SaveFlag.xAbsorptionAttributes))
            {
                m_SAAbsorptionAttributes.Serialize(writer);
            }

            if (GetSaveFlag(flags, SaveFlag.xNegativeAttributes))
            {
                m_NegativeAttributes.Serialize(writer);
            }
            #endregion

            if (GetSaveFlag(flags, SaveFlag.xExtendedWeaponAttributes))
            {
                m_ExtendedWeaponAttributes.Serialize(writer);
            }
        }

        [Flags]
        private enum SaveFlag : long
        {
            None = 0x00000000,
            DamageLevel = 0x00000001,
            AccuracyLevel = 0x00000002,
            DurabilityLevel = 0x00000004,
            Quality = 0x00000008,
            Hits = 0x00000010,
            MaxHits = 0x00000020,
            Slayer = 0x00000040,
            Poison = 0x00000080,
            PoisonCharges = 0x00000100,
            Crafter = 0x00000200,
            Identified = 0x00000400,
            StrReq = 0x00000800,
            DexReq = 0x00001000,
            IntReq = 0x00002000,
            MinDamage = 0x00004000,
            MaxDamage = 0x00008000,
            HitSound = 0x00010000,
            MissSound = 0x00020000,
            Speed = 0x00040000,
            MaxRange = 0x00080000,
            Skill = 0x00100000,
            Type = 0x00200000,
            Animation = 0x00400000,
            Resource = 0x00800000,
            xAttributes = 0x01000000,
            xWeaponAttributes = 0x02000000,
            PlayerConstructed = 0x04000000,
            SkillBonuses = 0x08000000,
            Slayer2 = 0x10000000,
            ElementalDamages = 0x20000000,
            EngravedText = 0x40000000,
            xAbsorptionAttributes = 0x80000000,
            xNegativeAttributes = 0x100000000,
            Altered = 0x200000000,
            xExtendedWeaponAttributes = 0x400000000
        }

        #region Mondain's Legacy Sets
        private static void SetSaveFlag(ref SetFlag flags, SetFlag toSet, bool setIf)
        {
            if (setIf)
            {
                flags |= toSet;
            }
        }

        private static bool GetSaveFlag(SetFlag flags, SetFlag toGet)
        {
            return ((flags & toGet) != 0);
        }

        [Flags]
        private enum SetFlag
        {
            None = 0x00000000,
            Attributes = 0x00000001,
            WeaponAttributes = 0x00000002,
            SkillBonuses = 0x00000004,
            Hue = 0x00000008,
            LastEquipped = 0x00000010,
            SetEquipped = 0x00000020,
            SetSelfRepair = 0x00000040,
            PhysicalBonus = 0x00000080,
            FireBonus = 0x00000100,
            ColdBonus = 0x00000200,
            PoisonBonus = 0x00000400,
            EnergyBonus = 0x00000800,
        }
        #endregion

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 18:
                case 17:
                    {
                        m_UsesRemaining = reader.ReadInt();
                        m_ShowUsesRemaining = reader.ReadBool();
                        goto case 16;
                    }
                case 16:
                    {
                        if (version == 17)
                            reader.ReadBool();

                        _Owner = reader.ReadMobile();
                        _OwnerName = reader.ReadString();
                        goto case 15;
                    }
                case 15:
                case 14:
                    {
                        m_IsImbued = reader.ReadBool();
                        goto case 13;
                    }
                case 13:
                case 12:
                    {
                        #region Runic Reforging
                        m_ReforgedPrefix = (ReforgedPrefix)reader.ReadInt();
                        m_ReforgedSuffix = (ReforgedSuffix)reader.ReadInt();
                        m_ItemPower = (ElementoPvM)reader.ReadInt();

                        if (version < 18 && reader.ReadBool())
                        {
                            Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
                            {
                                m_NegativeAttributes.NoRepair = 1;
                            });
                        }
                        #endregion

                        #region Stygian Abyss
                        m_DImodded = reader.ReadBool();
                        m_SearingWeapon = reader.ReadBool();
                        goto case 11;
                    }
                case 11:
                    {
                        m_TimesImbued = reader.ReadInt();

                        #endregion

                        goto case 10;
                    }
                case 10:
                    {
                        m_BlessedBy = reader.ReadMobile();
                        m_EngravedText = reader.ReadString();
                        m_Slayer3 = (TalismanSlayerName)reader.ReadInt();

                        SetFlag flags = (SetFlag)reader.ReadEncodedInt();
                        if (GetSaveFlag(flags, SetFlag.PhysicalBonus))
                        {
                            m_SetPhysicalBonus = reader.ReadEncodedInt();
                        }

                        if (GetSaveFlag(flags, SetFlag.FireBonus))
                        {
                            m_SetFireBonus = reader.ReadEncodedInt();
                        }

                        if (GetSaveFlag(flags, SetFlag.ColdBonus))
                        {
                            m_SetColdBonus = reader.ReadEncodedInt();
                        }

                        if (GetSaveFlag(flags, SetFlag.PoisonBonus))
                        {
                            m_SetPoisonBonus = reader.ReadEncodedInt();
                        }

                        if (GetSaveFlag(flags, SetFlag.EnergyBonus))
                        {
                            m_SetEnergyBonus = reader.ReadEncodedInt();
                        }

                        if (GetSaveFlag(flags, SetFlag.Attributes))
                        {
                            m_SetAttributes = new AosAttributes(this, reader);
                        }
                        else
                        {
                            m_SetAttributes = new AosAttributes(this);
                        }

                        if (GetSaveFlag(flags, SetFlag.WeaponAttributes))
                        {
                            m_SetSelfRepair = (new AosWeaponAttributes(this, reader)).SelfRepair;
                        }

                        if (GetSaveFlag(flags, SetFlag.SkillBonuses))
                        {
                            m_SetSkillBonuses = new AosSkillBonuses(this, reader);
                        }
                        else
                        {
                            m_SetSkillBonuses = new AosSkillBonuses(this);
                        }

                        if (GetSaveFlag(flags, SetFlag.Hue))
                        {
                            m_SetHue = reader.ReadInt();
                        }

                        if (GetSaveFlag(flags, SetFlag.LastEquipped))
                        {
                            m_LastEquipped = reader.ReadBool();
                        }

                        if (GetSaveFlag(flags, SetFlag.SetEquipped))
                        {
                            m_SetEquipped = reader.ReadBool();
                        }

                        if (GetSaveFlag(flags, SetFlag.SetSelfRepair))
                        {
                            m_SetSelfRepair = reader.ReadEncodedInt();
                        }

                        goto case 5;
                    }
                case 9:
                case 8:
                case 7:
                case 6:
                case 5:
                    {
                        SaveFlag flags;

                        if (version < 13)
                            flags = (SaveFlag)reader.ReadInt();
                        else
                            flags = (SaveFlag)reader.ReadLong();

                        if (GetSaveFlag(flags, SaveFlag.DamageLevel))
                        {
                            m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();

                            if (m_DamageLevel > WeaponDamageLevel.Vanq)
                            {
                                m_DamageLevel = WeaponDamageLevel.Ruin;
                            }
                        }

                        if (GetSaveFlag(flags, SaveFlag.AccuracyLevel))
                        {
                            m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();

                            if (m_AccuracyLevel > WeaponAccuracyLevel.Supremely)
                            {
                                m_AccuracyLevel = WeaponAccuracyLevel.Leve;
                            }
                        }

                        if (GetSaveFlag(flags, SaveFlag.DurabilityLevel))
                        {
                            m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();

                            if (m_DurabilityLevel > WeaponDurabilityLevel.Indestructible)
                            {
                                m_DurabilityLevel = WeaponDurabilityLevel.Durable;
                            }
                        }

                        if (GetSaveFlag(flags, SaveFlag.Quality))
                        {
                            m_Quality = (ItemQuality)reader.ReadInt();
                        }
                        else
                        {
                            m_Quality = ItemQuality.Normal;
                        }

                        if (GetSaveFlag(flags, SaveFlag.Hits))
                        {
                            m_Hits = reader.ReadInt();
                        }

                        if (GetSaveFlag(flags, SaveFlag.MaxHits))
                        {
                            m_MaxHits = reader.ReadInt();
                        }

                        if (GetSaveFlag(flags, SaveFlag.Slayer))
                        {
                            m_Slayer = (SlayerName)reader.ReadInt();
                        }

                        if (GetSaveFlag(flags, SaveFlag.Poison))
                        {
                            m_Poison = Poison.Deserialize(reader);
                        }

                        if (GetSaveFlag(flags, SaveFlag.PoisonCharges))
                        {
                            m_PoisonCharges = reader.ReadInt();
                        }

                        if (GetSaveFlag(flags, SaveFlag.Crafter))
                        {
                            m_Crafter = reader.ReadMobile();
                        }

                        if (GetSaveFlag(flags, SaveFlag.Identified))
                        {
                            m_Identified = (version >= 6 || reader.ReadBool());
                        }

                        if (GetSaveFlag(flags, SaveFlag.StrReq))
                        {
                            m_StrReq = reader.ReadInt();
                        }
                        else
                        {
                            m_StrReq = -1;
                        }

                        if (GetSaveFlag(flags, SaveFlag.DexReq))
                        {
                            m_DexReq = reader.ReadInt();
                        }
                        else
                        {
                            m_DexReq = -1;
                        }

                        if (GetSaveFlag(flags, SaveFlag.IntReq))
                        {
                            m_IntReq = reader.ReadInt();
                        }
                        else
                        {
                            m_IntReq = -1;
                        }

                        if (GetSaveFlag(flags, SaveFlag.MinDamage))
                        {
                            m_MinDamage = reader.ReadInt();
                        }
                        else
                        {
                            m_MinDamage = -1;
                        }

                        if (GetSaveFlag(flags, SaveFlag.MaxDamage))
                        {
                            m_MaxDamage = reader.ReadInt();
                        }
                        else
                        {
                            m_MaxDamage = -1;
                        }

                        if (GetSaveFlag(flags, SaveFlag.HitSound))
                        {
                            m_HitSound = reader.ReadInt();
                        }
                        else
                        {
                            m_HitSound = -1;
                        }

                        if (GetSaveFlag(flags, SaveFlag.MissSound))
                        {
                            m_MissSound = reader.ReadInt();
                        }
                        else
                        {
                            m_MissSound = -1;
                        }

                        if (GetSaveFlag(flags, SaveFlag.Speed))
                        {
                            if (version < 9)
                            {
                                m_Speed = reader.ReadInt();
                            }
                            else
                            {
                                m_Speed = reader.ReadFloat();
                            }
                        }
                        else
                        {
                            m_Speed = -1;
                        }

                        if (GetSaveFlag(flags, SaveFlag.MaxRange))
                        {
                            m_MaxRange = reader.ReadInt();
                        }
                        else
                        {
                            m_MaxRange = -1;
                        }

                        if (GetSaveFlag(flags, SaveFlag.Skill))
                        {
                            m_Skill = (SkillName)reader.ReadInt();
                        }
                        else
                        {
                            m_Skill = (SkillName)(-1);
                        }

                        if (GetSaveFlag(flags, SaveFlag.Type))
                        {
                            m_Type = (WeaponType)reader.ReadInt();
                        }
                        else
                        {
                            m_Type = (WeaponType)(-1);
                        }

                        if (GetSaveFlag(flags, SaveFlag.Animation))
                        {
                            m_Animation = (WeaponAnimation)reader.ReadInt();
                        }
                        else
                        {
                            m_Animation = (WeaponAnimation)(-1);
                        }

                        if (GetSaveFlag(flags, SaveFlag.Resource))
                        {
                            m_Resource = (CraftResource)reader.ReadInt();
                        }
                        else
                        {
                            m_Resource = CraftResource.Ferro;
                        }

                        if (GetSaveFlag(flags, SaveFlag.xAttributes))
                        {
                            m_AosAttributes = new AosAttributes(this, reader);
                        }
                        else
                        {
                            m_AosAttributes = new AosAttributes(this);
                        }

                        if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
                        {
                            m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
                        }
                        else
                        {
                            m_AosWeaponAttributes = new AosWeaponAttributes(this);
                        }

                        if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile)
                        {
                            var mult = 5;
                            if (Resource != CraftResource.Ferro && Resource != CraftResource.Cedro) // bonus menor em items color
                                mult = 2;
                            m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * mult);
                            ((Mobile)Parent).AddSkillMod(m_SkillMod);
                        }

                        if (version < 7 && m_AosWeaponAttributes.MageWeapon != 0)
                        {
                            m_AosWeaponAttributes.MageWeapon = 30 - m_AosWeaponAttributes.MageWeapon;
                        }

                        if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30 &&
                            Parent is Mobile)
                        {
                            m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
                            ((Mobile)Parent).AddSkillMod(m_MageMod);
                        }

                        if (GetSaveFlag(flags, SaveFlag.PlayerConstructed))
                        {
                            m_PlayerConstructed = true;
                        }

                        if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                        {
                            m_AosSkillBonuses = new AosSkillBonuses(this, reader);
                        }
                        else
                        {
                            m_AosSkillBonuses = new AosSkillBonuses(this);
                        }

                        if (GetSaveFlag(flags, SaveFlag.Slayer2))
                        {
                            m_Slayer2 = (SlayerName)reader.ReadInt();
                        }

                        if (GetSaveFlag(flags, SaveFlag.ElementalDamages))
                        {
                            m_AosElementDamages = new AosElementAttributes(this, reader);
                        }
                        else
                        {
                            m_AosElementDamages = new AosElementAttributes(this);
                        }

                        if (GetSaveFlag(flags, SaveFlag.EngravedText))
                        {
                            m_EngravedText = reader.ReadString();
                        }

                        #region Stygian Abyss
                        if (version > 9 && GetSaveFlag(flags, SaveFlag.xAbsorptionAttributes))
                        {
                            m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this, reader);
                        }
                        else
                        {
                            m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this);
                        }

                        if (version >= 13 && GetSaveFlag(flags, SaveFlag.xNegativeAttributes))
                        {
                            m_NegativeAttributes = new NegativeAttributes(this, reader);
                        }
                        else
                        {
                            m_NegativeAttributes = new NegativeAttributes(this);
                        }
                        #endregion

                        if (GetSaveFlag(flags, SaveFlag.Altered))
                        {
                            m_Altered = true;
                        }

                        if (GetSaveFlag(flags, SaveFlag.xExtendedWeaponAttributes))
                        {
                            m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this, reader);
                        }
                        else
                        {
                            m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this);
                        }

                        if (Core.TOL && m_ExtendedWeaponAttributes.MysticWeapon != 0 && m_ExtendedWeaponAttributes.MysticWeapon != 30 && Parent is Mobile)
                        {
                            m_MysticMod = new DefaultSkillMod(SkillName.Mysticism, true, -30 + m_ExtendedWeaponAttributes.MysticWeapon);
                            ((Mobile)Parent).AddSkillMod(m_MysticMod);
                        }

                        break;
                    }
                case 4:
                    {
                        m_Slayer = (SlayerName)reader.ReadInt();

                        goto case 3;
                    }
                case 3:
                    {
                        m_StrReq = reader.ReadInt();
                        m_DexReq = reader.ReadInt();
                        m_IntReq = reader.ReadInt();

                        goto case 2;
                    }
                case 2:
                    {
                        m_Identified = reader.ReadBool();

                        goto case 1;
                    }
                case 1:
                    {
                        m_MaxRange = reader.ReadInt();

                        goto case 0;
                    }
                case 0:
                    {
                        if (version == 0)
                        {
                            m_MaxRange = 1; // default
                        }

                        if (version < 5)
                        {
                            m_Resource = CraftResource.Ferro;
                            m_AosAttributes = new AosAttributes(this);
                            m_AosWeaponAttributes = new AosWeaponAttributes(this);
                            m_AosElementDamages = new AosElementAttributes(this);
                            m_AosSkillBonuses = new AosSkillBonuses(this);
                        }

                        m_MinDamage = reader.ReadInt();
                        m_MaxDamage = reader.ReadInt();

                        m_Speed = reader.ReadInt();

                        m_HitSound = reader.ReadInt();
                        m_MissSound = reader.ReadInt();

                        m_Skill = (SkillName)reader.ReadInt();
                        m_Type = (WeaponType)reader.ReadInt();
                        m_Animation = (WeaponAnimation)reader.ReadInt();
                        m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();
                        m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();
                        m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();
                        m_Quality = (ItemQuality)reader.ReadInt();

                        m_Crafter = reader.ReadMobile();

                        m_Poison = Poison.Deserialize(reader);
                        m_PoisonCharges = reader.ReadInt();

                        if (m_StrReq == OldStrengthReq)
                        {
                            m_StrReq = -1;
                        }

                        if (m_DexReq == OldDexterityReq)
                        {
                            m_DexReq = -1;
                        }

                        if (m_IntReq == OldIntelligenceReq)
                        {
                            m_IntReq = -1;
                        }

                        if (m_MinDamage == OldMinDamage)
                        {
                            m_MinDamage = -1;
                        }

                        if (m_MaxDamage == OldMaxDamage)
                        {
                            m_MaxDamage = -1;
                        }

                        if (m_HitSound == OldHitSound)
                        {
                            m_HitSound = -1;
                        }

                        if (m_MissSound == OldMissSound)
                        {
                            m_MissSound = -1;
                        }

                        if (m_Speed == OldSpeed)
                        {
                            m_Speed = -1;
                        }

                        if (m_MaxRange == OldMaxRange)
                        {
                            m_MaxRange = -1;
                        }

                        if (m_Skill == OldSkill)
                        {
                            m_Skill = (SkillName)(-1);
                        }

                        if (m_Type == OldType)
                        {
                            m_Type = (WeaponType)(-1);
                        }

                        if (m_Animation == OldAnimation)
                        {
                            m_Animation = (WeaponAnimation)(-1);
                        }

                        if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile)
                        {
                            m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                            ((Mobile)Parent).AddSkillMod(m_SkillMod);
                        }

                        break;
                    }
            }

            if (version < 15)
            {
                if (WeaponAttributes.HitLeechHits > 0 || WeaponAttributes.HitLeechMana > 0)
                {
                    WeaponAttributes.ScaleLeech(Attributes.WeaponSpeed);
                }
            }

            #region Mondain's Legacy Sets
            if (m_SetAttributes == null)
            {
                m_SetAttributes = new AosAttributes(this);
            }

            if (m_SetSkillBonuses == null)
            {
                m_SetSkillBonuses = new AosSkillBonuses(this);
            }
            #endregion

            if (Parent is Mobile)
            {
                m_AosSkillBonuses.AddTo((Mobile)Parent);
            }

            int strBonus = m_AosAttributes.BonusStr;
            int dexBonus = m_AosAttributes.BonusDex;
            int intBonus = m_AosAttributes.BonusInt;

            if (Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
            {
                Mobile m = (Mobile)Parent;

                string modName = Serial.ToString();

                if (strBonus != 0)
                {
                    m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));
                }

                if (dexBonus != 0)
                {
                    m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));
                }

                if (intBonus != 0)
                {
                    m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
                }
            }

            if (Parent is Mobile)
            {
                ((Mobile)Parent).CheckStatTimers();
            }

            if (m_Hits <= 0 && m_MaxHits <= 0)
            {
                m_Hits = m_MaxHits = Utility.RandomMinMax(InitMinHits, InitMaxHits);
            }

            if (version < 6)
            {
                m_PlayerConstructed = true; // we don't know, so, assume it's crafted
            }

            if (m_Slayer == SlayerName.Demonios || m_Slayer == SlayerName.Balrons)
                m_Slayer = SlayerName.Exorcismo;

            if (m_Slayer2 == SlayerName.Demonios || m_Slayer2 == SlayerName.Balrons)
                m_Slayer2 = SlayerName.Exorcismo;
        }
        #endregion

        public BaseWeapon(int itemID)
            : base(itemID)
        {
            Layer = (Layer)ItemData.Quality;

            m_Quality = ItemQuality.Normal;
            m_StrReq = -1;
            m_DexReq = -1;
            m_IntReq = -1;
            m_MinDamage = -1;
            m_MaxDamage = -1;
            m_HitSound = -1;
            m_MissSound = -1;
            m_Speed = -1;
            m_MaxRange = -1;
            m_Skill = (SkillName)(-1);
            m_Type = (WeaponType)(-1);
            m_Animation = (WeaponAnimation)(-1);

            m_Hits = m_MaxHits = Utility.RandomMinMax(InitMinHits, InitMaxHits);

            m_Resource = CraftResource.Ferro;

            m_AosAttributes = new AosAttributes(this);
            m_AosWeaponAttributes = new AosWeaponAttributes(this);
            m_AosSkillBonuses = new AosSkillBonuses(this);
            m_AosElementDamages = new AosElementAttributes(this);
            m_NegativeAttributes = new NegativeAttributes(this);
            m_ExtendedWeaponAttributes = new ExtendedWeaponAttributes(this);

            #region Stygian Abyss
            m_SAAbsorptionAttributes = new SAAbsorptionAttributes(this);
            #endregion

            #region Mondain's Legacy Sets
            m_SetAttributes = new AosAttributes(this);
            m_SetSkillBonuses = new AosSkillBonuses(this);
            #endregion

            m_AosSkillBonuses = new AosSkillBonuses(this);

            if (this is ITool)
            {
                m_UsesRemaining = Utility.RandomMinMax(25, 75);
            }
            else
            {
                m_UsesRemaining = 150;
            }
        }

        public BaseWeapon(Serial serial)
            : base(serial)
        { }

        private string GetNameString()
        {
            string name = Name;

            if (name == null)
            {
                name = String.Format("#{0}", LabelNumber);
            }

            return name;
        }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get { return base.Hue; }
            set
            {
                base.Hue = value;
                InvalidateProperties();
            }
        }

        public int GetElementalDamageHue()
        {
            int phys, fire, cold, pois, nrgy, chaos, direct;
            GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);
            //Order is Cold, Energy, Fire, Poison, Physical left

            int currentMax = 50;
            int hue = 0;

            if (pois >= currentMax)
            {
                hue = 1267 + (pois - 50) / 10;
                currentMax = pois;
            }

            if (fire >= currentMax)
            {
                hue = 1255 + (fire - 50) / 10;
                currentMax = fire;
            }

            if (nrgy >= currentMax)
            {
                hue = 1273 + (nrgy - 50) / 10;
                currentMax = nrgy;
            }

            if (cold >= currentMax)
            {
                hue = 1261 + (cold - 50) / 10;
                currentMax = cold;
            }

            return hue;
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (m_ExtendedWeaponAttributes.AssassinHoned > 0)
            {
                list.Add(1152207); // Assassin's Edge
                return;
            }

            int oreType;

            switch (m_Resource)
            {
                case CraftResource.Berilo:
                    oreType = 1053108;
                    break; // dull copper
                case CraftResource.Vibranium:
                    oreType = 1053107;
                    break; // shadow iron
                case CraftResource.Cobre:
                    oreType = 1053106;
                    break; // copper
                case CraftResource.Bronze:
                    oreType = 1053105;
                    break; // bronze
                case CraftResource.Dourado:
                    oreType = 1053104;
                    break; // golden
                case CraftResource.Niobio:
                    oreType = 1053103;
                    break; // agapite
                case CraftResource.Lazurita:
                    oreType = 1053102;
                    break; // verite
                case CraftResource.Quartzo:
                    oreType = 1053101;
                    break; // valorite
                case CraftResource.CouroSpinned:
                    oreType = 1061118;
                    break; // spined
                case CraftResource.CouroHorned:
                    oreType = 1061117;
                    break; // horned
                case CraftResource.CouroBarbed:
                    oreType = 1061116;
                    break; // barbed
                case CraftResource.EscamasVermelhas:
                    oreType = 1060814;
                    break; // red
                case CraftResource.EscamasAmarelas:
                    oreType = 1060818;
                    break; // yellow
                case CraftResource.EscamasNegras:
                    oreType = 1060820;
                    break; // black
                case CraftResource.EscamasVerdes:
                    oreType = 1060819;
                    break; // green
                case CraftResource.EscamasBrancas:
                    oreType = 1060821;
                    break; // white
                case CraftResource.EscamasAzuis:
                    oreType = 1060815;
                    break; // blue

                #region Mondain's Legacy
                case CraftResource.Carvalho:
                    oreType = 1072533;
                    break; // oak
                case CraftResource.Pinho:
                    oreType = 1072534;
                    break; // ash
                case CraftResource.Mogno:
                    oreType = 1072535;
                    break; // yew
                case CraftResource.Eucalipto:
                    oreType = 1072536;
                    break; // heartwood
                case CraftResource.Carmesim:
                    oreType = 1072538;
                    break; // bloodwood
                case CraftResource.Gelo:
                    oreType = 1072539;
                    break; // frostwood
                #endregion

                default:
                    oreType = 0;
                    break;
            }

            if (m_ReforgedPrefix != ReforgedPrefix.None || m_ReforgedSuffix != ReforgedSuffix.None)
            {
                if (m_ReforgedPrefix != ReforgedPrefix.None)
                {
                    int prefix = RunicReforging.GetPrefixName(m_ReforgedPrefix);

                    if (m_ReforgedSuffix == ReforgedSuffix.None)
                        list.Add(1151757, String.Format("#{0}\t{1}", prefix, GetNameString())); // ~1_PREFIX~ ~2_ITEM~
                    else
                        list.Add(1151756, String.Format("#{0}\t{1}\t#{2}", prefix, GetNameString(), RunicReforging.GetSuffixName(m_ReforgedSuffix))); // ~1_PREFIX~ ~2_ITEM~ of ~3_SUFFIX~
                }
                else if (m_ReforgedSuffix != ReforgedSuffix.None)
                {
                    RunicReforging.AddSuffixName(list, m_ReforgedSuffix, GetNameString());
                }
            }
            #region High Seas
            else if (m_SearingWeapon)
            {
                list.Add(1151318, String.Format("#{0}", LabelNumber));
            }
            #endregion
            var nome = Name;
            if (Name != null)
            {
                if (this.Quality == ItemQuality.ObraPrima)
                    nome += " obra prima";
                if (this.Quality == ItemQuality.Exceptional)
                    nome += " excepcional";
                if (this.Quality == ItemQuality.Low)
                    nome += " de baixa qualidade";
                if (m_Resource != CraftResource.Ferro && m_Resource != CraftResource.RegularLeather && m_Resource != CraftResource.Cedro)
                    nome += " de " + m_Resource;
            }

            if (Name == null)
            {
                list.Add(LabelNumber);
            }
            else
            {
                list.Add(nome);
            }

            /*
            * Want to move this to the engraving tool, let the non-harmful
            * formatting show, and remove CLILOCs embedded: more like OSI
            * did with the books that had markup, etc.
            *
            * This will have a negative effect on a few event things imgame
            * as is.
            *
            * If we cant find a more OSI-ish way to clean it up, we can
            * easily put this back, and use it in the deserialize
            * method and engraving tool, to make it perm cleaned up.
            */

            if (!String.IsNullOrEmpty(m_EngravedText))
            {
                list.AddTwoValues(Gump.Cor("Escrito", "orange"), Gump.Cor(Utility.FixHtml(m_EngravedText), "orange"));
            }
        }

        public override bool AllowEquipedCast(Mobile from)
        {
            if (base.AllowEquipedCast(from))
            {
                return true;
            }

            return m_AosAttributes.ResistMagica > 0 || Enhancement.GetValue(from, AosAttribute.SpellChanneling) > 0;
        }

        public virtual int ArtifactRarity { get { return 0; } }

        public override bool DisplayWeight
        {
            get
            {
                if (IsVvVItem)
                    return true;

                return base.DisplayWeight;
            }
        }

        public virtual int GetLuckBonus()
        {
            #region Mondain's Legacy
            if (m_Resource == CraftResource.Eucalipto)
            {
                return 0;
            }
            #endregion

            CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

            if (resInfo == null)
            {
                return 0;
            }

            CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

            if (attrInfo == null)
            {
                return 0;
            }

            return attrInfo.WeaponLuck;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            if (IsVvVItem)
                list.Add(1154937); // VvV Item
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (!this.Identified)
            {
                if (m_Slayer != SlayerName.None || m_Slayer2 != SlayerName.None ||
                     m_DurabilityLevel != WeaponDurabilityLevel.Regular || m_DamageLevel != WeaponDamageLevel.Regular ||
                     m_AccuracyLevel != WeaponAccuracyLevel.Regular)
                {
                    list.Add("[ Nao Identificado ]");
                }
            }
            else
            {
                if (DurabilityLevel > 0)
                {
                    list.Add("Durabilidade " + "+" + (int)DurabilityLevel * 20);
                }
                if (DamageLevel > 0)
                {
                    list.AddTwoValues("Dano", "+" + (int)DamageLevel * 2);
                }
                if (AccuracyLevel > 0)
                {
                    list.AddTwoValues("Skill", "+" + (int)AccuracyLevel * 5);
                }
            }

            /*
            var b = GetdDamageBonusSoPraMostrarProClient();
            list.AddTwoValues("Dano", (OldMinDamage + b) + " - " + (OldMaxDamage + b));
            list.AddTwoValues("Velocidade ", Speed.ToString());
            */

            if (m_MaxHits > 0)
            {
                double durabPct = m_Hits / (double)m_MaxHits;
                if (durabPct < 0.2)
                {
                    list.Add("Pessimo estado");
                }
                else if (durabPct < 0.5)
                {
                    list.Add("Mau estado");
                }
                else if (durabPct < 0.75)
                {
                    list.Add("Bom estado");
                }
                else if (durabPct < 0.99)
                {
                    list.Add("Quase novo");
                }
                else if (durabPct >= 1)
                {
                    list.Add("Novo");
                }
            }

            if (this is IUsesRemaining && ((IUsesRemaining)this).ShowUsesRemaining)
            {
                list.Add("Usos restantes: " + ((IUsesRemaining)this).UsesRemaining.ToString()); // uses remaining: ~1_val~
            }

            if (m_Crafter != null)
            {
                list.Add("Criado por " + m_Crafter.TitleName); // crafted by ~1_NAME~
            }

            if (RP)
                list.AddTwoValues("Item", "RP");

            #region Factions
            FactionEquipment.AddFactionProperties(this, list);
            #endregion

            #region Mondain's Legacy Sets
            if (IsSetItem)
            {
                list.Add(1073491, Pieces.ToString()); // Part of a Weapon/Armor Set (~1_val~ pieces)

                if (SetID == SetItem.Bestial)
                    list.Add(1151541, BestialSetHelper.GetTotalBerserk(this).ToString()); // Berserk ~1_VAL~

                if (BardMasteryBonus)
                    list.Add(1151553); // Activate: Bard Mastery Bonus x2<br>(Effect: 1 min. Cooldown: 30 min.)

                if (m_SetEquipped)
                {
                    list.Add(1073492); // Full Weapon/Armor Set Present
                    GetSetProperties(list);
                }
            }
            #endregion

            if (m_NegativeAttributes.Brittle == 0 && m_AosAttributes.Brittle != 0)
            {
                list.Add(1116209); // Brittle
            }

            if (m_NegativeAttributes != null)
                m_NegativeAttributes.GetProperties(list, this);

            if (m_AosSkillBonuses != null)
            {
                //m_AosSkillBonuses.GetProperties(list);
            }

            if (RequiredRace == Race.Elf)
            {
                list.Add(1075086); // Elves Only
            }

            #region Stygian Abyss
            else if (RequiredRace == Race.Gargoyle)
            {
                list.Add(1111709); // Gargoyles Only
            }
            #endregion

            if (ArtifactRarity > 0)
            {
                list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~
            }

            if (m_Poison != null && m_PoisonCharges > 0 && CanShowPoisonCharges())
            {
                #region Mondain's Legacy mod
                list.AddTwoValues("Cargas de Veneno", m_PoisonCharges.ToString());
                #endregion
            }

            if (m_Slayer != SlayerName.None)
            {
                SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
                if (entry != null)
                {
                    //list.Add(entry.Title);
                    list.AddTwoValues("Matador de", entry.Name.ToString());
                }
            }

            if (m_Slayer2 != SlayerName.None)
            {
                SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
                if (entry != null)
                {
                    //list.Add(entry.Title);
                    list.AddTwoValues("Matador de", entry.Name.ToString());
                }
            }

            #region Mondain's Legacy
            if (m_Slayer3 != TalismanSlayerName.None)
            {
                if (m_Slayer3 == TalismanSlayerName.Wolf)
                {
                    list.Add(1075462);
                }
                else if (m_Slayer3 == TalismanSlayerName.Goblin)
                {
                    list.Add(1095010);
                }
                else if (m_Slayer3 == TalismanSlayerName.Undead)
                {
                    list.Add(1060479);
                }
                else
                {
                    list.Add(1072503 + (int)m_Slayer3);
                }
            }
            #endregion

            if (HasSocket<Caddellite>())
            {
                list.Add(1158662); // Caddellite Infused
            }

            double focusBonus = 1;
            int enchantBonus = 0;
            bool fcMalus = false;
            int damBonus = 0;
            SpecialMove move = null;
            AosWeaponAttribute bonus = AosWeaponAttribute.HitColdArea;

            #region Focus Attack
            if (FocusWeilder != null)
            {
                move = SpecialMove.GetCurrentMove(FocusWeilder);

                if (move is FocusAttack)
                {
                    focusBonus = move.GetPropertyBonus(FocusWeilder);
                    damBonus = (int)(move.GetDamageScalar(FocusWeilder, null) * 100) - 100;
                }
            }
            #endregion

            #region Stygian Abyss
            if (EnchantedWeilder != null)
            {
                if (Server.Spells.Mysticism.EnchantSpell.IsUnderSpellEffects(EnchantedWeilder, this))
                {
                    bonus = Server.Spells.Mysticism.EnchantSpell.BonusAttribute(EnchantedWeilder);
                    enchantBonus = Server.Spells.Mysticism.EnchantSpell.BonusValue(EnchantedWeilder);
                    fcMalus = Server.Spells.Mysticism.EnchantSpell.CastingMalus(EnchantedWeilder, this);
                }
            }
            #endregion

            int prop;
            double fprop;

            int strReq = AOS.Scale(StrRequirement, 100 - GetLowerStatReq());
            var props = "";

            if (MaxRange > 1)
            {
                list.Add("Alcance " + MaxRange.ToString()); // range ~1_val~
            }

            XmlAttach.AddAttachmentProperties(this, list);

            // T2A
            if (m_ExtendedWeaponAttributes.Bane > 0)
            {
                list.Add(1154671); // Bane
            }

            if (m_ExtendedWeaponAttributes.BoneBreaker > 0)
            {
                list.Add(1157318); // Bone Breaker
            }

            if ((prop = m_ExtendedWeaponAttributes.HitSwarm) != 0)
            {
                list.Add(1157325, prop.ToString()); // Swarm ~1_val~%
            }

            if ((prop = m_ExtendedWeaponAttributes.HitSparks) != 0)
            {
                list.Add(1157326, prop.ToString()); // Sparks ~1_val~%
            }

            if ((prop = m_ExtendedWeaponAttributes.AssassinHoned) != 0)
            {
                list.Add(1152206); // Assassin Honed
            }

            if ((prop = m_AosWeaponAttributes.SplinteringWeapon) != 0)
            {
                list.Add(1112857, prop.ToString()); //splintering weapon ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitDispel * focusBonus) != 0)
            {
                list.Add(1060417, ((int)fprop).ToString()); // hit dispel ~1_val~%
            }
            else if (bonus == AosWeaponAttribute.HitDispel && enchantBonus != 0)
            {
                list.Add(1060417, ((int)(enchantBonus * focusBonus)).ToString()); // hit dispel ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitFireball * focusBonus) != 0)
            {
                list.Add(1060420, ((int)fprop).ToString()); // hit fireball ~1_val~%
            }
            else if (bonus == AosWeaponAttribute.HitFireball && enchantBonus != 0)
            {
                list.Add(1060420, ((int)((double)enchantBonus * focusBonus)).ToString()); // hit fireball ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitLightning * focusBonus) != 0)
            {
                list.Add(1060423, ((int)fprop).ToString()); // hit lightning ~1_val~%
            }
            else if (bonus == AosWeaponAttribute.HitLightning && enchantBonus != 0)
            {
                list.Add(1060423, ((int)(enchantBonus * focusBonus)).ToString()); // hit lightning ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitHarm * focusBonus) != 0)
            {
                list.Add(1060421, ((int)fprop).ToString()); // hit harm ~1_val~%
            }
            else if (bonus == AosWeaponAttribute.HitHarm && enchantBonus != 0)
            {
                list.Add(1060421, ((int)(enchantBonus * focusBonus)).ToString()); // hit harm ~1_val~%
            }

            if (m_SearingWeapon)
            {
                list.Add(1151183); // Searing Weapon
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitMagicArrow * focusBonus) != 0)
            {
                list.Add(1060426, ((int)fprop).ToString()); // hit magic arrow ~1_val~%
            }
            else if (bonus == AosWeaponAttribute.HitMagicArrow && enchantBonus != 0)
            {
                list.Add(1060426, ((int)(enchantBonus * focusBonus)).ToString()); // hit magic arrow ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitPhysicalArea * focusBonus) != 0)
            {
                list.Add(1060428, ((int)fprop).ToString()); // hit physical area ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitFireArea * focusBonus) != 0)
            {
                list.Add(1060419, ((int)fprop).ToString()); // hit fire area ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitColdArea * focusBonus) != 0)
            {
                list.Add(1060416, ((int)fprop).ToString()); // hit cold area ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitPoisonArea * focusBonus) != 0)
            {
                list.Add(1060429, ((int)fprop).ToString()); // hit poison area ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitEnergyArea * focusBonus) != 0)
            {
                list.Add(1060418, ((int)fprop).ToString()); // hit energy area ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitLeechStam * focusBonus) != 0)
            {
                list.Add(1060430, Math.Min(100, (int)fprop).ToString()); // hit stamina leech ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitLeechMana * focusBonus) != 0)
            {
                list.Add(1060427, Math.Min(100, (int)fprop).ToString()); // hit mana leech ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitLeechHits * focusBonus) != 0)
            {
                list.Add(1060422, Math.Min(100, (int)fprop).ToString()); // hit life leech ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitFatigue * focusBonus) != 0)
            {
                list.Add(1113700, ((int)fprop).ToString()); // Hit Fatigue ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitManaDrain * focusBonus) != 0)
            {
                list.Add(1113699, ((int)fprop).ToString()); // Hit Mana Drain ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitCurse * focusBonus) != 0)
            {
                list.Add(1113712, ((int)fprop).ToString()); // Hit Curse ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitLowerAttack * focusBonus) != 0)
            {
                list.Add(1060424, ((int)fprop).ToString()); // hit lower attack ~1_val~%
            }

            if ((fprop = (double)m_AosWeaponAttributes.HitLowerDefend * focusBonus) != 0)
            {
                list.Add(1060425, ((int)fprop).ToString()); // hit lower defense ~1_val~%
            }

            if ((prop = m_AosWeaponAttributes.BloodDrinker) != 0)
            {
                list.Add(1113591, prop.ToString()); // Blood Drinker
            }

            if ((prop = m_AosWeaponAttributes.BattleLust) != 0)
            {
                list.Add(1113710, prop.ToString()); // Battle Lust
            }

            if (ImmolatingWeaponSpell.IsImmolating(RootParent as Mobile, this))
            {
                list.Add(1111917); // Immolated
            }

            if (Core.ML && this is BaseRanged && (prop = ((BaseRanged)this).Velocity) != 0)
            {
                list.Add(1072793, prop.ToString()); // Velocity ~1_val~%
            }

            if ((prop = m_AosAttributes.LowerAmmoCost) != 0)
            {
                list.Add(1075208, prop.ToString()); // Lower Ammo Cost ~1_Percentage~%
            }

            if ((prop = m_ExtendedWeaponAttributes.MysticWeapon) != 0)
            {
                list.Add(1155881, (30 - prop).ToString());   // mystic weapon -~1_val~ skill
            }
            else if ((prop = Parent is Mobile ? Enhancement.GetValue((Mobile)Parent, ExtendedWeaponAttribute.MysticWeapon) : 0) != 0)
            {
                list.Add(1155881, (30 - prop).ToString());   // mystic weapon -~1_val~ skill
            }

            if ((prop = m_AosWeaponAttributes.SelfRepair) != 0)
            {
                list.Add(1060450, prop.ToString()); // self repair ~1_val~
            }

            if ((prop = m_AosAttributes.NightSight) != 0)
            {
                list.Add(1060441); // night sight
            }

            BaseArmor.AddPropsPvM(list, Attributes);

            return;


            if ((prop = fcMalus ? 1 : m_AosAttributes.ResistMagica) != 0)
            {
                list.Add(1060482); // spell channeling
            }

            if ((prop = m_AosWeaponAttributes.MageWeapon) != 0)
            {
                list.Add(1060438, (30 - prop).ToString()); // mage weapon -~1_val~ skill
            }

            if (Core.ML && m_AosAttributes.WeaponSkillDamage > 0 && Layer == Layer.TwoHanded)
            {
                list.Add(1072792); // Balanced
            }

            if ((prop = (GetLuckBonus() + m_AosAttributes.Luck)) != 0)
            {
                list.Add(1060436, prop.ToString()); // luck ~1_val~
            }

            if ((prop = m_AosAttributes.EnhancePotions) != 0)
            {
                list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%
            }

            if ((prop = m_AosWeaponAttributes.ReactiveParalyze) != 0)
            {
                list.Add(1112364); // reactive paralyze
            }

            if ((prop = m_AosAttributes.BonusStr) != 0)
            {
                list.Add(1060485, prop.ToString()); // strength bonus ~1_val~
            }

            if ((prop = m_AosAttributes.BonusInt) != 0)
            {
                list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~
            }

            if ((prop = m_AosAttributes.BonusDex) != 0)
            {
                list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~
            }

            if ((prop = m_AosAttributes.BonusHits) != 0)
            {
                list.Add(1060431, prop.ToString()); // hit point increase ~1_val~
            }

            if ((prop = m_AosAttributes.BonusStam) != 0)
            {
                list.Add(1060484, prop.ToString()); // stamina increase ~1_val~
            }

            if ((prop = m_AosAttributes.BonusMana) != 0)
            {
                list.Add(1060439, prop.ToString()); // mana increase ~1_val~
            }

            if ((prop = m_AosAttributes.RegenHits) != 0)
            {
                list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~
            }

            if ((prop = m_AosAttributes.RegenStam) != 0)
            {
                list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~
            }

            if ((prop = m_AosAttributes.RegenMana) != 0)
            {
                list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~
            }

            if ((prop = m_AosAttributes.ReflectPhysical) != 0)
            {
                list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%
            }

            if ((prop = m_AosAttributes.CastRecovery) != 0)
            {
                list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~
            }

            if ((prop = fcMalus ? m_AosAttributes.Resistence - 1 : m_AosAttributes.Resistence) != 0)
            {
                list.Add(1060413, prop.ToString()); // faster casting ~1_val~
            }

            if ((prop = (GetHitChanceBonus() + m_AosAttributes.AttackChance)) != 0)
            {
                list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%
            }

            if ((prop = m_AosAttributes.LowerRegCost) != 0)
            {
                list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%
            }

            if (Core.ML && (prop = m_AosAttributes.IncreasedKarmaLoss) != 0)
            {
                list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%
            }

            #region Stygian Abyss
            if ((prop = m_SAAbsorptionAttributes.CastingFocus) != 0)
            {
                list.Add(1113696, prop.ToString()); // Casting Focus ~1_val~%
            }

            if ((prop = m_SAAbsorptionAttributes.EaterFire) != 0)
            {
                list.Add(1113593, prop.ToString()); // Fire Eater ~1_Val~%
            }

            if ((prop = m_SAAbsorptionAttributes.EaterCold) != 0)
            {
                list.Add(1113594, prop.ToString()); // Cold Eater ~1_Val~%
            }

            if ((prop = m_SAAbsorptionAttributes.EaterPoison) != 0)
            {
                list.Add(1113595, prop.ToString()); // Poison Eater ~1_Val~%
            }

            if ((prop = m_SAAbsorptionAttributes.EaterEnergy) != 0)
            {
                list.Add(1113596, prop.ToString()); // Energy Eater ~1_Val~%
            }

            if ((prop = m_SAAbsorptionAttributes.EaterKinetic) != 0)
            {
                list.Add(1113597, prop.ToString()); // Kinetic Eater ~1_Val~%
            }

            if ((prop = m_SAAbsorptionAttributes.EaterDamage) != 0)
            {
                list.Add(1113598, prop.ToString()); // Damage Eater ~1_Val~%
            }

            if ((prop = m_SAAbsorptionAttributes.ResonanceFire) != 0)
            {
                list.Add(1113691, prop.ToString()); // Fire Resonance ~1_val~%
            }

            if ((prop = m_SAAbsorptionAttributes.ResonanceCold) != 0)
            {
                list.Add(1113692, prop.ToString()); // Cold Resonance ~1_val~%
            }

            if ((prop = m_SAAbsorptionAttributes.ResonancePoison) != 0)
            {
                list.Add(1113693, prop.ToString()); // Poison Resonance ~1_val~%
            }

            if ((prop = m_SAAbsorptionAttributes.ResonanceEnergy) != 0)
            {
                list.Add(1113694, prop.ToString()); // Energy Resonance ~1_val~%
            }

            if ((prop = m_SAAbsorptionAttributes.ResonanceKinetic) != 0)
            {
                list.Add(1113695, prop.ToString()); // Kinetic Resonance ~1_val~%
            }
            #endregion

            base.AddResistanceProperties(list);

            if ((prop = GetLowerStatReq()) != 0)
            {
                list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%
            }

            if ((prop = m_AosWeaponAttributes.UseBestSkill) != 0)
            {
                list.Add(1060400); // use best weapon skill
            }

            int phys, fire, cold, pois, nrgy, chaos, direct;

            GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

            #region Mondain's Legacy
            if (chaos != 0)
            {
                list.Add(1072846, chaos.ToString()); // chaos damage ~1_val~%
            }

            if (direct != 0)
            {
                list.Add(1079978, direct.ToString()); // Direct Damage: ~1_PERCENT~%
            }
            #endregion

            /*
			if (phys != 0)
			{
				list.Add(1060403, phys.ToString()); // physical damage ~1_val~%
			}

			if (fire != 0)
			{
				list.Add(1060405, fire.ToString()); // fire damage ~1_val~%
			}

			if (cold != 0)
			{
				list.Add(1060404, cold.ToString()); // cold damage ~1_val~%
			}

			if (pois != 0)
			{
				list.Add(1060406, pois.ToString()); // poison damage ~1_val~%
			}

			if (nrgy != 0)
			{
				list.Add(1060407, nrgy.ToString()); // energy damage ~1_val
			}

			if (Core.ML && chaos != 0)
			{
				list.Add(1072846, chaos.ToString()); // chaos damage ~1_val~%
			}

			if (Core.ML && direct != 0)
			{
				list.Add(1079978, direct.ToString()); // Direct Damage: ~1_PERCENT~%
            }
            */

            if (IsSetItem && !m_SetEquipped)
            {
                list.Add(1072378); // <br>Only when full set is present:
                GetSetProperties(list);
            }
        }

        public override void AddItemPowerProperties(ObjectPropertyList list)
        {
            if (m_ItemPower != ElementoPvM.None)
            {
                list.AddTwoValues("Elemento", Gump.Cor(m_ItemPower.ToString(), BaseArmor.CorElemento(m_ItemPower)));
            }
        }

        public bool CanShowPoisonCharges()
        {
            return true;
            /*
            if (PrimaryAbility == WeaponAbility.InfectiousStrike || SecondaryAbility == WeaponAbility.InfectiousStrike)
                return true;

            return RootParent is Mobile && SkillMasterySpell.HasSpell((Mobile)RootParent, typeof(InjectedStrikeSpell));
            */
        }

        public override void OnSingleClick(Mobile from)
        {
            var attrs = new List<EquipInfoAttribute>();

            if (DisplayLootType)
            {
                if (LootType == LootType.Blessed)
                {
                    attrs.Add(new EquipInfoAttribute(1038021)); // blessed
                }
                else if (LootType == LootType.Cursed)
                {
                    attrs.Add(new EquipInfoAttribute(1049643)); // cursed
                }
            }

            #region Factions
            if (m_FactionState != null)
            {
                attrs.Add(new EquipInfoAttribute(1041350)); // faction item
            }
            #endregion

            if (m_Quality == ItemQuality.Exceptional)
            {
                attrs.Add(new EquipInfoAttribute(1018305 - (int)m_Quality));
            }

            if (m_Identified || from.AccessLevel >= AccessLevel.GameMaster)
            {
                if (m_Slayer != SlayerName.None)
                {
                    SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
                    if (entry != null)
                    {
                        attrs.Add(new EquipInfoAttribute(entry.Title));
                    }
                }

                if (m_Slayer2 != SlayerName.None)
                {
                    SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
                    if (entry != null)
                    {
                        attrs.Add(new EquipInfoAttribute(entry.Title));
                    }
                }

                if (m_DurabilityLevel != WeaponDurabilityLevel.Regular)
                {
                    attrs.Add(new EquipInfoAttribute(1038000 + (int)m_DurabilityLevel));
                }

                if (m_DamageLevel != WeaponDamageLevel.Regular)
                {
                    attrs.Add(new EquipInfoAttribute(1038015 + (int)m_DamageLevel));
                }

                if (m_AccuracyLevel != WeaponAccuracyLevel.Regular)
                {
                    attrs.Add(new EquipInfoAttribute(1038010 + (int)m_AccuracyLevel));
                }
            }
            else if (m_Slayer != SlayerName.None || m_Slayer2 != SlayerName.None ||
                     m_DurabilityLevel != WeaponDurabilityLevel.Regular || m_DamageLevel != WeaponDamageLevel.Regular ||
                     m_AccuracyLevel != WeaponAccuracyLevel.Regular)
            {
                attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified
            }

            if (m_Poison != null && m_PoisonCharges > 0)
            {
                attrs.Add(new EquipInfoAttribute(1017383, m_PoisonCharges));
            }

            int number;

            if (Name == null)
            {
                number = LabelNumber;
            }
            else
            {
                LabelTo(from, Name);
                number = 1041000;
            }

            if (attrs.Count == 0 && Crafter == null && Name != null)
            {
                return;
            }

            EquipmentInfo eqInfo = new EquipmentInfo(number, m_Crafter, false, attrs.ToArray());

            from.Send(new DisplayEquipmentInfo(this, eqInfo));
        }

        public override bool DropToWorld(Mobile from, Point3D p)
        {
            bool drop = base.DropToWorld(from, p);

            EnchantedHotItemSocket.CheckDrop(from, this);

            return drop;
        }

        public static BaseWeapon Fists { get; set; }

        #region ICraftable Members
        public int OnCraft(
            int quality,
            bool makersMark,
            Mobile from,
            CraftSystem craftSystem,
            Type typeRes,
            ITool tool,
            CraftItem craftItem,
            int resHue)
        {
          

            if (makersMark)
            {
                Crafter = from;
            }

            Quality = (ItemQuality)quality;

            PlayerConstructed = true;

            if (typeRes == null)
            {
                typeRes = craftItem.Resources.GetAt(0).ItemType;
            }

            CraftResource thisResource = CraftResources.GetFromType(typeRes);

            var power = 0;

            double chancefoda = 0;
            if (Core.AOS)
            {
                if (!craftItem.ForceNonExceptional)
                {
                    Resource = CraftResources.GetFromType(typeRes);
                }

                CraftContext context = craftSystem.GetContext(from);

                if (Quality == ItemQuality.Exceptional)
                {
                    Attributes.WeaponDamage += 35;
                }

                if (!craftItem.ForceNonExceptional)
                {
                    if (tool is BaseRunicTool)
                    {
                        ((BaseRunicTool)tool).ApplyAttributesTo(this);
                    }
                }

                if (Core.ML && Quality == ItemQuality.Exceptional)
                {
                    double div = Siege.SiegeShard ? 12.5 : 20;

                    Attributes.WeaponDamage += (int)(from.Skills.ArmsLore.Value / div);
                    from.CheckSkillMult(SkillName.ArmsLore, 0, 100);
                }
            }
            else if (tool is BaseRunicTool)
            {
                if (craftItem != null && !craftItem.ForceNonExceptional)
                {
                    if (thisResource == ((BaseRunicTool)tool).Resource)
                    {
                        chancefoda += 100;
                        power = 5;
                    }
                }
            }

            if (tool is IResource) // so pra cajados por hora
            {
                var resTool = ((IResource)tool).Resource;
                if (resTool == thisResource)
                {
                    chancefoda += 5;
                    switch (this.Resource)
                    {
                        case CraftResource.Cobre:
                        case CraftResource.Bronze:
                        case CraftResource.Carvalho:
                            power += 4; break;
                        case CraftResource.Niobio:
                        case CraftResource.Dourado:
                        case CraftResource.Vibranium:
                        case CraftResource.Pinho:
                            power += 5; break;
                        case CraftResource.Lazurita:
                        case CraftResource.Eucalipto:
                        case CraftResource.Quartzo:
                            power += 6; break;
                        case CraftResource.Mogno:
                        case CraftResource.Berilo:
                            power += 7; break;
                        case CraftResource.Adamantium:
                        case CraftResource.Gelo:
                            power += 10; break;
                    }
                }
            }

            string toolname = tool.Name;
            if (toolname != null && toolname.Contains("Mistica") == true)
            {
                chancefoda += 100;
            }

            var props = 0;

            if (Shard.DebugEnabled)
                Shard.Debug("Chance craft obra prima: " + chancefoda);

            if (power < 5)
                power = 5; //verificar porque não está puxando o power quando é do mesmo material - paliativo.

            if (chancefoda == 0 && this is BaseStaff && this.Quality == ItemQuality.Exceptional)
            {
                chancefoda += 5;

                if (from.RP && !from.TemTalento(Talento.FabricadorDeCajados))
                {
                    power = 1;
                }
            }

            if (from.RP)
            {
                if (this.Quality == ItemQuality.Exceptional)
                {
                    if (from.TemTalento(Talento.ArmasEpicas))
                    {
                        chancefoda += 5;
                    }

                    if (this is BasePoleArm)
                    {
                        if (from.TemTalento(Talento.FabricadorDeArmasDeAste))
                        {
                            chancefoda += 1;
                        }
                        else
                        {
                            chancefoda /= 2;
                            power /= 2;
                        }
                    }

                    if (this is BaseStaff)
                    {
                        if (from.TemTalento(Talento.FabricadorDeCajados))
                        {
                            chancefoda += 1;
                        }
                        else
                        {
                            chancefoda /= 2;
                            power /= 2;
                        }
                    }

                    if (this is BaseSword)
                    {
                        if (from.TemTalento(Talento.FabricadorDeEspadas))
                        {
                            chancefoda += 1;
                        }
                        else
                        {
                            chancefoda /= 2;
                            power /= 2;
                        }
                    }

                    if (this is BaseBashing)
                    {
                        if (from.TemTalento(Talento.FabricadorDePorretes))
                        {
                            chancefoda += 1;
                        }
                        else
                        {
                            chancefoda /= 2;
                            power /= 2;
                        }
                    }

                    if (this is BaseAxe)
                    {
                        if (from.TemTalento(Talento.FabricadorDeMachados))
                        {
                            chancefoda += 1;
                        }
                        else
                        {
                            chancefoda /= 2;
                            power /= 2;
                        }
                    }

                    if (this is BaseSpear)
                    {
                        if (from.TemTalento(Talento.FabricadorDeLancas))
                        {
                            chancefoda += 1;
                        }
                        else
                        {
                            chancefoda /= 2;
                            power /= 2;
                        }
                    }
                }
            }

            if (chancefoda > 0 && Utility.Random(100) <= chancefoda)
            {
                if (power < 1)
                    power = 1;

                from.SendMessage(55, "Voce craftou uma obra prima. Use ferramentas color da mesma cor do minerio do item para aumentar suas chances de craftar obras primas e tambem craftar obras primas melhores !");
                Quality = ItemQuality.ObraPrima;
                quality = (int)quality;

                from.FixedParticles(0x376A, 10, 30, 5052, TintaBranca.COR, 1, EffectLayer.LeftFoot);
                Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
                Effects.PlaySound(from.Location, from.Map, 0x243);
                from.OverheadMessage("* obra prima *");
                this.Identified = true;
                var propChance = 0.55;
                if (this is BaseStaff)
                {
                    Attributes.SpellDamage = power;
                    if (Utility.RandomDouble() < propChance)
                        Attributes.LowerManaCost = power + Utility.Random(power * 4);
                    if (Utility.RandomDouble() < propChance)
                        Attributes.SpellDamage = power + Utility.Random(power * 4);
                    if (Utility.RandomDouble() < 0.8)
                        Slayer = BaseRunicTool.GetRandomSlayer();
                }
                else
                {
                    Attributes.WeaponDamage = power + Utility.Random(power * 3);
                    // if (Utility.RandomDouble() < 0.35)
                    Attributes.WeaponSpeed = power + Utility.Random(power * 3);
                    if (Utility.RandomDouble() < 0.5)
                        Slayer = BaseRunicTool.GetRandomSlayer();
                }
                this.HueRaridade = 1161;
                if(tool != null)
                {
                    tool.UsesRemaining -= 30;
                    if(tool != null)
                    {
                        if (tool.UsesRemaining <= 0 || (toolname != null && toolname.Contains("Mistica") == true))
                        {
                            tool.Delete();
                            from.SendMessage("Sua ferramenta quebrou");
                        }
                    }
                   
                }
            }

            if (craftItem != null && !craftItem.ForceNonExceptional)
            {
                CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);
                Resource = thisResource;

                if (resInfo == null)
                {
                    return quality;
                }

                CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

                if (attrInfo == null)
                {
                    return quality;
                }

                //DistributeMaterialBonus(attrInfo);
            }

            return quality;
        }



        public virtual void DistributeMaterialBonus(CraftAttributeInfo attrInfo)
        {
            if (m_Resource != CraftResource.Eucalipto)
            {
                m_AosAttributes.WeaponDamage += attrInfo.WeaponDamage;
                m_AosAttributes.WeaponSpeed += attrInfo.WeaponSwingSpeed;
                m_AosAttributes.AttackChance += attrInfo.WeaponHitChance;
                m_AosAttributes.RegenHits += attrInfo.WeaponRegenHits;
                m_AosWeaponAttributes.HitLeechHits += attrInfo.WeaponHitLifeLeech;
            }
            else
            {
                switch (Utility.Random(6))
                {
                    case 0: m_AosAttributes.WeaponDamage += attrInfo.WeaponDamage; break;
                    case 1: m_AosAttributes.WeaponSpeed += attrInfo.WeaponSwingSpeed; break;
                    case 2: m_AosAttributes.AttackChance += attrInfo.WeaponHitChance; break;
                    case 3: m_AosAttributes.Luck += attrInfo.WeaponLuck; break;
                    case 4: m_AosWeaponAttributes.LowerStatReq += attrInfo.WeaponLowerRequirements; break;
                    case 5: m_AosWeaponAttributes.HitLeechHits += attrInfo.WeaponHitLifeLeech; break;
                }
            }
        }

        #region Mondain's Legacy Sets
        public override bool OnDragLift(Mobile from)
        {
            if (Parent is Mobile && from == Parent)
            {
                if (IsSetItem && m_SetEquipped)
                {
                    SetHelper.RemoveSetBonus(from, SetID, this);
                }
            }

            return base.OnDragLift(from);
        }

        public virtual SetItem SetID { get { return SetItem.None; } }
        public virtual int Pieces { get { return 0; } }

        public virtual bool BardMasteryBonus
        {
            get
            {
                return (SetID == SetItem.Virtuoso);
            }
        }

        public bool IsSetItem { get { return SetID != SetItem.None; } }

        private int m_SetHue;
        private bool m_SetEquipped;
        private bool m_LastEquipped;

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetHue
        {
            get { return m_SetHue; }
            set
            {
                m_SetHue = value;
                InvalidateProperties();
            }
        }

        public bool SetEquipped { get { return m_SetEquipped; } set { m_SetEquipped = value; } }

        public bool LastEquipped { get { return m_LastEquipped; } set { m_LastEquipped = value; } }

        private AosAttributes m_SetAttributes;
        private AosSkillBonuses m_SetSkillBonuses;
        private int m_SetSelfRepair;
        private int m_SetPhysicalBonus, m_SetFireBonus, m_SetColdBonus, m_SetPoisonBonus, m_SetEnergyBonus;

        [CommandProperty(AccessLevel.GameMaster)]
        public AosAttributes SetAttributes { get { return m_SetAttributes; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosSkillBonuses SetSkillBonuses { get { return m_SetSkillBonuses; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetSelfRepair
        {
            get { return m_SetSelfRepair; }
            set
            {
                m_SetSelfRepair = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetPhysicalBonus
        {
            get
            {
                return m_SetPhysicalBonus;
            }
            set
            {
                m_SetPhysicalBonus = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetFireBonus
        {
            get
            {
                return m_SetFireBonus;
            }
            set
            {
                m_SetFireBonus = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetColdBonus
        {
            get
            {
                return m_SetColdBonus;
            }
            set
            {
                m_SetColdBonus = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetPoisonBonus
        {
            get
            {
                return m_SetPoisonBonus;
            }
            set
            {
                m_SetPoisonBonus = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SetEnergyBonus
        {
            get
            {
                return m_SetEnergyBonus;
            }
            set
            {
                m_SetEnergyBonus = value;
                InvalidateProperties();
            }
        }

        public virtual void GetSetProperties(ObjectPropertyList list)
        {
            int prop;

            if ((prop = m_SetSelfRepair) != 0 && WeaponAttributes.SelfRepair == 0)
            {
                list.Add(1060450, prop.ToString()); // self repair ~1_val~
            }

            SetHelper.GetSetProperties(list, this);
        }

        public int SetResistBonus(ResistanceType resist)
        {
            switch (resist)
            {
                case ResistanceType.Physical: return PhysicalResistance;
                case ResistanceType.Fire: return FireResistance;
                case ResistanceType.Cold: return ColdResistance;
                case ResistanceType.Poison: return PoisonResistance;
                case ResistanceType.Energy: return EnergyResistance;
            }

            return 0;
        }
        #endregion

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Altered
        {
            get { return m_Altered; }
            set
            {
                m_Altered = value;
                InvalidateProperties();
            }
        }
    }

    public enum CheckSlayerResult
    {
        None,
        Slayer,
        SuperSlayer,
        Opposition
    }
}
#endregion
#endregion
