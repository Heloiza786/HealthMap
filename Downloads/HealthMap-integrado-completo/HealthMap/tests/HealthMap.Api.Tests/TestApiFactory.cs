using Microsoft.AspNetCore.Mvc.Testing;

namespace HealthMap.Api.Tests;

/// <summary>
/// Factory de teste que aponta a API para um banco JSON temporário e isolado,
/// sobrescrevendo a configuração <c>Database:Path</c> via variável de ambiente.
/// </summary>
public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly string _tempDir;
    private readonly string _dbPath;

    public TestApiFactory()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "healthmap-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _dbPath = Path.Combine(_tempDir, "database.json");
        TestData.Seed(_dbPath);
        Environment.SetEnvironmentVariable("Database__Path", _dbPath);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        Environment.SetEnvironmentVariable("Database__Path", null);
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
