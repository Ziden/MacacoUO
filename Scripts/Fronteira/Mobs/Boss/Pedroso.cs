using System;
using Server.Items;
using System.Collections.Generic;
using Server.Services;
using Server.Network;
using Server.Misc.Custom;
using Server.Scripts.Custom.Items;

namespace Server.Mobiles
{
    [CorpseName("a vile corpse")]
    public class Pedroso : BaseCreature
    {
        public override bool ReduceSpeedWithDamage { get { return false; } }

        public override bool IsBoss => true;

        public override bool AutoDispel { get { return true; } }

        [Constructable]
        public Pedroso() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Pedroso de Drack";
            this.Body = 14;
            this.BaseSoundID = 268;

            Hue = 2500;

            SetStr(700, 700);
            SetDex(100, 100);
            SetInt(300, 350);

            SetHits(5000);
            SetStam(100, 100);

            SetDamage(20, 30);

            SetDamageType(ResistanceType.Physical, 75);
            SetDamageType(ResistanceType.Fire, 25);
            SetDamageType(ResistanceType.Energy, 50);

            SetResistance(ResistanceType.Physical, 75, 90);
            SetResistance(ResistanceType.Fire, 50, 65);
            SetResistance(ResistanceType.Cold, 45, 60);
            SetResistance(ResistanceType.Poison, 45, 60);
            SetResistance(ResistanceType.Energy, 45, 60);

            SetSkill(SkillName.EvalInt, 25);
            SetSkill(SkillName.MagicResist, 150);
            SetSkill(SkillName.Tactics, 90.1, 105.0);
            SetSkill(SkillName.Wrestling, 75, 75);
            SetSkill(SkillName.Magery, 70);
            SetSkill(SkillName.Poisoning, 50);
            SetSkill(SkillName.Parry, 100);
            SetSkill(SkillName.Anatomy, 120.0);
            SetSkill(SkillName.Healing, 120.0);
            SetSkill(SkillName.DetectHidden, 119.7, 128.9);

            Fame = 24000;
            Karma = -24000;

            VirtualArmor = 200;

            SetWeaponAbility(WeaponAbility.ConcussionBlow);
            AddItem(new Gold(1000));



            //AddItem(Carnage.GetRandomPS(110));
        }

        public override bool CanBeParagon { get { return false; } }
        public override bool Unprovokable { get { return true; } }
        public virtual double ChangeCombatant { get { return 0.5; } }
        public override bool AlwaysMurderer { get { return true; } }
        public override Poison PoisonImmune { get { return Poison.Greater; } }
        public override int TreasureMapLevel { get { return 3; } }

        public override bool OnBeforeDeath()
        {
            foreach (var e in GetLootingRights())
            {
                if (e.m_HasRight && e.m_Mobile != null)
                {


                    e.m_Mobile.AddToBackpack(Decos.RandomDeco(this));

                    e.m_Mobile.AddToBackpack(Loot.JoiaRaraRandom(this.Map));

                    e.m_Mobile.AddToBackpack(new Gold(2000));
                    e.m_Mobile.SendMessage(78, "Voce ganhou recompensas por ajudar a matar Pedroso");
                    e.m_Mobile.SendMessage(78, "As recompensas foram colocadas em sua mochila");
                }
            }

            GolemMecanico.JorraOuro(this.Location, this.Map, 150);
            return base.OnBeforeDeath();
        }

        public CraftResource getRecursoTalisman()
        {
            var random = Utility.Random(100);
            if(random < 50)
            {
                return CraftResource.Berilo;
            } else if(random < 75)
            {
                return CraftResource.Quartzo;
            } else if(random < 90)
            {
                return CraftResource.Vibranium;
            } else
            {
                return CraftResource.Adamantium;
            }
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            DistribuiItem(new CristalDoPoder() { Amount = 10 });
            SorteiaItem(new DecoRelPor());
            SorteiaItem(new Granite());
            //SorteiaItem(new Rock1Rand());
            //SorteiaItem(new Rock2Rand());
            SorteiaItem(new FragmentosAntigos());
            SorteiaItem(new FragmentosAntigos());
            SorteiaItem(new FragmentosAntigos());
            var itemRaro = new Necklace();
            itemRaro.Hue = TintaPreta.COR;
            itemRaro.Name = "[Boss][Pedroso] Colar de Pedra-Negra";
            itemRaro.Attributes.WeaponDamage = 10;
            SorteiaItem(itemRaro);
            SorteiaItem(Carnage.GetRandomPS(105));
            SorteiaItem(Carnage.GetRandomPS(105));

            if (Utility.RandomBool())
                SorteiaItem(new BronzeIngot(300));
            else
                SorteiaItem(new BronzeIngot(300));

            if (Utility.RandomBool())
            {
                //var bola = new ElementalBall(500);
                //bola.LootType = LootType.Regular;
                SorteiaItem(new AdamantiumIngot(40));
            }

            if (Utility.RandomBool())
                SorteiaItem(new QuartzoIngot(80));
            else
                SorteiaItem(new BeriloIngot(80));


            if (Utility.RandomDouble() < 0.3)
            {
                var t = new SmeltersTalisman(getRecursoTalisman());
                t.Charges = 20;
                SorteiaItem(t);
            }

            var wind = new Kirin();
            wind.MoveToWorld(c.Location, c.Map);
            wind.OverheadMessage("* se transformou *");
            wind.OverheadMessage("[2H Para Domar]");
            Timer.DelayCall(TimeSpan.FromHours(2), () =>
            {
                if (wind.Deleted || !wind.Alive || wind.ControlMaster != null || wind.Map == Map.Internal)
                {
                    return;
                }
                wind.Delete();
            });
        }

        public override void OnThink()
        {
            base.OnThink();
            var pl = Combatant as PlayerMobile;
            if (pl != null)
            {
                if (!IsCooldown("skill"))
                {
                    SetCooldown("skill", TimeSpan.FromSeconds(5));
                    Terremoto(pl);
                }

                if (Hue == 38 && !IsCooldown("nervoso") && pl.GetDistanceToSqrt(this) <= 10)
                {
                    SetCooldown("nervoso", TimeSpan.FromSeconds(10));
                    this.MovingParticles(pl, 6008, 5, 0, false, false, 9502, 4019, 0x160);
                    var dmg = 25 + Utility.Random(25);
                    AOS.Damage(pl, dmg, DamageType.Ranged);
                    this.PublicOverheadMessage(Network.MessageType.Emote, 0, false, "* joga uma pedra *");
                    BaseWeapon.AddBlood(pl, dmg);
                }
            }
        }

        private int casulos = 0;

        public void Nervoso(bool n)
        {
            if (n)
            {
                if (Hue == 2500)
                {
                    Hue = 38;
                    this.DamageMax += 10;
                    this.DamageMin += 10;
                    OverheadMessage("* nervoso *");
                }

            }
            else
            {
                if (Hue == 38)
                {
                    Hue = 2500;
                    this.DamageMax -= 10;
                    this.DamageMin -= 10;
                }
            }
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            if (from is PlayerMobile && Utility.RandomDouble() < 0.1)
            {
                var elem = new EarthElemental();
                elem.MoveToWorld(this.Location, this.Map);
                elem.Combatant = from;
            }

            if (this.Hits < this.HitsMax / 3)
            {
                Nervoso(true);
            }
            if (casulos <= 1 && this.Hits < this.HitsMax / 10)
            {
                Casulo();
            }
            base.OnDamage(amount, from, willKill);
        }

        public void Casulo()
        {
            casulos++;
            Nervoso(false);
            this.Hidden = true;
            this.Blessed = true;
            this.CanMove = false;
            var casulo = new Item(0x1772);
            casulo.Movable = false;
            casulo.Name = "pedra suspeita";
            casulo.MoveToWorld(this.Location, this.Map);
            casulo.Hue = 2500;
            Effects.SendLocationParticles(EffectItem.Create(this.Location, this.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
            new CasuloTimer(this, Point3D.Zero, null, casulo).Start();
        }

        public class CasuloTimer : Timer
        {
            private BaseCreature bixo;
            private Point3D local;
            private Map map;
            private Item i;

            public CasuloTimer(BaseCreature bixo, Point3D local, Map map, Item item) : base(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2))
            {
                this.map = map;
                this.local = local;
                this.bixo = bixo;
                i = item;
            }

            protected override void OnTick()
            {
                if (bixo.Hits < bixo.HitsMax)
                {
                    var heal = bixo.HitsMax / 10;
                    bixo.Hits += heal;
                    i.PublicOverheadMessage(MessageType.Regular, 1145, false, "+" + heal);
                }
                else
                {
                    Effects.SendLocationParticles(EffectItem.Create(i.Location, i.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
                    i.Delete();
                    bixo.Hidden = false;
                    bixo.Blessed = false;
                    bixo.CanMove = true;
                    bixo.PlayAngerSound();
                    bixo.PlayAttackAnimation();
                    bixo.OverheadMessage("pe dre GULHO !");
                    this.Stop();
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

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Gems, 40);
            AddLoot(LootPack.OldRich, 2);
        }

        public virtual int BonusExp => 500;

        public override void OnDamagedBySpell(Mobile attacker)
        {
            base.OnDamagedBySpell(attacker);

        }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);  //if it hits you it spawns vortices

        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker); //if you hit creature it spawns vortices
        }

        public Pedroso(Serial serial) : base(serial)
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
        }
    }
}
