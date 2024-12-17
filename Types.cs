using FastConsole;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using Pos = (int x, int y);

record PosDir(int x, int y, char dir) { }

//Part 2
public class WideGameState(char[][] _map, string _moves)
{
    private Pos _bot;

    public static WideGameState Read(string input)
    {
        var inputSplit = input.Split("\n\n");
        var moves = inputSplit[1];
        var map = inputSplit[0]
            .Split("\n")
            .Select(
            x =>
                x.SelectMany<char, char>(
                    x => x switch
                    {
                        'O' => ['[', ']'],
                        '@' => ['@', '.'],
                        _ => [x, x],
                    })
                .ToArray())
            .ToArray();

        var state = new WideGameState(map, moves.Replace("\n", ""));
        state._bot = state.Find('@');

        return state;
    }

    public void Run()
    {
        Draw();
        var i = 0;
        foreach (var move in _moves)
        {
            var axis = move switch
            {
                '^' => (0, -1),
                '<' => (-1, 0),
                '>' => (1, 0),
                'v' => (0, 1),
                _ => throw new ArgumentException("o"),
            };

            TryMove(axis);
            Draw();
            i++;
        }
    }

    public void Play()
    {
        while (true)
        {
            var k = Console.ReadKey().KeyChar;
            if(k == 'q')
            {
                return;
            }

            var axis = k switch
            {
                'w' => (0, -1),
                'a' => (-1, 0),
                'd' => (1, 0),
                's' => (0, 1),
                _ => (0,0),
            };

            TryMove(axis);
            Draw();

            Thread.Sleep(20);
        }
    }

    public int CalcGps()
    {
        var result = 0;
        for (int i = 0; i < _map.Length; i++)
        {
            for (int j = 0; j < _map[i].Length; j++)
            {
                if (_map[i][j] == '[')
                {
                    result += i * 100 + j;
                }
            }
        }

        return result;
    }

    private void TryMove(Pos axis)
    {
        if (ItemAt(axis) == '.')
        {
            Move(axis);
            return;
        }

        if (ItemAt(axis) == '#')
        {
            return;
        }

        if (ItemAt(axis) is '[' or ']')
        {
            if (axis.y == 0) // horizontal
            {
                for (int i = 3; ItemAt(axis, i) != '#'; i++)
                {
                    if (ItemAt(axis, i) == '.')
                    {
                        for (; i >= 2; i--)
                        {
                            var newPos = CalcPos(axis, i);
                            var boxPos = CalcPos(axis, i - 1);

                            Set(newPos, ItemAt(axis, i - 1));
                            Set(boxPos, '.');
                        }

                        Move(axis);
                        return;
                    }
                }
            }
            if (axis.x == 0) // vertical
            {
                var area = GetBoxArea(CalcPos(axis), axis);

                if (MoveArea(area, axis))
                {
                    Move(axis);
                }
            }
        }
    }

    private bool MoveArea(HashSet<Pos> area, Pos axis)
    {
        var cellsToMove = area.OrderByDescending(p => p.y * axis.y);
        foreach (var cell in cellsToMove)
        {
            var newPos = CalcPos(axis, 1, cell);
            if(ItemAtPos(newPos) == '#')
            {
                return false;
            }
        }

        foreach (var cell in cellsToMove)
        {
            var newPos = CalcPos(axis, 1, cell);
            var boxPos = cell;

            Set(newPos, ItemAtPos(cell));
            Set(boxPos, '.');
        }

        return true;
    }

    private HashSet<Pos> GetBoxArea(Pos pos, Pos axis)
    {
        HashSet<(int, int)> area = [];

        bool MapArea(Pos p, Pos offset)
        {
            var (x, y) = p;
            var curr = ItemAtPos(p);
            if (curr != '[' && curr != ']')
            {
                return false;
            }

            if (offset.x > 0 && curr == '[')
            {
                return false;
            }

            if (offset.x < 0 && curr == ']')
            {
                return false;
            }

            if (area.Contains(p))
            {
                return true;
            }

            area.Add(p);

            MapArea((x + 1, y), (1, 0));
            MapArea((x - 1, y), (-1, 0));
            MapArea((x, y + axis.y), axis);

            return true;
        }

        MapArea(pos, axis);
        return area;

    }

    private void Move(Pos axis)
    {
        var newPos = CalcPos(axis);
        var currPos = _bot;

        Set(newPos, '@');
        Set(currPos, '.');
        _bot = newPos;
    }

    private void Set(Pos pos, char c)
    {
        var (x, y) = pos;
        if (y < 0 || x < 0 || y > _map.Length || x > _map[0].Length)
        {
            return;
        }

        _map[y][x] = c;
    }

    private char ItemAt(Pos offset, int n = 1, Pos? init = null)
    {
        var pos = CalcPos(offset, n, init);
        return ItemAtPos(pos);
    }

    private char ItemAtPos(Pos pos)
    {
        var (x, y) = pos;
        if (y < 0 || x < 0 || y > _map.Length || x > _map[0].Length)
        {
            return '#';
        }

        return _map[y][x];
    }

    private Pos CalcPos(Pos offset, int n = 1, Pos? init = null)
    {
        var (x, y) = init ?? _bot;
        var (xOff, yOff) = offset;
        return (x: x + xOff * n, y: y + yOff * n);
    }

    private Pos Find(char c)
    {
        for (int i = 0; i < _map.Length; i++)
        {
            for (int j = 0; j < _map[i].Length; j++)
            {
                if (_map[i][j] == c)
                {
                    return (j, i);
                }
            }
        }

        return (-1, -1);
    }

    public void Draw()
    {
        for (int i = 0; i < _map.Length; i++)
        {
            FConsole.SetChar(
                0, i + 1, i.ToString()[0], ConsoleColor.White, ConsoleColor.Black);
        }

        for (int i = 0; i < _map[0].Length; i++)
        {
            FConsole.SetChar(
                i + 1, 0, i.ToString()[0], ConsoleColor.White, ConsoleColor.Black);
        }

        for (int i = 0; i < _map.Length; i++)
        {
            for (int j = 0; j < _map[i].Length; j++)
            {
                FConsole.SetChar(
                    j + 1, i + 1, _map[i][j], ConsoleColor.White, ConsoleColor.Black);
            }
        }

        FConsole.DrawBuffer();
    }
}

//Part 1
public class GameState(char[][] _map, string _moves)
{
    private Pos _bot;

    public static GameState Read(string input)
    {
        var inputSplit = input.Split("\n\n");
        var moves = inputSplit[1];
        var state =
            new GameState(
                inputSplit[0].Split("\n").Select(x => x.ToArray()).ToArray(),
                moves.Replace("\n",""));

        state._bot = state.Find('@');
        
        return state;
    }

    public void Run()
    {
        foreach (var move in _moves)
        {
            var axis = move switch
            {
                '^' => (0, -1),
                '<' => (-1, 0),
                '>' => (1, 0),
                'v' => (0, 1),
                _ => throw new ArgumentException("o"),
            };

            TryMove(axis);
            Draw();
        }
    }

    public int CalcGps()
    {
        var result = 0;
        for (int i = 0; i < _map.Length; i++)
        {
            for (int j = 0; j < _map[i].Length; j++)
            {
                if (_map[i][j] == 'O')
                {
                    result += i * 100 + j;
                }
            }
        }

        return result;
    }

    private void TryMove(Pos axis)
    {
        if(ItemAt(axis) == '.')
        {
            Move(axis);
            return;
        }

        if (ItemAt(axis) == '#')
        {
            return;
        }

        if(ItemAt(axis) == 'O')
        {
            for (int i = 2; ItemAt(axis, i) != '#'; i++)
            {
                if(ItemAt(axis, i) == '.')
                {
                    var newPos = CalcPos(axis, i);
                    var boxPos = CalcPos(axis);
                    
                    Set(newPos, 'O');
                    Set(boxPos, '.');

                    Move(axis);
                    return;
                }
            }
        }
    }

    private void Move(Pos axis)
    {
        var newPos = CalcPos(axis);
        var currPos = _bot;

        Set(newPos, '@');
        Set(currPos, '.');
        _bot = newPos;
    }

    private void Set(Pos pos, char c)
    {
        var (x, y) = pos;
        if(y < 0 || x < 0 || y > _map.Length || x > _map[0].Length)
        {
            return;
        }

        _map[y][x] = c;
    }

    private char ItemAt(Pos offset, int n = 1)
    {
        var (x, y) = CalcPos(offset, n);
        if(y < 0 || x < 0 || y > _map.Length || x > _map[0].Length)
        {
            return '#';
        }

        return _map[y][x];
    }

    private Pos CalcPos(Pos offset, int n = 1)
    {
        var (x, y) = _bot;
        var (xOff, yOff) = offset;
        return (x: x + xOff * n, y: y + yOff * n);
    }

    private Pos Find(char c)
    {
        for (int i = 0; i < _map.Length; i++)
        {
            for (int j = 0; j < _map[i].Length; j++)
            {
                if (_map[i][j] == c)
                {
                    return (j, i);
                }
            }
        }

        return (-1, -1);
    }

    public void Draw()
    {
        for (int i = 0; i < _map.Length; i++)
        {
            for (int j = 0; j < _map[i].Length; j++)
            {
                FConsole.SetChar(
                    j, i, _map[i][j], ConsoleColor.White, ConsoleColor.Black);
            }
        }

        FConsole.DrawBuffer();
    }
}

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
