using Server.Engines.Points;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Services.Harvest;
using System;
using System.Collections.Generic;

namespace Server.Fronteira.Recursos
{
    // Nodulos de recurso pelo mapa
    public class Recurso : Item
    {
        private static Dictionary<Sector, List<Recurso>> _recursos = new Dictionary<Sector, List<Recurso>>();

        private Item _folha = null;
        private CraftResource _resource;
        private ItemQuality _quality;

        private int coleta = 0;

        public Mobile Coletando = null;

        public static List<Recurso> RecursosNoSector(Map m, Sector sec)
        {
            if (!_recursos.ContainsKey(sec))
            {
                _recursos[sec] = new List<Recurso>();
                if (Shard.DebugEnabled)
                    Shard.Debug("Criando lista de recurso no setor " + sec.X + " " + sec.Y);
            }
            return _recursos[sec];
        }

        public static void Registra(Recurso r)
        {
            if (r.Map == null || r.Deleted)
                return;

            var s = r.Map.GetSector(r);
            RecursosNoSector(r.Map, s).Add(r);
            if (Shard.DebugEnabled)
                Shard.Debug("Recurso registrado no setor " + s.X + " " + s.Y + " " + r.Map.Name);
        }

        [Constructable]
        public Recurso()
        {
            Constroi(Utility.RandomBool() ? CraftResourceType.Metal : CraftResourceType.Wood);
        }

        public void Valida()
        {
            if (this.Deleted)
                return;

            var casa = BaseHouse.FindHouseAt(this);
            if (casa != null)
                this.Delete();
        }

        [Constructable]
        public Recurso(CraftResourceType type)
        {
            Constroi(type);
        }

        public Item Folha { get { return _folha; } }

        public Recurso(Serial s) : base(s)
        {

        }


        public void Constroi(CraftResourceType type)
        {
            Quality = QualidadeRandom();
            if (type == CraftResourceType.Metal)
                Resource = MinerioRandom();
            else
                Resource = MadeiraRandom();
            Movable = false;

            Timer.DelayCall(TimeSpan.FromSeconds(2), () =>
            {
                Registra(this);
            });

            Timer.DelayCall(TimeSpan.FromMilliseconds(500), this.Valida);

            Timer.DelayCall(TimeSpan.FromHours(Utility.Random(3, 3)), () =>
            {
                if (!this.Deleted && this.Coletando == null)
                {
                    if (this.Spawner is XmlSpawner) {
                        var spawner = this.Spawner as XmlSpawner;
                        if (Shard.DebugEnabled)
                            Shard.Debug($"{spawner.TotalSpawnedObjects} / {spawner.TotalSpawnObjectCount} spawns");
                        if(spawner.TotalSpawnedObjects <= spawner.TotalSpawnObjectCount / 3)
                        {
                            spawner.Respawn();
                        }
                    }
                    this.Delete();
                 
                }
            });
        }

        public static ItemQuality QualidadeRandom()
        {
            var rnd = Utility.Random(100);
            if (rnd < 10)
                return ItemQuality.Exceptional;
            else if (rnd < 50)
                return ItemQuality.Normal;
            else
                return ItemQuality.Low;
        }

        public static CraftResource MadeiraRandom()
        {
            var rnd = Utility.Random(100);
            if (rnd < 2)
                return CraftResource.Gelo;
            else if (rnd < 5)
                return CraftResource.Carmesim;
            else if (rnd < 20)
                return CraftResource.Mogno;
            else if (rnd < 40)
                return CraftResource.Eucalipto;
            else if (rnd < 65)
                return CraftResource.Pinho;
            else
                return CraftResource.Carvalho;
        }

        public static CraftResource MinerioRandom()
        {
            var rnd = Utility.Random(1000);
            if (rnd < 5)
                return CraftResource.Adamantium;
            else if (rnd < 10)
                return CraftResource.Vibranium;
            else if (rnd < 60)
                return CraftResource.Quartzo;
            else if (rnd < 110)
                return CraftResource.Berilo;
            else if (rnd < 300)
                return CraftResource.Lazurita;
            else if (rnd < 400)
                return CraftResource.Dourado;
            else if (rnd < 600)
                return CraftResource.Niobio;
            else if (rnd < 800)
                return CraftResource.Bronze;
            return CraftResource.Cobre;
        }

        public void StartHarvest(Mobile from, Item tool)
        {

        }

        public SkillName GetSkill()
        {
            if (Metal())
                return SkillName.Mining;
            else
                return SkillName.Lumberjacking;
        }

        public override void OnDelete()
        {
            if (_folha != null)
            {
                _folha.Delete();
                _folha = null;
            }

            RecursosNoSector(this.Map, Map.GetSector(this)).Remove(this);
            base.OnDelete();
        }

        public bool Coleta(Mobile from)
        {
            from.RevealingAction();
            var diff = Dificuldade.GetDificuldade(_resource);
            var skill = GetSkill();
            if (diff != null)
            {

                if (from.Skills[skill].Value < diff.Required)
                {
                    from.SendMessage("Voce precisaria de " + diff.Required + " " + skill.GetName() + " para coletar isto");
                    return false;
                }
                if (!from.CheckSkillMult(skill, diff.Min, diff.Min + 10, 0))
                {
                    from.SendMessage("Voce nao conseguiu extrair o recurso");
                    if (Utility.RandomDouble() < 0.25)
                    {
                        this.Consume();
                    }
                    return false;
                }
            }
            ushort exp = 1;

            coleta++;
            if (coleta == 3)
            {
                exp += 99;
                var dif = Math.Abs(from.Skills[skill].Value - diff.Required);
                if (dif <= 25)
                {
                    if (from.Skills[skill].Value < 65)
                        exp += 6000;
                    else if (from.Skills[skill].Value < 75)
                        exp += 1000;
                    else if (from.Skills[skill].Value < 85)
                        exp += 1000;
                    else if (from.Skills[skill].Value < 95)
                        exp += 500;
                    else if (from.Skills[skill].Value < 110)
                        exp += 200;
                    else
                        exp += 100;
                } 
            }

            var ups = (int)Math.Floor(exp / 1000d);
            ushort resto = (ushort)(exp % 1000);

            if (ups > 0)
            {
                SkillCheck.Gain(from, from.Skills[skill], ups);
            }

            if (resto > 0)
            {
                if (from.Skills[skill].IncreaseExp(resto))
                {
                    SkillCheck.Gain(from, from.Skills[skill]);
                }
            }

            Item i = GetItem();
            if (i == null)
            {
                from.SendMessage(38, "Erro, favor reportar a staff...");
                return false;
            }
            if (from.Player && from.RP)
            {
                if (((PlayerMobile)from).Talentos.Tem(Talentos.Talento.Naturalista))
                {
                    i.Amount += 3;
                }
            }

            if (from.Player)
            {
                if (i is CopperOre || i is BronzeOre)
                    i.Amount *= 2;

                var bonus = (int)Math.Round(i.Amount * from.GetBonusElemento(ElementoPvM.Gelo));

                if(from.RP && from.TemTalento(Talentos.Talento.ColetorAvancado))
                {
                    bonus += 1;
                }

                if (bonus > 0)
                    i.Amount += bonus;
            }

            Give(from, i, true);

            from.SendMessage("Voce coletou o recurso");

            if (skill == SkillName.Mining)
                PointsSystem.PontosMinerador.AwardPoints(from, diff.Max, false, false);
            else if (skill == SkillName.Lumberjacking)
                PointsSystem.PontosLenhador.AwardPoints(from, diff.Max, false, false);

            var tool = from.Weapon as BaseWeapon;
            if (tool is IUsesRemaining && (tool is BaseAxe || tool is Pickaxe || tool is HammerPick || tool is SturdyPickaxe || tool is GargoylesPickaxe || Siege.SiegeShard))
            {
                IUsesRemaining toolWithUses = (IUsesRemaining)tool;

                toolWithUses.ShowUsesRemaining = true;

                if (toolWithUses.UsesRemaining > 0)
                    --toolWithUses.UsesRemaining;

                if (toolWithUses.UsesRemaining < 1)
                {

                    tool.Delete();
                    from.SendMessage("Sua ferramenta quebrou");
                }
            }

            if (coleta == 3)
            {
                Delete();
                if (_folha != null)
                {
                    _folha.Delete();
                }
            }
            else
            {
                new ColetaTimer(this, from).Start();
            }
            return true;
        }

        public virtual bool Give(Mobile m, Item item, bool placeAtFeet)
        {
            if (m._PlaceInBackpack(item))
                return true;

            if (!placeAtFeet)
                return false;

            Shard.Debug("Dropando item por causa de peso ", m);

            Map map = m.Map;

            if (map == null || map == Map.Internal)
                return false;

            List<Item> atFeet = new List<Item>();

            IPooledEnumerable eable = m.GetItemsInRange(0);

            foreach (Item obj in eable)
                atFeet.Add(obj);

            eable.Free();

            for (int i = 0; i < atFeet.Count; ++i)
            {
                Item check = atFeet[i];

                if (check.StackWith(m, item, false))
                    return true;
            }

            ColUtility.Free(atFeet);

            item.MoveToWorld(m.Location, map);
            return true;
        }

        public bool Metal()
        {
            return CraftResources.GetType(_resource) == CraftResourceType.Metal;
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (CraftResources.GetType(_resource) == CraftResourceType.Metal)
                list.Add("Deposito de " + _resource.ToString());
            else
                list.Add("Arvore de " + _resource.ToString());
        }

        public void SetGrafico()
        {
            if (_folha != null)
            {
                _folha.Delete();
                _folha = null;
            }
            else
            {
                if (this.timer != null)
                {
                    this.timer.Stop();
                }
            }

            if (CraftResources.GetType(_resource) == CraftResourceType.Metal)
            {
                switch (_quality)
                {
                    case ItemQuality.Exceptional: this.ItemID = 0x9CAC; break;
                    case ItemQuality.Normal: this.ItemID = 0x9CAB; break;
                    case ItemQuality.Low: this.ItemID = 0x9CAA; break;
                }
            }
            else
            {
                var folha = 0;
                var caule = 0;
                switch (_resource)
                {
                    case CraftResource.Cedro: caule = 3293; folha = 3294; break;
                    case CraftResource.Carvalho: caule = 3290; folha = 3292; break;
                    case CraftResource.Pinho: caule = 3286; folha = 3287; break;
                    case CraftResource.Eucalipto: caule = 3293; folha = 3294; break;
                    case CraftResource.Mogno: caule = 3492; folha = 3495; break;
                    case CraftResource.Carmesim: caule = 3480; folha = 3481; break;
                    case CraftResource.Gelo: caule = 3286; folha = 9969; break;
                }
                if (Shard.DebugEnabled)
                {
                    Shard.Debug("ID FOLHA: " + folha);
                }
                if (folha != 0)
                {
                    this.timer = Timer.DelayCall(TimeSpan.FromSeconds(0.1), () =>
                    {
                        if (this.Deleted)
                            return;

                        _folha = new Item(folha);
                        _folha.Name = "Folhas de " + _resource.ToString();
                        _folha.Hue = this.Hue;
                        _folha.Movable = false;
                        _folha.MoveToWorld(Location, Map);
                        this.timer = null;
                    });
                }
                this.ItemID = caule;
            }
        }

        Timer timer;

        public Item GetItem()
        {
            switch (_resource)
            {
                case CraftResource.Cobre: return new CopperOre(Quantidade);
                case CraftResource.Bronze: return new BronzeOre(Quantidade);
                case CraftResource.Niobio: return new NiobioOre(Quantidade);
                case CraftResource.Dourado: return new SilverOre(Quantidade);
                case CraftResource.Adamantium: return new AdamantiumOre(Quantidade);
                case CraftResource.Berilo: return new BeriloOre(Quantidade);
                case CraftResource.Quartzo: return new QuartzoOre(Quantidade);
                case CraftResource.Lazurita: return new LazuritaOre(Quantidade);
                case CraftResource.Vibranium: return new VibraniumOre(Quantidade);
                case CraftResource.Eucalipto: return new HeartwoodLog(Quantidade);
                case CraftResource.Carmesim: return new BloodwoodLog(Quantidade);
                case CraftResource.Gelo: return new FrostwoodLog(Quantidade);
                case CraftResource.Pinho: return new AshLog(Quantidade);
                case CraftResource.Mogno: return new YewLog(Quantidade);
                case CraftResource.Carvalho: return new OakLog(Quantidade);
            }
            return null;
        }

        public int Quantidade { get { return 1 + (int)_quality + Utility.Random(2); } }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Talvez voce possa obter estes recursos com uma ferramenta...");
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            //base.AddNameProperties(list);
            if (CraftResources.GetType(_resource) == CraftResourceType.Metal)
                list.Add("Deposito de " + _resource.ToString());
            else
                list.Add("Arvore de " + _resource.ToString());

            if (_quality == ItemQuality.Low)
                list.Add("Pequeno");
            else if (_quality == ItemQuality.Exceptional)
                list.Add("Grande");
        }

        [CommandProperty(AccessLevel.Administrator)]
        public CraftResource Resource
        {
            get
            {
                return _resource;
            }
            set
            {
                _resource = value;
                Hue = CraftResources.GetHue(value);
                InvalidateProperties();
                SetGrafico();
                if (CraftResources.GetType(_resource) == CraftResourceType.Metal)
                    Name = "Deposito de " + _resource.ToString();
                else
                    Name = "Arvore de " + _resource.ToString();
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public ItemQuality Quality
        {
            get
            {
                return _quality;
            }
            set
            {
                _quality = value;
                InvalidateProperties();
                SetGrafico();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write((int)_resource);
            writer.Write((int)_quality);
            writer.Write(_folha);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var v = reader.ReadInt();
            _resource = (CraftResource)reader.ReadInt();
            _quality = (ItemQuality)reader.ReadInt();
            _folha = reader.ReadItem();

            Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
            {
                Registra(this);
            });

            Timer.DelayCall(TimeSpan.FromHours(4), () =>
            {
                if (!this.Deleted && this.Coletando == null)
                {
                    this.Delete();
                }
            });
        }
    }
}
