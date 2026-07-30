namespace TeamX.App.Services;

/// <summary>
/// Segredos do cliente. Deve ser idêntico a Activation:SigningSecret na API.
/// Em desktop o valor pode ser extraído do binário — ainda assim bloqueia requests manuais triviais.
/// </summary>
internal static class ClientSecrets
{
    // TROQUE pelos mesmos 32+ caracteres da API (Activation:SigningSecret)
    internal const string ActivationSigningSecret =
        "TeamXActSignKey2026ProdSecure32CharsXX";
}