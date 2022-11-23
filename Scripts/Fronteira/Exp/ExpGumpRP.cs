
using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using System.Collections.Generic;
using Server.Misc;
using Server.Engines.Points;
using Server.Mobiles;

namespace Server.Gumps
{
    public class ExpGumpRP : Gump
    {

        public static List<SkillName> UpComXP = new List<SkillName>();
        private static List<SkillName> UpComRepeticao = new List<SkillName>();

        public static void Initialize()
        {
            UpComXP.Add(SkillName.MagicResist);
            UpComXP.Add(SkillName.Magery);
            UpComXP.Add(SkillName.EvalInt);
            UpComXP.Add(SkillName.Meditation);
            UpComXP.Add(SkillName.Swords);
            UpComXP.Add(SkillName.Fencing);
            UpComXP.Add(SkillName.Macing);
            UpComXP.Add(SkillName.Tactics);
            UpComXP.Add(SkillName.Anatomy);
            UpComXP.Add(SkillName.Healing);
            UpComXP.Add(SkillName.ArmsLore);
            UpComXP.Add(SkillName.SpiritSpeak);
            UpComXP.Add(SkillName.Necromancy);
            UpComXP.Add(SkillName.Inscribe);
            UpComXP.Add(SkillName.Wrestling);
            UpComXP.Add(SkillName.Archery);
            UpComXP.Add(SkillName.Tracking);
            UpComXP.Add(SkillName.Hiding);
            UpComXP.Add(SkillName.Stealth);
            UpComXP.Add(SkillName.Ninjitsu);
            UpComXP.Add(SkillName.Focus);
            UpComXP.Add(SkillName.Parry);
            UpComXP.Add(SkillName.DetectHidden);
            UpComXP.Add(SkillName.Poisoning);
            UpComXP.Add(SkillName.Chivalry);
            UpComXP.Add(SkillName.Musicianship);
            UpComXP.Add(SkillName.Provocation);
            UpComXP.Add(SkillName.Discordance);
            UpComXP.Add(SkillName.Peacemaking);
            UpComXP.Add(SkillName.Stealing);
            UpComXP.Add(SkillName.Snooping);
            UpComXP.Add(SkillName.Forensics);
            UpComXP.Add(SkillName.Veterinary);
            UpComXP.Add(SkillName.AnimalLore);

            foreach (var s in Enum.GetValues(typeof(SkillName))) {
                var skill = (SkillName)s;
                if (UpComXP.Contains(skill)) continue;
                if (SkillCheck.Work.Contains(skill)) continue;
                UpComRepeticao.Add(skill);
            }
        }

        public ExpGumpRP(PlayerMobile from) : base(0, 0)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;


            AddPage(0);
            AddBackground(224, 74, 755, 629, 5150);
            AddImage(254, 135, 52);

            double total = 0;
            double capTotal = 0;
            var x = 0;
            foreach(var s in UpComXP)
            {
                if (from.Skills[s].Cap == 0) continue;

                var skill = from.Skills[s];

                AddHtml(251, 268 + x, 153, 21, skill.Value+" " +skill.Name, (bool)false, (bool)false);
                AddButton(407, 270 + x, 55, 55, skill.SkillID, GumpButtonType.Reply, 0);
                AddImage(467, 272 + x, 58);
                AddHtml(424, 270 + x, 40, 18, SkillExpGump.GetCustoUp(from, s), (bool)false, (bool)false);
                AddHtml(487, 270 + x, 70, 18, @"Max "+(int)skill.Cap, (bool)false, (bool)false);
                total += skill.Value;
                capTotal += skill.Cap;
                x += 20;
            }

            x = 0;
            foreach (var s in SkillCheck.Work)
            {
                if (from.Skills[s].Cap == 0) continue;

                var skill = from.Skills[s];
                var name = skill.Name;
                if (name == null) name = skill.SkillName.ToString();
                AddHtml(587, 267+x, 153, 21, skill.Value + " / " + skill.Cap + " " + name, (bool)false, (bool)false);
                total += skill.Value;
                capTotal += skill.Cap;
                x += 20;
            }

            x = 0;
            foreach (var s in UpComRepeticao)
            {
                if (from.Skills[s].Cap == 0) continue;

                var skill = from.Skills[s];
                var name = skill.Name;
                if (name == null) name = skill.SkillName.ToString();
                AddHtml(790, 265+x, 153, 21, skill.Value + " / " + skill.Cap + " " + name, (bool)false, (bool)false);
                total += skill.Value;
                capTotal += skill.Cap;
                x += 20;
            }

            var pct = (int)Math.Ceiling((total / capTotal) * 100);

            AddImage(247, 250, 50);
            AddHtml(377, 99, 46, 19, @"Skills", (bool)false, (bool)false);
            AddHtml(303, 141, 98, 21, string.Format("{0} EXP", PointsSystem.Exp.GetPoints(from)), (bool)false, (bool)false);
            AddImage(307, 250, 50);
            AddHtml(449, 141, 108, 18, total+" / "+capTotal, (bool)false, (bool)false);

            var expFalta = from.ExpProximoNivel() - from.ExpTotal;
            AddHtml(649, 141, 408, 18, "EXP para proximo talento: "+ expFalta, (bool)false, (bool)false);


            AddImage(413, 250, 50);
            AddHtml(449, 159, 108, 18, pct+"%", (bool)false, (bool)false);
            AddHtml(449, 122, 54, 19, @"Total:", (bool)false, (bool)false);

            AddHtml(300, 219, 175, 19, @"Up Em Combate", (bool)false, (bool)false);
            AddItem(238, 216, 5049);
            AddImage(595, 250, 50);
            AddItem(595, 218, 3717);
            AddHtml(637, 221, 175, 19, @"Up Trabalhando", (bool)false, (bool)false);

            AddImage(792, 249, 50);
            AddItem(593, 222, 4021);
            AddHtml(810, 220, 124, 19, @"Up Usando", (bool)false, (bool)false);
            AddItem(770, 218, 4029);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (Shard.DebugEnabled)
                Shard.Debug("Button ID " + info.ButtonID);

            if (info.ButtonID <= 0)
            {
                return;
            }

            var skill = from.Skills[info.ButtonID].SkillName;

            var exp = SkillExpGump.GetPontos(from, skill);
            var expAtual = PointsSystem.Exp.GetPoints(from);

            if (exp > expAtual)
            {
                from.SendMessage(string.Format("Voce precisa de {0} EXP para subir a skill {1}", exp, skill.ToString()));
                return;
            }

            var old = from.Skills[skill].Value;
            SkillCheck.Gain(from, from.Skills[skill], Shard.AVENTURA ? 10: 1);
            var nw = from.Skills[skill].Value;

            if (nw > old)
            {
                from.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);
                from.PlaySound(0x1FD);
                PointsSystem.Exp.DeductPoints(from, exp, false);
            }
            else
            {
                from.SendMessage("A skill chegou no limite ou voce chegou no seu cap e precisa setar alguma skill para baixar na janela de skills.");
            }
            from.SendGump(new ExpGumpRP(from as PlayerMobile));
        }
    }
}
