using System.Net;
using System.Text.Json;
using Xunit;

namespace HealthMap.Api.Tests;

public class UsuarioApiTests : ApiTestBase
{
    [Fact]
    public async Task Get_Usuarios_NaoExpoeSenhaHash()
    {
        var response = await Client.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.True(json.GetArrayLength() > 0);

        foreach (var usuario in json.EnumerateArray())
        {
            Assert.False(usuario.TryGetProperty("senhaHash", out _));
            Assert.True(usuario.TryGetProperty("idUsuario", out _));
            Assert.True(usuario.TryGetProperty("email", out _));
        }
    }

    [Fact]
    public async Task Get_UsuarioPorId_NaoExpoeSenhaHash()
    {
        var response = await Client.GetAsync("/api/usuarios/usr-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal("paciente@gmail.com", json.GetProperty("email").GetString());
        Assert.False(json.TryGetProperty("senhaHash", out _));
    }

    [Fact]
    public async Task Get_UsuarioInexistente_Retorna404()
    {
        var response = await Client.GetAsync("/api/usuarios/nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Usuário não encontrado", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Especialidade_NomeVazio_Retorna400()
    {
        var response = await PostJsonAsync("/api/especialidades", new { nome = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("nome", await ReadErrorAsync(response), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Post_Especialidade_Valida_Retorna201()
    {
        var response = await PostJsonAsync("/api/especialidades", new { nome = "Pediatria" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal("Pediatria", json.GetProperty("nome").GetString());
    }

    [Fact]
    public async Task Post_Especialidade_Duplicada_Retorna409()
    {
        var response = await PostJsonAsync("/api/especialidades", new { nome = "Ortopedia" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("Já existe", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Medico_SemCrm_Retorna400()
    {
        var response = await PostJsonAsync("/api/medicos", new { idUsuario = "usr-2", idEspecialidade = "esp-1", crm = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("CRM", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Medico_UsuarioInexistente_Retorna400()
    {
        var response = await PostJsonAsync("/api/medicos", new { idUsuario = "nao-existe", idEspecialidade = "esp-1", crm = "999999-SP" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Usuário", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Medico_EspecialidadeInexistente_Retorna400()
    {
        var response = await PostJsonAsync("/api/medicos", new { idUsuario = "usr-3", idEspecialidade = "nao-existe", crm = "999999-SP" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Especialidade", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Medico_Valido_Retorna201()
    {
        var response = await PostJsonAsync("/api/medicos", new { idUsuario = "usr-3", idEspecialidade = "esp-2", crm = "999999-SP" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal("999999-SP", json.GetProperty("crm").GetString());
    }

    [Fact]
    public async Task Post_Medico_CrmDuplicado_Retorna409()
    {
        var response = await PostJsonAsync("/api/medicos", new { idUsuario = "usr-3", idEspecialidade = "esp-2", crm = TestData.CrmMedico });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("CRM", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Put_Medico_Inexistente_Retorna404()
    {
        var response = await PutJsonAsync("/api/medicos/nao-existe", new { crm = "111111-SP" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_Medico_Valido_Retorna204()
    {
        var response = await PutJsonAsync("/api/medicos/usr-2", new { idEspecialidade = "esp-1", crm = "111111-SP" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Medico_Inexistente_Retorna404()
    {
        var response = await Client.DeleteAsync("/api/medicos/nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Medico_Existente_Retorna204()
    {
        var response = await Client.DeleteAsync("/api/medicos/usr-2");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Post_Paciente_SemIdUsuario_Retorna400()
    {
        var response = await PostJsonAsync("/api/pacientes", new { idUsuario = "", planoSaude = "X", endereco = "Y" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("IdUsuario", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Paciente_UsuarioInexistente_Retorna400()
    {
        var response = await PostJsonAsync("/api/pacientes", new { idUsuario = "nao-existe", planoSaude = "X", endereco = "Y" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Usuário", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Paciente_Valido_Retorna201()
    {
        var response = await PostJsonAsync("/api/pacientes", new { idUsuario = "usr-3", planoSaude = "Amil", endereco = "Rua B, 10" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal("usr-3", json.GetProperty("idUsuario").GetString());
    }

    [Fact]
    public async Task Put_Paciente_Inexistente_Retorna404()
    {
        var response = await PutJsonAsync("/api/pacientes/nao-existe", new { planoSaude = "X", endereco = "Y" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
