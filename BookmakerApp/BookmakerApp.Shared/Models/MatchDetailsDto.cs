using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookmakerApp.Shared.Models;
public class MatchDetailsDto
{
    public string HomeTeam { get; set; }
    public string AwayTeam { get; set; }
    public string HomeTeamLogo { get; set; }
    public string AwayTeamLogo { get; set; }
    public int HomeGoals { get; set; }
    public int AwayGoals { get; set; }
    public List<MatchStatDto> Statistics { get; set; } = new();
}

public class MatchStatDto
{
    public string Type { get; set; }
    public string HomeValue { get; set; }
    public string AwayValue { get; set; }
}
