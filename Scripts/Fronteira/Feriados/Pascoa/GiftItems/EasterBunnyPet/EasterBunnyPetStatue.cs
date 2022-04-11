//////////////////////////////////
//			           //
//      Scripted by Raelis      //
//		             	 //
//////////////////////////////////
using System;
using System.Collections;
using Server.Items;
using Server.Mobiles;
using Server.Misc;
using Server.Network;


namespace Server.Items
{
    public class EasterBunnyPetStatue : Item
    {
        public bool m_AllowEvolution;
        public Timer m_EvolutionTimer;
        private DateTime m_End;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AllowEvolution
        {
            get { return m_AllowEvolution; }
            set { m_AllowEvolution = value; }
        }

        [Constructable]
        public EasterBunnyPetStatue() : base(2622)
        {
            Weight = 0.0;
            Name = "estatua do coelinho da pascoa";
            Hue = 1272;
            AllowEvolution = true;

            m_EvolutionTimer = new EvolutionTimer(this, TimeSpan.FromDays(1.0));
            m_EvolutionTimer.Start();
            m_End = DateTime.Now + TimeSpan.FromDays(1.0);
        }

        public EasterBunnyPetStatue(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendMessage("Precisa estar em sua mochila.");
            }
            else if (this.AllowEvolution == true)
            {
                this.Delete();
                from.SendMessage("Voce liberou o coelinho da estatua !!");

                EasterBunnyPet dragon = new EasterBunnyPet();

                dragon.Map = from.Map;
                dragon.Location = from.Location;

                dragon.Controlled = true;

                dragon.ControlMaster = from;

                dragon.IsBonded = true;
            }
            else
            {
                from.SendMessage("Aguarde para libera-lo.");
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
            writer.WriteDeltaTime(m_End);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_End = reader.ReadDeltaTime();
                        m_EvolutionTimer = new EvolutionTimer(this, m_End - DateTime.Now);
                        m_EvolutionTimer.Start();

                        break;
                    }
                case 0:
                    {
                        TimeSpan duration = TimeSpan.FromDays(1.0);

                        m_EvolutionTimer = new EvolutionTimer(this, duration);
                        m_EvolutionTimer.Start();
                        m_End = DateTime.Now + duration;

                        break;
                    }
            }
        }

        private class EvolutionTimer : Timer
        {
            private EasterBunnyPetStatue de;

            private int cnt = 0;

            public EvolutionTimer(EasterBunnyPetStatue owner, TimeSpan duration) : base(duration)
            {
                de = owner;
            }

            protected override void OnTick()
            {
                cnt += 1;

                if (cnt == 1)
                {
                    de.AllowEvolution = true;
                }
            }
        }
    }
}
