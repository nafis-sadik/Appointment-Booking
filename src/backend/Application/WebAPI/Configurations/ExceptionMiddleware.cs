using System.Net;

namespace WebAPI.Configurations
{
    /// <summary>
    /// Global Exception Handeller Middleware
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Middleware Constructor
        /// </summary>
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Pass execution to next level
        /// </summary>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            if (exception.InnerException != null)
            {
                await context.Response.WriteAsync(exception.InnerException.Message);
            }
            await context.Response.WriteAsync(exception.Message);
        }
    }
}
