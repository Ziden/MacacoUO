using System;
using Server.Accounting;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Commands
{
    public class WipaPratinhas
    {
        public static void Initialize()
        {
            CommandSystem.Register("voltayoung", AccessLevel.Administrator, new CommandEventHandler(CMD));
            CommandSystem.Register("wipayoung", AccessLevel.Administrator, new CommandEventHandler(CMD2));
        }

        [Usage("wipayoung")]
        [Description("Valida e tira jogadores qn sao mais young.")]
        public static void CMD2(CommandEventArgs arg)
        {
            foreach (var gm in PlayerMobile.Instances)
            {
                if (gm == null || !gm.Young)
                    continue;

                foreach (var e in new ElementoPvM[] { ElementoPvM.Agua, ElementoPvM.Fogo, ElementoPvM.Terra, ElementoPvM.Gelo, ElementoPvM.Vento, ElementoPvM.Raio, ElementoPvM.Luz, ElementoPvM.Escuridao })
                {
                    if (gm.Elementos.GetNivel(e) >= 3)
                    {
                        var acc = gm.Account as Account;
                        acc.Young = false;
                        gm.Young = false;
                    }
                }
            }
            arg.Mobile.SendMessage("Foi");
        }

        [Usage("voltayoung")]
        [Description("Volta todos players q devem ser young pra young.")]
        public static void CMD(CommandEventArgs arg)
        {

            foreach (var gm in PlayerMobile.Instances)
            {
                if (gm == null || gm.Young)
                    continue;

                var acc = gm.Account as Account;
                if (acc.TotalGameTime < Account.YoungDuration)
                {
                    acc.Young = true;
                    gm.Young = true;
                    arg.Mobile.SendMessage(gm.Name + "Virou Young");
                }
            }
            arg.Mobile.SendMessage("Foi");
        }
    }
}
