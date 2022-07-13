using Server.Mobiles;
using System;

namespace Server.Items
{
    public class TalismanDaMorte : BaseTalisman
    {
        public int ExpPrecisa()
        {
            return this.Killer.Amount;
        }

        public int Exp { get; set; }

        [Constructable]
        public TalismanDaMorte()
            : base(0x2F58)
        {
            this.Name = "Talisman de Dano";
            this.Hue = Paragon.Hue;
        }

        public TalismanDaMorte(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (this.Killer == null || this.Killer.Amount == 0)
            {
                list.Add("Talisman da Punicao");
            }
            else
            {
                list.Add("Talisman da Punicao Contra " + this.Killer.Name);
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            AddNameProperty(list);
            if (this.Killer == null || this.Killer.Amount == 0)
            {
                list.Add("Mate um monstro com isto equipado para absorver a alma dele.");
            }
            else
            {
                list.Add(String.Format("Dano: {0}%", this.Killer.Amount.ToString()));
                if (this.Killer.Amount < 70)
                    list.Add("Exp: " + Exp + "/" + ExpPrecisa());
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
            writer.Write(Exp);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
            Exp = reader.ReadInt();
        }
    }

    public class TalismanDaProtecao : BaseTalisman
    {
        public static void Configure()
        {
            EventSink.CreatureDeath += BixoMorre;
        }

        public static void BixoMorre(CreatureDeathEventArgs e)
        {
            var bixo = e.Creature as BaseCreature;
            if (bixo == null)
                return;

            foreach (var m in bixo.GetLootingRights())
            {
                var pl = m.m_Mobile as PlayerMobile;
                if (pl == null)
                    continue;

                var tali = pl.Talisman as TalismanDaProtecao;
                if (tali != null)
                {
                    var protecao = (tali.Protection == null ? 0 : tali.Protection.Amount);

                    if (tali.Protection == null || protecao == 0)
                    {
                        tali.SetProtection(bixo.GetType(), bixo.Name, 1);
                        pl.SendMessage("Seu talisman absorveu a alma do monstro");
                        pl.PlaySound(0x1FA);
                        bixo.MovingParticles(pl, 0x36D4, 5, 0, false, true, 1, 9502, 4019, 0x160, 0, 0);
                        pl.FixedEffect(0x3735, 6, 30);
                        tali.InvalidateProperties();
                        continue;
                    }
                    else
                    {
                        if (tali.Protection.Amount >= 70)
                            continue;
                    }

                    if (bixo.Name != null && bixo.Name == tali.Protection.Name)
                    {
                        pl.PlaySound(0x1FA);

                        bixo.MovingParticles(pl, 0x36D4, 5, 0, false, true, 1, 9502, 4019, 0x160, 0, 0);
                        if (bixo.IsParagon)
                        {
                            tali.Exp += 5;
                        }
                        else
                        {
                            tali.Exp += 1;
                        }
                        if (tali.Exp >= tali.ExpPrecisa())
                        {
                            tali.Exp = 0;
                            tali.Protection.Amount += 1;
                            pl.SendMessage("Seu talisman subiu de nivel ao absorver a alma do monstro");
                        }
                        tali.InvalidateProperties();
                    }
                }
                else
                {
                    var tali2 = pl.Talisman as TalismanDaMorte;
                    if (tali2 != null)
                    {
                        var protecao = (tali2.Killer == null ? 0 : tali2.Killer.Amount);

                        if (tali2.Killer == null || protecao == 0)
                        {
                            tali2.SetKiller(bixo.GetType(), bixo.Name, 1);
                            pl.SendMessage("Seu talisman absorveu a alma do monstro");
                            pl.PlaySound(0x1FA);
                            bixo.MovingParticles(pl, 0x36D4, 5, 0, false, true, 1, 9502, 4019, 0x160, 0, 0);
                            pl.FixedEffect(0x3735, 6, 30);
                            tali2.InvalidateProperties();
                            continue;
                        }
                        else
                        {
                            if (tali2.Killer.Amount >= 70)
                                continue;
                        }

                        if (bixo.Name != null && bixo.Name == tali2.Killer.Name)
                        {
                            pl.PlaySound(0x1FA);

                            bixo.MovingParticles(pl, 0x36D4, 5, 0, false, true, 1, 9502, 4019, 0x160, 0, 0);
                            if (bixo.IsParagon)
                            {
                                tali2.Exp += 5;
                            }
                            else
                            {
                                tali2.Exp += 1;
                            }
                            if (tali2.Exp >= tali2.ExpPrecisa())
                            {
                                tali2.Exp = 0;
                                tali2.Killer.Amount += 1;
                                pl.SendMessage("Seu talisman subiu de nivel ao absorver a alma do monstro");
                            }
                            tali2.InvalidateProperties();
                        }
                    }
                }

            }
        }

        public int ExpPrecisa()
        {
            return this.Protection.Amount;
        }

        private int Exp { get; set; }

        [Constructable]
        public TalismanDaProtecao()
            : base(0x2F58)
        {
            this.Name = "Talisman da Protecao";
            this.Hue = Paragon.Hue;
        }

        public TalismanDaProtecao(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if(this.Protection == null || this.Protection.Amount == 0)
            {
                list.Add("Talisman da Protecao");
            } else
            {
                list.Add("Talisman da Protecao Contra " + this.Protection.Name);
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            AddNameProperty(list);
            if (this.Protection == null || this.Protection.Amount == 0)
            {
                list.Add("Mate um monstro com isto equipado para absorver a alma dele.");
            }
            else
            {
                list.Add(String.Format("Protecao: {0}%", this.Protection.Amount.ToString()));
                if(this.Protection.Amount < 70)
                    list.Add("Exp: " + Exp + "/" + ExpPrecisa());
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
            writer.Write(Exp);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
            Exp = reader.ReadInt();
        }
    }
}
