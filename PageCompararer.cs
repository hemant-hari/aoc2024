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
