namespace TeamX.Core.Enums;

/// <summary>
/// Espelho dos valores de string em LicenseStatuses.
/// O domínio persiste Status como string.
/// </summary>
public enum LicenseStatus
{
    Pending = 0,
    Active = 1,
    Suspended = 2,
    Expired = 3,
    Revoked = 4
}