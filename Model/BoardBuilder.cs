using System.Collections.Generic;
using System.Linq;
using Godot;
using TinyCrawl;

// adapted from PaGrom's dungeon generator
public class BoardBuilder
{
    private const int RoomTries = 100;
    private const int WindingPercent = 0;

    private int _width = 10;
    private int _height = 10;
    private Cell[,] _cells;

    private List<Rectangle> _rooms;
    private int _currentRegion;
    private int[,] _regions;

    public BoardBuilder Size(int width, int height)
    {
        _width = width;
        _height = height;
        return this;
    }

    public BoardModel Build()
    {
        _cells = new Cell[_width, _height];
        _regions = new int[_width, _height];
        _rooms = new List<Rectangle>();

        for (int i = 0; i < _width; i++)
            for (int j = 0; j < _height; j++)
                _cells[i, j] = WallCell.Instance;

        AddRooms();
        GrowMaze();
        ConnectRegions();
        RemoveDeadEnds();

        return new BoardModel(_cells);
    }

    private void AddRooms()
    {
        for (int t = 0; t < RoomTries; t++)
        {
            int size = RandomHolder.Instance.Next(2, 4) * 2 + 1;
            int rectangularity = RandomHolder.Instance.Next(0, 1 + size / 2) * 2;
            int height = size;
            int width = size;

            if (RandomHolder.Instance.Next(2) == 1)
            {
                width += rectangularity;
            }
            else
            {
                height += rectangularity;
            }

            int x = RandomHolder.Instance.Next((_width - width) / 2) * 2 + 1;
            int y = RandomHolder.Instance.Next((_height - height) / 2) * 2 + 1;

            var room = new Rectangle(x, y, width, height);

            // min distance > 0
            if (IsRoomOverlapped(new Rectangle(x - 1, y - 1, width + 2, height + 2)))
            {
                continue;
            }

            _rooms.Add(room);

            StartRegion();

            for (int i = y; i < y + height; i++)
            {
                for (int j = x; j < x + width; j++)
                {
                    Carve(j, i);
                }
            }
        }
    }

    private void GrowMaze()
    {
        for (int y = 1; y < _height - 1; y += 2)
        {
            for (int x = 1; x < _width - 1; x += 2)
            {
                if (_cells[x, y].IsPassable())
                {
                    continue;
                }

                GrowingTree(x, y);
            }
        }
    }

    private void GrowingTree(int startX, int startY)
    {
        var cells = new List<Point>();
        var lastDir = new Point(0, 0);

        StartRegion();
        Carve(startX, startY);

        cells.Add(new Point(startX, startY));

        while (cells.Any())
        {
            var cell = cells.Last();
            var unmadeCells = new List<Point>();
            foreach (var direction in Directions.Cardinal)
            {
                if (CanCarve(cell, direction))
                {
                    unmadeCells.Add(direction);
                }
            }

            if (unmadeCells.Any())
            {
                Point dir;

                if (unmadeCells.Contains(lastDir) && RandomHolder.Instance.Next(100) > WindingPercent)
                {
                    dir = lastDir;
                }
                else
                {
                    int i = RandomHolder.Instance.Next(unmadeCells.Count);
                    dir = unmadeCells[i];
                }

                Carve(cell.Add(dir));
                var nextCell = cell.Add(new Point(dir.X * 2, dir.Y * 2));
                Carve(nextCell);

                cells.Add(nextCell);
                lastDir = dir;
            }
            else
            {
                cells.RemoveAt(cells.Count - 1);
                lastDir = Point.Zero;
            }
        }
    }

    private void ConnectRegions()
    {
        var connectorRegions = new Dictionary<Point, HashSet<int>>();

        for (int y = 1; y < _height - 1; y++)
        {
            for (int x = 1; x < _width - 1; x++)
            {
                if (_cells[x, y].IsPassable())
                {
                    continue;
                }

                var reg = new HashSet<int>();
                foreach (var dir in Directions.Cardinal)
                {
                    var rCoord = new Point(x, y).Add(dir);
                    int region = _regions[rCoord.X, rCoord.Y];
                    if (region != 0)
                    {
                        reg.Add(region);
                    }
                }

                if (reg.Count < 2)
                {
                    continue;
                }

                connectorRegions[new Point(x, y)] = reg;
            }
        }

        var connectors = connectorRegions.Keys.ToList();

        var openRegions = new HashSet<int>();
        var merged = new Dictionary<int, int>();
        for (int i = 0; i <= _currentRegion; i++)
        {
            merged[i] = i;
            openRegions.Add(i);
        }


        while (openRegions.Count > 1 && connectors.Any())
        {
            var connector = connectors[RandomHolder.Instance.Next(connectors.Count - 1)];
            Carve(connector, false);
            var regions = connectorRegions[connector].Select(region => merged[region]).ToList();
            int dest = regions.First();
            var sources = regions.Skip(1).ToList();

            for (int i = 0; i <= _currentRegion; i++)
            {
                if (sources.Contains(merged[i]))
                {
                    merged[i] = dest;
                }
            }

            foreach (int i in sources)
            {
                openRegions.Remove(i);
            }

            var connectorsToRemove = new HashSet<Point>();
            foreach (var c in connectors)
            {
                if (connector.ToVector2().DistanceTo(c.ToVector2()) < 2)
                {
                    connectorsToRemove.Add(c);
                }

                var regs = new HashSet<int>(connectorRegions[c].Select(region => merged[region]).ToList());
                if (regs.Count > 1)
                {
                    continue;
                }

                connectorsToRemove.Add(c);
            }

            foreach (var c in connectorsToRemove)
            {
                connectors.Remove(c);
            }
        }
    }

    private void RemoveDeadEnds()
    {
        bool done = false;
        while (!done)
        {
            done = true;
            for (var y = 1; y < _height - 1; y++)
            {
                for (var x = 1; x < _width - 1; x++)
                {
                    if (_cells[x, y].IsPassable())
                    {
                        int exits = 0;

                        foreach (var dir in Directions.Cardinal)
                        {
                            var pos = new Point(x, y).Add(dir);
                            if (_cells[pos.X, pos.Y].IsPassable())
                            {
                                exits++;
                            }
                        }

                        if (exits == 1)
                        {
                            done = false;
                            Uncarve(x, y);
                        }
                    }
                }
            }
        }
    }

    private void StartRegion()
    {
        _currentRegion++;
    }

    private void Carve(Point pos, bool setRegion = true)
    {
        Carve(pos.X, pos.Y, setRegion);
    }

    private void Carve(int x, int y, bool setRegion = true)
    {
        _cells[x, y] = FloorCell.Instance;
        if (setRegion)
        {
            _regions[x, y] = _currentRegion;
        }
    }

    private void Uncarve(int x, int y)
    {
        _cells[x, y] = WallCell.Instance;
    }

    private bool CanCarve(Point position, Point direction)
    {
        var point = position.Add(new Point(direction.X * 3, direction.Y * 3));

        if (point.X < 0 || point.Y < 0 || point.X > _width -1 || point.Y > _height - 1)
        {
            return false;
        }

        var dest = position.Add(new Point(direction.X * 2, direction.Y * 2));

        return !_cells[dest.X, dest.Y].IsPassable();
    }

    private bool IsRoomOverlapped(Rectangle room)
    {
        foreach (var readyRoom in _rooms)
        {
            var readyRect = new Rectangle(
                readyRoom.X - 1,
                readyRoom.Y - 1,
                readyRoom.Width + 2,
                readyRoom.Height + 2);
            if (room.Intersects(readyRect))
            {
                return true;
            }
        }

        return false;
    }

    private class Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public readonly int X;
        public readonly int Y;

        public Point Add(Point point)
        {
            return new Point(X + point.X, Y + point.Y);
        }

        public static Point Zero => new Point(0, 0);

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
    }

    private class Rectangle
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(Point point)
        {
            return point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
        }

        // TODO
        public bool Intersects(Rectangle rect)
        {
            return Contains(new Point(rect.X, rect.Y))
                   || Contains(new Point(rect.X + rect.Width, rect.Y))
                   || Contains(new Point(rect.X, rect.Y + rect.Height))
                   || Contains(new Point(rect.X + rect.Width, rect.Y + rect.Height));
        }
    }

    private static class Directions
    {
        private static readonly Point N = new Point(0, -1);

        private static readonly Point E = new Point(1, 0);

        private static readonly Point S = new Point(0, 1);

        private static readonly Point W = new Point(-1, 0);

        public static IEnumerable<Point> Cardinal
        {
            get
            {
                yield return N;
                yield return E;
                yield return S;
                yield return W;
            }
        }
    }
}