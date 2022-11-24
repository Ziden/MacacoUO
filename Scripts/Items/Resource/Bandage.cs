#region References
using System;
using System.Collections.Generic;

using Server.Factions;
using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;
using Server.Network;
using Server.Engines.Despise;
using Server.Services.Virtues;
using Server.Regions;
using Server.Engines.Craft;
#endregion

namespace Server.Items
{
    public class Bandage : Item, IDyable, ICommodity, ICraftable
    {
        public static void Initialize()
        {
            EventSink.BandageTargetRequest += BandageTargetRequest;
        }

        public static int Range = (Core.AOS ? 2 : 1);

        public override double DefaultWeight { get { return 0.1; } }

        [Constructable]
        public Bandage()
            : this(1)
        { }

        [Constructable]
        public Bandage(int amount)
            : base(0xE21)
        {
            Name = "Bandagem";
            Stackable = true;
            Amount = amount;
        }

        public Bandage(Serial serial)
            : base(serial)
        {
            Name = "Bandagem";
        }

        TextDefinition ICommodity.Description { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return true; } }

        public virtual bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
            {
                return false;
            }

            Hue = sender.DyedHue;

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void OnDoubleClick(Mobile from)
        {

            if (from.Paralyzed || from.Frozen)
            {
                from.SendMessage("Voce nao pode fazer isto agora");
                return;
            }

            if (from.InRange(GetWorldLocation(), Range))
            {
                from.RevealingAction(false);

                from.SendMessage("Quem deseja curar ?"); // Who will you use the bandages on?

                from.Target = new BandageTarget(this);
            }
            else
            {
                from.SendMessage("Esta muito longe"); // You are too far away to do that.
            }
        }

        public static void BandageTargetRequest(BandageTargetRequestEventArgs e)
        {
            BandageTargetRequest(e.Bandage as Bandage, e.Mobile, e.Target);
        }

        public static void BandageTargetRequest(Bandage bandage, Mobile from, Mobile target)
        {
            if (bandage == null || bandage.Deleted)
                return;


            if (BandageContext.GetContext(from) != null)
            {
                from.SendMessage("Voce ja esta aplicando bandagens");
                return;
            }

            if (from.InRange(bandage.GetWorldLocation(), Range))
            {
                Target t = from.Target;

                if (t != null)
                {
                    Target.Cancel(from);
                    from.Target = null;
                }

                from.RevealingAction(false);
                from.SendMessage("Quem deseja curar?"); // Who will you use the bandages on?

                new BandageTarget(bandage).Invoke(from, target);
            }
            else
            {
                from.SendMessage("Muito longe"); // You are too far away to do that.
            }
        }

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            this.Amount += (int)(from.Skills[SkillName.Tailoring].Value / 10);
            if (from.Skills[SkillName.Tailoring].Value >= 100)
            {
                this.Amount += 10;
            }
            return 0;
        }

        public class BandageTarget : Target
        {
            private readonly Bandage m_Bandage;

            public BandageTarget(Bandage bandage)
                : base(Bandage.Range, false, TargetFlags.Beneficial)
            {
                m_Bandage = bandage;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Bandage.Deleted)
                {
                    return;
                }

                Corpse corpse = null;
                if (targeted is Corpse)
                {
                    corpse = (Corpse)targeted;
                    if (corpse.Owner is BaseCreature)
                    {
                        var bc = (BaseCreature)corpse.Owner;
                        if (!bc.IsDeadPet)
                        {
                            from.SendMessage("A criatura nao esta morta");
                            return;
                        }
                        if (from != bc.ControlMaster && bc.ControlMaster != null)
                        {
                            bc.ControlMaster.SendMessage(from.Name + " esta revivendo sua criatura");
                            from.SendMessage("Voce comeca a reviver a criatura de " + bc.ControlMaster.Name);
                        }
                        else
                        {
                            from.SendMessage("Voce comeca a reviver a criatura");
                        }
                        targeted = corpse.Owner;
                    }
                }

                if (targeted is Mobile)
                {
                    if (corpse == null && targeted is BaseCreature && ((BaseCreature)targeted).IsDeadPet)
                    {
                        if (targeted is IMount)
                        {
                            from.SendMessage("Para reviver sua montaria use bandagens no corpo da montaria ou a leve para o veterinario !");
                            return;
                        }
                    }

                    if (from.InRange(m_Bandage.GetWorldLocation(), Bandage.Range))
                    {
                        if (BandageContext.BeginHeal(from, (Mobile)targeted, m_Bandage is EnhancedBandage, corpse) != null)
                        {
                            NegativeAttributes.OnCombatAction(from);
                            m_Bandage.Consume();
                        }
                    }
                    else
                    {
                        from.SendMessage("Muito longe"); // You are too far away to do that.
                    }
                }
                else if (targeted is PlagueBeastInnard)
                {
                    if (((PlagueBeastInnard)targeted).OnBandage(from))
                    {
                        NegativeAttributes.OnCombatAction(from);
                        m_Bandage.Consume();
                    }
                }
                else
                {
                    from.SendMessage("Alvo invalido"); // Bandages can not be used on that.
                }
            }

            protected override void OnNonlocalTarget(Mobile from, object targeted)
            {
                if (targeted is PlagueBeastInnard)
                {
                    if (((PlagueBeastInnard)targeted).OnBandage(from))
                    {
                        m_Bandage.Consume();
                    }
                }
                else
                {
                    base.OnNonlocalTarget(from, targeted);
                }
            }
        }
    }

    public class BandageContext
    {
        private readonly Mobile m_Healer;
        private readonly Mobile m_Patient;
        private int m_Slips;
        private int m_HealedPoisonOrBleed;
        private InternalTimer m_Timer;
        private int m_HealingBonus;
        private bool curaPoison = false;
        public Mobile Healer { get { return m_Healer; } }
        public Mobile Patient { get { return m_Patient; } }
        public int Slips { get { return m_Slips; } set { m_Slips = value; } }
        public int HealedPoisonOrBleed { get { return m_HealedPoisonOrBleed; } set { m_HealedPoisonOrBleed = value; } }
        public InternalTimer Time { get { return m_Timer; } }
        public int HealingBonus { get { return m_HealingBonus; } }
        public TimeSpan delay;
        public Corpse corpse;

        public bool FullPower = true;

        public void Slip(bool pvm = false, int dano = 0)
        {
            FullPower = false;
            if (!pvm)
            {
                ++m_Slips;
                if (dano > 15)
                    ++m_Slips;
                if (dano > 35)
                    m_Slips += 2;
            }
            else if (Utility.RandomBool())
                m_Slips++;
            m_Healer.SendMessage("Seus dedos escorregam [- " + m_Slips * SLIP_MULT + " cura]"); // Your fingers slip!
        }

        public BandageContext(Mobile healer, Mobile patient, TimeSpan delay)
            : this(healer, patient, delay, false)
        { }

        public static void Initialize()
        {
            EventSink.Movement += Move;
        }

        public static void Move(MovementEventArgs e)
        {
            var pl = e.Mobile as PlayerMobile;
            if (pl != null && pl.RP)
            {
                var ctx = BandageContext.GetContext(pl);
                if (ctx != null)
                {
                    var chance = 0.8;
                    if (!pl.Correndo())
                        chance = 0.1;
                    if (Utility.RandomDouble() < chance)
                    {
                        ctx.Slip();
                        if (!pl.IsCooldown("dicabands"))
                        {
                            pl.SetCooldown("dicabands", TimeSpan.FromHours(10));
                            pl.SendMessage(78, "Evite se movimentar ou lutar enquanto se cura para uma cura mais eficiente");
                        }
                    }
                }
            }
        }

        private bool enhanced;

        public BandageContext(Mobile healer, Mobile patient, TimeSpan delay, bool enhanced, Corpse corpse = null)
        {
            m_Healer = healer;
            m_Patient = patient;
            this.enhanced = enhanced;
            if (enhanced)
                m_HealingBonus += EnhancedBandage.HealingBonus;

            this.corpse = corpse;
            m_Timer = new InternalTimer(this, delay);
            m_Timer.Start();
        }

        public void StopHeal()
        {
            m_Table.Remove(m_Healer);

            if (m_Timer != null)
            {
                m_Timer.Stop();
            }

            m_Timer = null;
        }

        private static readonly Dictionary<Mobile, BandageContext> m_Table = new Dictionary<Mobile, BandageContext>();

        public static BandageContext GetContext(Mobile healer)
        {
            BandageContext bc = null;
            m_Table.TryGetValue(healer, out bc);
            return bc;
        }

        public static SkillName GetPrimarySkill(Mobile healer, Mobile m)
        {
            if (m is DespiseCreature)
            {
                return healer.Skills[SkillName.Healing].Value > healer.Skills[SkillName.Veterinary].Value ? SkillName.Healing : SkillName.Veterinary;
            }

            if (!m.Player && (m.Body.IsMonster || m.Body.IsAnimal))
            {
                Shard.Debug("VET");
                return SkillName.Veterinary;
            }
            else
            {
                return SkillName.Healing;
            }
        }

        public static SkillName GetSecondarySkill(Mobile healer, Mobile m)
        {
            if (m is DespiseCreature)
            {
                return healer.Skills[SkillName.Healing].Value > healer.Skills[SkillName.Veterinary].Value ? SkillName.Anatomy : SkillName.AnimalLore;
            }

            if (!m.Player && (m.Body.IsMonster || m.Body.IsAnimal))
            {
                return SkillName.AnimalLore;
            }
            else
            {
                return SkillName.Anatomy;
            }
        }

        public void CheckBleed()
        {
            bool bleeding = BleedAttack.IsBleeding(m_Patient);

            if (bleeding)
            {
                double healing = m_Healer.Skills[SkillName.Healing].Value;
                double anatomy = m_Healer.Skills[SkillName.Anatomy].Value;
                double chance = ((healing + anatomy) - 120) * 25;

                chance /= 3 * 20;

                Shard.Debug("Chance curar poison: " + chance);

                if (chance >= Utility.Random(100))
                {
                    m_HealedPoisonOrBleed = 3;

                    if (BleedAttack.IsBleeding(m_Patient))
                    {
                        BleedAttack.EndBleed(m_Patient, false);
                    }
                    m_Patient.SendLocalizedMessage("Voce curou o sangramento"); // You bind the wound and stop the bleeding
                }
            }
        }

        public void CheckPoison()
        {
            bool poisoned = m_Patient.Poisoned;
            if (poisoned)
            {
                double healing = m_Healer.Skills[SkillName.Healing].Value;
                double anatomy = m_Healer.Skills[SkillName.Anatomy].Value;
                double chance = ((healing + anatomy) - 120) * 25;

                chance /= m_Patient.Poison.RealLevel * 20;

                Shard.Debug("Chance curar poison: " + chance);

                if (chance >= Utility.Random(100))
                {
                    m_HealedPoisonOrBleed = m_Patient.Poison.RealLevel;

                    if (poisoned && m_Patient.CurePoison(m_Healer))
                    {
                        m_Patient.SendLocalizedMessage(1010059); // You have been cured of all poisons.
                    }
                }
            }
        }

        public void CheckPoisonOrBleed()
        {
            CheckPoison();
            CheckBleed();
        }

        public bool IsHealing(Mobile from)
        {
            return GetContext(from) != null;
        }

        /*
        public void InterruptAndHealPartially()
        {
            if (this.m_Patient != this.m_Healer)
            {
                return;
            }
            var begin = this.Time.Began;

            var exp = this.Time.Expires - begin;

            var now = Core.TickCount - begin;

            var pct = (double)now / (double)exp;

            if (pct > 0.8)
                pct = 0.8; // max 80% do tempo

            var toHeal = GetToHeal();


            if (Shard.RP)
            {
                pct *= pct; // Qto menos ele se curou, mais nerfa a cura

                toHeal *= pct;
            }

            toHeal = Math.Floor(toHeal);

            if (toHeal <= 0 || BleedAttack.IsBleeding(m_Patient) || MortalStrike.IsWounded(m_Patient) || m_Patient.Poisoned)
            {
                m_Healer.SendMessage("Voce nao teve tempo de se curar direito");
            }
            else
            {
                m_Patient.Heal((int)toHeal, m_Healer, false);
                m_Healer.SendMessage("Voce se apressou e terminou de aplicar as bandagens");
            }

            StopHeal();
        }
        */

        public double GetToHeal()
        {
            if (Shard.SPHERE_STYLE)
            {
                var heal = Utility.Random(21, 9);
                heal -= m_Slips * SLIP_MULT;
                return heal;
            }
            double healing = m_Healer.Skills[SkillName.Healing].Value * 0.75 + 25;
            double anatomy = m_Healer.Skills[SkillName.Anatomy].Value * 0.75 + 25;

            if (Shard.DebugEnabled)
            {
                Shard.Debug("Heal " + healing + " anat " + anatomy);
            }

            bool vet = false;
            if (m_Patient is BaseCreature && !(m_Patient is BaseHire))
            {
                vet = true;
                healing = m_Healer.Skills[SkillName.Veterinary].Value;
                anatomy = (m_Healer.Skills[SkillName.AnimalLore].Value * 0.8) + (m_Healer.Skills[SkillName.Healing].Value + 0.2);
            }

            double min, max;

            min = (anatomy / 5.0) + (healing / 6.0) + 6.0;
            max = (anatomy / 5.0) + (healing / 3.0) + 12.0;

            double toHeal = (min + (Utility.RandomDouble() * (max - min))) * 0.8;

            if (vet && m_Healer.Skills[SkillName.Veterinary].Value > 100)
                toHeal += 20;

            if (this.enhanced)
                toHeal += 5;

            if (healing >= 100)
                toHeal *= 1.1;

            if (toHeal < 20)
                toHeal = 20;

            if ((m_Patient.Body.IsMonster || m_Patient.Body.IsAnimal) && !m_Patient.Player)
            {
                toHeal += m_Patient.HitsMax / 170;
                if (this.enhanced)
                    toHeal += m_Patient.HitsMax / 20;

                if (Shard.DebugEnabled)
                    Shard.Debug("Bonus Enhanced: " + (this.enhanced), m_Patient);

                toHeal -= m_Slips * SLIP_MULT * 3;
            }
            else
            {
                toHeal -= m_Slips * SLIP_MULT;
            }


            if (toHeal < 5)
                toHeal = 5;
            if (Shard.DebugEnabled)
                Shard.Debug("To Heal: " + toHeal + " Escorregadas: " + m_Slips, m_Healer);
            return toHeal;
        }

        public static int SLIP_MULT = Shard.RP ? 5 : 3;

        public void EndHeal()
        {
            StopHeal();
            var hue = 78;
            string healerNumber = null;
            string patientNumber = null;
            bool playSound = true;
            bool checkSkills = false;

            SkillName primarySkill = GetPrimarySkill(m_Healer, m_Patient);
            SkillName secondarySkill = GetSecondarySkill(m_Healer, m_Patient);

            BaseCreature petPatient = m_Patient as BaseCreature;
            if (petPatient != null && petPatient is BaseHire)
                petPatient = null;

            if (!m_Healer.Alive)
            {
                healerNumber = "Voce nao terminou de curar e morreu"; // You were unable to finish your work before you died.
                patientNumber = null;
                playSound = false;
            }
            else if (corpse == null && !m_Healer.InRange(m_Patient, Bandage.Range))
            {
                healerNumber = "Voce esta longe do alvo que estava curando"; // You did not stay close enough to heal your target.
                patientNumber = null;
                playSound = false;
            }
            else if (corpse != null && !m_Healer.InRange(corpse, Bandage.Range))
            {
                healerNumber = "Voce esta muito longe do corpo da criatura"; // You did not stay close enough to heal your target.
                patientNumber = null;
                playSound = false;
            }
            else if (!m_Patient.Alive || (petPatient != null && petPatient.IsDeadPet))
            {
                double healing = m_Healer.Skills[primarySkill].Value;
                double anatomy = m_Healer.Skills[secondarySkill].Value;
                double chance = ((healing - 55) / 50.0) - (m_Slips * 0.02);

                if (((checkSkills = (healing >= 80.0 && anatomy >= 80.0)) && chance > Utility.RandomDouble()) ||
                    (Core.SE && petPatient is FactionWarHorse && petPatient.ControlMaster == m_Healer) ||
                    (Server.Engines.VvV.ViceVsVirtueSystem.Enabled && petPatient is Server.Engines.VvV.VvVMount && petPatient.ControlMaster == m_Healer))
                //TODO: Dbl check doesn't check for faction of the horse here?
                {
                    if (m_Patient.Map == null || !m_Patient.Map.CanFit(m_Patient.Location, 16, false, false))
                    {
                        healerNumber = "Alvo nao pode ressuscitar naquele local"; // Target can not be resurrected at that location.
                        patientNumber = "Nao pode ser ressuscitar aqui"; // Thou can not be resurrected there!
                    }
                    else if (m_Patient.Region != null && m_Patient.Region.IsPartOf("Khaldun"))
                    {
                        healerNumber = "A magia negra daqui lhe impede de ressuscitar alguem"; // The veil of death in this area is too strong and resists thy efforts to restore life.
                        patientNumber = null;
                    }
                    else
                    {
                        healerNumber = "Voce ressuscitar o alvo"; // You are able to resurrect your patient.
                        patientNumber = null;

                        m_Patient.PlaySound(0x214);
                        m_Patient.FixedEffect(0x376A, 10, 16);

                        if (petPatient != null && petPatient.IsDeadPet)
                        {
                            Mobile master = petPatient.ControlMaster;

                            if (master != null)
                            {
                                petPatient.ResurrectPet();

                                for (int i = 0; i < petPatient.Skills.Length; ++i)
                                {
                                    petPatient.Skills[i].Base -= 0.2;
                                }
                            }
                            if (corpse != null)
                            {
                                petPatient.MoveToWorld(corpse.Location, corpse.Map);
                                corpse.Delete();
                            }
                        }
                        else
                        {
                            m_Patient.PlaySound(0x214);
                            m_Patient.FixedEffect(0x376A, 10, 16);
                            m_Patient.Resurrect();
                        }
                    }
                }
                else
                {
                    if (petPatient != null && petPatient.IsDeadPet)
                    {
                        healerNumber = "Voce falhou em ressuscitar a criatura"; // You fail to resurrect the creature.
                    }
                    else
                    {
                        healerNumber = "Voce nao conseguiu ressuscitar a criatura"; // You are unable to resurrect your patient.
                    }

                    patientNumber = null;
                }
            }
            else if (curaPoison && m_Patient.Poisoned)
            {
                m_Healer.SendMessage("Voce terminou de aplicar as bandagens"); // You finish applying the bandages.

                double healing = m_Healer.Skills[primarySkill].Value;
                double anatomy = m_Healer.Skills[secondarySkill].Value;
                double chance = ((healing - 30.0) / 50.0) - (m_Patient.Poison.RealLevel * 0.1) - (m_Slips * 0.02);

                if ((checkSkills = (healing >= 60.0 && anatomy >= 60.0)) && chance > Utility.RandomDouble())
                {
                    if (m_Patient.CurePoison(m_Healer))
                    {
                        healerNumber = (m_Healer == m_Patient) ? null : "Voce foi curado do veneno"; // You have cured the target of all poisons.
                        patientNumber = "Voce foi curado do veneno"; // You have been cured of all poisons.
                    }
                    else
                    {
                        healerNumber = null;
                        patientNumber = null;
                    }
                }
                else
                {
                    healerNumber = "Voce falhou em curar o alvo"; // You have failed to cure your target!
                    patientNumber = null;
                }
            }
            else if (BleedAttack.IsBleeding(m_Patient))
            {
                healerNumber = "Voce estancou o sangramento"; // You bind the wound and stop the bleeding
                patientNumber = "Seu sangramento foi curado"; // The bleeding wounds have healed, you are no longer bleeding!
                BleedAttack.EndBleed(m_Patient, false);
            }
            else if (MortalStrike.IsWounded(m_Patient))
            {
                // healerNumber = (m_Healer == m_Patient ? 1005000 : 1010398);
                // patientNumber = -1;
                playSound = false;
            }
            else if (m_Patient.Hits == m_Patient.HitsMax)
            {
                healerNumber = "Voce se curou um pouco"; // You heal what little damage your patient had.
                patientNumber = null;
            }
            else
            {
                checkSkills = true;
                patientNumber = null;

                double healing = m_Healer.Skills[primarySkill].Value;
                double anatomy = m_Healer.Skills[secondarySkill].Value;

                FirstAidBelt belt = m_Healer.FindItemOnLayer(Layer.Waist) as FirstAidBelt;

                if (belt != null)
                    m_HealingBonus += belt.HealingBonus;

                Item item = m_Healer.FindItemOnLayer(Layer.TwoHanded);

                if (item is Asclepius || item is GargishAsclepius)
                    m_HealingBonus += 15;

                if (m_HealingBonus != 0)
                    healing += m_HealingBonus;

                double chance = ((healing + 20.0) / 100.0) - (m_Slips * 0.02);
                var toHeal = GetToHeal();

                if (chance > Utility.RandomDouble())
                {
                    healerNumber = "Voce terminou de aplicar as bandagens"; // You finish applying the bandages.



                    #region City Loyalty
                    if (Server.Engines.CityLoyalty.CityLoyaltySystem.HasTradeDeal(m_Healer, Server.Engines.CityLoyalty.TradeDeal.GuildOfHealers))
                        toHeal += (int)Math.Ceiling(toHeal * 0.05);
                    #endregion

                    if (m_HealedPoisonOrBleed > 0)
                    {
                        toHeal /= m_HealedPoisonOrBleed;
                    }

                    if (SearingWounds.IsUnderEffects(m_Patient))
                    {
                        toHeal /= 2;
                        m_Patient.SendLocalizedMessage("Suas feridas prejudicam sua cura"); // The cauterized wound resists some of your healing.
                    }

                    if (toHeal < 3)
                    {
                        toHeal = 3;
                        hue = 32;
                        healerNumber = "Voce aplicou as bandagens, mas nao conseguiu curar muito"; // You apply the bandages, but they barely help.
                    }
                    else if (m_Patient != m_Healer && m_Patient is PlayerMobile && m_Healer is PlayerMobile)
                    {
                        SpiritualityVirtue.OnHeal(m_Healer, Math.Min((int)toHeal, m_Patient.HitsMax - m_Patient.Hits));
                    }

                    m_Patient.Heal((int)toHeal, m_Healer, false);
                }
                else
                {
                    m_Patient.Heal(5, m_Healer, false);
                    hue = 32;
                    healerNumber = "Voce aplicou as bandagens, mas nao conseguiu curar quase nada"; // You apply the bandages, but they barely help.
                    playSound = true;
                }
            }

            if (healerNumber != null)
            {
                m_Healer.SendMessage(hue, healerNumber);
            }

            if (patientNumber != null)
            {
                m_Patient.SendMessage(patientNumber);
            }

            if (playSound)
            {
                m_Patient.PlaySound(0x57);
            }

            if (checkSkills)
            {
                m_Healer.CheckSkillMult(secondarySkill, 0.0, 120.0);
                m_Healer.CheckSkillMult(primarySkill, 0.0, 120.0);
            }

            if (m_Patient is PlayerMobile)
                BuffInfo.RemoveBuff(m_Healer, BuffIcon.Healing);
            else
                BuffInfo.RemoveBuff(m_Healer, BuffIcon.Veterinary);
        }

        public class InternalTimer : Timer
        {
            private BandageContext m_Context;
            public long Began;
            public long Expires;
            private bool m_CheckedHealAndBleed;

            public bool CanCheckAtHalf
            {
                get
                {
                    return Core.SA &&
                           m_Context != null && m_Context.Healer == m_Context.Patient &&
                           m_Context.Healer.Skills[SkillName.Healing].Value >= 80 &&
                           m_Context.Healer.Skills[SkillName.Anatomy].Value >= 80;
                }
            }

            public InternalTimer(BandageContext context, TimeSpan delay)
                : base(TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250))
            {
                m_Context = context;
                Priority = TimerPriority.FiftyMS;

                Began = Core.TickCount;
                Expires = Core.TickCount + (int)delay.TotalMilliseconds;
            }

            protected override void OnTick()
            {
                if (Core.TickCount >= Expires)
                {
                    m_Context.EndHeal();
                    Stop();
                }
                else if (!m_CheckedHealAndBleed && CanCheckAtHalf && Began + ((Expires - Began) / 2) < Core.TickCount)
                {
                    m_Context.CheckPoisonOrBleed();
                    m_CheckedHealAndBleed = true;
                }
            }
        }

        public static BandageContext BeginHeal(Mobile healer, Mobile patient)
        {
            return BeginHeal(healer, patient, false);
        }

        public static BandageContext BeginHeal(Mobile healer, Mobile patient, bool enhanced, Corpse c = null)
        {
            bool isDeadPet = (patient is BaseCreature && ((BaseCreature)patient).IsDeadPet);


            var region = Region.Find(patient.Location, patient.Map) as HouseRegion;

            if (patient is IRepairableMobile)
            {

                healer.SendMessage("Voce nao pode curar isso"); // Bandages cannot be used on that.
            }
            else if (patient is BaseCreature && ((BaseCreature)patient).IsAnimatedDead)
            {

                healer.SendMessage("Voce nao pode curar isso"); // You cannot heal that.
            }
            else if (!patient.Poisoned && patient.Hits == patient.HitsMax && !BleedAttack.IsBleeding(patient) && !isDeadPet)
            {

                healer.SendMessage("O alvo nao necessita de cura"); // That being is not damaged!
            }
            else if (!patient.Alive && (patient.Map == null || !patient.Map.CanFit(patient.Location, 16, false, false)))
            {
                healer.SendMessage("Voce nao pode ressar a pessoa ali"); // Target cannot be resurrected at that location.
            }
            else if (healer.CanBeBeneficial(patient, true, true))
            {

                healer.DoBeneficial(patient);

                BandageContext context = GetContext(healer);

                if (context != null)
                {
                    context.StopHeal();
                }

                var delay = GetDelay(healer, patient);

                if (patient is PlayerMobile)
                    BuffInfo.AddBuff(healer, new BuffInfo(BuffIcon.Healing, 1002082, 1151400, delay, healer, String.Format("{0}", patient.Name)));
                else
                    BuffInfo.AddBuff(healer, new BuffInfo(BuffIcon.Veterinary, 1002167, 1151400, delay, healer, String.Format("{0}", patient.Name)));

                context = new BandageContext(healer, patient, delay, enhanced, c);

                if (healer.RP)
                {
                    healer.ClearHands();
                }

                m_Table[healer] = context;
                context.curaPoison = patient.Poisoned;

                if (healer != patient)
                {
                    patient.SendMessage(healer.Name + " comecou aplicar bandagens em voce"); //  : Attempting to heal you.
                }

                if (context.curaPoison)
                {
                    healer.SendMessage("Voce comecou aplicar as bandagens na ferida envenenada");
                }
                else
                {
                    healer.SendMessage("Voce comecou aplicar as bandagens nos ferimentos");
                }

                healer.OverheadMessage("* Aplicando bandagens *"); // You begin applying the bandages.

                if (healer.NetState != null && healer.NetState.IsEnhancedClient)
                {
                    healer.NetState.Send(new BandageTimerPacket((int)delay.TotalSeconds));
                }

                return context;
            }

            return null;
        }

        public static TimeSpan GetDelay(Mobile healer, Mobile patient)
        {
            return GetDelay(healer, patient, !patient.Alive || patient.IsDeadBondedPet);
        }

        public static TimeSpan GetDelay(Mobile healer, Mobile patient, bool dead)
        {
            return GetDelay(healer, patient, dead, GetPrimarySkill(healer, patient));
        }

        public static TimeSpan GetDelay(Mobile healer, Mobile patient, bool dead, SkillName skill)
        {

            if (Shard.SPHERE_STYLE)
            {
                if (dead)
                    return TimeSpan.FromSeconds(10);
                else
                    return TimeSpan.FromSeconds(4.5);

            }
            var resDelay = dead ? 5.0 : 0.0;

            var dex = healer.Dex < 35 ? 35 : healer.Dex * 1.5;

            if (dex > 190)
                dex = 190;

            double seconds;

            if (healer == patient)
            {
                if (Core.AOS)
                {
                    seconds = Math.Min(8, Math.Ceiling(11.0 - dex / 20));
                    seconds = Math.Max(seconds, 4);
                }
                else
                {
                    seconds = 9.4 + (0.6 * ((double)(130 - dex) / 10));
                }

            }
            else if (Core.AOS && skill == SkillName.Veterinary)
            {
                seconds = 2.0;
            }
            else if (Core.AOS)
            {
                seconds = Math.Ceiling((double)4 - dex / 60);
                seconds = Math.Max(seconds, 2);
            }
            else
            {
                seconds = 7.4 + (0.6 * ((double)(120 - dex) / 10));
            }

            if (healer != patient && healer.RP && patient.Player && healer.Player)
            {
                if (((PlayerMobile)healer).Talentos.Tem(Fronteira.Talentos.Talento.Curandeiro))
                    seconds -= 5;
            }

            return TimeSpan.FromSeconds(seconds);
        }
    }

    public sealed class BandageTimerPacket : Packet
    {
        public BandageTimerPacket(int duration)
            : base(0xBF)
        {
            EnsureCapacity(15);

            m_Stream.Write((short)0x31);
            m_Stream.Write((short)0x01);

            m_Stream.Write((int)0xE21);
            m_Stream.Write(duration);
        }
    }
}
