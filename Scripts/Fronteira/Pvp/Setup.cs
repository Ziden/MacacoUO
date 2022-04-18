using System;

using Server;
using Server.Commands;
using Server.Mobiles;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public static class Setup
    {
        public static void Initialize()
        {
            CommandSystem.Register("ArenaConfig", AccessLevel.Administrator, SetupGump);
        }

        public static void SetupGump(CommandEventArgs e)
        {
            BaseGump.SendGump(new SystemSetupGump(e.Mobile as PlayerMobile));
        }
    }

    public class SystemSetupGump : BaseTournamentGump
    {
        public SystemSetupGump(PlayerMobile pm)
            : base(pm, 50, 50)
        {
        }

        public override void AddGumpLayout()
        {
            AddBackground(0, 0, 400, 400, DarkBackground);
            AddBackground(10, 10, 380, 380, LightBackground);

            AddHtml(0, 10, 400, 20, ColorAndCenter("#FFFFFF", "PVP Tournament Setup Menu"), false, false);

            AddHtml(15, 30, 370, 100, _Description, true, true);

            AddCheck(15, 140, 210, 211, ArenaHelper.HasArena<HavenArena>(), 1);
            AddHtml(37, 140, 350, 20, String.Format("Arena {0}", ArenaHelper.HasArena<HavenArena>() ? "<basefont color=green>[Setup]" : "<basefont color=red>[Not Setup]"), false, false);
            AddButton(356, 140, 4014, 4015, 1, GumpButtonType.Reply, 0);

            /*
            AddCheck(15, 165, 210, 211, ArenaHelper.HasArena<HavenFelArena>(), 2);
            AddHtml(37, 165, 350, 20, String.Format("Ocllo Arena (Felucca) {0}", ArenaHelper.HasArena<HavenFelArena>() ? "<basefont color=green>[Setup]" : "<basefont color=red>[Not Setup]"), false, false);
            AddButton(356, 165, 4014, 4015, 2, GumpButtonType.Reply, 0);

            AddCheck(15, 190, 210, 211, ArenaHelper.HasArena<KhaldunArenaTram>(), 3);
            AddHtml(37, 190, 350, 20, String.Format("Khaldun Grand Arena (Trammel) {0}", ArenaHelper.HasArena<KhaldunArenaTram>() ? "<basefont color=green>[Setup]" : "<basefont color=red>[Not Setup]"), false, false);
            AddButton(356, 190, 4014, 4015, 3, GumpButtonType.Reply, 0);

            AddCheck(15, 215, 210, 211, ArenaHelper.HasArena<KhaldunArena>(), 4);
            AddHtml(37, 215, 350, 20, String.Format("Khaldun Grand Arena (Felucca) {0}", ArenaHelper.HasArena<KhaldunArena>() ? "<basefont color=green>[Setup]" : "<basefont color=red>[Not Setup]"), false, false);
            AddButton(356, 215, 4014, 4015, 4, GumpButtonType.Reply, 0);

            AddCheck(15, 240, 210, 211, ArenaHelper.HasArena<CTFArena1>(), 5);
            AddHtml(37, 240, 350, 20, String.Format("Capture the Flag Arena 1 {0}", ArenaHelper.HasArena<CTFArena1>() ? "<basefont color=green>[Setup]" : "<basefont color=red>[Not Setup]"), false, false);
            AddButton(356, 240, 4014, 4015, 5, GumpButtonType.Reply, 0);

            AddCheck(15, 265, 210, 211, ArenaHelper.HasArena<CTFArena2>(), 6);
            AddHtml(37, 265, 350, 20, String.Format("Capture the Flag Arena 2 {0}", ArenaHelper.HasArena<CTFArena2>() ? "<basefont color=green>[Setup]" : "<basefont color=red>[Not Setup]"), false, false);
            AddButton(356, 265, 4014, 4015, 6, GumpButtonType.Reply, 0);

            AddCheck(15, 290, 210, 211, ArenaHelper.HasArena<TokunoArena>(), 7);
            AddHtml(37, 290, 350, 20, String.Format("Tokuno Arena {0}", ArenaHelper.HasArena<TokunoArena>() ? "<basefont color=green>[Setup]" : "<basefont color=red>[Not Setup]"), false, false);
            AddButton(356, 290, 4014, 4015, 7, GumpButtonType.Reply, 0);

            AddCheck(15, 315, 210, 211, ArenaHelper.HasArena<CTFRoyalRumbleArena>(), 8);
            AddHtml(37, 315, 350, 20, String.Format("CTF Royal Rumble {0}", ArenaHelper.HasArena<CTFRoyalRumbleArena>() ? "<basefont color=green>[Setup]" : "<basefont color=red>[Not Setup]"), false, false);
            AddButton(356, 315, 4014, 4015, 8, GumpButtonType.Reply, 0);

            AddCheck(15, 340, 210, 211, ArenaHelper.HasArena<MalasArena>(), 9);
            AddHtml(37, 340, 350, 20, String.Format("Arena de Malas {0}", ArenaHelper.HasArena<MalasArena>() ? "<basefont color=green>[Setup]" : "<basefont color=red>[Not Setup]"), false, false);
            AddButton(356, 340, 4014, 4015, 9, GumpButtonType.Reply, 0);
                */
            AddButton(11, 367, 4005, 4006, 100, GumpButtonType.Reply, 0);
            AddHtml(44, 367, 100, 20, Color("#FFFFFF", "Setup Arenas"), false, false);

            AddButton(356, 367, 4017, 4018, 101, GumpButtonType.Reply, 0);
            AddHtml(253, 367, 100, 20, ColorAndAlignRight("#FFFFFF", "Remove Arenas"), false, false);
        
        }

        private string _Description =
            "Please Read:<br>When activating each arena, it will automatically remove the correpsonding EA PVP Tournament System Arena. The Capture the Flag Arenas are the only arenas that don't have a corresponding stock PVP arena. " +
            "By auto-deleting the stock ServUO Arena's, any statistics associated with that arena will be lost forever.<br><br>On the flip side, deleting a custom PVP arena will delete and remove any tournament infomation associated to that arena.";

        public override void OnResponse(RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 0: return;
                case 1:
                    User.MoveToWorld(new Point3D(3786, 2762, 6), Map.Trammel);
                    break;
                case 2:
                    User.MoveToWorld(new Point3D(3775, 2758, 6), Map.Felucca);
                    break;
                case 3:
                    User.MoveToWorld(new Point3D(6103, 3723, 25), Map.Trammel);
                    break;
                case 4: 
                    User.MoveToWorld(new Point3D(6103, 3723, 25), Map.Felucca);
                    break;
                case 5:
                    User.MoveToWorld(new Point3D(5221, 1287, 0), Map.Felucca);
                    break;
                case 6:
                    User.MoveToWorld(new Point3D(5226, 1499, 0), Map.Felucca);
                    break;
                case 7:
                    User.MoveToWorld(new Point3D(370, 1045, 11), Map.Tokuno);
                    break;
                case 8:
                    User.MoveToWorld(new Point3D(5951, 447, 22), Map.Felucca);
                    break;
                case 9:
                    User.MoveToWorld(new Point3D(370, 1045, 11), Map.Malas);
                    break;
                default:

                    bool setup = info.ButtonID == 100;

                    if (info.IsSwitched(1))
                    {
                        if (setup)
                            HavenArena.Setup(User);
                        else
                            HavenArena.Delete(User);
                    }

                    /*
                    if (info.IsSwitched(2))
                    {
                        if (setup)
                            HavenFelArena.Setup(User);
                        else
                            HavenFelArena.Delete(User);
                    }

                    if (info.IsSwitched(3))
                    {
                        if (setup)
                            KhaldunArenaTram.Setup(User);
                        else
                            KhaldunArenaTram.Delete(User);
                    }

                    if (info.IsSwitched(4))
                    {
                        if (setup)
                            KhaldunArena.Setup(User);
                        else
                            KhaldunArena.Delete(User);
                    }

                    if (info.IsSwitched(5))
                    {
                        if (setup)
                            CTFArena1.Setup(User);
                        else
                            CTFArena1.Delete(User);
                    }

                    if (info.IsSwitched(6))
                    {
                        if (setup)
                            CTFArena2.Setup(User);
                        else
                            CTFArena2.Delete(User);
                    }

                    if (info.IsSwitched(7))
                    {
                        if (setup)
                            TokunoArena.Setup(User);
                        else
                            TokunoArena.Delete(User);
                    }

                    if (info.IsSwitched(8))
                    {
                        if (setup)
                            CTFRoyalRumbleArena.Setup(User);
                        else
                            CTFRoyalRumbleArena.Delete(User);
                    }

                    if (info.IsSwitched(9))
                    {
                        if (setup)
                            MalasArena.Setup(User);
                        else
                            MalasArena.Delete(User);
                    }
                    */
                    Timer.DelayCall(TimeSpan.FromSeconds(.5), () => Refresh());

                    return;
            }

            Refresh();
        }
    }
}
