using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;

namespace HealthMap.Domain.Services;

/// <summary>
/// Regras de negócio para agendamento de consultas.
/// Garante integridade referencial e ausência de conflito de horário,
/// além de criar o registro de agendamento (1:1 com a consulta).
/// </summary>
public class ConsultaService
{
    private readonly IConsultaRepository _consultas;
    private readonly IAgendamentoRepository _agendamentos;
    private readonly IMedicoRepository _medicos;
    private readonly IPacienteRepository _pacientes;
    private readonly IDisponibilidadeMedicoRepository _disponibilidades;

    public ConsultaService(
        IConsultaRepository consultas,
        IAgendamentoRepository agendamentos,
        IMedicoRepository medicos,
        IPacienteRepository pacientes,
        IDisponibilidadeMedicoRepository disponibilidades)
    {
        _consultas = consultas;
        _agendamentos = agendamentos;
        _medicos = medicos;
        _pacientes = pacientes;
        _disponibilidades = disponibilidades;
    }

    public (Consulta Consulta, Agendamento Agendamento) Agendar(
        string idPaciente,
        string idMedico,
        DateTime dataHora,
        string queixaPrincipal,
        string idUsuarioSolicitante)
    {
        if (_pacientes.GetById(idPaciente) is null)
            throw new DomainException("Paciente não encontrado.");

        if (_medicos.GetById(idMedico) is null)
            throw new DomainException("Médico não encontrado.");

        if (dataHora < DateTime.Now)
            throw new DomainException("A data/hora da consulta deve ser futura.");

        if (TemConflito(idMedico, dataHora))
            throw new DomainException("Já existe uma consulta agendada para este médico neste horário.");

        var consulta = new Consulta
        {
            IdPaciente = idPaciente,
            IdMedico = idMedico,
            DataHora = dataHora,
            QueixaPrincipal = queixaPrincipal,
            Status = ConsultaStatus.Pendente,
        };

        var consultaSalva = _consultas.Add(consulta);

        var agendamento = new Agendamento
        {
            IdConsulta = consultaSalva.IdConsulta,
            IdUsuarioSolicitante = idUsuarioSolicitante,
            DataSolicitacao = DateTime.Now,
        };
        var agendamentoSalvo = _agendamentos.Add(agendamento);

        return (consultaSalva, agendamentoSalvo);
    }

    public void Cancelar(string idConsulta)
    {
        var consulta = _consultas.GetById(idConsulta)
            ?? throw new DomainException("Consulta não encontrada.");

        if (consulta.Status == ConsultaStatus.Concluida)
            throw new DomainException("Não é possível cancelar uma consulta já concluída.");

        consulta.Status = ConsultaStatus.Cancelada;
        _consultas.Update(consulta);
    }

    public void Concluir(string idConsulta)
    {
        var consulta = _consultas.GetById(idConsulta)
            ?? throw new DomainException("Consulta não encontrada.");

        consulta.Status = ConsultaStatus.Concluida;
        _consultas.Update(consulta);
    }

    private bool TemConflito(string idMedico, DateTime dataHora)
    {
        return _consultas.GetByMedico(idMedico).Any(c =>
            c.Status != ConsultaStatus.Cancelada &&
            Math.Abs((c.DataHora - dataHora).TotalMinutes) < 30);
    }
}

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
