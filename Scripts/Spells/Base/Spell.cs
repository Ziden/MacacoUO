#region References
using System;
using System.Collections.Generic;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Spells.Bushido;
using Server.Spells.Necromancy;
using Server.Spells.Chivalry;
using Server.Spells.Ninjitsu;
using Server.Spells.First;
using Server.Spells.Second;
using Server.Spells.Third;
using Server.Spells.Fourth;
using Server.Spells.Spellweaving;
using Server.Targeting;
using Server.Spells.SkillMasteries;
using System.Reflection;
using Server.Spells.Mysticism;
using System.Linq;
using Server.Spells.Fifth;
using Server.Spells.Sixth;
using Server.Misc.Custom;
using Server.Fronteira.Talentos;
using Server.Fronteira.Elementos;
#endregion

namespace Server.Spells
{
    public abstract class Spell : ISpell
    {
        public static int RANGE = Shard.POL_STYLE ? 17 : 10;

        public Mobile ManaToCaster;

        public Mobile m_Caster;
        private readonly Item m_Scroll;
        private readonly SpellInfo m_Info;
        private SpellState m_State;
        private long m_StartCastTime;
        private object m_InstantTarget;

        public int ID { get { return SpellRegistry.GetRegistryNumber(this); } }

        public SpellState State { get { return m_State; } set { m_State = value; } }

        public Mobile Caster { get { return m_Caster; } }
        public SpellInfo Info { get { return m_Info; } }
        public string Name { get { return m_Info.Name; } }
        public string Mantra { get { return m_Info.Mantra; } }
        public Type[] Reagents { get { return m_Info.Reagents; } }
        public Item Scroll { get { return m_Scroll; } }
        public long StartCastTime { get { return m_StartCastTime; } }

        public virtual int SkillNeeded { get { return 0; } }

        public object InstantTarget { get { return m_InstantTarget; } set { m_InstantTarget = value; } }

        private static readonly TimeSpan NextSpellDelay = TimeSpan.FromSeconds(0.0);
        private static TimeSpan AnimateDelay = TimeSpan.FromSeconds(1.5);

        public virtual SkillName CastSkill { get { return SkillName.Magery; } }
        public virtual SkillName DamageSkill { get { return SkillName.EvalInt; } }

        public virtual bool RevealOnCast { get { return true; } }
        public virtual bool ClearHandsOnCast { get { return true; } }
        public virtual bool ShowHandMovement { get { return true; } }

        public virtual bool DelayedDamage { get { return false; } }
        public virtual Type[] DelayDamageFamily { get { return null; } }
        // DelayDamageFamily can define spells so they don't stack, even though they are different spells
        // Right now, magic arrow and nether bolt are the only ones that have this functionality

        public virtual bool DelayedDamageStacking { get { return true; } }
        //In reality, it's ANY delayed Damage spell Post-AoS that can't stack, but, only 
        //Expo & Magic Arrow have enough delay and a short enough cast time to bring up 
        //the possibility of stacking 'em.  Note that a MA & an Explosion will stack, but
        //of course, two MA's won't.

        public virtual DamageType SpellDamageType { get { return DamageType.Spell; } }

        private static readonly Dictionary<Type, DelayedDamageContextWrapper> m_ContextTable =
            new Dictionary<Type, DelayedDamageContextWrapper>();

        private class DelayedDamageContextWrapper
        {
            private readonly Dictionary<IDamageable, Timer> m_Contexts = new Dictionary<IDamageable, Timer>();

            public void Add(IDamageable d, Timer t)
            {
                Timer oldTimer;

                if (m_Contexts.TryGetValue(d, out oldTimer))
                {
                    oldTimer.Stop();
                    m_Contexts.Remove(d);
                }

                m_Contexts.Add(d, t);
            }

            public bool Contains(IDamageable d)
            {
                return m_Contexts.ContainsKey(d);
            }

            public void Remove(IDamageable d)
            {
                m_Contexts.Remove(d);
            }
        }

        public virtual bool ValidateCast(Mobile from) { return true; }

        public void StartDelayedDamageContext(IDamageable d, Timer t)
        {
            if (DelayedDamageStacking)
            {
                return; //Sanity
            }

            DelayedDamageContextWrapper contexts;

            if (!m_ContextTable.TryGetValue(GetType(), out contexts))
            {
                contexts = new DelayedDamageContextWrapper();
                Type type = GetType();

                m_ContextTable.Add(type, contexts);

                if (DelayDamageFamily != null)
                {
                    foreach (var familyType in DelayDamageFamily)
                    {
                        m_ContextTable.Add(familyType, contexts);
                    }
                }
            }

            contexts.Add(d, t);
        }

        public bool HasDelayContext(IDamageable d)
        {
            if (DelayedDamageStacking)
            {
                return false; //Sanity
            }

            Type t = GetType();

            if (m_ContextTable.ContainsKey(t))
            {
                return m_ContextTable[t].Contains(d);
            }

            return false;
        }

        public void RemoveDelayedDamageContext(IDamageable d)
        {
            DelayedDamageContextWrapper contexts;
            Type type = GetType();

            if (!m_ContextTable.TryGetValue(type, out contexts))
            {
                return;
            }

            contexts.Remove(d);

            if (DelayDamageFamily != null)
            {
                foreach (var t in DelayDamageFamily)
                {
                    if (m_ContextTable.TryGetValue(t, out contexts))
                    {
                        contexts.Remove(d);
                    }
                }
            }
        }

        public void HarmfulSpell(IDamageable d)
        {
            if (d is BaseCreature)
            {
                ((BaseCreature)d).OnHarmfulSpell(m_Caster);
            }
            else if (d is IDamageableItem)
            {
                ((IDamageableItem)d).OnHarmfulSpell(m_Caster);
            }

            NegativeAttributes.OnCombatAction(Caster);

            if (d is Mobile)
            {
                if ((Mobile)d != m_Caster)
                    NegativeAttributes.OnCombatAction((Mobile)d);

                EvilOmenSpell.TryEndEffect((Mobile)d);
            }
        }

        public Spell(Mobile caster, Item scroll, SpellInfo info)
        {
            m_Caster = caster;
            m_Scroll = scroll;
            m_Info = info;
        }

        public virtual int GetNewAosDamage(int bonus, int dice, int sides, IDamageable singleTarget)
        {
            if (singleTarget != null)
            {
                return GetNewAosDamage(bonus, dice, sides, (Caster.Player && singleTarget is PlayerMobile), GetDamageScalar(singleTarget as Mobile), singleTarget);
            }
            else
            {
                return GetNewAosDamage(bonus, dice, sides, false, null);
            }
        }

        public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer, IDamageable damageable)
        {
            return GetNewAosDamage(bonus, dice, sides, playerVsPlayer, 1.0, damageable);
        }

        public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer, double scalar, IDamageable damageable)
        {
            Mobile target = damageable as Mobile;

            int damage = Utility.Dice(dice, sides, bonus) * 100;

            int inscribeSkill = GetInscribeFixed(m_Caster);
            int scribeBonus = inscribeSkill >= 1000 ? 10 : inscribeSkill / 200;

            int damageBonus = scribeBonus +
                              (Caster.Int / 10) +
                              SpellHelper.GetSpellDamageBonus(m_Caster, target, CastSkill, playerVsPlayer);

            int evalSkill = GetDamageFixed(m_Caster);
            int evalScale = 30 + ((9 * evalSkill) / 100);

            damage = AOS.Scale(damage, evalScale);
            damage = AOS.Scale(damage, 100 + damageBonus);
            damage = AOS.Scale(damage, (int)(scalar * 100));

            return damage / 100;
        }

        public virtual bool IsCasting { get { return m_State == SpellState.Casting; } }

        public virtual void OnCasterHurt(Mobile from = null)
        {
            if (from is BaseCreature && Utility.RandomBool())
                return;
            CheckCasterDisruption(false, 0, 0, 0, 0, 0);
        }

        public virtual bool CheckMovement(Mobile caster)
        {
            if (caster.Player && IsCasting && BlocksMovement && (!(m_Caster is BaseCreature) || ((BaseCreature)m_Caster).FreezeOnCast))
            {
                if (caster.RP)
                {
                    if (caster.TemTalento(Talento.Sagacidade))
                        return true;
                    return false;
                }
                return Shard.POL_SPHERE;
            }
            return true;

        }

        public virtual void CheckCasterDisruption(bool checkElem = false, int phys = 0, int fire = 0, int cold = 0, int pois = 0, int nrgy = 0)
        {
            if (Caster.AccessLevel > AccessLevel.VIP)
            {
                return;
            }

            if (!Caster.Player) // is BaseCreature
            {

                if (Caster is BaseCreature)
                {
                    var dmg = phys + fire + cold + pois + nrgy;

                    if (dmg > 5)
                    {
                        var bc = (BaseCreature)Caster;
                        var chance = bc.DisturbChance;
                        if (Utility.RandomDouble() > chance)
                        {
                            return;
                        }
                    }
                    else
                        return;
                }
            }

            if (IsCasting)
            {
                object o = ProtectionSpell.CastDisturbProtection[m_Caster];
                bool disturb = true;

                if (o != null && o is double)
                {
                    if (((double)o) > Utility.RandomDouble() * 100.0)
                    {
                        disturb = false;
                    }
                }

                #region Stygian Abyss

                if (!Shard.SPHERE_STYLE)
                {
                    int focus = SAAbsorptionAttributes.GetValue(Caster, SAAbsorptionAttribute.CastingFocus);

                    if (BaseFishPie.IsUnderEffects(m_Caster, FishPieEffect.CastFocus))
                        focus += 2;

                    if (focus > 12)
                        focus = 12;

                    if (m_Caster.Skills[SkillName.Focus].Value >= 50)
                    {
                        focus += (int)(m_Caster.Skills[SkillName.Focus].Fixed / 400d);
                        focus += (int)(m_Caster.Skills[SkillName.Meditation].Fixed / 400d);
                    }
                    else
                    {
                        if (!m_Caster.IsCooldown("dicafocus"))
                        {
                            m_Caster.SetCooldown("dicafocus");
                            m_Caster.SendMessage(78, "Tendo pelo menos 50 de focus vc pode diminuir suas chances de tomar disturb.");
                        }
                    }

                    if (focus > 0 && focus > Utility.Random(100))
                    {
                        disturb = false;
                        Caster.SendLocalizedMessage("Voce manteve seu foco"); // You regain your focus and continue casting the spell.
                    }
                    else if (checkElem)
                    {
                        int res = 0;

                        if (phys == 100)
                            res = Math.Min(40, SAAbsorptionAttributes.GetValue(m_Caster, SAAbsorptionAttribute.ResonanceKinetic));

                        else if (fire == 100)
                            res = Math.Min(40, SAAbsorptionAttributes.GetValue(m_Caster, SAAbsorptionAttribute.ResonanceFire));

                        else if (cold == 100)
                            res = Math.Min(40, SAAbsorptionAttributes.GetValue(m_Caster, SAAbsorptionAttribute.ResonanceCold));

                        else if (pois == 100)
                            res = Math.Min(40, SAAbsorptionAttributes.GetValue(m_Caster, SAAbsorptionAttribute.ResonancePoison));

                        else if (nrgy == 100)
                            res = Math.Min(40, SAAbsorptionAttributes.GetValue(m_Caster, SAAbsorptionAttribute.ResonanceEnergy));

                        if (res > Utility.Random(100))
                            disturb = false;
                    }
                }
                #endregion

                if (disturb)
                {
                    if ((DateTime.UtcNow - Caster.LastResist).TotalSeconds > 2)
                    {
                        Disturb(DisturbType.Dano, false, true);
                    }
                    else
                    {
                        Shard.Debug("Resist n toma disturb");
                    }
                }
            }
            Caster.LastResist = DateTime.MinValue;
        }

        public virtual bool PunishSpellMovementIfRepeated { get { return false; } }

        public virtual void OnCasterKilled()
        {
            Disturb(DisturbType.Morte);
        }

        public virtual void OnConnectionChanged()
        {
            FinishSequence();
        }


        public virtual bool DoStep(Mobile caster)
        {
            if (!Shard.POL_STYLE)
                return true;

            if (caster.SpellSteps > 1 && this is MarkSpell)
            {
                Disturb(DisturbType.Movimentar);
                Caster.SendMessage("Você não pode se movimentar usando esta magia");
                return true;
            }

            var maxDistance = 17;
            if (!caster.Mounted)
                maxDistance = 9;
            /*
            if (Caster.RP && Caster.Player)
            {
                var lvl = ((PlayerMobile)Caster).Talentos.GetNivel(Talento.Concentracao);
                if (lvl == 3)
                    return true;
            }
            */

            bool repeatedNerf = false;
            if (MagerySpell.MovementNerfWhenRepeated.IndexOf(this.GetType()) == -1)
            {

            }
            else
            {
                repeatedNerf = true;
                var repeatedSpells = 3 * this.Caster.GetRepeatedTypes(this.GetType(), TimeSpan.FromSeconds(20));
                maxDistance -= repeatedSpells;
            }

            caster.SpellSteps++;
            if (Shard.DebugEnabled)
                Shard.Debug("Passos castando: " + caster.SpellSteps);

            if (m_State == SpellState.Casting && caster.SpellSteps > maxDistance && !((this is TeleportSpell) || caster.IsCooldown("tp")))
            {
                Disturb(DisturbType.Movimentar);
                if (repeatedNerf)
                    Caster.SendMessage("Por usar esta magia repetidamente, voce perdeu a concentracao mais facilmente");
                Caster.SendMessage("Você se moveu muito e perdeu a concentração");
            }
            return true;
        }

        public virtual bool OnCasterEquiping(Item item)
        {
            if (IsCasting)
            {
                if ((item.Layer == Layer.OneHanded || item.Layer == Layer.TwoHanded) && item.AllowEquipedCast(Caster))
                {
                    return true;
                }



                if (Shard.POL_STYLE)
                {
                    var tempoPassou = Core.TickCount - this.m_StartCastTime;
                    if (Shard.DebugEnabled)
                    {
                        Shard.Debug("Tempo passou: " + tempoPassou);
                    }
                    if (tempoPassou < 500)
                    {
                        if (Caster.Skills.Focus.Value < 80)
                        {
                            Disturb(DisturbType.Equipar);
                            if (!Caster.IsCooldown("dicasp"))
                            {
                                Caster.SetCooldown("dicasp");
                                Caster.SendMessage(78, "Voce equipou sua arma muito rapido. Aguarde pelo menos 500ms para equipar sua arma depois de iniciar uma magia. Voce pode ter 80 ou mais da skill Focus para evitar isso.");
                            }
                        }
                    }

                }


            }

            return true;
        }

        public virtual bool OnCasterUsingObject(object o)
        {
            if (m_State == SpellState.Sequencing)
            {
                Disturb(DisturbType.Interagir);
            }

            return true;
        }

        public virtual bool OnCastInTown(Region r)
        {
            return m_Info.AllowTown;
        }

        public virtual bool ConsumeReagents()
        {
            if (Shard.WARSHARD || Shard.SPHERE_STYLE)
                return true;

            if ((m_Scroll != null && !(m_Scroll is SpellStone)) || !m_Caster.Player)
            {
                return true;
            }

            if (m_Caster.TemTalento(Talento.Livros) && m_Caster.Weapon is Spellbook)
            {
                if (60 > Utility.Random(100))
                    return true;
            }

            if (AosAttributes.GetValue(m_Caster, AosAttribute.LowerRegCost) > Utility.Random(100))
            {
                return true;
            }

            if (this is MagerySpell || this is NecromancerSpell)
            {
                if (ElementalBall.UseElementalBall(Caster))
                    return true;
            }

            Container pack = m_Caster.Backpack;

            if (pack == null)
            {
                return false;
            }

            if (pack.ConsumeTotal(m_Info.Reagents, m_Info.Amounts) == -1)
            {
                return true;
            }

            return false;
        }

        public virtual double GetInscribeSkill(Mobile m)
        {
            // There is no chance to gain
            // m.CheckSkill( SkillName.Inscribe, 0.0, 120.0 );
            return m.Skills[SkillName.Inscribe].Value;
        }

        public virtual int GetInscribeFixed(Mobile m)
        {
            // There is no chance to gain
            // m.CheckSkill( SkillName.Inscribe, 0.0, 120.0 );
            return m.Skills[SkillName.Inscribe].Fixed;
        }

        public virtual int GetDamageFixed(Mobile m)
        {
            //m.CheckSkill( DamageSkill, 0.0, m.Skills[DamageSkill].Cap );
            return m.Skills[DamageSkill].Fixed;
        }

        public virtual double GetDamageSkill(Mobile m)
        {
            //m.CheckSkill( DamageSkill, 0.0, m.Skills[DamageSkill].Cap );
            return m.Skills[DamageSkill].Value;
        }

        public virtual double GetResistSkill(Mobile m)
        {
            return m.Skills[SkillName.MagicResist].Value - EvilOmenSpell.GetResistMalus(m);
        }

        public virtual double GetDamageScalar(Mobile target, ElementoPvM elementoMagia = ElementoPvM.None)
        {
            double scalar = 1.0;

            if (target == null)
                return scalar;

            double casterEI = m_Caster.Skills[DamageSkill].Value;
            double targetRS = target.Skills[SkillName.MagicResist].Value;

            if (!target.Player && casterEI < 100)
                casterEI = 100;

            if (!target.Player && m_Caster.Player)
            {
                var bonus = targetRS * Caster.GetBonusElemento(ElementoPvM.Escuridao);
                if (bonus > targetRS) bonus = targetRS;
                targetRS -= bonus;

                scalar += AosAttributes.GetValue(Caster, AosAttribute.SpellDamage, true) / 100;

                if (elementoMagia == ElementoPvM.Escuridao)
                {
                    scalar += ColarElemental.GetNivel(m_Caster, ElementoPvM.Escuridao) / 15;
                }

                if (m_Scroll is BaseWand)
                {
                    scalar += 1; // bonus de dano wands em PvM

                    scalar += ColarElemental.GetNivel(m_Caster, ElementoPvM.Gelo) / 10;
                }
            }

            var pl = m_Caster as PlayerMobile;

            if (PsychicAttack.Registry.ContainsKey(target))
                scalar += 0.2;

            if (scalar < 0)
                scalar = 0;

            //m_Caster.CheckSkill( DamageSkill, 0.0, 120.0 );

            if (casterEI > targetRS)
            {
                scalar = (1.0 + ((casterEI - targetRS) / 500.0));
            }
            else
            {
                scalar = (1.0 + ((casterEI - targetRS) / 200.0));
            }

            if (pl != null && pl.RP)
            {
                if (pl != null && pl.Talentos.Tem(Talento.Elementalismo))
                {
                    if (pl != null)
                        scalar += 0.1;
                    if (target is BaseCreature)
                        scalar += 0.2;
                }
                else
                    scalar -= 0.15;
            }

            scalar += (m_Caster.Skills[CastSkill].Value - 100.0) / 400.0;

            if (!target.Player && !target.Body.IsHuman /*&& !Core.AOS*/)
            {
                scalar *= 2.5;
            }

            if (target is BaseCreature)
            {
                ((BaseCreature)target).AlterDamageScalarFrom(m_Caster, ref scalar);
            }

            if (m_Caster is BaseCreature)
            {
                ((BaseCreature)m_Caster).AlterDamageScalarTo(target, ref scalar);
            }

            scalar *= GetSlayerDamageScalar(target);

            target.Region.SpellDamageScalar(m_Caster, target, ref scalar);

            if (Evasion.CheckSpellEvasion(target)) //Only single target spells an be evaded
            {
                scalar = 0;
            }

            if (!target.Player)
            {
                if (target.Elemento != ElementoPvM.None && elementoMagia.ForteContra(target.Elemento))
                {
                    EfeitosElementos.Effect(target, elementoMagia);
                    scalar += 0.25;
                    Shard.Debug("Alvo forte contra elemento", m_Caster);
                }
                else if (target.Elemento != ElementoPvM.None && target.Elemento.ForteContra(elementoMagia))
                {
                    scalar -= 0.25;
                    Shard.Debug("Alvo fraco contra elemento", m_Caster);
                }
            }

            if (!target.Player)
            {
                if (elementoMagia != ElementoPvM.None)
                {
                    var bonus = (m_Caster.GetBonusElemento(elementoMagia) * 2);
                    if (m_Caster.Elemento == ElementoPvM.Agua && elementoMagia == ElementoPvM.Raio)
                        bonus += m_Caster.GetBonusElemento(ElementoPvM.Agua) * 2;
                    if (bonus > 0)
                        EfeitosElementos.Effect(target, elementoMagia);
                    Shard.Debug("Bonus elemento PvM: " + bonus, m_Caster);
                    scalar += bonus;
                }
            }

            if (!target.Player && m_Caster.Player)
            {
                Spellbook atkBook = Spellbook.FindEquippedSpellbook(m_Caster);
                if (atkBook != null && atkBook.SpellCount == 64)
                {
                    scalar += 0.2;
                }
            }

            Shard.Debug("Scalar da magia final: " + scalar, m_Caster);
            return scalar;
        }

        public virtual double GetSlayerDamageScalar(Mobile defender)
        {
            Spellbook atkBook = Spellbook.FindEquippedSpellbook(m_Caster);
            double scalar = 1.0;
            var staff = m_Caster.Weapon as BaseStaff;
            if (staff != null && staff.Slayer != SlayerName.None)
            {
                SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(staff.Slayer);
                if(atkSlayer != null && atkSlayer.Slays(defender))
                {
                    defender.FixedEffect(0x37B9, 10, 5);
                    if (atkSlayer != null && atkSlayer == atkSlayer.Group.Super)
                        scalar = 2;
                    else
                        scalar = 3;
                }

            } else if (atkBook != null)
            {
                SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(atkBook.Slayer);
                SlayerEntry atkSlayer2 = SlayerGroup.GetEntryByName(atkBook.Slayer2);

                if (atkSlayer == null && atkSlayer2 == null)
                {
                    atkSlayer = SlayerGroup.GetEntryByName(SlayerSocket.GetSlayer(atkBook));
                }

                if (atkSlayer != null && atkSlayer.Slays(defender) || atkSlayer2 != null && atkSlayer2.Slays(defender))
                {
                    defender.FixedEffect(0x37B9, 10, 5);

                    bool isSuper = false;

                    if (atkSlayer != null && atkSlayer == atkSlayer.Group.Super)
                        isSuper = true;
                    else if (atkSlayer2 != null && atkSlayer2 == atkSlayer2.Group.Super)
                        isSuper = true;

                    scalar = isSuper ? 2.0 : 3.0;
                }


                TransformContext context = TransformationSpellHelper.GetContext(defender);

                if ((atkBook.Slayer == SlayerName.Undeads || atkBook.Slayer2 == SlayerName.Undeads) && context != null && context.Type != typeof(HorrificBeastSpell))
                    scalar += .5; // Every necromancer transformation other than horrific beast take an additional 25% damage

                if (scalar != 1.0)
                {
                    if (Shard.DebugEnabled)
                        Shard.Debug("Scalar de livro slayer: " + scalar);
                    return scalar;
                }
            }


            ISlayer defISlayer = Spellbook.FindEquippedSpellbook(defender);

            if (defISlayer == null)
            {
                defISlayer = defender.Weapon as ISlayer;
            }

            if (defISlayer != null)
            {
                SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
                SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);

                if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(m_Caster) ||
                    defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(m_Caster))
                {
                    scalar = 2.0;
                }
            }

            return scalar;
        }

        public virtual void DoFizzle()
        {
            m_Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, true, "* magia se dissipa *"); // The spell fizzles.

            if (m_Caster.Player)
            {
                if (Core.AOS)
                {
                    m_Caster.FixedParticles(0x3735, 1, 30, 9503, EffectLayer.Waist);
                }
                else
                {
                    Effects.SendLocationParticles(EffectItem.Create(m_Caster.Location, m_Caster.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
                    //m_Caster.FixedEffect(0x3735, 6, 30);
                }

                m_Caster.PlaySound(0x5C);
            }
        }

        private CastTimer m_CastTimer;
        private AnimTimer m_AnimTimer;

        public void Disturb(DisturbType type)
        {
            Disturb(type, true, false);
        }

        public virtual bool CheckDisturb(DisturbType type, bool firstCircle, bool resistable)
        {
            if (resistable && m_Scroll is BaseWand)
            {
                return false;
            }

            return true;
        }

        public void Disturb(DisturbType type, bool firstCircle, bool resistable)
        {
            if (!CheckDisturb(type, firstCircle, resistable))
            {
                Shard.Debug("Sem check disturb");
                return;
            }

            if (m_State == SpellState.Casting)
            {
                Shard.Debug($"Disturb {type.ToString()} em casting", m_Caster);
                if (!firstCircle && !Core.AOS && this is MagerySpell && ((MagerySpell)this).Circle == SpellCircle.First)
                {
                    Shard.Debug("First");
                    return;
                }

                m_State = SpellState.None;
                m_Caster.Spell = null;
                Caster.Delta(MobileDelta.Flags);
                DoHurtFizzle();
                m_Caster.SendMessage(32, "Voce perdeu a concentracao de sua magia por " + type.ToString());
                OnDisturb(type, true);

                if (m_CastTimer != null)
                {
                    m_CastTimer.Stop();
                }

                if (m_AnimTimer != null)
                {
                    m_AnimTimer.Stop();
                }

                m_Caster.NextSpellTime = Core.TickCount + (int)GetDisturbRecovery().TotalMilliseconds;
            }
            else if (m_State == SpellState.Sequencing)
            {
                Shard.Debug("Disturb em sequencing", m_Caster);
                if (!firstCircle && !Core.AOS && this is MagerySpell && ((MagerySpell)this).Circle == SpellCircle.First)
                {
                    return;
                }

                m_State = SpellState.None;
                m_Caster.Spell = null;
                Caster.Delta(MobileDelta.Flags);
                OnDisturb(type, false);
                m_Caster.SendMessage(32, "Voce perdeu a concentracao de sua magia por " + type.ToString());
                Target.Cancel(m_Caster);
                DoHurtFizzle();
            }
            else
            {
                Shard.Debug("Disturb e magia tava no estado " + m_State);
            }
        }

        public virtual void DoHurtFizzle()
        {
            Effects.SendLocationParticles(EffectItem.Create(m_Caster.Location, m_Caster.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
            m_Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, true, "* Perdeu a concentracao *");
            m_Caster.PlaySound(0x5C);
        }

        public virtual void OnDisturb(DisturbType type, bool message)
        {
            //if (message)
            //{
            //    m_Caster.SendLocalizedMessage(500641); // Your concentration is disturbed, thus ruining thy spell.
            //}
        }

        public virtual bool CheckCast()
        {
            #region High Seas
            if (Server.Multis.BaseBoat.IsDriving(m_Caster) && m_Caster.AccessLevel <= AccessLevel.VIP)
            {
                m_Caster.SendLocalizedMessage(1049616); // You are too busy to do that at the moment.
                return false;
            }
            #endregion

            return true;
        }

        public Type[] curses = new Type[] {
            typeof(ParalyzeSpell)
        };

        public virtual void SayMantra()
        {
            if (m_Scroll is SpellStone)
            {
                return;
            }

            if (m_Scroll is BaseWand)
            {
                return;
            }

            if (m_Info.Mantra != null && m_Info.Mantra.Length > 0 && (m_Caster.Player || (m_Caster is BaseCreature && ((BaseCreature)m_Caster).ShowSpellMantra)))
            {
                m_Caster.PublicOverheadMessage(MessageType.Regular, m_Caster.SpeechHue, false, m_Info.Mantra);
                /*
                if (m_Caster is PlayerMobile)
                {
                    m_Caster.PublicOverheadMessage(MessageType.Emote, 1153, false, m_Info.Mantra);
                }
                else
                    m_Caster.PublicOverheadMessage(MessageType.Regular, 0, false, "* conjurando uma magia *");
                //m_Caster.PublicOverheadMessage(MessageType.Spell, m_Caster.SpeechHue, true, m_Info.Mantra, false);
                */
            }
        }

        public virtual bool BlockedByHorrificBeast
        {
            get
            {
                if (TransformationSpellHelper.UnderTransformation(Caster, typeof(HorrificBeastSpell)) &&
                    SpellHelper.HasSpellFocus(Caster, CastSkill))
                    return false;

                return true;
            }
        }

        public static int CicloArmadura(Mobile from)
        {
            var min = 8;
            var equips = from.GetEquipment();
            foreach (var i in equips)
            {
                if (i is BaseArmor)
                {
                    var ar = (BaseArmor)i;
                    if (ar.MaxMageryCircle < min)
                        min = ar.MaxMageryCircle;
                }
            }
            return min;

        }

        public virtual bool BlockedByAnimalForm { get { return true; } }
        public virtual bool BlocksMovement { get { return true; } }

        public virtual bool CheckNextSpellTime { get { return !(m_Scroll is BaseWand); } }

        private bool CastaMagiaSphere()
        {
            Caster.Target = new TargetMagiaSphere(this);
            return true;
        }

        private class TargetMagiaSphere : Target
        {
            private Spell m_Spell;

            public TargetMagiaSphere(Spell s) : base(Spell.RANGE, true, TargetFlags.None)
            {
                this.m_Spell = s;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                m_Spell.InstantTarget = targeted;
                m_Spell.CastMagiaPadrao();
                if (Shard.SPHERE_STYLE)
                    m_Spell.Caster.ClearHands();

                /*
                if(targeted is Mobile)
                {
                    Shard.Debug("Alvo Point 3D", from);
                    var alvo = (Point3D)targeted;
                    if(from.InLOS(alvo))
                    {
                        if(targeted is IDamageable)
                        {
                            m_Spell.InstantTarget = (IDamageable)targeted;
                        } else
                        {
                            m_Spell.alvoSphere = targeted;
                        }
                        m_Spell.CastMagiaPadrao();
                    } else
                    {
                        from.SendMessage("Voce nao consegue ver isto");
                    }
                } else
                {
                    from.SendMessage("Alvo invalido");
                }
                */
            }
        }

        private object alvoSphere = null;

        public bool CastMagiaPadrao()
        {
            #region Stygian Abyss
            if (m_Caster.Race == Race.Gargoyle && m_Caster.Flying)
            {
                if (BaseMount.OnFlightPath(m_Caster))
                {
                    if (m_Caster.IsPlayer())
                    {
                        m_Caster.SendLocalizedMessage(1113750); // You may not cast spells while flying over such precarious terrain.
                        return false;
                    }
                    else
                    {
                        m_Caster.SendMessage("Your staff level allows you to cast while flying over precarious terrain.");
                    }
                }
            }
            #endregion



            /*

            if (m_Caster.Spell != null)
            {
                Shard.Debug("Disturbando magia existente");
                if (m_Caster.Spell is Spell)
                {
                    Shard.Debug("Foi");
                    ((Spell)m_Caster.Spell).Disturb(DisturbType.NewCast, false, false);
                }
            }
            */

            if (m_Caster.Spell == null && m_Caster.CheckSpellCast(this) && CheckCast() &&
                m_Caster.Region.OnBeginSpellCast(m_Caster, this))
            {


                Shard.Debug("Castando", m_Caster);

                m_State = SpellState.Casting;
                m_Caster.Spell = this;
                m_Caster.SpellSteps = 0;

                Caster.Delta(MobileDelta.Flags);

                if (!(m_Scroll is BaseWand) && RevealOnCast)
                {
                    m_Caster.RevealingAction();
                }

                SayMantra();

                if (m_Caster is PlayerMobile)
                {
                    var player = (PlayerMobile)m_Caster;
                    player.SpellSteps = 0;
                }

                TimeSpan castDelay = GetCastDelay();

                if (ShowHandMovement && !(m_Scroll is SpellStone) && (m_Caster.Body.IsHuman || (m_Caster.Player && m_Caster.Body.IsMonster)))
                {
                    int count = (int)Math.Ceiling(castDelay.TotalSeconds / AnimateDelay.TotalSeconds);

                    if (count != 0)
                    {
                        m_AnimTimer = new AnimTimer(this, count);
                        m_AnimTimer.Start();
                    }

                    if (m_Info.LeftHandEffect > 0)
                    {
                        Caster.FixedParticles(0, 10, 5, m_Info.LeftHandEffect, EffectLayer.LeftHand);
                    }

                    if (m_Info.RightHandEffect > 0)
                    {
                        Caster.FixedParticles(0, 10, 5, m_Info.RightHandEffect, EffectLayer.RightHand);
                    }
                }

                if (ClearHandsOnCast)
                {
                    m_Caster.ClearHands(); // botar dps do target
                }

                WeaponAbility.ClearCurrentAbility(m_Caster);

                m_CastTimer = new CastTimer(this, castDelay);
                //m_CastTimer.Start();

                OnBeginCast();

                if (castDelay > TimeSpan.Zero)
                {
                    m_CastTimer.Start();
                }
                else
                {
                    m_CastTimer.Tick();
                }


                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool Cast()
        {
            /*
            if(Shard.POL_STYLE)
            {
                var bdg = BandageContext.GetContext(m_Caster);
                if (bdg != null)
                {
                    bdg.StopHeal();
                    m_Caster.PrivateOverheadMessage("* parou de se curar *");
                    m_Caster.SendMessage("Voce parou de se curar por usar suas maos.");
                }
            }
            */

            var bdg = BandageContext.GetContext(m_Caster);
            if (bdg != null)
            {
                bdg.FullPower = false;
            }

            if (this.Caster.Meditating)
            {
                this.Caster.Meditating = false;
                this.Caster.SendMessage(12, "Você parou de meditar");
            }

            m_StartCastTime = Core.TickCount;

            if (Caster.RP && Caster.Mounted && Caster is PlayerMobile && !((PlayerMobile)Caster).Talentos.Tem(Talento.Hipismo))
            {
                if (Utility.RandomDouble() < 0.05)
                {
                    Caster.Mount.Rider = null;
                    Caster.SendMessage("Voce se desequilibrou da sua montaria ao tentar conjurar a magia");
                    AOS.Damage(Caster, Utility.Random(5, 10));
                    Caster.PlayHurtSound();
                    Caster.PlayDamagedAnimation();
                    Caster.Paralyze(TimeSpan.FromSeconds(1));
                    return false;
                }
            }

            /*
            if (!Shard.POL_STYLE && m_Caster.Spell is Spell && ((Spell)m_Caster.Spell).State == SpellState.Sequencing)
            {
                ((Spell)m_Caster.Spell).Disturb(DisturbType.NewCast);
            }
            */

            var item = m_Caster.FindItemOnLayer(Layer.OneHanded);
            if (item != null && !item.AllowEquipedCast(this.Caster))
            {
                if (this is MagerySpell || this is NecromancerSpell)
                {
                    if (this.Caster.Paralyzed)
                    {
                        this.Caster.SendMessage("Você não pode conjurar magias com arma nas mãos");
                        return false;
                    }

                    if (!Shard.SPHERE_STYLE)
                        m_Caster.ClearHand(item);
                    //Caster.SendMessage("Você não pode conjurar magias com arma nas mãos");
                    //return false;
                }
            }

            if (Shard.SPHERE_STYLE)
            {
                var item2 = m_Caster.FindItemOnLayer(Layer.TwoHanded);
                if (item2 != null && !item2.AllowEquipedCast(this.Caster))
                {
                    if (this is MagerySpell || this is NecromancerSpell)
                    {
                        if (this.Caster.Paralyzed)
                        {
                            this.Caster.SendMessage("Você não pode conjurar magias com arma nas mãos");
                            return false;
                        }
                        //Caster.SendMessage("Você não pode conjurar magias com arma nas mãos");
                        //return false;
                    }
                }
            }

            if (!this.ValidateCast(m_Caster))
                return false;

            if (!m_Caster.CheckAlive())
            {
                return false;
            }
            else if (m_Caster is PlayerMobile && ((PlayerMobile)m_Caster).Peaced)
            {
                m_Caster.SendLocalizedMessage(1072060); // You cannot cast a spell while calmed.
            }
            else if (!Shard.SPHERE_STYLE && m_Scroll is BaseWand && m_Caster.Spell != null && m_Caster.Spell.IsCasting)
            {
                m_Caster.SendMessage("Você não pode conjurar magias paralizado"); // You can not cast a spell while frozen.
            }
            else if (BlockedByHorrificBeast && TransformationSpellHelper.UnderTransformation(m_Caster, typeof(HorrificBeastSpell)) ||
                     (BlockedByAnimalForm && AnimalForm.UnderTransformation(m_Caster)))
            {
                m_Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
            }
            else if (!(m_Scroll is BaseWand) && !(this is DispelSpell) && !Shard.SPHERE_STYLE && (m_Caster.Paralyzed || m_Caster.Frozen))
            {
                m_Caster.SendMessage("Você não pode conjurar magias paralizado"); // You can not cast a spell while frozen.
            }
            else if (SkillHandlers.SpiritSpeak.IsInSpiritSpeak(m_Caster))
            {
                m_Caster.SendMessage("Voce nao pode fazer isto agora");
            }

            /*
            else if (BandageContext.GetContext(m_Caster) != null)
            {
                m_Caster.SendMessage("Você não consegue conjurar magias enquanto aplica bandagens");
            }
            */
            /*
            else if (Shard.POL_STYLE && BaseExplosionPotion.Throwing(m_Caster))
            {
                m_Caster.SendMessage("Você não consegue conjurar magias enquanto segura uma pocao");
            }
            */
            /*
            else if (CheckNextSpellTime && Core.TickCount - m_Caster.NextSpellTime < 0)
            {
                m_Caster.SendMessage("Voce ainda nao pode usar outra magia"); // You have not yet recovered from casting a spell.
            }
            */
            else if (m_Caster is PlayerMobile && ((PlayerMobile)m_Caster).PeacedUntil > DateTime.UtcNow)
            {
                m_Caster.SendMessage("Você não pode usar magias estando acalmado"); // You cannot cast a spell while calmed.
            }
            else if (m_Caster.Spell != null)
            {
                m_Caster.SendMessage("Voce ja esta conjurando uma magia");
            }
            else if (m_Caster.Mana >= AjustaMana(GetMana()))
            {
                if (!ConsumeReagents())
                {
                    m_Caster.SendMessage(0x22, "Faltam reagentes para a magia. Voce pode comprar reagentes no npc Mago ou planta-los."); // More reagents are needed for this spell.
                    return false;
                }
                if (Shard.SPHERE_STYLE && this.Caster is PlayerMobile)
                    return CastaMagiaSphere();
                return CastMagiaPadrao();
            }
            else
            {
                m_Caster.PrivateOverheadMessage(MessageType.Regular, 0x22, false, "Mana insuficiente, voce precisa de " + AjustaMana(GetMana()).ToString() + " mana para esta magia", m_Caster.NetState); // Insufficient mana. You must have at least ~1_MANA_REQUIREMENT~ Mana to use this spell.
            }

            return false;
        }

        public abstract void OnCast();

        #region Enhanced Client
        public bool OnCastInstantTarget()
        {
            if (InstantTarget == null)
                return false;

            Type spellType = GetType();

            var types = spellType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (types != null)
            {
                Type targetType = types.FirstOrDefault(t => t.IsSubclassOf(typeof(Server.Targeting.Target)));

                if (targetType != null)
                {
                    Target t = null;

                    try
                    {
                        t = Activator.CreateInstance(targetType, this) as Target;
                    }
                    catch
                    {
                        LogBadConstructorForInstantTarget();
                    }

                    if (t != null)
                    {
                        t.Invoke(Caster, InstantTarget);
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        public virtual void OnBeginCast()
        {
            SendCastEffect();
        }

        public virtual void SendCastEffect()
        { }

        public virtual void GetCastSkills(out double min, out double max)
        {
            min = max = 0; //Intended but not required for overriding.
        }

        public virtual bool CheckFizzle()
        {
            if (m_Scroll is BaseWand)
            {
                return true; // wand nunca peida
            }

            if (Caster is BaseCreature)
            {
                return true; // mob nunca peida
            }

            double minSkill, maxSkill;

            GetCastSkills(out minSkill, out maxSkill);

            if (DamageSkill != CastSkill)
            {
                Caster.CheckSkillMult(DamageSkill, 0.0, Caster.Skills[DamageSkill].Cap);
            }

            //if(Shard.CombateAchatado())
            if (false)
            {
                var skill = minSkill;

                Shard.Debug("Mage spell ?" + (this is MagerySpell) + " SkillNeeded " + SkillNeeded);

                if (SkillNeeded != 0)
                {
                    skill = SkillNeeded;
                }
                else
                {
                    skill = minSkill;
                }
                bool consegueCast = Caster.Skills[CastSkill].Value >= skill;
                if (!consegueCast)
                {
                    Caster.SendMessage("Você precisa de pelo menos " + (int)skill + " " + CastSkill + " para conseguir conjurar esta magia");
                }
                else
                {
                    Caster.CheckSkillMult(CastSkill, minSkill, maxSkill);
                }
                return consegueCast;
            }
            else
            {
                if (this is MagerySpell && Caster.Skills[SkillName.Magery].Value >= 100)
                {
                    var circle = ((MagerySpell)this).Circle;
                    if (circle == SpellCircle.Seventh || circle == SpellCircle.Sixth)
                    {
                        return true;
                    }
                }
                bool skillCheck = Caster.CheckSkillMult(CastSkill, minSkill, maxSkill);
                return skillCheck;
            }
        }

        public abstract int GetMana();

        public virtual int ScaleStamina(int stamina)
        {
            return (int)StrangleSpell.ScaleStamina(Caster, stamina);
        }

        public virtual int AjustaMana(int mana)
        {
            double scalar = 1.0;

            if (ManaPhasingOrb.IsInManaPhase(Caster))
            {
                ManaPhasingOrb.RemoveFromTable(Caster);
                return 0;
            }

            if (MindRotSpell.GetMindRotScalar(Caster, ref scalar))
            {
                scalar = 1.5;
            }

            if (Mysticism.PurgeMagicSpell.IsUnderCurseEffects(Caster))
            {
                scalar += .5;
            }

            // Lower Mana Cost = 40%
            /*
            int lmc = AosAttributes.GetValue(m_Caster, AosAttribute.LowerManaCost);

            if (lmc > 40)
            {
                lmc = 40;
            }

            lmc += BaseArmor.GetInherentLowerManaCost(m_Caster);

            scalar -= (double)lmc / 100;
            */

            return (int)(mana * scalar);
        }

        public virtual TimeSpan GetDisturbRecovery()
        {
            if (Core.AOS)
            {
                return TimeSpan.Zero;
            }

            double delay = 1.0 - Math.Sqrt((Core.TickCount - m_StartCastTime) / 1000.0 / GetCastDelay().TotalSeconds);

            if (delay < 0.2)
            {
                delay = 0.2;
            }

            return TimeSpan.FromSeconds(delay);
        }

        public virtual int CastRecoveryBase { get { return 6; } }
        public virtual int CastRecoveryFastScalar { get { return 1; } }
        public virtual int CastRecoveryPerSecond { get { return 4; } }
        public virtual int CastRecoveryMinimum { get { return 0; } }

        public virtual TimeSpan GetCastRecovery()
        {
            if (!Core.AOS)
            {
                return NextSpellDelay;
            }

            int fcr = AosAttributes.GetValue(m_Caster, AosAttribute.CastRecovery);

            int fcrDelay = -(CastRecoveryFastScalar * fcr);

            int delay = CastRecoveryBase + fcrDelay;

            if (delay < CastRecoveryMinimum)
            {
                delay = CastRecoveryMinimum;
            }

            return TimeSpan.FromSeconds((double)delay / CastRecoveryPerSecond);
        }

        public abstract TimeSpan CastDelayBase { get; }

        public virtual double CastDelayFastScalar { get { return 1; } }
        public virtual double CastDelaySecondsPerTick { get { return 0.25; } }
        public virtual TimeSpan CastDelayMinimum { get { return TimeSpan.FromSeconds(0.25); } }

        public virtual TimeSpan GetCastDelay()
        {
            if (m_Scroll is SpellStone)
            {
                return TimeSpan.Zero;
            }

            if (m_Scroll is BaseWand)
            {
                return CastDelayBase; // TODO: Should FC apply to wands?
            }

            // Faster casting cap of 2 (if not using the protection spell) 
            // Faster casting cap of 0 (if using the protection spell) 
            // Paladin spells are subject to a faster casting cap of 4 
            // Paladins with magery of 70.0 or above are subject to a faster casting cap of 2 
            int fcMax = 4;

            if (CastSkill == SkillName.Magery || CastSkill == SkillName.Necromancy || CastSkill == SkillName.Mysticism ||
                (CastSkill == SkillName.Chivalry && (m_Caster.Skills[SkillName.Magery].Value >= 70.0 || m_Caster.Skills[SkillName.Mysticism].Value >= 70.0)))
            {
                fcMax = 2;
            }

            int fc = AosAttributes.GetValue(m_Caster, AosAttribute.Resistence);

            if (fc > fcMax)
            {
                fc = fcMax;
            }

            if (ProtectionSpell.CastDisturbProtection.ContainsKey(m_Caster) || EodonianPotion.IsUnderEffects(m_Caster, PotionEffect.Urali))
            {
                fc = Math.Min(fcMax - 2, fc - 2);
            }

            TimeSpan baseDelay = CastDelayBase;

            TimeSpan fcDelay = TimeSpan.FromSeconds(-(CastDelayFastScalar * fc * CastDelaySecondsPerTick));

            //int delay = CastDelayBase + circleDelay + fcDelay;
            TimeSpan delay = baseDelay + fcDelay;

            if (delay < CastDelayMinimum)
            {
                delay = CastDelayMinimum;
            }

            #region Mondain's Legacy
            if (DreadHorn.IsUnderInfluence(m_Caster))
            {
                delay.Add(delay);
            }
            #endregion

            //return TimeSpan.FromSeconds( (double)delay / CastDelayPerSecond );
            return delay;
        }

        public virtual void FinishSequence()
        {
            SpellState oldState = m_State;

            m_State = SpellState.None;

            if (oldState == SpellState.Casting)
            {
                Caster.Delta(MobileDelta.Flags);
            }

            if (m_Caster.Spell == this)
            {
                m_Caster.AddLastSpell(this.GetType());
                m_Caster.Spell = null;
            }


        }

        public virtual int ComputeKarmaAward()
        {
            return 0;
        }

        public virtual bool CheckSequence()
        {
            int mana = AjustaMana(GetMana());

            if (m_Caster.Deleted || !m_Caster.Alive || m_Caster.Spell != this || m_State != SpellState.Sequencing)
            {
                DoFizzle();
            }
            else if (m_Scroll != null && !(m_Scroll is Runebook) &&
                     (m_Scroll.Amount <= 0 || m_Scroll.Deleted || m_Scroll.RootParent != m_Caster ||
                      (m_Scroll is BaseWand && (((BaseWand)m_Scroll).Charges <= 0 || m_Scroll.Parent != m_Caster))))
            {
                DoFizzle();
            }
            else if (m_Caster.Mana < mana)
            {
                m_Caster.SendMessage(0x22, "Para esta magia você precisa de " + mana.ToString() + " de mana"); // Insufficient mana for this spell.
            }
            else if (!Shard.SPHERE_STYLE && (m_Caster.Frozen || m_Caster.Paralyzed))
            {
                m_Caster.SendMessage("Você não pode conjurar congelado"); // You cannot cast a spell while frozen.
                DoFizzle();
            }
            else if (!Shard.SPHERE_STYLE && m_Caster is PlayerMobile && ((PlayerMobile)m_Caster).PeacedUntil > DateTime.UtcNow)
            {
                m_Caster.SendMessage("Você não pode usar magias pacificado"); // You cannot cast a spell while calmed.
                DoFizzle();
            }
            else if (CheckFizzle())
            {
                m_Caster.Mana -= mana;

                /*
                var custoStam = (int)Math.Floor(mana / 5d);
                if (custoStam < 1)
                    custoStam = 1;
                m_Caster.Stam -= custoStam;
                */

                if (m_Scroll is SpellStone)
                {
                    ((SpellStone)m_Scroll).Use(m_Caster);
                }

                if (m_Scroll is SpellScroll)
                {
                    m_Scroll.Consume();
                }

                else if (m_Scroll is BaseWand)
                {
                    ((BaseWand)m_Scroll).ConsumeCharge(m_Caster);
                    m_Caster.RevealingAction();
                }

                if (m_Scroll is BaseWand)
                {
                    bool m = m_Scroll.Movable;

                    m_Scroll.Movable = false;

                    if (ClearHandsOnCast)
                    {
                        m_Caster.ClearHands();
                    }

                    m_Scroll.Movable = m;
                }
                else
                {
                    if (ClearHandsOnCast)
                    {
                        // Nao da disarm qnd vc da o target da magia
                        if (m_State != SpellState.Sequencing)
                            m_Caster.ClearHands();
                    }
                }

                int karma = ComputeKarmaAward();

                if (karma != 0)
                {
                    Titles.AwardKarma(Caster, karma, true);
                }

                if (TransformationSpellHelper.UnderTransformation(m_Caster, typeof(VampiricEmbraceSpell)))
                {
                    bool garlic = false;

                    for (int i = 0; !garlic && i < m_Info.Reagents.Length; ++i)
                    {
                        garlic = (m_Info.Reagents[i] == Reagent.Garlic);
                    }

                    if (garlic)
                    {
                        m_Caster.SendLocalizedMessage("O alho te queima"); // The garlic burns you!
                        AOS.Damage(m_Caster, Utility.RandomMinMax(8, 14), 0, 0, 0, 100, 0);
                    }
                }

                return true;
            }
            else
            {
                DoFizzle();
            }

            return false;
        }

        public bool CheckBSequence(Mobile target)
        {
            return CheckBSequence(target, false);
        }

        public bool CheckBSequence(Mobile target, bool allowDead)
        {
            if (!target.Alive && !allowDead)
            {
                m_Caster.SendLocalizedMessage(501857); // This spell won't work on that!
                return false;
            }
            else if (Caster.CanBeBeneficial(target, true, allowDead) && CheckSequence())
            {
                if (ValidateBeneficial(target))
                {
                    Caster.DoBeneficial(target);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckHSequence(IDamageable target)
        {
            if (!target.Alive || (target is IDamageableItem && !((IDamageableItem)target).CanDamage))
            {
                m_Caster.SendLocalizedMessage(501857); // This spell won't work on that!
                return false;
            }
            else if (Caster.CanBeHarmful(target) && CheckSequence())
            {
                Caster.DoHarmful(target);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ValidateBeneficial(Mobile target)
        {
            if (target == null)
                return true;

            if (this is HealSpell || this is GreaterHealSpell || this is CloseWoundsSpell)
            {
                return target.Hits < target.HitsMax;
            }

            if (this is CureSpell || this is CleanseByFireSpell)
            {
                return target.Poisoned;
            }

            return true;
        }

        public virtual IEnumerable<IDamageable> AcquireIndirectTargets(IPoint3D pnt, int range, bool allies = false)
        {
            return SpellHelper.AcquireIndirectTargets(Caster, pnt, Caster.Map, range, allies);
        }

        private class AnimTimer : Timer
        {
            private readonly Spell m_Spell;

            public AnimTimer(Spell spell, int count)
                : base(TimeSpan.Zero, AnimateDelay, count)
            {
                m_Spell = spell;

                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                if (m_Spell.State != SpellState.Casting || m_Spell.m_Caster.Spell != m_Spell)
                {
                    Stop();
                    return;
                }

                if (!m_Spell.Caster.Mounted && m_Spell.m_Info.Action >= 0)
                {
                    if (m_Spell.Caster.Body.IsHuman)
                    {
                        m_Spell.Caster.Animate(m_Spell.m_Info.Action, 7, 1, true, false, 0);
                    }
                    else if (m_Spell.Caster.Player && m_Spell.Caster.Body.IsMonster)
                    {
                        m_Spell.Caster.Animate(12, 7, 1, true, false, 0);
                    }
                }
                else if (m_Spell.Caster.Mounted)
                {
                    m_Spell.Caster.Animate(24, 6, 1, true, false, 1);
                }

                if (!Running)
                {
                    m_Spell.m_AnimTimer = null;
                }
            }
        }

        private class CastTimer : Timer
        {
            private readonly Spell m_Spell;

            public CastTimer(Spell spell, TimeSpan castDelay)
                : base(castDelay)
            {
                m_Spell = spell;

                Priority = TimerPriority.TwentyFiveMS;
            }

            protected override void OnTick()
            {
                if (m_Spell == null || m_Spell.m_Caster == null)
                {
                    return;
                }
                else if (m_Spell.m_State == SpellState.Casting && m_Spell.m_Caster.Spell == m_Spell)
                {
                    m_Spell.m_State = SpellState.Sequencing;
                    m_Spell.m_CastTimer = null;
                    m_Spell.m_Caster.OnSpellCast(m_Spell);

                    m_Spell.Caster.Delta(MobileDelta.Flags);

                    if (m_Spell.m_Caster.Region != null)
                    {
                        m_Spell.m_Caster.Region.OnSpellCast(m_Spell.m_Caster, m_Spell);
                    }

                    m_Spell.m_Caster.NextSpellTime = Core.TickCount + (int)m_Spell.GetCastRecovery().TotalMilliseconds;
                    Target originalTarget = m_Spell.m_Caster.Target;

                    if (m_Spell.InstantTarget == null || !m_Spell.OnCastInstantTarget())
                    {
                        m_Spell.OnCast();
                    }

                    if (m_Spell.m_Caster.Player && m_Spell.m_Caster.Target != originalTarget && m_Spell.Caster.Target != null)
                    {
                        m_Spell.m_Caster.Target.BeginTimeout(m_Spell.m_Caster, TimeSpan.FromSeconds(12));
                    }
                    m_Spell.m_CastTimer = null;
                }
            }

            public void Tick()
            {
                OnTick();
            }
        }

        public void LogBadConstructorForInstantTarget()
        {
            try
            {
                using (System.IO.StreamWriter op = new System.IO.StreamWriter("InstantTargetErr.log", true))
                {
                    op.WriteLine("# {0}", DateTime.UtcNow);
                    op.WriteLine("Target with bad contructor args:");
                    op.WriteLine("Offending Spell: {0}", this.ToString());
                    op.WriteLine("_____");
                }
            }
            catch
            { }
        }
    }
}
