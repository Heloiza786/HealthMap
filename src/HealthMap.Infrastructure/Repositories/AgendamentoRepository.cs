using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly JsonDatabase _db;

    public AgendamentoRepository(JsonDatabase db) => _db = db;

    public IEnumerable<Agendamento> GetAll() => _db.LoadDocument().Agendamentos;

    public Agendamento? GetById(string id) => _db.LoadDocument().Agendamentos.FirstOrDefault(a => a.IdAgendamento == id);

    public Agendamento? GetByConsulta(string idConsulta) => _db.LoadDocument().Agendamentos.FirstOrDefault(a => a.IdConsulta == idConsulta);

    public Agendamento Add(Agendamento agendamento)
    {
        var doc = _db.LoadDocument();
        doc.Agendamentos.Add(agendamento);
        _db.SaveDocument(doc);
        return agendamento;
    }

    public void Update(Agendamento agendamento)
    {
        var doc = _db.LoadDocument();
        var idx = doc.Agendamentos.FindIndex(a => a.IdAgendamento == agendamento.IdAgendamento);
        if (idx >= 0) doc.Agendamentos[idx] = agendamento;
        _db.SaveDocument(doc);
    }

    public void Delete(string id)
    {
        var doc = _db.LoadDocument();
        doc.Agendamentos.RemoveAll(a => a.IdAgendamento == id);
        _db.SaveDocument(doc);
    }
}
