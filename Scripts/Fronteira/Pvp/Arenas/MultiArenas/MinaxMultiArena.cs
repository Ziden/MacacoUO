using System;
using Server;
using Server.Items;

namespace Server.TournamentSystem
{
    public class MinaxMultiArena : BaseMultiArena
    {
        private ArenaDefinition _Definition;

        public override ArenaDefinition Definition
        {
            get
            {
                if (_Definition == null)
                {
                    _Definition = new ArenaDefinition(new Point3D(26, -5, -4), new Point3D(-13, 0, 0), new Point3D(-13, 0, 23),
                                                      new Point3D(29, -8, -4), new Point3D(25, -4, 10), new Point3D(25, 2, 10),
                                                      new Point3D(24, -6, -4), new Point3D(24, -8, -4), new Point3D(24, -10, -4),
                                                      new Point3D(20, 10, -4), new Point3D(23, 10, 9),
                                                      new Rectangle2D(27, -9, 5, 12),
                                                      new Rectangle2D[]
                                                      {
                                                          new Rectangle2D(-18, -9, 4, 4),
                                                          new Rectangle2D(-18, 7, 4, 4),
                                                          new Rectangle2D(-4, -9, 4, 4),
                                                          new Rectangle2D(-4, 5, 4, 4),
                                                      },
                                                      new Rectangle2D[]
                                                      {
                                                          new Rectangle2D(-24, -13, 43, 26),
                                                          new Rectangle2D(19, -13, 4, 23)
                                                      },
                                                      new Rectangle2D[]
                                                      {
                                                          new Rectangle2D(23, -13, 20, 35),
                                                          new Rectangle2D(20, 10, 17, 12)
                                                      },
                                                      new Rectangle2D(-3, -5, 17, 9));
                }

                return _Definition;
            }
        }

        private static Point3D[] _FenceLocOffsets =
        {
            new Point3D(16, -3, 0),
            new Point3D(16, -2, 0),
            new Point3D(16, -1, 0),
            new Point3D(16, 0, 0),
            new Point3D(16, 1, 0),

            new Point3D(23, -3, 0),
            new Point3D(23, -2, 0),
            new Point3D(23, -1, 0),
            new Point3D(23, 0, 0),
            new Point3D(23, 1, 0)
        };

        private static Point3D[] _PedestalLocOffsets =
        {
            new Point3D(25, -4, 0),
            new Point3D(25, 2, 0),
        };

        [Constructable]
        public MinaxMultiArena()
            : base(5000)
        {
        }

        public override void OnSystemConfigured()
        {
            base.OnSystemConfigured();
            var entries = new MultiTileEntry[_FenceLocOffsets.Length + _PedestalLocOffsets.Length];

            for(int i = 0; i < _FenceLocOffsets.Length; i++)
            {
                var p = _FenceLocOffsets[i];

                entries[i] = new MultiTileEntry(2081, (short)p.X, (short)p.Y, (short)p.Z, TileFlag.Background);
            }

            for (int i = 0; i < _PedestalLocOffsets.Length; i++)
            {
                var p = _PedestalLocOffsets[i];

                entries[i + _FenceLocOffsets.Length] = new MultiTileEntry(4643, (short)p.X, (short)p.Y, (short)p.Z, TileFlag.Background);
            }

            AddMultiFixtures(entries);
        }

        public MinaxMultiArena(Serial serial)
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
