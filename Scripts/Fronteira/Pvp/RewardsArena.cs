using Server.Engines.Auction;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Misc.Custom;
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
    public static class RecompensasArena
    {
        public static List<CollectionItem> Rewards { get; set; }

        public static void Initialize()
        {
            Rewards = new List<CollectionItem>();

            Rewards.Add(new CollectionItem(typeof(SacolaBands), 0xE21, "Sacola com 50 Bandagens", 0, 1));
            Rewards.Add(new CollectionItem(typeof(BagOfReagents), 0xE76, "Sacola com 50 Reagentes", 0, 1));
            Rewards.Add(new CollectionItem(typeof(SacolaPots), 0xE76, "Sacola com 10 pots medias de cada", 0, 2));

            Rewards.Add(new CollectionItem(typeof(BagOfArrows), 0xE76, "100 Flechas", 0, 2));
            Rewards.Add(new CollectionItem(typeof(BagOfBolts), 0xE76, "100 Dardos", 0, 2));

            Rewards.Add(new CollectionItem(typeof(FragmentosAntigos), 0x1053, "Fragmentos Antigos", 1152, 10));
            Rewards.Add(new CollectionItem(typeof(CristalDoPoder), 0x1053, "Cristal do Poder", 1151, 1));
            Rewards.Add(new CollectionItem(typeof(BagOfSending), 0xE76, "Sacola de Envio para Banco", 55, 100));

            Rewards.Add(new CollectionItem(typeof(SpellbookPreto), 0xEFA, "Livro de Magias Negro", TintaPreta.COR, 100));
            Rewards.Add(new CollectionItem(typeof(SpellbookBranco), 0xEFA, "Livro de Magias Negro", TintaBranca.COR, 100));

            Rewards.Add(new CollectionItem(typeof(SkillBook), 0xEFA, "Livro Cientifico</br>Upa uma skill de 0.1 a 0.5", 0, 300));

            Rewards.Add(new CollectionItem(typeof(PergaminhoCarregamento), 0x1F35, "Pergaminho do Carregamento<br>+1 Item na mochila", 0, 200));
            Rewards.Add(new CollectionItem(typeof(KegGH), 0x1940, "Keg de Vida Maior", 0, 500));
            Rewards.Add(new CollectionItem(typeof(KegManaMaior), 0x1940, "Keg de Mana Maior", 0, 500));
            Rewards.Add(new CollectionItem(typeof(KegMana), 0x1940, "Keg de Mana", 0, 100));
            Rewards.Add(new CollectionItem(typeof(KegStamina), 0x1940, "Keg de Stamina", 0, 100));
            Rewards.Add(new CollectionItem(typeof(KegCure), 0x1940, "Keg de Cura Maior", 0, 100));

            Rewards.Add(new CollectionItem(typeof(TemporaryForgeDeed), 0xFB1, "Forja Temporaria", 0, 5));
            // Rewards.Add(new CollectionItem(typeof(MagicalFishFinder), 0x14F6, "Encontrador de Peixes Magicos", 2500, 100));
            Rewards.Add(new CollectionItem(typeof(PergaminhoRunebook), 0x1F35, "Pergaminho de Runebook<br>Recarrega um runebook", 0, 1));

            Rewards.Add(new CollectionItem(typeof(ValeDecoracaoRara), 0x9F64, "Caixa Misteriosa", 0, 5));  // Greater Stam
            Rewards.Add(new CollectionItem(typeof(ElementalBall), 3630, "Bola de Cristal Elemental", 0, 100));  // Greater Stam
            Rewards.Add(new CollectionItem(typeof(DaviesLockerAddonDeed), 0x14F0, "Guarda Mapas", 0, 200));
            Rewards.Add(new CollectionItem(typeof(CannonDeed), 0x14F0, "Escritura de Canhao", 0, 300));
            Rewards.Add(new CollectionItem(typeof(RedSoulstone), 0x32F3, "Pedra das Almas</br>Guarda 1 Skill", 0, 100));
            Rewards.Add(new CollectionItem(typeof(CommodityDeedBox), 0x9AA, "Caixa de Commidities</br>Guarda recursos e comodities", 0, 100));
            Rewards.Add(new CollectionItem(typeof(AuctionSafeDeed), 0x9C18, "Cofre de Leilao</br>Permite leiloar items", 0, 300));
            Rewards.Add(new CollectionItem(typeof(BannerDeed), 0x14F0, "Banner</br>Escolha e bote um banner medieval", 0, 5));
            Rewards.Add(new CollectionItem(typeof(SpellbookDyeTub), 0xFAB, "Tinta de Livro de Magia</br>Permite pintar livros de magia", 0, 500));
            Rewards.Add(new CollectionItem(typeof(RunebookDyeTub), 0xFAB, "Tinta de Runebook</br>Permite pintar runebooks", 0, 500));
            Rewards.Add(new CollectionItem(typeof(MetallicDyeTub), 0xFAB, "Tinta de Armaduras</br>Permite pintar armaduras de ferro", 0, 500));
            Rewards.Add(new CollectionItem(typeof(RepairBenchDeed), 0x14F0, "Mesa de Reparos</br>Permite Reparar Items", 0, 1000));
        }
    }
}
