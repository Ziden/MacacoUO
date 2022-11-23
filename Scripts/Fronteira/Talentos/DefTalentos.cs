using System.Collections.Generic;

namespace Server.Fronteira.Talentos
{

    public class DefTalentos
    {
        private static Dictionary<Talento, DefTalentos> _talentos = new Dictionary<Talento, DefTalentos>();

        public string Desc1;
        public int Icone;
        public string Nome;
        public Dictionary<SkillName, int> Aprende = new Dictionary<SkillName, int>();

        public static DefTalentos GetDef(Talento t)
        {
            if (!_talentos.ContainsKey(t))
                throw new System.Exception("Talento " + t.ToString() + " nao ta registrado nas definicoes");
            return _talentos[t];
        }

        static DefTalentos()
        {
            _talentos.Add(Talento.Dispel, new DefTalentos()
            {
                Desc1 = "Magia Dispel tambem remove todos Buffs & Debuffs",
                Icone = 2293,
                Nome = "Dissipar Magia"
            });

            _talentos.Add(Talento.Herbalismo, new DefTalentos()
            {
                Desc1 = "Max 90 Farming",
                Icone = 2291,
                Nome = "Herbalismo"
            });

            _talentos.Add(Talento.Feiticeiro, new DefTalentos()
            {
                Desc1 = "Max 90 Spirit Speak, +100% Potencia Buffs/Debuffs",
                Icone = 2295,
                Nome = "Feiticaria"
            });

            _talentos.Add(Talento.Necromante, new DefTalentos()
            {
                Desc1 = "Max 90 Necromancy & Spirit Speak",
                Icone = 2300,
                Nome = "Magia Negra"
            });

            _talentos.Add(Talento.Arquimago, new DefTalentos()
            {
                Desc1 = "Max 90 Imbuing, Inscription & Focus",
                Icone = 2292,
                Nome = "Magia Arcana"
            });

            _talentos.Add(Talento.Investigador, new DefTalentos()
            {
                Desc1 = "Max 90 Detect Hidden, Forensics & ItemID",
                Icone = 2297,
                Nome = "Pacificador"
            });

            _talentos.Add(Talento.Pacificador, new DefTalentos()
            {
                Desc1 = "Max 90 Musicanship & 70 Peacemaking",
                Icone = 2297,
                Nome = "Pacificador"
            });

            _talentos.Add(Talento.Cajados, new DefTalentos()
            {
                Desc1 = "Cast 30% Mais Rapido com Cajados",
                Icone = 2249,
                Nome = "Ponto de Foco"
            });

            _talentos.Add(Talento.Esconderijo, new DefTalentos()
            {
                Desc1 = "Max 90 Hiding",
                Icone = 2274,
                Nome = "Andante das Sombras"
            });

            _talentos.Add(Talento.Alquimista, new DefTalentos()
            {
                Desc1 = "Max 90 Alchemy",
                Icone = 2257,
                Nome = "Alquimista"
            });

            _talentos.Add(Talento.EstudoSagrado, new DefTalentos()
            {
                Desc1 = "+25% Curas",
                Icone = 2241,
                Nome = "Estudo Sagrado"
            });

            _talentos.Add(Talento.BardoGuerreiro, new DefTalentos()
            {
                Desc1 = "90 Cap Tactics, Anatomy, Healing & Cooking",
                Icone = 2273,
                Nome = "Bardo de Bar"
            });

            _talentos.Add(Talento.Elementalismo, new DefTalentos()
            {
                Desc1 = "+25% Dano Magico",
                Icone = 2303,
                Nome = "Elementalismo"
            });

            _talentos.Add(Talento.CacadorDeTesouros, new DefTalentos()
            {
                Desc1 = "Max 90 Lockpick Cartography Remove Trap & Detect Hidden",
                Icone = 2253,
                Nome = "Cacador de Tesouros"
            });

            _talentos.Add(Talento.Assassino, new DefTalentos()
            {
                Desc1 = "Max 90 Tactics & Poisoning & 70 Alchemy",
                Icone = 2272,
                Nome = "Assassino"
            });

            _talentos.Add(Talento.Ranger, new DefTalentos()
            {
                Desc1 = "Max 90 Veterinary & Animal Taming",
                Icone = 2298,
                Nome = "Ranger"
            });

            _talentos.Add(Talento.MusicaBranca, new DefTalentos()
            {
                Desc1 = "Max 80 Magery, Incription & +25% Curas",
                Icone = 2292,
                Nome = "Musico Magico"
            });


            _talentos.Add(Talento.Encantador, new DefTalentos()
            {
                Desc1 = "Max 70 Veterinary, Animal Taming & Animal Lore",
                Icone = 2293,
                Nome = "Encantador de Animais"
            });

            _talentos.Add(Talento.Nenhum, new DefTalentos()
            {
                Desc1 = "Nenhum",
                Icone = 40875,
                Nome = "Nenhum"
            });

            _talentos.Add(Talento.AnimalLore, new DefTalentos()
            {
                Desc1 = "Max 90 Animal Lore",
                Icone = 2260,
                Nome = "Biologia"
            });

            _talentos.Add(Talento.Provocacao, new DefTalentos()
            {
                Desc1 = "Max 70 Musicanship & Provocation",
                Icone = 2256,
                Nome = "Trovador"
            });

            _talentos.Add(Talento.CorrerStealth, new DefTalentos()
            {
                Desc1 = "Permite Correr em Stealth",
                Icone = 2283,
                Nome = "Astuto"
            });

            _talentos.Add(Talento.Gatuno, new DefTalentos()
            {
                Desc1 = "Esconde automatico ao usar stealth",
                Icone = 39850,
                Nome = "Gatuno"
            });

            _talentos.Add(Talento.Adagas, new DefTalentos()
            {
                Desc1 = "+30% Bonus de dano com adagas & 90 Fencing",
                Icone = 39850,
                Nome = "Adagas"
            });

            _talentos.Add(Talento.Hab_Concussion, new DefTalentos()
            {
                Desc1 = "Permite usar Concussion Blow",
                Icone = 20495,
                Nome = "Golpe de Contusao"
            });

            _talentos.Add(Talento.Hab_ColWind, new DefTalentos()
            {
                Desc1 = "Permite usar ColdWind",
                Icone = 20495,
                Nome = "Bafo Frio"
            });

            // nao achei o q diabos disrobe siginifica no uo
            _talentos.Add(Talento.Hab_Disrobe, new DefTalentos()
            {
                Desc1 = "Permite usar Disrobe",
                Icone = 21014,
                Nome = "Fazer Disrobar"
            });

            _talentos.Add(Talento.Hab_MysticArc, new DefTalentos()
            {
                Desc1 = "Permite usar Mystic Arc",
                Icone = 21022,
                Nome = "Tiro Ricocheteado"
            });

            _talentos.Add(Talento.Hab_InfusedThrow, new DefTalentos()
            {
                Desc1 = "Permite usar Infused Throw",
                Icone = 21005,
                Nome = "Arremesso Incutido"
            });

            _talentos.Add(Talento.Hab_ForceOfNature, new DefTalentos()
            {
                Desc1 = "Permite usar Force Of Nature",
                Icone = 20491,
                Nome = "Forca da Natureza"
            });

            _talentos.Add(Talento.Hab_SerpentArrow, new DefTalentos()
            {
                Desc1 = "Permite usar Serpent Arrow",
                Icone = 20480,
                Nome = "Tiro Venenoso"
            });

            _talentos.Add(Talento.Hab_PsyAttack, new DefTalentos()
            {
                Desc1 = "Permite usar Psychic Attack",
                Icone = 2286,
                Nome = "Ataque Psiquico"
            });

            _talentos.Add(Talento.Hab_LightArrow, new DefTalentos()
            {
                Desc1 = "Permite usar Lightning Arrow",
                Icone = 21017,
                Nome = "Tiro Eletrico"
            });

            _talentos.Add(Talento.Hab_ForceArrow, new DefTalentos()
            {
                Desc1 = "Permite usar Force Arrow",
                Icone = 20488,
                Nome = "Tiro Vertigem"
            });

            _talentos.Add(Talento.Hab_Bladeweave, new DefTalentos()
            {
                Desc1 = "Permite usar Bladeweave",
                Icone = 20744,
                Nome = "Arma Inteligente"
            });

            _talentos.Add(Talento.Hab_ArmorPierce, new DefTalentos()
            {
                Desc1 = "Permite usar Armor Pierce",
                Icone = 20992,
                Nome = "Quebra-Armadura"
            });

            _talentos.Add(Talento.Hab_Doubleshot, new DefTalentos()
            {
                Desc1 = "Permite usar Double Shot",
                Icone = 21001,
                Nome = "Tiro Duplo"
            });

            _talentos.Add(Talento.Hab_DuelWeild, new DefTalentos()
            {
                Desc1 = "Permite usar Dual Wield",
                Icone = 20998,
                Nome = "Apunhalda Dupla"
            });

            _talentos.Add(Talento.Hab_Feint, new DefTalentos()
            {
                Desc1 = "Permite usar Feint",
                Icone = 21022,
                Nome = "Esquiva"
            });

            _talentos.Add(Talento.Hab_TalonStrike, new DefTalentos()
            {
                Desc1 = "Permite usar Talon Strike",
                Icone = 20741,
                Nome = "Apunhalada Perigosa"
            });

            _talentos.Add(Talento.Hab_NerveStrike, new DefTalentos()
            {
                Desc1 = "Permite usar Nerve Strike",
                Icone = 20496,
                Nome = "Ataque Nervoso"
            });

            _talentos.Add(Talento.Hab_DefenseMastery, new DefTalentos()
            {
                Desc1 = "Permite usar Defense Mastery",
                Icone = 20742,
                Nome = "Defesa Aumentada"
            });

            _talentos.Add(Talento.Hab_Block, new DefTalentos()
            {
                Desc1 = "Permite usar Block",
                Icone = 20994,
                Nome = "Bloqueia"
            });

            _talentos.Add(Talento.Hab_FrenziedWirlwing, new DefTalentos()
            {
                Desc1 = "Permite usar Frenzied Whirlwind",
                Icone = 21000,
                Nome = "Ataque Agressivo"
            });

            _talentos.Add(Talento.Hab_RidingSwipe, new DefTalentos()
            {
                Desc1 = "Permite usar Riding Swipe",
                Icone = 21005,
                Nome = "Ataque a Montaria"
            });

            _talentos.Add(Talento.Hab_Wirlwind, new DefTalentos()
            {
                Desc1 = "Permite usar Whirlwind Attack",
                Icone = 21004,
                Nome = "Ataque em Area"
            });


            _talentos.Add(Talento.Ferreiro, new DefTalentos()
            {
                Desc1 = "90 Blacksmith, Mining & Tinkering",
                Icone = 21004,
                Nome = "Ferreiro"
            });


            _talentos.Add(Talento.Carpinteiro, new DefTalentos()
            {
                Desc1 = "90 Carpentry, Bowcraft & Lumberjack",
                Icone = 2252,
                Nome = "Carpinteiro"
            });


            _talentos.Add(Talento.Alfaiate, new DefTalentos()
            {
                Desc1 = "90 Tailoring, Herding, Imbuing",
                Icone = 2274,
                Nome = "Alfaiate"
            });

            _talentos.Add(Talento.Hab_Shadowstrike, new DefTalentos()
            {
                Desc1 = "Permite usar Shadow Strike",
                Icone = 21008,
                Nome = "Apunhalada Escondida"
            });

            _talentos.Add(Talento.Hab_ParalizeBlow, new DefTalentos()
            {
                Desc1 = "Permite usar Paralyzing Blow",
                Icone = 21010,
                Nome = "Porrada Paralizante"
            });

            _talentos.Add(Talento.Hab_MovingSHot, new DefTalentos()
            {
                Desc1 = "Permite Ataque em Movimento",
                Icone = 21016,
                Nome = "Ataque em Movimento"
            });

            _talentos.Add(Talento.Hab_AtaqueMortal, new DefTalentos()
            {
                Desc1 = "Permite usar Mortal Strike",
                Icone = 24012,
                Nome = "Apunhalada Mortal"
            });

            _talentos.Add(Talento.Hab_Infectar, new DefTalentos()
            {
                Desc1 = "Permite usar Infectious Strike",
                Icone = 23013,
                Nome = "Apunhalada Infecciosa"
            });

            _talentos.Add(Talento.Hab_DoubleStrike, new DefTalentos()
            {
                Desc1 = "Permite usar Double Strike",
                Icone = 23010,
                Nome = "Apunhalada Dupla"
            });

            _talentos.Add(Talento.Hab_Dismount, new DefTalentos()
            {
                Desc1 = "Permite usar Dismount",
                Icone = 20997,
                Nome = "Desmonta"
            });

            _talentos.Add(Talento.Hab_Disarm, new DefTalentos()
            {
                Desc1 = "Permite usar Disarm",
                Icone = 23005,
                Nome = "Desarma"
            });

            _talentos.Add(Talento.Hab_CrushingBlow, new DefTalentos()
            {
                Desc1 = "Permite usar Crushing Blow",
                Icone = 23002,
                Nome = "Porrada Esmagadora"
            });

            _talentos.Add(Talento.Hab_BleedAttack, new DefTalentos()
            {
                Desc1 = "Permite usar Bleed Attack",
                Icone = 20737,
                Nome = "Fazer Sangrar"
            });

            _talentos.Add(Talento.Hab_ArmorIgnore, new DefTalentos()
            {
                Desc1 = "Permite usar Armor Ignore",
                Icone = 2304,
                Nome = "Ignorar Armaduras"
            });

            _talentos.Add(Talento.Porretes, new DefTalentos()
            {
                Desc1 = "+25% Dano com Macefight",
                Icone = 2282,
                Nome = "Porretes"
            });

            _talentos.Add(Talento.Lancas, new DefTalentos()
            {
                Desc1 = "+25% Dano com Fencing",
                Icone = 2244,
                Nome = "Lancas"
            });

            _talentos.Add(Talento.Machados, new DefTalentos()
            {
                Desc1 = "+25% Dano com Machados",
                Icone = 21020,
                Nome = "Machados"
            });

            _talentos.Add(Talento.Espadas, new DefTalentos()
            {
                Desc1 = "+25% Dano com Espadas",
                Icone = 21015,
                Nome = "Espadas"
            });

            _talentos.Add(Talento.Hastes, new DefTalentos()
            {
                Desc1 = "+25% Dano com Armas de Aste",
                Icone = 21015,
                Nome = "Armas de Aste"
            });

            _talentos.Add(Talento.Magia, new DefTalentos()
            {
                Desc1 = "Max 90 Magery",
                Icone = 24010,
                Nome = "Magia"
            });

            _talentos.Add(Talento.Rastreador, new DefTalentos()
            {
                Desc1 = "Max 90 Tracking, Max 90 Detect Hidden",
                Icone = 24013,
                Nome = "Rastreador"
            });

            _talentos.Add(Talento.Ladrao, new DefTalentos()
            {
                Desc1 = "Max 90 Stealing, Max 90 Snooping",
                Icone = 2290,
                Nome = "Ladrao"
            });

            _talentos.Add(Talento.ResistSpell, new DefTalentos()
            {
                Desc1 = "Max 90 Magic Resist",
                Icone = 2248,
                Nome = "Resistente"
            });


            _talentos.Add(Talento.Regeneracao, new DefTalentos()
            {
                Desc1 = "+200% Regen de Vida Passivo",
                Icone = 39831,
                Nome = "Eterno"
            });

            _talentos.Add(Talento.Bloqueador, new DefTalentos()
            {
                Desc1 = "Max 90 Parry",
                Icone = 2265,
                Nome = "Bloqueador"
            });

            _talentos.Add(Talento.Paladino, new DefTalentos()
            {
                Desc1 = "Max 90 Chivalry, Max 80 Meditation",
                Icone = 2245,
                Nome = "Fe"
            });

            _talentos.Add(Talento.Comandante, new DefTalentos()
            {
                Desc1 = "Permite ter aliados NPCS",
                Icone = 2285,
                Nome = "Lideranca"
            });

            _talentos.Add(Talento.Darknight, new DefTalentos()
            {
                Desc1 = "Max 90 Necromancy, Max 80 Meditation",
                Icone = 2249,
                Nome = "Perdicao"
            });

            _talentos.Add(Talento.Finta, new DefTalentos()
            {
                Desc1 = "-20% Chance de ser bloqueado",
                Icone = 2240,
                Nome = "Finta"
            });

            _talentos.Add(Talento.ArmaduraMagica, new DefTalentos()
            {
                Desc1 = "Medita com armaduras pesadas",
                Icone = 39841,
                Nome = "Armadura Magica"
            });

            _talentos.Add(Talento.ArmaduraPesada, new DefTalentos()
            {
                Desc1 = "Nao perde dex com armaduras",
                Icone = 2246,
                Nome = "Armadura Pesada"
            });

            _talentos.Add(Talento.Experiente, new DefTalentos()
            {
                Desc1 = "+500 EXP",
                Icone = 2247,
                Nome = "Experiente"
            });

            _talentos.Add(Talento.Hipismo, new DefTalentos()
            {
                Desc1 = "Monta sem cair",
                Icone = 20745,
                Nome = "Hipismo"
            });

            _talentos.Add(Talento.Perseveranca, new DefTalentos()
            {
                Desc1 = "Vida +40",
                Icone = 21000,
                Nome = "Perseveranca"
            });

            _talentos.Add(Talento.Defensor, new DefTalentos()
            {
                Desc1 = "+30% Bloqueio para Aliados",
                Icone = 39846,
                Nome = "Defensor"
            });

            _talentos.Add(Talento.EstudoAnatomico, new DefTalentos()
            {
                Desc1 = "70 Healing & Anatomy",
                Icone = 2261,
                Nome = "Estudo Anatomico"
            });

            _talentos.Add(Talento.ArmasEpicas, new DefTalentos()
            {
                Desc1 = "+10% Chance Armas Obras Prima",
                Icone = 20998,
                Nome = "Mestre Armeiro"
            });


            _talentos.Add(Talento.ColetorAvancado, new DefTalentos()
            {
                Desc1 = "+1 Recursos Coletados",
                Icone = 2273,
                Nome = "Coletor Avancado"
            });

            _talentos.Add(Talento.Lockpick, new DefTalentos()
            {
                Desc1 = "90 Lockpicking",
                Icone = 2262,
                Nome = "Chaveiro",
                Aprende = new Dictionary<SkillName, int>() { { SkillName.Lockpicking, 90 } }
            });

            _talentos.Add(Talento.Esquiva, new DefTalentos()
            {
                Desc1 = "+20% Esquiva",
                Icone = 39828,
                Nome = "Agilidade"
            });

            _talentos.Add(Talento.Sabedoria, new DefTalentos()
            {
                Desc1 = "+40 Mana",
                Icone = 39830,
                Nome = "Sabedoria"
            });

            _talentos.Add(Talento.AlquimiaMagica, new DefTalentos()
            {
                Desc1 = "Produz 2 Pocoes usando 1 ingrediente",
                Icone = 23000,
                Nome = "Alquimia Magica"
            });

            _talentos.Add(Talento.Sagacidade, new DefTalentos()
            {
                Desc1 = "Conjurar se movimentando",
                Icone = 39819,
                Nome = "Sagacidade"
            });

            _talentos.Add(Talento.Foco, new DefTalentos()
            {
                Desc1 = "Max 90 Focus",
                Icone = 39820,
                Nome = "Foco"
            });

            _talentos.Add(Talento.Livros, new DefTalentos()
            {
                Desc1 = "50% Economia de Reagentes Usando Livros",
                Icone = 39820,
                Nome = "Bom Leitor"
            });

            _talentos.Add(Talento.Envenenador, new DefTalentos()
            {
                Desc1 = "Max 90 Poisoning",
                Icone = 39820,
                Nome = "Mestre dos Venenos"
            });

            _talentos.Add(Talento.Combate, new DefTalentos()
            {
                Desc1 = "Max 90 Tactics & 40 Arms Lore",
                Icone = 23003,
                Nome = "Arte do Combate"
            });

            _talentos.Add(Talento.MentePerfurante, new DefTalentos()
            {
                Desc1 = "+10% Penetracao Magic Resist",
                Icone = 39820,
                Nome = "Mente Afinada"
            });

            _talentos.Add(Talento.Precisao, new DefTalentos()
            {
                Desc1 = "+25% Acerto",
                Icone = 39839,
                Nome = "Precisao"
            });

            _talentos.Add(Talento.Potencia, new DefTalentos()
            {
                Desc1 = "+20% Bonus Dano",
                Icone = 39840,
                Nome = "Potencia"
            });

            _talentos.Add(Talento.Brutalidade, new DefTalentos()
            {
                Desc1 = "+20% Menos Custo Dex",
                Icone = 39852,
                Nome = "Brutalidade"
            });

            _talentos.Add(Talento.FisicoPerfeito, new DefTalentos()
            {
                Desc1 = "+40 Stamina",
                Icone = 39853,
                Nome = "Fisico Perfeito"
            });

            _talentos.Add(Talento.Forjador, new DefTalentos()
            {
                Desc1 = "+30% Chance Exp",
                Icone = 39822,
                Nome = "Forjador"
            });

            _talentos.Add(Talento.EconomizaRecurso, new DefTalentos()
            {
                Desc1 = "+50% Economiza Recurso ao Falhar Craft",
                Icone = 39822,
                Nome = "Economizador de Recursos"
            });

            _talentos.Add(Talento.Naturalista, new DefTalentos()
            {
                Desc1 = "+3 Recursos por spot",
                Icone = 24001,
                Nome = "Naturalista"
            });

            _talentos.Add(Talento.PeleArcana, new DefTalentos()
            {
                Desc1 = "+10% Resist",
                Icone = 39843,
                Nome = "Pele Arcana"
            });

            _talentos.Add(Talento.ProtecaoPesada, new DefTalentos()
            {
                Desc1 = "+10 Armor",
                Icone = 39845,
                Nome = "Protecao Pesada"
            });

            _talentos.Add(Talento.Silencioso, new DefTalentos()
            {
                Desc1 = "+33% Chance Nao Ser Notado",
                Icone = 21286,
                Nome = "Silencioso"
            });

            _talentos.Add(Talento.Curandeiro, new DefTalentos()
            {
                Desc1 = "Max 90 Healing, -5 Segundos Bands Curar Aliados",
                Icone = 21286,
                Nome = "Curandeiro"
            });

            _talentos.Add(Talento.BurroDeCarga, new DefTalentos()
            {
                Desc1 = "+200 Peso Carregado",
                Icone = 21286,
                Nome = "Burro de Carga",
            });

            _talentos.Add(Talento.DurabilidadeArmas, new DefTalentos()
            {
                Desc1 = "+20% Durabilidade em Armas",
                Icone = 21286,
                Nome = "Armeiro Perfeccionista",
            });

            _talentos.Add(Talento.Armeiro, new DefTalentos()
            {
                Desc1 = "90 Arms Lore (+ armas exp)",
                Icone = 21286,
                Nome = "Armeiro",
                Aprende = new Dictionary<SkillName, int>() { { SkillName.ArmsLore, 90 } }
            });

            _talentos.Add(Talento.Musica, new DefTalentos()
            {
                Desc1 = "90 Musicanship [Craft Instrumentos]",
                Icone = 21286,
                Nome = "Fabricador de Instrumentos",
                Aprende = new Dictionary<SkillName, int>() { { SkillName.Musicianship, 90 } }
            });

            _talentos.Add(Talento.Tinkering, new DefTalentos()
            {
                Desc1 = "90 Tinkering",
                Icone = 2262,
                Nome = "Funileiro Mestre",
                Aprende = new Dictionary<SkillName, int>() { { SkillName.Tinkering, 90 } }
            });

            _talentos.Add(Talento.FabricadorDeCajados, new DefTalentos()
            {
                Desc1 = "+ Cajados Obra Prima",
                Icone = 2289,
                Nome = "Fabricador de Cajados",
            });

            _talentos.Add(Talento.FabricadorDeArcos, new DefTalentos()
            {
                Desc1 = "+ Arcos Obra Prima",
                Icone = 21001,
                Nome = "Fabricador de Arcos",
            });

            _talentos.Add(Talento.FabricadorDeEspadas, new DefTalentos()
            {
                Desc1 = "+ Espadas Obra Prima",
                Icone = 20738,
                Nome = "Fabricador de Espadas",
            });

            _talentos.Add(Talento.FabricadorDeLancas, new DefTalentos()
            {
                Desc1 = "+ Lancas Obra Prima",
                Icone = 2244,
                Nome = "Fabricador de Lancas",
            });

            _talentos.Add(Talento.FabricadorDeMachados, new DefTalentos()
            {
                Desc1 = "+ Machados Obra Prima",
                Icone = 2287,
                Nome = "Fabricador de Machados",
            });

            _talentos.Add(Talento.FabricadorDeArmasDeAste, new DefTalentos()
            {
                Desc1 = "+ Armas de Aste Obra Prima",
                Icone = 2248,
                Nome = "Fabricador de Armas de Haste",
            });

            _talentos.Add(Talento.FabricadorDePorretes, new DefTalentos()
            {
                Desc1 = "+ Porretes Obra Prima",
                Icone = 2280,
                Nome = "Fabricador de Porretes",
            });
        }

      
    }
}
