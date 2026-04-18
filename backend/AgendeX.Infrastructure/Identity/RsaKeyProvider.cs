using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AgendeX.Infrastructure.Identity;

public sealed class RsaKeyProvider : IDisposable
{
    private readonly RSA _privateRsa;
    private readonly RSA _publicRsa;

    public RsaKeyProvider()
    {
        _privateRsa = RSA.Create(2048);

        RSAParameters keyParameters = _privateRsa.ExportParameters(true);

        _publicRsa = RSA.Create();
        _publicRsa.ImportParameters(new RSAParameters
        {
            Modulus = keyParameters.Modulus,
            Exponent = keyParameters.Exponent
        });

        string keyId = ComputeKeyId(keyParameters.Modulus!);

        PrivateKey = new RsaSecurityKey(_privateRsa) { KeyId = keyId };
        PublicKey = new RsaSecurityKey(_publicRsa) { KeyId = keyId };
    }

    public SecurityKey PrivateKey { get; }
    public SecurityKey PublicKey { get; }

    public void Dispose()
    {
        _privateRsa.Dispose();
        _publicRsa.Dispose();
    }

    private static string ComputeKeyId(byte[] modulus)
    {
        byte[] hash = SHA256.HashData(modulus);
        return Convert.ToBase64String(hash)[..16];
    }
}
