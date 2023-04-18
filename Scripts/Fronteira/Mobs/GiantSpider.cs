using System;
using Server.Items;

namespace Server.Mobiles
{
    public class Teia : Item
    {
        public Mobile preso;

        [Constructable]
        public Teia(Mobile preso) : base(0x10D3)
        {
            this.preso = preso;
        }

        public Teia(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            { }
        }

        public override void Deserialize(GenericReader writer)
        {
            { }
        }

        //public override void OnDamage(int amount, Mobile from, bool willKill)
        //{
        //    base.OnDamage(amount, from, willKill);
        //    if (preso != null)
        //    {
        //        preso.SendMessage("A teia arrebentou e voce foi solto.");
        //        preso.PlayHurtSound();
        //        this.Delete();
        //    }
        //}
        public override void OnDoubleClick(Mobile from)
        {
            if(from!=preso)
            {
                preso.OverheadMessage("* foi solto *");
                from.OverheadMessage("* cortou a teia *");
                from.PlayAttackAnimation();
                preso.PlayHurtSound();
                preso.SendMessage(from.Name + " cortou a teia e lhe liberou");
                from.SendMessage("Voce cortou a teia, liberando " + preso.Name);
                this.Delete();
            }
        }
    }

    [CorpseName("a giant spider corpse")]
    public class GiantSpider : BaseCreature
    {
        [Constructable]
        public GiantSpider()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "aranha gigante";
            Body = 28;
            BaseSoundID = 0x388;

            SetStr(76, 100);
            SetDex(76, 95);
            SetInt(36, 60);

            SetHits(46, 60);
            SetMana(0);

            SetDamage(5, 13);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 15, 20);
            SetResistance(ResistanceType.Poison, 25, 35);

            SetSkill(SkillName.Poisoning, 60.1, 80.0);
            SetSkill(SkillName.MagicResist, 25.1, 40.0);
            SetSkill(SkillName.Tactics, 35.1, 50.0);
            SetSkill(SkillName.Wrestling, 50.1, 65.0);

            Fame = 600;
            Karma = -600;

            VirtualArmor = 16;

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 59.1;
        }

        public GiantSpider(Serial serial)
            : base(serial)
        {
        }

        public override void OnThink()
        {
            if (!this.IsCooldown("teia"))
            {
                this.SetCooldown("teia", TimeSpan.FromSeconds(3));
            }
            else
            {
                return;
            }
            if (this.Combatant != null && this.Combatant.InRange2D(this.Location, 7))
            {  
                if (!this.IsCooldown("teiab"))
                {
                    this.SetCooldown("teiab", TimeSpan.FromSeconds(30));
                }
                else
                {
                    return;
                }

                if (!this.InLOS(this.Combatant))
                {
                    return;
                }
                this.PlayAngerSound();
                this.MovingParticles(this.Combatant, 0x10D3, 15, 0, false, false, 9502, 4019, 0x160);
                var m = this.Combatant as Mobile;
                Timer.DelayCall(TimeSpan.FromMilliseconds(400), () =>
                {
                    m.SendMessage("Voce foi preso por uma teia e nao consegue se soltar. Talvez alguem possa te ajudar a se soltar.");
                    m.OverheadMessage("* Preso em uma teia *");
                    var teia = new Teia(m);
                    teia.MoveToWorld(m.Location, m.Map);
                    m.Freeze(TimeSpan.FromSeconds(4));
                    Timer.DelayCall(TimeSpan.FromSeconds(3), () =>
                    {
                        teia.Delete();
                        m.Frozen = false;
                    });
                });
            }
        }

        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Arachnid;
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
                return Poison.Lesser;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV1);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0 && (AbilityProfile == null || AbilityProfile.MagicalAbility == MagicalAbility.None))
            {
                SetMagicalAbility(MagicalAbility.Poisoning);
            }
        }

        public override void OnCarve(Mobile from, Corpse corpse, Item with)
        {
            corpse.Carved = true;
            from.PrivateOverheadMessage("* Coletou teias *");
            from.AddToBackpack(new SpidersSilk(7 + Utility.Random(10)));
            PlaySound(0x57);
        }
       
    }
}
