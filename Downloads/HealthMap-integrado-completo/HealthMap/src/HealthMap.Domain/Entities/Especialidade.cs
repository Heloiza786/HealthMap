namespace HealthMap.Domain.Entities;

public class Especialidade
{
    public string IdEspecialidade { get; set; } = Guid.NewGuid().ToString();
    public string Nome { get; set; } = string.Empty;
}
