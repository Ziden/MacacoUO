using System;
using Server.Items;

namespace Server.Mobiles
{
    public class WolfSpider : BaseCreature
    {
        [Constructable]
        public WolfSpider()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "aranha bocanheira";
            Body = 736;
            Hue = 0;

            SetStr(225, 268);
            SetDex(145, 165);
            SetInt(285, 310);

            SetHits(700, 700);
			SetMana(285, 310);
			SetStam(145, 165);

            SetDamage(11, 30);

            SetDamageType(ResistanceType.Physical, 70);
            SetDamageType(ResistanceType.Poison, 30);

            SetResistance(ResistanceType.Physical, 30, 35);
            SetResistance(ResistanceType.Fire, 20, 30);
            SetResistance(ResistanceType.Cold, 25, 35);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 25, 35);

            SetSkill(SkillName.MagicResist, 60.0, 75.0);
            SetSkill(SkillName.Poisoning, 100, 100);
            SetSkill(SkillName.Tactics, 84.1, 95.9);
            SetSkill(SkillName.Wrestling, 80.2, 90.0);
            SetSkill(SkillName.Hiding, 105.0, 110.0);
            SetSkill(SkillName.Stealth, 105.0, 110.0);

            Tamable = true;
            ControlSlots = 2;
            MinTameSkill = 105;

            Fame = 30000;
            Karma = -30000;
        }

        public WolfSpider(Serial serial)
            : base(serial)
        {
        }


        public override bool CanStealth { get { return true; } }

        public override void OnThink()
        {
            base.OnThink();
           

            Teia();
            if (!this.Hidden && this.Combatant == null && !(this.ControlMaster is PlayerMobile))
            {
                this.AllowedStealthSteps = 999;
                this.Hidden = true;
                this.IsStealthing = true;
            }
        }

        public void Teia()
        {
            if (!this.IsCooldown("teia"))
            {
                this.SetCooldown("teia", TimeSpan.FromSeconds(3));
            }
            else
            {
                return;
            }

            if (this.ControlMaster is PlayerMobile && this.Combatant is PlayerMobile)
                return;

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
                    this.Hidden = false;
                    teia.MoveToWorld(m.Location, m.Map);
                    m.Freeze(TimeSpan.FromSeconds(6));
                    Timer.DelayCall(TimeSpan.FromSeconds(5), () =>
                    {
                        teia.Delete();
                        m.Frozen = false;
                    });
                });
            }
        }

        public override void OnRevealed()
        {
            base.OnRevealed();
            PublicOverheadMessage(Network.MessageType.Emote, 0, false, "* saiu do esconderijo *");
        }

        public override void OnAfterSpawn()
        {
            base.OnAfterSpawn();
            this.Hidden = true;
            this.IsStealthing = true;
            this.AllowedStealthSteps = 999;
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
                return Poison.Regular;
            }
        }
        public override void GenerateLoot()
        {
            PackItem(new SpidersSilk(8));
            AddLoot(LootPack.LV4);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

			if (Controlled)
				return;

            if (!Controlled && Utility.RandomDouble() < 0.01)
                c.DropItem(new LuckyCoin());
        }

        public override int GetIdleSound()
        {
            return 1605;
        }

        public override int GetAngerSound()
        {
            return 1602;
        }

        public override int GetHurtSound()
        {
            return 1604;
        }

        public override int GetDeathSound()
        {
            return 1603;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)2);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0)
            {
                Hue = 0;
                Body = 736;
            }

            if (version == 1 && (AbilityProfile == null || AbilityProfile.MagicalAbility == MagicalAbility.None))
            {
                SetMagicalAbility(MagicalAbility.Poisoning);
            }
        }
    }
}
