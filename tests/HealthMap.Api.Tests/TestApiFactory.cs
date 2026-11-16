using Microsoft.AspNetCore.Mvc.Testing;

namespace HealthMap.Api.Tests;

/// <summary>
/// Factory de teste que aponta a API para um banco SQLite temporário e isolado,
/// sobrescrevendo a connection string <c>ConnectionStrings__HealthMap</c>.
/// O esquema e os dados de demonstração são criados pelo seeder da aplicação.
/// </summary>
public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly string _tempDir;
    private readonly string _dbPath;

    public TestApiFactory()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "healthmap-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _dbPath = Path.Combine(_tempDir, "healthmap-test.db");
        Environment.SetEnvironmentVariable("ConnectionStrings__HealthMap", $"Data Source={_dbPath}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        Environment.SetEnvironmentVariable("ConnectionStrings__HealthMap", null);
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch (IOException)
        {
            // Ignora falha de limpeza de diretório temporário.
        }
        catch (UnauthorizedAccessException)
        {
            // Ignora falha de limpeza de diretório temporário.
        }
    }
}
