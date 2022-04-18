using System;
using Server.Engines.Points;
using Server.Fronteira.Weeklies;
using Server.Items;
using Server.Mobiles;
using Server.Ziden.Dungeons.Goblins.Quest;

namespace Server.Engines.Quests
{
    public class Contrabando : Item
    {
        [Constructable]
        public Contrabando() : this(0x9A9)
        {
            Name = "Caixa de Contrabando";
            Stackable = false;
            Hue = TintaPreta.COR;
        }

        public Contrabando(Serial serial) : base(serial)
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

    public class QuestRPBom : BaseQuest
    {
        public override bool DoneOnce { get { return false; } }

        public override TimeSpan RestartDelay => TimeSpan.FromDays(1);

        public override object Title
        {
            get
            {
                return "Preparação para a Guerra";
            }
        }
        public override object Description
        {
            get
            {
                return "Há uma ameaça se formando a Oeste. Precisamos nos preparar! Nos chegou a notícia de que comerciantes estão contrabandeando armas e provisões para o… bom, não importa agora para quem! É nosso inimigo! Vamos interromper essas rotas comerciais e usar essas armas para nosso proveito. Você será bem recompensado. Tenha coragem! Vá e faça a coisa certa!";
            }
        }
        public override object Refuse
        {
            get
            {
                return "Ah uma pena";
            }
        }
        public override object Uncomplete
        {
            get
            {
                return "Vejo que retornou, mas ainda nao completou a missao !";
            }
        }

        public override object Complete
        {
            get
            {
                return "Muito bem ! Voce completou a missao !";
            }
        }

        public QuestRPBom()
            : base()
        {
            this.AddObjective(new ObtainObjective(typeof(Contrabando), "Caixa de Contrabando", 10));

            this.AddReward(new BaseReward(typeof(BagOfReagents), 1, "Sacola de Reagents"));
            this.AddReward(new BaseReward(typeof(Bandage), 200, "Bandagens"));
            this.AddReward(new BaseReward(typeof(GreaterHealPotion), 5, "5 Pocoes de Vida"));
            this.AddReward(new BaseReward(typeof(GreaterExplosionPotion), 3, "3 Coquetel Molotov"));
            this.AddReward(new BaseReward(typeof(Gold), 2000, "2000 Moedas de Ouro"));
        }

        public override void OnCompleted()
        {
            PointsSystem.Exp.AwardPoints(this.Owner, 1000);
            this.Owner.PlaySound(this.CompleteSound);
            SaveRP.PontosBons += 1;
            Owner.SendMessage("Pontos RP deste NPC: " + SaveRP.PontosBons);
            Owner.SendMessage("Os pontos RPs definem a historia do Shard.");
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

    public class NpcDoBem : MondainQuester
    {
        public override Type[] Quests
        {
            get
            {
                return new Type[] {
                    typeof(QuestRPBom)
        };
            }
        }

        [Constructable]
        public NpcDoBem()
            : base("Velho", "")
        {
            this.SetSkill(SkillName.Anatomy, 120.0, 120.0);
            this.SetSkill(SkillName.Parry, 120.0, 120.0);
            this.SetSkill(SkillName.Healing, 120.0, 120.0);
            this.SetSkill(SkillName.Tactics, 120.0, 120.0);
            this.SetSkill(SkillName.Swords, 120.0, 120.0);
            this.SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public NpcDoBem(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            this.Say("Voce teria tempo para uma historia ?");  // Know yourself, and you will become a true warrior.
        }

        public override void InitBody()
        {
            this.Female = false;
            this.CantWalk = true;
            this.Race = Race.Human;

            base.InitBody();
        }

        public override void InitOutfit()
        {
            switch (Utility.Random(3))
            {
                case 0:
                    SetWearable(new FancyShirt(GetRandomHue()));
                    break;
                case 1:
                    SetWearable(new Doublet(GetRandomHue()));
                    break;
                case 2:
                    SetWearable(new Shirt(GetRandomHue()));
                    break;
            }

            SetWearable(new Shoes(GetShoeHue()));
            int hairHue = GetHairHue();

            Utility.AssignRandomHair(this, hairHue);
            Utility.AssignRandomFacialHair(this, hairHue);

            if (Body == 0x191)
            {
                FacialHairItemID = 0;
            }

            if (Body == 0x191)
            {
                switch (Utility.Random(6))
                {
                    case 0:
                        SetWearable(new ShortPants(GetRandomHue()));
                        break;
                    case 1:
                    case 2:
                        SetWearable(new Kilt(GetRandomHue()));
                        break;
                    case 3:
                    case 4:
                    case 5:
                        SetWearable(new Skirt(GetRandomHue()));
                        break;
                }
            }
            else
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        SetWearable(new LongPants(GetRandomHue()));
                        break;
                    case 1:
                        SetWearable(new ShortPants(GetRandomHue()));
                        break;
                }
            }

            if (!Siege.SiegeShard)
                PackGold(100, 200);
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
