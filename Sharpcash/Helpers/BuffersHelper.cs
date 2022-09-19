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
}
