using System;
using System.Collections.Generic;

namespace ConnectFourGame.Services
{
    public class GameStateService
    {
        public const int Columns = 7;
        public const int Rows = 6;

        private readonly byte[,] _board = new byte[Rows, Columns];

        public byte PlayerTurn { get; private set; } = 1;  // 1 or 2
        public int CurrentTurn { get; private set; } = 0;   // 0..42

        public enum WinState
        {
            None,
            Player1_Wins,
            Player2_Wins,
            Tie
        }

        public WinState LastWinState { get; private set; } = WinState.None;

        // --- Win tracking ---
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

        public void ResetBoard()
        {
            Array.Clear(_board, 0, _board.Length);
            PlayerTurn = 1;
            CurrentTurn = 0;
            LastWinState = WinState.None;
        }

        public void ResetStats()
        {
            Player1Wins = 0;
            Player2Wins = 0;
            GameHistory.Clear();
            NotifyStateChanged();
        }

        /// <summary>
        /// Plays a piece in the given column (0-based). Returns landing row (1–6) for CSS.
        /// </summary>
        public byte PlayPiece(byte col)
        {
            if (col < 0 || col >= Columns)
                throw new ArgumentException("Invalid column.", nameof(col));

            if (LastWinState == WinState.Player1_Wins || LastWinState == WinState.Player2_Wins)
                throw new InvalidOperationException("Game is already over. Reset to start a new game.");

            // find lowest empty row in this column
            for (byte row = 0; row < Rows; row++)
            {
                if (_board[row, col] == 0)
                {
                    _board[row, col] = PlayerTurn;
                    CurrentTurn++;
                    return (byte)(row + 1); // we use 1..6 in CSS drop classes
                }
            }

            throw new ArgumentException("That column is full. Please choose another column.");
        }

        public void TogglePlayer()
        {
            PlayerTurn = (byte)(PlayerTurn == 1 ? 2 : 1);
        }

        public WinState CheckForWin()
        {
            var win = CalculateWinInternal();

            if (win == WinState.Player1_Wins || win == WinState.Player2_Wins)
            {
                // only record once
                if (LastWinState != win)
                {
                    if (win == WinState.Player1_Wins) Player1Wins++;
                    if (win == WinState.Player2_Wins) Player2Wins++;

                    GameHistory.Add(new GameResult
                    {
                        Winner = win == WinState.Player1_Wins ? 1 : 2,
                        Timestamp = DateTime.Now
                    });

                    NotifyStateChanged();
                }
            }

            LastWinState = win;
            return win;
        }

        private WinState CalculateWinInternal()
        {
            // directions: right, up, diag-up-right, diag-up-left
            int[,] directions = { { 1, 0 }, { 0, 1 }, { 1, 1 }, { 1, -1 } };

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    var player = _board[row, col];
                    if (player == 0) continue;

                    foreach (var dir in directions)
                    {
                        if (HasFourFrom(row, col, dir[0], dir[1], player))
                        {
                            return player == 1
                                ? WinState.Player1_Wins
                                : WinState.Player2_Wins;
                        }
                    }
                }
            }

            if (CurrentTurn >= Rows * Columns)
                return WinState.Tie;

            return WinState.None;
        }

        private bool HasFourFrom(int row, int col, int dRow, int dCol, byte player)
        {
            for (int i = 1; i < 4; i++)
            {
                int r = row + dRow * i;
                int c = col + dCol * i;

                if (r < 0 || r >= Rows || c < 0 || c >= Columns)
                    return false;

                if (_board[r, c] != player)
                    return false;
            }

            return true;
        }
    }
}
