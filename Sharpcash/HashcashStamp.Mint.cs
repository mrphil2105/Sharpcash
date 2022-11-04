using System.Buffers;
using System.Text;

namespace Sharpcash;

public partial class HashcashStamp
{
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
