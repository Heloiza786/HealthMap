using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface IMedicoRepository
{
    IEnumerable<Medico> GetAll();
    Medico? GetById(string idUsuario);
    Medico? GetByCrm(string crm);
    Medico Add(Medico medico);
    void Update(Medico medico);
    void Delete(string idUsuario);
}
