using System;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Commands
{
    public class PlayerPvM
    {
        public static void Initialize()
        {
            CommandSystem.Register("playerpvm", AccessLevel.Administrator, new CommandEventHandler(SetPvM_OnCommand));
        }

        [Usage("playerpvm")]
        [Description("seta flag player pvm")]
        public static void SetPvM_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            m.SendMessage("Selecione o jogador que deseja tornar player PvM.");
            m.Target = new SetPvMTarget(m);
        }
        public class SetPvMTarget : Target
        {
            private Mobile utilizzatore;

            public SetPvMTarget(Mobile m) : base(18, false, TargetFlags.None)
            {
                utilizzatore = m;
            }
            protected override void OnTarget(Mobile from, object target)
            {
                if (target is PlayerMobile)
                {
                    target.PvM = true;
                }
                else
                {
                    from.SendMessage("este não é um alvo válido");
                }
            }
        }
    }
}
