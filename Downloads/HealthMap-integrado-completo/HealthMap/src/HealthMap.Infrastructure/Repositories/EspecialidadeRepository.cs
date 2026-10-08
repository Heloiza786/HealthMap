using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class EspecialidadeRepository : IEspecialidadeRepository
{
    private readonly JsonDatabase _db;

    public EspecialidadeRepository(JsonDatabase db) => _db = db;

    public IEnumerable<Especialidade> GetAll() => _db.LoadDocument().Especialidades;

    public Especialidade? GetById(string id) => _db.LoadDocument().Especialidades.FirstOrDefault(e => e.IdEspecialidade == id);

    public Especialidade? GetByNome(string nome) => _db.LoadDocument().Especialidades.FirstOrDefault(e => e.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase));

    public Especialidade Add(Especialidade especialidade)
    {
        var doc = _db.LoadDocument();
        doc.Especialidades.Add(especialidade);
        _db.SaveDocument(doc);
        return especialidade;
    }

    public void Update(Especialidade especialidade)
    {
        var doc = _db.LoadDocument();
        var idx = doc.Especialidades.FindIndex(e => e.IdEspecialidade == especialidade.IdEspecialidade);
        if (idx >= 0) doc.Especialidades[idx] = especialidade;
        _db.SaveDocument(doc);
    }

    public void Delete(string id)
    {
        var doc = _db.LoadDocument();
        doc.Especialidades.RemoveAll(e => e.IdEspecialidade == id);
        _db.SaveDocument(doc);
    }
}
