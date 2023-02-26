namespace Server.Items
{
	public class VendorTile : Item
    {
        [Constructable]
        public VendorTile()
            : base(0x181D)
        {
            Name = "a vendor tile";
            Movable = false;
        }

        public override bool Decays
        {
            get
            {
                return false;
            }
        }

        public VendorTile(Serial serial)
            : base(serial)
        { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }
}
