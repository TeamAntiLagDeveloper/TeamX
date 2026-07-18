using System.Security.Cryptography;
using System.Text;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services;

public static class SignatureService
{
    private static readonly string Secret = "3A8E5F91B9C4D7F2E1A65C8B34D91F77TEAMX2026SECURE9F2A1B8C";

    public static string GenerateSignature(SecureActivateRequest request)
    {
        var data = $"{request.LicenseKey}|{request.HardwareFingerprint}|{request.Nonce}|{request.Timestamp}|{request.ExecutableHash}|{request.AppVersion}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash);
    }
}