using HealthMap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Persistence;

/// <summary>
/// Contexto do Entity Framework Core para persistência em banco SQL.
/// Modela o mesmo domínio que antes era gravado em JSON.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Secretaria> Secretarias => Set<Secretaria>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Especialidade> Especialidades => Set<Especialidade>();
    public DbSet<DisponibilidadeMedico> DisponibilidadesMedico => Set<DisponibilidadeMedico>();
    public DbSet<HistoricoMedico> HistoricosMedicos => Set<HistoricoMedico>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<ConfirmacaoMensagem> ConfirmacoesMensagem => Set<ConfirmacaoMensagem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Usuario ──
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.IdUsuario);
            e.Property(u => u.Nome).HasMaxLength(200).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.SenhaHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.Cpf).HasMaxLength(20);
            e.Property(u => u.Telefone).HasMaxLength(30);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Cpf).IsUnique();
        });

        // ── Perfis (FK compartilhando id_usuario) ──
        modelBuilder.Entity<Paciente>(e =>
        {
            e.HasKey(p => p.IdUsuario);
            e.Property(p => p.PlanoSaude).HasMaxLength(100);
            e.Property(p => p.Endereco).HasMaxLength(300);
            e.HasOne<Usuario>()
                .WithOne()
                .HasForeignKey<Paciente>(p => p.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Secretaria>(e =>
        {
            e.HasKey(s => s.IdUsuario);
            e.Property(s => s.TurnoTrabalho).HasMaxLength(50);
            e.HasOne<Usuario>()
                .WithOne()
                .HasForeignKey<Secretaria>(s => s.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Medico>(e =>
        {
            e.HasKey(m => m.IdUsuario);
            e.Property(m => m.Crm).HasMaxLength(30).IsRequired();
            e.Property(m => m.IdEspecialidade).HasMaxLength(50).IsRequired();
            e.HasIndex(m => m.Crm).IsUnique();
            e.HasOne<Usuario>()
                .WithOne()
                .HasForeignKey<Medico>(m => m.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Especialidade ──
        modelBuilder.Entity<Especialidade>(e =>
        {
            e.HasKey(x => x.IdEspecialidade);
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Nome).IsUnique();
        });

        modelBuilder.Entity<Medico>()
            .HasOne<Especialidade>()
            .WithMany()
            .HasForeignKey(m => m.IdEspecialidade)
            .OnDelete(DeleteBehavior.Restrict);

        // ── DisponibilidadeMedico ──
        modelBuilder.Entity<DisponibilidadeMedico>(e =>
        {
            e.HasKey(d => d.IdDisponibilidade);
            e.Property(d => d.IdMedico).HasMaxLength(50).IsRequired();
            e.Property(d => d.HoraInicio).HasMaxLength(5);
            e.Property(d => d.HoraFim).HasMaxLength(5);
            e.HasOne<Medico>()
                .WithMany()
                .HasForeignKey(d => d.IdMedico)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── HistoricoMedico ──
        modelBuilder.Entity<HistoricoMedico>(e =>
        {
            e.HasKey(h => h.IdHistorico);
            e.Property(h => h.IdPaciente).HasMaxLength(50).IsRequired();
            e.Property(h => h.Descricao).HasMaxLength(2000);
            e.HasOne<Paciente>()
                .WithMany()
                .HasForeignKey(h => h.IdPaciente)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Consulta ──
        modelBuilder.Entity<Consulta>(e =>
        {
            e.HasKey(c => c.IdConsulta);
            e.Property(c => c.IdPaciente).HasMaxLength(50).IsRequired();
            e.Property(c => c.IdMedico).HasMaxLength(50).IsRequired();
            e.Property(c => c.Status).HasMaxLength(30).IsRequired();
            e.Property(c => c.QueixaPrincipal).HasMaxLength(2000);
            e.HasOne<Paciente>()
                .WithMany()
                .HasForeignKey(c => c.IdPaciente)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Medico>()
                .WithMany()
                .HasForeignKey(c => c.IdMedico)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Agendamento ──
        modelBuilder.Entity<Agendamento>(e =>
        {
            e.HasKey(a => a.IdAgendamento);
            e.Property(a => a.IdConsulta).HasMaxLength(50).IsRequired();
            e.Property(a => a.IdUsuarioSolicitante).HasMaxLength(50).IsRequired();
            e.HasOne<Consulta>()
                .WithOne()
                .HasForeignKey<Agendamento>(a => a.IdConsulta)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Feedback ──
        modelBuilder.Entity<Feedback>(e =>
        {
            e.HasKey(f => f.IdFeedback);
            e.Property(f => f.IdConsulta).HasMaxLength(50).IsRequired();
            e.Property(f => f.Comentario).HasMaxLength(2000);
            e.HasOne<Consulta>()
                .WithOne()
                .HasForeignKey<Feedback>(f => f.IdConsulta)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ConfirmacaoMensagem ──
        modelBuilder.Entity<ConfirmacaoMensagem>(e =>
        {
            e.HasKey(m => m.IdMensagem);
            e.Property(m => m.IdConsulta).HasMaxLength(50).IsRequired();
            e.Property(m => m.TipoMensagem).HasMaxLength(50);
            e.Property(m => m.TextoMensagem).HasMaxLength(2000);
            e.Property(m => m.StatusEnvio).HasMaxLength(30);
            e.Property(m => m.RespostaPaciente).HasMaxLength(500);
            e.HasOne<Consulta>()
                .WithMany()
                .HasForeignKey(m => m.IdConsulta)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
