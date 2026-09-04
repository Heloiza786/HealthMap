using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Domain.Services;
using HealthMap.Api.Dtos;

namespace HealthMap.Api.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/usuarios");

        group.MapGet("/", (IUsuarioRepository repo) => Results.Ok(repo.GetAll()));

        group.MapGet("/{id}", (string id, IUsuarioRepository repo) =>
        {
            var u = repo.GetById(id);
            return u is null ? Results.NotFound(new { error = "Usuário não encontrado." }) : Results.Ok(u);
        });
    }

    public static void MapEspecialidadeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/especialidades");

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

        group.MapGet("/", (IMedicoRepository repo) => Results.Ok(repo.GetAll()));

        group.MapGet("/{id}", (string id, IMedicoRepository repo) =>
        {
            var m = repo.GetById(id);
            return m is null ? Results.NotFound(new { error = "Médico não encontrado." }) : Results.Ok(m);
        });

        group.MapPost("/", (Medico medico, IMedicoRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(medico.Crm))
                return Results.BadRequest(new { error = "CRM é obrigatório." });

            if (repo.GetByCrm(medico.Crm) is not null)
                return Results.Conflict(new { error = "CRM já cadastrado." });

            var novo = repo.Add(medico);
            return Results.Created($"/api/medicos/{novo.IdUsuario}", novo);
        });

        group.MapPut("/{id}", (string id, Medico medico, IMedicoRepository repo) =>
        {
            if (repo.GetById(id) is null)
                return Results.NotFound(new { error = "Médico não encontrado." });

            medico.IdUsuario = id;
            repo.Update(medico);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", (string id, IMedicoRepository repo) =>
        {
            repo.Delete(id);
            return Results.NoContent();
        });
    }

    public static void MapPacienteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pacientes");

        group.MapGet("/", (IPacienteRepository repo) => Results.Ok(repo.GetAll()));

        group.MapGet("/{id}", (string id, IPacienteRepository repo) =>
        {
            var p = repo.GetById(id);
            return p is null ? Results.NotFound(new { error = "Paciente não encontrado." }) : Results.Ok(p);
        });

        group.MapPost("/", (Paciente paciente, IPacienteRepository repo) =>
        {
            var novo = repo.Add(paciente);
            return Results.Created($"/api/pacientes/{novo.IdUsuario}", novo);
        });

        group.MapPut("/{id}", (string id, Paciente paciente, IPacienteRepository repo) =>
        {
            paciente.IdUsuario = id;
            repo.Update(paciente);
            return Results.NoContent();
        });
    }
}
