using System.Text.RegularExpressions;

namespace PresenceSharpUI.Helpers;

public static class RegexHelper
{
    private static readonly Regex InvalidNumericInputRegex =
        new(@"[^0-9.-]+", RegexOptions.Compiled);

    public static bool IsTextAllowed(string text) =>
        !InvalidNumericInputRegex.IsMatch(text);
}