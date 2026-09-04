using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface IPacienteRepository
{
    IEnumerable<Paciente> GetAll();
    Paciente? GetById(string idUsuario);
    Paciente Add(Paciente paciente);
    void Update(Paciente paciente);
    void Delete(string idUsuario);
}
