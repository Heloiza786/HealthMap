using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly JsonDatabase _db;

    public PacienteRepository(JsonDatabase db) => _db = db;

    public IEnumerable<Paciente> GetAll() => _db.LoadDocument().Pacientes;

    public Paciente? GetById(string idUsuario) => _db.LoadDocument().Pacientes.FirstOrDefault(p => p.IdUsuario == idUsuario);

    public Paciente Add(Paciente paciente)
    {
        var doc = _db.LoadDocument();
        doc.Pacientes.Add(paciente);
        _db.SaveDocument(doc);
        return paciente;
    }

    public void Update(Paciente paciente)
    {
        var doc = _db.LoadDocument();
        var idx = doc.Pacientes.FindIndex(p => p.IdUsuario == paciente.IdUsuario);
        if (idx >= 0) doc.Pacientes[idx] = paciente;
        _db.SaveDocument(doc);
    }

    public void Delete(string idUsuario)
    {
        var doc = _db.LoadDocument();
        doc.Pacientes.RemoveAll(p => p.IdUsuario == idUsuario);
        _db.SaveDocument(doc);
    }
}
