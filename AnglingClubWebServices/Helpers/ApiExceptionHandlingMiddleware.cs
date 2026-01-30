namespace AnglingClubWebServices.Helpers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Threading.Tasks;

    public sealed class ApiExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ApiExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ApiExceptionHandlingMiddleware(
            ILogger<ApiExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;


                _logger.LogError(ex,
                    "Unhandled exception. TraceId={TraceId} Method={Method} Path={Path} User={User}",
                    traceId,
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.User?.Identity?.Name ?? "anonymous");

                var (status, title, detail, type) = MapException(ex, _env);

                var problem = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = detail,
                    Type = type,
                    Instance = context.Request.Path
                };

                problem.Extensions["traceId"] = traceId;

                context.Response.StatusCode = status;
                context.Response.ContentType = "application/problem+json";

                // Ensure consistent JSON casing; match your API conventions.
                var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
            }
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        private static (int status, string title, string? detail, string type) MapException(Exception ex, IHostEnvironment env)
        {
            // Expected, “clean” exceptions (we’ll add these in Step 2)
            return ex switch
            {
                NotFoundException nf => (StatusCodes.Status404NotFound, nf.Message, null, "https://httpstatuses.com/404"),
                ConflictException cf => (StatusCodes.Status409Conflict, cf.Message, null, "https://httpstatuses.com/409"),
                ValidationException ve => (StatusCodes.Status400BadRequest, ve.Message, ve.Details, "https://httpstatuses.com/400"),
                ForbiddenException fe => (StatusCodes.Status403Forbidden, fe.Message, null, "https://httpstatuses.com/403"),

                // Everything else => 500, hide detail in prod
                _ => (StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.",
                    env.IsDevelopment() ? ex.ToString() : null,
                    "https://httpstatuses.com/500"),
            };
        }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    }

}
