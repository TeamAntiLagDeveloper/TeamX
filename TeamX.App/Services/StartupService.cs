using TeamX.App.Forms;
using TeamX.Security.AntiDebug;
using TeamX.Security.AntiInjection;
using TeamX.Security.AntiVM;
using TeamX.Security.Integrity;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services;

public class StartupService
{
    private readonly ApiService _apiService = new();

    public async Task<bool> ValidateSavedLicense()
    {
        var stored = LicenseStorage.Load();
        if (stored == null) return false;

        try
        {
            var hardwareFp = HardwareService.GetStrongFingerprint();

            var validation = await _apiService.ValidateTokenAsync(stored.Token, hardwareFp);

            return validation.Success && validation.IsValid;
        }
        catch
        {
            return false;
        }
    }

    public void RunSecurityChecks()
    {
        try
        {
            DebuggerDetector.FullCheck();

            if (AntiVM.IsRunningInVM())
            {
                MessageBox.Show("Execução não permitida em máquina virtual.",
                    "TeamX", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            // Outras verificações de integridade...
        }
        catch
        {
            Environment.Exit(1);
        }
    }
}