using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="Secretaria"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfSecretariaRepository : ISecretariaRepository
{
    private readonly AppDbContext _db;

    public EfSecretariaRepository(AppDbContext db) => _db = db;

    public IEnumerable<Secretaria> GetAll() => _db.Secretarias.AsNoTracking().ToList();

    public Secretaria? GetById(string idUsuario) => _db.Secretarias.AsNoTracking().FirstOrDefault(s => s.IdUsuario == idUsuario);

    public Secretaria Add(Secretaria secretaria)
    {
        _db.Secretarias.Add(secretaria);
        _db.SaveChanges();
        return secretaria;
    }

    public void Update(Secretaria secretaria)
    {
        _db.Secretarias.Update(secretaria);
        _db.SaveChanges();
    }

    public void Delete(string idUsuario)
    {
        var secretaria = _db.Secretarias.Find(idUsuario);
        if (secretaria is not null)
        {
            _db.Secretarias.Remove(secretaria);
            _db.SaveChanges();
        }
    }
}
