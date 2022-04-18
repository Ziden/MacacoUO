using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.TournamentSystem
{
	public class FlagHolder : Item
	{
        public static int FreezeAfterRez = 30; // in seconds

		private ArenaTeam m_Owner;
		private CTFFlag m_HomeFlag;
		private PVPTournamentSystem m_System;
		
		[CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeam Owner { get { return m_Owner; } set { m_Owner = value; InvalidateProperties(); } }
		
		[CommandProperty(AccessLevel.GameMaster)]
		public CTFFlag HomeFlag { get { return m_HomeFlag; } }
		
		[CommandProperty(AccessLevel.GameMaster)]
        public EnemyFlagInfo[] EnemyFlags { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
		public PVPTournamentSystem System { get { return m_System; } set { m_System = value; } }
		
		public override string DefaultName { get { return "Flag Holder"; } }
        public override bool ForceShowProperties { get { return true; } }

        public FlagHolderComponent Component1 { get; set; }
        public FlagHolderComponent Component2 { get; set; }

		public FlagHolder(PVPTournamentSystem system) : base(0x0077)
		{
            EnemyFlags = new EnemyFlagInfo[3];
            m_System = system;
			Movable = false;

            CheckComponents();
        }

        private void CheckComponents()
        {
            if (Component1 == null || Component1.Deleted)
            {
                Component1 = new FlagHolderComponent(108);
                Component1.MoveToWorld(new Point3D(Location.X, Location.Y, Location.Z + 18), Map);
                Component1.Holder = this;
            }

            if (Component2 == null || Component2.Deleted)
            {
                Component2 = new FlagHolderComponent(108);
                Component2.MoveToWorld(new Point3D(Location.X, Location.Y, Location.Z + 22), Map);
                Component2.Holder = this;
            }
        }

        public override void Delete()
        {
            base.Delete();

            if (Component1 != null)
            {
                Component1.Delete();
            }

            if (Component2 != null)
            {
                Component2.Delete();
            }
        }

        public override void OnMapChange()
        {
            base.OnMapChange();

            if (Component1 != null)
                Component1.Map = Map;

            if (Component2 != null)
                Component2.Map = Map;
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);

            if (Component1 != null)
                Component1.Location = new Point3D(X + Component1.X - oldLocation.X, Y + Component1.Y - oldLocation.Y, Z + Component1.Z - oldLocation.Z);

            if (Component2 != null)
                Component2.Location = new Point3D(X + Component2.X - oldLocation.X, Y + Component2.Y - oldLocation.Y, Z + Component2.Z - oldLocation.Z);
        }

		public void Reset()
		{
			m_Owner = null;
			m_HomeFlag = null;

            for (int i = 0; i < EnemyFlags.Length; i++)
            {
                EnemyFlags[i] = null;
            }

            InvalidateProperties();
		}

        public bool IsHomeFlag(CTFFlag flag)
        {
            return flag.Owner == m_Owner;
        }
		
		public bool AddFlag(CTFFlag flag)
		{
			if(flag.Owner != m_Owner)
			{
                for (int i = 0; i < EnemyFlags.Length; i++)
                {
                    if (EnemyFlags[i] == null)
                    {
                        EnemyFlags[i] = new EnemyFlagInfo(flag);

                        switch (i)
                        {
                            case 0:
                                flag.MoveToWorld(new Point3D(X + 1, Y, Z - 1), Map);

                                if (CTFFlag.FlagIDs.Any(id => id == flag.ItemID))
                                {
                                    flag.ItemID++;
                                }
                                break;
                            case 1:
                                flag.MoveToWorld(new Point3D(X, Y + 1, Z + 13), Map);

                                if (CTFFlag.FlagIDs.All(id => id != flag.ItemID))
                                {
                                    flag.ItemID--;
                                }
                                break;
                            case 2:
                                flag.MoveToWorld(new Point3D(X + 1, Y, Z + 13), Map);

                                if (CTFFlag.FlagIDs.Any(id => id == flag.ItemID))
                                {
                                    flag.ItemID++;
                                }
                                break;

                        }

                        Effects.PlaySound(this.Location, this.Map, 0x3D);
                        flag.Holder = this;

                        return true;
                    }
                }
			}
            else if (flag.Owner == m_Owner)
            {
                m_HomeFlag = flag;

                if (CTFFlag.FlagIDs.All(id => id != flag.ItemID))
                {
                    flag.ItemID--;
                }

                Effects.PlaySound(this.Location, this.Map, 0x3D);

                flag.MoveToWorld(new Point3D(X, Y + 1, Z - 1), Map);
                flag.Holder = this;

                return true;
            }

            return false;
		}
		
		public void RemoveFlag(Mobile from, CTFFlag flag)
		{
			if(from.Backpack != null)
			{
                if (flag == m_HomeFlag)
                {
                    m_HomeFlag = null;
                }
                else
                {
                    for (int i = 0; i < EnemyFlags.Length; i++)
                    {
                        if (EnemyFlags[i].Flag == flag)
                        {
                            EnemyFlags[i] = null;
                            break;
                        }
                    }
                }

                from.Backpack.DropItem(flag);
				flag.Holder = null;
			}

            Effects.PlaySound(this.Location, this.Map, 0x059);
		}

        public int GetRoundPoints()
        {
            int points = 0;

            for (int i = 0; i < EnemyFlags.Length; i++)
            {
                var enemyInfo = EnemyFlags[i];

                if (enemyInfo != null && enemyInfo.Placed != DateTime.MinValue && enemyInfo.Placed + TimeSpan.FromMinutes(1) <= DateTime.UtcNow)
                {
                    points++;
                }
            }

            return points;
        }

        public IEnumerable<ArenaTeam> GetEnemyLosers()
        {
            for (int i = 0; i < EnemyFlags.Length; i++)
            {
                if (EnemyFlags[i] != null)
                {
                    yield return EnemyFlags[i].Flag.Owner;
                }
            }
        }

        public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

            if(m_Owner != null)
                list.Add(String.Format("Bandeira por {0}", m_Owner.Name));
		}

        public override bool HandlesOnMovement { get { return m_System != null && m_System.CurrentFight != null; } }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (m_System != null && m_System.CurrentFight != null && !m.Alive && m.InRange(this.Location, 3))
            {
                TeamInfo info = m_System.CurrentFight.GetTeamInfo(m);

                if (m_Owner == null || (info != null && info.Team == m_Owner))
                {
                    m.SendGump(new ResurrectGump(m, null, ResurrectMessage.Generic, false, 0.0, Resurrect_Callback));
                }
            }
        }

        public void Resurrect_Callback(Mobile m)
        {
            ArenaKeeper.AfterResurrection(m);

            m.Hidden = true;
            m.Blessed = true;
            m.Frozen = true;
            m.PrivateOverheadMessage(Server.Network.MessageType.Regular, m.SpeechHue, false, String.Format("You are frozen for {0} more seconds!", FreezeAfterRez.ToString()), m.NetState);

            Timer.DelayCall(TimeSpan.FromSeconds(FreezeAfterRez), Unfreeze_Callback, m);
        }

        public void Unfreeze_Callback(Mobile m)
        {
            if (m != null)
            {
                m.Blessed = false;
                m.Frozen = false;
                m.PrivateOverheadMessage(Server.Network.MessageType.Regular, m.SpeechHue, false, "You can now move!", m.NetState);
            }
        }

        public void SetHue(int hue)
        {
            Hue = hue;

            CheckComponents();

            Component1.Hue = hue;
            Component2.Hue = hue;
        }
		
		public FlagHolder(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)1);

            writer.WriteItem(Component1);
            writer.WriteItem(Component2);
        }
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			var version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    Component1 = reader.ReadItem<FlagHolderComponent>();
                    Component2 = reader.ReadItem<FlagHolderComponent>();

                    if (Component1 != null)
                    {
                        Component1.Holder = this;
                    }

                    if (Component2 != null)
                    {
                        Component2.Holder = this;
                    }
                    break;
            }

            CheckComponents();

            SetHue(0);
		}
	}

    public class FlagHolderComponent : Static
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public FlagHolder Holder { get; set; }

        public override string DefaultName { get { return "Flag Holder"; } }
        public override bool ForceShowProperties { get { return true; } }

        public FlagHolderComponent(int id)
            : base(id)
        {
        }

        public FlagHolderComponent(Serial serial) : base(serial)
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
            reader.ReadInt();
        }
    }

    public class EnemyFlagInfo
    {
        public CTFFlag Flag { get; set; }
        public DateTime Placed { get; set; }

        public EnemyFlagInfo(CTFFlag flag)
        {
            Flag = flag;
            Placed = DateTime.UtcNow;
        }
    }
}
