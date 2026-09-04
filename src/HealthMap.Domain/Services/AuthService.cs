using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;

namespace HealthMap.Domain.Services;

/// <summary>
/// Regras de negócio relacionadas à sessão de usuários (login/cadastro).
/// A persistência de senhas é feita com hash (na infraestrutura utilizamos
/// o formato PBKDF2/SHA256 com salt).
/// </summary>
public class AuthService
{
    private readonly IUsuarioRepository _usuarios;

    public AuthService(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public Usuario? Autenticar(string email, string senha)
    {
        var usuario = _usuarios.GetByEmail(email);
        if (usuario is null)
            return null;

        var hash = PasswordHasher.Hash(senha, usuario.SenhaHash);
        return hash == usuario.SenhaHash ? usuario : null;
    }
}

/// <summary>
/// Gera e valida hashes de senha (PBKDF2 com salt único).
/// </summary>
public static class PasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string Hash(string senha, string? existingHash = null)
    {
        var salt = existingHash is null
            ? System.Security.Cryptography.RandomNumberGenerator.GetBytes(SaltSize)
            : Convert.FromBase64String(existingHash.Split(':')[0]);

        var hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            senha, salt, Iterations, System.Security.Cryptography.HashAlgorithmName.SHA256, HashSize);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string senha, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;
        return Hash(senha, storedHash) == storedHash;
    }
}
