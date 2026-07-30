using System.Diagnostics;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using TeamX.App.Services;
using TeamX.Core.Constants;
using TeamX.Shared.DTOs;

namespace TeamX.App.Forms;

public partial class MainForm : Form
{
    private readonly ApiService _apiService = new();
    private bool _isInitialized;
    private bool _isActivating;
    private bool _allowClose;

    public MainForm()
    {
        InitializeComponent();

        Opacity = 0;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;

        FormClosing += MainForm_FormClosing;
        Shown += MainForm_Shown;
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        try
        {
            await webView21.EnsureCoreWebView2Async(null);

            var settings = webView21.CoreWebView2.Settings;
            settings.IsStatusBarEnabled = false;
            settings.AreDefaultContextMenusEnabled = false;
            settings.AreDevToolsEnabled = false;
            settings.IsZoomControlEnabled = false;
            settings.IsSwipeNavigationEnabled = false;
            settings.IsBuiltInErrorPageEnabled = false;
            settings.IsWebMessageEnabled = true;
            settings.AreHostObjectsAllowed = false;

            webView21.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;
            webView21.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            var htmlPath = Path.Combine(Application.StartupPath, "Assets", "Active.html");
            if (!File.Exists(htmlPath))
                throw new FileNotFoundException("Arquivo de UI não encontrado.", htmlPath);

            webView21.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
        }
        catch (Exception ex)
        {
            Opacity = 1;
            MessageBox.Show(
                $"Erro ao carregar a interface:\n{ex.Message}",
                "TeamX",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _allowClose = true;
            Close();
        }
    }

    private void CoreWebView2_NavigationCompleted(
        object? sender,
        CoreWebView2NavigationCompletedEventArgs e)
    {
        Opacity = 1;

        if (!e.IsSuccess)
        {
            MessageBox.Show(
                "Falha ao carregar a interface de ativação.",
                "TeamX",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void WebView_WebMessageReceived(
        object? sender,
        CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => WebView_WebMessageReceived(sender, e));
            return;
        }

        string msg;
        try
        {
            msg = e.TryGetWebMessageAsString()?.Trim() ?? string.Empty;
        }
        catch
        {
            return;
        }

        if (string.IsNullOrEmpty(msg))
            return;

        if (msg.StartsWith("activate:", StringComparison.OrdinalIgnoreCase))
        {
            var licenseKey = msg["activate:".Length..].Trim();
            _ = RealActivateAsync(licenseKey);
            return;
        }

        switch (msg.ToLowerInvariant())
        {
            case "buy":
                OpenInBrowser(SystemConstants.LicenseUrl);
                break;
            case "support":
                OpenInBrowser(SystemConstants.SupportUrl);
                break;
            case "close":
                _allowClose = true;
                Close();
                break;
            case "minimize":
                WindowState = FormWindowState.Minimized;
                break;
        }
    }

    private async Task RealActivateAsync(string licenseKey)
    {
        if (_isActivating)
            return;

        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            await ShowErrorAsync("Informe a chave de licença.");
            return;
        }

        _isActivating = true;

        try
        {
            var key = licenseKey.Trim().ToUpperInvariant();

            var request = new SecureActivateRequest
            {
                LicenseKey = key,
                HardwareFingerprint = HardwareService.GetStrongFingerprint(),
                Nonce = Guid.NewGuid().ToString("N"),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExecutableHash = HardwareService.GetExecutableHash(),
                AppVersion = SystemConstants.CurrentVersion,
                ComputerName = Environment.MachineName,
                WindowsVersion = Environment.OSVersion.VersionString
            };

            request.Signature = ClientSignature.Sign(
                request,
                ClientSecrets.ActivationSigningSecret);

            var response = await _apiService.ActivateLicenseAsync(request);

            if (response.Success && !string.IsNullOrWhiteSpace(response.Token))
            {
                LicenseStorage.Save(response, request.HardwareFingerprint);
                await ExecuteJsAsync("showSuccess()");
                await Task.Delay(1200);

                _allowClose = true;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            await ShowErrorAsync(response.Message ?? "Não foi possível ativar a licença.");
        }
        catch (HttpRequestException)
        {
            await ShowErrorAsync("Sem conexão com o servidor. Verifique sua internet.");
        }
        catch (TaskCanceledException)
        {
            await ShowErrorAsync("Tempo esgotado. Tente novamente.");
        }
        catch (Exception)
        {
            await ShowErrorAsync("Erro inesperado ao ativar. Tente novamente.");
        }
        finally
        {
            _isActivating = false;
        }
    }

    private async Task ShowErrorAsync(string message)
    {
        var safeMessage = JsonSerializer.Serialize(
            message ?? "Chave inválida. Verifique e tente novamente.");
        await ExecuteJsAsync($"showError({safeMessage})");
    }

    private async Task ExecuteJsAsync(string script)
    {
        if (webView21?.CoreWebView2 is null || IsDisposed)
            return;

        try
        {
            await webView21.CoreWebView2.ExecuteScriptAsync(script);
        }
        catch
        {
            // WebView disposto
        }
    }

    private static void OpenInBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // ignore
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isActivating && !_allowClose)
        {
            e.Cancel = true;
            return;
        }

        try
        {
            if (webView21?.CoreWebView2 is not null)
                webView21.CoreWebView2.WebMessageReceived -= WebView_WebMessageReceived;
        }
        catch
        {
            // ignore
        }

        _apiService.Dispose();
    }
}