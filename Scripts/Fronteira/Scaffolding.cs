using System;
using Server.Gumps;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class ScaffoldingAddon : BaseAddon
    {
        [Constructable]
        public ScaffoldingAddon(DirectionType type)
        {
            switch (type)
            {
                case DirectionType.South:
                    AddComponent(new LocalizedAddonComponent(4786, 1024785), 0, -1, 0);
                    AddComponent(new LocalizedAddonComponent(4785, 1024785), 0, 0, 0);
                    AddComponent(new LocalizedAddonComponent(2212, 1076287), 0, 1, 0);
                    AddComponent(new LocalizedAddonComponent(4786, 1024785), 0, -1, 15);
                    AddComponent(new LocalizedAddonComponent(4785, 1024785), 0, 0, 15);
                    break;
                case DirectionType.East:
                    AddComponent(new LocalizedAddonComponent(4808, 1024785), -1, 0, 0);
                    AddComponent(new LocalizedAddonComponent(4809, 1024785), 0, 0, 0);
                    AddComponent(new LocalizedAddonComponent(2209, 1076287), 1, 0, 0);
                    AddComponent(new LocalizedAddonComponent(4808, 1024785), -1, 0, 15);
                    AddComponent(new LocalizedAddonComponent(4809, 1024785), 0, 0, 15);
                    break;
            }
        }

        

        public ScaffoldingAddon(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed { get { return new ScaffoldingDeed(); } }

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

    public class ScaffoldingDeed : BaseAddonDeed, IRewardOption
    {
        public override int LabelNumber { get { return 1024785; } } // Scaffold

        public override BaseAddon Addon { get { return new ScaffoldingAddon(_Direction); } }

        private DirectionType _Direction;

        [Constructable]
        public ScaffoldingDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public ScaffoldingDeed(Serial serial)
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
