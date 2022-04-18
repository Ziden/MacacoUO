using Server.Items;

namespace Server.TournamentSystem
{
    public class MalasArena : PVPTournamentSystem
    {
        public override string DefaultName { get { return "Arena de Malas"; } }
        public override Map ArenaMap { get { return Map.Malas; } }
        public override SystemType SystemType { get { return SystemType.Malas; } }

        private ArenaDefinition _Definition;

        public override ArenaDefinition Definition
        {
            get
            {
                if (_Definition == null)
                {
                    _Definition = new ArenaDefinition(new Point3D(1016, 498, -70), //Pedra
                                                     new Point3D(417, 1325, 0), //Time A
                                                     new Point3D(433, 1325, 0), //Time B
                                                     new Point3D(1015, 498, -70), //NPC
                                                     new Point3D(417, 1314, 22), //Wage A
                                                     new Point3D(434, 1314, 22), //Wage B
                                                     new Point3D(1015, 497, -70), //Quadro de Estatísticas
                                                     new Point3D(1017, 497, -70), //Quadro de Torneios
                                                     new Point3D(1013, 497, -70), //Quadro de Times
                                                     new Point3D(1011, 497, -69), //Bau
                                                     new Point3D(1009, 497, -70), //Sino
                                                     new Rectangle2D(1014, 505, 1, 2), //Local para sair
                                                     new Rectangle2D(424, 1316, 1, 20), //Muro
                                                     new Rectangle2D[] 
                                                     {
                                                         new Rectangle2D( 415, 1316, 20, 20 ) //Campo de batalha
                                                     },
                                                     new Rectangle2D[]
                                                     {
                                                         new Rectangle2D( 355, 1051, 4, 11 ), new Rectangle2D( 383, 1051, 4, 11 ),
                                                         new Rectangle2D( 355, 1043, 32, 8), new Rectangle2D( 355, 1063, 32, 8),
                                                     },
                                                     new Rectangle2D(415, 1316, 20, 20));
                }

                return _Definition;
            }
        }

        public MalasArena(TournamentStone stone) : base(stone)
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
            if (ArenaHelper.HasArena<MalasArena>())
            {
                from.SendMessage(22, "A Arena de Malas já está criada!");
                return;
            }

            MalasStone stone = new MalasStone();
            MalasArena arena = new MalasArena(stone);
            stone.MoveToWorld(arena.StoneLocation, arena.ArenaMap);

            from.SendMessage(1154, "Arena de Malas configurada com sucesso!");
        }

        public static void Delete(Mobile from)
        {
            var arena = ArenaHelper.GetArena<MalasArena>();

            if (arena == null)
                return;

            if (arena.Stone != null)
            {
                arena.Stone.Delete();
                from.SendMessage(22, "Arena de Malas removida!");
            }
            else
            {
                from.SendMessage(22, "Erro ao remover a Arena de Malas.");
            }
        }

        public MalasArena(GenericReader reader, TournamentStone stone) : base(reader, stone)
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

    [DeleteConfirm("Tem certeza que deseja remover? Ao deletar esta pedra, todos os eventos futuros e premiações serão perdidas, e a arena será apagada.")]
    public class MalasStone : TournamentStone
    {
        public MalasStone()
        {
        }

        public MalasStone(Serial serial) : base (serial)
        {
        }

        public override void LoadSystem(GenericReader reader)
        {
            System = new MalasArena(reader, this);
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
