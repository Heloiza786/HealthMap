using HealthMap.Domain.Entities;
using HealthMap.Domain.Services;
using HealthMap.Domain.Tests.Fakes;
using Xunit;

namespace HealthMap.Domain.Tests;

public class FeedbackServiceTests
{
    private readonly InMemoryFeedbackRepository _feedbacks;
    private readonly InMemoryConsultaRepository _consultas;
    private readonly FeedbackService _service;

    public FeedbackServiceTests()
    {
        _feedbacks = new InMemoryFeedbackRepository();
        _consultas = new InMemoryConsultaRepository(
            new Consulta { IdConsulta = "cons-1", IdPaciente = "pac-1", IdMedico = "med-1", Status = ConsultaStatus.Concluida });
        _service = new FeedbackService(_feedbacks, _consultas);
    }

    [Fact]
    public void Registrar_QuandoValido_CriaFeedback()
    {
        var feedback = _service.Registrar("cons-1", 5, "Ótimo atendimento");

        Assert.Equal("cons-1", feedback.IdConsulta);
        Assert.Equal(5, feedback.NotaAtendimento);
        Assert.Single(_feedbacks.GetAll());
    }

    [Fact]
    public void Registrar_ConsultaInexistente_LancaDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => _service.Registrar("cons-x", 5, ""));

        Assert.Equal("Consulta não encontrada.", ex.Message);
    }

    [Fact]
    public void Registrar_FeedbackDuplicado_LancaDomainException()
    {
        _service.Registrar("cons-1", 5, "Primeiro");

        var ex = Assert.Throws<DomainException>(() => _service.Registrar("cons-1", 4, "Segundo"));

        Assert.Equal("Esta consulta já possui feedback registrado.", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Registrar_NotaForaDoIntervalo_LancaDomainException(int nota)
    {
        var ex = Assert.Throws<DomainException>(() => _service.Registrar("cons-1", nota, ""));

        Assert.Equal("A nota deve estar entre 1 e 5.", ex.Message);
    }
}
