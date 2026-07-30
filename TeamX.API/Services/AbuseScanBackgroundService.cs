using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamX.Data.Context;

namespace TeamX.API.Services;

public class AbuseScanOptions
{
    public const string SectionName = "AbuseScan";

    /// <summary>Intervalo entre scans (padrão: 24 horas).</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Atraso inicial antes do primeiro scan.</summary>
    public TimeSpan StartupDelay { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>Se true, EvaluateLicenseAsync pode suspender licenças abusivas.</summary>
    public bool AutoSuspend { get; set; } = true;
}

public class AbuseScanBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AbuseScanBackgroundService> _logger;
    private readonly AbuseScanOptions _options;

    public AbuseScanBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AbuseScanBackgroundService> logger,
        IOptions<AbuseScanOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "AbuseScanBackgroundService iniciado. Intervalo: {Interval}, AutoSuspend: {AutoSuspend}",
            _options.Interval,
            _options.AutoSuspend);

        try
        {
            await Task.Delay(_options.StartupDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(_options.Interval);

        await RunScanAsync(stoppingToken);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunScanAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // shutdown normal
        }
    }

    private async Task RunScanAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return;

        _logger.LogInformation("Iniciando scan de abuso de licenças...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var abuseService = scope.ServiceProvider.GetRequiredService<AbuseDetectionService>();

            var suspiciousLicenseIds = await db.Licenses
                .AsNoTracking()
                .Where(l => l.Status != "Revoked" && l.Status != "Suspended")
                .Where(l => l.Devices.Count(d => d.IsActive) > l.MaxDevices)
                .Select(l => l.Id)
                .ToListAsync(ct);

            if (suspiciousLicenseIds.Count == 0)
            {
                _logger.LogInformation("Nenhuma licença suspeita encontrada no scan.");
                return;
            }

            _logger.LogWarning(
                "Scan encontrou {Count} licença(s) com mais devices ativos que o permitido",
                suspiciousLicenseIds.Count);

            if (!_options.AutoSuspend)
            {
                _logger.LogInformation(
                    "AutoSuspend=false — apenas detecção. IDs: {Ids}",
                    string.Join(", ", suspiciousLicenseIds));
                return;
            }

            var evaluated = 0;

            foreach (var licenseId in suspiciousLicenseIds)
            {
                if (ct.IsCancellationRequested)
                    break;

                try
                {
                    await abuseService.EvaluateLicenseAsync(licenseId, ct);
                    evaluated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Erro ao avaliar licença {LicenseId} durante o scan de abuso",
                        licenseId);
                }
            }

            _logger.LogInformation(
                "Scan finalizado. Suspeitas: {Total}. Avaliadas: {Evaluated}",
                suspiciousLicenseIds.Count,
                evaluated);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro crítico no scan de abuso");
        }
    }
}