namespace HealthMap.Domain.Entities;

public class Agendamento
{
    public string IdAgendamento { get; set; } = Guid.NewGuid().ToString();
    public string IdConsulta { get; set; } = string.Empty;
    public string IdUsuarioSolicitante { get; set; } = string.Empty;
    public DateTime DataSolicitacao { get; set; }
}
