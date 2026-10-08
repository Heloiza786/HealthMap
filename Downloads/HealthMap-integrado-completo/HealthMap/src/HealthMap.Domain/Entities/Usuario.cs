namespace HealthMap.Domain.Entities;

/// <summary>
/// Entidade base de usuário do sistema. Cada usuário pode ter, no máximo,
/// um perfil específico (Paciente, Secretaria ou Medico), modelado por uma
/// FK compartilhando o mesmo id_usuario.
/// </summary>
public class Usuario
{
    public string IdUsuario { get; set; } = Guid.NewGuid().ToString();
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
}
