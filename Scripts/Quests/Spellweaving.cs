using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class PatienceQuest : BaseQuest
    { 
        public PatienceQuest()
            : base()
        { 
            this.AddObjective(new ObtainObjective(typeof(MiniatureMushroom), "cogumelos em miniatura", 20, 0xD16, 3600));		
				
            this.AddReward(new BaseReward(1074872)); // A oportunidade de aprender os caminhos do Arcanista.
        }

        public override QuestChain ChainID
        {
            get
            {
                return QuestChain.Spellweaving;
            }
        }
        public override Type NextQuest
        {
            get
            {
                return typeof(NeedsOfManyHeartwoodQuest);
            }
        }
        /* Paciência */
        public override object Title
        {
            get
            {
                return 1072753;
            }
        }
        /* Aprender a tecer feitiços e controlar as forças da natureza exige sacrifício,
          disciplina, foco e uma dedicação inabalável à própria Sosaria. Nós não
          ensinar os indignos. Não compreendem as lições nem a dedicação
          requeridos. Se você deseja trilhar o caminho do Arcanista, deve fazer o que eu
          exigir sem hesitação ou pergunta. Sua primeira tarefa é reunir miniaturas
          cogumelos ... 20 deles dos galhos de nossa poderosa casa. eu te dou um
          hora para completar a tarefa. */
        public override object Description
        {
            get
            {
                return 1072762;
            }
        }
        /* *acena com a cabeça* Nem todo mundo tem temperamento para seguir o caminho do Arcanista. */
        public override object Refuse
        {
            get
            {
                return 1072767;
            }
        }
        /* Os cogumelos que procuro podem ser encontrados crescendo aqui no Cerne da Floresta. Procure-os
                e recolhê-los. Você está ficando sem tempo. */
        public override object Uncomplete
        {
            get
            {
                return 1072774;
            }
        }
        /* Você colheu os cogumelos? */
        public override object Complete
        {
            get
            {
                return 1074166;
            }
        }
        public override bool CanOffer()
        {
            return MondainsLegacy.Spellweaving;
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

    public class NeedsOfManyHeartwoodQuest : BaseQuest
    { 
        public NeedsOfManyHeartwoodQuest()
            : base()
        { 
            this.AddObjective(new ObtainObjective(typeof(Cotton), "fardo de algodão", 10, 0xDF9));			
			
            this.AddReward(new BaseReward(1074872)); // A oportunidade de aprender os caminhos do Arcanista.
        }

        public override QuestChain ChainID
        {
            get
            {
                return QuestChain.Spellweaving;
            }
        }
        public override Type NextQuest
        {
            get
            {
                return typeof(NeedsOfManyPartHeartwoodQuest);
            }
        }
        /* Necessidades de Muitos - O Cerne */
        public override object Title
        {
            get
            {
                return 1072797;
            }
        }
        /* O caminho do Arcanista envolve a cooperação com os outros e um forte
                compromisso com a comunidade do seu povo. Nós corremos baixo no
                algodão que usamos para fechar feridas e nosso povo precisa. Traga 10
                fardos de algodão para mim. */
        public override object Description
        {
            get
            {
                return 1072763;
            }
        }
        /* Você põe em risco seu progresso ao longo do caminho com sua falta de vontade. */
        public override object Refuse
        {
            get
            {
                return 1072768;
            }
        }
        /* Não me importa onde você adquire o algodão, apenas que você o forneça. */
        public override object Uncomplete
        {
            get
            {
                return 1072775;
            }
        }
        /* Bem, onde estão os fardos de algodão? */
        public override object Complete
        {
            get
            {
                return 1074110;
            }
        }
        public override bool CanOffer()
        {
            return MondainsLegacy.Spellweaving;
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

    public class NeedsOfManyPartHeartwoodQuest : BaseQuest
    { 
        public NeedsOfManyPartHeartwoodQuest()
            : base()
        { 
            this.AddObjective(new ObtainObjective(typeof(Board), "Pranchas", 250, 0x1BD7));			
			
            this.AddReward(new BaseReward(1074872)); //A oportunidade de aprender os caminhos do Arcanista.
        }

        public override QuestChain ChainID
        {
            get
            {
                return QuestChain.Spellweaving;
            }
        }
        public override Type NextQuest
        {
            get
            {
                return typeof(MakingContributionHeartwoodQuest);
            }
        }
        /* Necessidades de Muitos - O Cerne */
        public override object Title
        {
            get
            {
                return 1072797;
            }
        }
        /* Devemos cuidar da defesa de nosso povo! Traga pranchas para novas flechas. */
        public override object Description
        {
            get
            {
                return 1072764;
            }
        }
        /* As pessoas precisam desses itens. Você está se mostrando inadequado
                às demandas de um membro desta comunidade. */
        public override object Refuse
        {
            get
            {
                return 1072769;
            }
        }
        /* Os requisitos são simples -- 250 placas. */
        public override object Uncomplete
        {
            get
            {
                return 1072776;
            }
        }
        /* Bem, onde estão as placas? */
        public override object Complete
        {
            get
            {
                return 1074152;
            }
        }
        public override bool CanOffer()
        {
            return MondainsLegacy.Spellweaving;
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

    public class MakingContributionHeartwoodQuest : BaseQuest
    { 
        public MakingContributionHeartwoodQuest()
            : base()
        { 
            this.AddObjective(new ObtainObjective(typeof(SackFlour), "saco de farinha", 1, 0x1039));
            this.AddObjective(new ObtainObjective(typeof(JarHoney), "Pote de mel", 10, 0x9EC));
            this.AddObjective(new ObtainObjective(typeof(FishSteak), "filé de peixe", 20, 0x97B));		
				
            this.AddReward(new BaseReward(1074872)); //A oportunidade de aprender os caminhos do Arcanista.
        }

        public override QuestChain ChainID
        {
            get
            {
                return QuestChain.Spellweaving;
            }
        }
        public override Type NextQuest
        {
            get
            {
                return typeof(UnnaturalCreationsQuest);
            }
        }
        /* Making a Contribution - The Heartwood */
        public override object Title
        {
            get
            {
                return 1072798;
            }
        }
        /* Com saúde e defesa asseguradas, precisamos atender a necessidade da comunidade
          para comida e bebida. Vamos nos banquetear com bifes de peixe, doces e vinho. Você
          fornecerá os ingredientes, os cozinheiros prepararão a refeição. como arcanista
          depende de outros para construir o foco e emprestar seu poder ao seu trabalho, o
          comunidade precisa do esforço de todos para sobreviver. */
        public override object Description
        {
            get
            {
                return 1072765;
            }
        }
        /* Não vacile agora. Você começou a se mostrar promissor. */
        public override object Refuse
        {
            get
            {
                return 1072770;
            }
        }
        /* Onde estão os itens que você foi encarregado de fornecer para o banquete? */
        public override object Uncomplete
        {
            get
            {
                return 1072777;
            }
        }
        /* Que bom, você voltou. Estamos ansiosos pelo banquete. */
        public override object Complete
        {
            get
            {
                return 1074158;
            }
        }
        public override bool CanOffer()
        {
            return MondainsLegacy.Spellweaving;
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

    public class UnnaturalCreationsQuest : BaseQuest
    { 
        public UnnaturalCreationsQuest()
            : base()
        { 
            this.AddObjective(new SlayObjective(typeof(ExodusOverseer), "superintendentes de êxodo", 5));
            this.AddObjective(new SlayObjective(typeof(ExodusMinion), "asseclas do êxodo", 2));
			
            this.AddReward(new BaseReward(typeof(ArcaneCircleScroll), 1071026)); // Círculo Arcano			
            this.AddReward(new BaseReward(typeof(GiftOfRenewalScroll), 1071027)); // Presente de Renovação
            this.AddReward(new BaseReward(typeof(Spellbook), 1031600)); // Livro de Feitiços
        }

        public override QuestChain ChainID
        {
            get
            {
                return QuestChain.Spellweaving;
            }
        }
        /* Criações não naturais */
        public override object Title
        {
            get
            {
                return 1072758;
            }
        }
        /* Você provou seu desejo de contribuir com a comunidade e servir
                pessoas. Agora você deve demonstrar sua vontade de defender Sosaria de
                a maior praga que a atormenta. Criaturas não naturais, levadas a um
                tipo de vida pervertida, espoliar nosso mundo justo. Destrua-os - 5 Êxodo
                Superintendentes e 2 lacaios do Êxodo. */
        public override object Description
        {
            get
            {
                return 1072780;
            }
        }
        /* Você deve servir Sosaria com todo o seu coração e força.
                Sua falta de vontade não reflete favoravelmente sobre você. */
        public override object Refuse
        {
            get
            {
                return 1072771;
            }
        }
        /* A cada momento que você procrastina, essas criaturas não naturais danificam Sosaria. */
        public override object Uncomplete
        {
            get
            {
                return 1072779;
            }
        }
        /* Bem feito! Muito bem, de fato. Você é digno de se tornar um arcanista! */
        public override object Complete
        {
            get
            {
                return 1074167;
            }
        }
        public override bool CanOffer()
        {
            return MondainsLegacy.Spellweaving;
        }

        public override void GiveRewards()
        {
            this.Owner.Spellweaving = true;
			
            base.GiveRewards();
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
