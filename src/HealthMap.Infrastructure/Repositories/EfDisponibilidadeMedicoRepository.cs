using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="DisponibilidadeMedico"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfDisponibilidadeMedicoRepository : IDisponibilidadeMedicoRepository
{
    private readonly AppDbContext _db;

    public EfDisponibilidadeMedicoRepository(AppDbContext db) => _db = db;

    public IEnumerable<DisponibilidadeMedico> GetByMedico(string idMedico) =>
        _db.DisponibilidadesMedico.AsNoTracking().Where(d => d.IdMedico == idMedico).ToList();

    public DisponibilidadeMedico? GetById(string id) =>
        _db.DisponibilidadesMedico.AsNoTracking().FirstOrDefault(d => d.IdDisponibilidade == id);

    public DisponibilidadeMedico Add(DisponibilidadeMedico disponibilidade)
    {
        _db.DisponibilidadesMedico.Add(disponibilidade);
        _db.SaveChanges();
        return disponibilidade;
    }

    public void Update(DisponibilidadeMedico disponibilidade)
    {
        _db.DisponibilidadesMedico.Update(disponibilidade);
        _db.SaveChanges();
    }

    public void Delete(string id)
    {
        var disponibilidade = _db.DisponibilidadesMedico.Find(id);
        if (disponibilidade is not null)
        {
            _db.DisponibilidadesMedico.Remove(disponibilidade);
            _db.SaveChanges();
        }
    }
}
