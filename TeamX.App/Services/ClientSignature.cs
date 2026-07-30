using System.Security.Cryptography;
using System.Text;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services;

/// <summary>
/// Gera assinatura HMAC-SHA256 no mesmo formato do SignatureService da API.
/// </summary>
internal static class ClientSignature
{
    public static string Sign(SecureActivateRequest request, string secret)
    {
        ArgumentNullException.ThrowIfNull(request);

        static string N(string? value) => value?.Trim() ?? string.Empty;

        var payload = string.Join('|',
            N(request.LicenseKey).ToUpperInvariant(),
            N(request.HardwareFingerprint),
            N(request.Nonce),
            request.Timestamp.ToString(),
            N(request.ExecutableHash),
            N(request.AppVersion));

        var hash = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(payload));

        return Convert.ToHexString(hash);
    }
}