using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface IAgendamentoRepository
{
    IEnumerable<Agendamento> GetAll();
    Agendamento? GetById(string id);
    Agendamento? GetByConsulta(string idConsulta);
    Agendamento Add(Agendamento agendamento);
    void Update(Agendamento agendamento);
    void Delete(string id);
}
