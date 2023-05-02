#region References
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Server.Commands.Generic;
using Server.Engines.BulkOrders;
using Server.Items;
using Server.Network;
#endregion

namespace Server.Commands
{
    public class WikiCmd
    {
        public static void Initialize()
        {
            CommandSystem.Register("wiki", AccessLevel.Administrator, OnAction);
        }

        [Usage("wiki")]
        private static void OnAction(CommandEventArgs e)
        {
            e.Mobile.LaunchBrowser("http://www.dragonicage.com/doku");
        }
    }
}
