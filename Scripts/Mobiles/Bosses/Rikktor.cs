using System;
using System.Collections;
using Server.Engines.CannedEvil;
using Server.Fronteira.Elementos;
using Server.Items;

namespace Server.Mobiles
{
    public class Rikktor : BaseChampion
    {
        public override bool ReduceSpeedWithDamage => false;
        public override bool IsSmart => true;
        public override bool UseSmartAI => true;

        public virtual int BonusExp => 2900;

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
                        AOS.Damage(m, 10 + Utility.Random(20), DamageType.SpellAOE);
                        m.Freeze(TimeSpan.FromSeconds(3));
                        m.OverheadMessage("* atordoado *");
                    });
                }
                glr.Free();
                i.Delete();
            }
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

        [Constructable]
        public Rikktor()
            : base(AIType.AI_Melee)
        {
            Body = 172;
            Name = "Rikktor";

            SetStr(701, 900);
            SetDex(201, 350);
            SetInt(51, 100);

            SetHits(65000);
            SetStam(99999);

            SetDamage(38, 55);

            SetDamageType(ResistanceType.Physical, 25);
            SetDamageType(ResistanceType.Fire, 50);
            SetDamageType(ResistanceType.Energy, 25);

            SetResistance(ResistanceType.Physical, 80, 90);
            SetResistance(ResistanceType.Fire, 80, 90);
            SetResistance(ResistanceType.Cold, 30, 40);
            SetResistance(ResistanceType.Poison, 80, 90);
            SetResistance(ResistanceType.Energy, 80, 90);

            SetSkill(SkillName.Anatomy, 100.0);
            SetSkill(SkillName.MagicResist, 140.2, 160.0);
            SetSkill(SkillName.Tactics, 100.0);
            SetSkill(SkillName.Wrestling, 118.4, 123.9);

            Fame = 22500;
            Karma = -22500;

            VirtualArmor = 130;

            //AddItem(new TheMostKnowledgePerson());
            SetWeaponAbility(WeaponAbility.BleedAttack);
        }

        public Rikktor(Serial serial)
            : base(serial)
        {
        }

        public override ChampionSkullType SkullType
        {
            get
            {
                return ChampionSkullType.Power;
            }
        }

        public override void AlterSpellDamageTo(Mobile to, ref int damage, ElementoPvM elemento)
        {
            base.AlterSpellDamageTo(to, ref damage, elemento);
            if (to is BaseCreature)
                damage *= 4;
        }

        public override void AlterMeleeDamageTo(Mobile to, ref int damage)
        {
            base.AlterMeleeDamageTo(to, ref damage);
            if (to is BaseCreature)
                damage *= 4;
        }

        public override Type[] UniqueList
        {
            get
            {
                return new Type[] { typeof(CrownOfTalKeesh) };
            }
        }
        public override Type[] SharedList
        {
            get
            {
                return new Type[]
                {
                    typeof(TheMostKnowledgePerson),
                    typeof(BraveKnightOfTheBritannia),
                    typeof(LieutenantOfTheBritannianRoyalGuard)
                };
            }
        }
        public override Type[] DecorativeList
        {
            get
            {
                return new Type[]
                {
                    typeof(LavaTile),
                    typeof(MonsterStatuette),
                    typeof(MonsterStatuette)
                };
            }
        }
        public override MonsterStatuetteType[] StatueTypes
        {
            get
            {
                return new MonsterStatuetteType[]
                {
                    MonsterStatuetteType.OphidianArchMage,
                    MonsterStatuetteType.OphidianWarrior
                };
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return ScaleType.All;
            }
        }
        public override int Scales
        {
            get
            {
                return 20;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV6, 4);
            AddLoot(LootPack.Gems, 30);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            GolemMecanico.JorraOuro(this.Location, this.Map, 500);
            for (var i = 0; i < 2; i++)
            {
                var r = Utility.RandomDouble();
                if (r < 0.1)
                    DistribuiPs(120);
                else if (r < 0.4)
                    DistribuiPs(115);
                else
                    DistribuiPs(110);
                SorteiaItem(Carnage.GetRandomPS(115));
                SorteiaItem(Carnage.GetRandomPS(110));
                SorteiaItem(Carnage.GetRandomPS(105));
            }

            var wind = new CuSidhe();
            wind.MoveToWorld(c.Location, c.Map);
            wind.OverheadMessage("* se transformou *");
            wind.OverheadMessage("[2H Para Domar]");
            Timer.DelayCall(TimeSpan.FromHours(2), () =>
            {
                if (this.Deleted || !this.Alive || this.ControlMaster != null || this.Map == Map.Internal)
                {
                    return;
                }
                this.Delete();
            });

            DistribuiItem(new CristalTherathan(10));
            SorteiaItem(Decos.RandomDecoRara(this));
            SorteiaItem(Decos.RandomDeco(this));
            DistribuiItem(new FragmentosAntigos());
            for (var x = 0; x < 10; x++)
            {
                SorteiaItem(BaseEssencia.RandomEssencia(10));
                SorteiaItem(ElementoUtils.GetRandomPedraSuperior(10));
            }

            var capa = new HumilityCloak();
            capa.Hue = 1911;
            capa.Name = "[BOSS] Capa do Rikktor";
            SorteiaItem(capa);


        }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            if (0.2 >= Utility.RandomDouble())
                this.Earthquake();
        }

        public void Earthquake()
        {
            Map map = this.Map;

            if (map == null)
                return;

            OverheadMessage("* treme-chao *");

            ArrayList targets = new ArrayList();

            IPooledEnumerable eable = GetMobilesInRange(17);

            foreach (Mobile m in eable)
            {
                if (m == this || !this.CanBeHarmful(m))
                    continue;

                if (m is BaseCreature && (((BaseCreature)m).Controlled || ((BaseCreature)m).Summoned || ((BaseCreature)m).Team != this.Team))
                    targets.Add(m);
                else if (m.Player)
                    targets.Add(m);
            }

            eable.Free();

            this.PlaySound(0x2F3);

            for (int i = 0; i < targets.Count; ++i)
            {
                Mobile m = (Mobile)targets[i];

                double damage = m.Hits * 0.6;

                if (damage < 10.0)
                    damage = 10.0;
                else if (damage > 75.0)
                    damage = 75.0;

                this.DoHarmful(m);

                AOS.Damage(m, this, (int)damage, 100, 0, 0, 0, 0);

                if (m.Alive && m.Body.IsHuman && !m.Mounted)
                {
                    m.Paralyze(TimeSpan.FromSeconds(0.5));
                    m.Animate(20, 7, 1, true, false, 0); // take hit
                }
            }
        }

        public override bool HasBreath { get { return true; } } // fire breath enabled
        public override int BreathFireDamage { get { return 100; } }
        public override bool AutoDispel { get { return !Controlled; } }

        public override int GetAngerSound()
        {
            return Utility.Random(0x2CE, 2);
        }

        public override int GetIdleSound()
        {
            return 0x2D2;
        }

        public override int GetAttackSound()
        {
            return Utility.Random(0x2C7, 5);
        }

        public override int GetHurtSound()
        {
            return 0x2D1;
        }

        public override int GetDeathSound()
        {
            return 0x2CC;
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
