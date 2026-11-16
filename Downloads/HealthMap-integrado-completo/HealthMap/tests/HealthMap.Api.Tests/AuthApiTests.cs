using System.Net;
using Xunit;

namespace HealthMap.Api.Tests;

public class AuthApiTests : ApiTestBase
{
    [Fact]
    public async Task Login_ComCredenciaisValidas_RetornaTokenEUsuario()
    {
        var response = await PostJsonAsync("/api/auth/login", new { email = "medico@gmail.com", senha = "123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("token").GetString()));
        Assert.Equal("medico@gmail.com", json.GetProperty("email").GetString());
        Assert.Equal("usr-2", json.GetProperty("idUsuario").GetString());
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_Retorna401()
    {
        var response = await PostJsonAsync("/api/auth/login", new { email = "medico@gmail.com", senha = "senha-errada" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComEmailDesconhecido_Retorna401()
    {
        var response = await PostJsonAsync("/api/auth/login", new { email = "nao@existe.com", senha = "123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_SemEmail_Retorna400()
    {
        var response = await PostJsonAsync("/api/auth/login", new { email = "", senha = "123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("e-mail", await ReadErrorAsync(response), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_ComEmailInvalido_Retorna400()
    {
        var response = await PostJsonAsync("/api/auth/login", new { email = "email-invalido", senha = "123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("E-mail", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Login_ComCorpoMalformado_Retorna400()
    {
        var content = new StringContent("{ json inválido", System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/auth/login", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
