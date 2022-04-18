using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Items;
using Server.Engines.ArenaSystem;

namespace Server.TournamentSystem
{
    public class TokunoArena : PVPTournamentSystem
    {
        public override string DefaultName { get { return "Tokuno Arena"; } }
        public override Map ArenaMap { get { return Map.Tokuno; } }
        public override SystemType SystemType { get { return SystemType.Tokuno; } }

        private ArenaDefinition _Definition;

        public override ArenaDefinition Definition
        {
            get
            {
                if (_Definition == null)
                {
                    _Definition = new ArenaDefinition(new Point3D(366, 1044, 11), new Point3D(363, 1057, 11), new Point3D(379, 1056, 11),
                                                     new Point3D(371, 1047, 11), new Point3D(365, 1049, 21), new Point3D(365, 1045, 21),
                                                     new Point3D(360, 1044, 10), new Point3D(362, 1044, 13), new Point3D(364, 1044, 10),
                                                     new Point3D(364, 1050, 11), new Point3D(359, 1048, 19), new Rectangle2D(366, 1043, 11, 8),
                                                     new Rectangle2D(371, 1052, 1, 10),
                                                     new Rectangle2D[] 
                                                     {
                                                         new Rectangle2D( 361, 1052, 20, 10 )
                                                     },
                                                     new Rectangle2D[]
                                                     {
                                                         new Rectangle2D( 355, 1051, 4, 11 ), new Rectangle2D( 383, 1051, 4, 11 ),
                                                         new Rectangle2D( 355, 1043, 32, 8), new Rectangle2D( 355, 1063, 32, 8),
                                                     },
                                                     new Rectangle2D(361, 1052, 20, 10));
                }

                return _Definition;
            }
        }

        public TokunoArena(TournamentStone stone) : base(stone)
        {
            Active = true;
        }

        public override void InitializeSystem()
        {
            base.InitializeSystem();

            StatsBoard.ItemID = 7774;
            TournamentBoard.ItemID = 7774;
            TeamsBoard.ItemID = 7774;
        }

        public static void Setup(Mobile from)
        {
            if (ArenaHelper.HasArena<TokunoArena>())
            {
                from.SendMessage(22, "Tokukno Arena already exists!");
                return;
            }

            TokunoStone stone = new TokunoStone();
            TokunoArena arena = new TokunoArena(stone);
            stone.MoveToWorld(arena.StoneLocation, arena.ArenaMap);

            from.SendMessage(1154, "Tokuno Arena setup!");
        }

        public static void Delete(Mobile from)
        {
            var arena = ArenaHelper.GetArena<TokunoArena>();

            if (arena == null)
                return;

            if (arena.Stone != null)
            {
                arena.Stone.Delete();
                from.SendMessage(22, "Tokuno Arena removed!");
            }
            else
            {
                from.SendMessage(22, "Error removing Tokuno Arena.");
            }
        }

        public TokunoArena(GenericReader reader, TournamentStone stone) : base(reader, stone)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            /*int version = */reader.ReadInt();
        }
    }

    [DeleteConfirm("Are you sure you want to delete this? Deleting this stone will remove any upcoming tournaments and any prize items and all of the arena.")]
    public class TokunoStone : TournamentStone
    {
        public TokunoStone()
        {
        }

        public TokunoStone(Serial serial) : base (serial)
        {
        }

        public override void LoadSystem(GenericReader reader)
        {
            System = new TokunoArena(reader, this);
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
        }
    }
}
