using Server.Items;
using Server.Mobiles;
using Server.Ziden.Achievements;
using Server.Ziden.Dungeons.Goblins.Quest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Ziden.RecompensaLogin
{

    public class SpellbookPreto : Spellbook
    {
        [Constructable]
        public SpellbookPreto() : base()
        {
            Name = "Spellbook Preto";
            Hue = TintaPreta.COR;
        }

        public SpellbookPreto(Serial s) : base(s){}

        public override void Serialize(GenericWriter writer) => base.Serialize(writer);
        public override void Deserialize(GenericReader reader) =>base.Deserialize(reader);
    }

    public class SpellbookBranco : Spellbook
    {
        [Constructable]
        public SpellbookBranco() : base()
        {
            Name = "Spellbook Branco";
            Hue = TintaBranca.COR;
        }

        public SpellbookBranco(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer) => base.Serialize(writer);
        public override void Deserialize(GenericReader reader) => base.Deserialize(reader);
    }

    public static class RecompensasLogin
    {
        public static List<CollectionItem> Rewards { get; set; }

        public static void Initialize()
        {
            Rewards = new List<CollectionItem>();

            Rewards.Add(new CollectionItem(typeof(SacolaBands), 0xE21, "Sacola com 50 Bandagens", 0, 6));
            Rewards.Add(new CollectionItem(typeof(BagOfReagents), 0xE76, "Sacola com 50 Reagentes", 0, 6));
            Rewards.Add(new CollectionItem(typeof(BagOfArrows), 0x0F40, "100 Flechas", 0, 3));
            Rewards.Add(new CollectionItem(typeof(BagOfBolts), 0x1BFC, "100 Dardos", 0, 3));
            Rewards.Add(new CollectionItem(typeof(HealPotion), 0xF0C, "Pocao de Cura", 0, 3));
            Rewards.Add(new CollectionItem(typeof(SacolaDeOuro), 0x0EEF, "Sacola com 300 Moedas", 0, 3));
            Rewards.Add(new CollectionItem(typeof(BagOfSending), 0xE76, "Sacola de Envio para Banco", 55, 200));
            Rewards.Add(new CollectionItem(typeof(SpellbookPreto), 0xEFA, "Livro de Magias Negro", TintaPreta.COR, 200));

            Rewards.Add(new CollectionItem(typeof(CaixaDeGold), 0xE21, "Caixa com 10k Moedas", 0, 24 * 5));
            Rewards.Add(new CollectionItem(typeof(SkillBook), 0xEFA, "Livro Cientifico</br>Upa uma skill de 0.1 a 0.5", 0, 24 * 3)); // Yew]

            Rewards.Add(new CollectionItem(typeof(PergaminhoCarregamento), 0x1F35, "Pergaminho do Carregamento<br>+1 Item na mochila", 0, 24 * 5));
            Rewards.Add(new CollectionItem(typeof(KegGH), 0x1940, "Keg de Vida Maior", 0, 24 * 5));
            Rewards.Add(new CollectionItem(typeof(KegMana), 0x1940, "Keg de Mana", 0, 24 * 5));
            Rewards.Add(new CollectionItem(typeof(KegStamina), 0x1940, "Keg de Stamina", 0, 24 * 5));
            Rewards.Add(new CollectionItem(typeof(KegCure), 0x1940, "Keg de Cura Maior", 0, 24 * 5));

            Rewards.Add(new CollectionItem(typeof(TemporaryForgeDeed), 0xFB1, "Forja Temporaria", 0, 50));
            // Rewards.Add(new CollectionItem(typeof(MagicalFishFinder), 0x14F6, "Encontrador de Peixes Magicos", 2500, 100));
            Rewards.Add(new CollectionItem(typeof(PergaminhoRunebook), 0x1F35, "Pergaminho de Runebook<br>Recarrega um runebook", 0, 10));
            Rewards.Add(new CollectionItem(typeof(HoodedShroudOfShadowsNoob), 0x2684, "Tunica dos Mortos Pessoal", 0, 24 * 30)); // Yew
            Rewards.Add(new CollectionItem(typeof(EtherealHorse), 0x20DD, "Cavalo Magico", 0, 24 * 30)); // Yew
            
            Rewards.Add(new CollectionItem(typeof(SewingMachineDeed), 0x9A40, "Maquina de Tecer", 0, 24 * 10));
            Rewards.Add(new CollectionItem(typeof(SmithingPressDeed), 0x9AA9, "Maquina de Fundicao", 0, 24 * 10));
            Rewards.Add(new CollectionItem(typeof(SpinningLatheDeed), 0x9C24, "Maquina de Couros", 0, 24 * 10));
            Rewards.Add(new CollectionItem(typeof(FletchingStationDeed), 0x9C2E, "Mesa do arqueiro", 0, 24 * 10));
            Rewards.Add(new CollectionItem(typeof(BBQSmokerDeed), 0x9D98, "Churrasqueira", 0, 24 * 10));
            Rewards.Add(new CollectionItem(typeof(AlchemyStationDeed), 0x9D8E, "Estacao do Alquimista", 0, 24 * 10));
            Rewards.Add(new CollectionItem(typeof(WritingDeskDeed), 0x9FEA, "Mesa do Escriba", 0, 24 * 10));
            Rewards.Add(new CollectionItem(typeof(TinkerBenchDeed), 0xA213, "Mesa de Funilaria", 0, 24 * 10));

        }
    }
}
