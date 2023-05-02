#region References
using System;
using System.Collections.Generic;
using Server.Dueling;
using Server.Engines.ArenaSystem;
using Server.Engines.PartySystem;
using Server.Engines.Quests;
using Server.Engines.VvV;
using Server.Factions;
using Server.Guilds;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;
using Server.SkillHandlers;
using Server.Spells.Chivalry;
using Server.Spells.Seventh;
#endregion

namespace Server.Misc
{
    public class NotorietyHandlers
    {
        public static void Initialize()
        {
            Notoriety.Hues[Notoriety.Innocent] = 0x59;
            Notoriety.Hues[Notoriety.Ally] = 0x3F;
            Notoriety.Hues[Notoriety.CanBeAttacked] = 0x3B2;
            Notoriety.Hues[Notoriety.Criminal] = 0x3B2;
            Notoriety.Hues[Notoriety.Enemy] = 0x90;
            Notoriety.Hues[Notoriety.Murderer] = 0x22;
            Notoriety.Hues[Notoriety.Invulnerable] = 0x35;

            Notoriety.Handler = MobileNotoriety;

            Mobile.AllowBeneficialHandler = Mobile_AllowBeneficial;
            Mobile.AllowHarmfulHandler = Mobile_AllowHarmful;
        }

        private enum GuildStatus
        {
            None,
            Peaceful,
            Warring
        }

        private static GuildStatus GetGuildStatus(Mobile m)
        {
            if (m.Guild == null)
                return GuildStatus.None;

            if (((Guild)m.Guild).Enemies.Count == 0 && m.Guild.Type == GuildType.Regular)
                return GuildStatus.Peaceful;

            return GuildStatus.Warring;
        }

        private static bool CheckBeneficialStatus(GuildStatus from, GuildStatus target)
        {
            if (from == GuildStatus.Warring || target == GuildStatus.Warring)
                return false;

            return true;
        }

        public static bool Mobile_AllowBeneficial(Mobile from, Mobile target)
        {
            if (from == null || target == null || from.IsStaff() || target.IsStaff())
                return true;

            var map = from.Map;

            #region Factions/VvV
            /*
            if (Settings.Enabled)
            {
                var targetFaction = Faction.Find(target, true);

                if ((!Core.ML || map == Faction.Facet) && targetFaction != null)
                {
                    if (Faction.Find(from, true) != targetFaction)
                        return false;
                }
            }
            */

            if (Party.Get(from) == Party.Get(target))
                return true;

            if (ViceVsVirtueSystem.Enabled && ViceVsVirtueSystem.IsEnemy(from, target))
            {
                return false;
            }
            #endregion

            if (map != null && (map.Rules & MapRules.BeneficialRestrictions) == 0)
                return true; // In felucca, anything goes

            if (!from.Player)
                return true; // NPCs have no restrictions

            if (target is BaseCreature && !((BaseCreature)target).Controlled)
                return false; // Players cannot heal uncontrolled mobiles

            if (from is PlayerMobile && ((PlayerMobile)from).Young && target is BaseCreature &&
                ((BaseCreature)target).Controlled)
                return true;

            //if (from is PlayerMobile && ((PlayerMobile)from).Young &&
            //    (!(target is PlayerMobile) || !((PlayerMobile)target).Young))
            //    return false; // Young players cannot perform beneficial actions towards older players

            var fromGuild = from.Guild as Guild;
            var targetGuild = target.Guild as Guild;

            if (fromGuild != null && targetGuild != null)
            {
                if (targetGuild == fromGuild || fromGuild.IsAlly(targetGuild))
                    return true; // Guild members can be beneficial
            }

            return CheckBeneficialStatus(GetGuildStatus(from), GetGuildStatus(target));
        }

        private static bool ProtecaoRP(PlayerMobile from)
        {
            return from != null && from.RP;
        }

        public static bool Mobile_AllowHarmful(Mobile attacker, IDamageable damageable)
        {
            var defender = damageable as Mobile;

            if (defender is WispGuia)
                return false;

            if (attacker == null || defender == null || attacker.IsStaff() || defender.IsStaff())
                return true;

            var fromGuild = GetGuildFor(attacker.Guild as Guild, attacker);
            var targetGuild = GetGuildFor(defender.Guild as Guild, defender);

            // GVG
            if (fromGuild != null && targetGuild != null)
            {
                if (fromGuild == targetGuild || fromGuild.IsAlly(targetGuild) || fromGuild.IsEnemy(targetGuild))
                    return true; // Guild allies or enemies can be harmful
            }

            if (defender != null && defender.Region != null && defender.Region.IsPartOf("New Haven") && attacker != null && (attacker.Player || attacker.IsControlled()))
            {
                if (!DuelController.IsInDuel(defender));
                    return false;
            }
                

            // Factions vs Factions
            if (Settings.Enabled)
            {
                var srcFaction = Faction.Find(attacker, true, true);
                var trgFaction = Faction.Find(defender, true, true);

                if (srcFaction != null && trgFaction != null && srcFaction != trgFaction)
                    return true;
            }

            if(ViceVsVirtueSystem.IsEnemy(attacker, defender))
            {
                return true;
            }

            var map = attacker.Map;

            var atkCreature = attacker as BaseCreature;
            var bcTarget = damageable as BaseCreature;
            var targPlayer = damageable as PlayerMobile;
            var attackerPlayer = attacker as PlayerMobile;
            var defCreature = defender as BaseCreature;
            var defPlayer = defender as PlayerMobile;

            if (bcTarget != null && bcTarget.GetMaster() is PlayerMobile)
                targPlayer = bcTarget.GetMaster() as PlayerMobile;


            // if non player pet attacking
            if (attacker != null && !attacker.Player && !(atkCreature != null && atkCreature.GetMaster() != null && atkCreature.GetMaster().IsPlayer()))
            {
                if (targPlayer != null && ProtecaoRP(targPlayer) && atkCreature != null && atkCreature.GetMaster() != null && !atkCreature.GetMaster().RP)
                    return false;
                
                if (!CheckAggressor(attacker.Aggressors, defender) && !CheckAggressed(attacker.Aggressed, defender) && defender is PlayerMobile &&
                    ((PlayerMobile)defender).IsResProtected())
                {
                    return false;
                }
            }

            // PVPs
            if (attackerPlayer != null && targPlayer != null)
            {
                var targ = targPlayer;
                if (targ.Young)
                {
                    return false;
                }

                if (targ.RP != attacker.RP && ProtecaoRP(targ))
                {
                    attacker.SendMessage("Voce nao pode atacar um jogador com o modo de jogo RP difetente do seu.");
                    return false;
                }

                if (attackerPlayer.Young)
                {
                    attackerPlayer.SendMessage("Voce nao pode atacar outros jogadores ou pets deles por ser novato");
                    return false;
                    /*
                    attackerPlayer.Young = false;
                    attackerPlayer.SendMessage(38, "Voce perdeu seu status de novato por atacar outro jogador");
                    if (attackerPlayer.Wisp != null)
                    {
                        attackerPlayer.Wisp.Delete();
                        attackerPlayer.Wisp = null;
                    }
                    */
                }
                if (targ.HasPvMTag && targ.Region is DungeonRegion)
                {
                    attackerPlayer.SendMessage("Voce nao pode atacar jogador com status PvM dentro de dungeon.");
                    return false;
                }
                if (attackerPlayer.HasPvMTag && attackerPlayer.Region is DungeonRegion)
                {
                    attackerPlayer.SendMessage("Voce nao pode atacar jogador com status PvM dentro de dungeon.");
                    return false;
                }
            }

            // PvP com pet
            if (attacker.Player && defender is BaseCreature)
            {
                var master = ((BaseCreature)defender).ControlMaster;
                var masterPlayer = master as PlayerMobile;
                if (!attacker.RP && master != null && master.RP && ProtecaoRP(master as PlayerMobile))
                    return false;

                if ((attackerPlayer.HasPvMTag) && master != null && (attackerPlayer.Region is DungeonRegion))
                {
                    attackerPlayer.SendMessage("Voce nao pode atacar jogador com status PvM dentro de dungeon.");
                    return false;
                }

                if (attacker.IsYoung() && master is PlayerMobile)
                    return false;
            }

            if (map != null && (map.Rules & MapRules.HarmfulRestrictions) == 0)
                return true; // In felucca, anything goes

            // Summons should follow the same rules as their masters
            if (attacker is BaseCreature && ((BaseCreature)attacker).Summoned && ((BaseCreature)attacker).SummonMaster != null)
                attacker = ((BaseCreature)attacker).SummonMaster;

            if (defender is BaseCreature && ((BaseCreature)defender).Summoned && ((BaseCreature)defender).SummonMaster != null)
                defender = ((BaseCreature)defender).SummonMaster;

            if (!attacker.Player && !(atkCreature != null && atkCreature.GetMaster() != null && atkCreature.GetMaster().IsPlayer()))
            {                              /*
                if (!CheckAggressor(from.Aggressors, target) && !CheckAggressed(from.Aggressed, target) && target is PlayerMobile &&
                    ((PlayerMobile)target).CheckYoungProtection(from))
                    return false;
                    */
                return true; // Uncontrolled NPCs are only restricted by the young system
            }


            if (ViceVsVirtueSystem.Enabled && ViceVsVirtueSystem.EnhancedRules && ViceVsVirtueSystem.IsEnemy(attacker, damageable))
                return true;

            if (defender is BaseCreature)
            {
                if (((BaseCreature)defender).Controlled)
                    return false; // Cannot harm other controlled mobiles

                if (((BaseCreature)defender).Summoned && attacker != ((BaseCreature)defender).SummonMaster)
                    return false; // Cannot harm other controlled mobiles
            }

            if (defender.Player)
                return false; // Cannot harm other players

            if (!(defender is BaseCreature && ((BaseCreature)defender).InitialInnocent))
            {
                if (Notoriety.Compute(attacker, defender) == Notoriety.Innocent)
                    return false; // Cannot harm innocent mobiles
            }

            return true;
        }

        public static Guild GetGuildFor(Guild def, Mobile m)
        {
            var g = def;

            var c = m as BaseCreature;

            if (c != null && c.Controlled && c.ControlMaster != null && !c.ForceNotoriety)
            {
                c.DisplayGuildTitle = false;

                if (c.Map != null && c.Map != Map.Internal)
                {
                    if (Core.AOS || Guild.NewGuildSystem || c.ControlOrder == OrderType.Attack || c.ControlOrder == OrderType.Guard)
                        g = (Guild)(c.Guild = c.ControlMaster.Guild);
                    else if (c.Map == null || c.Map == Map.Internal || c.ControlMaster.Guild == null)
                        g = (Guild)(c.Guild = null);
                }
                else
                {
                    if (c.Map == null || c.Map == Map.Internal || c.ControlMaster.Guild == null)
                        g = (Guild)(c.Guild = null);
                }
            }

            return g;
        }

        public static int CorpseNotoriety(Mobile source, Corpse target)
        {
            if (target.AccessLevel > AccessLevel.VIP)
                return Notoriety.CanBeAttacked;

            Body body = target.Amount;

            var cretOwner = target.Owner as BaseCreature;

            if (cretOwner != null)
            {
                var sourceGuild = GetGuildFor(source.Guild as Guild, source);
                var targetGuild = GetGuildFor(target.Guild, target.Owner);

                if (sourceGuild != null && targetGuild != null)
                {
                    if (sourceGuild == targetGuild)
                        return Notoriety.Ally;

                    if (sourceGuild.IsAlly(targetGuild))
                        return Notoriety.Ally;

                    if (sourceGuild.IsEnemy(targetGuild))
                        return Notoriety.Enemy;
                }

                if (Settings.Enabled)
                {
                    var srcFaction = Faction.Find(source, true, true);
                    var trgFaction = Faction.Find(cretOwner, true, true);

                    if (srcFaction != null && trgFaction != null && srcFaction != trgFaction && source.Map == Faction.Facet)
                        return Notoriety.Enemy;
                }

                if (ViceVsVirtueSystem.Enabled && ViceVsVirtueSystem.IsEnemy(source, cretOwner))
                    return Notoriety.Enemy;

                if (CheckHouseFlag(source, cretOwner, target.Location, target.Map))
                    return Notoriety.CanBeAttacked;

                var actual = Notoriety.CanBeAttacked;

                if (target.Murderer)
                    actual = Notoriety.Murderer;
                else if (body.IsMonster && IsSummoned(cretOwner))
                    actual = Notoriety.Murderer;
                else if (cretOwner.AlwaysMurderer || cretOwner.IsAnimatedDead)
                    actual = Notoriety.Murderer;

                if (DateTime.UtcNow >= (target.TimeOfDeath + Corpse.MonsterLootRightSacrifice))
                    return actual;

                var sourceParty = Party.Get(source);

                foreach (var m in target.Aggressors)
                {
                    if (m == source || (sourceParty != null && Party.Get(m) == sourceParty) || (sourceGuild != null && m.Guild == sourceGuild))
                        return actual;
                }

                return Notoriety.Innocent;
            }
            else
            {
                if (target.Murderer)
                    return Notoriety.Murderer;

                if (target.Criminal && target.Map != null && ((target.Map.Rules & MapRules.HarmfulRestrictions) == 0))
                    return Notoriety.Criminal;

                var sourceGuild = GetGuildFor(source.Guild as Guild, source);
                var targetGuild = GetGuildFor(target.Guild, target.Owner);

                if (sourceGuild != null && targetGuild != null)
                {
                    if (sourceGuild == targetGuild || sourceGuild.IsAlly(targetGuild))
                        return Notoriety.Ally;

                    if (sourceGuild.IsEnemy(targetGuild))
                        return Notoriety.Enemy;
                }

                var srcFaction = Faction.Find(source, true, true);
                var trgFaction = Faction.Find(target.Owner, true, true);

                if (srcFaction != null && trgFaction != null && srcFaction != trgFaction && source.Map == Faction.Facet)
                {
                    foreach (var m in target.Aggressors)
                    {
                        if (m == source || m is BaseFactionGuard)
                            return Notoriety.Enemy;
                    }
                }

                if (CheckHouseFlag(source, target.Owner, target.Location, target.Map))
                    return Notoriety.CanBeAttacked;

                if (!(target.Owner is PlayerMobile) && !IsPet(target.Owner as BaseCreature))
                    return Notoriety.CanBeAttacked;

                var list = target.Aggressors;

                foreach (var m in list)
                {
                    if (m == source)
                        return Notoriety.CanBeAttacked;
                }

                return Notoriety.Innocent;
            }
        }

  

        public static int MobileNotoriety(Mobile source, IDamageable damageable)
        {
            if (damageable is PublicMoongate)
            {
                return Notoriety.Innocent;
            }



            var target = damageable as Mobile;

            if (target == null)
                return Notoriety.CanBeAttacked;

            if (target.Blessed)
                return Notoriety.Invulnerable;

            if (target is BaseVendor && ((BaseVendor)target).IsInvulnerable)
                return Notoriety.Invulnerable;

            if (target is PlayerVendor || target is TownCrier)
                return Notoriety.Invulnerable;


            var context = EnemyOfOneSpell.GetContext(source);

            if (context != null && context.IsEnemy(target))
                return Notoriety.Enemy;

            if (PVPArenaSystem.IsEnemy(source, target))
                return Notoriety.Enemy;

            if (PVPArenaSystem.IsFriendly(source, target))
                return Notoriety.Ally;

            //if (target.IsStaff())
            //    return Notoriety.CanBeAttacked;

            if (source.Player && target is BaseCreature)
            {
                var bc = (BaseCreature)target;

                var master = bc.GetMaster();

                if (master != null && master == source)
                    return Notoriety.CanBeAttacked;

                master = bc.ControlMaster;

                if (master != null && !bc.ForceNotoriety)
                {
                    if (source == master && CheckAggressor(target.Aggressors, source))
                    {
                        return Notoriety.CanBeAttacked;
                    }

                    if (CheckAggressor(source.Aggressors, bc))
                        return Notoriety.CanBeAttacked;

                    Shard.Debug("Vendo noto do master da criatura");

                    return MobileNotoriety(source, master);
                }
            }

            if (target.Murderer)
                return Notoriety.Murderer;

            if (target.Body.IsMonster && IsSummoned(target as BaseCreature))
            {
                if (!(target is BaseFamiliar) && !(target is ArcaneFey) && !(target is Golem))
                    return Notoriety.Murderer;
            }

            if (target is BaseCreature)
            {
                if (((BaseCreature)target).AlwaysMurderer || ((BaseCreature)target).IsAnimatedDead)
                    return Notoriety.Murderer;
            }

            if (source.Player && target is BaseEscort)
                return Notoriety.Innocent;

            if (target.Criminal)
                return Notoriety.Criminal;

            var sourceGuild = GetGuildFor(source.Guild as Guild, source);
            var targetGuild = GetGuildFor(target.Guild as Guild, target);

            if (sourceGuild != null && targetGuild != null)
            {
                if (sourceGuild == targetGuild)
                    return Notoriety.Ally;

                if (sourceGuild.IsAlly(targetGuild))
                    return Notoriety.Ally;

                if (sourceGuild.IsEnemy(targetGuild))
                    return Notoriety.Enemy;
            }

            if (Settings.Enabled)
            {
                var srcFaction = Faction.Find(source, true, true);
                var trgFaction = Faction.Find(target, true, true);

                if (srcFaction != null && trgFaction != null && srcFaction != trgFaction && source.Map == Faction.Facet)
                    return Notoriety.Enemy;
            }

            if (ViceVsVirtueSystem.Enabled && ViceVsVirtueSystem.IsEnemy(source, target) && (ViceVsVirtueSystem.EnhancedRules || source.Map == Faction.Facet))
                return Notoriety.Enemy;

            if (Stealing.ClassicMode && target is PlayerMobile && ((PlayerMobile)target).PermaFlags.Contains(source))
                return Notoriety.CanBeAttacked;

            if (target is BaseCreature && ((BaseCreature)target).AlwaysAttackable)
                return Notoriety.CanBeAttacked;

            if (CheckHouseFlag(source, target, target.Location, target.Map))
                return Notoriety.CanBeAttacked;

            //If Target is NOT A baseCreature, OR it's a BC and the BC is initial innocent...
            if (!(target is BaseCreature && ((BaseCreature)target).InitialInnocent))
            {
                if (!target.Body.IsHuman && !target.Body.IsGhost && !IsPet(target as BaseCreature) && !(target is PlayerMobile))
                    return Notoriety.CanBeAttacked;

                if (!Core.ML && !target.CanBeginAction(typeof(PolymorphSpell)))
                    return Notoriety.CanBeAttacked;
            }

            if (CheckAggressor(source.Aggressors, target))
                return Notoriety.CanBeAttacked;

            if (source is PlayerMobile && CheckPetAggressor((PlayerMobile)source, target))
                return Notoriety.CanBeAttacked;

            if (CheckAggressed(source.Aggressed, target))
                return Notoriety.CanBeAttacked;

            if (source is PlayerMobile && CheckPetAggressed((PlayerMobile)source, target))
                return Notoriety.CanBeAttacked;

            if (target is BaseCreature)
            {
                var bc = (BaseCreature)target;

                if (bc.Controlled && bc.ControlOrder == OrderType.Guard && bc.ControlTarget == source)
                    return Notoriety.CanBeAttacked;
            }

            if (source is BaseCreature)
            {
                var bc = (BaseCreature)source;
                var master = bc.GetMaster();

                if (master != null)
                {
                    if (CheckAggressor(master.Aggressors, target))
                    {
                        return Notoriety.CanBeAttacked;
                    }

                    if (MobileNotoriety(master, target) == Notoriety.CanBeAttacked)
                    {
                        return Notoriety.CanBeAttacked;
                    }

                    if (target is BaseCreature)
                    {
                        return Notoriety.CanBeAttacked;
                    }
                }
            }

            return Notoriety.Innocent;
        }

        public static bool CheckHouseFlag(Mobile from, Mobile m, Point3D p, Map map)
        {
            var house = BaseHouse.FindHouseAt(p, map, 16);

            if (house == null || house.Public || !house.IsFriend(from))
                return false;

            if (m != null && house.IsFriend(m))
                return false;

            var c = m as BaseCreature;

            if (c != null && !c.Deleted && c.Controlled && c.ControlMaster != null)
                return !house.IsFriend(c.ControlMaster);

            return true;
        }

        public static bool IsPet(BaseCreature c)
        {
            return (c != null && c.Controlled);
        }

        public static bool IsSummoned(BaseCreature c)
        {
            return (c != null && /*c.Controlled &&*/ c.Summoned);
        }

        public static bool CheckAggressor(List<AggressorInfo> list, Mobile target)
        {
            foreach (var o in list)
            {
                if (o.Attacker == target)
                    return true;
            }

            return false;
        }

        public static bool CheckAggressed(List<AggressorInfo> list, Mobile target)
        {
            foreach (var info in list)
            {
                if (!info.CriminalAggression && info.Defender == target)
                    return true;
            }

            return false;
        }

        public static bool CheckPetAggressor(PlayerMobile source, Mobile target)
        {
            foreach (var bc in source.AllFollowers)
            {
                if (CheckAggressor(bc.Aggressors, target))
                    return true;
            }

            return false;
        }

        public static bool CheckPetAggressed(PlayerMobile source, Mobile target)
        {
            foreach (var bc in source.AllFollowers)
            {
                if (CheckAggressed(bc.Aggressed, target))
                    return true;
            }

            return false;
        }
    }
}
