using Server.Menus.Questions;
using Server.Mobiles;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Fronteira.Quests
{
    public class Tamavel : IEquatable<Tamavel>
    {

        private static HashSet<Tamavel> l50 = new HashSet<Tamavel>();
        private static HashSet<Tamavel> l70 = new HashSet<Tamavel>();
        private static HashSet<Tamavel> l80 = new HashSet<Tamavel>();
        private static HashSet<Tamavel> l90 = new HashSet<Tamavel>();
        private static HashSet<Tamavel> l100 = new HashSet<Tamavel>();
        private static HashSet<Tamavel> l120 = new HashSet<Tamavel>();

        private static Tamavel Randomiza(HashSet<Tamavel> lista, double skill)
        {
            if (lista.Count == 0)
            {
                if (lista == l70)  return Randomiza(l50, skill);
                if (lista == l80)  return Randomiza(l70, skill);
                if (lista == l90)  return Randomiza(l80, skill);
                if (lista == l100) return Randomiza(l90, skill);
                if (lista == l120) return Randomiza(l100, skill);
            }
            var filtrada = lista.Where(e => e.Skill >= skill - 20);
            if(filtrada.Count()==0)
            {
                return l70.TakeRandom(1).First();
            }
            return filtrada.ElementAt(Utility.Random(filtrada.Count()));
        }

        public static Tuple<Tamavel, int> Sorteia(double skill)
        {
            if (skill > 100)
                if (Utility.RandomDouble() < 0.35)
                    return new Tuple<Tamavel, int>(Randomiza(l80, skill), 6);
                else if (Utility.RandomBool())
                    return new Tuple<Tamavel, int>(Randomiza(l120, skill), 3);
                else
                    return new Tuple<Tamavel, int>(Randomiza(l100, skill), 6);
            else if (skill > 90)
                if (Utility.RandomDouble() < 0.35)
                    return new Tuple<Tamavel, int>(Randomiza(l80, skill), 8);
                else
                    return new Tuple<Tamavel, int>(Randomiza(l90, skill), 6);
            else if (skill > 80)
                if (Utility.RandomDouble() < 0.35)
                    return new Tuple<Tamavel, int>(Randomiza(l70, skill), 8);
                else
                    return new Tuple<Tamavel, int>(Randomiza(l80, skill), 6);
            else if (skill >= 70)
                if (Utility.RandomDouble() < 0.35)
                    return new Tuple<Tamavel, int>(Randomiza(l50, skill), 15);
                else
                    return new Tuple<Tamavel, int>(Randomiza(l70, skill), 6);
            return new Tuple<Tamavel, int>(Randomiza(l50, skill), 5);
        }

        // registra tds possiveis bixos q tem no shard
        public static void RegistraBixo(BaseCreature bc)
        {
            if (bc == null || bc.Map != Map.Trammel)
                return;

            if (bc.ControlMaster != null)
                return;

            if (!Shard.T2A && StuckMenu.IsInSecondAgeArea(bc))
                return;

            if (bc.Owners != null && bc.Owners.Count > 0)
                return;

            if (bc != null && bc.Tamable)
            {
                if (bc.MinTameSkill <= 50 && !(bc.Region is DungeonRegion) && !StuckMenu.IsInSecondAgeArea(bc))
                    l50.Add(new Tamavel(bc));
                else if (bc.MinTameSkill < 70 && !(bc.Region is DungeonRegion) && !StuckMenu.IsInSecondAgeArea(bc))
                    l70.Add(new Tamavel(bc));
                else if (bc.MinTameSkill < 80)
                    l80.Add(new Tamavel(bc));
                else if (bc.MinTameSkill < 90)
                    l90.Add(new Tamavel(bc));
                else if (bc.MinTameSkill < 100)
                    l100.Add(new Tamavel(bc));
                else if (bc.MinTameSkill <= 120)
                    l120.Add(new Tamavel(bc));

                Shard.Debug("Registrei bixo taming", bc);
            }
        }

        public string Name;
        public int Hue;
        public double Skill;
        public int Body;
        public Type tipo;

        public Tamavel(BaseCreature bc)
        {
            Hue = bc.Hue;
            Name = bc.Name;
            Skill = bc.MinTameSkill;
            Body = bc.Body;
            tipo = bc.GetType();
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + Name.GetHashCode();
            hash = (hash * 7) + Hue.GetHashCode();
            return hash;
        }

        public bool Equals(Tamavel other)
        {
            return other.Name == this.Name && other.Hue == this.Hue;
        }
    }
}
