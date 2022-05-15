using Fronteira.Discord;
using Server.Commands;
using Server.Engines.VvV;
using Server.Guilds;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Fronteira.Items
{
    public class TamborDaDiscordia : Item
    {
        [Constructable]
        public TamborDaDiscordia() : base(0xE9C)
        {
            Name = "Tambor da Discordia";
        }

        public TamborDaDiscordia(Serial s) : base(s) { }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Chame para o PvP e/ou guerra infinita !");
            list.Add("A paz eh uma soh");
            list.Add("Mas a treta eh infinita !");
            list.Add("Feito Por : R o m e r o C o X");
        }

        private Dictionary<Guild, DateTime> Chamaram = new Dictionary<Guild, DateTime>();

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);
            if (ViceVsVirtueSystem.Instance.Battle.OnGoing)
            {
                from.SendMessage("Uma batalha ja esta acontecendo");
                return;
            }
            if (!from.IsCooldown("tambor"))
            {
                from.PlayAttackAnimation();
                from.PlaySound(0x39);
                from.OverheadMessage("* tocou *");
                from.SetCooldown("tambor", TimeSpan.FromMinutes(10));
                var msgPlayer = $"{from.RawName} esta tocando o Tambor da Discordia em { from.Region?.Name}";
                var msg = $":drum: {msgPlayer}";
                DiscordBot.SendMessage(msg);
                foreach (var p in NetState.GetOnlinePlayerMobiles())
                    p.SendMessage(msgPlayer);
                from.CriminalAction(false);
                if (from.Guild != null)
                {
                    var g = from.Guild as Guild;
                    Chamaram[g] = DateTime.UtcNow;
                    var recentes = ChamadasRecentes();
                    if (recentes.Count >= 2)
                    {
                        var guildas = recentes.Select(guilda => guilda.Name).Aggregate(
                           "", (current, next) => current + ", " + next);
                        DiscordBot.SendMessage($":drum: As guildas {guildas} tocaram o tambor da discordia e iniciaram uma Guerra");
                        ViceVsVirtueSystem.Instance.Battle.ForceStart = true;
                    }
                    else
                    {
                        from.SendMessage(78, "Se outra guilda tocar o tambor da discordia em breve, a guerra infinita se iniciara !");
                    }
                }
                else
                {
                    from.SendMessage(78, "Estando em uma guilda e tocando o tambor da discordia, voce pode chamar outras guildas para guerra !");

                }
            }
            else
            {
                from.SendMessage("Aguarde para tocar o tambor da discordia novamente");
            }
        }

        public List<Guild> ChamadasRecentes()
        {
            var i = new List<Guild>();
            foreach (var tag in Chamaram.Keys)
            {
                if (Chamaram[tag] + TimeSpan.FromMinutes(10) > DateTime.UtcNow)
                {
                    i.Add(tag);
                }
            }
            return i;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var v = reader.ReadInt();
        }
    }
}
