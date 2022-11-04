using System.Security.Cryptography;

namespace Sharpcash;

internal readonly ref struct StampValidator
{
    private readonly int _bytesToCheck;
    private readonly byte _remainderMask;

    private readonly HashAlgorithm _hasher;
    private readonly Span<byte> _hash;

    public StampValidator(int bits, HashAlgorithm hasher, Span<byte> hash)
    {
        _bytesToCheck = bits / 8;
        var remainderBits = bits % 8;
        _remainderMask = (byte)(0xFF << (8 - remainderBits));

        _hasher = hasher;
        _hash = hash;
    }

    public bool VerifyOnce(ReadOnlySpan<byte> stampBytes)
    {
        _hasher.TryComputeHash(stampBytes, _hash, out _);

        for (var i = 0; i < _bytesToCheck; i++)
        {
            if (_hash[i] != 0)
            {
                return false;
            }
        }

        return (_hash[_bytesToCheck] & _remainderMask) == 0;
    }
}
