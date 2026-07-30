using System.Security.Cryptography;
using System.Text;

namespace TeamX.Security.Licensing;

/// <summary>
/// Formato: TX-XXXX-XXXX-XXXX-XXXX
/// Sem I, O, 0, 1 (menos confusão na digitação).
/// </summary>
public sealed class LicenseKeyGenerator : ILicenseKeyGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public string Generate()
    {
        return $"TX-{GenerateBlock()}-{GenerateBlock()}-{GenerateBlock()}-{GenerateBlock()}";
    }

    public IEnumerable<string> Generate(int quantity)
    {
        if (quantity <= 0)
            yield break;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var safety = Math.Max(quantity * 5, quantity + 10);

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

        var parts = Normalize(key).Split('-', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 5 || parts[0] != "TX")
            return false;

        for (var i = 1; i < 5; i++)
        {
            if (parts[i].Length != 4)
                return false;

            foreach (var c in parts[i])
            {
                if (Alphabet.IndexOf(c) < 0)
                    return false;
            }
        }

        return true;
    }

    public string Normalize(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        return key.Trim().ToUpperInvariant().Replace(" ", string.Empty);
    }

    private static string GenerateBlock()
    {
        Span<char> chars = stackalloc char[4];
        for (var i = 0; i < 4; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return new string(chars);
    }
}