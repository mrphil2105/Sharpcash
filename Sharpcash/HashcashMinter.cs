using System.Buffers.Text;
using Sharpcash.Helpers;

namespace Sharpcash;

internal class HashcashMinter : IDisposable
{
    private readonly int _bytesToCheck;
    private readonly byte _remainderMask;

    private readonly HashAlgorithm _hasher;
    private readonly Memory<byte> _hash;

    public HashcashMinter(HashcashStamp stamp, HashAlgorithmName hashAlgorithm)
    {
        var remainderBits = stamp.Bits % 8;
        var hasher = CryptoHelper.CreateHashAlgorithm(hashAlgorithm);

        Stamp = stamp;

        _bytesToCheck = stamp.Bits / 8;
        _remainderMask = (byte)(0xFF << (8 - remainderBits));

        _hasher = hasher ??
            throw new CryptographicException("Unable to create an instance of the specified hash algorithm.");
        _hash = new byte[hasher.HashSize / 8];
    }

    public HashcashStamp Stamp { get; set; }

    public HashcashStamp Mint(long offset, long increment, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tuple = SerializeToUtf8Bytes();
        var stampBytes = tuple.StampBytes.Span;

        using (tuple.MemoryOwner)
        {
            var counterBase64Bytes = stampBytes.Slice(stampBytes.Length - MaxCounterLength, MaxCounterLength);
            var counterBytes = counterBase64Bytes[..sizeof(long)];

            var counter = Stamp.Counter + offset;

            while ((counter += increment) < long.MaxValue)
            {
                cancellationToken.ThrowIfCancellationRequested();

                BinaryPrimitives.WriteInt64LittleEndian(counterBytes, counter);
                var trimmedLength = BuffersHelper.TrimZeroPadding(counterBytes);
                Base64.EncodeToUtf8InPlace(counterBase64Bytes, trimmedLength, out var charsWritten);

                var trailingChars = MaxCounterLength - charsWritten;
                var currentStampBytes = stampBytes[..^trailingChars];

                if (!VerifyOnce(currentStampBytes))
                {
                    continue;
                }

                Stamp = new HashcashStamp(Stamp.Bits, Stamp.Date, Stamp.Resource, Stamp.Random, counter);

                return Stamp;
            }

            throw new CryptographicException("A solution could not be found.");
        }
    }

    public bool Verify()
    {
        var tuple = SerializeToUtf8Bytes();
        var stampBytes = tuple.StampBytes.Span;

        using (tuple.MemoryOwner)
        {
            var trimmedLength = BuffersHelper.TrimZeroPadding(stampBytes);
            stampBytes = stampBytes[..trimmedLength];

            return VerifyOnce(stampBytes);
        }
    }

    private bool VerifyOnce(ReadOnlySpan<byte> stampBytes)
    {
        var hash = _hash.Span;
        _hasher.TryComputeHash(stampBytes, hash, out _);

        for (var i = 0; i < _bytesToCheck; i++)
        {
            if (hash[i] != 0)
            {
                return false;
            }
        }

        return (hash[_bytesToCheck] & _remainderMask) == 0;
    }

    private (IDisposable MemoryOwner, Memory<byte> StampBytes) SerializeToUtf8Bytes()
    {
        var maxLength = Stamp.GetMaxLength();
        using var charsMemoryOwner = MemoryPool<char>.Shared.Rent(maxLength);
        var stampChars = charsMemoryOwner.Memory.Span[..maxLength];
        Stamp.TryFormat(stampChars, out _);

        var byteCount = Encoding.UTF8.GetByteCount(stampChars);
        var bytesMemoryOwner = MemoryPool<byte>.Shared.Rent(byteCount);
        var stampBytes = bytesMemoryOwner.Memory[..byteCount];
        Encoding.UTF8.GetBytes(stampChars, stampBytes.Span);

        return (bytesMemoryOwner, stampBytes);
    }

    public void Dispose()
    {
        _hasher.Dispose();
    }
}
