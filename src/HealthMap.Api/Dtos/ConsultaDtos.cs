using System.ComponentModel.DataAnnotations;

namespace HealthMap.Api.Dtos;

public class AgendarConsultaRequest
{
    [Required(ErrorMessage = "O id do paciente é obrigatório.")]
    public string IdPaciente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O id do médico é obrigatório.")]
    public string IdMedico { get; set; } = string.Empty;

    public DateTime DataHora { get; set; }

    public string? QueixaPrincipal { get; set; }

    [Required(ErrorMessage = "O id do usuário solicitante é obrigatório.")]
    public string IdUsuarioSolicitante { get; set; } = string.Empty;
}

public class ReagendarConsultaRequest
{
    public DateTime DataHora { get; set; }
}

public record ConsultaResponse(
    string IdConsulta,
    string IdPaciente,
    string IdMedico,
    DateTime DataHora,
    string Status,
    string QueixaPrincipal);

public class FeedbackRequest
{
    [Required(ErrorMessage = "O id da consulta é obrigatório.")]
    public string IdConsulta { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5.")]
    public int NotaAtendimento { get; set; }

    public string? Comentario { get; set; }
}
