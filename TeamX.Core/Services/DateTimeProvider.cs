using TeamX.Core.Interfaces;

namespace TeamX.Core.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}