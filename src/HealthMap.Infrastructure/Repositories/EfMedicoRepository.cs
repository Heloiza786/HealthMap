using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="Medico"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfMedicoRepository : IMedicoRepository
{
    private readonly AppDbContext _db;

    public EfMedicoRepository(AppDbContext db) => _db = db;

    public IEnumerable<Medico> GetAll() => _db.Medicos.AsNoTracking().ToList();

    public Medico? GetById(string idUsuario) => _db.Medicos.AsNoTracking().FirstOrDefault(m => m.IdUsuario == idUsuario);

    public Medico? GetByCrm(string crm) => _db.Medicos.AsNoTracking().FirstOrDefault(m => m.Crm == crm);

    public Medico Add(Medico medico)
    {
        _db.Medicos.Add(medico);
        _db.SaveChanges();
        return medico;
    }

    public void Update(Medico medico)
    {
        _db.Medicos.Update(medico);
        _db.SaveChanges();
    }

    public void Delete(string idUsuario)
    {
        var medico = _db.Medicos.Find(idUsuario);
        if (medico is not null)
        {
            _db.Medicos.Remove(medico);
            _db.SaveChanges();
        }
    }
}
