using System.Net.Http.Json;
using System.Text.Json;

namespace HealthMap.Api.Tests;

/// <summary>
/// Base para os testes de integração: cria uma API isolada com banco JSON
/// temporário para cada teste.
/// </summary>
public abstract class ApiTestBase : IDisposable
{
    protected readonly TestApiFactory Factory;
    protected readonly HttpClient Client;

    protected ApiTestBase()
    {
        Factory = new TestApiFactory();
        Client = Factory.CreateClient();
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    /// <summary>Retorna uma data futura garantidamente em uma segunda-feira (dia com disponibilidade no seed).</summary>
    protected static DateTime ProximaSegunda(int hora)
    {
        var hoje = DateTime.Today;
        var dias = ((int)DayOfWeek.Monday - (int)hoje.DayOfWeek + 7) % 7;
        if (dias == 0) dias = 7;
        return hoje.AddDays(dias).AddHours(hora);
    }

    protected async Task<HttpResponseMessage> PostJsonAsync(string url, object body) =>
        await Client.PostAsJsonAsync(url, body);

    protected async Task<HttpResponseMessage> PutJsonAsync(string url, object body) =>
        await Client.PutAsJsonAsync(url, body);

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "{}" : content);
        return document.RootElement.Clone();
    }

    protected static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var json = await ReadJsonAsync(response);
        return json.TryGetProperty("error", out var error) ? error.GetString() ?? string.Empty : string.Empty;
    }
}
