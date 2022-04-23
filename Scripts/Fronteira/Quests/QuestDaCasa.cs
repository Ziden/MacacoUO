using System;
using System.Collections.Generic;
using System.Linq;
using Server.Accounting;
using Server.Engines.Points;
using Server.Items;
using Server.Mobiles;
using Server.Multis.Deeds;
using Server.Ziden.Dungeons.Goblins.Quest;

namespace Server.Engines.Quests
{
    // CONFIGURACAO DA QUEST
    public class QuestCasa : BaseQuest
    {
        // AQUI VC BOTA SE EH QUETS REPETIVEL OU NAO (tipo as de matar X ganhar Y XP podem ser td repetivel)
        public override bool DoneOnce { get { return true; } }

        public override object Title
        {
            get
            {
                return "Dungeon da Casa Propria";
            }
        }
        public override object Description
        {
            get
            {
                return "Ola viajante. Sou um construtor, estou querendo construir minha casa, no fundo da dungeon de Deceit. Muito mais dificil para ladinos invadirem minha casa, se ela ficar no fundo da dungeon. Eu sou um genio. </br></br> Tem apenas um problema... a dungeon esta muito cheia de monstros ! Poderia me ajudar a limpa-los para eu construir minha casa ? Posso lhe ajudar com sua casa em troca !";
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
                return "Vejo que retornou, mas nao terminou a missao !!";
            }
        }
        public override object Complete
        {
            get
            {
                return "Ah muito obrigado aventureiro.";
            }
        }

        public static HashSet<string> pegaram = new HashSet<string>();

        public QuestCasa()
            : base()
        {
            // AQUI BOTA O OBJETIVO DA QUEST
            this.AddObjective(new SlayObjective(typeof(Skeleton), "Esqueleto", 10));
            this.AddObjective(new SlayObjective(typeof(Zombie), "Zumbi", 10));
            this.AddObjective(new SlayObjective(typeof(Shade), "Sombra", 5));
            this.AddObjective(new SlayObjective(typeof(Spectre), "Espectro", 5));
            this.AddObjective(new SlayObjective(typeof(Mummy), "Mumia", 5));

            // RECOMPENSAS DA QUEST
            this.AddReward(new BaseReward(typeof(SmallBedSouthDeed), 1, "Cama"));
            this.AddReward(new BaseReward(typeof(Armario), 1, "Armario"));
            this.AddReward(new BaseReward(typeof(WoodenChest), 1, "Bau de Madeira"));
            this.AddReward(new BaseReward(typeof(Drawer), 1, "Escrivaninha"));
            this.AddReward(new BaseReward(typeof(WoodenTableEastDeed), 1, "Mesa"));
            this.AddReward(new BaseReward(typeof(BambooChair), 1, "Cadeira"));
        }

        public override void OnCompleted()
        {
            PointsSystem.Exp.AwardPoints(this.Owner, 500);
            this.Owner.PlaySound(this.CompleteSound);
            var conta = Owner.Account as Account;
            var pl = Owner as PlayerMobile;
            if (pl != null && pl.Wisp != null)
            {
                pl.Wisp.QuestCasa();
            }
            if (conta.GetSharedAccounts().Any(c => c.Casa))
                return;

            if (this.Owner.Young && conta != null && !conta.Casa && !pegaram.Contains(Owner.NetState.Address.ToString()))
            {
                pegaram.Add(Owner.NetState.Address.ToString());
                conta.Casa = true;
                switch(Utility.Random(5))
                {
                    case 0: this.Owner.AddToBackpack(new ThatchedRoofCottageDeed()); break;
                    case 1: this.Owner.AddToBackpack(new StonePlasterHouseDeed()); break;
                    case 2: this.Owner.AddToBackpack(new FieldStoneHouseDeed()); break;
                    case 3: this.Owner.AddToBackpack(new SmallBrickHouseDeed()); break;
                    case 4: this.Owner.AddToBackpack(new WoodHouseDeed()); break;
                }
              
                this.Owner.SendMessage("Voce ganhou uma escritura para uma casa !");
                var npc = Quester as BaseVendor;
                if (npc != null)
                    npc.PrivateOverheadMessage("Tome aqui, uma escritura de casa para voce morar !", Owner, 0);
            } else
            {
                var npc = Quester as BaseVendor;
                if(npc != null)
                    npc.PrivateOverheadMessage("Ja te vi por aqui ?", Owner, 0);
            }
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
    public class CasaNpc : MondainQuester
    {
        /// AQUI REGISTRA QUAL QUEST ELE VAI DAR 
        public override Type[] Quests
        {
            get
            {
                return new Type[] {
                    typeof(QuestCasa)
        };
            }
        }


        [Constructable]
        public CasaNpc()
            : base("Silvio Sumbis", "O Construtor de Casas")
        {
            this.SetSkill(SkillName.Anatomy, 120.0, 120.0);
            this.SetSkill(SkillName.Parry, 120.0, 120.0);
            this.SetSkill(SkillName.Healing, 120.0, 120.0);
            this.SetSkill(SkillName.Tactics, 120.0, 120.0);
            this.SetSkill(SkillName.Swords, 120.0, 120.0);
            this.SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public CasaNpc(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            this.Say("Quer ter uma linda e mobilhada moradia ? Me ajude, que te ajudo !!");  // Know yourself, and you will become a true warrior.
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
