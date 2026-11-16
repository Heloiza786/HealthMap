using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="Feedback"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfFeedbackRepository : IFeedbackRepository
{
    private readonly AppDbContext _db;

    public EfFeedbackRepository(AppDbContext db) => _db = db;

    public IEnumerable<Feedback> GetAll() => _db.Feedbacks.AsNoTracking().ToList();

    public Feedback? GetById(string id) => _db.Feedbacks.AsNoTracking().FirstOrDefault(f => f.IdFeedback == id);

    public Feedback? GetByConsulta(string idConsulta) =>
        _db.Feedbacks.AsNoTracking().FirstOrDefault(f => f.IdConsulta == idConsulta);

    public Feedback Add(Feedback feedback)
    {
        _db.Feedbacks.Add(feedback);
        _db.SaveChanges();
        return feedback;
    }

    public void Update(Feedback feedback)
    {
        _db.Feedbacks.Update(feedback);
        _db.SaveChanges();
    }

    public void Delete(string id)
    {
        var feedback = _db.Feedbacks.Find(id);
        if (feedback is not null)
        {
            _db.Feedbacks.Remove(feedback);
            _db.SaveChanges();
        }
    }
}
