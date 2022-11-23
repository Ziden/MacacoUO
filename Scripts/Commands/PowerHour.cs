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
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
#endregion

namespace Server.Commands
{
    public class PowerCmd
    {
        public static void Initialize()
        {
            CommandSystem.Register("powerhour", AccessLevel.Administrator, OnAction);
            EventSink.Login += OnLogin;
            Inicial();

        }

        public static void OnLogin(LoginEventArgs e)
        {
            if(SkillCheck.BONUS_GERAL != 0)
            {
                e.Mobile.SendGump(new AnuncioGump(e.Mobile as PlayerMobile, "Bonus de XP esta Ativo"));
            }
            if (GoldHour.GOLD_MULT != 0)
            {
                e.Mobile.SendGump(new AnuncioGump(e.Mobile as PlayerMobile, "Bonus de GOLD esta Ativo"));
            }
        }

        public static void Inicial()
        {
            if (Shard.RP)
                return;

            var dateNow = DateTime.Now;
            var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 23, 0, 0);
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
                if (SkillCheck.BONUS_GERAL != 0)
                    return;

                Anuncio.Anuncia("POWEHOUR !! Bonus de UP por 2 Horas !");
                SkillCheck.BONUS_GERAL = 2;
                Timer.DelayCall(TimeSpan.FromHours(2), () => {
                    SkillCheck.BONUS_GERAL = 0;
                    Anuncio.Anuncia("O PowerHour de XP Terminou !");
                    Inicial();
                });
            });
        }


        [Usage("Action")]
        private static void OnAction(CommandEventArgs e)
        {
            var horas = 1;
            if(e.Arguments.Count() > 0)
                horas = e.GetInt32(0);

            if(SkillCheck.BONUS_GERAL > 0)
            {
                e.Mobile.SendMessage("Ja tem um bonus ativo");
                return;
            }

            SkillCheck.BONUS_GERAL = 1.5;

            var str = horas + "hora";
            if (horas > 1)
                str += "s";

            Anuncio.Anuncia("POWEHOUR !! Bonus de UP por " + str);

            Timer.DelayCall(TimeSpan.FromHours(horas), () => {
                SkillCheck.BONUS_GERAL = 0;
                Anuncio.Anuncia("O PowerHour de XP Terminou !");
            });


        }
    }
}
