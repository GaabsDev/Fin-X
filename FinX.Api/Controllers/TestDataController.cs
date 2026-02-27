using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinX.Api.Models;
using FinX.Api.Services;

namespace FinX.Api.Controllers
{
    /// <summary>
    /// Controller para demonstração e testes do DESAFIO 3 - preferred_slot_4196d208
    /// Criação de dados de teste e execução de scripts de unificação
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TestDataController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IHospitalService _hospitalService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IGrupoPacienteHospitalService _grupoService;
        private readonly IPatientUnificationService _unificationService;

        public TestDataController(
            IPatientService patientService,
            IHospitalService hospitalService,
            IAgendamentoService agendamentoService,
            IGrupoPacienteHospitalService grupoService,
            IPatientUnificationService unificationService)
        {
            _patientService = patientService;
            _hospitalService = hospitalService;
            _agendamentoService = agendamentoService;
            _grupoService = grupoService;
            _unificationService = unificationService;
        }

        /// <summary>
        /// DESAFIO 3: Cria dados de teste com pacientes duplicados
        /// </summary>
        [HttpPost("create-duplicate-patients")]
        public async Task<IActionResult> CreateDuplicatePatients()
        {
            var results = new List<object>();

            // Criar hospitais de teste
            var hospital1 = await _hospitalService.CreateAsync(new Hospital
            {
                Nome = "Hospital São Lucas",
                Cnpj = "11.111.111/0001-11"
            });

            var hospital2 = await _hospitalService.CreateAsync(new Hospital
            {
                Nome = "Hospital da Cidade",
                Cnpj = "22.222.222/0001-22"
            });

            results.Add(new { Type = "Hospital", Data = hospital1 });
            results.Add(new { Type = "Hospital", Data = hospital2 });

            // Criar pacientes duplicados (mesmo CPF)
            var baseDate = DateTime.UtcNow.AddDays(-30);

            // Maria Silva - 3 registros duplicados
            var maria1 = await _patientService.CreateAsync(new Patient
            {
                Name = "Maria Silva",
                CPF = "12345678901",
                DateOfBirth = new DateTime(1985, 5, 15),
                Contact = "maria1@email.com",
                DataCadastro = baseDate.AddDays(-10) // Mais antigo
            });

            var maria2 = await _patientService.CreateAsync(new Patient
            {
                Name = "Maria da Silva",
                CPF = "12345678901",
                DateOfBirth = new DateTime(1985, 5, 15),
                Contact = "maria.silva@email.com",
                DataCadastro = baseDate.AddDays(-5) // Meio termo
            });

            var maria3 = await _patientService.CreateAsync(new Patient
            {
                Name = "Maria Santos Silva",
                CPF = "12345678901",
                DateOfBirth = new DateTime(1985, 5, 15),
                Contact = "maria.santos@email.com | (11) 99999-9999",
                DataCadastro = baseDate // Mais recente - deve ser mantido
            });

            // João Santos - 2 registros duplicados
            var joao1 = await _patientService.CreateAsync(new Patient
            {
                Name = "João Santos",
                CPF = "98765432100",
                DateOfBirth = new DateTime(1990, 8, 20),
                Contact = "joao@email.com",
                DataCadastro = baseDate.AddDays(-15)
            });

            var joao2 = await _patientService.CreateAsync(new Patient
            {
                Name = "João Pedro Santos",
                CPF = "98765432100",
                DateOfBirth = new DateTime(1990, 8, 20),
                Contact = "joao.pedro@email.com | (11) 88888-8888",
                DataCadastro = baseDate.AddDays(-2) // Mais recente - deve ser mantido
            });

            results.Add(new { Type = "Patient", CPF = "12345678901", Data = new[] { maria1, maria2, maria3 } });
            results.Add(new { Type = "Patient", CPF = "98765432100", Data = new[] { joao1, joao2 } });

            // Criar vínculos hospital-paciente
            await _grupoService.CreateAsync(new GrupoPacienteHospital
            {
                PacienteId = maria1.Id,
                HospitalId = hospital1.Id,
                Codigo = "MARIA001"
            });

            await _grupoService.CreateAsync(new GrupoPacienteHospital
            {
                PacienteId = maria2.Id,
                HospitalId = hospital2.Id,
                Codigo = "MARIA002"
            });

            await _grupoService.CreateAsync(new GrupoPacienteHospital
            {
                PacienteId = joao1.Id,
                HospitalId = hospital1.Id,
                Codigo = "JOAO001"
            });

            // Criar agendamentos
            await _agendamentoService.CreateAsync(new Agendamento
            {
                HospitalId = hospital1.Id,
                PacienteId = maria1.Id,
                Data = DateTime.UtcNow.AddDays(7),
                Descricao = "Consulta cardiologia",
                Status = "Agendado"
            });

            await _agendamentoService.CreateAsync(new Agendamento
            {
                HospitalId = hospital2.Id,
                PacienteId = maria2.Id,
                Data = DateTime.UtcNow.AddDays(14),
                Descricao = "Exame de sangue",
                Status = "Agendado"
            });

            return Ok(new
            {
                Message = "Dados de teste criados com sucesso!",
                Summary = "Pacientes duplicados: Maria Silva (3x), João Santos (2x)",
                Details = results,
                NextStep = "Use POST /api/patients/unify-duplicates para unificar os pacientes duplicados"
            });
        }

        /// <summary>
        /// DESAFIO 3: Executa script completo de demonstração
        /// </summary>
        [HttpPost("run-unification-demo")]
        public async Task<IActionResult> RunUnificationDemo()
        {
            var results = new
            {
                Step1_BeforeUnification = await _unificationService.FindDuplicatePatientsByCpfAsync(),
                Step2_UnificationResult = await _unificationService.UnifyDuplicatePatientsByCpfAsync(),
                Step3_AfterUnification = await _unificationService.FindDuplicatePatientsByCpfAsync()
            };

            return Ok(new
            {
                Message = "Demonstração de unificação completada!",
                Results = results,
                Explanation = new
                {
                    Rule = "Pacientes com mesmo CPF são unificados, mantendo o cadastrado mais recentemente (DataCadastro)",
                    Process = "1. Identifica duplicatas por CPF, 2. Mantém o mais recente, 3. Atualiza referências, 4. Remove duplicatas"
                }
            });
        }
    }
}