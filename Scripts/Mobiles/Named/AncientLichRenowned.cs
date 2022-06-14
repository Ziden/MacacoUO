using System;
using Server.Fronteira.Elementos;
using Server.Items;
using Server.Items.Functional.Pergaminhos;
using Server.Ziden;
using Server.Ziden.Items;

namespace Server.Mobiles
{
    [CorpseName("Ancient Lich [Renowned] corpse")]
    public class AncientLichRenowned : BaseRenowned
    {
        public override double DisturbChance { get { return 0; } }

        [Constructable]
        public AncientLichRenowned()
            : base(AIType.AI_NecroMage)
        {
            this.Name = "Lich Rei";
            //this.Title = "[Renomado]";
            this.Body = 78;
            this.BaseSoundID = 412;

            this.SetStr(250, 305);
            this.SetDex(96, 115);
            this.SetInt(966, 1045);

            this.SetHits(6000, 6000);

            this.SetDamage(25, 37);

            this.SetDamageType(ResistanceType.Physical, 20);
            this.SetDamageType(ResistanceType.Cold, 40);
            this.SetDamageType(ResistanceType.Energy, 40);

            this.SetResistance(ResistanceType.Physical, 55, 65);
            this.SetResistance(ResistanceType.Fire, 25, 30);
            this.SetResistance(ResistanceType.Cold, 50, 60);
            this.SetResistance(ResistanceType.Poison, 50, 60);
            this.SetResistance(ResistanceType.Energy, 25, 30);

            SetSkill(SkillName.Poisoning, 90.0, 99.0);
            this.SetSkill(SkillName.EvalInt, 110, 120);
            this.SetSkill(SkillName.Magery, 120.1, 130.0);
            this.SetSkill(SkillName.Meditation, 100.1, 101.0);
            this.SetSkill(SkillName.MagicResist, 175.2, 200.0);
            this.SetSkill(SkillName.Tactics, 90.1, 100.0);
            this.SetSkill(SkillName.Wrestling, 75.1, 100.0);

            this.Fame = 23000;
            this.Karma = -23000;

            this.VirtualArmor = 60;
            this.PackNecroReg(200, 375);

        }

        public AncientLichRenowned(Serial serial)
            : base(serial)
        {
        }

        public override Type[] UniqueSAList
        {
            get
            {
                return new Type[] { /*typeof(SpinedBloodwormBracers), typeof(DefenderOfTheMagus)*/ };
            }
        }
        public override Type[] SharedSAList
        {
            get
            {
                return new Type[] { /*typeof(SummonersKilt)*/ };
            }
        }
        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            SorteiaItem(Decos.RandomDeco(this));
            SorteiaItem(new LivroAntigo());
            DistribuiItem(new FragmentosAntigos());
            SorteiaItem(new PergaminhoSagradoDeRunebook());
            DistribuiItem(new CristalDoPoder() { Amount = 5 });
            var book = new Spellbook();
            book.Hue = TintaPreta.COR;
            book.Name = "Livro do Lich Rei";
            book.Attributes.SpellDamage = 10;
            SorteiaItem(book);
            for (var x = 0; x < 4; x++)
            {
                SorteiaItem(ElementoUtils.GetRandomPedraSuperior());
            }
            if (Utility.RandomDouble() < 0.3)
                    SorteiaItem(Carnage.GetRandomPS(105));
            if (Utility.RandomDouble() < 0.1)
                    SorteiaItem(Carnage.GetRandomPS(110));
        }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override bool Unprovokable
        {
            get
            {
                return true;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override int GetIdleSound()
        {
            return 0x19D;
        }

        public override int GetAngerSound()
        {
            return 0x175;
        }

        public override int GetDeathSound()
        {
            return 0x108;
        }

        public override int GetAttackSound()
        {
            return 0xE2;
        }

        public override int GetHurtSound()
        {
            return 0x28B;
        }

        public override bool OnBeforeDeath()
        {
            var rights = this.GetLootingRights();
            foreach(var d in rights)
            {
                if(d.m_HasRight)
                {
                    d.m_Mobile.Backpack.AddItem(new LichKiller());
                }
            }
            return base.OnBeforeDeath();
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV5, 3);
            this.AddLoot(LootPack.Gems, 20);
        }

        public virtual int BonusExp => 400;

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
