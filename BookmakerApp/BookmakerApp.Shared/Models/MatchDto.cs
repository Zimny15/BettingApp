﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookmakerApp.Shared.Models;
public class MatchDto
{
    public int FixtureId { get; set; }
    public DateTime Date { get; set; }
    public string LeagueName { get; set; }
    public string LeagueLogo { get; set; }

    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public string HomeTeam { get; set; }
    public string AwayTeam { get; set; }

    public string? HomeTeamLogo { get; set; }
    public string? AwayTeamLogo { get; set; }
    public string? Status { get; set; }
    public int? HomeGoals { get; set; }
    public int? AwayGoals { get; set; }
}
