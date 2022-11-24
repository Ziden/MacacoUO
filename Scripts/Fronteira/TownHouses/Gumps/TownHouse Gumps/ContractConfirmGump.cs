#region References
using System;

using Server;
#endregion

namespace Knives.TownHouses
{
	public class ContractConfirmGump : GumpPlusLight
	{
		private readonly RentalContract c_Contract;

		public ContractConfirmGump(Mobile m, RentalContract rc)
			: base(m, 100, 100)
		{
			m.CloseGump(typeof(ContractConfirmGump));

			c_Contract = rc;
		}

		protected override void BuildGump()
		{
			var width = 300;
			var y = 0;

			if (c_Contract.RentalClient == null)
			{
				AddHtml(0, y + 5, width, HTML.Black + "<CENTER>Alugar esse espaco?");
			}
			else
			{
				AddHtml(0, y + 5, width, HTML.Black + "<CENTER>Contrato de aluguel");
			}

			var text =
				String.Format(
					"  Eu, {0}, concordo em alugar essa propriedade {1} pelo valor de {2} a cada {3}.  " +
					"O valor do aluguel sera retirado direto do banco. Caso eu nao possa  " +
					"pagar esse valor, essa propriedade e devolvida para {1}.  Eu posso cancelar esse acordo a qualquer " +
					"momento e sair da propriedade.  {1} tambem pode cancelar esse acordo a qualquer momento se desfazendo " +
					"dessa propriedade ou cancelando o contrato com a caucao sendo prontamente devolvida.",
					c_Contract.RentalClient == null ? "_____" : c_Contract.RentalClient.Name,
					c_Contract.RentalMaster.Name,
					c_Contract.Free ? 0 : c_Contract.Price,
					c_Contract.PriceTypeShort.ToLower());

			text += "<BR>   Aqui estao mais informacoes sobre a propriedade:<BR>";

			text += String.Format("<CENTER>Lockdowns: {0}<BR>", c_Contract.Locks);
			text += String.Format("Secures: {0}<BR>", c_Contract.Secures);
			text += String.Format(
				"Floors: {0}<BR>",
				(c_Contract.MaxZ - c_Contract.MinZ < 200) ? (c_Contract.MaxZ - c_Contract.MinZ) / 20 + 1 : 1);
			text += String.Format("Space: {0} cubic units", c_Contract.CalcVolume());

			AddHtml(40, y += 30, width - 60, 200, HTML.Black + text, false, true);

			y += 200;

			if (c_Contract.RentalClient == null)
			{
				AddHtml(60, y += 20, 60, HTML.Black + "Preview");
				AddButton(40, y + 3, 0x837, 0x838, "Preview", Preview);

				var locsec = c_Contract.ValidateLocSec();

				if (Owner != c_Contract.RentalMaster && locsec)
				{
					AddHtml(width - 100, y, 60, HTML.Black + "Aceitar");
					AddButton(width - 60, y + 3, 0x232C, 0x232D, "Aceitar", Accept);
				}
				else
				{
					AddImage(width - 60, y - 10, 0x232C);
				}

				if (!locsec)
				{
					Owner.SendMessage(
						(Owner == c_Contract.RentalMaster
							? "Voce nao tem os lockdowns ou secures disponiveis para esse contrato."
							: "O dono dessa propriedade nao pode alugar esse espaço nesse momento."));
				}
			}
			else
			{
				if (Owner == c_Contract.RentalMaster)
				{
					AddHtml(60, y += 20, 100, HTML.Black + "Cancelar Contrato");
					AddButton(40, y + 3, 0x837, 0x838, "Cancelar Contrato", CancelContract);
				}
				else
				{
					AddImage(width - 60, y += 20, 0x232C);
				}
			}

			AddBackgroundZero(0, 0, width, y + 23, 0x24A4);
		}

		protected override void OnClose()
		{
			c_Contract.ClearPreview();
		}

		private void Preview()
		{
			c_Contract.ShowAreaPreview(Owner);
			NewGump();
		}

		private void CancelContract()
		{
			if (Owner == c_Contract.RentalClient)
			{
				c_Contract.House.Delete();
			}
			else
			{
				c_Contract.Delete();
			}
		}

		private void Accept()
		{
			if (!c_Contract.ValidateLocSec())
			{
				Owner.SendMessage("O dono dessa propriedade nao pode alugar esse espaço nesse momento.");
				return;
			}

			c_Contract.Purchase(Owner);

			if (!c_Contract.Owned)
			{
				return;
			}

			c_Contract.Visible = true;
			c_Contract.RentalClient = Owner;
			c_Contract.RentalClient.AddToBackpack(new RentalContractCopy(c_Contract));
		}
	}
}
