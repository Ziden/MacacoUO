
#region References
using System;
using System.Linq;
using Server.Engines.Quests;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Spells.SkillMasteries;
#endregion

namespace Server.Misc
{
    public class SkillCheck
    {
        private static readonly TimeSpan _StatGainDelay;
        private static readonly TimeSpan _PetStatGainDelay;

        private static readonly int _PlayerChanceToGainStats;
        private static readonly int _PetChanceToGainStats;

        private static readonly bool _AntiMacroCode;

        /// <summary>
        ///     How long do we remember targets/locations?
        /// </summary>
        public static TimeSpan AntiMacroExpire = TimeSpan.FromMinutes(1.0);

        /// <summary>
        ///     How many times may we use the same location/target for gain
        /// </summary>
        public const int Allowance = 3;

        /// <summary>
        ///     The size of each location, make this smaller so players dont have to move as far
        /// </summary>
        private const int LocationSize = 4;

        public static bool GGSActive { get { return !Siege.SiegeShard; } }

        static SkillCheck()
        {
            _AntiMacroCode = false;//Config.Get("PlayerCaps.EnableAntiMacro", false);

            _StatGainDelay = Config.Get("PlayerCaps.PlayerStatTimeDelay", TimeSpan.FromMinutes(15.0));
            _PetStatGainDelay = Config.Get("PlayerCaps.PetStatTimeDelay", TimeSpan.FromMinutes(5.0));

            _PlayerChanceToGainStats = 7;//Config.Get("PlayerCaps.PlayerChanceToGainStats", 5);
            _PetChanceToGainStats = Config.Get("PlayerCaps.PetChanceToGainStats", 5);

            //if (!Config.Get("PlayerCaps.EnablePlayerStatTimeDelay", false))
                _StatGainDelay = TimeSpan.FromSeconds(0.5);

            if (!Config.Get("PlayerCaps.EnablePetStatTimeDelay", false))
                _PetStatGainDelay = TimeSpan.FromSeconds(0.5);
        }

        private static readonly bool[] UseAntiMacro =
        {
			// true if this skill uses the anti-macro code, false if it does not
			false, // Alchemy = 0,
			true, // Anatomy = 1,
			true, // AnimalLore = 2,
			true, // ItemID = 3,
			true, // ArmsLore = 4,
			false, // Parry = 5,
			true, // Begging = 6,
			false, // Blacksmith = 7,
			false, // Fletching = 8,
			true, // Peacemaking = 9,
			true, // Camping = 10,
			false, // Carpentry = 11,
			false, // Cartography = 12,
			false, // Cooking = 13,
			true, // DetectHidden = 14,
			true, // Discordance = 15,
			true, // EvalInt = 16,
			true, // Healing = 17,
			true, // Fishing = 18,
			true, // Forensics = 19,
			true, // Herding = 20,
			true, // Hiding = 21,
			true, // Provocation = 22,
			false, // Inscribe = 23,
			true, // Lockpicking = 24,
			true, // Magery = 25,
			true, // MagicResist = 26,
			false, // Tactics = 27,
			true, // Snooping = 28,
			true, // Musicianship = 29,
			true, // Poisoning = 30,
			false, // Archery = 31,
			true, // SpiritSpeak = 32,
			true, // Stealing = 33,
			false, // Tailoring = 34,
			true, // AnimalTaming = 35,
			true, // TasteID = 36,
			false, // Tinkering = 37,
			true, // Tracking = 38,
			true, // Veterinary = 39,
			false, // Swords = 40,
			false, // Macing = 41,
			false, // Fencing = 42,
			false, // Wrestling = 43,
			true, // Lumberjacking = 44,
			true, // Mining = 45,
			true, // Meditation = 46,
			true, // Stealth = 47,
			true, // RemoveTrap = 48,
			true, // Necromancy = 49,
			false, // Focus = 50,
			true, // Chivalry = 51
			true, // Bushido = 52
			true, //Ninjitsu = 53
			true, // Spellweaving = 54

			#region Stygian Abyss
			true, // Mysticism = 55
			true, // Imbuing = 56
			false // Throwing = 57
			#endregion
		};

        public static void Initialize()
        {
            Mobile.SkillCheckLocationHandler = XmlSpawnerSkillCheck.Mobile_SkillCheckLocation;
            Mobile.SkillCheckDirectLocationHandler = XmlSpawnerSkillCheck.Mobile_SkillCheckDirectLocation;

            Mobile.SkillCheckTargetHandler = XmlSpawnerSkillCheck.Mobile_SkillCheckTarget;
            Mobile.SkillCheckDirectTargetHandler = XmlSpawnerSkillCheck.Mobile_SkillCheckDirectTarget;
        }

        public static bool Mobile_SkillCheckLocation(Mobile from, SkillName skillName, double minSkill, double maxSkill, double mult)
        {
            var skill = from.Skills[skillName];

            if (skill == null)
                return false;

            var value = skill.Value;

            //TODO: Is there any other place this can go?
            if (skillName == SkillName.Fishing && BaseGalleon.FindGalleonAt(from, from.Map) is TokunoGalleon)
                value += 1;

            if (value < minSkill)
                return false; // Too difficult

            if (value >= maxSkill)
                return true; // No challenge

            var chance = (value - minSkill) / (maxSkill - minSkill);

            CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

            var loc = new Point2D(from.Location.X / LocationSize, from.Location.Y / LocationSize);

            return CheckSkill(from, skill, loc, chance, mult);
        }

        public static bool Mobile_SkillCheckDirectLocation(Mobile from, SkillName skillName, double chance, double mult)
        {
            var skill = from.Skills[skillName];

            if (skill == null)
                return false;

            CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

            if (chance < 0.0)
                return false; // Too difficult

            if (chance >= 1.0)
                return true; // No challenge

            var loc = new Point2D(from.Location.X / LocationSize, from.Location.Y / LocationSize);


            return CheckSkill(from, skill, loc, chance, mult);
        }

        public static double GC_INICIAL = 0.78;

        public static SkillName[] Work = new SkillName[] {
            SkillName.Alchemy, SkillName.Blacksmith, SkillName.Bowcraft,
            SkillName.Mining, SkillName.Tinkering, SkillName.Carpentry,
            SkillName.TasteID, SkillName.Cooking, SkillName.Tailoring,
            SkillName.AnimalTaming, SkillName.Fishing, SkillName.Lumberjacking
        };

        public static SkillName[] Craft = new SkillName[] {
            SkillName.Alchemy, SkillName.Blacksmith, SkillName.Bowcraft,
            SkillName.Tinkering, SkillName.Carpentry,
            SkillName.Cooking, SkillName.Tailoring,
        };

        public static double BONUS_GERAL = 0;

        public static double GetExp(double skill, double skillDifficulty, bool work, bool pvm, bool craft, double gcBonus = 0)
        {
            if(skillDifficulty== SkillInfo.EASY)
            {
                return 1;
            }

            // Formuleta simplona de up
            var gc = GC_INICIAL + (gcBonus * 4);
            Shard.Debug("GC INICIAL: " + gc);
            if(skillDifficulty==SkillInfo.HARD)
            {
                gc -= 0.05;
            }
            var ratio = 1.00001 - (skill / 100);
            gc *= ratio * (ratio * 4);
            
            gc *= skillDifficulty;

            if(!work && pvm)
            {
                if (gc < 0.05)
                    gc = 0.05;
            } else
            {
                if (craft)
                    gc /= 10;

                if (skillDifficulty == SkillInfo.COMBAT && skill >= 97)
                    gc /= 2;

                if (gc < 0.0015)
                    gc = 0.0015;

                if (skillDifficulty == SkillInfo.EASY)
                {
                    if (gc < 0.008)
                        gc = 0.008;
                }
                if (skillDifficulty == SkillInfo.MEDIUM)
                {
                    if (gc < 0.006)
                        gc = 0.006;
                }
                if (skillDifficulty == SkillInfo.COMBAT)
                {
                    if (skill < 80 && gc < 0.01)
                        gc = 0.01;
                    else if (skill < 90 && gc < 0.006)
                        gc = 0.006;
                }
            }

            if (skill < 50)
                gc *= 3;

            return gc;
        }

        static bool work = false;

        public static bool CheckSkill(Mobile from, Skill skill, object amObj, double chance, double mult)
        {
            if (Shard.DebugEnabled)
                Shard.Debug("Check skill " + skill.SkillName+" mult=" +mult, from);

            if (from.Skills.Cap == 0)
                return false;

            if(Shard.SPHERE_STYLE && !Work.Contains(skill.SkillName))
            {
                Shard.Debug("Gain easy");
                Gain(from, skill);
                return Utility.RandomDouble() <= chance;
            }

            var success = Utility.RandomDouble() <= chance;

            if (mult == 0)
                return success;

            if(Shard.AVENTURA)
            {
                Gain(from, skill, 5);
                return success;
            }

            if (Shard.RP && ExpGumpRP.UpComXP.Contains(skill.SkillName))
                return success;

            // monstros nao tamados nao upam skill
            if (from is BaseCreature && ((BaseCreature)from).GetMaster() == null)
                return success;

            var dificuldade = skill.Info.GainFactor;


            work = Work.Any(s => s == skill.SkillName);
            var craft = Craft.Any(s => s == skill.SkillName);

            var gcBonus = 0.0;

          
            if (!from.Player)
            {
                if (dificuldade < 0.5)
                    dificuldade = 0.5;
            } 

            var gc = GetExp(skill.Value, dificuldade, work, craft, false, gcBonus) * mult;

            if(BONUS_GERAL > 0)
            {
                gc *= BONUS_GERAL;
            }

            while(gc > 1 && AllowGain(from, skill, amObj))
            {
                Gain(from, skill);
                gc -= 1;
            }

            if(Shard.DebugEnabled)
                Shard.Debug("MULT: " + mult + " GC BONUS " + gcBonus+" GC "+gc);

            if (AllowGain(from, skill, amObj))
            {
                if(from is PlayerMobile)
                {
                    var player = (PlayerMobile)from;
                    if(player.Alive)
                    {
                        var expGanha = Math.Round(gc, 5) * 1000;
                        //Shard.Debug("Ganhando " + exp / 10 + "% Exp - GC Original " + gc, from);
                        if (skill.IncreaseExp((ushort)expGanha))
                        {
                            Gain(from, skill);
                        } else{
                            if (Shard.DebugGritando)
                            {
                                var xp = skill.GetExp();
                                Shard.Debug("EXP " + skill.SkillName + " +" + expGanha + " " +xp+"/100%", from);
                            }
                        }
                    }
                } else
                {
                    if (from.Alive && (skill.Base < 10.0 || Utility.RandomDouble() <= gc || CheckGGS(from, skill)))
                    {
                        Gain(from, skill);
                    }
                }
              
            
            }
            else
            {
                //Z.Debug("NO GAIN");
            }

            return success;
        }

        public static bool Mobile_SkillCheckTarget(
            Mobile from,
            SkillName skillName,
            object target,
            double minSkill,
            double maxSkill,
            double mult)
        {
            var skill = from.Skills[skillName];

            if (skill == null)
                return false;

            var value = skill.Value;

            if (value < minSkill)
                return false; // Too difficult

            if (value >= maxSkill)
                return true; // No challenge

            var chance = (value - minSkill) / (maxSkill - minSkill);

            CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

            return CheckSkill(from, skill, target, chance, mult);
        }

        public static bool Mobile_SkillCheckDirectTarget(Mobile from, SkillName skillName, object target, double chance, double mult)
        {
            var skill = from.Skills[skillName];

            if (skill == null)
                return false;

            CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

            if (chance < 0.0)
                return false; // Too difficult

            if (chance >= 1.0)
                return true; // No challenge

            return CheckSkill(from, skill, target, chance, mult);
        }

        private static bool AllowGain(Mobile from, Skill skill, object obj)
        {
            if (Core.AOS && Faction.InSkillLoss(from)) //Changed some time between the introduction of AoS and SE.
                return false;

            if (from is PlayerMobile)
            {
                #region SA
                if (skill.Info.SkillID == (int)SkillName.Archery && from.Race == Race.Gargoyle)
                    return false;

                if (skill.Info.SkillID == (int)SkillName.Throwing && @from.Race != Race.Gargoyle)
                    return false;
                #endregion

                if (_AntiMacroCode && UseAntiMacro[skill.Info.SkillID])
                    return ((PlayerMobile)from).AntiMacroCheck(skill, obj);
            }
            return true;
        }

        public enum Stat
        {
            Str,
            Dex,
            Int
        }

        public static void Gain(Mobile from, Skill skill, int amt = 1)
        {
            if (Shard.DebugEnabled)
                Shard.Debug("Ganhando pontos " + amt + " em " + skill.Name, from);

            if (from.Region.IsPartOf<Jail>())
                return;

            if (from is BaseCreature && ((BaseCreature)from).IsDeadPet)
                return;

            if (skill.SkillName == SkillName.Focus && from is BaseCreature &&
                (!PetTrainingHelper.Enabled || !((BaseCreature)from).Controlled))
                return;

            if (skill.Base < skill.Cap && skill.Lock == SkillLock.Up)
            {
                var toGain = amt;
                var skills = from.Skills;

                if (from is PlayerMobile && Siege.SiegeShard)
                {
                    var minsPerGain = Siege.MinutesPerGain(from, skill);

                    if (minsPerGain > 0)
                    {
                        if (Siege.CheckSkillGain((PlayerMobile)from, minsPerGain, skill))
                        {
                            CheckReduceSkill(skills, toGain, skill);

                            if (skills.Total + toGain <= skills.Cap)
                            {
                                skill.BaseFixedPoint += toGain;
                            }
                        }

                        return;
                    }
                }

                if (skill.Base <= 10.0 && amt==1)
                    toGain = Utility.Random(4) + 1;

                #region Mondain's Legacy
                if (from is PlayerMobile && QuestHelper.EnhancedSkill((PlayerMobile)from, skill))
                {
                    toGain *= Utility.RandomMinMax(2, 4);
                }
                #endregion

                #region Scroll of Alacrity
                if (from is PlayerMobile && skill.SkillName == ((PlayerMobile)from).AcceleratedSkill &&
                    ((PlayerMobile)from).AcceleratedStart > DateTime.UtcNow)
                {
                    // You are infused with intense energy. You are under the effects of an accelerated skillgain scroll.
                    ((PlayerMobile)from).SendLocalizedMessage(1077956);

                    toGain += Utility.RandomMinMax(2, 5);
                }
                #endregion

                #region Skill Masteries
                else if (from is BaseCreature && (((BaseCreature)from).Controlled || ((BaseCreature)from).Summoned))
                {
                    var master = ((BaseCreature)from).GetMaster();

                    if (master != null)
                    {
                        var spell = SkillMasterySpell.GetSpell(master, typeof(WhisperingSpell)) as WhisperingSpell;

                        if (spell != null && master.InRange(from.Location, spell.PartyRange) && master.Map == from.Map &&
                            spell.EnhancedGainChance >= Utility.Random(100))
                        {
                            toGain = Utility.RandomMinMax(2, 5);
                        }
                    }
                }
                #endregion

                if (from is PlayerMobile)
                {
                    CheckReduceSkill(skills, toGain, skill);
                }

                if (!from.Player || (skills.Total + toGain <= skills.Cap))
                {
                    skill.BaseFixedPoint = Math.Min(skill.CapFixedPoint, skill.BaseFixedPoint + toGain);

                    EventSink.InvokeSkillGain(new SkillGainEventArgs(from, skill, toGain));

                    if (from is PlayerMobile)
                        UpdateGGS(from, skill);
                }
            }

            #region Mondain's Legacy
            if (from is PlayerMobile)
                QuestHelper.CheckSkill((PlayerMobile)from, skill);
            #endregion

            if (skill.Lock == SkillLock.Up &&
                (!Siege.SiegeShard || !(from is PlayerMobile) || Siege.CanGainStat((PlayerMobile)from)))
            {
                var info = skill.Info;

                // Old gain mechanic
                if (!Core.ML)
                {
                    var scalar = 1.2;

                    if (from.StrLock == StatLockType.Up && (info.StrGain / 33.3) * scalar > Utility.RandomDouble())
                        GainStat(from, Stat.Str);
                    else if (from.DexLock == StatLockType.Up && (info.DexGain / 33.3) * scalar > Utility.RandomDouble())
                        GainStat(from, Stat.Dex);
                    else if (from.IntLock == StatLockType.Up && (info.IntGain / 33.3) * scalar > Utility.RandomDouble())
                        GainStat(from, Stat.Int);
                }
                else
                {
                    TryStatGain(info, from);
                }
            }
        }

        private static void CheckReduceSkill(Skills skills, int toGain, Skill gainSKill)
        {
            Shard.Debug("Tentando baixar skill");
            if (skills.Total + toGain / skills.Cap >= Utility.RandomDouble())
            {
                Shard.Debug("Passou random estranho");
                foreach (var toLower in skills)
                {
                    if(Shard.DebugEnabled && toLower.Lock == SkillLock.Down)
                        Shard.Debug("Tentando baixar "+ toLower.Name+" - "+ (toLower.BaseFixedPoint >= toGain));
                    if (toLower != gainSKill && toLower.Lock == SkillLock.Down && toLower.BaseFixedPoint >= toGain)
                    {
                        toLower.BaseFixedPoint -= toGain;
                        break;
                    }
                }
            }
        }

        public static void TryStatGain(SkillInfo info, Mobile from)
        {
            // Chance roll
            double chance;

            if (from is BaseCreature && ((BaseCreature)from).Controlled)
            {
                if (PetTrainingHelper.Enabled)
                {
                    chance = 0.0;
                }
                else
                {
                    chance = _PetChanceToGainStats / 100.0;
                }
            }
            else
            {
                chance = _PlayerChanceToGainStats / 100.0;
            }

            if (Utility.RandomDouble() >= chance)
            {
                return;
            }

            // Selection
            var primaryLock = StatLockType.Locked;
            var secondaryLock = StatLockType.Locked;

            switch (info.Primary)
            {
                case StatCode.Str:
                    primaryLock = from.StrLock;
                    break;
                case StatCode.Dex:
                    primaryLock = from.DexLock;
                    break;
                case StatCode.Int:
                    primaryLock = from.IntLock;
                    break;
            }

            switch (info.Secondary)
            {
                case StatCode.Str:
                    secondaryLock = from.StrLock;
                    break;
                case StatCode.Dex:
                    secondaryLock = from.DexLock;
                    break;
                case StatCode.Int:
                    secondaryLock = from.IntLock;
                    break;
            }

            // Gain
            // Decision block of both are selected to gain
            if (primaryLock == StatLockType.Up && secondaryLock == StatLockType.Up)
            {
                if (Utility.Random(4) == 0)
                    GainStat(from, (Stat)info.Secondary);
                else
                    GainStat(from, (Stat)info.Primary);
            }
            else // Will not do anything if neither are selected to gain
            {
                if (primaryLock == StatLockType.Up)
                    GainStat(from, (Stat)info.Primary);
                else if (secondaryLock == StatLockType.Up)
                    GainStat(from, (Stat)info.Secondary);
            }
        }

        public static bool CanLower(Mobile from, Stat stat)
        {
            switch (stat)
            {
                case Stat.Str:
                    return (from.StrLock == StatLockType.Down && from.RawStr > 10);
                case Stat.Dex:
                    return (from.DexLock == StatLockType.Down && from.RawDex > 10);
                case Stat.Int:
                    return (from.IntLock == StatLockType.Down && from.RawInt > 10);
            }

            return false;
        }

        public static bool CanRaise(Mobile from, Stat stat, bool atTotalCap)
        {
            switch (stat)
            {
                case Stat.Str:
                    if (from.RawStr < from.StrCap)
                    {
                        if (atTotalCap && from is PlayerMobile)
                        {
                            return CanLower(from, Stat.Dex) || CanLower(from, Stat.Int);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
                case Stat.Dex:
                    if (from.RawDex < from.DexCap)
                    {
                        if (atTotalCap && from is PlayerMobile)
                        {
                            return CanLower(from, Stat.Str) || CanLower(from, Stat.Int);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
                case Stat.Int:
                    if (from.RawInt < from.IntCap)
                    {
                        if (atTotalCap && from is PlayerMobile)
                        {
                            return CanLower(from, Stat.Str) || CanLower(from, Stat.Dex);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
            }

            return false;
        }

        public static void IncreaseStat(Mobile from, Stat stat)
        {
            bool atTotalCap = from.RawStatTotal >= from.StatCap;

            switch (stat)
            {
                case Stat.Str:
                    {
                        if (CanRaise(from, Stat.Str, atTotalCap))
                        {
                            if (atTotalCap)
                            {
                                if (CanLower(from, Stat.Dex) && (from.RawDex < from.RawInt || !CanLower(from, Stat.Int)))
                                    --from.RawDex;
                                else if (CanLower(from, Stat.Int))
                                    --from.RawInt;
                            }

                            ++from.RawStr;

                            if (Siege.SiegeShard && from is PlayerMobile)
                            {
                                Siege.IncreaseStat((PlayerMobile)from);
                            }
                        }

                        break;
                    }
                case Stat.Dex:
                    {
                        if (CanRaise(from, Stat.Dex, atTotalCap))
                        {
                            if (atTotalCap)
                            {
                                if (CanLower(from, Stat.Str) && (from.RawStr < from.RawInt || !CanLower(from, Stat.Int)))
                                    --from.RawStr;
                                else if (CanLower(from, Stat.Int))
                                    --from.RawInt;
                            }

                            ++from.RawDex;
                            if(from.RawDex == 80)
                            {
                                from.SendMessage(78, "Agora que voce tem 80 dex, voce pode usar habilidades de armas !!");
                            }

                            if (Siege.SiegeShard && from is PlayerMobile)
                            {
                                Siege.IncreaseStat((PlayerMobile)from);
                            }
                        }

                        break;
                    }
                case Stat.Int:
                    {
                        if (CanRaise(from, Stat.Int, atTotalCap))
                        {
                            if (atTotalCap)
                            {
                                if (CanLower(from, Stat.Str) && (from.RawStr < from.RawDex || !CanLower(from, Stat.Dex)))
                                    --from.RawStr;
                                else if (CanLower(from, Stat.Dex))
                                    --from.RawDex;
                            }

                            ++from.RawInt;

                            if (Siege.SiegeShard && from is PlayerMobile)
                            {
                                Siege.IncreaseStat((PlayerMobile)from);
                            }
                        }

                        break;
                    }
            }
        }

        public static void GainStat(Mobile from, Stat stat)
        {
            if (!CheckStatTimer(from, stat))
                return;

            IncreaseStat(from, stat);
        }

        public static bool CheckStatTimer(Mobile from, Stat stat)
        {
            switch (stat)
            {
                case Stat.Str:
                    {
                        if (from is BaseCreature && ((BaseCreature)from).Controlled)
                        {
                            if ((from.LastStrGain + _PetStatGainDelay) >= DateTime.UtcNow)
                                return false;
                        }
                        else if ((from.LastStrGain + _StatGainDelay) >= DateTime.UtcNow)
                            return false;

                        from.LastStrGain = DateTime.UtcNow;
                        break;
                    }
                case Stat.Dex:
                    {
                        if (from is BaseCreature && ((BaseCreature)from).Controlled)
                        {
                            if ((from.LastDexGain + _PetStatGainDelay) >= DateTime.UtcNow)
                                return false;
                        }
                        else if ((from.LastDexGain + _StatGainDelay) >= DateTime.UtcNow)
                            return false;

                        from.LastDexGain = DateTime.UtcNow;
                        break;
                    }
                case Stat.Int:
                    {
                        if (from is BaseCreature && ((BaseCreature)from).Controlled)
                        {
                            if ((from.LastIntGain + _PetStatGainDelay) >= DateTime.UtcNow)
                                return false;
                        }
                        else if ((from.LastIntGain + _StatGainDelay) >= DateTime.UtcNow)
                            return false;

                        from.LastIntGain = DateTime.UtcNow;
                        break;
                    }
            }
            return true;
        }

        private static bool CheckGGS(Mobile from, Skill skill)
        {
            if (!GGSActive)
                return false;

            if (from is PlayerMobile && skill.NextGGSGain < DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }

        public static void UpdateGGS(Mobile from, Skill skill)
        {
            if (!GGSActive)
                return;

            var list = (int)Math.Min(GGSTable.Length - 1, skill.Base / 5);
            var column = from.Skills.Total >= 7000 ? 2 : from.Skills.Total >= 3500 ? 1 : 0;

            skill.NextGGSGain = DateTime.UtcNow + TimeSpan.FromMinutes(GGSTable[list][column]);
        }

        private static readonly int[][] GGSTable =
        {
            new[] {1, 3, 5}, // 0.0 - 4.9
			new[] {4, 10, 18}, new[] {7, 17, 30}, new[] {9, 24, 44}, new[] {12, 31, 57}, new[] {14, 38, 90}, new[] {17, 45, 84},
            new[] {20, 52, 96}, new[] {23, 60, 106}, new[] {25, 66, 120}, new[] {27, 72, 138}, new[] {33, 90, 162},
            new[] {55, 150, 264}, new[] {78, 216, 390}, new[] {114, 294, 540}, new[] {144, 384, 708}, new[] {180, 492, 900},
            new[] {228, 606, 1116}, new[] {276, 744, 1356}, new[] {336, 894, 1620}, new[] {396, 1056, 1920},
            new[] {468, 1242, 2280}, new[] {540, 1440, 2580}, new[] {618, 1662, 3060}
        };
    }
}
