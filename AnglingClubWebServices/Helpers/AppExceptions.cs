using System;

namespace AnglingClubWebServices.Helpers
{
    public abstract class AppException : Exception
    {
        protected AppException(string message) : base(message) { }
    }

    public sealed class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message) { }
    }

    public sealed class ConflictException : AppException
    {
        public ConflictException(string message) : base(message) { }
    }

    public sealed class ForbiddenException : AppException
    {
        public ForbiddenException(string message) : base(message) { }
    }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public sealed class ValidationException : AppException
    {
        public string? Details
        {
            get;
        }
        public ValidationException(string message, string? details = null) : base(message)
            => Details = details;
    }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

}
