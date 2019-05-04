using System;
using TinyCrawl;

public class BoardModel
{
    private readonly Cell[,] _cells;

    public BoardModel(Cell[,] cells)
    {
        _cells = cells;
    }

    public int Width => _cells.GetLength(0);

    public int Height => _cells.GetLength(1);

    public Cell GetCell(int x, int y)
    {
        return _cells[x, y];
    }
    
    public void FindFreeCell(out int x, out int y)
    {
        Random random = RandomHolder.Instance;
        while (true)
        {
            x = random.Next(Width);
            y = random.Next(Height);
            if (GetCell(x, y).IsPassable())
                break;
        }
    }

}