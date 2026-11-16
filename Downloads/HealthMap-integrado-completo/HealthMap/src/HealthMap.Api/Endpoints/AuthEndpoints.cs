using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Domain.Services;
using HealthMap.Api.Dtos;
using HealthMap.Api.Middleware;

namespace HealthMap.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");
        group.AddEndpointFilter<ValidationEndpointFilter>();

        group.MapPost("/login", (LoginRequest req, AuthService auth) =>
        {
            var usuario = auth.Autenticar(req.Email, req.Senha);
            if (usuario is null) return Results.Unauthorized();
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{usuario.IdUsuario}:{DateTime.UtcNow.Ticks}"));
            return Results.Ok(new LoginResponse(token, usuario.IdUsuario, usuario.Nome, usuario.Email));
        });

        group.MapPost("/register-paciente", (RegisterPacienteRequest req, IUsuarioRepository usuarios, IPacienteRepository pacientes) =>
        {
            if (usuarios.GetByEmail(req.Email) is not null) return Results.Conflict(new { error = "E-mail já cadastrado." });
            if (usuarios.GetByCpf(req.Cpf) is not null) return Results.Conflict(new { error = "CPF já cadastrado." });
            var id = Guid.NewGuid().ToString();
            usuarios.Add(new Usuario { IdUsuario = id, Nome = req.Nome.Trim(), Email = req.Email.Trim(), Cpf = req.Cpf.Trim(), Telefone = req.Telefone.Trim(), DataNascimento = req.DataNascimento, SenhaHash = PasswordHasher.Hash(req.Senha) });
            var paciente = pacientes.Add(new Paciente { IdUsuario = id, PlanoSaude = req.PlanoSaude?.Trim() ?? string.Empty, Endereco = req.Endereco?.Trim() ?? string.Empty });
            return Results.Created($"/api/pacientes/{id}", new { usuarioId = id, paciente });
        });

        group.MapPost("/register-medico", (RegisterMedicoRequest req, IUsuarioRepository usuarios, IMedicoRepository medicos, IEspecialidadeRepository especialidades) =>
        {
            if (usuarios.GetByEmail(req.Email) is not null) return Results.Conflict(new { error = "E-mail já cadastrado." });
            if (usuarios.GetByCpf(req.Cpf) is not null) return Results.Conflict(new { error = "CPF já cadastrado." });
            if (medicos.GetByCrm(req.Crm) is not null) return Results.Conflict(new { error = "CRM já cadastrado." });
            var especialidade = especialidades.GetByNome(req.Especialidade.Trim());
            if (especialidade is null) especialidade = especialidades.Add(new Especialidade { Nome = req.Especialidade.Trim() });
            var id = Guid.NewGuid().ToString();
            usuarios.Add(new Usuario { IdUsuario = id, Nome = req.Nome.Trim(), Email = req.Email.Trim(), Cpf = req.Cpf.Trim(), Telefone = req.Telefone.Trim(), DataNascimento = req.DataNascimento, SenhaHash = PasswordHasher.Hash(req.Senha) });
            var medico = medicos.Add(new Medico { IdUsuario = id, IdEspecialidade = especialidade.IdEspecialidade, Crm = req.Crm.Trim() });
            return Results.Created($"/api/medicos/{id}", new { usuarioId = id, medico });
        });

        group.MapPost("/register-secretaria", (RegisterSecretariaRequest req, IUsuarioRepository usuarios, ISecretariaRepository secretarias) =>
        {
            if (usuarios.GetByEmail(req.Email) is not null) return Results.Conflict(new { error = "E-mail já cadastrado." });
            if (usuarios.GetByCpf(req.Cpf) is not null) return Results.Conflict(new { error = "CPF já cadastrado." });
            var id = Guid.NewGuid().ToString();
            usuarios.Add(new Usuario { IdUsuario = id, Nome = req.Nome.Trim(), Email = req.Email.Trim(), Cpf = req.Cpf.Trim(), Telefone = req.Telefone.Trim(), DataNascimento = req.DataNascimento, SenhaHash = PasswordHasher.Hash(req.Senha) });
            var secretaria = secretarias.Add(new Secretaria { IdUsuario = id, TurnoTrabalho = req.TurnoTrabalho?.Trim() ?? string.Empty });
            return Results.Created($"/api/secretarias/{id}", new { usuarioId = id, secretaria });
        });
    }
}
