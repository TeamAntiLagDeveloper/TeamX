using TeamX.Core.Interfaces;

namespace TeamX.Core.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}