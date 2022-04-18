using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Mobiles;
using Server.Targeting;
using Server.Items;
using Server.Spells;
using Server.Spells.Fourth;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.Fifth;
using Server.Spells.Eighth;
using Server.Spells.Necromancy;
using Server.Spells.Chivalry;
using Server.Spells.Bushido;
using Server.Spells.Ninjitsu;
using Server.Spells.Spellweaving;
using Server.Spells.Mysticism;
using Server.Regions;
using Server.Spells.SkillMasteries;

namespace Server.TournamentSystem
{
    public class FightRegion : BaseRegion
    {
        public static void Initialize()
        {
            EventSink.SetAbility += new SetAbilityEventHandler(EventSink_SetAbility);
            EventSink.Login += new LoginEventHandler(EventSink_Login);
        }

        public override double InsuranceMultiplier { get { return 0; } }

        public PVPTournamentSystem System { get; set; }
 
        public FightRegion(PVPTournamentSystem system)
            : base(system.Name, system.ArenaMap, Region.DefaultPriority, system.FightingRegionBounds)
        {
            System = system;
            Music = MusicName.SerpentIsleCombat_U7;
            Register();
        }
 
        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }
 
        public override void OnLocationChanged(Mobile m, Point3D oldLocation)
        {
            if (System == null || m.AccessLevel > AccessLevel.VIP)
                base.OnLocationChanged(m, oldLocation);
            else
            {
                Mobile check = m;

                if (m is BaseCreature && (((BaseCreature)m).Summoned || ((BaseCreature)m).Controlled))
                    check = ((BaseCreature)m).GetMaster();
 
                if (System.CurrentFight != null)
                {
                    if (System.CurrentFight.HasRule(FightRules.NoSummons) && m is BaseCreature && ((BaseCreature)m).Summoned)
                        Kick(m); //Kick summons if no summons as a rule
                    else if (!System.CurrentFight.HasRule(FightRules.AllowPets) && m is BaseCreature && ((BaseCreature)m).Controlled && ((BaseCreature)m).SummonMaster == null)
                        Kick(m); //Kick pets
                    else if (check is PlayerMobile && !System.CurrentFight.IsParticipant(check))
                    {
                        Kick(check);      //kick non-participants
                        if (check != m)   //and/or kick thier pets
                            Kick(m);
                    }
                }
            }
            base.OnLocationChanged(m, oldLocation);
        }

        public override bool CanUseStuckMenu(Mobile m)
        {
            return false;
        }

		public override bool OnSkillUse( Mobile m, int Skill )
        {
            if(System == null || System.CurrentFight == null || m.Backpack == null)
				return base.OnSkillUse(m, Skill);
			
			if(Skill == 21 && System.CurrentFight.ArenaFightType == ArenaFightType.CaptureTheFlag && m.Backpack.FindItemByType(typeof(CTFFlag)) != null)
			{
				m.SendMessage("You are forbidden from hiding while holding a flag!");
				return false;
			}
				
			return base.OnSkillUse(m, Skill);
        }
 
        private void Kick(Mobile m)
        {
            m.MoveToWorld(System.GetRandomKickLocation(), System.ArenaMap);
            m.SendMessage("You have been ejected from the arena!");
        }
 
        public override bool OnDoubleClick(Mobile m, object o)
        {
            if (m.AccessLevel > AccessLevel.VIP)
                return base.OnDoubleClick(m, o);
 
            if (o is Item)
            {
                if (o is Corpse && ((Corpse)o).Owner != m)
                    return false;

                Item item = (Item)o;
 
                if (System.CurrentFight != null)
                {
                    ArenaFight f = System.CurrentFight;
                    if (f.HasRule(FightRules.NoPots) || (f.HasRule(FightRules.NoPrecasting) && f.PreFight))
                    {
                        if(Config.NonConsumableList.Any(t => item.GetType() == t || item.GetType().IsSubclassOf(t)))    
                            return false;
                    }
 
                    if (!f.HasRule(FightRules.AllowMounts) && (item is IMount || item is IMountItem))
                        return false;

                    if (item is BallOfSummoning || item is BaseTalisman || item is BraceletOfBinding || (item is Corpse && ((Corpse)item).Owner != m && System.CurrentFight.ArenaFightType != ArenaFightType.CaptureTheFlag))
                        return false;
                }

            }
 
            return base.OnDoubleClick(m, o);
        }
 
        public override bool OnBeginSpellCast(Mobile m, ISpell s)
        {
            if (s is RecallSpell || s is GateTravelSpell || s is SacredJourneySpell || s is MarkSpell)
                return false;
 
            if (System.CurrentFight != null)
            {
                ArenaFight f = System.CurrentFight;
 
                if (f.HasRule(FightRules.NoPrecasting) && f.PreFight)
                    return false;
 
                if (!f.HasRule(FightRules.AllowResurrections) && (s is ResurrectionSpell || s is NobleSacrificeSpell))
                    return false;

                if ((f.HasRule(FightRules.PureDexxer) || (f.Tournament != null && f.Tournament.TourneyStyle == TourneyStyle.DexxersOnly)) && s is MagerySpell)
                    return false;

                if (f.HasRule(FightRules.NoSummons))
                {
                    if (s is MagerySpell && ((MagerySpell)s).Circle == SpellCircle.Eighth && !(s is EarthquakeSpell) && !(s is ResurrectionSpell))
                        return false;
                    else if (s is BladeSpiritsSpell || s is AnimateDeadSpell || s is VengefulSpiritSpell || s is SummonFeySpell || s is SummonFiendSpell || s is SummonCreatureSpell || s is RisingColossusSpell || s is AnimatedWeaponSpell)
                        return false;
                    else if (s is AnimatedWeaponSpell || s is RisingColossusSpell || s is SummonReaperSpell)
                        return false;
                }

 
                if (f.HasRule(FightRules.PureMage) || f.HasRule(FightRules.PureDexxer))
                {
                    if (s is PaladinSpell || s is NecromancerSpell || s is ArcanistSpell || s is SamuraiSpell || s is NinjaSpell || s is MysticSpell || s is SkillMasterySpell)
                        return false;
                }
 
                if (f.HasRule(FightRules.NoAreaSpells))
                {
                    if (s is WitherSpell || s is PoisonStrikeSpell || s is HolyLightSpell || s is EarthquakeSpell || s is MeteorSwarmSpell || s is ChainLightningSpell || s is MassCurseSpell || s is ThunderstormSpell
                        || s is EssenceOfWindSpell || s is WildfireSpell || s is MassSleepSpell || s is HailStormSpell || s is NetherCycloneSpell || s is NetherBlastSpell)
                        return false;
                }

                if (!f.HasRule(FightRules.AllowMounts) && s is FlySpell)
                {
                    return false;
                }
            }
 
            return base.OnBeginSpellCast(m, s);
        }
 
        public override bool OnResurrect(Mobile m)
        {
            if (System != null && System.CurrentFight != null)
            {
                ArenaFight f = System.CurrentFight;

                Timer.DelayCall(() => 
                    {
                        if (System.CurrentFight != null && !System.CurrentFight.PostFight)
                        {
                            f.RefreshGumps(true);
                        }
                    });

                return f.HasRule(FightRules.AllowResurrections) || f.ArenaFightType == ArenaFightType.CaptureTheFlag;
            }
 
            return false;
        }
 
        private static void EventSink_SetAbility(SetAbilityEventArgs e)
        {
            if (e.Mobile == null)
                return;
 
            int index = e.Index;
            PVPTournamentSystem system = PVPTournamentSystem.GetRegionalSystem(e.Mobile.Region);
 
            if (system != null && system.CurrentFight != null && index != 0 && system.CurrentFight.HasRule(FightRules.NoSpecials))
            {
                WeaponAbility.ClearCurrentAbility(e.Mobile);  //this returns ability for penalty?
                e.Mobile.SendMessage("You have agreed to not using weapon special moves during this fight!");
            }
        }
 
        public override void OnDeath(Mobile m)
        {
            Mobile killer = m.LastKiller;

            if (killer is BaseCreature)
                killer = ((BaseCreature)killer).GetMaster();

            if (System == null || System.CurrentFight == null || System.CurrentFight.PreFight)
			{
                base.OnDeath(m);
				return;
			}

			if(killer != null && killer != m)
			{
                TeamInfo killerInfo = System.CurrentFight.GetTeamInfo(killer);
                TeamInfo victimInfo = System.CurrentFight.GetTeamInfo(m);

                if (killerInfo != null && victimInfo != null && killerInfo != victimInfo && !ArenaHelper.IsSameAccount(killer, m))
                    System.HandleKill(killerInfo, victimInfo, killer, m);
			}

            if (m is PlayerMobile && m.Backpack != null && m.Corpse != null)
            {
                Item[] items = m.Backpack.FindItemsByType(typeof(CTFFlag));

                if (items != null && items.Length > 0)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        items[i].MoveToWorld(new Point3D(m.Corpse.X, m.Corpse.Y, m.Corpse.Z + (i * 2)), m.Corpse.Map);

                        if (items[i] is CTFFlag)
                            ((CTFFlag)items[i]).LastTaken = DateTime.MinValue;
                    }
                }

                items = m.Corpse.FindItemsByType(typeof(CTFFlag));

                if (items != null && items.Length > 0)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        items[i].MoveToWorld(new Point3D(m.Corpse.X, m.Corpse.Y, m.Corpse.Z + (i * 2)), m.Corpse.Map);

                        if (items[i] is CTFFlag)
                            ((CTFFlag)items[i]).LastTaken = DateTime.MinValue;
                    }
                }
            }

            if (m is PlayerMobile)
            {
                if (System.CurrentFight.ArenaFightType != ArenaFightType.CaptureTheFlag)
                {
                    ArenaFight f = System.CurrentFight;
                    ArenaTeam team = ArenaTeam.GetTeam(m, f.FightType);

                    if (f.ArenaFightType == ArenaFightType.LastManStanding)
                    {
                        int left = f.Teams.Select(x => x.Team).Where(t => !t.AllDead(m)).Count();

                        if (left == 1)
                        {
                            f.EndFight(f.Teams.Select(x => x.Team).FirstOrDefault(t => !t.AllDead(m)), null, false);
                        }
                        else if (killer != null && killer != m)
                        {
                            killer.SendMessage(ArenaHelper.ParticipantMessageHue, "You have eliminated {0} from the arena fight. You have {1} {2} left.", m.Name, left - 1, left - 1 == 1 ? "team" : "teams");
                            ArenaHelper.DoAudienceRegionMessage(String.Format("{0} has eliminated {1} from the last man standing fight!", killer.Name, m.Name), 0, System);
                        }
                    }
                    else if (team != null && team.AllDead(m))
                    {
                        f.EndFight(team);
                    }
                }

                Timer.DelayCall(() => 
                    {
                        if (System.CurrentFight != null && !System.CurrentFight.PostFight)
                        {
                            System.CurrentFight.RefreshGumps(true);
                        }
                    });
            }

            if (System.Stone != null)
            {
                Timer.DelayCall(System.Stone.InvalidateGumps);
            }

            base.OnDeath(m);
        }

        public override bool OnTarget(Mobile m, Target t, object o)
        {
            if (System == null || System.CurrentFight == null)
                return true;

            Mobile target = o as Mobile;

            if (target != null && t is InvisibilitySpell.InternalTarget && System.CurrentFight.ArenaFightType == ArenaFightType.CaptureTheFlag && target.Backpack.FindItemByType(typeof(CTFFlag)) != null)
            {
                if (target == m)
                    m.SendMessage("You are forbidden from hiding while holding a flag!");
                else
                    m.SendMessage("You cannot hide them while they are holding a flag!");

                return false;
            }

            return true;
        }
 
        public override bool AllowBeneficial(Mobile from, Mobile target)
        {
            if (System == null)
                return base.AllowBeneficial(from, target);
 
            if (System.CurrentFight != null)
            {
                //foreach (ArenaTeam team in System.CurrentFight.Teams)
                foreach (TeamInfo info in System.CurrentFight.Teams)
                {
                    if (info == null || info.Team == null)
                        continue;

                    if (info.Team.IsInTeam(from) && info.Team.IsInTeam(target))
                        return true;
                }
            }
 
            return false;
        }
 
        public override bool AllowHarmful(Mobile from, IDamageable d)
        {
            Mobile target = d as Mobile;

            if (System != null && target != null && System.CurrentFight != null)
            {
                if (System.CurrentFight.PreFight || System.CurrentFight.PostFight || !target.Region.IsPartOf<FightRegion>())
                    return false;

                foreach (TeamInfo info in System.CurrentFight.Teams)
                {
                    ArenaTeam team = info.Team;

                    if (team == null)
                        continue;

                    if (team.IsInTeam(from) && team.IsInTeam(target))
                        return false;

                    if ((team.IsInTeam(from) && !team.IsInTeam(target)) || (!team.IsInTeam(from) && team.IsInTeam(target)))
                        return true;
                }
            }
 
            return base.AllowHarmful(from, target);
        }
 
        public override void OnSpeech(SpeechEventArgs args)
        {
            Mobile mob = args.Mobile;
            string str = args.Speech;
 
            if (mob is PlayerMobile && str != null && str.ToLower().Trim() == "rules" && System != null && System.CurrentFight != null)
            {
                if (System.CurrentFight.Rules == 0)
                    mob.SendMessage("No Rules");
                else
                {
                    foreach (int i in Enum.GetValues(typeof(FightRules)))
                    {
                        if (System.CurrentFight.HasRule((FightRules)i))
                            mob.SendMessage(ArenaHelper.ParticipantMessageHue, ArenaHelper.GetRules((FightRules)i));
                    }
                }
            }
        }

        public override void OnEnter(Mobile from)
        {
            if (System != null)
                System.RegionOnEnter(from);

            base.OnEnter(from);
        }

        public override void OnExit(Mobile from)
        {
            if (System != null)
                System.RegionOnExit(from);

            base.OnExit(from);
        }

        public override bool CheckTravel(Mobile traveller, Point3D p, TravelCheckType type)
        {
            if (type == TravelCheckType.TeleportTo)
            {
                if (Region.Find(traveller.Location, traveller.Map) != this || traveller.Z != p.Z)
                {
                    traveller.SendLocalizedMessage(501035); // You cannot teleport from here to the destination.
                    return false;
                }
            }

            return type > TravelCheckType.Mark;
        }

        public static void EventSink_Login(LoginEventArgs e)
        {
            if (e.Mobile.AccessLevel > AccessLevel.VIP)
                return;

            Mobile from = e.Mobile;
            Region r = Region.Find(from.Location, from.Map);

            if (r is FightRegion)
            {
                FightRegion region = r as FightRegion;

                if (region.System.CurrentFight == null || !region.System.CurrentFight.IsParticipant(from))
                    region.Kick(from);
            }
        }
    }
}
