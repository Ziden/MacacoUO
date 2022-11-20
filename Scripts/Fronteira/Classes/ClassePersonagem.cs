using Server.Fronteira.Talentos;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Fronteira.Classes
{

    public class SkillClasse
    {
        public ushort Cap;
        public SkillName skill;

        public SkillClasse(SkillName n, ushort c)
        {
            this.Cap = c;
            this.skill = n;
        }
    }

    public class OpcaoTalentos
    {
        public Talento T1 { get { return Talentos[0]; } }
        public Talento T2 { get { return Talentos[1]; } }
        public Talento T3 { get { return Talentos[2]; } }

        public Talento[] Talentos;

        public OpcaoTalentos(Talento t1, Talento t2, Talento t3)
        {
            Talentos = new Talento[] { t1, t2, t3 };
        }
    }

    public class ClassePersonagem
    {
        public int ID;
        public string Nome;
        public int Icone;
        public string Desc;

        public Dictionary<SkillName, ushort> ClassSkills = new Dictionary<SkillName, ushort>();
        public OpcaoTalentos[] Talentos;
        public Item[] ItemsIniciais; 

        public ClassePersonagem(string nome, int icone, string descricao, params SkillClasse[] skills)
        {
            this.Nome = nome;
            foreach (var s in skills)
            {
                if (ClassSkills.ContainsKey(s.skill)) throw new Exception("Classe " + nome + " ja tinha a skill " + s.skill);
                ClassSkills.Add(s.skill, s.Cap);
            }
            this.Icone = icone;
            this.Desc = descricao;
        }

        public ClassePersonagem ComItemsIniciais(params Item[] items)
        {
            ItemsIniciais = items;
            return this;
        }

        public void ViraClasse(PlayerMobile player)
        {
            player.Skills.Cap = int.MaxValue;
            foreach(var skill in player.Skills)
            {
                if(ClassSkills.ContainsKey(skill.SkillName))
                {
                    skill.Cap = ClassSkills[skill.SkillName];
                } else
                {
                    skill.Cap = 0;
                    skill.Base = 0;
                }
            }
            player.Profession = ID;
            player.SendMessage("Voce agora e da classe " + this.Nome);
        }
    }
}
