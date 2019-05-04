public class WallCell : Cell
{
    public static readonly WallCell Instance = new WallCell();

    public override bool IsPassable() => false;
}