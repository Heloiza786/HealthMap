namespace HealthMap.Domain.Entities;

public class Consulta
{
    public string IdConsulta { get; set; } = Guid.NewGuid().ToString();
    public string IdPaciente { get; set; } = string.Empty;
    public string IdMedico { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public string Status { get; set; } = "pendente";
    public string QueixaPrincipal { get; set; } = string.Empty;
}
