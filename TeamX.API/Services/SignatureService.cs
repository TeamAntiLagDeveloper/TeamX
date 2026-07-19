using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;
using TeamX.Core.Interfaces;
using TeamX.Shared.DTOs;

namespace TeamX.API.Services;

public class SignatureService : ISignatureService
{
    private const string Secret =
    "7f3a9d2c8e1b5f6a4d9c7e2b8a1f6d3c9e5a7b2d4f8c1a6e9b3c7d5f2a8";
    public string GenerateSignature(SecureActivateRequest request, string secret)
    {
        var data =
            $"{request.LicenseKey}|{request.HardwareFingerprint}|{request.Nonce}|{request.Timestamp}|{request.ExecutableHash}|{request.AppVersion}";

        Console.WriteLine("API DATA:");
        Console.WriteLine(data);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

        return Convert.ToHexString(hash);
    }

    public bool ValidateSignature(
        SecureActivateRequest request,
        string receivedSignature)
    {
        var data =
            $"{request.LicenseKey}|{request.HardwareFingerprint}|{request.Nonce}|{request.Timestamp}|{request.ExecutableHash}|{request.AppVersion}";

        Console.WriteLine("LICENSE:");
        Console.WriteLine(request.LicenseKey);

        Console.WriteLine("HW:");
        Console.WriteLine(request.HardwareFingerprint);

        Console.WriteLine("NONCE:");
        Console.WriteLine(request.Nonce);

        Console.WriteLine("TIMESTAMP:");
        Console.WriteLine(request.Timestamp);

        Console.WriteLine("HASH:");
        Console.WriteLine(request.ExecutableHash);

        Console.WriteLine("VERSION:");
        Console.WriteLine(request.AppVersion);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

        var expectedSignature = Convert.ToHexString(hash);

        return expectedSignature.Equals(receivedSignature, StringComparison.OrdinalIgnoreCase);
    }
}