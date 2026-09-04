using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface IConsultaRepository
{
    IEnumerable<Consulta> GetAll();
    Consulta? GetById(string id);
    IEnumerable<Consulta> GetByMedico(string idMedico);
    IEnumerable<Consulta> GetByPaciente(string idPaciente);
    Consulta Add(Consulta consulta);
    void Update(Consulta consulta);
    void Delete(string id);
}
