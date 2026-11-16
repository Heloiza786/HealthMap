using HealthMap.Domain.Entities;

namespace HealthMap.Infrastructure.Persistence;

/// <summary>
/// Espelha a estrutura do arquivo JSON de banco de dados.
/// </summary>
public class DatabaseDocument
{
    public List<Usuario> Usuarios { get; set; } = new();
    public List<Paciente> Pacientes { get; set; } = new();
    public List<Secretaria> Secretarias { get; set; } = new();
    public List<Especialidade> Especialidades { get; set; } = new();
    public List<Medico> Medicos { get; set; } = new();
    public List<DisponibilidadeMedico> DisponibilidadesMedico { get; set; } = new();
    public List<HistoricoMedico> HistoricosMedicos { get; set; } = new();
    public List<Consulta> Consultas { get; set; } = new();
    public List<Agendamento> Agendamentos { get; set; } = new();
    public List<Feedback> Feedbacks { get; set; } = new();
    public List<ConfirmacaoMensagem> ConfirmacoesMensagem { get; set; } = new();
}
