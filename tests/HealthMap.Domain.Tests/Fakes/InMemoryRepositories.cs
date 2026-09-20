using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;

namespace HealthMap.Domain.Tests.Fakes;

/// <summary>
/// Repositórios em memória usados pelos testes unitários dos serviços de domínio.
/// </summary>
public class InMemoryUsuarioRepository : IUsuarioRepository
{
    private readonly List<Usuario> _items = new();

    public InMemoryUsuarioRepository(params Usuario[] items) => _items.AddRange(items);

    public IEnumerable<Usuario> GetAll() => _items.ToList();
    public Usuario? GetById(string id) => _items.FirstOrDefault(u => u.IdUsuario == id);
    public Usuario? GetByEmail(string email) => _items.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    public Usuario? GetByCpf(string cpf) => _items.FirstOrDefault(u => u.Cpf == cpf);

    public Usuario Add(Usuario usuario)
    {
        _items.Add(usuario);
        return usuario;
    }

    public void Update(Usuario usuario)
    {
        var idx = _items.FindIndex(u => u.IdUsuario == usuario.IdUsuario);
        if (idx >= 0) _items[idx] = usuario;
    }

    public void Delete(string id) => _items.RemoveAll(u => u.IdUsuario == id);
}

public class InMemoryPacienteRepository : IPacienteRepository
{
    private readonly List<Paciente> _items = new();

    public InMemoryPacienteRepository(params Paciente[] items) => _items.AddRange(items);

    public IEnumerable<Paciente> GetAll() => _items.ToList();
    public Paciente? GetById(string idUsuario) => _items.FirstOrDefault(p => p.IdUsuario == idUsuario);

    public Paciente Add(Paciente paciente)
    {
        _items.Add(paciente);
        return paciente;
    }

    public void Update(Paciente paciente)
    {
        var idx = _items.FindIndex(p => p.IdUsuario == paciente.IdUsuario);
        if (idx >= 0) _items[idx] = paciente;
    }

    public void Delete(string idUsuario) => _items.RemoveAll(p => p.IdUsuario == idUsuario);
}

public class InMemoryMedicoRepository : IMedicoRepository
{
    private readonly List<Medico> _items = new();

    public InMemoryMedicoRepository(params Medico[] items) => _items.AddRange(items);

    public IEnumerable<Medico> GetAll() => _items.ToList();
    public Medico? GetById(string idUsuario) => _items.FirstOrDefault(m => m.IdUsuario == idUsuario);
    public Medico? GetByCrm(string crm) => _items.FirstOrDefault(m => m.Crm == crm);

    public Medico Add(Medico medico)
    {
        _items.Add(medico);
        return medico;
    }

    public void Update(Medico medico)
    {
        var idx = _items.FindIndex(m => m.IdUsuario == medico.IdUsuario);
        if (idx >= 0) _items[idx] = medico;
    }

    public void Delete(string idUsuario) => _items.RemoveAll(m => m.IdUsuario == idUsuario);
}

public class InMemoryEspecialidadeRepository : IEspecialidadeRepository
{
    private readonly List<Especialidade> _items = new();

    public InMemoryEspecialidadeRepository(params Especialidade[] items) => _items.AddRange(items);

    public IEnumerable<Especialidade> GetAll() => _items.ToList();
    public Especialidade? GetById(string id) => _items.FirstOrDefault(e => e.IdEspecialidade == id);
    public Especialidade? GetByNome(string nome) => _items.FirstOrDefault(e => e.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase));

    public Especialidade Add(Especialidade especialidade)
    {
        _items.Add(especialidade);
        return especialidade;
    }

    public void Update(Especialidade especialidade)
    {
        var idx = _items.FindIndex(e => e.IdEspecialidade == especialidade.IdEspecialidade);
        if (idx >= 0) _items[idx] = especialidade;
    }

    public void Delete(string id) => _items.RemoveAll(e => e.IdEspecialidade == id);
}

public class InMemoryConsultaRepository : IConsultaRepository
{
    private readonly List<Consulta> _items = new();

    public InMemoryConsultaRepository(params Consulta[] items) => _items.AddRange(items);

    public IEnumerable<Consulta> GetAll() => _items.ToList();
    public Consulta? GetById(string id) => _items.FirstOrDefault(c => c.IdConsulta == id);
    public IEnumerable<Consulta> GetByMedico(string idMedico) => _items.Where(c => c.IdMedico == idMedico).ToList();
    public IEnumerable<Consulta> GetByPaciente(string idPaciente) => _items.Where(c => c.IdPaciente == idPaciente).ToList();

    public Consulta Add(Consulta consulta)
    {
        _items.Add(consulta);
        return consulta;
    }

    public void Update(Consulta consulta)
    {
        var idx = _items.FindIndex(c => c.IdConsulta == consulta.IdConsulta);
        if (idx >= 0) _items[idx] = consulta;
    }

    public void Delete(string id) => _items.RemoveAll(c => c.IdConsulta == id);
}

public class InMemoryAgendamentoRepository : IAgendamentoRepository
{
    private readonly List<Agendamento> _items = new();

    public InMemoryAgendamentoRepository(params Agendamento[] items) => _items.AddRange(items);

    public IEnumerable<Agendamento> GetAll() => _items.ToList();
    public Agendamento? GetById(string id) => _items.FirstOrDefault(a => a.IdAgendamento == id);
    public Agendamento? GetByConsulta(string idConsulta) => _items.FirstOrDefault(a => a.IdConsulta == idConsulta);

    public Agendamento Add(Agendamento agendamento)
    {
        _items.Add(agendamento);
        return agendamento;
    }

    public void Update(Agendamento agendamento)
    {
        var idx = _items.FindIndex(a => a.IdAgendamento == agendamento.IdAgendamento);
        if (idx >= 0) _items[idx] = agendamento;
    }

    public void Delete(string id) => _items.RemoveAll(a => a.IdAgendamento == id);
}

public class InMemoryFeedbackRepository : IFeedbackRepository
{
    private readonly List<Feedback> _items = new();

    public InMemoryFeedbackRepository(params Feedback[] items) => _items.AddRange(items);

    public IEnumerable<Feedback> GetAll() => _items.ToList();
    public Feedback? GetById(string id) => _items.FirstOrDefault(f => f.IdFeedback == id);
    public Feedback? GetByConsulta(string idConsulta) => _items.FirstOrDefault(f => f.IdConsulta == idConsulta);

    public Feedback Add(Feedback feedback)
    {
        _items.Add(feedback);
        return feedback;
    }

    public void Update(Feedback feedback)
    {
        var idx = _items.FindIndex(f => f.IdFeedback == feedback.IdFeedback);
        if (idx >= 0) _items[idx] = feedback;
    }

    public void Delete(string id) => _items.RemoveAll(f => f.IdFeedback == id);
}

public class InMemoryDisponibilidadeMedicoRepository : IDisponibilidadeMedicoRepository
{
    private readonly List<DisponibilidadeMedico> _items = new();

    public InMemoryDisponibilidadeMedicoRepository(params DisponibilidadeMedico[] items) => _items.AddRange(items);

    public IEnumerable<DisponibilidadeMedico> GetByMedico(string idMedico) => _items.Where(d => d.IdMedico == idMedico).ToList();
    public DisponibilidadeMedico? GetById(string id) => _items.FirstOrDefault(d => d.IdDisponibilidade == id);

    public DisponibilidadeMedico Add(DisponibilidadeMedico disponibilidade)
    {
        _items.Add(disponibilidade);
        return disponibilidade;
    }

    public void Update(DisponibilidadeMedico disponibilidade)
    {
        var idx = _items.FindIndex(d => d.IdDisponibilidade == disponibilidade.IdDisponibilidade);
        if (idx >= 0) _items[idx] = disponibilidade;
    }

    public void Delete(string id) => _items.RemoveAll(d => d.IdDisponibilidade == id);
}
