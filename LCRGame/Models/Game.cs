namespace LCRGame.Models;

public class Game
{
    public int Id { get; set; }

    public int Chips { get; set; }

    public int Turns { get; set; }

    public int WinnerId { get; set; }
}