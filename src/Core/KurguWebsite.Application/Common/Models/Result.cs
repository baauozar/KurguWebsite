using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    public class Result<T>
    {
        public bool Succeeded { get; }
        public T? Data { get; }
        public string[] Errors { get; }
        public string? Message { get; }

        private Result(bool succeeded, T? data, IEnumerable<string>? errors = null, string? message = null)
        {
            Succeeded = succeeded;
            Data = data;
            Errors = errors?.ToArray() ?? Array.Empty<string>();
            Message = message;
        }

        public static Result<T> Success(T? data = default, string? message = null)
        {
            return new Result<T>(true, data, null, message);
        }

        public static Result<T> Failure(IEnumerable<string> errors)
        {
            return new Result<T>(false, default, errors);
        }

        public static Result<T> Failure(string error)
        {
            return new Result<T>(false, default, new[] { error });
        }

        public static Result<T> Failure(string error, string? message)
        {
            return new Result<T>(false, default, new[] { error }, message);
        }
    }
}
