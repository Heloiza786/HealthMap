using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class MedicoRepository : IMedicoRepository
{
    private readonly JsonDatabase _db;

    public MedicoRepository(JsonDatabase db) => _db = db;

    public IEnumerable<Medico> GetAll() => _db.LoadDocument().Medicos;

    public Medico? GetById(string idUsuario) => _db.LoadDocument().Medicos.FirstOrDefault(m => m.IdUsuario == idUsuario);

    public Medico? GetByCrm(string crm) => _db.LoadDocument().Medicos.FirstOrDefault(m => m.Crm == crm);

    public Medico Add(Medico medico)
    {
        var doc = _db.LoadDocument();
        doc.Medicos.Add(medico);
        _db.SaveDocument(doc);
        return medico;
    }

    public void Update(Medico medico)
    {
        var doc = _db.LoadDocument();
        var idx = doc.Medicos.FindIndex(m => m.IdUsuario == medico.IdUsuario);
        if (idx >= 0) doc.Medicos[idx] = medico;
        _db.SaveDocument(doc);
    }

    public void Delete(string idUsuario)
    {
        var doc = _db.LoadDocument();
        doc.Medicos.RemoveAll(m => m.IdUsuario == idUsuario);
        _db.SaveDocument(doc);
    }
}
