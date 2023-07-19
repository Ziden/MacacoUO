#region References
using System;

using Server.Factions;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Spells;
#endregion

namespace Server.Items
{
    [DispellableField]
    public class Moongate : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Target { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Map TargetMap { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Dispellable { get; set; }

        public virtual bool ShowFeluccaWarning { get { return false; } }

        public virtual bool TeleportPets { get { return true; } }

        [Constructable]
        public Moongate()
            : this(Point3D.Zero, null)
        {
            Dispellable = true;
        }

        [Constructable]
        public Moongate(bool bDispellable)
            : this(Point3D.Zero, null)
        {
            Dispellable = bDispellable;
        }

        [Constructable]
        public Moongate(Point3D target, Map targetMap)
            : base(0xF6C)
        {
            Movable = false;
            Light = LightType.Circle300;

            Target = target;
            TargetMap = targetMap;
        }

        public Moongate(Serial serial)
            : base(serial)
        { }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.Player)
                return;

            if (from.InRange(GetWorldLocation(), 1))
                CheckGate(from, 1);
            else
                from.SendLocalizedMessage(500446); // That is too far away.
        }

        public override bool OnMoveOver(Mobile m)
        {
            if (!this.Visible)
                return true;

            if (m.Player)
                CheckGate(m, 0);

            return true;
        }

        public override bool OnMoveOff(Mobile m)
        {
            if (m.HasGump(typeof(MoongateConfirmGump)))
                m.CloseGump(typeof(MoongateConfirmGump));
            return base.OnMoveOff(m);
        }

        public virtual void CheckGate(Mobile m, int range)
        {
            new DelayTimer(m, this, range).Start();
        }

        public virtual void OnGateUsed(Mobile m)
        {
            if (TargetMap == null || TargetMap == Map.Internal)
                return;

            if (TeleportPets)
                BaseCreature.TeleportPets(m, Target, TargetMap);

            m.MoveToWorld(Target, TargetMap);
            m.SendMessage("Voce sente seu corpo atravessando o portal frio");

            if (m.IsPlayer() || !m.Hidden)
                m.PlaySound(0x1FE);
        }

        public virtual void UseGate(Mobile m)
        {
            if (!this.Visible)
                return;

            var flags = m.NetState == null ? ClientFlags.None : m.NetState.Flags;

            if (Sigil.ExistsOn(m))
            {
                m.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
            }
            else if ((SpellHelper.RestrictRedTravel && m.Murderer && TargetMap != Map.Felucca && !Siege.SiegeShard) ||
                     (TargetMap == Map.Tokuno && (flags & ClientFlags.Tokuno) == 0) ||
                     (TargetMap == Map.Malas && (flags & ClientFlags.Malas) == 0) ||
                     (TargetMap == Map.Ilshenar && (flags & ClientFlags.Ilshenar) == 0))
            {
                m.SendLocalizedMessage(1019004); // You are not allowed to travel there.
            }
            else if (m.Spell != null || BaseBoat.IsDriving(m))
            {
                m.SendLocalizedMessage(1049616); // You are too busy to do that at the moment.
            }
            else if (m.Holding != null)
            {
                m.SendLocalizedMessage(1071955); // You cannot teleport while dragging an object.
            }
            else if (TargetMap != null && TargetMap != Map.Internal)
            {
                OnGateUsed(m);
            }
            else
            {
                m.SendMessage("This moongate does not seem to go anywhere.");
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1); // version

            writer.Write(Target);
            writer.Write(TargetMap);

            // Version 1
            writer.Write(Dispellable);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();

            Target = reader.ReadPoint3D();
            TargetMap = reader.ReadMap();

            if (version >= 1)
                Dispellable = reader.ReadBool();
        }

        public virtual bool ValidateUse(Mobile from, bool message)
        {
            if (from.Deleted || Deleted)
                return false;

            if (from.Map != Map || !from.InRange(this, 1))
            {
                if (message)
                    from.SendLocalizedMessage(500446); // That is too far away.

                return false;
            }

            return true;
        }

        public virtual void BeginConfirmation(Mobile from)
        {
            if (from.IsPlayer() || !from.Hidden)
                from.Send(new PlaySound(0x102, from.Location));
            from.CloseGump(typeof(MoongateConfirmGump));
            from.SendGump(new MoongateConfirmGump(from, this));
            /*
			if (IsInTown(from.Location, from.Map) && !IsInTown(Target, TargetMap) ||
				(from.Map != Map.Avalon && TargetMap == Map.Avalon && ShowFeluccaWarning))
			{
				if (from.IsPlayer() || !from.Hidden)
					from.Send(new PlaySound(0x20E, from.Location));
				from.CloseGump(typeof(MoongateConfirmGump));
				from.SendGump(new MoongateConfirmGump(from, this));
			}
			else
			{
				EndConfirmation(from);
			}
            */
        }

        public virtual void EndConfirmation(Mobile from)
        {
            if (!ValidateUse(from, true))
                return;

            UseGate(from);
        }

        public virtual void DelayCallback(Mobile from, int range)
        {
            if (!ValidateUse(from, false) || !from.InRange(this, range))
                return;

            if (TargetMap != null)
                BeginConfirmation(from);
            else
                from.SendMessage("This moongate does not seem to go anywhere.");
        }

        public static bool IsInTown(Point3D p, Map map)
        {
            if (map == null)
                return false;

            var reg = (GuardedRegion)Region.Find(p, map).GetRegion(typeof(GuardedRegion));

            return (reg != null && !reg.IsDisabled());
        }

        private class DelayTimer : Timer
        {
            private readonly Mobile m_From;
            private readonly Moongate m_Gate;
            private readonly int m_Range;

            public DelayTimer(Mobile from, Moongate gate, int range)
                : base(TimeSpan.FromSeconds(0.0))
            {
                m_From = from;
                m_Gate = gate;
                m_Range = range;
            }

            protected override void OnTick()
            {
                m_Gate.DelayCallback(m_From, m_Range);
            }
        }
    }

    public class ConfirmationMoongate : Moongate
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int GumpWidth { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GumpHeight { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TitleColor { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MessageColor { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TitleNumber { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string TitleString { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MessageNumber { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string MessageString { get; set; }

        [Constructable]
        public ConfirmationMoongate()
            : this(Point3D.Zero, null)
        { }

        [Constructable]
        public ConfirmationMoongate(Point3D target, Map targetMap)
            : base(target, targetMap)
        { }

        public ConfirmationMoongate(Serial serial)
            : base(serial)
        { }

        public virtual void Warning_Callback(Mobile from, bool okay, object state)
        {
            if (okay)
                EndConfirmation(from);
        }

        public override void BeginConfirmation(Mobile from)
        {
            if (GumpWidth > 0 && GumpHeight > 0 && (TitleNumber > 0 || TitleString != null) &&
                (MessageNumber > 0 || MessageString != null))
            {
                from.CloseGump(typeof(WarningGump));

                from.SendGump(
                    new WarningGump(
                        new TextDefinition(TitleNumber, TitleString),
                        TitleColor,
                        new TextDefinition(MessageNumber, MessageString),
                        MessageColor,
                        GumpWidth,
                        GumpHeight,
                        Warning_Callback,
                        from));
            }
            else
            {
                base.BeginConfirmation(from);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1); // version

            writer.Write(TitleString);

            writer.WriteEncodedInt(GumpWidth);
            writer.WriteEncodedInt(GumpHeight);

            writer.WriteEncodedInt(TitleColor);
            writer.WriteEncodedInt(MessageColor);

            writer.WriteEncodedInt(TitleNumber);
            writer.WriteEncodedInt(MessageNumber);

            writer.Write(MessageString);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        TitleString = reader.ReadString();
                        goto case 0;
                    }
                case 0:
                    {
                        GumpWidth = reader.ReadEncodedInt();
                        GumpHeight = reader.ReadEncodedInt();

                        TitleColor = reader.ReadEncodedInt();
                        MessageColor = reader.ReadEncodedInt();

                        TitleNumber = reader.ReadEncodedInt();
                        MessageNumber = reader.ReadEncodedInt();

                        MessageString = reader.ReadString();

                        break;
                    }
            }
        }
    }

    public class MoongateConfirmGump : Gump
    {
        private readonly Mobile m_From;
        private readonly Moongate m_Gate;

        public MoongateConfirmGump(Mobile from, Moongate gate)
            : base(110,100)
        {
            m_From = from;
            m_Gate = gate;

            AddPage(0);
            AddBackground(20, 32, 190, 90, 3500);
            AddHtml(40, 44, 200, 64, @"Deseja entrar no portal?", (bool)false, (bool)false);
            AddButton(140, 80, 30534, 30534, 1, GumpButtonType.Reply, 0); // ok
            AddButton(60, 80, 30535, 30535, 0, GumpButtonType.Reply, 0); //cancelar

        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (info.ButtonID == 1)
                m_Gate.EndConfirmation(m_From);
        }
    }
}
