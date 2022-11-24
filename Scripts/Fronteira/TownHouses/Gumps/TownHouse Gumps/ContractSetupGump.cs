#region References
using System;
using System.Collections;

using Server;
using Server.Targeting;
#endregion

namespace Knives.TownHouses
{
	public class ContractSetupGump : GumpPlusLight
	{
		public enum Page
		{
			Blocks,
			Floors,
			Sign,
			LocSec,
			Length,
			Price
		}

		public enum TargetType
		{
			SignLoc,
			MinZ,
			MaxZ,
			BlockOne,
			BlockTwo
		}

		private readonly RentalContract c_Contract;
		private Page c_Page;

		public ContractSetupGump(Mobile m, RentalContract contract)
			: base(m, 50, 50)
		{
			m.CloseGump(typeof(ContractSetupGump));

			c_Contract = contract;
		}

		protected override void BuildGump()
		{
			var width = 300;
			var y = 0;

			switch (c_Page)
			{
				case Page.Blocks:
					BlocksPage(width, ref y);
					break;
				case Page.Floors:
					FloorsPage(width, ref y);
					break;
				case Page.Sign:
					SignPage(width, ref y);
					break;
				case Page.LocSec:
					LocSecPage(width, ref y);
					break;
				case Page.Length:
					LengthPage(width, ref y);
					break;
				case Page.Price:
					PricePage(width, ref y);
					break;
			}

			AddBackgroundZero(0, 0, width, y + 40, 0x13BE);
		}

		private void BlocksPage(int width, ref int y)
		{
			if (c_Contract == null)
			{
				return;
			}

			c_Contract.ShowAreaPreview(Owner);

			AddHtml(0, y += 10, width, "<CENTER>Crie uma area");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			y += 25;

			if (!General.HasOtherContract(c_Contract.ParentHouse, c_Contract))
			{
				AddHtml(60, y, 90, "Casa Inteira");
				AddButton(30, y, c_Contract.EntireHouse ? 0xD3 : 0xD2, "Entire House", EntireHouse);
			}

			if (!c_Contract.EntireHouse)
			{
				AddHtml(170, y, 70, "Adicione Area");
				AddButton(240, y, 0x15E1, 0x15E5, "Add Area", AddBlock);

				AddHtml(170, y += 20, 70, "Limpar");
				AddButton(240, y, 0x15E1, 0x15E5, "Clear All", ClearBlocks);
			}

			var helptext =
				String.Format(
					"   Bemvindo ao menu de contrato de aluguel. Para comecar voce deve " +
					"primeiro criar a area que deseja vender.  Como acima, existem duas maneiras de fazer isso: " +
					"alugar a casa inteira, ou partes dela. Como voce cria a area, O preview vai mostrar exatamente " +
					"a area que voce ja selecionou.  Voce pode criar qualquer formado de area adicionando areas!");

			AddHtml(10, y += 35, width - 20, 170, helptext, false, false);

			y += 170;

			if (c_Contract.EntireHouse || c_Contract.Blocks.Count != 0)
			{
				AddHtml(width - 80, y += 20, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + (c_Contract.EntireHouse ? 4 : 1));
			}
		}

		private void FloorsPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Andares");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(40, y += 25, 80, "Terreo");
			AddButton(110, y, 0x15E1, 0x15E5, "Base Floor", MinZSelect);

			AddHtml(160, y, 80, "Cobertura");
			AddButton(230, y, 0x15E1, 0x15E5, "Top Floor", MaxZSelect);

			AddHtml(
				100,
				y += 25,
				100,
				String.Format(
					"{0} total de andares{1}",
					c_Contract.Floors > 10 ? "1" : "" + c_Contract.Floors,
					c_Contract.Floors == 1 || c_Contract.Floors > 10 ? "" : "s"));

			var helptext =
				String.Format(
					"   Agora voce seleciona o andar que quer alugar.  " +
					"Caso so queria um andar, pode pular essa parte.  Tudo no terreo " +
					"e no andar de cima vai ser alugado, quando mais andares, mais caro");

			AddHtml(10, y += 35, width - 20, 120, helptext, false, false);

			y += 120;

			AddHtml(30, y += 20, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Contract.MinZ != short.MinValue)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
			}
		}

		private void SignPage(int width, ref int y)
		{
			if (c_Contract == null)
			{
				return;
			}

			c_Contract.ShowSignPreview();

			AddHtml(0, y += 10, width, "<CENTER>Localizacao da Placa");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(100, y += 25, 80, "Escolher Local");
			AddButton(180, y, 0x15E1, 0x15E5, "Sign Loc", SignLocSelect);

			var helptext =
				String.Format(
					"   Com esta placa, o locatario tem todos os poderes que o dono tem " +
					"sobre este espaco.  Se eles demolires essa propriedade, eles cancelaram seus  " +
					"contratos e nao receberam de volta sua caucao. ");

			AddHtml(10, y += 35, width - 20, 110, helptext, false, false);

			y += 110;

			AddHtml(30, y += 20, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Contract.SignLoc != Point3D.Zero)
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

			AddHtml(0, y += 25, width, "<CENTER>Secures sugeridos");
			AddButton(width / 2 - 70, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);
			AddButton(width / 2 + 60, y + 3, 0x2716, "Suggest LocSec", SuggestLocSec);

			AddHtml(
				30,
				y += 25,
				width / 2 - 20,
				"<DIV ALIGN=RIGHT>Secures (Max: " + (General.RemainingSecures(c_Contract.ParentHouse) + c_Contract.Secures) + ")");
			AddTextField(width / 2 + 50, y, 50, 20, 0x480, 0xBBC, "Secures", c_Contract.Secures.ToString());
			AddButton(width / 2 + 25, y + 3, 0x2716, "Secures", Secures);

			AddHtml(
				30,
				y += 20,
				width / 2 - 20,
				"<DIV ALIGN=RIGHT>Lockdowns (Max: " + (General.RemainingLocks(c_Contract.ParentHouse) + c_Contract.Locks) + ")");
			AddTextField(width / 2 + 50, y, 50, 20, 0x480, 0xBBC, "Lockdowns", c_Contract.Locks.ToString());
			AddButton(width / 2 + 25, y + 3, 0x2716, "Lockdowns", Lockdowns);

			var helptext =
				String.Format(
					"   Sem containers isso nao seria bem uma casa!  aqui voce pode dar lockdowns " +
					"e secures da sua propria casa.  Use o botao de sugestao para uma ideia do quanto dar. Tenha cuidado ao " +
					"alugar sua propriedade. Se voce tiver muitos items vai comecar a usar o espaço reservado a seus clientes  " +
					"Voce vai receber um aviso 48hrs antes disso acontecer, mas depois disso o contrato e cancelado!");

			AddHtml(10, y += 35, width - 20, 180, helptext, false, false);

			y += 180;

			AddHtml(30, y += 20, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Contract.Locks != 0 && c_Contract.Secures != 0)
			{
				AddHtml(width - 80, y, 60, "Proximo");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
			}
		}

		private void LengthPage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Tempo");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(120, y += 25, 50, c_Contract.PriceType);
			AddButton(170, y + 8, 0x985, "LengthUp", LengthUp);
			AddButton(170, y - 2, 0x983, "LengthDown", LengthDown);

			var helptext =
				String.Format(
					"   A cada {0} O banco vai automaticamente transferir o valor do aluguel deles pra voce  " +
					"Usando as setas, voce escolhe de quanto em quanto tempo e feita essa coleta.",
					c_Contract.PriceTypeShort.ToLower());

			AddHtml(10, y += 35, width - 20, 100, helptext, false, false);

			y += 100;

			AddHtml(30, y += 20, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - (c_Contract.EntireHouse ? 4 : 1));

			AddHtml(width - 80, y, 60, "Proximo");
			AddButton(width - 30, y, 0x15E1, 0x15E5, "Next", ChangePage, (int)c_Page + 1);
		}

		private void PricePage(int width, ref int y)
		{
			AddHtml(0, y += 10, width, "<CENTER>Cobranca por tempo");
			AddImage(width / 2 - 100, y + 2, 0x39);
			AddImage(width / 2 + 70, y + 2, 0x3B);

			AddHtml(0, y += 25, width, "<CENTER>Gratis");
			AddButton(width / 2 - 80, y, c_Contract.Free ? 0xD3 : 0xD2, "Free", Free);
			AddButton(width / 2 + 60, y, c_Contract.Free ? 0xD3 : 0xD2, "Free", Free);

			if (!c_Contract.Free)
			{
				AddHtml(0, y += 25, width / 2 - 20, "<DIV ALIGN=RIGHT>Por " + c_Contract.PriceTypeShort);
				AddTextField(width / 2 + 20, y, 70, 20, 0x480, 0xBBC, "Price", c_Contract.Price.ToString());
				AddButton(width / 2 - 5, y + 3, 0x2716, "Price", Price);

				AddHtml(0, y += 20, width, "<CENTER>Sugestao");
				AddButton(width / 2 - 70, y + 3, 0x2716, "Suggest", SuggestPrice);
				AddButton(width / 2 + 60, y + 3, 0x2716, "Suggest", SuggestPrice);
			}

			var helptext =
				String.Format(
					"   Agora voce pode finalizar o projeto incluindo seu preco por {0}.  " +
					"Uma vez finalizado, Esse contrato nao pode ser modificado!  Usando o botao " +
					"de sugestao, um valor sera estipulado a partir de tais criterios:<BR>",
					c_Contract.PriceTypeShort);

			helptext += String.Format("<CENTER>Tamanho: {0}<BR>", c_Contract.CalcVolume());
			helptext += String.Format("Custo por unidade: {0} ouro</CENTER>", General.SuggestionFactor);
			helptext += "<br>   Voce tambem pode doar esse espaco gratis com o botao acima.";

			AddHtml(10, y += 35, width - 20, 150, helptext, false, true);

			y += 150;

			AddHtml(30, y += 20, 80, "Anterior");
			AddButton(10, y, 0x15E3, 0x15E7, "Previous", ChangePage, (int)c_Page - 1);

			if (c_Contract.Price != 0)
			{
				AddHtml(width - 70, y, 60, "Finalize");
				AddButton(width - 30, y, 0x15E1, 0x15E5, "Finalize", FinalizeSetup);
			}
		}

		protected override void OnClose()
		{
			c_Contract.ClearPreview();
		}

		private void SuggestPrice()
		{
			if (c_Contract == null)
			{
				return;
			}

			c_Contract.Price = c_Contract.CalcVolume() * General.SuggestionFactor;

			if (c_Contract.RentByTime == TimeSpan.FromDays(1))
			{
				c_Contract.Price /= 60;
			}
			if (c_Contract.RentByTime == TimeSpan.FromDays(7))
			{
				c_Contract.Price = (int)(c_Contract.Price / 8.57);
			}
			if (c_Contract.RentByTime == TimeSpan.FromDays(30))
			{
				c_Contract.Price /= 2;
			}

			NewGump();
		}

		private void SuggestLocSec()
		{
			var price = c_Contract.CalcVolume() * General.SuggestionFactor;
			c_Contract.Secures = price / 75;
			c_Contract.Locks = c_Contract.Secures / 2;

			c_Contract.FixLocSec();

			NewGump();
		}

		private void Price()
		{
			c_Contract.Price = GetTextFieldInt("Price");
			Owner.SendMessage("Valor Definido!");
			NewGump();
		}

		private void Secures()
		{
			c_Contract.Secures = GetTextFieldInt("Secures");
			Owner.SendMessage("Secures Definidos!");
			NewGump();
		}

		private void Lockdowns()
		{
			c_Contract.Locks = GetTextFieldInt("Lockdowns");
			Owner.SendMessage("Lockdowns Definidos!");
			NewGump();
		}

		private void ChangePage(object obj)
		{
			if (c_Contract == null || !(obj is int))
			{
				return;
			}

			c_Contract.ClearPreview();

			c_Page = (Page)(int)obj;

			NewGump();
		}

		private void EntireHouse()
		{
			if (c_Contract == null || c_Contract.ParentHouse == null)
			{
				return;
			}

			c_Contract.EntireHouse = !c_Contract.EntireHouse;

			c_Contract.ClearPreview();

			if (c_Contract.EntireHouse)
			{
				var list = new ArrayList();

				var once = false;
				foreach (var rect in RUOVersion.RegionArea(c_Contract.ParentHouse.Region))
				{
					list.Add(new Rectangle2D(new Point2D(rect.Start.X, rect.Start.Y), new Point2D(rect.End.X, rect.End.Y)));

					if (once)
					{
						continue;
					}

					if (rect.Start.Z >= rect.End.Z)
					{
						c_Contract.MinZ = rect.End.Z;
						c_Contract.MaxZ = rect.Start.Z;
					}
					else
					{
						c_Contract.MinZ = rect.Start.Z;
						c_Contract.MaxZ = rect.End.Z;
					}

					once = true;
				}

				c_Contract.Blocks = list;
			}
			else
			{
				c_Contract.Blocks.Clear();
				c_Contract.MinZ = short.MinValue;
				c_Contract.MaxZ = short.MinValue;
			}

			NewGump();
		}

		private void SignLocSelect()
		{
			Owner.Target = new InternalTarget(this, c_Contract, TargetType.SignLoc);
		}

		private void MinZSelect()
		{
			Owner.SendMessage("Aponte o terreo da sua area de alugel.");
			Owner.Target = new InternalTarget(this, c_Contract, TargetType.MinZ);
		}

		private void MaxZSelect()
		{
			Owner.SendMessage("Aponte a cobertura da sua area de aluguel");
			Owner.Target = new InternalTarget(this, c_Contract, TargetType.MaxZ);
		}

		private void LengthUp()
		{
			if (c_Contract == null)
			{
				return;
			}

			c_Contract.NextPriceType();

			if (c_Contract.RentByTime == TimeSpan.FromDays(0))
			{
				c_Contract.RentByTime = TimeSpan.FromDays(1);
			}

			NewGump();
		}

		private void LengthDown()
		{
			if (c_Contract == null)
			{
				return;
			}

			c_Contract.PrevPriceType();

			if (c_Contract.RentByTime == TimeSpan.FromDays(0))
			{
				c_Contract.RentByTime = TimeSpan.FromDays(30);
			}

			NewGump();
		}

		private void Free()
		{
			c_Contract.Free = !c_Contract.Free;

			NewGump();
		}

		private void AddBlock()
		{
			Owner.SendMessage("Aponte a quina noroeste da area");
			Owner.Target = new InternalTarget(this, c_Contract, TargetType.BlockOne);
		}

		private void ClearBlocks()
		{
			if (c_Contract == null)
			{
				return;
			}

			c_Contract.Blocks.Clear();

			c_Contract.ClearPreview();

			NewGump();
		}

		private void FinalizeSetup()
		{
			if (c_Contract == null)
			{
				return;
			}

			if (c_Contract.Price == 0)
			{
				Owner.SendMessage("Voce nao pode alugar essa area com 0 ouros");
				NewGump();
				return;
			}

			c_Contract.Completed = true;
			c_Contract.BanLoc = c_Contract.ParentHouse.Region.GoLocation;

			if (c_Contract.EntireHouse)
			{
				var point = c_Contract.ParentHouse.Sign.Location;
				c_Contract.SignLoc = new Point3D(point.X, point.Y, point.Z - 5);
				c_Contract.Secures = Core.AOS ? c_Contract.ParentHouse.GetAosMaxSecures() : c_Contract.ParentHouse.MaxSecures;
				c_Contract.Locks = Core.AOS ? c_Contract.ParentHouse.GetAosMaxLockdowns() : c_Contract.ParentHouse.MaxLockDowns;
			}

			Owner.SendMessage("Voce finalizou o contrato de aluguel.  Agora ache um locatario!");
		}

		private class InternalTarget : Target
		{
			private readonly ContractSetupGump c_Gump;
			private readonly RentalContract c_Contract;
			private readonly TargetType c_Type;
			private readonly Point3D c_BoundOne;

			public InternalTarget(ContractSetupGump gump, RentalContract contract, TargetType type)
				: this(gump, contract, type, Point3D.Zero)
			{ }

			public InternalTarget(ContractSetupGump gump, RentalContract contract, TargetType type, Point3D point)
				: base(20, true, TargetFlags.None)
			{
				c_Gump = gump;
				c_Contract = contract;
				c_Type = type;
				c_BoundOne = point;
			}

			protected override void OnTarget(Mobile m, object o)
			{
				var point = (IPoint3D)o;

				if (c_Contract == null || c_Contract.ParentHouse == null)
				{
					return;
				}

				if (!c_Contract.ParentHouse.Region.Contains(new Point3D(point.X, point.Y, point.Z)))
				{
					m.SendMessage("Voce deve apontar para dentro da casa");
					m.Target = new InternalTarget(c_Gump, c_Contract, c_Type, c_BoundOne);
					return;
				}

				switch (c_Type)
				{
					case TargetType.SignLoc:
						c_Contract.SignLoc = new Point3D(point.X, point.Y, point.Z);
						c_Contract.ShowSignPreview();
						c_Gump.NewGump();
						break;

					case TargetType.MinZ:
						if (!c_Contract.ParentHouse.Region.Contains(new Point3D(point.X, point.Y, point.Z)))
						{
							m.SendMessage("Isso nao esta dentro da sua casa");
						}
						else if (c_Contract.HasContractedArea(point.Z))
						{
							m.SendMessage("Essa area ja foi alugada");
						}
						else
						{
							c_Contract.MinZ = point.Z;

							if (c_Contract.MaxZ < c_Contract.MinZ + 19)
							{
								c_Contract.MaxZ = point.Z + 19;
							}
						}

						c_Contract.ShowFloorsPreview(m);
						c_Gump.NewGump();
						break;

					case TargetType.MaxZ:
						if (!c_Contract.ParentHouse.Region.Contains(new Point3D(point.X, point.Y, point.Z)))
						{
							m.SendMessage("Isso nao esta dentro da sua casa");
						}
						else if (c_Contract.HasContractedArea(point.Z))
						{
							m.SendMessage("Essa area ja foi alugada");
						}
						else
						{
							c_Contract.MaxZ = point.Z + 19;

							if (c_Contract.MinZ > c_Contract.MaxZ)
							{
								c_Contract.MinZ = point.Z;
							}
						}

						c_Contract.ShowFloorsPreview(m);
						c_Gump.NewGump();
						break;

					case TargetType.BlockOne:
						m.SendMessage("Agora aponte a quina sudeste da area");
						m.Target = new InternalTarget(c_Gump, c_Contract, TargetType.BlockTwo, new Point3D(point.X, point.Y, point.Z));
						break;

					case TargetType.BlockTwo:
						var rect = TownHouseSetupGump.FixRect(new Rectangle2D(c_BoundOne, new Point3D(point.X + 1, point.Y + 1, point.Z)));

						if (c_Contract.HasContractedArea(rect, point.Z))
						{
							m.SendMessage("Essa area ja foi alugada");
						}
						else
						{
							c_Contract.Blocks.Add(rect);
							c_Contract.ShowAreaPreview(m);
						}

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
