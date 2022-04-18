using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;
using Server.Gumps;
using Server.Commands;

namespace Server.TournamentSystem
{
    public class BracketGump : BaseTournamentGump
    {
        public static void Initialize()
        {
            //CommandSystem.Register("BracketGump", AccessLevel.GameMaster, e =>
            CommandSystem.Register("ChavesGump", AccessLevel.GameMaster, e =>
            {
                BaseGump.SendGump(new BracketGump((PlayerMobile)e.Mobile, 16));
                BaseGump.SendGump(new BracketGump((PlayerMobile)e.Mobile, 8));
                BaseGump.SendGump(new BracketGump((PlayerMobile)e.Mobile, 4));
            });
        }

        private int Size { get; set; }
        public LeagueRecord Record { get; set; }
        public bool CloseBrackets { get; set; }

        private readonly int YOffset = 30;

        public BracketGump(PlayerMobile pm, int size)
           : base(pm, 0, 0)
        {
            Size = size;
        }

        public BracketGump(PlayerMobile pm, LeagueRecord record)
            : base(pm, 0, 0)
        {
            Record = record;
            CloseBrackets = record.ElimFormat == EliminationFormat.Random;
            Size = Record.EliminationRoundLineup[1].Count * 2;
        }

        public override void AddGumpLayout()
        {
            var length = Size == 16 ? 802 : Size == 8 ? 560 : 320;

            AddBackground(0, 0, length, 300, 3000);
            //AddHtml(0, 10, length, 20, Center(String.Format("Elimination Bracket: {0} League", Record != null ? Record.LeagueName : "Unknown")), false, false);
            AddHtml(0, 10, length, 20, Center(String.Format("Chave Eliminatória: {0} Liga", Record != null ? Record.LeagueName : "Desconhecido")), false, false);

            DrawBrackets();
        }

        private void DrawBrackets()
        {
            Dictionary<int, List<RoundMatch>> matches = Record != null ? Record.EliminationRoundLineup : null;
            var padList = Size == 16 ? Padding16 : Size == 8 ? Padding8 : Padding4;

            for (int i = 0; i < padList.Length; i++)
            {
                for (int j = 0; j < padList[i].Length; j++)
                {
                    var list = padList[i][j];

                    if (list.Length == 6 && list[5] == 666 && !CloseBrackets)
                    {
                        continue;
                    }

                    AddImageTiled(list[0], list[1] + YOffset, list[2], list[3], 9404);

                    if (matches != null && list.Length > 5 && matches.ContainsKey(list[5]))
                    {
                        var match = matches[list[5]][list[6]];
                        var team = list[7] == 0 ? match.Team1 : match.Team2;

                        AddHtml(list[0], (list[1] - 18) + YOffset, 100, 20, ColorAndCenter(String.Format("{0}", match.Winner == team ? "green" : match.Loser == team ? "red" : "black"), team.Name), false, false);

                        if (padList[i].Length == 1)
                        {
                            team = match.Team2;

                            AddHtml(list[0], list[1] + YOffset, 100, 20, ColorAndCenter(String.Format("{0}", match.Winner == team ? "green" : match.Loser == team ? "red" : "black"), team.Name), false, false);
                        }
                    }

                    if (list[4] > 0)
                    {
                        AddImageTiled(list[0] + list[4], list[1] + YOffset, list[2], list[3], 9404);

                        if (matches != null && list.Length > 5 && matches.ContainsKey(list[5]))
                        {
                            var match = matches[list[5]][list[8]];
                            var team = list[9] == 0 ? match.Team1 : match.Team2;

                            AddHtml(list[0] + list[4], (list[1] - 18) + YOffset, 100, 20, ColorAndCenter(String.Format("{0}", match.Winner == team ? "green" : match.Loser == team ? "red" : "black"), team.Name), false, false);
                        }
                    }
                }
            }
        }

        private int[][][] Padding16 =
        {
            new int[][]
            {
                // x, y, length, heigth, offsetX, round, matchIndex, teamIndex, matchIndexOffset, teamIndexOffset
                new int[] { 10, 30, 100, 2, 682, 1, 0, 0, 4, 0 },
                new int[] { 10, 55, 100, 2, 682, 1, 0, 1, 4, 1 },
                new int[] { 10, 80, 100, 2, 682, 1, 1, 0, 5, 0 },
                new int[] { 10, 105, 100, 2, 682, 1, 1, 1, 5, 1},
                new int[] { 10, 130, 100, 2, 682, 1, 2, 0, 6, 0},
                new int[] { 10, 155, 100, 2, 682, 1, 2, 1, 6, 1 },
                new int[] { 10, 180, 100, 2, 682, 1, 3, 0, 7, 0 },
                new int[] { 10, 205, 100, 2, 682, 1, 3, 1, 7, 1 },

                new int[] { 110, 30, 2, 27, 582 },
                new int[] { 110, 80, 2, 27, 582 },
                new int[] { 110, 130, 2, 27, 582 },
                new int[] { 110, 180, 2, 27, 582 },
            },

            new int[][]
            {
                new int[] { 112, 42, 20, 2, 560, 666 },
                new int[] { 112, 92, 20, 2, 560, 666},
                new int[] { 112, 142, 20, 2, 560, 666 },
                new int[] { 112, 192, 20, 2, 560, 666},
                new int[] { 132, 42, 2, 51, 540, 666 },
                new int[] { 132, 142, 2, 51, 540, 666 },
            },

            new int[][]
            {
                new int[] { 132, 55, 100, 2, 440, 2, 0, 0, 2, 0 },
                new int[] { 132, 80, 100, 2, 440, 2, 0, 1, 2, 1 },
                new int[] { 132, 155, 100, 2, 440, 2, 1, 0, 3, 0 },
                new int[] { 132, 180, 100, 2, 440, 2, 1, 1, 3, 1 },

                new int[] { 232, 55, 2, 27, 340 },
                new int[] { 232, 155, 2, 27, 340 },
            },

            new int[][]
            {
                new int[] { 234, 67, 20, 2, 320, 666 },
                new int[] { 234, 167, 20, 2, 320, 666 },
                new int[] { 254, 67, 2, 102, 300, 666 },
            },

            new int[][]
            {
                new int[] { 254, 105, 100, 2, 200, 3, 0, 0, 1, 0 },
                new int[] { 254, 130, 100, 2, 200, 3, 0, 1, 1, 1 },

                new int[] { 354, 105, 2, 27, 100, 0, 0, 0, 1 },
            },

            new int[][]
            {
                new int[] { 356, 117, 100, 2, 0, 4, 0, 0, 0, 1 }
            },
        };

        public int[][][] Padding8 =
        {
            new int[][]
            {
                new int[] { 10, 30, 100, 2, 440, 1, 0, 0, 2, 0 },
                new int[] { 10, 55, 100, 2, 440, 1, 0, 1, 2, 1 },
                new int[] { 10, 80, 100, 2, 440, 1, 1, 0, 3, 0 },
                new int[] { 10, 105, 100, 2, 440, 1, 1, 1, 3, 1 },

                 new int[] { 110, 30, 2, 27, 340 },
                 new int[] { 110, 80, 2, 27, 340 },
            },

            new int[][]
            {
                 new int[] { 112, 42, 20, 2, 320, 666 },
                 new int[] { 112, 92, 20, 2, 320, 666 },
                 new int[] { 132, 42, 2, 51, 300, 666 },
            },

            new int[][]
            {
                new int[] { 132, 55, 100, 2, 200, 2, 0, 0, 1, 0 },
                new int[] { 132, 80, 100, 2, 200, 2, 0, 1, 1, 1 },
                new int[] { 232, 55, 2, 27, 100 },
            },

            new int[][]
            {
                new int[] { 234, 67, 100, 2, 0, 3, 0, 0, 0, 1 },
            },
        };

        public int[][][] Padding4 =
        {
            new int[][]
            {
                new int[] { 10, 30, 100, 2, 200, 1, 0, 0, 1, 0 },
                new int[] { 10, 55, 100, 2, 200, 1, 0, 1, 1, 1 },

                new int[] { 110, 30, 2, 27, 100 },
            },

            new int[][]
            {
                new int[] { 110, 43, 100, 2, 0, 2, 0, 0, 0, 1 },
            },
        };
    }
}
