using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class DisponibilidadeMedicoRepository : IDisponibilidadeMedicoRepository
{
    private readonly JsonDatabase _db;

    public DisponibilidadeMedicoRepository(JsonDatabase db) => _db = db;

    public IEnumerable<DisponibilidadeMedico> GetByMedico(string idMedico) => _db.LoadDocument().DisponibilidadesMedico.Where(d => d.IdMedico == idMedico);

    public DisponibilidadeMedico? GetById(string id) => _db.LoadDocument().DisponibilidadesMedico.FirstOrDefault(d => d.IdDisponibilidade == id);

    public DisponibilidadeMedico Add(DisponibilidadeMedico disponibilidade)
    {
        var doc = _db.LoadDocument();
        doc.DisponibilidadesMedico.Add(disponibilidade);
        _db.SaveDocument(doc);
        return disponibilidade;
    }

    public void Update(DisponibilidadeMedico disponibilidade)
    {
        var doc = _db.LoadDocument();
        var idx = doc.DisponibilidadesMedico.FindIndex(d => d.IdDisponibilidade == disponibilidade.IdDisponibilidade);
        if (idx >= 0) doc.DisponibilidadesMedico[idx] = disponibilidade;
        _db.SaveDocument(doc);
    }

    public void Delete(string id)
    {
        var doc = _db.LoadDocument();
        doc.DisponibilidadesMedico.RemoveAll(d => d.IdDisponibilidade == id);
        _db.SaveDocument(doc);
    }
}
