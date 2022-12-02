using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Ziden
{
    public class Titulos
    {
        public static void Initialize()
        {
            CommandSystem.Register("titulo", AccessLevel.Player, OnAction);
        }

        [Usage("Action")]
        private static void OnAction(CommandEventArgs e)
        {
            e.Mobile.CloseGump(typeof(TitlesGump));
            e.Mobile.SendGump(new TitlesGump((PlayerMobile)e.Mobile));
        }

    }


    public class LichKiller : BaseRewardTitleDeed
    {
        public override TextDefinition Title { get { return new TextDefinition("Matador de Liches"); } } // Naughty

        [Constructable]
        public LichKiller()
        {
        }

        public LichKiller(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class TitulosDejabu : BaseRewardTitleDeed
    {
        public override TextDefinition Title { get { return new TextDefinition("Matador de Dejabu"); } } // Naughty

        [Constructable]
        public TitulosDejabu()
        {
        }

        public TitulosDejabu(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class TitulosToppvm : BaseRewardTitleDeed
    {
        public override TextDefinition Title { get { return new TextDefinition("Campeão Pvm 2022"); } } // Naughty

        [Constructable]
        public TitulosToppvm()
        {
        }

        public TitulosToppvm(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class TitulosTopPvP : BaseRewardTitleDeed
    {
        public override TextDefinition Title { get { return new TextDefinition("Campeão PvP 2022"); } } // Naughty

        [Constructable]
        public TitulosTopPvP()
        {
        }

        public TitulosTopPvP(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class TitulosTopTrabalhador : BaseRewardTitleDeed
    {
        public override TextDefinition Title { get { return new TextDefinition("Campeão Woker 2022"); } } // Naughty

        [Constructable]
        public TitulosTopTrabalhador()
        {
        }

        public TitulosTopTrabalhador(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class TitulosLuciferKiller : BaseRewardTitleDeed
    {
        public override TextDefinition Title { get { return new TextDefinition("Matador de Lucifer"); } } // Naughty

        [Constructable]
        public TitulosLuciferKiller()
        {
        }

        public TitulosLuciferKiller(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class Titulospinga : BaseRewardTitleDeed
    {
        public override TextDefinition Title { get { return new TextDefinition("O  Pau-d'água"); } } // Naughty

        [Constructable]
        public Titulospinga()
        {
        }

        public Titulospinga(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class BetaTester : BaseRewardTitleDeed
    {
        public override TextDefinition Title { get { return new TextDefinition("Beta Tester"); } } // Naughty

        [Constructable]
        public BetaTester()
        {
        }

        public BetaTester(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class DeedDeTitulo : BaseRewardTitleDeed
    {
        public override TextDefinition Title { get { return new TextDefinition(Titulo); } } // Naughty

        [CommandProperty(AccessLevel.GameMaster)]
        public string Titulo { get; set; }

        [Constructable]
        public DeedDeTitulo()
        {
        }

        public DeedDeTitulo(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(Titulo);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var v = reader.ReadInt();
            Titulo = reader.ReadString();
        }
    }
}
