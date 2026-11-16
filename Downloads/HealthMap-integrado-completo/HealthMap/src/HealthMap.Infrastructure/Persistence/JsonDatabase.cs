using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthMap.Infrastructure.Persistence;

/// <summary>
/// Leitura/escrita thread-safe do banco de dados em arquivo JSON.
/// Utiliza System.Text.Json (JsonSerializer) como solicitado.
/// </summary>
public class JsonDatabase
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _options;

    public JsonDatabase(string filePath)
    {
        _filePath = filePath;
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            SaveDocument(new DatabaseDocument());
        }
    }

    public DatabaseDocument LoadDocument()
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<DatabaseDocument>(json, _options) ?? new DatabaseDocument();
        }
    }

    public void SaveDocument(DatabaseDocument document)
    {
        lock (_lock)
        {
            var json = JsonSerializer.Serialize(document, _options);
            File.WriteAllText(_filePath, json);
        }
    }
}
