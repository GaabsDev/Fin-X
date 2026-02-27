using System.Security.Cryptography;

namespace FinX.Api.Services
{
    public interface IKeyService
    {
        RSA GetPrivateKey();
        RSA GetPublicKey();
        string GetKeyId();
    }
}
