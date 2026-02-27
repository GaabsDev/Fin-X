using System.Threading.Tasks;

namespace FinX.Api.Services
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(string username, string password);
    }
}
