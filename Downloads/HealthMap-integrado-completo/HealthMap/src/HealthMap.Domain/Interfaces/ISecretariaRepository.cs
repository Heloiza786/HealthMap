using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface ISecretariaRepository
{
    IEnumerable<Secretaria> GetAll();
    Secretaria? GetById(string idUsuario);
    Secretaria Add(Secretaria secretaria);
    void Update(Secretaria secretaria);
    void Delete(string idUsuario);
}
