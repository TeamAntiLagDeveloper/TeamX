namespace TeamX.API.DTOs.License;

/// <summary>
/// Resposta da criação de uma licença.
/// </summary>
public record CreateLicenseResponse
{
    public required bool Success { get; init; }
    public required string LicenseKey { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? Message { get; init; }
    public Guid? LicenseId { get; init; }
    public string? Status { get; init; }

    // Factory methods (opcional, mas muito útil)
    public static CreateLicenseResponse Ok(
        string licenseKey,
        DateTime? expiresAt = null,
        Guid? licenseId = null,
        string? status = null)
    {
        return new CreateLicenseResponse
        {
            Success = true,
            LicenseKey = licenseKey,
            ExpiresAt = expiresAt,
            LicenseId = licenseId,
            Status = status,
            Message = "Licença criada com sucesso"
        };
    }

    public static CreateLicenseResponse Fail(string message)
    {
        return new CreateLicenseResponse
        {
            Success = false,
            LicenseKey = string.Empty,
            Message = message
        };
    }
}