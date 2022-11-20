using Server.Fronteira.Classes;
using Server.Fronteira.Talentos;
using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Commands
{
    public class CmdTalentos
    {

        public static void Initialize()
        {
            CommandSystem.Register("talento", AccessLevel.Player, new CommandEventHandler(CMD));
            CommandSystem.Register("talentos", AccessLevel.Player, new CommandEventHandler(CMD2));
            CommandSystem.Register("aprendetalento", AccessLevel.Administrator, new CommandEventHandler(CMD3));
            CommandSystem.Register("desaprendetalento", AccessLevel.Administrator, new CommandEventHandler(CMD4));
            CommandSystem.Register("resetartalentos", AccessLevel.Administrator, new CommandEventHandler(CMD5));
        }

        [Usage("aprendetalento")]
        private static void CMD3(CommandEventArgs e)
        {
            var talentoStr = e.GetString(0);
            try
            {
                var talento = (Talento)System.Enum.Parse(typeof(Talento), talentoStr);
                ((PlayerMobile)e.Mobile).AprendeTalento(talento);
            }
            catch (Exception ex)
            {
                var ts = System.Enum.GetValues(typeof(Talento));
                var s = "";
                foreach(var t in ts)
                {
                    s += t.ToString() + " ";
                }
                e.Mobile.SendMessage("Nao achei este talento");
                e.Mobile.SendMessage("Talentos registrados: " + s);
            }
        }

        [Usage("aprendetalento")]
        private static void CMD5(CommandEventArgs e)
        {
            var pl = e.Mobile as PlayerMobile;
            if (pl == null) return;

            pl.Talentos.Wipa();
            pl.Nivel = 1;
            pl.ExpTotal = 0;
            pl.PontosTalento = 0;
            pl.SendMessage("Talentos resetados");
        }

        [Usage("desaprendetalento")]
        private static void CMD4(CommandEventArgs e)
        {
            var talentoStr = e.GetString(0);
            try
            {
                var talento = (Talento)System.Enum.Parse(typeof(Talento), talentoStr);
                ((PlayerMobile)e.Mobile).DesaprendeTalento(talento);
            }
            catch (Exception ex)
            {
                var ts = System.Enum.GetValues(typeof(Talento));
                var s = "";
                foreach (var t in ts)
                {
                    s += t.ToString() + " ";
                }
                e.Mobile.SendMessage("Nao achei este talento. Talentos registrados: " + s);

            }
        }

        [Usage("talentos")]
        [Description("Visualiza seus talentos")]
        public static void CMD2(CommandEventArgs arg)
        {
            var player = arg.Mobile as PlayerMobile;
            if (player == null) return;
            var talentos = player.Talentos.ToArray();
            player.SendGump(new GumpLivroTalento(player, talentos));
        }

        [Usage("talento")]
        [Description("Aprende um talento novo")]
        public static void CMD(CommandEventArgs arg)
        {
            var pl = arg.Mobile as PlayerMobile;
            var qtsTalentos = pl.Talentos.Quantidade();
            if(pl.TemTalentoNovo())
            {
                var idClasse = pl.Profession;
                var classe = ClassDef.GetClasse(idClasse);
                if(classe != null)
                {
                    var talentosDoNivel = classe.Talentos[pl.Nivel - 1];
                    pl.SendGump(new GumpAprenderTalento(pl, talentosDoNivel));
                } else
                {
                    pl.SendMessage("Deu algo ruim, voce nao tem classe...");
                }
            } else
            {
                pl.SendMessage("Voce nao tem nenhum talento novo. Digite .xp para ver quanto XP voce precisa para seu proximo talento.");
                if (pl.Talentos.Quantidade() > 0)
                    pl.SendMessage("Digite .talentos para ver seus talentos");
                if (pl.IsStaff())
                    pl.SendMessage("Voce ta nivel " + pl.Nivel.ToString() + " e tem " + pl.Talentos.Quantidade() + " talentos");
            }
        }


        /*
        public static void Initialize()
        {
            CommandSystem.Register("talentos", AccessLevel.Player, new CommandEventHandler(CMD));
            CommandSystem.Register("resettalentos", AccessLevel.GameMaster, new CommandEventHandler(CMD2));
            CommandSystem.Register("vertalentos", AccessLevel.GameMaster, new CommandEventHandler(CMD3));
        }

        [Usage("talentos")]
        [Description("Visualiza seus talentos")]
        public static void CMD(CommandEventArgs arg)
        {
            var pl = arg.Mobile as PlayerMobile;
            pl.SendGump(new GumpLivroTalento(pl, null));
        }


        [Usage("talentos")]
        [Description("Visualiza seus talentos")]
        public static void CMD3(CommandEventArgs arg)
        {
            arg.Mobile.SendMessage("Selecione um player pra resetar os talentos");
            arg.Mobile.Target = new IT();
        }

        [Usage("talentos")]
        [Description("Visualiza seus talentos")]
        public static void CMD2(CommandEventArgs arg)
        {
            arg.Mobile.SendMessage("Selecione um player pra resetar os talentos");
            arg.Mobile.Target = new IT();
        }

        private class IT : Target
        {
            public IT() :base(10, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if(targeted is PlayerMobile)
                {
                    ((PlayerMobile)targeted).Talentos.Wipa();
                    from.SendMessage("Talentos wipados");
                } else
                {
                    from.SendMessage("Isso nao tem talentos");
                }
            }
        }

        private class IT2 : Target
        {
            public IT2() : base(10, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is PlayerMobile)
                {
                    from.SendGump(new GumpLivroTalento((PlayerMobile)targeted, null));
                }
                else
                {
                    from.SendMessage("Isso nao tem talentos");
                }
            }
        }
        */
    }
}
