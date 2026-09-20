using System.Net;
using System.Text.Json;
using Xunit;

namespace HealthMap.Api.Tests;

public class ConsultaApiTests : ApiTestBase
{
    private object AgendarRequest(DateTime dataHora) => new
    {
        idPaciente = TestData.IdPaciente,
        idMedico = TestData.IdMedico,
        dataHora,
        queixaPrincipal = "Dor",
        idUsuarioSolicitante = TestData.IdSolicitante,
    };

    [Fact]
    public async Task Post_Agendar_Valido_Retorna201ComStatusPendente()
    {
        var response = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal("pendente", json.GetProperty("status").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("idConsulta").GetString()));
    }

    [Fact]
    public async Task Post_Agendar_PacienteInexistente_Retorna422()
    {
        var body = new
        {
            idPaciente = "nao-existe",
            idMedico = TestData.IdMedico,
            dataHora = ProximaSegunda(10),
            queixaPrincipal = "",
            idUsuarioSolicitante = TestData.IdSolicitante,
        };

        var response = await PostJsonAsync("/api/consultas/agendar", body);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Paciente não encontrado.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Agendar_MedicoInexistente_Retorna422()
    {
        var body = new
        {
            idPaciente = TestData.IdPaciente,
            idMedico = "nao-existe",
            dataHora = ProximaSegunda(10),
            queixaPrincipal = "",
            idUsuarioSolicitante = TestData.IdSolicitante,
        };

        var response = await PostJsonAsync("/api/consultas/agendar", body);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Médico não encontrado.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Agendar_DataPassada_Retorna422()
    {
        var response = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(DateTime.Now.AddHours(-2)));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("A data/hora da consulta deve ser futura.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Agendar_ForaDaDisponibilidade_Retorna422()
    {
        var response = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(19)));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("O horário selecionado está fora da disponibilidade do médico.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Agendar_EmDiaSemDisponibilidade_Retorna422()
    {
        // Terça-feira não tem disponibilidade cadastrada no seed.
        var terca = ProximaSegunda(10).AddDays(1);
        var response = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(terca));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("O horário selecionado está fora da disponibilidade do médico.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Agendar_ComConflito_Retorna422()
    {
        await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10)));

        var response = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10).AddMinutes(15)));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("Já existe uma consulta", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Get_ConsultaPorId_Existente_Retorna200()
    {
        var criada = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10)));
        var jsonCriada = await ReadJsonAsync(criada);
        var id = jsonCriada.GetProperty("idConsulta").GetString()!;

        var response = await Client.GetAsync($"/api/consultas/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal(id, json.GetProperty("idConsulta").GetString());
    }

    [Fact]
    public async Task Get_ConsultaPorId_Inexistente_Retorna404()
    {
        var response = await Client.GetAsync("/api/consultas/nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Consulta não encontrada.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Cancelar_DefineStatusCancelada()
    {
        var criada = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10)));
        var jsonCriada = await ReadJsonAsync(criada);
        var id = jsonCriada.GetProperty("idConsulta").GetString()!;

        var response = await Client.PostAsync($"/api/consultas/{id}/cancelar", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var consulta = await ReadJsonAsync(await Client.GetAsync($"/api/consultas/{id}"));
        Assert.Equal("cancelada", consulta.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Post_Cancelar_Inexistente_Retorna422()
    {
        var response = await Client.PostAsync("/api/consultas/nao-existe/cancelar", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Consulta não encontrada.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Concluir_DefineStatusConcluida()
    {
        var criada = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10)));
        var jsonCriada = await ReadJsonAsync(criada);
        var id = jsonCriada.GetProperty("idConsulta").GetString()!;

        var response = await Client.PostAsync($"/api/consultas/{id}/concluir", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var consulta = await ReadJsonAsync(await Client.GetAsync($"/api/consultas/{id}"));
        Assert.Equal("concluida", consulta.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Post_Cancelar_ConsultaConcluida_Retorna422()
    {
        var criada = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10)));
        var jsonCriada = await ReadJsonAsync(criada);
        var id = jsonCriada.GetProperty("idConsulta").GetString()!;
        await Client.PostAsync($"/api/consultas/{id}/concluir", null);

        var response = await Client.PostAsync($"/api/consultas/{id}/cancelar", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Não é possível cancelar uma consulta já concluída.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Concluir_ConsultaCancelada_Retorna422()
    {
        var criada = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10)));
        var jsonCriada = await ReadJsonAsync(criada);
        var id = jsonCriada.GetProperty("idConsulta").GetString()!;
        await Client.PostAsync($"/api/consultas/{id}/cancelar", null);

        var response = await Client.PostAsync($"/api/consultas/{id}/concluir", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Não é possível concluir uma consulta cancelada.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Reagendar_Valido_Retorna204EAtualizaStatus()
    {
        var criada = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10)));
        var jsonCriada = await ReadJsonAsync(criada);
        var id = jsonCriada.GetProperty("idConsulta").GetString()!;

        var response = await PostJsonAsync($"/api/consultas/{id}/reagendar", new { dataHora = ProximaSegunda(14) });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var consulta = await ReadJsonAsync(await Client.GetAsync($"/api/consultas/{id}"));
        Assert.Equal("reagendada", consulta.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Post_Reagendar_ConsultaConcluida_Retorna422()
    {
        var criada = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10)));
        var jsonCriada = await ReadJsonAsync(criada);
        var id = jsonCriada.GetProperty("idConsulta").GetString()!;
        await Client.PostAsync($"/api/consultas/{id}/concluir", null);

        var response = await PostJsonAsync($"/api/consultas/{id}/reagendar", new { dataHora = ProximaSegunda(14) });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Não é possível reagendar uma consulta já concluída.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Reagendar_ComConflito_Retorna422()
    {
        var criada = await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(10)));
        var jsonCriada = await ReadJsonAsync(criada);
        var id = jsonCriada.GetProperty("idConsulta").GetString()!;
        await PostJsonAsync("/api/consultas/agendar", AgendarRequest(ProximaSegunda(11)));

        var response = await PostJsonAsync($"/api/consultas/{id}/reagendar", new { dataHora = ProximaSegunda(11).AddMinutes(5) });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("Já existe uma consulta", await ReadErrorAsync(response));
    }
}

public class FeedbackApiTests : ApiTestBase
{
    private async Task<string> CriarConsultaConcluida()
    {
        var criada = await PostJsonAsync("/api/consultas/agendar", new
        {
            idPaciente = TestData.IdPaciente,
            idMedico = TestData.IdMedico,
            dataHora = ProximaSegunda(10),
            queixaPrincipal = "",
            idUsuarioSolicitante = TestData.IdSolicitante,
        });
        var json = await ReadJsonAsync(criada);
        var id = json.GetProperty("idConsulta").GetString()!;
        await Client.PostAsync($"/api/consultas/{id}/concluir", null);
        return id;
    }

    [Fact]
    public async Task Post_Feedback_Valido_Retorna201()
    {
        var idConsulta = await CriarConsultaConcluida();

        var response = await PostJsonAsync("/api/feedback", new { idConsulta, notaAtendimento = 5, comentario = "Ótimo" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal(5, json.GetProperty("notaAtendimento").GetInt32());
    }

    [Fact]
    public async Task Post_Feedback_ConsultaInexistente_Retorna422()
    {
        var response = await PostJsonAsync("/api/feedback", new { idConsulta = "nao-existe", notaAtendimento = 5, comentario = "" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Consulta não encontrada.", await ReadErrorAsync(response));
    }

    [Fact]
    public async Task Post_Feedback_NotaInvalida_Retorna400()
    {
        var idConsulta = await CriarConsultaConcluida();

        var response = await PostJsonAsync("/api/feedback", new { idConsulta, notaAtendimento = 6, comentario = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("nota", await ReadErrorAsync(response), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Post_Feedback_Duplicado_Retorna422()
    {
        var idConsulta = await CriarConsultaConcluida();
        await PostJsonAsync("/api/feedback", new { idConsulta, notaAtendimento = 5, comentario = "Primeiro" });

        var response = await PostJsonAsync("/api/feedback", new { idConsulta, notaAtendimento = 4, comentario = "Segundo" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Esta consulta já possui feedback registrado.", await ReadErrorAsync(response));
    }
}
