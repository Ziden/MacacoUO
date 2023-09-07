using System;
using Server;
using Server.Engines.Help;
using Server.Gumps;
using Server.Items;
using Server.Menus.Questions;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Gumps
{
    public class NewHelpGump : Gump
    {
        public NewHelpGump(Mobile m)
            : base(0, 0)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            this.AddPage(0);
            var sizeY = 336 + 80;

            if (m.IsYoung())
            {
                sizeY += 90;
            }

            this.AddBackground(114, 104, 499, sizeY, 39925);

            this.AddLabel(320, 110, 1153, @"Menu de Ajuda");
            this.AddHtml(172, 130, 1153, 23, @"             D r a g o n i c  S h a r d", (bool)false, (bool)false);

            this.AddHtml(172, 162, 172, 23, @"Enviar Pombo Correio", (bool)true, (bool)false);
            this.AddHtml(172, 186, 410, 44, @"Envie um pombo correio a staff que sera respondido assim que possivel.", 32512,  (bool)false, (bool)false);
            this.AddButton(144, 159, 2472, 2472, (int)Buttons.Page, GumpButtonType.Reply, 0);

            this.AddHtml(171, 246, 172, 23, @"Estou Preso", (bool)true, (bool)false);
            this.AddHtml(173, 271, 408, 54, @"Voce sera congelado por dois minuto e depois sera levado a cidade", 32512, (bool)false, (bool)false);
            this.AddButton(140, 244, 2472, 2472, (int)Buttons.Lock, GumpButtonType.Reply, 0);

            this.AddHtml(172, 341, 172, 23, @"Aonde devo ir ?", (bool)true, (bool)false);
            this.AddHtml(173, 366, 408, 54, @"Mostra locais que voce possa ter interesse em visitar", 32512, (bool)false, (bool)false);
            this.AddButton(140, 340, 2472, 2472, (int)Buttons.Locations, GumpButtonType.Reply, 0);

            this.AddHtml(172, 341+80, 172, 23, @"Comandos", (bool)true, (bool)false);
            this.AddHtml(173, 366+80, 408, 54, @"Mostra os comandos que voce pode usar", 32512, (bool)false, (bool)false);
            this.AddButton(140, 340+80, 2472, 2472, (int)Buttons.Comandos, GumpButtonType.Reply, 0);

            this.AddHtml(172, 341 + 80, 172, 23, @"Comandos", (bool)true, (bool)false);
            this.AddHtml(173, 366 + 80, 408, 54, @"Mostra os comandos que voce pode usar", 32512, (bool)false, (bool)false);
            this.AddButton(140, 340 + 80, 2472, 2472, (int)Buttons.Comandos, GumpButtonType.Reply, 0);

            if (m.IsYoung())
            {
                this.AddHtml(172, 341 + 180, 172, 23, @"Renunciar Status Novato", (bool)true, (bool)false);
                this.AddHtml(173, 366 + 180, 408, 54, @"Renuncia de ser um novato", 32512, (bool)false, (bool)false);
                this.AddButton(140, 340 + 180, 2472, 2472, (int)Buttons.Renuncia, GumpButtonType.Reply, 0);
            }

            
            /* this.AddImage(141, 162, 7814);
             this.AddImage(139, 248, 7814);
             this.AddImage(137, 343, 7814);
             this.AddImage(64, 80, 10440);
             this.AddImage(581, 66, 10441);*/
        }

        public static void PomboCorreio(Mobile from)
        {
            var pombo = new Bird();
            pombo.Tamable = false;
            pombo.Name = "pombo correio";
            pombo.Blessed = true;
            pombo.MoveToWorld(from.Location, from.Map);
            pombo.AIObject.MoveTo(new Point3D(from.Location.X + 10, (from.Location.X + 10), 0), false, 10);
            Timer.DelayCall(TimeSpan.FromSeconds(3), () =>
            {
                pombo.Say("* pruuu *");
            });
            Timer.DelayCall(TimeSpan.FromSeconds(9), () =>
            {
                pombo.Say("* decolando *");
            });
            Timer.DelayCall(TimeSpan.FromSeconds(10), () =>
            {
                pombo.Delete();
                from.SendMessage("Seu pombo correio esta voando para entregar a mensagem");
            });
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            base.OnResponse(sender, info);

            var player = (PlayerMobile)sender.Mobile;
            var from = player;

            switch (info.ButtonID)
            {
                case (int)Buttons.Renuncia:
                    if (from.Young && !from.HasGump(typeof(RenounceYoungGump)))
                    {
                        from.SendGump(new RenounceYoungGump());
                    }
                    break;
                case (int)Buttons.Locations:
                    player.CloseAllGumps();
                    player.SendGump(new OndeIrGump(player));
                    break;
                case (int)Buttons.Page:
                    player.SendGump(new PagePromptGump(player, PageType.Other));
                    return;
                case (int)Buttons.Comandos:
                    player.SendGump(new ListaComandosGump(player));
                    return;
                case (int)Buttons.Lock:
                    BaseHouse house = BaseHouse.FindHouseAt(from);

                    if (house != null && house.IsAosRules)
                    {
                        from.Location = house.BanLocation;
                    }
                    else if (from.Region.IsPartOf<Server.Regions.Jail>())
                    {
                        from.SendLocalizedMessage(1114345, "", 0x35); // You'll need a better jailbreak plan than that!
                    }
                    else if (Factions.Sigil.ExistsOn(from))
                    {
                        from.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
                    }
                    else if (Shard.WARSHARD)
                    {
                        if(CheckCombat(from))
                        {
                            from.SendMessage("Voce nao pode fazer isto em combate");
                            return;
                        }
                        var hall = CharacterCreation.WSHALL;
                        BaseCreature.TeleportPets(from, hall, Map.Malas);
                        from.PlaySound(0x214);
                        from.FixedEffect(0x376A, 10, 16);
                        from.MoveToWorld(hall, Map.Malas);
                        from.Frozen = false;
                        from.SendMessage(0x00FE, "Você retornou ao Hall.");
                    }
                    else
                    {
                        if (from.CanUseStuckMenu() && from.Region.CanUseStuckMenu(from) && !CheckCombat(from) && !from.Frozen && !from.Criminal)
                        {
                            StuckMenu menu = new StuckMenu(from, from, true);
                            menu.BeginClose();
                            from.SendGump(menu);
                        }
                        else
                        {
                            from.SendMessage("Voce precisa aguardar 1 hora para usar a opcao de destravar novamente.");
                        }

                    }
                    return;
            }
        }

        public static bool CheckCombat(Mobile m)
        {
            for (int i = 0; i < m.Aggressed.Count; ++i)
            {
                AggressorInfo info = m.Aggressed[i];

                if (DateTime.UtcNow - info.LastCombatTime < TimeSpan.FromSeconds(30.0))
                    return true;
            }

            return false;
        }

        public enum Buttons
        {
            Nada = 0,
            Page = 1,
            Lock = 2,
            Locations = 3,
            Comandos = 5,
            Renuncia = 4,
            Factions = 6,
        }

    }
}
