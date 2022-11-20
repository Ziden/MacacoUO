
using System;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Regions;

namespace Server.Items
{
    public class Hearthstone : Item
    {
        // Initiate the starting variables to be used
        private DateTime i_lastused;
        private Point3D i_home = CharacterCreation.HAVEN;
        private Map i_homemap = Map.Trammel;
        private static readonly TimeSpan delay = TimeSpan.FromMinutes(20);
        private Timer hearth;
        private Mobile m;
        private Item i;

        // Setup the GM access/viewable variables for the item
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime lastused
        {
            get { return i_lastused; }
            set { i_lastused = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D home
        {
            get { return i_home; }
            set { i_home = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Map map
        {
            get { return i_homemap; }
            set { i_homemap = value; }
        }

        // starting the item itself
        [Constructable]
        public Hearthstone() : base(0x3197)
        {
            Weight = 1.0;
            Name = "hearthstone";
        }

        // this controls the list you see when you mouse-over the stone
        public override void GetProperties(ObjectPropertyList list)
        {
            // make sure the normal mouse-over props show up
            base.GetProperties(list);

            // initial variables for use only inside the property list
            TimeSpan timetouse = ((this.lastused + Server.Items.Hearthstone.delay) - DateTime.Now);
            string lisths;

 
            if(this.map != null && this.i_home != Point3D.Zero)
            {
                var region = Region.Find(this.i_home, this.map);
                if(region != null && region.Name != null)
                {
                    list.Add("Salva em " + region.Name);
                }
            }

            // determine the info the next-use display shows
            if (timetouse.Minutes > 0)
            {
                int min = timetouse.Minutes;
                lisths = String.Format("{0} minutos.", min.ToString());
            }
            else if (timetouse.Seconds > 0)
            {
                int sec = timetouse.Seconds;
                lisths = String.Format("{0} segundos.", sec.ToString());
            }
            else
            {
                lisths = ("agora");
            }

            // adding the next-use to the property list
            list.Add("Usavel em: {0}", lisths);

            // because we do not want the server spamming updates, slow down how fast the mouse-over info refreshes
            // Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerCallback(InvalidateProperties));
        }

        public override void OnDoubleClick(Mobile from)
        {
            this.InvalidateProperties();
            i = this;
            m = from;

            if (!IsChildOf(from.Backpack)) // initiate the checks for the double-click to restrict use
            {
                from.SendMessage("Precisa estar em sua mochila.");
                return;
            }

            if(Shard.RP)
            {
                if(from.Region is DungeonRegion)
                {
                    from.SendMessage("Voce nao pode usar isto em dungeons");
                    return;
                }
            }

            from.RevealingAction();
            from.SendGump(new GumpOpcoes("Hearthstone", (opt) =>
            {
                if(opt==0)
                {
                    if (!CheckHearth(m, i)) // execute common method for failure checks
                    {
                        return;
                    }
                    m.Freeze(TimeSpan.FromSeconds(6));
                    hearth = Timer.DelayCall(TimeSpan.FromSeconds(6), new TimerCallback(HearthTeleport));
                    m.SendMessage("Voce esta retornando para casa em 6 segundos.");
                    m.FixedEffect(0x375A, 10, 15);
                    m.PlaySound(0x1E7);
                    m.OverheadMessage("* retornando *");
                } else
                {
                    if (from.Region is GuardedRegion || from.Region is NoGuardCity || from.Region is NoHousingRegion)  // see if they are in an "inn" zone and mark if they are
                    {
                        from.FixedEffect(0x375A, 10, 15);
                        from.PlaySound(0x1E7);
                        this.home = from.Location;
                        this.map = from.Map;
                        from.OverheadMessage("* salvou *");
                        this.lastused = DateTime.Now - TimeSpan.FromMinutes(19);
                        return;
                    } else
                    {
                        from.SendMessage("Voce precisa estar em algum local seguro para salvar sua pedra");
                    }
                }
            }, 0x3197, 0, "Retornar", "Salvar"));
        }

        private void HearthTeleport()  // the teleporting method
        {
            if (CheckHearth(m, i))  // double check after "casting time" that its legal still
            {
                BaseCreature.TeleportPets(m, home, map);
                m.MoveToWorld(home, map);
              
                m.FixedEffect(0x375A, 10, 15);
                m.PlaySound(0x1E7);
                this.lastused = DateTime.Now;
                m.CantWalk = false;
            }
            else  // or else fail the check and deny successful use
            {
                m.CantWalk = false;
                m.SendMessage("Sua pedra nao conseguiu te teleportar !");
                return;
            }
        }

        private bool CheckHearth(Mobile m, Item i)  // the common checks to ensure valid use
        {
            if (m.Criminal)  // make sure they are not criminal
            {
                m.SendMessage("Voce e criminoso!");
                return false;
            }
            else if ((this.lastused + Server.Items.Hearthstone.delay) > DateTime.Now) // make sure its not too soon since last use
            {
                var diff = (this.lastused + Server.Items.Hearthstone.delay) - DateTime.Now;

                m.SendMessage("Aguarde "+diff.TotalSeconds+" segundos");
                return false;
            }
            else if (Server.Spells.SpellHelper.CheckCombat(m)) // use a spell system check to make sure they are not in combat
            {
                m.SendMessage("Voce esta em combate!");
                return false;
            }
            else  // they are eligible to teleport
            {
                return true;
            }

        }

        public Hearthstone(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer) // generic info to write and save
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write((Point3D)i_home);
            writer.Write((DateTime)i_lastused);
            writer.Write((Map)i_homemap);
        }

        public override void Deserialize(GenericReader reader) // make sure proper variables are read
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            i_home = reader.ReadPoint3D();
            i_lastused = reader.ReadDateTime();
            i_homemap = reader.ReadMap();
        }
    }
}
