using System.Collections.Generic;

namespace LCRGame.Models;

public class GameResult
{
    public List<Game> Games { get; set; }

    public int ShortestGame { get; set; }

    public int LongestGame { get; set; }

    public double AverageGame { get; set; }

    public int WinnerId { get; set; }

    public int GamesWonByWinner { get; set; }

    public double WinningPercentage { get; set; }
}