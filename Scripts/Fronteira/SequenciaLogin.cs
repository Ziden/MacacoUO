using CustomsFramework;
using Server.Fronteira.Tutorial.WispGuia;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Scripts.New.Adam.NewGuild;
using Server.Ziden.Tutorial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Fronteira
{
    public class SequenciaLogin
    {
        public static void Initialize()
        {
            Console.WriteLine("Carregando sequencia de login");
            EventSink.Login += OnLogin;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            var pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            if (Shard.WARSHARD)
            {
                return;
            }

            if (pm.Young && !pm.Account.Young)
                pm.Young = false;

            if (Shard.WHITELIST)
            {
                if (pm.IsStaff() || pm.Name.StartsWith("Tester"))
                    return;

                pm.SendGump(new GumpWhitelist());
                pm.Frozen = true;
            }
            else if (pm.Frozen)
                pm.Frozen = false;

            if (CharacterCreation.Novos.Contains(e.Mobile))
            {
                CharacterCreation.Novos.Remove(e.Mobile);
                if (pm.RP && pm.Profession == 0)
                {
                    GumpClasse.Mostra(pm);
                }
                else if (pm.Profession == 0)
                {
                    if (pm.Young)
                    {
                        pm.SendGump(new GumpFala((n) =>
                        {
                            pm.SendGump(new GumpFala((n2) =>
                            {
                                pm.SendGump(new GumpFala((n3) =>
                                {
                                    TutorialNoob.InicializaWisp(pm);
                                }, Faces.GM_PRETO, "Voce agora recebera uma fada guia dos newbies.", "Caso nao queira fazer o tutorial, clique 2x nela e mande ela embora!", "<center>Porem se completar tudo ira ganhar items e uma casa</center>", "Isso mesmo, uma CASA !!!"));
                            }, Faces.GM_PRETO, "Rates de upar skills sao faceis no shard !", "", "UP de skills em Macro: Hard.", "Up de skills matando Monstro: EASY !!!"));
                        }, Faces.GM_PRETO, "Bem vindo ao Dragonic Age !", "", "Quests sao opcionais, porem recomendamos o tutorial !", "No tutorial vc ganhara equipamentos e uma casa !", "Fique sempre atento a mensagens !"));
                    }
                    else
                    {
                        pm.SendGump(new GumpFala((n2) => {
                            pm.SendGump(new NonRPClassGump());
                        }, Faces.GM_PRETO, "Bem vindo...novamente ! Voce nao e mais um novato !", "Tera de re-escrever a historia com suas proprias pernas!"));
                    }
                }
            }
        }
    }
}
