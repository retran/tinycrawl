using TinyCrawl;

public class Goblin : Character
{
    public void MakeTurn()
    {
        bool flag = false;

        while (!flag)
        {
            var direction = RandomHolder.Instance.Next(4);

            switch (direction)
            {
                case 0:
                    flag = TryMoveNorth();
                    break;
                case 1:
                    flag = TryMoveEast();
                    break;
                case 2:
                    flag = TryMoveWest();
                    break;
                case 3:
                    flag = TryMoveSouth();
                    break;
                default:
                    flag = true;
                    break;
            }
        }
    }
}
