using System.Diagnostics.CodeAnalysis;
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

    public HashcashStamp Mint(HashAlgorithmName hashAlgorithm, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var minter = new HashcashMinter(this, hashAlgorithm);

        return minter.Mint(0, 1, cancellationToken);
    }

    public async Task<HashcashStamp> MintAsync(HashAlgorithmName hashAlgorithm,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var minter = new HashcashMinter(this, hashAlgorithm);

        return await Task.Factory.StartNew(() => minter.Mint(0, 1, cancellationToken), cancellationToken,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public async Task<HashcashStamp> MintAsync(HashAlgorithmName hashAlgorithm, int threadCount,
        CancellationToken cancellationToken = default)
    {
        if (threadCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(threadCount), "Value must be greater than 0.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var localCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var tasks = new List<Task<HashcashStamp>>(threadCount);

        for (var i = 0; i < threadCount; i++)
        {
            var offset = i;
            var minter = new HashcashMinter(this, hashAlgorithm);
            var task = Task.Factory.StartNew(() => minter.Mint(offset, threadCount, localCts.Token), localCts.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);

            #pragma warning disable CA2016
            // ReSharper disable once MethodSupportsCancellation
            _ = task.ContinueWith(_ => minter.Dispose(), TaskContinuationOptions.ExecuteSynchronously);
            #pragma warning restore CA2016

            tasks.Add(task);
        }

        var completedTask = await Task.WhenAny(tasks);
        localCts.Cancel();

        return await completedTask;
    }

    public bool Verify(HashAlgorithmName hashAlgorithm)
    {
        using var minter = new HashcashMinter(this, hashAlgorithm);

        return minter.Verify();
    }

    public static HashcashStamp Parse(ReadOnlySpan<char> stampChars)
    {
        if (!TryParse(stampChars, out var stamp))
        {
            throw new ArgumentException("The specified hashcash stamp string is invalid.", nameof(stampChars));
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

        var index = 0;

        var bits = StampHelper.ParseBits(stampChars, ref index);
        var date = StampHelper.ParseDate(stampChars, ref index);
        var resourceChars = StampHelper.ParseResource(stampChars, ref index, out var resourceLength);
        var randomChars = StampHelper.ParseRandom(stampChars, resourceLength, ref index, out var randomLength);
        var counter = StampHelper.ParseCounter(stampChars, randomLength, ref index);

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
