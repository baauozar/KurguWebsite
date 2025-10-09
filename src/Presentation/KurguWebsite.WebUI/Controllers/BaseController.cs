using KurguWebsite.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Controllers
{
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Handles Result<T> and returns appropriate IActionResult based on ErrorCode
        /// </summary>
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.Succeeded)
            {
                return Ok(result);
            }

            // Map ErrorCode to HTTP status codes
            return result.ErrorCode switch
            {
                ErrorCodes.EntityNotFound => NotFound(new
                {
                    succeeded = false,
                    errors = result.Errors,
                    errorCode = result.ErrorCode,
                    message = result.Message
                }),

                ErrorCodes.ValidationError => BadRequest(new
                {
                    succeeded = false,
                    errors = result.Errors,
                    errorCode = result.ErrorCode,
                    message = result.Message,
                    metadata = result.Metadata
                }),

                ErrorCodes.DuplicateEntity => Conflict(new
                {
                    succeeded = false,
                    errors = result.Errors,
                    errorCode = result.ErrorCode,
                    message = result.Message
                }),

                ErrorCodes.Unauthorized => Unauthorized(new
                {
                    succeeded = false,
                    errors = result.Errors,
                    errorCode = result.ErrorCode,
                    message = result.Message
                }),

                ErrorCodes.Forbidden => StatusCode(403, new
                {
                    succeeded = false,
                    errors = result.Errors,
                    errorCode = result.ErrorCode,
                    message = result.Message
                }),

                ErrorCodes.ConcurrencyConflict => Conflict(new
                {
                    succeeded = false,
                    errors = result.Errors,
                    errorCode = result.ErrorCode,
                    message = result.Message
                }),

                _ => StatusCode(500, new
                {
                    succeeded = false,
                    errors = result.Errors,
                    errorCode = result.ErrorCode ?? "INTERNAL_ERROR",
                    message = result.Message ?? "An error occurred"
                })
            };
        }

        /// <summary>
        /// Handles non-generic Result (for void operations)
        /// Note: You'll need to create a non-generic Result class
        /// </summary>
        protected IActionResult HandleResult(Result<object> result)
        {
            return HandleResult<object>(result);
        }

        /// <summary>
        /// Gets the first error message from Result
        /// </summary>
        protected string? GetFirstError<T>(Result<T> result)
        {
            return result.Errors?.FirstOrDefault();
        }

        /// <summary>
        /// Gets all error messages as a single string
        /// </summary>
        protected string GetAllErrors<T>(Result<T> result)
        {
            return string.Join(", ", result.Errors ?? Array.Empty<string>());
        }

        // TempData message helpers
        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void SetWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }

        protected void SetInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }

        /// <summary>
        /// Sets error message from Result errors
        /// </summary>
        protected void SetErrorFromResult<T>(Result<T> result)
        {
            if (!result.Succeeded && result.Errors.Any())
            {
                SetErrorMessage(string.Join(", ", result.Errors));
            }
        }
    }
}