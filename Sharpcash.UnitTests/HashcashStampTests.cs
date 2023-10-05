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

    [Theory]
    [HashcashData]
    public void Parse_ReturnsStamp_GivenValidStampString(HashcashStamp stamp)
    {
        var stampString = stamp.ToString();

        var otherStamp = HashcashStamp.Parse(stampString);

        otherStamp.Should().BeEquivalentTo(stamp);
    }

    [Theory]
    [AutoData]
    public void Parse_ThrowsArgumentException_GivenInvalidStampString(string invalid)
    {
        var act = () => HashcashStamp.Parse(invalid);

        act.Should()
            .Throw<ArgumentException>()
            .Where(e => e.Message.StartsWith("The specified hashcash stamp string is invalid."));
    }
}
