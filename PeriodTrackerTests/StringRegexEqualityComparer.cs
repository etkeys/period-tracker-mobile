
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace PeriodTrackerTests;

public class StringRegexEqualityComparer: IEqualityComparer<string>
{
    public static StringRegexEqualityComparer Default => new();

    public bool Equals(string? x, string? y)
    {
        if (x is null && y is null) return true;
        if (x is null ^ y is null) return false;

        if (string.Equals(x, y, StringComparison.OrdinalIgnoreCase)) return true;

        return Regex.IsMatch(y!, x!, RegexOptions.IgnoreCase);
    }

    public int GetHashCode(string? x) => throw new NotSupportedException();
}