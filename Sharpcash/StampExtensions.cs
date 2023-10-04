using System.Buffers.Binary;
using Sharpcash.Helpers;

namespace Sharpcash;

public static class StampExtensions
{
    public static int GetLength(this HashcashStamp stamp)
    {
        if (stamp is null)
        {
            throw new ArgumentNullException(nameof(stamp));
        }

        Span<byte> counterBytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(counterBytes, stamp.Counter);

        var counterLength = BuffersHelper.TrimZeroPadding(counterBytes);
        counterLength = (int)Math.Ceiling(counterLength / 3.0) * 4;

        return BasicLength + stamp.Resource.Length + stamp.Random.Length + counterLength;
    }
}
