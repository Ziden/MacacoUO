using System;
using System.Linq;
using Server.Mobiles;
using Server.Gumps;
using Server.TournamentSystem;

namespace Server.Items
{
    [DeleteConfirm("Are you sure you want to delete this? Deleting this stone will remove any upcoming tournaments and any prize items and all of the arena.")]
    public abstract class TournamentStone : Item
    {
        public override bool ForceShowProperties { get { return true; } }

        //private bool _ShowRegionBounds;

        [CommandProperty(AccessLevel.GameMaster)]
        public PVPTournamentSystem System { get; set; }

        /*[CommandProperty(AccessLevel.GameMaster)]
        public bool ShowRegionBounds
        {
            get { return _ShowRegionBounds; }
            set
            {

                if(value)
                    System.ShowRegionBounds();

                _ShowRegionBounds = value; 
            }
        }*/

        public override string DefaultName
        {
            get 
            {
                if (System != null)
                    return String.Format("{0} Stone", System.Name);

                return "Arena Stone";
            }
        }

        public TournamentStone() : base (3804)
        {
            Movable = false;
        }

        public abstract void LoadSystem(GenericReader reader);

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(Location, 3))
                from.SendLocalizedMessage(500446); // That is too far away.
            else if (from is PlayerMobile)
            {
                from.CloseGump(typeof(BaseTournamentGump));
                BaseGump.SendGump(new TournamentStoneGump(System, from as PlayerMobile));
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (System != null && System.Active)
            {
                list.Add(1153035); // Active

                list.Add(1150733, String.Format("{0}\t{1}", "Status", System.CurrentFight == null ? "Disponivel" : "Em Uso")); // ~1_NAME~ : ~2_NUMBER~
            }
            else
            {
                list.Add(1153036); // Inactive
            }
        }

        public override void OnAfterDelete()
        {
            if (System == null)
            {
                base.OnAfterDelete();
            }
            else
            {
                System.RemoveSystem();
            }
        }

        public void InvalidateGumps()
        {
            if (Map == null || Map == Map.Internal || System == null)
            {
                return;
            }

            InvalidateProperties();

            foreach (var m in System.AudienceRegion.GetEnumeratedMobiles().OfType<PlayerMobile>())
            {
                var gump = m.FindGump<TournamentStoneGump>();

                if (gump != null)
                {
                    gump.Refresh();
                }
            }

            foreach (var m in System.FightRegion.GetEnumeratedMobiles().OfType<PlayerMobile>())
            {
                var gump = m.FindGump<TournamentStoneGump>();

                if (gump != null)
                {
                    gump.Refresh();
                }
            }
        }

        public override void OnMapChange()
        {
            base.OnMapChange();

            if (Server.TournamentSystem.Config.UseStoneDefaultHue)
            {
                ValidateHue();
            }
        }

        private void ValidateHue()
        {
            if (Map != null && Map.Rules == MapRules.FeluccaRules)
            {
                Hue = 2512;
            }
            else
            {
                Hue = 2540;
            }
        }

        public TournamentStone(Serial serial) : base (serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);

            //System.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0)
            {
                LoadSystem(reader);
            }
        }
    }
}
