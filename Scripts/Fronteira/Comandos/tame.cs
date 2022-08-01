using System;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Commands
{
    public class IstaTame
    {
        public static void Initialize()
        {
            CommandSystem.Register("tame", AccessLevel.Administrator, new CommandEventHandler(IstaTame_OnCommand));
        }

        [Usage("Tame")]
        [Description("domesticar imediatamente")]
        public static void IstaTame_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            m.SendMessage("O que você deseja domar l");
            m.Target = new IstaTameTarget(m);
        }
        public class IstaTameTarget : Target
        {
            private Mobile utilizzatore;

            public IstaTameTarget(Mobile m) : base(18, false, TargetFlags.None)
            {
                utilizzatore = m;
            }
            protected override void OnTarget(Mobile from, object target)
            {
                if (target is BaseCreature)
                {
                    BaseCreature t = (BaseCreature)target;
                    t.Controlled = true;
                    t.ControlMaster = utilizzatore;
                    if (t.Tamable) t.OnAfterTame(from);
                    from.SendMessage(58, "a criatura está agora sob seu controle");
                    if (!t.Tamable) from.SendMessage(1172, "isso não pode ser domado");
                }
                else
                {
                    from.SendMessage("este não é um alvo válido");
                }
            }
        }
    }
}
