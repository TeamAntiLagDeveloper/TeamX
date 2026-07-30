using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TeamX.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                exception,
                "Response já iniciada. Exception não mapeada. Path: {Path}",
                context.Request.Path);
            return;
        }

        // Cliente cancelou / request abortada — não tratar como erro de servidor
        if (exception is OperationCanceledException &&
            context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug(
                "Request cancelada pelo cliente. Path: {Path}",
                context.Request.Path);
            return;
        }

        var (statusCode, title, detail) = MapException(exception);

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Erro não tratado | Path: {Path} | Method: {Method} | Status: {StatusCode}",
                context.Request.Path,
                context.Request.Method,
                statusCode);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Falha tratada | Path: {Path} | Method: {Method} | Status: {StatusCode}",
                context.Request.Path,
                context.Request.Method,
                statusCode);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = _env.IsDevelopment() ? detail : GetPublicDetail(statusCode, detail),
            Instance = context.Request.Path
        };

        if (_env.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static string? GetPublicDetail(int statusCode, string detail)
    {
        // Em produção: mensagens de 4xx podem ir ao cliente; 5xx ficam genéricas
        return statusCode is >= 400 and < 500 ? detail : null;
    }

    private static (int statusCode, string title, string detail) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentException or ArgumentNullException =>
                ((int)HttpStatusCode.BadRequest, "Requisição inválida", exception.Message),

            UnauthorizedAccessException =>
                ((int)HttpStatusCode.Unauthorized, "Não autorizado", exception.Message),

            KeyNotFoundException or FileNotFoundException =>
                ((int)HttpStatusCode.NotFound, "Recurso não encontrado", exception.Message),

            // Regras de negócio (plano inexistente, limite, etc.)
            InvalidOperationException =>
                ((int)HttpStatusCode.BadRequest, "Operação inválida", exception.Message),

            NotImplementedException =>
                ((int)HttpStatusCode.NotImplemented, "Funcionalidade não implementada", exception.Message),

            OperationCanceledException =>
                ((int)HttpStatusCode.RequestTimeout, "Requisição cancelada", "A operação foi cancelada."),

            _ =>
                ((int)HttpStatusCode.InternalServerError, "Erro interno do servidor", "Ocorreu um erro inesperado.")
        };
    }
}