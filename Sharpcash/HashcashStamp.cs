using System.Buffers;
using System.Text;
using Sharpcash.Helpers;

namespace Sharpcash;

public class HashcashStamp
{
    public HashcashStamp(int bits, DateTime date, string resource, string random = "", long counter = 0)
    {
        if (bits is < 10 or > 99)
        {
            throw new ArgumentOutOfRangeException(nameof(bits), "Value must be between 10 and 99.");
        }

        if (resource is null)
        {
            throw new ArgumentNullException(nameof(resource));
        }

        if (resource.Contains(':'))
        {
            throw new ArgumentException("Value cannot contain a colon.", nameof(resource));
        }

        if (random is null)
        {
            throw new ArgumentNullException(nameof(random));
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

    public bool TryFormat(Span<char> destination, out int charsWritten)
    {
        var length = this.GetLength();

        if (destination.Length < length)
        {
            charsWritten = 0;

            return false;
        }

        Span<char> dateChars = stackalloc char[6];
        Date.TryFormat(dateChars, out _, DateFormat);

        Span<char> counterBase64 = stackalloc char[MaxCounterLength];
        var counterBase64Length = BuffersHelper.GetBase64(Counter, counterBase64);
        counterBase64 = counterBase64[..counterBase64Length];

        var builder = new StringBuilder(length).Append(Version)
            .Append(':')
            .Append(Bits)
            .Append(':')
            .Append(dateChars)
            .Append(':')
            .Append(Resource)
            .Append(':')
            .Append(':')
            .Append(Random)
            .Append(':')
            .Append(counterBase64);

        Debug.Assert(builder.Length == length);

        builder.CopyTo(0, destination, length);
        charsWritten = length;

        return true;
    }

    public override string ToString()
    {
        var length = this.GetLength();
        using var memoryOwner = MemoryPool<char>.Shared.Rent(length);
        var stampChars = memoryOwner.Memory.Span[..length];

        TryFormat(stampChars, out _);

        return new string(stampChars);
    }
}
