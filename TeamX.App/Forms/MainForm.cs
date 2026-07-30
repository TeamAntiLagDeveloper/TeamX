using TeamX.App.Services;

namespace TeamX.App;

public partial class Form1 : Form
{
    private readonly ApiService _apiService = new();
    private readonly StartupService _startupService = new();

    private System.Windows.Forms.Timer? _licenseTimer;
    private System.Windows.Forms.Timer? _heartbeatTimer;

    private readonly SemaphoreSlim _revalidateLock = new(1, 1);
    private readonly SemaphoreSlim _heartbeatLock = new(1, 1);

    private int _failedValidations;
    private bool _isClosing;
    private CancellationTokenSource? _cts;

    private const int MaxFailedValidations = 2;
    private const int LicenseIntervalMs = 15 * 60 * 1000;
    private const int HeartbeatIntervalMs = 8 * 60 * 1000;
    private const int FirstHeartbeatDelayMs = 30_000;

    public Form1()
    {
        InitializeComponent();
        Load += Form1_Load;
        FormClosed += Form1_FormClosed;
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        _cts = new CancellationTokenSource();

        _licenseTimer = new System.Windows.Forms.Timer { Interval = LicenseIntervalMs };
        _licenseTimer.Tick += LicenseTimer_Tick;
        _licenseTimer.Start();

        _heartbeatTimer = new System.Windows.Forms.Timer { Interval = HeartbeatIntervalMs };
        _heartbeatTimer.Tick += HeartbeatTimer_Tick;
        _heartbeatTimer.Start();

        _ = DelayedHeartbeatAsync(_cts.Token);
    }

    private async void LicenseTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            await RevalidateLicenseAsync(_cts?.Token ?? CancellationToken.None);
        }
        catch
        {
            // não deixa exception escapar do tick
        }
    }

    private async void HeartbeatTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            await SendHeartbeatAsync(_cts?.Token ?? CancellationToken.None);
        }
        catch
        {
            // silencioso
        }
    }

    private async Task DelayedHeartbeatAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(FirstHeartbeatDelayMs, ct);
            await SendHeartbeatAsync(ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
        }
    }

    private async Task RevalidateLicenseAsync(CancellationToken ct)
    {
        if (_isClosing || ct.IsCancellationRequested)
            return;

        if (!await _revalidateLock.WaitAsync(0, ct))
            return;

        try
        {
            var valid = await _startupService.ValidateSavedLicense(ct);

            if (ct.IsCancellationRequested || _isClosing)
                return;

            if (!valid && _startupService.RequiresUpdate)
            {
                SafeUi(() =>
                {
                    MessageBox.Show(
                        this,
                        "Uma nova versão do TeamX é necessária para continuar.",
                        "TeamX",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    ExitApp();
                });
                return;
            }

            if (valid)
            {
                Interlocked.Exchange(ref _failedValidations, 0);
                return;
            }

            var failures = Interlocked.Increment(ref _failedValidations);

            if (failures >= MaxFailedValidations)
            {
                try { LicenseStorage.Clear(); } catch { /* ignore */ }
                SafeUi(ExitApp);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
        }
        finally
        {
            _revalidateLock.Release();
        }
    }

    private async Task SendHeartbeatAsync(CancellationToken ct)
    {
        if (_isClosing || ct.IsCancellationRequested)
            return;

        if (!await _heartbeatLock.WaitAsync(0, ct))
            return;

        try
        {
            var stored = LicenseStorage.Load();
            if (stored is null || string.IsNullOrWhiteSpace(stored.Token))
                return;

            var hw = HardwareService.GetStrongFingerprint();
            var ok = await _apiService.SendHeartbeatAsync(stored.Token, hw, ct);

            if (ok)
                LicenseStorage.TouchOnlineValidation();
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
        }
        finally
        {
            _heartbeatLock.Release();
        }
    }

    private void ExitApp()
    {
        if (_isClosing)
            return;

        _isClosing = true;
        StopTimers();

        try
        {
            Close();
        }
        catch
        {
            Application.Exit();
        }
    }

    private void SafeUi(Action action)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        try
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private void StopTimers()
    {
        try { _cts?.Cancel(); } catch { /* ignore */ }

        if (_licenseTimer is not null)
        {
            _licenseTimer.Stop();
            _licenseTimer.Tick -= LicenseTimer_Tick;
            _licenseTimer.Dispose();
            _licenseTimer = null;
        }

        if (_heartbeatTimer is not null)
        {
            _heartbeatTimer.Stop();
            _heartbeatTimer.Tick -= HeartbeatTimer_Tick;
            _heartbeatTimer.Dispose();
            _heartbeatTimer = null;
        }
    }

    private void Form1_FormClosed(object? sender, FormClosedEventArgs e)
    {
        _isClosing = true;
        StopTimers();

        _apiService.Dispose();
        _startupService.Dispose();

        _revalidateLock.Dispose();
        _heartbeatLock.Dispose();
        _cts?.Dispose();
    }
}