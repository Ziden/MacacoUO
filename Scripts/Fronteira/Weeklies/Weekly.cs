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
    public class Weekly : BaseQuest
    {
        public override bool DoneOnce { get { return false; } }

        public override object Title
        {
            get
            {
                return "Desafio Semanal";
            }
        }
        public override object Description
        {
            get
            {
                return "Desafio Semanal: Mate monstros e colete items para completar a missao e ganhar recompensas !";
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

        public KillCombo[][] Possiveis = new KillCombo[][]
        {
            // Ice
            new KillCombo [] {
                 new KillCombo("Aranha do Gelo", typeof(FrostSpider), 200),
                 new KillCombo("Elemental da Neve", typeof(SnowElemental), 200),
            },

            // Fire
            new KillCombo [] {
                 new KillCombo("Aranha do Gelo", typeof(LavaLizard), 200),
                 new KillCombo("Elemental da Neve", typeof(LavaSerpent), 200),
            },

            // Shame
            new KillCombo [] {
                 new KillCombo("Grande Elemental da Terra", typeof(GreaterEarthElemental), 200),
                 new KillCombo("Magolho Anciao", typeof(ElderGazer), 100),
            },

            // Destard
            new KillCombo [] {
                 new KillCombo("Dragao", typeof(Dragon), 100),
                 new KillCombo("Drake", typeof(Drake), 200),
            },

            // Hythloth
            new KillCombo [] {
                 new KillCombo("Capeta", typeof(CrystalDaemon), 60),
                 new KillCombo("Demonio", typeof(Daemon), 200),
            },

            // Wrong
            new KillCombo [] {
                 new KillCombo("Lagarto Defensor", typeof(LizardmanDefender), 200),
                 new KillCombo("Lagarto Agressivo", typeof(LizardmanSquatter), 200),
            },

            // Despise
            new KillCombo [] {
                 new KillCombo("Rotting Corpse", typeof(RottingCorpse), 100),
                 new KillCombo("Elfo Anarquista", typeof(ElfBrigand), 100),
            },

            // Orc Cave
            new KillCombo [] {
                 new KillCombo("Orc", typeof(Orc), 200),
                 new KillCombo("Orc Mago", typeof(OrcishMage), 200),
            },

            // Goblins
            new KillCombo [] {
                 new KillCombo("Goblin Alquimista", typeof(GreenGoblinAlchemist), 100),
                 new KillCombo("Goblin Mago", typeof(GreenGoblinMage), 100),
            }
        };

        public class KillCombo
        {
            public KillCombo(String n, Type t, int q)
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

            var semana = GetSemana();
            if (semana != SaveWeekly.SEMANA_ATUAL)
            {
                Shard.Debug("!!!! Novos desafios semanais sendo gerados !!!");
                var sorteado = Possiveis[Utility.Random(Possiveis.Length)];
                SaveWeekly.Kills.Clear();
                SaveWeekly.Kills.Add(sorteado[1]);
                SaveWeekly.Kills.Add(sorteado[0]);
                SaveWeekly.SEMANA_ATUAL = semana;
                SaveWeekly.JaCompletou.Clear();
                Anuncio.Anuncia($"Semana {semana}: Desafio Semanal Recarregado !!");
                foreach (var i in SaveWeekly.Kills)
                    DiscordBot.SendMessage($":star: Matar {i.qtd} {i.n}");
            }
        }

        public Weekly()
            : base()
        {

            Check();
            this.Objectives.Clear();
            foreach (var obj in SaveWeekly.Kills)
            {
                this.AddObjective(obj.GetObj());
            }
            this.AddReward(new BaseReward("2500 EXP"));
            this.AddReward(new BaseReward(typeof(PergaminhoSkillcap), 1, "Pergaminho +1 Skillcap"));
            this.AddReward(new BaseReward(typeof(PergaminhoPeso), 1, "Pergaminho +3 Peso na Bag"));
        }

        public static int DiasFaltam()
        {
            return 7 - (int)(TimeSpan.FromTicks(DateTime.Now.Ticks).TotalDays % 7);
        }

        public static int GetSemana()
        {
            return (int)Math.Floor(TimeSpan.FromTicks(DateTime.Now.Ticks).TotalDays / 7d);
        }

        public override bool CanOffer()
        {
            Check();
            if (SaveWeekly.JaCompletou.Contains(this.Owner.Serial.Value))
            {
                this.Owner.SendMessage($"Voce ja completou o desafio semanal. Aguarde {DiasFaltam()} dias.");
                return false;
            }
            return base.CanOffer();
        }

        public override void OnCompleted()
        {
            PointsSystem.Exp.AwardPoints(this.Owner, 2500);
            this.Owner.PlaySound(this.CompleteSound);
            SaveWeekly.JaCompletou.Add(this.Owner.Serial.Value);
            DiscordBot.SendMessage($":star: {Owner.Name} completou o desafio semanal !");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
            writer.Write(SaveWeekly.SEMANA_ATUAL);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            var semana = reader.ReadInt();
            if(semana != GetSemana())
            {
                OnResign(true);
            }
        }
    }

    //// AQUI EH A CLASSE DO NPC Q VAI DAR A QUETS ///   
    public class QuestGiverSemanal : MondainQuester
    {
        /// AQUI REGISTRA QUAL QUEST ELE VAI DAR 
        public override Type[] Quests
        {
            get
            {
                return new Type[] {
                    typeof(Weekly)
        };
            }
        }


        [Constructable]
        public QuestGiverSemanal()
            : base("Semanal", "O Recompensador")
        {
            this.SetSkill(SkillName.Anatomy, 120.0, 120.0);
            this.SetSkill(SkillName.Parry, 120.0, 120.0);
            this.SetSkill(SkillName.Healing, 120.0, 120.0);
            this.SetSkill(SkillName.Tactics, 120.0, 120.0);
            this.SetSkill(SkillName.Swords, 120.0, 120.0);
            this.SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public QuestGiverSemanal(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            this.Say("Fale comigo para fazer a missao semanal !");  // Know yourself, and you will become a true warrior.
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
