using System.ComponentModel.DataAnnotations;

namespace HealthMap.Api.Dtos;

public class LoginRequest
{
    [Required(ErrorMessage = "O campo e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo senha é obrigatório.")]
    public string Senha { get; set; } = string.Empty;
}

public record LoginResponse(string Token, string IdUsuario, string Nome, string Email);

public record UsuarioResponse(
    string IdUsuario,
    string Nome,
    string Email,
    string Cpf,
    string Telefone,
    DateTime? DataNascimento)
{
    public static UsuarioResponse From(HealthMap.Domain.Entities.Usuario usuario) =>
        new(usuario.IdUsuario, usuario.Nome, usuario.Email, usuario.Cpf, usuario.Telefone, usuario.DataNascimento);
}


public class RegisterPacienteRequest
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Senha { get; set; } = string.Empty;
    [Required] public string Cpf { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public string PlanoSaude { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
}

public class RegisterMedicoRequest
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Senha { get; set; } = string.Empty;
    [Required] public string Cpf { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    [Required] public string Crm { get; set; } = string.Empty;
    [Required] public string Especialidade { get; set; } = string.Empty;
}

public class RegisterSecretariaRequest
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Senha { get; set; } = string.Empty;
    [Required] public string Cpf { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public string TurnoTrabalho { get; set; } = string.Empty;
}

public class AtualizarUsuarioRequest
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Cpf { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public string? Senha { get; set; }
}
