using System;
using System.Linq;
using System.Collections.Generic;

using Server;
using Server.Mobiles;
using Server.Items;
using Server.Engines.ArenaSystem;

namespace Server.TournamentSystem
{
    public class HavenFelArena : PVPTournamentSystem
    {
        public override string DefaultName { get { return "Ocllo Arena (Felucca)"; } }
        public override SystemType SystemType { get { return SystemType.NewHavenFel; } }
        public override Map ArenaMap { get { return Map.Felucca; } }

        private ArenaDefinition _Definition;

        public override ArenaDefinition Definition
        {
            get
            {
                if (_Definition == null)
                {
                    _Definition = new ArenaDefinition(new Point3D(3775, 2759, 6), new Point3D(3752, 2765, 5), new Point3D(3771, 2765, 5),
                                                      new Point3D(3782, 2766, 6), new Point3D(3779, 2772, 7), new Point3D(3779, 2758, 7),
                                                      new Point3D(3775, 2763, 6), new Point3D(3775, 2767, 6), new Point3D(3775, 2765, 8),
                                                      new Point3D(3779, 2766, 6), new Point3D(3776, 2757, 12), new Rectangle2D(new Point3D(3780, 2756, 5), new Point3D(3783, 2774, 5)),
                                                      new Rectangle2D(new Point3D(3761, 2757, 5), new Point3D(3762, 2772, 5)),
                                                      new Rectangle2D[]
                                                      {
                                                          new Rectangle2D( 3749, 2757, 25, 16 )
                                                      },
                                                      new Rectangle2D[]
                                                      {
                                                          new Rectangle2D( 3743, 2747, 46, 9 ), new Rectangle2D( 3775, 2757, 15, 34 ),
                                                          new Rectangle2D( 3731, 2741, 17, 50), new Rectangle2D( 3749, 2774, 39, 20)
                                                      },
                                                      new Rectangle2D(3749, 2757, 25, 16));
                }

                return _Definition;
            }
        }

        public HavenFelArena(TournamentStone stone) : base(stone)
        {
            Active = true;
        }

        public static void Setup(Mobile from)
        {
            if (ArenaHelper.HasArena<HavenFelArena>())
            {
                from.SendMessage(22, "Ocllo Arena already exists!");
                return;
            }

            if (PVPArenaSystem.Instance != null && PVPArenaSystem.Arenas != null)
            {
                var oldArena = PVPArenaSystem.Arenas.FirstOrDefault(a => a.Definition == Server.Engines.ArenaSystem.ArenaDefinition.HavenFelucca);

                if (oldArena != null)
                {
                    PVPArenaSystem.Instance.AddBlockedArena(oldArena);
                    from.SendMessage(22, "Old Arena Removed: {0} [Felucca]", oldArena.Definition.Name);
                }
            }

            HavenFelStone stone = new HavenFelStone();
            HavenFelArena arena = new HavenFelArena(stone);
            stone.MoveToWorld(arena.StoneLocation, arena.ArenaMap);

            from.SendMessage(1154, "Ocllo Arena setup!");
        }

        public static void Delete(Mobile from)
        {
            var arena = ArenaHelper.GetArena<HavenFelArena>();

            if (arena == null)
                return;

            if (arena.Stone != null)
            {
                arena.Stone.Delete();
                from.SendMessage(22, "Ocllo Arena removed!");
            }
            else
            {
                from.SendMessage(22, "Error removing Ocllo Arena.");
            }

            if (PVPArenaSystem.Instance != null && PVPArenaSystem.Instance.IsBlocked(Server.Engines.ArenaSystem.ArenaDefinition.HavenFelucca))
            {
                PVPArenaSystem.Instance.RemoveBlockedArena(Server.Engines.ArenaSystem.ArenaDefinition.HavenFelucca);
            }
        }

        public override void SetLinkedSystem()
        {
            foreach (PVPTournamentSystem system in PVPTournamentSystem.SystemList)
            {
                if (system is HavenArena)
                {
                    LinkedSystem = system;

                    if (system.LinkedSystem != this)
                        system.LinkedSystem = this;
                }
            }
        }

        public HavenFelArena(GenericReader reader, TournamentStone stone) : base(reader, stone)
        {
        }
    }

    [DeleteConfirm("Are you sure you want to delete this? Deleting this stone will remove any upcoming tournaments and any prize items and all of the arena.")]
    public class HavenFelStone : TournamentStone
    {
        public HavenFelStone()
        {
        }

        public HavenFelStone(Serial serial) : base (serial)
        {
        }

        public override void LoadSystem(GenericReader reader)
        {
            System = new HavenFelArena(reader, this);
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
