namespace TeamX.Core.Enums;

/// <summary>
/// Representa o status atual de uma licença.
/// </summary>
public enum LicenseStatus
{
    /// <summary>
    /// Licença criada, mas ainda não ativada.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Licença ativa e válida para uso.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Licença temporariamente suspensa (pode ser reativada).
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Licença expirou por tempo.
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Licença permanentemente revogada.
    /// </summary>
    Revoked = 4
}