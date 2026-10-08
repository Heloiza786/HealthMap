using HealthMap.Domain.Entities;
using HealthMap.Domain.Services;
using HealthMap.Domain.Tests.Fakes;
using Xunit;

namespace HealthMap.Domain.Tests;

public class AuthServiceTests
{
    private static Usuario CriarUsuario(string id, string email, string senhaHash) =>
        new() { IdUsuario = id, Nome = "Teste", Email = email, SenhaHash = senhaHash };

    [Fact]
    public void Autenticar_ComEmailDesconhecido_RetornaNulo()
    {
        var repo = new InMemoryUsuarioRepository();
        var service = new AuthService(repo);

        var resultado = service.Autenticar("nao@existe.com", "qualquer");

        Assert.Null(resultado);
    }

    [Fact]
    public void Autenticar_ComHashPbkdf2_RetornaUsuario()
    {
        var hash = PasswordHasher.Hash("senha123");
        var repo = new InMemoryUsuarioRepository(CriarUsuario("usr-1", "a@b.com", hash));
        var service = new AuthService(repo);

        var resultado = service.Autenticar("a@b.com", "senha123");

        Assert.NotNull(resultado);
        Assert.Equal("usr-1", resultado.IdUsuario);
    }

    [Fact]
    public void Autenticar_ComSenhaErrada_RetornaNulo()
    {
        var hash = PasswordHasher.Hash("senha123");
        var repo = new InMemoryUsuarioRepository(CriarUsuario("usr-1", "a@b.com", hash));
        var service = new AuthService(repo);

        var resultado = service.Autenticar("a@b.com", "senha-errada");

        Assert.Null(resultado);
    }

    [Fact]
    public void Autenticar_ComSenhaLegadaEmTextoPuro_RetornaUsuario()
    {
        // Compatibilidade com o seed de demonstração ("senhaHash": "demo").
        var repo = new InMemoryUsuarioRepository(CriarUsuario("usr-1", "a@b.com", "demo"));
        var service = new AuthService(repo);

        var resultado = service.Autenticar("a@b.com", "demo");

        Assert.NotNull(resultado);
        Assert.Equal("usr-1", resultado.IdUsuario);
    }

    [Fact]
    public void Autenticar_EmailComparadoSemCaseSensitive()
    {
        var repo = new InMemoryUsuarioRepository(CriarUsuario("usr-1", "a@b.com", "demo"));
        var service = new AuthService(repo);

        var resultado = service.Autenticar("A@B.COM", "demo");

        Assert.NotNull(resultado);
    }
}

public class PasswordHasherTests
{
    [Fact]
    public void Hash_GeraFormatoSaltHash()
    {
        var hash = PasswordHasher.Hash("senha123");

        var partes = hash.Split(':');
        Assert.Equal(2, partes.Length);
        Assert.NotEmpty(partes[0]);
        Assert.NotEmpty(partes[1]);
    }

    [Fact]
    public void Verify_SenhaCorreta_RetornaTrue()
    {
        var hash = PasswordHasher.Hash("senha123");

        Assert.True(PasswordHasher.Verify("senha123", hash));
    }

    [Fact]
    public void Verify_SenhaIncorreta_RetornaFalse()
    {
        var hash = PasswordHasher.Hash("senha123");

        Assert.False(PasswordHasher.Verify("outra", hash));
    }

    [Theory]
    [InlineData("demo")]
    [InlineData("")]
    [InlineData(":::")]
    [InlineData("a:b:c")]
    public void Verify_HashMalformado_RetornaFalse(string hash)
    {
        Assert.False(PasswordHasher.Verify("qualquer", hash));
    }

    [Fact]
    public void Hash_MesmaSenha_GeraHashesDiferentesPorSalt()
    {
        var hash1 = PasswordHasher.Hash("senha123");
        var hash2 = PasswordHasher.Hash("senha123");

        Assert.NotEqual(hash1, hash2);
        Assert.True(PasswordHasher.Verify("senha123", hash1));
        Assert.True(PasswordHasher.Verify("senha123", hash2));
    }
}
