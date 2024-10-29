using System.Net.Security;

namespace Sharpcash;

public class StampFormatOptions
{
    public StampDateFormat DateFormat { get; set; } = StampDateFormat.Medium;

    public bool CounterPaddingEnabled { get; set; }
}
