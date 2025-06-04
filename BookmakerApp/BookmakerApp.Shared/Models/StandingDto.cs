using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookmakerApp.Shared.Models;
public class StandingDto
{
    public int Rank { get; set; }
    public string TeamName { get; set; }
    public string Logo { get; set; }
    public int Points { get; set; }
    public int Played { get; set; }
    public int Win { get; set; }
    public int Draw { get; set; }
    public int Lose { get; set; }
    public string Goals { get; set; }
}
