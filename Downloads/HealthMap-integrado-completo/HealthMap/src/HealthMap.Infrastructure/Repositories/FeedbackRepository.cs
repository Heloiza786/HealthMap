using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly JsonDatabase _db;

    public FeedbackRepository(JsonDatabase db) => _db = db;

    public IEnumerable<Feedback> GetAll() => _db.LoadDocument().Feedbacks;

    public Feedback? GetById(string id) => _db.LoadDocument().Feedbacks.FirstOrDefault(f => f.IdFeedback == id);

    public Feedback? GetByConsulta(string idConsulta) => _db.LoadDocument().Feedbacks.FirstOrDefault(f => f.IdConsulta == idConsulta);

    public Feedback Add(Feedback feedback)
    {
        var doc = _db.LoadDocument();
        doc.Feedbacks.Add(feedback);
        _db.SaveDocument(doc);
        return feedback;
    }

    public void Update(Feedback feedback)
    {
        var doc = _db.LoadDocument();
        var idx = doc.Feedbacks.FindIndex(f => f.IdFeedback == feedback.IdFeedback);
        if (idx >= 0) doc.Feedbacks[idx] = feedback;
        _db.SaveDocument(doc);
    }

    public void Delete(string id)
    {
        var doc = _db.LoadDocument();
        doc.Feedbacks.RemoveAll(f => f.IdFeedback == id);
        _db.SaveDocument(doc);
    }
}
