#region Header
//   Vorspire    _,-'/-'/  FFABattle.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using Fronteira.Discord;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Ziden.Achievements;
using VitaNex.Schedules;
#endregion

namespace VitaNex.Modules.AutoPvP.Battles
{
	public class FFABattle : PvPBattle
	{
		public FFABattle()
		{
			Name = "Free For All";
			Category = "Free For All";
			Description = "The last participant alive wins!";

			AddTeam(NameList.RandomName("daemon"), 5, 40, 85);

			Schedule.Info.Months = ScheduleMonths.All;
			Schedule.Info.Days = ScheduleDays.All;
			Schedule.Info.Times = ScheduleTimes.EveryHour;

			Options.Timing.PreparePeriod = TimeSpan.FromMinutes(5.0);
			Options.Timing.RunningPeriod = TimeSpan.FromMinutes(15.0);
			Options.Timing.EndedPeriod = TimeSpan.FromMinutes(5.0);

			Options.Rules.AllowBeneficial = true;
			Options.Rules.AllowHarmful = true;
			Options.Rules.AllowHousing = false;
			Options.Rules.AllowPets = false;
			Options.Rules.AllowSpawn = false;
			Options.Rules.AllowSpeech = true;
			Options.Rules.CanBeDamaged = true;
			Options.Rules.CanDamageEnemyTeam = true;
			Options.Rules.CanDamageOwnTeam = true;
			Options.Rules.CanDie = false;
			Options.Rules.CanHeal = true;
			Options.Rules.CanHealEnemyTeam = false;
			Options.Rules.CanHealOwnTeam = false;
			Options.Rules.CanMount = false;
			Options.Rules.CanFly = false;
			Options.Rules.CanResurrect = false;
			Options.Rules.CanUseStuckMenu = false;

		}

		public FFABattle(GenericReader reader)
			: base(reader)
		{ }


        protected override void BroadcastStartMessage(TimeSpan timeLeft)
        {
            foreach(var pl in NetState.GetOnlinePlayerMobiles())
            {
                if(!pl.IsCooldown("dicaffa"))
                {
                    pl.SetCooldown("dicaffa", TimeSpan.FromMinutes(10));
                    pl.SendMessage(78, "No PvP FFA nao se perde items nem fama, e ganham ouro e pocoes soh de participar !! Digite .pvp e escolha a batalha FFA para participar !");
                }
            }
            base.BroadcastStartMessage(timeLeft);
        }

        protected override void OnQueueJoin(PlayerMobile pm, PvPTeam team)
        {
            base.OnQueueJoin(pm, team);
            var msg = $"[Arena] {pm.Name} entrou na fila para arena {Name}";
            foreach (var pl in NetState.GetOnlinePlayerMobiles())
            {
                pl.SendMessage(msg);
            }
            DiscordBot.SendMessage(":crossed_swords:" + msg);

            if (Options.Rules.AutoStart)
            {
                if (Queue.Count >= this.MaxCapacity)
                    this.State = PvPBattleState.Preparando;
            }
        }

        public override void GiveWinnerReward(PlayerMobile pm)
        {
            base.GiveWinnerReward(pm);
            var trofeu = new Trofeu();
            trofeu.Hue = 0x8A5;
            var data = DateTime.UtcNow;
            trofeu.Textos = new string[]
            {
                "Arena Free For All",
                $"{data.Day}/{data.Month}/{data.Year}",
                pm.Female ? "Campea" : "Campeao",
                pm.Name
            };
            trofeu.Name = "[OURO] Trofeu de Arena PvP";
            trofeu.Hue = Paragon.Hue;
            pm._PlaceInBackpack(trofeu);
        }

        public override bool AddTeam(string name, int minCapacity, int capacity, int color)
        {
            return AddTeam(new FFATeam(this, name, minCapacity, capacity, color));
        }


        public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}
}
