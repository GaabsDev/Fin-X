using FinX.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FinX.Tests
{
    public class FakeHttpClientFactory : System.Net.Http.IHttpClientFactory
    {
        public System.Net.Http.HttpClient CreateClient(string name) => new System.Net.Http.HttpClient();
    }

    public class ExternalExamsControllerTests
    {
        [Fact]
        public void GetExams_Is_Deterministic_For_Same_CPF()
        {
            var logger = new LoggerFactory().CreateLogger<ExternalExamsController>();
            var factory = new FakeHttpClientFactory();
            var ctrl = new ExternalExamsController(factory, logger);
            var r1 = ctrl.GetExams("12345678900") as OkObjectResult;
            var r2 = ctrl.GetExams("12345678900") as OkObjectResult;
            Assert.NotNull(r1);
            Assert.NotNull(r2);
            Assert.Equal(Newtonsoft.Json.JsonConvert.SerializeObject(r1!.Value), Newtonsoft.Json.JsonConvert.SerializeObject(r2!.Value));
        }
    }
}
