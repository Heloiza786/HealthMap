using HealthMap.Domain.Interfaces;
using HealthMap.Domain.Services;
using HealthMap.Api.Dtos;

namespace HealthMap.Api.Endpoints;

public static class ConsultaEndpoints
{
    public static void MapConsultaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/consultas");

        group.MapGet("/", (IConsultaRepository repo) => Results.Ok(repo.GetAll()));

        group.MapGet("/{id}", (string id, IConsultaRepository repo) =>
        {
            var c = repo.GetById(id);
            return c is null ? Results.NotFound(new { error = "Consulta não encontrada." }) : Results.Ok(c);
        });

        group.MapPost("/agendar", (AgendarConsultaRequest req, ConsultaService service) =>
        {
            var (consulta, _) = service.Agendar(
                req.IdPaciente, req.IdMedico, req.DataHora,
                req.QueixaPrincipal ?? string.Empty, req.IdUsuarioSolicitante);

            return Results.Created($"/api/consultas/{consulta.IdConsulta}", consulta);
        });

        group.MapPost("/{id}/cancelar", (string id, ConsultaService service) =>
        {
            service.Cancelar(id);
            return Results.NoContent();
        });

        group.MapPost("/{id}/concluir", (string id, ConsultaService service) =>
        {
            service.Concluir(id);
            return Results.NoContent();
        });
    }

    public static void MapFeedbackEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feedback");

        group.MapPost("/", (FeedbackRequest req, FeedbackService service) =>
        {
            var feedback = service.Registrar(req.IdConsulta, req.NotaAtendimento, req.Comentario ?? string.Empty);
            return Results.Created($"/api/feedback/{feedback.IdFeedback}", feedback);
        });
    }
}
