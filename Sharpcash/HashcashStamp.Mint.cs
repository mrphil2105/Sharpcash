using System.Buffers;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Sharpcash.Helpers;

namespace Sharpcash;

public partial class HashcashStamp
{
    public HashcashStamp Mint(HashAlgorithmName hashAlgorithm, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return MintCore(hashAlgorithm, 0, 1, cancellationToken);
    }

    public Task<HashcashStamp> MintAsync(HashAlgorithmName hashAlgorithm, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Factory.StartNew(() => MintCore(hashAlgorithm, 0, 1, cancellationToken), cancellationToken,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public async Task<HashcashStamp> MintAsync(HashAlgorithmName hashAlgorithm, int threadCount,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var localCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var tasks = new List<Task<HashcashStamp>>(threadCount);

        for (var i = 0; i < threadCount; i++)
        {
            var offset = i;
            var task = Task.Factory.StartNew(() => MintCore(hashAlgorithm, offset, threadCount, localCts.Token),
                localCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            tasks.Add(task);
        }

        var completedTask = await Task.WhenAny(tasks);
        localCts.Cancel();

        return await completedTask;
    }

    private HashcashStamp MintCore(HashAlgorithmName hashAlgorithm, int offset, int increment,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using (SerializeToUtf8Bytes(out var stampBytes))
        {
            using var hasher = HashAlgorithm.Create(hashAlgorithm.Name);

            if (hasher == null)
            {
                throw new CryptographicException("Unable to create an instance of the specified hash algorithm.");
            }

            Span<byte> hash = stackalloc byte[hasher.HashSize / 8];
            var validator = new StampValidator(Bits, hasher, hash);

            var counterBase64Bytes = stampBytes.Slice(stampBytes.Length - StampConstants.MaxCounterLength,
                StampConstants.MaxCounterLength);
            var counterBytes = counterBase64Bytes[..sizeof(long)];

            var counter = Counter + offset;

            while ((counter += increment) < long.MaxValue)
            {
                cancellationToken.ThrowIfCancellationRequested();

                BinaryPrimitives.WriteInt64LittleEndian(counterBytes, counter);
                var trimmedLength = BuffersHelper.TrimZeroPadding(counterBytes);
                Base64.EncodeToUtf8InPlace(counterBase64Bytes, trimmedLength, out var charsWritten);

                var trailingChars = StampConstants.MaxCounterLength - charsWritten;
                var currentStampBytes = stampBytes[..^trailingChars];

                if (validator.VerifyOnce(currentStampBytes))
                {
                    return new HashcashStamp(Bits, Date, Resource, Random, counter);
                }
            }

            throw new CryptographicException("A solution could not be found.");
        }
    }

    private IDisposable SerializeToUtf8Bytes(out Span<byte> stampBytes)
    {
        var maxLength = this.GetMaxLength();
        using var charsMemoryOwner = MemoryPool<char>.Shared.Rent(maxLength);
        var stampChars = charsMemoryOwner.Memory.Span[..maxLength];
        TryFormat(stampChars, out _);

        var byteCount = Encoding.UTF8.GetByteCount(stampChars);
        var bytesMemoryOwner = MemoryPool<byte>.Shared.Rent(byteCount);
        stampBytes = bytesMemoryOwner.Memory.Span[..byteCount];
        Encoding.UTF8.GetBytes(stampChars, stampBytes);

        return bytesMemoryOwner;
    }
}
