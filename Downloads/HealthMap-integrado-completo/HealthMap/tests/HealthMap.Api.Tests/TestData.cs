using System.Text.Json;
using HealthMap.Domain.Entities;
using HealthMap.Domain.Services;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Api.Tests;

/// <summary>
/// Grava um banco JSON de demonstração isolado para os testes de integração.
/// </summary>
public static class TestData
{
    public const string IdPaciente = "usr-1";
    public const string IdMedico = "usr-2";
    public const string IdSolicitante = "usr-3";
    public const string IdEspecialidade = "esp-1";
    public const string CrmMedico = "123456-SP";

    public static void Seed(string dbPath)
    {
        var documento = new DatabaseDocument
        {
            Usuarios =
            {
                new Usuario { IdUsuario = "usr-1", Nome = "Maria Oliveira", Email = "paciente@gmail.com", SenhaHash = PasswordHasher.Hash("123"), Cpf = "123.456.789-00", Telefone = "(11) 98888-0001", DataNascimento = new DateTime(1990, 5, 12) },
                new Usuario { IdUsuario = "usr-2", Nome = "Dr. Carlos Silva", Email = "medico@gmail.com", SenhaHash = PasswordHasher.Hash("123"), Cpf = "111.222.333-44", Telefone = "(11) 99999-0001", DataNascimento = new DateTime(1980, 3, 20) },
                new Usuario { IdUsuario = "usr-3", Nome = "Secretaria Ana", Email = "secretaria@gmail.com", SenhaHash = PasswordHasher.Hash("123"), Cpf = "555.666.777-88", Telefone = "(11) 97777-0001", DataNascimento = new DateTime(1995, 7, 1) },
            },
            Pacientes =
            {
                new Paciente { IdUsuario = "usr-1", PlanoSaude = "Unimed", Endereco = "Rua A, 123" },
            },
            Secretarias =
            {
                new Secretaria { IdUsuario = "usr-3", TurnoTrabalho = "Manhã" },
            },
            Especialidades =
            {
                new Especialidade { IdEspecialidade = "esp-1", Nome = "Ortopedia" },
                new Especialidade { IdEspecialidade = "esp-2", Nome = "Dermatologia" },
            },
            Medicos =
            {
                new Medico { IdUsuario = "usr-2", IdEspecialidade = "esp-1", Crm = CrmMedico },
            },
            DisponibilidadesMedico =
            {
                new DisponibilidadeMedico { IdDisponibilidade = "disp-1", IdMedico = "usr-2", DiaSemana = (int)DayOfWeek.Monday, HoraInicio = "08:00", HoraFim = "17:00" },
            },
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        File.WriteAllText(dbPath, JsonSerializer.Serialize(documento, options));
    }
}
