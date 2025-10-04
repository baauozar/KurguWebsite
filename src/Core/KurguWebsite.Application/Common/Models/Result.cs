using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    // src/Core/KurguWebsite.Application/Common/Models/Result.cs
    // Result.cs
    public partial class Result<T>
    {
        public bool Succeeded { get; }
        public T? Data { get; }
        public string[] Errors { get; }
        public string? Message { get; }
        public string? ErrorCode { get; }
        public Dictionary<string, object>? Metadata { get; }
        public bool IsPartialSuccess { get; private set; } // <- private set for immutability

        private Result(bool succeeded, T? data,
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

        public static Result<T> Success(T? data = default, string? message = null,
                                        Dictionary<string, object>? metadata = null)
            => new(true, data, null, message, null, metadata);

        public static Result<T> Failure(string error, string? errorCode = null)
            => new(false, default, new[] { error }, null, errorCode);

        public static Result<T> Failure(IEnumerable<string> errors, string? errorCode = null)
            => new(false, default, errors, null, errorCode);

        public static Result<T> NotFound(string entityName, object id)
            => new(false, default, new[] { $"{entityName} with id {id} was not found" }, null, "ENTITY_NOT_FOUND");

        public static Result<T> ValidationError(Dictionary<string, string[]> validationErrors)
            => new(false, default,
                   validationErrors.SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}")),
                   "Validation failed", "VALIDATION_ERROR",
                   new Dictionary<string, object> { ["ValidationErrors"] = validationErrors });

        // ✅ Partial success factory (in the same class)
        public static Result<T> PartialSuccess(T? data = default, string? message = null,
                                               Dictionary<string, object>? metadata = null,
                                               IEnumerable<string>? partialErrors = null)
        {
            var r = new Result<T>(true, data, partialErrors, message, null, metadata);
            r.IsPartialSuccess = true;
            return r;
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
    public static class ResultExtensions
    {
        public static bool IsPartial<T>(this Result<T> r) => r.Succeeded && r.IsPartialSuccess;
    }
}
