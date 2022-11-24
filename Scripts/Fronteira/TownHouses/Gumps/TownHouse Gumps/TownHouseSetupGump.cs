#region References
using System;

using Server;
using Server.Targeting;
#endregion

namespace Knives.TownHouses
{
	public class TownHouseSetupGump : GumpPlusLight
	{
		public static Rectangle2D FixRect(Rectangle2D rect)
		{
			var pointOne = Point3D.Zero;
			var pointTwo = Point3D.Zero;

			if (rect.Start.X < rect.End.X)
			{
				pointOne.X = rect.Start.X;
				pointTwo.X = rect.End.X;
			}
			else
			{
				pointOne.X = rect.End.X;
				pointTwo.X = rect.Start.X;
			}

			if (rect.Start.Y < rect.End.Y)
			{
				pointOne.Y = rect.Start.Y;
				pointTwo.Y = rect.End.Y;
			}
			else
			{
				pointOne.Y = rect.End.Y;
				pointTwo.Y = rect.Start.Y;
			}

			return new Rectangle2D(pointOne, pointTwo);
		}

		public enum Page
		{
			Welcome,
			Blocks,
			Floors,
			Sign,
			Ban,
			LocSec,
			Items,
			Length,
			Price,
			Skills,
			Other,
			Other2
		}

		public enum TargetType
		{
			BanLoc,
			SignLoc,
			MinZ,
			MaxZ,
			BlockOne,
			BlockTwo
		}

		private readonly TownHouseSign c_Sign;
		private Page c_Page;
		private bool c_Quick;

		public TownHouseSetupGump(Mobile m, TownHouseSign sign)
			: base(m, 50, 50)
		{
			m.CloseGump(typeof(TownHouseSetupGump));

			c_Sign = sign;
		}

		protected override void BuildGump()
		{
			if (c_Sign == null)
			{
				return;
			}

			var width = 300;
			var y = 0;

			if (c_Quick)
			{
				QuickPage(width, ref y);
			}
			else
			{
				switch (c_Page)
				{
					case Page.Welcome:
						WelcomePage(width, ref y);
						break;
					case Page.Blocks:
						BlocksPage(width, ref y);
						break;
					case Page.Floors:
						FloorsPage(width, ref y);
						break;
					case Page.Sign:
						SignPage(width, ref y);
						break;
					case Page.Ban:
						BanPage(width, ref y);
						break;
					case Page.LocSec:
						LocSecPage(width, ref y);
						break;
					case Page.Items:
						ItemsPage(width, ref y);
						break;
					case Page.Length:
						LengthPage(width, ref y);
						break;
					case Page.Price:
						PricePage(width, ref y);
						break;
					case Page.Skills:
						SkillsPage(width, ref y);
						break;
					case Page.Other:
						OtherPage(width, ref y);
						break;
					case Page.Other2:
						OtherPage2(width, ref y);
						break;
				}

				BuildTabs(width, ref y);
			}

			AddBackgroundZero(0, 0, width, y += 30, 0x13BE);

			if (c_Sign.PriceReady && !c_Sign.Owned)
			{
				AddBackground(width / 2 - 50, y, 100, 30, 0x13BE);
				AddHtml(width / 2 - 50 + 25, y + 5, 100, "Pegar Casa");
				AddButton(width / 2 - 50 + 5, y + 10, 0x837, 0x838, "Claim", Claim);
			}
		}

		private void BuildTabs(int width, ref int y)
		{
			var x = 20;

			y += 30;

			AddButton(x - 5, y - 3, 0x768, "Quick", Quick);
			AddLabel(x, y - 3, c_Quick ? 0x34 : 0x47E, "Q");

			AddButton(x += 20, y, c_Page == Page.Welcome ? 0x939 : 0x93A, "Welcome Page", ChangePage, 0);
			AddButton(x += 20, y, c_Page == Page.Blocks ? 0x939 : 0x93A, "Blocks Page", ChangePage, 1);

			if (c_Sign.BlocksReady)
			{
				AddButton(x += 20, y, c_Page == Page.Floors ? 0x939 : 0x93A, "Floors Page", ChangePage, 2);
			}

			if (c_Sign.FloorsReady)
			{
				AddButton(x += 20, y, c_Page == Page.Sign ? 0x939 : 0x93A, "Sign Page", ChangePage, 3);
			}

			if (c_Sign.SignReady)
			{
				AddButton(x += 20, y, c_Page == Page.Ban ? 0x939 : 0x93A, "Ban Page", ChangePage, 4);
			}

			if (c_Sign.BanReady)
			{
				AddButton(x += 20, y, c_Page == Page.LocSec ? 0x939 : 0x93A, "LocSec Page", ChangePage, 5);
			}

			if (c_Sign.LocSecReady)
			{
				AddButton(x += 20, y, c_Page == Page.Items ? 0x939 : 0x93A, "Items Page", ChangePage, 6);

				if (!c_Sign.Owned)
				{
					AddButton(x += 20, y, c_Page == Page.Length ? 0x939 : 0x93A, "Length Page", ChangePage, 7);
				}
				else
				{
					x += 20;
				}

				AddButton(x += 20, y, c_Page == Page.Price ? 0x939 : 0x93A, "Price Page", ChangePage, 8);
			}

			if (c_Sign.PriceReady)
			{
				AddButton(x += 20, y, c_Page == Page.Skills ? 0x939 : 0x93A, "Skills Page", ChangePage, 9);
				AddButton(x += 20, y, c_Page == Page.Other ? 0x939 : 0x93A, "Other Page", ChangePage, 10);
				AddButton(x += 20, y, c_Page == Page.Other2 ? 0x939 : 0x93A, "Other Page 2", ChangePage, 11);
			}
		}

		private void QuickPage(int width, ref int y)
		{
			c_Sign.ClearPreview();

			AddHtml(0, y += 10, width, "<CENTER>Setup Rapido");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddButton(5, 5, 0x768, "Quick", Quick);
			AddLabel(10, 5, c_Quick ? 0x34 : 0x47E, "Q");

			AddHtml(0, y += 25, width / 2 - 55, "<DIV ALIGN=RIGHT>Nome");
			AddTextField(width / 2 - 15, y, 100, 20, 0x480, 0xBBC, "Name", c_Sign.Name);
			AddButton(width / 2 - 40, y + 3, 0x2716, "Name", Name);

			AddHtml(0, y += 25, width / 2, "<CENTER>Add Area");
			AddButton(width / 4 - 50, y + 3, 0x2716, "Add Area", AddBlock);
			AddButton(width / 4 + 40, y + 3, 0x2716, "Add Area", AddBlock);

			AddHtml(width / 2, y, width / 2, "<CENTER>Limpa");
			AddButton(width / 4 * 3 - 50, y + 3, 0x2716, "ClearAll", ClearAll);
			AddButton(width / 4 * 3 + 40, y + 3, 0x2716, "ClearAll", ClearAll);

			AddHtml(0, y += 25, width, "<CENTER>Terreo: " + c_Sign.MinZ);
			AddButton(width / 2 - 80, y + 3, 0x2716, "Base Floor", MinZSelect);
			AddButton(width / 2 + 70, y + 3, 0x2716, "Base Floor", MinZSelect);

			AddHtml(0, y += 17, width, "<CENTER>Cobertura: " + c_Sign.MaxZ);
			AddButton(width / 2 - 80, y + 3, 0x2716, "Top Floor", MaxZSelect);
			AddButton(width / 2 + 70, y + 3, 0x2716, "Top Floor", MaxZSelect);

			AddHtml(0, y += 25, width / 2, "<CENTER>Loc Placa");
			AddButton(width / 4 - 50, y + 3, 0x2716, "Sign Loc", SignLocSelect);
			AddButton(width / 4 + 40, y + 3, 0x2716, "Sign Loc", SignLocSelect);

			AddHtml(width / 2, y, width / 2, "<CENTER>Loc Ban");
			AddButton(width / 4 * 3 - 50, y + 3, 0x2716, "Ban Loc", BanLocSelect);
			AddButton(width / 4 * 3 + 40, y + 3, 0x2716, "Ban Loc", BanLocSelect);

			AddHtml(0, y += 25, width, "<CENTER>Secures Sugeridos");
			AddButton(width / 2 - 70, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);
			AddButton(width / 2 + 60, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);

			AddHtml(0, y += 20, width / 2 - 20, "<DIV ALIGN=RIGHT>Secures");
			AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Secures", c_Sign.Secures.ToString());
			AddButton(width / 2 - 5, y + 3, 0x2716, "Secures", Secures);

			AddHtml(0, y += 22, width / 2 - 20, "<DIV ALIGN=RIGHT>Lockdowns");
			AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Lockdowns", c_Sign.Locks.ToString());
			AddButton(width / 2 - 5, y + 3, 0x2716, "Lockdowns", Lockdowns);

			AddHtml(0, y += 25, width, "<CENTER>Dar items ao comprador");
			AddButton(width / 2 - 110, y, c_Sign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);
			AddButton(width / 2 + 90, y, c_Sign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);

			if (c_Sign.KeepItems)
			{
				AddHtml(0, y += 25, width / 2 - 25, "<DIV ALIGN=RIGHT>Pelo Valor");
				AddTextField(width / 2 + 15, y, 70, 20, 0x480, 0xBBC, "ItemsPrice", c_Sign.ItemsPrice.ToString());
				AddButton(width / 2 - 10, y + 5, 0x2716, "ItemsPrice", ItemsPrice);
			}
			else
			{
				AddHtml(0, y += 25, width, "<CENTER>Nao deletar itens");
				AddButton(width / 2 - 110, y, c_Sign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
				AddButton(width / 2 + 90, y, c_Sign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
			}

			if (!c_Sign.Owned)
			{
				AddHtml(120, y += 25, 50, c_Sign.PriceType);
				AddButton(170, y + 8, 0x985, 0x985, "LengthUp", PriceUp);
				AddButton(170, y - 2, 0x983, 0x983, "LengthDown", PriceDown);
			}

			if (c_Sign.RentByTime != TimeSpan.Zero)
			{
				AddHtml(0, y += 25, width, "<CENTER>Aluguel Recorrente");
				AddButton(width / 2 - 80, y, c_Sign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);
				AddButton(width / 2 + 60, y, c_Sign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);

				if (c_Sign.RecurRent)
				{
					AddHtml(0, y += 20, width, "<CENTER>Alugar para Comprar");
					AddButton(width / 2 - 80, y, c_Sign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
					AddButton(width / 2 + 60, y, c_Sign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
				}
			}

			AddHtml(0, y += 25, width, "<CENTER>Gratis");
			AddButton(width / 2 - 80, y, c_Sign.Free ? 0xD3 : 0xD2, "Free", Free);
			AddButton(width / 2 + 60, y, c_Sign.Free ? 0xD3 : 0xD2, "Free", Free);

			if (!c_Sign.Free)
			{
				AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>" + c_Sign.PriceType + " Preco");
				AddTextField(width / 2 + 20, y, 70, 20, 0x480, 0xBBC, "Price", c_Sign.Price.ToString());
				AddButton(width / 2 - 5, y + 5, 0x2716, "Price", Price);

				AddHtml(0, y += 25, width, "<CENTER>Preco Sugerido");
				AddButton(width / 2 - 70, y + 3, 0x2716, "Suggest", SuggestPrice);
				AddButton(width / 2 + 50, y + 3, 0x2716, "Suggest", SuggestPrice);
			}
		}

		private void WelcomePage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Bemvindo");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			var helptext = "";

			AddHtml(0, y += 25, width / 2 - 55, "<DIV ALIGN=RIGHT>Nome");
			AddTextField(width / 2 - 15, y, 100, 20, 0x480, 0xBBC, "Name", c_Sign.Name);
			AddButton(width / 2 - 40, y + 3, 0x2716, "Name", Name);

			if (c_Sign != null && c_Sign.Map != Map.Internal && c_Sign.RootParent == null)
			{
				AddHtml(0, y += 25, width, "<CENTER>Ir");
				AddButton(width / 2 - 50, y + 3, 0x2716, "Goto", Goto);
				AddButton(width / 2 + 40, y + 3, 0x2716, "Goto", Goto);
			}

			if (c_Sign.Owned)
			{
				helptext =
					String.Format(
						"  Essa casa e propriedade de {0}, Entao saiba que ao mudar algo " +
						"atravez desse menu ira mudar a casa!  Voce pode adicionar area, mudar de dono " +
						"regras, quase tudo!  voce nao pode, porem, mudar o statos de alugada da casa, muitas " +
						"maneiras de fazer o mal. Caso mude as restricoes e o dono da casa nao atender mais, " +
						"eles vao receber um aviso de 24 horas antes da demolicao.",
						c_Sign.House.Owner.Name);

				AddHtml(10, y += 25, width - 20, 180, helptext, false, false);

				y += 180;
			}
			else
			{
				helptext =
					String.Format(
						"  Bem vindo ao menu de Setup do Townhouses!  Esse menu vai te guiar a " +
						"cada passo do processo.  Voce pode definir qualquer area para ser uma casa, e detalhar tudo" +
						"da quantidade de itens e preco, a alugar ou comprar.  Vamos comecar com o nome " +
						"dessa nova casa!");

				AddHtml(10, y += 25, width - 20, 130, helptext, false, false);

				y += 130;
			}

			AddHtml(width - 80, y += 15, 60, "Proximo");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
		}

		private void BlocksPage(int width, ref int y)
		{
			if (c_Sign == null)
			{
				return;
			}

			c_Sign.ShowAreaPreview(Owner);

			AddHtml(0, y += 10, width, "<CENTER>Cria uma area");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Adiciona area");
			AddButton(width / 2 - 50, y + 3, 0x2716, "Add Area", AddBlock);
			AddButton(width / 2 + 40, y + 3, 0x2716, "Add Area", AddBlock);

			AddHtml(0, y += 20, width, "<CENTER>Limpa");
			AddButton(width / 2 - 50, y + 3, 0x2716, "ClearAll", ClearAll);
			AddButton(width / 2 + 40, y + 3, 0x2716, "ClearAll", ClearAll);

			var helptext =
				String.Format(
					"   Comece definindo qual area voce quer vender ou alugar  " +
					"Voce pode adicionar quantas caixas quiser, cada vez o programa te mostra " +
					" o que voce ja selecionou.  Se quiser recomecar, basta limpar! Voce deve ter " +
					"pelo menos 1 tile definido antes de ir pra a proxima etapa.");

			AddHtml(10, y += 35, width - 20, 140, helptext, false, false);

			y += 140;

			AddHtml(30, y += 15, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Sign.BlocksReady)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
			}
		}

		private void FloorsPage(int width, ref int y)
		{
			c_Sign.ShowFloorsPreview(Owner);

			AddHtml(0, y += 10, width, "<CENTER>Andares");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Terreo: " + c_Sign.MinZ);
			AddButton(width / 2 - 80, y + 3, 0x2716, "Base Floor", MinZSelect);
			AddButton(width / 2 + 70, y + 3, 0x2716, "Base Floor", MinZSelect);

			AddHtml(0, y += 20, width, "<CENTER>Cobertu: " + c_Sign.MaxZ);
			AddButton(width / 2 - 80, y + 3, 0x2716, "Top Floor", MaxZSelect);
			AddButton(width / 2 + 70, y + 3, 0x2716, "Top Floor", MaxZSelect);

			var helptext =
				String.Format(
                    "   Agora aponte os andares que deseja vender ou alugar  " +
					"Se for apenas um andar, Nao precisa apontar o andar superior.  Tudo que estiver entre a base e o andar superior " +
					"sera parte da casa,e quanto mais andares, maior o custo depois.");

			AddHtml(10, y += 35, width - 20, 110, helptext, false, false);

			y += 110;

			AddHtml(30, y += 15, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Sign.FloorsReady)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
			}
		}

		private void SignPage(int width, ref int y)
		{
			if (c_Sign == null)
			{
				return;
			}

			c_Sign.ShowSignPreview();

			AddHtml(0, y += 10, width, "<CENTER>Local da Placa");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Definir lcoal");
			AddButton(width / 2 - 60, y + 3, 0x2716, "Sign Loc", SignLocSelect);
			AddButton(width / 2 + 50, y + 3, 0x2716, "Sign Loc", SignLocSelect);

			var helptext =
				String.Format(
					"   Com essa placa, o dono ganha todos os poderes possiveis " +
					"de uma propriedade.  Se usarem a placa para demolir a casa, automaticamente " +
					"volta a ser vendida ou alugada.  A placa usada para comprar a casa vai aparecer no mesmo lugar " +
					"depois, um pouco abaixo da placa original.");

			AddHtml(10, y += 35, width - 20, 130, helptext, false, false);

			y += 130;

			AddHtml(30, y += 15, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Sign.SignReady)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
			}
		}

		private void BanPage(int width, ref int y)
		{
			if (c_Sign == null)
			{
				return;
			}

			c_Sign.ShowBanPreview();

			AddHtml(0, y += 10, width, "<CENTER>Local do Ban");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Definir Local");
			AddButton(width / 2 - 60, y + 3, 0x2716, "Ban Loc", BanLocSelect);
			AddButton(width / 2 + 50, y + 3, 0x2716, "Ban Loc", BanLocSelect);

			var helptext =
				String.Format(
					"   O local do ban define para onde os jogadores vao quando levam ban" +
					"de uma casa. Se nao definir, eles aparecem na quina inferiror direita " +
					"da casa");

			AddHtml(10, y += 35, width - 20, 100, helptext, false, false);

			y += 100;

			AddHtml(30, y += 15, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Sign.BanReady)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
			}
		}

		private void LocSecPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Lockdowns e Secures");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Sugestao");
			AddButton(width / 2 - 50, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);
			AddButton(width / 2 + 40, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Secures");
			AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Secures", c_Sign.Secures.ToString());
			AddButton(width / 2 - 5, y + 3, 0x2716, "Secures", Secures);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Lockdowns");
			AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "Lockdowns", c_Sign.Locks.ToString());
			AddButton(width / 2 - 5, y + 3, 0x2716, "Lockdowns", Lockdowns);

			var helptext =
				String.Format(
					"   Nesse passo voce vai definir a quantidade de itens da casa, ou deixar que  " +
					"faca isso por voce usando o botao de sugestao.  Em geral os jogadores ganham metade " +
					"dos lockdowns como possiveis secure. ");

			AddHtml(10, y += 35, width - 20, 90, helptext, false, false);

			y += 90;

			AddHtml(30, y += 15, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Sign.LocSecReady)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
			}
		}

		private void ItemsPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Itens de Decoracao");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Dar ao jogador itens na casa");
			AddButton(width / 2 - 110, y, c_Sign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);
			AddButton(width / 2 + 90, y, c_Sign.KeepItems ? 0xD3 : 0xD2, "Keep Items", KeepItems);

			if (c_Sign.KeepItems)
			{
				AddHtml(0, y += 25, width / 2 - 25, "<DIV ALIGN=RIGHT>Pelo valor");
				AddTextField(width / 2 + 15, y, 70, 20, 0x480, 0xBBC, "ItemsPrice", c_Sign.ItemsPrice.ToString());
				AddButton(width / 2 - 10, y + 5, 0x2716, "ItemsPrice", ItemsPrice);
			}
			else
			{
				AddHtml(0, y += 25, width, "<CENTER>Nao delete itens");
				AddButton(width / 2 - 110, y, c_Sign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
				AddButton(width / 2 + 90, y, c_Sign.LeaveItems ? 0xD3 : 0xD2, "LeaveItems", LeaveItems);
			}

			var helptext =
				String.Format(
					"   Como padrao o sistema deleta todos os itens nao estaticos da casa " +
					"na hora da compra.  Esses itens sao chamados de itens decorativos " +
					"Nao incluem addons, como forjas e camas.  Incluem containers.  Voce pode " +
					"permitir que jogadores mantem esses itens dizendo, aqui, e pode cobra-los por isso!");

			AddHtml(10, y += 35, width - 20, 160, helptext, false, false);

			y += 160;

			AddHtml(30, y += 15, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Sign.ItemsReady)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + (c_Sign.Owned ? 2 : 1));
			}
		}

		private void LengthPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Vender ou Alugar");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(120, y += 25, 50, c_Sign.PriceType);
			AddButton(170, y + 8, 0x985, 0x985, "LengthUp", PriceUp);
			AddButton(170, y - 2, 0x983, 0x983, "LengthDown", PriceDown);

			if (c_Sign.RentByTime != TimeSpan.Zero)
			{
				AddHtml(0, y += 25, width, "<CENTER>Aluguel Recorrente");
				AddButton(width / 2 - 80, y, c_Sign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);
				AddButton(width / 2 + 60, y, c_Sign.RecurRent ? 0xD3 : 0xD2, "RecurRent", RecurRent);

				if (c_Sign.RecurRent)
				{
					AddHtml(0, y += 20, width, "<CENTER>Alugar para Comprar");
					AddButton(width / 2 - 80, y, c_Sign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
					AddButton(width / 2 + 60, y, c_Sign.RentToOwn ? 0xD3 : 0xD2, "RentToOwn", RentToOwn);
				}
			}

			var helptext =
				String.Format(
					"   Chegando ao fim!  Agora voce espcifica se essa casa " +
					"vai ser posta a venda ou para aluguel.  So use as flechas ate chegar a definicao da sua escolha.  Para " +
					"casas de aluguel, voce pode fazer com que nao possam renovar, que faz com que jogadores sejam expulsos " +
					"ao fim do periodo!  Com a recorrencia, tendo dinheiro no banco eles podem continuar.  Voce pode tambem, " +
					"escolher alguar para comprar, onde os jogadores pagam parcelas em forma de alguel ate comprar a casa.");

			AddHtml(10, y += 35, width - 20, 160, helptext, false, true);

			y += 160;

			AddHtml(30, y += 15, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Sign.LengthReady)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
			}
		}

		private void PricePage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Preco");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Gratis");
			AddButton(width / 2 - 80, y, c_Sign.Free ? 0xD3 : 0xD2, "Free", Free);
			AddButton(width / 2 + 60, y, c_Sign.Free ? 0xD3 : 0xD2, "Free", Free);

			if (!c_Sign.Free)
			{
				AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>" + c_Sign.PriceType + " preco");
				AddTextField(width / 2 + 20, y, 70, 20, 0x480, 0xBBC, "Price", c_Sign.Price.ToString());
				AddButton(width / 2 - 5, y + 5, 0x2716, "Price", Price);

				AddHtml(0, y += 20, width, "<CENTER>Sugestao");
				AddButton(width / 2 - 50, y + 3, 0x2716, "Suggest", SuggestPrice);
				AddButton(width / 2 + 40, y + 3, 0x2716, "Suggest", SuggestPrice);
			}

			var helptext =
				String.Format(
					"   Agora voce pode definir o valor da casa. Se lembre que caso seja " +
					"aluguel o sistema ira cobrar esse valor pelo periodo!  O botao de sugestao " +
					"leva isso em conta.  Se nao quiser adivinhar, deixe o sistema escolher o valor por voce.  " +
					"Voce tambem pode dar a casa de graça.");

			AddHtml(10, y += 35, width - 20, 130, helptext, false, false);

			y += 130;

			AddHtml(30, y += 15, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - (c_Sign.Owned ? 2 : 1));

			if (c_Sign.PriceReady)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
			}
		}

		private void SkillsPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Restricoes");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Skill");
			AddTextField(width / 2 + 20, y, 100, 20, 0x480, 0xBBC, "Skill", c_Sign.Skill);
			AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Valor");
			AddTextField(width / 2 + 20, y, 50, 20, 0x480, 0xBBC, "SkillReq", c_Sign.SkillReq.ToString());
			AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Min Total");
			AddTextField(width / 2 + 20, y, 60, 20, 0x480, 0xBBC, "MinTotalSkill", c_Sign.MinTotalSkill.ToString());
			AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

			AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Max Total");
			AddTextField(width / 2 + 20, y, 60, 20, 0x480, 0xBBC, "MaxTotalSkill", c_Sign.MaxTotalSkill.ToString());
			AddButton(width / 2 - 5, y + 5, 0x2716, "Skill", Skill);

			var helptext =
				String.Format(
					"   Essas definicoes sao opcionais.  Se voce quer restringir quem pode comprar " +
					"essa casa pelas skills, aqui eh o lugar.  Voce pode especificar a skill e o valor, ou pela " +
					"quantidade total de skills que o player tem");

			AddHtml(10, y += 35, width - 20, 90, helptext, false, false);

			y += 90;

			AddHtml(30, y += 15, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Sign.PriceReady)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
			}
		}

		private void OtherPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Outras Opçoes");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Novo Jogador");
			AddButton(width / 2 - 80, y, c_Sign.YoungOnly ? 0xD3 : 0xD2, "Young Only", Young);
			AddButton(width / 2 + 60, y, c_Sign.YoungOnly ? 0xD3 : 0xD2, "Young Only", Young);

			if (!c_Sign.YoungOnly)
			{
				AddHtml(0, y += 25, width, "<CENTER>Inocentes");
				AddButton(width / 2 - 80, y, c_Sign.Murderers == Intu.No ? 0xD3 : 0xD2, "No Murderers", Murderers, Intu.No);
				AddButton(width / 2 + 60, y, c_Sign.Murderers == Intu.No ? 0xD3 : 0xD2, "No Murderers", Murderers, Intu.No);
				AddHtml(0, y += 20, width, "<CENTER>Assassinos");
				AddButton(width / 2 - 80, y, c_Sign.Murderers == Intu.Yes ? 0xD3 : 0xD2, "Yes Murderers", Murderers, Intu.Yes);
				AddButton(width / 2 + 60, y, c_Sign.Murderers == Intu.Yes ? 0xD3 : 0xD2, "Yes Murderers", Murderers, Intu.Yes);
				AddHtml(0, y += 20, width, "<CENTER>Todos");
				AddButton(
					width / 2 - 80,
					y,
					c_Sign.Murderers == Intu.Neither ? 0xD3 : 0xD2,
					"Assassinos Nao",
					Murderers,
					Intu.Neither);
				AddButton(
					width / 2 + 60,
					y,
					c_Sign.Murderers == Intu.Neither ? 0xD3 : 0xD2,
					"Assassinos Nao",
					Murderers,
					Intu.Neither);
			}

			AddHtml(0, y += 25, width, "<CENTER>Retrancar as portas ao demolir");
			AddButton(width / 2 - 110, y, c_Sign.Relock ? 0xD3 : 0xD2, "Relock", Relock);
			AddButton(width / 2 + 90, y, c_Sign.Relock ? 0xD3 : 0xD2, "Relock", Relock);

			var helptext =
				String.Format(
					"   Essa parte tambem e opcional.  Com a definicao de novo, voce pode restringir " +
					" quem pode comprar ou alugar as casas.  Voce pode especificar se assassinos ou inocentes " +
					" podem residir ali.  Tambem pode decidir se as portas ficam fechadas e trancadas " +
					" quando um jogador demole a casa");

			AddHtml(10, y += 35, width - 20, 180, helptext, false, false);

			y += 180;

			AddHtml(30, y += 15, 80, "anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			AddHtml(width - 80, y, 60, "Proximo");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
		}

		private void OtherPage2(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Outras Opcoes 2");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Forcar Publico");
			AddButton(width / 2 - 110, y, c_Sign.ForcePublic ? 0xD3 : 0xD2, "Public", ForcePublic);
			AddButton(width / 2 + 90, y, c_Sign.ForcePublic ? 0xD3 : 0xD2, "Public", ForcePublic);

			AddHtml(0, y += 25, width, "<CENTER>Forcar Privado");
			AddButton(width / 2 - 110, y, c_Sign.ForcePrivate ? 0xD3 : 0xD2, "Private", ForcePrivate);
			AddButton(width / 2 + 90, y, c_Sign.ForcePrivate ? 0xD3 : 0xD2, "Private", ForcePrivate);

			AddHtml(0, y += 25, width, "<CENTER>Sem Comercio");
			AddButton(width / 2 - 110, y, c_Sign.NoTrade ? 0xD3 : 0xD2, "NoTrade", NoTrade);
			AddButton(width / 2 + 90, y, c_Sign.NoTrade ? 0xD3 : 0xD2, "NoTrade", NoTrade);

			AddHtml(0, y += 25, width, "<CENTER>Sem Banir");
			AddButton(width / 2 - 110, y, c_Sign.NoBanning ? 0xD3 : 0xD2, "NoBan", NoBan);
			AddButton(width / 2 + 90, y, c_Sign.NoBanning ? 0xD3 : 0xD2, "NoBan", NoBan);

			var helptext =
				String.Format(
					"   Mais uma pagina de outras opcoes!  As vezes existem caracteristicas que voce nao quer que o player tenha.  " +
					"Aqui voce pode focar a casa a ser publica ou privada. Pode escolher nao ter comercio e nao poder banir jogadores da casa");

			AddHtml(10, y += 35, width - 20, 180, helptext, false, false);

			y += 180;

			AddHtml(30, y += 15, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);
		}

		private bool SkillNameExists(string text)
		{
			try
			{
				var index = (SkillName)Enum.Parse(typeof(SkillName), text, true);
				return true;
			}
			catch
			{
				Owner.SendMessage("Voce escolheu uma skill que nao existe.");
				return false;
			}
		}

		private void ChangePage(object obj)
		{
			if (c_Sign == null)
			{
				return;
			}

			if (!(obj is int))
			{
				return;
			}

			c_Page = (Page)(int)obj;

			c_Sign.ClearPreview();

			NewGump();
		}

		private void Name()
		{
			c_Sign.Name = GetTextField("Name");
			Owner.SendMessage("Nome definido!");
			NewGump();
		}

		private void Goto()
		{
			Owner.Location = c_Sign.Location;
			Owner.Z += 5;
			Owner.Map = c_Sign.Map;

			NewGump();
		}

		private void Quick()
		{
			c_Quick = !c_Quick;
			NewGump();
		}

		private void BanLocSelect()
		{
			Owner.SendMessage("Aponte o local do ban");
			Owner.Target = new InternalTarget(this, c_Sign, TargetType.BanLoc);
		}

		private void SignLocSelect()
		{
			Owner.SendMessage("Aponte o local da placa da casa");
			Owner.Target = new InternalTarget(this, c_Sign, TargetType.SignLoc);
		}

		private void MinZSelect()
		{
			Owner.SendMessage("Aponte o terreo");
			Owner.Target = new InternalTarget(this, c_Sign, TargetType.MinZ);
		}

		private void MaxZSelect()
		{
			Owner.SendMessage("Aponte o andar mais alto");
			Owner.Target = new InternalTarget(this, c_Sign, TargetType.MaxZ);
		}

		private void Young()
		{
			c_Sign.YoungOnly = !c_Sign.YoungOnly;
			NewGump();
		}

		private void Murderers(object obj)
		{
			if (!(obj is Intu))
			{
				return;
			}

			c_Sign.Murderers = (Intu)obj;

			NewGump();
		}

		private void Relock()
		{
			c_Sign.Relock = !c_Sign.Relock;
			NewGump();
		}

		private void ForcePrivate()
		{
			c_Sign.ForcePrivate = !c_Sign.ForcePrivate;
			NewGump();
		}

		private void ForcePublic()
		{
			c_Sign.ForcePublic = !c_Sign.ForcePublic;
			NewGump();
		}

		private void NoTrade()
		{
			c_Sign.NoTrade = !c_Sign.NoTrade;
			NewGump();
		}

		private void NoBan()
		{
			c_Sign.NoBanning = !c_Sign.NoBanning;
			NewGump();
		}

		private void KeepItems()
		{
			c_Sign.KeepItems = !c_Sign.KeepItems;
			NewGump();
		}

		private void LeaveItems()
		{
			c_Sign.LeaveItems = !c_Sign.LeaveItems;
			NewGump();
		}

		private void ItemsPrice()
		{
			c_Sign.ItemsPrice = GetTextFieldInt("ItemsPrice");
			Owner.SendMessage("Valor do item definido!");
			NewGump();
		}

		private void RecurRent()
		{
			c_Sign.RecurRent = !c_Sign.RecurRent;
			NewGump();
		}

		private void RentToOwn()
		{
			c_Sign.RentToOwn = !c_Sign.RentToOwn;
			NewGump();
		}

		private void Skill()
		{
			if (GetTextField("Skill") != "" && SkillNameExists(GetTextField("Skill")))
			{
				c_Sign.Skill = GetTextField("Skill");
			}
			else
			{
				c_Sign.Skill = "";
			}

			c_Sign.SkillReq = GetTextFieldInt("SkillReq");
			c_Sign.MinTotalSkill = GetTextFieldInt("MinTotalSkill");
			c_Sign.MaxTotalSkill = GetTextFieldInt("MaxTotalSkill");

			Owner.SendMessage("Info da skill definida!");

			NewGump();
		}

		private void Claim()
		{
			new TownHouseConfirmGump(Owner, c_Sign);
			OnClose();
		}

		private void SuggestLocSec()
		{
			var price = c_Sign.CalcVolume() * General.SuggestionFactor;
			c_Sign.Secures = price / 75;
			c_Sign.Locks = c_Sign.Secures / 2;

			NewGump();
		}

		private void Secures()
		{
			c_Sign.Secures = GetTextFieldInt("Secures");
			Owner.SendMessage("Secures definidos!");
			NewGump();
		}

		private void Lockdowns()
		{
			c_Sign.Locks = GetTextFieldInt("Lockdowns");
			Owner.SendMessage("Lockdowns definidos!");
			NewGump();
		}

		private void SuggestPrice()
		{
			c_Sign.Price = c_Sign.CalcVolume() * General.SuggestionFactor;

			if (c_Sign.RentByTime == TimeSpan.FromDays(1))
			{
				c_Sign.Price /= 60;
			}
			if (c_Sign.RentByTime == TimeSpan.FromDays(7))
			{
				c_Sign.Price = (int)(c_Sign.Price / 8.57);
			}
			if (c_Sign.RentByTime == TimeSpan.FromDays(30))
			{
				c_Sign.Price /= 2;
			}

			NewGump();
		}

		private void Price()
		{
			c_Sign.Price = GetTextFieldInt("Price");
			Owner.SendMessage("Valor definido!");
			NewGump();
		}

		private void Free()
		{
			c_Sign.Free = !c_Sign.Free;
			NewGump();
		}

		private void AddBlock()
		{
			if (c_Sign == null)
			{
				return;
			}

			Owner.SendMessage("Aponte a quina noroeste da casa.");
			Owner.Target = new InternalTarget(this, c_Sign, TargetType.BlockOne);
		}

		private void ClearAll()
		{
			if (c_Sign == null)
			{
				return;
			}

			c_Sign.Blocks.Clear();
			c_Sign.ClearPreview();
			c_Sign.UpdateBlocks();

			NewGump();
		}

		private void PriceUp()
		{
			c_Sign.NextPriceType();
			NewGump();
		}

		private void PriceDown()
		{
			c_Sign.PrevPriceType();
			NewGump();
		}

		protected override void OnClose()
		{
			c_Sign.ClearPreview();
		}

		private class InternalTarget : Target
		{
			private readonly TownHouseSetupGump c_Gump;
			private readonly TownHouseSign c_Sign;
			private readonly TargetType c_Type;
			private readonly Point3D c_BoundOne;

			public InternalTarget(TownHouseSetupGump gump, TownHouseSign sign, TargetType type)
				: this(gump, sign, type, Point3D.Zero)
			{ }

			public InternalTarget(TownHouseSetupGump gump, TownHouseSign sign, TargetType type, Point3D point)
				: base(20, true, TargetFlags.None)
			{
				c_Gump = gump;
				c_Sign = sign;
				c_Type = type;
				c_BoundOne = point;
			}

			protected override void OnTarget(Mobile m, object o)
			{
				var point = (IPoint3D)o;

				switch (c_Type)
				{
					case TargetType.BanLoc:
						c_Sign.BanLoc = new Point3D(point.X, point.Y, point.Z);
						c_Gump.NewGump();
						break;

					case TargetType.SignLoc:
						c_Sign.SignLoc = new Point3D(point.X, point.Y, point.Z);
						c_Sign.MoveToWorld(c_Sign.SignLoc, c_Sign.Map);
						c_Sign.Z -= 5;
						c_Sign.ShowSignPreview();
						c_Gump.NewGump();
						break;

					case TargetType.MinZ:
						c_Sign.MinZ = point.Z;

						if (c_Sign.MaxZ < c_Sign.MinZ + 19)
						{
							c_Sign.MaxZ = point.Z + 19;
						}

						if (c_Sign.MaxZ == short.MaxValue)
						{
							c_Sign.MaxZ = point.Z + 19;
						}

						c_Gump.NewGump();
						break;

					case TargetType.MaxZ:
						c_Sign.MaxZ = point.Z + 19;

						if (c_Sign.MinZ > c_Sign.MaxZ)
						{
							c_Sign.MinZ = point.Z;
						}

						c_Gump.NewGump();
						break;

					case TargetType.BlockOne:
						m.SendMessage("Agora aponde a quina sudeste da casa");
						m.Target = new InternalTarget(c_Gump, c_Sign, TargetType.BlockTwo, new Point3D(point.X, point.Y, point.Z));
						break;

					case TargetType.BlockTwo:
						c_Sign.Blocks.Add(FixRect(new Rectangle2D(c_BoundOne, new Point3D(point.X + 1, point.Y + 1, point.Z))));
						c_Sign.UpdateBlocks();
						c_Sign.ShowAreaPreview(m);
						c_Gump.NewGump();
						break;
				}
			}

			protected override void OnTargetCancel(Mobile m, TargetCancelType cancelType)
			{
				c_Gump.NewGump();
			}
		}
	}
}
