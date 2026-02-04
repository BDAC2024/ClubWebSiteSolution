using AnglingClubWebsite.Models;
using System.Net;
using System.Text.Json;

namespace AnglingClubWebsite.Helpers
{
    public sealed class ProblemDetailsHttpHandler : DelegatingHandler
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // Optional: keep last traceId for UI error reporting
        private readonly IClientTraceContext _traceContext;

        public ProblemDetailsHttpHandler(IClientTraceContext traceContext)
            => _traceContext = traceContext;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                // Try parse ApiProblemDetails; if not possible, still throw a typed exception.
                var problem = await TryReadApiProblemDetailsAsync(response, cancellationToken);

                var traceId = TryGetTraceId(problem) ?? TryGetTraceIdFromHeaders(response);
                if (!string.IsNullOrWhiteSpace(traceId))
                {
                    _traceContext.LastTraceId = traceId;
                }

                var message = BuildUserMessage(problem, response.StatusCode);

                throw CreateException(response.StatusCode, message, problem, traceId);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // HttpClient timeout
                throw new ApiNetworkException("The request timed out. Please try again.", ex);
            }
            catch (HttpRequestException ex)
            {
                // DNS/offline/TLS/etc.
                throw new ApiNetworkException("Network error contacting the server. Please check your connection.", ex);
            }
        }

        private static async Task<ApiProblemDetails?> TryReadApiProblemDetailsAsync(HttpResponseMessage response, CancellationToken ct)
        {
            // If server returns application/problem+json (or even JSON), attempt parse
            if (response.Content is null)
            {
                return null;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;

            // Read body (bounded defensively)
            var body = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            // Only parse if it looks like JSON (avoid exceptions on HTML error pages)
            if (!LooksLikeJson(body))
            {
                return null;
            }

            try
            {
                // Try ApiProblemDetails first
                var pd = JsonSerializer.Deserialize<ApiProblemDetails>(body, _jsonOptions);
                return pd;
            }
            catch
            {
                return null;
            }
        }

        private static bool LooksLikeJson(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                return c == '{' || c == '[';
            }
            return false;
        }

        private static string? TryGetTraceId(ApiProblemDetails? pd)
        {
            var traceId = pd?.TraceId
              ?? (pd?.ExtensionData is not null &&
                  pd.ExtensionData.TryGetValue("traceId", out var v) ? v.GetString() : null);

            return traceId;
        }

        private static string? TryGetTraceIdFromHeaders(HttpResponseMessage response)
        {
            // Optional: if you ever emit trace id in a header, read it here.
            // For now, return null.
            return null;
        }

        private static string BuildUserMessage(ApiProblemDetails? problem, HttpStatusCode status)
        {
            // Prefer server-provided safe title/detail for 4xx; for 5xx keep generic.
            var s = (int)status;

            if (s >= 500)
            {
                return "Something went wrong on the server. Please try again.";
            }

            // 4xx: safe to show clearer info (as long as you keep server responses clean)
            if (!string.IsNullOrWhiteSpace(problem?.Title))
            {
                return problem!.Title!;
            }

            return status switch
            {
                HttpStatusCode.NotFound => "The requested item was not found.",
                HttpStatusCode.Conflict => "That change could not be completed due to a conflict. Please refresh and try again.",
                HttpStatusCode.Unauthorized => "You are not signed in.",
                HttpStatusCode.Forbidden => "You do not have permission to do that.",
                _ => "The request could not be completed."
            };
        }

        private static ApiException CreateException(HttpStatusCode status, string message, ApiProblemDetails? problem, string? traceId)
        {
            var code = (int)status;

            return status switch
            {
                HttpStatusCode.BadRequest => new ApiValidationException(message, code, problem, traceId),
                HttpStatusCode.NotFound => new ApiNotFoundException(message, code, problem, traceId),
                HttpStatusCode.Unauthorized => new ApiUnauthorizedException(message, code, problem, traceId),
                HttpStatusCode.Forbidden => new ApiForbiddenException(message, code, problem, traceId),
                HttpStatusCode.Conflict => new ApiConflictException(message, code, problem, traceId),
                _ when code >= 500 => new ApiServerException(message, code, problem, traceId),
                _ => new ApiServerException(message, code, problem, traceId) // default bucket
            };
        }
    }

    public interface IClientTraceContext
    {
        string? LastTraceId {
            get; set;
        }
    }

    public sealed class ClientTraceContext : IClientTraceContext
    {
        public string? LastTraceId {
            get; set;
        }
    }
}
