using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using FinX.Api.Models;
using Microsoft.Extensions.Logging;

namespace FinX.Api.Services
{
    public interface IPatientUnificationService
    {
        Task<PatientUnificationResult> UnifyDuplicatePatientsByCpfAsync();
        Task<PatientUnificationResult> UnifyDuplicatePatientsByCpfAsync(string cpf);
        Task<List<Patient>> FindDuplicatePatientsByCpfAsync();
        Task<List<Patient>> FindDuplicatePatientsByCpfAsync(string cpf);
    }

    public class PatientUnificationResult
    {
        public int TotalDuplicatesFound { get; set; }
        public int PatientsRemoved { get; set; }
        public int PatientsKept { get; set; }
        public int GroupsUpdated { get; set; }
        public int AppointmentsUpdated { get; set; }
        public List<string> ProcessedCpfs { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class PatientUnificationService : IPatientUnificationService
    {
        private readonly IMongoCollection<Patient> _patients;
        private readonly IMongoCollection<GrupoPacienteHospital> _grupos;
        private readonly IMongoCollection<Agendamento> _agendamentos;
        private readonly IMongoCollection<MedicalRecord> _records;
        private readonly ILogger<PatientUnificationService> _logger;

        public PatientUnificationService(
            IMongoDatabase db,
            ILogger<PatientUnificationService> logger)
        {
            _patients = db.GetCollection<Patient>("patients");
            _grupos = db.GetCollection<GrupoPacienteHospital>("grupospacientehospital");
            _agendamentos = db.GetCollection<Agendamento>("agendamentos");
            _records = db.GetCollection<MedicalRecord>("medicalrecords");
            _logger = logger;
        }

        public async Task<List<Patient>> FindDuplicatePatientsByCpfAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    ["_id"] = "$CPF",
                    ["count"] = new BsonDocument("$sum", 1),
                    ["patients"] = new BsonDocument("$push", "$$ROOT")
                }),
                new BsonDocument("$match", new BsonDocument("count", new BsonDocument("$gt", 1))),
                new BsonDocument("$unwind", "$patients"),
                new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$patients"))
            };

            return await _patients.Aggregate<Patient>(pipeline).ToListAsync();
        }

        public async Task<List<Patient>> FindDuplicatePatientsByCpfAsync(string cpf)
        {
            return await _patients.Find(p => p.CPF == cpf).ToListAsync();
        }

        public async Task<PatientUnificationResult> UnifyDuplicatePatientsByCpfAsync()
        {
            var result = new PatientUnificationResult();

            try
            {
                // Find all CPFs with duplicates
                var duplicateCpfs = await GetDuplicateCpfsAsync();

                foreach (var cpf in duplicateCpfs)
                {
                    var cpfResult = await UnifyDuplicatePatientsByCpfAsync(cpf);
                    result.TotalDuplicatesFound += cpfResult.TotalDuplicatesFound;
                    result.PatientsRemoved += cpfResult.PatientsRemoved;
                    result.PatientsKept += cpfResult.PatientsKept;
                    result.GroupsUpdated += cpfResult.GroupsUpdated;
                    result.AppointmentsUpdated += cpfResult.AppointmentsUpdated;
                    result.ProcessedCpfs.AddRange(cpfResult.ProcessedCpfs);
                    result.Errors.AddRange(cpfResult.Errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during patient unification process");
                result.Errors.Add($"General error: {ex.Message}");
            }

            return result;
        }

        public async Task<PatientUnificationResult> UnifyDuplicatePatientsByCpfAsync(string cpf)
        {
            var result = new PatientUnificationResult();

            try
            {
                var duplicatePatients = await FindDuplicatePatientsByCpfAsync(cpf);

                if (duplicatePatients.Count <= 1)
                {
                    return result; // No duplicates found
                }

                result.TotalDuplicatesFound = duplicatePatients.Count;
                result.ProcessedCpfs.Add(cpf);

                // Keep the patient with the latest DataCadastro
                var patientToKeep = duplicatePatients.OrderByDescending(p => p.DataCadastro).First();
                var patientsToRemove = duplicatePatients.Where(p => p.Id != patientToKeep.Id).ToList();

                _logger.LogInformation($"Unifying {duplicatePatients.Count} patients with CPF {cpf}. Keeping patient {patientToKeep.Id} (registered on {patientToKeep.DataCadastro})");

                // Update all related records to point to the kept patient
                foreach (var patientToRemove in patientsToRemove)
                {
                    // Update GrupoPacienteHospital records
                    var groupsFilter = Builders<GrupoPacienteHospital>.Filter.Eq(g => g.PacienteId, patientToRemove.Id);
                    var groupsUpdate = Builders<GrupoPacienteHospital>.Update.Set(g => g.PacienteId, patientToKeep.Id);
                    var groupsResult = await _grupos.UpdateManyAsync(groupsFilter, groupsUpdate);
                    result.GroupsUpdated += (int)groupsResult.ModifiedCount;

                    // Update Agendamento records
                    var appointmentsFilter = Builders<Agendamento>.Filter.Eq(a => a.PacienteId, patientToRemove.Id);
                    var appointmentsUpdate = Builders<Agendamento>.Update.Set(a => a.PacienteId, patientToKeep.Id);
                    var appointmentsResult = await _agendamentos.UpdateManyAsync(appointmentsFilter, appointmentsUpdate);
                    result.AppointmentsUpdated += (int)appointmentsResult.ModifiedCount;

                    // Update MedicalRecord records
                    var recordsFilter = Builders<MedicalRecord>.Filter.Eq(r => r.PatientId, patientToRemove.Id);
                    var recordsUpdate = Builders<MedicalRecord>.Update.Set(r => r.PatientId, patientToKeep.Id);
                    await _records.UpdateManyAsync(recordsFilter, recordsUpdate);

                    // Remove duplicate patient
                    await _patients.DeleteOneAsync(p => p.Id == patientToRemove.Id);
                    result.PatientsRemoved++;

                    _logger.LogInformation($"Removed duplicate patient {patientToRemove.Id} and updated related records");
                }

                result.PatientsKept = 1;

                // Remove duplicate GrupoPacienteHospital entries for the same patient-hospital combination
                await RemoveDuplicateGruposForPatientAsync(patientToKeep.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unifying patients with CPF {cpf}");
                result.Errors.Add($"Error processing CPF {cpf}: {ex.Message}");
            }

            return result;
        }

        private async Task<List<string>> GetDuplicateCpfsAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    ["_id"] = "$CPF",
                    ["count"] = new BsonDocument("$sum", 1)
                }),
                new BsonDocument("$match", new BsonDocument("count", new BsonDocument("$gt", 1)))
            };

            var results = await _patients.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => doc["_id"].AsString).ToList();
        }

        private async Task RemoveDuplicateGruposForPatientAsync(Guid patientId)
        {
            var grupos = await _grupos.Find(g => g.PacienteId == patientId).ToListAsync();
            var uniqueGroups = grupos
                .GroupBy(g => new { g.PacienteId, g.HospitalId })
                .Where(group => group.Count() > 1)
                .ToList();

            foreach (var group in uniqueGroups)
            {
                var groupsToKeep = group.OrderByDescending(g => g.Id).First();
                var groupsToRemove = group.Where(g => g.Id != groupsToKeep.Id).ToList();

                foreach (var groupToRemove in groupsToRemove)
                {
                    await _grupos.DeleteOneAsync(g => g.Id == groupToRemove.Id);
                }
            }
        }
    }
}