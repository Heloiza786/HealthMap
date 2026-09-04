using System.ComponentModel.DataAnnotations;

namespace HealthMap.Api.Dtos;

public record LoginRequest(
    [Required(ErrorMessage = "O campo e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    string Email,

    [Required(ErrorMessage = "O campo senha é obrigatório.")]
    string Senha);

public record LoginResponse(string Token, string IdUsuario, string Nome, string Email);

public record UsuarioResponse(
    string IdUsuario,
    string Nome,
    string Email,
    string Cpf,
    string Telefone,
    DateTime? DataNascimento);
