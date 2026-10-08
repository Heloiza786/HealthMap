using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface IHistoricoMedicoRepository
{
    IEnumerable<HistoricoMedico> GetByPaciente(string idPaciente);
    HistoricoMedico? GetById(string id);
    HistoricoMedico Add(HistoricoMedico historico);
    void Update(HistoricoMedico historico);
    void Delete(string id);
}
