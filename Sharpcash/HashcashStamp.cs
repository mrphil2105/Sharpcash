namespace Sharpcash;

public class HashcashStamp
{
    public HashcashStamp(int bits, DateTime date, string resource, string random = "", long counter = 0)
    {
        if (bits is < 10 or > 99)
        {
            throw new ArgumentOutOfRangeException(nameof(bits), "Value must be between 10 and 99.");
        }

        if (resource.Contains(':'))
        {
            throw new ArgumentException("Value cannot contain a colon.", nameof(resource));
        }

        if (random.Contains(':'))
        {
            throw new ArgumentException("Value cannot contain a colon.", nameof(random));
        }

        Bits = bits;
        Date = date.Date;
        Resource = resource;
        Random = random;
        Counter = counter;
    }

    public int Version => 1;

    public int Bits { get; }

    public DateTime Date { get; }

    public string Resource { get; }

    public string Extension => string.Empty;

    public string Random { get; }

    public long Counter { get; }
}
