#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinX.Api.Models;
using FinX.Api.Services;

namespace FinX.Tests
{
    public class FakePatientService : IPatientService
    {
        private readonly List<Patient> _patients = new();
        private readonly List<MedicalRecord> _records = new();

        public Task<Patient> CreateAsync(Patient patient)
        {
            patient.Id = Guid.NewGuid();
            _patients.Add(patient);
            return Task.FromResult(patient);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var p = _patients.FirstOrDefault(x => x.Id == id);
            if (p == null) return Task.FromResult(false);
            _patients.Remove(p);
            _records.RemoveAll(r => r.PatientId == id);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<Patient>> GetAllAsync()
        {
            return Task.FromResult(_patients.AsEnumerable());
        }

        public Task<Patient?> GetAsync(Guid id)
        {
            return Task.FromResult(_patients.FirstOrDefault(x => x.Id == id));
        }

        public Task<bool> UpdateAsync(Patient patient)
        {
            var idx = _patients.FindIndex(p => p.Id == patient.Id);
            if (idx < 0) return Task.FromResult(false);
            _patients[idx] = patient;
            return Task.FromResult(true);
        }

        public Task<MedicalRecord> AddMedicalRecordAsync(Guid patientId, MedicalRecord record)
        {
            record.Id = Guid.NewGuid();
            record.PatientId = patientId;
            _records.Add(record);
            return Task.FromResult(record);
        }

        public Task<IEnumerable<MedicalRecord>> GetMedicalHistoryAsync(Guid patientId)
        {
            return Task.FromResult(_records.Where(r => r.PatientId == patientId).AsEnumerable());
        }
    }
}
