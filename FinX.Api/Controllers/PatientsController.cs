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
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _svc;
        private readonly IPatientUnificationService _unificationService;

        public PatientsController(IPatientService svc, IPatientUnificationService unificationService)
        {
            _svc = svc;
            _unificationService = unificationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Patient patient)
        {
            var created = await _svc.CreateAsync(patient);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var p = await _svc.GetAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var all = await _svc.GetAllAsync();
            return Ok(all);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Patient patient)
        {
            if (id != patient.Id) return BadRequest();
            var ok = await _svc.UpdateAsync(patient);
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        [HttpPost("{id:guid}/medical-records")]
        public async Task<IActionResult> AddRecord(Guid id, [FromBody] MedicalRecord record)
        {
            var added = await _svc.AddMedicalRecordAsync(id, record);
            return CreatedAtAction(nameof(GetMedicalHistory), new { id }, added);
        }

        [HttpGet("{id:guid}/medical-records")]
        public async Task<IActionResult> GetMedicalHistory(Guid id)
        {
            var history = await _svc.GetMedicalHistoryAsync(id);
            return Ok(history);
        }

        /// <summary>
        /// DESAFIO 3: Unifica pacientes duplicados com base no CPF, mantendo o mais recente
        /// </summary>
        [HttpPost("unify-duplicates")]
        public async Task<IActionResult> UnifyDuplicatePatients()
        {
            var result = await _unificationService.UnifyDuplicatePatientsByCpfAsync();
            return Ok(result);
        }

        /// <summary>
        /// DESAFIO 3: Unifica pacientes duplicados para um CPF específico
        /// </summary>
        [HttpPost("unify-duplicates/{cpf}")]
        public async Task<IActionResult> UnifyDuplicatePatientsByCpf(string cpf)
        {
            var result = await _unificationService.UnifyDuplicatePatientsByCpfAsync(cpf);
            return Ok(result);
        }

        /// <summary>
        /// DESAFIO 3: Lista todos os pacientes duplicados (mesmo CPF)
        /// </summary>
        [HttpGet("duplicates")]
        public async Task<IActionResult> GetDuplicatePatients()
        {
            var duplicates = await _unificationService.FindDuplicatePatientsByCpfAsync();
            return Ok(duplicates);
        }

        /// <summary>
        /// DESAFIO 3: Lista pacientes duplicados para um CPF específico
        /// </summary>
        [HttpGet("duplicates/{cpf}")]
        public async Task<IActionResult> GetDuplicatePatientsByCpf(string cpf)
        {
            var duplicates = await _unificationService.FindDuplicatePatientsByCpfAsync(cpf);
            return Ok(duplicates);
        }
    }
}
