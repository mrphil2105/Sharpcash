namespace Sharpcash;

public class StampFormatOptionsBuilder
{
    public StampFormatOptions Options { get; } = new();

    public StampFormatOptionsBuilder UseDateFormat(StampDateFormat dateFormat)
    {
        Options.DateFormat = dateFormat;

        return this;
    }

    public StampFormatOptionsBuilder EnableCounterPadding(bool counterPaddingEnabled = true)
    {
        Options.CounterPaddingEnabled = counterPaddingEnabled;

        return this;
    }
}
