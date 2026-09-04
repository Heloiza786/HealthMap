using System.ComponentModel.DataAnnotations;

namespace HealthMap.Api.Dtos;

public record AgendarConsultaRequest(
    [Required] string IdPaciente,
    [Required] string IdMedico,
    [Required] DateTime DataHora,
    string? QueixaPrincipal,
    [Required] string IdUsuarioSolicitante);

public record ConsultaResponse(
    string IdConsulta,
    string IdPaciente,
    string IdMedico,
    DateTime DataHora,
    string Status,
    string QueixaPrincipal);

public record FeedbackRequest(
    [Required] string IdConsulta,
    [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5.")]
    int NotaAtendimento,
    string? Comentario);
