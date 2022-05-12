#region AuthorHeader
//
//	Auction version 2.1, by Xanthos and Arya
//
//  Based on original ideas and code by Arya
//
#endregion AuthorHeader
using System;
using System.Collections;

using Server;
using Server.Gumps;
using Leilaum.Utilities;

namespace Server.Leilaum
{
	/// <summary>
	/// The admin gump for the auction system
	/// </summary>
	public class AuctionAdminGump : Gump
	{
		public AuctionAdminGump( Mobile m ) : base ( 100, 100 )
		{
			m.CloseGump( typeof( AuctionAdminGump ) );
			MakeGump();
		}

		private void MakeGump()
		{
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage(0);
			AddBackground(0, 0, 270, 270, 9350);
			AddAlphaRegion(0, 0, 270, 270);
			AddLabel(36, 5, LUtils.kRedHue, @"Auction System Administration");
			AddImageTiled(16, 30, 238, 1, 9274);

			AddLabel(15, 65, LUtils.kLabelHue, string.Format( @"Deadline: {0} at {1}", AuctionScheduler.Deadline.ToShortDateString(), AuctionScheduler.Deadline.ToShortTimeString() ) );
			AddLabel(15, 40, LUtils.kGreenHue, string.Format( @"{0} Auctions, {1} Pending", AuctionSystem.Auctions.Count, AuctionSystem.Pending.Count ) );

			// B 1 : Validate
			AddButton(15, 100, 30533, 30534, 1, GumpButtonType.Reply, 0);
			AddLabel(55, 100, LUtils.kLabelHue, @"Force Verification");

			// B 2 : Profile
			AddButton(15, 130, 30533, 30534, 2, GumpButtonType.Reply, 0);
			AddLabel(55, 130, LUtils.kLabelHue, @"Profile the System");

			// B 3 : Temporary Shutdown
			AddButton(15, 160, 30533, 30534, 3, GumpButtonType.Reply, 0);
			AddLabel(55, 160, LUtils.kLabelHue, @"Temporarily Shut Down");

			// B 4 : Delete
			AddButton(15, 190, 30533, 30534, 4, GumpButtonType.Reply, 0);
			AddLabel(55, 190, LUtils.kLabelHue, @"Permanently Shut Down");

			// B 0 : Close
			AddButton(15, 230, 30535, 30535, 0, GumpButtonType.Reply, 0);
			AddLabel(55, 230, LUtils.kLabelHue, @"Exit");
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			switch ( info.ButtonID )
			{
				case 1: // Validate

					AuctionSystem.VerifyAuctions();
					AuctionSystem.VerifyPendencies();

					sender.Mobile.SendGump( new AuctionAdminGump( sender.Mobile ) );
					break;

				case 2: // Profile

					AuctionSystem.ProfileAuctions();

					sender.Mobile.SendGump( new AuctionAdminGump( sender.Mobile ) );
					break;

				case 3: // Disable

					AuctionSystem.Disable();
					sender.Mobile.SendMessage( AuctionConfig.MessageHue, "The system has been stopped. It will be restored with the next reboot." );
					break;

				case 4: // Delete

					sender.Mobile.SendGump( new DeleteAuctionGump( sender.Mobile ) );
					break;
			}
		}

	}
}
