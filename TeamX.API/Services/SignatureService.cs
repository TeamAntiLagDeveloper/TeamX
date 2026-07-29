using System.Security.Cryptography;
using System.Text;
using TeamX.Core.Interfaces;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class SignatureService : ISignatureService
{
    public string GenerateSignature(SecureActivateRequest request, string secret)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret é obrigatório.", nameof(secret));

        var payload = BuildPayload(request);
        var hash = ComputeHmac(payload, secret);

        // Hex uppercase (padrão do Convert.ToHexString)
        return Convert.ToHexString(hash);
    }

    public bool ValidateSignature(SecureActivateRequest request, string secret)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Signature) ||
            string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        byte[] providedHash;
        try
        {
            // Aceita hex com ou sem hífens, maiúsculo/minúsculo
            providedHash = Convert.FromHexString(request.Signature.Trim());
        }
        catch (FormatException)
        {
            return false; // assinatura malformada
        }

        var payload = BuildPayload(request);
        var expectedHash = ComputeHmac(payload, secret);

        // Comparação em tempo constante nos bytes do hash (não na string hex)
        return CryptographicOperations.FixedTimeEquals(expectedHash, providedHash);
    }

    private static byte[] ComputeHmac(string payload, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(payload);

        return HMACSHA256.HashData(key, data);
    }

    private static string BuildPayload(SecureActivateRequest request)
    {
        // Campos normalizados na mesma ordem em que o cliente assina.
        // Null → string vazia para manter o formato estável.
        static string N(string? value) => value?.Trim() ?? string.Empty;

        return string.Join('|',
            N(request.LicenseKey).ToUpperInvariant(),
            N(request.HardwareFingerprint),
            N(request.Nonce),
            request.Timestamp.ToString(),
            N(request.ExecutableHash),
            N(request.AppVersion));
    }
}