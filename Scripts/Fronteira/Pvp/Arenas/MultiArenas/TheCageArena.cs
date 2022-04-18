//#define PVP_UsingNewMultis

#if PVP_UsingNewMultis
using System;
using Server;
using Server.Items;

namespace Server.TournamentSystem
{
    public class TheCageArena : BaseMultiArena
    {
        private ArenaDefinition _Definition;

        public override ArenaDefinition Definition
        {
            get
            {
                if (_Definition == null)
                {
                    _Definition = new ArenaDefinition(new Point3D(0, 18, 0), new Point3D(-4, 0, 7), new Point3D(5, 0, 7),
                                                      new Point3D(-3, 17, 0), new Point3D(2, 15, 6), new Point3D(0, 15, 6),
                                                      new Point3D(-4, 16, 4), new Point3D(-1, 16, 4), new Point3D(2, 16, 4),
                                                      new Point3D(6, 15, 0), new Point3D(5, 16, 8),
                                                      new Rectangle2D(-6, 17, 13, 5),
                                                      new Rectangle2D(0, -6, 2, 13),
                                                      new Rectangle2D[]
                                                      {
                                                          new Rectangle2D(-7, -6, 16, 13)
                                                      },
                                                      new Rectangle2D[]
                                                      {
                                                          new Rectangle2D(-18, -18, 11, 45),
                                                          new Rectangle2D(9, -18, 11, 45),
                                                          new Rectangle2D(-8, 7, 18, 20),
                                                          new Rectangle2D(-8, -18, 18, 12)
                                                      },
                                                      new Rectangle2D(-7, -6, 17, 14));
                }

                return _Definition;
            }
        }

        private static int[][] _StaticOffsets =
        {
            new int[] { -15, 16, 2, 1856 },
            new int[] { 16, 16, 2, 1854 },
            new int[] { 16, 15, 2, 1850 },
            new int[] { 16, 14, 2, 1850 }
        };

        public override string DefaultName { get { return "The Cage Arena"; } }

        [Constructable]
        public TheCageArena()
            : base("TheCageArena")
        {
        }

        public override void OnSystemConfigured()
        {
            base.OnSystemConfigured();
            var entries = new MultiTileEntry[_StaticOffsets.Length];

            for (int i = 0; i < _StaticOffsets.Length; i++)
            {
                var list = _StaticOffsets[i];

                entries[i] = new MultiTileEntry((ushort)list[3], (short)list[0], (short)list[1], (short)list[2], TileFlag.Background);
            }

            AddMultiFixtures(entries);

            Stone.System.StatsBoard.ItemID = 7774;
            Stone.System.TeamsBoard.ItemID = 7774;
            Stone.System.TournamentBoard.ItemID = 7774;
        }

        public TheCageArena(Serial serial)
            : base(serial)
        {
        }


        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
#endif
