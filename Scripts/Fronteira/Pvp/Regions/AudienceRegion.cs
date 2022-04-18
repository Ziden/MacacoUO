using System;
using Server;
using Server.Mobiles;
using Server.Targeting;
using Server.Items;
using Server.TournamentSystem;
using Server.Gumps;
using Server.Regions;

namespace Server.TournamentSystem
{
    public class AudienceRegion : Region
    {
        public PVPTournamentSystem System { get; set; }

        public AudienceRegion(PVPTournamentSystem system)
            : this(system, system.ArenaMap)
        {
        }

        public AudienceRegion(PVPTournamentSystem system, Map map)
            : base(system.Name + " Audience Region", map, Region.DefaultPriority, system.AudienceRegionBounds)
        {
            System = system;
            Register();

            Music = MusicName.Britain1;
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }

        public override bool AllowHarmful(Mobile from, IDamageable target)
        {
            if (System != null && System.AudienceRegionNoAttack)
            {
                return false;
            }

            return base.AllowHarmful(from, target);
        }

        public override bool OnDoubleClick(Mobile m, object o)
        {
            if (o is Item && m.AccessLevel <= AccessLevel.VIP)
            {
                Item item = (Item)o;

                if (item is Corpse && ((Corpse)item).Owner != m)
                    return false;
            }
            return base.OnDoubleClick(m, o);
        }

        public override bool OnBeginSpellCast(Mobile m, ISpell s)
        {
            if (m.AccessLevel <= AccessLevel.VIP && System != null && System.AudienceRegionNoSpells)
            {
                m.SendMessage("You are prohibited from casting spells in the arena audience area.");
                return false;
            }

            return base.OnBeginSpellCast(m, s);
        }

        public override void OnSpeech(SpeechEventArgs args)
        {
            Mobile mob = args.Mobile;
            string str = args.Speech;

            if (mob is PlayerMobile && str != null && str.ToLower().Trim() == "current tournament" && System != null && System.CurrentTournament != null)
            {
                if (mob.HasGump(typeof(TournamentStatsGump2)))
                    mob.CloseGump(typeof(TournamentStatsGump2));

                if (mob.HasGump(typeof(TournamentsGump)))
                    mob.CloseGump(typeof(TournamentsGump));

                if(mob is PlayerMobile)
                    BaseGump.SendGump(new TournamentStatsGump2(System.CurrentTournament, null, mob as PlayerMobile));
            }
        }
    }
}
