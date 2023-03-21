using System;
using Server;
using Server.Gumps;
using System.IO;
using Server.Items;
using Server.Network;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Server.Items
{
	[Flipable(0x1E5E, 0x1E5F)]
	public class AnnouncementBoard : Item
	{
		[Constructable]
		public AnnouncementBoard( ) : base( 0x1E5E )
		{
			Weight = 1.0;
			Name = "Announcements";
			Movable = false;
		}
		public override void OnDoubleClick( Mobile m )
		{
			if (m.AccessLevel < AccessLevel.GameMaster)
			{
				m.SendGump( new AnnouncementGump( ) );
			}
			else
			{
				m.SendGump( new AnnouncementEditGump(0) );
			}
		}

		public AnnouncementBoard(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}

namespace Server.Gumps
{
	public class AnnouncementGump : Gump
	{
		public static void Initialize()
		{
			EventSink.Login += new LoginEventHandler( OnLogin );
		}
		
		public static void OnLogin( LoginEventArgs args )
		{
			Mobile m = args.Mobile;
			m.SendGump( new AnnouncementGump() );
		}

		
		public AnnouncementGump()
			: base( 0, 0 )
		{
			this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;
			this.AddPage(0);
			this.AddBackground(60, 115, 400, 400, 2520);
			this.AddLabel(185, 150, 88, @"NEWS and Announcements");
			
			string news = null;
			string newsaccumulator = null;
			string path = "Data/Announcements.txt";
			StreamReader sr;
			
			if ( File.Exists( path ))
			{
				try{
						sr = new StreamReader(path, System.Text.Encoding.Default, false);
						while (!sr.EndOfStream)
						{
							newsaccumulator += sr.ReadLine();
							newsaccumulator += "<BR>";
						}
						if (newsaccumulator != null)
							news = newsaccumulator.ToString();
						
						if (news != null)
						{
							this.AddHtml( 98, 179, 332, 285, @"<basefont color=black>" + news + "</basefont>", (bool)false, (bool)true);
							sr.Close();
						}
						else
						{
							this.AddHtml( 98, 179, 332, 285, @"<basefont color=black>...no NEWS at this time...</basefont>", (bool)false, (bool)true);
							sr.Close();
						}
					}
				catch (Exception e)
				{
					Console.WriteLine("Announcements file error occurred");
					Console.WriteLine(e.ToString());
				}
			}
			else
			{
				using ( StreamWriter sw = new StreamWriter("Data/Announcements.txt", false) )
				{
					news = "No NEWS";
					sw.WriteLine(news);
					sw.Close();
				}
				this.AddHtml( 98, 179, 332, 285, @"<basefont color=black>There are no announcements.</basefont>", (bool)false, (bool)true);
			}
		}
	}
	
	public class AnnouncementEditGump : Gump
	{
		private string path = "Data/Announcements.txt";
		private List<string> news = new List<string>();
		private int CurrentAnnouncement; //which page we're on
		private int Announcements; //number of announcements
		
		public AnnouncementEditGump(int current): base( 0, 0 )
		{
			this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;
			this.AddPage(0);
			this.AddBackground(160, 130, 400, 250, 9200);
			this.AddLabel(295, 150, 88, @"Announcement Editor");
			this.AddBackground(200, 190, 325, 100, 3000);
			
			string newsreader = null;
			StreamReader sr;
			CurrentAnnouncement = current;
			
			if ( File.Exists( path ))
			{
				try{
						sr = new StreamReader(path, System.Text.Encoding.Default, false);
						Announcements=0;
						while (!sr.EndOfStream)
						{
							newsreader = sr.ReadLine();
							news.Add(newsreader);
							++Announcements;
						}
						sr.Close();
						
						if ((Announcements > 0) && (CurrentAnnouncement <= Announcements-1))
						{
							this.AddTextEntry(205, 195, 320, 95, 0, 1, news[CurrentAnnouncement]);
						}
						else
						{
							news.Add("No NEWS...bad read");
							this.AddTextEntry(205, 195, 320, 95, 0, 1, news[0]);
						}
					}
				catch (Exception e)
				{
					Console.WriteLine("Announcements file error occurred");
					Console.WriteLine(e.ToString());
				}
			}
			else
			{
				using ( StreamWriter sw = new StreamWriter("Data/Announcements.txt", false) )
				{
					news[0] = "No NEWS...no file";
					sw.WriteLine(news[0]);
					sw.Close();
				}
				this.AddHtml( 205, 195, 320, 95, @"<basefont color=black>Announcements not found</basefont>", (bool)false, (bool)true);
			}
			
			this.AddButton(195, 300, 2460, 2461, 3, GumpButtonType.Reply, 0); //add 3
			this.AddButton(255, 300, 2463, 2464, 4, GumpButtonType.Reply, 0); //delete 4
			this.AddLabel(332, 295, 88, (CurrentAnnouncement+1).ToString());
			this.AddLabel(350, 295, 88, @":");
			this.AddLabel(356, 295, 88, Announcements.ToString());
			this.AddButton(380, 300, 2466, 2467, 5, GumpButtonType.Reply, 0); //previous 5
			this.AddButton(470, 300, 2469, 2470, 6, GumpButtonType.Reply, 0); //next 6
			
			this.AddButton(340, 350, 2450, 2451, 2, GumpButtonType.Reply, 0); //Okay = 2
			this.AddButton(410, 350, 2453, 2454, 0, GumpButtonType.Reply, 0); //Cancel Button = 0
			this.AddLabel(230, 355, 88, @"Preview");
			this.AddButton(200, 350, 2472, 2473, 7, GumpButtonType.Reply, 0); //Preview = 7
		}
		
		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile from = sender.Mobile;
			if ( from == null )
				return;
			
			if (info.ButtonID == 0) //Cancel 
			{
				from.SendMessage("Edit Canceled");
			}
			else if (info.ButtonID == 2) //Okay 
			{
				news[CurrentAnnouncement] = (string)info.GetTextEntry(1).Text;
				using ( StreamWriter sw = new StreamWriter("Data/Announcements.txt", false) )
				{
					for (int i=0;i<Announcements;++i)
					{
						sw.WriteLine(news[i]);
					}
					sw.Close();
				}
				from.CloseGump( typeof(AnnouncementEditGump) );
				from.SendGump( new AnnouncementEditGump(CurrentAnnouncement) );
			}
			else if (info.ButtonID == 3) //add
			{
				news.Add((string)info.GetTextEntry(1).Text);
				++Announcements;
				CurrentAnnouncement = Announcements-1;
				using ( StreamWriter sw = new StreamWriter("Data/Announcements.txt", false) )
				{
					for (int i=0;i<Announcements;++i)
					{
						sw.WriteLine(news[i]);
					}
					sw.Close();
				}
				from.CloseGump( typeof(AnnouncementEditGump) );
				from.SendGump( new AnnouncementEditGump(CurrentAnnouncement) );
			}
			else if (info.ButtonID == 4) //delete
			{
				if (Announcements > 1)
				{
					news.Remove(news[CurrentAnnouncement]);
					using ( StreamWriter sw = new StreamWriter("Data/Announcements.txt", false) )
					{
						for (int i=0;i<Announcements-1;++i)
						{
							sw.WriteLine(news[i]);
						}
						sw.Close();
					}
					if (CurrentAnnouncement-1 >= 0)
						--CurrentAnnouncement;
					else
						CurrentAnnouncement=0;
				}
				else
				{
					using ( StreamWriter sw = new StreamWriter("Data/Announcements.txt", false) )
					{
						sw.WriteLine("No NEWS or Announcements at this time.");
						sw.Close();
					}
				}
				from.CloseGump( typeof(AnnouncementEditGump) );
				from.SendGump( new AnnouncementEditGump(CurrentAnnouncement) );
			}
			else if (info.ButtonID == 5) //previous
			{
				if (CurrentAnnouncement > 0)
				{
					--CurrentAnnouncement;
				}
				else 
					CurrentAnnouncement = 0;
				from.CloseGump( typeof(AnnouncementEditGump) );
				from.SendGump( new AnnouncementEditGump(CurrentAnnouncement) );
			}
			else if (info.ButtonID == 6) //next
			{
				if (CurrentAnnouncement < Announcements-1)
				{
					++CurrentAnnouncement;
				}
				else
					CurrentAnnouncement = Announcements-1;
				from.CloseGump( typeof(AnnouncementEditGump) );
				from.SendGump( new AnnouncementEditGump(CurrentAnnouncement) );
			}
			else if (info.ButtonID == 7) //preview
			{
				from.CloseGump( typeof(AnnouncementEditGump) );
				from.SendGump( new AnnouncementEditGump(CurrentAnnouncement) );
				from.CloseGump( typeof(AnnouncementGump) );
				from.SendGump( new AnnouncementGump() );
			}
		}
	}
}