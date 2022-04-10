using System;
using System.Collections;
using Server.Gumps.Newbie;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a plant corpse")]
    public class RestoDeMerda : BaseCreature
    {
        [Constructable]
        public RestoDeMerda()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.6, 1.2)
        {
            this.Name = "monstro seboso";
            this.Body = 780;

            this.SetStr(200, 200);
            this.SetDex(46, 65);
            this.SetInt(36, 50);

            this.SetHits(80, 120);
            this.SetMana(0);

            this.SetDamage(5, 15);

            this.SetDamageType(ResistanceType.Physical, 60);
            this.SetDamageType(ResistanceType.Poison, 40);

            this.SetResistance(ResistanceType.Physical, 30, 40);
            this.SetResistance(ResistanceType.Fire, 20, 25);
            this.SetResistance(ResistanceType.Cold, 10, 15);
            this.SetResistance(ResistanceType.Poison, 40, 50);
            this.SetResistance(ResistanceType.Energy, 20, 25);

            this.SetSkill(SkillName.MagicResist, 0, 0);
            this.SetSkill(SkillName.Tactics, 70.1, 85.0);
            this.SetSkill(SkillName.Wrestling, 50, 51);

            this.Fame = 8000;
            this.Karma = -8000;

            this.VirtualArmor = 0;

            var c = new SmallCrate();
            c.Resource = CraftResource.Pinho;
            var livro = new RedBook(1, false);
            livro.Title = "Livro Seboso";
            livro.Author = "Jill a velha";
            livro.Pages[0].Lines = new string[] {
               "Ca estou, perto do banco de Haven",
               "Aguardando meu velho Zeh com meu cajado",
               "Hoje o dia choveu. Preciso do meu cajado para nao escorregar na lama."
            };
            c.DropItem(livro);
            this.PackItem(c);



            this.PackReg(3);
        }

        public RestoDeMerda(Serial serial)
            : base(serial)
        {
        }

        public override bool BardImmune
        {
            get
            {
                return !Core.AOS;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV3, 3);
            AddPackedLoot(LootPack.MeagerProvisions, typeof(Bag));
            AddPackedLoot(LootPack.PoorProvisions, typeof(Bag));
        }

        public override bool OnBeforeDeath()
        {
            var manolos = this.GetLootingRights();
            foreach (var r in manolos)
            {
                if (r.m_HasRight && r.m_Mobile != null && r.m_Mobile is PlayerMobile)
                {
                    var p = (PlayerMobile)r.m_Mobile;
                    if(p.Young && p.Wisp != null)
                    {
                        switch (p.Profession)
                        {
                            case 4://StarterKits.ARCHER:
                                var loot = new Bow();
                                loot.Quality = ItemQuality.Exceptional;
                                loot.Resource = CraftResource.Carvalho;
                                loot.Attributes.BonusDex = -1;
                                //loot.Hue = 1444;
                                loot.Name = "Arco pegajoso";
                                loot.Owner = p;
                                AddItem(loot);
                                var b = new BraceleteDoPoder();
                                b.Attributes.WeaponDamage = 3;
                                AddItem(b);
                                break;
                            case 3:// StarterKits.BARD:
                            case 5:// StarterKits.TAMER:
                                var spear = new ShortSpear();
                                spear.Quality = ItemQuality.Exceptional;
                                spear.Resource = CraftResource.Cobre;
                                spear.Attributes.BonusDex = -1;
                                //spear.Hue = 1444;
                                spear.Name = "Lanca pegajosa";
                                spear.Owner = p;
                                AddItem(spear);
                                var b2 = new BraceleteDoPoder();
                                b2.Attributes.WeaponSkillDamage = 4;
                                AddItem(b2);
                                break;
                            case 2://  StarterKits.BS:
                                var marreta = new WarHammer();
                                marreta.Quality = ItemQuality.Exceptional;
                                marreta.Attributes.BonusDex = -1;
                                marreta.Resource = CraftResource.Cobre;
                                //marreta.Hue = 1444;
                                marreta.Name = "Marretona pegajosa";
                                marreta.Owner = p;
                                AddItem(marreta);
                                var b3 = new BraceleteDoPoder();
                                b3.Attributes.WeaponSkillDamage = 4;
                                AddItem(b3);
                                break;
                            case 6:// StarterKits.MAGE
                                var cajado = new QuarterStaff();
                                cajado.Quality = ItemQuality.Exceptional;
                                cajado.Attributes.BonusDex = -1;
                                cajado.Resource = CraftResource.Carvalho;
                                //cajado.Hue = 1444;
                                cajado.Name = "Cajado pegajoso";
                                cajado.Owner = p;
                                AddItem(cajado);
                                AddItem(new BagOfReagents());
                                var b4 = new BraceleteDoPoder();
                                b4.Attributes.WeaponSkillDamage = 4;
                                AddItem(b4);
                                if (p.Young)
                                {
                                    p._PlaceInBackpack(new EnergyBoltScroll());
                                }
                                break;
                            default:
                                var espada = new VikingSword();
                                espada.Quality = ItemQuality.Exceptional;
                                espada.Attributes.BonusDex = -1;
                                //espada.Hue = 1444;
                                espada.Name = "Espada pegajosa";
                                espada.Owner = p;
                                AddItem(espada);
                                AddItem(new BagOfReagents());
                                var b5 = new BraceleteDoPoder();
                                b5.Attributes.WeaponSkillDamage = 4;
                                AddItem(b5);
                                break;
                        }
                    }
                   

                    if (p.Wisp != null)
                    {
                        p.Wisp.MataMerda();
                        AddItem(new Gold(100 + Utility.Random(100)));
                    }
                }
            }
            return base.OnBeforeDeath();
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
           
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
        }

        public Bogling bog;

        public void SpawnBogling(Mobile m)
        {
            if (bog != null && (bog.Alive || !bog.Deleted))
            {
                if(bog != null && bog.GetDistance(this) > 10)
                {
                    bog.MoveToWorld(this.Location, this.Map);
                    bog.Combatant = m;
                }
                return;
            }


            Map map = this.Map;

            if (map == null)
                return;

            bog = new Bogling();

            bog.Team = this.Team;

            bool validLocation = false;
            Point3D loc = this.Location;

            for (int j = 0; !validLocation && j < 10; ++j)
            {
                int x = this.X + Utility.Random(3) - 1;
                int y = this.Y + Utility.Random(3) - 1;
                int z = map.GetAverageZ(x, y);

                if (validLocation = map.CanFit(x, y, this.Z, 16, false, false))
                    loc = new Point3D(x, y, this.Z);
                else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                    loc = new Point3D(x, y, z);
            }

            bog.MoveToWorld(loc, map);
            bog.Combatant = m;
        }

        public void EatBoglings()
        {
            ArrayList toEat = new ArrayList();
            IPooledEnumerable eable = GetMobilesInRange(2);

            foreach (Mobile m in eable)
            {
                if (m is Bogling)
                    toEat.Add(m);
            }
            eable.Free();

            if (toEat.Count > 0)
            {
                this.PlaySound(Utility.Random(0x3B, 2)); // Eat sound
                OverheadMessage("* Chomp *");
                foreach (Mobile m in toEat)
                {
                    this.Hits += (m.Hits / 2);
                    m.Delete();
                }
            }
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            if (this.Hits > (this.HitsMax / 4))
            {
                if (0.25 >= Utility.RandomDouble())
                    this.SpawnBogling(attacker);
            }
            else if (0.25 >= Utility.RandomDouble())
            {
                this.EatBoglings();
            }
        }
    }
}
