using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinX.Api.Controllers;
using FinX.Api.Models;
using FinX.Api.Services;
using FinX.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FinX.Tests
{
    public class FakePatientUnificationService : IPatientUnificationService
    {
        public Task<List<Patient>> FindDuplicatePatientsByCpfAsync()
        {
            return Task.FromResult(new List<Patient>());
        }

        public Task<List<Patient>> FindDuplicatePatientsByCpfAsync(string cpf)
        {
            return Task.FromResult(new List<Patient>());
        }

        public Task<PatientUnificationResult> UnifyDuplicatePatientsByCpfAsync()
        {
            return Task.FromResult(new PatientUnificationResult());
        }

        public Task<PatientUnificationResult> UnifyDuplicatePatientsByCpfAsync(string cpf)
        {
            return Task.FromResult(new PatientUnificationResult());
        }
    }

    public class PatientsControllerTests
    {
        [Fact]
        public async Task Create_Get_Update_Delete_Workflow()
        {
            var svc = new FakePatientService();
            var unificationSvc = new FakePatientUnificationService();
            var ctrl = new PatientsController(svc, unificationSvc);

            var p = new Patient { Name = "Joao", CPF = "99988877766", DateOfBirth = DateTime.UtcNow.AddYears(-40), Contact = "+55" };
            var create = await ctrl.Create(p) as CreatedAtActionResult;
            Assert.NotNull(create);
            var created = create!.Value as Patient;
            Assert.NotNull(created);

            var id = created!.Id;
            var get = await ctrl.Get(id) as OkObjectResult;
            Assert.NotNull(get);

            var all = await ctrl.GetAll() as OkObjectResult;
            Assert.NotNull(all);
            Assert.NotNull(all.Value);

            created.Contact = "(11) 99999-0000";
            var upd = await ctrl.Update(id, created) as NoContentResult;
            Assert.NotNull(upd);

            var rec = new MedicalRecord { Type = "Consulta", Description = "Consulta rotineira" };
            var addRec = await ctrl.AddRecord(id, rec) as CreatedAtActionResult;
            Assert.NotNull(addRec);

            var hist = await ctrl.GetMedicalHistory(id) as OkObjectResult;
            Assert.NotNull(hist);

            var del = await ctrl.Delete(id) as NoContentResult;
            Assert.NotNull(del);
        }
    }
}
