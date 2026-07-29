using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Resend;
using Serilog;
using TeamX.API.Middleware;
using TeamX.API.Services;
using TeamX.Core.Interfaces;
using TeamX.Data.Context;
using TeamX.Data.Repositories;
using TeamX.Security.Licensing;

namespace TeamX.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        // ─── Serilog cedo (captura erros de startup) ───────────────
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Remove as fontes padrão que ligam FileSystemWatcher (reloadOnChange = true)
            builder.Configuration.Sources.Clear();

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

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

            // Health checks
            builder.Services.AddHealthChecks()
                .AddNpgSql(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string não configurada"),
                    name: "postgres");

            // ─── Forwarded headers (proxy / Docker / reverse proxy) ─
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // Em produção com proxy conhecido, restrinja KnownNetworks / KnownProxies
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // ─── E-mail (Resend) ────────────────────────────────────
            builder.Services.AddOptions();
            builder.Services.AddHttpClient<ResendClient>();
            builder.Services.Configure<ResendClientOptions>(
                builder.Configuration.GetSection("Resend"));
            builder.Services.AddTransient<IResend, ResendClient>();

            // ─── Database ──────────────────────────────────────────
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var cs = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string não configurada");

                options.UseNpgsql(cs, npgsql =>
                {
                    npgsql.CommandTimeout(30);
                    npgsql.EnableRetryOnFailure(3);
                });

                if (builder.Environment.IsDevelopment())
                    options.EnableSensitiveDataLogging(false);
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

            // ─── Background jobs ───────────────────────────────────
            builder.Services.AddHostedService<AbuseScanBackgroundService>();
            // Opcional: builder.Services.AddHostedService<NonceCleanupBackgroundService>();

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

            // URL: preferir ASPNETCORE_URLS / launchSettings em vez de hardcoded
            // builder.WebHost.UseUrls("http://0.0.0.0:8080");

            var app = builder.Build();

            // ─── Startup: migrate + seed ───────────────────────────
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
            // ForwardedHeaders o mais cedo possível
            app.UseForwardedHeaders();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                // Em produção atrás de proxy, HTTPS redirection costuma ser do proxy
                // app.UseHttpsRedirection();
            }

            app.UseSerilogRequestLogging();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseRateLimiter();

            // Se no futuro tiver auth JWT no pipeline:
            // app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            Log.Information("TeamX.API iniciando...");
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Aplicação encerrada inesperadamente");
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}