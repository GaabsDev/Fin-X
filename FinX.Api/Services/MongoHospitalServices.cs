using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using FinX.Api.Models;

namespace FinX.Api.Services
{
    public class MongoHospitalService : IHospitalService
    {
        private readonly IMongoCollection<Hospital> _hospitals;

        public MongoHospitalService(IMongoDatabase db)
        {
            _hospitals = db.GetCollection<Hospital>("hospitals");
        }

        public async Task<Hospital> CreateAsync(Hospital hospital)
        {
            hospital.Id = Guid.NewGuid();
            await _hospitals.InsertOneAsync(hospital);
            return hospital;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var res = await _hospitals.DeleteOneAsync(h => h.Id == id);
            return res.DeletedCount > 0;
        }

        public async Task<IEnumerable<Hospital>> GetAllAsync()
        {
            return await _hospitals.Find(_ => true).ToListAsync();
        }

        public async Task<Hospital?> GetAsync(Guid id)
        {
            return await _hospitals.Find(h => h.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Hospital?> GetByCnpjAsync(string cnpj)
        {
            return await _hospitals.Find(h => h.Cnpj == cnpj).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(Hospital hospital)
        {
            var res = await _hospitals.ReplaceOneAsync(h => h.Id == hospital.Id, hospital);
            return res.ModifiedCount > 0 || res.MatchedCount > 0;
        }
    }

    public class MongoAgendamentoService : IAgendamentoService
    {
        private readonly IMongoCollection<Agendamento> _agendamentos;

        public MongoAgendamentoService(IMongoDatabase db)
        {
            _agendamentos = db.GetCollection<Agendamento>("agendamentos");
        }

        public async Task<Agendamento> CreateAsync(Agendamento agendamento)
        {
            agendamento.Id = Guid.NewGuid();
            await _agendamentos.InsertOneAsync(agendamento);
            return agendamento;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var res = await _agendamentos.DeleteOneAsync(a => a.Id == id);
            return res.DeletedCount > 0;
        }

        public async Task<IEnumerable<Agendamento>> GetAllAsync()
        {
            return await _agendamentos.Find(_ => true).ToListAsync();
        }

        public async Task<Agendamento?> GetAsync(Guid id)
        {
            return await _agendamentos.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Agendamento>> GetByHospitalAsync(Guid hospitalId)
        {
            return await _agendamentos.Find(a => a.HospitalId == hospitalId).ToListAsync();
        }

        public async Task<IEnumerable<Agendamento>> GetByPacienteAsync(Guid pacienteId)
        {
            return await _agendamentos.Find(a => a.PacienteId == pacienteId).ToListAsync();
        }

        public async Task<bool> UpdateAsync(Agendamento agendamento)
        {
            var res = await _agendamentos.ReplaceOneAsync(a => a.Id == agendamento.Id, agendamento);
            return res.ModifiedCount > 0 || res.MatchedCount > 0;
        }
    }

    public class MongoGrupoPacienteHospitalService : IGrupoPacienteHospitalService
    {
        private readonly IMongoCollection<GrupoPacienteHospital> _grupos;

        public MongoGrupoPacienteHospitalService(IMongoDatabase db)
        {
            _grupos = db.GetCollection<GrupoPacienteHospital>("grupospacientehospital");
        }

        public async Task<GrupoPacienteHospital> CreateAsync(GrupoPacienteHospital grupo)
        {
            grupo.Id = Guid.NewGuid();
            await _grupos.InsertOneAsync(grupo);
            return grupo;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var res = await _grupos.DeleteOneAsync(g => g.Id == id);
            return res.DeletedCount > 0;
        }

        public async Task<GrupoPacienteHospital?> GetAsync(Guid id)
        {
            return await _grupos.Find(g => g.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<GrupoPacienteHospital>> GetByHospitalAsync(Guid hospitalId)
        {
            return await _grupos.Find(g => g.HospitalId == hospitalId).ToListAsync();
        }

        public async Task<IEnumerable<GrupoPacienteHospital>> GetByPacienteAsync(Guid pacienteId)
        {
            return await _grupos.Find(g => g.PacienteId == pacienteId).ToListAsync();
        }

        public async Task<GrupoPacienteHospital?> GetByPacienteHospitalAsync(Guid pacienteId, Guid hospitalId)
        {
            return await _grupos.Find(g => g.PacienteId == pacienteId && g.HospitalId == hospitalId).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(GrupoPacienteHospital grupo)
        {
            var res = await _grupos.ReplaceOneAsync(g => g.Id == grupo.Id, grupo);
            return res.ModifiedCount > 0 || res.MatchedCount > 0;
        }
    }
}