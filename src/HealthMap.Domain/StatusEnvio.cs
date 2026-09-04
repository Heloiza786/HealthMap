namespace HealthMap.Domain;

/// <summary>
/// Valores válidos para o status de envio de uma mensagem de confirmação.
/// </summary>
public static class StatusEnvio
{
    public const string Pendente = "pendente";
    public const string Enviado = "enviado";
    public const string Erro = "erro";
}
