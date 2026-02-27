#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinX.Api.Models;
using FinX.Api.Services;

namespace FinX.Tests.Fakes
{
    public class FakePatientService : IPatientService
    {
        private readonly ConcurrentDictionary<Guid, Patient> _store = new();
        private readonly ConcurrentDictionary<Guid, System.Collections.Generic.List<MedicalRecord>> _records = new();

        public Task<Patient> CreateAsync(Patient patient)
        {
            patient.Id = Guid.NewGuid();
            _store[patient.Id] = patient;
            return Task.FromResult(patient);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            return Task.FromResult(_store.TryRemove(id, out _));
        }

        public Task<IEnumerable<Patient>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Patient>>(_store.Values.ToList());
        }

        public Task<Patient?> GetAsync(Guid id)
        {
            _store.TryGetValue(id, out var p);
            return Task.FromResult(p);
        }

        public Task<MedicalRecord> AddMedicalRecordAsync(Guid patientId, MedicalRecord record)
        {
            record.Id = Guid.NewGuid();
            record.PatientId = patientId;
            record.Date = record.Date == default ? DateTime.UtcNow : record.Date;
            if (_store.ContainsKey(patientId))
            {
                var list = _records.GetOrAdd(patientId, _ => new System.Collections.Generic.List<MedicalRecord>());
                list.Add(record);
                return Task.FromResult(record);
            }
            throw new KeyNotFoundException();
        }

        public Task<IEnumerable<MedicalRecord>> GetMedicalHistoryAsync(Guid patientId)
        {
            if (_store.ContainsKey(patientId) && _records.TryGetValue(patientId, out var list))
            {
                return Task.FromResult<IEnumerable<MedicalRecord>>(list.ToList());
            }
            return Task.FromResult<IEnumerable<MedicalRecord>>(Array.Empty<MedicalRecord>());
        }

        public Task<bool> UpdateAsync(Patient patient)
        {
            if (!_store.ContainsKey(patient.Id)) return Task.FromResult(false);
            _store[patient.Id] = patient;
            return Task.FromResult(true);
        }
    }
}
