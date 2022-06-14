using System;
using Server;
using Server.Items;
using Server.Engines.ShameRevamped;
using Server.Engines.PartySystem;
using Server.Misc.Custom;

namespace Server.Mobiles
{
    public class ShameGuardian : BaseCreature
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public ShameAltar Altar { get; set; }

        public ShameGuardian(AIType type)
            : base(type, FightMode.Aggressor, 10, 1, .4, .2)
        {
            Name = "Boss " + Name;
            Title = "[BOSS]";
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Altar != null)
                Altar.OnGuardianKilled();

            c.PublicOverheadMessage("* dropou 1/3 da pedra elemental *");

            SorteiaItem(new CristalElemental(Utility.RandomMinMax(3, 5)));

            Altar = null;
        }

        public override int Damage(int amount, Mobile from, bool informMount, bool checkfizzle)
        {
            if (from == null)
                return 0;

            if (Altar == null || Altar.Summoner == null)
                amount = base.Damage(amount, from, informMount, checkfizzle);
            else
            {
                bool good = false;

                if (from == Altar.Summoner || (Altar.DeadLine > DateTime.UtcNow &&
                                               Altar.DeadLine - DateTime.UtcNow < TimeSpan.FromMinutes(10)))
                    good = true;
                else if (from is BaseCreature && ((BaseCreature)from).GetMaster() == Altar.Summoner)
                    good = true;
                else if (ShameAltar.AllowParties)
                {
                    var p1 = Server.Engines.PartySystem.Party.Get(from);
                    var p2 = Server.Engines.PartySystem.Party.Get(Altar.Summoner);

                    if (p2 != null && p1 != null && p2 == p1)
                        good = true;
                }

                if (good)
                    amount = base.Damage(amount, from, informMount, checkfizzle);
                else
                {
                    amount = 0;
                    from.SendLocalizedMessage("Voce nao pode bater neste monstro pois nao invocou ele"); // You did not summon this champion, so you may not attack it at this time.
                }
            }

            return amount;
        }

        public override bool AutoDispel { get { return true; } }
        public override bool AlwaysMurderer { get { return true; } }
        public override Poison PoisonImmune { get { return Poison.Lethal; } }

        public ShameGuardian(Serial serial)
            : base(serial)
        {
        }


        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a quartz elemental corpse")]
    public class QuartzElemental : ShameGuardian
    {
        public override bool ReduceSpeedWithDamage { get { return false; } }

        public override void OnThink()
        {
            base.OnThink();
            var pl = Combatant as PlayerMobile;
            if (pl != null)
            {
                if (!IsCooldown("skill"))
                {
                    SetCooldown("skill", TimeSpan.FromSeconds(6));
                    Terremoto(pl);
                }
            }
        }

        public void Terremoto(Mobile alvo)
        {
            Effects.SendMovingParticles(this, new Entity(Serial.Zero, new Point3D(this.X, this.Y, this.Z + 20), this.Map), 0x11B6, 5, 20, true, true, 0, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            var pentagrama = new BloodyPentagramAddon();
            var loc = new Point3D(alvo.Location.X - 2, alvo.Location.Y - 2, alvo.Location.Z);
            pentagrama.MoveToWorld(loc, alvo.Map);

            //pentagrama.PublicOverheadMessage(Network.MessageType.Regular, 0, true, "* Tremidao *");
            new TerremotoTimer(this, alvo.Location, alvo.Map, pentagrama).Start();
        }

        public class TerremotoTimer : Timer
        {
            private BaseCreature bixo;
            private Point3D local;
            private Map map;
            private Item i;

            public TerremotoTimer(BaseCreature bixo, Point3D local, Map map, Item item) : base(TimeSpan.FromSeconds(2))
            {
                this.map = map;
                this.local = local;
                this.bixo = bixo;
                i = item;
            }

            protected override void OnTick()
            {
                var glr = map.GetClientsInRange(local, 3);
                foreach (var netstate in glr)
                {
                    var m = netstate.Mobile;
                    m.SendMessage("Voce sente o chao tremer...");
                    bixo.DoHarmful(m);
                    m.PlaySound(0x20D);
                    Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(m.X, m.Y, m.Z + 20), m.Map), m, 0x11B6, 5, 20, true, true, 0, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                    Timer.DelayCall(TimeSpan.FromSeconds(0.6), () =>
                    {
                        AOS.Damage(m, 20 + Utility.Random(40), DamageType.SpellAOE);
                        m.Freeze(TimeSpan.FromSeconds(1));
                        m.OverheadMessage("* atordoado *");
                    });
                }
                glr.Free();
                i.Delete();
            }
        }

        [Constructable]
        public QuartzElemental()
            : base(AIType.AI_Melee)
        {
            Name = "guardiao de quartzo";
            Body = 14;
            BaseSoundID = 268;
            Hue = 778;

            SetStr(240, 260);
            SetDex(70, 80);
            SetInt(100, 110);

            SetHits(1000, 1100);

            SetDamage(14, 21);

            SetDamageType(ResistanceType.Physical, 60);
            SetDamageType(ResistanceType.Energy, 40);

            SetResistance(ResistanceType.Physical, 30, 35);
            SetResistance(ResistanceType.Fire, 20, 30);
            SetResistance(ResistanceType.Cold, 20, 30);
            SetResistance(ResistanceType.Poison, 20, 30);
            SetResistance(ResistanceType.Energy, 15, 25);

            SetSkill(SkillName.MagicResist, 110, 120);
            SetSkill(SkillName.Tactics, 100, 110.0);
            SetSkill(SkillName.Wrestling, 100, 100);

            AddItem(new QuartzoOre(30 + Utility.Random(100)));

            Fame = 4500;
            Karma = -4500;
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV6, 1);
        }

        public override void OnDeath(Container c)
        {

            base.OnDeath(c);

            if ((c.Map != null && c.Map.Rules == MapRules.FeluccaRules) || 0.5 > Utility.RandomDouble())
                c.DropItem(new QuartzGrit());
        }

        public QuartzElemental(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a flame elemental corpse")]
    public class FlameElemental : ShameGuardian
    {
        public override bool ReduceSpeedWithDamage { get { return false; } }

        public override void OnThink()
        {
            if (Combatant != null && !IsCooldown("fog"))
            {
                SetCooldown("fog", TimeSpan.FromSeconds(3));

                var jatem = false;
                var aqui = this.FindItemsInRange(this.Map, 0);
                foreach (var i in aqui)
                {
                    if (i is Spells.Fourth.FireFieldSpell.FireFieldItem)
                    {
                        jatem = true;
                        break;
                    }
                }

                if (jatem)
                {
                    aqui = Combatant.FindItemsInRange(Combatant.Map, 0);
                    foreach (var i in aqui)
                    {
                        if (i is Spells.Fourth.FireFieldSpell.FireFieldItem)
                        {
                            return;
                        }
                    }
                    var alvo = Combatant;
                    this.MovingParticles(Combatant, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
                    Timer.DelayCall(TimeSpan.FromSeconds(0.5), () =>
                    {
                        if (alvo != null && alvo.Alive)
                        {
                            new Spells.Fourth.FireFieldSpell.FireFieldItem(0x398C, alvo.Location, this, alvo.Map, TimeSpan.FromSeconds(30), 10);
                        }
                    });
                    return;
                }
                new Spells.Fourth.FireFieldSpell.FireFieldItem(0x398C, this.Location, this, this.Map, TimeSpan.FromSeconds(30), 10);
            }
        }

        [Constructable]
        public FlameElemental()
            : base(AIType.AI_Mage)
        {
            Name = "guardiao da chama";
            Body = 15;
            BaseSoundID = 838;
            Hue = 1161;

            SetStr(420, 460);
            SetDex(160, 210);
            SetInt(120, 190);

            SetHits(700, 800);
            SetMana(1000, 1300);

            SetDamage(13, 15);

            SetDamageType(ResistanceType.Physical, 25);
            SetDamageType(ResistanceType.Fire, 75);

            SetResistance(ResistanceType.Physical, 40, 60);
            SetResistance(ResistanceType.Fire, 100);
            SetResistance(ResistanceType.Cold, 30, 40);
            SetResistance(ResistanceType.Poison, 60, 70);
            SetResistance(ResistanceType.Energy, 60, 70);

            SetSkill(SkillName.MagicResist, 90, 140);
            SetSkill(SkillName.Tactics, 90, 130.0);
            SetSkill(SkillName.Wrestling, 90, 100);
            SetSkill(SkillName.Magery, 100, 145);
            SetSkill(SkillName.EvalInt, 90, 140);
            SetSkill(SkillName.Meditation, 80, 120);
            SetSkill(SkillName.Parry, 20, 30);

            Fame = 4500;
            Karma = -4500;

            PackItem(new SulfurousAsh(5));
            var cloth = new UncutCloth(Utility.Random(10, 5));
            cloth.Hue = 1161;
            cloth.Name = "Tecido Magmatico";
            AddItem(cloth);

            AddItem(new FireElementalCostume());
        }

        public override bool HasBreath { get { return true; } } // fire breath enabled
        public override bool HasAura { get { return true; } }
        public override int AuraRange { get { return 5; } }
        public override int AuraBaseDamage { get { return 5; } }
        public override int AuraFireDamage { get { return 10; } }
        public override int AuraEnergyDamage { get { return 10; } }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV6, 1);
            this.AddLoot(LootPack.Gems, 15);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if ((c.Map != null && c.Map.Rules == MapRules.FeluccaRules) || 0.5 > Utility.RandomDouble())
                c.DropItem(new CorrosiveAsh());

            if (Utility.RandomDouble() < 0.3)
                    SorteiaItem(Carnage.GetRandomPS(105));
        }

        public FlameElemental(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [CorpseName("a wind elemental corpse")]
    public class WindElemental : ShameGuardian
    {
        public override bool ReduceSpeedWithDamage { get { return false; } }

        public override void OnThink()
        {
            if (Combatant != null && !IsCooldown("fog"))
            {
                Effects.PlaySound(this, this.Map, 0x20B);
                SetCooldown("fog", TimeSpan.FromSeconds(3));
                new Spells.Sixth.ParalyzeFieldSpell.InternalItem(0x3967, this.Location, this, this.Map, TimeSpan.FromSeconds(30));
            }
        }

        [Constructable]
        public WindElemental()
            : base(AIType.AI_Mage)
        {
            Name = "guardiao do vento";
            Body = 13;
            BaseSoundID = 655;
            Hue = 33765;

            SetStr(370, 460);
            SetDex(160, 250);
            SetInt(150, 220);

            SetHits(2500, 2600);
            SetMana(1000, 1300);

            SetDamage(15, 17);

            SetDamageType(ResistanceType.Physical, 20);
            SetDamageType(ResistanceType.Cold, 40);
            SetDamageType(ResistanceType.Energy, 40);

            SetResistance(ResistanceType.Physical, 65, 75);
            SetResistance(ResistanceType.Fire, 55, 65);
            SetResistance(ResistanceType.Cold, 55, 65);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 60, 75);

            SetSkill(SkillName.MagicResist, 60, 80);
            SetSkill(SkillName.Tactics, 60, 80.0);
            SetSkill(SkillName.Wrestling, 60, 80);
            SetSkill(SkillName.Magery, 60, 80);
            SetSkill(SkillName.EvalInt, 60, 80);

            Fame = 4500;
            Karma = -4500;
            AddItem(new ElementalBall());
            SetWeaponAbility(WeaponAbility.ParalyzingBlow);
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV6, 1);
            this.AddLoot(LootPack.HighScrolls, Utility.RandomMinMax(3, 5));
            this.AddLoot(LootPack.Gems, 15);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if ((c.Map != null && c.Map.Rules == MapRules.FeluccaRules) || 0.5 > Utility.RandomDouble())
                c.DropItem(new CursedOilstone());

            if (Utility.RandomDouble() < 0.3)
                    SorteiaItem(Carnage.GetRandomPS(105));
        }

        public WindElemental(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
