using System;
using Server.Gumps;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class HammockAddon : BaseAddon
    {
        [Constructable]
        public HammockAddon(DirectionType type)
        {
            switch (type)
            {
                case DirectionType.South:
                    AddComponent(new LocalizedAddonComponent(4595, 1024592), 0, -1, 0);
                    AddComponent(new LocalizedAddonComponent(4594, 1024592), 0, 1, 0);
                    AddComponent(new LocalizedAddonComponent(411, 1024592), 0, -2, 2);
                    AddComponent(new LocalizedAddonComponent(417, 1024592), 0, 0, 2);
                    AddComponent(new LocalizedAddonComponent(417, 1024592), 0, 1, 2);
                    AddComponent(new LocalizedAddonComponent(416, 1024592), 0, -1, 2);
                    AddComponent(new LocalizedAddonComponent(418, 1024592), 0, 2, 2);
                    break;
                case DirectionType.East:
                    AddComponent(new LocalizedAddonComponent(411, 1024592), -2, 0, 1);
                    AddComponent(new LocalizedAddonComponent(413, 1024592), -1, 0, 1);
                    AddComponent(new LocalizedAddonComponent(415, 1024592), 2, 0, 1);
                    AddComponent(new LocalizedAddonComponent(414, 1024592), 1, 0, 1);
                    AddComponent(new LocalizedAddonComponent(4592, 1024592), -1, 0, 0);
                    AddComponent(new LocalizedAddonComponent(4593, 1024592), 1, 0, 0);
                    AddComponent(new LocalizedAddonComponent(414, 1024592), 0, 0, 1);
                    break;
            }
        }

        

        public HammockAddon(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed { get { return new HammockAddonDeed(); } }

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

    public class HammockAddonDeed : BaseAddonDeed, IRewardOption
    {
        public override int LabelNumber { get { return 1024592; } } // Hammock

        public override BaseAddon Addon { get { return new HammockAddon(_Direction); } }

        private DirectionType _Direction;

        [Constructable]
        public HammockAddonDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public HammockAddonDeed(Serial serial)
            : base(serial)
        {
        }

        public void GetOptions(RewardOptionList list)
        {
            list.Add((int)DirectionType.South, 1075386); // South
            list.Add((int)DirectionType.East, 1075387); // East
        }

        public void OnOptionSelected(Mobile from, int choice)
        {
            _Direction = (DirectionType)choice;

            if (!Deleted)
                base.OnDoubleClick(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(AddonOptionGump));
                from.SendGump(new AddonOptionGump(this, 1154194)); // Choose a Facing:
            }
            else
            {
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
            }
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
