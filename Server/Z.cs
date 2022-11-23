using System;

namespace Server
{
    public class Shard
    {
        public static int MaxOnline = 200;
        public static bool SohNossoClient = false;
        public static bool DebugGritando = false;

        public static bool WARSHARD { get { return Config.Get("General.Warshard", false); } }
        public static bool RP { get { return Config.Get("General.RP", false); } }
        public static bool GUIA { get { return Config.Get("General.Guia", false); } }
        public static bool WHITELIST { get => false; }

        public static bool PASCOA = false;

        public static bool CRASH = true;

        public static string BotID { get { return Config.Get("General.BotID", ""); } }
        public static string BotKey { get { return Config.Get("General.BotKey", ""); } }

        public static bool T2A = true;

        public static bool TITULOS_RP = false;

        public static string SEGREDO_WEB_API = "segredowebapi";

        public static bool MAPA_CUSTOM = true;
        public static bool TROCA_ARMA_RAPIDA = true;

        public static bool TEMPLATES = false;

        public static bool DebugEnabled { get { return Config.Get("General.Debug", false); } }

        public static bool POL_STYLE { get { return Config.Get("General.POL", false); } }

        public static bool CAST_CLASSICO { get { return Config.Get("General.CAST_CLASSICO", false); } }

        public static bool COMBATE_SPHERE => SPHERE_STYLE || RP;
        public static bool SPHERE_STYLE { get { return Config.Get("General.SPHERE", false); } }

        public static bool POL_SPHERE { get { return SPHERE_STYLE || POL_STYLE; } }

        public static bool AVENTURA { get { return Config.Get("General.AVENTURA", false); } }

        public static bool EXP = !Shard.SPHERE_STYLE;

        public static bool NECRO = false;

        public static void Erro(string str, Mobile from = null)
        {
            if(from != null)
                Console.WriteLine(from.Name+" [Error]: "+str);
            else
                Console.WriteLine("[Error]: " + str);
        }

        public static void Info(string str)
        {
            Console.WriteLine("[Info]: " + str);
        }

        public static void Debug(string str, Mobile from = null)
        {
            if (!DebugEnabled)
                return;

            if(DebugGritando)
            {
                if (from == null)
                    World.Broadcast(78, false, AccessLevel.GameMaster, str);
                else
                    from.SendMessage(78, "[Debug]" + str);
            }

            if (from != null)
            {
                str += "[" + from.Name + "]";
                //if (from.Player && from.AccessLevel > AccessLevel.Player)
                //    from.SendMessage(str);
            }
            Console.WriteLine(str);
        }
    }
}
