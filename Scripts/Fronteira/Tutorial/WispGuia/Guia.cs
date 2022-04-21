using Server.Gumps;
using Server.Gumps.Newbie;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Engines.Quests;

namespace Server.Fronteira.Tutorial.WispGuia
{
    public class ObjetivoGuia
    {
        public Point3D LocalDungeon;
        public Point3D Local;
        public string FraseProgresso;
        public string FraseCompletar;
        public string FraseIniciar;
        public PassoTutorial Proximo;
        public Func<PlayerMobile, PassoTutorial> GetProximo;
        public bool PrecisaEvento = false;
        public Action<PlayerMobile> Completar;
        public Action<PlayerMobile, ObjetivoGuia> OnCheck;
    }

    public enum PassoTutorial
    {
        NADA,

        // Flow Padrao De Combat
        PEGAR_CAVALO,
        IR_BANCO,
        IR_FERREIRO,
        PEGAR_KIT,
        PEGAR_QUEST,
        MATAR_MAGO,
        VOLTAR_QUEST,
        ALAVANCA,
        BIXO_ESGOTO,
        JILL,

        // Tamer
        QUEST_TAMER,

        // Worker
        TRABALHO,

        MINE,
        GETORE,
        SMELT,
        MACE,
        SELL,
        PLANT,
        TAILOR,

        FIM,

        ALCHEMIST,
        MOONGATE,
        QUEST_CASA,
        COMPLETA_JILL,

        // NOVOS

    }

    public class Guia
    {
        public Dictionary<PassoTutorial, ObjetivoGuia> Objetivos = new Dictionary<PassoTutorial, ObjetivoGuia>();

        public Guia()
        {
            Objetivos.Add(PassoTutorial.PEGAR_CAVALO, new ObjetivoGuia()
            {
                Local = new Point3D(3526, 2577, 7),
                FraseIniciar = "Ei o que acha de conseguir um cavalo ?! Siga a seta no canto do mapa que eu arrumo um pra voce !",
                FraseProgresso = "Siga a setinha no canto da sua tela para chegar no Coxeiro.",
                FraseCompletar = "Tome aqui, um cavalo. Clique duas vezes para montar.",
                Completar = (pl) =>
                {
                    var c = new Horse();
                    c.SetControlMaster(pl);
                    c.MoveToWorld(pl.Wisp.Location, pl.Wisp.Map);
                    c.AIObject.DoOrderCome();
                },
                Proximo = PassoTutorial.IR_FERREIRO
            });

            Objetivos.Add(PassoTutorial.IR_FERREIRO, new ObjetivoGuia()
            {
                Local = new Point3D(3469, 2540, 10),
                FraseIniciar = "Vamos agora ao ferreiro ! Fale 'Comprar' a ele para se equipar. Items de npc tem qualidade baixa e ferreiros podem criar algo melhor",
                FraseProgresso = "Encontre o ferreiro e diga Comprar a ele para comprar items.",
                FraseCompletar = "Se voce tiver skills de trabalho, pode falar 'trabalho' para trabalhar pro NPC ou 'recompensas'. ",
                Completar = (pl) =>
                {
                    pl.Backpack.DropItem(new Gold(300));
                    pl.PlaySound(0x2E6);
                },
                Proximo = PassoTutorial.IR_BANCO
            });

            Objetivos.Add(PassoTutorial.IR_BANCO, new ObjetivoGuia()
            {
                Local = new Point3D(3487, 2573, 21),
                FraseIniciar = "Quel tal ir ao banco guardar seu dinheiro ? Va ao banco e fale 'Banco' !",
                FraseProgresso = "Va ao banco e fale 'Banco' para abrir seu banco.",
                PrecisaEvento = true,
                FraseCompletar = "Voce tambem pode fazer cheques falando 'cheque'. Coloquei um cheque em seu banco.",
                Completar = (pl) =>
                {
                    pl.BankBox.DropItem(new BankCheck(300));
                },
                GetProximo = (pl) =>
                {
                    return PassoTutorial.PEGAR_KIT;
                }
            });

            Objetivos.Add(PassoTutorial.PEGAR_KIT, new ObjetivoGuia()
            {
                Local = new Point3D(3420, 2520, 21),
                FraseIniciar = "Vamos agora ao salao do conhecimento. La voce podera escolher seus conhecimentos iniciais.Voce pode ter qualquer skill que quiser isto e apenas um inicio",
                FraseProgresso = "Va ao salao do conhecimento obter seus conhecimentos iniciais.",
                FraseCompletar = "Perfeito. Agora escolha bem seus conhecimentos iniciais.",
                Completar = (pl) =>
                {
                    if(pl.Profession == 0)
                        pl.SendGump(new NonRPClassGump());
                },
                GetProximo = (pl) =>
                {
                    return PassoTutorial.ALCHEMIST;
                }
            });

            Objetivos.Add(PassoTutorial.ALCHEMIST, new ObjetivoGuia()
            {
                Local = new Point3D(3461, 2566, 35),
                FraseIniciar = "Agora vamos ao alquimista para comprar pocoes de vida.",
                FraseProgresso = "Va ao alquimista para comprar pocoes de vida.",
                FraseCompletar = "Perfeito. Agora vamos ter um pouco de combate!.",
                Completar = (pl) =>
                {
                    pl.Backpack.DropItem(new HealPotion());
                    pl.Backpack.DropItem(new HealPotion());
                    pl.Backpack.DropItem(new HealPotion());
                    pl.Backpack.DropItem(new CurePotion());
                    pl.Backpack.DropItem(new CurePotion());
                },
                GetProximo = (pl) =>
                {
                    if (pl.Profession == StarterKits.TAMER)
                        return PassoTutorial.QUEST_TAMER;
                    if (pl.Profession == StarterKits.MERC)
                        return PassoTutorial.TRABALHO;
                    return PassoTutorial.PEGAR_QUEST;
                }
            });

            //

            Objetivos.Add(PassoTutorial.QUEST_TAMER, new ObjetivoGuia()
            {
                Local = new Point3D(3526, 2577, 7),
                FraseIniciar = "Vamos voltar ao coxeiro agora. Fale com ele e inicie a quest para treinamento de animais.",
                FraseProgresso = "Vamos voltar ao coxeiro agora. Fale com ele e inicie a quest para treinamento de animais.",
                FraseCompletar = "Muito bem, clique duas vezes no coxeiro para fazer a quest de domar animais.",
                Proximo = PassoTutorial.FIM
            });

            Objetivos.Add(PassoTutorial.TRABALHO, new ObjetivoGuia()
            {
                Local = new Point3D(3469, 2540, 10),
                FraseIniciar = "Vamos ao ferreiro. Diga 'trabalho' a ele.",
                FraseProgresso = "Diga 'trabalho' ao ferreiro para trabalhar.",
                FraseCompletar = "Voce sobe suas skills de trabalho, trabalhando para NPC's.",
                Proximo = PassoTutorial.FIM
            });

            Objetivos.Add(PassoTutorial.PEGAR_QUEST, new ObjetivoGuia()
            {
                Local = new Point3D(3503, 2483, 26),
                OnCheck = (PlayerMobile player, ObjetivoGuia obj) => {
                    if (player.DoneQuests.Any(q => q.QuestType == typeof(SapatoLindoQ)) || player.Quests.Any(q => q.GetType() == typeof(SapatoLindoQ)))
                    {
                        if(player.Wisp != null)
                        {
                            ((NovoWispGuia)player.Wisp).Completa(obj);
                        }
                    }
                },
                PrecisaEvento = true,
                FraseIniciar = "Vamos agora a norte de haven. Encontre o Ze e de 2 cliques nele para pegar iniciar a aventura.",
                FraseProgresso = "Va a norte de Haven e encontre o Zeh Roela e de dois cliques nele !",
                FraseCompletar = "Otimo, agora vamos a dungeon",
                Proximo = PassoTutorial.MATAR_MAGO
            });

            Objetivos.Add(PassoTutorial.MATAR_MAGO, new ObjetivoGuia()
            {
                LocalDungeon = new Point3D(59, 1480, -28),
                PrecisaEvento = true,
                FraseIniciar = "Siga o caminho a norte para encontrar a dungeon",
                FraseProgresso = "Encontre o mago putrido na dungeon norte do Zeh",
                FraseCompletar = "Voce e muito bom ! Pegue a lanterna e entregue agora",
                Proximo = PassoTutorial.VOLTAR_QUEST
            });

            Objetivos.Add(PassoTutorial.VOLTAR_QUEST, new ObjetivoGuia()
            {
                Local = new Point3D(3503, 2483, 26),
                PrecisaEvento = true,
                FraseIniciar = "Vamos sair desse buraco. Saia da dungeon e arraste a lanterna para o Ze.",
                FraseProgresso = "Saia da dungeon e arraste a lanterna para o Zeh para entrega-la",
                FraseCompletar = "Excelente ! Voce eh demais !",
                Proximo = PassoTutorial.ALAVANCA
            });

            Objetivos.Add(PassoTutorial.ALAVANCA, new ObjetivoGuia()
            {
                LocalDungeon = new Point3D(84, 1478, -28),
                FraseIniciar = "Hmm ele falou algo sobre pentagrama. Vamos voltar a dungeon e procurar esse pentagrama ?",
                FraseProgresso = "Alavanca... perto das teias de aranha na sala do pentagrama...hmm...",
                FraseCompletar = "Hmmm parece ter uma alavanca por aqui ! Que exitante !",
                Proximo = PassoTutorial.BIXO_ESGOTO
            });

            Objetivos.Add(PassoTutorial.BIXO_ESGOTO, new ObjetivoGuia()
            {
                PrecisaEvento = true,
                FraseIniciar = "Que curiosidade. Deve ter alguma alavanca nessa sala",
                FraseProgresso = "Vamos vamos, dentro da dungeon de haven deve ter algum monstro poderoso...",
                FraseCompletar = "Eca que nojo !",
                Proximo = PassoTutorial.JILL
            });

            Objetivos.Add(PassoTutorial.JILL, new ObjetivoGuia()
            {
                PrecisaEvento = true,
                FraseIniciar = "Hmm voce viu o livro que tinha dentro do monstro ?! Vamos procurar essa Jill perto do Banco",
                FraseProgresso = "Vamos procurar a tal Jill perto do banco...",
                FraseCompletar = "Que diferente esse mapa que te deram ! Que tal tentar usar ele ?",
                Proximo = PassoTutorial.COMPLETA_JILL
            });


            Objetivos.Add(PassoTutorial.COMPLETA_JILL, new ObjetivoGuia()
            {
                PrecisaEvento = true,
                FraseIniciar = "Uma fada Ma ?! Vamos acha-la. Veja o mapa que a Jill lhe deu",
                FraseProgresso = "Encontre essa fada ma, o mapa que a Jill te deu parece que indica para o local",
                FraseCompletar = "Muito bom ! O que acha agora de ir para uma dungeon ?",
                Proximo = PassoTutorial.MOONGATE
            });

            Objetivos.Add(PassoTutorial.MOONGATE, new ObjetivoGuia()
            {
                Local = new Point3D(3471, 2599, 10),
                PrecisaEvento = true,
                FraseIniciar = "Vamos agora usar o Portal Publico para Deceit",
                FraseProgresso = "Va ate o portal publico e vamos para Deceit !",
                FraseCompletar = "Otimo, agora fale com Silvio para pegar a missao !",
                Proximo = PassoTutorial.QUEST_CASA
            });

            Objetivos.Add(PassoTutorial.QUEST_CASA, new ObjetivoGuia()
            {
                PrecisaEvento = true,
                FraseIniciar = "Fale com o Silvio para fazer a missao. Voce ira ganhar uma casa !",
                FraseProgresso = "Vamos, Silvio esta te esperando ! Fale com ele para pegar a missao !",
                FraseCompletar = "Muito bom, agora voce sabe o que fazer !!",
                Proximo = PassoTutorial.FIM
            });
        }
    }
}
