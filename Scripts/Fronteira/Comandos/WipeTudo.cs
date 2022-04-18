using System;
using System.Collections.Generic;
using Server.Accounting;
using Server.Engines.Points;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;

namespace Server.Commands
{
    public class WipeGeral
    {
        public static void Initialize()
        {
            CommandSystem.Register("wipegeral", AccessLevel.Owner, new CommandEventHandler(CMD));
            CommandSystem.Register("wipeskillcap", AccessLevel.Owner, new CommandEventHandler(CMD2));
            CommandSystem.Register("wipepets", AccessLevel.Owner, new CommandEventHandler(CMD3));
            CommandSystem.Register("wipecasas", AccessLevel.Owner, new CommandEventHandler(CMD4));
        }

        public static void CMD2(CommandEventArgs arg)
        {
            arg.Mobile.SendMessage("Wipando");
            foreach(var player in new List<PlayerMobile>(PlayerMobile.Instances))
            {
                foreach (var pet in new List<Mobile>(player.AllFollowers))
                    pet.Delete();
                player.Delete();
            }
            PlayerMobile.Instances.Clear();
            arg.Mobile.SendMessage("Wipado");
        }

        public static void CMD3(CommandEventArgs arg)
        {
            arg.Mobile.SendMessage("Wipando");
            foreach (var m in new List<Mobile>(World.Mobiles.Values))
            {
                if (m is BaseCreature)
                {
                    var bc = m as BaseCreature;
                    if(bc.Controlled || bc.ControlMaster is PlayerMobile)
                        m.Delete();
                }
                  
            }
            arg.Mobile.SendMessage("Wipado");
        }

        public static void CMD4(CommandEventArgs arg)
        {
            arg.Mobile.SendMessage("Wipando");
            foreach (var m in new List<BaseHouse>(BaseHouse.GetAll()))
            {
                m.Delete();
            }
            arg.Mobile.SendMessage("Wipado");
        }


        [Usage("receitas")]
        [Description("Camping Menu.")]
        public static void CMD(CommandEventArgs arg)
        {
            arg.Mobile.SendMessage("Wipando");
            var all = new List<IAccount>(Accounts.GetAccounts());
            foreach (var acc in all)
            {
                if (acc.AccessLevel >= AccessLevel.VIP)
                {
                    continue;
                }
                acc.Delete();
            }
            arg.Mobile.SendMessage("Wipado");
        }
    }
}
