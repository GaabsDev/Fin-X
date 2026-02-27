using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExternalExamsController : ControllerBase
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<ExternalExamsController> _log;

        public ExternalExamsController(IHttpClientFactory http, ILogger<ExternalExamsController> log)
        {
            _http = http;
            _log = log;
        }

        [HttpGet("{cpf}")]
        public IActionResult GetExams(string cpf)
        {
            var types = new[] { "Hemograma", "Colesterol", "Glicemia de Jejum", "Ureia/Creatinina", "TSH", "PCR", "Raio-X Torácico", "USG Abdominal", "Ressonância Magnética" };
            var labs = new[] { "LabVida", "Unilabs", "LabSaude", "InstitutoDiagnostico" };
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(cpf ?? string.Empty));
            var seed = System.BitConverter.ToInt32(hash, 0);
            var rng = new System.Random(seed);
            var count = rng.Next(1, 4);
            var exams = new System.Collections.Generic.List<object>();
            for (int i = 0; i < count; i++)
            {
                var type = types[rng.Next(types.Length)];
                var provider = labs[rng.Next(labs.Length)];
                var date = System.DateTime.UtcNow.AddDays(-rng.Next(1, 365)).ToString("yyyy-MM-dd");

                // Generate deterministic GUID based on CPF and exam index
                var examSeed = $"{cpf}-{i}";
                var examHash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(examSeed));
                var guidBytes = new byte[16];
                Array.Copy(examHash, guidBytes, 16);
                var examId = new System.Guid(guidBytes);

                object result;
                if (type == "Hemograma")
                {
                    var hb = Math.Round(11 + rng.NextDouble() * 6, 1);
                    result = new { Summary = hb < 12 ? "Baixa hemoglobina" : "Dentro do esperado", Values = new { Hemoglobina = hb, Leucocitos = rng.Next(4000, 11000) } };
                }
                else if (type == "Colesterol")
                {
                    var total = rng.Next(150, 280);
                    result = new { Summary = total >= 240 ? "Alto" : "Normal", Values = new { Total = total, HDL = rng.Next(35, 70), LDL = rng.Next(80, 190) } };
                }
                else if (type == "Glicemia de Jejum")
                {
                    var g = Math.Round(70 + rng.NextDouble() * 100, 1);
                    result = new { Summary = g >= 126 ? "Diabetes" : g >= 100 ? "Pré-diabetes" : "Normal", Values = new { Jejum = g } };
                }
                else if (type == "Raio-X Torácico" || type == "USG Abdominal" || type == "Ressonância Magnética")
                {
                    var findings = new[] { "Sem achados relevantes", "Opacidade focal discreta", "Alteração sugestiva de atelectasia", "Cisto simples" };
                    result = new { Summary = findings[rng.Next(findings.Length)], ReportUrl = (string?)null };
                }
                else
                {
                    result = new { Summary = "Normal", Values = System.Array.Empty<object>() };
                }

                exams.Add(new { Id = examId, Provider = provider, Date = date, Type = type, Result = result });
            }

            return Ok(new { cpf, exams });
        }
    }
}
