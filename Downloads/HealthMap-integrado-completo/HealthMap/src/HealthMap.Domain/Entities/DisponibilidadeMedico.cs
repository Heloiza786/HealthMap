namespace HealthMap.Domain.Entities;

public class DisponibilidadeMedico
{
    public string IdDisponibilidade { get; set; } = Guid.NewGuid().ToString();
    public string IdMedico { get; set; } = string.Empty;
    public int DiaSemana { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFim { get; set; } = string.Empty;
}
