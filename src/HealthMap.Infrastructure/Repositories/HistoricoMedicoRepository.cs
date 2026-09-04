using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class HistoricoMedicoRepository : IHistoricoMedicoRepository
{
    private readonly JsonDatabase _db;

    public HistoricoMedicoRepository(JsonDatabase db) => _db = db;

    public IEnumerable<HistoricoMedico> GetByPaciente(string idPaciente) => _db.LoadDocument().HistoricosMedicos.Where(h => h.IdPaciente == idPaciente);

    public HistoricoMedico? GetById(string id) => _db.LoadDocument().HistoricosMedicos.FirstOrDefault(h => h.IdHistorico == id);

    public HistoricoMedico Add(HistoricoMedico historico)
    {
        var doc = _db.LoadDocument();
        doc.HistoricosMedicos.Add(historico);
        _db.SaveDocument(doc);
        return historico;
    }

    public void Update(HistoricoMedico historico)
    {
        var doc = _db.LoadDocument();
        var idx = doc.HistoricosMedicos.FindIndex(h => h.IdHistorico == historico.IdHistorico);
        if (idx >= 0) doc.HistoricosMedicos[idx] = historico;
        _db.SaveDocument(doc);
    }

    public void Delete(string id)
    {
        var doc = _db.LoadDocument();
        doc.HistoricosMedicos.RemoveAll(h => h.IdHistorico == id);
        _db.SaveDocument(doc);
    }
}
