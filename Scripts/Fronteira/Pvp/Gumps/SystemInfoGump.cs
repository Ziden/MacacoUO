using System;
using System.Collections.Generic;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
using Server.Gumps;

namespace Server.TournamentSystem
{
    public class SystemInfoGump : BaseTournamentGump
    {
        private PVPTournamentSystem System;

        public SystemInfoGump(PlayerMobile user, PVPTournamentSystem sys)
            : base(user, 20, 20)
        {
            System = sys;
        }

        public override void AddGumpLayout()
        {
            AddBackground(0, 0, 600, 450, 5120);

            AddHtml(0, 15, 560, 20, ColorAndCenter("#FFFFFF", "PVP Arena Info"), false, false);
            AddHtml(20, 40, 560, 60, m_TournamentInfo1, true, false);
            AddHtml(20, 101, 560, 340, m_TournamentInfo2, true, true);

            AddButton(20, 410, 0xFAE, 0xFB0, 1, GumpButtonType.Reply, 0);
        }

        private string m_TournamentInfo1
        {
            get
            {
                return "Welcome to the PVP Tournament System Info Dipslay. This will briefly cover the basic player functions " +
                       "of the PVP Tournament System. All arena functions can be initiated at the arena stone located at each arena.";
            }
        }

        private string m_TournamentInfo2
        {
            get
            {
                return "<br><B>Fight Teams - </B>In order to be eligable to participate in arena fights or tournaments, you " +
                       "first register a fight team.  They can be single teams, two player teams, or four player teams. " +
                       "The system will keep track of each teams stats, to include arena wins, losses, tournament wins and tournament championships.<br>" +
                       "<B>Arena Fights - </B>Once you establish an arena team, you can now challenge or be challenged to arena duels. " +
                       "Both teams must have the same team type in regards to amount of players, and both must agree on the rules " +
                       "set forth by the challenger. <br>" +
                       "<b>Fight Types - </b><br>" +
                       "  <u>Single Elimination</u> - Fight ends when all players from one team are dead.<br>" +
                       "  <u>Double Elimination</u> - Best out of 3 fights determine the winner.<br>" +
                       "  <u>Last Man Standing</u> - Free for all, last person alive wins!<br>" +
                       "  <u>Capture the Flag</u> - Only at selected Arena's, earn points by capturing and defending your enemies flag.<br>" +
                       "<b>Fight Wager - </b>The challenging team can choose a wager in gold. This wager applies to <b>each</b> fighter in each team, and will be automatically " +
                       "deducted from their account once the fight is agreed upon.<br>" +
                       "<B>Fight Rules - </B><br>" +
                       "  <u>No Precasting</u> - No casting any type of spells during the \"Wall\" period.<br>" +
                       "  <u>No Summons</u> - No Summoning creatures<br>" +
                       "  <u>No Consumables</u> - No using consumeable items, such as potions, orange petals, etc.<br>" +
                       "  <u>No Specials</u> - No using weapon special moves.<br>" +
                       "  <u>Pure Mage</u> - No secondary casting spells, such as chivalry, ninjitsu, spellweaving, bushido, and necromancy.<br>" +
                       "  <u>Pure Dexxer</u> - No casting spells at all, including Magery.<br>" +
                       "  <u>Allow Resurrections</u> - Players can resurrect team mates during the course of the arena fight.<br>" +
                       "  <u>Allow Mounts</u> - Players can utilize mounts during the course of the arena fight.<br>" +
                       "  <u>No Ties</u> - Ties are not allowed. See 'Tie Breakers' below.<br>" +
                       "  <u>No Area Spells</u> - No casting area effect spells such as wither, earthquake, etc.<br>" +
                       "<b>Tournaments - </b>Tournaments can be set up by staff or players. They are a series of pre-designed fights " +
                       "that will have a tounament champion and runner up. The tournament creator can elect to establish a tournament entry fee. " +
                       "A player can only have one tournament registered at a time, and only one tournament can be scheduled per day, and at least 12 hours apart, to avoid scheduling conflicts.<br>" +
                       "<b>Entry Fee and Tournament Pot</b> - Like a wager, entry fees are collected from each team member, once that team joins the tournament. " +
                       "At the conclusion of the tournament, the winning team will take half the pot, the runner up with take a quarter, and the " +
                       "tournament creator, if applicable, will take the other quarter. The pot also includes any gold contributed by any sponsors.<br>" +
                       "<b>Tournament Style</b> - Picking one of these two conditions automatically add other rules.<br>" +
                       "  <u>Mage Only</u> - Only magery based characters can join the tournament. <br>" +
                       "  <u>Dexxer Only</u> - Only non-spell casting characters can join the tournament. <br>" +
                       "<b>Tournament Type</b> - There are 4 types of tournaments, each having different elimination rules.<br>" +
                       "  <u>Single Elimination</u> - The default setting, once you lose, your out of the tournament.<br>" +
                       "  <u>Double Elimination</u> - On a teams first defeat, they continue on. Once they are defeated for the second time, they are eliminated from the tournament.<br>" +
                       "  <u>Best of 3</u> - Same as single elimination, however the two teams will have a best of 3 duel to determine who moves on in the tournament.<br>" +
                       "<b>Leagues - </b>Staff can now setup leages for more drawn out, extended time based matchups<br>" +
                       "  - Leagues will consist of 4 to 20 team leagues (must be even number for matchups)<br>" +
                       "  - All arena fight types, except last man standing, are supported<br>" +
                       "  - All team sizes are supported<br>" +
                       "  - Entry fees, if applicable, refer to the fee each player pays. So, if the team size is foursome, the fee for each team will be the league fee X 4<br>" +
                       "  - During the entire league, each team will fight every other team once<br>" +
                       "  - The league is broken down in rounds. Each team will have a match in each round<br>" +
                       "  - In each round, each team will have a certain amount of time to complete their match. It will be upon the teams/players to meet up and conduct their match<br>" +
                       "  - Round matches can be done at any arena (default), or set to fight at a specific arena. Capture the Flag fights will have to be fought at one of the designated CTF arenas<br>" +
                       "  - Arena matches will be handled in the arena stone gump. The initiating team leader and their opponent will have to be in range of the stone<br>" +
                       "  - League Rankings will be actively kept as each fight ends. Tie breakers, in order, are ties, then opponenet strength of victories<br>" +
                       "  - Teams that do not complete their match in the alloted time, will both be given a tie. This will adversly effect their rankings<br>" +
                       "  - At the conclusion of the league, an elimination period will ensue. This is like the playoffs. Once a team loses a match, they are out of the elmination period<br>" +
                       "  <b>Elimination Formats:</b><br> " +
                       "    <u>Strong Vs. Weak</u> - Elmination matches are determined by best league record vs worst league record<br>" +
                       "    <u>Weak Vs Weak</u> - Elmination matches are determined by like league records <br> " +
                       "    <u>Random</u> - Elmination matches are completely random<br> " +
                       "  - Elimination rounds act the same as standard league rounds, however if a match is not complete, the round resets. No ties. " +
                       "  - Winner Pot: 75% total fees <br>" +
                       "  - Runner Up Pot: 25% total fees <br>" +
                       "<b>Tie Breakers - </b> During an arena fight, the participants can agree to have tie breakers. Tournament fights always use a tie breaker, for obvious resons. The following variables, in order, are taken into consideration for a tie breaker:<br>" +
                       "  - Who has the most players alive<br>" +
                       "  - A calculation of total damage done to their opponent<br>" +
                       "  - Random Pick, though this scenario should be extremely unlikely.<br>" +
                       "<b>Alternate Arenas</b> - " +
                       "Some tournaments, depending on the arena location, will have the option to use its fel/tram counterpart to help speed " +
                       "the tournament that have a large number of participants. This feature is disabled by default. When this is enabled, " +
                       "every other tournament fight will be automatically moved to the alternate arena. Essentially, 2 fights will happen at once.<br>" +
                       "<b>Commands - </b><br>" +
                       "<u>[MyTeams</u> displays the player any arena teams they belong to. They can toggle between " +
                       "their teams statistics and information pages<br>" +
                       "<u>[ArenaTeams</u> displays all arena team info and statistics.";
            }
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID != 0 && !User.HasGump(typeof(TournamentStoneGump)))
            {
                BaseGump.SendGump(new TournamentStoneGump(System, User));
            }
        }
    }
}
