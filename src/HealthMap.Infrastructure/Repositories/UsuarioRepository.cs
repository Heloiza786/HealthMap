using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly JsonDatabase _db;

    public UsuarioRepository(JsonDatabase db) => _db = db;

    public IEnumerable<Usuario> GetAll() => _db.LoadDocument().Usuarios;

    public Usuario? GetById(string id) => _db.LoadDocument().Usuarios.FirstOrDefault(u => u.IdUsuario == id);

    public Usuario? GetByEmail(string email) => _db.LoadDocument().Usuarios.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public Usuario? GetByCpf(string cpf) => _db.LoadDocument().Usuarios.FirstOrDefault(u => u.Cpf == cpf);

    public Usuario Add(Usuario usuario)
    {
        var doc = _db.LoadDocument();
        doc.Usuarios.Add(usuario);
        _db.SaveDocument(doc);
        return usuario;
    }

    public void Update(Usuario usuario)
    {
        var doc = _db.LoadDocument();
        var idx = doc.Usuarios.FindIndex(u => u.IdUsuario == usuario.IdUsuario);
        if (idx >= 0) doc.Usuarios[idx] = usuario;
        _db.SaveDocument(doc);
    }

    public void Delete(string id)
    {
        var doc = _db.LoadDocument();
        doc.Usuarios.RemoveAll(u => u.IdUsuario == id);
        _db.SaveDocument(doc);
    }
}
