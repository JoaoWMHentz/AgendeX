using AgendeX.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace AgendeX.Tests.Infrastructure.Auth;

public sealed class RsaKeyProviderTests
{
    [Fact]
    public void Constructor_GeneratesNonNullPrivateAndPublicKeys()
    {
        using RsaKeyProvider rsaKeyProvider = new();

        rsaKeyProvider.PrivateKey.Should().NotBeNull();
        rsaKeyProvider.PublicKey.Should().NotBeNull();
    }

    [Fact]
    public void PrivateKey_IsRsaSecurityKey()
    {
        using RsaKeyProvider rsaKeyProvider = new();

        rsaKeyProvider.PrivateKey.Should().BeOfType<RsaSecurityKey>();
    }

    [Fact]
    public void PublicKey_IsRsaSecurityKey()
    {
        using RsaKeyProvider rsaKeyProvider = new();

        rsaKeyProvider.PublicKey.Should().BeOfType<RsaSecurityKey>();
    }

    [Fact]
    public void PrivateAndPublicKeys_AreDifferentInstances()
    {
        using RsaKeyProvider rsaKeyProvider = new();

        rsaKeyProvider.PrivateKey.Should().NotBeSameAs(rsaKeyProvider.PublicKey);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        RsaKeyProvider rsaKeyProvider = new();

        Action act = () => rsaKeyProvider.Dispose();

        act.Should().NotThrow();
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
