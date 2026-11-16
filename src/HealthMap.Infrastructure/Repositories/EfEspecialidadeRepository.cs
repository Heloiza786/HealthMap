using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="Especialidade"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfEspecialidadeRepository : IEspecialidadeRepository
{
    private readonly AppDbContext _db;

    public EfEspecialidadeRepository(AppDbContext db) => _db = db;

    public IEnumerable<Especialidade> GetAll() => _db.Especialidades.AsNoTracking().ToList();

    public Especialidade? GetById(string id) => _db.Especialidades.AsNoTracking().FirstOrDefault(e => e.IdEspecialidade == id);

    public Especialidade? GetByNome(string nome) => _db.Especialidades.AsNoTracking()
        .FirstOrDefault(e => e.Nome.ToLower() == nome.ToLower());

    public Especialidade Add(Especialidade especialidade)
    {
        _db.Especialidades.Add(especialidade);
        _db.SaveChanges();
        return especialidade;
    }

    public void Update(Especialidade especialidade)
    {
        _db.Especialidades.Update(especialidade);
        _db.SaveChanges();
    }

    public void Delete(string id)
    {
        var especialidade = _db.Especialidades.Find(id);
        if (especialidade is not null)
        {
            _db.Especialidades.Remove(especialidade);
            _db.SaveChanges();
        }
    }
}
