using System.Security.Cryptography;
using System.Text;

namespace Sharpcash.UnitTests;

public class HashcashStampTests
{
    private static readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA256;

    [Theory]
    [HashcashData]
    public async Task MintAsync_CreatesZeroBits_SingleThreaded(HashcashStamp stamp)
    {
        var bytesToCheck = stamp.Bits / 8;
        var remainderBits = stamp.Bits % 8;
        var remainderMask = (byte)(0xFF << (8 - remainderBits));

        var mintedStamp = await stamp.MintAsync(_hashAlgorithm);

        using var hasher = CryptoHelper.CreateHashAlgorithm(_hashAlgorithm);
        var stampString = mintedStamp.ToString();
        var stampBytes = Encoding.UTF8.GetBytes(stampString);
        var hash = hasher!.ComputeHash(stampBytes);

        hash.Take(bytesToCheck).Should().OnlyContain(b => b == 0);
        (hash[bytesToCheck] & remainderMask).Should().Be(0);
    }

    [Theory]
    [HashcashData]
    public async Task MintAsync_MintsStamp_SingleThreaded(HashcashStamp stamp)
    {
        var mintedStamp = await stamp.MintAsync(_hashAlgorithm);

        var isValid = mintedStamp.Verify(_hashAlgorithm);
        isValid.Should().BeTrue();
    }

    [Theory]
    [HashcashData]
    public async Task MintAsync_MintsStamp_MultiThreaded(HashcashStamp stamp)
    {
        var mintedStamp = await stamp.MintAsync(_hashAlgorithm, Environment.ProcessorCount);

        var isValid = mintedStamp.Verify(_hashAlgorithm);
        isValid.Should().BeTrue();
    }

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
            .WithMessage("The specified hashcash stamp string is invalid.");
    }

    [Fact]
    public void FooTest()
    {
        var bytes = new byte[20];
        var foo = Convert.TryFromBase64String("ePa=", bytes, out var written);

        // var stamp = HashcashStamp.Parse("1:20:1303030600:anni@cypherspace.org::McMybZIhxKXu57jd:ckvi");
        var stamp = HashcashStamp.Parse("1:20:060408:anni@cypherspace.org::1QTjaYd7niiQA/sc:ePa");

        var isValid = stamp.Verify(HashAlgorithmName.SHA1);

        isValid.Should().BeTrue();
    }
}
