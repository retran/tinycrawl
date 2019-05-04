using Godot;

public class Session : Node
{
    private Board _board;
    private Player _player;
    private PackedScene _goblinScene;

    private Goblin[] _goblins;
    private BoardModel _boardModel;
    private Label _label;

    private int _turns = 0;
    
    public override void _Ready()
    {
        _goblinScene = ResourceLoader.Load<PackedScene>("res://Scenes/Goblin/Goblin.tscn");

        _boardModel = new BoardBuilder()
            .Size(100, 100)
            .Build();

        _label = GetNode<Label>("UILayer/TurnsCountLabel");
        
        _board = GetNode<Board>("Board");
        _board.Inject(_boardModel);
        _board.Invalidate();
        
        _player = GetNode<Player>("Player");
        _player.Inject(_boardModel);
        PlaceToRandomFreeCell(_player);

        _goblins = new Goblin[100];
        for (int i = 0; i < _goblins.Length; i++)
        {
            _goblins[i] = CreateGoblin();
            PlaceToRandomFreeCell(_goblins[i]);
        }
    }

    public override void _Input(InputEvent e)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
            GetTree().Quit();

        if (Input.IsActionJustPressed("ui_accept"))
            OS.WindowFullscreen = !OS.WindowFullscreen;

        bool hasMoved = false;

        if (Input.IsActionJustPressed("ui_up"))
            hasMoved = _player.TryMoveNorth();

        if (Input.IsActionJustPressed("ui_down"))
            hasMoved = _player.TryMoveSouth();

        if (Input.IsActionJustPressed("ui_left"))
            hasMoved = _player.TryMoveWest();

        if (Input.IsActionJustPressed("ui_right"))
            hasMoved = _player.TryMoveEast();

        if (hasMoved)
        {
            _turns++;
            _label.SetText($"Turns: {_turns}");
            CallDeferred(nameof(MoveGoblins));
        }

        base._Input(e);
    }

    private void MoveGoblins()
	{
        foreach (var goblin in _goblins)
        {
            goblin.MakeTurn();
        }
	}

    private void PlaceToRandomFreeCell(Character character)
    {
        _boardModel.FindFreeCell(out int x, out int y);
        character.Place(x, y);
    }
    
    private Goblin CreateGoblin()
    {
        var goblinInstance = _goblinScene.Instance<Goblin>();
        goblinInstance.Inject(_boardModel);
        AddChild(goblinInstance);
        return goblinInstance;
    }
}