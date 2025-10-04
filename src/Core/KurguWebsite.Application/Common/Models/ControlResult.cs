using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    public class ControlResult
    {
        public bool Succeeded { get; }
        public string[] Errors { get; }
        public string? Message { get; set; }

        private ControlResult(bool succeeded, IEnumerable<string>? errors = null)
        {
            Succeeded = succeeded;
            Errors = errors?.ToArray() ?? Array.Empty<string>();
        }

        public static ControlResult Success()
        {
            return new ControlResult(true);
        }

        public static ControlResult Failure(IEnumerable<string> errors)
        {
            return new ControlResult(false, errors);
        }

        public static ControlResult Failure(string error)
        {
            return new ControlResult(false, new[] { error });
        }
    }
}