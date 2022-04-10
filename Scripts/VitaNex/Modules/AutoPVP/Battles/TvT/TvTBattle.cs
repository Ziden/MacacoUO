#region Header
//   Vorspire    _,-'/-'/  TvTBattle.cs
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
using System.Linq;
using Fronteira.Discord;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Ziden.Achievements;
using VitaNex.Schedules;
#endregion

namespace VitaNex.Modules.AutoPvP.Battles
{
	public class TvTBattle : PvPBattle
	{
		public TvTBattle()
		{
			Name = "Team vs Team";
			Category = "Team vs Team";
			Description = "The last team alive wins!";

			Ranked = true;
			RewardTeam = true;

			AddTeam(NameList.RandomName("daemon"), 1, 1, 0x22);
			AddTeam(NameList.RandomName("daemon"), 1, 1, 0x55);

			Schedule.Info.Months = ScheduleMonths.All;
			Schedule.Info.Days = ScheduleDays.All;
			Schedule.Info.Times = ScheduleTimes.EveryQuarterHour;

			Options.Timing.PreparePeriod = TimeSpan.FromMinutes(2.0);
			Options.Timing.RunningPeriod = TimeSpan.FromMinutes(8.0);
			Options.Timing.EndedPeriod = TimeSpan.FromMinutes(1.0);

			Options.Rules.AllowBeneficial = true;
			Options.Rules.AllowHarmful = true;
			Options.Rules.AllowHousing = false;
			Options.Rules.AllowPets = false;
			Options.Rules.AllowSpawn = false;
			Options.Rules.AllowSpeech = true;
			Options.Rules.CanBeDamaged = true;
			Options.Rules.CanDamageEnemyTeam = true;
			Options.Rules.CanDamageOwnTeam = false;
			Options.Rules.CanDie = false;
			Options.Rules.CanHeal = true;
			Options.Rules.CanHealEnemyTeam = false;
			Options.Rules.CanHealOwnTeam = true;
			Options.Rules.CanMount = false;
			Options.Rules.CanFly = false;
			Options.Rules.CanResurrect = false;
			Options.Rules.CanUseStuckMenu = false;
			Options.Rules.CanEquip = true;
            Options.Rules.AutoStart = true;
        }

        public override void GiveWinnerReward(PlayerMobile pm)
        {
            base.GiveWinnerReward(pm);
            var trofeu = new Trofeu();
            trofeu.Hue = 0x8A5;
            var data = DateTime.UtcNow;
            trofeu.Textos = new string[]
            {
                $"Arena {Name}",
                $"{data.Day}/{data.Month}/{data.Year}",
                pm.Female ? "Campea" : "Campeao",
                pm.Name
            };
            trofeu.Name = "[OURO] Trofeu de Arena PvP";
            trofeu.Hue = Paragon.Hue;
            pm._PlaceInBackpack(trofeu);
        }

        public TvTBattle(GenericReader reader)
			: base(reader)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(1);
		}

        public override bool AddTeam(string name, int minCapacity, int capacity, int color)
        {
            return AddTeam(new TvTTeam(this, name, minCapacity, capacity, color));
        }

        protected override void OnBattleStarted()
        {
            base.OnBattleStarted();
        }

        public override void OnTeamMemberDeath(PvPTeam team, PlayerMobile pm)
        {
            base.OnTeamMemberDeath(team, pm);
        }

        public override void OnAfterTeamMemberDeath(PvPTeam team, PlayerMobile pm)
        {
            base.OnAfterTeamMemberDeath(team, pm);
            if(!team.Members.Keys.Any(m => m.Alive))
            {
                if (team.Battle.State == PvPBattleState.Batalhando)
                {
                    team.Battle.State = PvPBattleState.Terminando;
                }
            }
        }

        protected override void OnBattlePreparing()
        {
            base.OnBattlePreparing();
            var msg = $"[Arena] {Name} esta iniciando ! Use .pvp para assistir !";
            foreach (var pl in NetState.GetOnlinePlayerMobiles())
            {
                pl.SendMessage(msg);
            }
            DiscordBot.SendMessage(":crossed_swords:" + msg);
        }

        protected override void OnQueueJoin(PlayerMobile pm, PvPTeam team)
        {
            base.OnQueueJoin(pm, team);
            var msg = $"[Arena] {pm.Name} entrou na fila para arena {Name}";
            foreach(var pl in NetState.GetOnlinePlayerMobiles())
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

        public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var v = reader.GetVersion();

			if (v < 1)
			{
				RewardTeam = true;
			}
		}
	}
}
