using System.Globalization;

namespace Sharpcash;

internal static class StampConstants
{
    public const int VersionLength = 1;
    public const int BitsLength = 2;
    public const int DateLength = 6;

    public const int ColonLength = 1;
    public const int ColonCount = 6;

    // Version + Bits + Date + 6 Colons (e.g. 1:20:081216::::)
    public const int BasicLength = VersionLength + BitsLength + DateLength + ColonLength * ColonCount;
    public const int MaxCounterLength = 12;

    public const string ShortDateFormat = "yyMMdd";
    public const string MediumDateFormat = "yyMMddhhmm";
    public const string LongDateFormat = "yyMMddhhmmss";

    public const string StampStringPattern =
        @"^\d:\d{2}:\d{6}:[^:]*::[^:]*:(?=.{0,12}$)(?:[A-Za-z\d+/]{4})*(?:[A-Za-z\d+/]{2}==|[A-Za-z\d+/]{3}=)?$";

    public static readonly IFormatProvider DateFormatProvider = CultureInfo.GetCultureInfo("en-US");
}
