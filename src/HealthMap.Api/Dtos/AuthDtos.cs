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
