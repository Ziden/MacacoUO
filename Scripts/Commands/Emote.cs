using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Gumps;
using Server.Commands.Generic;
using Server.Commands;
using Server.Engines.XmlSpawner2;
using Server.Prompts;
using Server.Misc;


namespace Server.Scripts.Commands
{
    public enum EmotePage
    {
        P1,
        P2,
        P3,
        P4,
    }
    public class Emote
    {
        public static void Initialize()
        {		
            CommandSystem.Register("e", AccessLevel.Player, new CommandEventHandler(Emote_OnCommand));
        }

        [Usage("<sound>")]
        [Description("Emotes com palavras, sons e animações num comando!")]
        private static void Emote_OnCommand(CommandEventArgs e)
        {
            Mobile pm = e.Mobile;
            string em = e.ArgString.Trim();
            int SoundInt;
            switch (em)
            {
                case "ah":
                    SoundInt = 1;
                    break;
                case "ahha":
                    SoundInt = 2;
                    break;
                case "aplaude":
                    SoundInt = 3;
                    break;
                case "assoa":
                    SoundInt = 4;
                    break;
                case "reverencia":
                    SoundInt = 5;
                    break;
                case "tosseironica":
                    SoundInt = 6;
                    break;
                case "arrota":
                    SoundInt = 7;
                      break;
                case "limpagarganta":
                    SoundInt = 8;
                    break;
                case "tosse":
                    SoundInt = 9;
                    break;
                case "chora":
                    SoundInt = 10;
                    break;
                case "berra":
                    SoundInt = 42;
                    break;
                case "peida":
                    SoundInt = 12;
                    break;
                case "arfa":
                    SoundInt = 13;
                    break;
                case "risadinha":
                    SoundInt = 14;
                    break;
                case "geme":
                    SoundInt = 15;
                    break;
                case "rosna":
                    SoundInt = 16;
                    break;
                case "hey":
                    SoundInt = 17;
                    break;
                case "soluca":
                    SoundInt = 18;
                    break;
                case "huh?":
                    SoundInt = 19;
                    break;
                case "beija":
                    SoundInt = 20;
                    break;
                case "ri":
                    SoundInt = 21;
                    break;
                case "nao":
                    SoundInt = 22;
                    break;
                case "oh":
                    SoundInt = 23;
                    break;
                case "oooh":
                    SoundInt = 24;
                    break;
                case "oops":
                    SoundInt = 25;
                    break;
                case "vomita":
                    SoundInt = 26;
                    break;
                case "esmurra":
                    SoundInt = 27;
                    break;
                case "grita":
                    SoundInt = 28;
                    break;
                case "shhh!":
                    SoundInt = 29;
                    break;
                case "suspira":
                    SoundInt = 30;
                    break;
                case "estapeia":
                    SoundInt = 31;
                    break;
                case "espirra":
                    SoundInt = 32;
                    break;
                case "fareja":
                    SoundInt = 33;
                    break;
                case "ronca":
                    SoundInt = 34;
                    break;
                case "cospe":
                    SoundInt = 35;
                    break;
                case "estiralingua":
                    SoundInt = 36;
                    break;
                case "batepe":
                    SoundInt = 37;
                    break;
                case "assobia":
                    SoundInt = 38;
                    break;
                case "woohoo":
                    SoundInt = 39;
                    break;
                case "boceja":
                    SoundInt = 40;
                    break;
                case "yea":
                    SoundInt = 41;
                    break;
                default:
                    SoundInt = 0;
                    e.Mobile.SendGump(new EmoteGump(e.Mobile, EmotePage.P1));
                    break;
            }
            if (SoundInt > 0)
                new ESound(pm, SoundInt);
        }
    }
    public class EmoteGump : Gump
    {
        private Mobile m_From;
        private EmotePage m_Page;
        private const int Blanco = 0xFFFFFF;
        private const int Azul = 0x8080FF;
        public void AddPageButton(int x, int y, int buttonID, string text, EmotePage page, params EmotePage[] subpage)
        {
            bool seleccionado = (m_Page == page);
            for (int i = 0; !seleccionado && i < subpage.Length; ++i)
                seleccionado = (m_Page == subpage[i]);
            AddButton(x, y - 1, seleccionado ? 4006 : 4005, 4007, buttonID, GumpButtonType.Reply, 0);
            AddHtml(x + 35, y, 200, 20, Color(text, seleccionado ? Azul : Blanco), false, false);
        }
        public void AddButtonLabeled(int x, int y, int buttonID, string text)
        {
            AddButton(x, y - 1, 4005, 4007, buttonID, GumpButtonType.Reply, 0);
            AddHtml(x + 35, y, 240, 20, Color(text, Blanco), false, false);
        }
        public int GetButtonID(int type, int index)
        {
            return 1 + (index * 15) + type;
        }
        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        public EmoteGump(Mobile from, EmotePage page)
            : base(600, 50)
        {
            from.CloseGump(typeof(EmoteGump));
            m_From = from;
            m_Page = page;
            Closable = true;
            Dragable = true;
            AddPage(0);
            AddBackground(0, 65, 130, 360, 5054);
            AddAlphaRegion(10, 70, 110, 350);
            AddImageTiled(10, 70, 110, 20, 9354);
            AddLabel(13, 70, 200, "Sons RP");
            AddImage(100, 0, 10410);
            AddImage(100, 305, 10412);
            AddImage(100, 150, 10411);
            switch (page)
            {
                case EmotePage.P1:
                    {
                        AddButtonLabeled(10, 90, GetButtonID(1, 1), "ah");
                        AddButtonLabeled(10, 115, GetButtonID(1, 2), "ahha");
                        AddButtonLabeled(10, 140, GetButtonID(1, 3), "aplaude");
                        AddButtonLabeled(10, 165, GetButtonID(1, 4), "assoa");
                        AddButtonLabeled(10, 190, GetButtonID(1, 5), "reverencia");
                        AddButtonLabeled(10, 215, GetButtonID(1, 6), "tosseironica");
                        AddButtonLabeled(10, 240, GetButtonID(1, 7), "arrota");
                        AddButtonLabeled(10, 265, GetButtonID(1, 8), "limpagarganta");
                        AddButtonLabeled(10, 290, GetButtonID(1, 9), "tosse");
                        AddButtonLabeled(10, 315, GetButtonID(1, 10), "chora");
                        AddButtonLabeled(10, 340, GetButtonID(1, 11), "berra");
                        AddButtonLabeled(10, 365, GetButtonID(1, 12), "peida");
                        AddButton(70, 380, 4502, 0504, GetButtonID(0, 2), GumpButtonType.Reply, 0);
                        break;
                    }
                case EmotePage.P2:
                    {
                        AddButtonLabeled(10, 90, GetButtonID(1, 13), "arfa");
                        AddButtonLabeled(10, 115, GetButtonID(1, 14), "risadinha");
                        AddButtonLabeled(10, 140, GetButtonID(1, 15), "geme");
                        AddButtonLabeled(10, 165, GetButtonID(1, 16), "rosna");
                        AddButtonLabeled(10, 190, GetButtonID(1, 17), "hey");
                        AddButtonLabeled(10, 215, GetButtonID(1, 18), "soluca");
                        AddButtonLabeled(10, 240, GetButtonID(1, 19), "huh?");
                        AddButtonLabeled(10, 265, GetButtonID(1, 20), "beija");
                        AddButtonLabeled(10, 290, GetButtonID(1, 21), "ri");
                        AddButtonLabeled(10, 315, GetButtonID(1, 22), "nao");
                        AddButtonLabeled(10, 340, GetButtonID(1, 23), "oh");
                        AddButtonLabeled(10, 365, GetButtonID(1, 24), "oooh");
                        AddButton(10, 380, 4506, 4508, GetButtonID(0, 1), GumpButtonType.Reply, 0);
                        AddButton(70, 380, 4502, 0504, GetButtonID(0, 3), GumpButtonType.Reply, 0);
                        break;
                    }
                case EmotePage.P3:
                    {
                        AddButtonLabeled(10, 90, GetButtonID(1, 25), "oops");
                        AddButtonLabeled(10, 115, GetButtonID(1, 26), "vomita");
                        AddButtonLabeled(10, 140, GetButtonID(1, 27), "esmurra");
                        AddButtonLabeled(10, 165, GetButtonID(1, 28), "grita");
                        AddButtonLabeled(10, 190, GetButtonID(1, 29), "shhh!");
                        AddButtonLabeled(10, 215, GetButtonID(1, 30), "suspira");
                        AddButtonLabeled(10, 240, GetButtonID(1, 31), "estapeia");
                        AddButtonLabeled(10, 265, GetButtonID(1, 32), "espirra");
                        AddButtonLabeled(10, 290, GetButtonID(1, 33), "fareja");
                        AddButtonLabeled(10, 315, GetButtonID(1, 34), "ronca");
                        AddButtonLabeled(10, 340, GetButtonID(1, 35), "cospe");
                        AddButtonLabeled(10, 365, GetButtonID(1, 36), "estiraligua");
                        AddButton(10, 380, 4506, 4508, GetButtonID(0, 2), GumpButtonType.Reply, 0);
                        AddButton(70, 380, 4502, 0504, GetButtonID(0, 4), GumpButtonType.Reply, 0);
                        break;
                    }
                case EmotePage.P4:
                    {
                        AddButtonLabeled(10, 90, GetButtonID(1, 37), "batepe");
                        AddButtonLabeled(10, 115, GetButtonID(1, 38), "assobia");
                        AddButtonLabeled(10, 140, GetButtonID(1, 39), "woohoo");
                        AddButtonLabeled(10, 165, GetButtonID(1, 40), "boceja");
                        AddButtonLabeled(10, 190, GetButtonID(1, 41), "yea");
                        AddButton(10, 380, 4506, 4508, GetButtonID(0, 3), GumpButtonType.Reply, 0);
                        break;
                    }
            }
        }
        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {

            int val = info.ButtonID - 1;
            if (val < 0)
                return;

            Mobile from = m_From;
            int type = val % 15;
            int index = val / 15;

            switch (type)
            {
                case 0:
                    {
                        EmotePage page;
                        switch (index)
                        {
                            case 1: page = EmotePage.P1; break;
                            case 2: page = EmotePage.P2; break;
                            case 3: page = EmotePage.P3; break;
                            case 4: page = EmotePage.P4; break;
                            default: return;
                        }

                        from.SendGump(new EmoteGump(from, page));
                        break;
                    }
                case 1:
                    {
                        if (index > 0 && index < 13)
                        {
                            from.SendGump(new EmoteGump(from, EmotePage.P1));
                        }
                        else if (index > 12 && index < 25)
                        {
                            from.SendGump(new EmoteGump(from, EmotePage.P2));
                        }
                        else if (index > 24 && index < 37)
                        {
                            from.SendGump(new EmoteGump(from, EmotePage.P3));
                        }
                        else if (index > 36 && index < 43)
                        {
                            from.SendGump(new EmoteGump(from, EmotePage.P4));
                        }
                        new ESound(from, index);
                        break;
                    }
            }
        }
    }
    public class ItemRemovalTimer : Timer
    {
        private Item i_item;
        public ItemRemovalTimer(Item item)
            : base(TimeSpan.FromSeconds(180.0))
        {
            Priority = TimerPriority.OneSecond;
            i_item = item;
        }

        protected override void OnTick()
        {
            if ((i_item != null) && (!i_item.Deleted))
            {
                i_item.Delete();
                Stop();
            }
        }
    }

    public class Puke : Item
    {
        private Timer m_Timer;

        [Constructable]
        public Puke()
            : base(Utility.RandomList(0xf3b, 0xf3c))
        {
            Name = "vomito";
            Hue = 0x557;
            Movable = false;

            m_Timer = new ItemRemovalTimer(this);
            m_Timer.Start();

        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Timer != null)
                m_Timer.Stop();
        }

        public override void OnSingleClick(Mobile from)
        {
            this.LabelTo(from, this.Name);
        }

        public Puke(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            this.Delete(); // none when the world starts 
        }
    }

    public class ESound
    {

        public ESound(Mobile pm, int SoundMade)
        {
            switch (SoundMade)
            {
                case 1:
                    pm.PlaySound(pm.Female ? 778 : 1049);
                    pm.Say("suspira*");
                    break;
                case 2:
                    pm.PlaySound(pm.Female ? 779 : 1050);
                    pm.Say("*ah ha!*");
                    break;
                case 3:
                    pm.PlaySound(pm.Female ? 780 : 1051);
                    pm.Say("*apaude*");
                    break;
                case 4:
                    pm.PlaySound(pm.Female ? 781 : 1052);
                    pm.Say("*assoa o nariz*");
                    if (!pm.Mounted)
                        pm.Animate(34, 5, 1, true, false, 0);
                    break;
                case 5:
                    pm.Say("*faz reverencia*");
                    if (!pm.Mounted)
                        pm.Animate(32, 5, 1, true, false, 0);
                    break;
                case 6:
                    pm.PlaySound(pm.Female ? 786 : 1057);
                    pm.Say("*tosse irônicamente*");
                    break;
                case 7:
                    pm.PlaySound(pm.Female ? 782 : 1053);
                    pm.Say("*arrota!*");
                    if (!pm.Mounted)
                        pm.Animate(33, 5, 1, true, false, 0);
                    break;
                case 8:
                    pm.PlaySound(pm.Female ? 0x310 : 1055);
                    pm.Say("*limpa a garganta*");
                    if (!pm.Mounted)
                        pm.Animate(33, 5, 1, true, false, 0);
                    break;
                case 9:
                    pm.PlaySound(pm.Female ? 785 : 1056);
                    pm.Say("*tosse!*");
                    if (!pm.Mounted)
                        pm.Animate(33, 5, 1, true, false, 0);
                    break;
                case 10:
                    pm.PlaySound(pm.Female ? 787 : 1058);
                    pm.Say("*chora*");
                    break;
                case 11:
                    pm.PlaySound(pm.Female ? 0x338 : 1098);
                    pm.Say("*Berra!!*");
                    break;
                case 12:
                    pm.PlaySound(pm.Female ? 792 : 1064);
                    pm.Say("*peida*");
                    break;
                case 13:
                    pm.PlaySound(pm.Female ? 793 : 1065);
                    pm.Say("*arfa!*");
                    break;
                case 14:
                    pm.PlaySound(pm.Female ? 794 : 1066);
                    pm.Say("*ri*");
                    break;
                case 15:
                    pm.PlaySound(pm.Female ? 795 : 1067);
                    pm.Say("*geme*");
                    break;
                case 16:
                    pm.PlaySound(pm.Female ? 796 : 1068);
                    pm.Say("*rosna*");
                    break;
                case 17:
                    pm.PlaySound(pm.Female ? 797 : 1069);
                    pm.Say("*hey!*");
                    break;
                case 18:
                    pm.PlaySound(pm.Female ? 798 : 1070);
                    pm.Say("*soluça!*");
                    break;
                case 19:
                    pm.PlaySound(pm.Female ? 799 : 1071);
                    pm.Say("*huh?*");
                    break;
                case 20:
                    pm.PlaySound(pm.Female ? 800 : 1072);
                    pm.Say("*beija*");
                    break;
                case 21:
                    pm.PlaySound(pm.Female ? 801 : 1073);
                    pm.Say("*gargalha*");
                    break;
                case 22:
                    pm.PlaySound(pm.Female ? 802 : 1074);
                    pm.Say("*nao!*");
                    break;
                case 23:
                    pm.PlaySound(pm.Female ? 803 : 1075);
                    pm.Say("*oh!*");
                    break;
                case 24:
                    pm.PlaySound(pm.Female ? 811 : 1085);
                    pm.Say("*oooh*");
                    break;
                case 25:
                    pm.PlaySound(pm.Female ? 812 : 1086);
                    pm.Say("*oops*");
                    break;
                case 26:
                    pm.PlaySound(pm.Female ? 813 : 1087);
                    pm.Say("*vomita*");
                    if (pm.Hunger > 5)
                        pm.Hunger -= 5; 
                    if (!pm.Mounted)
                        pm.Animate(32, 5, 1, true, false, 0);
                    Point3D p = new Point3D(pm.Location);
                    switch (pm.Direction)
                    {
                        case Direction.North:
                            p.Y--; break;
                        case Direction.South:
                            p.Y++; break;
                        case Direction.East:
                            p.X++; break;
                        case Direction.West:
                            p.X--; break;
                        case Direction.Right:
                            p.X++; p.Y--; break;
                        case Direction.Down:
                            p.X++; p.Y++; break;
                        case Direction.Left:
                            p.X--; p.Y++; break;
                        case Direction.Up:
                            p.X--; p.Y--; break;
                        default:
                            break;
                    }
                    p.Z = pm.Map.GetAverageZ(p.X, p.Y);

                    bool canFit = Server.Spells.SpellHelper.AdjustField(ref p, pm.Map, 12, false);

                    if (canFit)
                    {
                        Puke puke = new Puke();
                        puke.Map = pm.Map;
                        puke.Location = p;
                    }
                    /*else
                        pm.SendMessage( "your puke won't fit!" ); /* Debug testing */
                    break;
                case 27:
                    pm.PlaySound(315);
                    pm.Say("*esmurra*");
                    if (!pm.Mounted)
                        pm.Animate(31, 5, 1, true, false, 0);
                    break;
                case 28:
                    pm.PlaySound(pm.Female ? 814 : 1088);
                    pm.Say("*ahhhh!*");
                    break;
                case 29:
                    pm.PlaySound(pm.Female ? 815 : 1089);
                    pm.Say("*shhh!*");
                    break;
                case 30:
                    pm.PlaySound(pm.Female ? 816 : 1090);
                    pm.Say("*suspira*");
                    break;
                case 31:
                    pm.PlaySound(948);
                    pm.Say("*estapeia*");
                    if (!pm.Mounted)
                        pm.Animate(11, 5, 1, true, false, 0);
                    break;
                case 32:
                    pm.PlaySound(pm.Female ? 817 : 1091);
                    pm.Say("*ATCHIN*");
                    if (!pm.Mounted)
                        pm.Animate(32, 5, 1, true, false, 0);
                    break;
                case 33:
                    pm.PlaySound(pm.Female ? 818 : 1092);
                    pm.Say("*fareja*");
                    if (!pm.Mounted)
                        pm.Animate(34, 5, 1, true, false, 0);
                    break;
                case 34:
                    pm.PlaySound(pm.Female ? 819 : 1093);
                    pm.Say("*ronca*");
                    break;
                case 35:
                    pm.PlaySound(pm.Female ? 820 : 1094);
                    pm.Say("*cospe*");
                    if (!pm.Mounted)
                        pm.Animate(6, 5, 1, true, false, 0);
                    break;
                case 36:
                    pm.PlaySound(792);
                    pm.Say("*Estira a ligua*");
                    break;
                case 37:
                    pm.PlaySound(874);
                    pm.Say("*bate o pé*");
                    if (!pm.Mounted)
                        pm.Animate(38, 5, 1, true, false, 0);
                    break;
                case 38:
                    pm.PlaySound(pm.Female ? 821 : 1095);
                    pm.Say("*assobia*");
                    if (!pm.Mounted)
                        pm.Animate(5, 5, 1, true, false, 0);
                    break;
                case 39:
                    pm.PlaySound(pm.Female ? 783 : 1054);
                    pm.Say("*woohoo!*");
                    break;
                case 40:
                    pm.PlaySound(pm.Female ? 822 : 1096);
                    pm.Say("*boceja*");
                    if (!pm.Mounted)
                        pm.Animate(17, 5, 1, true, false, 0);
                    break;
                case 41:
                    pm.PlaySound(pm.Female ? 823 : 1097);
                    pm.Say("*yea!*");
                    break;
            }
        }
    }
}
