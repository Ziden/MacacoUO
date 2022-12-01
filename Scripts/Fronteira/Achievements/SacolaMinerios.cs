using Server.Engines.Craft;
using Server.Engines.Points;
using Server.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Ziden.Achievements
{
    public class SacolaMinerios : Bag
    {
        [Constructable]
        public SacolaMinerios()
        {
            this.AddItem(new IronIngot(100));
            Name = "Sacola";
        }

        public SacolaMinerios(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class VitoriaFFA : Bag
    {
        [Constructable]
        public VitoriaFFA()
        {
            this.DropItem(new PergaminhoCarregamento());
            this.DropItem(new SacolaJoias());
            this.DropItem(new SacolaPots());
            this.DropItem(new Gold(5000));
            Name = "Vitoria do FFA";
        }

        public VitoriaFFA(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class PontoArena : Item
    {
        [Constructable]
        public PontoArena(): base(0x9EC0)
        {
            Name = "Morango da Coragem";
            this.Stackable = true;
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Coma para ganhar +1 ponto de Arena PvP");
        }

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);
            var amt = this.Amount;
            var pontos = PointsSystem.PontosArena.GetPoints(from);
            pontos += amt;
            PointsSystem.PontosArena.AwardPoints(from, amt);
            from.SendMessage($"Voce ganhou {amt} pontos de arena. Use .pontos para ver todos seus pontos");
            from.OverheadMessage("* morangou *");
            from.Animate(AnimationType.Eat, 0);
            from.PlaySound(Utility.Random(0x3A, 3));
            Consume(amt);
        }

        public PontoArena(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class ParticipacaoPvP : Bag
    {
        [Constructable]
        public ParticipacaoPvP()
        {
            this.DropItem(new CombatSkillBook());
            this.DropItem(new CombatSkillBook());
            this.DropItem(new SacolaJoias(6));
            for (var x = 0; x < 10; x++)
            {
                this.DropItem(new HealPotion());
                this.DropItem(new CurePotion());
                this.DropItem(new ExplosionPotion());
                this.DropItem(new ManaPotion());
                this.DropItem(new RefreshPotion());
            }
            this.DropItem(new Gold(3000));
            Name = "Sacola";
        }

        public ParticipacaoPvP(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class MiniParticipacaoPvP : Bag
    {
        [Constructable]
        public MiniParticipacaoPvP()
        {
            this.DropItem(new CombatSkillBook());
            this.DropItem(new CombatSkillBook());
            this.DropItem(new SacolaJoias(3));
            for (var x = 0; x < 3; x++)
            {
                this.DropItem(new HealPotion());
                this.DropItem(new CurePotion());
                this.DropItem(new ExplosionPotion());
                this.DropItem(new ManaPotion());
                this.DropItem(new RefreshPotion());
            }
            this.DropItem(new Gold(1000));
            Name = "Sacola";
        }

        public MiniParticipacaoPvP(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaPots : Bag
    {
        [Constructable]
        public SacolaPots()
        {
            for (var x = 0; x < 10; x++)
            {
                this.DropItem(new HealPotion());
                this.DropItem(new CurePotion());
                this.DropItem(new ExplosionPotion());
                this.DropItem(new ManaPotion());
                this.DropItem(new RefreshPotion());
            }
            Name = "Sacola";
        }

        public SacolaPots(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class Trofeu : Item
    {
        [CommandProperty(AccessLevel.Administrator)]
        public String[] Textos { get; set; }

        [Constructable]
        public Trofeu() : base(0x1227)
        {
            Name = "Trofeu";
            Textos = new string[] { "Apenas um trofeu", "Nada importante" };
        }

        [Constructable]
        public Trofeu(string texto1, string texto2) : base(0x1227)
        {
            Name = "Trofeu";
            Textos = new string[] { texto1, texto2 };
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            if (Textos == null)
                return;
            foreach (var s in Textos)
            {
                list.Add(s);
            }
        }

        public Trofeu(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Textos == null ? 0 : Textos.Count());
            if (Textos != null)
            {
                foreach (var s in Textos)
                    writer.Write(s);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var ct = reader.ReadInt();
            var t = new List<String>();
            for (var x = 0; x < ct; x++)
                t.Add(reader.ReadString());
            if (t.Count() > 0)
                Textos = t.ToArray();

        }
    }


    public class SacolaFFA : Bag
    {
        [Constructable]
        public SacolaFFA()
        {
            Name = "Sacola";
        }

        public SacolaFFA(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaCristais : Bag
    {
        [Constructable]
        public SacolaCristais()
        {
            this.AddItem(new CristalElemental(100));
            Name = "Sacola";
        }

        public SacolaCristais(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaCristalTherathan : Bag
    {
        [Constructable]
        public SacolaCristalTherathan()
        {
            this.AddItem(new CristalTherathan(50));
            Name = "Sacola Therathan";
            Hue = 1175;
        }

        public SacolaCristalTherathan(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class ScolaJoias : Bag
    {
        [Constructable]
        public ScolaJoias()
        {
            foreach (var type in BasePedraPreciosa.Elementos.Keys)
            {
                var joia = (BasePedraPreciosa)Activator.CreateInstance(type);
                joia.Amount = 20;
                this.AddItem(joia);
            }
            Name = "Sacola";
        }

        public ScolaJoias(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaHalloween : Bag
    {

        public static Type[] Tipos = new Type[]
        {
            typeof(HalloweenBloodFountainAddonDeed), typeof(HalloweenCasketTempleAddonDeed), typeof(HalloweenGhoulPicnicAddonDeed), typeof(HalloweenGuillotinePatchAddonDeed),
            typeof(HalloweenHellPitAddonDeed), typeof(HalloweenSkullPostAddonDeed), typeof(HalloweenTortureChamberAddonDeed), typeof(HalloweenTreeRedAddonDeed), typeof(HalloweenTreeBlackAddonDeed)
        };

        [Constructable]
        public SacolaHalloween()
        {
            Name = "Sacola de Halloween";
            Hue = 38;
            try
            {
                var item = Tipos[Utility.Random(Tipos.Length)];
                var i = Activator.CreateInstance(item) as Item;
                AddItem(i);
            }
            catch (Exception e)
            {
                Name = "Sacola de Halloween da Travessura";
            }
        }

        public SacolaHalloween(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }


    public class SacolaReceitaAlch : Bag
    {
        [Constructable]
        public SacolaReceitaAlch()
        {
            this.AddItem(DefAlchemy.GetRandomRecipe());
            Name = "Sacola";
        }

        public SacolaReceitaAlch(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class ScolaDizimo : Bag
    {
        [Constructable]
        public ScolaDizimo()
        {
            this.AddItem(new IronIngot(100));
            Name = "Sacola";
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("Voce recebeu 100 pontos para usar habilidades de paladino (Tithe points)");
            from.TithingPoints += 100;
            Delete();

        }

        public ScolaDizimo(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaBands : Bag
    {
        [Constructable]
        public SacolaBands()
        {
            this.DropItem(new Bandage(50));
            Name = "Sacola";
        }

        public SacolaBands(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaJoias : Bag
    {
        [Constructable]
        public SacolaJoias()
        {
            for (var i = 0; i < 20; i++)
            {
                this.DropItem(Loot.RandomGem());
            }
            Name = "Sacola de Joias";
        }

        [Constructable]
        public SacolaJoias(int qtd)
        {
            for (var i = 0; i < qtd; i++)
            {
                this.DropItem(Loot.RandomGem());
            }
            Name = "Sacola de Joias";
        }

        public SacolaJoias(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaDeOuro : Bag
    {
        [Constructable]
        public SacolaDeOuro()
        {
            this.AddItem(new Gold(300));
            Name = "Sacola";
        }

        public SacolaDeOuro(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaDeOuro3000 : Bag
    {
        [Constructable]
        public SacolaDeOuro3000()
        {
            this.AddItem(new Gold(3000));
            Name = "Sacola";
        }

        public SacolaDeOuro3000(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }


    public class SacolaMadeiras : Bag
    {
        [Constructable]
        public SacolaMadeiras()
        {
            this.AddItem(new Board(100));
            Name = "Sacola";
        }

        public SacolaMadeiras(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaTecidos : Bag
    {
        [Constructable]
        public SacolaTecidos()
        {
            this.AddItem(new Cloth(200));
            Name = "Sacola";
        }

        public SacolaTecidos(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaMadeirasGrande : Bag
    {
        [Constructable]
        public SacolaMadeirasGrande()
        {
            this.AddItem(new Board(300));
            Name = "Sacola";
        }

        public SacolaMadeirasGrande(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public class SacolaMineriosGrande : Bag
    {
        [Constructable]
        public SacolaMineriosGrande()
        {
            Name = "Sacola";
            this.AddItem(new IronIngot(300));
        }

        public SacolaMineriosGrande(Serial s) : base(s) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }
}
