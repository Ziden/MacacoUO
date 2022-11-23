#region References
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Server.Commands.Generic;
using Server.Engines.BulkOrders;
using Server.Items;
using Server.Misc;
using Server.Network;
#endregion

namespace Server.Commands
{
    public class GoldHour
    {
        public static double GOLD_MULT = 0;

        public static void Initialize()
        {
            CommandSystem.Register("goldhour", AccessLevel.Administrator, OnAction);

            if (Shard.RP)
                return;

            var dateNow = DateTime.Now;
            var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 01, 00, 00);
            TimeSpan ts;
            if (date > dateNow)
                ts = date - dateNow;
            else
            {
                date = date.AddHours(12);
                ts = date - dateNow;
            }
            var cooldown = ts;
            Timer.DelayCall(cooldown, () =>
            {
                if (GOLD_MULT != 0)
                    return;

                Anuncio.Anuncia("GOLDHOUR !! Bonus de GOLD de monstros por 2 Horas !");
                GOLD_MULT = 1.5;
                Timer.DelayCall(TimeSpan.FromHours(2), () => {
                    GOLD_MULT = 0;
                    Anuncio.Anuncia("O GoldHour Terminou !");
                });
            });

        }

        [Usage("Action")]
        private static void OnAction(CommandEventArgs e)
        {
            var horas = 1;
            if(e.Arguments.Count() > 0)
                horas = e.GetInt32(0);


            if (GOLD_MULT > 0)
            {
                e.Mobile.SendMessage("Ja tem um bonus ativo");
                return;
            }

            GOLD_MULT = 1.5;

            var str = horas + "hora";
            if (horas > 1)
                str += "s";

            Anuncio.Anuncia("GOLDHOUR !! Bonus de GOLD por " + str);

            Timer.DelayCall(TimeSpan.FromHours(horas), () => {
                GOLD_MULT = 0;
                Anuncio.Anuncia("O GoldHour Terminou !");
            });


        }
    }
}
