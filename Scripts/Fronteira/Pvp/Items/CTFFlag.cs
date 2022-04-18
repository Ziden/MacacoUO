using System;
using System.Linq;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.TournamentSystem
{
    public class CTFFlag : Item
    {
        private CaptureTheFlagFight m_Fight;
        private ArenaTeam m_Owner;
        private FlagHolder m_Holder;
        private FlagHolder m_LastHolder;
        private DateTime m_LastTaken;

        [CommandProperty(AccessLevel.GameMaster)]
        public CaptureTheFlagFight Fight { get { return m_Fight; } set { m_Fight = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeam Owner { get { return m_Owner; } set { m_Owner = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public FlagHolder Holder
        {
            get { return m_Holder; }
            set
            {
                if (m_Holder != null && value == null)
                    m_LastHolder = m_Holder;

                m_Holder = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public FlagHolder LastHolder { get { return m_LastHolder; } set { m_LastHolder = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastTaken { get { return m_LastTaken; } set { m_LastTaken = value; } }

        public virtual int PlayerHue { get { return this.Hue; } }

        public override string DefaultName 
        {
            get
            {
                if(m_Holder != null && m_Holder.Owner != m_Owner)
                    return "Bandeira (Capturada)";

                return "Bandeira";
            } 
        }

        public override bool ForceShowProperties { get { return true; } }

        public bool Home { get { return m_Holder != null && m_Holder.IsHomeFlag(this); } }

        public CTFFlag(CaptureTheFlagFight fight, ArenaTeam owner, int id, int hue)
            : base(id)
        {
            Owner = owner;
            m_Fight = fight;
            Movable = false;
            Hue = hue;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if(m_Owner != null)
                list.Add(String.Format("Flag for {0}", m_Owner.Name));
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.Target = new InternalTarget(this);

                from.SendMessage("Where or who would you like to pass the flag off to?");
            }
            else if (RootParent == null)
            {
                if (from.InRange(this.Location, 1))
                {
                    from.RevealingAction();

                    if (from.Backpack == null || m_Fight == null || m_Fight.System == null || m_Fight.System.FightRegion == null || from.Region != m_Fight.System.FightRegion)
                        return;

                    if (from.Backpack.FindItemByType(typeof(CTFFlag)) != null)
                    {
                        from.SendMessage("You can only hold one flag at a time!");
                    }
                    else if (m_Holder != null)
                    {
                        TeamInfo team = m_Fight.GetTeamInfo(from);

                        if (team != null)
                        {
                            if (m_Holder.Owner == team.Team || (FlagBelongsTo(team.Team) && m_Owner == m_Holder.Owner))
                            {
                                from.SendMessage("You cannot take a flag while its in your base!");
                            }
                            else
                            {
                                ShowFlagEffect();

                                m_LastTaken = DateTime.UtcNow;
                                m_Holder.RemoveFlag(from, this);
                                OnFlagTaken(from);
                            }
                        }
                    }
                    else
                    {
                        m_LastTaken = DateTime.UtcNow;

                        from.Backpack.DropItem(this);
                        m_Holder = null;
                        OnFlagTaken(from);
                    }
                }
                else
                {
                    from.SendMessage("You must be next to the flag to take it!");
                }
            }
        }

        public void ShowFlagEffect()
        {
            Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x376A, 1, 22, Hue, 7, 9502, 0);
        }

        public override void OnParentDeleted(object Parent)
        {
            ReturnToHome();
        }

        public void OnFlagTaken(Mobile from)
        {
            if (from == null || m_Fight == null)
                return;

            var fromTeam = m_Fight.GetTeam(from);

            foreach (var info in m_Fight.Teams)
            {
                var team = info.Team;

                foreach (var fighter in team.Fighters)
                {
                    if (fighter != from)
                    {
                        if (m_Owner == null)
                        {
                            fighter.SendMessage(353, "{0} [{1}] has taken the flag!", from.Name, fromTeam != null ? fromTeam.Name : "Unknown");
                        }
                        else if (FlagBelongsTo(fromTeam))
                        {
                            fighter.SendMessage(353, "{0} [{1}] has retaken {2} teams flag!", from.Name, fromTeam != null ? fromTeam.Name : "Unknown", from.Female ? "her" : "his");
                        }
                        else if (FlagBelongsTo(team))
                        {
                            fighter.SendMessage(353, "{0} [{1}] has captured your flag!", from.Name, fromTeam != null ? fromTeam.Name : "Unknown");
                        }
                        else
                        {
                            fighter.SendMessage(353, "{0} [{1}] has captured {2} enemies flag!", from.Name, fromTeam != null ? fromTeam.Name : "Unknown", from.Female ? "her" : "his");
                        }
                    }
                    else
                    {
                        fighter.SendMessage(353, "You have {0} flag!", FlagBelongsTo(team) ? "retaken your own" : "taken your enemies");
                    }
                }
            }
        }

        public void ReturnToHome()
        {
            if (m_Fight != null)
            {
                var system = m_Fight.System as CTFArena;

                if (system != null)
                {
                    var holder = system.FlagHolders.FirstOrDefault(h => h.IsHomeFlag(this));

                    if (holder != null)
                    {
                        holder.AddFlag(this);
                    }
                }
            }
        }

        public void OnFlagPlaced(Mobile from)
        {
            if (from == null || m_Fight == null)
                return;

            var fromTeam = m_Fight.GetTeam(from);

            foreach (var team in m_Fight.Teams.Select(x => x.Team))
            {
                foreach (var fighter in team.Fighters)
                {
                    if (fighter != from)
                    {
                        if (m_Owner == null)
                        {
                            fighter.SendMessage(353, "{0} [{1}] has placed the flag on the holder!", from.Name, fromTeam != null ? fromTeam.Name : "Unknown");
                        }
                        else if (FlagBelongsTo(team))
                        {
                            fighter.SendMessage(353, "{0} [{1}] has placed your flag on {2} holder!", from.Name, fromTeam != null ? fromTeam.Name : "Unknown", from.Female ? "her" : "his");
                        }
                        else if (FlagBelongsTo(fromTeam))
                        {
                            fighter.SendMessage(353, "{0} [{1}] has placed their flag back on {2} holder!", from.Name, fromTeam != null ? fromTeam.Name : "Unknown", from.Female ? "her" : "his");
                        }
                        else
                        {
                            fighter.SendMessage(353, "{0} [{1}] has placed their enemies flag on {2} holder!", from.Name, fromTeam != null ? fromTeam.Name : "Unknown", from.Female ? "her" : "his");
                        }
                    }
                    else
                    {
                        fighter.SendMessage(353, "You place the flag on the holder!");
                    }
                }
            }
        }

        public bool FlagBelongsTo(ArenaTeam team)
        {
            if (m_Owner == null)
            {
                return false;
            }

            return m_Owner == team;
        }

        public void ReturnToLast()
        {
            if (m_LastHolder != null)
            {
                Mobile m = RootParent as Mobile;

                if (m != null)
                    m.SendMessage("You have taken too long! The flag has been returned to its last holder.");

                m_LastHolder.AddFlag(this);
            }
        }

        private class InternalTarget : Target
        {
            private CTFFlag m_Flag;

            public InternalTarget(CTFFlag flag) : base(2, false, TargetFlags.None)
            {
                m_Flag = flag;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Flag == null || m_Flag.Deleted || !m_Flag.IsChildOf(from.Backpack) || m_Flag.Fight == null)
                    return;

                if (targeted is Mobile)
                {
                    Mobile mob = targeted as Mobile;

                    if (mob.InRange(from.Location, 1) && m_Flag.Fight.IsFriendly(mob, from) && mob.Backpack != null && from != mob)
                    {
                        mob.Backpack.DropItem(m_Flag);

                        mob.SendMessage("{0} has handed the flag off to you!", from.Name);
                        from.SendMessage("You have handed the flag off to {0}!", mob.Name);

                        IPooledEnumerable eable = mob.Map.GetMobilesInRange(mob.Location, 20);

                        foreach (Mobile m in eable)
                        {
                            if (m != from && m != mob)
                                m.SendMessage("You notice {0} passing the flag off to {1}!", from.Name, mob.Name);
                        }

                        eable.Free();
                    }
                }
                else if (targeted is FlagHolderComponent)
                {
                    OnTargetFlagholder(from, ((FlagHolderComponent)targeted).Holder);
                }
                else if (targeted is FlagHolder)
                {
                    OnTargetFlagholder(from, (FlagHolder)targeted);
                }
            }

            private void OnTargetFlagholder(Mobile from, FlagHolder holder)
            {
                if (holder != null && holder.AddFlag(m_Flag))
                {
                    m_Flag.LastTaken = DateTime.MinValue;
                    m_Flag.OnFlagPlaced(from);
                }
            }
        }

        public override void OnAdded(object parent)
        {
            base.OnAdded(parent);

            Mobile mob = FindOwner(parent);

            if (mob != null && m_Fight != null)
            {
                mob.SolidHueOverride = PlayerHue;
            }
        }
        
        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            Mobile mob = FindOwner(parent);

            if (mob != null && m_Fight != null)
            {
                mob.SolidHueOverride = -1;
            }
        }

        private Mobile FindOwner(object parent)
        {
            if (parent is Item)
                return ((Item)parent).RootParent as Mobile;

            if (parent is Mobile)
                return (Mobile)parent;

            return null;
        }

        public override bool Decays { get { return false; } }

        public CTFFlag(Serial serial)
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
            reader.ReadInt();

            Delete();
        }

        public static int[] FlagIDs { get { return _FlagIDs; } }
        private static int[] _FlagIDs = new int[]
        {
            5558, 5562, 5566, 5570, 5574, 5578, 5606, 5610 
        };
    }
}
