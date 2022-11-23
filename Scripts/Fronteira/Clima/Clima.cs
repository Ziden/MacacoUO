using Server.Gumps;
using Server.Misc;
using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Fronteira.Clima
{
    public class Clima
    {
        public static readonly int MAX = 50;

        public static readonly int MIN = -50;

        private static HashSet<Mobile> _emTemp = new HashSet<Mobile>();

        private static Dictionary<string, int> _temperaturas = new Dictionary<string, int>();

        private static string FilePath = Path.Combine("Saves/Climas", "Climas.bin");

        public static void Configure()
        {
            /*
            if(Shard.RP)
            {
                EventSink.OnEnterRegion += EnterRegion;
                EventSink.Login += Loga;
                EventSink.Logout += Desloga;
                EventSink.WorldLoad += OnLoad;
                EventSink.WorldSave += OnSave;
                var timer = new Loop();
                timer.Start();
            }
            */
        }

        public static void OnSave(WorldSaveEventArgs e)
        {
            Console.WriteLine("Salvando Climas");
            Persistence.Serialize(FilePath, Salva);
        }

        public static void OnLoad()
        {
            Console.WriteLine("Carregando Climas");
            Persistence.Deserialize(FilePath, Carrega);
        }

        private static void Salva(GenericWriter writer)
        {
            writer.Write(_temperaturas.Count);
            foreach (var temp in _temperaturas)
            {
                writer.Write(temp.Key);
                writer.Write(temp.Value);
            }
        }

        private static void Carrega(GenericReader reader)
        {
            var ct = reader.ReadInt();
            for (var x = 0; x < ct; x++)
            {
                var k = reader.ReadString();
                var v = reader.ReadInt();
                _temperaturas[k] = v;
            }
        }

        public static void Loga(LoginEventArgs e)
        {
            Mobile mob = e.Mobile;
            if (Shard.DebugEnabled)
                Shard.Debug("trackeando Temperatura", mob);
            _emTemp.Add(mob);

            var tempVeia = mob.Temperatura;
            var tempNova = Clima.GetTemperatura(mob.Region);

            switch (mob.Map.Season)
            {
                case 0: //primavera
                    tempNova += 5;
                    break;
                case 1: //verão
                    tempNova += 10;
                    break;
                case 2: //outono
                    tempNova -= 5;
                    break;
                case 3: //inverno
                    tempNova -= 10;
                    break;
                case 4: //desolation
                    tempNova *= 2;
                    break;
                default:
                    break;
            }
            mob.Temperatura = tempNova;

            if (tempVeia < tempNova)
            {
                mob.SendMessage("Voce sente o clima esquentar");
               
                if (mob.HasGump(typeof(FomeSede)))
                        mob.CloseGump(typeof(FomeSede));
                    mob.SendGump(new FomeSede(mob));

            }
            else if (tempVeia > tempNova)
            {
                mob.SendMessage("Voce sente o clima esfriar");

                if (mob.HasGump(typeof(FomeSede)))
                    mob.CloseGump(typeof(FomeSede));
                mob.SendGump(new FomeSede(mob));
            } 
        }
        public static void Desloga(LogoutEventArgs e)
        {
            Mobile mob = e.Mobile;
            if (Shard.DebugEnabled)
                Shard.Debug("trackeando Temperatura", mob);
            _emTemp.Remove(mob);
        }

        public static int GetProtecao(Mobile m)
        {
            if (Shard.DebugEnabled)
                Shard.Debug(String.Format("Prot Calor: {0} Prot Frio: {1} ", m.FireResistance, m.ColdResistance), m);
            return m.FireResistance - m.ColdResistance;
        }

        public static int GetSensacaoTermica(Mobile m)
        {
            return 0;
            /*
            int protecao = GetProtecao(m);
            int sensacao = m.Temperatura - protecao;
            if (Shard.DebugEnabled)
                Shard.Debug(String.Format("Temperatura: {0} Proteção: {1} Sensação: {2}", m.Temperatura, protecao, sensacao), m);
            return sensacao;
             */
        }

        private class Loop : Timer
        {
            public Loop() : base(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20))
            {

            }

            private static Dictionary<Mobile, int> Danos = new Dictionary<Mobile, int>();

            protected override void OnTick()
            {
                int SensacaoTermica;
                if (Shard.DebugEnabled)
                    Shard.Debug("Timer de Clima");

                Danos.Clear();
                foreach (var player in _emTemp)
                {
                    if (!player.Alive)
                        continue;

                    if (Shard.DebugEnabled)
                        Shard.Debug("Vendo temperatura do manolo " + player.Name);

                    //Atualiza temperatura dos Players

                    var tempVeia = player.Temperatura;
                    var tempNova = Clima.GetTemperatura(player.Region);

                    switch (player.Map.Season)
                    {
                        case 0: //primavera
                            tempNova += 5;
                            break;
                        case 1: //verão
                            tempNova += 10;
                            break;
                        case 2: //outono
                            tempNova -= 5;
                            break;
                        case 3: //inverno
                            tempNova -= 10;
                            break;
                        case 4: //desolation
                            tempNova *= 2;
                            break;
                        default:
                            break;
                    }
                    foreach (Weather tempo in Weather.GetWeatherList(player.Map)) { 
                        if(tempo.IntersectsWith(new Rectangle2D(player.X, player.Y, 1, 1)))
                        {
                            tempNova += tempo.Temperature;
                            break;
                        }
                    }

                    player.Temperatura = tempNova;

                    if (tempVeia < tempNova)
                    {
                        player.SendMessage("Voce sente o clima esquentar");

                        if (player.HasGump(typeof(FomeSede)))
                            player.CloseGump(typeof(FomeSede));
                        player.SendGump(new FomeSede(player));
                    }
                    else if (tempVeia > tempNova)
                    {
                        player.SendMessage("Voce sente o clima esfriar");

                        if (player.HasGump(typeof(FomeSede)))
                            player.CloseGump(typeof(FomeSede));
                        player.SendGump(new FomeSede(player));
                    }

                    SensacaoTermica = Clima.GetSensacaoTermica(player);

                    //Calcula efeitos nocivo no personagem
                    if (SensacaoTermica <= -10) //Só causa efeitos de frios com sensação térmica de pelo menos -10 no valor base
                    {
                        var dano = (Math.Abs(SensacaoTermica)-10)/2; 
                        if (dano > 25) // O dano máximo de frio é 25
                            dano = 25;

                        player.Stam -= dano;
                        player.SendMessage("Voce esta com frio e precisa de roupas mais quentes para se esquentar");

                        if (player.Female)
                            player.PlaySound(0x332);
                        else
                            player.PlaySound(0x444);

                        if (player.AvisoTemperatura)
                        {
                            Danos.Add(player, dano);
                        }
                        else
                        {
                            player.SendMessage(78, "Se voce permanecer muito tempo em um local frio sem roupas adequadas sofrera dano e perda de stamina");
                            player.AvisoTemperatura = true;
                        }
                    }
                    else if (SensacaoTermica >= 10)
                    {

                        var dano = (Math.Abs(SensacaoTermica) - 10) / 2;
                        if (dano > 25) // O dano máximo e calor é 25
                            dano = 25;

                        player.SendMessage("Voce esta com calor");
                        FomeSedeDecay.ThirstDecay(player); //aumenta a sede mais rápido
                        player.Stam -= dano;

                        if (player.AvisoTemperatura)
                        {
                            Danos.Add(player, dano);
                        }
                        else
                        {
                            player.SendMessage(78, "Se voce permanecer muito tempo em um local quente sem roupas apropriadas ira receber dano e ficar com sede mais rapidamente");
                            player.AvisoTemperatura = true;
                        }
                    }
                    else
                    {
                        player.AvisoTemperatura = false;
                    }
                }

                foreach (var m in Danos)
                {
                    AOS.Damage(m.Key, m.Value, DamageType.Spell);
                }
            }
        }

        private static void EnterRegion(OnEnterRegionEventArgs e)
        {
            Mobile mob = e.From;
            var tempVeia = mob.Temperatura;
            var tempNova = Clima.GetTemperatura(e.NewRegion);

            switch (mob.Map.Season)
            {
                case 0: //primavera
                    tempNova += 5;
                    break;
                case 1: //verão
                    tempNova += 10;
                    break;
                case 2: //outono
                    tempNova -= 5;
                    break;
                case 3: //inverno
                    tempNova -= 10;
                    break;
                case 4: //desolation
                    tempNova *= 2;
                    break;
                default:
                    break;
            }

            mob.Temperatura = tempNova;

            if (tempVeia < tempNova)
            {
                mob.SendMessage("Voce sente o clima esquentar");

                if (mob.HasGump(typeof(FomeSede)))
                    mob.CloseGump(typeof(FomeSede));
                mob.SendGump(new FomeSede(mob));

            }
            else if (tempVeia > tempNova)
            {
                mob.SendMessage("Voce sente o clima esfriar");

                if (mob.HasGump(typeof(FomeSede)))
                    mob.CloseGump(typeof(FomeSede));
                mob.SendGump(new FomeSede(mob));
            }

        }

        
        public static void SetTemperatura(Region region, int graus)
        {
            _temperaturas[region.Name] = graus;
        }

        public static int GetTemperatura(Region region)
        {
            int temperatura = 0; // neutro
            if (region.Name != null)
            {
                if (_temperaturas.ContainsKey(region.Name))
                {
                    temperatura = _temperaturas[region.Name];
                }
            }
            return temperatura;
        }

    }
}
