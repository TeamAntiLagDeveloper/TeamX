using System.Security.Cryptography;
using System.Text;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services;

public static class SignatureService
{
    private static readonly string Secret = "MESMA_CHAVE_USADA_NO_SERVIDOR"; // Deve ser igual à do servidor

    public static string GenerateSignature(SecureActivateRequest request)
    {
        var data = $"{request.LicenseKey}|{request.HardwareFingerprint}|{request.Nonce}|{request.Timestamp}|{request.ExecutableHash}|{request.AppVersion}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash);
    }
}