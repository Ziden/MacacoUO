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
using Server.Engines.Points;
using Server.Items;
using Server.Network;
#endregion

namespace Server.Commands
{
    public class ZeraRankCmd
    {
        public static void Initialize()
        {
            CommandSystem.Register("zeraunsrank", AccessLevel.Administrator, OnAction);
        }

        [Usage("Action")]
        private static void OnAction(CommandEventArgs e)
        {
            PointsSystem.PontosAlfaiate.Clear();
            PointsSystem.PontosAlquimista.Clear();
            PointsSystem.PontosCarpinteiro.Clear();
            PointsSystem.PontosCozinha.Clear();
            PointsSystem.PontosFerreiro.Clear();
            PointsSystem.PontosLenhador.Clear();
            PointsSystem.PontosMinerador.Clear();
            PointsSystem.PontosPescador.Clear();
            PointsSystem.PontosPvmEterno.Clear();
            PointsSystem.PontosRP.Clear();
            PointsSystem.PontosTaming.Clear();
            PointsSystem.PontosTrabalho.Clear();
            PointsSystem.ViceVsVirtue.Clear();
        } 
    }
}
