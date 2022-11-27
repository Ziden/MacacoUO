

using System;
using Server.Network;
using Server.Commands;
using System.Collections.Generic;
using Server.Engines.Points;
using Server.Misc;
using Server.Mobiles;

namespace Server.Gumps
{
    public class SkillExpGump : Gump
    {
        Mobile caller;

        public static void Initialize()
        {

            NaoMostra.Add(SkillName.Meditation);

            NaoMostra.Add(SkillName.Tactics);
            NaoMostra.Add(SkillName.Anatomy);
           
            NaoMostra.Add(SkillName.ArmsLore);
            NaoMostra.Add(SkillName.SpiritSpeak);
            NaoMostra.Add(SkillName.Focus);

            Combate.Add(SkillName.MagicResist);
            Combate.Add(SkillName.Magery);
            Combate.Add(SkillName.EvalInt);
            Combate.Add(SkillName.Meditation);
            Combate.Add(SkillName.Swords);
            Combate.Add(SkillName.Fencing);
            Combate.Add(SkillName.Macing);
            Combate.Add(SkillName.Tactics);
            Combate.Add(SkillName.Anatomy);
            Combate.Add(SkillName.Healing);
            //Combate.Add(SkillName.ArmsLore);
            Combate.Add(SkillName.SpiritSpeak);
            Combate.Add(SkillName.Necromancy);
            Combate.Add(SkillName.Inscribe);
            Combate.Add(SkillName.Wrestling);
            Combate.Add(SkillName.Archery);
            Combate.Add(SkillName.Tracking);
            Combate.Add(SkillName.Hiding);
            Combate.Add(SkillName.Stealth);
            Combate.Add(SkillName.Ninjitsu);
            Combate.Add(SkillName.Focus);
            Combate.Add(SkillName.Parry);
            Combate.Add(SkillName.DetectHidden);
            Combate.Add(SkillName.Poisoning);
            Combate.Add(SkillName.Chivalry);
            Combate.Add(SkillName.TasteID);
            Combate.Add(SkillName.Musicianship);
            Combate.Add(SkillName.Provocation);
            Combate.Add(SkillName.Discordance);
            Combate.Add(SkillName.Peacemaking);
            Combate.Add(SkillName.Lockpicking);
            Combate.Add(SkillName.RemoveTrap);
            Combate.Add(SkillName.Begging);
            Combate.Add(SkillName.Forensics);
            CommandSystem.Register("xp", AccessLevel.Player, new CommandEventHandler(SkillsGump_OnCommand));
        }

        public static bool UpaComXP(SkillName s)
        {
            return Combate.Contains(s);
        }

        [Usage("SkillsGump")]
        [Description("Makes a call to your custom gump.")]
        public static void SkillsGump_OnCommand(CommandEventArgs e)
        {
            if (Shard.RP)
            {
                if (e.Mobile.HasGump(typeof(ExpGumpRP)))
                {
                    e.Mobile.SendMessage("Gump de xp ja aberto");
                    return;
                }
                e.Mobile.SendGump(new ExpGumpRP(e.Mobile as PlayerMobile));
                return;
            }

            if (e.ArgString != null && e.ArgString != "")
            {
                foreach (var skill in SkillInfo.Table)
                {
                    if (skill.Name.ToLower() == e.ArgString.ToLower())
                    {
                        e.Mobile.SendGump(new SkillExperienceGump(e.Mobile as PlayerMobile, (SkillName)skill.SkillID, 0));
                        return;
                    }
                }
                e.Mobile.SendMessage("Nao encontrei a skill");
                return;
            }
            if (e.Mobile.HasGump(typeof(SkillExpGump)))
            {
                e.Mobile.SendMessage("Gump de xp ja aberto");
                return;
            }
            e.Mobile.SendGump(new SkillExpGump(e.Mobile));
        }

        public static HashSet<SkillName> Combate = new HashSet<SkillName>();
        public static HashSet<SkillName> NaoMostra = new HashSet<SkillName>();

        public static int V = 0;

        public static int GetPontos(Mobile m, SkillName s)
        {
            return formulaCusto(m.Skills[s].Base);
        }

        public static int formulaCusto(double skill)
        {
            V = (int)Math.Pow(skill / 28, 8) / 17;

            if(Shard.RP)
                V /= 5;
            else
                V /= 10;

            if (V <= 0)
                V = 1;
          
            return V;
        }

        public static string GetCustoUp(Mobile m, SkillName s)
        {
            return String.Format("{0}", GetPontos(m, s));
        }

        public SkillExpGump(Mobile caller) : base(0, 0)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);
            AddBackground(328, 147, 926, 333, 1579);

            AddItem(390, 220, 3643);
            AddItem(374, 223, 3640);
            AddItem(413, 223, 3640);
            AddItem(635, 223, 7034);
            AddItem(614, 218, 5049);
            AddItem(849, 215, 5041);

            AddHtml(345, 290, 158, 21, string.Format("{0} Magic Resist", caller.Skills.MagicResist.Value), (bool)false, (bool)false);
            AddHtml(345, 269, 158, 21, string.Format(@"{0} Magery", caller.Skills.Magery.Value), (bool)false, (bool)false);
            AddHtml(345, 312, 158, 21, string.Format(@"{0} Evaluating Int.", caller.Skills.EvalInt.Value), (bool)false, (bool)false);
            AddHtml(345, 334, 158, 21, string.Format(@"{0} Meditation", caller.Skills.Meditation.Value), (bool)false, (bool)false);
            AddHtml(573, 439, 158, 21, string.Format("{0} TasteID", caller.Skills.TasteID.Value), (bool)false, (bool)false);

            AddHtml(573, 266, 158, 21, string.Format(@"{0} Swordsmanship", caller.Skills.Swords.Value), (bool)false, (bool)false);
            AddHtml(573, 288, 158, 21, string.Format(@"{0} Fencing", caller.Skills.Fencing.Value), (bool)false, (bool)false);
            AddHtml(573, 310, 158, 21, string.Format(@"{0} Macefight", caller.Skills.Macing.Value), (bool)false, (bool)false);
            AddHtml(573, 332, 158, 21, string.Format(@"{0} Tactics", caller.Skills.Tactics.Value), (bool)false, (bool)false);
            AddHtml(573, 354, 158, 21, string.Format(@"{0} Anatomy", caller.Skills.Anatomy.Value), (bool)false, (bool)false);
            AddHtml(573, 376, 158, 21, string.Format(@"{0} Healing", caller.Skills.Healing.Value), (bool)false, (bool)false);
            AddHtml(573, 398, 158, 21, string.Format(@"{0} ArmsLore", caller.Skills.ArmsLore.Value), (bool)false, (bool)false);
            AddHtml(345, 356, 158, 21, string.Format(@"{0} Spirit Speak", caller.Skills.SpiritSpeak.Value), (bool)false, (bool)false);
            AddHtml(345, 418, 158, 21, string.Format(@"{0} Necromancy", caller.Skills.Necromancy.Value), 3333, (bool)false, (bool)false);
            AddHtml(345, 398, 158, 21, string.Format(@"{0} Inscription", caller.Skills.Inscribe.Value), (bool)false, (bool)false);

            AddHtml(345, 378, 158, 21, string.Format(@"{0} Wrestling", caller.Skills.Wrestling.Value), (bool)false, (bool)false);
            AddHtml(794, 268, 158, 21, string.Format(@"{0} Archery", caller.Skills.Archery.Value), (bool)false, (bool)false);
            AddHtml(794, 289, 158, 21, string.Format(@"{0} Tracking", caller.Skills.Tracking.Value), (bool)false, (bool)false);
            AddHtml(794, 310, 158, 21, string.Format(@"{0} Hiding", caller.Skills.Hiding.Value), (bool)false, (bool)false);
            AddHtml(794, 331, 158, 21, string.Format(@"{0} Stealth", caller.Skills.Stealth.Value), (bool)false, (bool)false);
            AddHtml(794, 396 + 42 , 158, 21, string.Format(@"{0} Ninjutsu", caller.Skills.Ninjitsu.Value), 3333, (bool)false, (bool)false);
            AddHtml(794, 374, 158, 21, string.Format(@"{0} Focus", caller.Skills.Focus.Value), (bool)false, (bool)false);
            AddHtml(573, 419, 158, 21, string.Format(@"{0} Parrying", caller.Skills.Parry.Value), (bool)false, (bool)false);
            //AddHtml(794, 396, 158, 21, string.Format(@"{0} Detect Hidden", caller.Skills.DetectHidden.Value), (bool)false, (bool)false);
            AddHtml(794, 352 , 158, 21, string.Format(@"{0} Poisoning", caller.Skills.Poisoning.Value), (bool)false, (bool)false);
            AddHtml(794, 396+21, 158, 21, string.Format(@"{0} Lockpicking", caller.Skills.Lockpicking.Value), (bool)false, (bool)false);
            AddHtml(794, 396, 158, 21, string.Format(@"{0} Remove Trap", caller.Skills.RemoveTrap.Value), (bool)false, (bool)false);

            AddHtml(1025, 267, 158, 21, string.Format(@"{0} Musicanship", caller.Skills.Musicianship.Value), (bool)false, (bool)false);
            AddHtml(1025, 288, 158, 21, string.Format(@"{0} Peacemaking", caller.Skills.Peacemaking.Value), (bool)false, (bool)false);
            AddHtml(1025, 309, 158, 21, string.Format(@"{0} Provocation", caller.Skills.Provocation.Value), (bool)false, (bool)false);
            AddHtml(1025, 330, 158, 21, string.Format(@"{0} Discordance", caller.Skills.Discordance.Value), (bool)false, (bool)false);
            AddHtml(1025, 351, 158, 21, string.Format(@"{0} Detect Hidden", caller.Skills.DetectHidden.Value), (bool)false, (bool)false);
            AddHtml(1025, 351+21, 158, 21, string.Format(@"{0} Begging", caller.Skills.Begging.Value), (bool)false, (bool)false);
            AddHtml(1025, 351+42, 158, 21, string.Format(@"{0} Forensics", caller.Skills.Forensics.Value), (bool)false, (bool)false);

            AddHtml(345 + 158, 290, 158, 21, GetCustoUp(caller, SkillName.MagicResist), (bool)false, (bool)false);
            AddHtml(345 + 158, 269, 158, 21, GetCustoUp(caller, SkillName.Magery), (bool)false, (bool)false);
            AddHtml(345 + 158, 312, 158, 21, GetCustoUp(caller, SkillName.EvalInt), (bool)false, (bool)false);
            AddHtml(345 + 158, 334, 158, 21, GetCustoUp(caller, SkillName.Meditation), (bool)false, (bool)false);

            AddHtml(573 + 158, 266, 158, 21, GetCustoUp(caller, SkillName.Swords), (bool)false, (bool)false);
            AddHtml(573 + 158, 288, 158, 21, GetCustoUp(caller, SkillName.Fencing), (bool)false, (bool)false);
            AddHtml(573 + 158, 310, 158, 21, GetCustoUp(caller, SkillName.Macing), (bool)false, (bool)false);
            AddHtml(573 + 158, 332, 158, 21, GetCustoUp(caller, SkillName.Tactics), (bool)false, (bool)false);
            AddHtml(573 + 158, 354, 158, 21, GetCustoUp(caller, SkillName.Anatomy), (bool)false, (bool)false);
            AddHtml(573 + 158, 376, 158, 21, GetCustoUp(caller, SkillName.Healing), (bool)false, (bool)false);
            AddHtml(573 + 158, 398, 158, 21, GetCustoUp(caller, SkillName.ArmsLore), (bool)false, (bool)false);
            AddHtml(345 + 158, 356, 158, 21, GetCustoUp(caller, SkillName.SpiritSpeak), (bool)false, (bool)false);
            //AddHtml(345 + 158, 418 , 158, 21, GetCustoUp(caller, SkillName.Necromancy), (bool)false, (bool)false);
            AddHtml(345 + 158, 398, 158, 21, GetCustoUp(caller, SkillName.Inscribe), (bool)false, (bool)false);

            AddHtml(345 + 158, 378, 158, 21, GetCustoUp(caller, SkillName.Wrestling), (bool)false, (bool)false);
            AddHtml(794 + 158, 268, 158, 21, GetCustoUp(caller, SkillName.Archery), (bool)false, (bool)false);
            AddHtml(794 + 158, 289, 158, 21, GetCustoUp(caller, SkillName.Tracking), (bool)false, (bool)false);
            AddHtml(794 + 158, 310, 158, 21, GetCustoUp(caller, SkillName.Hiding), (bool)false, (bool)false);
            AddHtml(794 + 158, 331, 158, 21, GetCustoUp(caller, SkillName.Stealth), (bool)false, (bool)false);
            // AddHtml(794 + 158, 417+21 , 158, 21, GetCustoUp(caller, SkillName.Ninjitsu), (bool)false, (bool)false);
            AddHtml(794 + 158, 374, 158, 21, GetCustoUp(caller, SkillName.Focus), (bool)false, (bool)false);
            AddHtml(573 + 158, 419, 158, 21, GetCustoUp(caller, SkillName.Parry), (bool)false, (bool)false);
            AddHtml(573 + 158, 444, 158, 21, GetCustoUp(caller, SkillName.TasteID), (bool)false, (bool)false);
            //AddHtml(794 + 158, 396, 158, 21, GetCustoUp(caller, SkillName.DetectHidden), (bool)false, (bool)false);
            AddHtml(794 + 158, 352 , 158, 21, GetCustoUp(caller, SkillName.Poisoning), (bool)false, (bool)false);
            AddHtml(794 + 158, 417, 158, 21, GetCustoUp(caller, SkillName.Lockpicking), (bool)false, (bool)false);
            AddHtml(794 + 158, 396, 158, 21, GetCustoUp(caller, SkillName.RemoveTrap), (bool)false, (bool)false);

            AddHtml(1025 + 158, 267, 158, 21, GetCustoUp(caller, SkillName.Musicianship), (bool)false, (bool)false);
            AddHtml(1025 + 158, 288, 158, 21, GetCustoUp(caller, SkillName.Peacemaking), (bool)false, (bool)false);
            AddHtml(1025 + 158, 309, 158, 21, GetCustoUp(caller, SkillName.Provocation), (bool)false, (bool)false);
            AddHtml(1025 + 158, 330, 158, 21, GetCustoUp(caller, SkillName.Discordance), (bool)false, (bool)false);
            AddHtml(1025 + 158, 351, 158, 21, GetCustoUp(caller, SkillName.DetectHidden), (bool)false, (bool)false);
            AddHtml(1025 + 158, 351+21, 158, 21, GetCustoUp(caller, SkillName.Begging), (bool)false, (bool)false);
            AddHtml(1025 + 158, 351+42, 158, 21, GetCustoUp(caller, SkillName.Forensics), (bool)false, (bool)false);

            AddButton(486, 271, 55, 55, (int)Buttons.Magery, GumpButtonType.Reply, 0);
            AddImage(351, 252, 50);
            AddImage(578, 250, 50);
            AddImage(800, 250, 50);
            AddImage(679, 169, 92);
            AddImage(731, 169, 93);
            AddImage(843, 169, 94);
            AddHtml(731, 176, 101, 26, string.Format("{0} EXP", PointsSystem.Exp.GetPoints(caller)), (bool)false, (bool)false);
            AddButton(486, 292, 55, 55, (int)Buttons.MagicResist, GumpButtonType.Reply, 0);
            AddButton(486, 314, 55, 55, (int)Buttons.EvalInt, GumpButtonType.Reply, 0);
            AddButton(486, 336, 55, 55, (int)Buttons.Meditation, GumpButtonType.Reply, 0);
            AddButton(485, 358, 55, 55, (int)Buttons.SpiritSpeak, GumpButtonType.Reply, 0);
            //AddButton(485, 420 , 55, 55, (int)Buttons.Necromancy, GumpButtonType.Reply, 0);
            AddButton(485, 400, 55, 55, (int)Buttons.Inscribe, GumpButtonType.Reply, 0);
            AddButton(485, 380, 55, 55, (int)Buttons.Wrestling, GumpButtonType.Reply, 0);
            AddButton(714, 268, 55, 55, (int)Buttons.Swords, GumpButtonType.Reply, 0);
            AddButton(714, 290, 55, 55, (int)Buttons.Fencing, GumpButtonType.Reply, 0);
            AddButton(714, 312, 55, 55, (int)Buttons.Macing, GumpButtonType.Reply, 0);
            AddButton(714, 333, 55, 55, (int)Buttons.Tactics, GumpButtonType.Reply, 0);
            AddButton(714, 357, 55, 55, (int)Buttons.Anatomy, GumpButtonType.Reply, 0);
            AddButton(714, 377, 55, 55, (int)Buttons.Healing, GumpButtonType.Reply, 0);
            AddButton(714, 399, 55, 55, (int)Buttons.ArmsLore, GumpButtonType.Reply, 0);
            AddButton(714, 420, 55, 55, (int)Buttons.Parry, GumpButtonType.Reply, 0);
            AddButton(935, 269, 55, 55, (int)Buttons.Archery, GumpButtonType.Reply, 0);
            AddButton(935, 289, 55, 55, (int)Buttons.Tracking, GumpButtonType.Reply, 0);
            AddButton(935, 312, 55, 55, (int)Buttons.Hiding, GumpButtonType.Reply, 0);
            AddButton(935, 333, 55, 55, (int)Buttons.Stealth, GumpButtonType.Reply, 0);
            //AddButton(935,375 + 63 , 55, 55, (int)Buttons.Ninjitsu, GumpButtonType.Reply, 0);
            AddButton(935, 375, 55, 55, (int)Buttons.Focus, GumpButtonType.Reply, 0);
        
            AddButton(935, 354, 55, 55, (int)Buttons.Poisoning, GumpButtonType.Reply, 0);
            AddButton(935, 375 + 42, 55, 55, (int)Buttons.Lockpicking, GumpButtonType.Reply, 0);
            AddButton(935, 375 + 21 , 55, 55, (int)Buttons.RemoveTrap, GumpButtonType.Reply, 0);
            //AddButton(1119, 166, 1491, 1491, (int)Buttons.Help, GumpButtonType.Reply, 0);
            AddImage(1026, 251, 50);

            AddImage(321, 142, 10460);
            AddImage(1230, 134, 10460);
            AddImage(320, 456, 10460);
            AddImage(1229, 460, 10460);


           
            AddButton(1168, 268, 55, 55, (int)Buttons.Musicianship, GumpButtonType.Reply, 0);
            AddButton(1168, 288, 55, 55, (int)Buttons.Peacemaking, GumpButtonType.Reply, 0);
            AddButton(1168, 309, 55, 55, (int)Buttons.Provocation, GumpButtonType.Reply, 0);
            AddButton(1168, 332, 55, 55, (int)Buttons.Discordance, GumpButtonType.Reply, 0);
            AddButton(1168, 332+21, 55, 55, (int)Buttons.DetectHidden, GumpButtonType.Reply, 0);
            AddButton(1168, 353 + 21, 55, 55, (int)Buttons.Begging, GumpButtonType.Reply, 0);
            AddButton(1168, 353 + 42, 55, 55, (int)Buttons.Forensics, GumpButtonType.Reply, 0);

            AddItem(1081, 226, 3763);

            AddButton(714, 441, 55, 55, (int)Buttons.TasteID, GumpButtonType.Reply, 0);

            // Stats
            var modX = 250;
            var modY = 35;
            //AddBackground(763+modX, 84+modY, 195, 139, 1579);
            AddHtml(784 + modX, 186 + modY-20, 61, 21, caller.RawInt + " Int", false, false);
            AddButton(851 + modX, 187 + modY-20, 55, 55, (int)Buttons.Int, GumpButtonType.Reply, 0);
            AddHtml(871 + modX, 187 + modY-20, 61, 21, (formulaCusto(caller.RawInt)/2).ToString(), false, false);

            AddHtml(784 + modX, 158 + modY - 10, 61, 21, caller.RawDex + " Dex", false, false);
            AddButton(851 + modX, 159 + modY-10, 55, 55, (int)Buttons.Dex, GumpButtonType.Reply, 0);
            AddHtml(871 + modX, 159 + modY-10, 61, 21, (formulaCusto(caller.RawDex)/2).ToString(), false, false);

            AddHtml(783 + modX, 130 + modY, 61, 21, caller.RawStr + " Str", false, false);
            AddButton(850 + modX, 131 + modY, 55, 55, (int)Buttons.Str, GumpButtonType.Reply, 0);
            AddHtml(870 + modX, 131 + modY, 61, 21, (formulaCusto(caller.RawStr)/2).ToString(), false, false);
            //AddItem(911 + modX, 93 + modY, 6225);
            //AddItem(759 + modX, 93 + modY, 6226);
            //AddHtml(837 + modX, 100 + modY, 61, 21, "Stats", false, false);
        }


        public enum Buttons
        {
            Fechar,
            Nada,
            Magery,
            MagicResist,
            EvalInt,
            Meditation,
            SpiritSpeak,
            Necromancy,
            Inscribe,
            Wrestling,
            Swords,
            Fencing,
            Macing,
            Tactics,
            Anatomy,
            Healing,
            ArmsLore,
            Parry,
            Archery,
            Tracking,
            Hiding,
            Stealth,
            Ninjitsu,
            Focus,
            DetectHidden,
            Poisoning,
            Help,
            Musicianship,
            Peacemaking,
            Provocation,
            Discordance,
            Chivalry,
            Lockpicking,
            RemoveTrap,
            Begging,
            Forensics,
            TasteID,
            Str, Dex, Int
        }

        public SkillName GetSkill(int button)
        {
            var name = Enum.GetName(typeof(Buttons), button);
            var skill = (SkillName)Enum.Parse(typeof(SkillName), name);
            return skill;
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (Shard.DebugEnabled)
                Shard.Debug("Button ID " + info.ButtonID);

            if (info.ButtonID == (int)Buttons.Help)
            {
                Shard.Debug("Clicou no help");
                return;
            }
            if(info.ButtonID == (int)Buttons.Str)
            {
                if (from.RawStr > 100)
                    return;

                var custo = formulaCusto(from.RawStr)/2;
                var tem = PointsSystem.Exp.GetPoints(from);

                if (custo > tem)
                {
                    from.SendMessage(string.Format("Voce precisa de {0} EXP para upar", custo));
                    return;
                }

                SkillCheck.GainStat(from, SkillCheck.Stat.Str);
                PointsSystem.Exp.DeductPoints(from, custo, false);
                from.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);
                from.PlaySound(0x1FD);
                from.SendGump(new SkillExpGump(from));
                return;
            } else if (info.ButtonID == (int)Buttons.Dex)
            {
                if (from.RawDex > 100)
                    return;

                var custo = formulaCusto(from.RawDex)/2;
                var tem = PointsSystem.Exp.GetPoints(from);

                if (custo > tem)
                {
                    from.SendMessage(string.Format("Voce precisa de {0} EXP para upar", custo));
                    return;
                }

                SkillCheck.GainStat(from, SkillCheck.Stat.Dex);
                PointsSystem.Exp.DeductPoints(from, custo, false);
                from.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);
                from.PlaySound(0x1FD);
                from.SendGump(new SkillExpGump(from));
                return;
            }
            else if (info.ButtonID == (int)Buttons.Int)
            {
                if (from.RawInt > 100)
                    return;

                var custo = formulaCusto(from.RawInt)/2;
                var tem = PointsSystem.Exp.GetPoints(from);

                if (custo > tem)
                {
                    from.SendMessage(string.Format("Voce precisa de {0} EXP para upar", custo));
                    return;
                }

                SkillCheck.GainStat(from, SkillCheck.Stat.Int);
                PointsSystem.Exp.DeductPoints(from, custo, false);
                from.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);
                from.PlaySound(0x1FD);
                from.SendGump(new SkillExpGump(from));
                return;
            }

            if (info.ButtonID <= 0)
            {
                return;
            }

            var skill = GetSkill(info.ButtonID);

            var exp = GetPontos(from, skill);
            var expAtual = PointsSystem.Exp.GetPoints(from);

            if (exp > expAtual)
            {
                from.SendMessage(string.Format("Voce precisa de {0} EXP para subir a skill {1}", exp, skill.ToString()));
                return;
            }

            var old = from.Skills[skill].Base;
            var gain = 10;

            if(Shard.RP)
            {
                gain = 1;
            }

            if(old >= 99)
            {
                gain = 1;
            }

            SkillCheck.Gain(from, from.Skills[skill], gain);
            var nw = from.Skills[skill].Value;

            if (nw > old)
            {
                from.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);
                from.PlaySound(0x1FD);
                PointsSystem.Exp.DeductPoints(from, exp, false);
            }
            else
            {
                var tenta = true;
                while(tenta && gain > 0)
                {
                    gain--;
                    SkillCheck.Gain(from, from.Skills[skill], gain);
                    nw = from.Skills[skill].Value;
                    if (nw > old)
                    {
                        from.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);
                        from.PlaySound(0x1FD);
                        PointsSystem.Exp.DeductPoints(from, exp, false);
                        from.SendGump(new SkillExpGump(from));
                        return;
                    }
                }
                from.SendMessage("A skill chegou no limite ou voce chegou no seu cap e precisa setar alguma skill para baixar na janela de skills.");
            }
            from.SendGump(new SkillExpGump(from));
        }
    }
}
