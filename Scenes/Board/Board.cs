using Godot;

public class Board : TileMap
{
    private BoardModel _model;

    public void Inject(BoardModel boardModel)
    {
        _model = boardModel;
    }
	
    public void Invalidate()
    {
        for (int i = 0; i < _model.Width; i++)
            for (int j = 0; j < _model.Height; j++)
                Invalidate(i, j, _model.GetCell(i, j));
    }

    public void Invalidate(int x, int y, Cell cell)
    {
        switch (cell)
        {
            case WallCell wallCell:
                SetCell(x, y, 0);
                break;

            case FloorCell wallCell:
                SetCell(x, y, 1);
                break;
        }
    }
}
