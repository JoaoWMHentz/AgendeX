using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AgendeX.Infrastructure.Auth;

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

        PrivateKey = new RsaSecurityKey(_privateRsa);
        PublicKey = new RsaSecurityKey(_publicRsa);
    }

    public SecurityKey PrivateKey { get; }
    public SecurityKey PublicKey { get; }

    public void Dispose()
    {
        _privateRsa.Dispose();
        _publicRsa.Dispose();
    }
}
