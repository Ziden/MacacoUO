
using Server.Engines.Points;
using Server.Items;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Fronteira.Talentos
{

    public static class TalentoEffect
    {

        public static void GanhaEfeito(PlayerMobile m, Talento t)
        {

            var def = DefTalentos.GetDef(t);

            foreach(var kp in def.Aprende)
            {
                m.Skills[kp.Key].CapApenasMax = kp.Value;
            }
            switch (t)
            {
                //// MODO ANTIGO DE BOTAR SKILL EM TALENTO, NAO USAR MAIS
                case Talento.Experiente:
                    PointsSystem.Exp.AwardPoints(m, 500);
                    break;
                case Talento.Paladino:
                    m.Skills[SkillName.Chivalry].CapApenasMax = 90;
                    m.Skills[SkillName.Meditation].CapApenasMax = 80;
                    m._PlaceInBackpack(new BookOfChivalry());
                    m.TithingPoints = 500;
                    break;

                case Talento.Alfaiate:
                    m.Skills[SkillName.Tailoring].CapApenasMax = 90;
                    m.Skills[SkillName.Herding].CapApenasMax = 90;
                    m.Skills[SkillName.Imbuing].CapApenasMax = 90;
                    break;

                case Talento.Ferreiro:
                    m.Skills[SkillName.Blacksmith].CapApenasMax = 90;
                    m.Skills[SkillName.Mining].CapApenasMax = 90;
                    m.Skills[SkillName.Tinkering].CapApenasMax = 90;
                    break;

                case Talento.Carpinteiro:
                    m.Skills[SkillName.Carpentry].CapApenasMax = 90;
                    m.Skills[SkillName.Bowcraft].CapApenasMax = 90;
                    m.Skills[SkillName.Lumberjacking].CapApenasMax = 90;
                    break;

                case Talento.Darknight:
                    m.Skills[SkillName.Necromancy].CapApenasMax = 90;
                    m.Skills[SkillName.Meditation].CapApenasMax = 80;
                    m._PlaceInBackpack(new NecromancerSpellbook());
                    m._PlaceInBackpack(new AnimateDeadScroll());
                    break;
                case Talento.Comandante: 
                    m.Skills[SkillName.Begging].CapApenasMax = 90;
                    m.SendMessage(78, "Voce pode convencer mercenarios a se unirem a voce usando a skill Begging");
                    break;
                case Talento.Curandeiro:
                    m.Skills[SkillName.Healing].CapApenasMax = 90;
                    break;
                case Talento.Bloqueador:
                    m.Skills[SkillName.Parry].CapApenasMax = 90;
                    break;
                case Talento.Magia:
                    m.Skills[SkillName.Magery].CapApenasMax = 90;
                    m._PlaceInBackpack(new Spellbook());
                    m._PlaceInBackpack(new FireballScroll());
                    m._PlaceInBackpack(new HealScroll());
                    break;
                case Talento.Ladrao:
                    m.Skills[SkillName.Stealing].CapApenasMax = 90;
                    m.Skills[SkillName.Snooping].CapApenasMax = 90;
                    break;
                case Talento.Adagas:
                    m.Skills[SkillName.Fencing].CapApenasMax = 90;
                    break;
                case Talento.Rastreador:
                    m.Skills[SkillName.Tracking].CapApenasMax = 90;
                    m.Skills[SkillName.DetectHidden].CapApenasMax = 90;
                    break;
                case Talento.MusicaBranca:
                    m.Skills[SkillName.Magery].CapApenasMax = 70;
                    m.Skills[SkillName.Inscribe].CapApenasMax = 70;
                    break;
                case Talento.Encantador:
                    m.Skills[SkillName.Veterinary].CapApenasMax = 70;
                    m.Skills[SkillName.AnimalTaming].CapApenasMax = 70;
                    m.Skills[SkillName.AnimalLore].CapApenasMax = 70;
                    break;
                case Talento.Ranger:
                    m.Skills[SkillName.Veterinary].CapApenasMax = 90;
                    m.Skills[SkillName.AnimalTaming].CapApenasMax = 90;
                    m.Skills[SkillName.AnimalLore].CapApenasMax = 90;
                    break;
                case Talento.Assassino:
                    m.Skills[SkillName.Poisoning].CapApenasMax = 90;
                    m.Skills[SkillName.Tactics].CapApenasMax = 90;
                    m.Skills[SkillName.Alchemy].CapApenasMax = 70;
                    m._PlaceInBackpack(new MortarPestle());
                    m._PlaceInBackpack(new Bottle(10));
                    m._PlaceInBackpack(new BagOfReagents(20));
                    break;
                case Talento.CacadorDeTesouros:
                    m.Skills[SkillName.Lockpicking].CapApenasMax = 90;
                    m.Skills[SkillName.RemoveTrap].CapApenasMax = 90;
                    m.Skills[SkillName.Cartography].CapApenasMax = 90;
                    m.Skills[SkillName.DetectHidden].CapApenasMax = 90;
                    break;
                case Talento.Provocacao:
                    m._PlaceInBackpack(new Lute());
                    m.Skills[SkillName.Musicianship].CapApenasMax = 70;
                    m.Skills[SkillName.Provocation].CapApenasMax = 70;
                    break;
                case Talento.AnimalLore:
                    m.Skills[SkillName.AnimalLore].CapApenasMax = 90;
                    break;
                case Talento.Alquimista:
                    m.Skills[SkillName.Alchemy].CapApenasMax = 90;
                    m._PlaceInBackpack(new MortarPestle());
                    m._PlaceInBackpack(new Bottle(10));
                    m._PlaceInBackpack(new BagOfReagents(20));
                    break;
                case Talento.Herbalismo:
                    m.Skills[SkillName.Herding].CapApenasMax = 90;
                    break;
                case Talento.Esconderijo:
                    m.Skills[SkillName.Hiding].CapApenasMax = 90;
                    break;
                case Talento.Investigador:
                    m.Skills[SkillName.DetectHidden].CapApenasMax = 90;
                    m.Skills[SkillName.Forensics].CapApenasMax = 90;
                    m.Skills[SkillName.ItemID].CapApenasMax = 90;
                    break;
                case Talento.BardoGuerreiro:
                    m.Skills[SkillName.Anatomy].CapApenasMax = 90;
                    m.Skills[SkillName.Tactics].CapApenasMax = 90;
                    m.Skills[SkillName.Healing].CapApenasMax = 90;
                    m.Skills[SkillName.Cooking].CapApenasMax = 90;
                    break;
                case Talento.Pacificador:
                    m._PlaceInBackpack(new Lute());
                    m.Skills[SkillName.Peacemaking].CapApenasMax = 70;
                    m.Skills[SkillName.Musicianship].CapApenasMax = 90;
                    break;
                case Talento.Arquimago:
                    m.Skills[SkillName.Inscribe].CapApenasMax = 90;
                    m.Skills[SkillName.Focus].CapApenasMax = 90;
                    m.Skills[SkillName.Imbuing].CapApenasMax = 90;
                    break;
                case Talento.Necromante:
                    m._PlaceInBackpack(new NecromancerSpellbook());
                    m._PlaceInBackpack(new AnimateDeadScroll());
                    m.Skills[SkillName.Necromancy].CapApenasMax = 90;
                    m.Skills[SkillName.SpiritSpeak].CapApenasMax = 90;
                    break;
                case Talento.Foco:
                    m.Skills[SkillName.Focus].CapApenasMax = 90;
                    break;
                case Talento.Envenenador:
                    m.Skills[SkillName.Poisoning].CapApenasMax = 90;
                    break;
                case Talento.Combate:
                    m.Skills[SkillName.Tactics].CapApenasMax = 90;
                    break;
            }
        }
    }

    public enum Talento // sempre adicionar no fim da lista
    {
        Nenhum,

        /// TALENTOS DE SKILLS ///
        Hab_ArmorIgnore,
        Hab_BleedAttack,
        Hab_CrushingBlow,
        Hab_Disarm,
        Hab_Dismount,
        Hab_DoubleStrike,
        Hab_Infectar,
        Hab_AtaqueMortal,
        Hab_MovingSHot,
        Hab_ParalizeBlow,
        Hab_Shadowstrike,
        Hab_Wirlwind,
        Hab_RidingSwipe,
        Hab_FrenziedWirlwing,
        Hab_Block,
        Hab_DefenseMastery,
        Hab_NerveStrike,
        Hab_TalonStrike,
        Hab_Feint,
        Hab_DuelWeild,
        Hab_Doubleshot,
        Hab_ArmorPierce,
        Hab_Bladeweave,
        Hab_ForceArrow,
        Hab_LightArrow,
        Hab_PsyAttack,
        Hab_SerpentArrow,
        Hab_ForceOfNature,
        Hab_InfusedThrow,
        Hab_MysticArc,
        Hab_Disrobe,
        Hab_ColWind,
        Hab_Concussion,
        Hab_MovingBlow,
        AnimalLore,

        // "SubClasses"

        // Subs de Guerreiro
        Paladino,
        Darknight,
        Comandante,

        Ferreiro,
        Carpinteiro,
        Alfaiate,



        Arquimago, // Inscript + Focus + Imbue
        Necromante, // Necro Poisoning
        Feiticeiro, // Buffs & Debuffs fortes

        Encantador, // 80 taming & animal lore & vet
        MusicaBranca,
        BardoGuerreiro,

        Ranger, // 90 taming 90 veterinary
        Assassino, // 90 Poisoning 90 Tactics 70 alchemy
        CacadorDeTesouros, // 90 lockpick 90 remove trap 90  cartography 
      
        /// TALENTOS DE HABILIDADES UNICAS //

        Provocacao,
        Pacificador,

        Dispel,

        CorrerStealth,
        Herbalismo, 
        // 90 Parry
        Bloqueador,
        // 90 Magic Resist
        ResistSpell,
        // 90 Tracking
        Rastreador,
        // 90 Detect Hidden
        Investigador,
        // 90 Stealing+Snooping
        Ladrao,
        // 90 Magery
        Magia,

        // Dano Magico
        Elementalismo,

        // Magias Brancas
        EstudoSagrado,

        Combate,

        // +500 XP "Free"
        Experiente,

        // Montar
        Hipismo,

        Alquimista,

        // Bonus de dano com armas especificas
        Espadas,
        Lancas,
        Porretes,
        Machados,
        Hastes,
        Adagas,

        // Nao perde dex com armadura
        ArmaduraPesada,

        // Menos chance tomar parry
        Finta,

        Lockpick,

        // Vitalidade
        Perseveranca,
        // Parry por outros
        Defensor,
        // Esquiva
        Esquiva,

        Foco,

        // Mana
        Sabedoria,
        // Cast Time - TODO
        Sagacidade,
        // Cast Move
        MentePerfurante,

        Cajados,
        Livros,
        Musica,
        // Bonus Acerto 
        Precisao,
        // Bonus Dano
        Potencia,
        // Custo Power Moves
        Brutalidade,
        // Max Dex
        FisicoPerfeito,

        // Item Exp
        Forjador,
        // Bonus harvest
        Naturalista,
        // Ataques pesados usando dinheiro
        // Mamonita,

        // Resist Magias
        PeleArcana,

        // Resist Fisico
        ProtecaoPesada,

        // Plate Castando
        ArmaduraMagica,

        // Inimigos nao reparam tanto
        Silencioso,
        Envenenador,
        AlquimiaMagica,

        // Corre em stealth
        Gatuno,
        // Cura aliados rapido
        Curandeiro,
        // + regen de HP
        Regeneracao,
        Esconderijo,

        Armeiro,
        BurroDeCarga, 

        DurabilidadeArmas,
        ArmasEpicas,
        ColetorAvancado,

        EstudoAnatomico,
        FabricadorDeCajados,

        FabricadorDeEspadas,
        FabricadorDeLancas,
        FabricadorDeArcos,
        FabricadorDeMachados,
        FabricadorDeArmasDeAste,
        FabricadorDePorretes,
        EconomizaRecurso,

        Tinkering

        // Loja anywhere
        // Comerciante,
    }
}
