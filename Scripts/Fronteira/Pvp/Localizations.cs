using System;
using System.Collections.Generic;

using Server;

namespace Server.TournamentSystem
{
    public static class Localizations
    {
        public static Dictionary<int, string> LocalizationTable { get; set; }

        public static void Configure()
        {
            LocalizationTable = new Dictionary<int, string>();

            /*LocalizationTable[1] = "View Statistics for previously completed tournaments";
            LocalizationTable[2] = "View a detailed 'how to' on utilizing this system";
            LocalizationTable[3] = "View detailed statistics of each arena team";
            LocalizationTable[4] = "View detailed statistics of your arena teams";
            LocalizationTable[5] = "View details on any future tournaments";
            LocalizationTable[6] = "Register a single, twosome or foursome arena team";
            LocalizationTable[7] = "Furture tournament registration";
            LocalizationTable[8] = "Sign up for a quick, 1 v 1 match. Target your opponent...";
            LocalizationTable[9] = "Register a new arena fight";
            LocalizationTable[10] = "Select your team size: Single, Twosome, or Foursome";
            LocalizationTable[11] = "Choose an appropriate team name. Must be unique and no more than 16 characters";
            LocalizationTable[12] = "Choose a player for your team";
            LocalizationTable[13] = "Like a wager, entry fees are collected from each team member, once that team joins the tournament";
            LocalizationTable[14] = "Tournament Start Time - Month";
            LocalizationTable[15] = "Tournament Start Time - Day";
            LocalizationTable[16] = "Tournament Start Time - Hour";
            LocalizationTable[17] = "Tournament Type - this determines the way players are eliminated";
            LocalizationTable[18] = "Choose the team size that can enter the tournament";
            LocalizationTable[19] = "Choose a tournament Style - this determines the type of player that can join";
            LocalizationTable[20] = "Choose a fight duration, in minutes";
            LocalizationTable[21] = "Use this arena's tram/fel counterpart to speed up the tournament";
            LocalizationTable[22] = "Magery spells only - no other spell schools allowed";
            LocalizationTable[23] = "Allow/Disallow all spell schools";
            LocalizationTable[24] = "Allow/Disallow consuming buff items, ie potions";
            LocalizationTable[25] = "Allow/Disallow casting spells prior to the wall dropping";
            LocalizationTable[26] = "Allow/Disallow summoning spells, ie Energy Vortex or Summon Daemon";
            LocalizationTable[27] = "Allow/Disallow weapon special moves";
            LocalizationTable[28] = "Allow/Disallow resurrecting - only pertains to Twosome or Foursome fights";
            LocalizationTable[29] = "Allow/Disallow mounts";
            LocalizationTable[30] = "Allow/Disallow ties. No ties will result in a tie breaker in the event time runs out";
            LocalizationTable[31] = "Allow/Disallow area spells, ie wither and earthquake";
            LocalizationTable[32] = "Choose the reward item for the tournament champion";
            LocalizationTable[33] = "Choose the reward item for the tournament runner up";
            LocalizationTable[37] = "Select your opponent";
            LocalizationTable[38] = "The challenging team can choose a wager in gold. This wager applies to <b>each</b> fighter in each team ";
            LocalizationTable[39] = "and will be automatically deducted from their account once the fight is agreed upon";
            LocalizationTable[40] = "View Fighters";
            LocalizationTable[41] = "Any type of character may join the tournament";
            LocalizationTable[42] = "Only magery based characters can join the tournament";
            LocalizationTable[43] = "Only dexxer based characters can join the tournament";
            LocalizationTable[44] = "Once a team loses, they are out of the tournament";
            LocalizationTable[45] = "On a teams first defeat, they continue on. Once they are defeated for the second time,";
            LocalizationTable[46] = "they are eliminated from the tournament";
            LocalizationTable[47] = "Same as single elimination, however the two teams will have a best of 3 duel to determine who";
            LocalizationTable[48] = "moves on in the tournament";
            LocalizationTable[49] = "Designed to give a pass to those with pre-fight jitters. Losers of round one will all fight in";
            LocalizationTable[50] = "round two, where only those winners can continue on";

            LocalizationTable[51] = "- Once a team loses, they are out of the tournament";
            LocalizationTable[52] = "- Once a team loses for the second time, they are out of the tournament";
            LocalizationTable[53] = "- Once a team loses a best of 3 duel, they are out of the tournament";
            LocalizationTable[54] = "- Round one losers move to a losers bracket, where next loss they are out of the tournament";

            LocalizationTable[55] = "- Any type of character may join the tournament";
            LocalizationTable[56] = "- Only magery based characters can join the tournament";
            LocalizationTable[57] = "- Only dexxer based characters can join the tournament";

            LocalizationTable[58] = "If not using your own gear, each player will utilize a standardized robe.";
            LocalizationTable[59] = "Your current gear will be moved to your bankbox once the duel begins.";

            LocalizationTable[60] = "Choose a unique name for your league.";
            LocalizationTable[61] = "Choose the size of your league. 8 and 16 teams are supported at this time.";
            LocalizationTable[62] = "The time in days in which each team must complete their duel for the current stage.";
            LocalizationTable[63] = String.Format("League start date.");
            LocalizationTable[64] = "Each players fee to enter with their team into the league. NO REFUNDS!!!";
            LocalizationTable[65] = "League size in total team participation. Currently supports 8 or 16 team leagues.";
            LocalizationTable[66] = "Elimination round format, think of playoffs.";
            LocalizationTable[660] = "None: No elimination round, league ends when the final stage ends.";
            LocalizationTable[661] = "Random: Elimination round matches are picked at random.";
            LocalizationTable[662] = "Strong Vs Weak: Elimination round matches are teams with the most wins vs teams with the least wins.";
            LocalizationTable[663] = "Strong Vs Strong: Elimination round matches are teams with the most wins vs teams with most wins.";
            LocalizationTable[66] = "Elimination Type indicates the amount of teams that are in the elimination round.";
            LocalizationTable[67] = "Elimination Type:";
            LocalizationTable[670] = "All: All teams move on to the elimination rounds";
            LocalizationTable[671] = "Half: Half of the teams will move on to the elimination rounds.";
            LocalizationTable[68] = "Force which arena will have to host the duals.";
            LocalizationTable[69] = "Standard arena duel types: Single elimination, best of 3, or capture the flag.";
            LocalizationTable[70] = "Standard teams types: single, twosome or foursome.";
            LocalizationTable[71] = "Cycle through available rules. Add/Remove by pushing the button to the right of the rule.";
            LocalizationTable[72] = "How long each duel will last.";

            LocalizationTable[73] = "Use this option to create a league...Staff Only!";
            LocalizationTable[74] = "Use this option to challenge your opponent in a league match.";
            LocalizationTable[75] = "Send match challenge to your opponent.";
            LocalizationTable[76] = "Your round match has already concluded.";
            LocalizationTable[77] = "{0} are already fighting or pending an arena duel.";
            LocalizationTable[78] = "Match opponent[s] are out of range.";

            LocalizationTable[79] = "Resign from team";
            LocalizationTable[80] = "Kick From Team";
            LocalizationTable[81] = "Add Fighter";
            LocalizationTable[82] = "Rename Team";
            LocalizationTable[83] = "{0} are currently pending an arena fight.";
            LocalizationTable[84] = "Proceed to challenge your opponent.";

            LocalizationTable[85] = "Allow/Disallow pets";*/

            LocalizationTable[1] = "Ver estatísticas de torneios concluídos anteriormente";
            LocalizationTable[2] = "Veja um 'como' detalhado sobre a utilização deste sistema";
            LocalizationTable[3] = "Ver estatísticas detalhadas de cada equipe da arena";
            LocalizationTable[4] = "Ver estatísticas detalhadas dos seus times de arena";
            LocalizationTable[5] = "Ver detalhes de torneios futuros";
            LocalizationTable[6] = "Registre uma equipe de arena solo, dupla ou quarteto";
            LocalizationTable[7] = "Inscrição no torneio Furture";
            LocalizationTable[8] = "Entre em uma partida rápida de 1 v 1. Escolha seu oponente...";
            LocalizationTable[9] = "Registrar uma nova luta na arena";
            LocalizationTable[10] = "Selecione o tamanho do seu time: Solo, Dupla ou Quarteto";
            LocalizationTable[11] = "Escolha um nome de equipe apropriado. Deve ser exclusivo e ter no máximo 16 caracteres";
            LocalizationTable[12] = "Escolha um jogador para o seu time";
            LocalizationTable[13] = "Como uma aposta, as taxas de inscrição são coletadas de cada membro da equipe, uma vez que a equipe ingressa no torneio";
            LocalizationTable[14] = "Horário de início do Torneio - Mês";
            LocalizationTable[15] = "Horário de início do Torneio - Dia";
            LocalizationTable[16] = "Horário de início do Torneio - Hora";
            LocalizationTable[17] = "Tipo de torneio - isso determina a maneira como os jogadores são eliminados";
            LocalizationTable[18] = "Escolha o tamanho da equipe que pode entrar no torneio";
            LocalizationTable[19] = "Escolha um estilo de torneio - isso determina o tipo de jogador que pode participar";
            LocalizationTable[20] = "Escolha a duração da luta, em minutos";
            LocalizationTable[21] = "Use a contraparte de bonde / fel desta arena para acelerar o torneio";
            LocalizationTable[22] = "Somente feitiços de magia - nenhuma outra escola de feitiços é permitida";
            LocalizationTable[23] = "Permitir/Proibir todas as escolas de feitiços";
            LocalizationTable[24] = "Permitir/Proibir consumir itens de buff, ou seja, poções";
            LocalizationTable[25] = "Permitir/Proibir feitiços lançados antes da queda da parede";
            LocalizationTable[26] = "Permitir/Proibir feitiços de invocação, ou seja, Vórtice Energético ou Daemon de Invocação";
            LocalizationTable[27] = "Permitir/Proibir movimentos especiais de armas";
            LocalizationTable[28] = "Permitir/Proibir ressuscitar - refere-se apenas a lutas duplas ou quartéis";
            LocalizationTable[29] = "Permitir/Proibir montarias";
            LocalizationTable[30] = "Permitir/Proibir empates. Nenhum empate resultará em um desempate no final do evento";
            LocalizationTable[31] = "Permitir/Proibir feitiços de área, isto é, murchar e terremoto";
            LocalizationTable[32] = "Escolha o item de recompensa para o campeão do torneio";
            LocalizationTable[33] = "Escolha o item de recompensa para o vice-campeão do torneio";
            LocalizationTable[34] = "A luta termina quando todos os jogadores de um time estão mortos";
            LocalizationTable[35] = "O melhor de 3 lutas determina o vencedor";
            LocalizationTable[36] = "Livre para todos, todos contra todos, a última pessoa viva vence!";
            LocalizationTable[37] = "Selecione seu oponente";
            LocalizationTable[38] = "A equipe desafiadora pode escolher uma aposta em ouro. Esta aposta se aplica a <b>cada</b> lutador em cada equipe";
            LocalizationTable[39] = "e serão automaticamente deduzidos da conta assim que a luta for acertada";
            LocalizationTable[40] = "Exibir lutadores";
            LocalizationTable[41] = "Qualquer tipo de personagem pode participar do torneio";
            LocalizationTable[42] = "Apenas personagens baseados em Magia podem participar do torneio";
            LocalizationTable[43] = "Apenas personagens baseados em Dexxer podem participar do torneio";
            LocalizationTable[44] = "Quando um time perde, está fora do torneio";
            LocalizationTable[45] = "Na primeira derrota das equipes, eles continuam. Depois de derrotados pela segunda vez";
            LocalizationTable[46] = "eles são eliminados do torneio";
            LocalizationTable[47] = "Igual à eliminação única, no entanto, as duas equipes terão o melhor de 3 duelos para determinar quem";
            LocalizationTable[48] = "segue em frente no torneio";
            LocalizationTable[49] = "Projetado para dar um passe para quem tem nervosismo antes da luta. Os perdedores da primeira rodada vão lutar todos";
            LocalizationTable[50] = "segunda rodada, onde apenas os vencedores podem continuar";

            LocalizationTable[51] = "- Quando um time perde, está fora do torneio";
            LocalizationTable[52] = "- Quando um time perde pela segunda vez, está fora do torneio";
            LocalizationTable[53] = "- Quando um time perde o melhor de 3 duelos, fica fora do torneio";
            LocalizationTable[54] = "- Os perdedores da primeira rodada passam para um grupo de perdedores, onde na próxima perda estão fora do torneio";

            LocalizationTable[55] = "- Qualquer tipo de personagem pode participar do torneio";
            LocalizationTable[56] = "- Apenas personagens baseados em Magia podem participar do torneio";
            LocalizationTable[57] = "- Apenas personagens baseados em Dexxer podem participar do torneio";

            LocalizationTable[58] = "Se não estiver usando seu próprio equipamento, cada jogador utilizará uma túnica padronizada.";
            LocalizationTable[59] = "Seu equipamento atual será movido para sua caixa bancária assim que o duelo começar.";

            LocalizationTable[60] = "Escolha um nome único para sua liga.";
            LocalizationTable[61] = "Escolha o tamanho da sua liga. 8 e 16 equipes são suportadas no momento.";
            LocalizationTable[62] = "O tempo em dias em que cada equipe deve concluir seu duelo para a fase atual.";

            LocalizationTable[63] = "Data de início da liga";
            LocalizationTable[64] = "Cada jogador cobra uma taxa para entrar com sua equipe na liga. NENHUM REEMBOLSO !!!";
            LocalizationTable[65] = "Tamanho da liga na participação total da equipe. Atualmente, suporta 8 ou 16 ligas de equipe.";
            LocalizationTable[66] = "Formato de eliminação da rodada, pense nos playoffs.";
            LocalizationTable[660] = "Nenhum: Nenhuma rodada de eliminação, a liga termina quando a fase final termina.";
            LocalizationTable[661] = "Aleatório: As partidas da rodada de eliminação são escolhidas aleatoriamente.";
            LocalizationTable[662] = "Fortes vs. Fracos: Partidas da rodada de eliminação são equipes com mais vitórias vs equipes com menos vitórias.";
            LocalizationTable[663] = "Fortes vs Fortes: Partidas da rodada de eliminação são equipes com mais vitórias vs equipes com mais vitórias.";
            LocalizationTable[66] = "Tipo de eliminação indica a quantidade de equipes que estão na rodada de eliminação.";
            LocalizationTable[67] = "Tipo de eliminação:";
            LocalizationTable[670] = "Todos: Todas as equipes passam para as eliminatórias";
            LocalizationTable[671] = "Metade: Metade das equipes passará para as eliminatórias.";
            LocalizationTable[68] = "Forçar qual arena terá que hospedar as duplas.";
            LocalizationTable[69] = "Tipos de duelos de arena padrão: eliminação única, melhor de 3 ou captura de bandeira.";
            LocalizationTable[70] = "Tipos de equipes padrão: solo, duplas ou quarteto.";
            LocalizationTable[71] = "Leia as regras disponíveis. Adicione/Remova pressionando o botão à direita da regra.";
            LocalizationTable[72] = "Quanto tempo durará cada duelo.";


            LocalizationTable[73] = "Use esta opção para criar uma liga ... Apenas STAFF!";
            LocalizationTable[74] = "Use esta opção para desafiar seu oponente em uma partida da liga.";
            LocalizationTable[75] = "Envie o desafio da partida ao seu oponente.";
            LocalizationTable[76] = "Sua partida da rodada já terminou.";
            LocalizationTable[77] = "{0} já está lutando ou aguardando um duelo na arena.";
            LocalizationTable[78] = "Os adversários da partida estão fora de alcance.";

            LocalizationTable[79] = "Sair da equipe";
            LocalizationTable[80] = "Tirar da equipe";
            LocalizationTable[81] = "Adicionar lutador";
            LocalizationTable[82] = "Renomear equipe";
            LocalizationTable[83] = "{0} está atualmente pendente de uma luta na arena.";
            LocalizationTable[84] = "Prossiga com o desafio do seu oponente.";

            LocalizationTable[85] = "Permitir/Proibir montarias";

            LocalizationTable[86] = "Join Team Rumble CTF Game";
            LocalizationTable[87] = "Remove yourself from the current Team Rumble game";
            LocalizationTable[88] = "Cancel current Team Rumble CTF";
            LocalizationTable[89] = "The current Team Rumble game is in progress!";


            LocalizationTable[100] = "Fight ends when all players from one team are dead";
            LocalizationTable[101] = "Best out of 3 fights determine the winner";
            LocalizationTable[102] = "Free for all, everybody vs everybody, last person alive wins!";
            LocalizationTable[103] = "Play your existing Arena Team to capture and defend your enemies flag!";
            LocalizationTable[104] = "Randomly picked teams to capture and defend your enemies flag!";
        }

        public static string GetLocalization(int cliloc)
        {
            if (LocalizationTable.ContainsKey(cliloc))
            {
                return LocalizationTable[cliloc];
            }

            return String.Empty;
        }

        public static string GetStyleTooltip(Tournament tournament)
        {
            switch (tournament.TourneyStyle)
            {
                default:
                case TourneyStyle.Standard: return Localizations.GetLocalization(55);
                case TourneyStyle.MagesOnly: return Localizations.GetLocalization(56);
                case TourneyStyle.DexxersOnly: return Localizations.GetLocalization(57);
            }
        }

        public static string GetTournyTypeTooltip(Tournament tournament)
        {
            switch (tournament.TourneyType)
            {
                default:
                case TourneyType.SingleElim: return Localizations.GetLocalization(51);
                case TourneyType.DoubleElimination: return Localizations.GetLocalization(52);
                case TourneyType.BestOf3: return Localizations.GetLocalization(53);
                //case TourneyType.Bracketed: return Localizations.GetLocalization(54);
            }
        }
    }
}
