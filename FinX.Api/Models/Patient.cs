using System;
using System.Collections.Generic;

namespace FinX.Api.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class Patient
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Contact { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }

    public class Hospital
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
    }

    public class GrupoPacienteHospital
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid PacienteId { get; set; }
        public Guid HospitalId { get; set; }
        public string Codigo { get; set; } = string.Empty;
    }

    public class Agendamento
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid HospitalId { get; set; }
        public Guid PacienteId { get; set; }
        public DateTime Data { get; set; }
        public string? Descricao { get; set; }
        public string Status { get; set; } = "Agendado";
    }
}
