using LCRGame.Framework;

namespace LCRGame.Models;

public class GameInput
{
    public GameInput(int players, int games) => (Players, Games) = (players, games);

    public int Players { get; }

    public int Games { get; }

    public override string ToString()
    {
        if (Players == 0 && Games == 0) return Constants.CustomSettings;
        return $"{Players} players x {Games:0,0} games";
    }
}