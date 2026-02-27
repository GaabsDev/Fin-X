using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinX.Api.Models;

namespace FinX.Api.Services
{
    public interface IPatientService
    {
        Task<Patient> CreateAsync(Patient patient);
        Task<Patient?> GetAsync(Guid id);
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<bool> UpdateAsync(Patient patient);
        Task<bool> DeleteAsync(Guid id);
        Task<MedicalRecord> AddMedicalRecordAsync(Guid patientId, MedicalRecord record);
        Task<IEnumerable<MedicalRecord>> GetMedicalHistoryAsync(Guid patientId);
    }
}
