using System.Net;
using System.Text.Json;
using KurguWebsite.Application.Common.Exceptions;
using KurguWebsite.Domain.Exceptions;

namespace KurguWebsite.WebAPI.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case ValidationException validationException:
                    response.Status = (int)HttpStatusCode.BadRequest;
                    response.Title = "Validation Error";
                    response.Errors = validationException.Errors;
                    break;

                case NotFoundException notFoundException:
                    response.Status = (int)HttpStatusCode.NotFound;
                    response.Title = "Resource Not Found";
                    response.Detail = notFoundException.Message;
                    break;

                case ForbiddenAccessException:
                    response.Status = (int)HttpStatusCode.Forbidden;
                    response.Title = "Access Forbidden";
                    response.Detail = "You do not have permission to access this resource";
                    break;

                case UnauthorizedAccessException:
                    response.Status = (int)HttpStatusCode.Unauthorized;
                    response.Title = "Unauthorized";
                    response.Detail = "Authentication is required to access this resource";
                    break;

                case BusinessRuleValidationException businessException:
                    response.Status = (int)HttpStatusCode.BadRequest;
                    response.Title = "Business Rule Violation";
                    response.Detail = businessException.Message;
                    break;

                case DomainException domainException:
                    response.Status = (int)HttpStatusCode.BadRequest;
                    response.Title = "Domain Error";
                    response.Detail = domainException.Message;
                    break;

                case TimeoutException:
                    response.Status = (int)HttpStatusCode.RequestTimeout;
                    response.Title = "Request Timeout";
                    response.Detail = "The request took too long to complete";
                    break;

                case NotImplementedException:
                    response.Status = (int)HttpStatusCode.NotImplemented;
                    response.Title = "Not Implemented";
                    response.Detail = "This feature is not yet implemented";
                    break;

                default:
                    response.Status = (int)HttpStatusCode.InternalServerError;
                    response.Title = "Internal Server Error";
                    response.Detail = _environment.IsDevelopment()
                        ? exception.Message
                        : "An error occurred while processing your request";

                    if (_environment.IsDevelopment())
                    {
                        response.StackTrace = exception.StackTrace;
                    }
                    break;
            }

            context.Response.StatusCode = response.Status;

            response.Instance = context.Request.Path;
            response.TraceId = context.TraceIdentifier;
            response.Timestamp = DateTime.UtcNow;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? Detail { get; set; }
        public string Instance { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? StackTrace { get; set; }
        public IDictionary<string, string[]>? Errors { get; set; }
    }
}
