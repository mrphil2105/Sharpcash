using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

    public static HashcashStamp Parse(ReadOnlySpan<char> stampChars)
    {
        if (!TryParse(stampChars, out var stamp))
        {
            throw new ArgumentException("The specified hashcash stamp is invalid.", nameof(stampChars));
        }

        return stamp;
    }

    public static bool TryParse(ReadOnlySpan<char> stampChars, [NotNullWhen(true)] out HashcashStamp? stamp)
    {
        if (!StampHelper.VerifyStampString(stampChars))
        {
            stamp = null;

            return false;
        }

        var index = StampConstants.VersionLength + StampConstants.ColonLength;
        var bitsChars = stampChars.Slice(index, StampConstants.BitsLength);
        var bits = int.Parse(bitsChars);

        index += StampConstants.BitsLength + StampConstants.ColonLength;
        var dateChars = stampChars.Slice(index, StampConstants.DateLength);
        var date = DateTime.ParseExact(dateChars, StampConstants.DateFormat, StampConstants.FormatProvider);

        index += StampConstants.DateLength + StampConstants.ColonLength;
        var resourceLength = stampChars[index..]
            .IndexOf(':');
        var resourceChars = stampChars.Slice(index, resourceLength);

        // Multiply colon length by two to skip Extension.
        index += resourceLength + StampConstants.ColonLength * 2;
        var randomLength = stampChars[index..]
            .IndexOf(':');
        var randomChars = stampChars.Slice(index, randomLength);

        index += randomLength + StampConstants.ColonLength;
        var counterChars = stampChars[index..];
        var counter = BuffersHelper.GetInt64(counterChars);

        stamp = new HashcashStamp(bits, date, new string(resourceChars), new string(randomChars), counter);

        return true;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten)
    {
        var length = this.GetLength();

        if (destination.Length < length)
        {
            charsWritten = 0;

            return false;
        }

        Span<char> dateChars = stackalloc char[6];
        Date.TryFormat(dateChars, out _, StampConstants.DateFormat);

        Span<char> counterBase64 = stackalloc char[StampConstants.MaxCounterLength];
        var counterBase64Length = BuffersHelper.GetBase64(Counter, counterBase64);
        counterBase64 = counterBase64[..counterBase64Length];

        var builder = new StringBuilder(length);
        builder.Append(Version);
        builder.Append(':');
        builder.Append(Bits);
        builder.Append(':');
        builder.Append(dateChars);
        builder.Append(':');
        builder.Append(Resource);
        builder.Append(':');
        builder.Append(':');
        builder.Append(Random);
        builder.Append(':');
        builder.Append(counterBase64);

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
