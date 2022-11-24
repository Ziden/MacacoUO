#if ServUO58
#define ServUOX
#endif

using System.Collections;
using Server;
using Server.Network;
using Server.Gumps;
using System.IO;

namespace Knives.TownHouses
{
    public class GumpResponse
    {
        private static PacketHandler m_Successor;

        public static void Initialize()
        {
            m_Successor = PacketHandlers.GetHandler(0xB1);

            PacketHandlers.Register(0xB1, 0, true, DisplayGumpResponse);

#if !ServUOX
            PacketHandlers.Register6017(0xB1, 0, true, DisplayGumpResponse);
#endif
        }

        public static void DisplayGumpResponse(NetState state, PacketReader pvSrc)
        {
            int serial = pvSrc.ReadInt32();
            int typeID = pvSrc.ReadInt32();
            int button = pvSrc.ReadInt32();

            pvSrc.Seek(-12, SeekOrigin.Current);

            int index = state.Gumps.Count;

            while (--index >= 0)
            {
                if (index >= state.Gumps.Count)
                    continue;

                Gump gump = state.Gumps[index];

                if (gump == null)
                    continue;

                if (gump.Serial == serial && gump.TypeID == typeID)
                {
                    state.Gumps.RemoveAt(index);

                    if (!CheckResponse(gump, state.Mobile, button))
                        return;

                    if (!state.Gumps.Contains(gump))
                        state.Gumps.Insert(index, gump);
                }
            }

            if (m_Successor != null)
                m_Successor.OnReceive(state, pvSrc);
            else
                PacketHandlers.DisplayGumpResponse(state, pvSrc);
        }

        private static bool CheckResponse(Gump gump, Mobile m, int id)
        {
            if (m == null || !m.Player)
                return true;

            TownHouse th = null;

            ArrayList list = new ArrayList();

            foreach (Item item in m.GetItemsInRange(20))
            {
                if (item is TownHouse)
                    list.Add(item);
            }

            foreach (TownHouse t in list)
            {
                if (t.Owner == m)
                {
                    th = t;
                    break;
                }
            }

            if (th == null || th.ForSaleSign == null)
                return true;

#if ServUOX
            if (gump is HouseGump)
#else
            if (gump is HouseGumpAOS)
#endif
            {
                int val = id - 1;

                if (val < 0)
                    return true;

                int type = val % 15;
                int index = val / 15;

                if (th.ForSaleSign.ForcePublic && type == 3 && index == 12 && th.Public)
                {
                    m.SendMessage("This house cannot be private.");
                    m.SendGump(gump);
                    return false;
                }

                if (th.ForSaleSign.ForcePrivate && type == 3 && index == 13 && !th.Public)
                {
                    m.SendMessage("This house cannot be public.");
                    m.SendGump(gump);
                    return false;
                }

                if (th.ForSaleSign.NoTrade && type == 6 && index == 1)
                {
                    m.SendMessage("This house cannot be traded.");
                    m.SendGump(gump);
                    return false;
                }
            }

            return true;
        }
    }
}
