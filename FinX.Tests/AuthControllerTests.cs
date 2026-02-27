#nullable enable
using System.Threading.Tasks;
using FinX.Api.Controllers;
using FinX.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FinX.Tests
{
    public class FakeAuthService : IAuthService
    {
        public Task<string?> AuthenticateAsync(string username, string password)
        {
            if (username == "admin" && password == "password") return Task.FromResult<string?>("fake-token");
            return Task.FromResult<string?>(null);
        }
    }

    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_Returns_Token_On_Valid_Credentials()
        {
            var svc = new FakeAuthService();
            var ctrl = new AuthController(svc);
            var req = new AuthController.LoginRequest("admin", "password");
            var res = await ctrl.Login(req) as OkObjectResult;
            Assert.NotNull(res);
            Assert.NotNull(res!.Value);
        }

        [Fact]
        public async Task Login_Returns_Unauthorized_On_Invalid_Credentials()
        {
            var svc = new FakeAuthService();
            var ctrl = new AuthController(svc);
            var req = new AuthController.LoginRequest("x", "y");
            var res = await ctrl.Login(req) as UnauthorizedResult;
            Assert.NotNull(res);
        }
    }
}
