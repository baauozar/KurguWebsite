using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    // src/Core/KurguWebsite.Application/Common/Models/Result.cs
    public class Result<T>
    {
        public bool Succeeded { get; }
        public T? Data { get; }
        public string[] Errors { get; }
        public string? Message { get; }
        public string? ErrorCode { get; } // ADD THIS
        public Dictionary<string, object>? Metadata { get; } // ADD THIS

        private Result(
            bool succeeded,
            T? data,
            IEnumerable<string>? errors = null,
            string? message = null,
            string? errorCode = null,
            Dictionary<string, object>? metadata = null)
        {
            Succeeded = succeeded;
            Data = data;
            Errors = errors?.ToArray() ?? Array.Empty<string>();
            Message = message;
            ErrorCode = errorCode;
            Metadata = metadata;
        }

        public static Result<T> Success(T? data = default, string? message = null, Dictionary<string, object>? metadata = null)
        {
            return new Result<T>(true, data, null, message, null, metadata);
        }

        public static Result<T> Failure(string error, string? errorCode = null)
        {
            return new Result<T>(false, default, new[] { error }, null, errorCode);
        }

        public static Result<T> Failure(IEnumerable<string> errors, string? errorCode = null)
        {
            return new Result<T>(false, default, errors, null, errorCode);
        }

        public static Result<T> NotFound(string entityName, object id)
        {
            return new Result<T>(
                false,
                default,
                new[] { $"{entityName} with id {id} was not found" },
                null,
                "ENTITY_NOT_FOUND");
        }

        public static Result<T> ValidationError(Dictionary<string, string[]> validationErrors)
        {
            var errors = validationErrors.SelectMany(kvp =>
                kvp.Value.Select(v => $"{kvp.Key}: {v}"));

            return new Result<T>(
                false,
                default,
                errors,
                "Validation failed",
                "VALIDATION_ERROR",
                new Dictionary<string, object> { ["ValidationErrors"] = validationErrors });
        }
    }

    // Error codes enum
    public static class ErrorCodes
    {
        public const string EntityNotFound = "ENTITY_NOT_FOUND";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string DuplicateEntity = "DUPLICATE_ENTITY";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    }
}
