using System;
using Server;
using Server.TournamentSystem;

namespace Server.Items
{
    public class ArenaWall : Item
    {
        public ArenaWall(PVPTournamentSystem sys) : base(sys.WallItemID)
        {
            Movable = false;
            Name = "Arena Wall";

            Timer.DelayCall(sys.PreFightDelay, new TimerCallback(Delete));
        }

        public ArenaWall(Serial serial)
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
            Delete();
        }
    }
}
