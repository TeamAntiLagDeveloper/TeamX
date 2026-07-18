using System.Security.Cryptography;
using System.Text;

namespace TeamX.Security.Integrity;

public static class IntegrityChecker
{
    private static readonly string ExpectedHash = "SEU_HASH_AQUI"; // Você vai gerar isso

    /// <summary>
    /// Verifica se o executável foi modificado
    /// </summary>
    public static bool IsTampered()
    {
        try
        {
            string currentHash = CalculateCurrentExeHash();

            if (string.IsNullOrEmpty(currentHash))
                return true;

            return currentHash != ExpectedHash;
        }
        catch
        {
            return true; // Se falhar ao calcular, considera como tampering
        }
    }

    private static string CalculateCurrentExeHash()
    {
        string exePath = Environment.ProcessPath ?? AppDomain.CurrentDomain.BaseDirectory;

        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(exePath);

        byte[] hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Gera o hash atual (use isso uma vez após build final)
    /// </summary>
    public static string GenerateCurrentHash()
    {
        string hash = CalculateCurrentExeHash();
        Console.WriteLine($"HASH GERADO: {hash}");
        return hash;
    }

    public static void CheckAndExitIfTampered()
    {
        if (IsTampered())
        {
            HandleTamperingDetected();
        }
    }

    private static void HandleTamperingDetected()
    {
        try
        {
            // Comportamento discreto
            Environment.Exit(0);

            // Ou mensagem falsa:
            // MessageBox.Show("Erro crítico de inicialização.", "TeamX", 
            //     MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch
        {
            Environment.Exit(1);
        }
    }
}