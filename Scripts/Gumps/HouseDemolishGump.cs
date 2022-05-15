#region References
using Server.Accounting;
using Server.Guilds;
using Server.Items;
using Server.Multis;
using Server.Network;
#endregion

namespace Server.Gumps
{
	public class HouseDemolishGump : Gump
	{
		private readonly BaseHouse m_House;
		private readonly Mobile m_Mobile;

		public HouseDemolishGump(Mobile mobile, BaseHouse house)
			: base(110, 100)
		{
			m_Mobile = mobile;
			m_House = house;

			mobile.CloseGump(typeof(HouseDemolishGump));

			Closable = false;

			AddPage(0);

			AddBackground(0, 0, 420, 280, 9350);

			AddImageTiled(10, 10, 400, 20, 2624);
			AddAlphaRegion(10, 10, 400, 20);

			AddHtml(10, 10, 400, 20, "<center>ATENCAO</center>", 30720, false, false); // <CENTER>WARNING</CENTER>

			AddImageTiled(10, 40, 400, 200, 2624);
			AddAlphaRegion(10, 40, 400, 200);

			AddHtml(10, 40, 400, 200, @"Você está prestes a demolir sua casa.
O valor da casa será devolvido diretamente na sua caixa bancária.
Todos os itens da casa permanecerão para trás e podem ser livremente recolhidos por qualquer pessoa.
Após a demolição da casa, qualquer pessoa pode tentar colocar uma nova casa no terreno baldio.
Esta ação não cancelará a condenação de nenhuma outra casa em sua conta, nem encerrará seu período de espera de 7 dias (se aplicável a você).
Tem certeza que deseja continuar?", 32512, false, true);

			AddImageTiled(10, 250, 400, 20, 2624);
			AddAlphaRegion(10, 250, 400, 20);

			AddButton(10, 250, 30533, 30534, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(40, 250, 170, 20, 1011036, 32767, false, false); // OKAY

			AddButton(210, 250, 30533, 30535, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(240, 250, 170, 20, 1011012, 32767, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (info.ButtonID == 1 && !m_House.Deleted)
			{
				if (m_House.IsOwner(m_Mobile))
				{
					if (m_House.MovingCrate != null || m_House.InternalizedVendors.Count > 0)
					{
						return;
					}

					if (!Guild.NewGuildSystem && m_House.FindGuildstone() != null)
					{
						m_Mobile.SendLocalizedMessage(501389); // You cannot redeed a house with a guildstone inside.
						return;
					}
					/*
					if (m_House.PlayerVendors.Count > 0)
                    {
						m_Mobile.SendLocalizedMessage(503236); // You need to collect your vendor's belongings before moving.
						return;
                    }
					*/
					if (m_House.HasRentedVendors && m_House.VendorInventories.Count > 0)
					{
						m_Mobile.SendLocalizedMessage(1062679);
						// You cannot do that that while you still have contract vendors or unclaimed contract vendor inventory in your house.
						return;
					}

					if (m_House.HasRentedVendors)
					{
						m_Mobile.SendLocalizedMessage(1062680);
						// You cannot do that that while you still have contract vendors in your house.
						return;
					}

					if (m_House.VendorInventories.Count > 0)
					{
						m_Mobile.SendLocalizedMessage(1062681);
						// You cannot do that that while you still have unclaimed contract vendor inventory in your house.
						return;
					}

                    else if (m_House.HasActiveAuction)
                    {
                        m_Mobile.SendLocalizedMessage(1156453); 
                        // You cannot currently take this action because you have auction safes locked down in your home. You must remove them first.
                        return;
                    }

					if (m_Mobile.AccessLevel >= AccessLevel.GameMaster)
					{
						m_Mobile.SendMessage("You do not get a refund for your house as you are not a player");
						m_House.RemoveKeys(m_Mobile);
						m_House.Delete();
					}
					else
					{
						Item toGive;

						if (m_House.IsAosRules)
						{
                            var deed = m_House.GetDeed();
                            if(deed != null)
                            {
                                toGive = deed;
                            } else
                            {
                                toGive = new BankCheck(m_House.Price);
                            }
                           
						}
						else
						{
                            var deed = m_House.GetDeed();
                            if (deed != null)
                            {
                                toGive = deed;
                            }
                            else
                            {
                                toGive = new BankCheck(m_House.Price);
                            }
                        }

						if (AccountGold.Enabled && toGive is BankCheck)
						{
							var worth = ((BankCheck)toGive).Worth;

							if (m_Mobile.Account != null && m_Mobile.Account.DepositGold(worth))
							{
								toGive.Delete();

								m_Mobile.SendLocalizedMessage("Valor da casa depositado na conta: "+worth.ToString("#,0"));
								// ~1_AMOUNT~ gold has been deposited into your bank box.

								m_House.RemoveKeys(m_Mobile);
								m_House.Delete();
								return;
							}
						}

						if (toGive != null)
						{
							var box = m_Mobile.BankBox;

							if (box.TryDropItem(m_Mobile, toGive, false))
							{
								if (toGive is BankCheck)
								{
									m_Mobile.SendLocalizedMessage(1060397, ((BankCheck)toGive).Worth.ToString("#,0"));
									// ~1_AMOUNT~ gold has been deposited into your bank box.
								}

								m_House.RemoveKeys(m_Mobile);
								m_House.Delete();
							}
							else
							{
								toGive.Delete();
								m_Mobile.SendLocalizedMessage(500390); // Your bank box is full.
							}
						}
						else
						{
							m_Mobile.SendMessage("Unable to refund house.");
						}
					}
				}
				else
				{
					m_Mobile.SendLocalizedMessage(501320); // Only the house owner may do this.
				}
			}
		}
	}
}
