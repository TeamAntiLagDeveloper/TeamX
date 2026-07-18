using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var response = new
            {
                success = false,
                message = "Ocorreu um erro interno.",
                error = ex.Message // Remova em produção
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}