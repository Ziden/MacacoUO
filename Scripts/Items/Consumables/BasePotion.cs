using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Engines.Craft;
using Server.Fronteira.Talentos;
using VitaNex.Modules.AutoPvP;

namespace Server.Items
{
    public enum PotionEffect
    {
        VisaoNoturna,
        CuraMenor,
        Cura,
        CuraMaior,
        Agilidade,
        AgilidadeMaior,
        Forca,
        ForcaMaior,
        VenenoFraco,
        Veneno,
        VenenoForte,
        VenenoMortal,
        Stamina,
        StaminaTotal,
        VidaFraca,
        Vida,
        VidaForte,
        ExplosaoFraca,
        Explosion,
        ExplosionGreater,
        Conflagration,
        ConflagrationGreater,
        MaskOfDeath,		// Mask of Death is not available in OSI but does exist in cliloc files
        MaskOfDeathGreater,	// included in enumeration for compatability if later enabled by OSI
        ConfusionBlast,
        ConfusionBlastGreater,
        Invisibilidade,
        Parasitic,
        Darkglow,
        ExplodingTarPotion,
        #region TOL Publish 93
        Barrab,
        Jukari,
        Kurak,
        Barako,
        Urali,
        Sakkhra,
        #endregion
        Mana,
        ManaFraca,
        ManaForte,
        Inteligencia,
        InteligenciaMaior,
        AntiParalize
    }

    public abstract class BasePotion : Item, ICraftable, ICommodity
    {
        private PotionEffect m_PotionEffect;

        public virtual double Delay { get { return 0; } }

        public virtual bool OnValidateDrink(Mobile from) { return true; }

        public PotionEffect PotionEffect
        {
            get
            {
                return this.m_PotionEffect;
            }
            set
            {
                this.m_PotionEffect = value;
                this.InvalidateProperties();
            }
        }

        TextDefinition ICommodity.Description
        {
            get
            {
                return this.LabelNumber;
            }
        }
        bool ICommodity.IsDeedable
        {
            get
            {
                return true;
            }
        }

        public override string DefaultName
        {
            get
            {
                return "Poção de " + PotionEffect;
            }
        }

        public BasePotion(int itemID, PotionEffect effect)
            : base(itemID)
        {
            this.m_PotionEffect = effect;

            this.Stackable = true;
            this.Weight = 0.8;
        }

        public BasePotion(Serial serial)
            : base(serial)
        {
        }

        public virtual bool RequireFreeHand
        {
            get
            {
                return true;
            }
        }

        public static bool HasFreeHand(Mobile m)
        {
            Item handOne = m.FindItemOnLayer(Layer.OneHanded);
            Item handTwo = m.FindItemOnLayer(Layer.TwoHanded);

            /*
            if (handTwo is BaseWeapon)
                handOne = handTwo;
            */
            if (handTwo is BaseWeapon)
            {
                BaseWeapon wep = (BaseWeapon)handTwo;

                if (wep.Attributes.WeaponSkillDamage > 0)
                    return true;
            }

            return (handOne == null || handTwo == null);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!this.Movable)
                return;

            if (from.Paralyzed)
            {
                if(!(this is AntiParaPotion))
                {
                    from.SendMessage("Voce nao pode beber esta pocao estando paralizado");
                    return;
                }
                var a1 = from.FindItemOnLayer(Layer.OneHanded);
                var a2 = from.FindItemOnLayer(Layer.TwoHanded);
                if (a1 != null || a2 != null)
                {
                    from.SendMessage("Voce nao consegue usar a pocao por estar com suas maos ocupadas durante uma paralizia");
                    return;
                }
            }

            if (!IsChildOf(from.Backpack))
            {
                from.SendMessage("Isto precisa estar em sua mochila");
                return;
            }

            if (from.InRange(this.GetWorldLocation(), 1))
            {
                if (!this.RequireFreeHand || HasFreeHand(from))
                {
                    var reusingExploPot = false;
                    var hasExploPot = this is BaseExplosionPotion && BaseExplosionPotion.Using.ContainsKey(from);
                    if (hasExploPot)
                    {
                        var exploPot = BaseExplosionPotion.Using[from];
                        if (exploPot == this)
                        {
                            reusingExploPot = true;
                        }
                        else
                        {
                            exploPot.OnDoubleClick(from);
                            return;
                        }
                    }

                    if (!OnValidateDrink(from))
                    {
                        from.SendMessage("Aguarde para beber outra pocao deste tipo");
                        return;
                    }

                    var tipoCooldown = Shard.POL_SPHERE ? typeof(BasePotion) : this.GetType();

                    if (!reusingExploPot && !from.BeginAction(tipoCooldown))
                    {
                        from.SendMessage("Aguarde para beber outra pocao"); // You must wait to perform another action.
                        return;
                    }

                    // DELAY GLOBAL DAS POTIONS, 10 segundos
                    Timer.DelayCall(TimeSpan.FromSeconds(10), () => from.EndAction(tipoCooldown));

                    if (this is BaseExplosionPotion && this.Amount > 1)
                    {
                        BasePotion pot = (BasePotion)Activator.CreateInstance(this.GetType());

                        if (pot != null)
                        {
                            Shard.Debug("Begin Action Pot");

                            this.Amount--;
                            if (from.Backpack != null && !from.Backpack.Deleted)
                            {
                                from.Backpack.DropItem(pot);
                            }
                            else
                            {
                                pot.MoveToWorld(from.Location, from.Map);
                            }
                            Shard.Debug("Explo target", from);
                            pot.Drink(from);
                        }
                    }
                    else
                    {
                        from.OverheadMessage("* bebendo *");
                        this.Drink(from);
                        from.SendMessage("Voce tomou uma pocao de " + this.PotionEffect);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(502172); // You must have a free hand to drink a potion.
                }
            }
            else
            {
                from.SendLocalizedMessage(502138); // That is too far away for you to use
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)this.m_PotionEffect);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                case 0:
                    {
                        this.m_PotionEffect = (PotionEffect)reader.ReadInt();
                        break;
                    }
            }

            if (version == 0)
                this.Stackable = Core.ML;
        }

        public abstract void Drink(Mobile from);

        public static void PlayDrinkEffect(Mobile m)
        {
            m.RevealingAction();
            m.PlaySound(0x031);

            if (!(m.Region is PvPRegion))
                m.AddToBackpack(new Bottle());

            if (m.Body.IsHuman && !m.Mounted)
            {
                if (Core.SA)
                {
                    m.Animate(AnimationType.Eat, 0);
                }
                else
                {
                    m.Animate(34, 5, 1, true, false, 0);
                }
            }
        }

        public static int EnhancePotions(Mobile m)
        {
            if (!Core.AOS)
                return 0;

            int EP = AosAttributes.GetValue(m, AosAttribute.EnhancePotions);
            int skillBonus = m.Skills.Alchemy.Fixed / 330 * 10;

            if (Core.ML && EP > 50 && m.IsPlayer())
                EP = 50;

            return (skillBonus);
        }

        public static TimeSpan Scale(Mobile m, TimeSpan v)
        {
            if (!Core.AOS)
                return v;

            double scalar = 1.0 + (0.01 * EnhancePotions(m));

            return TimeSpan.FromSeconds(v.TotalSeconds * scalar);
        }

        public static double Scale(Mobile m, double v)
        {
            if (!Core.AOS)
                return v;

            double scalar = 1.0 + (0.01 * EnhancePotions(m));

            return v * scalar;
        }

        public static int Scale(Mobile m, int v)
        {
            return AOS.Scale(v, 100 + EnhancePotions(m));
        }

        public override bool WillStack(Mobile from, Item dropped)
        {
            return dropped is BasePotion && ((BasePotion)dropped).m_PotionEffect == this.m_PotionEffect && base.WillStack(from, dropped);
        }

        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            if (craftSystem is DefAlchemy)
            {

               

                Container pack = from.Backpack;

                if (pack != null)
                {
                    if ((int)this.PotionEffect >= (int)PotionEffect.Invisibilidade)
                        return 1;

                    List<PotionKeg> kegs = pack.FindItemsByType<PotionKeg>();

                    for (int i = 0; i < kegs.Count; ++i)
                    {
                        PotionKeg keg = kegs[i];

                        if (keg == null)
                            continue;

                        if (keg.Held <= 0 || keg.Held >= 100)
                            continue;

                        if (keg.Type != this.PotionEffect)
                            continue;

                        if (keg.Held < 100 && from.TemTalento(Talento.AlquimiaMagica))
                        {
                            ++keg.Held;
                        }
                        ++keg.Held;

                        this.Consume();
                        from.AddToBackpack(new Bottle());

                        return -1; // signal placed in keg
                    }
                }
            }

            if (from.TemTalento(Talento.AlquimiaMagica))
            {
                var dupe = Dupe.DupeItem(this);
                from.AddToBackpack(dupe);
            }

            return 1;
        }
        #endregion
    }
}
