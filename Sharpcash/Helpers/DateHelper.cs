using System.ComponentModel;

namespace Sharpcash.Helpers;

internal static class DateHelper
{
    public static string GetDateFormatString(StampDateFormat dateFormat)
    {
        return dateFormat switch
        {
            StampDateFormat.Short => ShortDateFormat,
            StampDateFormat.Medium => MediumDateFormat,
            StampDateFormat.Long => LongDateFormat,
            _ => throw new InvalidEnumArgumentException(nameof(dateFormat), (int)dateFormat, typeof(StampDateFormat))
        };
    }
}
