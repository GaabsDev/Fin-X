using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinX.Api.Models;

namespace FinX.Api.Services
{
    public interface IHospitalService
    {
        Task<Hospital> CreateAsync(Hospital hospital);
        Task<Hospital?> GetAsync(Guid id);
        Task<IEnumerable<Hospital>> GetAllAsync();
        Task<bool> UpdateAsync(Hospital hospital);
        Task<bool> DeleteAsync(Guid id);
        Task<Hospital?> GetByCnpjAsync(string cnpj);
    }

    public interface IAgendamentoService
    {
        Task<Agendamento> CreateAsync(Agendamento agendamento);
        Task<Agendamento?> GetAsync(Guid id);
        Task<IEnumerable<Agendamento>> GetAllAsync();
        Task<IEnumerable<Agendamento>> GetByPacienteAsync(Guid pacienteId);
        Task<IEnumerable<Agendamento>> GetByHospitalAsync(Guid hospitalId);
        Task<bool> UpdateAsync(Agendamento agendamento);
        Task<bool> DeleteAsync(Guid id);
    }

    public interface IGrupoPacienteHospitalService
    {
        Task<GrupoPacienteHospital> CreateAsync(GrupoPacienteHospital grupo);
        Task<GrupoPacienteHospital?> GetAsync(Guid id);
        Task<IEnumerable<GrupoPacienteHospital>> GetByPacienteAsync(Guid pacienteId);
        Task<IEnumerable<GrupoPacienteHospital>> GetByHospitalAsync(Guid hospitalId);
        Task<GrupoPacienteHospital?> GetByPacienteHospitalAsync(Guid pacienteId, Guid hospitalId);
        Task<bool> UpdateAsync(GrupoPacienteHospital grupo);
        Task<bool> DeleteAsync(Guid id);
    }
}