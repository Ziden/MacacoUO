using System;
using Server.Items;
using Server.Regions;
using Server.Targeting;

namespace Server.Spells.Third
{
    public class TeleportSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Teleport", "Rel Por",
            215,
            9031,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);
        public TeleportSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Third;
            }
        }
        public override bool CheckCast()
        {
            if (Factions.Sigil.ExistsOn(this.Caster))
            {
                this.Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
                return false;
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(this.Caster))
            {
                this.Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
                return false;
            }

            return SpellHelper.CheckTravel(this.Caster, TravelCheckType.TeleportFrom);
        }

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public static void Teleporta(Mobile m, IPoint3D p)
        {
            IPoint3D orig = p;
            Map map = m.Map;

            SpellHelper.GetSurfaceTop(ref p);

            Point3D from = m.Location;
            Point3D to = new Point3D(p);

            var st = m.Map.GetStaticTiles(to);
            bool staticInvalido = false;
            foreach (var s in st)
            {
                if (s.IsWater())
                {
                    staticInvalido = true;
                }
                var td = TileData.ItemTable[s.ID & TileData.MaxItemValue];
                if (td.Impassable)
                    staticInvalido = true;
            }

            var land = m.Map.Tiles.GetLandTile(to.X, to.Y);
            if (land.IsWater() || land.IsCoastline())
                staticInvalido = true;
            else
            {
                var landdata = TileData.LandTable[land.ID & TileData.MaxItemValue];
                if ((landdata.Flags & TileFlag.Impassable) != 0)
                {
                    staticInvalido = true;
                }
            }
            if (staticInvalido)
            {
                m.SendMessage("Voce nao pode teleportar ali");
            }
            else if (Factions.Sigil.ExistsOn(m))
            {
                m.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(m))
            {
                m.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
            }
            else if (!SpellHelper.CheckTravel(m, TravelCheckType.TeleportFrom))
            {
            }
            else if (!SpellHelper.CheckTravel(m, map, to, TravelCheckType.TeleportTo))
            {
            }
            else if (map == null || !map.CanFit(p.X, p.Y, p.Z, 16, false, false, true))
                m.SendLocalizedMessage("Este local esta bloqueado"); // That location is blocked.

            else if (SpellHelper.CheckMulti(to, map))
            {
                m.SendLocalizedMessage(502831); // Cannot teleport to that spot.
            }
            else if (Region.Find(to, map).GetRegion(typeof(HouseRegion)) != null)
            {
                m.SendLocalizedMessage(502829); // Cannot teleport to that spot.
            }
            else
            {
                m.Location = to;
                m.ProcessDelta();

                if (m.Player)
                {
                    Effects.SendLocationParticles(EffectItem.Create(from, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
                    Effects.SendLocationParticles(EffectItem.Create(to, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);
                }
                else
                {
                    m.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
                }

                m.PlaySound(0x1FE);

                m.SetCooldown("tp", TimeSpan.FromSeconds(4));

                IPooledEnumerable eable = m.GetItemsInRange(0);

                foreach (Item item in eable)
                {
                    if (item is Server.Spells.Fifth.PoisonFieldSpell.PoisonFieldItem || item is Server.Spells.Fourth.FireFieldSpell.FireFieldItem)
                        item.OnMoveOver(m);
                }
                eable.Free();
            }
        }

        public void Target(IPoint3D p)
        {




            IPoint3D orig = p;
            Map map = this.Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            Point3D from = this.Caster.Location;
            Point3D to = new Point3D(p);

            if(!Caster.InLOS(to))
            {
                Caster.SendMessage("Voce nao pode ver isto");
                this.FinishSequence();
                return;
            }

            var st = this.Caster.Map.GetStaticTiles(to);
            bool staticInvalido = false;
            foreach (var s in st)
            {
                if (s.IsWater())
                {
                    staticInvalido = true;
                }
                var td = TileData.ItemTable[s.ID & TileData.MaxItemValue];
                if (td.Impassable)
                    staticInvalido = true;
            }

            var land = this.Caster.Map.Tiles.GetLandTile(to.X, to.Y);
            if (land.IsWater() || land.IsCoastline())
                staticInvalido = true;
            else
            {
                var landdata = TileData.LandTable[land.ID & TileData.MaxItemValue];
                if ((landdata.Flags & TileFlag.Impassable) != 0)
                {
                    staticInvalido = true;
                }
            }

            if (staticInvalido)
            {
                this.Caster.SendMessage("Voce nao pode teleportar ali");
            }
            else if (Factions.Sigil.ExistsOn(this.Caster))
            {
                this.Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(this.Caster))
            {
                this.Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
            }
            else if (!SpellHelper.CheckTravel(this.Caster, TravelCheckType.TeleportFrom))
            {
            }
            else if (!SpellHelper.CheckTravel(this.Caster, map, to, TravelCheckType.TeleportTo))
            {
            }
            else if (map == null || !map.CanFit(p.X, p.Y, p.Z, 16, false, false, true))
                Caster.SendLocalizedMessage("Este local esta bloqueado"); // That location is blocked.

            else if (SpellHelper.CheckMulti(to, map))
            {
                this.Caster.SendLocalizedMessage(502831); // Cannot teleport to that spot.
            }
            else if (Region.Find(to, map).GetRegion(typeof(HouseRegion)) != null)
            {
                this.Caster.SendLocalizedMessage(502829); // Cannot teleport to that spot.
            }
            else if (this.CheckSequence())
            {
                Mobile m = this.Caster;

                m.Location = to;
                m.ProcessDelta();

                if (m.Player)
                {
                    Effects.SendLocationParticles(EffectItem.Create(from, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
                    Effects.SendLocationParticles(EffectItem.Create(to, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);
                }
                else
                {
                    m.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
                }

                m.PlaySound(0x1FE);

                IPooledEnumerable eable = m.GetItemsInRange(0);

                foreach (Item item in eable)
                {
                    if (item is Server.Spells.Fifth.PoisonFieldSpell.PoisonFieldItem || item is Server.Spells.Fourth.FireFieldSpell.FireFieldItem)
                        item.OnMoveOver(m);
                }
                eable.Free();
            }
            this.FinishSequence();
        }

        public override bool PunishSpellMovementIfRepeated { get { return Shard.POL_STYLE; } }

        public override TimeSpan GetCastDelay()
        {
            if (Shard.POL_STYLE && Caster.Player)
                return TimeSpan.FromSeconds(2.5);
            else
                return base.GetCastDelay();
        }

        public class InternalTarget : Target
        {
            private readonly TeleportSpell m_Owner;
            public InternalTarget(TeleportSpell owner)
                : base(14, true, TargetFlags.None)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                IPoint3D p = o as IPoint3D;

                if (p != null)
                    this.m_Owner.Target(p);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}
