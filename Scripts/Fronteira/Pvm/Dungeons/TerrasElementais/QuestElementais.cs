using System;
using Server.Engines.Points;
using Server.Items;
using Server.Mobiles;
using Server.Ziden.Dungeons.Esgoto;
using Server.Ziden.Dungeons.Goblins.Quest;
using Server.Ziden.Items;

namespace Server.Engines.Quests
{
    public class QuestElementais : BaseQuest
    {
        public override bool DoneOnce { get { return false; } }

        public override object Title
        {
            get
            {
                return "Cozinhando com os Elementos";
            }
        }
        public override object Description
        {
            get
            {
                return @"Da guerra infinita, cozinha cozinha. Os trolls do muro, cozinha cozinha. O sabio da terra, cozinha cozinha.
<br>- O homem se volta para voce.
<br> 'Voce...vai na caverna vai ?
<br> - Ele apoia uma colher de madeira gigante no caldeirao.
<br> Poderia pegar alguns mat...ingredientes, para minha... canja de galinha ? Eu tenho .... tenho ... moedas sim ! Muitas moedas !
<br> - O homem levanta a mao vazia. Ao desviar o olhar para a face do homem e a para a mao dele novamente, existem agora algumas moedas na mao do homem.
<br> Preciso de... carvao, para o fogo, uma pedra de potassio para o gosto, po preto para a cor e o toque final, salpiro !
<br> Me traga 50 de...... ";

            }
        }
        public override object Refuse
        {
            get
            {
                return "Da guerra infinita, cozinha cozinha. Dos trolls do muro, cozinha cozinha. Do sabio da terra, cozinha cozinha";
            }
        }
        public override object Uncomplete
        {
            get
            {
                return "Da guerra infinita, cozinha cozinha. Dos trolls do muro, cozinha cozinha. Do sabio da terra, cozinha cozinha. Ja tem meus ingredientes ?";
            }
        }

        public override object Complete
        {
            get
            {
                return @"- O homem pega os ingredientes";
            }
        }

        public QuestElementais()
            : base()
        {
            this.AddObjective(new ObtainObjective(typeof(Saltpeter), "Salpiro", 10));
            this.AddObjective(new ObtainObjective(typeof(Potash), "Potassa", 10));
            this.AddObjective(new ObtainObjective(typeof(Charcoal), "Carvao", 10));
            this.AddObjective(new ObtainObjective(typeof(BlackPowder), "Po Preto", 10));

            this.AddReward(new BaseReward(typeof(Gold), 2000, "5000 Moedas de Ouro"));
            this.AddReward(new BaseReward(typeof(CaixaDeGold), 1, "Caixa Totalmente Convencional"));
            this.AddReward(new BaseReward(typeof(CombatSkillBook), 1, "Livro de Combate"));
            this.AddReward(new BaseReward(typeof(LivroAntigo), 1, "Arma Aleatoria"));
        }

        public override void OnCompleted()
        {
            BaseWeapon arma = null;
            if (Owner.Skills.Swords.Value > 50)
                arma = new Katana();
            else if (Owner.Skills.Fencing.Value > 50)
                arma = new Spear();
            else if (Owner.Skills.Macing.Value > 50)
                arma = new WarHammer();
            else
                arma = Loot.RandomWeapon();
            arma.Resource = CraftResource.Lazurita;
            arma.Quality = ItemQuality.Exceptional;
            Owner.AddToBackpack(arma);
            PointsSystem.Exp.AwardPoints(this.Owner, 500);
            this.Owner.PlaySound(this.CompleteSound);
        }

        public override void OnAccept()
        {
            base.OnAccept();
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

    public class HomemCozinhando : MondainQuester
    {
        public override Type[] Quests
        {
            get
            {
                return new Type[] {
                    typeof(QuestElementais),
                    //typeof(SapatoLindoQ2)
                };
            }
        }


        [Constructable]
        public HomemCozinhando()
            : base("Homem da Cozinha", "")
        {
            this.SetSkill(SkillName.Anatomy, 120.0, 120.0);
            this.SetSkill(SkillName.Parry, 120.0, 120.0);
            this.SetSkill(SkillName.Healing, 120.0, 120.0);
            this.SetSkill(SkillName.Tactics, 120.0, 120.0);
            this.SetSkill(SkillName.Swords, 120.0, 120.0);
            this.SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public HomemCozinhando(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            this.Say("Da guerra infinita, cozinha cozinha. Dos trolls do muro, cozinha cozinha. Do sabio da terra, cozinha cozinha");  // Know yourself, and you will become a true warrior.
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
