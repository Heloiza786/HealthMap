namespace HealthMap.Domain.Entities;

public class HistoricoMedico
{
    public string IdHistorico { get; set; } = Guid.NewGuid().ToString();
    public string IdPaciente { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataRegistro { get; set; }
}
