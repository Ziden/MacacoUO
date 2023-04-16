using System;
using System.Collections.Generic;
using System.Linq;
using Server.Commands;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Targeting;

namespace Server.Items
{
    public abstract class BasePlantable : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        private bool plantada { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int colhidas { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        private string NomePlantador { get; set; }

        public BasePlantable(int itemID)
            : base(itemID)
        {
            Stackable = true;
        }

        public void Planta(Mobile from, string nomeDono)
        {
            this.Movable = false;
            new GrowTimer(this, from, nomeDono).Start();
            Effects.PlaySound(this.Location, from.Map, 0x12E);
            this.plantada = true;
        }

        public virtual int GetMinSkill()
        {
            return 0;
        }

        public virtual int GetMaxSkill()
        {
            return 70;
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (plantada)
            {
                if (ItemID == ItemIDCrescendo)
                    list.Add("Crescendo");
                else
                    list.Add("Germinando");
            }
            list.Add("Herding: " + GetMinSkill());
        }

        public BasePlantable(Serial serial)
        : base(serial)
        {
        }

        public abstract Item GetToPlant();

        private class PlantTarget : Target
        {
            private BasePlantable toPlant;
            public PlantTarget(BasePlantable toPlant) : base(6, true, TargetFlags.None)
            {
                this.toPlant = toPlant;
            }

            public static List<int> allows = new List<int>(new int[] { 9, 2, 3, 4, 5, 6, 6013 });

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;

                if (p == null || from.Map == null)
                    return;

                if (p is IPoint3D)
                {
                    var target = (IPoint3D)p;

                    if(from.Region is GuardedRegion)
                    {
                        from.SendMessage("Voce nao pode plantar em cidades. Plante em gramados ou fazendas fora da cidade ou em casas");
                        return;
                    }

                    if(target is LandTarget)
                    {
                        var land = (LandTarget)target;
                        if (!allows.Contains(land.TileID))
                        {
                            if (land.Name == null || !land.Name.Contains("grass"))
                            {
                                from.SendMessage("Voce apenas pode plantar isto em fazendas ou gramados");
                                return;
                            }
                        }
                    } else if(target is StaticTarget)
                    {
                        var s = (StaticTarget)target;
                        if (!allows.Contains(s.ItemID))
                        {
                            if (s.Name == null || !s.Name.Contains("grass"))
                            {
                                from.SendMessage("Voce apenas pode plantar isto em fazendas ou gramados");
                                return;
                            }
                        }
                    } else
                    {
                        from.SendMessage("Voce apenas pode plantar isto em fazendas ou gramados");
                        return;
                    }
                  

                    var items = from.Map.GetItemsInRange(target.ToPoint3D(), 0);
                    bool r = false;
                    foreach (var item in items)
                    {
                        if (item is BasePlantable)
                        {
                            from.SendMessage("Ja existe uma planta aqui");
                            r = true;
                        }
                    }
                    items.Free();
                    if (r)
                        return;

                    if(from.Skills[SkillName.Herding].Value < toPlant.GetMinSkill())
                    {
                        from.SendMessage("Voce precisa de mais herding para plantar isto");
                        return;
                    }

                    if (from.CheckSkillMult(SkillName.Herding, toPlant.GetMinSkill(), toPlant.GetMaxSkill() + 15))
                    {


                        from.Emote("* plantando *");
                        from.Animate(AnimationType.Attack, 3);

                        if(!from.IsCooldown("dp"))
                        {
                            from.SetCooldown("dp", TimeSpan.FromSeconds(60));
                            from.SendMessage(78, "Colha plantas pela floresta para conseguir mais recursos !!!");
                        }
                        var tipo = toPlant.GetType();
                       
                        var semente = (BasePlantable)Activator.CreateInstance(tipo);
                        semente.Hue = toPlant.Hue;
                        semente.Name = toPlant.Name;

                        toPlant.Consume(1);
                        semente.Amount = 1;
                        semente.colhidas = toPlant.colhidas;
                        var loc = target.ToPoint3D();
                        loc.Z = from.Map.GetAverageZ(loc.X, loc.Y) + 1;
                        semente.DropToWorld(from, loc);
                        semente.NomePlantador = from.Name;
                        semente.Planta(from, from.Name);
                        semente.InvalidateProperties();
                        if(from is PlayerMobile)
                        {
                            var fada = ((PlayerMobile)from).Wisp;
                            if(fada != null)
                            {
                                fada.Planta();
                            }
                        }
                    }
                    else
                    {
                        from.SendMessage("Voce nao conseguiu colocar a planta corretamente, talvez com mais Herding voce consiga fazer isto");
                    }
                }
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.Target = new PlantTarget(this);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            try
            {
                writer.WriteEncodedInt(2);
                writer.Write(plantada);
                writer.Write(NomePlantador);
                writer.Write(colhidas);
            }
            catch (Exception e) { }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            try
            {
                Stackable = true;
                var version = reader.ReadEncodedInt();
                plantada = reader.ReadBool();

                if (version >= 1)
                    NomePlantador = reader.ReadString();

                if (version >= 2)
                    colhidas = reader.ReadInt();

                if (plantada)
                {
                    var planta = (BaseFarmable)GetToPlant();
                    planta.plantouQuando = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    planta.MoveToWorld(this.Location, this.Map);
                    planta.nomeQuemPlantou = this.NomePlantador;
                    this.Consume();
                }

            }
            catch (Exception e) { }
        }

        public virtual int ItemIDCrescendo { get { return 3254; } }

        public class GrowTimer : Timer
        {
            private BasePlantable plantable;
            private Mobile plantador;
            private string nomeDono;

            private int count;

            public GrowTimer(BasePlantable plantable, Mobile plantador, string nomeDono) : base(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2), 2)
            {
                this.nomeDono = nomeDono;
                this.plantable = plantable;
                this.plantador = plantador;
            }

            // Crescimento da PRANTA
            protected override void OnTick()
            {
                count++;
                if (count == 1)
                {
                    plantable.ItemID = plantable.ItemIDCrescendo; // matinho q ta crescendo

                    if (plantador != null)
                    {
                        plantador.SendMessage("Sua " + plantable.Name + " esta crescendo");
                    }

                }
                if (count == 2)
                {
                    // crescendo ela toda
                    var planta = (BaseFarmable)plantable.GetToPlant();
                    if (plantador != null)
                    {
                        plantador.SendMessage("Sua " + plantable.Name + " esta pronta para colheita !");
                    }
                    planta.nomeQuemPlantou = nomeDono;
                    planta.plantouQuando = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    planta.MoveToWorld(plantable.Location, plantable.Map);
                    planta.Colhidas = plantable.colhidas;
                    plantable.Consume();

                }
                plantable.InvalidateProperties();
            }
        }
    }
}