using System;
using Server.Engines.CannedEvil;
using Server.Items;
using Server.Regions;

namespace Server.Mobiles
{
    public class Paragon
    {
        public static double ChestChance = .25;// Chance that a paragon will carry a paragon chest
        public static double ChocolateIngredientChance = .20;// Chance that a paragon will drop a chocolatiering ingredient

        public static int Hue = 0x501;// Paragon hue

        // Buffs
        public static double HitsBuff = 10;
        public static double StrBuff = 1.05;
        public static double IntBuff = 1.20;
        public static double DexBuff = 1.30;
        public static double SkillsBuff = 1.20;
        public static double SpeedBuff = 1.20;
        public static double FameBuff = 1.40;
        public static double KarmaBuff = 1.40;
        public static int DamageBuff = 10;

        public static bool ValidParagon(BaseCreature bc)
        {
            if (bc.IsParagon ||
             !bc.CanBeParagon)
                return false;

            if (!bc.Murderer)
                return false;

            if (bc.Region is ChampionSpawnRegion)
                return false;

            if (bc.IsChampionSpawn)
                return false;

            return true;
        }

        public static void Convert(BaseCreature bc)
        {
            if (!ValidParagon(bc))
                return;

            bc.Hue = Hue;

            if (bc.HitsMaxSeed >= 0)
                bc.HitsMaxSeed = (int)(bc.HitsMaxSeed * HitsBuff);

            bc.RawStr = (int)(bc.RawStr * StrBuff);
            bc.RawInt = (int)(bc.RawInt * IntBuff);
            bc.RawDex = (int)(bc.RawDex * DexBuff);

            bc.Hits = bc.HitsMax;
            bc.Mana = bc.ManaMax;
            bc.Stam = bc.StamMax;

            for (int i = 0; i < bc.Skills.Length; i++)
            {
                Skill skill = (Skill)bc.Skills[i];

                if (skill.Base > 0.0)
                    skill.Base *= SkillsBuff;
            }

            bc.PassiveSpeed /= SpeedBuff;
            bc.ActiveSpeed /= SpeedBuff;
            bc.CurrentSpeed = bc.PassiveSpeed;

            bc.DamageMin += DamageBuff;
            bc.DamageMax += DamageBuff;

            if (bc.Fame > 0)
                bc.Fame = (int)(bc.Fame * FameBuff);

            if (bc.Fame > 32000)
                bc.Fame = 32000;

            // TODO: Mana regeneration rate = Sqrt( buffedFame ) / 4

            if (bc.Karma != 0)
            {
                bc.Karma = (int)(bc.Karma * KarmaBuff);

                if (Math.Abs(bc.Karma) > 32000)
                    bc.Karma = 32000 * Math.Sign(bc.Karma);
            }
        }

        public static void UnConvert(BaseCreature bc)
        {
            if (!bc.IsParagon)
                return;

            bc.Hue = 0;

            if (bc.HitsMaxSeed >= 0)
                bc.HitsMaxSeed = (int)(bc.HitsMaxSeed / HitsBuff);

            bc.RawStr = (int)(bc.RawStr / StrBuff);
            bc.RawInt = (int)(bc.RawInt / IntBuff);
            bc.RawDex = (int)(bc.RawDex / DexBuff);

            bc.Hits = bc.HitsMax;
            bc.Mana = bc.ManaMax;
            bc.Stam = bc.StamMax;

            for (int i = 0; i < bc.Skills.Length; i++)
            {
                Skill skill = (Skill)bc.Skills[i];

                if (skill.Base > 0.0)
                    skill.Base /= SkillsBuff;
            }

            bc.PassiveSpeed *= SpeedBuff;
            bc.ActiveSpeed *= SpeedBuff;
            bc.CurrentSpeed = bc.PassiveSpeed;

            bc.DamageMin -= DamageBuff;
            bc.DamageMax -= DamageBuff;

            if (bc.Fame > 0)
                bc.Fame = (int)(bc.Fame / FameBuff);
            if (bc.Karma != 0)
                bc.Karma = (int)(bc.Karma / KarmaBuff);
        }

        public static bool CheckConvert(BaseCreature bc)
        {
            return CheckConvert(bc, bc.Location, bc.Map);
        }

        public static bool CheckConvert(BaseCreature bc, Point3D location, Map m)
        {
            //if (!Core.AOS)
            //    return false;

            //if (Array.IndexOf(Maps, m) == -1)
            //    return false;

            if (bc is BaseChampion || bc is Harrower || bc is BaseVendor || bc is BaseEscortable || bc is Clone || bc.IsParagon)
                return false;

            if (bc.Region is DungeonProtectedRegion)
                return false;

            if (bc.IsChampionSpawn)
                return false;

            if (bc.Region is ChampionSpawnRegion)
                return false;

            return (0.02 > Utility.RandomDouble());
        }

        public static bool CheckArtifactChance(Mobile m, BaseCreature bc)
        {
            //if (!Core.AOS)
            //    return false;

            double fame = (double)bc.Fame;

            if (fame > 32000)
                fame = 32000;

            int luck = m is PlayerMobile ? ((PlayerMobile)m).RealLuck : m.Luck;

            double chance = 1 / (Math.Max(10, 100 * (0.83 - Math.Round(Math.Log(Math.Round(fame / 6000, 3) + 0.001, 10), 3))) * (100 - Math.Sqrt(luck)) / 100.0);

            return chance > Utility.RandomDouble();
        }

        public static void GiveArtifactTo(Mobile m, BaseCreature bc)
        {
            var name = bc.Name;
            if(name != null && m is PlayerMobile && bc.Name != null)
            {
                ((PlayerMobile)m).AddRewardTitle("Matador de " + bc.Name+" Paragon");
                m.SendMessage(78, "Voce ganhou um titulo !");
                m.SendMessage(78, "Use .titulos para ver seus titulos !");
            }

            Item i = null;
            if (Utility.RandomDouble() < 0.2)
            {
                i = Decos._RandomDecoComum();
                i.Hue = Paragon.Hue;
                i.Name = "Artefato banhado a ouro";
            }
            else if (Utility.RandomBool())
            {
                if(Utility.RandomDouble() < 0.2)
                {
                    i = Loot.RandomInstrument();
                    ((IResource)i).Resource = CraftResource.Mogno;
                    ((IQuality)i).Quality = ItemQuality.Exceptional;
                    i.Hue = Paragon.Hue;
                } else
                {
                    i = Loot.RandomWeapon();
                    BaseWeapon weapon = (BaseWeapon)i;
                    ((IResource)i).Resource = CraftResource.Dourado;
                    ((IQuality)i).Quality = ItemQuality.Exceptional;
                    weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(6);
                    weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(6);
                    weapon.DurabilityLevel = (WeaponDurabilityLevel)Utility.Random(6);
                }
            } else
            {
                i = Loot.RandomArmor();
                BaseArmor armor = (BaseArmor)i;
                ((IResource)i).Resource = CraftResource.Dourado;
                ((IQuality)i).Quality = ItemQuality.Exceptional;
                armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(6);
                armor.Durability = (ArmorDurabilityLevel)Utility.Random(6);
            }

            if (i.Name != null)
                i.Name = i.Name + " Paragon";

            if (m.AddToBackpack(i))
                m.SendMessage("Voce recebeu um item dourado.");
        }
    }
}
