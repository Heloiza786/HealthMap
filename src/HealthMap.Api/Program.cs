using HealthMap.Domain.Interfaces;
using HealthMap.Domain.Services;
using HealthMap.Infrastructure.Persistence;
using HealthMap.Infrastructure.Repositories;
using HealthMap.Api.Endpoints;
using HealthMap.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// ── Persistência: banco em arquivo JSON (System.Text.Json) ──
var dataDir = Path.Combine(builder.Environment.ContentRootPath, "data");
var dbPath = Path.Combine(dataDir, "database.json");
builder.Services.AddSingleton(new JsonDatabase(dbPath));

// ── Repositories ──
builder.Services.AddSingleton<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddSingleton<IPacienteRepository, PacienteRepository>();
builder.Services.AddSingleton<IMedicoRepository, MedicoRepository>();
builder.Services.AddSingleton<ISecretariaRepository, SecretariaRepository>();
builder.Services.AddSingleton<IEspecialidadeRepository, EspecialidadeRepository>();
builder.Services.AddSingleton<IConsultaRepository, ConsultaRepository>();
builder.Services.AddSingleton<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddSingleton<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddSingleton<IConfirmacaoMensagemRepository, ConfirmacaoMensagemRepository>();
builder.Services.AddSingleton<IHistoricoMedicoRepository, HistoricoMedicoRepository>();
builder.Services.AddSingleton<IDisponibilidadeMedicoRepository, DisponibilidadeMedicoRepository>();

// ── Serviços de domínio ──
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ConsultaService>();
builder.Services.AddScoped<FeedbackService>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));

app.MapAuthEndpoints();
app.MapUsuarioEndpoints();
app.MapEspecialidadeEndpoints();
app.MapMedicoEndpoints();
app.MapPacienteEndpoints();
app.MapConsultaEndpoints();
app.MapFeedbackEndpoints();

app.Run();
