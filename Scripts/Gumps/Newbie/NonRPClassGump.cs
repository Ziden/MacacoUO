
using Server.Network;
using Server.Commands;
using Server.Gumps.Newbie;
using Server.Items;
using Server.Mobiles;
using Server.Scripts.New.Adam.NewGuild;
using Server.Misc.Templates;
using Server.Misc.Custom;

namespace Server.Gumps
{
    public class NonRPClassGump : Gump
    {
        string chosen = null;
        string desc = null;

        private StarterKits.Kit k;
        private bool newCharacter;

        public NonRPClassGump(StarterKits.Kit kit = null, bool newCharacter = true) : base(0, 0)
        {
            this.Closable = !newCharacter;
            this.Disposable = !newCharacter;
            this.Dragable = false;
            this.Resizable = false;
            this.newCharacter = newCharacter;

            StarterKits.BuildKits();

            k = kit;
            var code = -1;
            if (kit != null)
                code = kit.Code;

            AddPage(0);
            AddBackground(79, 69, 493, 464, 9200);
            AddButton(353, 104, 5553, 5554, 3, GumpButtonType.Reply, 0);
            //if (newCharacter)
            //    AddButton(473, 185, 5545, 5546, 8, GumpButtonType.Reply, 0);
            AddButton(107, 103, 5577, 5578, 1, GumpButtonType.Reply, 0);
            AddButton(227, 102, 5555, 5556, 2, GumpButtonType.Reply, 0);
            AddButton(472, 103, 5551, 5552, 4, GumpButtonType.Reply, 0);
            AddButton(107, 181, 5549, 5550, 5, GumpButtonType.Reply, 0);
            AddButton(228, 183, 5569, 5570, 6, GumpButtonType.Reply, 0);
            AddButton(355, 183, 5571, 5572, 7, GumpButtonType.Reply, 0);
            AddImage(29, 13, 10440);
            AddImage(540, 14, 10441);
            AddImage(234, -167, 1418);
            AddHtml(100, 84, 86, 19, @"Guerreiro", (bool)false, (bool)false);
            AddHtml(224, 84, 86, 19, @"Ferreiro", (bool)false, (bool)false);
            AddHtml(359, 83, 86, 19, @"Bardo", (bool)false, (bool)false);
            AddHtml(471, 83, 124, 19, @"Arqueiro", (bool)false, (bool)false);
            AddHtml(105, 163, 124, 19, @"Domador", (bool)false, (bool)false);
            AddHtml(235, 164, 124, 19, @"Mago", (bool)false, (bool)false);
            AddHtml(352, 165, 124, 19, @"Mercador", (bool)false, (bool)false);

            //if (newCharacter)
            //    AddHtml(450, 166, 124, 19, @"Manter Skills", (bool)false, (bool)false);

            var desc = "Selecione uma template de skills iniciais.";

            if (kit != null)
            {
                desc = kit.Name + "<br>" + kit.Desc;
                AddHtml(103, 258, 441, 83, desc, (bool)true, (bool)false);
                AddButton(473, 498, 247, 248, 0, GumpButtonType.Reply, 0);

                AddHtml(104, 368, 441, 83, "", (bool)true, (bool)false);
                AddHtml(106, 345, 200, 20, @"Skills", (bool)false, (bool)false);

                var x = 0;
                var y = 0;

                foreach (var skillname in kit.Skills.Keys)
                {
                    var value = kit.Skills[skillname];
                    AddHtml(110 + x, 370 + y, 441, 83, skillname + ": " + value, false, false);
                    x += 120;
                    if (x > 330)
                    {
                        x = 0;
                        y += 30;
                    }
                }
            }
            else
            {
                AddHtml(103, 258, 445, 233, @"<br>Escolha seu kit de skills iniciais. <br><br>Voce podera ter quaisquer skills que quiser, isto e apenas um inicio.", (bool)true, (bool)false);
            }
        }

        private static void EquipItem(Mobile mob, Item item)
        {
            if (mob != null && mob.EquipItem(item))
                return;

            var pack = mob.Backpack;

            if (pack != null)
                pack.DropItem(item);
        }

        private static void PackItem(Mobile mob, Item item)
        {

            var pack = mob.Backpack;

            if (pack != null)
                pack.DropItem(item);
            else
                item.Delete();
        }

        public static int TEMP_ID = 2;

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == 0)
            {
                if (k != null)
                {

                    if (newCharacter)
                    {
                        var robe = from.FindItemOnLayer(Layer.OuterTorso);
                        if (robe != null)
                        {
                            robe.Consume();
                        }
                    }

                    PackItem(from, new Torch());

                    if (k.Code == 8)
                    {
                        if (newCharacter)
                            PackItem(from, new Gold(500));
                    }
                    else
                    {
                        if (Shard.SPHERE_STYLE)
                        {
                            from.Str = 50;
                            from.Dex = 50;
                            from.Int = 50;
                        }
                        else
                        {
                            from.Str = k.Str;
                            from.Dex = k.Dex;
                            from.Int = k.Int;
                        }

                        from.Stam = from.StamMax;
                        from.Mana = from.ManaMax;
                        from.Hits = from.HitsMax;
                    }

                    foreach (var skill in from.Skills)
                    {
                        skill.SendPacket = false;
                        skill.m_Exp = 0;
                        skill.SetLockNoRelay(SkillLock.Up);
                        if (k.Skills.ContainsKey(skill.SkillName))
                        {
                            if (!newCharacter || from.Skills[skill.SkillName].Base < k.Skills[skill.SkillName])
                                from.Skills[skill.SkillName].Base = k.Skills[skill.SkillName];

                        }
                        else if (!newCharacter)
                        {
                            from.Skills[skill.SkillName].Base = 0;
                        }
                        skill.SendPacket = true;
                    }

                    from.Send(new SkillUpdate(from.Skills));

                    if (newCharacter)
                    {
                        from.Hunger = 20;
                        from.Thirst = 20;
                        var hue = StarterKits.GetNoobColor();

                        var ball = new ElementalBall();
                        ball.BoundTo = from.RawName;
                        ball.InvalidateProperties();
                        PackItem(from, ball);

                        foreach (var item in k.items)
                        {
                            var dupe = Dupe.DupeItem(item);
                            if (dupe.Hue == 78)
                            {
                                dupe.Hue = hue;
                            }
                            if (dupe is IQuality)
                            {
                                if (dupe is BaseWeapon)
                                    ((IQuality)dupe).Quality = ItemQuality.Exceptional;
                                else
                                    ((IQuality)dupe).Quality = ItemQuality.Low;
                            }
                            PackItem(from, dupe);
                        }

                        PackItem(from, new CombatSkillBook() { Exp = 4000 });

                        foreach (var item in k.equips)
                        {
                            var dupe = Dupe.DupeItem(item);
                            if (dupe.Hue == 78)
                            {
                                dupe.Hue = hue;
                            }
                            if (dupe is IQuality && dupe is BaseWeapon)
                            {
                                ((IQuality)dupe).Quality = ItemQuality.Low;
                            }
                            EquipItem(from, dupe);
                        }
                        var player = (PlayerMobile)from;
                        player.Profession = k.Code;
                        player.SendMessage("Kit Inicial Escolhido - Bem Vindo !");
                        //NewPlayerGuildAutoJoin.SendStarterGuild(player);
                    }
                    else
                    {

                        var player = (PlayerMobile)from;
                        player.SendMessage("Nova template criada");
                        var template = new Template();
                        template.Name = k.Name + Utility.Random(9999999);
                        template.FromPlayer(player);
                        player.Templates.Templates.Add(template);
                        player.CurrentTemplate = template.Name;
                        if (player.Wisp != null)
                        {
                            player.Wisp.TrocaTemplate(k.Code);
                        }
                    }
                }
            }

            if (info.ButtonID == 8)
            {
                var k = new StarterKits.Kit()
                {
                    Name = "Manter",
                    Code = 8,
                    Desc = "Manter skills escolhidas na criacao do personagem"
                };
                foreach (var skill in from.Skills)
                {
                    if (skill.Value > 0)
                        k.Skills.Add(skill.SkillName, (int)skill.Value);
                }
                from.CloseGump(typeof(NonRPClassGump));
                from.SendGump(new NonRPClassGump(k, newCharacter));
                return;
            }

            var kit = StarterKits.GetKit(info.ButtonID);

            if (kit != null)
            {
                from.CloseGump(typeof(NonRPClassGump));
                from.SendGump(new NonRPClassGump(kit, newCharacter));
            }
        }
    }
}
