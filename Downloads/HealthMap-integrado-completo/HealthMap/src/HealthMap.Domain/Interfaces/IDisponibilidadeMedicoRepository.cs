using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface IDisponibilidadeMedicoRepository
{
    IEnumerable<DisponibilidadeMedico> GetByMedico(string idMedico);
    DisponibilidadeMedico? GetById(string id);
    DisponibilidadeMedico Add(DisponibilidadeMedico disponibilidade);
    void Update(DisponibilidadeMedico disponibilidade);
    void Delete(string id);
}
