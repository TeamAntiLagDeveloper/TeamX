using System.Security.Cryptography;
using System.Text;
using TeamX.Core.Interfaces;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class SignatureService : ISignatureService
{
    public string GenerateSignature(SecureActivateRequest request, string secret)
    {
        var data =
            $"{request.LicenseKey}|{request.HardwareFingerprint}|{request.Nonce}|{request.Timestamp}|{request.ExecutableHash}|{request.AppVersion}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

        return Convert.ToHexString(hash);
    }

    public bool ValidateSignature(SecureActivateRequest request, string secret)
    {
        var expected = GenerateSignature(request, secret);

        return string.Equals(
            expected,
            request.Signature,
            StringComparison.OrdinalIgnoreCase);
    }
}