
using System;
using Server.Items;

namespace Server.Mobiles
{

    public class OgroSuspeito : BaseOgre
    {
        [Constructable]
        public OgroSuspeito()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "ogro suspeito";
            this.Body = 1;
            this.BaseSoundID = 427;

            this.SetStr(166, 195);
            this.SetDex(90, 120);
            this.SetInt(46, 70);

            this.SetHits(100, 150);
            this.SetMana(0);

            this.SetDamage(9, 18);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 30, 35);
            this.SetResistance(ResistanceType.Fire, 15, 25);
            this.SetResistance(ResistanceType.Cold, 15, 25);
            this.SetResistance(ResistanceType.Poison, 15, 25);
            this.SetResistance(ResistanceType.Energy, 25);

            this.SetSkill(SkillName.MagicResist, 55.1, 70.0);
            this.SetSkill(SkillName.Tactics, 60.1, 70.0);
            this.SetSkill(SkillName.Wrestling, 70.1, 80.0);

            this.Fame = 500;
            this.Karma = -500;

            this.VirtualArmor = 0;
        }

        public OgroSuspeito(Serial serial)
            : base(serial)
        {
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            Timer.DelayCall(TimeSpan.FromSeconds(1), () => {
                if (c == null)
                    return;
                c.PublicOverheadMessage(Network.MessageType.Regular, 32, true, "* se meche *");
            });

            Timer.DelayCall(TimeSpan.FromSeconds(2), () => {
                if (c == null)
                    return;
                c.PublicOverheadMessage(Network.MessageType.Regular, 32, true, "* se meche mais *");
            });

            Timer.DelayCall(TimeSpan.FromSeconds(3), () => {
                if (c == null)
                    return;
                c.PublicOverheadMessage(Network.MessageType.Regular, 32, true, "* sinistro *");
            });

            Timer.DelayCall(TimeSpan.FromSeconds(5), () => {
                if (c == null)
                    return;

                c.PublicOverheadMessage(Network.MessageType.Regular, 32, true, "* se transforma *");
                FadaMa rm = new FadaMa();
                rm.Team = this.Team;
                rm.Combatant = this.Combatant;


                rm.MoveToWorld(c.Location, c.Map);
                Effects.PlaySound(c, c.Map, 0x208);
                Effects.SendLocationEffect(c.Location, c.Map, 0x3709, 30, 10, 542, 0);

                var from = rm;
                Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
                Effects.PlaySound(from.Location, from.Map, 0x243);

                Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 542, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 542, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 542, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

                Effects.SendTargetParticles(from, 0x375A, 35, 90, 542, 0x00, 9502, (EffectLayer)255, 0x100);
                c.Delete();
                rm.OverheadMessage("Xiii mioou");
            });
        }

        public override void OnThink()
        {
            if (this.Combatant != null)
            {
                if (!this.IsCooldown("act"))
                {
                    this.SetCooldown("act", TimeSpan.FromSeconds(10));
                    this.OverheadMessage(falas[Utility.Random(falas.Length)]);
                }
            }
        }

        private static string[] falas = new string[]
        {
            "Ow, eu sou soh um ogro, me deixe", "Apenas um ogro aqui",
            "Graw tenha medo do ogro", "Sou um ogro", "Voce sabe falar ogres ? Eu sei pq eu sou um ogro",
            "Mim Ogro",
        };

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
                return 2;
            }
        }
        public override void GenerateLoot()
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
