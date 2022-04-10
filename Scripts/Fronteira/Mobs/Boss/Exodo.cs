using System;
using Server.Items;
using System.Collections.Generic;
using Server.Ziden;
using Server.Ziden.Achievements;

namespace Server.Mobiles
{
    [CorpseName("a vile corpse")]
    public class ClockworkExodus : BaseCreature
    {
        public static int m_MinHits;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinHits
        {
            get { return m_MinHits; }
            set { m_MinHits = value; }
        }

        public static List<ClockworkExodus> Instances { get; set; }

        private static readonly Type[] m_Artifact = new Type[]
        {
        };

        private Point3D m_LastTarget;

        [Constructable]
        public ClockworkExodus() : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.3, 0.6)
        {
            Name = "Exodo";
            Body = 1248;
            BaseSoundID = 639;
            Hue = 2500;
            Female = true;

            SetStr(100, 110);
            SetDex(10, 20);
            SetInt(300, 350);

            SetHits(1000);
            SetStam(20, 30);

            SetDamage(5, 15);

            SetDamageType(ResistanceType.Physical, 75);
            SetDamageType(ResistanceType.Fire, 25);
            SetDamageType(ResistanceType.Energy, 50);

            SetResistance(ResistanceType.Physical, 75, 90);
            SetResistance(ResistanceType.Fire, 50, 65);
            SetResistance(ResistanceType.Cold, 45, 60);
            SetResistance(ResistanceType.Poison, 45, 60);
            SetResistance(ResistanceType.Energy, 45, 60);

            SetSkill(SkillName.EvalInt, 0);
            SetSkill(SkillName.MagicResist, 0);
            SetSkill(SkillName.Tactics, 90.1, 105.0);
            SetSkill(SkillName.Wrestling, 75, 75);
            SetSkill(SkillName.Magery, 70);
            SetSkill(SkillName.Poisoning, 50);
            SetSkill(SkillName.Anatomy, 120.0);
            SetSkill(SkillName.Healing, 120.0);

            Fame = 24000;
            Karma = -24000;

            VirtualArmor = 30;

            m_MinHits = Hits;

            if (Instances == null)
                Instances = new List<ClockworkExodus>();

            Instances.Add(this);

            AddItem(new ExodusChest());

            SetWeaponAbility(WeaponAbility.BleedAttack);
            AddItem(new Gold(2000));
        }

        /*
        public static void DistributeRandomArtifact(BaseCreature bc, Type[] typelist)
        {
            int random = Utility.Random(typelist.Length);
            Item item = Loot.Construct(typelist[random]);
            DistributeArtifact(DemonKnight.FindRandomPlayer(bc), item);
        }
        

        public static void DistributeArtifact(Mobile to, Item artifact)
        {
            if (to == null || artifact == null)
                return;

            Container pack = to.Backpack;

            if (pack == null || !pack.TryDropItem(to, artifact, false))
                to.BankBox.DropItem(artifact);

            to.SendLocalizedMessage(502088); // A special gift has been placed in your backpack.
		}
        */

        public static void DaItem(Mobile to, Item artifact)
        {
            if (to == null || artifact == null)
                return;

            Container pack = to.Backpack;

            if (pack == null || !pack.TryDropItem(to, artifact, false))
                to.BankBox.DropItem(artifact);

            to.SendLocalizedMessage(502088); // A special gift has been placed in your backpack.
        }

        public override bool OnBeforeDeath()
        {
            //if (Utility.RandomDouble() < 0.2)
            //    DistributeRandomArtifact(this, m_Artifact);

            bool ajudouNoob = false;

            foreach (var m in this.GetLootingRights())
            {
                if (m != null && m.m_Mobile != null && m.m_Mobile.Player)
                {
                    var pl = m.m_Mobile as PlayerMobile;
                    if (pl.Young)
                    {
                        ajudouNoob = true;
                        break;
                    }
                }
            }

            if(ajudouNoob)
            {
                foreach (var m in this.GetLootingRights())
                {
                    if (m != null && m.m_Mobile != null && m.m_Mobile.Player)
                    {
                        var pl = m.m_Mobile as PlayerMobile;
                        if (!pl.Young)
                        {
                            pl.SendMessage("Voce eh uma boa alma.");
                            pl.SendMessage("Por ter ajudado um iniciante nesta missao voce recebeu essencias magicas e pedras preciosas");
                            var ess = new EssenciaMagica();
                            ess.Amount = 50 + Utility.Random(75);
                            pl._PlaceInBackpack(ess);
                            pl._PlaceInBackpack(new SacolaJoias());
                            pl._PlaceInBackpack(new SacolaJoias());
                        }
                    }
                }
            }
            
            DaItem(DemonKnight.FindRandomPlayer(this), new ExodusChest());
            DaItem(DemonKnight.FindRandomPlayer(this), new ExodusChest());

            if (Utility.Random(3)==1)
                DaItem(DemonKnight.FindRandomPlayer(this), new DupresSword());

            Map map = Map;

            if (map != null)
            {
                for (int x = -8; x <= 8; ++x)
                {
                    for (int y = -8; y <= 8; ++y)
                    {
                        double dist = Math.Sqrt(x * x + y * y);

                        if (dist <= 8 && Utility.RandomBool())
                            new GoldTimer(map, X + x, Y + y, Location.Z).Start();
                    }
                }
            }

            if (Instances != null && Instances.Contains(this))
                Instances.Remove(this);

            return base.OnBeforeDeath();
        }

        public override bool CanBeParagon { get { return false; } }
        public override bool Unprovokable { get { return true; } }
        public virtual double ChangeCombatant { get { return 0.3; } }
        public override bool AlwaysMurderer { get { return true; } }
        public override Poison PoisonImmune { get { return Poison.Greater; } }
        public override int TreasureMapLevel { get { return 3; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.OldRich, 2);
        }

        public void SpawnVortices(Mobile target)
        {
            Map map = Map;

            if (map == null)
                return;
            
            MovingParticles(target, 0x1AF6, 5, 0, false, false, 0x816, 0, 3006, 0, 0, 0);

            DeathVortexTrap dv;

            for (int i = 0; i < 3; ++i)
            {
                dv = new DeathVortexTrap();
                dv.MoveToWorld(GetSpawnPosition(target.Location, map, 3), map);
            }

            target.SendLocalizedMessage(1152693); // The power of the Void surges around you! 

            m_LastTarget = target.Location;
        }

        public override bool HasAura { get { return true; } }
        public override TimeSpan AuraInterval { get { return TimeSpan.FromSeconds(10); } }
        public override int AuraRange { get { return 3; } }
        public override int AuraBaseDamage { get { return 3; } }
        public override int AuraEnergyDamage { get { return 5; } }

        public override void AuraEffect(Mobile m)
        {
            if (m.NetState != null)
                m.SendMessage("A aura da criatura esta te danificando"); // : The creature's aura of energy is damaging you!
        }

        public void DoSpecialAbility(Mobile target)
        {
            if(IsCooldown("v"))
            {
                return;
            }
            SetCooldown("v", TimeSpan.FromSeconds(10));
            if (target != null)
            {
                if (m_LastTarget != target.Location)
                {

                    target.SendMessage("A criatura conjura um vortice de energia em voce"); // ~1_CREATURE~ casts a deadly vortex at you!                    
                    SpawnVortices(target);
                }                
            }
        }

        public override void OnDamagedBySpell(Mobile attacker)
        {
            base.OnDamagedBySpell(attacker);

            DoSpecialAbility(attacker);
        }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);  //if it hits you it spawns vortices

            DoSpecialAbility(defender);
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker); //if you hit creature it spawns vortices

            DoSpecialAbility(attacker);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            base.OnDamage(amount, from, willKill);

            if (Hits < m_MinHits && Hits < HitsMax * 0.60)
                m_MinHits = Hits;

            if (Hits >= HitsMax * 0.75)
                m_MinHits = HitsMax;            
        }

        public ClockworkExodus(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
            writer.Write((int)m_MinHits);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_MinHits = reader.ReadInt();

            if (Instances == null)
                Instances = new List<ClockworkExodus>();

            Instances.Add(this);
        }

        public class GoldTimer : Timer
        {
            private Map m_Map;
            private int m_X, m_Y;
            private int z;

            public GoldTimer(Map map, int x, int y, int z) : base(TimeSpan.FromSeconds(Utility.RandomDouble() * 10.0))
            {
                m_Map = map;
                m_X = x;
                m_Y = y;
                this.z = z;
            }

            protected override void OnTick()
            {
                //int z = m_Map.GetAverageZ(m_X, m_Y);
                bool canFit = m_Map.CanFit(m_X, m_Y, z, z, false, false);

                for (int i = -3; !canFit && i <= 3; ++i)
                {
                    canFit = m_Map.CanFit(m_X, m_Y, z + i, z, false, false);

                    if (canFit)
                        z += i;
                }

                if (!canFit)
                    return;

                Gold g = new Gold(100, 150);
                g.MoveToWorld(new Point3D(m_X, m_Y, z), m_Map);

                if (0.3 >= Utility.RandomDouble())
                {                    
                    Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
                    Effects.PlaySound(g, g.Map, 0x208);
                }
            }
        }
    }
}
