using System;
using System.Collections.Generic;
using Server.Accounting;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Commands
{
    public class BancoGuilda
    {
        public static void Initialize()
        {
            CommandSystem.Register("bancoguilda", AccessLevel.Administrator, new CommandEventHandler(CMD));
        }

        public static void CMD(CommandEventArgs arg)
        {
            var from = arg.Mobile;
            var nome = arg.GetString(0);
            if (nome == null)
                return;
            var guilda = BaseGuild.FindByAbbrev(nome);
            if (guilda == null)
                guilda = BaseGuild.FindByName(nome);
            if (guilda == null)
            {
                arg.Mobile.SendMessage("Guilda nao encontrada");
                return;
            }

            var g = guilda as Guild;
            if (g.Banco == null)
            {
                g.Banco = new Fronteira.Guildas.BauDeGuilda(g.Abbreviation);
            }
            if (g.Banco.BoundTo != null)
            {
                from.SendMessage($"O banco de guilda esta sendo usado por {g.Banco.BoundTo}");
            }
            g.Banco.DisplayTo(arg.Mobile);
            arg.Mobile.SendMessage("ABrindo");
        }

        public static void AbreBancoGuilda(Mobile from)
        {
          
        }
    }
}
