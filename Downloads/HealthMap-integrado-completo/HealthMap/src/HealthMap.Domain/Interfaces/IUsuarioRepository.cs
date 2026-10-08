using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface IUsuarioRepository
{
    IEnumerable<Usuario> GetAll();
    Usuario? GetById(string id);
    Usuario? GetByEmail(string email);
    Usuario? GetByCpf(string cpf);
    Usuario Add(Usuario usuario);
    void Update(Usuario usuario);
    void Delete(string id);
}
