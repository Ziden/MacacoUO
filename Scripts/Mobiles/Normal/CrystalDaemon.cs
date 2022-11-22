using System;
using Server.Items;
using Server.Scripts.Custom.Items;

namespace Server.Mobiles
{
    [CorpseName("a crystal daemon corpse")]
    public class CrystalDaemon : BaseCreature
    {
        public override bool ReduceSpeedWithDamage { get { return false; } }

        public override double DisturbChance { get { return 0.2; } }

        public override bool IsSmart { get { return true; } }

        [Constructable]
        public CrystalDaemon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "demonio de cristal";
            this.Body = 0x310;
            this.Hue = 0x3E8;
            this.BaseSoundID = 0x47D;

            this.SetStr(140, 200);
            this.SetDex(120, 150);
            this.SetInt(800, 850);

            this.SetHits(500, 700);

            this.SetDamage(16, 20);

            this.SetDamageType(ResistanceType.Physical, 0);
            this.SetDamageType(ResistanceType.Cold, 40);
            this.SetDamageType(ResistanceType.Energy, 60);

            this.SetResistance(ResistanceType.Physical, 20, 40);
            this.SetResistance(ResistanceType.Fire, 0, 20);
            this.SetResistance(ResistanceType.Cold, 60, 80);
            this.SetResistance(ResistanceType.Poison, 20, 40);
            this.SetResistance(ResistanceType.Energy, 65, 75);

            this.SetSkill(SkillName.Wrestling, 60.0, 80.0);
            this.SetSkill(SkillName.Tactics, 70.0, 80.0);
            this.SetSkill(SkillName.MagicResist, 100.0, 110.0);
            this.SetSkill(SkillName.Magery, 140.0, 150.0);
            this.SetSkill(SkillName.EvalInt, 120.0, 120.0);
            this.SetSkill(SkillName.Inscribe, 150.0, 180.0);
            this.SetSkill(SkillName.Meditation, 100.0, 110.0);

            this.Fame = 15000;
            this.Karma = -15000;

            if(Utility.RandomDouble() < 0.1)
            {
                PackItem(new ValeDecoracaoRara());
            }

            if (Utility.RandomDouble() > 0.05)
            {
                PackItem(new DawnsMusicBox());
            }
            /*
            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                this.PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }
            */
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            if(!willKill && !IsCooldown("capetinha"))
            {
                SetCooldown("capetinha", TimeSpan.FromSeconds(10));

                var alvo = this.Combatant;
                if(alvo is BaseCreature)
                {
                    var bc = (BaseCreature)alvo;
                    if(bc.ControlMaster != null)
                    {
                        alvo = bc.ControlMaster;
                    }
                }

                BaseCreature mob = new Imp();
                BaseCreature mob2 = new Imp();
                mob.MoveToWorld(this.Location, this.Map);
                mob.OverheadMessage("Siiim mestre ?");
                mob.Combatant = alvo;

                this.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);

                mob2.MoveToWorld(this.Location, this.Map);
                mob2.OverheadMessage("O que deseja mestre ?");
                mob2.Combatant = alvo;
                if (this.Combatant is PlayerMobile)
                {
                    this.OverheadMessage("Elimine " + this.Combatant.Name);
                }
                Timer.DelayCall(TimeSpan.FromMinutes(1), () => {
                    if(mob != null && mob.Alive && !mob.Deleted)
                    {
                        mob.Delete();
                    }
                    if (mob2 != null && mob2.Alive && !mob2.Deleted)
                    {
                        mob2.Delete();
                    }
                });
            }
            base.OnDamage(amount, from, willKill);
            
        }

        public override void OnDeath( Container c )
        {
            base.OnDeath( c );

            if(Utility.RandomBool())
            {
                var cloth = new UncutCloth(Utility.Random(2, 2));
                cloth.Hue = 1151;
                cloth.Name = "Tecido do Coracao Gelido";
                c.DropItem(cloth);
            }
            //if ( Utility.RandomDouble() < 0.4 )
            //c.DropItem( new ScatteredCrystals() );
        }

        public CrystalDaemon(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV4, 1);
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
