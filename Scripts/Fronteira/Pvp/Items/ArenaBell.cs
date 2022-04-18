using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;
using Server.Gumps;
using Server.TournamentSystem;

namespace Server.Items
{
    public class ArenaBell : Item
    {
        public Dictionary<Mobile, DateTime> DelayTable { get; set; }

        public static readonly TimeSpan Delay = TimeSpan.FromMinutes(2);

        [CommandProperty(AccessLevel.GameMaster)]
        public PVPTournamentSystem System { get; set; }

        public override bool ForceShowProperties { get { return true; } }

        public ArenaBell(PVPTournamentSystem sys, int id)
            : base(id)
        {
            System = sys;

            Movable = false;
            Name = "Sino da Arena";
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add("2 Clicks para convidar para um duelo.");
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (System == null)
                return;

            if (m.InRange(GetWorldLocation(), 2))
            {
                if (HasDelay(m))
                {
                    m.SendMessage("Aguarde um pouco.");
                }
                else if(System != null)
                {
                    BaseGump.SendGump(new BroadcastGump((PlayerMobile)m,
                        (mob, amount) =>
                        {
                            if (amount > 0)
                            {
                                World.Broadcast(0x21, true, String.Format("{0} oferecendo {1} por um duelo em {2}!", mob.Name, amount.ToString(), System.Name));
                            }
                            else
                            {
                                World.Broadcast(0x21, true, String.Format("{0} procurando oponentes na {1}!", mob.Name, System.Name));
                            }

                            AddDelay(mob);
                        }));
                }
            }
            else
            {
                m.SendLocalizedMessage(500295); // You are too far away to do that.
            }
        }

        public bool HasDelay(Mobile m)
        {
            return DelayTable != null && DelayTable.ContainsKey(m);
        }

        public void AddDelay(Mobile m)
        {
            if (DelayTable == null)
            {
                DelayTable = new Dictionary<Mobile, DateTime>();
            }

            DelayTable[m] = DateTime.UtcNow + Delay;

            Timer.DelayCall(Delay, mob => RemoveDelay(mob), m);
        }

        public void RemoveDelay(Mobile m)
        {
            if (DelayTable != null && DelayTable.ContainsKey(m))
            {
                DelayTable.Remove(m);

                if(DelayTable.Count == 0)
                {
                    DelayTable.Remove(m);
                }
            }
        }

        public ArenaBell(Serial serial)
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

    public class BroadcastGump : BaseGump
    {
        public Action<Mobile, int> Callback { get; set; }

        public BroadcastGump(PlayerMobile pm, Action<Mobile, int> callback)
            : base(pm, 200, 200)
        {
            Callback = callback;
        }

        public override void AddGumpLayout()
        {
            AddBackground(0, 0, 300, 100, 311);
            AddHtml(10, 15, 280, 40, Center("Adicione uma aposta no campo ou deixe em branco para nao apostar."), false, false);

            AddButton(15, 68, 4005, 4006, 1, GumpButtonType.Reply, 0);
            AddTooltip("Anunciar");

            AddImageTiled(125, 68, 50, 20, 2702);
            AddAlphaRegion(125, 68, 50, 20);
            AddTextEntry(126, 68, 49, 20, 1153, 0, "");
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID == 1 && Callback != null)
            {
                TextRelay relay = info.GetTextEntry(0);

                if (relay != null && !String.IsNullOrEmpty(relay.Text))
                {
                    Callback(User, Utility.ToInt32(relay.Text));
                }
                else
                {
                    Callback(User, -1);
                }

                return;
            }

            User.SendMessage("Anuncio Cancelado.");
        }
    }
}
