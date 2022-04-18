using System;
using System.Collections.Generic;
using Server.Accounting;
using Server.Engines.Points;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Commands
{
    public class DebugMobiles
    {
        public static void Initialize()
        {
            CommandSystem.Register("wipegeral", AccessLevel.Owner, new CommandEventHandler(CMD));
        }

        public static void CMD(CommandEventArgs arg)
        {
            arg.Mobile.SendMessage("Iniciando");
            foreach(var player in new List<PlayerMobile>(PlayerMobile.Instances))
            {
                foreach (var pet in new List<Mobile>(player.AllFollowers))
                    pet.Delete();
                player.Delete();
            }
            PlayerMobile.Instances.Clear();
            arg.Mobile.SendMessage("Terminando");
        }

    }
}
