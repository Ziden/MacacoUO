using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;

namespace Server.TournamentSystem
{
    public class TeamRumbleCaptureTheFlag : CaptureTheFlagFight
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public override ArenaFightType ArenaFightType { get { return ArenaFightType.TeamRumbleCaptureTheFlag; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxTeamCount { get { return GetMaxTeamCount(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinPlayerCount { get { return MaxTeamCount == 4 ? Config.TeamRumbleCTFMinPlayerCount * 2 : Config.TeamRumbleCTFMinPlayerCount; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime Begins { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner { get; set; }

        public List<Mobile> InitialParticipants = new List<Mobile>();
        public bool CanBegin { get { return ParticipantCount() >= MinPlayerCount; } }

        public TeamRumbleCaptureTheFlag(PVPTournamentSystem system)
            : base(system, null)
        {
            BuildTeams();
            _Expires = DateTime.UtcNow + Config.TeamRumbleCTFExpirePeriod;
        }

        private void BuildTeams()
        {
            for (int i = 0; i < MaxTeamCount; i++)
            {
                Teams.Add(new TeamInfo(new ArenaTeam() { Name = String.Format("Team {0}", i + 1) }));
            }
        }

        public override void BeginPreFight()
        {
            CheckParticipantList();

            if (InitialParticipants.Count < MinPlayerCount)
            {
                CancelFight(CancelReason.NotEnoughParticipants);
            }
            else
            {
                League.Shuffle(InitialParticipants);

                for (int i = 0; i < InitialParticipants.Count; i++)
                {
                    AddToTeam(InitialParticipants[i]);
                }

                base.BeginPreFight();
            }
        }

        private void CheckParticipantList()
        {
            for (int i = InitialParticipants.Count - 1; i >= 0; i--)
            {
                var m = InitialParticipants[i];

                if (m.Map == Map.Internal)
                {
                    InitialParticipants.Remove(m);
                }
            }
        }

        public int ParticipantCount()
        {
            return InitialParticipants.Count;
        }

        public void AddParticipant(Mobile m)
        {
            if (!InitialParticipants.Contains(m))
            {
                InitialParticipants.Add(m);

                foreach (var pm in System.GetAudience())
                {
                    if (pm != m)
                    {
                        pm.SendMessage(ArenaHelper.AudienceMessageHue, String.Format("{0} has joined the next Team Rumble CTF Game!", m.Name));
                    }
                }
            }
        }

        public void RemoveParticipant(Mobile m)
        {
            if (InitialParticipants.Contains(m))
            {
                InitialParticipants.Remove(m);

                foreach (var pm in System.GetAudience())
                {
                    if (pm != m)
                    {
                        pm.SendMessage(ArenaHelper.AudienceMessageHue, String.Format("{0} has left the Team Rumble CTF Game!", m.Name));
                    }
                }
            }
        }

        public void AddToTeam(Mobile m)
        {
            if (CTFSystem == null)
            {
                return;
            }

            var teams = MaxTeamCount;

            int count = Teams[0].Team.GetPlayerCount();
            var nextAdd = Teams[0].Team;

            for (int i = 1; i < Teams.Count; i++)
            {
                var team = Teams[i].Team;

                if (team.GetPlayerCount() < count)
                {
                    nextAdd = team;
                    break;
                }
            }

            nextAdd.AddFighter(m);
        }

        private DateTime _NextMessage;
        private DateTime _Expires;

        public void PreFightTick()
        {
            if (System.CurrentFight == this && Begins < DateTime.UtcNow && !PreFight)
            {
                return;
            }

            if (_NextMessage == DateTime.MinValue)
            {
                _NextMessage = DateTime.UtcNow + TimeSpan.FromSeconds(10);
            }

            if (_NextMessage < DateTime.UtcNow)
            {
                if (System.ArenaKeeper != null)
                {
                    if (PreFight && Begins > DateTime.UtcNow)
                    {
                        int seconds = (int)(Begins - DateTime.UtcNow).TotalSeconds;

                        System.ArenaKeeper.Say("CTF Team Rumble will being in {0}!",
                            string.Format("{0} {1}",
                            seconds >= 60 ? seconds / 60 : seconds,
                            seconds == 60 ? "minute" : seconds > 60 ? "minutes" : "seconds"));
                    }

                    System.ArenaKeeper.Say("Join the pending Team Rumble CTF! Sign up with the arena stone{0}", System is CTFRoyalRumbleArena ? "!" : " or enter the waiting arena by double clicking the doors!");
                }

                _NextMessage = DateTime.UtcNow + TimeSpan.FromSeconds(30);
            }
        }

        public void CheckExpired()
        {
            if (_Expires < DateTime.UtcNow)
            {
                CancelFight(CancelReason.TimedOut);
            }
        }

        protected override void RemoveWager()
        {
        }

        protected override void DoStartDelayMessage()
        {
            if (!System.DoStartDelayMessage())
            {
                return;
            }

            Timer.DelayCall(TimeSpan.FromSeconds(3), () =>
            {
                foreach (var mob in GetParticipantsAndAudience())
                {
                    int seconds = (int)(Begins - DateTime.UtcNow).TotalSeconds;

                    mob.SendMessage(ArenaHelper.ParticipantMessageHue, "Team Rumble Capture the Flag will begin in about {0}!",
                        string.Format("{0} {1}",
                        seconds >= 60 ? seconds / 60 : seconds,
                        seconds == 60 ? "minute" : seconds > 60 ? "minutes" : "seconds"));

                    if (InitialParticipants.Contains(mob))
                    {
                        mob.SendMessage(ArenaHelper.ParticipantMessageHue, "Once the game begins, you will be randomly placed on a team.");
                        mob.SendSound(0x403);
                    }
                }
            });
        }

        public override void OnFightWon(ArenaTeam winner)
        {
            if (winner.TeamType == ArenaTeamType.Temp)
            {
                for (int i = 0; i < winner.Fighters.Count; i++)
                {
                    var team = ArenaTeam.GetTeam(winner.Fighters[i], ArenaTeamType.Single);

                    if (team != null)
                    {
                        team.CTFTeamRumbleWins++;
                    }
                }
            }
            else
            {
                base.OnFightWon(winner);
            }
        }

        public override void CancelFight(CancelReason reason)
        {
            if (System.CurrentFight == this)
            {
                base.CancelFight(reason);
                return;
            }

            string message;

            switch (reason)
            {
                case CancelReason.NotEnoughParticipants:
                    message = "The CTF Game has been canceled due to not meeting the minimum number of participants.";
                    break;
                case CancelReason.Aborted:
                    message = "The CTF Game has been canceled.";
                    break;
                case CancelReason.TimedOut:
                    message = "The CTF game has timed out before enough players can join. Try again later.";
                    break;
                default:
                    message = "The fight has been canceled due to unknown reasons.";
                    break;
            }

            for (int i = 0; i < InitialParticipants.Count; i++)
            {
                var m = InitialParticipants[i];

                if (CTFSystem != null && CTFSystem.WaitingRoom != null)
                {
                    CTFSystem.WaitingRoom.Kick(m);
                }
                else
                {
                    var p = System != null ? System.GetRandomKickLocation() : new Point3D(989, 520, -50);
                    var map = System != null ? System.ArenaMap : Map.Malas;

                    BaseCreature.TeleportPets(m, p, map);
                    m.MoveToWorld(p, map);
                }

                m.SendMessage(message);
            }

            RefundWager();
            Clear();
            EndTimer();
            ResetFlags();

            CTFSystem.PendingTeamRumble = null;
        }

        public int GetMaxTeamCount()
        {
            if (CTFSystem != null)
            {
                return CTFSystem.FlagHolders.Count;
            }

            return 0;
        }
    }
}
