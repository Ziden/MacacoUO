using System;
using Server;
using Server.Items;
using Server.Engines.CannedEvil;
using System.Collections.Generic;
using System.Linq;
using Server.Ziden;
using Server.Fronteira.Elementos;

namespace Server.Mobiles
{
    [CorpseName("a dragon turtle corpse")]
    public class DragonTurtle : BaseChampion
    {
        public override bool IsBoss => true;
        public override bool ReduceSpeedWithDamage => false;
        public override bool IsSmart => true;
        public override bool UseSmartAI => true;

        public override Type[] UniqueList { get { return new Type[] { }; } }
        public override Type[] SharedList { get { return new Type[] { }; } }
        public override Type[] DecorativeList { get { return new Type[] { }; } }
        public override MonsterStatuetteType[] StatueTypes { get { return new MonsterStatuetteType[] { }; } }

        public override ChampionSkullType SkullType { get { return ChampionSkullType.None; } }

        [Constructable]
        public DragonTurtle() : base(AIType.AI_Mage)
        {
            Name = "tartaruga dragao";
            Body = 1288;
            BaseSoundID = 362;

            SetStr(750, 800);
            SetDex(185, 240);
            SetInt(99999);

            SetDamage(35, 47);

            SetHits(75000);
            SetStam(99999);
            SetMana(99999);

            SetResistance(ResistanceType.Physical, 75, 85);
            SetResistance(ResistanceType.Fire, 65, 75);
            SetResistance(ResistanceType.Cold, 70, 75);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 65, 75);

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Poison, 50);

            SetSkill(SkillName.EvalInt, 200, 200);
            SetSkill(SkillName.Magery, 200, 200);
            SetSkill(SkillName.MagicResist, 90, 120);
            SetSkill(SkillName.Tactics, 200, 110);
            SetSkill(SkillName.Wrestling, 225, 227);

            VirtualArmor = 100;

            Fame = 11000;
            Karma = -11000;

            SetWeaponAbility(WeaponAbility.Dismount);
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.LV7, 3);
            AddLoot(LootPack.Gems, 50);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            DistribuiItem(new PersonalTelescope());
            SorteiaItem(new LegacyGuildstone());
            DistribuiItem(new PergaminhoPeso());
            DistribuiItem(new PergaminhoCarregamento());
            SorteiaItem(new DragonTurtleFountainAddonDeed());
            DistribuiItem(Decos.RandomDecoRara(this));
            //DistribuiItem(Decos.RandomDecoRara(this));
            DistribuiItem(Decos.RandomDeco(this));
            DistribuiItem(new Gold(20000));
            DistribuiItem(new CristalTherathan(10));
            DistribuiItem(new FragmentosAntigos());
            for (var i = 0; i < 2; i++)
            {
                if (Utility.RandomDouble() < 0.3)
                    DistribuiPs(115);
                else
                    DistribuiPs(110);
            }

            for (var x = 0; x < 5; x++)
            {
                SorteiaItem(BaseEssencia.RandomEssencia(10));
                SorteiaItem(ElementoUtils.GetRandomPedraSuperior(10));
            }

            if(Utility.RandomDouble () > 0.2)
            {
                var a = new CarpenterApron();
                a.Bonus = Utility.Random(5, 25);
                a.Skill = SkillName.Tailoring;
                a.Name = "Avental do Artesao da Tartaruga Dragao";
                SorteiaItem(a);
            } else
            {
                var a = new CarpenterApron();
                a.Bonus = Utility.Random(5, 25);
                a.Skill = SkillName.Blacksmith;
                a.Name = "Avental do Ferreiro da Tartaruga Dragao";
                SorteiaItem(a);
            }
        }

        public virtual int BonusExp => 1900;

        public override int Meat { get { return 1; } }
        public override int Hides { get { return 33; } }
        public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies; } }
        public override bool HasBreath { get { return true; } }
        public override bool TeleportsTo { get { return true; } }
        public override TimeSpan TeleportDuration { get { return TimeSpan.FromSeconds(30); } }
        public override int TeleportRange { get { return 10; } }
        public override bool ReacquireOnMovement { get { return true; } }

        public override void OnCarve(Mobile from, Corpse corpse, Item with)
        {
            if (corpse != null && !corpse.Carved)
            {
                from.SendLocalizedMessage(1156198); // You cut away some scoots, but they remain on the corpse.
                corpse.DropItem(new DragonTurtleScute(18));
            }

            base.OnCarve(from, corpse, with);
        }

        public override Item GetArtifact()
        {
            return new DragonTurtleEgg();
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            base.OnMovement(m, oldLocation);

            if (!_DoingBubbles || _BubbleLocs == null)
                return;

            List<Tuple<Point3D, int>> copy = new List<Tuple<Point3D, int>>(_BubbleLocs.Where(tup => tup.Item1 == m.Location));

            foreach (Tuple<Point3D, int> t in copy)
            {
                Point3D p = m.Location;
                int hue = 0;

                for (int i = 0; i < _BubbleLocs.Count; i++)
                {
                    if (_BubbleLocs[i].Item1 == p)
                    {
                        hue = _BubbleLocs[i].Item2;
                        _BubbleLocs[i] = new Tuple<Point3D, int>(Point3D.Zero, hue);
                    }

                    Effects.SendTargetEffect(m, 13920, 10, 60, hue == 0 ? 0 : hue - 1, 5);
                    ApplyMod(m, hue);
                }
            }

            ColUtility.Free(copy);
        }

        private long _NextBubbleWander;
        private long _NextBubbleAttack;
        private bool _DoingBubbles;
        public List<Tuple<Point3D, int>> _BubbleLocs { get; set; }
        public Dictionary<Mobile, int> _Affected { get; set; }

        private readonly Direction[] _Directions = { Direction.North, Direction.Right, Direction.East, Direction.Down, Direction.South, Direction.Left, Direction.West, Direction.Up };
        private readonly int[] _Hues = { 0, 33, 44, 9, 63, 53, 117 };

        public override void OnThink()
        {
            base.OnThink();

            if (Combatant != null && Core.TickCount - _NextBubbleWander >= 0)
            {
                DoBubble();
                _NextBubbleWander = Core.TickCount + Utility.RandomMinMax(25000, 35000);
            }

            if (Combatant != null && Core.TickCount - _NextBubbleAttack >= 0)
            {
                DoBubbleAttack();
                _NextBubbleAttack = Core.TickCount + Utility.RandomMinMax(40000, 60000);
            }
        }

        public void DoBubble()
        {
            if (!Alive || Map == Map.Internal || Map == null)
                return;

            int pathLength = Utility.RandomMinMax(5, 11);
            _DoingBubbles = true;
            _BubbleLocs = new List<Tuple<Point3D, int>>();

            for (int i = 0; i < 8; i++)
            {
                _BubbleLocs.Add(new Tuple<Point3D, int>(Location, _Hues[Utility.Random(_Hues.Length)]));
            }

            for (int i = 0; i < pathLength; i++)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(i + 1.0), () =>
                {
                    for (int j = 0; j < _BubbleLocs.Count; j++)
                    {
                        Map map = Map;

                        if (!Alive || map == null || (i > 0 && _BubbleLocs[j].Item1 == Point3D.Zero))
                            continue;

                        Direction d = _Directions[j];

                        int hue = _BubbleLocs[j].Item2;
                        int x = _BubbleLocs[j].Item1.X;
                        int y = _BubbleLocs[j].Item1.Y;

                        if (i > 2 && 0.4 > Utility.RandomDouble())
                        {
                            if (d == Direction.Up)
                                d = Direction.North;
                            else
                                d += 1;

                            Movement.Movement.Offset(d, ref x, ref y);
                        }
                        else
                            Movement.Movement.Offset(d, ref x, ref y);

                        IPoint3D p = new Point3D(x, y, Map.GetAverageZ(x, y));
                        Spells.SpellHelper.GetSurfaceTop(ref p);

                        var newLoc = new Point3D(p);

                        bool hasMobile = false;
                        IPooledEnumerable eable = Map.GetMobilesInRange(newLoc, 0);

                        foreach (Mobile m in eable)
                        {
                            if (m != this && CanBeHarmful(m) && (m is PlayerMobile || (m is BaseCreature && ((BaseCreature)m).GetMaster() is PlayerMobile)))
                            {
                                hasMobile = true;

                                Effects.SendTargetEffect(m, 13920, 10, 60, hue == 0 ? 0 : hue - 1, 5);
                                _BubbleLocs[j] = new Tuple<Point3D, int>(Point3D.Zero, hue);

                                ApplyMod(m, hue);
                            }
                        }

                        if (!hasMobile)
                        {
                            Effects.SendLocationEffect(newLoc, Map, 13920, 20, 10, hue == 0 ? 0 : hue - 1, 5);
                            _BubbleLocs[j] = new Tuple<Point3D, int>(newLoc, hue);
                        }
                    }

                    if (i == pathLength - 1)
                    {
                        Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
                        {
                            _DoingBubbles = false;
                            _BubbleLocs.Clear();
                            _BubbleLocs = null;
                        });
                    }
                });
            }
        }

        public void DoBubbleAttack()
        {
            if (!Alive || Map == Map.Internal || Map == null)
                return;

            List<Mobile> toget = new List<Mobile>();

            IPooledEnumerable eable = Map.GetMobilesInRange(Location, 11);

            foreach (Mobile m in eable)
            {
                if (m != this && CanBeHarmful(m) && InLOS(m) && (m is PlayerMobile || (m is BaseCreature && ((BaseCreature)m).GetMaster() is PlayerMobile)))
                    toget.Add(m);
            }

            eable.Free();

            toget.ForEach(mob =>
            {
                int hue;

                if (_Affected != null && _Affected.ContainsKey(mob))
                    hue = _Affected[mob];
                else
                    hue = _Hues[Utility.Random(_Hues.Length)];

                MovingParticles(mob, 13920, 10, 0, false, true, hue == 0 ? 0 : hue - 1, 5, 9502, 14120, 0, 0);

                Timer.DelayCall(TimeSpan.FromSeconds(.7), DoAttack_Callback, new object[] { mob, hue });
            });
        }

        private void ApplyMod(Mobile m, int hue)
        {
            ResistanceType type = GetResistanceFromHue(hue);

            if (_Affected == null)
                _Affected = new Dictionary<Mobile, int>();

            if (_Affected.ContainsKey(m))
                return;

            _Affected[m] = hue;

            //ResistanceMod mod = new ResistanceMod(type, -25);
            //m.AddResistanceMod(mod);
            m.VirtualArmorMod -= 30;
            m.SendMessage("Voce sente suas defesas se desfazendo");
            var mod = new TimedSkillMod(SkillName.MagicResist, false, -100, DateTime.UtcNow + TimeSpan.FromSeconds(30));
            m.AddSkillMod(mod);
            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.DragonTurtleDebuff, 1156192, 1156192));

            Server.Timer.DelayCall(TimeSpan.FromSeconds(30), RemoveMod_Callback, new object[] { m });
        }

        private void RemoveMod_Callback(object obj)
        {
            object[] o = obj as object[];
            Mobile m = o[0] as Mobile;
            m.VirtualArmorMod += 30;

            m.SendMessage("Suas resistencias voltaram ao normal");

            BuffInfo.RemoveBuff(m, BuffIcon.DragonTurtleDebuff);

            if (_Affected != null && _Affected.ContainsKey(m))
                _Affected.Remove(m);
        }

        private void DoAttack_Callback(object obj)
        {
            if (!this.Alive)
                return;

            object[] o = obj as object[];
            Mobile mob = o[0] as Mobile;
            int hue = (int)o[1];

            ResistanceType type = GetResistanceFromHue(hue);
            int damage = Utility.RandomMinMax(50, 70);
            damage += (int)(35 - mob.Skills.MagicResist.Value / 3);

            if (!Core.AOS)
            {
                AOS.Damage(mob, this, damage, 100, 0, 0, 0, 0);
                return;
            }

            switch (type)
            {
                case ResistanceType.Physical: AOS.Damage(mob, this, damage, 100, 0, 0, 0, 0); break;
                case ResistanceType.Fire: AOS.Damage(mob, this, damage, 0, 100, 0, 0, 0); break;
                case ResistanceType.Cold: AOS.Damage(mob, this, damage, 0, 0, 100, 0, 0); break;
                case ResistanceType.Poison: AOS.Damage(mob, this, damage, 0, 0, 0, 100, 0); break;
                case ResistanceType.Energy: AOS.Damage(mob, this, damage, 0, 0, 0, 0, 100); break;
            }
        }

        private ResistanceType GetResistanceFromHue(int hue)
        {
            switch (hue)
            {
                case 0: return ResistanceType.Physical;
                case 33:
                case 44: return ResistanceType.Fire;
                case 9: return ResistanceType.Cold;
                case 63: return ResistanceType.Poison;
                case 53:
                case 126: return ResistanceType.Energy;
            }

            return ResistanceType.Physical;
        }

        public DragonTurtle(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
