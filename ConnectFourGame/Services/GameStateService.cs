namespace ConnectFourGame.Services;

public class GameStateService
{
    public int Player1Wins { get; private set; } = 0;
    public int Player2Wins { get; private set; } = 0;
    public List GameHistory { get; private set; } = new();

    public event Action? OnStateChanged;

    public void RecordWin(int player)
    {
        if (player == 1)
            Player1Wins++;
        else if (player == 2)
            Player2Wins++;

        GameHistory.Add(new GameRecord
        {
            Winner = player,
            Timestamp = DateTime.Now
        });

        NotifyStateChanged();
    }

    public void ResetStats()
    {
        Player1Wins = 0;
        Player2Wins = 0;
        GameHistory.Clear();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}

public class GameRecord
{
    public int Winner { get; set; }
    public DateTime Timestamp { get; set; }
}