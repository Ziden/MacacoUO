using System;
using Server.Items;
using Server.Items.Functional.Pergaminhos;
using Server.Misc.Custom;

namespace Server.Mobiles
{
    [CorpseName("corpo de formiga-aranha")]
    public class SolenLouca : BaseCreature
    {
        public bool IsBoss => true;
        public override int BonusExp => 400;
        public override bool ReduceSpeedWithDamage => false;

        [Constructable]
        public SolenLouca()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.1, 0.2)
        {
            this.Name = "formiga atomica";
            this.Body = 805;
            this.BaseSoundID = 959;
            this.Hue = 0x47D;

            this.SetStr(96, 120);
            this.SetDex(20, 30);
            this.SetInt(36, 60);

            this.SetHits(5000, 5000);

            this.SetDamage(5, 20);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Fire, 20, 30);
            this.SetResistance(ResistanceType.Cold, 10, 20);
            this.SetResistance(ResistanceType.Poison, 10, 20);
            this.SetResistance(ResistanceType.Energy, 20, 30);

            this.SetSkill(SkillName.DetectHidden, 100);
            this.SetSkill(SkillName.Tactics, 100);
            this.SetSkill(SkillName.Wrestling, 100);

            this.Fame = 1500;
            this.Karma = -1500;

            this.VirtualArmor = 0;

            SetWeaponAbility(WeaponAbility.BleedAttack);

            this.PackGold(Utility.Random(100, 180));

            SolenHelper.PackPicnicBasket(this);

            this.PackItem(new ZoogiFungus());
        }

        public override void OnDamagedBySpell(Mobile from)
        {
            base.OnDamagedBySpell(from);
            if (from != this)
            {
                var rnd = Utility.RandomDouble();
                if (from != this && rnd < 0.4)
                {
                    this.Combatant = from;
                    this.PlaySound(this.GetAngerSound());
                }
            }
        }

        public override bool OnBeforeDeath()
        {
            if (NoKillAwards)
                return base.OnBeforeDeath();

            DistribuiItem(Carnage.GetRandomPS(105));
            GolemMecanico.JorraOuro(this.Location, this.Map, 150);
            //var bola = new ElementalBall();
            //bola.LootType = LootType.Regular;
            //bola.Cargas = 100;
            //SorteiaItem(bola);
            SorteiaItem(new ZoogiFungus(25));
            DistribuiItem(Decos.RandomDeco(this));
            var b = new BraceleteDoPoder();
            SorteiaItem(b);
            var arco = Loot.RandomWeapon();
            arco.Resource = CraftResource.Bronze;
            arco.Quality = ItemQuality.Exceptional;
            SorteiaItem(arco);
            //SorteiaItem(new TemplateDeed());
            DistribuiItem(new Gold(1000));
            DistribuiPs(105);
            //DistribuiItem(new FragmentosAntigos());
            DistribuiItem(new CristalDoPoder());
            SorteiaItem(new FragmentosAntigos());

            var r = Utility.Random(5);
            if (r == 0)
            {
                SorteiaItem(new AncientSmithyHammer(1));
            }
            else if (r == 1)
            {
                var a = new CarpenterApron();
                a.Bonus = Utility.Random(1, 1);
                a.Name = "Avental do Ferreiro Atomico";
                a.Skill = SkillName.Blacksmith;
                SorteiaItem(a);
            }
            else
            {
                var martelo = new SledgeHammer();
                martelo.Resource = CraftResource.Dourado;
                SorteiaItem(martelo);
            }
            var pots = new GreaterHealPotion(10);
            DistribuiItem(pots);
            return base.OnBeforeDeath();
        }

        public override void AlterMeleeDamageTo(Mobile to, ref int damage)
        {
            base.AlterMeleeDamageTo(to, ref damage);
            if (to is BaseCreature)
                damage *= 10;
        }

        public override void OnThink()
        {
            var from = Combatant;
            if (!(from is PlayerMobile))
                return;

            if (from == null)
                return;

            if (!IsCooldown("acido"))
            {
                SetCooldown("acido", TimeSpan.FromSeconds(8));

                if (!from.InRange2D(this, 15))
                    return;

                if (!this.InLOS(from))
                    return;

                var loc1 = from.Location;
                var loc2 = from.Location;
                if (Utility.RandomBool())
                    loc1.X += 1;
                else
                    loc1.X -= 1;
                if (Utility.RandomBool())
                    loc1.Y += 1;
                else
                    loc1.Y -= 1;
                if (Utility.RandomBool())
                    loc2.X += 1;
                else
                    loc2.X -= 1;
                if (Utility.RandomBool())
                    loc2.Y += 1;
                else
                    loc2.Y -= 1;

                if (from == null || from.Map == null || from.Map == Map.Internal || !from.Alive)
                    return;

                loc1.Z = from.Map.GetAverageZ(loc1.X, loc1.Y);
                if (Math.Abs(loc1.Z - this.Location.Z) > 4)
                {
                    loc1.Z = this.Location.Z;
                }
                loc2.Z = from.Map.GetAverageZ(loc2.X, loc2.Y);
                if (Math.Abs(loc2.Z - this.Location.Z) > 4)
                {
                    loc2.Z = this.Location.Z;
                }

                if (from.Map.CanFit(loc1, 16))
                {
                    Item acid1 = NewAcido(30, "acido de formiga");
                    acid1.MoveToWorld(loc1, from.Map);
                    Effects.SendMovingEffect(this, acid1, acid1.ItemID, 15, 10, true, false, acid1.Hue, 0);
                    acid1.PublicOverheadMessage("* acido *");
                }

                if (from.Map.CanFit(loc2, 16))
                {
                    Item acid1 = NewAcido(30, "acido de formiga");
                    acid1.MoveToWorld(loc2, from.Map);
                    Effects.SendMovingEffect(this, acid1, acid1.ItemID, 15, 10, true, false, acid1.Hue, 0);
                    acid1.PublicOverheadMessage("* acido *");
                }

                Item acid12 = NewAcido(10, "acido de formiga");
                acid12.MoveToWorld(from.Location, from.Map);
                Effects.SendMovingEffect(this, acid12, acid12.ItemID, 15, 10, true, false, acid12.Hue, 0);
                acid12.PublicOverheadMessage("* acido *");

                OverheadMessage("* jorra acido *");

                this.SpillAcid(2, power: 20, name: "acido");
            }

        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            this.SetStam(400);
            base.OnDamage(amount, from, willKill);
            var rnd = Utility.RandomDouble();
            if (from != this && rnd < 0.4)
            {
                this.Combatant = from;
                this.PlaySound(this.GetAngerSound());
            }
            SolenHelper.OnBlackDamage(from);
        }

        public SolenLouca(Serial serial)
            : base(serial)
        {
        }


        public override void AlterSpellDamageTo(Mobile to, ref int damage, ElementoPvM elemento)
        {
            base.AlterSpellDamageTo(to, ref damage, elemento);
            if (to is BaseCreature)
                damage *= 6;
        }




        public override int GetAngerSound()
        {
            return 0x269;
        }

        public override int GetIdleSound()
        {
            return 0x269;
        }

        public override int GetAttackSound()
        {
            return 0x186;
        }

        public override int GetHurtSound()
        {
            return 0x1BE;
        }

        public override int GetDeathSound()
        {
            return 0x8E;
        }

        public override bool BardImmune => true;

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Gems, Utility.RandomMinMax(5, 10));
            this.AddLoot(LootPack.Gems, Utility.RandomMinMax(5, 10));
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
    }
}
