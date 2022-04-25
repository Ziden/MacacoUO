using System;
using System.Collections.Generic;
using System.Linq;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Spells.Ninjitsu;
using VitaNex;

namespace Server.Gumps
{
    public class TemplatesGump : Gump
    {
        private int color = 0xFFFFFF;

        public static readonly int max_templates = 10;

        public TemplatesGump(PlayerMobile player) : base(0, 0)
        {
            if(!player.Region.IsPartOf<GuardedRegion>() && !player.Region.IsPartOf<NoHousingRegion>() && !player.Region.IsPartOf<NoGuardCity>())
            {
                var house = BaseHouse.FindHouseAt(player);
                if(house == null || (!house.IsCoOwner(player) && !house.IsFriend(player) && !house.IsOwner(player))) {
                    player.SendMessage("Voce precisa estar em uma cidade ou em sua casa para fazer isto");
                    player.CloseGump(typeof(TemplatesGump));
                    return;
                }
            }

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            this.AddPage(0);
            this.AddBackground(536, 199, 469, 477, 9200);
            this.AddBackground(763, 264, 230, 41, 9270);
            this.AddBackground(763, 301, 230, 199, 9270);

            var skillsStr = "";

            List<SavedSkill> toDisplay = new List<SavedSkill>();
            int str;
            int dex;
            int inte;

            var currentTemplate = player.Templates.GetCurrentTemplate(player);
            str = player.RawStr;
            dex = player.RawDex;
            inte = player.RawInt;
            foreach (var skill in player.Skills)
            {
                if (skill.Base > 0)
                {
                    toDisplay.Add(new SavedSkill(skill.SkillName, skill.Value, skill.GetExp()));
                }
            }

            toDisplay = toDisplay.OrderByDescending(e => e.value).ToList();

            foreach (var skill in toDisplay)
            {
                skillsStr += skill.skill + ": " + skill.value + " <br>";
            }

            this.AddHtml(777, 315, 200, 167, skillsStr, color, (bool)false, (bool)false);

            this.AddHtml(774, 276, 200, 20, @"Skills", color, (bool)false, (bool)false);
            this.AddBackground(763, 500, 230, 41, 9270);
            this.AddBackground(763, 537, 230, 81, 9270);

            this.AddHtml(777, 551, 200, 54, @"Str " + str + " <br>Dex " + dex + " <br>Int " + inte, color, (bool)false, (bool)false);

            this.AddHtml(774, 511, 200, 20, @"Stats", color, (bool)false, (bool)false);
            this.AddHtml(765, 224, 223, 32, @"", (bool)true, (bool)false);
            this.AddItem(611, 208, 2713);
            this.AddItem(622, 246, 2960);
            this.AddItem(649, 267, 4030);
            this.AddItem(638, 251, 4031);

            this.AddItem(630, 278, 2908);
            this.AddTextEntry(775, 230, 200, 20, 0, (int)Buttons.TemplateName, player.CurrentTemplate);

            this.AddBackground(555, 381, 197, 285, 9270);
            this.AddBackground(555, 343, 197, 41, 9270);
            this.AddHtml(565, 353, 173, 22, @"Templates", color, (bool)false, (bool)false);

            if(player.Templates.Templates.Count >= max_templates)
            {
                this.AddHtml(772, 640, 89, 27, @"Deletar", (bool)true, (bool)false);
            } else
            {
                this.AddHtml(772, 640, 89, 27, @"Nova", (bool)false, (bool)false);
            }

            this.AddImage(486, 224, 10440);
            this.AddImage(972, 243, 10441);
            this.AddImage(544, 171, 10462);
            this.AddImage(940, 170, 10462);
            this.AddImage(741, 171, 10462);
            this.AddImage(844, 171, 10462);
            this.AddImage(639, 171, 10462);

            int y = 0;
            var index = 100;
            foreach (var template in player.Templates.Templates)
            {
                this.AddHtml(568, 396 + y, 149, 22, template.Name, color, (bool)false, (bool)false);
                if (template.Name != player.CurrentTemplate)
                    this.AddButton(720, 399 + y, 5601, 5601, index, GumpButtonType.Reply, 0);
                index++;
                y += 25;
            }

            this.AddHtml(669, 203, 200, 19, @"Templates", color, (bool)false, (bool)false);

            this.AddButton(802, 644, 1209, 1210, (int)Buttons.NewTemplate, GumpButtonType.Reply, 0);
   
            this.AddButton(868, 644, 1151, 1152, (int)Buttons.DeleteTemplate, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            base.OnResponse(sender, info);
            try
            {
                var player = (PlayerMobile)sender.Mobile;

                if (!player.Alive)
                    return;

                if (player.Templates == null)
                    return;

                var typedName = info.GetTextEntry((int)Buttons.TemplateName).Text;

                // Atualizando template atual
                var currentTemplate = player.Templates.GetCurrentTemplate(player);

                if (player.Templates.NomeJaExiste(player, typedName))
                {
                    player.SendMessage("Ja existe uma template com este nome");
                    return;
                }

                if (currentTemplate != null)
                {
                    currentTemplate.FromPlayer(player);
                    currentTemplate.Name = typedName;
                    player.CurrentTemplate = typedName;
                }

                // mudando template
                /*
                if (player.IsCooldown("template"))
                {
                    player.SendMessage("Aguarde pelo menos 10 segundos");
                    return;
                }
                player.SetCooldown("template", TimeSpan.FromSeconds(10));
                */

                // Mudando de template
                if (info.ButtonID >= 100)
                {
                    var index = info.ButtonID - 100;
                    var template = player.Templates.Templates[index];
                    if(template.ToPlayer(player))
                    {
                        player.CurrentTemplate = template.Name;
                        player.CloseAllGumps();
                        player.SendGump(new TemplatesGump(player));

                        SpellUtility.NegateEffects(player, false, true, false, true);
                    }
                }

                if (info.ButtonID == (int)Buttons.NewTemplate)
                {
                    if (player.Templates.Templates.Count >= max_templates)
                    {
                        player.SendGump(new ConfirmDelete(player, player.Templates.GetCurrentTemplate(player)));
                    }
                    else
                    {
                        player.SendGump(new NonRPClassGump(null, false));
                    }
                } else if (info.ButtonID == (int)Buttons.DeleteTemplate)
                {
                    if (player.Templates.Templates.Count == 1)
                    {
                        player.SendMessage("Voce nao pode deletar sua unica template");
                        return;
                    }
                    player.SendGump(new ConfirmDelete(player, player.Templates.GetCurrentTemplate(player)));
                }
            } catch (Exception e)
            {
                Console.WriteLine("[DEBUG] PAU !!! " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public enum Buttons
        {
            TemplateName,
            LoadTemplate,
            Button3,
            CopyofButton3,
            NewTemplate,
            DeleteTemplate,
        }

    }
}


