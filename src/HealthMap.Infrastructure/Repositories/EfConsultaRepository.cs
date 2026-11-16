using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="Consulta"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfConsultaRepository : IConsultaRepository
{
    private readonly AppDbContext _db;

    public EfConsultaRepository(AppDbContext db) => _db = db;

    public IEnumerable<Consulta> GetAll() => _db.Consultas.AsNoTracking().ToList();

    public Consulta? GetById(string id) => _db.Consultas.AsNoTracking().FirstOrDefault(c => c.IdConsulta == id);

    public IEnumerable<Consulta> GetByMedico(string idMedico) =>
        _db.Consultas.AsNoTracking().Where(c => c.IdMedico == idMedico).ToList();

    public IEnumerable<Consulta> GetByPaciente(string idPaciente) =>
        _db.Consultas.AsNoTracking().Where(c => c.IdPaciente == idPaciente).ToList();

    public Consulta Add(Consulta consulta)
    {
        _db.Consultas.Add(consulta);
        _db.SaveChanges();
        return consulta;
    }

    public void Update(Consulta consulta)
    {
        _db.Consultas.Update(consulta);
        _db.SaveChanges();
    }

    public void Delete(string id)
    {
        var consulta = _db.Consultas.Find(id);
        if (consulta is not null)
        {
            _db.Consultas.Remove(consulta);
            _db.SaveChanges();
        }
    }
}
