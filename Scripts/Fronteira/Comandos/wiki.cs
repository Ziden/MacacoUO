using System;
using System.Collections.Generic;
using Server.Accounting;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Commands
{
    public class Wiki
    {
        public static void Initialize()
        {
            CommandSystem.Register("wiki", AccessLevel.Player, new CommandEventHandler(CMD));
        }

        [Description("Abre a wiki do shard.")]
        public static void CMD(CommandEventArgs arg)
        {
            var pl = arg.Mobile as PlayerMobile;
            pl.LaunchBrowser(" http://www.dragonicage.com/doku/doku.php?id=start");
        }
    }
}
