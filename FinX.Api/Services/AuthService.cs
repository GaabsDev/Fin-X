using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FinX.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IKeyService _keys;
        private readonly string _issuer;

        public AuthService(IKeyService keys, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _keys = keys;
            _issuer = config["Jwt:Issuer"] ?? "FinXApi";
        }

        public Task<string?> AuthenticateAsync(string username, string password)
        {
            if (username != "admin" || password != "password") return Task.FromResult<string?>(null);

            var rsa = _keys.GetPrivateKey();
            var securityKey = new Microsoft.IdentityModel.Tokens.RsaSecurityKey(rsa) { KeyId = _keys.GetKeyId() };
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            var claims = new[] { new Claim(ClaimTypes.Name, username), new Claim("role", "admin") };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Task.FromResult<string?>(tokenString);
        }
    }
}
