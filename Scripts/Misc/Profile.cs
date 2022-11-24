using System;
using Felladrin.Engines;
using Server.Accounting;
using Server.Engines.Points;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
    public class Profile
    {
        public static void Initialize()
        {
            EventSink.ProfileRequest += new ProfileRequestEventHandler(EventSink_ProfileRequest);
            EventSink.ChangeProfileRequest += new ChangeProfileRequestEventHandler(EventSink_ChangeProfileRequest);
        }

        public static void EventSink_ChangeProfileRequest(ChangeProfileRequestEventArgs e)
        {
            Mobile from = e.Beholder;
            if (from.ProfileLocked)
                from.SendMessage("Perfil Trancado.");
            else
            {
                var pl = from as PlayerMobile;
                if(pl != null)
                {
                    from.SendMessage("Salvo");
                    Shard.Debug(e.Text);
                    from.Profile = e.Text;
                    //pl.FichaRP. = e.Text;
                }
            }
        }

        public static void EventSink_ProfileRequest(ProfileRequestEventArgs e)
        {
            Mobile beholder = e.Beholder;
            Mobile beheld = e.Beheld;

            if (beheld is BaseHire)
            {
                var bh = (BaseHire)beheld;
                if (bh.ControlMaster == beholder)
                {
                    beholder.SendGump(new SkillsGump(beholder, bh));
                } else
                {
                    bh.SayTo(beholder, "Oxe vc n eh meu mestre, nao vou te revelar minhas skills...");
                }
                return;
            }

            if (e.Beheld == e.Beholder && e.Beheld.Player)
            {
                e.Beholder.SendGump(new FichaRP(e.Beheld as PlayerMobile));
                if(!Shard.RP)
                    e.Beholder.SendGump(new GlobalChat.ChatHistoryGump());
            }
            else
            {
                if (e.Beheld is PlayerMobile)
                {
                    var alvo = e.Beheld as PlayerMobile;
                    beholder.Send(new DisplayProfile(beholder != beheld || !beheld.ProfileLocked, beheld, "Aparencia de "+beheld.Name, alvo.FichaRP.Aparencia, ""));
                    //EventSink.InvokeProfileRequest(new ProfileRequestEventArgs(e.Beholder, e.Beheld, alvo.FichaRP.Aparencia == null ? "Em Branco.." : alvo.FichaRP.Aparencia));
                }
                else
                {
                    e.Beholder.SendMessage("Ainda nao implementado");
                }

            }

            /*

            if (!beheld.Player)
                return;

            if (beholder.Map != beheld.Map || !beholder.InRange(beheld, 12) || !beholder.CanSee(beheld))
                return;

            string header = Titles.ComputeTitle(beholder, beheld);

            string footer = "";

            if (beheld.ProfileLocked)
            {
                if (beholder == beheld)
                    footer = "Your profile has been locked.";
                else if (beholder.IsStaff())
                    footer = "This profile has been locked.";
            }

            string body = e.TextOver != null ? e.TextOver : beheld.Profile;

            beholder.Send(new DisplayProfile(beholder != beheld || !beheld.ProfileLocked, beheld, header, body, footer));
            */
        }
    }
}
