public class FloorCell : Cell
{
    public static readonly Cell Instance = new FloorCell();

    public override bool IsPassable() => true;
}