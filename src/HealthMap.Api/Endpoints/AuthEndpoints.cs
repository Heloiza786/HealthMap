using HealthMap.Domain.Interfaces;
using HealthMap.Domain.Services;
using HealthMap.Api.Dtos;

namespace HealthMap.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", (LoginRequest req, AuthService auth, IUsuarioRepository usuarios) =>
        {
            var usuario = auth.Autenticar(req.Email, req.Senha);
            if (usuario is null)
                return Results.Unauthorized();

            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{usuario.IdUsuario}:{DateTime.UtcNow.Ticks}"));

            return Results.Ok(new LoginResponse(
                token, usuario.IdUsuario, usuario.Nome, usuario.Email));
        });
    }
}
