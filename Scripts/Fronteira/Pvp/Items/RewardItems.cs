using Server;
using System;
using Server.TournamentSystem;
using System.Collections.Generic;

namespace Server.Items
{
    public static class TourneyRewards
    {
        /*
         * TODO: Hooks needed
         * Trophies - OnTournamentEnd(...); also a null check for incrementing points to winner/runnerup
         * Loser Sashes - EliminateTeam(...)
         * Sponsors - AwardHightestSponsor(); take out everytyhing after if(winner != null)
        */

        public static void GenerateTrophies(ArenaTeam winner, ArenaTeam loser, string tourneyName, List<ArenaTeam> origParts)
        {
            TournamentTrophy trophy = null;
            if (winner != null)
            {
                trophy = new TournamentTrophy(winner, tourneyName, 1);
                Timer.DelayCall(TimeSpan.FromSeconds(30.0), new TimerStateCallback(DropReward), new object[] { winner.TeamLeader, trophy });
            }
            if (loser != null)
            {
                trophy = new TournamentTrophy(loser, tourneyName, 2);
                Timer.DelayCall(TimeSpan.FromSeconds(30.0), new TimerStateCallback(DropReward), new object[] { loser.TeamLeader, trophy });
            }

            if (origParts == null || origParts.Count == 0)
                return;

            if (origParts.Contains(winner))
                origParts.Remove(winner);
            if (origParts.Contains(loser))
                origParts.Remove(loser);

            foreach (ArenaTeam team in origParts)
                GenerateLoserSashes(team, tourneyName);
        }

        public static void GenerateLoserSashes(ArenaTeam loser, string tournyName)
        {
            if (loser == null)
                return;

            foreach (Mobile mob in loser.Fighters)
            {
                BodySash sash = new BodySash();
                sash.Name = String.Format("Participante do Torneio {0} - {1}", tournyName, DateTime.Now.ToShortDateString());
                Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerStateCallback(DropReward), new object[] { mob, sash });
            }
        }

        public static void GenerateSponsorReward(Mobile winner, Tournament tourney)
        {
            if (winner != null && tourney != null)
            {
                string str = String.Format("{0} eh quem patrocinou o torneio {1}! Por favor, deem a {2} uma salva de palmas.", winner.Name, tourney.Name, winner.Female ? "ela" : "ele");
                tourney.DoTournamentMessage(str, false);

                BaseClothing clothing;
                switch (Utility.Random(5))
                {
                    default:
                    case 0: clothing = new FancyShirt(Utility.Random(2707, 12)); break;
                    case 1: clothing = new Shirt(Utility.Random(2707, 12)); break;
                    case 2: clothing = new HalfApron(Utility.Random(2707, 12)); break;
                    case 3: clothing = new FullApron(Utility.Random(2707, 12)); break;
                    case 4: clothing = new FormalShirt(Utility.Random(2707, 12)); break;
                }

                clothing.LootType = LootType.Blessed;
                clothing.Name = String.Format("{0}, Patrocinador do Torneio {1}", winner.Name, tourney.Name);
                clothing.Attributes.Luck = Utility.RandomMinMax(10, 15);

                DropReward(new object[] { winner, clothing });
            }

        }

        public static void DropReward(object obj)
        {
            object[] o = (object[])obj;
            Mobile mob = (Mobile)o[0];
            Item reward = (Item)o[1];
            Container pack = mob.Backpack;

            if (pack == null || !pack.TryDropItem(mob, reward, false))
                mob.BankBox.DropItem(reward);
        }
    }

    public class TournamentTrophy : Item
    {
        private string m_TeamName;
        private List<string> m_FighterNames = new List<string>();

        public TournamentTrophy(ArenaTeam team, string tourney, int placing)
            : base(Utility.Random(0x139A, 4))
        {
            m_TeamName = team != null ? team.Name : "Unknown";
            Hue = Utility.Random(2958, 12);
            LootType = LootType.Blessed;

            if (team != null)
            {
                foreach (Mobile mob in team.Fighters)
                    m_FighterNames.Add(mob.Name);
            }

            Name = String.Format("{0} Torneio: {1} Lugar", tourney, placing == 1 ? "1st" : "2nd");
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add(1060659, "{0}\t{1}", "Time", m_TeamName);
            int cliloc = 1060660;
            if (m_FighterNames.Count == 1)
                list.Add(m_FighterNames[0]);
            else
            {
                for (int i = 0; i < m_FighterNames.Count; i++)
                {
                    string title;
                    if (i == 0) title = "Lider";
                    else title = "Membro";
                    list.Add(cliloc + i, "{0}\t{1}", title, m_FighterNames[i]);
                }
            }
        }

        public TournamentTrophy(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt((int)0); // version

            writer.Write(m_TeamName);
            writer.Write(m_FighterNames.Count);

            for (int i = 0; i < m_FighterNames.Count; i++)
                writer.Write(m_FighterNames[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadEncodedInt();

            m_TeamName = reader.ReadString();
            int count = reader.ReadInt();

            for (int i = 0; i < count; i++)
                m_FighterNames.Add(reader.ReadString());
        }

    }
}
