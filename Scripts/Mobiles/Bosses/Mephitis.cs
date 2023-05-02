using System;
using Server.Engines.CannedEvil;
using Server.Fronteira.Elementos;
using Server.Items;

namespace Server.Mobiles
{
    public class Mephitis : BaseChampion
    {
        [Constructable]
        public Mephitis()
            : base(AIType.AI_Melee)
        {
            Body = 173;
            Name = "Mephitis";

            BaseSoundID = 0x183;

            SetStr(505, 1000);
            SetDex(102, 300);
            SetInt(402, 600);

            SetHits(12000);
            SetStam(105, 600);

            SetDamage(21, 33);

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Poison, 50);

            SetResistance(ResistanceType.Physical, 75, 80);
            SetResistance(ResistanceType.Fire, 60, 70);
            SetResistance(ResistanceType.Cold, 60, 70);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 60, 70);

            SetSkill(SkillName.MagicResist, 70.7, 140.0);
            SetSkill(SkillName.Tactics, 97.6, 100.0);
            SetSkill(SkillName.Wrestling, 97.6, 100.0);

            Fame = 122500;
            Karma = -122500;

            VirtualArmor = 80;

            SetSpecialAbility(SpecialAbility.Webbing);
        }

        public Mephitis(Serial serial)
            : base(serial)
        {
        }

        public override void AlterMeleeDamageTo(Mobile to, ref int damage)
        {
            base.AlterMeleeDamageTo(to, ref damage);
            if (to is BaseCreature)
                damage *= 3;
        }

        public override ChampionSkullType SkullType
        {
            get
            {
                return ChampionSkullType.Venom;
            }
        }
        public override Type[] UniqueList
        {
            get
            {
                return new Type[] { typeof(Calm) };
            }
        }
        public override Type[] SharedList
        {
            get
            {
                return new Type[] { typeof(ANecromancerShroud) };
            }
        }
        public override Type[] DecorativeList
        {
            get
            {
                return new Type[] { typeof(Web), typeof(MonsterStatuette) };
            }
        }
        public override MonsterStatuetteType[] StatueTypes
        {
            get
            {
                return new MonsterStatuetteType[] { MonsterStatuetteType.Spider };
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV7, 1);
            this.AddLoot(LootPack.Gems, 50);
        }
        public virtual int BonusExp => 900;

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            SorteiaItem(Carnage.GetRandomPS(110));
            SorteiaItem(Carnage.GetRandomPS(110));
            SorteiaItem(Carnage.GetRandomPS(110));
            SorteiaItem(Decos.RandomDecoRara(this));
            //SorteiaItem(Decos.RandomDecoRara(this));
            DistribuiPs(105);
            if(Utility.RandomDouble() < 0.1)
                SorteiaItem(Carnage.GetRandomPS(115));
            SorteiaItem(new StoneStatueDeed());
            SorteiaItem(new WarriorStatueEastDeed());
            SorteiaItem(new HalloweenSpiderForestAddonDeed());
            //DistribuiItem(new Web());
            if (Utility.RandomBool())
            {
                var s = new MinersSatchel();
                s.WeightReduction = 50;
                SorteiaItem(s);
            }
            else
            {
                var s = new LumbjacksSatchel();
                s.WeightReduction = 50;
                SorteiaItem(s);
            }

            for (var x = 0; x < 5; x++)
            {
                SorteiaItem(BaseEssencia.RandomEssencia(5));
                SorteiaItem(ElementoUtils.GetRandomPedraSuperior(5));
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
