using EntityFrameworkCore.DataProtection.Extensions;
using FluentAssertions;

namespace Klean.EntityFrameworkCore.DataProtection.Test;

internal sealed class StringExtTests
{
    [OneTimeSetUp]
    public void InitializeEnv()
    {
        var value = Environment.GetEnvironmentVariable(StringExt.HashingSaltKey);
        Environment.SetEnvironmentVariable(StringExt.HashingSaltKey, value ?? Guid.NewGuid().ToString());
    }

    [Test]
    public void Test_HmacSha256_Is_Deterministic()
    {
        var payload = "hello world";

        var hash = payload.HmacSha256Hash();
        Console.WriteLine(hash);

        var hash2 = payload.HmacSha256Hash();
        Console.WriteLine(hash2);

        hash.Should().BeEquivalentTo(hash2);
    }
}