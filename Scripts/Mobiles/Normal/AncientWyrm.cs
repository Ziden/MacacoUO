using Server.Items;
using Server.Items.Crops;
using Server.Items.Functional.Pergaminhos;
using Server.Spells;
using Server.Spells.Fourth;
using System;

namespace Server.Mobiles
{
    [CorpseName("a dragon corpse")]
    public class AncientWyrm : BaseCreature
    {
        public override bool IsBoss => true;

        public override Spell ChooseSpell()
        {
            var alvo = Combatant as Mobile;
            if (alvo != null && Utility.RandomBool())
            {
                return new FireFieldSpell(this, null);
            }
            return null;
        }


        [Constructable]
        public AncientWyrm()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "dragao anciao";
            this.Body = 46;
            this.BaseSoundID = 362;

            this.SetStr(1096, 1185);
            this.SetDex(86, 175);
            this.SetInt(686, 775);

            this.SetHits(15000, 15000);

            this.SetDamage(29, 45);

            this.SetDamageType(ResistanceType.Physical, 75);
            this.SetDamageType(ResistanceType.Fire, 25);

            this.SetResistance(ResistanceType.Fire, 80, 90);
            this.SetResistance(ResistanceType.Cold, 70, 80);
            this.SetResistance(ResistanceType.Poison, 60, 70);
            this.SetResistance(ResistanceType.Energy, 60, 70);

            this.SetSkill(SkillName.EvalInt, 80.1, 100.0);
            this.SetSkill(SkillName.Magery, 200, 200);
            this.SetSkill(SkillName.Meditation, 52.5, 75.0);
            this.SetSkill(SkillName.MagicResist, 100.5, 150.0);
            this.SetSkill(SkillName.Tactics, 97.6, 100.0);
            this.SetSkill(SkillName.Wrestling, 120, 120);
            this.SetSkill(SkillName.Parry, 60, 60);

            this.Fame = 22500;
            this.Karma = -22500;

            this.VirtualArmor = 30;
            Tamable = false;
        }

        public AncientWyrm(Serial serial)
            : base(serial)
        {
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            //DistribuiItem(new TemplateDeed());
            SorteiaItem(new DragonWolfCostume());
            SorteiaItem(new SnapdragonSeed(3));
            DistribuiItem(new CottonSeed(20));
            SorteiaItem(new FragmentosAntigos());
            SorteiaItem(new FragmentosAntigos());
            GolemMecanico.JorraOuro(this.Location, this.Map, 500);

            switch (Utility.Random(4))
            {
                case 0: SorteiaItem(new VinyardGroundAddonDeed()); break;
                case 1: SorteiaItem(new kegstorageAddonDeed()); break;
                case 2: SorteiaItem(new bottlerackAddonDeed()); break;
                case 3: SorteiaItem(new BeerKeg()); break;
            }

            if (Utility.RandomBool())
            {
                SorteiaItem(new DraconicOrb());
            }
            var a = new CarpenterApron();
            a.Bonus = Utility.Random(5, 15);
            a.Name = "Avental do Artesao do Dragao Anciao";
            a.Skill = SkillName.Carpentry;
            SorteiaItem(a);
            SorteiaItem(new BagOfSending());
            SorteiaItem(new CristalDoPoder() { Amount = 5 });
            SorteiaItem(new CristalDoPoder() { Amount = 5 });
            SorteiaItem(Carnage.GetRandomPS(105));
            SorteiaItem(Carnage.GetRandomPS(105));
            SorteiaItem(Carnage.GetRandomPS(105));
            SorteiaItem(Carnage.GetRandomPS(110));
            var arma = Loot.RandomWeapon();
            arma.Resource = CraftResource.Bronze;
            arma.Quality = ItemQuality.Exceptional;
            arma.WeaponAttributes.HitColdArea = 50;
            arma.Attributes.WeaponDamage = 20;
            if (arma.Name != null)
                arma.Name += " de gelo de dragao";
        }

        public override bool ReacquireOnMovement
        {
            get
            {
                return true;
            }
        }
        public override bool HasBreath
        {
            get
            {
                return true;
            }
        }// fire breath enabled
        public override bool AutoDispel
        {
            get
            {
                return true;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Barbed;
            }
        }
        public override int Hides
        {
            get
            {
                return 40;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
            }
        }

        public override int Scales
        {
            get
            {
                return 12;
            }
        }

        public override ScaleType ScaleType
        {
            get
            {
                return (ScaleType)Utility.Random(4);
            }
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Utility.RandomBool() ? Poison.Lesser : Poison.Regular;
            }
        }

        public override double TreasureMapChance => 1;

        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.LV5, 3);
            this.AddLoot(LootPack.Gems, 25);
        }

        public virtual int BonusExp => 500;

        public override int GetIdleSound()
        {
            return 0x2D3;
        }

        public override int GetHurtSound()
        {
            return 0x2D1;
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
