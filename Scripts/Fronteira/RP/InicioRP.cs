using Server.Gumps;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Fronteira.RP
{

    public class TeleporterInicio : Item
    {
        [Constructable]
        public TeleporterInicio() : base(0xF6C)
        {
            Name = "Portal da Vida";
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }

        public override void OnDoubleClick(Mobile m)
        {
            base.OnDoubleClick(m);
            if (m.HasGump<RPClassGump>())
                return;

            if (m.IsCooldown("portalvida"))
                return;

            m.SetCooldown("portalvida", TimeSpan.FromSeconds(3));
            m.SendMessage("Algumas escolhas na vida, podemos fazer...");
            m.SendMessage("Eis sua primeira escolha...");
            Timer.DelayCall(TimeSpan.FromSeconds(3), () => {
                m.SendGump(new RPClassGump());
            });
        }

        public override bool OnMoveOver(Mobile m)
        {
            base.OnMoveOver(m);
            if (m.HasGump<RPClassGump>())
                return false;

            if (m.IsCooldown("portalvida"))
                return true;

            m.SetCooldown("portalvida", TimeSpan.FromSeconds(3));
            m.SendMessage("Algumas escolhas na vida, podemos fazer...");
            m.SendMessage("Eis sua primeira escolha...");
            Timer.DelayCall(TimeSpan.FromSeconds(3), () => {
                m.SendGump(new RPClassGump());
            });
            return true;
        }
    }

    public class InicioRP
    {
        public static void InitializaPlayer(PlayerMobile player)
        {
            player.MoveToWorld(new Point3D(857, 2784, 5), Map.TerMur);
            if(player.BodyMod == 0)
            {
                player.OverheadMessage("* sua alma revive *");
                Timer.DelayCall(TimeSpan.FromSeconds(2), () => {
                    player.OverheadMessage("* explorando *");
                });
                player.BodyMod = 58;
            }
        }
    }
}
