using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;

namespace HealthMap.Domain.Services;

/// <summary>
/// Registra o feedback de uma consulta (cardinalidade 1:0..1).
/// </summary>
public class FeedbackService
{
    private readonly IFeedbackRepository _feedbacks;
    private readonly IConsultaRepository _consultas;

    public FeedbackService(IFeedbackRepository feedbacks, IConsultaRepository consultas)
    {
        _feedbacks = feedbacks;
        _consultas = consultas;
    }

    public Feedback Registrar(string idConsulta, int nota, string comentario)
    {
        if (_consultas.GetById(idConsulta) is null)
            throw new DomainException("Consulta não encontrada.");

        if (_feedbacks.GetByConsulta(idConsulta) is not null)
            throw new DomainException("Esta consulta já possui feedback registrado.");

        if (nota < 1 || nota > 5)
            throw new DomainException("A nota deve estar entre 1 e 5.");

        return _feedbacks.Add(new Feedback
        {
            IdConsulta = idConsulta,
            NotaAtendimento = nota,
            Comentario = comentario,
            DataFeedback = DateTime.Now,
        });
    }
}
