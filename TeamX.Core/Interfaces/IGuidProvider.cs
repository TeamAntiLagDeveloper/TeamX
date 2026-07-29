namespace TeamX.Core.Interfaces;

/// <summary>
/// Abstração para geração de GUIDs.
/// Permite mockar a criação de identificadores em testes.
/// </summary>
public interface IGuidProvider
{
    /// <summary>
    /// Gera um novo identificador único global (GUID).
    /// </summary>
    Guid NewGuid();
}