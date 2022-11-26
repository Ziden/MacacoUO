
using Server.Network;
using Server.Commands;
using Server.Gumps.Newbie;
using Server.Items;
using Server.Mobiles;
using Server.Scripts.New.Adam.NewGuild;
using Server.Misc.Templates;
using Server.Misc.Custom;
using Server.Fronteira.Classes;
using System;
using Server.Fronteira.RP;

namespace Server.Gumps
{
    public class RPClassGump : Gump
    {
        string chosen = null;
        string desc = null;

        private ClassePersonagem classe;
        private bool newCharacter;

        public RPClassGump(ClassePersonagem classe = null, bool newCharacter = true) : base(0, 0)
        {
            this.Closable = !newCharacter;
            this.Disposable = !newCharacter;
            this.Dragable = false;
            this.Resizable = false;
            this.newCharacter = newCharacter;

            StarterKits.BuildKits();

            this.classe = classe;
        
            AddPage(0);
            AddBackground(79, 69, 493, 464, 9200);
         
            //if (newCharacter)
            //    AddButton(473, 185, 5545, 5546, 8, GumpButtonType.Reply, 0);
            AddButton(107, 103, 5577, 5578, 1, GumpButtonType.Reply, 0);
            AddButton(227, 102, 5561, 5562, 2, GumpButtonType.Reply, 0);
            AddButton(353, 104, 5553, 5554, 4, GumpButtonType.Reply, 0);
            AddButton(472, 103, 5571, 5572, 5, GumpButtonType.Reply, 0);
            AddButton(107, 183, 5569, 5570, 3, GumpButtonType.Reply, 0);
            AddImage(29, 13, 10440);
            AddImage(540, 14, 10441);
            AddImage(234, -167, 1418);
            AddHtml(100, 84, 86, 19, @"Guerreiro", (bool)false, (bool)false);
            AddHtml(224, 84, 86, 19, @"Ladino", (bool)false, (bool)false);
            AddHtml(359, 83, 86, 19, @"Bardo", (bool)false, (bool)false);
            AddHtml(471, 83, 124, 19, @"Mercador", (bool)false, (bool)false);    
            AddHtml(100, 164, 124, 19, @"Mago", (bool)false, (bool)false);

            //if (newCharacter)
            //    AddHtml(450, 166, 124, 19, @"Manter Skills", (bool)false, (bool)false);

            var desc = "Selecione sua classe.";

            if (classe != null)
            {
                desc = classe.Nome + "<br>" + classe.Desc;
                AddHtml(103, 258, 441, 83, desc, (bool)true, (bool)false);
                AddButton(473, 498, 247, 248, 0, GumpButtonType.Reply, 0);

                AddHtml(104, 368, 441, 83, "", (bool)true, (bool)false);
                AddHtml(106, 345, 200, 20, @"Skill Caps", (bool)false, (bool)false);

                var x = 0;
                var y = 0;

                var skillString = "";

                foreach (var skillname in classe.ClassSkills.Keys)
                {
                    var value = classe.ClassSkills[skillname];
                    skillString += skillname + ": " + value +" | ";
                    x += 1;
                    if (x > 3)
                    {
                        skillString += "</br>";
                        x = 0;
                    }
                }
                AddHtml(106, 368, 441, 83, skillString, (bool)true, (bool)true);
            } else
            {
                AddHtml(103, 258, 445, 233, @"<br>Escolha sua classe. <br><br>Sua classe define seus caps de skills iniciais.", (bool)true, (bool)false);
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
                if (classe != null)
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

                    if (newCharacter)
                        PackItem(from, new Gold(500));

                    from.Skills.Cap = int.MaxValue;

                    foreach (var skill in from.Skills)
                    {
                        skill.SendPacket = false;
                        skill.m_Exp = 0;
                        skill.SetLockNoRelay(SkillLock.Up);
                        if (classe.ClassSkills.ContainsKey(skill.SkillName))
                        {
                            var valor = classe.ClassSkills[skill.SkillName];
                            if(valor >= 90)
                            {
                                valor = 30;
                            } else if(valor >= 60)
                            {
                                valor = 15;
                            } else
                            {
                                valor = 0;
                            }
                            if(valor != 0)
                            {
                                from.Skills[skill.SkillName].Base = valor;
                            }
                            from.Skills[skill.SkillName].Cap = classe.ClassSkills[skill.SkillName];
                        }
                        else
                        {
                            from.Skills[skill.SkillName].Base = 0;
                            from.Skills[skill.SkillName].Cap = 0;
                        }
                        skill.SendPacket = true;
                    }

                    from.Send(new SkillUpdate(from.Skills));


                    if (newCharacter)
                    {
                        from.Hunger = 20;
                        from.Thirst = 20;
                        var hue = StarterKits.GetNoobColor();

                        foreach (var item in classe.ItemsIniciais)
                        {
                            var dupe = Dupe.DupeItem(item);
                            if (dupe.Hue == 78)
                            {
                                dupe.Hue = hue;
                            }
                            if (dupe is IQuality)
                            {
                                ((IQuality)dupe).Quality = ItemQuality.Low;
                            }

                            PackItem(from, dupe);
                            if (dupe.Layer != Layer.Invalid && dupe is ICombatEquipment)
                            {
                                if(from.FindItemOnLayer(dupe.Layer) == null)
                                {
                                    from.EquipItem(dupe);
                                }
                            }
                        }

                        var player = (PlayerMobile)from;
                        player.Profession = classe.ID;
                        InicioRP.EscolheClasse(player, classe);
                    }
                }
            }

            var kit = ClassDef.GetClasse(info.ButtonID);

            if (kit != null)
            {
                from.CloseGump(typeof(RPClassGump));
                from.SendGump(new RPClassGump(kit));
            }
        }
    }
}
