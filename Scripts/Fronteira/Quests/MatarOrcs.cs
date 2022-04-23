using System;
using Server.Engines.Points;
using Server.Items;
using Server.Mobiles;
using Server.Ziden.Dungeons.Goblins.Quest;

namespace Server.Engines.Quests
{
    // CONFIGURACAO DA QUEST
    public class MatarOrcs : BaseQuest
    {
        // AQUI VC BOTA SE EH QUETS REPETIVEL OU NAO (tipo as de matar X ganhar Y XP podem ser td repetivel)
        public override bool DoneOnce { get { return false; } }

        public override object Title
        {
            get
            {
                return "Orcs Malditos";
            }
        }
        public override object Description
        {
            get
            {
                return "Ola viajante. Voce ja viu a <b>nordeste daqui</b> varios <b>orcs</b> estao causando problemas ?. Esses Orcs roubam minhas colheitas... Poderia me ajudar ? Posso lhe dar sementes e uma boa recompensa !";
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
                return "Vejo que retornou, mas ainda acho que tem mais orcs a serem mortos !";
            }
        }
        public override object Complete
        {
            get
            {
                return "Ah muito obrigado aventureiro, agora poderei plantar com mais sossego";
            }
        }

        public MatarOrcs()
            : base()
        {
            // AQUI BOTA O OBJETIVO DA QUEST
            this.AddObjective(new SlayObjective(typeof(Orc), "Orcs", 50));

            // RECOMPENSAS DA QUEST
            this.AddReward(new BaseReward(typeof(OrcishKinMask), 1, "Mascara"));
            this.AddReward(new BaseReward(typeof(Gold), 500, "500 Moedas"));
            this.AddReward(new BaseReward(typeof(CottonSeeds), 10, "10 Sementes de Algodao"));
        }

        public override void OnCompleted()
        {
            // AQUI VC BOTA QUANTO DE EXP VAI DAR A QUEST
            PointsSystem.Exp.AwardPoints(this.Owner, 300);
            this.Owner.PlaySound(this.CompleteSound);
            this.Owner.SendMessage("Completou a quest de matar orcs");
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

    //// AQUI EH A CLASSE DO NPC Q VAI DAR A QUETS ///   
    public class FazendeiroDoido : MondainQuester
    {
        /// AQUI REGISTRA QUAL QUEST ELE VAI DAR 
        public override Type[] Quests
        {
            get
            {
                return new Type[] {
                    typeof(MatarOrcs)
        };
            }
        }


        [Constructable]
        public FazendeiroDoido()
            : base("Helton", "O Fazendeiro Hermitao")
        {
            this.SetSkill(SkillName.Anatomy, 120.0, 120.0);
            this.SetSkill(SkillName.Parry, 120.0, 120.0);
            this.SetSkill(SkillName.Healing, 120.0, 120.0);
            this.SetSkill(SkillName.Tactics, 120.0, 120.0);
            this.SetSkill(SkillName.Swords, 120.0, 120.0);
            this.SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public FazendeiroDoido(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            this.Say("Por favor, me ajudem ! Orcs estao acabando com minha fazenda !");  // Know yourself, and you will become a true warrior.
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
