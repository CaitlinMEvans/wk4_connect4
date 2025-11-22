namespace ConnectFour.Components
{
    public class GameState
    {
        static GameState()
        {
            CalculateWinningPlaces();
        }

        public enum WinState
        {
            No_Winner = 0,
            Player1_Wins = 1,
            Player2_Wins = 2,
            Tie = 3
        }

        public int PlayerTurn => TheBoard.Count(x => x != 0) % 2 + 1;

        public int CurrentTurn => TheBoard.Count(x => x != 0);

        public static readonly List<int[]> WinningPlaces = new();

        public static void CalculateWinningPlaces()
        {
            // Horizontal rows
            for (byte row = 0; row < 6; row++)
            {
                byte rowCol1 = (byte)(row * 7);
                byte rowColEnd = (byte)((row + 1) * 7 - 1);
                byte checkCol = rowCol1;
                while (checkCol <= rowColEnd - 3)
                {
                    WinningPlaces.Add(new int[]
                    {
                        checkCol,
                        (byte)(checkCol + 1),
                        (byte)(checkCol + 2),
                        (byte)(checkCol + 3)
                    });
                    checkCol++;
                }
            }

            // Vertical columns
            for (byte col = 0; col < 7; col++)
            {
                byte colRow1 = col;
                byte colRowEnd = (byte)(35 + col);
                byte checkRow = colRow1;
                while (checkRow <= 14 + col)
                {
                    WinningPlaces.Add(new int[]
                    {
                        checkRow,
                        (byte)(checkRow + 7),
                        (byte)(checkRow + 14),
                        (byte)(checkRow + 21)
                    });
                    checkRow += 7;
                }
            }

            // forward slash diagonal "/"
            for (byte col = 0; col < 4; col++)
            {
                byte colRow1 = (byte)(21 + col);
                byte colRowEnd = (byte)(35 + col);
                byte checkPos = colRow1;
                while (checkPos <= colRowEnd)
                {
                    WinningPlaces.Add(new int[]
                    {
                        checkPos,
                        (byte)(checkPos - 6),
                        (byte)(checkPos - 12),
                        (byte)(checkPos - 18)
                    });
                    checkPos += 7;
                }
            }

            // backslash diagonal "\"
            for (byte col = 0; col < 4; col++)
            {
                byte colRow1 = col;
                byte colRowEnd = (byte)(14 + col);
                byte checkPos = colRow1;
                while (checkPos <= colRowEnd)
                {
                    WinningPlaces.Add(new int[]
                    {
                        checkPos,
                        (byte)(checkPos + 8),
                        (byte)(checkPos + 16),
                        (byte)(checkPos + 24)
                    });
                    checkPos += 7;
                }
            }
        }

        public WinState CheckForWin()
        {
            if (TheBoard.Count(x => x != 0) < 7) return WinState.No_Winner;

            foreach (var scenario in WinningPlaces)
            {
                if (TheBoard[scenario[0]] == 0) continue;

                if (TheBoard[scenario[0]] ==
                    TheBoard[scenario[1]] &&
                    TheBoard[scenario[1]] ==
                    TheBoard[scenario[2]] &&
                    TheBoard[scenario[2]] ==
                    TheBoard[scenario[3]])
                    return (WinState)TheBoard[scenario[0]];
            }

            if (TheBoard.Count(x => x != 0) == 42) return WinState.Tie;

            return WinState.No_Winner;
        }

        public byte PlayPiece(int column)
        {
            if (CheckForWin() != 0) throw new ArgumentException("Game is over");

            if (TheBoard[column] != 0) throw new ArgumentException("Column is full");

            var landingSpot = column;
            for (var i = column; i < 42; i += 7)
            {
                if (landingSpot + 7 >= 42 || TheBoard[landingSpot + 7] != 0) break;
                landingSpot = i;
            }

            TheBoard[landingSpot] = PlayerTurn;

            return ConvertLandingSpotToRow(landingSpot);
        }

        public List<int> TheBoard { get; private set; } = new List<int>(new int[42]);

        public void ResetBoard()
        {
            TheBoard = new List<int>(new int[42]);
        }

        private static byte ConvertLandingSpotToRow(int landingSpot)
        {
            return (byte)(Math.Floor(landingSpot / 7m) + 1);
        }
    }
}
