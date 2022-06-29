using System;
using Server.Mobiles;

namespace Server.Items
{
    public class MysteriousTunnel : Item
    {
        public override int LabelNumber { get { return 1152265; } } // mysterious tunnel
        private Point3D m_PointDest;
        private Map m_Map;

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D PointDest
        {
            get { return m_PointDest; }
            set { m_PointDest = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Map MapDest
        {
            get { return m_Map; }
            set { m_Map = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Mensagem { get; set; }

        [Constructable]
        public MysteriousTunnel()
            : base(0x1B71)
        {
            this.Movable = false;
            Name = "Tunel Misterioso";
        }

        public MysteriousTunnel(Serial serial)
            : base(serial)
        {
        }

        public override bool OnMoveOver(Mobile m)
        {
            if(base.OnMoveOver(m))
            {
                if(!m.IsCooldown("msgtunel"))
                {
                    m.SetCooldown("msgtunel");
                    m.SendMessage(78, "Clique duas vezes para entrar em um tunel");
                }
                return true;
            }
            return false;
        }

        public override void OnDoubleClick(Mobile m)
        {
            var dist = m.GetDistance(this);
            if (dist < 2)
                Entra(m);
            else
                m.SendMessage(78, "Você deve estar ao lado do túnel para usá-lo.");
        }

        public void Entra(Mobile m)
        {
            if (m is PlayerMobile)
            {
                if (m_Map == null || m_Map == Map.Internal)
                    m_Map = m.Map;

                Point3D loc = PointDest;
                BaseCreature.TeleportPets(m, loc, m_Map);
                m.MoveToWorld(loc, m_Map);
                if (Mensagem == null)
                    m.SendMessage("Voce entrou no tunel");
                else
                    m.SendMessage(Mensagem);
                m.PrivateOverheadMessage("* desceu *");
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1); // version

            writer.Write(this.m_PointDest);
            writer.Write(this.m_Map);
            writer.Write(Mensagem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            this.m_PointDest = reader.ReadPoint3D();
            if(version >= 1)
            {
                this.m_Map = reader.ReadMap();
                Mensagem = reader.ReadString();
            }
        }
    }
}
