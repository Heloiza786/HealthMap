using HealthMap.Domain.Entities;
using HealthMap.Domain.Services;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Api.Data;

/// <summary>
/// Cria o esquema do banco SQL (SQLite) e garante os dados iniciais de demonstração.
/// Substitui o antigo seed do arquivo JSON, preservando as mesmas credenciais demo.
/// </summary>
public static class DatabaseSeeder
{
    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Usuarios.Any()) return;

        // Usuário paciente
        var pacienteUser = new Usuario
        {
            IdUsuario = "usr-1",
            Nome = "Maria Oliveira",
            Email = "paciente@gmail.com",
            SenhaHash = PasswordHasher.Hash("123"),
            Cpf = "123.456.789-00",
            Telefone = "(11) 98888-0001",
            DataNascimento = new DateTime(1990, 5, 12),
        };

        // Usuário médico
        var medicoUser = new Usuario
        {
            IdUsuario = "usr-2",
            Nome = "Dr. Carlos Silva",
            Email = "medico@gmail.com",
            SenhaHash = PasswordHasher.Hash("123"),
            Cpf = "111.222.333-44",
            Telefone = "(11) 99999-0001",
            DataNascimento = new DateTime(1980, 3, 20),
        };

        // Usuário secretária
        var secretariaUser = new Usuario
        {
            IdUsuario = "usr-3",
            Nome = "Secretaria Ana",
            Email = "secretaria@gmail.com",
            SenhaHash = PasswordHasher.Hash("123"),
            Cpf = "555.666.777-88",
            Telefone = "(11) 97777-0001",
            DataNascimento = new DateTime(1995, 7, 1),
        };

        db.Usuarios.AddRange(pacienteUser, medicoUser, secretariaUser);

        var especialidades = new[]
        {
            new Especialidade { IdEspecialidade = "esp-1", Nome = "Ortopedia" },
            new Especialidade { IdEspecialidade = "esp-2", Nome = "Dermatologia" },
            new Especialidade { IdEspecialidade = "esp-3", Nome = "Cardiologia" },
        };
        db.Especialidades.AddRange(especialidades);

        db.Pacientes.Add(new Paciente { IdUsuario = "usr-1", PlanoSaude = "Unimed", Endereco = "Rua A, 123" });
        db.Secretarias.Add(new Secretaria { IdUsuario = "usr-3", TurnoTrabalho = "Manhã" });
        db.Medicos.Add(new Medico { IdUsuario = "usr-2", IdEspecialidade = "esp-1", Crm = "123456-SP" });

        db.DisponibilidadesMedico.Add(new DisponibilidadeMedico
        {
            IdDisponibilidade = "disp-1",
            IdMedico = "usr-2",
            DiaSemana = 1,
            HoraInicio = "08:00",
            HoraFim = "17:00",
        });

        db.SaveChanges();
    }
}
