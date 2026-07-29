using TeamX.Core.Interfaces;

namespace TeamX.Core.Services;

/// <summary>
/// Implementação padrão de <see cref="IDateTimeProvider"/>.
/// Encapsula o acesso à data/hora atual para facilitar testes.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Retorna a data e hora atual em UTC.
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
}