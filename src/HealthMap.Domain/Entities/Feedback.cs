namespace HealthMap.Domain.Entities;

public class Feedback
{
    public string IdFeedback { get; set; } = Guid.NewGuid().ToString();
    public string IdConsulta { get; set; } = string.Empty;
    public int NotaAtendimento { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime DataFeedback { get; set; }
}
