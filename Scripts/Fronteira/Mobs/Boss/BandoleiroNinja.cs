using System;

using Server.Items;
using Server.Network;
using Server.Services;
using Server.Ziden.Items;

namespace Server.Mobiles
{
    public class CabecaAnthony : Bag
    {
        [Constructable]
        public CabecaAnthony()
        {
            Name = "Percentes de Anthony";
            Hue = 38;
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            return false;
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            return false;
        }

        public CabecaAnthony(Serial s) : base(s) { }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Quem sabe mostrando isso ao orc lider ele nao te da uma recompensa ?");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class AnthonyErtsem : BaseCreature
    {
        public override bool ClickTitle { get { return false; } }
        public override bool CanStealth { get { return true; } }
        public override bool ReduceSpeedWithDamage { get { return false; } }

        public override bool IsBoss => true;

        public virtual int BonusExp => 800;

        private DateTime m_NextWeaponChange;

        [Constructable]
        public AnthonyErtsem() : base(AIType.AI_Ninja, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            SpeechHue = Utility.RandomDyedHue();
            Hue = Utility.RandomSkinHue();
            Name = "Anthony Ertsem";

            Body = 0x190;

            SetHits(9000, 9000);

            SetStr(90, 120);
            SetDex(81, 95);
            SetInt(151, 165);
            SetStam(200);

            SetDamage(3, 11);

            SetDamageType(ResistanceType.Physical, 65);
            SetDamageType(ResistanceType.Fire, 15);
            SetDamageType(ResistanceType.Poison, 15);
            SetDamageType(ResistanceType.Energy, 5);

            SetResistance(ResistanceType.Physical, 35, 65);
            SetResistance(ResistanceType.Fire, 40, 60);
            SetResistance(ResistanceType.Cold, 25, 45);
            SetResistance(ResistanceType.Poison, 40, 60);
            SetResistance(ResistanceType.Energy, 35, 55);

            SetSkill(SkillName.Anatomy, 105.0, 120.0);
            SetSkill(SkillName.MagicResist, 0, 0);
            SetSkill(SkillName.Tactics, 115.0, 130.0);
            SetSkill(SkillName.Wrestling, 95.0, 120.0);
            SetSkill(SkillName.Fencing, 95.0, 120.0);
            SetSkill(SkillName.Macing, 95.0, 120.0);
            SetSkill(SkillName.Swords, 95.0, 120.0);

            SetSkill(SkillName.Ninjitsu, 60, 70);
            SetSkill(SkillName.Hiding, 100.0);
            SetSkill(SkillName.Stealth, 120.0);

            Item LeatherChest = new LeatherChest();
            LeatherChest.Movable = false;
            LeatherChest.Hue = TintaPreta.COR;
            AddItem(LeatherChest);

            Item LeatherArms = new LeatherArms();
            LeatherArms.Movable = false;
            LeatherArms.Hue = TintaPreta.COR;
            AddItem(LeatherArms);

            Item LeatherLegs = new LeatherLegs();
            LeatherLegs.Movable = false;
            LeatherLegs.Hue = TintaPreta.COR;
            AddItem(LeatherLegs);

            Item LeatherGloves = new LeatherGloves();
            LeatherGloves.Movable = false;
            LeatherGloves.Hue = TintaPreta.COR;
            AddItem(LeatherGloves);

            Item LeatherGorget = new LeatherGorget();
            LeatherGorget.Movable = false;
            LeatherGorget.Hue = TintaPreta.COR;
            AddItem(LeatherGorget);

            var arma = new Dagger();
            arma.Hue = TintaPreta.COR;
            arma.Name = "Adaga de Anthony";
            arma.Attributes.BonusDex = 5;
            arma.Movable = false;
            arma.Poison = Poison.Regular;
            arma.PoisonCharges = 9999;
            AddItem(arma);

            PackItem(Decos.RandomDeco(this));
            SetWeaponAbility(WeaponAbility.BleedAttack);
            SetMagicalAbility(MagicalAbility.Piercing);

            Fame = 8500;
            Karma = -8500;
            AddItem(new Gold(3000));
            Utility.AssignRandomHair(this);
        }

        public override void OnDeath(Container c)
        {
            DistribuiItem(new CristalDoPoder() { Amount = 5 });
            base.OnDeath(c);
            SorteiaItem(Carnage.GetRandomPS(105));
            SorteiaItem(Carnage.GetRandomPS(105));
            SorteiaItem(Carnage.GetRandomPS(110));
        }

        public override bool BardImmune { get { return true; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV6);
            AddLoot(LootPack.Gems, 30);
        }
        public class PotTimer : Timer
        {
            private BaseCreature m_Defender;
            private Mobile m_Target;
            private int count = 3;

            public PotTimer(BaseCreature defender, Mobile target)
                : base(TimeSpan.FromSeconds(0.65), TimeSpan.FromSeconds(0.65), 4)
            {
                m_Defender = defender;
                m_Target = target;
                Start();
            }

            protected override void OnTick()
            {
                if (m_Defender == null || m_Target == null || !m_Defender.Alive || !m_Target.Alive)
                {
                    Stop();
                    return;
                }

                m_Defender.PublicOverheadMessage(MessageType.Regular, 23, true, "" + count);
                count--;
                if (count < 0)
                {
                    Stop();
                    var distancia = m_Defender.GetDistance(m_Target.Location);

                    if (m_Defender.Paralyzed || !m_Defender.InLOS(m_Target) || distancia > 16)
                    {
                        m_Defender.OverheadMessage("* ... *");
                    }
                    else
                    {
                        Effects.SendMovingEffect(m_Defender, m_Target, 0xF0D, 12, 0, false, false, 0, 0);
                        Timer.DelayCall(TimeSpan.FromMilliseconds(500), () =>
                        {

                            if (m_Target == null || m_Target.Map == null)
                                return;

                            var dmg = 15 + Utility.Random(25);
                            m_Target.Damage(dmg);
                            DamageNumbers.ShowDamage(dmg, m_Defender, m_Target, 32);
                            Effects.PlaySound(m_Target.Location, m_Target.Map, 0x307);
                            Effects.SendLocationEffect(m_Target.Location, m_Target.Map, 0x36B0, 9, 10, 0, 0);
                            BleedAttack.BeginBleed(m_Target, m_Defender);
                            if (m_Target == null || !m_Target.Alive || m_Target.Map == Map.Internal)
                                return;

                            var alvos = m_Target.Map.GetClientsInRange(m_Target.Location, 12);
                            foreach (var alvo in alvos)
                            {
                                if (m_Target != alvo.Mobile && m_Target.InLOS(alvo.Mobile))
                                {
                                    m_Target.MovingParticles(alvo.Mobile, 0x10D3, 15, 0, false, false, 9502, 4019, 0x160);
                                    var teia = new Teia(alvo.Mobile);
                                    alvo.Mobile.SendMessage("Voce foi preso por uma teia e nao consegue se soltar. Talvez alguem consiga te soltar clicando na teia.");
                                    alvo.Mobile.OverheadMessage("* preso *");
                                    teia.MoveToWorld(alvo.Mobile.Location, alvo.Mobile.Map);
                                    alvo.Mobile.Freeze(TimeSpan.FromSeconds(20));
                                    Timer.DelayCall(TimeSpan.FromSeconds(20), () =>
                                    {
                                        if (alvo == null || alvo.Mobile == null)
                                            return;

                                        if (teia != null && !teia.Deleted)
                                            teia.Delete();
                                        alvo.Mobile.Frozen = false;
                                    });
                                }
                            }
                            alvos.Free();

                        });
                    }

                }
            }
        }

        public override void OnThink()
        {
            if (this.Combatant != null && this.Combatant is Mobile)
            {
                if (!IsCooldown("pot"))
                {
                    OverheadMessage("* pega uma pocao pegajosa *");
                    SetCooldown("pot", TimeSpan.FromSeconds(15));
                    new PotTimer(this, (Mobile)this.Combatant);
                }
            }
            base.OnThink();
        }


        public override bool OnBeforeDeath()
        {
            foreach (var e in GetLootingRights())
            {
                if (e.m_HasRight && e.m_Mobile != null)
                {
                    if(Utility.RandomDouble() < 0.1)
                        e.m_Mobile.AddToBackpack(Carnage.GetRandomPS(110));
                    else
                        e.m_Mobile.AddToBackpack(Carnage.GetRandomPS(105));

                    e.m_Mobile.AddToBackpack(new CabecaAnthony());
                    e.m_Mobile.AddToBackpack(new LivroAntigo());

                    e.m_Mobile.SendMessage(78, "Voce ganhou recompensas por ajudar a matar o boss");
                    e.m_Mobile.SendMessage(78, "As recompensas foram colocadas em sua mochila");

                }
            }
            DistribuiItem(new FragmentosAntigos());
            DistribuiItem(new FragmentosAntigos());
            GolemMecanico.JorraOuro(this.Location, this.Map, 150);
            return base.OnBeforeDeath();
        }

        public override bool AlwaysMurderer { get { return true; } }

        public AnthonyErtsem(Serial serial) : base(serial)
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
    }
}


/*
using Server.Items;
using Server.Network;
using Server.Services;
using Server.Ziden.Items;
using System;

namespace Server.Mobiles
{
    [CorpseName("a dragon wolf corpse")]
    public class AnthonyErtsem : BaseCreature
    {
        public override bool ReduceSpeedWithDamage { get { return false; } }

        public override bool CanStealth { get { return true; } }

        [Constructable]
        public AnthonyErtsem()
            : base(AIType.AI_Ninja, FightMode.Closest, 10, 1, 0.05, 0.2)
        {
            Name = "Anthony Ertsem";

            SpeechHue = Utility.RandomDyedHue();

            Body = 0x190;

            SetStr(200, 200);
            SetDex(400, 400);
            SetStam(500, 500);

            SetInt(50, 55);

            SetHits(6000, 6000);

            SetDamage(3, 12);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 45, 55);
            SetResistance(ResistanceType.Fire, 30, 40);
            SetResistance(ResistanceType.Cold, 30, 40);
            SetResistance(ResistanceType.Poison, 40, 50);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.Anatomy, 60.0, 70.0);
            SetSkill(SkillName.MagicResist, 0, 0);
            SetSkill(SkillName.Tactics, 95.0, 110.0);
            SetSkill(SkillName.Wrestling, 90.0, 105.0);
            SetSkill(SkillName.DetectHidden, 60.0);
            SetSkill(SkillName.Magery, 100, 100);
            SetSkill(SkillName.EvalInt, 100, 100);
            SetSkill(SkillName.Meditation, 100, 100);

            SetSkill(SkillName.Hiding, 120, 120);
            SetSkill(SkillName.Stealth, 120, 120);
            SetSkill(SkillName.Ninjitsu, 120, 120);
            SetSkill(SkillName.Parry, 80, 80);

            Fame = 8500;
            Karma = -8500;

            VirtualArmor = 0;

            Tamable = false;
            ControlSlots = 4;
            MinTameSkill = 102.0;

            Item LeatherChest = new LeatherChest();
            LeatherChest.Movable = false;
            LeatherChest.Hue = TintaPreta.COR;
            AddItem(LeatherChest);

            Item LeatherArms = new LeatherArms();
            LeatherArms.Movable = false;
            LeatherArms.Hue = TintaPreta.COR;
            AddItem(LeatherArms);

            Item LeatherLegs = new LeatherLegs();
            LeatherLegs.Movable = false;
            LeatherLegs.Hue = TintaPreta.COR;
            AddItem(LeatherLegs);

            Item LeatherGloves = new LeatherGloves();
            LeatherGloves.Movable = false;
            LeatherGloves.Hue = TintaPreta.COR;
            AddItem(LeatherGloves);

            Item LeatherGorget = new LeatherGorget();
            LeatherGorget.Movable = false;
            AddItem(LeatherGorget);

            var arma = new Dagger();
            arma.Name = "Adaga de Anthony";
            arma.Attributes.BonusDex = 5;
            arma.Movable = false;
            arma.Poison = Poison.Regular;
            arma.PoisonCharges = 9999;
            AddItem(arma);
            AddItem(new Buckler());
            PackItem(Decos.RandomDeco());
            SetWeaponAbility(WeaponAbility.BleedAttack);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            this.SetStam(400);
            base.OnDamage(amount, from, willKill);
        }


        public override void OnBeforeDamage(Mobile from, ref int totalDamage, DamageType type)
        {

        }

        public class PotTimer : Timer
        {
            private BaseCreature m_Defender;
            private Mobile m_Target;
            private int count = 3;

            public PotTimer(BaseCreature defender, Mobile target)
                : base(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5), 4)
            {
                m_Defender = defender;
                m_Target = target;
                Start();
            }

            protected override void OnTick()
            {
                if (m_Defender == null || m_Target == null || !m_Defender.Alive || !m_Target.Alive)
                {
                    Stop();
                    return;
                }

                m_Defender.PublicOverheadMessage(MessageType.Regular, 23, true, "" + count);
                count--;
                if (count < 0)
                {
                    Stop();
                    var distancia = m_Defender.GetDistance(m_Target.Location);

                    if (m_Defender.Paralyzed || !m_Defender.InLOS(m_Target) || distancia > 16)
                    {
                        m_Defender.OverheadMessage("* ... *");
                    }
                    else
                    {
                        Effects.SendMovingEffect(m_Defender, m_Target, 0xF0D, 12, 0, false, false, 0, 0);
                        Timer.DelayCall(TimeSpan.FromMilliseconds(500), () => {
                            var dmg = 15 + Utility.Random(25);
                            m_Target.Damage(dmg);
                            DamageNumbers.ShowDamage(dmg, m_Defender, m_Target, 32);
                            Effects.PlaySound(m_Target.Location, m_Target.Map, 0x307);
                            Effects.SendLocationEffect(m_Target.Location, m_Target.Map, 0x36B0, 9, 10, 0, 0);

                            var alvos = m_Target.Map.GetClientsInRange(m_Target.Location, 12);
                            foreach(var alvo in alvos)
                            {
                                if(m_Target != alvo.Mobile && m_Target.InLOS(alvo.Mobile))
                                {
                                    m_Target.MovingParticles(alvo.Mobile, 0x10D3, 15, 0, false, false, 9502, 4019, 0x160);
                                    var teia = new Teia(alvo.Mobile);
                                    alvo.Mobile.SendMessage("Voce foi preso por uma teia e nao consegue se soltar. Talvez alguem consiga te soltar clicando na teia.");
                                    alvo.Mobile.OverheadMessage("* preso *");
                                    teia.MoveToWorld(alvo.Mobile.Location, alvo.Mobile.Map);
                                    alvo.Mobile.Freeze(TimeSpan.FromSeconds(20));
                                    Timer.DelayCall(TimeSpan.FromSeconds(20), () =>
                                    {
                                        if(teia != null && !teia.Deleted)
                                            teia.Delete();
                                        alvo.Mobile.Frozen = false;
                                    });
                                }
                            }
                            alvos.Free();

                        });
                    }

                }
            }
        }

        public override void OnThink()
        {
            if (this.Combatant != null && this.Combatant is Mobile)
            {
                if (!IsCooldown("pot"))
                {
                    OverheadMessage("* pega uma pocao pegajosa *");
                    SetCooldown("pot", TimeSpan.FromSeconds(15));
                    new PotTimer(this, (Mobile)this.Combatant);
                }
            }
            base.OnThink();
        }

        public static Item GetRandomPS(int skill)
        {
            return new Gold(10000);
        }

        public override bool OnBeforeDeath()
        {
            foreach (var e in GetLootingRights())
            {
                if (e.m_HasRight && e.m_Mobile != null)
                {
                    var ps = GetRandomPS(105);
                    if(ps != null)
                    {
                        e.m_Mobile.AddToBackpack(new SkillBook());
                        e.m_Mobile.AddToBackpack(new LivroAntigo());
                        e.m_Mobile.AddToBackpack(ps);
                        e.m_Mobile.SendMessage(78, "Voce ganhou recompensas por ajudar a matar o boss");
                        e.m_Mobile.SendMessage(78, "As recompensas foram colocadas em sua mochila");
                    }
                }
            }

            GolemMecanico.JorraOuro(this.Location, this.Map, 150);
            return base.OnBeforeDeath();
        }

        public AnthonyErtsem(Serial serial)
            : base(serial)
        {
        }

        public override bool AlwaysMurderer { get { return true; } }

        public override void GenerateLoot()
        {;
            AddLoot(LootPack.FilthyRich, 3);
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
        }
    }
}
*/
