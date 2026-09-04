namespace HealthMap.Domain.Entities;

public class ConfirmacaoMensagem
{
    public string IdMensagem { get; set; } = Guid.NewGuid().ToString();
    public string IdConsulta { get; set; } = string.Empty;
    public string TipoMensagem { get; set; } = string.Empty;
    public string TextoMensagem { get; set; } = string.Empty;
    public string StatusEnvio { get; set; } = "pendente";
    public DateTime? DataEnvio { get; set; }
    public string RespostaPaciente { get; set; } = string.Empty;
}
