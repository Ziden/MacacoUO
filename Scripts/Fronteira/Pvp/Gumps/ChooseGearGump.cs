using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using Server;
using Server.Mobiles;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class ChooseOwnGearGump : BaseTournamentGump
    {
        public Action<Mobile, ArenaFight> Challenge { get; set; }

        public ArenaFight Fight { get; set; }

        public ChooseOwnGearGump(PlayerMobile pm, ArenaFight fight = null, Action<Mobile, ArenaFight> challenge = null)
            : base(pm, 200, 200)
        {
            Fight = fight;
            Challenge = challenge;
        }

        public override void AddGumpLayout()
        {
            AddBackground(0, 0, 150, 200, DarkBackground);
            AddBackground(10, 10, 130, 180, LightBackground);

            var pref = ForcedGear.GetPreference(User);
            var entry = ForcedGear.GetEntry(pref);

            //AddHtml(0, 15, 150, 20, Center(String.Format("Gear: {0}", pref.ToString())), false, false);
            AddHtml(0, 15, 150, 20, Center(String.Format("Equipamento: {0}", pref.ToString())), false, false);

            if (entry != null)
            {
                Rectangle2D b = ItemBounds.Table[entry.ItemID];
                AddItem(75 - b.Width / 2 - b.X, 90 - b.Height / 2 - b.Y, entry.ItemID);

                var display = ForcedGear.GetDisplay(pref);

                if (display != null)
                {
                    AddItemProperty(display);
                }
            }

            AddButton(76, 168, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddButton(46, 168, 4014, 4016, 2, GumpButtonType.Reply, 0);

            if (Challenge != null)
            {
                AddButton(61, 145, 4023, 4025, 3, GumpButtonType.Reply, 0);
                AddTooltip(Localizations.GetLocalization(84));
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 1:
                    var pref = ForcedGear.GetPreference(User);

                    if (pref == ForcedGearType.Dexxer)
                    {
                        pref = ForcedGearType.Caster;
                    }
                    else
                    {
                        pref++;
                    }

                    ForcedGear.SetPreference(User, pref);
                    //User.SendMessage("Preference set.");
                    User.SendMessage("Preferência setada.");

                    Refresh();
                    break;
                case 2:
                    var preference = ForcedGear.GetPreference(User);

                    if (preference == ForcedGearType.Caster)
                    {
                        preference = ForcedGearType.Dexxer;
                    }
                    else
                    {
                        preference--;
                    }

                    ForcedGear.SetPreference(User, preference);
                    //User.SendMessage("Preference set.");
                    User.SendMessage("Preferência setada.");
                    break;
                case 3:
                    if (Challenge != null)
                    {
                        Challenge(User, Fight);
                    }
                    break;
            }
        }
    }
}
