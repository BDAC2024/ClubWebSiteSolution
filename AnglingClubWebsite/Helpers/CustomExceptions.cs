namespace AnglingClubWebsite.Helpers
{
    using Microsoft.AspNetCore.Mvc;

    public abstract class ApiException : Exception
    {
        public int StatusCode
        {
            get;
        }
        public ProblemDetails? Problem
        {
            get;
        }
        public string? TraceId
        {
            get;
        }

        protected ApiException(string message, int statusCode, ProblemDetails? problem, string? traceId, Exception? inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
            Problem = problem;
            TraceId = traceId;
        }
    }

    public sealed class ApiValidationException : ApiException
    {
        public ApiValidationException(string message, int statusCode, ProblemDetails? problem, string? traceId)
            : base(message, statusCode, problem, traceId) { }
    }

    public sealed class ApiNotFoundException : ApiException
    {
        public ApiNotFoundException(string message, int statusCode, ProblemDetails? problem, string? traceId)
            : base(message, statusCode, problem, traceId) { }
    }

    public sealed class ApiUnauthorizedException : ApiException
    {
        public ApiUnauthorizedException(string message, int statusCode, ProblemDetails? problem, string? traceId)
            : base(message, statusCode, problem, traceId) { }
    }

    public sealed class ApiForbiddenException : ApiException
    {
        public ApiForbiddenException(string message, int statusCode, ProblemDetails? problem, string? traceId)
            : base(message, statusCode, problem, traceId) { }
    }

    public sealed class ApiConflictException : ApiException
    {
        public ApiConflictException(string message, int statusCode, ProblemDetails? problem, string? traceId)
            : base(message, statusCode, problem, traceId) { }
    }

    public sealed class ApiServerException : ApiException
    {
        public ApiServerException(string message, int statusCode, ProblemDetails? problem, string? traceId)
            : base(message, statusCode, problem, traceId) { }
    }

    public sealed class ApiNetworkException : ApiException
    {
        public ApiNetworkException(string message, Exception inner)
            : base(message, statusCode: 0, problem: null, traceId: null, inner: inner) { }
    }
}
