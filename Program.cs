// See https://aka.ms/new-console-template for more information
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

var text = File.ReadAllLines(@"C:\Users\Hemant Hari\source\repos\aoc2024\day6.txt");

var testText = @"....#.....
.........#
..........
..#.......
.......#..
..........
.#..^.....
........#.
#.........
......#...".Split("\r\n");

string[] input = text!;

TraverseWithLoopDetection(input.Select(x => x.ToArray()).ToArray());

static void Day6a(string[] input)
{
    var positions = input.Select(x => x.ToArray()).ToArray();

    var height = input.Length;
    var width = input[0].Length;

    var pos = GetInitialPos();
    var direction = "up";

    bool InBounds() =>
        pos.y < height && pos.y >= 0
           && pos.x < width && pos.x >= 0;

    while (InBounds())
    {
        var nextBlock = NextBlock();

        // rotate
        if (nextBlock == '#')
        {
            direction = direction switch
            {
                "up" => "right",
                "right" => "down",
                "down" => "left",
                "left" => "up",
                _ => null
            };
        }

        //mark as visited
        positions[pos.y][pos.x] = 'X';

        // move
        pos = direction switch
        {
            "up" => (pos.y - 1, pos.x),
            "right" => (pos.y, pos.x + 1),
            "down" => (pos.y + 1, pos.x),
            "left" => (pos.y, pos.x - 1),
            _ => throw new Exception()
        };
    }

    Console.WriteLine(positions.SelectMany(x => x).Count(x => x == 'X'));

    char NextBlock()
    {
        var (y, x) = pos;
        switch (direction)
        {
            case "up" when y > 0:
                y--;
                break;
            case "right" when x < width - 1:
                x++;
                break;
            case "down" when y < height - 1:
                y++;
                break;
            case "left" when x > 0:
                x--;
                break;
            default:
                return '.';
        }

        return input[y][x];
    }

    (int y, int x) GetInitialPos()
    {
        var (y, match) = input.Index().First(x => x.Item.IndexOf('^') != -1);
        var x = match.IndexOf('^');
        return (y, x);
    }
}

static bool TraverseWithLoopDetection(char[][] input)
{
    var positions = input.Select(x => x.ToArray()).ToArray();

    var height = input.Length;
    var width = input[0].Length;

    var pos = GetInitialPos();
    var direction = 'u';

    bool InBounds() =>
        pos.y < height && pos.y >= 0
           && pos.x < width && pos.x >= 0;

    while (InBounds())
    {

        var nextBlock = NextBlock();

        // rotate
        if (nextBlock == '#')
        {
            direction = direction switch
            {
                'u' => 'r',
                'r' => 'd',
                'd' => 'l',
                'l' => 'u',
                _ => '0'
            };
        }

        //mark as visited
        positions[pos.y][pos.x] = direction;

        // move
        pos = direction switch
        {
            'u' => (pos.y - 1, pos.x),
            'r' => (pos.y, pos.x + 1),
            'd' => (pos.y + 1, pos.x),
            'l' => (pos.y, pos.x - 1),
            _ => throw new Exception()
        };
    }

    Console.WriteLine(positions.SelectMany(x => x).Count(x => x is 'u' or 'r' or 'd' or 'l'));

    return true;

    char NextBlock()
    {
        var (y, x) = pos;
        switch (direction)
        {
            case 'u' when y > 0:
                y--;
                break;
            case 'r' when x < width - 1:
                x++;
                break;
            case 'd' when y < height - 1:
                y++;
                break;
            case 'l' when x > 0:
                x--;
                break;
            default:
                return '.';
        }

        return input[y][x];
    }

    (int y, int x) GetInitialPos()
    {
        var (y, match) = input.Index().First(x => x.Item.Contains('^'));
        var x = match.ToList().IndexOf('^');
        return (y, x);
    }
}


    #region completed

    static void Day1a(string[] lines)
{
    var items = lines.Select(l => l.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    var items1 = items.Select(l => int.Parse(l[0])).Order();
    var items2 = items.Select(l => int.Parse(l[1])).Order();

    Console.WriteLine(items1.Zip(items2, (a, b) => Math.Abs(a - b)).Sum());
}

static void Day1b(string[] lines)
{
    var items = lines.Select(l => l.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    var items1 = items.Select(l => int.Parse(l[0]));
    var items2 = items.Select(l => int.Parse(l[1])).GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

    Console.WriteLine(items1.Select(x => x * (items2.TryGetValue(x, out var y) ? y : 0)).Sum());
}

static void Day2a(string[] input)
{
    Console.WriteLine(input.Select(ProcessReport).Sum());

    static int ProcessReport(string line)
    {
        var nums = line.Split(' ').Select(int.Parse).ToArray();

        var curr = nums[0];
        var increment = (nums[1] - nums[0]) > 0;
        foreach (var num in nums[1..])
        {
            var diff = curr - num;
            if (Math.Abs(diff) > 3 || diff == 0 || diff > 0 == increment)
            {
                return 0;
            }

            curr = num;
        }

        return 1;
    }
}

static void Day2b(string[] input)
{
    Console.WriteLine(input.Select(ProcessReport).Sum());

    static int ProcessReport(string line)
    {
        var nums = line.Split(' ').Select(int.Parse).ToImmutableList();

        if (ProcessReportArray(nums.ToArray()))
        {
            return 1;
        }

        for (int i = 0; i < nums.Count; i++)
        {
            if (ProcessReportArray(nums.RemoveAt(i).ToArray()))
            {
                return 1;
            }
        }

        return 0;
    }

    static bool ProcessReportArray(int[] nums)
    {
        //Console.WriteLine(string.Join(' ', nums));
        var curr = nums[0];
        var increment = (nums[1] - nums[0]) > 0;
        foreach (var num in nums[1..])
        {
            var diff = curr - num;
            if (Math.Abs(diff) > 3 || diff == 0 || diff > 0 == increment)
            {
                return false;
            }

            curr = num;
        }

        return true;
    }
}

static void Day3a(string input)
{
    Regex re = new(@"mul\(([0-9]*?),([0-9]*?)\)");

    var result = re.Matches(input)
      .Select(x => int.Parse(x.Groups[1].Value) * int.Parse(x.Groups[2].Value))
      .Sum();

    Console.WriteLine(result);
}

static void Day3b(string input)
{
    Regex re = new(@"don't\(\)|do\(\)|mul\(([0-9]*?),([0-9]*?)\)");

    MatchCollection matches = re.Matches(input);

    var muls = new List<Match>();
    var doMul = true;
    foreach (Match match in matches)
    {
        if (match.Value.StartsWith("don't"))
        {
            doMul = false;
        }
        else if (match.Value.StartsWith("do"))
        {
            doMul = true;
        }
        else if (doMul)
        {
            muls.Add(match);
        }
    }


    var result = muls
          .Select(x => int.Parse(x.Groups[1].Value) * int.Parse(x.Groups[2].Value))
          .Sum();

    Console.WriteLine(result);
}

static void Day4b(string[] input)
{
    int count = 0;

    for (int i = 1; i < input.Length - 1; i++)
    {
        for (int j = 1; j < input[i].Length - 1; j++)
        {
            char topLeft = input[i - 1][j - 1];
            char topRight = input[i - 1][j + 1];
            char bottomLeft = input[i + 1][j - 1];
            char bottomRight = input[i + 1][j + 1];

            if (input[i][j] == 'A')
            {
                if ((topLeft == 'S' && bottomRight == 'M') || (topLeft == 'M' && bottomRight == 'S'))
                {
                    if ((topRight == 'S' && bottomLeft == 'M') || (topRight == 'M' && bottomLeft == 'S'))
                    {
                        count++;
                    }
                }
            }
        }
    }

    Console.WriteLine(count);
}

static void Day4a(string[] input)
{
    Regex reForward = new Regex("XMAS");
    Regex reBackward = new Regex("SAMX");

    var horizontal = input;
    var vertical = GetVertical();
    var diagonal1 = GetDiagonals(input);
    var diagonal2 = GetDiagonals(input.Reverse().ToArray());

    var result = input.Concat(vertical)
        .Concat(diagonal1)
        .Concat(diagonal2)
        .Select(x =>
        {
            return reForward.Matches(x).Count + reBackward.Matches(x).Count;
        })
        .Sum();

    Console.WriteLine(result);

    IEnumerable<string> GetVertical()
    {
        for (int i = 0; i < input[0].Length; i++)
        {
            yield return new string(input.Select(x => x[i]).ToArray());
        }
    }

    IEnumerable<string> GetDiagonals(string[] input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            List<char> diagonal = new();
            for (int j = 0; j < Math.Min(i + 1, Math.Min(input.Length, input[0].Length)); j++)
            {
                diagonal.Add(input[i - j][j]);
            }

            yield return new string(diagonal.ToArray());
        }

        for (int i = 0; i < input[0].Length - 1; i++)
        {
            List<char> diagonal = new();
            for (int j = 0; j < Math.Min(i + 1, Math.Min(input.Length, input[0].Length)); j++)
            {
                diagonal.Add(input[^(i - j + 1)][^(j + 1)]);
            }

            yield return new string(diagonal.ToArray());
        }
    }
}

static void Day5(string text)
{
    var orderAndPages = text.Split("\n\n");

    Dictionary<int, HashSet<int>> orderingGraphForward =
        orderAndPages[0]
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Split("|"))
            .GroupBy(x => int.Parse(x[0]))
            .ToDictionary(x => x.Key, x => x.Select(x => int.Parse(x[1])).ToHashSet());

    Dictionary<int, HashSet<int>> orderingGraphBackward =
        orderAndPages[0]
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Split("|"))
            .GroupBy(x => int.Parse(x[1]))
            .ToDictionary(x => x.Key, x => x.Select(x => int.Parse(x[0])).ToHashSet());

    IEnumerable<int[]> pages =
        orderAndPages[1]
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Split(",").Select(int.Parse).ToArray());

    var fixedPages = pages.Where(x => GetMiddlePage(x) == 0)
        .Select(x => x.Order(new PageCompararer(orderingGraphForward, orderingGraphBackward)).ToArray())
        .Select(GetMiddlePage)
        .Sum();

    Console.WriteLine(pages.Select(GetMiddlePage).Sum());
    Console.WriteLine(fixedPages);

    int GetMiddlePage(int[] pages)
    {
        for (int i = pages.Length - 1; i >= 0; i--)
        {
            var page = pages[i];
            if (!orderingGraphForward.TryGetValue(page, out var forwardItems))
            {
                continue;
            }

            if (pages[0..i].Any(forwardItems.Contains))
            {
                return 0;
            }
        }

        return pages[pages.Length / 2];
    }
}


#endregion