
using Server.Engines.Harvest;
using Server.Items;

namespace Server.Services.Harvest
{
    public class Dificuldade
    {
        public int Min;
        public int Max;
        public int Required;

        public Dificuldade(int required, int max)
        {
            this.Required = required;
            this.Min = required - 15;
            this.Max = max;
        }

        public static Dificuldade GetDificuldade(CraftResource res)
        {
            switch(res)
            {
                // Metais
                case CraftResource.Ferro: return Mining.FERRO;
                case CraftResource.Cobre: return Mining.COBRE;
                case CraftResource.Bronze: return Mining.BRONZE;
                case CraftResource.Dourado: return Mining.DOURADO;
                case CraftResource.Niobio: return Mining.NIOBIO;
                case CraftResource.Lazurita: return Mining.LAZURITA;
                case CraftResource.Berilo: return Mining.BERILO;
                case CraftResource.Quartzo: return Mining.QUARTZO;
                case CraftResource.Vibranium: return Mining.VIBRANIUM;
                case CraftResource.Adamantium: return Mining.ADAMANTIUM;

                // Paus
                case CraftResource.Gelo: return Lumberjacking.GELO;
                case CraftResource.Carmesim: return Lumberjacking.CARMESIN;
                case CraftResource.Eucalipto: return Lumberjacking.EUCALIPTO;
                case CraftResource.Mogno: return Lumberjacking.MOGNO;
                case CraftResource.Pinho: return Lumberjacking.PINHO;
                case CraftResource.Carvalho: return Lumberjacking.CARVALHO;
            }
            return null;
        }
    }



}
