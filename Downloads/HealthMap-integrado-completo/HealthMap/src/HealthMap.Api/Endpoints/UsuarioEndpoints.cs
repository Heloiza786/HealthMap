using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Domain.Services;
using HealthMap.Api.Dtos;
using HealthMap.Api.Middleware;

namespace HealthMap.Api.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/usuarios");

        group.MapGet("/", (IUsuarioRepository repo) =>
            Results.Ok(repo.GetAll().Select(UsuarioResponse.From)));

        group.MapGet("/{id}", (string id, IUsuarioRepository repo) =>
        {
            var u = repo.GetById(id);
            return u is null ? Results.NotFound(new { error = "Usuário não encontrado." }) : Results.Ok(UsuarioResponse.From(u));
        });

        group.MapPut("/{id}", (string id, AtualizarUsuarioRequest req, IUsuarioRepository repo) =>
        {
            var atual = repo.GetById(id);
            if (atual is null) return Results.NotFound(new { error = "Usuário não encontrado." });
            var emailOutro = repo.GetByEmail(req.Email);
            if (emailOutro is not null && emailOutro.IdUsuario != id) return Results.Conflict(new { error = "E-mail já cadastrado." });
            var cpfOutro = repo.GetByCpf(req.Cpf);
            if (cpfOutro is not null && cpfOutro.IdUsuario != id) return Results.Conflict(new { error = "CPF já cadastrado." });
            atual.Nome = req.Nome.Trim(); atual.Email = req.Email.Trim(); atual.Cpf = req.Cpf.Trim(); atual.Telefone = req.Telefone?.Trim() ?? string.Empty; atual.DataNascimento = req.DataNascimento;
            if (!string.IsNullOrWhiteSpace(req.Senha)) atual.SenhaHash = PasswordHasher.Hash(req.Senha);
            repo.Update(atual);
            return Results.Ok(UsuarioResponse.From(atual));
        });
    }

    public static void MapEspecialidadeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/especialidades");
        group.AddEndpointFilter<ValidationEndpointFilter>();

        group.MapGet("/", (IEspecialidadeRepository repo) => Results.Ok(repo.GetAll()));

        group.MapPost("/", (Especialidade esp, IEspecialidadeRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(esp.Nome))
                return Results.BadRequest(new { error = "O nome da especialidade é obrigatório." });

            if (repo.GetByNome(esp.Nome) is not null)
                return Results.Conflict(new { error = "Já existe especialidade com este nome." });

            var nova = repo.Add(esp);
            return Results.Created($"/api/especialidades/{nova.IdEspecialidade}", nova);
        });
    }

    public static void MapMedicoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/medicos");
        group.AddEndpointFilter<ValidationEndpointFilter>();

        group.MapGet("/", (IMedicoRepository repo) => Results.Ok(repo.GetAll()));

        group.MapGet("/{id}", (string id, IMedicoRepository repo) =>
        {
            var m = repo.GetById(id);
            return m is null ? Results.NotFound(new { error = "Médico não encontrado." }) : Results.Ok(m);
        });

        group.MapPost("/", (Medico medico, IMedicoRepository repo, IUsuarioRepository usuarios, IEspecialidadeRepository especialidades) =>
        {
            if (string.IsNullOrWhiteSpace(medico.Crm))
                return Results.BadRequest(new { error = "CRM é obrigatório." });

            if (repo.GetByCrm(medico.Crm) is not null)
                return Results.Conflict(new { error = "CRM já cadastrado." });

            if (string.IsNullOrWhiteSpace(medico.IdUsuario))
                return Results.BadRequest(new { error = "IdUsuario é obrigatório." });

            if (usuarios.GetById(medico.IdUsuario) is null)
                return Results.BadRequest(new { error = "Usuário não encontrado para o médico." });

            if (string.IsNullOrWhiteSpace(medico.IdEspecialidade))
                return Results.BadRequest(new { error = "IdEspecialidade é obrigatório." });

            if (especialidades.GetById(medico.IdEspecialidade) is null)
                return Results.BadRequest(new { error = "Especialidade não encontrada." });

            var novo = repo.Add(medico);
            return Results.Created($"/api/medicos/{novo.IdUsuario}", novo);
        });

        group.MapPut("/{id}", (string id, Medico medico, IMedicoRepository repo) =>
        {
            if (repo.GetById(id) is null)
                return Results.NotFound(new { error = "Médico não encontrado." });

            if (string.IsNullOrWhiteSpace(medico.Crm))
                return Results.BadRequest(new { error = "CRM é obrigatório." });

            medico.IdUsuario = id;
            repo.Update(medico);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", (string id, IMedicoRepository repo, IUsuarioRepository usuarios) =>
        {
            if (repo.GetById(id) is null) return Results.NotFound(new { error = "Médico não encontrado." });
            repo.Delete(id);
            usuarios.Delete(id);
            return Results.NoContent();
        });
    }

    public static void MapPacienteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pacientes");
        group.AddEndpointFilter<ValidationEndpointFilter>();

        group.MapGet("/", (IPacienteRepository repo) => Results.Ok(repo.GetAll()));

        group.MapGet("/{id}", (string id, IPacienteRepository repo) =>
        {
            var p = repo.GetById(id);
            return p is null ? Results.NotFound(new { error = "Paciente não encontrado." }) : Results.Ok(p);
        });

        group.MapPost("/", (Paciente paciente, IPacienteRepository repo, IUsuarioRepository usuarios) =>
        {
            if (string.IsNullOrWhiteSpace(paciente.IdUsuario))
                return Results.BadRequest(new { error = "IdUsuario é obrigatório." });

            if (usuarios.GetById(paciente.IdUsuario) is null)
                return Results.BadRequest(new { error = "Usuário não encontrado para o paciente." });

            var novo = repo.Add(paciente);
            return Results.Created($"/api/pacientes/{novo.IdUsuario}", novo);
        });

        group.MapPut("/{id}", (string id, Paciente paciente, IPacienteRepository repo) =>
        {
            if (repo.GetById(id) is null) return Results.NotFound(new { error = "Paciente não encontrado." });
            paciente.IdUsuario = id; repo.Update(paciente); return Results.NoContent();
        });

        group.MapDelete("/{id}", (string id, IPacienteRepository repo, IUsuarioRepository usuarios) =>
        {
            if (repo.GetById(id) is null) return Results.NotFound(new { error = "Paciente não encontrado." });
            repo.Delete(id); usuarios.Delete(id); return Results.NoContent();
        });
    }
    public static void MapSecretariaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/secretarias");
        group.MapGet("/", (ISecretariaRepository repo) => Results.Ok(repo.GetAll()));
        group.MapGet("/{id}", (string id, ISecretariaRepository repo) =>
        {
            var s = repo.GetById(id);
            return s is null ? Results.NotFound(new { error = "Secretaria não encontrada." }) : Results.Ok(s);
        });
        group.MapPut("/{id}", (string id, Secretaria secretaria, ISecretariaRepository repo) =>
        {
            if (repo.GetById(id) is null) return Results.NotFound(new { error = "Secretaria não encontrada." });
            secretaria.IdUsuario = id; repo.Update(secretaria); return Results.NoContent();
        });
        group.MapDelete("/{id}", (string id, ISecretariaRepository repo, IUsuarioRepository usuarios) =>
        {
            if (repo.GetById(id) is null) return Results.NotFound(new { error = "Secretaria não encontrada." });
            repo.Delete(id); usuarios.Delete(id); return Results.NoContent();
        });
    }

}
