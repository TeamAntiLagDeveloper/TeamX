namespace TeamX.Core.Constants;

public static class SystemConstants
{
    public const string ProductName = "TeamX";
    public const string ProductDisplayName = "TeamX Optimizer";
    public const string ApiName = "TeamX API";
    public const string Company = "Team AntiLag";

    public const string Website = "https://teamantilag.com";
    public const string DownloadUrl = "https://teamantilag.com/teamx";
    public const string LicenseUrl = "https://teamantilag.com/license";
    public const string SupportUrl = "https://teamantilag.com/support";

    /// <summary>
    /// Versão do cliente. Formato: MAJOR.MINOR.PATCH
    /// Deve ser atualizada a cada release (e alinhada ao assembly, se possível).
    /// </summary>
    public const string CurrentVersion = "1.0.0";

    /// <summary>
    /// User-Agent enviado pelo app desktop.
    /// </summary>
    public const string UserAgent = ProductName + "/" + CurrentVersion;

    /// <summary>
    /// Pasta em LocalApplicationData (ex.: %LOCALAPPDATA%\TeamX).
    /// </summary>
    public const string AppDataFolderName = "TeamX";
}