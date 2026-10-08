using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class ConsultaRepository : IConsultaRepository
{
    private readonly JsonDatabase _db;

    public ConsultaRepository(JsonDatabase db) => _db = db;

    public IEnumerable<Consulta> GetAll() => _db.LoadDocument().Consultas;

    public Consulta? GetById(string id) => _db.LoadDocument().Consultas.FirstOrDefault(c => c.IdConsulta == id);

    public IEnumerable<Consulta> GetByMedico(string idMedico) => _db.LoadDocument().Consultas.Where(c => c.IdMedico == idMedico);

    public IEnumerable<Consulta> GetByPaciente(string idPaciente) => _db.LoadDocument().Consultas.Where(c => c.IdPaciente == idPaciente);

    public Consulta Add(Consulta consulta)
    {
        var doc = _db.LoadDocument();
        doc.Consultas.Add(consulta);
        _db.SaveDocument(doc);
        return consulta;
    }

    public void Update(Consulta consulta)
    {
        var doc = _db.LoadDocument();
        var idx = doc.Consultas.FindIndex(c => c.IdConsulta == consulta.IdConsulta);
        if (idx >= 0) doc.Consultas[idx] = consulta;
        _db.SaveDocument(doc);
    }

    public void Delete(string id)
    {
        var doc = _db.LoadDocument();
        doc.Consultas.RemoveAll(c => c.IdConsulta == id);
        _db.SaveDocument(doc);
    }
}
