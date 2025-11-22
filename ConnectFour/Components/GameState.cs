using System;
using System.Collections.Generic;
using System.Linq;

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

        // ======== Board state ========

        public List<int> TheBoard { get; private set; } = new List<int>(new int[42]);

        /// <summary>
        /// The player whose turn it is. Player 1 starts.
        /// </summary>
        public int PlayerTurn => TheBoard.Count(x => x != 0) % 2 + 1;

        /// <summary>
        /// Number of pieces played so far.
        /// </summary>
        public int CurrentTurn => TheBoard.Count(x => x != 0);

        public void ResetBoard()
        {
            TheBoard = new List<int>(new int[42]);
            _currentWinState = WinState.No_Winner;
        }

        private static byte ConvertLandingSpotToRow(int landingSpot)
        {
            return (byte)(Math.Floor(landingSpot / 7m) + 1);
        }

        // ======== Win tracking across games ========

        public int Player1Wins { get; private set; }
        public int Player2Wins { get; private set; }

        public class GameResult
        {
            public int Winner { get; set; }       // 1 or 2
            public DateTime Timestamp { get; set; }
        }

        public List<GameResult> GameHistory { get; } = new();

        public event Action? OnStateChanged;

        private void NotifyStateChanged() => OnStateChanged?.Invoke();

        /// <summary>
        /// Reset accumulated statistics (wins + history), but not the current board.
        /// </summary>
        public void ResetStats()
        {
            Player1Wins = 0;
            Player2Wins = 0;
            GameHistory.Clear();
            NotifyStateChanged();
        }

        // We remember the last win we logged so we don't double-count.
        private WinState _lastLoggedWin = WinState.No_Winner;
        private WinState _currentWinState = WinState.No_Winner;

        // ======== Win detection setup ========

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

        /// <summary>
        /// Check the board for a win or tie, update stats once when a win happens.
        /// </summary>
        public WinState CheckForWin()
        {
            // Existing win detection
            if (TheBoard.Count(x => x != 0) < 7)
            {
                _currentWinState = WinState.No_Winner;
                return _currentWinState;
            }

            foreach (var scenario in WinningPlaces)
            {
                if (TheBoard[scenario[0]] == 0) continue;

                if (TheBoard[scenario[0]] ==
                    TheBoard[scenario[1]] &&
                    TheBoard[scenario[1]] ==
                    TheBoard[scenario[2]] &&
                    TheBoard[scenario[2]] ==
                    TheBoard[scenario[3]])
                {
                    _currentWinState = (WinState)TheBoard[scenario[0]];
                    break;
                }
            }

            if (_currentWinState == WinState.No_Winner &&
                TheBoard.Count(x => x != 0) == 42)
            {
                _currentWinState = WinState.Tie;
            }

            // === update stats only when a new win is detected ===
            if ((_currentWinState == WinState.Player1_Wins ||
                 _currentWinState == WinState.Player2_Wins) &&
                _currentWinState != _lastLoggedWin)
            {
                if (_currentWinState == WinState.Player1_Wins)
                {
                    Player1Wins++;
                    GameHistory.Add(new GameResult
                    {
                        Winner = 1,
                        Timestamp = DateTime.Now
                    });
                }
                else if (_currentWinState == WinState.Player2_Wins)
                {
                    Player2Wins++;
                    GameHistory.Add(new GameResult
                    {
                        Winner = 2,
                        Timestamp = DateTime.Now
                    });
                }

                _lastLoggedWin = _currentWinState;
                NotifyStateChanged();
            }

            return _currentWinState;
        }

        /// <summary>
        /// Takes the current turn and places a piece in the 0-indexed column requested.
        /// </summary>
        /// <param name="column">0-indexed column to place the piece into</param>
        /// <returns>The final row (1–6) where the piece resides, for CSS drop animation.</returns>
        public byte PlayPiece(int column)
        {
            // Prevent playing after game is over
            if (_currentWinState == WinState.Player1_Wins ||
                _currentWinState == WinState.Player2_Wins ||
                _currentWinState == WinState.Tie)
            {
                throw new ArgumentException("Game is over");
            }

            if (column < 0 || column >= 7)
                throw new ArgumentException("Invalid column");

            // Check the column
            if (TheBoard[column] != 0)
                throw new ArgumentException("Column is full");

            // Drop the piece in
            var landingSpot = column;
            for (var i = column; i < 42; i += 7)
            {
                if (landingSpot + 7 >= 42 || TheBoard[landingSpot + 7] != 0)
                    break;
                landingSpot = i;
            }

            TheBoard[landingSpot] = PlayerTurn;

            return ConvertLandingSpotToRow(landingSpot);
        }
    }
}
