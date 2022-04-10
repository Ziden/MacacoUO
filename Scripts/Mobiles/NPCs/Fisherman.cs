using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Fisherman : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        [Constructable]
        public Fisherman()
            : base("o pescador")
        {
            this.SetSkill(SkillName.Fishing, 75.0, 98.0);
        }

        public Fisherman(Serial serial)
            : base(serial)
        {
        }

        public override NpcGuild NpcGuild
        {
            get
            {
                return NpcGuild.FishermensGuild;
            }
        }
        protected override List<SBInfo> SBInfos
        {
            get
            {
                return this.m_SBInfos;
            }
        }
        public override void InitSBInfo()
        {
            this.m_SBInfos.Add(new SBFisherman());
        }

        public override void VendorSell(Mobile from)
        {
            var valor = 0;
            foreach(var item in new List<Item>(from.Backpack.Items))
            {
                if(item is Fish ||item is BigFish || item is BaseHighseasFish || item is BaseMagicFish)
                {
                    valor += 1;
                    item.Delete();
                }
            }
            if(valor > 0)
            {
                SayTo(from, true, "Opa, posso comprar todos esses peixes. Para um pescador, nunca se tem peixes demais !");
                from.SendMessage("Voce vendeu todos seus peixes para o pescador por "+valor+" moedas de ouro");
                if(valor > 200)
                {
                    from.SendMessage("Mais de 200 moedas em peixe... voce fica espantado com o pescador comprando tanto peixe.");
                }
                from.PlaySound(0x5B5);
                from._PlaceInBackpack(new Gold(valor));
                return;
            } else
            {
                SayTo(from, true, "Se tiver peixes, posso comprar !");
            }
            base.VendorSell(from);
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            this.AddItem(new Server.Items.FishingPole());
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
