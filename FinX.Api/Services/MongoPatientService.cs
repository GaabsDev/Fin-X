using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using FinX.Api.Models;

namespace FinX.Api.Services
{
    public class MongoPatientService : IPatientService
    {
        private readonly IMongoCollection<Patient> _patients;
        private readonly IMongoCollection<MedicalRecord> _records;

        public MongoPatientService(IMongoDatabase db)
        {
            _patients = db.GetCollection<Patient>("patients");
            _records = db.GetCollection<MedicalRecord>("medicalrecords");
        }

        public async Task<Patient> CreateAsync(Patient patient)
        {
            patient.Id = Guid.NewGuid();
            await _patients.InsertOneAsync(patient);
            return patient;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var res = await _patients.DeleteOneAsync(p => p.Id == id);
            await _records.DeleteManyAsync(r => r.PatientId == id);
            return res.DeletedCount > 0;
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            var all = await _patients.Find(_ => true).ToListAsync();
            return all;
        }

        public async Task<Patient?> GetAsync(Guid id)
        {
            return await _patients.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(Patient patient)
        {
            var res = await _patients.ReplaceOneAsync(p => p.Id == patient.Id, patient);
            return res.ModifiedCount > 0 || res.MatchedCount > 0;
        }

        public async Task<MedicalRecord> AddMedicalRecordAsync(Guid patientId, MedicalRecord record)
        {
            record.Id = Guid.NewGuid();
            record.PatientId = patientId;
            await _records.InsertOneAsync(record);
            return record;
        }

        public async Task<IEnumerable<MedicalRecord>> GetMedicalHistoryAsync(Guid patientId)
        {
            return await _records.Find(r => r.PatientId == patientId).ToListAsync();
        }
    }
}
