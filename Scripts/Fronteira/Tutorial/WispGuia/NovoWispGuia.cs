using System;
using Server.Fronteira.Tutorial.WispGuia;
using Server.Gumps;
using Server.Regions;
using Server.Spells.First;

namespace Server.Mobiles
{
    [CorpseName("a wisp corpse")]
    public class NovoWispGuia : WispGuia
    {
        public static Guia Guia = new Guia();

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            base.OnDamage(amount, from, willKill);
            if (!this.IsCooldown("ai"))
            {
                this.SetCooldown("ai", TimeSpan.FromMinutes(1));
                Fala("Ouch, essa doeu.");
            }
        }

        [Constructable]
        public NovoWispGuia(PlayerMobile player)
            : base(player)
        {
            this.Name = "Fada dos Noobs";
            this.Body = 58;
            this.Jogador = player;
            this.SetStr(1, 1);
            this.SetDex(1, 1);
            this.SetInt(1, 1);

            this.SetHits(1000);

            this.SetDamage(1, 1);

            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Energy, 50);

            this.SetResistance(ResistanceType.Physical, 35, 45);
            this.SetResistance(ResistanceType.Fire, 20, 40);
            this.SetResistance(ResistanceType.Cold, 10, 30);
            this.SetResistance(ResistanceType.Poison, 5, 10);
            this.SetResistance(ResistanceType.Energy, 50, 70);

            this.SetSkill(SkillName.EvalInt, 80.0);
            this.SetSkill(SkillName.Magery, 80.0);
            this.SetSkill(SkillName.MagicResist, 80.0);
            this.SetSkill(SkillName.Tactics, 80.0);
            this.SetSkill(SkillName.Wrestling, 80.0);

            this.Fame = 4000;
            this.Karma = 0;
            this.ControlSlots = 0;
            this.VirtualArmor = 40;
            SetCooldown("pensa", TimeSpan.FromSeconds(10));

        }

        public bool ValidaChegouNoLugar(ObjetivoGuia obj)
        {
            if (!obj.PrecisaEvento)
            {
                if(obj.Local != Point3D.Zero && Jogador.GetDistance(obj.Local) <= 6)
                    return true;
                if (obj.LocalDungeon != Point3D.Zero && Jogador.GetDistance(obj.LocalDungeon) <= 6)
                    return true;
            }
            return false;
        }

        public void Completa(ObjetivoGuia obj)
        {
            if (Jogador.QuestArrow != null)
            {
                Jogador.QuestArrow.Stop();
                Jogador.QuestArrow = null;
            }

            if (obj.Completar != null)
            {
                obj.Completar(Jogador);
                if (obj.FraseCompletar != null)
                {
                    Fala(obj.FraseCompletar);
                }
            }
            var proximo = obj.Proximo;
            if(obj.GetProximo != null)
            {
                proximo = obj.GetProximo(Jogador);
            }
            if (proximo != PassoTutorial.NADA && proximo != PassoTutorial.FIM)
            {
                this.DebugSay("Avancando objetivo para " + proximo);
                SetCooldown("pensa", TimeSpan.FromSeconds(5));
                Jogador.PassoWispGuia = (int)PassoTutorial.NADA;
                Timer.DelayCall(TimeSpan.FromSeconds(3), () =>
                {
                    Jogador.PassoWispGuia = (int)proximo;
                    var objProximo = Guia.Objetivos[proximo];
                    if (objProximo.FraseIniciar != null)
                    {
                        if(!Jogador.InCombat(TimeSpan.FromSeconds(2)))
                        {
                            Jogador.SendMessage(objProximo.FraseIniciar);

                            var splitado = objProximo.FraseIniciar.Split('.');
                            if(splitado.Length <= 1 || splitado[splitado.Length-1].Length <= 2)
                            {
                                splitado = objProximo.FraseIniciar.Split(',');
                            }
                            if (splitado.Length <= 1 || splitado[splitado.Length - 1].Length <= 2)
                            {
                                splitado = objProximo.FraseIniciar.Split('!');
                            }
                           for(var i = 0; i < splitado.Length; i++)
                            {
                                splitado[i] = splitado[i].Trim();
                            }
                            Jogador.SendGump(new GumpFala((int i) => { }, Faces.FADA, splitado));
                        } else
                        {
                            Fala(objProximo.FraseIniciar);
                        }
                       
                        SetCooldown("fala", TimeSpan.FromSeconds(20));
                    }
                });
            } else if(proximo == PassoTutorial.FIM)
            {
                Jogador.PassoWispGuia = (int)PassoTutorial.FIM;
            }
        }

        private int falaAtual = 0;

        public void FazAlgo()
        {
            if(Jogador.Combatant != null && this.Combatant == null)
            {
                var bixo = Jogador.Combatant as BaseCreature;
                if(bixo != null)
                {
                    if(Utility.RandomDouble() < 0.2)
                    {
                        Fala("Toma isso, " + bixo.Name);
                    }
                    this.Combatant = bixo;
                }
            }

            this.DebugSay("Fazendo algo");
            var objetivoAtual = (PassoTutorial)Jogador.PassoWispGuia;

            if (objetivoAtual == PassoTutorial.FIM)
            {
                if (!IsCooldown("fala") && falaAtual != -1)
                {
                    SetCooldown("fala", TimeSpan.FromSeconds(100));
                    switch (falaAtual)
                    {
                        case 0: Fala("Upe sempre em dungeons, voce aprende as skills mais rapido matando monstros !"); break;
                        case 1: Fala("Quando tiver 70 nas skills das armas, voce pode usar as habilidades de arma clicando no seu paperdoll (ALT + P) e depois no livro lilas no seu pe. !"); break;
                        case 2: Fala("Sabia que as moedas de ouro , quando usando .grupo, sao mais faceis de conseguir ?"); break;
                        case 3: Fala("Fazendo grupos no pergaminho no pe do paperdoll o gold divide automatico !"); break;
                        case 4: Fala("Se voce quiser upar skills worker, voce pode fazer Ordens de Compra nos npcs se tiver skills de trabalho !"); break;
                        case 5: Fala("Voce pode upar elementos PvM e ficar 10000x mais forte em PvM !"); break;
                        case 6: Fala("Voce precisa de um bom dinheiro e equipamento pra conseguir destravar seus Elementos PvM"); break;
                        case 7: Fala("Use o moongate publico em Haven para se locomover facilmente"); break;
                        case 8: Fala("No seu paperdoll, em Help, voce pode encontrar onde pode ir como dungeons e missoes"); break;
                        case 9: Fala("Se tiver alguma duvida, mande page a staff clicando em Help no seu paperdoll"); break;
                        case 10: falaAtual = -2; break;
                    }
                    falaAtual++;
                }
                return;
            }

            if (objetivoAtual == PassoTutorial.NADA)
                return;

            var obj = Guia.Objetivos[objetivoAtual];

            if (ValidaChegouNoLugar(obj))
            {
                Completa(obj);
                return;
            }

            if (obj.Local != Point3D.Zero)
            {
                if (!(Jogador.Region is DungeonRegion || Jogador.Region is DungeonGuardedRegion))
                {
                    Jogador.QuestArrow = new QuestArrow(Jogador, obj.Local);
                    Jogador.QuestArrow.Update();
                } 
            }

            if (obj.LocalDungeon != Point3D.Zero)
            {
                if ((Jogador.Region is DungeonRegion || Jogador.Region is DungeonGuardedRegion))
                {
                    Jogador.QuestArrow = new QuestArrow(Jogador, obj.LocalDungeon);
                    Jogador.QuestArrow.Update();
                }     
            }

            if (!IsCooldown("fala") && obj.FraseProgresso != null)
            {

                SetCooldown("fala", TimeSpan.FromSeconds(60));
                Fala(obj.FraseProgresso);

                if(obj.OnCheck != null)
                {
                    obj.OnCheck(Jogador, obj);
                }

            }
        }

        public override void OnThink()
        {
          
            if (Jogador == null)
            {
                if (ControlMaster != null && ControlMaster is PlayerMobile)
                {
                    Jogador = (PlayerMobile)ControlMaster;
                    Jogador.Wisp = this;
                    if (!IsPetFriend(Jogador))
                        AddPetFriend(Jogador);
                    this.AIObject.EndPickTarget(Jogador, Jogador, OrderType.Follow);
                    this.AceitaOrdens = false;
                    this.Hue = 0;
                }
                else
                {
                    this.Hue = 38;
                    return;
                }
            }

            if (Jogador == null)
                return;


            if (IsCooldown("pensa"))
                return;

            if (Jogador.PassoWispGuia == (int)PassoTutorial.NADA)
            {
                Jogador.PassoWispGuia = (int)PassoTutorial.PEGAR_CAVALO;
            }

            var dist = this.GetDistance(Jogador);
            if (dist > 20)
            {
                this.DebugSay("Teleportando");
                this.MoveToWorld(Jogador.Location, Jogador.Map);
            }
            else
            {
                this.DebugSay("Seguindo");
                this.AIObject.EndPickTarget(Jogador, Jogador, OrderType.Follow);
            }

            if (Jogador.Region is DungeonRegion || Jogador.Region is DungeonGuardedRegion)
            {
                if (!Jogador.HasAction(typeof(LightCycle)))
                {
                    var spell = new NightSightSpell(this, null);
                    spell.InstantTarget = Jogador;
                    spell.Cast();
                }
            }
            FazAlgo();
            SetCooldown("pensa", TimeSpan.FromSeconds(4));
            SetHits(10000);
        }

        public override void AbreBanco()
        {
            if (Jogador.PassoWispGuia == (int)PassoTutorial.IR_BANCO)
                Completa(Guia.Objetivos[PassoTutorial.IR_BANCO]);
        }

        public override void QuestNoob()
        {
            if (Jogador.PassoWispGuia == (int)PassoTutorial.PEGAR_QUEST)
                Completa(Guia.Objetivos[PassoTutorial.PEGAR_QUEST]);
        }

        public override void EntregaSapato()
        {
            if (Jogador.PassoWispGuia == (int)PassoTutorial.VOLTAR_QUEST)
                Completa(Guia.Objetivos[PassoTutorial.VOLTAR_QUEST]);
        }

        public override void MataMerda()
        {
            if (Jogador.PassoWispGuia == (int)PassoTutorial.BIXO_ESGOTO)
                Completa(Guia.Objetivos[PassoTutorial.BIXO_ESGOTO]);
        }

        public override void FalaJill()
        {
            if (Jogador.PassoWispGuia == (int)PassoTutorial.JILL)
                Completa(Guia.Objetivos[PassoTutorial.JILL]);
        }

        public override void MataMago()
        {
            if (Jogador.PassoWispGuia == (int)PassoTutorial.MATAR_MAGO)
                Completa(Guia.Objetivos[PassoTutorial.MATAR_MAGO]);
        }

        public override void Moongate()
        {
            if (Jogador.PassoWispGuia == (int)PassoTutorial.MOONGATE)
                Completa(Guia.Objetivos[PassoTutorial.MOONGATE]);
        }

        public override void QuestCasa()
        {
            if (Jogador.PassoWispGuia == (int)PassoTutorial.QUEST_CASA)
                Completa(Guia.Objetivos[PassoTutorial.QUEST_CASA]);
        }

        public override void CompletaJill()
        {
            if (Jogador.PassoWispGuia == (int)PassoTutorial.COMPLETA_JILL)
                Completa(Guia.Objetivos[PassoTutorial.COMPLETA_JILL]);
        }

        public NovoWispGuia(Serial serial)
            : base(serial)
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
    }
}
