using HealthMap.Domain.Entities;
using HealthMap.Domain.Services;
using HealthMap.Domain.Tests.Fakes;
using Xunit;

namespace HealthMap.Domain.Tests;

public class ConsultaServiceTests
{
    private const string IdPaciente = "pac-1";
    private const string IdMedico = "med-1";
    private const string IdSolicitante = "usr-3";

    private readonly InMemoryPacienteRepository _pacientes;
    private readonly InMemoryMedicoRepository _medicos;
    private readonly InMemoryConsultaRepository _consultas;
    private readonly InMemoryAgendamentoRepository _agendamentos;
    private readonly InMemoryDisponibilidadeMedicoRepository _disponibilidades;
    private readonly ConsultaService _service;

    public ConsultaServiceTests()
    {
        _pacientes = new InMemoryPacienteRepository(new Paciente { IdUsuario = IdPaciente, PlanoSaude = "Unimed" });
        _medicos = new InMemoryMedicoRepository(new Medico { IdUsuario = IdMedico, IdEspecialidade = "esp-1", Crm = "123456-SP" });
        _consultas = new InMemoryConsultaRepository();
        _agendamentos = new InMemoryAgendamentoRepository();
        _disponibilidades = new InMemoryDisponibilidadeMedicoRepository(
            new DisponibilidadeMedico
            {
                IdDisponibilidade = "disp-1",
                IdMedico = IdMedico,
                DiaSemana = (int)DayOfWeek.Monday,
                HoraInicio = "08:00",
                HoraFim = "17:00",
            });
        _service = new ConsultaService(_consultas, _agendamentos, _medicos, _pacientes, _disponibilidades);
    }

    /// <summary>Cria uma data futura garantidamente em uma segunda-feira dentro do horário de disponibilidade.</summary>
    private static DateTime ProximaSegunda(int hora)
    {
        var hoje = DateTime.Today;
        var dias = ((int)DayOfWeek.Monday - (int)hoje.DayOfWeek + 7) % 7;
        if (dias == 0) dias = 7;
        return hoje.AddDays(dias).AddHours(hora);
    }

    [Fact]
    public void Agendar_QuandoValido_CriaConsultaPendenteEAgendamento()
    {
        var dataHora = ProximaSegunda(10);

        var (consulta, agendamento) = _service.Agendar(IdPaciente, IdMedico, dataHora, "Dor", IdSolicitante);

        Assert.Equal(ConsultaStatus.Pendente, consulta.Status);
        Assert.Equal(IdPaciente, consulta.IdPaciente);
        Assert.Equal(IdMedico, consulta.IdMedico);
        Assert.Equal(consulta.IdConsulta, agendamento.IdConsulta);
        Assert.Equal(IdSolicitante, agendamento.IdUsuarioSolicitante);
        Assert.Single(_consultas.GetAll());
        Assert.Single(_agendamentos.GetAll());
    }

    [Fact]
    public void Agendar_PacienteInexistente_LancaDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            _service.Agendar("pac-x", IdMedico, ProximaSegunda(10), "", IdSolicitante));

        Assert.Equal("Paciente não encontrado.", ex.Message);
    }

    [Fact]
    public void Agendar_MedicoInexistente_LancaDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            _service.Agendar(IdPaciente, "med-x", ProximaSegunda(10), "", IdSolicitante));

        Assert.Equal("Médico não encontrado.", ex.Message);
    }

    [Fact]
    public void Agendar_DataPassada_LancaDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            _service.Agendar(IdPaciente, IdMedico, DateTime.Now.AddHours(-2), "", IdSolicitante));

        Assert.Equal("A data/hora da consulta deve ser futura.", ex.Message);
    }

    [Fact]
    public void Agendar_ForaDaDisponibilidade_LancaDomainException()
    {
        var segunda = ProximaSegunda(19); // 19h está fora de 08:00-17:00

        var ex = Assert.Throws<DomainException>(() =>
            _service.Agendar(IdPaciente, IdMedico, segunda, "", IdSolicitante));

        Assert.Equal("O horário selecionado está fora da disponibilidade do médico.", ex.Message);
    }

    [Fact]
    public void Agendar_DiaSemanaSemDisponibilidade_LancaDomainException()
    {
        // Terça-feira não tem disponibilidade cadastrada.
        var terca = ProximaSegunda(10).AddDays(1);

        var ex = Assert.Throws<DomainException>(() =>
            _service.Agendar(IdPaciente, IdMedico, terca, "", IdSolicitante));

        Assert.Equal("O horário selecionado está fora da disponibilidade do médico.", ex.Message);
    }

    [Fact]
    public void Agendar_SemDisponibilidadeCadastrada_LancaDomainException()
    {
        var service = new ConsultaService(_consultas, _agendamentos, _medicos, _pacientes,
            new InMemoryDisponibilidadeMedicoRepository());

        Assert.Throws<DomainException>(() =>
            service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante));
    }

    [Fact]
    public void Agendar_ConflitoDentroDe30Minutos_LancaDomainException()
    {
        var primeira = ProximaSegunda(10);
        _service.Agendar(IdPaciente, IdMedico, primeira, "", IdSolicitante);

        var ex = Assert.Throws<DomainException>(() =>
            _service.Agendar(IdPaciente, IdMedico, primeira.AddMinutes(20), "", IdSolicitante));

        Assert.Contains("Já existe uma consulta", ex.Message);
    }

    [Fact]
    public void Agendar_MesmoHorarioDeConsultaCancelada_NaoGeraConflito()
    {
        var primeira = ProximaSegunda(10);
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, primeira, "", IdSolicitante);
        _service.Cancelar(consulta.IdConsulta);

        var (segunda, _) = _service.Agendar(IdPaciente, IdMedico, primeira, "", IdSolicitante);

        Assert.NotEqual(consulta.IdConsulta, segunda.IdConsulta);
        Assert.Equal(2, _consultas.GetAll().Count());
    }

    [Fact]
    public void Cancelar_ConsultaNaoEncontrada_LancaDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => _service.Cancelar("cons-x"));

        Assert.Equal("Consulta não encontrada.", ex.Message);
    }

    [Fact]
    public void Cancelar_ConsultaConcluida_LancaDomainException()
    {
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante);
        _service.Concluir(consulta.IdConsulta);

        var ex = Assert.Throws<DomainException>(() => _service.Cancelar(consulta.IdConsulta));

        Assert.Equal("Não é possível cancelar uma consulta já concluída.", ex.Message);
    }

    [Fact]
    public void Cancelar_ConsultaAtiva_DefineStatusCancelada()
    {
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante);

        _service.Cancelar(consulta.IdConsulta);

        Assert.Equal(ConsultaStatus.Cancelada, _consultas.GetById(consulta.IdConsulta)!.Status);
    }

    [Fact]
    public void Concluir_ConsultaNaoEncontrada_LancaDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => _service.Concluir("cons-x"));

        Assert.Equal("Consulta não encontrada.", ex.Message);
    }

    [Fact]
    public void Concluir_ConsultaCancelada_LancaDomainException()
    {
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante);
        _service.Cancelar(consulta.IdConsulta);

        var ex = Assert.Throws<DomainException>(() => _service.Concluir(consulta.IdConsulta));

        Assert.Equal("Não é possível concluir uma consulta cancelada.", ex.Message);
    }

    [Fact]
    public void Concluir_ConsultaAtiva_DefineStatusConcluida()
    {
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante);

        _service.Concluir(consulta.IdConsulta);

        Assert.Equal(ConsultaStatus.Concluida, _consultas.GetById(consulta.IdConsulta)!.Status);
    }

    [Fact]
    public void Reagendar_QuandoValido_DefineStatusReagendadaENovaData()
    {
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante);
        var novaData = ProximaSegunda(14);

        _service.Reagendar(consulta.IdConsulta, novaData);

        var atualizada = _consultas.GetById(consulta.IdConsulta)!;
        Assert.Equal(ConsultaStatus.Reagendada, atualizada.Status);
        Assert.Equal(novaData, atualizada.DataHora);
    }

    [Fact]
    public void Reagendar_ConsultaNaoEncontrada_LancaDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => _service.Reagendar("cons-x", ProximaSegunda(10)));

        Assert.Equal("Consulta não encontrada.", ex.Message);
    }

    [Fact]
    public void Reagendar_ConsultaConcluida_LancaDomainException()
    {
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante);
        _service.Concluir(consulta.IdConsulta);

        var ex = Assert.Throws<DomainException>(() => _service.Reagendar(consulta.IdConsulta, ProximaSegunda(14)));

        Assert.Equal("Não é possível reagendar uma consulta já concluída.", ex.Message);
    }

    [Fact]
    public void Reagendar_ConsultaCancelada_LancaDomainException()
    {
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante);
        _service.Cancelar(consulta.IdConsulta);

        var ex = Assert.Throws<DomainException>(() => _service.Reagendar(consulta.IdConsulta, ProximaSegunda(14)));

        Assert.Equal("Não é possível reagendar uma consulta cancelada.", ex.Message);
    }

    [Fact]
    public void Reagendar_DataPassada_LancaDomainException()
    {
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante);

        var ex = Assert.Throws<DomainException>(() =>
            _service.Reagendar(consulta.IdConsulta, DateTime.Now.AddHours(-1)));

        Assert.Equal("A nova data/hora deve ser futura.", ex.Message);
    }

    [Fact]
    public void Reagendar_ForaDaDisponibilidade_LancaDomainException()
    {
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante);

        var ex = Assert.Throws<DomainException>(() =>
            _service.Reagendar(consulta.IdConsulta, ProximaSegunda(19)));

        Assert.Equal("O horário selecionado está fora da disponibilidade do médico.", ex.Message);
    }

    [Fact]
    public void Reagendar_ComConflito_LancaDomainException()
    {
        var (consulta, _) = _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(10), "", IdSolicitante);
        _service.Agendar(IdPaciente, IdMedico, ProximaSegunda(11), "", IdSolicitante);

        var ex = Assert.Throws<DomainException>(() =>
            _service.Reagendar(consulta.IdConsulta, ProximaSegunda(11).AddMinutes(5)));

        Assert.Contains("Já existe uma consulta", ex.Message);
    }
}
