namespace HealthMap.Domain;

/// <summary>
/// Valores válidos para o status de uma consulta.
/// </summary>
public static class ConsultaStatus
{
    public const string Pendente = "pendente";
    public const string Confirmada = "confirmada";
    public const string Concluida = "concluida";
    public const string Cancelada = "cancelada";
    public const string Reagendada = "reagendada";
}
