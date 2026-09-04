using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface IEspecialidadeRepository
{
    IEnumerable<Especialidade> GetAll();
    Especialidade? GetById(string id);
    Especialidade? GetByNome(string nome);
    Especialidade Add(Especialidade especialidade);
    void Update(Especialidade especialidade);
    void Delete(string id);
}
