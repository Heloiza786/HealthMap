using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="Paciente"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfPacienteRepository : IPacienteRepository
{
    private readonly AppDbContext _db;

    public EfPacienteRepository(AppDbContext db) => _db = db;

    public IEnumerable<Paciente> GetAll() => _db.Pacientes.AsNoTracking().ToList();

    public Paciente? GetById(string idUsuario) => _db.Pacientes.AsNoTracking().FirstOrDefault(p => p.IdUsuario == idUsuario);

    public Paciente Add(Paciente paciente)
    {
        _db.Pacientes.Add(paciente);
        _db.SaveChanges();
        return paciente;
    }

    public void Update(Paciente paciente)
    {
        _db.Pacientes.Update(paciente);
        _db.SaveChanges();
    }

    public void Delete(string idUsuario)
    {
        var paciente = _db.Pacientes.Find(idUsuario);
        if (paciente is not null)
        {
            _db.Pacientes.Remove(paciente);
            _db.SaveChanges();
        }
    }
}
