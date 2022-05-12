using Server.Network;
using Server.Commands;
using Server.Mobiles;
using Server.Items;
using Server.Fronteira.Elementos;
using Server.Leilaum;
using System;

namespace Server.Gumps
{
    public class ElementosGump : Gump
    {
        public static void Initialize()
        {
            CommandSystem.Register("elementos", AccessLevel.Player, new CommandEventHandler(_OnCommand));
        }

        [Usage("")]
        [Description("Makes a call to your custom gump.")]
        public static void _OnCommand(CommandEventArgs e)
        {
            EstatuaElemental.Envia(e.Mobile);
        }

        private ElementoPvM e;

        public ElementosGump(PlayerMobile pl, ElementoPvM elemento = ElementoPvM.None) : base(0, 0)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            e = elemento;
            AddPage(0);
            AddBackground(306, 127, 642, 338, 9350);

            AddImageTiled(362, 201, 110, 10, 2053);
            var pct = (pl.Elementos.GetExp(ElementoPvM.Fogo) / ElementoUtils.CustoUpExp(pl.Elementos.GetNivel(ElementoPvM.Fogo))) * 100;
            AddImageTiled(362, 201, (int)(pct * 1.1), 10, 2054);
            AddHtml(365, 173, 71, 19, @"Fogo", (bool)false, (bool)false);

            AddImageTiled(518, 200, 110, 10, 2053);
            pct = (pl.Elementos.GetExp(ElementoPvM.Raio) / ElementoUtils.CustoUpExp(pl.Elementos.GetNivel(ElementoPvM.Raio))) * 100;
            AddImageTiled(518, 200, (int)(pct * 1.1), 10, 2054);
            AddHtml(521, 172, 71, 19, @"Raio", (bool)false, (bool)false);

            AddImageTiled(674, 201, 110, 10, 2053);
            pct = (pl.Elementos.GetExp(ElementoPvM.Gelo) / ElementoUtils.CustoUpExp(pl.Elementos.GetNivel(ElementoPvM.Gelo))) * 100;
            AddImageTiled(674, 201, (int)(pct * 1.1), 10, 2054);
            AddHtml(677, 173, 71, 19, @"Gelo", (bool)false, (bool)false);

            AddImageTiled(830, 200, 110, 10, 2053);
            pct = (pl.Elementos.GetExp(ElementoPvM.Agua) / ElementoUtils.CustoUpExp(pl.Elementos.GetNivel(ElementoPvM.Agua))) * 100;
            AddImageTiled(830, 200, (int)(pct * 1.1), 10, 2054);
            AddHtml(833, 172, 86, 19, @"Agua", (bool)false, (bool)false);

            AddImageTiled(362, 248, 110, 10, 2053);
            pct = (pl.Elementos.GetExp(ElementoPvM.Terra) / ElementoUtils.CustoUpExp(pl.Elementos.GetNivel(ElementoPvM.Terra))) * 100;
            AddImageTiled(362, 248, (int)(pct * 1.1), 10, 2054);
            AddHtml(365, 220, 71, 19, @"Terra", (bool)false, (bool)false);

            AddImageTiled(521, 219 + 25, 110, 10, 2053);
            pct = (pl.Elementos.GetExp(ElementoPvM.Vento) / ElementoUtils.CustoUpExp(pl.Elementos.GetNivel(ElementoPvM.Vento))) * 100;
            AddImageTiled(521, 219 + 25, (int)(pct * 1.1), 10, 2054);
            AddHtml(521, 219, 71, 19, @"Vento", (bool)false, (bool)false);

            pct = (pl.Elementos.GetExp(ElementoPvM.Luz) / ElementoUtils.CustoUpExp(pl.Elementos.GetNivel(ElementoPvM.Luz))) * 100;
            AddImageTiled(677, 220 + 25, 96, 7, 2053);
            AddImageTiled(677, 220 + 25, (int)(pct * 1.1), 10, 2054);
            AddHtml(677, 220, 71, 19, @"Luz", (bool)false, (bool)false);

            AddImageTiled(830, 247, 110, 10, 2053);
            pct = (pl.Elementos.GetExp(ElementoPvM.Escuridao) / ElementoUtils.CustoUpExp(pl.Elementos.GetNivel(ElementoPvM.Escuridao))) * 100;
            AddImageTiled(830, 247, (int)(pct * 1.1), 10, 2054);
            AddHtml(833, 219, 90, 19, @"Escuridao", (bool)false, (bool)false);

            AddButton(628, 214, 20742, 20742, (int)ElementoPvM.Luz, GumpButtonType.Reply, 0);
            AddButton(472, 165, 2288, 2288, (int)ElementoPvM.Raio, GumpButtonType.Reply, 0);
            AddButton(628, 165, 21002, 21002, (int)ElementoPvM.Gelo, GumpButtonType.Reply, 0);
            AddButton(784, 166, 2303, 2303, (int)ElementoPvM.Agua, GumpButtonType.Reply, 0);
            AddButton(315, 211, 2294, 2294, (int)ElementoPvM.Terra, GumpButtonType.Reply, 0);
            AddButton(472, 211, 20740, 20740, (int)ElementoPvM.Vento, GumpButtonType.Reply, 0);
            AddButton(315, 161, 2245, 2245, (int)ElementoPvM.Fogo, GumpButtonType.Reply, 0);
            AddButton(783, 212, 20495, 20495, (int)ElementoPvM.Escuridao, GumpButtonType.Reply, 0);

            AddHtml(543, 136, 123, 20, @"Elementos PvM", (bool)false, (bool)false);
            AddBackground(316, 263, 620, 189, 3500);
            AddImage(254, 62, 10440);
            AddImage(916, 65, 10441);

            if (elemento != ElementoPvM.None)
            {
                var nivel = pl.Elementos.GetNivel(elemento);
                AddHtml(533, 282, 124, 20, "Nivel: " + nivel, (bool)false, (bool)false);
                AddHtml(403, 282, 124, 20, Gump.Cor(elemento.ToString(), BaseArmor.CorElemento(elemento)), (bool)false, (bool)false);
                AddImage(344, 283, (int)elemento);
                AddBackground(331, 329, 299, 101, 3500);

                var efeitos = EfeitosElementos.GetEfeitos(elemento);
                var bonusAtual = pl.Elementos.BonusPorNivel(nivel) * 100;
                var proximoNivel = pl.Elementos.BonusPorNivel(nivel + 1) * 100;
                var ganho = proximoNivel - bonusAtual;

                var str = "";
                for (var x = 0; x < efeitos.Length; x++)
                {
                    var efs = efeitos[x].Split(':');
                    var ratio = Double.Parse(efs[0]);
                    str += Gump.Cor((bonusAtual*ratio).ToString("0.00") + "%", "brown") + " -> " + Gump.Cor((proximoNivel*ratio).ToString("0.00") + "% ", "green") + efs[1] + "<br>";
                }
                AddHtml(355, 342, 249, 73, str, (bool)false, (bool)false);
                AddHtml(404, 316, 124, 20, @"Prox Nivel:", (bool)false, (bool)false);
                AddHtml(784, 315, 124, 20, @"Subir de Nivel", (bool)false, (bool)false);

                var custos = ElementoUtils.GetCustos(elemento);

                var qtdItems = ElementoUtils.QuantidadeItems(nivel);
                var qtdOuro = qtdItems;
                if (nivel > 20)
                    qtdOuro *= 2;
                AddBackground(673, 334, 111, 101, 3500);
                AddHtml(711, 350, 183, 22, (qtdOuro).ToString() + "K", (bool)false, (bool)false);
                AddHtml(678, 406, 100, 22, "Ouro", (bool)true, (bool)false);
                //AddItem(703, 374, custos.Item);
                NewAuctionGump.AddItemCentered(673, 334, 111, 101, 3823, 0, this);

                AddBackground(784, 335, 111, 101, 3500);
                AddHtml(827, 350, 83, 22, qtdItems.ToString(), (bool)false, (bool)false);
                AddHtml(793, 405, 100, 22, custos[nivel <= 20 ? 0 : 1].name, (bool)true, (bool)false);
                //AddItem(811, 367, 576);
                NewAuctionGump.AddItemCentered(784, 335, 111, 101, custos[nivel <= 20 ? 0 : 1].itemID, custos[nivel <= 20 ? 0 : 1].hue, this);

                AddHtml(534, 317, 324, 20, "Exp: " + pl.Elementos.GetExp(elemento) + " / " + ElementoUtils.CustoUpExp(nivel), (bool)false, (bool)false);
                AddButton(900, 435, 30534, 30534, (int)ElementoButtons.Upar, GumpButtonType.Reply, 0);
            }
            else
            {
                AddHtml(336, 283, 523, 20, @"Equipe sets de armadura do elemento e ganhe XP para upar.", (bool)false, (bool)false);
            }
        }

        public enum ElementoButtons
        {
            Nads,
            Upar,
        }


        public override void OnResponse(NetState sender, RelayInfo info)
        {
            var from = sender.Mobile as PlayerMobile;

            switch (info.ButtonID)
            {
                case (int)ElementoPvM.Luz:
                    {
                        from.SendGump(new ElementosGump(from, ElementoPvM.Luz));
                        break;
                    }
                case (int)ElementoPvM.Raio:
                    {

                        from.SendGump(new ElementosGump(from, ElementoPvM.Raio));
                        break;
                    }
                case (int)ElementoPvM.Gelo:
                    {
                        from.SendGump(new ElementosGump(from, ElementoPvM.Gelo));
                        break;
                    }
                case (int)ElementoPvM.Agua:
                    {
                        from.SendGump(new ElementosGump(from, ElementoPvM.Agua));
                        break;
                    }
                case (int)ElementoPvM.Terra:
                    {
                        from.SendGump(new ElementosGump(from, ElementoPvM.Terra));
                        break;
                    }
                case (int)ElementoPvM.Vento:
                    {
                        from.SendGump(new ElementosGump(from, ElementoPvM.Vento));
                        break;
                    }
                case (int)ElementoPvM.Fogo:
                    {
                        from.SendGump(new ElementosGump(from, ElementoPvM.Fogo));
                        break;
                    }
                case (int)ElementoPvM.Escuridao:
                    {
                        from.SendGump(new ElementosGump(from, ElementoPvM.Escuridao));
                        break;
                    }
                case (int)1:
                    {
                        var nivel = from.Elementos.GetNivel(e);
                        var exp = from.Elementos.GetExp(e);
                        var expPrecisa = ElementoUtils.CustoUpExp(nivel);
                        if (exp < expPrecisa)
                        {
                            from.SendMessage("Voce tem " + exp + " exp neste elemento. Para subir o nivel precisa de um total de " + expPrecisa + " exp");
                            return;
                        }
                        var itemPrecisa = ElementoUtils.GetCustos(e)[0];
                        var itemPrecisaPower = ElementoUtils.GetCustos(e)[1];

                        if (nivel > 20)
                            itemPrecisa = itemPrecisaPower;

                        var qtdPrecisa = ElementoUtils.QuantidadeItems(nivel);
                        var qtdOuro = qtdPrecisa;
                        if (nivel > 20)
                            qtdOuro *= 2;
                        if (!sender.Mobile.Backpack.HasItem(itemPrecisa.type, qtdPrecisa, true))
                        {
                            from.SendMessage("Voce precisa de " + qtdPrecisa + "x " + itemPrecisa.name + " para isto");
                            return;
                        }
                        else if (!Banker.Withdraw(sender.Mobile, qtdOuro * 1000))
                        {
                            from.SendMessage("Voce precisa de " + qtdOuro * 1000 + " moedas de ouro para isto");
                            return;
                        }
                        else
                        {
                            from.Backpack.ConsumeTotal(new System.Type[] { itemPrecisa.type }, new int[] { qtdPrecisa });
                        }

                        Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
                        Effects.PlaySound(from.Location, from.Map, 0x243);

                        Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                        Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                        Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

                        Effects.SendTargetParticles(from, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);

                        from.Elementos.SetExp(e, 0);
                        from.Elementos.SetNivel(e, (ushort)(nivel + 1));
                        from.InvalidateProperties();
                        from.SendMessage("Voce sente mais poder em seu corpo");
                        from.CloseAllGumps();
                        from.SendGump(new ElementosGump(from, e));

                        if(nivel > 1 && from.Young)
                        {
                            if(from.Wisp != null)
                            {
                                from.Wisp.Delete();
                            }
                            from.Young = false;
                        }

                        break;
                    }

            }
        }
    }
}
