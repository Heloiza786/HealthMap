using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HealthMap.Api.Middleware;

/// <summary>
/// Valida automaticamente os argumentos de entrada dos endpoints que possuem
/// atributos de validação (<see cref="ValidationAttribute"/>), retornando
/// 400 Bad Request com a primeira mensagem de erro encontrada.
/// </summary>
public sealed class ValidationEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument is null) continue;
            if (!PossuiAtributosDeValidacao(argument.GetType())) continue;

            var results = new List<ValidationResult>();
            var validationContext = new ValidationContext(argument);
            if (!Validator.TryValidateObject(argument, validationContext, results, validateAllProperties: true))
            {
                var message = results[0].ErrorMessage ?? "Requisição inválida.";
                return Results.BadRequest(new { error = message });
            }
        }

        return await next(context);
    }

    private static bool PossuiAtributosDeValidacao(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(p => p.GetCustomAttributes(true).OfType<ValidationAttribute>().Any());
}
