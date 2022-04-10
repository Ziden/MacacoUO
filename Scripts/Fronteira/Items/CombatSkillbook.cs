using Server.Gumps;
using Server.Network;
using System.Collections.Generic;
using System;
using Server.Misc;
using Server.Engines.Points;

namespace Server.Items
{
    public class CombatSkillBook : Item
    {

        [CommandProperty(AccessLevel.Administrator)]
        public int Exp { get; set; } = 300;

        private class SkillRecord
        {
            public String name;
            public int idx;
            public SkillRecord(
                String new_name,
                int new_idx
            )
            {
                name = new_name;
                idx = new_idx;
            }
        }
        [Constructable]
        public CombatSkillBook()
         : base(0xEFA)
        {
            Weight = 3.0;
            Name = "Livro de Combate";
            Hue = 0xA33;
        }
        public CombatSkillBook(
            Serial serial
        )
            : base(serial)
        {
        }
        public override void Serialize(
            GenericWriter writer
        )
        {
            base.Serialize(writer);
            writer.Write(2);
            writer.Write(Exp);
        }
        public override void Deserialize(
            GenericReader reader
        )
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if(version > 1)
            {
                Exp = reader.ReadInt();
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            if(Shard.EXP)
                list.Add($"Receba +{Exp} Exp");
            else
                list.Add($"Escolha uma skill de combate para ganhar 0.5%");
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add("Livro de Combate");
        }
        public override void OnDoubleClick(
            Mobile from
        )
        {
            if (!this.IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001);
            }
            else
            {
                from.CloseAllGumps();
                from.PlaySound(0x55);

                if(Shard.EXP)
                {
                    var pontos = PointsSystem.Exp.GetPoints(from);
                    if(pontos + Exp > PointsSystem.Exp.MaxPoints)
                    {
                        from.SendMessage("Voce ultrapassaria os " + PointsSystem.Exp.MaxPoints + " pontos de XP");
                        from.SendMessage("Por favor use seus pontos antes de usar um livro de XP novamente");
                        return;
                    }
                    PointsSystem.Exp.AwardPoints(from, Exp);
                    from.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);
                    from.PlaySound(0x1FD);
                    Consume();
                    return;
                }

                from.SendGump(new InternalGump(from, this));
            }
        }
        private class InternalGump : Gump
        {
            private readonly Mobile m_From;
            private readonly CombatSkillBook m_book;
            private int m_page;
            private int m_start;
            private int m_end;
            private SkillRecord[] m_skills;
            private static readonly int max_count = 8;
            private static readonly int double_max = max_count * 2;
            private static readonly int count_offset = 3;
            private static readonly int num_skills = Enum.GetNames(typeof(SkillName)).Length;
            private static readonly int max_pages = (num_skills / double_max);
            private static readonly int v_spacing = 20;
            private static readonly int x_pos = 140;
            private static readonly int y_pos = 44;

            public InternalGump(
                Mobile from,
                CombatSkillBook book,
                int page = 0
            )
                : base(150, 200)
            {
                m_From = from;

                m_book = book;

                m_page = page;

                m_skills = GetSkills(from);

                m_start = m_page * double_max;

                m_end = (
                    m_start + max_count
                ) > num_skills ? num_skills : m_start + max_count;
                AddImage(100, 10, 2200);
                if (m_page > 0)
                {
                    AddButton(125, 14, 2205, 2205, 1, GumpButtonType.Reply, 0);
                }
                if (m_page < max_pages)
                {
                    AddButton(393, 14, 2206, 2206, 2, GumpButtonType.Reply, 0);
                }
                for (int j = 0; j < 2; j++)
                {
                    int adj_start = m_start + j * max_count;
                    int adj_end = m_end + j * max_count;
                    adj_end = adj_end > num_skills ? num_skills : adj_end;
                    for (int i = adj_start; i < adj_end; i++)
                    {
                        AddButton(
                            x_pos + 160 * j,
                            y_pos + (i % max_count) * v_spacing,
                            2103,
                            2104,
                            count_offset + i,
                            GumpButtonType.Reply,
                            0
                        );
                        AddHtml(
                            x_pos + 160 * j + 15,
                            y_pos + (i % max_count) * v_spacing - 4,
                            100,
                            35,
                            formatSkillName(m_skills[i].name),
                            false,
                            false
                        );
                    }
                }
            }

            private String formatSkillName(String strToFormat)
            {
                if (strToFormat.Contains(" "))
                {
                    String[] comp = strToFormat.Split(' ');
                    String tempString = "";
                    foreach (String c in comp)
                    {
                        if (c.Length > 6)
                        {
                            String abbrString = "";
                            if ("aeiou".Contains(c.Substring(2, 1)))
                            {
                                abbrString = c.Substring(0, 4) + ".";
                            }
                            else
                            {
                                abbrString = c.Substring(0, 3) + ".";
                            }
                            if ("fig.for.".Contains(abbrString.ToLower()))
                            {
                                abbrString = c.Substring(0, 5) + ".";
                            }
                            tempString += abbrString;
                        }
                        else
                        {
                            tempString += c;
                        }
                        tempString += " ";
                    }
                    strToFormat = tempString.Substring(0, tempString.Length - 1);
                }
                else if (strToFormat.Contains("/"))
                {
                    strToFormat = strToFormat.Split('/')[0];
                }
                return strToFormat;
            }
            private SkillRecord[] GetSkills(Mobile from)
            {
                SkillRecord[] skills = new SkillRecord[num_skills];
                for (int i = 0; i < num_skills; i++)
                {
                    skills[i] = new SkillRecord(from.Skills[i].Name, i);
                }
                Array.Sort(skills, delegate (SkillRecord l, SkillRecord r)
                {
                    return l.name.CompareTo(r.name);
                });
                return skills;
            }

            public static List<SkillName> Validas = new List<SkillName>(new SkillName[] {
                SkillName.Fencing, SkillName.Swords, SkillName.Macing, SkillName.Wrestling,
                SkillName.Tactics, SkillName.Anatomy, SkillName.Magery, SkillName.MagicResist,
                SkillName.Chivalry, SkillName.Necromancy, SkillName.Ninjitsu, SkillName.Healing,
                SkillName.Musicianship, SkillName.Peacemaking, SkillName.Provocation, SkillName.Discordance,
                SkillName.Archery, SkillName.DetectHidden, SkillName.Lockpicking, SkillName.Parry, SkillName.Poisoning
            });

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                Mobile from = sender.Mobile;
                switch (info.ButtonID)
                {
                    case 0:

                        break;
                    case 1:
                        from.PlaySound(0x55);
                        from.SendGump(new InternalGump(from, m_book, m_page - 1));
                        break;
                    case 2:
                        from.PlaySound(0x55);
                        from.SendGump(new InternalGump(from, m_book, m_page + 1));
                        break;
                    default:
                        int idx = info.ButtonID - count_offset;
                        int s_idx = m_skills[idx].idx;

                        var skill = from.Skills[s_idx];
                        if (!Validas.Contains(skill.SkillName))
                        {
                            from.SendMessage("Voce apenas pode escolher skills de combate");
                            return;
                        }
                        var bonus = 5;
                        from.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);
                        from.PlaySound(0x1FD);

                        for (var x = 0; x < bonus; x++)
                            SkillCheck.Gain(from, from.Skills[s_idx]);

                        m_book.Delete();
                        break;
                }
            }
        }
    }
}
