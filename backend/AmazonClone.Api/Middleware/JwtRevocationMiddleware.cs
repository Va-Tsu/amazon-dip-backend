using AmazonClone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Api.Middleware;

public class JwtRevocationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtRevocationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var revoked = await dbContext.RevokedTokens
                .AnyAsync(rt => rt.Token == token && rt.Expiration > DateTime.UtcNow);

            if (revoked)
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Token has been revoked. Please login again.");
                return;
            }
        }

        await _next(context);
    }
}