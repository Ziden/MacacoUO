using Server.Commands;
using Server.Mobiles;

namespace Server.Items
{
    public class GamblingStone : Item
    {
        private int m_GamblePot = 2500;
        [Constructable]
        public GamblingStone()
            : base(0xED4)
        {
            Movable = false;
            Hue = 0x56;
        }

        public GamblingStone(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GamblePot
        {
            get
            {
                return m_GamblePot;
            }
            set
            {
                m_GamblePot = value;
                InvalidateProperties();
            }
        }
        public override string DefaultName
        {
            get
            {
                return "Pedra Mega Sena";
            }
        }
        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add("Acumulado: {0} moedas", m_GamblePot);
            list.Add("Apostas: Apenas 250 moedas de ouro");
            list.Add("Chance de ganhar: 1 em 1200");
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);
            base.LabelTo(from, "Acumulado: {0} moedas", m_GamblePot);
        }

        public override void OnDoubleClick(Mobile from)
        {
            Container pack = from.Backpack;

            if (pack != null && pack.ConsumeTotal(typeof(Gold), 250))
            {
                m_GamblePot += 150;
                InvalidateProperties();

                int roll = Utility.Random(1200);
                from.SendMessage(79, $"Sorte: {1200 - roll}/1200");

                if (roll == 0) // Jackpot
                {
                    int maxCheck = 1000000;

                    from.SendMessage(0x35, "Voce ganhou {0} moedas !", m_GamblePot);

                    Anuncio.Anuncia(from.Name + " ganhou " + m_GamblePot + " moedas na pedra de aposta !");

                    while (m_GamblePot > maxCheck)
                    {

                        Banker.Deposit(from, maxCheck, true);

                        m_GamblePot -= maxCheck;
                    }

                    Banker.Deposit(from, m_GamblePot, true);

                    m_GamblePot = 2500;

                }
                else if (roll <= 20) // Chance for a regbag
                {
                    from.SendMessage(0x35, "Voce ganhou uma sacola de reagentes!");
                    from.AddToBackpack(new BagOfReagents(50));
                }
                else if (roll <= 40) // Chance for gold
                {
                    from.SendMessage(0x35, "Voce ganhou bandagens!");

                    from.AddToBackpack(new Bandage(100));
                }
                else if (roll <= 100) // Another chance for gold
                {
                    from.SendMessage(0x35, "Voce ganhou 1000 moedas!");
                    Banker.Deposit(from, 1000, true);
                }
                else // Loser!
                {
                    from.SendMessage(0x22, $"Voce perdeu!");
                }
            }
            else
            {
                from.SendMessage(0x22, "Voce precisa de pelo menos 250 moedas em sua mochila");
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)m_GamblePot);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_GamblePot = reader.ReadInt();

                        break;
                    }
            }
        }
    }
}
