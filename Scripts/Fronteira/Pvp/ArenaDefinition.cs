using System;
using Server;

namespace Server.TournamentSystem
{
    public class ArenaDefinition
    {
        public static ArenaDefinition Blank { get; set; }

        public Point3D StoneLocation { get; set; }
        public Point3D TeamAStartLocation { get; set; }
        public Point3D TeamBStartLocation { get; set; }
        public Point3D ArenaKeeperLocation { get; set; }
        public Point3D TeamAWageDisplay { get; set; }
        public Point3D TeamBWageDisplay { get; set; }
        public Point3D StatsBoardLocation { get; set; }
        public Point3D TournamentInfoBoardLocation { get; set; }
        public Point3D TeamsBoardLocation { get; set; }
        public Point3D ChestLocation { get; set; }
        public Point3D BellLocation { get; set; }
        public Point3D LeagueBoardLocation { get; set; }
        public Point3D WaitingRoomLocation { get; set; }

        public Map ArenaMap;
        public Map ItemsMap;

        public Rectangle2D KickZone { get; set; }
        public Rectangle2D[] WallArea { get; set; }
        public Rectangle2D[] FightingRegionBounds { get; set; }
        public Rectangle2D[] AudienceRegionBounds { get; set; }
        public Rectangle2D RandomStartBounds { get; set; }

        public ArenaDefinition(Point3D stoneLoc, Point3D teamAStart, Point3D teamBStart, Point3D keeperLoc, Point3D wageALoc, Point3D wageBLoc,
                                Point3D statsBoardLoc, Point3D tournyBoardLoc, Point3D teamsBoardLoc, Point3D chestLoc, Point3D bellLoc,
                                Rectangle2D kickZone, Rectangle2D wallArea, Rectangle2D[] fightBounds, Rectangle2D[] audienceBounds, Rectangle2D randomStart)
            : this(stoneLoc, teamAStart, teamBStart, keeperLoc, wageALoc, wageBLoc, statsBoardLoc, tournyBoardLoc, teamsBoardLoc, chestLoc, bellLoc,
                                kickZone, new Rectangle2D[] { wallArea }, fightBounds, audienceBounds, randomStart, Point3D.Zero, Point3D.Zero)
        {
        }

        public ArenaDefinition(Point3D stoneLoc, Point3D teamAStart, Point3D teamBStart, Point3D keeperLoc, Point3D wageALoc, Point3D wageBLoc,
                        Point3D statsBoardLoc, Point3D tournyBoardLoc, Point3D teamsBoardLoc, Point3D chestLoc, Point3D bellLoc,
                        Rectangle2D kickZone, Rectangle2D[] wallArea, Rectangle2D[] fightBounds, Rectangle2D[] audienceBounds, Rectangle2D randomStart)
            : this(stoneLoc, teamAStart, teamBStart, keeperLoc, wageALoc, wageBLoc, statsBoardLoc, tournyBoardLoc, teamsBoardLoc, chestLoc, bellLoc,
                        kickZone, wallArea, fightBounds, audienceBounds, randomStart, Point3D.Zero, Point3D.Zero)
        {
        }

        public ArenaDefinition(Point3D stoneLoc, Point3D teamAStart, Point3D teamBStart, Point3D keeperLoc, Point3D wageALoc, Point3D wageBLoc,
                        Point3D statsBoardLoc, Point3D tournyBoardLoc, Point3D teamsBoardLoc, Point3D chestLoc, Point3D bellLoc,
                        Rectangle2D kickZone, Rectangle2D wallArea, Rectangle2D[] fightBounds, Rectangle2D[] audienceBounds, Rectangle2D randomStart, Point3D leagueLoc)
            : this(stoneLoc, teamAStart, teamBStart, keeperLoc, wageALoc, wageBLoc, statsBoardLoc, tournyBoardLoc, teamsBoardLoc, chestLoc, bellLoc,
                        kickZone, new Rectangle2D[] { wallArea }, fightBounds, audienceBounds, randomStart, leagueLoc, Point3D.Zero)
        {
        }

        public ArenaDefinition(Point3D stoneLoc, Point3D teamAStart, Point3D teamBStart, Point3D keeperLoc, Point3D wageALoc, Point3D wageBLoc,
                Point3D statsBoardLoc, Point3D tournyBoardLoc, Point3D teamsBoardLoc, Point3D chestLoc, Point3D bellLoc,
                Rectangle2D kickZone, Rectangle2D wallArea, Rectangle2D[] fightBounds, Rectangle2D[] audienceBounds, Rectangle2D randomStart, Point3D leagueLoc, Point3D platformLoc)
            : this(stoneLoc, teamAStart, teamBStart, keeperLoc, wageALoc, wageBLoc, statsBoardLoc, tournyBoardLoc, teamsBoardLoc, chestLoc, bellLoc,
                kickZone, new Rectangle2D[] { wallArea }, fightBounds, audienceBounds, randomStart, leagueLoc, platformLoc)
        {
        }

        public ArenaDefinition(Point3D stoneLoc, Point3D teamAStart, Point3D teamBStart, Point3D keeperLoc, Point3D wageALoc, Point3D wageBLoc,
                                Point3D statsBoardLoc, Point3D tournyBoardLoc, Point3D teamsBoardLoc, Point3D chestLoc, Point3D bellLoc,
                                Rectangle2D kickZone, Rectangle2D[] wallArea, Rectangle2D[] fightBounds, Rectangle2D[] audienceBounds, Rectangle2D randomStart, Point3D leagueLoc, Point3D platformLoc)
        {
            StoneLocation = stoneLoc;
            TeamAStartLocation = teamAStart;
            TeamBStartLocation = teamBStart;
            ArenaKeeperLocation = keeperLoc;
            TeamAWageDisplay = wageALoc;
            TeamBWageDisplay = wageBLoc;
            StatsBoardLocation = statsBoardLoc;
            TournamentInfoBoardLocation = tournyBoardLoc;
            TeamsBoardLocation = teamsBoardLoc;
            ChestLocation = chestLoc;
            BellLocation = bellLoc;
            LeagueBoardLocation = leagueLoc;
            WaitingRoomLocation = platformLoc;

            KickZone = kickZone;
            WallArea = wallArea;
            FightingRegionBounds = fightBounds;
            AudienceRegionBounds = audienceBounds;
            RandomStartBounds = randomStart;
        }

        public static void Configure()
        {
            Blank = new ArenaDefinition(Point3D.Zero, Point3D.Zero, Point3D.Zero, Point3D.Zero, Point3D.Zero, Point3D.Zero,
                Point3D.Zero, Point3D.Zero, Point3D.Zero, Point3D.Zero, Point3D.Zero,
                new Rectangle2D(0, 0, 0, 0), new Rectangle2D[] { }, new Rectangle2D[] { }, new Rectangle2D[] { }, new Rectangle2D(0, 0, 0, 0), Point3D.Zero, Point3D.Zero);
        }
    }
}
