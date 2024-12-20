﻿using FastConsole;
using System.Collections.Immutable;
using System.Drawing;
using System.Numerics;
using System.Text.RegularExpressions;
using Pos = (int x, int y);
using PosCheat = ((int x, int y) pos, (int x, int y)? cheat1, (int x, int y)? cheat2, int cheatTime);

FConsole.Initialize("test");
var text = File.ReadAllLines(@"C:\Users\Hemant Hari\source\repos\aoc2024\day20.txt");

var testText = @"###############
#...#...#.....#
#.#.#.#.#.###.#
#S#...#.#.#...#
#######.#.#.###
#######.#.#...#
#######.#.###.#
###..E#...#...#
###.#######.###
#...###...#...#
#.#####.#.###.#
#.#...#.#.#...#
#.#.#.#.#.#.###
#...#...#...###
###############".Split("\r\n");

var testText2 = """
    #######
    #...#.#
    #S#...#
    #####.#
    ###..E#
    #######
    """.Split("\r\n");

Console.WriteLine(Day20a(testText, 20));
//Console.WriteLine(new int[] { 32, 31, 29, 39, 25, 23, 20, 19, 12, 14, 12, 22, 4, 3 }.Sum());

//Console.WriteLine(Day20a(testText, 2));
//Console.WriteLine(Day20a(text, 2));

int Day20a(string[] input, int maxTimeInCheat)
{
    PosCheat start = default;
    Pos endPos = (-1, -1);

    Dictionary<Pos, char> map = [];
    for (int i = 0; i < input.Length; i++)
    {
        for (int j = 0; j < input[i].Length; j++)
        {
            var x = input[i][j];
            if (x == 'S')
            {
                start = ((x: j, y: i), null, null, 0);
            }
            else if (x == 'E')
            {
                endPos = (x: j, y: i);
            }

            map[(j, i)] = x;
        }
    }

    map[start.pos] = '.';
    map[endPos] = '.';

    var noCheatMap = RunMapping((endPos, null, null, 0), cheatsEnabled: false);

    var noCheatTime = noCheatMap[start];

    var distMap = RunMapping(start, cheatsEnabled: true, noCheatMap);

    var allCheats = distMap.Keys
        .Where(x => x.pos == endPos && x.cheat2 != null)
        .Where(x => noCheatTime - distMap[x] > 0)
        .GroupBy(x => (x.pos, x.cheat1, x.cheat2))
        .Select(x => (x.Key.pos, x.Key.cheat1, x.Key.cheat2, x.Min(x => x.cheatTime)))
        .GroupBy(x => noCheatTime - distMap[x])
        .Select(x => (Saved: x.Key, EndKeys: x.ToArray()));

    allCheats
        .Where(x => x.Saved >= 50)
        .OrderBy(x => x.Saved)
        .ToList()
        .ForEach(x => Console.WriteLine((x.Saved, x.EndKeys.Length)));

    return allCheats.Where(x => x.Saved >= 100)
                    .Sum(x => x.EndKeys.Length);

    Dictionary<PosCheat, int> RunMapping(
        PosCheat start,
        bool cheatsEnabled,
        Dictionary<PosCheat, int>? noCheatMap = null)
    {
        Dictionary<PosCheat, int> distMap = [];
        distMap[start] = 0;
        int getDist(PosCheat pc) => distMap.GetValueOrDefault(pc, int.MaxValue);

        PriorityQueue<PosCheat, int> q = new();
        q.Enqueue(start, 0);

        while (q.TryDequeue(out var posCheat, out var dist))
        {
            var (pos, cheat1, cheat2, time) = posCheat;
            ReadOnlySpan<Pos> neighbours = [
                (pos.x, pos.y - 1),
                (pos.x + 1, pos.y),
                (pos.x - 1, pos.y),
                (pos.x, pos.y + 1),
            ];

            foreach (var neighbour in neighbours)
            {
                if (!map.TryGetValue(neighbour, out var c))
                {
                    continue;
                }

                bool isFree = c == '.';
                var (newC1, newC2, newTime) = (cheat1, cheat2, time);
                if (cheat1 != null)
                {
                    newTime += 1;
                }
                else if (!isFree)
                {
                    newC1 = pos;
                    newTime = 1;
                }

                if(newTime == maxTimeInCheat || (newTime >= 1 && isFree))
                {
                    newC2 = neighbour;
                }

                // no more passing through walls
                var notInCheat = newC2 != null;
                if ((notInCheat || !cheatsEnabled) && c != '.')
                {
                    continue;
                }

                var distFromSource = getDist(posCheat) + 1;
                var neighbourCheat = (neighbour, newC1, newC2, newTime);
                if (newC2 != null && noCheatMap?.TryGetValue((neighbour, null, null, 0), out var remainingDist) == true)
                {
                    distMap[(endPos, newC1, newC2, newTime)] = distFromSource + remainingDist;
                }
                else if (distFromSource < getDist(neighbourCheat))
                {
                    q.Enqueue(neighbourCheat, 1);
                    distMap[neighbourCheat] = distFromSource;
                }
            }
        }

        return distMap;
    }

    /*
 0012345678901234
 0###############
 1#...#...#.....#
 2#.#.#.#.#.###.#
 3#S#...#.#.#...#
 4#######.#.#.###
 5#######.#.#...#
 6#######.#.###.#
 7###..E#...#...#
 8###.#######.###
 9#...###...#...#
10#.#####.#.###.#
11#.#...#.#.#...#
12#.#.#.#.#.#.###
13#...#...#...###
14###############
     */
}


static IEnumerable<int> Hardcoded(BigInteger a)
{
    BigInteger b = 0;
    BigInteger c = 0;

    while (a != 0)
    {
        b = a % 8;
        b = b ^ 1;
        c = a / BigInteger.Pow(2, (int)b);

        b = b ^ 0b101;
        b = b ^ c;
        yield return (int)(b%8);
        a = a / 8;
    }
}

BigInteger Day17b()
{
    var input = """
        Register A: 38610541
        Register B: 0
        Register C: 0

        Program: 2,4,1,1,7,5,1,5,4,3,5,5,0,3,3,0
        """;

    var vm = VirtualMachine.Init(input);

    var (rA, rB, rC, inst) = vm;
    rA = BigInteger.Pow(2, 48) - BigInteger.Pow(2, 45) + 0b110_000 + 0b110;
    rA = 205292059155489;

    while (true)
    {
        var output = Hardcoded(rA);
        // Console.WriteLine(string.Join(",", output));
        // Console.WriteLine(string.Join(",", inst));
        if(Enumerable.SequenceEqual(vm.Run(), inst))
        {
            return rA;
        }

        rA++;
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
            List<char> diagonal = [];
            for (int j = 0; j < Math.Min(i + 1, Math.Min(input.Length, input[0].Length)); j++)
            {
                diagonal.Add(input[i - j][j]);
            }

            yield return new string(diagonal.ToArray());
        }

        for (int i = 0; i < input[0].Length - 1; i++)
        {
            List<char> diagonal = [];
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

void Day11a(IEnumerable<long> initial, int iters)
{
    var stones = new LinkedList<long>(initial);
    for (var i = 0; i < iters; i++)
    {
        for (var item = stones.First; item != null; item = item.Next)
        {
            ref var itemVal = ref item.ValueRef;
            if (itemVal == 0)
            {
                itemVal = 1;
                continue;
            }

            var numDigits = NumDigits(itemVal);
            if (numDigits % 2 == 0)
            {
                var halfLen = numDigits / 2;
                var halfMultiplier = Math.Pow(10, halfLen);
                var first = Math.Floor(itemVal / halfMultiplier);
                var second = itemVal - first * halfMultiplier;

                itemVal = (long)second;
                stones.AddBefore(item, (long)first);
                continue;
            }

            itemVal = itemVal * 2024;
        }

        Console.WriteLine(i);
    }

    Console.WriteLine(stones.Count);
}

void Day11b(IEnumerable<long> initial, int iters)
{
    var cache = new Dictionary<(long num, int iters), long>();

    Console.WriteLine(initial.Select(x => NumStones((x, iters))).Sum());

    long NumStones((long num, int iters) item)
    {
        var (num, iters) = item;
        if (cache.TryGetValue(item, out var numStones))
        {
            return numStones;
        }

        if (iters == 0)
        {
            return 1;
        }

        if (num == 0)
        {
            var zeroVal = NumStones((1, iters - 1));
            cache[item] = zeroVal;
            return zeroVal;
        }

        var numDigits = NumDigits(num);
        if (numDigits % 2 == 0)
        {
            var halfLen = numDigits / 2;
            var halfMultiplier = Math.Pow(10, halfLen);
            var first = (long)Math.Floor(num / halfMultiplier);
            var second = (long)(num - first * halfMultiplier);

            var evenNumDigitsVal = NumStones((first, iters - 1)) + NumStones((second, iters - 1));
            cache[item] = evenNumDigitsVal;

            return evenNumDigitsVal;
        }

        var result = NumStones((num * 2024, iters - 1));
        cache.Add(item, result);
        return result;
    }
}

int NumDigits(long n)
{
    return (int)Math.Floor(Math.Log10(n) + 1);
}

long Day12a(string[] input)
{
    var map = input
        .SelectMany((row, i) => row.Select((c, j) => (c, j, i)))
        .ToDictionary(k => (k.i, k.j), v => v.c);
    var result = 0;
    HashSet<(int, int)> visited = [];
    for (int i = 0; i < input.Length; i++)
    {
        for (int j = 0; j < input[i].Length; j++)
        {
            if (visited.Contains((i, j)))
            {
                continue;
            }

            result += CalculatePrice(i, j);
        }
    }

    int CalculatePrice(int i, int j)
    {
        HashSet<(int, int)> area = [];
        HashSet<(decimal, decimal)> neighbour = [];
        var type = map[(i, j)];

        bool GetPerimeter(int i, int j)
        {
            if (!map.TryGetValue((i, j), out var curr) || curr != type)
            {
                return false;
            }

            if (area.Contains((i, j)))
            {
                return true;
            }

            area.Add((i, j));

            if (GetPerimeter(i, j + 1)) neighbour.Add((i, j + 0.5m));
            if (GetPerimeter(i, j - 1)) neighbour.Add((i, j - 0.5m));
            if (GetPerimeter(i + 1, j)) neighbour.Add((i + 0.5m, j));
            if (GetPerimeter(i - 1, j)) neighbour.Add((i - 0.5m, j));

            return true;
        }

        GetPerimeter(i, j);
        visited.UnionWith(area);
        var perimeter = (area.Count * 4 - neighbour.Count * 2);
        return area.Count * perimeter;
    }

    return result;
}


long Day12b(string[] input)
{
    var map = input
        .SelectMany((row, i) => row.Select((c, j) => (c, j, i)))
        .ToDictionary(k => (k.i, k.j), v => v.c);
    var result = 0;
    HashSet<(int, int)> visited = [];
    for (int i = 0; i < input.Length; i++)
    {
        for (int j = 0; j < input[i].Length; j++)
        {
            if (visited.Contains((i, j)))
            {
                continue;
            }

            result += CalculatePrice(i, j);
        }
    }

    int CalculatePrice(int i, int j)
    {
        HashSet<(int i, int j)> area = [];
        var type = map[(i, j)];

        bool TraverseArea(int i, int j)
        {
            if (!map.TryGetValue((i, j), out var curr) || curr != type)
            {
                return false;
            }

            if (area.Contains((i, j)))
            {
                return true;
            }

            area.Add((i, j));

            TraverseArea(i, j + 1);
            TraverseArea(i, j - 1);
            TraverseArea(i + 1, j);
            TraverseArea(i - 1, j);

            return true;
        }

        TraverseArea(i, j);
        visited.UnionWith(area);

        var sides = NumSides();

        int NumSides()
        {
            var numSides = 0;
            for (int i = 0; i < input.Length; i++)
            {
                var onEdge = false;
                for (int j = 0; j < input[i].Length; j++)
                {
                    if (!area.Contains((i, j)))
                    {
                        onEdge = false;
                        continue;
                    }

                    if (!map.TryGetValue((i - 1, j), out var curr) || curr != type)
                    {
                        if (!onEdge)
                        {
                            numSides++;
                        }

                        onEdge = true;
                    }
                    else
                    {
                        onEdge = false;
                    }
                }

                onEdge = false;
                for (int j = 0; j < input[i].Length; j++)
                {
                    if (!area.Contains((i, j)))
                    {
                        onEdge = false;
                        continue;
                    }

                    if (!map.TryGetValue((i + 1, j), out var curr) || curr != type)
                    {
                        if (!onEdge)
                        {
                            numSides++;
                        }

                        onEdge = true;
                    }
                    else
                    {
                        onEdge = false;
                    }
                }
            }

            for (int j = 0; j < input[0].Length; j++)
            {
                var onEdge = false;
                for (int i = 0; i < input.Length; i++)
                {
                    if (!area.Contains((i, j)))
                    {
                        onEdge = false;
                        continue;
                    }

                    if (!map.TryGetValue((i, j - 1), out var curr) || curr != type)
                    {
                        if (!onEdge)
                        {
                            numSides++;
                        }

                        onEdge = true;
                    }
                    else
                    {
                        onEdge = false;
                    }
                }

                onEdge = false;
                for (int i = 0; i < input.Length; i++)
                {
                    if (!area.Contains((i, j)))
                    {
                        onEdge = false;
                        continue;
                    }

                    if (!map.TryGetValue((i, j + 1), out var curr) || curr != type)
                    {
                        if (!onEdge)
                        {
                            numSides++;
                        }

                        onEdge = true;
                    }
                    else
                    {
                        onEdge = false;
                    }
                }
            }

            return numSides;
        }

        return area.Count * sides;
    }

    return result;
}

const int aCost = 1;
const int bCost = 3;

int Day13a(string[] input)
{
    Regex re = new(@"Button A: X\+(\d*), Y\+(\d*)\nButton B: X\+(\d*), Y\+(\d*)\nPrize: X=(\d*), Y=(\d*)");

    return input.Select(ProcessGame).Sum();

    int ProcessGame(string game)
    {
        var match = re.Match(game);
        var g = (int x) => int.Parse(match.Groups[x].Value);
        var a = (x: g(1), y: g(2));
        var b = (x: g(3), y: g(4));
        var prize = (x: g(5), y: g(6));

        for (int i = 0; i < 10000; i++)
        {
            var (x, y) = (prize.x - b.x * i, prize.y - b.y * i);
            var numUntilTarget = NumUntilTarget(a, (x, y));
            if (numUntilTarget != null)
            {
                var cost = (bCost * numUntilTarget.Value) + (aCost * i);
                return cost;
            }
        }

        return 0;
    }

    int? NumUntilTarget((int x, int y) button, (int x, int y) target)
    {
        var i = 0;
        while (true)
        {
            if (button.x * i > target.x || button.y > target.y)
            {
                return null;
            }

            if (button.x * i == target.x && button.y * i == target.y)
            {
                return i;
            }

            i++;
        }
    }
}

decimal Day13b(string[] input)
{
    Regex re = new(@"Button A: X\+(\d*), Y\+(\d*)\nButton B: X\+(\d*), Y\+(\d*)\nPrize: X=(\d*), Y=(\d*)");

    return input.Select(ProcessGame).Sum();

    decimal ProcessGame(string game, int index)
    {
        var match = re.Match(game);
        var g = (int x) => long.Parse(match.Groups[x].Value);
        var a = (x: g(1), y: g(2));
        var b = (x: g(3), y: g(4));
        var prize = (x: g(5) + 10000000000000, y: g(6) + 10000000000000);

        decimal bClick = (a.x * prize.y - a.y * prize.x) / (a.x * b.y - b.x * a.y);

        decimal aClick = (prize.x * b.y - prize.y * b.x) / (a.x * b.y - a.y * b.x);

        if (((a.x * aClick) + (b.x * bClick), (a.y * aClick) + (b.y * bClick)) == prize)
        {
            return aClick * aCost + bClick * bCost;
        }

        return 0;
    }
}


Bot.Size = (x: 11, y: 7);
Bot.Size = (x: 101, y: 103);
var size = Bot.Size;

int Day14(string[] input, int s)
{
    Regex re = new(@"p=([^,]*),([^,]*) v=([^,]*),([^,]*)");

    var g = (Match match, int x) => int.Parse(match.Groups[x].Value);
    var bots = input
        .Select(Bot.Create)
        .ToArray();

    Debug(0);

    for (int i = 0; i < s; i++)
    {
        foreach (var bot in bots)
        {
            bot.Move();
        }
        Debug(i + 1);
    }

    Console.ReadKey();

    var q1 = bots.Count(b => (b.pos.y < size.y / 2) && (b.pos.x < size.x / 2));
    var q2 = bots.Count(b => (b.pos.y > size.y / 2) && (b.pos.x < size.x / 2));
    var q3 = bots.Count(b => (b.pos.y < size.y / 2) && (b.pos.x > size.x / 2));
    var q4 = bots.Count(b => (b.pos.y > size.y / 2) && (b.pos.x > size.x / 2));

    return q1 * q2 * q3 * q4;

    void Debug(int frame)
    {
        var bitMap = new Bitmap(size.x, size.y);
        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                var num = bots.Count(x => (x.pos.x, x.pos.y) == (j, i));
                if (num != 0)
                {
                    bitMap.SetPixel(j, i, Color.Green);
                }
                else
                {
                    bitMap.SetPixel(j, i, Color.Black);
                }
            }
        }

        bitMap.Save($@"C:\Users\Hemant Hari\Desktop\AOC\{frame}.bmp");
    }

    void DebugConsole()
    {
        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                var num = bots.Count(x => (x.pos.x, x.pos.y) == (j, i));
                if (num != 0)
                {
                    FConsole.SetChar(j, i, num.ToString()[0], ConsoleColor.White, ConsoleColor.Black);
                }
                else
                {
                    FConsole.SetChar(j, i, '.', ConsoleColor.White, ConsoleColor.Black);
                }
            }
        }

        FConsole.DrawBuffer();
    }
}
static void Day15a(string text)
{
    var state = GameState.Read(text);

    state.Run();
    state.Draw();
    Console.WriteLine(state.CalcGps());
}

static void Day15b(string text)
{
    var state = WideGameState.Read(text);

    state.Run();
    state.Draw();
    Console.WriteLine(state.CalcGps());
}

int Day16(string[] input)
{
    PosDir start = new(-1, -1, '0');
    PosDir endPosDir = new(-1, -1, '0');
    Dictionary<PosDir, int> distMap = [];
    Dictionary<PosDir, HashSet<PosDir>> prevMap = [];
    PriorityQueue<PosDir, int> q = new();
    for (int i = 0; i < input.Length; i++)
    {
        for (int j = 0; j < input[i].Length; j++)
        {
            var x = input[i][j];
            if (x == 'S')
            {
                start = new(x: j, y: i, 'e');
                distMap[(start)] = 0;
                distMap[new(j, i, 'n')] = int.MaxValue;
                distMap[new(j, i, 'w')] = int.MaxValue;
                distMap[new(j, i, 's')] = int.MaxValue;
            }
            else if (x == '.')
            {
                distMap[new(j, i, 'n')] = int.MaxValue;
                distMap[new(j, i, 'e')] = int.MaxValue;
                distMap[new(j, i, 'w')] = int.MaxValue;
                distMap[new(j, i, 's')] = int.MaxValue;
            }
            else if (x == 'E')
            {
                endPosDir = new(x: j, y: i, 'n');
                distMap[endPosDir] = int.MaxValue;
                distMap[new(j, i, 'e')] = int.MaxValue;
                distMap[new(j, i, 'w')] = int.MaxValue;
                distMap[new(j, i, 's')] = int.MaxValue;
            }
        }
    }

    q.Enqueue(start, 0);

    while (q.TryDequeue(out var pos, out var dist))
    {
        PosDir forward = pos.dir switch
        {
            'n' => new(pos.x, pos.y - 1, pos.dir),
            'e' => new(pos.x + 1, pos.y, pos.dir),
            'w' => new(pos.x - 1, pos.y, pos.dir),
            's' => new(pos.x, pos.y + 1, pos.dir),
            _ => throw new Exception(),
        };

        HashSet<PosDir> neighbours = [
            forward,
            pos with { dir = 'n' },
            pos with { dir = 'e' },
            pos with { dir = 'w' },
            pos with { dir = 's' },
        ];

        neighbours.Remove(pos);

        foreach (var neighbour in neighbours)
        {
            if (!distMap.ContainsKey(neighbour))
            {
                continue;
            }

            var distFromPos = pos.dir == neighbour.dir ? 1 : 1000;
            var distFromSource = distMap[pos] + distFromPos;
            if (distFromSource < distMap[neighbour])
            {
                q.Enqueue(neighbour, 1);
                prevMap[neighbour] = [pos];
                distMap[neighbour] = distFromSource;
            }
            else if (distFromSource == distMap[neighbour])
            {
                prevMap[neighbour].Add(pos);
            }
        }
    }

    HashSet<Pos> seats = [];

    void NumBestPathCells(PosDir end)
    {
        if (end == start) return;

        seats.Add(end.ToPos());
        var prevs = prevMap[end];
        foreach (var prev in prevs)
        {
            NumBestPathCells(prev);
        }
    }

    NumBestPathCells(endPosDir);
    Console.WriteLine(seats.Count);

    return new[] {
            endPosDir with { dir = 'n' },
            endPosDir with { dir = 'e' },
            endPosDir with { dir = 'w' },
            endPosDir with { dir = 's' },
    }.Select(x => distMap[x]).Min();
}

static void Day17a(string input)
{
    var vm = VirtualMachine.Init(input);
    var output = vm.Run().ToList();
    Console.WriteLine(string.Join(",", output));
}

// solved part 2 with a manual binary search!
int Day18((string[] text, (int x, int y) endPos, int bytesToTake) input)
{
    Pos start = (0, 0);
    Pos endPos = input.endPos;
    HashSet<Pos> map = Enumerable.Range(0, endPos.x + 1)
        .SelectMany(x => Enumerable.Range(0, endPos.y + 1).Select(y => (x, y)))
        .ToHashSet();

    input.text
        .Select(x => x.Split(','))
        .Select(l => (x: int.Parse(l[0]), y: int.Parse(l[1])))
        .Take(input.bytesToTake)
        .ToList()
        .ForEach(x => map.Remove(x));

    Dictionary<Pos, int> distMap = map.ToDictionary(k => k, v => int.MaxValue);
    distMap[start] = 0;

    Dictionary<Pos, HashSet<Pos>> prevMap = [];

    PriorityQueue<Pos, int> q = new();
    q.Enqueue(start, 0);

    while (q.TryDequeue(out var pos, out var dist))
    {
        Pos[] neighbours = [
            (pos.x, pos.y - 1),
            (pos.x + 1, pos.y),
            (pos.x - 1, pos.y),
            (pos.x, pos.y + 1),
        ];

        foreach (var neighbour in neighbours.Where(map.Contains))
        {
            if (!distMap.ContainsKey(neighbour))
            {
                continue;
            }

            var distFromPos = 1;
            var distFromSource = distMap[pos] + distFromPos;
            if (distFromSource < distMap[neighbour])
            {
                q.Enqueue(neighbour, 1);
                prevMap[neighbour] = [pos];
                distMap[neighbour] = distFromSource;
            }
            else if (distFromSource == distMap[neighbour])
            {
                prevMap[neighbour].Add(pos);
            }
        }
    }

    return distMap[endPos];
}

(long, long) Day19(string input)
{
    var inputSplit = input.Split("\n\n");
    var towels = inputSplit[0].Split(", ").Select(x => x.Trim()).ToHashSet();
    var patterns = inputSplit[1].Split("\n").Select(x => x.Trim());

    var solved = new Dictionary<string, long>();

    long Solve(string pattern)
    {
        if (solved.TryGetValue(pattern, out var num))
        {
            return num;
        }

        if (pattern.Length == 0)
        {
            solved[pattern] = 1;
            return 1;
        }

        long numSolutions = 0;
        foreach (var towel in towels)
        {
            if (pattern.StartsWith(towel))
            {
                numSolutions += Solve(pattern[towel.Length..]);
            }
        }

        solved[pattern] = numSolutions;
        return numSolutions;
    }

    long resultA = 0;
    long resultB = 0;
    foreach (var pattern in patterns)
    {
        var numSolutions = Solve(pattern);
        resultB += numSolutions;
        if (numSolutions != 0)
        {
            resultA++;
        }
    }

    return (resultA, resultB);
}

#endregion