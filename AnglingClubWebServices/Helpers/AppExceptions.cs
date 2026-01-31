using System;

#nullable enable

namespace AnglingClubWebServices.Helpers
{
    public abstract class AppException : Exception
    {
        protected AppException(string message) : base(message) { }

        protected AppException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class AppNotFoundException : AppException
    {
        /// <summary>
        /// Used when an item does not exist. Add item name to message if it is user friendly
        /// </summary>
        public AppNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Used when an item does not exist. Add item name to message if it is user friendly
        /// </summary>
        public AppNotFoundException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class AppConflictException : AppException
    {
        public AppConflictException(string message)
            : base(message)
        {
        }

        public AppConflictException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class AppForbiddenException : AppException
    {
        public AppForbiddenException(string message)
            : base(message)
        {
        }

        public AppForbiddenException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class AppValidationException : AppException
    {
        public string? Details {
            get;
        }

        public AppValidationException(string message, string? details = null)
            : base(message)
        {
            Details = details;
        }

        public AppValidationException(string message, string? details, Exception? innerException)
            : base(message, innerException)
        {
            Details = details;
        }
    }

}
