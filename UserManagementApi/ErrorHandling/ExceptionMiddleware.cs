using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Authentication;
using System.Text.Json;

namespace UserManagementApi
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate  next;
        private readonly ILogger<ExceptionMiddleware>  logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex.Message}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                KeyNotFoundException => (int)HttpStatusCode.NotFound,  // 404
                UnauthorizedAccessException or AuthenticationException => (int)HttpStatusCode.Unauthorized, // 401
                ArgumentException or ValidationException => (int)HttpStatusCode.BadRequest,  // 400
                _ => (int)HttpStatusCode.InternalServerError  // 500
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                StatusCode = statusCode,
                Message = exception.Message,
                Errors = (exception is ValidationException valEx)
                    ? new List<string> { valEx.Message }
                    : (exception is AggregateException aggEx)
                        ? aggEx.InnerExceptions.Select(e => e.Message).ToList()
                        : null
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
