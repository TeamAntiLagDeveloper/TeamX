using TeamX.Core.Interfaces;

namespace TeamX.Core.Services;

/// <summary>
/// Implementação padrão de <see cref="IGuidProvider"/>.
/// Encapsula a geração de GUIDs para facilitar testes e substituição.
/// </summary>
public sealed class GuidProvider : IGuidProvider
{
    /// <summary>
    /// Gera um novo identificador único global (GUID).
    /// </summary>
    public Guid NewGuid() => Guid.NewGuid();
}