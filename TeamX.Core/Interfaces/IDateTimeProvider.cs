namespace TeamX.Core.Interfaces;

/// <summary>
/// Abstração para obtenção da data/hora atual.
/// Facilita testes e controle de tempo.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Retorna a data e hora atual em UTC.
    /// </summary>
    DateTime UtcNow { get; }
}