using AutoFixture;

namespace Sharpcash.UnitTests;

public class HashcashDataAttribute : AutoDataAttribute
{
    public HashcashDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Register((DateTime date, string resource, string random, long counter) =>
            new HashcashStamp(10, date, resource, random, counter));

        return fixture;
    }
}
