using System.Diagnostics;
using Godot;

public class Character : Node2D
{
    private Sprite _sprite;
    private BoardModel _boardModel;

    public int X { get; private set; }
    public int Y { get; private set; }
    
    public override void _Ready()
    {
        _sprite = GetNode<Sprite>("View");
    }

    public void Inject(BoardModel boardModel)
    {
        _boardModel = boardModel;
    }
    
    public bool TryMoveNorth()
    {
        return MoveTo(X, Y - 1);
    }

    public bool TryMoveSouth()
    {
        return MoveTo(X, Y + 1);
    }

    public bool TryMoveWest()
    {
        return MoveTo(X - 1, Y);
    }

    public bool TryMoveEast()
    {
        return MoveTo(X + 1, Y);
    }
    
    private bool MoveTo(int x, int y)
    {
        Debug.Assert(_boardModel != null);
        
        if (x < 0 || x >= _boardModel.Width || y < 0 || y >= _boardModel.Height)
            return false;

        if (!_boardModel.GetCell(x, y).IsPassable())
            return false;
        
        if (x < X)
        {
            _sprite.FlipH = true;
        }

        if (x > X)
        {
            _sprite.FlipH = false;
        }
        
        Place(x, y);
        return true;
    }
    
    public void Place(int x, int y)
    {
        X = x;
        Y = y;

        SetPosition(new Vector2(x * 8, y * 8));
    }
}