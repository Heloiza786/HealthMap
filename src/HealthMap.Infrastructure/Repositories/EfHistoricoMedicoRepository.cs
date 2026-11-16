using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="HistoricoMedico"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfHistoricoMedicoRepository : IHistoricoMedicoRepository
{
    private readonly AppDbContext _db;

    public EfHistoricoMedicoRepository(AppDbContext db) => _db = db;

    public IEnumerable<HistoricoMedico> GetByPaciente(string idPaciente) =>
        _db.HistoricosMedicos.AsNoTracking().Where(h => h.IdPaciente == idPaciente).ToList();

    public HistoricoMedico? GetById(string id) =>
        _db.HistoricosMedicos.AsNoTracking().FirstOrDefault(h => h.IdHistorico == id);

    public HistoricoMedico Add(HistoricoMedico historico)
    {
        _db.HistoricosMedicos.Add(historico);
        _db.SaveChanges();
        return historico;
    }

    public void Update(HistoricoMedico historico)
    {
        _db.HistoricosMedicos.Update(historico);
        _db.SaveChanges();
    }

    public void Delete(string id)
    {
        var historico = _db.HistoricosMedicos.Find(id);
        if (historico is not null)
        {
            _db.HistoricosMedicos.Remove(historico);
            _db.SaveChanges();
        }
    }
}
