using TeamX.Core.Constants;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services;

public sealed class StartupService : IDisposable
{
    /// <summary>
    /// Grace period offline após a última validação online bem-sucedida.
    /// </summary>
    private static readonly TimeSpan OfflineGracePeriod = TimeSpan.FromHours(48);

    private readonly ApiService _apiService = new();

    public bool RequiresUpdate { get; private set; }

    public async Task<bool> ValidateSavedLicense(CancellationToken ct = default)
    {
        RequiresUpdate = false;

        var stored = LicenseStorage.Load();
        if (stored is null)
            return false;

        // Barreira local de expiração
        if (stored.ExpiresAt <= DateTime.UtcNow)
        {
            LicenseStorage.Clear();
            return false;
        }

        try
        {
            var hardwareFp = HardwareService.GetStrongFingerprint();
            var exeHash = HardwareService.GetExecutableHash();

            var validation = await _apiService.ValidateTokenAsync(
                stored.Token,
                hardwareFp,
                exeHash,
                ct);

            // Update forçado pelo servidor
            if (validation.ForceUpdate)
            {
                RequiresUpdate = true;
                return false;
            }

            // Versão mínima
            if (!string.IsNullOrWhiteSpace(validation.MinAppVersion) &&
                IsVersionLower(SystemConstants.CurrentVersion, validation.MinAppVersion))
            {
                RequiresUpdate = true;
                return false;
            }

            // Online OK
            if (validation.Success && validation.IsValid)
            {
                LicenseStorage.TouchOnlineValidation();
                return true;
            }

            // Falha transitória (rede/timeout/5xx) → grace period, NÃO apaga a licença
            if (IsTransientFailure(validation))
                return IsWithinGracePeriod(stored);

            // Rejeição explícita do servidor (revogado, hardware, expirado, etc.)
            LicenseStorage.Clear();
            return false;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // Exception inesperada / rede → grace period
            return IsWithinGracePeriod(stored);
        }
    }

    private static bool IsWithinGracePeriod(StoredLicense stored)
    {
        return stored.LastSuccessfulOnlineValidation.Add(OfflineGracePeriod) > DateTime.UtcNow;
    }

    /// <summary>
    /// Indica falha que não deve queimar a licença local.
    /// Alinhado às mensagens do ApiService melhorado.
    /// </summary>
    private static bool IsTransientFailure(TokenValidationResponse validation)
    {
        if (validation.Success)
            return false;

        var msg = validation.Message ?? string.Empty;

        // Mensagens típicas de rede / timeout / servidor indisponível
        if (msg.Contains("conexão", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("esgotado", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("internet", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("servidor", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("indisponível", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Status HTTP embutido (caso algum caminho ainda devolva isso)
        if (msg.Contains("HTTP 408", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("HTTP 429", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("HTTP 502", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("HTTP 503", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("HTTP 504", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool IsVersionLower(string? current, string minimum)
    {
        if (!Version.TryParse(NormalizeVersion(current), out var c))
            return false;

        if (!Version.TryParse(NormalizeVersion(minimum), out var m))
            return false;

        return c < m;
    }

    private static string NormalizeVersion(string? v)
    {
        var parts = (v ?? "0").Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        while (parts.Count < 3)
            parts.Add("0");

        return string.Join('.', parts.Take(3));
    }

    public void Dispose()
    {
        _apiService.Dispose();
    }
}