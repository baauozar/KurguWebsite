using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using KurguWebsite.Application.Common.Exceptions;

namespace KurguWebsite.API.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionFilter> _logger;
        private readonly IWebHostEnvironment _environment;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An exception occurred: {Message}", context.Exception.Message);

            if (context.Exception is ValidationException validationException)
            {
                var problemDetails = new ValidationProblemDetails
                {
                    Title = "Validation Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = validationException.Message,
                    Instance = context.HttpContext.Request.Path
                };

                foreach (var error in validationException.Errors)
                {
                    problemDetails.Errors[error.Key] = error.Value;
                }

                context.Result = new BadRequestObjectResult(problemDetails);
                context.ExceptionHandled = true;
                return;
            }

            if (!_environment.IsDevelopment())
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "An error occurred",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An error occurred while processing your request",
                    Instance = context.HttpContext.Request.Path
                };

                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };

                context.ExceptionHandled = true;
            }
        }
    }
}
