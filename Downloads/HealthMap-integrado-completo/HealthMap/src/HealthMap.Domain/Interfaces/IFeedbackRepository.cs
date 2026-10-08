using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface IFeedbackRepository
{
    IEnumerable<Feedback> GetAll();
    Feedback? GetById(string id);
    Feedback? GetByConsulta(string idConsulta);
    Feedback Add(Feedback feedback);
    void Update(Feedback feedback);
    void Delete(string id);
}
