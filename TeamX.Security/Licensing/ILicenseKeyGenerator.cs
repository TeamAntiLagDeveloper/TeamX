using System.Collections.Generic;

namespace TeamX.Security.Licensing;

public interface ILicenseKeyGenerator
{
    string Generate();

    IEnumerable<string> Generate(int quantity);

    bool IsValidFormat(string key);

    string Normalize(string key);
}