using System.Security.Cryptography;
using System.Text;

namespace TeamX.Security.Licensing;

public class LicenseKeyGenerator : ILicenseKeyGenerator
{
    private const string Alphabet =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";


    public string Generate()
    {
        var blocks = new List<string>();

        for (int i = 0; i < 4; i++)
        {
            blocks.Add(GenerateBlock());
        }

        return $"TX-{string.Join("-", blocks)}";
    }


    public IEnumerable<string> Generate(int quantity)
    {
        var keys = new List<string>();

        for (int i = 0; i < quantity; i++)
        {
            keys.Add(Generate());
        }

        return keys;
    }


    public bool IsValidFormat(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;


        var parts = key.Split('-');


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
                if (!Alphabet.Contains(c))
                    return false;
            }
        }


        return true;
    }


    public string Normalize(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;


        return key
            .Trim()
            .ToUpper()
            .Replace(" ", "")
            .Replace("-", "");
    }


    private string GenerateBlock()
    {
        var result = new StringBuilder();


        for (int i = 0; i < 4; i++)
        {
            int index = RandomNumberGenerator
                .GetInt32(Alphabet.Length);


            result.Append(Alphabet[index]);
        }


        return result.ToString();
    }
}