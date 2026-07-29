namespace TeamX.Security.Licensing;

/// <summary>
/// Responsável pela geração, validação e normalização de chaves de licença.
/// </summary>
public interface ILicenseKeyGenerator
{
    /// <summary>
    /// Gera uma nova chave de licença.
    /// </summary>
    string Generate();

    /// <summary>
    /// Gera múltiplas chaves de licença.
    /// </summary>
    /// <param name="quantity">Quantidade de chaves a serem geradas.</param>
    IEnumerable<string> Generate(int quantity);

    /// <summary>
    /// Verifica se a chave possui o formato válido.
    /// </summary>
    /// <param name="key">Chave a ser validada.</param>
    bool IsValidFormat(string key);

    /// <summary>
    /// Normaliza a chave (remove espaços, hífens, converte para maiúsculo, etc.).
    /// </summary>
    /// <param name="key">Chave a ser normalizada.</param>
    string Normalize(string key);
}