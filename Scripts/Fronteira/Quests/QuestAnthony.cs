using System;
using Server.Engines.Points;
using Server.Items;
using Server.Mobiles;
using Server.Ziden.Dungeons.Esgoto;

namespace Server.Engines.Quests
{
    public class AnthonyQ : BaseQuest
    {
        public override bool DoneOnce { get { return false; } }

        public override object Title
        {
            get
            {
                return "A Paz entre Racas";
            }
        }
        public override object Description
        {
            get
            {
                return "Traga-me os percentes de Anthony e mostre que existe ainda paz entre as racas.";
            }
        }
        public override object Refuse
        {
            get
            {
                return "...";
            }
        }
        public override object Uncomplete
        {
            get
            {
                return "Sonho com o dia em que possamos ter paz.";
            }
        }

        public override object Complete
        {
            get
            {
                return @"Ah bom de mais da conta ! Ceh muito bao ! <br> Vou te contar um segredo em troca ! <br><br>
<b><u>No esgoto, na sala do pentagrama, tem um segredo atraz da teia de aranha</u></b>.
<br><br>Ja contei uqui sabia, ce eh baum dimais da conta brigado pelo meu sapato.";
            }
        }

        public AnthonyQ()
            : base()
        {
            //this.AddObjective(new ObtainObjective(typeof(LuckyDagger), "lucky dagger", 1));
            this.AddObjective(new ObtainObjective(typeof(CabecaAnthony), "Pertences de Anthony", 1));
            this.AddReward(new BaseReward("3 Livros Estranhos"));
            //this.AddReward(new BaseReward(typeof(SkillBook), 1, "1 Livro Tambem Estranho"));
        }

        public override void OnCompleted()
        {
            PointsSystem.Exp.AwardPoints(this.Owner, 500);
            this.Owner.PlaySound(this.CompleteSound);
            this.Owner._PlaceInBackpack(new CombatSkillBook());
            this.Owner._PlaceInBackpack(new CombatSkillBook());
            this.Owner._PlaceInBackpack(new CombatSkillBook());
        }

        public override void OnAccept()
        {
            base.OnAccept();
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
   
    public class LiderOrc : MondainQuester
    {
        public override Type[] Quests
        {
            get
            {
                return new Type[] {
                    typeof(AnthonyQ),
                    //typeof(SapatoLindoQ2)
                };
            }
        }


        [Constructable]
        public LiderOrc()
            : base("Lider Orc", "")
        {
            this.Body = 17;
            this.SetSkill(SkillName.Anatomy, 120.0, 120.0);
            this.SetSkill(SkillName.Parry, 120.0, 120.0);
            this.SetSkill(SkillName.Healing, 120.0, 120.0);
            this.SetSkill(SkillName.Tactics, 120.0, 120.0);
            this.SetSkill(SkillName.Swords, 120.0, 120.0);
            this.SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public LiderOrc(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            this.Say("Sonho com o dia da que poderemos ter paz...");  // Know yourself, and you will become a true warrior.
        }

        public override void InitBody()
        {
            this.Female = false;
            this.CantWalk = true;
            this.Race = Race.Human;

            base.InitBody();
        }

        public override void InitOutfit()
        {
            switch (Utility.Random(3))
            {
                case 0:
                    SetWearable(new FancyShirt(GetRandomHue()));
                    break;
                case 1:
                    SetWearable(new Doublet(GetRandomHue()));
                    break;
                case 2:
                    SetWearable(new Shirt(GetRandomHue()));
                    break;
            }

            SetWearable(new Shoes(GetShoeHue()));
            int hairHue = GetHairHue();

            Utility.AssignRandomHair(this, hairHue);
            Utility.AssignRandomFacialHair(this, hairHue);

            if (Body == 0x191)
            {
                FacialHairItemID = 0;
            }

            if (Body == 0x191)
            {
                switch (Utility.Random(6))
                {
                    case 0:
                        SetWearable(new ShortPants(GetRandomHue()));
                        break;
                    case 1:
                    case 2:
                        SetWearable(new Kilt(GetRandomHue()));
                        break;
                    case 3:
                    case 4:
                    case 5:
                        SetWearable(new Skirt(GetRandomHue()));
                        break;
                }
            }
            else
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        SetWearable(new LongPants(GetRandomHue()));
                        break;
                    case 1:
                        SetWearable(new ShortPants(GetRandomHue()));
                        break;
                }
            }

            if (!Siege.SiegeShard)
                PackGold(100, 200);
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
