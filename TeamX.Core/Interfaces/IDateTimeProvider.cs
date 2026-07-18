namespace TeamX.Core.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}