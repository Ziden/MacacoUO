using System;
using System.Collections.Generic;
using Fronteira.Discord;
using Server.Commands;
using Server.Engines.Points;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;
using Server.Ziden;

namespace Server.Fronteira.Weeklies
{
    public class Daily : BaseQuest
    {
        public override bool DoneOnce { get { return false; } }

        public override object Title
        {
            get
            {
                return "Desafio Diario";
            }
        }

        public override object Description
        {
            get
            {
                return "Desafio Diario: Mate monstros e colete items para completar a missao e ganhar recompensas !";
            }
        }

        public override object Refuse
        {
            get
            {
                return "Entendo";
            }
        }

        public override object Uncomplete
        {
            get
            {
                return "Termine a longa e desafiante missao para obter seu pergaminho !";
            }
        }

        public override object Complete
        {
            get
            {
                return "Muito bem, receba aqui entao suas recompensas !";
            }
        }

        public KillComboDia[][] Possiveis = new KillComboDia[][]
        {
            // Ice
            new KillComboDia [] {
                 new KillComboDia("Aranha do Gelo", typeof(FrostSpider), 15),
                 new KillComboDia("Elemental da Neve", typeof(SnowElemental), 10),
            },

            // Fire
            new KillComboDia [] {
                 new KillComboDia("Aranha do Gelo", typeof(LavaLizard), 25),
                 new KillComboDia("Elemental da Neve", typeof(LavaSerpent), 10),
            },

            // Shame
            new KillComboDia [] {
                 new KillComboDia("Grande Elemental da Terra", typeof(GreaterEarthElemental), 15),
                 new KillComboDia("Magolho Anciao", typeof(ElderGazer), 15),
            },

            // Destard
            new KillComboDia [] {
                 new KillComboDia("Dragao", typeof(Dragon), 10),
                 new KillComboDia("Drake", typeof(Drake), 15),
            },

            // Hythloth
            new KillComboDia [] {
                 new KillComboDia("Capeta", typeof(CrystalDaemon), 5),
                 new KillComboDia("Demonio", typeof(Daemon), 20),
            },

            // Wrong
            new KillComboDia [] {
                 new KillComboDia("Lagarto Defensor", typeof(LizardmanDefender), 15),
                 new KillComboDia("Lagarto Agressivo", typeof(LizardmanSquatter), 15),
            },

            // Despise
            new KillComboDia [] {
                 new KillComboDia("Rotting Corpse", typeof(RottingCorpse), 5),
                 new KillComboDia("Elfo Anarquista", typeof(ElfBrigand), 15),
            },

            // Orc Cave
            new KillComboDia [] {
                 new KillComboDia("Orc", typeof(Orc), 25),
                 new KillComboDia("Orc Mago", typeof(OrcishMage), 15),
            },

            // Goblins
            new KillComboDia [] {
                 new KillComboDia("Goblin Alquimista", typeof(GreenGoblinAlchemist), 15),
                 new KillComboDia("Goblin Mago", typeof(GreenGoblinMage), 15),
            }

        };

        public class KillComboDia
        {
            public KillComboDia(String n, Type t, int q)
            {
                Monstro = t;
                qtd = q;
                this.n = n;
            }
            public Type Monstro;
            public int qtd;
            public String n;

            public BaseObjective GetObj()
            {
                return new SlayObjective(Monstro, n, qtd);
            }
        }

        private void Check()
        {
            if (!SaveWeekly.Carregado)
                SaveWeekly.OnLoad();

            var dia = GetDia();
            if (dia != SaveWeekly.DIA_ATUAL)
            {
                Shard.Debug("!!!! Novos desafios diarios sendo gerados !!!");
                var sorteado = Possiveis[Utility.Random(Possiveis.Length)];
                SaveWeekly.KillsDia.Clear();
                SaveWeekly.KillsDia.Add(sorteado[1]);
                SaveWeekly.KillsDia.Add(sorteado[0]);
                SaveWeekly.DIA_ATUAL = dia;
                SaveWeekly.JaCompletouDia.Clear();
                Anuncio.Anuncia($"Dia {dia}: Desafio Diario Recarregado !!");
                foreach (var i in SaveWeekly.KillsDia)
                    DiscordBot.SendMessage($":star: Matar {i.qtd} {i.n}");
            }
        }

        public Daily()
            : base()
        {

            Check();
            this.Objectives.Clear();
            foreach (var obj in SaveWeekly.KillsDia)
            {
                this.AddObjective(obj.GetObj());
            }
            this.AddReward(new BaseReward("2500 EXP"));
            this.AddReward(new BaseReward(typeof(PergaminhoSkillcap), 1, "Pergaminho +1 Peso da Bag"));
            this.AddReward(new BaseReward(typeof(PergaminhoPeso), 1, "Pergaminho +1 Item na Bag"));
        }

        public static int HorasFaltam()
        {
            return 24 - (int)(TimeSpan.FromTicks(DateTime.Now.Ticks).TotalHours % 24);
        }

        public static int GetDia()
        {
            return (int)Math.Floor(TimeSpan.FromTicks(DateTime.Now.Ticks).TotalHours / 24);
        }

        public override bool CanOffer()
        {
            Check();
            if (SaveWeekly.JaCompletouDia.Contains(this.Owner.Serial.Value))
            {
                this.Owner.SendMessage($"Voce ja completou o desafio diario. Aguarde {HorasFaltam()} horas.");
                return false;
            }
            return base.CanOffer();
        }

        public override void OnCompleted()
        {
            PointsSystem.Exp.AwardPoints(this.Owner, 2500);
            this.Owner.PlaySound(this.CompleteSound);
            SaveWeekly.JaCompletouDia.Add(this.Owner.Serial.Value);
            DiscordBot.SendMessage($":star: {Owner.Name} completou o desafio diario !");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
            writer.Write(SaveWeekly.DIA_ATUAL);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            var dia = reader.ReadInt();
            if(dia != GetDia())
            {
                OnResign(true);
            }
        }
    }

    //// AQUI EH A CLASSE DO NPC Q VAI DAR A QUETS ///   
    public class QuestGiverDiario : MondainQuester
    {
        /// AQUI REGISTRA QUAL QUEST ELE VAI DAR 
        public override Type[] Quests
        {
            get
            {
                return new Type[] {
                    typeof(Daily)
        };
            }
        }


        [Constructable]
        public QuestGiverDiario()
            : base("Diario", "O Recompensador")
        {
            this.SetSkill(SkillName.Anatomy, 120.0, 120.0);
            this.SetSkill(SkillName.Parry, 120.0, 120.0);
            this.SetSkill(SkillName.Healing, 120.0, 120.0);
            this.SetSkill(SkillName.Tactics, 120.0, 120.0);
            this.SetSkill(SkillName.Swords, 120.0, 120.0);
            this.SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public QuestGiverDiario(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            this.Say("Fale comigo para fazer a missao diaria !");  // Know yourself, and you will become a true warrior.
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

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();


        }
    }
}
