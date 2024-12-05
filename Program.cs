// See https://aka.ms/new-console-template for more information
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

var input = File.ReadAllLines(@"C:\Users\Hemant Hari\source\repos\aoc2024\day4.txt");


var testInput = @"....XXMAS.
.SAMXMS...
...S..A...
..A.A.MS.X
XMASAMX.MM
X.....XA.A
S.S.S.S.SS
.A.A.A.A.A
..M.M.M.MM
.X.X.XMASX".Split("\r\n");

Day4a(input);

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
            for(int j = 0; j < Math.Min(i+1, Math.Min(input.Length, input[0].Length)); j++)
            {
                diagonal.Add(input[i-j][j]);
            }

            yield return new string(diagonal.ToArray());
        }

        for (int i = 0; i < input[0].Length - 1; i++)
        {
            List<char> diagonal = new();
            for(int j = 0; j < Math.Min(i+1, Math.Min(input.Length, input[0].Length)); j++)
            {
                diagonal.Add(input[^(i-j + 1)][^(j + 1)]);
            }

            yield return new string(diagonal.ToArray());
        }
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

#endregion