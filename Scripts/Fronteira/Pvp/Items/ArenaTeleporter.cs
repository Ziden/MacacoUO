using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;
using Server.Gumps;
using Server.TournamentSystem;
using Server.Multis;
using Server.ContextMenus;
using Server.Spells;

namespace Server.Items
{
    public class ArenaTransportCrystal : Item, ISecurable
    {
        private SecureLevel m_SecureLevel;
        private bool m_Public;

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level
        {
            get { return m_SecureLevel; }
            set { m_SecureLevel = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Public
        {
            get { return m_Public; }
            set { m_Public = value; }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            SetSecureLevelEntry.AddTo(from, this, list);
        }

        [Constructable]
        public ArenaTransportCrystal()
            : this(false)
        {
        }

        [Constructable]
        public ArenaTransportCrystal(bool pub)
            : base(7961)
        {
            Public = pub;

            if (Public)
            {
                Movable = false;
            }

            Name = "Cristal de Transporte da Arena";
            LootType = LootType.Blessed;
        }

        public override bool ForceShowProperties { get { return true; } }

        public override void OnDoubleClick(Mobile from)
        {
            if ((IsLockedDown || IsSecure || Public) && from.InRange(GetWorldLocation(), 2))
            {
                BaseGump.SendGump(new InternalGump(from as PlayerMobile, this));
            }
            else if (!from.InRange(GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(500295); // You are too far away to do that.
            }
            else
            {
                from.SendLocalizedMessage(502692); // This must be in a house and be locked down to work.
            }
        }

        private class InternalGump : BaseGump
        {
            public Item Crystal { get; set; }

            public InternalGump(PlayerMobile pm, Item crystal)
                : base(pm, 75, 75)
            {
                Crystal = crystal;
            }

            public override void AddGumpLayout()
            {
                AddBackground(0, 0, 400, 500, 9270);

                AddHtmlLocalized(0, 15, 400, 16, 1154645, "#1156704", 0xFFFF, false, false); // Select your destination:

                for (int i = 0; i < PVPTournamentSystem.SystemList.Count; i++)
                {
                    AddHtml(60, 45 + (i * 25), 250, 16, Color("#FFFFFF", PVPTournamentSystem.SystemList[i].Name), false, false);
                    AddButton(20, 50 + (i * 25), 2117, 2118, i + 100, GumpButtonType.Reply, 0);
                }
            }

            public override void OnResponse(RelayInfo info)
            {
                if (info.ButtonID > 0)
                {
                    int id = info.ButtonID - 100;

                    if (id >= 0 && id < PVPTournamentSystem.SystemList.Count)
                    {
                        var system = PVPTournamentSystem.SystemList[id];
                        var p = system.GetRandomKickLocation();
                        var map = system.ArenaMap;

                        if (CheckTravel(p))
                        {
                            Effects.PlaySound(User.Location, User.Map, 0x1FE);

                            BaseCreature.TeleportPets(User, p, map);
                            User.Combatant = null;
                            User.Warmode = false;
                            User.Hidden = true;

                            User.MoveToWorld(p, map);

                            Effects.PlaySound(p, map, 0x1FE);
                        }
                    }
                }
            }

            private bool CheckTravel(Point3D p)
            {
                if (!User.InRange(Crystal.GetWorldLocation(), 2) || User.Map != Crystal.Map)
                {
                    User.SendLocalizedMessage(500295); // You are too far away to do that.
                }
                else if (SpellHelper.RestrictRedTravel && User.Murderer)
                {
                    User.SendLocalizedMessage(1019004); // You are not allowed to travel there.
                }
                else if (User.Criminal)
                {
                    User.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
                }
                else if (Server.Spells.SpellHelper.CheckCombat(User))
                {
                    User.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
                }
                else if (User.Spell != null)
                {
                    User.SendLocalizedMessage(1049616); // You are too busy to do that at the moment.
                }
                else if (User.Map == Map.Ilshenar && User.InRange(p, 1))
                {
                    User.SendLocalizedMessage(1019003); // You are already there.
                }
                else
                    return true;

                return false;
            }
        }

        public ArenaTransportCrystal(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(m_Public);
            writer.Write((int)m_SecureLevel);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_Public = reader.ReadBool();
            m_SecureLevel = (SecureLevel)reader.ReadInt();
        }
    }
}
