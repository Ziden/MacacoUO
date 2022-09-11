using System;

namespace Server.Items
{
    public class SobretudodoCarrasco : HoodedShroudOfShadowsNoob
    {
        [Constructable]
        public SobretudodoCarrasco()
            : base()
        {
            Name = "Sobretudo Do Carrasco";
            Hue = 0;
        }


        public override bool Dye(Mobile from, DyeTub sender)
        {
            return true;
        }

        public SobretudodoCarrasco(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SobretudoScale : HoodedShroudOfShadowsNoob
    {
        [Constructable]
        public SobretudoScale()
            : base()
        {
            Name = "Sobretudo Scale";
            Hue = 1161;
            LootType = LootType.Blessed;
        }


        public override bool Dye(Mobile from, DyeTub sender)
        {
            return true;
        }

        public SobretudoScale(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    //Categoria = Vestuario !
    //Branco 
    public class AventalDaLuz : BaseWaist
    {
        [Constructable]
        public AventalDaLuz()
            : this(0)
        {
            Name = "Avental Da Luz";
            Hue = 2045;
            LootType = LootType.Blessed;
        }

        [Constructable]
        public AventalDaLuz(int hue)
            : base(0x153b, hue)
        {
            this.Weight = 2.0;
        }

        public AventalDaLuz(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class BandanaDaluz : BaseHat
    {
        public override int BasePhysicalResistance {
            get {
                return 0;
            }
        }
        public override int BaseFireResistance {
            get {
                return 3;
            }
        }
        public override int BaseColdResistance {
            get {
                return 5;
            }
        }
        public override int BasePoisonResistance {
            get {
                return 8;
            }
        }
        public override int BaseEnergyResistance {
            get {
                return 8;
            }
        }

        public override int InitMinHits {
            get {
                return 20;
            }
        }
        public override int InitMaxHits {
            get {
                return 30;
            }
        }

        [Constructable]
        public BandanaDaluz()
            : this(0)
        {
        }

        [Constructable]
        public BandanaDaluz(int hue)
            : base(0x1540, hue)
        {
            Name = "Bandana Da Luz";
            Hue = 2045;
            LootType = LootType.Blessed;
        }

        public BandanaDaluz(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class RobeAnjoDaluz : Robe
    {
        public override bool IsArtifact { get { return true; } }
        [Constructable]
        public RobeAnjoDaluz()
        {
            Name = "Robe Anjo Da Luz";
            Hue = 2045;
            LootType = LootType.Blessed;
        }

        public RobeAnjoDaluz(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber {
            get {
                return 1095236;
            }
        }// Acid-Proof Robe [Replica]
        public override int BaseFireResistance {
            get {
                return 4;
            }
        }
        public override int InitMinHits {
            get {
                return 150;
            }
        }
        public override int InitMaxHits {
            get {
                return 150;
            }
        }
        public override bool CanFortify {
            get {
                return false;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 1 && this.Hue == 1)
            {
                this.Hue = 0x455;
            }
        }
    }
    // Preto 
    public class AventalDaDasTrevas : BaseWaist
    {
        [Constructable]
        public AventalDaDasTrevas()
            : this(0)
        {
            Name = "Avental Das Trevas";
            Hue = 1;
            LootType = LootType.Blessed;
        }

        [Constructable]
        public AventalDaDasTrevas(int hue)
            : base(0x153b, hue)
        {
            this.Weight = 2.0;
        }

        public AventalDaDasTrevas(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class BandanaDasTrevas : BaseHat
    {
        public override int BasePhysicalResistance {
            get {
                return 0;
            }
        }
        public override int BaseFireResistance {
            get {
                return 3;
            }
        }
        public override int BaseColdResistance {
            get {
                return 5;
            }
        }
        public override int BasePoisonResistance {
            get {
                return 8;
            }
        }
        public override int BaseEnergyResistance {
            get {
                return 8;
            }
        }

        public override int InitMinHits {
            get {
                return 20;
            }
        }
        public override int InitMaxHits {
            get {
                return 30;
            }
        }

        [Constructable]
        public BandanaDasTrevas()
            : this(0)
        {
        }

        [Constructable]
        public BandanaDasTrevas(int hue)
            : base(0x1540, hue)
        {
            Name = "Bandana Das Trevas";
            Hue = 1;
            LootType = LootType.Blessed;
        }

        public BandanaDasTrevas(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class RobeAnjoDasTrevas : Robe
    {
        public override bool IsArtifact { get { return true; } }
        [Constructable]
        public RobeAnjoDasTrevas()
        {
            Name = "Robe Anjo Das Treva";
            Hue = 1;
            LootType = LootType.Blessed;
        }

        public RobeAnjoDasTrevas(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber {
            get {
                return 1095236;
            }
        }// Acid-Proof Robe [Replica]
        public override int BaseFireResistance {
            get {
                return 4;
            }
        }
        public override int InitMinHits {
            get {
                return 150;
            }
        }
        public override int InitMaxHits {
            get {
                return 150;
            }
        }
        public override bool CanFortify {
            get {
                return false;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 1 && this.Hue == 1)
            {
                this.Hue = 0x455;
            }
        }
    }

    public class DragonicRobe : BaseOuterTorso
    {
        public override int LabelNumber { get { return 1157009; } } // Commemorative Robe

        [Constructable]
        public DragonicRobe()
            : base(0x4B9D)
        {
            Name = "Robe Dragonic";
            LootType = LootType.Blessed;
        }

        public DragonicRobe(Serial serial)
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
        }
    }
}
