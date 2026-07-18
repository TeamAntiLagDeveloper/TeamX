using System.Security.Cryptography;

namespace TeamX.Security.Integrity;

public static class DigitalSignatureVerifier
{
    // Chave pública (você gera uma vez e embute no código)
    private static readonly string PublicKeyXml =
    @"<RSAKeyValue>
<Modulus>
ywZwdneTOqzvmctxpE0NzQ8cqOMdoWJW8zn+3q6JqYm7KxetuGKYY7yExgqSQrNamxWUKiZuKDqDlyDBEMnCoVYQFuM5Ovw7+ePqEq47pXDjPvnZgCiqwArBO49kOoDTZ/ZrpbyDjo2+b2QeEGvzgHLkPTnTQE4M3xpYexEERdOVdlwfuvohA/oy4F/a6Ro0XcT8HvLc0m5po4A6e18UOSVK2dzyKtSr2shI3rQDxgmsZLrJ6eSxtHL9mHLF/VyTXZOp0hM75VWTMUmDCAO6lNC/dmvTdl8WZC/erJvuFzL9LZwE73mfNqDVquf8DI5sj4DS8Osg/uiRHUIvNgygWQ==
</Modulus>
<Exponent>AQAB</Exponent>
</RSAKeyValue>";

    /// <summary>
    /// Verifica se o executável tem assinatura válida
    /// </summary>
    public static bool VerifySignature()
    {
        try
        {
            string exePath = Environment.ProcessPath!;

            byte[] fileBytes = File.ReadAllBytes(exePath);

            byte[]? signature = GetSignature();


            if (signature == null)
                return false;


            using RSA rsa = RSA.Create();

            rsa.FromXmlString(PublicKeyXml);


            return rsa.VerifyData(
                fileBytes,
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );
        }
        catch
        {
            return false;
        }
    }

    private static byte[]? GetSignature()
    {
        string exePath = Environment.ProcessPath!;

        string sigPath = exePath + ".sig";

        if (!File.Exists(sigPath))
            return null;

        return File.ReadAllBytes(sigPath);
    }

    public static void CheckAndExitIfInvalid()
    {
        if (!VerifySignature())
        {
            HandleInvalidSignature();
        }
    }

    private static void HandleInvalidSignature()
    {
        try
        {

        }
        catch { }

        Environment.Exit(0);
    }
}