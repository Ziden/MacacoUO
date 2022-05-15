using System;
using Server.Items;
using Server.Spells;
using Server.Spells.Second;
using Server.Spells.Third;

namespace Server.Mobiles
{
    [CorpseName("a giant spider corpse")]
    public class GreaterGiantSpider : BaseCreature
    {

        public override Spell ChooseSpell()
        {
            if (Combatant is Mobile && !((Mobile)Combatant).Poisoned)
                return new PoisonSpell(this, null);
            return new TeleportSpell(this, null);
        }

        [Constructable]
        public GreaterGiantSpider()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "viuva magi-negra";
            Body = 28;
            BaseSoundID = 0x388;
            Hue = TintaPreta.COR;

            SetStr(76, 100);
            SetDex(200, 200);
            SetInt(36, 60);

            SetHits(950, 1000);
            SetMana(0);

            SetDamage(35, 50);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 15, 20);
            SetResistance(ResistanceType.Poison, 25, 35);

            SetSkill(SkillName.Poisoning, 120, 120);
            SetSkill(SkillName.MagicResist, 120, 120);
            SetSkill(SkillName.Tactics, 35.1, 50.0);
            SetSkill(SkillName.Wrestling, 120, 120);

            Fame = 13600;
            Karma = -1200;

            VirtualArmor = 80;

            Tamable = true;
            ControlSlots = 3;
            MinTameSkill = 100;

            SetSpecialAbility(SpecialAbility.Webbing);
        }

        public override void OnAfterTame(Mobile tamer)
        {
            base.OnAfterTame(tamer);
            this.SetHits(50, 100);
        }

        public GreaterGiantSpider(Serial serial)
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
            if (this.Combatant != null && this.Combatant.InRange2D(this.Location, 9))
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
                    m.SendMessage("Voce foi preso por uma teia e nao consegue se soltar");
                    m.OverheadMessage("* Preso em uma teia *");
                    var teia = new Teia(m);
                    teia.MoveToWorld(m.Location, m.Map);
                    m.Freeze(TimeSpan.FromSeconds(2));
                    Timer.DelayCall(TimeSpan.FromSeconds(2), () =>
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
            AddLoot(LootPack.LV3);
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
            from.AddToBackpack(new SpidersSilk(17 + Utility.Random(10)));
            PlaySound(0x57);

            if (Utility.RandomDouble() < 0.05)
            {
                from.AddToBackpack(BaseEssencia.RandomEssencia());
                from.SendMessage("Voce encontrou uma essencia magica no corpo da aranha");
            } else if (Utility.RandomDouble() < 0.2)
            {
                from.SendMessage("Voce encontrou uma runa misteriosa no corpo");
                from.AddToBackpack(new T2ARecallRune());
            } else
            {
                from.SendMessage("Dessa vez voce nao encontrou nada valioso no corpo da aranha");
            }
        }

    }
}
