namespace oop_s2_2_mvc_78286.Middleware;
using Serilog.Context;

public class UserLoggingMiddleware
{
    private readonly RequestDelegate _next;
    public UserLoggingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var userName = context.User.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name
            : "Anonymous";

        using (LogContext.PushProperty("UserName", userName)) // Enriches every log in this request
        {
            await _next(context);
        }
    }
}