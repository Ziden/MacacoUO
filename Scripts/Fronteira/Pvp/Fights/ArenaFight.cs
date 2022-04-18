using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Network;
using Server.Items;
using Server.Regions;
using Server.Mobiles;
using Server.Accounting;
using Server.Gumps;
using Server.Spells;
 
namespace Server.TournamentSystem
{
    public enum ArenaFightType
    {
        SingleElimination,
        BestOf3,
        LastManStanding, 
		CaptureTheFlag,
        TeamRumbleCaptureTheFlag
    }
 
    public enum CancelReason
    {
        NoReason,
        NotEnoughParticipants,
        SystemError,
        Aborted,
        TimedOut
    }
 
    /*
     *Rules:
     *None:          Duh!
     *No Precasting: No casting prior to fight starting including potions
     *No Summons:    No Summon Type.  Transformations allowed
     *No Pots:       No Potions allowed
     *No Specials:   No WEAPON Specials - excludes Ninja/Bushido specials as they are considered spells
     *Pure Mage:     Magery only, no support skills ie Chiv, Necro, Bushido or Ninjitsu
     *Pure Dexxer:   Same as Pure Mage, no magery allowed
     *No Ties:       Duhh, no ties!
     *No Area Spells No wither, meteor swarm type spells
     */
 
    [Flags]
    public enum FightRules
    {
        None                = 0x00000000,
        NoPrecasting        = 0x00000001,
        NoSummons           = 0x00000002,
        NoPots              = 0x00000004,         
        NoSpecials          = 0x00000008,
        PureMage            = 0x00000010,
        PureDexxer          = 0x00000020, 
        AllowResurrections  = 0x00000040,
        AllowMounts         = 0x00000080,
        NoTies              = 0x00000100,
        NoAreaSpells        = 0x00000200,
        AllowPets           = 0x00000400,
    }

    [PropertyObject]
    public class ArenaFight
    {
        private PVPTournamentSystem m_System;
        private Timer m_Timer;
        private bool m_DoTieBreaker;
        private Tournament m_Tournament;
        private bool m_PreFight;
        private bool m_PostFight;
        private ArenaTeamType m_FightType;
        private TimeSpan m_Duration;
        private DateTime m_StartTime;
        private DateTime m_EndTime;
        private bool m_UseOwnGear;
        private int m_WagerTotal;

        private int m_Wager;
        private FightRules m_FightRules;
 
        private List<TeamInfo> m_Teams = new List<TeamInfo>();

        public PVPTournamentSystem System { get { return m_System; } set { m_System = value; } }
        public Timer Timer { get { return m_Timer; } }
 
        [CommandProperty(AccessLevel.GameMaster)]
        public bool TieBreaker { get { return m_DoTieBreaker; } set { m_DoTieBreaker = value; } }
 
        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsTournament { get { return m_Tournament != null; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Tournament Tournament { get { return m_Tournament; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PreFight { get { return m_PreFight; } set { m_PreFight = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PostFight { get { return m_PostFight; } set { m_PostFight = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeam TeamA 
        { 
            get 
            {
                if (m_Teams != null && m_Teams.Count > 0)
                {
                    return m_Teams[0].Team;
                }

                return null;
            } 
            set 
            {
                if (m_Teams.Count > 0)
                {
                    m_Teams.RemoveAt(0);
                }

                m_Teams.Insert(0, new TeamInfo(value)); 
            } 
        }
 
        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeam TeamB 
        { 
            get 
            {
                if (m_Teams != null && m_Teams.Count > 1)
                {
                    return m_Teams[1].Team;
                }

                return null;
            } 
            set
            {
                if (value != null)
                {
                    if (m_Teams.Count > 1)
                    {
                        m_Teams.RemoveAt(1);

                        m_Teams.Insert(1, new TeamInfo(value));
                    }
                    else if (m_Teams.Count == 1)
                    {
                        m_Teams.Add(new TeamInfo(value));
                    }
                }
                else if (m_Teams.Count > 1)
                {
                    m_Teams.RemoveAt(1);
                }
            } 
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Wager { get { return m_Wager; } set { m_Wager = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int WagerTotal { get { return m_WagerTotal; } set { m_WagerTotal = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaTeamType FightType 
		{ 
			get { return m_FightType; } 
			set { m_FightType = value; } 
		}
 
        [CommandProperty(AccessLevel.GameMaster)]
        public FightRules Rules { get { return m_FightRules; } set { m_FightRules = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public FightRules ForcedRules { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan FightDuration 
		{ 
			get { return m_Duration; } 
			set
			{
                if (ArenaFightType == ArenaFightType.CaptureTheFlag && value < TimeSpan.FromMinutes(10))
                    m_Duration = TimeSpan.FromMinutes(10);
                else
                    m_Duration = value;
			} 
		}
 
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime StartTime { get { return m_StartTime; } set { m_StartTime = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime EndTime { get { return m_EndTime; } set { m_EndTime = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan TimeLeft 
        { 
            get 
            { 
                if (m_Duration != TimeSpan.Zero && DateTime.UtcNow < m_StartTime + m_Duration)
                    return (m_StartTime + m_Duration) - DateTime.UtcNow;
 
                return TimeSpan.Zero;
            } 
        }

        public List<TeamInfo> Teams { get { return m_Teams; } }
 
        private Dictionary<Mobile, Dictionary<Mobile, int>> m_DamageEntries = new Dictionary<Mobile, Dictionary<Mobile, int>>();

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual ArenaFightType ArenaFightType { get { return ArenaFightType.SingleElimination; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UseOwnGear
        {
            get { return m_UseOwnGear; }
            set { m_UseOwnGear = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public League League { get; set; }

        public ArenaFight(PVPTournamentSystem system, Tournament tourney)
        {
            m_System = system;
            m_Duration = m_System.DefaultDuration;
            m_Tournament = tourney;
            m_FightType = Config.DefaultTeamType;
            m_DoTieBreaker = true;

            UseOwnGear = true;
        }

        public override string ToString()
        {
            return "...";
        }

        public virtual void ActionTimedOut(Mobile creator)
        {
        }

        public void RegisterFight()
        {
            RegisterFight(TimeSpan.Zero);
        }

        public void RegisterFight(TimeSpan delay)
        {
            if (!m_System.CanUse && !IsTournament)
            {
                m_System.AddToQueue(this);
            }
            else
            {
                m_System.CurrentFight = this;
                m_System.InUse = true;

                Timer.DelayCall(delay, OnBeforeFight);
            }
        }
 
        protected virtual void OnBeforeFight()
        {
            if (TeamA == null || TeamB == null)
                return;
 
            m_System.RemoveFromQueue(this);
            m_PreFight = true;

            if (HasRule(FightRules.NoTies))
                m_DoTieBreaker = true;
            else
                m_DoTieBreaker = false;
 
            Timer.DelayCall(m_System.StartDelay, new TimerCallback(BeginPreFight));
            DoStartDelayMessage();
            m_System.DoArenaKeeperMessage("Your all set, good luck!");
 
            if (m_Wager > 0)
                RemoveWager();

            m_System.OnBeforeFight();
        }
 
        public virtual void BeginPreFight()
        {
            if (m_System == null || m_System.Stone == null)
            {
                CancelFight(CancelReason.SystemError);
                return;
            }

            Timer.DelayCall(m_System.PreFightDelay, new TimerCallback(BeginFight));

            RefreshGumps();
            BeginTimer();

            if (!m_System.BeginPrefight())
                return;

            DoPrefightSetup();

            ForcedGear.RegisterFight(this);

            if (!HasRule(FightRules.AllowMounts))
            {
                foreach (var mob in m_System.FightRegion.GetEnumeratedMobiles().OfType<PlayerMobile>())
                {
                    if (mob.Mounted)
                    {
                        IMount mount = mob.Mount;
                        BaseMount mobMount = mob.Mount as BaseMount;
 
                        if (mount != null)
                        {
                            Point3D pnt = m_System.GetRandomKickLocation();
 
                            mob.PlaySound(0x140);
                            mob.FixedParticles(0x3728, 10, 15, 9955, EffectLayer.Waist);
 
                            mount.Rider = null;
 
                            if (mobMount != null)
                            {
                                mobMount.MoveToWorld(pnt, m_System.ArenaMap);

                                if (mobMount is BaseCreature)
                                    ((BaseCreature)mount).ControlOrder = OrderType.Stay;
                            }
 
                            mob.SendMessage("Mounts are not allowed in this arena, therefore have been ejected from the fighting grounds.");
                        }
                    }
                    else if (mob is PlayerMobile && mob.Flying)
                    {
                        if (mob.Spell is Spell)
                            ((Spell)mob.Spell).Disturb(DisturbType.Algo, false, false);

                        mob.Animate(AnimationType.Land, 0);
                        mob.Flying = false;
                        BuffInfo.RemoveBuff(mob, BuffIcon.Fly);

                        mob.SendMessage("No mounts means no flying!");
                    }
                }
            }
        }

        public void RefreshGumps(bool force = false)
        {
            if (!force && NextGumpRefresh > DateTime.UtcNow)
                return;

            foreach (var pm in GetFighters())
            {
                ParticipantGump gump = pm.FindGump<ParticipantGump>();

                if (gump == null)
                {
                    BaseGump.SendGump(new ParticipantGump(pm, this));
                }
                else
                {
                    gump.Refresh();
                }
            }

            NextGumpRefresh = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        }

        public virtual void DoPrefightSetup()
        {
            m_System.DoWall();

            Point3D a, b;

            if (m_System.RandomizeStartLocations)
            {
                if (Utility.RandomBool())
                {
                    a = m_System.TeamAStartLocation;
                    b = m_System.TeamBStartLocation;
                }
                else
                {
                    a = m_System.TeamBStartLocation;
                    b = m_System.TeamAStartLocation;
                }
            }
            else
            {
                a = m_System.TeamAStartLocation;
                b = m_System.TeamBStartLocation;
            }

            for (int i = 0; i < TeamA.Fighters.Count; ++i)
                MoveToStartSpot(TeamA.Fighters[i], a, i);

            for (int i = 0; i < TeamB.Fighters.Count; ++i)
                MoveToStartSpot(TeamB.Fighters[i], b, i);

            Timer.DelayCall(TimeSpan.FromMilliseconds(250), () =>
                {
                    foreach (var pm in GetFighters())
                    {
                        pm.Delta(MobileDelta.Noto);
                        pm.ProcessDelta();
                    }
                });
        }

        protected void BeginTimer()
        {
            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), OnTick);
            m_StartTime = DateTime.UtcNow;

            if (m_Duration == TimeSpan.Zero)
                m_EndTime = DateTime.UtcNow + m_System.DefaultDuration;
            else
                m_EndTime = DateTime.UtcNow + m_Duration;
        }

        protected void EndTimer()
        {
            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = null;
        }
 
        public virtual void BeginFight()
        {
            if (!m_System.BeginFight())
                return;

            OnBeginFight();

            m_PreFight = false;
            m_PostFight = false;

            if (m_System.Stone != null)
            {
                m_System.Stone.InvalidateGumps();
            }
        }

        public virtual void OnBeginFight()
        {
            int countA = TeamA.Fighters.Where(mob => mob.Map != Map.Internal && mob.Alive && mob.Region.IsPartOf<FightRegion>()).Count();
            int countB = TeamB.Fighters.Where(mob => mob.Map != Map.Internal && mob.Alive && mob.Region.IsPartOf<FightRegion>()).Count();

            if (countA == 0 && countB > 0)      // TEAMA = Pussies
            {
                EndFight(TeamB, TeamA, true);
            }
            else if (countB == 0 && countA > 0) // TEAMB = Pussies
            {
                EndFight(TeamA, TeamB, true);
            }
            else if (countB == 0 && countA == 0) // NOBODY SHOWED UP?  WTF?
            {
                if (!IsTournament)
                    m_DoTieBreaker = false;

                if (m_System.CurrentTournament != null)
                {
                    m_System.CurrentTournament.CancelTournamentFight(TeamA, TeamB);
                    EndTimer();
                    EjectPlayers(TimeSpan.FromSeconds(0.1));
                }
            }
            else
            {
                DoStartMessage();

                foreach (var fighter in GetFighters())
                {
                    fighter.Hidden = false;
                }
            }
        }

        public virtual void CancelTournamentFight()
        {
            if (!IsTournament || m_System == null || m_System.CurrentTournament == null)
                return;
 
            m_System.CurrentTournament.CancelTournamentFight(TeamA, TeamB);
        }
 
        public virtual void EndFight()
        {
            if (m_System == null || m_System.Stone == null)
            {
                CancelFight(CancelReason.SystemError);
                return;
            }

            if (m_DoTieBreaker)
            {
                DoTieBreaker();
                return;
            }
            else
            {
                if (League != null)
                {
                    League.OnFightEnd(TeamA, TeamB, true);
                }

                DoTieMessage();
            }

            EjectPlayers(m_System.EjectDelay);

            if (!IsTournament)
            {
                TeamA.Draws++;
                TeamB.Draws++;
            }

            m_System.EndFight(IsTournament, null, null, this);

            InvalidateNotoriety();
        }
 
        public virtual void EndFight(ArenaTeam loser)
        {
            if (loser == TeamA)
                EndFight(TeamB, TeamA);
            else
                EndFight(TeamA, TeamB);
        }
 
        public virtual void EndFight(ArenaTeam winner, ArenaTeam loser)
        {
            EndFight(winner, loser, false);
        }
 
        public virtual void EndFight(ArenaTeam winner, ArenaTeam loser, bool forfeit)
        {
            EndTimer();

            m_PostFight = true;
             
            if (m_System == null || m_System.Stone == null)
            {
                CancelFight(CancelReason.SystemError);
                return;
            }

            m_System.EndFight(IsTournament, winner, loser, this);

            TimeSpan ts;
 
            if (forfeit)
                ts = TimeSpan.FromSeconds(5);
            else
                ts = m_System.EjectDelay;

            if (!forfeit)
                winner.RegisterWin(loser, this, ArenaFightType);
            else
                winner.SendMessageToFighters("You earn 0 points as your opponent forfeit.");

            if (!CheckEliminated(winner, loser))
            {
                DoNextFight(winner, loser, ts);
                return;
            }
            else if (IsTournament && m_System.CurrentTournament != null)
            {
                EjectPlayers(ts, true);

                m_System.CurrentTournament.EndFight(winner, loser, forfeit, GetTotalDamageGiven(loser));
                InvalidateNotoriety();

                return;
            }

            DoEndFightMessage(winner, loser, forfeit);
            EjectPlayers(ts);

            if (m_Wager > 0)
            {
                AwardWager(winner);
            }
 
            if (!forfeit) //Forfeits will not give wons/losses!
            {
                winner.Wins++;
                RecordDamageEntries();

                foreach (var info in m_Teams.Where(t => t.Team != winner))
                {
                    var team = info.Team;

                    team.Losses++;
                    team.DamageGiven += GetTotalDamageGiven(team);
                    team.DamageTaken += GetTotalDamageTaken(team);
                }

                winner.DamageGiven += GetTotalDamageGiven(winner);
                winner.DamageTaken += GetTotalDamageTaken(winner);

                OnFightWon(winner);
            }

            if (League != null)
            {
                League.OnFightEnd(winner, loser, false);
            }

            LeaderStats.OnFightEnd(winner, ArenaFightType);

            InvalidateNotoriety();
        }

        public virtual void OnFightWon(ArenaTeam winner)
        {
        }

        public virtual bool CheckEliminated(ArenaTeam winner, ArenaTeam loser)
        {
            return true;
        }

        public virtual void DoNextFight(ArenaTeam winner, ArenaTeam loser, TimeSpan ts)
        {
        }

        public virtual void Clear()
        {
            if (m_Teams != null)
                ColUtility.Free(m_Teams);

            if(m_DamageEntries != null)
                m_DamageEntries.Clear();

            m_System.CurrentFight = null;

            m_Teams = null;
            m_DamageEntries = null;

            if (!IsTournament)
                m_System.InUse = false;
        }

        public void RecordDamageEntries()
        {
            foreach (var info in m_Teams)
            {
                var team = info.Team;

                if (team == null)
                {
                    continue;
                }

                foreach(var fighter in team.Fighters.Where(f => f.DamageEntries != null))
                {
                    foreach(var de in fighter.DamageEntries.Where(e => !team.IsInTeam(e.Damager)))
                    {
                        RegisterDamage(fighter, de.Damager, de.DamageGiven);
                        de.DamageGiven = 0;
                    }
                }
            }
        }

        public void RegisterDamage(Mobile victim, Mobile attacker, int damage)
        {
            if(!m_DamageEntries.ContainsKey(victim))
                m_DamageEntries[victim] = new Dictionary<Mobile, int>();
 
            if (!m_DamageEntries[victim].ContainsKey(attacker))
                m_DamageEntries[victim][attacker] = damage;
            else
                m_DamageEntries[victim][attacker] += damage;
        }
 
        public int GetTotalDamageGiven(ArenaTeam team)
        {
            int damage = 0;

            foreach (var tm in m_Teams.Select(x => x.Team).Where(t => t != team))
            {
                foreach (var fighter in tm.Fighters)
                {
                    if (m_DamageEntries.ContainsKey(fighter))
                    {
                        foreach (var kvp in m_DamageEntries[fighter])
                        {
                            if (team.IsInTeam(kvp.Key))
                            {
                                damage += kvp.Value;
                            }
                        }
                    }
                }
            }

            return damage;
        }

        public int GetTotalDamageTaken(ArenaTeam team)
        {
            int damage = 0;

            foreach (var fighter in team.Fighters)
            {
                if (m_DamageEntries.ContainsKey(fighter))
                {
                    foreach (var kvp in m_DamageEntries[fighter])
                    {
                        damage += kvp.Value;
                    }
                }
            }

            return damage;
        }

        public virtual void DoTieBreaker()
        {
            RecordDamageEntries();

            List<TeamInfo> teams;

            if (HasRule(FightRules.AllowResurrections))
            {
                teams = Teams.
                    OrderByDescending(info => info.Points).
                    ThenByDescending(info => GetTotalDamageGiven(info.Team)).
                    ThenByDescending(info => info.Team.Fighters.Where(f => f.Alive).Count()).ToList();
            }
            else
            {
                teams = Teams.
                    OrderByDescending(info => info.Points).
                    ThenByDescending(info => info.Team.Fighters.Where(f => f.Alive).Count()).
                    ThenByDescending(info => GetTotalDamageGiven(info.Team)).ToList();
            }

            EndFight(teams[0].Team, teams[1].Team);
            ColUtility.Free(teams);
        }
 
        public virtual void MoveToStartSpot(Mobile mob, Point3D loc, int idx)
        {
            if (mob == null || mob.Deleted || mob.Region == null || m_System == null || mob.Map == Map.Internal || !mob.Alive)
                return;
 
            if (mob.Region != m_System.FightRegion && mob.Region != m_System.AudienceRegion && m_System.ArenaKeeper != null && !mob.InRange(m_System.ArenaKeeper.Location, 150))
                return;
 
            int yOffset = 0;
            if (idx == 1) yOffset = 1;
            else if (idx == 2) yOffset = -1;
            else if (idx == 3) yOffset = 2;
            else if (idx == 4) yOffset = -2;

            var p = new Point3D(loc.X, loc.Y + yOffset, loc.Z);

            mob.MoveToWorld(p, m_System.ArenaMap);

            if (mob is PlayerMobile && HasRule(FightRules.AllowPets))
            {
                foreach (var pet in ((PlayerMobile)mob).AllFollowers.Where(m => m.Map != null && m.Map != Map.Internal && m.Alive && !m.IsDeadBondedPet))
                {
                    pet.MoveToWorld(p, m_System.ArenaMap);
                }
            }

            return;
        }

        public virtual void EjectPlayers(TimeSpan ts, bool clear = true)
        {
            List<PlayerMobile> eject = GetFighters().Where(f => f.Region.IsPartOf(m_System.FightRegion)).ToList();

            foreach (var pm in eject)
            {
                pm.SendMessage("You will be removed from the arena in approximately {0} seconds.", ts.TotalSeconds);
            }

            Timer.DelayCall<List<PlayerMobile>>(ts, ejectList =>
            {
                EjectPlayers(ejectList, clear);
                ClearAggressors(ejectList);

                ColUtility.Free(ejectList);

                ForcedGear.UnregisterFight(this);

                ArenaHelper.ClearRegion(m_System);
            }, eject);
        }

        public void EjectPlayers(List<PlayerMobile> list, bool clear)
        {
            if (list == null)
                return;

            foreach (var mob in list)
            {
                if (m_System == null)
                {
                    BaseCreature.TeleportPets(mob, new Point3D(992, 519, -50), Map.Malas);
                    mob.MoveToWorld(new Point3D(992, 519, -50), Map.Malas);
                }
                else if (mob.Region.IsPartOf(m_System.FightRegion))
                {
                    Map map;
                    Point3D pnt;

                    if (IsTournament)
                    {
                        map = Tournament.System.ArenaMap;
                        pnt = Tournament.System.GetRandomKickLocation();
                    }
                    else
                    {
                        map = m_System.ArenaMap;
                        pnt = m_System.GetRandomKickLocation();
                    }

                    mob.MoveToWorld(pnt, map);

                    if (mob.AllFollowers.Count > 0)
                    {
                        foreach (var pet in mob.AllFollowers.Where(pet => pet.Region.IsPartOf<FightRegion>()))
                        {
                            pet.MoveToWorld(pnt, map);
                        }
                    }

                    m_System.OnAfterEject(mob);
                }
            }

            if (clear)
            {
                Clear();
            }
        }
 
        public virtual void InvalidateNotoriety()
        {
            if (m_Teams == null)
                return;

            foreach (var m in GetFighters())
            {
                m.Criminal = false;
                m.Warmode = false;
                m.Combatant = null;
                m.Delta(MobileDelta.Noto);

                m.InvalidateProperties();
            }
        }

        public virtual void ClearAggressors(List<PlayerMobile> list)
        {
            foreach (var pm in list)
            {
                pm.Aggressed.Clear();
                pm.Aggressors.Clear();
            }
        }

        public bool Warning { get; protected set; }
        public DateTime NextGumpRefresh { get; protected set; }

        public virtual void OnTick()
        {
            if (m_EndTime < DateTime.UtcNow)
            {
                EndFight();
                EndTimer();
                RefreshGumps();
                return;
            }
            else if (DateTime.UtcNow + TimeSpan.FromMinutes(1) >= m_EndTime && !Warning)
            {
                DoWarning();
                Warning = true;
            }

            if (!m_PostFight)
            {
                RefreshGumps();
            }

            RecordDamageEntries();
            m_System.OnTickFight();
        }
 
        public void CancelFight()
        {
            CancelFight(CancelReason.NoReason);
        }
 
        public virtual void CancelFight(CancelReason reason)
        {
            string message;
            switch (reason)
            {
                default:
                case CancelReason.NoReason: message = "The fight has been canceled due to unknown reasons."; break;
                case CancelReason.NotEnoughParticipants: message = "The fight has been canceled due to not meeting the minimum number of participants."; break;
                case CancelReason.SystemError: message = "The fight has been canceled due to an unexpected system error."; break;
                case CancelReason.Aborted: message = "The fight has been canceled."; break;
                case CancelReason.TimedOut: message = "The fight has timed out."; break;
            }

            foreach (var pm in GetFighters())
            {
                pm.SendMessage(message);
            }

            EjectPlayers(TimeSpan.FromSeconds(10));

            RefundWager();

            EndTimer();
        }

        public void AwardWager(ArenaTeam winners)
        {
            if (m_Wager <= 0)
                return;

            int pot = m_WagerTotal / winners.Fighters.Count;

            foreach (var player in winners.Fighters)
            {
                Banker.Deposit(player, pot, false);
                player.SendMessage("Your team takes the purse! {0} gold has been deposited into your account.", pot.ToString("N0"));
            }

            m_Wager = 0;
        }

        protected virtual void RemoveWager()
        {
            if (m_Wager <= 0)
            {
                return;
            }

            foreach (var player in GetFighters())
            {
                RemoveWager(player);
            }
        }

        public virtual bool RemoveWager(Mobile m)
        {
            if (m_Wager <= 0)
            {
                return true;
            }

            if (Banker.Withdraw(m, m_Wager))
            {
                m.SendMessage("A wager of {0} has been withdrawn from your bank account.", m_Wager.ToString("N0"));
                m_WagerTotal += m_Wager;

                return true;
            }

            return false;
        }

        public void RefundWager()
        {
            if (m_Wager <= 0)
                return;

            foreach (var player in GetFighters())
            {
                Banker.Deposit(player, m_Wager, true);
            }

            m_Wager = 0;
        }
 
        public bool HasRule(FightRules rule)
        {
            return (m_FightRules & rule) != 0;
        }
 
        public void AddToRules(FightRules rule)
        {
            if (!HasRule(rule))
            {
                m_FightRules |= rule;
            }
        }
 
        public void RemoveRules(FightRules rule)
        {
            if (HasRule(rule) && !HasForcedRule(rule))
            {
                m_FightRules ^= rule;
            }
        }

        public bool HasForcedRule(FightRules rule)
        {
            return (ForcedRules & rule) != 0;
        }

        public void AddToForcedRules(FightRules rule)
        {
            // We're using a seprate one for the register fight gump, that way we can't remove the rule
            if (!HasForcedRule(rule))
                ForcedRules |= rule;

            if (!HasRule(rule))
                AddToRules(rule);
        }
 
        #region Announcements!!!
        protected virtual void DoStartDelayMessage()
        {
            if (m_System == null || !m_System.DoStartDelayMessage())
                return;

            bool isAlternate = IsTournament && Tournament.IsAlternateArena(TeamA);

            foreach (var mob in GetParticipantsAndAudience())
            {
                TimeSpan delay = m_System.PreFightDelay + m_System.StartDelay;

                if (IsParticipant(mob))
                {
                    mob.SendMessage(ArenaHelper.ParticipantMessageHue, "Get set to fight in {0} seconds!", delay.TotalSeconds.ToString());
                    mob.SendSound(0x403);
                }
                else if (TeamA != null && TeamB != null)
                {
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "The fight between {0} and {1} will begin in {2} seconds{3}!", TeamA.Name, TeamB.Name, delay.TotalSeconds.ToString(), isAlternate ? " at the alternate arena" : "");
                }
                else
                {
                    mob.SendMessage("The next fight will begin in {0} seconds!", delay.TotalSeconds.ToString());
                }
            }
        }

        protected virtual void DoStartMessage()
        {
            if (m_System == null || !m_System.DoStartMessage())
                return;

            bool isAlternate = IsTournament && Tournament.IsAlternateArena(TeamA);

            foreach (var mob in GetParticipantsAndAudience())
            {
                if (IsParticipant(mob))
                {
                    mob.SendMessage(ArenaHelper.ParticipantMessageHue, "FIGHT!", m_System.StartDelay.ToString());
                }
                else if (TeamA != null && TeamB != null)
                {
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "The fight between the {0} and the {1} has begun{2}!", TeamA.Name, TeamB.Name, isAlternate ? " at the alternate arena" : "");
                }
                else
                {
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "The next fight has begun!", m_System.StartDelay.ToString());
                }

                mob.SendSound(0x664); //Cannon shot
            }
        }

        protected virtual void DoTieMessage()
        {
            if (m_System == null || !m_System.DoTieMessage())
                return;

            foreach (var mob in GetParticipantsAndAudience())
            {
                if (IsParticipant(mob))
                    mob.SendMessage(ArenaHelper.ParticipantMessageHue, "Your fight has ended in a tie!");
                else if (TeamA != null && TeamB != null)
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "The fight between the {0} and the {1} has ended in a tie!", TeamA.Name, TeamB.Name);
                else
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "The fight has ended in a tie!");
            }
        }

        protected virtual void DoEndFightMessage(ArenaTeam winner, ArenaTeam loser, bool forfeit)
        {
            if (m_System == null || winner == null || !m_System.DoEndOfFightMessage(winner, loser, forfeit))
                return;
 
            foreach (var mob in GetParticipantsAndAudience())
            {
                TeamInfo info = GetTeamInfo(mob);

                if (IsParticipant(mob) && info != null && info.Team == winner)
                {
                    mob.SendMessage(ArenaHelper.ParticipantMessageHue, "You have emerged victorious in your duel{0}!", loser != null ? " versus " + loser.Name : "");
                }
                else if (IsParticipant(mob) && info != null && info.Team != winner)
                {
                    mob.SendSound(0x5B3);
                    DoDelayedMessage("Your team has been defeated despite your valiant efforts.", ArenaHelper.ParticipantMessageHue, mob, TimeSpan.FromSeconds(18));
                }
                else if (winner != null)
                {
                    mob.SendSound(mob.Female ? 0x30C : 0x41B);

                    if (loser != null)
                    {
                        mob.SendMessage(ArenaHelper.AudienceMessageHue, "{0} Arena Team has achieved victory over {1} in the {2}!", winner.Name, loser.Name, m_System.Name);
                    }
                }
                else
                {
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "The fight has ended in a tie!");
                }
            }

            if (winner != null)
            {
                m_System.DoArenaKeeperMessage(String.Format("{0} has achieved victory!", winner.Name));
            }
        }

        protected virtual void DoWarning()
        {
            if (m_System == null || m_Duration < TimeSpan.FromMinutes(1))
                return;
 
            foreach (var mob in GetParticipantsAndAudience())
            {
                if (IsParticipant(mob))
                {
                    mob.SendMessage(ArenaHelper.ParticipantMessageHue, "You have one minute remaining in the fight!");
                    mob.SendSound(0x04B);
                }
                else
                    mob.SendMessage(ArenaHelper.AudienceMessageHue, "The one minute warning has sounded!");
            }
        }

        protected virtual void DoDelayedMessage(string message, int hue, Mobile to, TimeSpan delay)
        {
            Timer.DelayCall(delay, new TimerStateCallback(DelayedMessage_Callback), new object[] { message, hue, to });
        }
 
        private void DelayedMessage_Callback(object o)
        {
            object[] list = (object[])o;
            string message = (string)list[0];
            int hue = (int)list[1];
            Mobile to = (Mobile)list[2];
 
            if (to != null)
            {
                if (hue > 0)
                    to.SendMessage(hue, message);
                else
                    to.SendMessage(message);
            }
        }
        #endregion

        public bool IsParticipant(Mobile mob)
        {
            return GetFighters().Any(pm => pm == mob);
        }

        public IEnumerable<PlayerMobile> GetFighters()
        {
            if (m_Teams == null)
            {
                yield break;
            }

            foreach (var team in m_Teams.Select(x => x.Team))
            {
                foreach (var pm in team.Fighters.OfType<PlayerMobile>())
                {
                    yield return pm;
                }
            }
        }

        public IEnumerable<PlayerMobile> GetAudience()
        {
            return m_System.GetAudience();
        }

        public IEnumerable<PlayerMobile> GetParticipantsAndAudience()
        {
            foreach (var pm in GetFighters().Where(p => !p.Region.IsPartOf(m_System.AudienceRegion)))
            {
                yield return pm;
            }

            foreach (var pm in GetAudience())
            {
                yield return pm;
            }
        }

		public TeamInfo GetTeamInfo(Mobile m)
		{
            if (Teams == null)
            {
                return null;
            }

            return Teams.FirstOrDefault(info => info.Team != null && info.Team.IsInTeam(m));
		}
		
		public TeamInfo GetTeamInfo(ArenaTeam team)
		{
            if (Teams == null)
            {
                return null;
            }

            return Teams.FirstOrDefault(info => info.Team == team);
		}

        public ArenaTeam GetTeam(Mobile m)
        {
            if (Teams == null)
            {
                return null;
            }

            var info = GetTeamInfo(m);

            return info != null ? info.Team : null;
        }
		
		public bool IsFriendly(Mobile from, Mobile to)
		{
            return !IsEnemy(from, to);
		}
		
		public bool IsEnemy(Mobile from, Mobile to)
		{
            TeamInfo fromInfo = GetTeamInfo(from);
            TeamInfo toInfo = GetTeamInfo(to);

            return fromInfo != null && toInfo != null && fromInfo != toInfo;
		}
    }
	
	public class TeamInfo
	{
		private ArenaTeam m_Team;
		private int m_Kills;
		private int m_Points;
        private int m_TeamHue;
        private DateTime m_TimeJoined;

        public ArenaTeam Team { get { return m_Team; } set { m_Team = value; } }
        public int Kills { get { return m_Kills; } set { m_Kills = value; } }
        public int Points { get { return m_Points; } set { m_Points = value; } }
        public int TeamHue { get { return m_TeamHue; } set { m_TeamHue = value; } }
        public DateTime TimeJoined { get { return m_TimeJoined; } set { m_TimeJoined = value; } }

        public TeamInfo(ArenaTeam team) : this(team, 0)
        {
        }

		public TeamInfo(ArenaTeam team, int hue)
		{
			m_Team = team;
			m_Kills = 0;
			m_Points = 0;
            m_TeamHue = hue;
		}
	}
}
