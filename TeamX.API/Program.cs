using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.NpgSql;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Resend;
using Serilog;
using System.Threading.RateLimiting;
using TeamX.API.Middleware;
using TeamX.API.Services;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;
using TeamX.Data.Repositories;
using TeamX.Data.Seeders;
using TeamX.Security.Licensing;

namespace TeamX.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Evita FileSystemWatcher / inotify no Linux (Render, etc.)
        Environment.SetEnvironmentVariable("DOTNET_HOSTBUILDER_RELOADCONFIGONCHANGE", "false");

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.Sources.Clear();
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile(
                $"appsettings.{builder.Environment.EnvironmentName}.json",
                optional: true,
                reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets(typeof(Program).Assembly, optional: true);

        Serilog.Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            builder.Host.UseSerilog((ctx, services, lc) => lc
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "TeamX.API")
                .WriteTo.Console()
                .WriteTo.File(
                    "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30));

            ValidateRequiredConfiguration(builder.Configuration, builder.Environment);

            // ─── Options ───────────────────────────────────────────
            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection(JwtOptions.SectionName));
            builder.Services.Configure<EmailOptions>(
                builder.Configuration.GetSection(EmailOptions.SectionName));
            builder.Services.Configure<AbuseDetectionOptions>(
                builder.Configuration.GetSection(AbuseDetectionOptions.SectionName));
            builder.Services.Configure<AbuseScanOptions>(
                builder.Configuration.GetSection(AbuseScanOptions.SectionName));

            // ─── Controllers / API ─────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            if (builder.Environment.IsDevelopment())
                builder.Services.AddSwaggerGen();

            builder.Services
                .AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddMemoryCache();
            builder.Services.AddProblemDetails();

            // ─── Health checks ─────────────────────────────────────
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var healthChecks = builder.Services.AddHealthChecks();

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                healthChecks.AddNpgSql(connectionString, name: "postgres");
            }
            else
            {
                Serilog.Log.Warning("ConnectionStrings:DefaultConnection não configurada. Health check Postgres desabilitado.");
            }

            // ─── Forwarded headers ─────────────────────────────────
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // ─── E-mail (Resend) ────────────────────────────────────
            builder.Services.AddHttpClient<ResendClient>();
            builder.Services.Configure<ResendClientOptions>(o =>
            {
                o.ApiToken = builder.Configuration["Resend:ApiToken"] ?? string.Empty;
            });
            builder.Services.AddTransient<IResend, ResendClient>();

            // ─── Database ──────────────────────────────────────────
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var cs = builder.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(cs))
                {
                    throw new InvalidOperationException(
                        "ConnectionStrings:DefaultConnection não configurada. " +
                        "Use user-secrets (local) ou variáveis de ambiente (produção).");
                }

                options.UseNpgsql(cs, npgsql =>
                {
                    npgsql.CommandTimeout(30);
                    npgsql.EnableRetryOnFailure(3);
                });
            });

            // ─── Application services ──────────────────────────────
            builder.Services.AddScoped<INonceService, NonceService>();
            builder.Services.AddScoped<ISignatureService, SignatureService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<AbuseDetectionService>();
            builder.Services.AddScoped<ILicenseRepository, LicenseRepository>();
            builder.Services.AddScoped<ILicenseService, LicenseService>();
            builder.Services.AddScoped<ILicenseActivationService, LicenseActivationService>();
            builder.Services.AddScoped<IHeartbeatService, HeartbeatService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<ILicenseKeyGenerator, LicenseKeyGenerator>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddHostedService<AbuseScanBackgroundService>();

            // ─── Rate limiting ─────────────────────────────────────
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, ct) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Muitas requisições. Tente novamente em instantes."
                    }, ct);
                };

                options.AddFixedWindowLimiter("activate", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });

                options.AddFixedWindowLimiter("validate", opt =>
                {
                    opt.PermitLimit = 30;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("webhook", opt =>
                {
                    opt.PermitLimit = 20;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("heartbeat", opt =>
                {
                    opt.PermitLimit = 60;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });
            });

            var app = builder.Build();

            // ─── Migrate + seed ────────────────────────────────────
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    await db.Database.MigrateAsync();
                    await DatabaseSeeder.SeedAsync(db);
                    logger.LogInformation("Database migrada e seed concluído.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Falha na migração/seed do banco.");
                    throw;
                }
            }

            // ─── Pipeline ──────────────────────────────────────────
            app.UseForwardedHeaders();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseRateLimiter();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            Serilog.Log.Information("TeamX.API iniciando...");
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Serilog.Log.Fatal(ex, "Aplicação encerrada inesperadamente");
            throw;
        }
        finally
        {
            await Serilog.Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Garante que segredos mínimos existem antes de subir a API.
    /// </summary>
    private static void ValidateRequiredConfiguration(
        IConfiguration config,
        IHostEnvironment env)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.GetConnectionString("DefaultConnection")))
            errors.Add("ConnectionStrings:DefaultConnection");

        var jwtSecret = config["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
            errors.Add("Jwt:Secret (mínimo 32 caracteres)");

        var signingSecret = config["Activation:SigningSecret"];
        if (string.IsNullOrWhiteSpace(signingSecret) || signingSecret.Length < 32)
            errors.Add("Activation:SigningSecret (mínimo 32 caracteres)");

        var webhookSecret = config["Eremby:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
            errors.Add("Eremby:WebhookSecret");

        // Em Development ainda exigimos — senão activate/webhook quebram em silêncio
        if (errors.Count > 0)
        {
            var msg =
                "Configuração incompleta. Defina via user-secrets ou variáveis de ambiente: " +
                string.Join(", ", errors);

            Serilog.Log.Fatal(msg);
            throw new InvalidOperationException(msg);
        }

        // Avisos (não bloqueiam)
        if (string.IsNullOrWhiteSpace(config["Resend:ApiToken"]))
            Serilog.Log.Warning("Resend:ApiToken não configurado — e-mails de licença vão falhar.");

        if (string.IsNullOrWhiteSpace(config["Admin:ApiKey"]) ||
            (config["Admin:ApiKey"]?.Length ?? 0) < 16)
        {
            Serilog.Log.Warning("Admin:ApiKey fraca ou ausente — POST /api/license/create ficará bloqueado.");
        }

        if (env.IsProduction() && string.IsNullOrWhiteSpace(config["Admin:ApiKey"]))
            Serilog.Log.Warning("Produção sem Admin:ApiKey — endpoint create admin indisponível (esperado se só usar webhook).");
    }
}