using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="Usuario"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfUsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;

    public EfUsuarioRepository(AppDbContext db) => _db = db;

    public IEnumerable<Usuario> GetAll() => _db.Usuarios.AsNoTracking().ToList();

    public Usuario? GetById(string id) => _db.Usuarios.AsNoTracking().FirstOrDefault(u => u.IdUsuario == id);

    public Usuario? GetByEmail(string email) => _db.Usuarios.AsNoTracking()
        .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

    public Usuario? GetByCpf(string cpf) => _db.Usuarios.AsNoTracking().FirstOrDefault(u => u.Cpf == cpf);

    public Usuario Add(Usuario usuario)
    {
        _db.Usuarios.Add(usuario);
        _db.SaveChanges();
        return usuario;
    }

    public void Update(Usuario usuario)
    {
        _db.Usuarios.Update(usuario);
        _db.SaveChanges();
    }

    public void Delete(string id)
    {
        var usuario = _db.Usuarios.Find(id);
        if (usuario is not null)
        {
            _db.Usuarios.Remove(usuario);
            _db.SaveChanges();
        }
    }
}
