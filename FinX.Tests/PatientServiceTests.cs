using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FinX.Api.Services;
using FinX.Api;
using FinX.Api.Models;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FinX.Tests
{
    public class FakeKeyService : IKeyService
    {
        private readonly RSA _rsa;
        private readonly string _keyId;

        public FakeKeyService()
        {
            _rsa = RSA.Create(2048);
            _keyId = "test-key-id";
        }

        public RSA GetPrivateKey() => _rsa;
        public RSA GetPublicKey() => _rsa;
        public string GetKeyId() => _keyId;
    }
    public class PatientServiceTests
    {
        [Fact]
        public async Task AuthService_ReturnsToken_ForValidCredentials()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] {
                    new KeyValuePair<string, string>("Jwt:Key", "testkey1234567890"),
                    new KeyValuePair<string, string>("Jwt:Issuer", "FinXApi")
                })
                .Build();
            var keyService = new FakeKeyService();
            var auth = new AuthService(keyService, config);
            var token = await auth.AuthenticateAsync("admin", "password");
            Assert.False(string.IsNullOrWhiteSpace(token));
        }
    }
}
