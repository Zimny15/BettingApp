namespace BookmakerApp.Models;

public class Odds
{
}

public class TeamStats
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int RecentFormPoints { get; set; }
    public int HomeAdvantage { get; set; }
    public int HeadToHeadPoints { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class MatchOdds
{
    public int Id { get; set; }
    public long MatchId { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public decimal OddsHomeWin { get; set; }
    public decimal OddsDraw { get; set; }
    public decimal OddsAwayWin { get; set; }
    public DateTime MatchDate { get; set; }
    public DateTime CalculatedAt { get; set; }
}


