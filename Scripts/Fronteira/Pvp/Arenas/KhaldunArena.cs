using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Items;
using Server.Commands;
using Server.Engines.ArenaSystem;

namespace Server.TournamentSystem
{
    public class KhaldunArena : PVPTournamentSystem
    {
        public override string DefaultName { get { return "Khaldun Grand Arena (Felucca)"; } }
        public override SystemType SystemType { get { return SystemType.KhaldunFel; } }
        public override Map ArenaMap { get { return Map.Felucca; } }
        public override int WallItemID { get { return 0x82; } }

        private ArenaDefinition _Definition;

        public override ArenaDefinition Definition
        {
            get
            {
                if (_Definition == null)
                {
                    _Definition = new ArenaDefinition(new Point3D(6103, 3724, 25), new Point3D(6078, 3720, 20), new Point3D(6090, 3719, 20),
                                                     new Point3D(6102, 3721, 25), new Point3D(6100, 3722, 26), new Point3D(6100, 3719, 26),
                                                     new Point3D(6099, 3721, 23), new Point3D(6099, 3723, 25), new Point3D(6099, 3719, 25),
                                                     new Point3D(6099, 3724, 25), new Point3D(6098, 3727, 27),
                                                     new Rectangle2D(new Point3D(6101, 3718, 25), new Point3D(6103, 3724, 25)),
                                                     new Rectangle2D(new Point3D(6083, 3713, 20), new Point3D(6084, 3728, 20)),
                                                     new Rectangle2D[]
                                                     {
                                                         //new Rectangle2D( 6071, 3713, 26, 16 )
                                                         new Rectangle2D(6077, 3713, 14, 16),
                                                         new Rectangle2D(6091, 3714, 2, 14),
                                                         new Rectangle2D(6093, 3715, 1, 13),
                                                         new Rectangle2D(6094, 3716, 1, 10),
                                                         new Rectangle2D(6095, 3717, 1, 9),
                                                         new Rectangle2D(6096, 3719, 1, 6),

                                                         new Rectangle2D(6075, 3714, 2, 14),
                                                         new Rectangle2D(6074, 3715, 1, 13),
                                                         new Rectangle2D(6073, 3716, 1, 11),
                                                         new Rectangle2D(6072, 3717, 1, 9),
                                                         new Rectangle2D(6071, 3719, 1, 5)
                                                     },
                                                     new Rectangle2D[]
                                                     {
                                                         new Rectangle2D( 6066, 3702, 41, 9 ), new Rectangle2D( 6067, 3730, 41, 13 ),
                                                         new Rectangle2D( 6099, 3710, 8, 36 ), new Rectangle2D( 6092, 3731, 143, 91)
                                                     },
                                                     new Rectangle2D(6077, 3713, 13, 14));
                }

                return _Definition;
            }
        }

        public KhaldunArena(TournamentStone stone) : base(stone)
        {
            Active = true;
        }

        public static void Setup(Mobile from)
        {
            if (ArenaHelper.HasArena<KhaldunArena>())
            {
                from.SendMessage(22, "Khaldun Fel Arena already exists!");
                return;
            }

            if (PVPArenaSystem.Instance != null && PVPArenaSystem.Arenas != null)
            {
                var oldArena = PVPArenaSystem.Arenas.FirstOrDefault(a => a.Definition == Server.Engines.ArenaSystem.ArenaDefinition.LostLandsFelucca);

                if (oldArena != null)
                {
                    PVPArenaSystem.Instance.AddBlockedArena(oldArena);
                    from.SendMessage(22, "Old Arena Removed: {0}", oldArena.Definition.Name);
                }
            }

            KhaldunStone stone = new KhaldunStone();
            KhaldunArena arena = new KhaldunArena(stone);
            stone.MoveToWorld(arena.StoneLocation, Map.Felucca);

            from.SendMessage(1154, "Khaldun Fel setup!");
        }

        public static void Delete(Mobile from)
        {
            var arena = ArenaHelper.GetArena<KhaldunArena>();

            if (arena == null)
                return;

            if (arena.Stone != null)
            {
                arena.Stone.Delete();
                from.SendMessage(22, "Khaldun Fel Arena removed!");
            }
            else
            {
                from.SendMessage(22, "Error removing Khaldun Fel Arena.");
            }

            if (PVPArenaSystem.Instance != null && PVPArenaSystem.Instance.IsBlocked(Server.Engines.ArenaSystem.ArenaDefinition.LostLandsFelucca))
            {
                PVPArenaSystem.Instance.RemoveBlockedArena(Server.Engines.ArenaSystem.ArenaDefinition.LostLandsFelucca);
            }
        }

        public override void SetLinkedSystem()
        {
            foreach (PVPTournamentSystem system in PVPTournamentSystem.SystemList)
            {
                if (system is KhaldunArenaTram)
                {
                    LinkedSystem = system;

                    if (system.LinkedSystem != this)
                        system.LinkedSystem = this;
                }
            }
        }

        public KhaldunArena(GenericReader reader, TournamentStone stone) : base(reader, stone)
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
    public class KhaldunStone : TournamentStone
    {
        public KhaldunStone()
        {
        }

        public KhaldunStone(Serial serial) : base (serial)
        {
        }

        public override void LoadSystem(GenericReader reader)
        {
            System = new KhaldunArena(reader, this);
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
