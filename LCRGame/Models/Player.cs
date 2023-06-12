namespace LCRGame.Models;

public class Player
{
    public Player(int id, int chips)
    {
        Id = id;
        Chips = chips;
    }

    public int Id { get; }

    public int Chips { get; set; }

    public Player LeftPlayer { get; set; }

    public Player RightPlayer { get; set; }
}