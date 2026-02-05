namespace EasySave.Presentation.Cli;

/// <summary>
/// Parses command-line arguments representing backup job IDs.
/// </summary>
internal static class CommandLineArgumentParser
{
    /// <summary>
    /// Supported formats:
    /// - "1-3"  -> 1, 2, 3
    /// - "1;3"  -> 1 and 3
    /// - "2"    -> 2 only
    /// </summary>
    internal static List<int> Parse(string arg)
    {
        var result = new List<int>();
        string normalized = arg.Replace(" ", string.Empty);

        if (normalized.Contains('-'))
        {
            string[] parts = normalized.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new FormatException();

            int start = int.Parse(parts[0]);
            int end = int.Parse(parts[1]);
            if (start <= 0 || end <= 0)
                throw new FormatException();

            if (end < start)
                (start, end) = (end, start);

            for (int i = start; i <= end; i++)
                result.Add(i);
        }
        else if (normalized.Contains(';'))
        {
            result = normalized
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
        }
        else
        {
            result.Add(int.Parse(normalized));
        }

        // Avoid duplicates while keeping the initial order.
        return result.Distinct().ToList();
    }
}


