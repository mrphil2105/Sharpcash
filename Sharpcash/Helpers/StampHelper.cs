using System.Globalization;
using System.Text.RegularExpressions;
using static Sharpcash.StampConstants;

namespace Sharpcash.Helpers;

internal static class StampHelper
{
    public static bool VerifyStampString(ReadOnlySpan<char> stampChars)
    {
        // TODO: Use the ReadOnlySpan<char> directly when Regex supports it.
        var stampString = new string(stampChars);

        if (!Regex.IsMatch(stampString, StampStringPattern))
        {
            return false;
        }

        var dateChars = stampChars.Slice(VersionLength + BitsLength + ColonLength * 2, DateLength);

        return DateTime.TryParseExact(dateChars, DateFormat, FormatProvider, DateTimeStyles.None, out _);
    }
}
