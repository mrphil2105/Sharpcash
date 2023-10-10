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

    public static long GetInt64(ReadOnlySpan<char> base64)
    {
        Span<byte> valueBytes = stackalloc byte[sizeof(long)];

        if (Convert.TryFromBase64Chars(base64, valueBytes, out _))
        {
            return BinaryPrimitives.ReadInt64LittleEndian(valueBytes);
        }

        throw new ArgumentException("The destination buffer contains invalid base-64 characters or is too long.",
            nameof(base64));
    }
}
