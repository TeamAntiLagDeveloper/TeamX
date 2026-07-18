using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TeamX.API.Services;
using TeamX.Core.Interfaces;
using TeamX.Core.Services;
using TeamX.Data.Context;
using TeamX.Data.Repositories;
using TeamX.Security.Licensing;
using Microsoft.AspNetCore.HttpOverrides;
using Resend;

namespace TeamX.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpClient<ResendClient>();

        builder.Services.Configure<ResendClientOptions>(
            builder.Configuration.GetSection("Resend"));

        builder.Services.AddTransient<IResend, ResendClient>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
        builder.WebHost.UseUrls("http://0.0.0.0:8080");

        // === Logging ===
        builder.Host.UseSerilog((ctx, lc) => lc
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day));

        // === Services ===
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<INonceService, NonceService>();
        builder.Services.AddScoped<ISignatureService, SignatureService>();
        builder.Services.AddScoped<IEmailService, EmailService>();

        // Banco de Dados
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        // Repositories
        builder.Services.AddScoped<ILicenseRepository, LicenseRepository>();

        // Core Services
        builder.Services.AddScoped<ILicenseService, LicenseService>();
        builder.Services.AddScoped<ILicenseActivationService, LicenseActivationService>();
        builder.Services.AddScoped<IHeartbeatService, HeartbeatService>();
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IEmailService, EmailService>();

        // Security Services
        builder.Services.AddScoped<ILicenseKeyGenerator, LicenseKeyGenerator>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<INonceService, NonceService>(); // Implementar depois
        // === App ===
        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseForwardedHeaders();
        app.UseHttpsRedirection();
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}