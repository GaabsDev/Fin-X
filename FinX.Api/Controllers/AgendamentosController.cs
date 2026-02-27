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
    public class AgendamentosController : ControllerBase
    {
        private readonly IAgendamentoService _agendamentoService;
        private readonly IPatientService _patientService;
        private readonly IHospitalService _hospitalService;

        public AgendamentosController(
            IAgendamentoService agendamentoService,
            IPatientService patientService,
            IHospitalService hospitalService)
        {
            _agendamentoService = agendamentoService;
            _patientService = patientService;
            _hospitalService = hospitalService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Agendamento agendamento)
        {
            // Validar se paciente existe
            var patient = await _patientService.GetAsync(agendamento.PacienteId);
            if (patient == null)
                return BadRequest($"Paciente com ID {agendamento.PacienteId} não encontrado.");

            // Validar se hospital existe
            var hospital = await _hospitalService.GetAsync(agendamento.HospitalId);
            if (hospital == null)
                return BadRequest($"Hospital com ID {agendamento.HospitalId} não encontrado.");

            var created = await _agendamentoService.CreateAsync(agendamento);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var agendamento = await _agendamentoService.GetAsync(id);
            if (agendamento == null) return NotFound();
            return Ok(agendamento);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var agendamentos = await _agendamentoService.GetAllAsync();
            return Ok(agendamentos);
        }

        [HttpGet("patient/{patientId:guid}")]
        public async Task<IActionResult> GetByPatient(Guid patientId)
        {
            var agendamentos = await _agendamentoService.GetByPacienteAsync(patientId);
            return Ok(agendamentos);
        }

        [HttpGet("hospital/{hospitalId:guid}")]
        public async Task<IActionResult> GetByHospital(Guid hospitalId)
        {
            var agendamentos = await _agendamentoService.GetByHospitalAsync(hospitalId);
            return Ok(agendamentos);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Agendamento agendamento)
        {
            if (id != agendamento.Id) return BadRequest();
            var updated = await _agendamentoService.UpdateAsync(agendamento);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _agendamentoService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            var agendamento = await _agendamentoService.GetAsync(id);
            if (agendamento == null) return NotFound();

            agendamento.Status = status;
            var updated = await _agendamentoService.UpdateAsync(agendamento);
            return updated ? NoContent() : BadRequest();
        }
    }
}