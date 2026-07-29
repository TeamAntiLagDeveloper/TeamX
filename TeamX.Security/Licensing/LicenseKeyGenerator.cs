using System.Security.Cryptography;
using System.Text;
using TeamX.Security.Licensing;

namespace TeamX.Security.Licensing;

/// <summary>
/// Gerador de chaves de licença no formato TX-XXXX-XXXX-XXXX-XXXX.
/// Remove caracteres ambíguos (I, O, 0, 1) para evitar confusão na digitação.
/// </summary>
public sealed class LicenseKeyGenerator : ILicenseKeyGenerator
{
    // Sem I, O, 0, 1 — evita confusão na digitação
    private static readonly string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public string Generate()
    {
        var blocks = new string[4];

        for (int i = 0; i < 4; i++)
            blocks[i] = GenerateBlock();

        return $"TX-{string.Join("-", blocks)}";
    }

    public IEnumerable<string> Generate(int quantity)
    {
        if (quantity <= 0)
            yield break;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var safety = quantity * 5; // proteção contra loop infinito

        while (seen.Count < quantity && safety-- > 0)
        {
            var key = Generate();

            if (seen.Add(key))
                yield return key;
        }
    }

    public bool IsValidFormat(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var parts = key.Trim()
                       .ToUpperInvariant()
                       .Split('-', StringSplitOptions.RemoveEmptyEntries);

        // Formato esperado: TX-XXXX-XXXX-XXXX-XXXX
        if (parts.Length != 5)
            return false;

        if (parts[0] != "TX")
            return false;

        foreach (var part in parts.Skip(1))
        {
            if (part.Length != 4)
                return false;

            foreach (var c in part)
            {
                if (Alphabet.IndexOf(c) < 0)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Remove espaços e normaliza para maiúsculas.
    /// Mantém os hífens (formato canônico).
    /// </summary>
    public string Normalize(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        return key.Trim()
                  .ToUpperInvariant()
                  .Replace(" ", string.Empty);
    }

    private static string GenerateBlock()
    {
        var result = new StringBuilder(4);

        for (int i = 0; i < 4; i++)
        {
            var index = RandomNumberGenerator.GetInt32(Alphabet.Length);
            result.Append(Alphabet[index]);
        }

        return result.ToString();
    }
}