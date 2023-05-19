using System;
using Server;
using Server.Items;
using Server.Fronteira.Elementos;
using Server.Commands;
using Server.Mobiles;
using Server.Targeting;

namespace CustomCommands
{
    public class TauntCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("taunt", AccessLevel.Player, new CommandEventHandler(Taunt_Command));
        }

        [Usage("taunt")]
        [Description("Dá um urro para chamar a atenção dos inimigos e aumenta a defesa.")]
        private static void Taunt_Command(CommandEventArgs e)
        {
            PlayerMobile player = e.Mobile as PlayerMobile;

            if (player == null)
                return;

            // Verifica se o jogador é de elemento Luz e tem nível maior que 0
            int luzNivel = player.Elementos.GetNivel(ElementoPvM.Luz);

            if (luzNivel > 0 && player.Elemento == ElementoPvM.Luz)
            {
                DateTime nextCommandTime = player.LastTauntCommand.AddSeconds(Math.Max(40 - luzNivel / 2.5, 5));

                if (nextCommandTime > DateTime.Now)
                {
                    TimeSpan timeRemaining = nextCommandTime - DateTime.Now;
                    player.SendMessage($"Você ainda não pode usar este comando novamente, espere {timeRemaining.TotalSeconds:N0} segundos!");
                    return;
                }

                if (player.Stam < 15)
                {
                    player.SendMessage("Você está muito cansado para usar este comando.");
                    return;
                }

                player.Stam -= 15; // Subtrai a stamina

                player.LastTauntCommand = DateTime.Now; // Atualiza a última vez que o comando foi usado

                player.SendMessage("Você usa um taunt e atrai a atenção dos monstros!");

                // Aumenta temporariamente a defesa física e mágica
                int physicalDefenseBonus = 300; // Bônus de defesa física em 300%
                ResistanceMod mod = new ResistanceMod(ResistanceType.Physical, physicalDefenseBonus); // Modificador de resistência
                player.AddResistanceMod(mod);

                double baseMagicResistSkill = player.Skills[SkillName.MagicResist].Base;
                player.Skills[SkillName.MagicResist].Base *= 4; // Multiplica a habilidade MagicResist por 4 para obter o bônus em defesa mágica

                // Emitir som e emote
                player.PlaySound(0x229);
                player.Emote("*Rugido*");

                // Atrair monstros dentro do alcance
                foreach (Mobile mob in player.GetMobilesInRange(18)) // O alcance da visão é tipicamente 18
                {
                    if (mob is BaseCreature creature)
                    {
                        if (creature.Controlled || creature.BardPacified || creature.BardProvoked)
                            continue;

                        creature.Combatant = player;
                    }
                }

                // Criar um timer para remover os bônus após a duração
                Timer.DelayCall(TimeSpan.FromSeconds(6), () =>
                {
                    if (player.Deleted)
                        return;

                    player.SendMessage("O efeito do taunt acabou!");

                    // Remover os bônus de defesa física e mágica
                    player.RemoveResistanceMod(mod);
                    player.Skills[SkillName.MagicResist].Base = baseMagicResistSkill;
                });
            }
            else
            {
                player.SendMessage("Você não possui o elemento Luz!.");
            }
        }
    }
}