using System;
using Server.Factions;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("a wisp corpse")]
    public class FadaMa : BaseCreature
    {

        public override bool CanBeParagon { get { return false; } }

        [Constructable]
        public FadaMa()
            : base(AIType.AI_Runner, FightMode.Aggressor, 10, 1, 0.1, 0.1)
        {
            this.Name = "fada ma";
            this.Body = 58;
            this.BaseSoundID = 466;
            this.Hue = TintaPreta.COR;

            this.SetStr(196, 225);
            this.SetDex(196, 225);
            this.SetInt(196, 225);

            this.SetHits(150, 150);

            this.SetDamage(5, 7);

            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Energy, 50);

            this.SetResistance(ResistanceType.Physical, 35, 45);
            this.SetResistance(ResistanceType.Fire, 20, 40);
            this.SetResistance(ResistanceType.Cold, 10, 30);
            this.SetResistance(ResistanceType.Poison, 5, 10);
            this.SetResistance(ResistanceType.Energy, 50, 70);

            this.SetSkill(SkillName.EvalInt, 80.0);
            this.SetSkill(SkillName.Magery, 80.0);
            this.SetSkill(SkillName.MagicResist, 80.0);
            this.SetSkill(SkillName.Tactics, 80.0);
            this.SetSkill(SkillName.Wrestling, 80.0);

            this.Fame = 200;
            this.Karma = -200;

            this.VirtualArmor = 0;

            if (Utility.RandomDouble() < .33)
                this.PackItem(Engines.Plants.Seed.RandomPeculiarSeed(4));

            this.AddItem(new LightSource());
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {

            if (!IsCooldown("fala"))
            {
                SetCooldown("fala", TimeSpan.FromSeconds(40));
                Say("Ai essa doeu !!!");
            }
        }

        public override void OnThink()
        {
            base.OnThink();
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
            "Sai sai sai daqui !", "Me deixa !",
            "Aaah que raiva", "Como vc me achou ?", "Para de me apurrinhar !",
            "Nhaaaaa",
        };


        public FadaMa(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get
            {
                return InhumanSpeech.Wisp;
            }
        }
     
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV4);
            this.AddLoot(LootPack.LV3);
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
