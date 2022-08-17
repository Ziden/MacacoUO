using System;
using Server.Factions;
using Server.Mobiles;
using Server.Multis;
using Server.Targeting;
using Server.Engines.VvV;
using Server.Items;
using System.Collections.Generic;
using System.Linq;
using Server.Spells;

namespace Server.Items
{
    public interface IRevealableItem
    {
        bool CheckReveal(Mobile m);
        bool CheckPassiveDetect(Mobile m);
        void OnRevealed(Mobile m);

        bool CheckWhenHidden { get; }
    }
}

namespace Server.SkillHandlers
{
    public class DetectHidden
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.DetectHidden].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile src)
        {
            src.RevealingAction();
            src.SendLocalizedMessage(500819);//Where will you search?
            src.Target = new InternalTarget();

            return TimeSpan.FromSeconds(10.0);
        }

        public class InternalTarget : Target
        {
            public InternalTarget()
                : base(12, true, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile src, object targ)
            {
                bool foundAnyone = false;

                if(src.Player)
                    src.OverheadMessage("* procurando *");

                Point3D p;
                if (targ is Mobile)
                    p = ((Mobile)targ).Location;
                else if (targ is Item)
                    p = ((Item)targ).Location;
                else if (targ is IPoint3D)
                    p = new Point3D((IPoint3D)targ);
                else
                    p = src.Location;

                double srcSkill = src.Skills[SkillName.DetectHidden].Value + 10;
                int range = Math.Max(2, (int)(srcSkill / 10.0));

                if (!src.CheckSkillMult(SkillName.DetectHidden, 0.0, 100.0))
                    range /= 2;

                src.RevealingAction();

                if(Shard.DebugEnabled)
                Shard.Debug("Range Detect " + range);

                int found = 0;
                if (range > 0)
                {
                    IPooledEnumerable inRange = src.Map.GetMobilesInRange(p, range);

                    foreach (Mobile trg in inRange)
                    {
                        if (trg.Hidden && src != trg)
                        {
                            var bc = src as BaseCreature;
                            if (bc != null && bc.Controlled && bc.ControlMaster == trg)
                            {
                                continue;
                            }

                            double ss = srcSkill + Utility.Random(29) - 10;
                            double ts = trg.Skills[SkillName.Hiding].Value + Utility.Random(21) - 10;
                            double shadow = Server.Spells.SkillMasteries.ShadowSpell.GetDifficultyFactor(trg);

                            Shard.Debug("Detect Points: " + ss + " HidePoints " + ts);

                            var distance = (int)trg.GetDistanceToSqrt(p);
                            if(distance <= 3)
                            {
                                ss += 50;
                            }

                            if (src.AccessLevel >= trg.AccessLevel && (ss >= ts) && Utility.RandomDouble() > shadow)
                            {
                                if ((trg is ShadowKnight && (trg.X != p.X || trg.Y != p.Y)) ||
                                     (!CanDetect(src, trg)))
                                    continue;

                                if(trg.Hidden)
                                    trg.SendMessage("Voce foi revelado"); // You have been revealed!
                                trg.RevealingAction();
                                foundAnyone = true;
                            }
                        }
                    }

                    inRange.Free();

                    IPooledEnumerable itemsInRange = src.Map.GetItemsInRange(p, range);

                    foreach (Item item in itemsInRange)
                    {
                        if (item is LibraryBookcase && Server.Engines.Khaldun.GoingGumshoeQuest3.CheckBookcase(src, item))
                        {
                            foundAnyone = true;
                        }
                        else
                        {
                            IRevealableItem dItem = item as IRevealableItem;

                            if (dItem == null || (item.Visible && dItem.CheckWhenHidden))
                                continue;

                            if (dItem.CheckReveal(src))
                            {
                                dItem.OnRevealed(src);
                                found++;
                                foundAnyone = true;
                            }
                        }
                    }

                    itemsInRange.Free();
                }

                if(!foundAnyone)
                {
                    src.SendMessage("Voce nao encontrou nada");
                } else
                {
                    src.SendMessage("Voce encontrou o alvo");
                }
            }
        }

        public static void DoPassiveDetect(Mobile src)
        {
            if (src == null || src.Map == null || src.Location == Point3D.Zero)
                return;

            double ss = src.Skills[SkillName.DetectHidden].Value*1.5 + src.Skills[SkillName.Tracking].Value;
            Shard.Debug(" Detect skills: " + ss);
            if (ss <= 0)
                return;

            IPooledEnumerable eable = src.Map.GetMobilesInRange(src.Location, 4);

            if (eable == null)
                return;

            foreach (Mobile m in eable)
            {
                if (m == null || m == src || m is ShadowKnight || !CanDetect(src, m))
                    continue;

                double ts = m.Skills[SkillName.Hiding].Value + m.Skills[SkillName.Stealth].Value/4;

                var distance = (int)m.GetDistanceToSqrt(src.Location);
                var chance = (ss - ts) + (41- distance*10);

                Shard.Debug("Chance detect: " + chance + " distancia " + distance);
                if (src.AccessLevel >= m.AccessLevel && Utility.Random(200) < chance)
                {
                    if(m.Hidden)
                        m.SendMessage("Voce foi revelado"); // You have been revealed!
                    m.RevealingAction(true);

                }
            }

            eable.Free();

            eable = src.Map.GetItemsInRange(src.Location, 8);

            if (ss > 50)
            {
                foreach (Item item in eable)
                {
                    if (!item.Visible && item is IRevealableItem && ((IRevealableItem)item).CheckPassiveDetect(src))
                    {
                        src.SendMessage("Voce sente que existe algo escondido por esta area"); // Your keen senses detect something hidden in the area...
                    }
                }
            }


            eable.Free();
        }

        public static bool CanDetect(Mobile src, Mobile target)
        {
            if (src.Map == null || target.Map == null || !src.CanBeHarmful(target, false))
                return false;

            // No invulnerable NPC's
            if (src.Blessed || (src is BaseCreature && ((BaseCreature)src).IsInvulnerable))
                return false;

            if (target.Blessed || (target is BaseCreature && ((BaseCreature)target).IsInvulnerable))
                return false;

            // Checked aggressed/aggressors
            if (src.Aggressed.Any(x => x.Defender == target) || src.Aggressors.Any(x => x.Attacker == target))
                return true;

            // Follow the same rules as indirect spells such as wither
            return /*src.Map.Rules == MapRules.FeluccaRules ||*/Server.Spells.SpellHelper.ValidIndirectTarget(target, src, true,true);
        }
    }
}
