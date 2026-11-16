using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class SecretariaRepository : ISecretariaRepository
{
    private readonly JsonDatabase _db;

    public SecretariaRepository(JsonDatabase db) => _db = db;

    public IEnumerable<Secretaria> GetAll() => _db.LoadDocument().Secretarias;

    public Secretaria? GetById(string idUsuario) => _db.LoadDocument().Secretarias.FirstOrDefault(s => s.IdUsuario == idUsuario);

    public Secretaria Add(Secretaria secretaria)
    {
        var doc = _db.LoadDocument();
        doc.Secretarias.Add(secretaria);
        _db.SaveDocument(doc);
        return secretaria;
    }

    public void Update(Secretaria secretaria)
    {
        var doc = _db.LoadDocument();
        var idx = doc.Secretarias.FindIndex(s => s.IdUsuario == secretaria.IdUsuario);
        if (idx >= 0) doc.Secretarias[idx] = secretaria;
        _db.SaveDocument(doc);
    }

    public void Delete(string idUsuario)
    {
        var doc = _db.LoadDocument();
        doc.Secretarias.RemoveAll(s => s.IdUsuario == idUsuario);
        _db.SaveDocument(doc);
    }
}
