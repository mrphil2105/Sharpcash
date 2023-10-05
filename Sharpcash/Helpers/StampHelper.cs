using System.Globalization;
using System.Text.RegularExpressions;

namespace Sharpcash.Helpers;

public static partial class StampHelper
{
    [GeneratedRegex(StampStringPattern)]
    private static partial Regex StampStringRegex();

    public static bool VerifyStampString(ReadOnlySpan<char> stampChars)
    {
        if (!StampStringRegex().IsMatch(stampChars))
        {
            return false;
        }

        var dateChars = stampChars.Slice(VersionLength + BitsLength + ColonLength * 2, DateLength);

        return DateTime.TryParseExact(dateChars, DateFormat, DateFormatProvider, DateTimeStyles.None, out _);
    }

    public static int ParseBits(ReadOnlySpan<char> stampChars, ref int index)
    {
        index += VersionLength + ColonLength;
        var bitsChars = stampChars.Slice(index, BitsLength);
        var bits = int.Parse(bitsChars);

        return bits;
    }

    public static DateTime ParseDate(ReadOnlySpan<char> stampChars, ref int index)
    {
        index += BitsLength + ColonLength;
        var dateChars = stampChars.Slice(index, DateLength);
        var date = DateTime.ParseExact(dateChars, DateFormat, DateFormatProvider);

        return date;
    }

    public static ReadOnlySpan<char> ParseResource(ReadOnlySpan<char> stampChars, ref int index, out int resourceLength)
    {
        index += DateLength + ColonLength;
        resourceLength = stampChars[index..].IndexOf(':');
        var resourceChars = stampChars.Slice(index, resourceLength);

        return resourceChars;
    }

    public static ReadOnlySpan<char> ParseRandom(ReadOnlySpan<char> stampChars, int resourceLength, ref int index,
        out int randomLength)
    {
        // Multiply colon length by two to skip Extension.
        index += resourceLength + ColonLength * 2;
        randomLength = stampChars[index..].IndexOf(':');
        var randomChars = stampChars.Slice(index, randomLength);

        return randomChars;
    }

    public static long ParseCounter(ReadOnlySpan<char> stampChars, int randomLength, ref int index)
    {
        index += randomLength + ColonLength;
        var counterChars = stampChars[index..];
        var counter = BuffersHelper.GetInt64(counterChars);

        return counter;
    }
}
