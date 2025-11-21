using System;
using System.Collections.Generic;

namespace ConnectFourGame.Services
{
    public class GameResult
    {
        public int Winner { get; set; }   // 1 or 2
        public DateTime Timestamp { get; set; }
    }

    public class GameStateService
    {
        public int Player1Wins { get; private set; }
        public int Player2Wins { get; private set; }

        public List<GameResult> GameHistory { get; } = new();

        public event Action? OnStateChanged;

        public void RecordWin(int winner)
        {
            if (winner == 1)
            {
                Player1Wins++;
            }
            else if (winner == 2)
            {
                Player2Wins++;
            }

            GameHistory.Add(new GameResult
            {
                Winner = winner,
                Timestamp = DateTime.Now
            });

            OnStateChanged?.Invoke();
        }

        public void ResetStats()
        {
            Player1Wins = 0;
            Player2Wins = 0;
            GameHistory.Clear();
            OnStateChanged?.Invoke();
        }
    }
}
