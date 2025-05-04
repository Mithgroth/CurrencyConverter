using System.ComponentModel.DataAnnotations;

namespace Api;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var input = context.Arguments.OfType<T>().FirstOrDefault();
        if (input is null && !HttpMethods.IsGet(context.HttpContext.Request.Method)) // GET requests can have no parameters (like /api/v1/rates)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { typeof(T).Name, new[] { "Input was null." } }
            });
        }

        var validationContext = new ValidationContext(input);
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(input, validationContext, validationResults, true);
        if (!isValid)
        {
            var errors = validationResults
                .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => r.ErrorMessage ?? "Invalid").ToArray()
                );

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}

// This is just nicer
public static class ValidationExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter<ValidationFilter<T>>();
    }
}
