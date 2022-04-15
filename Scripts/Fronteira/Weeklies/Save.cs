using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Server.Fronteira.Weeklies.Weekly;

namespace Server.Fronteira.Weeklies
{
    public class SaveRP
    {
        private static string FilePath = Path.Combine("Saves/RP", "RP.bin");
        private static bool Carregado;

        public static int PontosBons = 0;
        public static int PontosRuims = 0;

        public static void Configure()
        {
            Console.WriteLine("Inicializando save RP");
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }
       
        private static void Salva(GenericWriter writer)
        {
            Console.WriteLine("Salvando RP");
            writer.Write(0);
            writer.Write(PontosBons);
            writer.Write(PontosRuims);
           
        }

        private static void Carrega(GenericReader reader)
        {
            Console.WriteLine("Carregando RP");
            var ver = reader.ReadInt();
            PontosBons = reader.ReadInt();
            PontosRuims = reader.ReadInt();

        }

        public static void OnSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(FilePath, Salva);

        }

        public static void OnLoad()
        {
            if (!Carregado)
            {
                Console.WriteLine("Carregando weeklies");
                Persistence.Deserialize(FilePath, Carrega);
                Carregado = true;
            }

        }
    }
}
