using System;
using Server.Engines.CannedEvil;
using Server.Fronteira.Elementos;
using Server.Items;
using Server.Spells.Fifth;
using Server.Spells.Seventh;

namespace Server.Mobiles
{
    public class Barracoon : BaseChampion
    {
        public override bool IsBoss => true;
        public override bool ReduceSpeedWithDamage => false;
        public override bool IsSmart => true;
        public override bool UseSmartAI => true;

        [Constructable]
        public Barracoon()
            : base(AIType.AI_Melee)
        {
            Name = "Barracoon";
            Title = "o cachimbeiro";
            Body = 0x190;
            Hue = 0x83EC;

            SetStr(283, 425);
            SetDex(72, 150);
            SetInt(505, 750);

            SetHits(82000);
            SetStam(102, 300);
            SetMana(505, 750);

            SetDamage(29, 38);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 65, 75);
            SetResistance(ResistanceType.Fire, 70, 80);
            SetResistance(ResistanceType.Cold, 65, 80);
            SetResistance(ResistanceType.Poison, 70, 75);
            SetResistance(ResistanceType.Energy, 70, 80);

            SetSkill(SkillName.Hiding, 100.0);
            SetSkill(SkillName.Stealth, 100.0);
            SetSkill(SkillName.MagicResist, 100.0);
            SetSkill(SkillName.Tactics, 118.3, 120.2);
            SetSkill(SkillName.Wrestling, 118.4, 122.7);

            Fame = 22500;
            Karma = -22500;

            VirtualArmor = 70;

            AddItem(new FancyShirt(Utility.RandomGreenHue()));
            AddItem(new LongPants(Utility.RandomYellowHue()));
            AddItem(new JesterHat(Utility.RandomPinkHue()));
            AddItem(new Cloak(Utility.RandomPinkHue()));
            AddItem(new Sandals());

            HairItemID = 0x203B; // Short Hair
            HairHue = 0x94;

            m_SpecialSlayerMechanics = true;
        }


        public Barracoon(Serial serial)
            : base(serial)
        {
        }

        public override ChampionSkullType SkullType
        {
            get
            {
                return ChampionSkullType.Greed;
            }
        }
        public override Type[] UniqueList
        {
            get
            {
                return new Type[] { };
            }
        }

        public override bool CanGivePowerscrolls { get { return false; } }

        public override Type[] SharedList
        {
            get
            {
                return new Type[]
                {

                };
            }
        }
        public override Type[] DecorativeList
        {
            get
            {
                return new Type[] { typeof(SwampTile), typeof(MonsterStatuette) };
            }
        }
        public override MonsterStatuetteType[] StatueTypes
        {
            get
            {
                return new MonsterStatuetteType[] { MonsterStatuetteType.Slime };
            }
        }
        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return true;
            }
        }
        public override double AutoDispelChance
        {
            get
            {
                return 1.0;
            }
        }
        public override bool BardImmune
        {
            get
            {
                return false;
            }
        }
		public override bool AllureImmune
		{
			get
			{
				return true;
			}
		}
        public override bool Unprovokable
        {
            get
            {
                return false;
            }
        }
        public override bool Uncalmable
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override bool ShowFameTitle
        {
            get
            {
                return false;
            }
        }
        public override bool ClickTitle
        {
            get
            {
                return false;
            }
        }

        public override bool ForceStayHome { get { return true; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV7, 1);
            AddLoot(LootPack.Gems, 40);
            //AddItem(new WindrunnerStatue());
            //Carnage.GetRandomPS(110);
        }

        public void Polymorph(Mobile m)
        {
            if (!m.CanBeginAction(typeof(PolymorphSpell)) || !m.CanBeginAction(typeof(IncognitoSpell)) || m.IsBodyMod)
                return;

            IMount mount = m.Mount;

            if (mount != null)
                mount.Rider = null;

            if (m.Flying)
                m.ToggleFlying();

            if (m.Mounted)
                return;

            if (m.BeginAction(typeof(PolymorphSpell)))
            {
                m.BodyMod = 42;
                m.HueMod = 0;
                if (m == this) {
                    m_SlayerVulnerabilities.Add("Vermin");
                    m_SlayerVulnerabilities.Add("Repond");
                }

                new ExpirePolymorphTimer(m).Start();
            }
        }

        public override int BonusExp => 1800;

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            DistribuiItem(new T2ARecallRune());
            DistribuiItem(new DirtPatch());
            SorteiaItem(new GrapeVine());
            SorteiaItem(new WhiteHangingLantern());
            //DistribuiItem(Decos.RandomDecoRara(this));
            SorteiaItem(Decos.RandomDecoRara(this));

            DistribuiItem(new FragmentosAntigos());
            DistribuiItem(new FragmentosAntigos());
            DistribuiItem(new CristalTherathan(10));

            switch (Utility.Random(5))
            {
                case 0: SorteiaItem(new GrayBrickFireplaceEastDeedExp()); break;
                case 1: SorteiaItem(new SandstoneFireplaceEastDeedExp()); break;
                case 2: SorteiaItem(new GreenTeaCauldronDeed()); break;
                case 3: SorteiaItem(new MushroomGardenEastDeed()); break;
                case 4: SorteiaItem(new SmallPersonalGardenFieldAddonDeed()); break;
            }

            for(var i =0; i < 3; i++)
            {

                SorteiaItem(new CristalDoPoder() { Amount = 5 });

                if (Utility.RandomDouble() < 0.5)
                    SorteiaItem(Carnage.GetRandomPS(110));
                else
                    SorteiaItem(Carnage.GetRandomPS(105));
                SorteiaItem(Carnage.GetRandomPS(110));
                if (Utility.RandomDouble() < 0.1)
                    SorteiaItem(Carnage.GetRandomPS(115));
            }

            for (var x = 0; x < 10; x++)
            {
                SorteiaItem(BaseEssencia.RandomEssencia(7));
                SorteiaItem(ElementoUtils.GetRandomPedraSuperior(7));
            }
            /*var wind = new Windrunner();
            wind.MoveToWorld(c.Location, c.Map);
            wind.OverheadMessage("* se transformou *");
            wind.OverheadMessage("[2H Para Domar]");*/
            Timer.DelayCall(TimeSpan.FromHours(2), () =>
            {
                if(this.Deleted || !this.Alive || this.ControlMaster != null || this.Map==Map.Internal)
                {
                    return;
                }
                this.Delete();
            });
        }

        public void SpawnRatmen(Mobile target)
        {
            Map map = Map;

            if (map == null)
                return;

            int rats = 0;

            IPooledEnumerable eable = GetMobilesInRange(10);

            foreach (Mobile m in eable)
            {
                if (m is Ratman || m is RatmanArcher || m is RatmanMage)
                    ++rats;
            }

            eable.Free();

            if (rats < 16)
            {
                PlaySound(0x3D);

                int newRats = Utility.RandomMinMax(3, 6);

                for (int i = 0; i < newRats; ++i)
                {
                    BaseCreature rat;

                    switch ( Utility.Random(5) )
                    {
                        default:
                        case 0:
                        case 1:
                            rat = new Ratman();
                            break;
                        case 2:
                        case 3:
                            rat = new RatmanArcher();
                            break;
                        case 4:
                            rat = new RatmanMage();
                            break;
                    }

                    rat.Team = Team;

                    bool validLocation = false;
                    Point3D loc = target.Location;

                    for (int j = 0; !validLocation && j < 10; ++j)
                    {
                        int x = X + Utility.Random(3) - 1;
                        int y = Y + Utility.Random(3) - 1;
                        int z = map.GetAverageZ(x, y);

                        if (validLocation = map.CanFit(x, y, Z, 16, false, false))
                            loc = new Point3D(x, y, Z);
                        else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                            loc = new Point3D(x, y, z);
                    }

                    rat.IsChampionSpawn = true;
                    rat.MoveToWorld(loc, map);
                    rat.Combatant = target;
                }
            }
        }

        public void DoSpecialAbility(Mobile target)
        {
            if (target == null || target.Deleted) //sanity
                return;

            if (target.Player && 0.6 >= Utility.RandomDouble()) // 60% chance to polymorph attacker into a ratman
                Polymorph(target);

            if (0.1 >= Utility.RandomDouble()) // 10% chance to more ratmen
                SpawnRatmen(target);

            if (0.05 >= Utility.RandomDouble() && !IsBodyMod) // 5% chance to polymorph into a ratman
                Polymorph(this);
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            DoSpecialAbility(attacker);
        }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            DoSpecialAbility(defender);
        }

        public override void OnDamagedBySpell(Mobile from)
        {
            base.OnDamagedBySpell(from);

            DoSpecialAbility(from);
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

            m_SlayerVulnerabilities.Clear();
        }

        private class ExpirePolymorphTimer : Timer
        {
            private Mobile m_Owner;
            public ExpirePolymorphTimer(Mobile owner)
                : base(TimeSpan.FromMinutes(3.0)) //3.0
            {
                m_Owner = owner;

                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                if (!m_Owner.CanBeginAction(typeof(PolymorphSpell)))
                {
                    m_Owner.BodyMod = 0;
                    m_Owner.HueMod = -1;
                    m_Owner.EndAction(typeof(PolymorphSpell));
                    if (m_Owner.SlayerVulnerabilities != null)
                    {
                        m_Owner.SlayerVulnerabilities.Remove("Vermin");
                        m_Owner.SlayerVulnerabilities.Remove("Repond");
                    }
                }
            }
        }
    }
}
