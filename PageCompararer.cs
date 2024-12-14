using System.Text.RegularExpressions;

record Bot(Bot.Pos pos, Bot.Vel vel)
{
    private static readonly Regex re = new(@"p=([^,]*),([^,]*) v=([^,]*),([^,]*)");

    public static (int x, int y) Size;

    public static Bot Create(string definition)
    {
        var match = re.Match(definition);
        var g = (int x) => int.Parse(match.Groups[x].Value);

        return new Bot(
            new Pos { x = g(1), y = g(2) },
            new Vel(g(3), g(4)));
    }

    public void Move()
    {
        pos.x += vel.x;
        pos.y += vel.y;

        if(pos.x < 0)
        {
            pos.x = Size.x + pos.x;
        }

        if(pos.y < 0)
        {
            pos.y = Size.y + pos.y;
        }

        pos.x %= Size.x;
        pos.y %= Size.y;
    }

    public class Pos
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public record Vel(int x, int y) { }
}

class PageCompararer(
    Dictionary<int, HashSet<int>> forward,
    Dictionary<int, HashSet<int>> backward) : IComparer<int>
{
    public int Compare(int x, int y)
    {
        forward.TryGetValue(x, out var xForward);
        if (xForward?.Contains(y) ?? false)
        {
            return -1;
        }

        backward.TryGetValue(x, out var xBackward);
        if (xBackward?.Contains(y) ?? false)
        {
            return +1;
        }

        return 0;
    }
}
