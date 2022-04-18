using System;
using System.Linq;
using System.Collections.Generic;

using Server;
using Server.Gumps;
using Server.TournamentSystem;

namespace Server.Mobiles
{
    public class ArenaKeeper : Healer
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public PVPTournamentSystem System { get; set; }

        public ArenaKeeper(PVPTournamentSystem system)
        {
            System = system;
            this.Name = "Bruce Buffer";
            this.Title = "";
            this.Blessed = true;
            this.CanMove = false;
            this.Direction = Direction.South;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (System != null && !string.IsNullOrEmpty(System.Name))
            {
                list.Add(String.Format("Gerente da {0}", System.Name));
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!InRange(from, 3))
                base.OnDoubleClick(from);
            else if (System != null)
            {
                if (from.HasGump(typeof(TournamentStoneGump)))
                    from.CloseGump(typeof(TournamentStoneGump));

                if (from.HasGump(typeof(BaseTournamentGump)))
                    return;

                if(from is PlayerMobile)
                    BaseGump.SendGump(new TournamentStoneGump(System, from as PlayerMobile));

                SayTo(from, false, String.Format("Bem-vindo à {0}, {1}!", System.Name, from.Name));
            }
        }

        public override void OfferResurrection(Mobile m)
        {
            Direction = GetDirectionTo(m);

            m.PlaySound(0x1F2);
            m.FixedEffect(0x376A, 10, 16);
            m.Resurrect();

            Timer.DelayCall(TimeSpan.FromSeconds(0.5), AfterResurrection, m);
        }

        public override bool CheckResurrect(Mobile m)
        {
            if (m.Criminal)
            {
                Say(501222); // Thou art a criminal.  I shall not resurrect thee.
                return false;
            }

            return true;
        }

        public override bool HandlesOnSpeech(Mobile from)
        {
            return System != null;
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            var speech = e.Speech;
            e.Handled = true;

            if (!string.IsNullOrEmpty(speech) && _Sayings.Any(s => s == speech.ToLower()))
            {
                var m = e.Mobile as PlayerMobile;
                var team = ArenaTeam.GetTeam(m, ArenaTeamType.Single);

                if (m != null && team != null && System.FightTypes.Any(type => type == ArenaFightType.SingleElimination))
                {
                    if (ActionTimer.WaitingAction.ContainsKey(m))
                    {
                        //m.SendMessage("You must wait a few moments to use this command.");
                        m.SendMessage("Aguarde para usar este comando novamente.");
                    }
                    else if (!System.CanRegisterFight(m))
                    {
                        //m.SendMessage("You are already in queue or in a duel.");
                        m.SendMessage("Você já está na fila ou em um duelo.");
                    }
                    else
                    {
                        ArenaFight fight = new SingleEliminationFight(System, null);
                        fight.FightType = ArenaTeamType.Single;

                        m.Target = new RegisterFightGump.InternalTarget(System, fight, m, true, true);
                        //m.SendMessage("Target the player you'd like to duel.");
                        m.SendMessage("Selecione o jogador que deseja duelar.");
                    }
                }
            }
        }

        // all lower case please
        private static string[] _Sayings = new[]
        {
            /*"i wish to duel",
            "i wish to challenge you to a duel",
            "[i wish to duel",
            "[i wish to challenge you to a duel"*/
            "duelo",
            "duelar"
        };

        private Dictionary<Mobile, DateTime> _TalkTable;

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (m.Alive && !m.Hidden &&
                m is PlayerMobile &&
                m.InRange(Location, 7) &&
                System != null &&
                System.Stone != null &&
                League.HasValidLeagues((PlayerMobile)m, System.Stone) &&
                (_TalkTable == null || !_TalkTable.ContainsKey(m) || _TalkTable[m] < DateTime.UtcNow))
            {
                var oldSpeech = SpeechHue;
                SpeechHue = 0x3B2;
                //SayTo(m, "Hail league participant! Use the arena stone to register your league match!");
                SayTo(m, "Olá participante! Use a pedra da arena para registrar sua partida da liga!");
                SpeechHue = oldSpeech;

                if (_TalkTable == null)
                {
                    _TalkTable = new Dictionary<Mobile, DateTime>();
                }

                _TalkTable[m] = DateTime.UtcNow + TimeSpan.FromMinutes(2);
            }
        }

        private void DefragTalkTable()
        {
            if (_TalkTable == null)
            {
                return;
            }

            var mobs = new List<Mobile>(_TalkTable.Keys);

            foreach (var m in mobs)
            {
                if (_TalkTable[m] < DateTime.UtcNow)
                {
                    _TalkTable.Remove(m);
                }
            }

            if (_TalkTable.Count == 0)
            {
                _TalkTable = null;
            }
        }


        public static void AfterResurrection(Mobile m)
        {
            if (m == null)
                return;

            if (m.Corpse != null)
            {
                Items.Corpse corpse = (Items.Corpse)m.Corpse;
                corpse.Location = m.Location;
                corpse.Map = m.Map;
                corpse.Open(m, true);
            }

            m.Hits = m.HitsMax;
            m.Mana = m.ManaMax;
            m.Stam = m.StamMax;

            //ArenaHelper.DoArenaKeeperMessage(String.Format("Better luck next time, {0}!", m.Name), m);
            ArenaHelper.DoArenaKeeperMessage(String.Format("Boa sorte da próxima vez, {0}!", m.Name), m);
        }

        public ArenaKeeper( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
        	writer.Write( (int) 0 ); // version

            DefragTalkTable();
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
    }
}
