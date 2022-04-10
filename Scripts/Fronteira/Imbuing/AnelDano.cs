using Server.Gumps;
using Server.Items;
using Server.Leilaum;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Items
{
    public class AnelDano : BaseRing
    {
        [Constructable]
        public AnelDano()
            : base(0x108a)
        {
            Name = "Anel PvM";
            Hue = 1152;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (this.Attributes.WeaponDamage >= 20 || this.Attributes.DefendChance >= 20  || this.Attributes.SpellDamage >= 20 || this.Attributes.Resistence >= 20 || !from.Player)
                return;

            from.SendGump(new ColarDanoGump(from as PlayerMobile, this));
        }

        public AnelDano(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1); // version

        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if(version == 0)
            {
                var Nivel = reader.ReadInt();
                var Tipo = reader.ReadInt();
                if(Tipo == 1)
                {
                    this.Attributes.WeaponDamage = Nivel;
                } else if(Tipo == 2)
                {
                    this.Attributes.DefendChance = Nivel;
                } else if(Tipo == 3)
                {
                    this.Attributes.SpellDamage = Nivel;
                }
            }
          
        }
    }

    public class ColarDanoGump : Gump
    {
        private AnelDano e;

        public ColarDanoGump(PlayerMobile pl, AnelDano colar) : base(0, 0)
        {
            e = colar;
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            this.AddPage(0);
            this.AddBackground(59, 76, 344, 199, 9200);
            this.AddHtml(191, 82, 90, 19, @"Aprimorar Anel", (bool)false, (bool)false);

            this.AddBackground(67, 112, 100, 100, 3500);

            this.AddBackground(62, 192, 111, 24, 3000);


            this.AddHtml(62, 192, 111, 24, $"<CENTER>Frg. Reliquia</CENTER>", false, false);
            NewAuctionGump.AddItemCentered(67, 112, 111, 101, 0x2DB3, Paragon.Hue, this);
            this.AddHtml(71, 125, 89, 21, $"<CENTER>5</CENTER>", false, false);

            this.AddBackground(180, 112, 100, 100, 3500);

            this.AddBackground(174, 192, 111, 24, 3000);
            this.AddHtml(174, 192, 111, 24, $"<CENTER>Crtl Elemental</CENTER>", false, false);
            NewAuctionGump.AddItemCentered(180, 112, 111, 101, 16395, 2611, this);
            this.AddHtml(184, 125, 89, 21, $"<CENTER>20</CENTER>", false, false);

            this.AddBackground(292, 112, 100, 100, 3500);
            this.AddBackground(286, 192, 111, 24, 3000);
            this.AddHtml(286, 192, 111, 24, $"<CENTER>Frg. Antigo</CENTER>", false, false);
            NewAuctionGump.AddItemCentered(292, 112, 111, 101, 0x1053, 1152, this);
            this.AddHtml(296, 125, 89, 21, $"<CENTER>10</CENTER>", false, false);

            this.AddImage(49, 61, 113);
            this.AddImage(381, 63, 113);
            this.AddImage(386, 259, 113);
            this.AddImage(46, 254, 113);
            this.AddHtml(310, 231, 85, 21, @"Aprimorar", (bool)false, (bool)false);
            this.AddButton(282, 228, 2472, 2472, 1, GumpButtonType.Reply, 0);

            this.AddImage(381, 63, 113);
            this.AddImage(386, 259, 113);
            this.AddImage(46, 254, 113);
            this.AddImage(49, 61, 113);
        }

        public enum ElementoButtons
        {
            Nads,
            Upar,
            Derreter
        }


        public override void OnResponse(NetState sender, RelayInfo info)
        {
            var from = sender.Mobile as PlayerMobile;

            if (info.ButtonID != 1)
            {
                return;
            }

            if (info.ButtonID == 2)
            {
                from.SendGump(new ConfirmaGump(from, "Confirmar Derreter?", "Voce deseja derreter este anel, assim destruindo-o e recuperando material ?", () => {
                    e.Consume();
                    //var i = new 
                    // from.PlaceInBackpack()
                }));
                return;
            }

            if (!from.Backpack.HasItems(new Type[] { typeof(RelicFragment) }, new int[] { 5 }))
            {
                from.SendMessage("Falta fragmentos de reliquia");
                return;
            }
            var cristal = from.Backpack.FindItemByType<CristalElemental>();
            if (cristal == null || cristal.Amount < 20)
            {
                from.SendMessage("Faltam cristais elementais");
                return;
            }

            var frag = from.Backpack.FindItemByType<FragmentosAntigos>();
            if (frag == null || frag.Amount < 10)
            {
                from.SendMessage("Faltam fragmentos antigos");
                return;
            }

            frag.Consume(10);
            cristal.Consume(20);
            from.Backpack.ConsumeTotal(new Type[] { typeof(RelicFragment) }, new int[] { 5 });

            Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
            Effects.PlaySound(from.Location, from.Map, 0x243);

            Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
            Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Map), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

            Effects.SendTargetParticles(from, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);

            if(e.Attributes.WeaponDamage > 0)
                e.Attributes.WeaponDamage += 1;
            else if(e.Attributes.SpellDamage > 0)
                e.Attributes.SpellDamage += 1;
            else if (e.Attributes.DefendChance > 0)
                e.Attributes.DefendChance += 1;
            from.PlayAttackAnimation();
            from.OverheadMessage("* encantou *");
            from.SendMessage("Seu anel absorveu a energia");
            from.CloseAllGumps();
        }
    }
}
