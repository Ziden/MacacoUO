using Server.Items;
using System;

namespace Server.Mobiles
{
    [CorpseName("an ettins corpse")]
    public class Ettin : BaseCreature
    {
        public override int BonusExp => 10;

        [Constructable]
        public Ettin()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "ogrologro";
            this.Body = 18;
            this.BaseSoundID = 367;

            this.SetStr(136, 165);
            this.SetDex(56, 75);
            this.SetInt(31, 55);

            this.SetHits(300, 450);

            this.SetDamage(5, 20);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 35, 40);
            this.SetResistance(ResistanceType.Fire, 15, 25);
            this.SetResistance(ResistanceType.Cold, 40, 50);
            this.SetResistance(ResistanceType.Poison, 15, 25);
            this.SetResistance(ResistanceType.Energy, 15, 25);

            SetSkill(SkillName.Parry, 50);
            this.SetSkill(SkillName.MagicResist, 90, 90);
            this.SetSkill(SkillName.Tactics, 100, 100);
            this.SetSkill(SkillName.Wrestling, 70, 100);

            this.Fame = 3000;
            this.Karma = -3000;

            if (Utility.Random(100) == 1)
            {
                PackItem(Decos.RandomDeco(this));
            }

            Imp.Converte(this);

            this.VirtualArmor = 60;
        }

        public void JogaPedra(Mobile target, bool cd)
        {
            if (!IsCooldown("pedra"))
            {
                SetCooldown("pedra", TimeSpan.FromSeconds(5));
                new PedraTimer(this, target).Start();
                PublicOverheadMessage(Network.MessageType.Emote, 0, false, "* as cabecas estao discutindo sobre a magia *");
            }
        }

        public override void OnDamagedBySpell(Mobile from)
        {
            base.OnDamagedBySpell(from);

            JogaPedra(from, true);
        }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            Orc.TentaAtacarMaster(this, defender);
            if (Utility.Random(0, 3) == 1)
            {
                if (!IsCooldown("chute"))
                {
                    this.PlayAttackAnimation();
                    this.PlaySound(0x13C);
                    SetCooldown("chute", TimeSpan.FromSeconds(5));
                    this.OverheadMessage("* Marretada Epica *");
                    new ChuteTimer(this, defender).Start();
                }
            }
        }

        public class ChuteTimer : Timer
        {
            private BaseCreature m_Defender;
            private Mobile player;
            private Direction dir;
            private int tick;

            public ChuteTimer(BaseCreature defender, Mobile player)
                : base(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100), 7)
            {
                this.dir = defender.Direction;
                this.m_Defender = defender;
                this.player = player;
            }

            public static Point3D GetPoint(Mobile m, Direction d)
            {
                var loc = m.Location.Clone3D();
                var x = 0;
                var y = 0;
                switch (d & Direction.Mask)
                {
                    case Direction.North:
                        --y;
                        break;
                    case Direction.Right:
                        ++x;
                        --y;
                        break;
                    case Direction.East:
                        ++x;
                        break;
                    case Direction.Down:
                        ++x;
                        ++y;
                        break;
                    case Direction.South:
                        ++y;
                        break;
                    case Direction.Left:
                        --x;
                        ++y;
                        break;
                    case Direction.West:
                        --x;
                        break;
                    case Direction.Up:
                        --x;
                        --y;
                        break;
                }
                loc.X += x;
                loc.Y += y;
                return loc; 
            }

            protected override void OnTick()
            {
                if (this.player == null || this.m_Defender == null)
                    return;

                if (this.m_Defender.Map == null || !this.m_Defender.Alive)
                    return;

                if (this.player.Map == null || !this.player.Alive)
                    return;

                int z = 0;
                if(this.player.CheckMovement(this.dir, out z))
                {
                    this.player.MoveToWorld(GetPoint(this.player, this.dir), this.player.Map);
                } else
                {
                    this.player.PlaySound(0x13C);
                    this.player.OverheadMessage("* Ouch! *");
                    var b = new Rectangle2D(this.player.X - 1, this.player.Y - 1, 3, 3);
                    for(var i = 0; i < 3; i ++)
                        BaseWeapon.AddBlood(this.player, this.player.Map.GetRandomSpawnPoint(b), this.player.Map);
                    this.player.FixedParticles(6008, 9, 1, 1, 0, 1, EffectLayer.Head, 0);
                    AOS.Damage(this.player, Utility.Random(20, 30));
                    this.player.SendMessage("Voce bateu seu corpo contra a parede");
                    this.Stop();
                }
                this.player.Move(this.dir, true);
            }
        }

        public Ettin(Serial serial)
            : base(serial)
        {
        }

        public class PedraTimer : Timer
        {
            private BaseCreature m_Defender;
            private Mobile player;

            public PedraTimer(BaseCreature defender, Mobile player)
                : base(TimeSpan.FromSeconds(Utility.Random(4) + 2))
            {
                m_Defender = defender;
                this.player = player;
                Start();
            }

            protected override void OnTick()
            {
                m_Defender.MovingParticles(player, 6008, 5, 0, false, false, 9502, 4019, 0x160);
                var dmg = 25 + Utility.Random(25);
                player.Damage(dmg, m_Defender);

                m_Defender.PublicOverheadMessage(Network.MessageType.Emote, 0, false, "* joga uma pedra *");
                BaseWeapon.AddBlood(player, dmg);
            }
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            base.OnDamage(amount, from, willKill);

            if (Utility.Random(20) == 1)
            {
                PublicOverheadMessage(Network.MessageType.Emote, 0, false, "* as cabecas estao discutindo *");
            }
        }

        public override bool CanProvoke { get { return false; } }
        public override bool CanPeace { get { return false; } }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override int Meat
        {
            get
            {
                return 4;
            }
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV2);
            this.AddLoot(LootPack.LV3, 2);
            this.AddLoot(LootPack.Potions);
            AddPackedLoot(LootPack.MeagerProvisions, typeof(Bag));
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
