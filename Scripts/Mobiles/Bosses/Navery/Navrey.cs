using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Engines.Quests;
using Server.Fronteira.Elementos;
using Server.Items;
using Server.Ziden;
using Server.Ziden.Dungeons.Goblins.Quest;

namespace Server.Mobiles
{
    [CorpseName("a navrey corpse")]
    public class Navrey : BaseCreature
    {
        private NavreysController m_Spawner;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UsedPillars { get; set; }

        private static readonly Type[] m_Artifact = new Type[]
        {
            typeof(NightEyes),
            typeof(Tangle1)
        };		
        
        [Constructable]
        public Navrey(NavreysController spawner)
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.1, 0.2)
        {
            m_Spawner = spawner;

            Name = "Navrey Night-Eyes";
            Body = 735;
            BaseSoundID = 389;

            SetStr(1000, 1500);
            SetDex(200, 250);
            SetInt(150, 200);

            SetHits(30000, 35000);

            SetDamage(25, 40);

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Fire, 25);
            SetDamageType(ResistanceType.Energy, 25);

            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Fire, 45, 55);
            SetResistance(ResistanceType.Cold, 60, 70);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 65, 80);

            SetSkill(SkillName.Anatomy, 50.0, 80.0);
            SetSkill(SkillName.EvalInt, 90.0, 100.0);
            SetSkill(SkillName.Magery, 90.0, 100.0);
            SetSkill(SkillName.MagicResist, 100.0, 130.0);
            SetSkill(SkillName.Meditation, 80.0, 100.0);
            SetSkill(SkillName.Poisoning, 100.0);
            SetSkill(SkillName.Tactics, 90.0, 100.0);
            SetSkill(SkillName.Wrestling, 91.6, 98.2);

            Fame = 24000;
            Karma = -24000;

            VirtualArmor = 90;

            for (int i = 0; i < Utility.RandomMinMax(1, 3); i++)
            {
                PackItem(Loot.RandomScroll(0, Loot.MysticismScrollTypes.Length, SpellbookType.Mystic));
            }

            SetSpecialAbility(SpecialAbility.Webbing);
        }

        public Navrey(Serial serial)
            : base(serial)
        {
        }
 
        public override double TeleportChance { get { return 0; } }
	    public override bool AlwaysMurderer { get { return true; } }
        public override Poison PoisonImmune { get { return Poison.Parasitic; } }
        public override Poison HitPoison { get { return Poison.Lethal; } }
        public override int Meat { get { return 1; } }

        public static void DistributeRandomArtifact(BaseCreature bc, Type[] typelist)
        {
            int random = Utility.Random(typelist.Length);
            Item item = Loot.Construct(typelist[random]);
            DistributeArtifact(DemonKnight.FindRandomPlayer(bc), item);
        }

        public static void DistributeArtifact(Mobile to, Item artifact)
        {
            if (artifact == null)
                return;

            if (to != null)
            {
                Container pack = to.Backpack;

                if (pack == null || !pack.TryDropItem(to, artifact, false))
                    to.BankBox.DropItem(artifact);

                to.SendLocalizedMessage(502088); // A special gift has been placed in your backpack.
            }
            else
            {
                artifact.Delete();
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.AosSuperBoss, 3);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            SorteiaItem(new BagOfSending());
            SorteiaItem(Carnage.GetRandomPS(105));
            SorteiaItem(Carnage.GetRandomPS(105));
            SorteiaItem(Carnage.GetRandomPS(105));
            SorteiaItem(Carnage.GetRandomPS(110));
            SorteiaItem(Carnage.GetRandomPS(110));
            DistribuiItem(new CristalDoPoder() { Amount = 10 });
            SorteiaItem(DefJewelcrafting.GetReceitaPower());
            SorteiaItem(DefJewelcrafting.GetReceitaPower());
            SorteiaItem(DefJewelcrafting.GetRandomReceitaNoob());
            DistribuiItem(new FragmentosAntigos());
            DistribuiItem(new FragmentosAntigos());
            var sandalia = new Sandals();
            sandalia.Hue = 1161;
            sandalia.Name = "[Boss][Navrey] Sandalias da Humildade";
            sandalia.Attributes.WeaponDamage = 10;
            SorteiaItem(sandalia);
            SorteiaItem(new PergaminhoPeso());

            GolemMecanico.JorraOuro(this.Location, this.Map, 1000);
            DistribuiItem(new CaixaDeGold());
            for (var x= 0; x < 10; x++)
            {
                SorteiaItem(ElementoUtils.GetRandomPedraSuperior(10));
            }
            DistribuiItem(Decos.RandomDeco(this));

            if (m_Spawner != null)
                m_Spawner.OnNavreyKilled();

            if (Utility.RandomBool())
                SorteiaItem(new UntranslatedAncientTome());

            if (0.3 >= Utility.RandomDouble())
                SorteiaItem(ScrollOfTranscendence.CreateRandom(30, 30));

            if (0.3 >= Utility.RandomDouble())
                SorteiaItem(new TatteredAncientScroll());

            if (Utility.RandomDouble() < 0.1)
                SorteiaItem(new LuckyCoin());

            if (Utility.RandomDouble() < 0.2)
                DistributeRandomArtifact(this, m_Artifact);

            // distribute quest items for the 'Green with Envy' quest given by Vernix
            List<DamageStore> rights = GetLootingRights();
            for (int i = rights.Count - 1; i >= 0; --i)
            {
                DamageStore ds = rights[i];
                if (!ds.m_HasRight)
                    rights.RemoveAt(i);
            }

            // for each with looting rights... give an eye of navrey if they have the quest
            foreach (DamageStore d in rights)
            {
                PlayerMobile pm = d.m_Mobile as PlayerMobile;
                if (null != pm)
                {
                    foreach (BaseQuest quest in pm.Quests)
                    {
                        if (quest is GreenWithEnvyQuest)
                        {
                            Container pack = pm.Backpack;
                            Item item = new EyeOfNavrey();
                            if (pack == null || !pack.TryDropItem(pm, item, false))
                                pm.BankBox.DropItem(item);
                            pm.SendLocalizedMessage(1095155); // As Navrey Night-Eyes dies, you find and claim one of her eyes as proof of her demise.
                            break;
                        }
                    }
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);

            writer.Write((Item)m_Spawner);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version >= 1)
                m_Spawner = reader.ReadItem() as NavreysController;
        }
    }
}
