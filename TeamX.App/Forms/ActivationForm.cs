using TeamX.App.Services;
using TeamX.Core.Constants;
using TeamX.Shared.DTOs;

namespace TeamX.App.Forms;

public partial class MainForm : Form
{
    private readonly ApiService _apiService = new();

    public MainForm()
    {
        InitializeComponent();
        LoadHardwareInfo();
    }

    private void LoadHardwareInfo()
    {
        string fp = HardwareService.GetStrongFingerprint();
        txtHardwareId.Text = fp;
        txtComputerName.Text = fp;
        txtWindowsVersion.Text = fp;
        txtIpAddress.Text = fp;
    }

    private async void btnActivate_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtLicenseKey.Text))
        {
            MessageBox.Show("Digite a chave de licença!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnActivate.Enabled = false;
        btnActivate.Text = "Ativando...";

        try
        {
            var request = new SecureActivateRequest
            {
                LicenseKey = txtLicenseKey.Text.Trim().ToUpper(),
                HardwareFingerprint = HardwareService.GetStrongFingerprint(),
                Nonce = Guid.NewGuid().ToString(),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExecutableHash = HardwareService.GetExecutableHash(),
                AppVersion = SystemConstants.CurrentVersion,
            };

            // Preenche Signature depois de criar o objeto
            request.Signature = SignatureService.GenerateSignature(request); // ajuste conforme implementação

            var response = await _apiService.ActivateLicenseAsync(request);

            if (response.Success)
            {
                LicenseStorage.Save(response, request.HardwareFingerprint);

                MessageBox.Show("✅ Licença ativada com sucesso!", "Sucesso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show($"❌ {response.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao ativar: {ex.Message}", "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnActivate.Enabled = true;
            btnActivate.Text = "Ativar Licença";
        }
    }
}