using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using FinX.Api.Models;
using Microsoft.Extensions.Logging;

namespace FinX.Scripts
{
    /// <summary>
    /// Script para criar dados de teste com pacientes duplicados
    /// Útil para demonstrar e testar o processo de unificação
    /// </summary>
    public class CreateTestData
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<CreateTestData> _logger;

        private readonly IMongoCollection<Patient> _patients;
        private readonly IMongoCollection<Hospital> _hospitals;
        private readonly IMongoCollection<GrupoPacienteHospital> _grupos;
        private readonly IMongoCollection<Agendamento> _agendamentos;

        public CreateTestData(IMongoDatabase database, ILogger<CreateTestData> logger)
        {
            _database = database;
            _logger = logger;

            _patients = _database.GetCollection<Patient>("patients");
            _hospitals = _database.GetCollection<Hospital>("hospitals");
            _grupos = _database.GetCollection<GrupoPacienteHospital>("grupospacientehospital");
            _agendamentos = _database.GetCollection<Agendamento>("agendamentos");
        }

        /// <summary>
        /// Cria cenário de teste completo com pacientes duplicados
        /// </summary>
        public async Task<TestDataResult> ExecuteAsync(bool clearExistingData = true)
        {
            var result = new TestDataResult();

            try
            {
                _logger.LogInformation("[TESTE] Iniciando criação de dados de teste para DESAFIO 3");

                // 1. Limpar dados existentes se solicitado
                if (clearExistingData)
                {
                    await ClearExistingDataAsync();
                }

                // 2. Criar hospitais
                var hospitals = await CreateHospitalsAsync();
                result.HospitalsCreated = hospitals.Count;

                // 3. Criar pacientes duplicados
                var patients = await CreateDuplicatePatientsAsync();
                result.PatientsCreated = patients.Count;

                // 4. Criar vínculos hospital-paciente
                var grupos = await CreatePatientHospitalGroupsAsync(hospitals, patients);
                result.GroupsCreated = grupos.Count;

                // 5. Criar agendamentos
                var agendamentos = await CreateAppointmentsAsync(hospitals, patients);
                result.AppointmentsCreated = agendamentos.Count;

                _logger.LogInformation("[TESTE] Dados de teste criados com sucesso");
                _logger.LogInformation($"  - Hospitais: {result.HospitalsCreated}");
                _logger.LogInformation($"  - Pacientes (c/ duplicatas): {result.PatientsCreated}");
                _logger.LogInformation($"  - Vínculos: {result.GroupsCreated}");
                _logger.LogInformation($"  - Agendamentos: {result.AppointmentsCreated}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar dados de teste");
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        private async Task ClearExistingDataAsync()
        {
            _logger.LogInformation("Limpando dados existentes...");

            await _agendamentos.DeleteManyAsync(_ => true);
            await _grupos.DeleteManyAsync(_ => true);
            await _patients.DeleteManyAsync(_ => true);
            await _hospitals.DeleteManyAsync(_ => true);
        }

        private async Task<List<Hospital>> CreateHospitalsAsync()
        {
            var hospitals = new List<Hospital>
            {
                new Hospital
                {
                    Id = Guid.NewGuid(),
                    Nome = "Hospital São Lucas",
                    Cnpj = "11.111.111/0001-11"
                },
                new Hospital
                {
                    Id = Guid.NewGuid(),
                    Nome = "Hospital da Cidade",
                    Cnpj = "22.222.222/0001-22"
                },
                new Hospital
                {
                    Id = Guid.NewGuid(),
                    Nome = "Hospital Regional",
                    Cnpj = "33.333.333/0001-33"
                }
            };

            await _hospitals.InsertManyAsync(hospitals);
            _logger.LogInformation($"Criados {hospitals.Count} hospitais");

            return hospitals;
        }

        private async Task<List<Patient>> CreateDuplicatePatientsAsync()
        {
            var baseDate = DateTime.UtcNow.AddDays(-30);
            var patients = new List<Patient>();

            // CENÁRIO 1: Maria Silva - 3 duplicatas (mesmo CPF)
            var mariaCPF = "12345678901";
            patients.AddRange(new[]
            {
                new Patient
                {
                    Id = Guid.NewGuid(),
                    Name = "Maria Silva",
                    CPF = mariaCPF,
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Contact = "maria1@email.com",
                    DataCadastro = baseDate.AddDays(-20) // Mais antigo
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    Name = "Maria da Silva",
                    CPF = mariaCPF,
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Contact = "maria.silva@email.com",
                    DataCadastro = baseDate.AddDays(-10) // Meio termo
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    Name = "Maria Santos Silva",
                    CPF = mariaCPF,
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Contact = "maria.santos@email.com | (11) 99999-9999",
                    DataCadastro = baseDate // Mais recente - DEVE SER MANTIDA
                }
            });

            // CENÁRIO 2: João Santos - 2 duplicatas (mesmo CPF)
            var joaoCPF = "98765432100";
            patients.AddRange(new[]
            {
                new Patient
                {
                    Id = Guid.NewGuid(),
                    Name = "João Santos",
                    CPF = joaoCPF,
                    DateOfBirth = new DateTime(1990, 8, 20),
                    Contact = "joao@email.com",
                    DataCadastro = baseDate.AddDays(-15) // Mais antigo
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    Name = "João Pedro Santos",
                    CPF = joaoCPF,
                    DateOfBirth = new DateTime(1990, 8, 20),
                    Contact = "joao.pedro@email.com | (11) 88888-8888",
                    DataCadastro = baseDate.AddDays(-2) // Mais recente - DEVE SER MANTIDO
                }
            });

            // CENÁRIO 3: Ana Costa - 4 duplicatas (mesmo CPF)
            var anaCPF = "11122233344";
            patients.AddRange(new[]
            {
                new Patient
                {
                    Id = Guid.NewGuid(),
                    Name = "Ana Costa",
                    CPF = anaCPF,
                    DateOfBirth = new DateTime(1978, 12, 3),
                    Contact = "ana1@email.com",
                    DataCadastro = baseDate.AddDays(-25)
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    Name = "Ana C. Silva",
                    CPF = anaCPF,
                    DateOfBirth = new DateTime(1978, 12, 3),
                    Contact = "ana.costa@email.com",
                    DataCadastro = baseDate.AddDays(-18)
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    Name = "Ana Silva Costa",
                    CPF = anaCPF,
                    DateOfBirth = new DateTime(1978, 12, 3),
                    Contact = "anasilva@email.com",
                    DataCadastro = baseDate.AddDays(-8)
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    Name = "Ana Costa Silva",
                    CPF = anaCPF,
                    DateOfBirth = new DateTime(1978, 12, 3),
                    Contact = "ana.costa.silva@email.com | (11) 77777-7777",
                    DataCadastro = baseDate.AddDays(-1) // Mais recente - DEVE SER MANTIDA
                }
            });

            // Paciente sem duplicatas (controle)
            patients.Add(new Patient
            {
                Id = Guid.NewGuid(),
                Name = "Carlos Oliveira",
                CPF = "55544433322",
                DateOfBirth = new DateTime(1992, 3, 10),
                Contact = "carlos@email.com",
                DataCadastro = baseDate.AddDays(-5)
            });

            await _patients.InsertManyAsync(patients);
            _logger.LogInformation($"Criados {patients.Count} pacientes (incluindo duplicatas)");
            _logger.LogInformation($"  - Maria Silva: 3 duplicatas (CPF: {mariaCPF})");
            _logger.LogInformation($"  - João Santos: 2 duplicatas (CPF: {joaoCPF})");
            _logger.LogInformation($"  - Ana Costa: 4 duplicatas (CPF: {anaCPF})");
            _logger.LogInformation($"  - Carlos Oliveira: sem duplicatas");

            return patients;
        }

        private async Task<List<GrupoPacienteHospital>> CreatePatientHospitalGroupsAsync(
            List<Hospital> hospitals, List<Patient> patients)
        {
            var grupos = new List<GrupoPacienteHospital>();
            var random = new Random();

            // Criar vínculos para alguns pacientes duplicados
            // Isso testa se os vínculos são corretamente atualizados na unificação

            foreach (var patient in patients.Take(8)) // Vincular os primeiros 8 pacientes
            {
                var hospital = hospitals[random.Next(hospitals.Count)];

                grupos.Add(new GrupoPacienteHospital
                {
                    Id = Guid.NewGuid(),
                    PacienteId = patient.Id,
                    HospitalId = hospital.Id,
                    Codigo = $"PAC{random.Next(1000, 9999)}"
                });
            }

            await _grupos.InsertManyAsync(grupos);
            _logger.LogInformation($"Criados {grupos.Count} vínculos paciente-hospital");

            return grupos;
        }

        private async Task<List<Agendamento>> CreateAppointmentsAsync(
            List<Hospital> hospitals, List<Patient> patients)
        {
            var agendamentos = new List<Agendamento>();
            var random = new Random();
            var baseDate = DateTime.UtcNow;

            var tiposConsulta = new[]
            {
                "Consulta de rotina",
                "Exame cardiológico",
                "Consulta neurológica",
                "Exame de sangue",
                "Retorno médico"
            };

            // Criar agendamentos para pacientes duplicados
            // Isso testa se os agendamentos são corretamente atualizados na unificação

            foreach (var patient in patients.Take(6)) // Agendar para os primeiros 6 pacientes
            {
                var hospital = hospitals[random.Next(hospitals.Count)];

                agendamentos.Add(new Agendamento
                {
                    Id = Guid.NewGuid(),
                    HospitalId = hospital.Id,
                    PacienteId = patient.Id,
                    Data = baseDate.AddDays(random.Next(1, 30)),
                    Descricao = tiposConsulta[random.Next(tiposConsulta.Length)],
                    Status = "Agendado"
                });
            }

            await _agendamentos.InsertManyAsync(agendamentos);
            _logger.LogInformation($"Criados {agendamentos.Count} agendamentos");

            return agendamentos;
        }
    }

    public class TestDataResult
    {
        public int HospitalsCreated { get; set; }
        public int PatientsCreated { get; set; }
        public int GroupsCreated { get; set; }
        public int AppointmentsCreated { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}