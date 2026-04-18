using AgendeX.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace AgendeX.Tests.Infrastructure.Auth;

public sealed class RsaKeyProviderTests
{
    [Fact]
    public void Constructor_BothKeysAreNonNullRsaSecurityKeys()
    {
        using RsaKeyProvider rsaKeyProvider = new();

        rsaKeyProvider.PrivateKey.Should().NotBeNull().And.BeOfType<RsaSecurityKey>();
        rsaKeyProvider.PublicKey.Should().NotBeNull().And.BeOfType<RsaSecurityKey>();
    }

    [Fact]
    public void MultipleInstances_GenerateDifferentKeyModuli()
    {
        using RsaKeyProvider first = new();
        using RsaKeyProvider second = new();

        byte[]? firstModulus = ((RsaSecurityKey)first.PrivateKey).Rsa.ExportParameters(false).Modulus;
        byte[]? secondModulus = ((RsaSecurityKey)second.PrivateKey).Rsa.ExportParameters(false).Modulus;

        firstModulus.Should().NotBeEquivalentTo(secondModulus);
    }
}
