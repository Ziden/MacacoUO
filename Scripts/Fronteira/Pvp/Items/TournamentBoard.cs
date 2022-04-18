using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.TournamentSystem;
using Server.Gumps;

namespace Server.Items
{
    public class TournamentBoard : Item
    {
        public override bool ForceShowProperties { get { return true; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public PVPTournamentSystem System { get; set; }

        public static List<TournamentBoard> Boards { get; set; }

        [Constructable]
        public TournamentBoard()
            : this(null)
        { 
        }

        [Constructable]
        public TournamentBoard(PVPTournamentSystem sys) : base(7775)
        {
            System = sys;
            Movable = false;
            Name = "Quadro do Torneio Atual";

            if (sys != null)
                sys.TournamentBoard = this;

            AddBoard(this);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(Location, 15))
                from.SendLocalizedMessage(500446); // That is too far away.
            else if (from is PlayerMobile)
            {
                if (System != null)
                {
                    if (from.HasGump(typeof(TournamentStatsGump2)))
                        from.CloseGump(typeof(TournamentStatsGump2));

                    if (from.HasGump(typeof(TournamentsGump)))
                        from.CloseGump(typeof(TournamentsGump));

                    if (System.CurrentTournament != null)
                        BaseGump.SendGump(new TournamentStatsGump2(System.CurrentTournament, null, from as PlayerMobile));
                    else
                        BaseGump.SendGump(new TournamentsGump(System, from as PlayerMobile));
                }
                else
                {
                    BaseGump.SendGump(new TournamentsGump(null, from as PlayerMobile));
                }
            }
        }

        public override void Delete()
        {
            base.Delete();

            RemoveBoard(this);
        }

        public static void UpdateBoards()
        {
            if (Boards == null)
            {
                return;
            }

            bool hasTourny = PVPTournamentSystem.SystemList.Any(sys => sys.Tournaments.Count > 0);

            foreach (var board in Boards)
            {
                if (board.System != null)
                {
                    if (board.System.Tournaments.Count > 0 && board.Hue != 2119)
                    {
                        board.Hue = 2119;
                    }
                    else if (board.System.Tournaments.Count == 0 && board.Hue != 0)
                    {
                        board.Hue = 0;
                    }
                }
                else
                {
                    if (hasTourny && board.Hue != 2119)
                    {
                        board.Hue = 2119;
                    }
                    else if (!hasTourny && board.Hue != 0)
                    {
                        board.Hue = 0;
                    }
                }
            }
        }

        public static void UpdateSystemHue()
        {
           
        }

        public static void AddBoard(TournamentBoard board)
        {
            if (Boards == null)
            {
                Boards = new List<TournamentBoard>();
            }

            Boards.Add(board);
        }

        public static void RemoveBoard(TournamentBoard board)
        {
            if (Boards != null)
            {
                Boards.Remove(board);

                if (Boards.Count == 0)
                {
                    Boards = null;
                }
            }
        }

        public TournamentBoard(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            AddBoard(this);
        }
    }
}
