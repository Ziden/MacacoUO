

#define RunUo2_0

using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Mobiles;
using System.Linq;
using Server.Multis;

namespace Server.Gumps
{
    public class CampingTimer : Timer
    {
        private Mobile player;
        Point3D point;
        Map m;
        public CampingTimer(Mobile player, Map map, Point3D point)
            : base(TimeSpan.FromSeconds(3))
        {
            this.m = map;
            this.point = point;
            this.player = player;
            Priority = TimerPriority.FiftyMS;
            player.Freeze(TimeSpan.FromSeconds(3));
            player.SendMessage("Voce esta indo ao seu destino");
            player.OverheadMessage("* viajando *");
        }

        protected override void OnTick()
        {
            BaseCreature.TeleportPets(player, point, m);
            player.MoveToWorld(point, m);
            player.SendMessage("Voce chegou ao seu destino");
        }
    }

    public class GumpCamping : Gump
    {
        public static void Initialize()
        {
            CommandSystem.Register("campz", AccessLevel.Administrator, new CommandEventHandler(CMD));
        }

        public static void CMD(CommandEventArgs arg)
        {
            arg.Mobile.SendGump(new GumpCamping(arg.Mobile as PlayerMobile));
        }

        private int pagina;

        public GumpCamping(PlayerMobile camper, int pageAtual = 0) : base(0, 0)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            this.pagina = pageAtual;

            var page = 0;
            AddPage(page);
            AddBackground(88, 82, 313, 321, 9350);

            var campsLiberados = camper.CampfireLocations.Split(';').ToList();
            var y = 0;
            var ct = 0;

            if (camper.CampfireLocations == "")
            {
                AddHtml(120, 126 + y, 272, 25, "Nenhum acampamento", (bool)true, (bool)false);
                return;
            }

            for (var xx = pageAtual * 8; xx < pageAtual * 8 + 8; xx++)
            {
                if (xx < campsLiberados.Count)
                {
                    var camp = campsLiberados[xx];
                    if (Acampamento.Points.ContainsKey(camp))
                    {
                        AddHtml(120, 126 + y, 272, 25, camp, (bool)true, (bool)false);
                        AddButton(94, 121 + y, 2472, 2473, xx + 1, GumpButtonType.Reply, 0);
                        y += 30;
                    }

                }
            }

            AddItem(360, 87, 3557);
            AddItem(81, 86, 3557);
            AddHtml(148, 90, 184, 25, @"Campings Descobertos", (bool)false, (bool)false);
            AddButton(365, 376, 30534, 30534, 666, GumpButtonType.Reply, 0);

            if (pageAtual > 0)
                AddButton(94, 376, 30535, 30535, 999, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            var from = sender.Mobile as PlayerMobile;

            switch (info.ButtonID)
            {
                case 666:
                    {
                        this.pagina++;
                        from.SendGump(new GumpCamping(from, this.pagina));
                        return;
                    }
                case 999:
                    {
                        this.pagina--;
                        from.SendGump(new GumpCamping(from, this.pagina));
                        return;
                    }
            }
            if (info.ButtonID > 0)
            {
                if (from.InCombat())
                {
                    from.SendMessage("Voce esta em combate");
                    return;
                }
                var index = info.ButtonID - 1;
                var campsLiberados = from.CampfireLocations.Split(';').ToList();
                var camp = Acampamento.Points[campsLiberados[index]];
                new CampingTimer(from, camp.Map, camp.Location).Start();
            }
        }
    }
}
