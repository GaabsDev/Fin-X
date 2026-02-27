using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FinX.Api.Services;

namespace FinX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var token = await _auth.AuthenticateAsync(req.Username, req.Password);
            if (token == null) return Unauthorized();
            return Ok(new { token });
        }

        public record LoginRequest(string Username, string Password);
    }
}
