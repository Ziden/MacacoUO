using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public abstract class BaseFarmable : Item
    {
        private bool m_Picked = false;

        [CommandProperty(AccessLevel.GameMaster)]
        public int Colhidas { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string nomeQuemPlantou { get; set; }

        public long plantouQuando;

        public BaseFarmable(int itemID)
            : base(itemID)
        {
            this.Movable = false;
        }

        public virtual int GetMinSkill()
        {
            return 50;
        }

        public virtual int GetMaxSkill()
        {
            return 80;
        }

        public virtual BasePlantable GetSeed()
        {
            return null;
        }

        public BaseFarmable(Serial serial)
            : base(serial)
        {
        }

        public abstract Item GetCropObject();

        public abstract int GetPickedID();

        public override void OnDoubleClick(Mobile from)
        {
            Map map = this.Map;
            Point3D loc = this.Location;

            if (this.Parent != null || this.Movable || this.IsLockedDown || this.IsSecure || map == null || map == Map.Internal)
                return;

            if (SendoColhida)
            {
                from.SendMessage("Esta planta ja esta sendo colida");
                return;
            }

            if (!from.InRange(loc, 2) || !from.InLOS(this))
                from.PrivateOverheadMessage("Estou muito longe"); // I can't reach that.
            else if (!this.m_Picked)
                this.OnPicked(from, loc, map);
        }

        public virtual void OnPicked(Mobile from, Point3D loc, Map map)
        {
            if (this.nomeQuemPlantou != null && from.Name != this.nomeQuemPlantou)
            {
                var agora = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                var diferenca = (agora - plantouQuando) / 1000;

                if (diferenca < 60 * 60) // nao passou X segundos
                {
                    from.CloseAllGumps();
                    from.SendGump(new ConfirmaRoubo((int)diferenca, from, loc, map, this));
                    return;
                }
            }
            // Colhe(from, loc, map);
            SendoColhida = true;
            new ColheitaTimer(this, from, loc, map).Start();
        }

        public bool SendoColhida = false;

        public class ConfirmaRoubo : BaseConfirmGump
        {
            public int segundos;
            public Mobile from;
            public Point3D loc;
            public Map map;
            public BaseFarmable farm;

            public override string LabelString { get { return "Rouba-la o fara criminoso, ok ?"; } }
            public override string TitleString
            {
                get
                {
                    return "Esta planta ainda e protegida";
                }
            }

            public ConfirmaRoubo(int segundos, Mobile from, Point3D loc, Map map, BaseFarmable farm)
            {
                this.map = map;
                this.loc = loc;
                this.from = from;
                this.segundos = segundos;
                this.farm = farm;
                this.Closable = true;
                this.Disposable = true;
                this.Dragable = true;
                this.Resizable = false;
            }

            public override void Confirm(Mobile from)
            {
                from.CriminalAction(false);
                // farm.Colhe(from, loc, map);
                new ColheitaTimer(farm, from, loc, map).Start();
            }
        }


        public class ColheitaTimer : Timer
        {
            int ct = 0;
            private BaseFarmable farm;
            private Mobile from;
            private Point3D loc;
            private Map map;
            private bool roubo = false;
            private bool cancel = false;

            public ColheitaTimer(BaseFarmable farm, Mobile from, Point3D loc, Map map, bool roubo = false) : base(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1.3), 3)
            {
                this.farm = farm;
                this.from = from;
                this.loc = loc;
                this.map = map;
                this.roubo = roubo;
            }

            protected override void OnTick()
            {

                if (cancel)
                {
                    return;
                }

                farm.SendoColhida = true;
                from.Direction = from.GetDirectionTo(loc);

                if (ct == 2)
                {
                    farm.Colhe(from, loc, map);
                }
                else
                {
                    if (!from.InRange(loc, 2))
                    {
                        from.SendMessage("Voce esta muito longe para colher");
                        cancel = true;
                        farm.SendoColhida = false;
                        return;
                    }
                    if (ct == 0)
                    {
                        if (!roubo)
                            from.Emote("* colhendo a planta *");
                        else
                            from.Emote("* roubando a planta *");

                    }

                    if (ct != 2)
                    {
                        ((PlayerMobile)from).HarvestAnimation();
                    }
                    Timer.DelayCall(TimeSpan.FromMilliseconds(150), b => Effects.PlaySound(loc, map, 0x12E), null);
                    ct++;
                }
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Herding: " + GetMinSkill());
        }

        public void Colhe(Mobile from, Point3D loc, Map map)
        {
            if (map == Map.Internal)
                return;

            this.ItemID = 0x1014;
            Item spawn = this.GetCropObject();

            var Skill = (int)from.Skills[SkillName.Herding].Value;
            var amt = 1 + Skill / 20;
            if (nomeQuemPlantou == null)
            {
                amt += 1;
                amt *= 2;
            }
            if(spawn is BaseReagent)
            {
                amt += 1;
                amt *= 2;
            }

            spawn.Amount = amt;
            spawn.Amount += (int)Math.Round(amt * from.GetBonusElemento(ElementoPvM.Gelo));

            var check = from.CheckSkillMult(SkillName.Herding, GetMinSkill(), GetMaxSkill());

            if (!check)
            {
                from.Emote("* tentou colher e estragou tudo *");
                Effects.PlaySound(this.Location, this.Map, 0x12E);
                this.m_Picked = true;
                this.Unlink();
                spawn.Amount = 1;
                spawn.MoveToWorld(loc, map);
                Timer.DelayCall(TimeSpan.FromMinutes(1.0), new TimerCallback(Delete));
                return;
            }

            from.Emote("* colheu a planta *");
            ((PlayerMobile)from).HarvestAnimation(0);
            if (Skill < 10 || !check)
            {
                from.SendMessage("Por ser inexperiente voce retirou a planta de forma errada");
                spawn.MoveToWorld(loc, map);
            }
            else
            {
                var seed = GetSeed();
                if (nomeQuemPlantou == null && seed != null && Utility.Random(5) == 1)
                {
                    from.SendMessage("Voce conseguiu obter uma semente da planta selvagem");
                    from.AddToBackpack(seed);
                }
                else
                {
                    if (nomeQuemPlantou != null)
                    {
                        if (seed != null)
                            seed.Consume();
                    }
                    else
                    {
                        if (seed != null)
                            seed.Consume();
                    }
                }

                from.AddToBackpack(spawn);
            }

            SendoColhida = false;
            Effects.PlaySound(this.Location, this.Map, 0x12E);
            this.m_Picked = true;
            this.Unlink();
            Timer.DelayCall(TimeSpan.FromMinutes(1.0), new TimerCallback(Delete));
        }

        public void Unlink()
        {
            ISpawner se = this.Spawner;

            if (se != null)
            {
                this.Spawner.Remove(this);
                this.Spawner = null;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(4); // version
            writer.Write(Colhidas);
            writer.Write(this.m_Picked);
            writer.Write(nomeQuemPlantou);
            writer.Write(plantouQuando);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadEncodedInt();
            if (version >= 4)
            {
                Colhidas = reader.ReadInt();
                this.m_Picked = reader.ReadBool();
                this.nomeQuemPlantou = reader.ReadString();
                this.plantouQuando = reader.ReadLong();
            }
            else
            {
                switch (version)
                {
                    case 0:
                    case 1:
                    case 2:
                        return;
                    case 3:
                    case 4:
                        this.m_Picked = reader.ReadBool();
                        this.nomeQuemPlantou = reader.ReadString();
                        this.plantouQuando = reader.ReadLong();
                        break;
                }
            }

            if (this.m_Picked)
            {
                this.Unlink();
                this.Delete();
            }
        }
    }
}
