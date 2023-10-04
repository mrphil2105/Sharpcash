using System.Buffers.Binary;

namespace Sharpcash.Helpers;

internal static class BuffersHelper
{
    public static int TrimZeroPadding(ReadOnlySpan<byte> buffer)
    {
        var index = buffer.Length - 1;

        while (index >= 0 && buffer[index] == 0)
        {
            index--;
        }

        return index + 1;
    }

    public static int GetBase64(long value, Span<char> destination)
    {
        Span<byte> valueBytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(valueBytes, value);

        var trimmedLength = TrimZeroPadding(valueBytes);
        valueBytes = valueBytes[..trimmedLength];

        var success = Convert.TryToBase64Chars(valueBytes, destination, out var charsWritten);
        Debug.Assert(success);

        return charsWritten;
    }
}
