using Server.Fronteira.Classes;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Fronteira.RP
{

    public class ItemTexto : Item
    {

        [CommandProperty(AccessLevel.GameMaster)]
        public string Texto { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Cor { get; set; }


        [Constructable]
        public ItemTexto() : base(0xF6C)
        {
            Name = "Mude Me";
        }

        public ItemTexto(Serial s) : base(s)
        {

        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(Texto);
            writer.Write(Cor);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var v = reader.ReadInt();
            Texto = reader.ReadString();
            Cor = reader.ReadInt();
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (!m.InRange(this, 2))
            {
                m.SendMessage("Muito longe...");
                return;
            }
            this.PrivateMessage(Texto, m, Cor);
        }
    }

    public class TeleporterInicio : Item
    {
        [Constructable]
        public TeleporterInicio() : base(0xF6C)
        {
            Name = "Portal da Vida";
        }

        public TeleporterInicio(Serial s) : base(s)
        {

        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }

        public override void OnDoubleClick(Mobile m)
        {
            base.OnDoubleClick(m);
            InicioRP.VaiProBarco(m);
        }

        public override bool OnMoveOver(Mobile m)
        {
            base.OnMoveOver(m);
            InicioRP.VaiProBarco(m);
            return true;
        }
    }

    public class InicioRP
    {
        public static void Initialize()
        {

        }

        public static void Chega(Mobile m)
        {
            m.VisivelPraOutrosPlayers = true;
            m.MoveToWorld(new Point3D(1238, 1146, -24), Map.Ilshenar);
            m.OverheadMessage("* desembarcou *");
            m.SendMessage("O barco chega e todos desembarcam e seguem seu caminho.");
            m.SendMessage($"Voce esta {(m.Female ? "Pronta":"Pronto")} para iniciar sua nova vida.");
        }

        public static void VaiProBarco(Mobile m)
        {
            m.MoveToWorld(new Point3D(6953, 1582, 18), Map.Trammel);
            Timer.DelayCall(TimeSpan.FromSeconds(0.01), () => {
                m.Location = new Point3D(6953, 1582, 18);
            });

            m.VisivelPraOutrosPlayers = false;
            m.OverheadMessage("* !! *");
            Timer.DelayCall(TimeSpan.FromSeconds(5), () =>
            {
                GumpFala.MostraFalas(m,
                    () =>
                    {
                        m.PlaySound(0x119);
                        Timer.DelayCall(TimeSpan.FromSeconds(3), () => m.PlaySound(0x119));
                        Timer.DelayCall(TimeSpan.FromSeconds(6), () => m.PlaySound(0x119));
                        Timer.DelayCall(TimeSpan.FromSeconds(10), () => m.PlaySound(0x119));
                    },
                    new Fala(Faces.ENGENHEIRA).Textos("Estamos chegando ?", "Ja perdi quanto dias estamos indo a esta 'missao' ?", "Fala serio"),
                    new Fala(Faces.PUNK_BARBUDO).Textos("Acalme-se mulher, iremos chegar quando for a hora", "Nao cause problemas e faca o que lhe for dito", "e tera seu ouro")

                );
            });

            Timer.DelayCall(TimeSpan.FromSeconds(25), () =>
            {
                GumpFala.MostraFalas(m,
                    () =>
                    {
                        m.SendGump(new RPClassGump());
                    },
                    new Fala(Faces.ENGENHEIRA).Textos($"E voce {(m.Female ? "Novata" : "Novato")} ?", m.Name + " nao eh ?", "O que voce sabe fazer ??")
                );
            });
        }

        public static void EscolheClasse(PlayerMobile m, ClassePersonagem classe)
        {
            var c = classe.Talentos[5];
            GumpFala.MostraFalas(m,
                () =>
                {
                    m.SendGump(new GumpOpcoes("Resposta", (n) =>
                    {
                        GumpFala.MostraFalas(m,() => {
                            m.PlaySound(0x86);

                            GumpFala.MostraFalas(m, () => {
                                m.PlaySound(0x86);

                                Timer.DelayCall(TimeSpan.FromSeconds(5), () =>
                                {
                                    Chega(m);
                                });

                            }, new Fala(Faces.CACHORRO).Textos($" WOOF ", "( adoro batatas )"),  new Fala(Faces.PUNK_BARBUDO).Textos($"Terra a vista !", "Chegaremos em breve !!")
                            );

                        }, new Fala(Faces.ENGENHEIRA).Textos($"Que figura voce {m.Name}", "Espero que consiga o que deseja", "Se nao vai acabar cortando batatas pro Toquinho comer", "Neh toquinho ?"));
                    }, 11047, 0, "Sim claro", "Ainda nao"));
                },
                new Fala(Faces.ENGENHEIRA).Textos($"Aha, mais um {classe.Nome} no mundo", $"Ouvi dizer que {classe.Nome} pode se especializar", $"como {c.T1} {c.T2} ou {c.T3}", "Voce tem ideia de que caminho ira seguir ?")
            );
        }

        public static void InitializaPlayer(PlayerMobile player)
        {
            VaiProBarco(player);
            /*
            player.MoveToWorld(new Point3D(857, 2784, 5), Map.TerMur);
            if (player.BodyMod == 0)
            {
                player.BodyMod = 58; // wisp
                player.OverheadMessage("* sua alma revive *");
                Timer.DelayCall(TimeSpan.FromSeconds(2), () =>
                {
                    player.OverheadMessage("* explorando *");
                });
                Timer.DelayCall(TimeSpan.FromSeconds(4), () =>
                {
                    player.OverheadMessage("* vida *");
                });
            }
            */
        }
    }
}
