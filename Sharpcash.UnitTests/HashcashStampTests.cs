namespace Sharpcash.UnitTests;

public class HashcashStampTests
{
    [Theory]
    [HashcashData]
    public void ToString_ReturnsStampString_GivenDetails(HashcashStamp stamp)
    {
        var dateString = stamp.Date.ToString(DateFormat);
        var counterChars = new char[MaxCounterLength];
        var base64Length = BuffersHelper.GetBase64(stamp.Counter, counterChars);
        var counterBase64 = new string(counterChars[..base64Length]);
        var expected = $"{stamp.Version}:{stamp.Bits}:{dateString}:{stamp.Resource}::{stamp.Random}:{counterBase64}";

        var actual = stamp.ToString();

        actual.Should().Be(expected);
    }
}
