using System.Security.Cryptography;
using System.Text;
using TeamX.Shared.DTOs;

namespace TeamX.App.Services;

public static class SignatureService
{
    private const string Secret =
        "7f3a9d2c8e1b5f6a4d9c7e2b8a1f6d3c9e5a7b2d4f8c1a6e9b3c7d5f2a8";

    public static string GenerateSignature(SecureActivateRequest request)
    {
        var data =
            $"{request.LicenseKey}|{request.HardwareFingerprint}|{request.Nonce}|{request.Timestamp}|{request.ExecutableHash}|{request.AppVersion}";
        MessageBox.Show(
$"""
LICENSE:
{request.LicenseKey}

HW:
{request.HardwareFingerprint}

NONCE:
{request.Nonce}

TIMESTAMP:
{request.Timestamp}

HASH:
{request.ExecutableHash}

VERSION:
{request.AppVersion}
""");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));

        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

        return Convert.ToHexString(hash);
    }
}