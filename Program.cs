// See https://aka.ms/new-console-template for more information
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Pos = (int x, int y);

var text = File.ReadAllLines(@"C:\Users\Hemant Hari\source\repos\aoc2024\day10.txt");

var testText = @"89010123
78121874
87430965
96549874
45678903
32019012
01329801
10456732".Split("\r\n");

var input = text.Select(x => x.Select(char.GetNumericValue).ToArray()).ToArray();


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

static int? Day6a(char[][] input)
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
        var nextBlock = '.';
        var iter = 0;
        do
        {
            nextBlock = NextBlock();

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

            nextBlock = NextBlock();
            iter++;
            if (iter > 5)
            {
                return null;
            }
        } while (nextBlock == '#');

        if (positions[pos.y][pos.x] == direction)
        {
            return null;
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

    return positions.SelectMany(x => x).Count(x => x is 'u' or 'r' or 'd' or 'l');

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

static void Day6b(string[] text)
{
    var input = text!.Select(x => x.ToArray()).ToArray();

    var count = 0;
    for (int i = 0; i < input.Length; i++)
    {
        for (int j = 0; j < input[0].Length; j++)
        {
            char[][] curr = input.Select(x => (char[])x.Clone()).ToArray()!;
            ref var posToUpdate = ref curr[i][j];
            if (posToUpdate == '.')
            {
                posToUpdate = '#';
            }

            if (!Day6a(curr).HasValue)
            {
                count++;
                //Console.WriteLine(string.Join("\n", curr.Select(x => new string(x))));
                //Console.WriteLine();
            }
        }
    }

    Console.WriteLine(count);
}

static long Day7a(string line)
{
    if (line.Split(": ") is not [string totalStr, string numbersStr]) return 0;

    var total = long.Parse(totalStr);
    var numbers = numbersStr.Split(" ").Select(long.Parse).ToArray();
    Func<long, long, long>[] ops = [Mul, Add];

    return CanTotal(ImmutableList<long>.Empty) ? total : 0;

    bool CanTotal(ImmutableList<long> currOps)
    {
        if (currOps.Count() >= numbers.Length - 1)
        {
            return Calculate(currOps) == total;
        }

        foreach (long op in new long[] { 0, 1 })
        {
            if (CanTotal(currOps.Add(op)))
            {
                return true;
            }
        }

        return false;
    }

    long Calculate(ImmutableList<long> currOps)
    {
        var result = numbers[0];

        foreach (var (idx, num) in numbers[1..].Index())
        {
            result = ops[currOps[idx]](num, result);
        }

        return result;
    }

    static long Mul(long a, long b) => a * b;
    static long Add(long a, long b) => a + b;
}

static long Day7b(string line)
{
    if (line.Split(": ") is not [string totalStr, string numbersStr]) return 0;

    var total = long.Parse(totalStr);
    var numbers = numbersStr.Split(" ").Select(long.Parse).ToArray();
    Func<long, long, long>[] ops = [Mul, Add, Conc];

    return CanTotal(ImmutableList<long>.Empty) ? total : 0;

    bool CanTotal(ImmutableList<long> currOps)
    {
        if (currOps.Count() >= numbers.Length - 1)
        {
            return Calculate(currOps) == total;
        }

        foreach (var op in ops.Index())
        {
            if (CanTotal(currOps.Add(op.Index)))
            {
                return true;
            }
        }

        return false;
    }

    long Calculate(ImmutableList<long> currOps)
    {
        var result = numbers[0];

        foreach (var (idx, num) in numbers[1..].Index())
        {
            result = ops[currOps[idx]](num, result);
        }

        return result;
    }

    static long Mul(long a, long b) => a * b;
    static long Add(long a, long b) => a + b;
    static long Conc(long a, long b) => long.Parse($"{b}{a}");
}

static void Day8a(string[] input)
{
    var dictionary = new Dictionary<char, List<Pos>>();

    for (int i = 0; i < input.Length; i++)
    {
        for (int j = 0; j < input[i].Length; j++)
        {
            char c = input[i][j];
            if (char.IsAsciiLetterOrDigit(c))
            {
                if (dictionary.TryGetValue(c, out var list))
                {
                    list.Add((j, i));
                }
                else
                {
                    dictionary.Add(c, [(j, i)]);
                }
            }
        }
    }

    var antiNodes = new HashSet<Pos>();

    foreach (var c in dictionary.Keys)
    {
        foreach (var antiNode in GetAllPairs([.. dictionary[c]]).SelectMany(GetAllAntiNodes))
        {
            switch (antiNode)
            {
                case var (x, y) when x < 0 || y < 0:
                    continue;
                case var (x, y) when x >= input[0].Length || y >= input.Length:
                    continue;
                default:
                    antiNodes.Add(antiNode);
                    break;
            };
        }
    }

    string[] output = new string[0];
    /*
    output = input.Select(x => x.ToArray()).ToArray();
    foreach (var (x, y) in antiNodes)
    {
        output[y][x] = '#';
    }
    */

    Console.WriteLine(string.Join("\n", output.Select(x => new string(x))));
    Console.WriteLine(antiNodes.Count);

    static IEnumerable<(Pos, Pos)> GetAllPairs(ImmutableList<Pos> charPos)
    {
        if (charPos.Count < 2)
        {
            yield break;
        }

        if (charPos.Count == 2)
        {
            yield return (charPos[0], charPos[1]);
        }

        foreach (var item in charPos)
        {
            foreach (var pair in GetAllPairs(charPos.Remove(item)))
            {
                yield return pair;
            }
        }
    }

    static IEnumerable<Pos> GetAllAntiNodes((Pos a, Pos b) pair)
    {
        var (a, b) = pair;
        var (ax, ay) = a;
        var (bx, by) = b;

        var xDiff = (bx - ax);
        var yDiff = (by - ay);

        yield return new Pos(bx + xDiff, by + yDiff);
        yield return new Pos(ax - xDiff, ay - yDiff);
    }
}

static void Day8b(string[] input)
{
    var dictionary = new Dictionary<char, List<Pos>>();

    for (int i = 0; i < input.Length; i++)
    {
        for (int j = 0; j < input[i].Length; j++)
        {
            char c = input[i][j];
            if (char.IsAsciiLetterOrDigit(c))
            {
                if (dictionary.TryGetValue(c, out var list))
                {
                    list.Add((j, i));
                }
                else
                {
                    dictionary.Add(c, [(j, i)]);
                }
            }
        }
    }

    var antiNodes = new HashSet<Pos>();

    foreach (var c in dictionary.Keys)
    {
        foreach (var antiNode in GetAllPairs([.. dictionary[c]]).SelectMany(GetAllAntiNodes))
        {
            switch (antiNode)
            {
                case var (x, y) when x < 0 || y < 0:
                    continue;
                case var (x, y) when x >= input[0].Length || y >= input.Length:
                    continue;
                default:
                    antiNodes.Add(antiNode);
                    break;
            };
        }
    }

    string[] output = new string[0];
    /*
    output = input.Select(x => x.ToArray()).ToArray();
    foreach (var (x, y) in antiNodes)
    {
        output[y][x] = '#';
    }
    */

    Console.WriteLine(string.Join("\n", output.Select(x => new string(x))));
    Console.WriteLine(antiNodes.Count);

    static IEnumerable<(Pos, Pos)> GetAllPairs(ImmutableList<Pos> charPos)
    {
        if (charPos.Count < 2)
        {
            yield break;
        }

        if (charPos.Count == 2)
        {
            yield return (charPos[0], charPos[1]);
        }

        foreach (var item in charPos)
        {
            foreach (var pair in GetAllPairs(charPos.Remove(item)))
            {
                yield return pair;
            }
        }
    }

    static IEnumerable<Pos> GetAllAntiNodes((Pos a, Pos b) pair)
    {
        var (a, b) = pair;
        var (ax, ay) = a;
        var (bx, by) = b;

        var xDiff = (bx - ax);
        var yDiff = (by - ay);

        for (var i = 0; i < 100; i++)
        {
            yield return new Pos(bx + (xDiff * i), by + (yDiff * i));
            yield return new Pos(ax - (xDiff * i), ay - (yDiff * i));
        }
    }
}

static void Day9a(string input)
{
    var size = input.Select(x => int.Parse($"{x}")).Sum();
    var memory = new List<long>(size);
    for (int i = 0; i < input.Length; i++)
    {
        var isFile = i % 2 == 0;
        var fileId = isFile ? i / 2 : -1;
        var blockLength = char.GetNumericValue(input[i]);

        for (int j = 0; j < blockLength; j++)
        {
            memory.Add(fileId);
        }
    }

    var inputMem = memory.ToArray();
    var finalMem = memory.ToArray();
    foreach (var cell in inputMem.Index().Reverse())
    {
        var empty = inputMem.Index().First(x => x.Item == -1).Index;

        if (cell.Index <= empty)
        {
            break;
        }

        if (cell.Item == -1)
        {
            continue;
        }

        inputMem[empty] = cell.Item;
        finalMem[empty] = cell.Item;
        finalMem[cell.Index] = -1;
        //Console.WriteLine(string.Join("", finalMem.Select(x => x == -1 ? "." : x.ToString())));
    }

    //Console.WriteLine(string.Join("", finalMem.Select(x => x == -1 ? "." : x.ToString())));
    //Console.WriteLine(memory.Count);

    var result = finalMem.Index().Where(x => x.Item != -1).Select(x => x.Item * x.Index).Sum();
    Console.WriteLine(result);
}

static void Day9b(string input)
{
    var size = input.Select(x => int.Parse($"{x}")).Sum();
    var memory = new List<long>(size);
    for (int i = 0; i < input.Length; i++)
    {
        var isFile = i % 2 == 0;
        var fileId = isFile ? i / 2 : -1;
        var blockLength = char.GetNumericValue(input[i]);

        for (int j = 0; j < blockLength; j++)
        {
            memory.Add(fileId);
        }
    }

    var inputMem = memory.ToArray();
    var finalMem = memory.ToArray();
    var previousItem = long.MaxValue;

    //Console.WriteLine(string.Join("", finalMem.Select(x => x == -1 ? "." : x.ToString())));
    for (int i = inputMem.Length - 1; i >= 0; i--)
    {
        var item = inputMem[i];
        if (item == -1)
        {
            continue;
        }

        if (item >= previousItem)
        {
            continue;
        }

        var requiredSize = (int)char.GetNumericValue(input[(int)item * 2]);
        var slot = GetFirstSlot(requiredSize);

        if (slot.start == -1)
        {
            continue;
        }

        if (slot.start >= i)
        {
            continue;
        }

        SwapItems(inputMem, finalMem, slot, item, ref i);
        previousItem = item;
        //Console.WriteLine(string.Join("", finalMem.Select(x => x == -1 ? "." : x.ToString())));
    }

    void SwapItems(long[] inputMem, long[] finalMem, (int start, int length) slot, long item, ref int itemPos)
    {
        for (int i = slot.start; i < slot.start + slot.length; i++)
        {
            inputMem[i] = item;
            finalMem[i] = item;
        }

        var itemEnd = itemPos;
        for (; itemPos > itemEnd - slot.length; itemPos--)
        {
            finalMem[itemPos] = -1;
        }
        itemPos++;
    }

    (int start, int length) GetFirstSlot(int minSize)
    {
        var start = -1;
        int length = 0;
        var inBlank = false;
        for (int i = 0; i < inputMem.Length; i++)
        {
            var currentIsBlank = inputMem[i] == -1;
            if (!inBlank && currentIsBlank)
            {
                start = i;
            }

            if (currentIsBlank)
            {
                inBlank = true;
                length++;
            }
            else if (!currentIsBlank && inBlank)
            {
                inBlank = false;
                start = -1;
                length = 0;
            }


            if (length >= minSize)
            {
                return (start, length);
            }
        }

        return (-1, -1);
    }

    var result = finalMem.Index().Where(x => x.Item != -1).Select(x => x.Item * x.Index).Sum();
    Console.WriteLine(result);
}

static void Day10(double[][] input)
{
    var numPeaks = 0;
    var totalRating = 0;
    for (int i = 0; i < input.Length; i++)
    {
        for (int j = 0; j < input[i].Length; j++)
        {
            if (input[i][j] == 0)
            {
                HashSet<Pos> peaks = [];
                totalRating += NumPeaks(i, j, peaks);
                numPeaks += peaks.Count;
            }
        }
    }

    Console.WriteLine(numPeaks);
    Console.WriteLine(totalRating);

    int NumPeaks(int i, int j, HashSet<Pos> peaks)
    {
        var itemValue = input[i][j];
        if (itemValue == 9)
        {
            peaks.Add((j, i));
            return 1;
        }

        Pos[] surroundings = [(j + 1, i), (j - 1, i), (j, i + 1), (j, i - 1)];

        var validPositions = surroundings
            .Where(pos => InBounds(pos) && input[pos.y][pos.x] == itemValue + 1);

        var totalSurroundingPaths = validPositions
            .Select(pos => NumPeaks(pos.y, pos.x, peaks));

        var numPaths = totalSurroundingPaths.Sum();

        return numPaths;
    }

    bool InBounds(Pos pos)
    {
        return pos.x >= 0 && pos.y >= 0
            && pos.x < input[0].Length
            && pos.y < input.Length;
    }
}
#endregion