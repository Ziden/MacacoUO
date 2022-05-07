using Server;
using Server.Items;
using System;

public class AntiParaPotion : BasePotion
{
    public override double Delay
    {
        get
        {
            return 10.0;
        }
    }

    [Constructable]
    public AntiParaPotion(): base(0x5748, PotionEffect.AntiParalize)
    {
        Name = "Pocao Anti Paralizia";
        Hue = 255;
        Stackable = true;
    }

    public AntiParaPotion(Serial serial)
        : base(serial)
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
        ItemID = 0x5748;
        Hue = 255;
    }

    public override void Drink(Mobile m)
    {
        m.PotAntiPara = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.AnticipateHit, 1114057, "Anti-Paralyze"));
        m.SendMessage("Voce agora esta mais resistente a paralizia por 10 segundos");
        Timer.DelayCall(TimeSpan.FromSeconds(10), () =>
        {
            BuffInfo.RemoveBuff(m, BuffIcon.AnticipateHit);
        });

        /*
        Timer.DelayCall(TimeSpan.FromSeconds(1.8), () =>
        {
            if(m.Alive && m.Paralyzed)
            {
                m.SendMessage("Voce terminou de tomar uma pocao para paralizia");
                m.Paralyzed = false;
                m.Stam /= 2;
                m.Damage(m.Hits / 10);
                Consume();
                m.FixedEffect(0x375A, 10, 15);
                m.PlaySound(0x1E7);
            }
        });
        */
    }
}
