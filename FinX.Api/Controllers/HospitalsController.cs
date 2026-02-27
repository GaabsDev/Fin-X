using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinX.Api.Models;
using FinX.Api.Services;

namespace FinX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HospitalsController : ControllerBase
    {
        private readonly IHospitalService _hospitalService;
        private readonly IGrupoPacienteHospitalService _grupoService;

        public HospitalsController(IHospitalService hospitalService, IGrupoPacienteHospitalService grupoService)
        {
            _hospitalService = hospitalService;
            _grupoService = grupoService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Hospital hospital)
        {
            // Verificar se CNPJ já existe
            var existing = await _hospitalService.GetByCnpjAsync(hospital.Cnpj);
            if (existing != null)
                return BadRequest($"Hospital com CNPJ {hospital.Cnpj} já existe.");

            var created = await _hospitalService.CreateAsync(hospital);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var hospital = await _hospitalService.GetAsync(id);
            if (hospital == null) return NotFound();
            return Ok(hospital);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var hospitals = await _hospitalService.GetAllAsync();
            return Ok(hospitals);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Hospital hospital)
        {
            if (id != hospital.Id) return BadRequest();
            var updated = await _hospitalService.UpdateAsync(hospital);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _hospitalService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("{hospitalId:guid}/patients/{patientId:guid}")]
        public async Task<IActionResult> AddPatientToHospital(Guid hospitalId, Guid patientId, [FromBody] string codigo)
        {
            // Verificar se já existe relação
            var existing = await _grupoService.GetByPacienteHospitalAsync(patientId, hospitalId);
            if (existing != null)
                return BadRequest("Paciente já está vinculado a este hospital.");

            var grupo = new GrupoPacienteHospital
            {
                PacienteId = patientId,
                HospitalId = hospitalId,
                Codigo = codigo
            };

            var created = await _grupoService.CreateAsync(grupo);
            return CreatedAtAction(nameof(GetPatientHospitalRelation),
                new { hospitalId, patientId }, created);
        }

        [HttpGet("{hospitalId:guid}/patients/{patientId:guid}")]
        public async Task<IActionResult> GetPatientHospitalRelation(Guid hospitalId, Guid patientId)
        {
            var grupo = await _grupoService.GetByPacienteHospitalAsync(patientId, hospitalId);
            if (grupo == null) return NotFound();
            return Ok(grupo);
        }

        [HttpGet("{hospitalId:guid}/patients")]
        public async Task<IActionResult> GetHospitalPatients(Guid hospitalId)
        {
            var grupos = await _grupoService.GetByHospitalAsync(hospitalId);
            return Ok(grupos);
        }
    }
}