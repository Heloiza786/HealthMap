using HealthMap.Domain.Interfaces;
using HealthMap.Domain.Services;
using HealthMap.Infrastructure.Persistence;
using HealthMap.Infrastructure.Repositories;
using HealthMap.Api.Endpoints;
using HealthMap.Api.Middleware;
using HealthMap.Api.Data;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// ── Persistência: banco SQL (SQLite via Entity Framework Core) ──
// A connection string pode ser sobrescrita via configuração
// (ConnectionStrings__HealthMap) para usar outro banco/testes.
var connectionString = builder.Configuration.GetConnectionString("HealthMap")
    ?? "Data Source=healthmap.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// ── Repositories ──
builder.Services.AddScoped<IUsuarioRepository, EfUsuarioRepository>();
builder.Services.AddScoped<IPacienteRepository, EfPacienteRepository>();
builder.Services.AddScoped<IMedicoRepository, EfMedicoRepository>();
builder.Services.AddScoped<ISecretariaRepository, EfSecretariaRepository>();
builder.Services.AddScoped<IEspecialidadeRepository, EfEspecialidadeRepository>();
builder.Services.AddScoped<IConsultaRepository, EfConsultaRepository>();
builder.Services.AddScoped<IAgendamentoRepository, EfAgendamentoRepository>();
builder.Services.AddScoped<IFeedbackRepository, EfFeedbackRepository>();
builder.Services.AddScoped<IConfirmacaoMensagemRepository, EfConfirmacaoMensagemRepository>();
builder.Services.AddScoped<IHistoricoMedicoRepository, EfHistoricoMedicoRepository>();
builder.Services.AddScoped<IDisponibilidadeMedicoRepository, EfDisponibilidadeMedicoRepository>();

// ── Serviços de domínio ──
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ConsultaService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddSingleton<ValidationEndpointFilter>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));

app.MapAuthEndpoints();
app.MapUsuarioEndpoints();
app.MapEspecialidadeEndpoints();
app.MapMedicoEndpoints();
app.MapPacienteEndpoints();
app.MapSecretariaEndpoints();
app.MapConsultaEndpoints();
app.MapFeedbackEndpoints();

// ── Inicialização do banco ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DatabaseSeeder.Initialize(db);
}

app.Run();

/// <summary>
/// Declaração parcial para permitir que os testes de integração usem
/// <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/>.
/// </summary>
public partial class Program { }
