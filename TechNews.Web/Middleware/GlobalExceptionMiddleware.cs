using System.Net;
using System.Text.Json;

namespace TechNews.Web.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (!context.Response.HasStarted && context.Response.StatusCode >= 400)
                {
                    if (!context.Request.Path.StartsWithSegments("/api"))
                    {
                        switch (context.Response.StatusCode)
                        {
                            case 404:
                                context.Request.Path = "/Home/Error404";
                                await _next(context);
                                break;
                            case 403:
                                context.Request.Path = "/Home/Error403";
                                await _next(context);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Không có quyền truy cập"),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Không tìm thấy tài nguyên"),
                _ => (HttpStatusCode.InternalServerError, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.")
            };

            context.Response.StatusCode = (int)statusCode;

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                var result = JsonSerializer.Serialize(new
                {
                    statusCode = (int)statusCode,
                    message
                });
                await context.Response.WriteAsync(result);
            }
            else
            {
                context.Response.Redirect("/Home/Error");
            }
        }
    }

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}