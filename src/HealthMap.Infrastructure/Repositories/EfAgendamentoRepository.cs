using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="Agendamento"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfAgendamentoRepository : IAgendamentoRepository
{
    private readonly AppDbContext _db;

    public EfAgendamentoRepository(AppDbContext db) => _db = db;

    public IEnumerable<Agendamento> GetAll() => _db.Agendamentos.AsNoTracking().ToList();

    public Agendamento? GetById(string id) => _db.Agendamentos.AsNoTracking().FirstOrDefault(a => a.IdAgendamento == id);

    public Agendamento? GetByConsulta(string idConsulta) =>
        _db.Agendamentos.AsNoTracking().FirstOrDefault(a => a.IdConsulta == idConsulta);

    public Agendamento Add(Agendamento agendamento)
    {
        _db.Agendamentos.Add(agendamento);
        _db.SaveChanges();
        return agendamento;
    }

    public void Update(Agendamento agendamento)
    {
        _db.Agendamentos.Update(agendamento);
        _db.SaveChanges();
    }

    public void Delete(string id)
    {
        var agendamento = _db.Agendamentos.Find(id);
        if (agendamento is not null)
        {
            _db.Agendamentos.Remove(agendamento);
            _db.SaveChanges();
        }
    }
}
