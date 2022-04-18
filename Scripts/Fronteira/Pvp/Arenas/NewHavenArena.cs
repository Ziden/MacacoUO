using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Items;
using Server.Engines.ArenaSystem;

namespace Server.TournamentSystem
{
    public class HavenArena : PVPTournamentSystem
    {
        public override string DefaultName { get { return "Haven Island Arena (Trammel)"; } }
        public override SystemType SystemType { get { return SystemType.NewHavenTram; } }
        public override Map ArenaMap { get { return Map.Trammel; } }

        private ArenaDefinition _Definition;

        public override ArenaDefinition Definition
        {
            get
            {
                if (_Definition == null)
                {
                    _Definition = new ArenaDefinition(new Point3D(3786, 2763, 6), new Point3D(3763, 2769, 5), new Point3D(3782, 2769, 5),
                                                     new Point3D(3793, 2770, 6), new Point3D(3790, 2775, 7), new Point3D(3790, 2762, 7),
                                                     new Point3D(3786, 2771, 6), new Point3D(3786, 2767, 6), new Point3D(3786, 2769, 8),
                                                     new Point3D(3790, 2770, 6), new Point3D(3787, 2761, 13), new Rectangle2D(new Point3D(3791, 2763, 6), new Point3D(3794, 2775, 6)),
                                                     new Rectangle2D(new Point3D(3772, 2761, 5), new Point3D(3773, 2776, 5)),
                                                     new Rectangle2D[] 
                                                     {
                                                         new Rectangle2D( 3760, 2761, 25, 16 )
                                                     },
                                                     new Rectangle2D[]
                                                     {
                                                         new Rectangle2D( 3754, 2751, 46, 9 ), new Rectangle2D( 3786, 2761, 15, 34 ),
                                                         new Rectangle2D( 3742, 2745, 17, 50), new Rectangle2D( 3760, 2778, 39, 20),
                                                     },
                                                     new Rectangle2D(3760, 2761, 25, 16));
                }

                return _Definition;
            }
        }

        public HavenArena(TournamentStone stone) : base(stone)
        {
            Active = true;
        }

        public static void Setup(Mobile from)
        {
            if (ArenaHelper.HasArena<HavenArena>())
            {
                from.SendMessage(22, "Ocllo Arena already exists!");
                return;
            }

            if (PVPArenaSystem.Instance != null && PVPArenaSystem.Arenas != null)
            {
                var oldArena = PVPArenaSystem.Arenas.FirstOrDefault(a => a.Definition == Server.Engines.ArenaSystem.ArenaDefinition.HavenTrammel);

                if (oldArena != null)
                {
                    PVPArenaSystem.Instance.AddBlockedArena(oldArena);
                    from.SendMessage(22, "Old Arena Removed: {0} [Trammel]", oldArena.Definition.Name);
                }
            }

            HavenStone stone = new HavenStone();
            HavenArena arena = new HavenArena(stone);
            stone.MoveToWorld(arena.StoneLocation, arena.ArenaMap);

            from.SendMessage(1154, "Haven Arena setup!");
        }

        public static void Delete(Mobile from)
        {
            var arena = ArenaHelper.GetArena<HavenArena>();

            if (arena == null)
                return;

            if (arena.Stone != null)
            {
                arena.Stone.Delete();
                from.SendMessage(22, "Haven Arena removed!");
            }
            else
            {
                from.SendMessage(22, "Error removing Haven Arena.");
            }

            if (PVPArenaSystem.Instance != null && PVPArenaSystem.Instance.IsBlocked(Server.Engines.ArenaSystem.ArenaDefinition.HavenTrammel))
            {
                PVPArenaSystem.Instance.RemoveBlockedArena(Server.Engines.ArenaSystem.ArenaDefinition.HavenTrammel);
            }
        }

        public override void SetLinkedSystem()
        {
            foreach (PVPTournamentSystem system in PVPTournamentSystem.SystemList)
            {
                if (system is HavenFelArena)
                {
                    LinkedSystem = system;

                    if (system.LinkedSystem != this)
                        system.LinkedSystem = this;
                }
            }
        }

        public HavenArena(GenericReader reader, TournamentStone stone) : base(reader, stone)
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
    public class HavenStone : TournamentStone
    {
        public HavenStone()
        {
        }

        public HavenStone(Serial serial) : base (serial)
        {
        }

        public override void LoadSystem(GenericReader reader)
        {
            System = new HavenArena(reader, this);
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
